namespace ErbToYaml;

/// <summary>
/// Summary report for batch conversion operations
/// Feature 634 - AC#6
/// Counts ERB input files (not YAML outputs)
/// </summary>
public class BatchReport
{
    /// <summary>
    /// Total number of ERB files processed
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Number of ERB files where all YAML outputs succeeded
    /// </summary>
    public int Success { get; set; }

    /// <summary>
    /// Number of ERB files where any YAML output failed
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// List of failed conversions with error details
    /// </summary>
    public List<ConversionResult> Failures { get; set; } = new();
}
