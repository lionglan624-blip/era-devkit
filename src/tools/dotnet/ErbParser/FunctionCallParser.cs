using System.Text;
using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parser for function call conditions
/// Pattern: FunctionName(arg1, arg2, ...)
/// Examples:
///   - HAS_VAGINA(TARGET) → FunctionCall(Name="HAS_VAGINA", Args=["TARGET"])
///   - FIRSTTIME() → FunctionCall(Name="FIRSTTIME", Args=[])
///   - SOME_FUNC(A, B, C) → FunctionCall(Name="SOME_FUNC", Args=["A", "B", "C"])
///   - FIRSTTIME(TOSTR(350), 1) → FunctionCall(Name="FIRSTTIME", Args=["TOSTR(350)", "1"])
/// </summary>
public class FunctionCallParser
{
    private static readonly Regex FunctionNamePattern = new Regex(
        @"^([A-Z_][A-Z0-9_]*)\(",
        RegexOptions.Compiled
    );

    public FunctionCall? ParseFunctionCall(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();

        var match = FunctionNamePattern.Match(condition);
        if (!match.Success)
            return null;

        var functionName = match.Groups[1].Value;

        if (!condition.EndsWith(")"))
            return null;

        int startIdx = match.Index + match.Length; // Position after "FNAME("
        int endIdx = condition.Length - 1; // Position of final ')'

        // Balanced-paren validation
        int depth = 1;
        int i = startIdx;
        while (i < condition.Length && depth > 0)
        {
            if (condition[i] == '(')
                depth++;
            else if (condition[i] == ')')
                depth--;
            i++;
        }

        if (depth != 0 || i - 1 != endIdx)
            return null;

        var argsString = condition.Substring(startIdx, endIdx - startIdx);
        var args = SplitArguments(argsString);

        return new FunctionCall
        {
            Name = functionName,
            Args = args
        };
    }

    private string[] SplitArguments(string argsString)
    {
        if (string.IsNullOrWhiteSpace(argsString))
            return Array.Empty<string>();

        var args = new List<string>();
        var currentArg = new StringBuilder();
        int depth = 0;

        foreach (char c in argsString)
        {
            if (c == '(')
            {
                depth++;
                currentArg.Append(c);
            }
            else if (c == ')')
            {
                depth--;
                currentArg.Append(c);
            }
            else if (c == ',' && depth == 0)
            {
                args.Add(currentArg.ToString().Trim());
                currentArg.Clear();
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
            args.Add(currentArg.ToString().Trim());

        return args.ToArray();
    }
}
