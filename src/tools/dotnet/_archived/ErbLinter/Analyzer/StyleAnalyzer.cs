using System.Text.RegularExpressions;
using ErbLinter.Parser;
using ErbLinter.Reporter;

namespace ErbLinter.Analyzer;

/// <summary>
/// Analyzes ERB code for style issues (naming conventions, etc.)
/// </summary>
public class StyleAnalyzer
{
    // Character ID to directory name mapping
    private static readonly Dictionary<int, string> CharacterDirs = new()
    {
        { 1, "1_美鈴" },
        { 2, "2_小悪魔" },
        { 3, "3_パチュリー" },
        { 4, "4_咲夜" },
        { 5, "5_レミリア" },
        { 6, "6_フラン" },
        { 7, "7_子悪魔" },
        { 8, "8_チルノ" },
        { 9, "9_大妖精" },
        { 10, "10_魔理沙" }
    };

    // Regex for kojo function names: KOJO_*_K{N}_* or NTR_KOJO_*_K{N}_* or KOJO_K{N}
    private static readonly Regex KojoPattern = new(
        @"^(NTR_)?KOJO_(?:\w+_)?K(\d+)(?:_\w+)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Regex for universal kojo: KOJO_*_KU_* or KOJO_KU
    private static readonly Regex KojoUniversalPattern = new(
        @"^(NTR_)?KOJO_(?:\w+_)?KU(?:_\w+)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Analyze function index for style issues
    /// </summary>
    public IEnumerable<Issue> Analyze(FunctionIndex index)
    {
        var issues = new List<Issue>();

        foreach (var funcName in index.FunctionNames)
        {
            foreach (var def in index.GetFunction(funcName))
            {
                // Check if this is a kojo function in the 口上 directory
                if (!def.FilePath.Contains("口上"))
                    continue;

                // Check if function name follows kojo pattern
                var match = KojoPattern.Match(funcName);
                if (match.Success)
                {
                    // Extract character ID from function name
                    if (int.TryParse(match.Groups[2].Value, out var charId))
                    {
                        // Verify the character ID matches the directory
                        var expectedDir = CharacterDirs.GetValueOrDefault(charId);
                        if (expectedDir != null && !def.FilePath.Contains(expectedDir))
                        {
                            // Function in wrong directory
                            var actualDir = ExtractKojoDirectory(def.FilePath);
                            issues.Add(new Issue(
                                def.FilePath,
                                def.Line,
                                1,
                                IssueLevel.Info,
                                "STYLE001",
                                $"Kojo function @{funcName} has K{charId} but is in {actualDir} directory"));
                        }
                    }
                }
                else if (!KojoUniversalPattern.IsMatch(funcName))
                {
                    // In 口上 directory but doesn't follow naming convention
                    // Only report if it looks like it should be a kojo function
                    if (funcName.StartsWith("KOJO_", StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new Issue(
                            def.FilePath,
                            def.Line,
                            1,
                            IssueLevel.Info,
                            "STYLE002",
                            $"Kojo function @{funcName} doesn't follow K{{N}} naming pattern"));
                    }
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Extract the kojo directory name from a file path
    /// </summary>
    private static string ExtractKojoDirectory(string filePath)
    {
        // Find the directory after 口上/
        var kojoIndex = filePath.IndexOf("口上", StringComparison.Ordinal);
        if (kojoIndex < 0)
            return "unknown";

        var afterKojo = filePath.Substring(kojoIndex + 2); // Skip "口上"
        var parts = afterKojo.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "unknown";
    }
}
