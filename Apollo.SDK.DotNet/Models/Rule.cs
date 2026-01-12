namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 规则
/// </summary>
public class Rule
{
    /// <summary>
    /// 规则 Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 规则 属性
    /// </summary>
    public string Attribute { get; set; }

    /// <summary>
    /// 规则 操作符
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// 规则 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 规则 自定义属性
    /// </summary>
    public string? CustomAttribute { get; set; }

    /// <summary>
    /// 规则 真实属性
    /// </summary>
    public string EffectiveAttribute => Attribute == "custom" ? CustomAttribute ?? "" : Attribute;
}
