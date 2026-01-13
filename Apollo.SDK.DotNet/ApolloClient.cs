using System.Collections.Concurrent;
using System.Text.Json;

using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

public class ApolloClient
{
    // 存储开关配置
    private readonly ConcurrentDictionary<string, Toggle> _toggles = new();
    // 规则执行器
    private readonly RuleEvaluator _evaluator = new();

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

    /// <summary>
    /// 判断是否符合
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <param name="userId">用户ID</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合开关条件</returns>
    public bool IsToggleAllow(string key, string userId, Dictionary<string, object> context)
    {
        if (!_toggles.TryGetValue(key, out var toggle) || toggle.Status != "enabled")
            return false;

        // 添加 userId 到上下文
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
}
