using Apollo.SDK.NET.Benchmarks;

using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<ApolloClientBenchmark>();