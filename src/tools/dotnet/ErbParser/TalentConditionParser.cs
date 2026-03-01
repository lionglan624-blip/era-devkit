using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parses TALENT condition strings into structured TalentRef objects
/// Pattern: TALENT:(target:)?(name|index)( op value)?
/// Examples:
///   - TALENT:恋人 → TalentRef(Target="", Name="恋人", Operator=null, Value=null)
///   - TALENT:PLAYER → TalentRef(Target="PLAYER", Name="", Operator=null, Value=null)
///   - TALENT:2 → TalentRef(Target="", Name="", Index=2, Operator=null, Value=null)
///   - TALENT:MASTER:恋人 → TalentRef(Target="MASTER", Name="恋人", Operator=null, Value=null)
///   - TALENT:PLAYER:処女 → TalentRef(Target="PLAYER", Name="処女", Operator=null, Value=null)
///   - TALENT:PLAYER:2 → TalentRef(Target="PLAYER", Name="", Index=2, Operator=null, Value=null)
/// </summary>
public class TalentConditionParser
{
    // Regex pattern for TALENT conditions with optional operator/value
    // Supports: TALENT:name, TALENT:target:name, TALENT:target:name != 0
    // Name group uses [^:\s&]+ to stop at & even without spaces (robustness per C13)
    private static readonly Regex TalentPattern = new Regex(
        @"^TALENT:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    // ERA system variable keywords that are treated as target references
    public static readonly HashSet<string> TargetKeywords = new HashSet<string>
    {
        "PLAYER", "MASTER", "TARGET", "ASSI"
    };

    public TalentRef? ParseTalentCondition(string condition)
    {
        // Graceful handling for null/empty input
        if (string.IsNullOrWhiteSpace(condition))
        {
            return null;
        }

        // Trim the condition string
        condition = condition.Trim();

        // Match the pattern
        var match = TalentPattern.Match(condition);

        if (!match.Success)
        {
            // Malformed TALENT condition - return null gracefully
            return null;
        }

        // Extract target (group 1) and nameOrIndex (group 2)
        var target = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
        var nameOrIndex = match.Groups[2].Value;

        // Validate that nameOrIndex is not empty
        if (string.IsNullOrWhiteSpace(nameOrIndex))
        {
            return null;
        }

        // Extract operator (group 3) and value (group 4) if present
        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var result = new TalentRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        // DISAMBIGUATION LOGIC:
        // 1. Keyword allowlist → Target (only for two-part patterns)
        // 2. int.TryParse → Index
        // 3. Fallback → Name

        if (string.IsNullOrEmpty(target) && TargetKeywords.Contains(nameOrIndex))
        {
            // Two-part pattern with keyword: TALENT:PLAYER
            result.Target = nameOrIndex;
            result.Name = string.Empty;
            result.Index = null;
        }
        else if (int.TryParse(nameOrIndex, out int index))
        {
            // Numeric index (either two-part TALENT:2 or three-part TALENT:PLAYER:2)
            result.Index = index;
            result.Name = string.Empty;
        }
        else
        {
            // Name (string identifier)
            result.Name = nameOrIndex;
            result.Index = null;
        }

        return result;
    }
}
