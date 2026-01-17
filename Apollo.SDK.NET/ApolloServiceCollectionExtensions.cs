using Apollo.SDK.NET;
using Apollo.SDK.NET.Interfaces;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 提供 Apollo 服务的扩展方法，用于将 Apollo 客户端添加到依赖注入容器中。
/// </summary>
public static class ApolloServiceCollectionExtensions
{
    /// <summary>
    /// 注入 Apollo 客户端服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddApollo(this IServiceCollection services, ApolloOptions configureOptions)
    {
        _ = services.AddSingleton<IApolloClient>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                return new ApolloClient(configureOptions, loggerFactory);
            }
        );

        return services;
    }
}
