namespace Apollo.SDK.DotNet;

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
