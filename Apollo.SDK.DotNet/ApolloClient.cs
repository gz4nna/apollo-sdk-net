using System.Collections.Concurrent;
using System.Text.Json;

using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

public class ApolloClient
{
    private readonly ConcurrentDictionary<string, Toggle> _toggles = new();
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
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var toggle = JsonSerializer.Deserialize<Toggle>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

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
    /// <param name="context"></param>
    /// <returns></returns>
    public bool IsToggleAllow(string key, Dictionary<string, object> context)
    {
        if (!_toggles.TryGetValue(key, out var toggle) || toggle.Status != "enabled")
            return false;

        return toggle.Audiences.Any(audience =>
            audience.Rules.All(rule =>
                _evaluator.Evaluate(rule, context)
            )
        );
    }
}
