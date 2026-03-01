using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a logical operation (AND/OR) combining two conditions
/// Pattern: condition1 && condition2 OR condition1 || condition2
/// </summary>
public class LogicalOp : ICondition
{
    [JsonPropertyName("left")]
    public ICondition? Left { get; set; }

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = string.Empty;

    [JsonPropertyName("right")]
    public ICondition? Right { get; set; }
}
