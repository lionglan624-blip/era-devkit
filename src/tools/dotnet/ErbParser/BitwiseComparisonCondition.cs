using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a compound bitwise-comparison condition: (VAR & mask) op value
/// Example: (TALENT:性別嗜好 & 3) == 3
/// F759: Two-stage evaluation: (1) compute VAR & mask, (2) compare result with value
/// </summary>
public class BitwiseComparisonCondition : ICondition
{
    /// <summary>
    /// Inner bitwise expression (must have Operator="&amp;")
    /// Example: TALENT:性別嗜好 &amp; 3 → TalentRef with Name="性別嗜好", Operator="&amp;", Value="3"
    /// </summary>
    [JsonPropertyName("inner")]
    public required ICondition Inner { get; init; }

    /// <summary>
    /// Comparison operator applied to bitwise result
    /// Valid values: "==", "!=", ">", ">=", "&lt;", "&lt;="
    /// </summary>
    [JsonPropertyName("comparison_op")]
    public required string ComparisonOp { get; init; }

    /// <summary>
    /// Expected value after bitwise operation
    /// Example: For (TALENT:性別嗜好 &amp; 3) == 3, this is "3"
    /// </summary>
    [JsonPropertyName("comparison_value")]
    public required string ComparisonValue { get; init; }
}
