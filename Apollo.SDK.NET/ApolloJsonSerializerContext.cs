using System.Text.Json.Serialization;

using Apollo.SDK.NET.Models;

namespace Apollo.SDK.NET;

/// <summary>
/// 源生成器序列化上下文
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Toggle))]
internal partial class ApolloJsonSerializerContext : JsonSerializerContext
{
}
