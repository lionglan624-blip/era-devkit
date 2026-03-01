using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a LOCAL variable condition
/// Pattern: LOCAL( op value)? | LOCAL:N( op value)?
/// Note: Bare LOCAL in IF means truthiness check (LOCAL != 0)
/// </summary>
public class LocalRef : ICondition
{
    /// <summary>
    /// Array index for LOCAL:N form. Null means bare LOCAL (implicit index 0).
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    /// <summary>
    /// Comparison operator (==, !=, >, <, >=, <=). Null means truthiness check.
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// Right-hand side value for comparison. Null if no operator.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
