# apollo-sdk-net

如果你用了 [ricejson/apollo-frontend](https://github.com/ricejson/apollo-frontend) 来实现你的灰度发布，那么可以使用这款 SDK 来完成在 .NET 中的配置解析和人群校验。

目前已可支持：
- 依赖注入
- 自定义日志接入
- AOT 模式
- `.NET6` / `.NET8` / `.NET10`

## 如何使用

`ApolloClient` 接受两个参数，一个是配置 `ApolloOptions`，一个是可选的 `ILoggerFactory`。

如果你的项目支持通过 `IServiceCollection` 注入，那么可以直接在服务中注册：

```csharp
// 日志服务

builder.Services.AddApollo(new()
{
    TogglesPath = Path.Combine(Environment.CurrentDirectory, "toggles")
});

// 其他服务
```

在扩展方法中会通过 `IServiceProvider` 去寻找已注册的日志仓库，所以请确保日志的注册在该注册之前！

可以通过依赖注入或者直接获取来拿到 `IApolloClient`，判断人群的方法是 `IsToggleAllowed`，接受一个 开关配置文件的 key 和一个 待判断的上下文，该类型 `ApolloContext` 实际上继承了 `Dictionary<string, object>` ：

```csharp
var apolloClient = app.Services.GetService<IApolloClient>();

var context = new ApolloContext("user_123")
        .Set("city", "Beijing");

var toggleKey = "smart_recommender_v2";

bool? enabled = apolloClient?.IsToggleAllowed(toggleKey, context);
```

如果你的项目不支持依赖注入，当然也可以直接创建一个实例，像这样子去使用：

```csharp
ApolloClient client = new(new ApolloOptions
{
    TogglesPath = Path.Combine(Environment.CurrentDirectory, "toggles")
});

var context = new ApolloContext("user_123")
    .Set("city", "Beijing");

if (client.IsToggleAllowed("smart_recommender_v2", context))
{
    // 校验通过的逻辑
}
```

## TODO

- ~~预编译优化~~
- ~~支持依赖注入~~
- ~~兼容 AOT 模式~~
- ~~支持日志接入~~
- ~~.NET 版本兼容~~
- 支持远程判断
- 内存分配优化
- 异步流处理优化
