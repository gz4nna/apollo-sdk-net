using System.Collections.Concurrent;
using System.Text.Json;

using Apollo.SDK.DotNet.Interfaces;
using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

/// <summary>
/// Apollo 客户端，用于管理开关配置和评估规则
/// </summary>
public class ApolloClient : IApolloClient
{
    #region 核心成员
    // 存储开关配置
    private readonly ConcurrentDictionary<string, Toggle> _toggles = new();
    // 规则执行器
    private readonly RuleEvaluator _evaluator = new();
    // 文件系统监视器
    private readonly FileSystemWatcher _watcher;
    #endregion

    #region 初始化
    /// <summary>
    /// 构造时加载配置
    /// </summary>
    /// <param name="options">客户端配置</param>
    public ApolloClient(ApolloOptions options)
    {
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
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Path not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath, "*.json");
        if (files.Length == 0)
            throw new FileNotFoundException($"File not found: {directoryPath}");

        // var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                var toggle = JsonSerializer.Deserialize(content, ApolloJsonSerializerContext.Default.Toggle);

                // 预处理 between
                toggle?.Initialize();

                if (toggle?.Key != null)
                {
                    _toggles[toggle.Key] = toggle;
                }
            }
            catch (Exception ex)
            {
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
            Console.WriteLine($"Toggle not found: {key}");
            return false;
        }
        if (toggle.Status != "enabled")
            return false;

        // 添加 userId 到上下文
        if (!context.ContainsKey("traffic"))
        {
            context["traffic"] = context["user_id"];
        }

        return toggle.Audiences.Any(audience =>
            audience.Rules.All(rule =>
                _evaluator.Evaluate(rule, context)
            )
        );
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
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
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
            throw new KeyNotFoundException($"Audience not found: {audienceId}");
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }
    #endregion

    #region 状态检查    
    public bool GetToggleStatus(string key)
    {
        if (_toggles.TryGetValue(key, out var toggle))
        {
            return toggle.Status == "enabled";
        }
        throw new KeyNotFoundException($"Toggle not found: {key}");
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
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
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
            throw new KeyNotFoundException($"Audience not found: {audienceId}");
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }
    #endregion

    #region 列表获取
    public List<string> GetAllToggleKeys()
    {
        return [.. _toggles.Keys];
    }

    public List<string> GetAudienceIds(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return [.. toggle.Audiences.Select(a => a.Id)];
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }

    public List<string> GetRuleIds(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            var audience = toggle.Audiences.FirstOrDefault(a => a.Id == audienceId);
            if (audience != null)
            {
                return [.. audience.Rules.Select(r => r.Id)];
            }
            throw new KeyNotFoundException($"Audience not found: {audienceId}");
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }
    #endregion

    #region 模型获取
    public Toggle GetToggle(string key)
    {
        if (_toggles.TryGetValue(key, out var toggle))
        {
            return toggle;
        }
        throw new KeyNotFoundException($"Toggle not found: {key}");
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
            throw new KeyNotFoundException($"Audience not found: {audienceId}");
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
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
                throw new KeyNotFoundException($"Rule not found: {ruleId}");
            }
            throw new KeyNotFoundException($"Audience not found: {audienceId}");
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }
    #endregion
}
