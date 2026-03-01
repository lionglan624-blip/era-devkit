using System.Text.RegularExpressions;
using ErbLinter.Data;
using ErbLinter.Reporter;

namespace ErbLinter.Analyzer;

/// <summary>
/// Analyzes ERB code for variable usage issues
/// </summary>
public class VariableAnalyzer
{
    // Pattern for FLAG:index or FLAG:name references
    private static readonly Regex FlagPattern = new(
        @"\bFLAG\s*:\s*(\d+|\w+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pattern for CFLAG:chara:index or CFLAG:chara:name references
    private static readonly Regex CFlagPattern = new(
        @"\bCFLAG\s*:\s*\w+\s*:\s*(\d+|\w+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly VariableRegistry _registry;

    public VariableAnalyzer(VariableRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Analyze a file for variable usage issues
    /// </summary>
    public IEnumerable<Issue> Analyze(string filePath, string[] lines)
    {
        var issues = new List<Issue>();

        // Skip if registry is empty (no CSV loaded)
        if (_registry.FlagCount == 0 && _registry.CFlagCount == 0)
            return issues;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNum = i + 1;

            // Skip comments
            if (line.TrimStart().StartsWith(";"))
                continue;

            // Check FLAG references
            foreach (Match match in FlagPattern.Matches(line))
            {
                var indexOrName = match.Groups[1].Value;
                if (int.TryParse(indexOrName, out var index))
                {
                    // Numeric index - check if defined
                    if (!_registry.HasFlag(index) && index >= 0 && index < 10000)
                    {
                        issues.Add(new Issue(
                            filePath, lineNum, match.Index + 1,
                            IssueLevel.Info, "VAR001",
                            $"FLAG:{index} is not defined in FLAG.CSV"));
                    }
                }
                // Name references are usually fine (aliased)
            }

            // Check CFLAG references
            foreach (Match match in CFlagPattern.Matches(line))
            {
                var indexOrName = match.Groups[1].Value;
                if (int.TryParse(indexOrName, out var index))
                {
                    // Numeric index - check if defined
                    if (!_registry.HasCFlag(index) && index >= 0 && index < 10000)
                    {
                        issues.Add(new Issue(
                            filePath, lineNum, match.Index + 1,
                            IssueLevel.Info, "VAR002",
                            $"CFLAG index {index} is not defined in CFLAG.csv"));
                    }
                }
            }
        }

        return issues;
    }
}
