using PipeScript.NET.Core;
using PipeScript.NET.Engine;

namespace PipeScript.NET.Steps;

public class OrderByStep : IPipelineStep
{
    private readonly string _field;
    private readonly bool _descending;

    public OrderByStep(string field, string direction = "Asc")
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field cannot be empty.", nameof(field));
        _field = field;
        _descending = direction.Equals("Desc", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<dynamic> Execute(IEnumerable<dynamic> data)
    {
        var list = data.ToList();
        Func<dynamic, object?> keySelector = item => ExpressionEngine.EvaluateValue(_field, item);
        return _descending
            ? list.OrderByDescending(keySelector, DynamicComparer.Instance)
            : list.OrderBy(keySelector, DynamicComparer.Instance);
    }
}

internal class DynamicComparer : IComparer<object?>
{
    public static readonly DynamicComparer Instance = new();

    public int Compare(object? x, object? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        if (x is IComparable cx) return cx.CompareTo(y);
        return string.Compare(x.ToString(), y?.ToString(), StringComparison.Ordinal);
    }
}
