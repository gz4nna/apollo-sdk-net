using Apollo.SDK.NET.Models;

namespace Apollo.SDK.NET.Tests;

/// <summary>
/// 自定义属性测试
/// </summary>
public class CustomAttributeTests
{
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
            .Set("vip_level", "9");

        Assert.True(RuleEvaluator.Evaluate(rule, context));
    }
}
