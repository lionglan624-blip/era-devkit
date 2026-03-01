using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parser for logical operators (AND/OR) in conditions
/// Handles operator precedence: && (AND) has higher precedence than || (OR)
/// Builds left-associative parse trees for chained operators
/// </summary>
public class LogicalOperatorParser
{
    private readonly TalentConditionParser _talentParser = new();
    private readonly FunctionCallParser _functionParser = new();
    private readonly List<(string prefix, Func<string, ICondition?> parser)> _variableParsers;

    public LogicalOperatorParser()
    {
        var cflagParser = new VariableConditionParser<CflagRef>("CFLAG");
        var tcvarParser = new VariableConditionParser<TcvarRef>("TCVAR");
        var equipParser = new VariableConditionParser<EquipRef>("EQUIP");
        var itemParser = new VariableConditionParser<ItemRef>("ITEM");
        var stainParser = new VariableConditionParser<StainRef>("STAIN");
        var markParser = new VariableConditionParser<MarkRef>("MARK");
        var expParser = new VariableConditionParser<ExpRef>("EXP");
        var nowexParser = new VariableConditionParser<NowexRef>("NOWEX");
        var ablParser = new VariableConditionParser<AblRef>("ABL");
        var flagParser = new VariableConditionParser<FlagRef>("FLAG");
        var tflagParser = new VariableConditionParser<TflagRef>("TFLAG");
        var tequipParser = new VariableConditionParser<TequipRef>("TEQUIP");
        var palamParser = new VariableConditionParser<PalamRef>("PALAM");
        var argParser = new ArgConditionParser();
        var localParser = new LocalConditionParser();

        _variableParsers =
        [
            ("CFLAG", cflagParser.Parse),
            ("TCVAR", tcvarParser.Parse),
            ("EQUIP", equipParser.Parse),
            ("ITEM", itemParser.Parse),
            ("STAIN", stainParser.Parse),
            ("MARK", markParser.Parse),
            ("EXP", expParser.Parse),
            ("NOWEX", nowexParser.Parse),
            ("ABL", ablParser.Parse),
            ("FLAG", flagParser.Parse),
            ("TFLAG", tflagParser.Parse),
            ("TEQUIP", tequipParser.Parse),
            ("PALAM", palamParser.Parse),
            ("ARG", argParser.Parse),
            ("LOCAL", localParser.Parse),
        ];
    }

    /// <summary>
    /// Parses a logical expression into an ICondition tree
    /// Supports: &&, ||, chained operators, mixed precedence
    /// Returns single condition unwrapped if no operators present
    /// </summary>
    public ICondition? ParseLogicalExpression(string condition)
    {
        // Graceful handling for null/empty input
        if (string.IsNullOrWhiteSpace(condition))
        {
            return null;
        }

        // Trim the condition string
        condition = condition.Trim();

        // Parse with operator precedence: OR has lowest precedence, so parse it first
        return ParseOrExpression(condition);
    }

