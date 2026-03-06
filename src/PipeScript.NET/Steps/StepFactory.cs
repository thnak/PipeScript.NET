using PipeScript.NET.Config;
using PipeScript.NET.Core;

namespace PipeScript.NET.Steps;

public static class StepFactory
{
    public static IPipelineStep Create(StepConfig config)
    {
        return config.Type.ToLowerInvariant() switch
        {
            "where" => CreateWhereStep(config),
            "calculate" => CreateCalculateStep(config),
            "orderby" => CreateOrderByStep(config),
            "transform" => CreateTransformStep(config),
            _ => throw new NotSupportedException($"Unknown step type: '{config.Type}'. Supported types: Where, Calculate, OrderBy, Transform.")
        };
    }

    private static WhereStep CreateWhereStep(StepConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Condition))
            throw new InvalidOperationException("Where step requires a 'condition' property.");
        return new WhereStep(config.Condition);
    }

    private static CalculateStep CreateCalculateStep(StepConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Field))
            throw new InvalidOperationException("Calculate step requires a 'field' property.");
        if (string.IsNullOrWhiteSpace(config.Expression))
            throw new InvalidOperationException("Calculate step requires an 'expression' property.");
        return new CalculateStep(config.Field, config.Expression);
    }

    private static OrderByStep CreateOrderByStep(StepConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Field))
            throw new InvalidOperationException("OrderBy step requires a 'field' property.");
        return new OrderByStep(config.Field, config.Direction ?? "Asc");
    }

    private static TransformStep CreateTransformStep(StepConfig config)
    {
        if (config.Mappings == null || config.Mappings.Count == 0)
            throw new InvalidOperationException("Transform step requires a non-empty 'mappings' property.");
        return new TransformStep(config.Mappings);
    }
}
