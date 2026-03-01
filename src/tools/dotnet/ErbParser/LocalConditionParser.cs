using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parser for LOCAL variable conditions.
/// Supports 4 patterns:
/// - LOCAL (truthy check, index null/implicit 0)
/// - LOCAL == N (comparison, index null)
/// - LOCAL:N (truthy check, explicit index)
/// - LOCAL:N == M (comparison, explicit index)
/// </summary>
public class LocalConditionParser
{
    // Try indexed form first (LOCAL:N...), then bare form (LOCAL...)
    // Indexed: ^LOCAL:(\d+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$
    // Bare: ^LOCAL(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$
    // CRITICAL: Must NOT match "LOCALS" (different variable type)

    private static readonly Regex IndexedPattern = new(
        @"^LOCAL:(\d+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    private static readonly Regex BarePattern = new(
        @"^LOCAL(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    public LocalRef? Parse(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();

        // Try indexed form first (LOCAL:N)
        var match = IndexedPattern.Match(condition);
        if (match.Success)
        {
            var index = int.TryParse(match.Groups[1].Value, out var idx) ? idx : 0;
            return new LocalRef
            {
                Index = index,
                Operator = match.Groups[2].Success ? match.Groups[2].Value : null,
                Value = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
            };
        }

        // Try bare form (LOCAL)
        match = BarePattern.Match(condition);
        if (match.Success)
        {
            return new LocalRef
            {
                Index = null, // bare LOCAL = null (implicit index 0)
                Operator = match.Groups[1].Success ? match.Groups[1].Value : null,
                Value = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null
            };
        }

        return null;
    }
}
