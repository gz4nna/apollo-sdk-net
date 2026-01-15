using Apollo.SDK.DotNet;
using Apollo.SDK.DotNet.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 提供 Apollo 服务的扩展方法，用于将 Apollo 客户端添加到依赖注入容器中。
/// </summary>
public static class ApolloServiceCollectionExtensions
{
    public static IServiceCollection AddApollo(this IServiceCollection services, Action<ApolloOptions> configureOptions)
    {
        var options = new ApolloOptions();
        configureOptions(options);

        _ = services.AddSingleton<IApolloClient>(serviceProvider => new ApolloClient(options));

        return services;
    }
}
