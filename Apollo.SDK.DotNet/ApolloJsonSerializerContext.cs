using System.Text.Json.Serialization;

using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

/// <summary>
/// 源生成器序列化上下文
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Toggle))]
internal partial class ApolloJsonSerializerContext : JsonSerializerContext
{
}
