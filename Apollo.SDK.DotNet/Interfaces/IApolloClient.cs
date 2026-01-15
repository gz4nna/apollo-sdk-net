namespace Apollo.SDK.DotNet.Interfaces;

public interface IApolloClient
{
    /// <summary>
    /// 判断是否符合
    /// </summary>
    /// <param name="key">开关 Key</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合开关条件</returns>
    bool IsToggleAllowed(string key, ApolloContext context);
}
