using Apollo.SDK.NET.Interfaces;

using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.Logging.Abstractions;

namespace Apollo.SDK.NET.Benchmarks;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ApolloClientBenchmark
{
    private IApolloClient _client;
    private ApolloContext _context;

    [GlobalSetup]
    public void Setup()
    {
        var json = @"
        [
            {
                ""id"": ""tg_1"",
                ""key"": ""promo_feature"",
                ""status"": ""enabled"",
                ""audiences"": [
                    {
                        ""rules"": [
                            { ""attribute"": ""city"", ""operator"": ""eq"", ""values"": [""Beijing""] },
                            { ""attribute"": ""age"", ""operator"": ""gt"", ""values"": [""18""] },
                            { ""attribute"": ""traffic"", ""operator"": ""lt"", ""values"": [""50""] }
                        ]
                    }
                ]
            }
        ]";

        var options = new ApolloOptions { TogglesPath = "./" };
        _client = new ApolloClient(options, NullLoggerFactory.Instance);


        _context = new ApolloContext("user_50")
            .Set("city", "Beijing")
            .Set("age", "25");
    }

    [Benchmark]
    public bool IsToggleAllowed_ComplexRules()
    {
        return _client.IsToggleAllowed("promo_feature", _context);
    }
}
