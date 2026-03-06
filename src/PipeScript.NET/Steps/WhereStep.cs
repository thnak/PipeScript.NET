using PipeScript.NET.Core;
using PipeScript.NET.Engine;

namespace PipeScript.NET.Steps;

public class WhereStep : IPipelineStep
{
    private readonly string _condition;

    public WhereStep(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be empty.", nameof(condition));
        _condition = condition;
    }

    public IEnumerable<dynamic> Execute(IEnumerable<dynamic> data)
    {
        return data.Where(item => ExpressionEngine.EvaluateCondition(_condition, item));
    }
}
