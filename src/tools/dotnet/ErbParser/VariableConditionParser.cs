using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Generic parser for variable condition types
/// Supports comparison operators and bitwise &amp;
/// </summary>
public class VariableConditionParser<TRef> where TRef : VariableRef, new()
{
    private readonly string _prefix;
    private readonly Regex _pattern;

    public VariableConditionParser(string prefix)
    {
        _prefix = prefix;
        // Name group uses [^:\s&]+ to stop at & even without spaces (robustness per C13)
        _pattern = new Regex(
            $@"^{prefix}:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
            RegexOptions.Compiled
        );
    }

    public TRef? Parse(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = _pattern.Match(condition);

        if (!match.Success)
            return null;

        var target = match.Groups[1].Success ? match.Groups[1].Value : null;
        var nameOrIndex = match.Groups[2].Value;

        if (string.IsNullOrWhiteSpace(nameOrIndex))
            return null;

        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var result = new TRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        if (int.TryParse(nameOrIndex, out int index))
        {
            result.Index = index;
            result.Name = null;
        }
        else
        {
            result.Name = nameOrIndex;
            result.Index = null;
        }

        return result;
    }
}
