using PipeScript.NET.Config;
using PipeScript.NET.Steps;

namespace PipeScript.NET.Core;

public class PipelineEngine
{
    private readonly List<IPipelineStep> _steps;

    public PipelineEngine(IEnumerable<IPipelineStep> steps)
    {
        _steps = steps.ToList();
    }

    public IEnumerable<dynamic> Execute(IEnumerable<dynamic> data)
    {
        IEnumerable<dynamic> result = data;
        foreach (var step in _steps)
            result = step.Execute(result);
        return result;
    }

    public static PipelineEngine FromConfig(PipelineConfig config)
    {
        var steps = config.Steps.Select(StepFactory.Create);
        return new PipelineEngine(steps);
    }

    public static PipelineEngine FromJson(string json)
    {
        var config = PipelineConfig.FromJson(json);
        return FromConfig(config);
    }
}
