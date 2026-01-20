namespace Apollo.SDK.NET;

/// <summary>
/// 用户输入上下文
/// </summary>
public class ApolloContext
{
    private readonly Dictionary<string, ApolloValue> _items = new(StringComparer.OrdinalIgnoreCase);

    public string UserId { get; }

    public ApolloContext(string userId)
    {
        UserId = userId;
        _items["user_id"] = userId;
        _items["traffic"] = userId;
    }

    public ApolloContext Set(string key, ApolloValue value)
    {
        _items[key] = value;
        return this;
    }

    public bool ContainsKey(string key)
        => _items.ContainsKey(key);

    public bool TryGetValue(string key, out ApolloValue value)
        => _items.TryGetValue(key, out value);

    public ApolloValue this[string key] => _items[key];
}

public readonly struct ApolloValue
{
    private readonly string? _stringValue;
    private readonly double _numberValue;
    public readonly ValueType Type;

    public enum ValueType { Empty, String, Number }

    public ApolloValue(string value)
    {
        _stringValue = value;
        _numberValue = 0;
        Type = ValueType.String;
    }

    public ApolloValue(double value)
    {
        _stringValue = null;
        _numberValue = value;
        Type = ValueType.Number;
    }

    public static implicit operator ApolloValue(string v) => new(v);
    public static implicit operator ApolloValue(int v) => new((double)v);
    public static implicit operator ApolloValue(double v) => new(v);

    public string AsString() => _stringValue ?? _numberValue.ToString();
    public double AsNumber() => _numberValue;
}