    /// <summary>
    /// Parse OR (||) expressions - lowest precedence
    /// Pattern: andExpr || andExpr || ...
    /// </summary>
    private ICondition? ParseOrExpression(string condition)
    {
        // Split on || operator (not part of a quoted string or nested expression)
        var parts = SplitOnOperator(condition, "||");

        if (parts.Count == 1)
        {
            // No OR operators, parse as AND expression
            return ParseAndExpression(parts[0]);
        }

        // Build left-associative tree: ((a || b) || c)
        ICondition? result = null;

        foreach (var part in parts)
        {
            var parsed = ParseAndExpression(part);
            if (parsed == null)
            {
                // Invalid sub-expression
                return null;
            }

            if (result == null)
            {
                result = parsed;
            }
            else
            {
                result = new LogicalOp
                {
                    Left = result,
                    Operator = "||",
                    Right = parsed
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Parse AND (&&) expressions - higher precedence than OR
    /// Pattern: condition && condition && ...
    /// </summary>
    private ICondition? ParseAndExpression(string condition)
    {
        // Split on && operator
        var parts = SplitOnOperator(condition, "&&");

        if (parts.Count == 1)
        {
            // No AND operators, parse as atomic condition
            return ParseAtomicCondition(parts[0]);
        }

        // Build left-associative tree: ((a && b) && c)
        ICondition? result = null;

        foreach (var part in parts)
        {
            var parsed = ParseAtomicCondition(part);
            if (parsed == null)
            {
                // Invalid sub-expression
                return null;
            }

            if (result == null)
            {
                result = parsed;
            }
            else
            {
                result = new LogicalOp
                {
                    Left = result,
                    Operator = "&&",
                    Right = parsed
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Parse atomic condition (TALENT, CFLAG, TCVAR, function call, negation, or parenthesized expression)
    /// Tries each parser in order until one succeeds
    /// </summary>
    private ICondition? ParseAtomicCondition(string condition)
    {
        condition = condition.Trim();

        // Handle parenthesized sub-expressions: (A || B) → strip outer parens, recurse
        if (condition.StartsWith("(") && FindMatchingClosingParen(condition, 0) == condition.Length - 1)
        {
            var inner = condition.Substring(1, condition.Length - 2).Trim();
            return ParseLogicalExpression(inner);
        }

        // F759: Check for compound bitwise-comparison pattern: (expr) op value
        // Must come AFTER paren-stripping guard and BEFORE prefix-anchored parsers
        if (condition.StartsWith("("))
        {
            var closingParenIndex = FindMatchingClosingParen(condition, 0);
            if (closingParenIndex > 0 && closingParenIndex < condition.Length - 1)
            {
                var remainder = condition.Substring(closingParenIndex + 1).Trim();
                var comparisonMatch = Regex.Match(remainder, @"^(==|!=|>=|<=|>|<)\s+(.+)$");
                if (comparisonMatch.Success)
                {
                    var innerExpr = condition.Substring(1, closingParenIndex - 1).Trim();
                    var comparisonOp = comparisonMatch.Groups[1].Value;
                    var comparisonValue = comparisonMatch.Groups[2].Value.Trim();

                    var innerCondition = ParseAtomicCondition(innerExpr);
                    if (innerCondition != null && HasBitwiseOperator(innerCondition))
                    {
                        return new BitwiseComparisonCondition
                        {
                            Inner = innerCondition,
                            ComparisonOp = comparisonOp,
                            ComparisonValue = comparisonValue
                        };
                    }
                }
            }
        }

        // Handle negation prefix (!)
        if (condition.StartsWith("!"))
        {
            var innerCondition = condition.Substring(1).Trim();
            var parsed = ParseAtomicCondition(innerCondition); // Recurse: handles parens, nested negation
            if (parsed == null)
                return null;

            return new NegatedCondition { Inner = parsed };
        }

        // Try TALENT parser
        var talent = _talentParser.ParseTalentCondition(condition);
        if (talent != null)
        {
            return talent;
        }

        // Try variable parsers (list-based registration)
        foreach (var (prefix, parser) in _variableParsers)
        {
            var result = parser(condition);
            if (result != null)
                return result;
        }

        // Try function call parser
        var function = _functionParser.ParseFunctionCall(condition);
        if (function != null)
        {
            return function;
        }

        // No parser could handle this condition
        return null;
    }

    /// <summary>
    /// Split condition string on logical operator, respecting parentheses
    /// Returns list of sub-expressions (operator removed)
    /// </summary>
    private List<string> SplitOnOperator(string condition, string op)
    {
        var parts = new List<string>();
        var currentPart = new System.Text.StringBuilder();
        int parenDepth = 0;
        int i = 0;

        while (i < condition.Length)
        {
            // Track parentheses depth
            if (condition[i] == '(')
            {
                parenDepth++;
                currentPart.Append(condition[i]);
                i++;
            }
            else if (condition[i] == ')')
            {
                parenDepth--;
                currentPart.Append(condition[i]);
                i++;
            }
            // Check for operator at current position (only at depth 0)
            else if (parenDepth == 0 && i + op.Length <= condition.Length &&
                     condition.Substring(i, op.Length) == op)
            {
                // Found operator - save current part and skip operator
                parts.Add(currentPart.ToString().Trim());
                currentPart.Clear();
                i += op.Length;

                // Skip whitespace after operator
                while (i < condition.Length && char.IsWhiteSpace(condition[i]))
                {
                    i++;
                }
            }
            else
            {
                currentPart.Append(condition[i]);
                i++;
            }
        }

        // Add final part
        if (currentPart.Length > 0)
        {
            parts.Add(currentPart.ToString().Trim());
        }

        return parts;
    }

    /// <summary>
    /// Finds the matching closing parenthesis for the opening paren at startIndex.
    /// Uses depth counter consistent with SplitOnOperator's paren-tracking approach.
    /// Returns -1 if no matching paren found.
    /// F759: Used for compound bitwise-comparison detection
    /// </summary>
    private static int FindMatchingClosingParen(string input, int startIndex)
    {
        var depth = 0;
        for (var i = startIndex; i < input.Length; i++)
        {
            if (input[i] == '(')
                depth++;
            else if (input[i] == ')')
                depth--;
            if (depth == 0)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Returns true if condition is a variable reference with Operator="&amp;".
    /// F759: Parse-time validation for compound bitwise-comparison
    /// </summary>
    private static bool HasBitwiseOperator(ICondition condition) => condition switch
    {
        TalentRef t => t.Operator == "&",
        VariableRef v => v.Operator == "&",
        _ => false
    };
}
