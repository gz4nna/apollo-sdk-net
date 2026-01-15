using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet.Tests;

/// <summary>
/// 自定义属性测试
/// </summary>
public class CustomAttributeTests
{
    private readonly RuleEvaluator _evaluator = new();

    /// <summary>
    /// 测试自定义属性规则评估
    /// </summary>
    [Fact]
    public void CustomAttributeTest()
    {
        var rule = new Rule
        {
            Id = "rule_custom_1",
            ToggleKey = "test_toggle",
            Attribute = "custom",
            CustomAttribute = "vip_level",
            Operator = "equals",
            Value = "9"
        };

        var context = new ApolloContext("user_123")
        {
            { "vip_level", "9" }
        };

        Assert.True(_evaluator.Evaluate(rule, context));
    }
}
