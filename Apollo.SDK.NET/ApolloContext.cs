namespace Apollo.SDK.NET;

/// <summary>
/// 用户输入上下文
/// </summary>
public class ApolloContext : Dictionary<string, object>
{
    public ApolloContext(string userId)
    {
        this["user_id"] = userId;
    }

    public ApolloContext Set(string attribute, string value)
    {
        this[attribute] = value;
        return this;
    }
}
