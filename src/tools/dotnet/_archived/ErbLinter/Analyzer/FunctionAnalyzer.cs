using ErbLinter.Parser;
using ErbLinter.Reporter;

namespace ErbLinter.Analyzer;

/// <summary>
/// Analyzes function definitions and calls
/// </summary>
public class FunctionAnalyzer
{
    /// <summary>
    /// Analyze function index for issues like duplicates
    /// </summary>
    public IEnumerable<Issue> AnalyzeIndex(FunctionIndex index)
    {
        var issues = new List<Issue>();

        // Check for duplicate function definitions
        foreach (var (name, definitions) in index.GetDuplicateFunctions())
        {
            // Report each duplicate after the first
            foreach (var def in definitions.Skip(1))
            {
                var firstDef = definitions[0];
                issues.Add(new Issue(
                    def.FilePath,
                    def.Line,
                    1,
                    IssueLevel.Warning,
                    "FUNC004",
                    $"Duplicate function @{name} (first defined at {Path.GetFileName(firstDef.FilePath)}:{firstDef.Line})"));
            }
        }

        return issues;
    }

    /// <summary>
    /// Analyze CALL statements in a file against the function index
    /// </summary>
    public IEnumerable<Issue> AnalyzeCalls(string filePath, string[] lines, FunctionIndex index)
    {
        var issues = new List<Issue>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNum = i + 1;

            // Skip comments and empty
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            // Check for CALL statements
            var callTarget = ExtractCallTarget(line);
            if (callTarget != null)
            {
                if (!index.HasFunction(callTarget))
                {
                    // Determine if it's CALL or TRYCALL
                    var isTryCall = line.StartsWith("TRYCALL", StringComparison.OrdinalIgnoreCase);

                    if (isTryCall)
                    {
                        // TRYCALL to undefined function is just info
                        issues.Add(new Issue(filePath, lineNum, 1, IssueLevel.Info, "FUNC002",
                            $"TRYCALL to undefined function @{callTarget}"));
                    }
                    else
                    {
                        // CALL to undefined function is a warning (might be dynamic or in included file)
                        issues.Add(new Issue(filePath, lineNum, 1, IssueLevel.Warning, "FUNC001",
                            $"CALL to undefined function @{callTarget}"));
                    }
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Extract call target from a CALL/TRYCALL/JUMP/GOTO line
    /// Returns null if not a call statement or target is dynamic
    /// </summary>
    private static string? ExtractCallTarget(string line)
    {
        string? rest = null;

        // Standard CALL (but not CALLFORM, CALLFORMF, etc.)
        if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) &&
            !line.StartsWith("CALLFORM", StringComparison.OrdinalIgnoreCase))
        {
            rest = line.Substring(5).Trim();
        }
        // TRYCALL (but not TRYCALLFORM, TRYCCALL, etc.)
        else if (line.StartsWith("TRYCALL ", StringComparison.OrdinalIgnoreCase) &&
                 !line.StartsWith("TRYCALLFORM", StringComparison.OrdinalIgnoreCase) &&
                 !line.StartsWith("TRYCCALL", StringComparison.OrdinalIgnoreCase))
        {
            rest = line.Substring(8).Trim();
        }
        // JUMP (but not JUMPFORM)
        else if (line.StartsWith("JUMP ", StringComparison.OrdinalIgnoreCase) &&
                 !line.StartsWith("JUMPFORM", StringComparison.OrdinalIgnoreCase))
        {
            rest = line.Substring(5).Trim();
        }
        // GOTO (but not GOTOFORM)
        else if (line.StartsWith("GOTO ", StringComparison.OrdinalIgnoreCase) &&
                 !line.StartsWith("GOTOFORM", StringComparison.OrdinalIgnoreCase))
        {
            rest = line.Substring(5).Trim();
        }

        if (rest == null)
            return null;

        // Skip if dynamic (starts with %)
        if (rest.StartsWith("%"))
            return null;

        // Extract function name (until space, comma, or open paren)
        var endIndex = rest.Length;
        for (int i = 0; i < rest.Length; i++)
        {
            var c = rest[i];
            if (c == ' ' || c == ',' || c == '(' || c == ';')
            {
                endIndex = i;
                break;
            }
        }

        var target = rest.Substring(0, endIndex).Trim();
        return string.IsNullOrEmpty(target) ? null : target;
    }
}
