namespace ErbLinter.Config;

/// <summary>
/// Output format for linter results
/// </summary>
public enum OutputFormat
{
    Text,
    Json
}

/// <summary>
/// Command-line options for the linter
/// </summary>
public class LinterOptions
{
    /// <summary>
    /// Path to scan (file or directory)
    /// </summary>
    public string Path { get; set; } = ".";

    /// <summary>
    /// Output file path (null = stdout)
    /// </summary>
    public string? OutputFile { get; set; }

    /// <summary>
    /// Output format (text or json)
    /// </summary>
    public OutputFormat Format { get; set; } = OutputFormat.Text;

    /// <summary>
    /// Minimum issue level to report
    /// </summary>
    public Reporter.IssueLevel MinLevel { get; set; } = Reporter.IssueLevel.Info;

    /// <summary>
    /// Disable colored output
    /// </summary>
    public bool NoColor { get; set; }

    /// <summary>
    /// Show help
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// CSV directory path (for variable definitions)
    /// </summary>
    public string? CsvPath { get; set; }

    /// <summary>
    /// Enable dead code detection
    /// </summary>
    public bool DeadCode { get; set; }

    /// <summary>
    /// Path to entry points definition file
    /// </summary>
    public string? EntryPointsFile { get; set; }

    /// <summary>
    /// Enable call graph generation mode
    /// </summary>
    public bool CallGraph { get; set; }

    /// <summary>
    /// Root function for call graph filtering
    /// </summary>
    public string? CallGraphRoot { get; set; }

    /// <summary>
    /// Maximum depth for call graph traversal
    /// </summary>
    public int? CallGraphDepth { get; set; }

    /// <summary>
    /// Enable impact analysis mode
    /// </summary>
    public bool Impact { get; set; }

    /// <summary>
    /// Target function for impact analysis
    /// </summary>
    public string? ImpactFunction { get; set; }

    /// <summary>
    /// Output reverse graph (DOT format) instead of text report
    /// </summary>
    public bool ReverseGraph { get; set; }
}
