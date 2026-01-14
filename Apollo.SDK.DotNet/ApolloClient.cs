using System.Collections.Concurrent;
using System.Text.Json;

using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

/// <summary>
/// Apollo 客户端，用于管理开关配置和评估规则
/// </summary>
public class ApolloClient
{
    #region 核心成员
    // 存储开关配置
    private readonly ConcurrentDictionary<string, Toggle> _toggles = new();
    // 规则执行器
    private readonly RuleEvaluator _evaluator = new();
    #endregion

    #region 初始化
    /// <summary>
    /// 加载开关配置文件
    /// </summary>
    /// <param name="directoryPath">开关配置文件路径</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public void SetTogglesPath(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Path not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath, "*.json");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var toggle = JsonSerializer.Deserialize<Toggle>(content, options);

            // 预处理 between
            toggle?.Initialize();

            if (toggle?.Key != null)
            {
                _toggles[toggle.Key] = toggle;
            }
        }
    }
    #endregion

    #region 核心方法
    /// <summary>
    /// 判断是否符合
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <param name="userId">用户ID</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合开关条件</returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    public bool IsToggleAllow(string key, string userId, Dictionary<string, object> context)
    {
        if (!_toggles.TryGetValue(key, out var toggle))
            throw new KeyNotFoundException($"Toggle not found: {key}");
        if (toggle.Status != "enabled")
            return true;

        // 添加 userId 到上下文
        context["userId"] = userId;
        // 同时 traffic 使用的也是 userId
        if (!context.ContainsKey("traffic"))
        {
            context["traffic"] = userId;
        }

        return toggle.Audiences.Any(audience =>
            audience.Rules.All(rule =>
                _evaluator.Evaluate(rule, context)
            )
        );
    }
    #endregion

    #region 存在性检查
    /// <summary>
    /// 判断开关是否存在
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    public bool IsToggleExist(string key)
    {
        return _toggles.ContainsKey(key);
    }

    /// <summary>
    /// 判断群组是否存在
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    public bool IsAudienceExist(string toggleKey, string audienceId)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Audiences.Any(a => a.Id == audienceId);
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }

    /// <summary>
    /// 判断规则是否存在
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <param name="ruleId">规则 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关或群组不存在时抛出</exception>
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
    /// <summary>
    /// 获取开关状态
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
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
    /// <summary>
    /// 获取开关数量
    /// </summary>
    /// <returns></returns>
    public int GetToggleCount()
    {
        return _toggles.Count;
    }

    /// <summary>
    /// 获取群组数量
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    public int GetAudienceCount(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return toggle.Audiences.Count;
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }

    /// <summary>
    /// 获取规则数量
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关或群组不存在时抛出</exception>
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
    /// <summary>
    /// 获取所有开关 Key
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllToggleKeys()
    {
        return [.. _toggles.Keys];
    }

    /// <summary>
    /// 获取指定开关的所有群组 ID
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    public List<string> GetAudienceIds(string toggleKey)
    {
        if (_toggles.TryGetValue(toggleKey, out var toggle))
        {
            return [.. toggle.Audiences.Select(a => a.Id)];
        }
        throw new KeyNotFoundException($"Toggle not found: {toggleKey}");
    }

    /// <summary>
    /// 获取指定开关和群组的所有规则 ID
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关或群组不存在时抛出</exception>
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
    /// <summary>
    /// 获取指定开关模型
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关不存在时抛出</exception>
    public Toggle GetToggle(string key)
    {
        if (_toggles.TryGetValue(key, out var toggle))
        {
            return toggle;
        }
        throw new KeyNotFoundException($"Toggle not found: {key}");
    }

    /// <summary>
    /// 获取指定开关和群组模型
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关或群组不存在时抛出</exception>
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

    /// <summary>
    /// 获取指定开关、群组和规则模型
    /// </summary>
    /// <param name="toggleKey">开关 Key</param>
    /// <param name="audienceId">群组 ID</param>
    /// <param name="ruleId">规则 ID</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">当开关、群组或规则不存在时抛出</exception>
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
