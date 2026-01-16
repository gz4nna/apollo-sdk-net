using System.Text.Json.Serialization;

namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 开关
/// </summary>
public class Toggle
{
    /// <summary>
    /// 开关 Id
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 开关 Key
    /// </summary>
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    /// <summary>
    /// 开关 状态
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }

    /// <summary>
    /// 开关 群组集
    /// </summary>
    [JsonPropertyName("audiences")]
    public List<Audience> Audiences { get; set; } = [];

    public void Initialize()
    {
        Audiences?.ForEach(Audience =>
            Audience.Rules?.ForEach(Rule =>
                Rule.Prepare()
            )
        );
        Audiences?.ForEach(Audience =>
            Audience.Rules?.ForEach(Rule =>
                Rule.ToggleKey = Key
            )
        );
    }
}
