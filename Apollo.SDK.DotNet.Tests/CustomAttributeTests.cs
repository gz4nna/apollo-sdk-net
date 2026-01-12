using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet.Tests;

public class CustomAttributeTests
{
    private readonly RuleEvaluator _evaluator = new();

    [Fact]
    public void CustomAttributeTest()
    {
        var rule = new Rule
        {
            Attribute = "custom",
            CustomAttribute = "vip_level",
            Operator = "equals",
            Value = "9"
        };
        var context = new Dictionary<string, object> { { "vip_level", "9" } };

        Assert.True(_evaluator.Evaluate(rule, context));
    }
}
