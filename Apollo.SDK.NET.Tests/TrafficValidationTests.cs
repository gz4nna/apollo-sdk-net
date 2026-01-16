using Apollo.SDK.NET.Models;

namespace Apollo.SDK.NET.Tests;

/// <summary>
/// 流量验证测试
/// </summary>
public class TrafficValidationTests
{
    /// <summary>
    /// 测试流量分配是否符合预期
    /// </summary>
    [Fact]
    public void TrafficValidationTest()
    {
        var rule = new Rule
        {
            Id = "rule_traffic_1",
            Attribute = "traffic",
            Operator = "lt",
            Value = "30",
            ToggleKey = "test_toggle"
        };

        rule.Prepare();

        var evaluator = new RuleEvaluator();
        int hitCount = 0;
        int totalUserCount = 100000;

        for (int i = 0; i < totalUserCount; i++)
        {
            var userId = $"user_{i}";
            var context = new ApolloContext(userId) { { "traffic", userId } };
            if (evaluator.Evaluate(rule, context))
            {
                hitCount++;
            }
        }

        double hitRate = (double)hitCount / totalUserCount * 100;
        // 允许一定的误差范围
        Assert.InRange(hitRate, 29.0, 31.0);
    }
}