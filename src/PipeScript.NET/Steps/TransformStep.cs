using System.Dynamic;
using PipeScript.NET.Core;
using PipeScript.NET.Engine;

namespace PipeScript.NET.Steps;

/// <summary>
/// Projects and renames fields. The mappings dictionary maps new field names to source field names (or expressions).
/// </summary>
public class TransformStep : IPipelineStep
{
    private readonly Dictionary<string, string> _mappings;

    public TransformStep(Dictionary<string, string> mappings)
    {
        _mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
    }

    public IEnumerable<dynamic> Execute(IEnumerable<dynamic> data)
    {
        foreach (var item in data)
        {
            dynamic result = new ExpandoObject();
            var resultDict = (IDictionary<string, object>)result;
            foreach (var (newName, sourceExpression) in _mappings)
            {
                var value = ExpressionEngine.EvaluateValue(sourceExpression, item);
                resultDict[newName] = value!;
            }
            yield return result;
        }
    }
}
