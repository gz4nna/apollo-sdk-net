using System.Text.Json.Serialization;

namespace Apollo.SDK.NET.Models;

/// <summary>
/// 开关
/// </summary>
public class Toggle
{
    /// <summary>
    /// 开关 Id
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 开关名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 开关 Key
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// 开关描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 开关 状态
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// 开关 创建时间
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 开关 更新时间
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 开关 群组集
    /// </summary>
    [JsonPropertyName("audiences")]
    public List<Audience> Audiences { get; set; } = new();

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
