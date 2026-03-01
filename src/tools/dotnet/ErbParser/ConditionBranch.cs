using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a single branch in a condition tree (IF/ELSEIF/ELSE)
/// </summary>
public class ConditionBranch
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "if", "elseif", "else"

    [JsonPropertyName("condition")]
    public ICondition? Condition { get; set; }

    [JsonPropertyName("hasBody")]
    public bool HasBody { get; set; }
}
