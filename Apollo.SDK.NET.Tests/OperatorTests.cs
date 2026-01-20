using Apollo.SDK.NET.Models;

namespace Apollo.SDK.NET.Tests;

public class OperatorTests
{
    /// <summary>
    /// 测试各种操作符的规则评估
    /// </summary>
    /// <param name="op">操作符</param>
    /// <param name="configVal">配置值</param>
    /// <param name="userVal">用户值</param>
    /// <param name="expected">期望结果</param>
    [Theory]
    [MemberData(nameof(GetOperatorTestData))]
    public void OperatorTest(string op, string configVal, ApolloValue userVal, bool expected)
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

        var context = new ApolloContext("user_123")
            .Set("test_key", userVal);

        var result = RuleEvaluator.Evaluate(rule, context);

        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> GetOperatorTestData()
    {
        #region equals
        yield return new object[] { "equals", "100", (ApolloValue)"100", true };
        yield return new object[] { "equals", "100", (ApolloValue)"101", false };
        #endregion

        #region not_equals
        yield return new object[] { "not_equals", "A", (ApolloValue)"B", true };
        yield return new object[] { "not_equals", "A", (ApolloValue)"A", false };
        #endregion

        #region contains
        yield return new object[] { "contains", "apple", (ApolloValue)"I like apple", true };
        #endregion

        #region in
        yield return new object[] { "in", "Beijing,Shanghai,Guangzhou", (ApolloValue)"Beijing", true };
        yield return new object[] { "in", "Beijing,Shanghai", (ApolloValue)"Tokyo", false };
        #endregion

        #region gt
        yield return new object[] { "gt", "30", (ApolloValue)20, false };
        yield return new object[] { "gt", "30", (ApolloValue)30, false };
        yield return new object[] { "gt", "30", (ApolloValue)40, true };
        yield return new object[] { "gt", "30.0", (ApolloValue)30.00000000000001, true };
        yield return new object[] { "gt", "30.00000000000001", (ApolloValue)30.000000, false };
        #endregion

        #region lt
        yield return new object[] { "lt", "30", (ApolloValue)20, true };
        yield return new object[] { "lt", "30", (ApolloValue)30, false };
        yield return new object[] { "lt", "30", (ApolloValue)40, false };
        #endregion

        #region between
        yield return new object[] { "between", "0,10", (ApolloValue)5, true };
        yield return new object[] { "between", "0,10", (ApolloValue)0, true };
        yield return new object[] { "between", "0,10", (ApolloValue)10, true };
        yield return new object[] { "between", "0,10", (ApolloValue)11, false };
        yield return new object[] { "between", "0,10", (ApolloValue)(-1), false };
        #endregion
    }
}
