using System.Text.Json.Serialization;

namespace Apollo.SDK.NET.Models;

/// <summary>
/// 群组
/// </summary>
public class Audience
{
    /// <summary>
    /// 群组 Id
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 群组 名称
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// 群组 规则集
    /// </summary>
    [JsonPropertyName("rules")]
    public List<Rule> Rules { get; set; } = [];
}
