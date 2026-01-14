namespace Apollo.SDK.DotNet.Models;

/// <summary>
/// 开关
/// </summary>
public class Toggle
{
    /// <summary>
    /// 开关 Id
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 开关 Key
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// 开关 状态
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// 开关 群组集
    /// </summary>
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
