using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Apollo.SDK.NET.Interfaces;
using Apollo.SDK.NET.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apollo.SDK.NET;

/// <summary>
/// Apollo 客户端，用于管理开关配置和评估规则
/// </summary>
public class ApolloClient : IApolloClient
{
    #region 核心成员
    // 存储开关配置
    private readonly ConcurrentDictionary<string, Toggle> _toggles = new();
    // 文件系统监视器
    private readonly FileSystemWatcher _watcher;
    // 日志工厂
    private readonly ILoggerFactory _loggerFactory;
    // 日志记录器
    private readonly ILogger<ApolloClient> _logger;
    #endregion

    #region 初始化
    /// <summary>
    /// 构造时加载配置
    /// </summary>
    /// <param name="options">客户端配置</param>
    /// <param name="loggerFactory">日志工厂</param>
    public ApolloClient(ApolloOptions options, ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _logger = _loggerFactory.CreateLogger<ApolloClient>();
        _logger.LogInformation("Initializing ApolloClient...");

        // 冷热分离
        RuleEvaluator.Logger = _loggerFactory.CreateLogger("Apollo.SDK.NET.RuleEvaluator");
        RuleEvaluator.IsLogEnabled = _logger.IsEnabled(LogLevel.Error);

        SetTogglesPath(options.TogglesPath);

        // 热更新配置
        _watcher = new FileSystemWatcher(options.TogglesPath, "*.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
        };
        _watcher.Created += (s, e) => OnChanged(options.TogglesPath, e);
        _watcher.Deleted += (s, e) => OnChanged(options.TogglesPath, e);
        _watcher.Changed += (s, e) => OnChanged(options.TogglesPath, e);
        _watcher.Renamed += (s, e) => OnChanged(options.TogglesPath, e);
    }

