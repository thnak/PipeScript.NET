using Microsoft.Extensions.DependencyInjection;
using PipeScript.NET.Config;
using PipeScript.NET.Core;

namespace PipeScript.NET.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers PipelineEngine as a singleton from a JSON config string.
    /// </summary>
    public static IServiceCollection AddPipelineFromJson(this IServiceCollection services, string json)
    {
        services.AddSingleton(_ => PipelineEngine.FromJson(json));
        return services;
    }

    /// <summary>
    /// Registers PipelineEngine as a singleton from a PipelineConfig object.
    /// </summary>
    public static IServiceCollection AddPipeline(this IServiceCollection services, PipelineConfig config)
    {
        services.AddSingleton(_ => PipelineEngine.FromConfig(config));
        return services;
    }
}
