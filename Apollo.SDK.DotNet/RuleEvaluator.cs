using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

/// <summary>
/// 规则执行器
/// </summary>
public class RuleEvaluator
{
    private readonly Dictionary<string, Func<string, object, bool>> _operators;

    public RuleEvaluator()
    {
        _operators = new Dictionary<string, Func<string, object, bool>>
        {
            { "equals", (refVal, actVal) => actVal?.ToString() == refVal },
            { "not_equals", (refVal, actVal) => actVal?.ToString() != refVal },
            { "gt", (refVal, actVal) => Convert.ToDouble(actVal) > Convert.ToDouble(refVal) },
            { "lt", (refVal, actVal) => Convert.ToDouble(actVal) < Convert.ToDouble(refVal) },
            { "contains", (refVal, actVal) => actVal?.ToString()?.Contains(refVal) ?? false },
            { "in", (refVal, actVal) => refVal.Split(',').Select(x => x.Trim()).Contains(actVal?.ToString()) },
            { "between", (refVal, actVal) =>
                {
                    var parts = refVal.Split(',');
                    if (parts.Length != 2) return false;

                    if (double.TryParse(parts[0], out double min) &&
                        double.TryParse(parts[1], out double max) &&
                        double.TryParse(actVal?.ToString(), out double actual))
                    {
                        return actual >= min && actual <= max;
                    }
                    return false;
                }
            }
        };
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="rule">规则</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Evaluate(Rule rule, Dictionary<string, object> context)
    {
        if (!context.TryGetValue(rule.EffectiveAttribute, out var actualValue))
            return false;

        if (_operators.TryGetValue(rule.Operator, out var func))
        {
            try
            {
                return func(rule.Value, actualValue);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
}
