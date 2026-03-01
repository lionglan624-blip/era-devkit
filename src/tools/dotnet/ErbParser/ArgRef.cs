using System.Text.Json.Serialization;

namespace ErbParser;

public class ArgRef : ICondition
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
