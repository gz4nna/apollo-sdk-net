using System.Text.Json.Serialization;

namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 规则
/// </summary>
public class Rule
{
    /// <summary>
    /// 规则 Id
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 规则 属性
    /// </summary>
    [JsonPropertyName("attribute")]
    public required string Attribute { get; set; }

    /// <summary>
    /// 规则 操作符
    /// </summary>
    [JsonPropertyName("operator")]
    public required string Operator { get; set; }

    /// <summary>
    /// 规则 值
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    /// <summary>
    /// 规则 自定义属性
    /// </summary>
    [JsonPropertyName("customAttribute")]
    public string? CustomAttribute { get; set; }

    /// <summary>
    /// 规则 真实属性
    /// </summary>
    public string EffectiveAttribute => Attribute == "custom" ? CustomAttribute ?? "" : Attribute;

    /// <summary>
    /// 开关 Key  可缺失
    /// </summary>
#pragma warning disable CS8618
    public string ToggleKey { get; set; }
#pragma warning restore CS8618 

    /// <summary>
    /// 预编译缓存
    /// </summary>
    private object? _parsedValue;

    /// <summary>
    /// 预编译方法 加速 in 和 between
    /// </summary>
    public void Prepare()
    {
        if (string.IsNullOrEmpty(Value)) return;

        switch (Operator)
        {
            case "between":
                var parts = Value.Split(',');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out double min) &&
                    double.TryParse(parts[1], out double max))
                {
                    // 区间元组
                    _parsedValue = (min, max);
                }
                break;
            case "in":
                var set = new HashSet<string>(
                    Value.Split(',').Select(x => x.Trim()),
                    StringComparer.OrdinalIgnoreCase
                );
                // 哈希集合
                _parsedValue = set;
                break;
            case "traffic":
                // 直接缓存百分比数值
                if (double.TryParse(Value, out double percentValue))
                {
                    _parsedValue = percentValue;
                }
                break;
        }
    }

    /// <summary>
    /// 获取预编译缓存
    /// </summary>
    /// <returns></returns>
    public object? GetParsedValue() => _parsedValue;
}
