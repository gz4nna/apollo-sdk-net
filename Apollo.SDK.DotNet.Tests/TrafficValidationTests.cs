using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet.Tests;

public class TrafficValidationTests
{
    [Fact]
    public void TrafficValidationTest()
    {
        var rule = new Rule
        {
            Id = "rule_traffic_1",
            Attribute = "traffic",
            Operator = "traffic",
            Value = "30",
            ToggleKey = "test_toggle"
        };

        rule.Prepare();

        var evaluator = new RuleEvaluator();
        int hitCount = 0;
        int totalUserCount = 10000;

        for (int i = 0; i < totalUserCount; i++)
        {
            var userId = $"user_{i}";
            var context = new Dictionary<string, object> { { "traffic", userId } };
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