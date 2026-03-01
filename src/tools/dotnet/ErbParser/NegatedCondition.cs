using System.Text.Json.Serialization;

namespace ErbParser;

public class NegatedCondition : ICondition
{
    [JsonPropertyName("inner")]
    public required ICondition Inner { get; init; }
}
