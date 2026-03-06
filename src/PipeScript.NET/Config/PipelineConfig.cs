using System.Text.Json;
using System.Text.Json.Serialization;

namespace PipeScript.NET.Config;

public class PipelineConfig
{
    [JsonPropertyName("steps")]
    public List<StepConfig> Steps { get; set; } = new();

    public static PipelineConfig FromJson(string json)
    {
        return JsonSerializer.Deserialize<PipelineConfig>(json)
               ?? throw new InvalidOperationException("Failed to deserialize pipeline config.");
    }
}
