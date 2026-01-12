namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 开关
/// </summary>
public class Toggle
{
    /// <summary>
    /// 开关 Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 开关 Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 开关 状态
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 开关 群组集
    /// </summary>
    public List<Audience> Audiences { get; set; } = [];
}