    /// <summary>
    /// 热更新防抖
    /// </summary>
    /// <param name="sender">路径</param>
    /// <param name="e"></param>
    private void OnChanged(object sender, EventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Information)) LogFilesChanged();
        Thread.Sleep(100);
        SetTogglesPath((string)sender);
    }

    /// <summary>
    /// 加载开关配置文件
    /// </summary>
    /// <param name="directoryPath">开关配置文件路径</param>
    /// <exception cref="DirectoryNotFoundException">路径不存在</exception>
    /// <exception cref="FileNotFoundException">配置文件未找到</exception>
    /// <exception cref="FileLoadException">配置文件加载失败</exception>
    private void SetTogglesPath(string directoryPath)
    {
        // 加载时允许出现异常
        if (!Directory.Exists(directoryPath))
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Directory not found: {directoryPath}", directoryPath);
            }
            throw new DirectoryNotFoundException($"Path not found: {directoryPath}");
        }

        var files = Directory.GetFiles(directoryPath, "*.json");
        if (files.Length == 0)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("No toggle configuration files found in directory: {directoryPath}", directoryPath);
            }
            throw new FileNotFoundException($"File not found: {directoryPath}");
        }

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                var toggle = JsonSerializer.Deserialize(content, ApolloJsonSerializerContext.Default.Toggle);

                toggle?.Initialize();

                if (toggle?.Key != null)
                {
                    _toggles[toggle.Key] = toggle;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "Failed to load toggle from file: {file}", file);
                }
                throw new FileLoadException($"Failed to load toggle from file: {file}", ex);
            }
        }
    }
    #endregion

    #region 核心方法    
    public bool IsToggleAllowed(string key, ApolloContext context)
    {
        if (!_toggles.TryGetValue(key, out var toggle))
        {
            // 静默失败
            if (_logger.IsEnabled(LogLevel.Warning)) LogToggleNotFound(key);
            return false;
        }

        if (toggle.Status != "enabled")
        {
            if (_logger.IsEnabled(LogLevel.Warning)) LogToggleDisabled(key);
            return false;
        }

        var audiences = toggle.Audiences;
        for (int i = 0; i < audiences.Count; i++)
        {
            var audience = audiences[i];
            var rules = audience.Rules;
            bool allRulesPassed = true;

            for (int j = 0; j < rules.Count; j++)
            {
                if (!RuleEvaluator.Evaluate(rules[j], context))
                {
                    allRulesPassed = false;
                    break;
                }
            }

            if (allRulesPassed) return true;
        }

        return false;
    }
    #endregion

    #region 存在性检查    
    public bool IsToggleExist(string key)
    {
        return _toggles.ContainsKey(key);
    }

    public bool IsAudienceExist(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Audiences.Any(a => a.Id == audienceId);
        }
        LogToggleNotFound(toggleKey);
        return false;
    }

    public bool IsRuleExist(string toggleKey, string audienceId, string ruleId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                return audience.Rules.Any(r => r.Id == ruleId);
            }
            LogAudienceNotFound(toggleKey, audienceId);
            return false;
        }
        LogToggleNotFound(toggleKey);
        return false;
    }
    #endregion

    #region 状态检查    
    public bool GetToggleStatus(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Status == "enabled";
        }
        LogToggleNotFound(toggleKey);
        return false;
    }
    #endregion

    #region 数量检查    
    public int GetToggleCount()
    {
        return _toggles.Count;
    }

    public int GetAudienceCount(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Audiences.Count;
        }
        LogToggleNotFound(toggleKey);
        return 0;
    }

    public int GetRuleCount(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                return audience.Rules.Count;
            }
            LogAudienceNotFound(toggleKey, audienceId);
            return 0;
        }
        LogToggleNotFound(toggleKey);
        return 0;
    }
    #endregion

    #region 列表获取
    public List<string> GetAllToggleKeys()
    {
        return new(_toggles.Keys);
    }

    public List<string> GetAudienceIds(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Audiences.Select(a => a.Id).ToList();
        }
        LogToggleNotFound(toggleKey);
        return new();
    }

    public List<string> GetRuleIds(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                return audience.Rules.Select(r => r.Id).ToList();
            }
            LogAudienceNotFound(toggleKey, audienceId);
            return new();
        }
        LogToggleNotFound(toggleKey);
        return new();
    }
    #endregion

    #region 模型获取
    public Toggle GetToggle(string key)
    {
        if (_toggles.TryGetValue(key, out var toggle))
        {
            return toggle;
        }

        LogToggleNotFound(key);
        return new();
    }

    public Audience GetAudience(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                return audience;
            }

            LogAudienceNotFound(toggleKey, audienceId);
            return new();
        }

        LogToggleNotFound(toggleKey);
        return new();
    }

    public Rule GetRule(string toggleKey, string audienceId, string ruleId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                var rule = audience.Rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule != null)
                {
                    return rule;
                }

                LogRuleNotFound(toggleKey, audienceId, ruleId);
                return new();
            }

            LogAudienceNotFound(toggleKey, audienceId);
            return new();
        }

        LogToggleNotFound(toggleKey);
        return new();
    }
    #endregion

    #region 冷路径日志方法
    /// <summary>
    /// 请求的开关被禁用
    /// </summary>
    /// <param name="key"></param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogToggleDisabled(string key)
    {
        _logger.LogWarning("Toggle is disabled: {key}", key);
    }

    /// <summary>
    /// 请求的开关不存在
    /// </summary>
    /// <param name="key"></param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogToggleNotFound(string key)
    {
        _logger.LogWarning("Toggle not found: {key}", key);
    }

    /// <summary>
    /// 请求的群组不存在
    /// </summary>
    /// <param name="toggleKey"></param>
    /// <param name="audienceId"></param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogAudienceNotFound(string toggleKey, string audienceId)
    {
        _logger.LogWarning("Audience not found: {audienceId} in toggle: {toggleKey}", audienceId, toggleKey);
    }

    /// <summary>
    /// 请求的规则不存在
    /// </summary>
    /// <param name="toggleKey"></param>
    /// <param name="audienceId"></param>
    /// <param name="ruleId"></param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogRuleNotFound(string toggleKey, string audienceId, string ruleId)
    {
        _logger.LogWarning("Rule not found: {ruleId} in audience: {audienceId} of toggle: {toggleKey}", ruleId, audienceId, toggleKey);
    }

    /// <summary>
    /// 配置文件重新加载
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogFilesChanged()
    {
        _logger.LogInformation("Detected changes in toggle configuration. Reloading...");
    }
    #endregion
}
