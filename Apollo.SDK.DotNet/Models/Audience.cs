using System.Diagnostics.CodeAnalysis;

namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 群组
/// </summary>
public class Audience
{
    /// <summary>
    /// 群组 Id
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 群组 名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 群组 规则集
    /// </summary>
    public List<Rule> Rules { get; set; } = [];
}
