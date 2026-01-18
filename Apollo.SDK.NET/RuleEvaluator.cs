using Apollo.SDK.NET.Algorithms;
using Apollo.SDK.NET.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apollo.SDK.NET;

/// <summary>
/// 规则执行器
/// </summary>
public class RuleEvaluator
{
    private readonly Dictionary<string, Func<Rule, object, bool>> _operators;
    private readonly ILogger<RuleEvaluator> _logger;

    public RuleEvaluator(ILogger<RuleEvaluator>? logger = null)
    {
        _logger = logger ?? NullLogger<RuleEvaluator>.Instance;
        _logger.LogInformation("Initializing RuleEvaluator...");

        _operators = new Dictionary<string, Func<Rule, object, bool>>
        {
            { "equals", (rule, actVal) => actVal?.ToString() == rule.Value },
            { "not_equals", (rule, actVal) => actVal?.ToString() != rule.Value },
            { "gt", (rule, actVal) =>
                {
                    if(rule.GetParsedValue() is not double value)
                        return Convert.ToDouble(actVal) > Convert.ToDouble(rule.Value);
                    return Convert.ToDouble(actVal) > value;
                }
            },
            { "lt", (rule, actVal) =>
                {
                    if(rule.GetParsedValue() is not double value)
                        return Convert.ToDouble(actVal) < Convert.ToDouble(rule.Value);
                    return Convert.ToDouble(actVal) < value;
                }
            },
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
            }
        };
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="rule">规则</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合规则</returns>
    public bool Evaluate(Rule rule, ApolloContext context)
    {
        if (!context.TryGetValue(rule.EffectiveAttribute, out var actualValue))
        {
            _logger.LogError("Attribute {Attribute} not found in context.", rule.EffectiveAttribute);
            return false;
        }

        // 如果 EffectiveAttribute 是 traffic，则在这里做加盐哈希
        if (rule.EffectiveAttribute == "traffic")
        {
            // 用 开关Key_用户ID 做盐值
            string salt = $"{rule.ToggleKey}_{actualValue}";
            uint hash = MurmurHash3.Hash(salt);

            double bucket = hash % 100;
            actualValue = bucket;
        }

        if (_operators.TryGetValue(rule.Operator, out var func))
        {
            try
            {
                return func(rule, actualValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating rule {RuleId} with operator {Operator}.", rule.Id, rule.Operator);
                return false;
            }
        }
        _logger.LogError("Operator {Operator} not supported.", rule.Operator);
        return false;
    }
}
