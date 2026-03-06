using System.Text.Json.Serialization;

namespace PipeScript.NET.Config;

public class StepConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("direction")]
    public string? Direction { get; set; }

    [JsonPropertyName("mappings")]
    public Dictionary<string, string>? Mappings { get; set; }
}
