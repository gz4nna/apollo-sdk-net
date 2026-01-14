using Apollo.SDK.DotNet.Models;

namespace Apollo.SDK.DotNet;

/// <summary>
/// 规则执行器
/// </summary>
public class RuleEvaluator
{
    private readonly Dictionary<string, Func<Rule, object, bool>> _operators;

    public RuleEvaluator()
    {
        _operators = new Dictionary<string, Func<Rule, object, bool>>
        {
            { "equals", (rule, actVal) => actVal?.ToString() == rule.Value },
            { "not_equals", (rule, actVal) => actVal?.ToString() != rule.Value },
            { "gt", (rule, actVal) => Convert.ToDouble(actVal) > Convert.ToDouble(rule.Value) },
            { "lt", (rule, actVal) => Convert.ToDouble(actVal) < Convert.ToDouble(rule.Value) },
            { "contains", (rule, actVal) => actVal?.ToString()?.Contains(rule.Value) ?? false },
            { "in", (rule, actVal) =>
                {
                    // 降级
                    if (rule.GetParsedValue() is not HashSet<string> set)
                        return rule.Value.Split(',').Select(x => x.Trim()).Contains(actVal?.ToString());
                    return set.Contains(actVal?.ToString() ?? string.Empty);
                }
            },
            { "between", (rule, actVal) =>
                {
                    if (!double.TryParse(actVal?.ToString(), out double actual))
                        return false;
                    // 降级
                    if(rule.GetParsedValue() is not ValueTuple<double, double> range)
                    {
                        var parts = rule.Value.Split(',');
                        if (parts.Length != 2) return false;

                        if (double.TryParse(parts[0], out double min) &&
                            double.TryParse(parts[1], out double max) )
                        {
                            return actual >= min && actual <= max;
                        }
                        return false;
                    }
                    return actual >= range.Item1 && actual <= range.Item2;
                }
            },
            { "traffic", (rule, actVal) =>
                {
                    // 降级
                    if (rule.GetParsedValue() is not double percentValue)
                        if (!double.TryParse(rule.Value, out percentValue))
                            return false;

                    // 用 开关Key_用户ID 做盐值
                    string salt = $"{rule.ToggleKey}_{actVal}";
                    uint hash = MurmurHash3.Hash(salt);
                    double bucket = hash % 100;

                    return bucket < percentValue;
                }
            }
        };
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="rule">规则</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合规则</returns>
    public bool Evaluate(Rule rule, Dictionary<string, object> context)
    {
        if (!context.TryGetValue(rule.EffectiveAttribute, out var actualValue))
            return false;

        if (_operators.TryGetValue(rule.Operator, out var func))
        {
            try
            {
                return func(rule, actualValue);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
}
