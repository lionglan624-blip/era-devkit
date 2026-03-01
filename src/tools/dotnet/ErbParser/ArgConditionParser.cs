using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parser for ARG (function parameter) conditions.
/// Supports 4 patterns:
/// - ARG (truthy check, index 0)
/// - ARG == N (comparison, index 0)
/// - ARG:N (truthy check, explicit index)
/// - ARG:N == M (comparison, explicit index)
/// </summary>
public class ArgConditionParser
{
    private static readonly Regex Pattern = new(
        @"^ARG(?::(\d+))?(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    public ArgRef? Parse(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = Pattern.Match(condition);

        if (!match.Success)
            return null;

        var index = match.Groups[1].Success && int.TryParse(match.Groups[1].Value, out var idx)
            ? idx
            : 0;

        var operatorValue = match.Groups[2].Success ? match.Groups[2].Value : null;
        var value = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;

        return new ArgRef
        {
            Index = index,
            Operator = operatorValue,
            Value = value
        };
    }
}
