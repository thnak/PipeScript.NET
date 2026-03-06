using System.Collections.Concurrent;
using System.Dynamic;

namespace PipeScript.NET.Engine;

/// <summary>
/// Thread-safe expression evaluation engine with parsed AST caching.
/// </summary>
public class ExpressionEngine
{
    private static readonly ConcurrentDictionary<string, ExpressionNode> _cache = new();

    private static ExpressionNode GetOrParse(string expression)
    {
        return _cache.GetOrAdd(expression, expr =>
        {
            var tokens = new Tokenizer(expr).Tokenize();
            return new ExpressionParser(tokens).Parse();
        });
    }

    /// <summary>
    /// Evaluates a boolean expression against a dynamic object.
    /// </summary>
    public static bool EvaluateCondition(string expression, dynamic obj)
    {
        var node = GetOrParse(expression);
        var context = ToContext(obj);
        var result = node.Evaluate(context);
        if (result is bool b) return b;
        if (result == null) return false;
        return Convert.ToBoolean(result);
    }

    /// <summary>
    /// Evaluates an arithmetic/value expression against a dynamic object.
    /// </summary>
    public static object? EvaluateValue(string expression, dynamic obj)
    {
        var node = GetOrParse(expression);
        var context = ToContext(obj);
        return node.Evaluate(context);
    }

    private static IDictionary<string, object?> ToContext(dynamic obj)
    {
        if (obj is IDictionary<string, object?> dict)
            return dict;

        if (obj is ExpandoObject expando)
        {
#pragma warning disable CS8619 // ExpandoObject implements IDictionary<string, object> (non-nullable values at runtime)
            var raw = (IDictionary<string, object>)expando;
#pragma warning restore CS8619
            return raw.ToDictionary(k => k.Key, v => (object?)v.Value);
        }

        // For other dynamic objects, use reflection
        var context = new Dictionary<string, object?>();
        foreach (var prop in obj.GetType().GetProperties())
            context[prop.Name] = prop.GetValue(obj);
        return context;
    }

    /// <summary>
    /// Clears the expression cache (useful for testing).
    /// </summary>
    public static void ClearCache() => _cache.Clear();
}
