using System.Runtime.CompilerServices;

using Apollo.SDK.NET.Algorithms;
using Apollo.SDK.NET.Models;

using Microsoft.Extensions.Logging;

namespace Apollo.SDK.NET;

/// <summary>
/// 规则执行器
/// </summary>
public static class RuleEvaluator
{
    // 使用静态强类型扩展进行外部关联
    internal static ILogger? Logger;
    // IsLogEnabled 用来减少检查
    internal static bool IsLogEnabled = false;

    // 以前的实现
    //private readonly Dictionary<string, Func<Rule, ApolloValue, bool>> _operators;
    //public RuleEvaluator(ILogger<RuleEvaluator>? logger = null)
    //{
    //    _logger = logger ?? NullLogger<RuleEvaluator>.Instance;
    //    _logger.LogInformation("Initializing RuleEvaluator...");

    //    _operators = new Dictionary<string, Func<Rule, ApolloValue, bool>>
    //    {
    //        { "equals", (rule, actVal) => actVal.AsString() == rule.Value },
    //        { "not_equals", (rule, actVal) => actVal.AsString() != rule.Value },
    //        { "gt", (rule, actVal) =>
    //            {
    //                if(rule.GetParsedValue() is not double value)
    //                    return actVal.AsNumber() > Convert.ToDouble(rule.Value);
    //                return actVal.AsNumber() > value;
    //            }
    //        },
    //        { "lt", (rule, actVal) =>
    //            {
    //                if(rule.GetParsedValue() is not double value)
    //                    return actVal.AsNumber() < Convert.ToDouble(rule.Value);
    //                return actVal.AsNumber() < value;
    //            }
    //        },
    //        { "contains", (rule, actVal) => actVal.AsString()?.Contains(rule.Value) ?? false },
    //        { "in", (rule, actVal) =>
    //            {
    //                // 降级
    //                if (rule.GetParsedValue() is not HashSet<string> set)
    //                    return rule.Value.Split(',').Select(x => x.Trim()).Contains(actVal.AsString());
    //                return set.Contains(actVal.AsString() ?? string.Empty);
    //            }
    //        },
    //        { "between", (rule, actVal) =>
    //            {
    //                if (!double.TryParse(actVal.AsString(), out double actual))
    //                    return false;
    //                // 降级
    //                if(rule.GetParsedValue() is not ValueTuple<double, double> range)
    //                {
    //                    var parts = rule.Value.Split(',');
    //                    if (parts.Length != 2) return false;

    //                    if (double.TryParse(parts[0], out double min) &&
    //                        double.TryParse(parts[1], out double max) )
    //                    {
    //                        return actual >= min && actual <= max;
    //                    }
    //                    return false;
    //                }
    //                return actual >= range.Item1 && actual <= range.Item2;
    //            }
    //        }
    //    };
    //}

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="rule">规则</param>
    /// <param name="context">上下文参数</param>
    /// <returns>是否符合规则</returns>
    public static bool Evaluate(Rule rule, ApolloContext context)
    {
        if (!context.TryGetValue(rule.EffectiveAttribute, out var actualValue))
        {
            if (IsLogEnabled) LogAttributeNotFound(rule.EffectiveAttribute);
            return false;
        }

        // 如果 EffectiveAttribute 是 traffic，则在这里做加盐哈希
        if (rule.EffectiveAttribute == "traffic")
        {
            // 用 开关Key_用户ID 做盐值
            // 假设 ToggleKey 和 UserId 长度不会超过 256
            Span<char> saltBuffer = stackalloc char[256];
            int written = 0;

            rule.ToggleKey.AsSpan().CopyTo(saltBuffer);
            written += rule.ToggleKey.Length;

            saltBuffer[written++] = '_';

            // 这里可以确保 userid 为 string
            actualValue.AsString().AsSpan().CopyTo(saltBuffer.Slice(written));
            written += actualValue.AsString().Length;

            uint hash = MurmurHash3.Hash(saltBuffer.Slice(0, written));
            actualValue = new ApolloValue(hash % 100);

            // 以前的实现：
            //string salt = $"{rule.ToggleKey}_{actualValue.AsString()}";
            //uint hash = MurmurHash3.Hash(salt);

            //double bucket = hash % 100;
            //actualValue = bucket;
        }

        return ExecuteOperator(rule, actualValue);

        //if (_operators.TryGetValue(rule.Operator, out var func))
        //{
        //    try
        //    {                
        //        return func(rule, actualValue);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error evaluating rule {RuleId} with operator {Operator}.", rule.Id, rule.Operator);
        //        return false;
        //    }
        //}
        //_logger.LogError("Operator {Operator} not supported.", rule.Operator);
        //return false;
    }

    /// <summary>
    /// 匹配具体方法
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="actVal"></param>
    /// <returns></returns>
    private static bool ExecuteOperator(Rule rule, ApolloValue actVal)
    {
        return rule.Operator switch
        {
            "equals" => actVal.AsString() == rule.Value,
            "not_equals" => actVal.AsString() != rule.Value,
            "gt" => EvaluateNumeric(rule, actVal),
            "lt" => EvaluateNumeric(rule, actVal),
            "contains" => actVal.AsString()?.Contains(rule.Value, StringComparison.OrdinalIgnoreCase) ?? false,
            "in" => EvaluateIn(rule, actVal),
            "between" => EvaluateBetween(rule, actVal),
            _ => HandleUnknownOperator(rule.Operator)
        };
    }

    /// <summary>
    /// 大于小于
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="actVal"></param>
    /// <returns></returns>
    private static bool EvaluateNumeric(Rule rule, ApolloValue actVal)
    {
        double target = rule.GetParsedValue() is double d ? d : Convert.ToDouble(rule.Value);
        double actual = actVal.AsNumber();

        return rule.Operator == "gt" ? actual > target : actual < target;
    }

    /// <summary>
    /// 包含
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="actVal"></param>
    /// <returns></returns>
    private static bool EvaluateIn(Rule rule, ApolloValue actVal)
    {
        var actual = actVal.AsString();
        if (actual == null) return false;

        if (rule.GetParsedValue() is HashSet<string> set)
            return set.Contains(actual);

        // 确保初始化则不需要降级处理的逻辑
        return false;
    }

    /// <summary>
    /// 区间
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="actVal"></param>
    /// <returns></returns>
    private static bool EvaluateBetween(Rule rule, ApolloValue actVal)
    {
        double actual = actVal.AsNumber();

        if (rule.GetParsedValue() is ValueTuple<double, double> range)
            return actual >= range.Item1 && actual <= range.Item2;

        return false;
    }

    /// <summary>
    /// 找不到操作符
    /// </summary>
    /// <param name="operatorName"></param>
    /// <returns></returns>
    private static bool HandleUnknownOperator(string operatorName)
    {
        if (IsLogEnabled) LogOperatorNotValid(operatorName);
        return false;
    }

    #region 冷路径日志方法
    // 调用前有检查，这里不用管空值判断
#pragma warning disable CS8604 // 引用类型参数可能为 null。
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LogAttributeNotFound(string attr)
    {
        Logger.LogError("Attribute {Attribute} not found in context.", attr);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LogOperatorNotValid(string operatorName)
    {
        Logger.LogError("Operator {Operator} not supported.", operatorName);
    }
#pragma warning restore CS8604 // 引用类型参数可能为 null。
    #endregion
}
