using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a TALENT condition
/// Pattern: TALENT:(target:)?(name|index)( op value)?
/// Examples:
///   - TALENT:恋人 → Name="恋人"
///   - TALENT:PLAYER → Target="PLAYER"
///   - TALENT:2 → Index=2
///   - TALENT:PLAYER:処女 → Target="PLAYER", Name="処女"
///   - TALENT:PLAYER:2 → Target="PLAYER", Index=2
/// </summary>
public class TalentRef : ICondition
{
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
