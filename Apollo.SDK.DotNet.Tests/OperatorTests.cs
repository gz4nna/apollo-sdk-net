using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet.Tests;

public class OperatorTests
{
    private readonly RuleEvaluator _evaluator = new();

    [Theory]
    #region equals
    [InlineData("equals", "100", "100", true)]
    [InlineData("equals", "100", "101", false)]
    #endregion

    #region not_equals
    [InlineData("not_equals", "A", "B", true)]
    [InlineData("not_equals", "A", "A", false)]
    #endregion

    #region contains
    [InlineData("contains", "apple", "I like apple", true)]
    #endregion

    #region in
    [InlineData("in", "Beijing,Shanghai,Guangzhou", "Beijing", true)]
    [InlineData("in", "Beijing,Shanghai", "Tokyo", false)]
    #endregion

    #region gt
    [InlineData("gt", "30", 20, false)]
    [InlineData("gt", "30", 30, false)]
    [InlineData("gt", "30", 40, true)]
    #endregion

    #region lt
    [InlineData("lt", "30", 20, true)]
    [InlineData("lt", "30", 30, false)]
    [InlineData("lt", "30", 40, false)]
    #endregion

    #region between
    [InlineData("between", "0,10", 5, true)]
    [InlineData("between", "0,10", 0, true)]
    [InlineData("between", "0,10", 10, true)]
    [InlineData("between", "0,10", 11, false)]
    [InlineData("between", "0,10", -1, false)]
    #endregion

    public void OperatorTest(string op, string configVal, object userVal, bool expected)
    {
        var rule = new Rule
        {
            ToggleKey = "test_toggle",
            Id = "rule_1",
            Operator = op,
            Value = configVal,
            Attribute = "test_key"
        };
        rule.Prepare();

        var context = new Dictionary<string, object> { { "test_key", userVal } };

        var result = _evaluator.Evaluate(rule, context);

        Assert.Equal(expected, result);
    }
}
