using System.Dynamic;
using PipeScript.NET.Core;
using PipeScript.NET.Engine;

namespace PipeScript.NET.Steps;

public class CalculateStep : IPipelineStep
{
    private readonly string _field;
    private readonly string _expression;

    public CalculateStep(string field, string expression)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field cannot be empty.", nameof(field));
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be empty.", nameof(expression));
        _field = field;
        _expression = expression;
    }

    public IEnumerable<dynamic> Execute(IEnumerable<dynamic> data)
    {
        foreach (var item in data)
        {
            var value = ExpressionEngine.EvaluateValue(_expression, item);
            if (item is ExpandoObject expando)
            {
#pragma warning disable CS8619 // ExpandoObject implements IDictionary<string, object> (non-nullable values at runtime)
                var expandoDict = (IDictionary<string, object?>)(IDictionary<string, object>)expando;
#pragma warning restore CS8619
                expandoDict[_field] = value;
            }
            else if (item is IDictionary<string, object?> dict)
            {
                dict[_field] = value;
            }
            yield return item;
        }
    }
}
