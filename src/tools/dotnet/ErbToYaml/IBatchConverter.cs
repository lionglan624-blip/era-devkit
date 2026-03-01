namespace ErbToYaml;

/// <summary>
/// Options for batch conversion
/// </summary>
public class BatchOptions
{
    /// <summary>
    /// Enable parallel processing of files
    /// </summary>
    public bool EnableParallel { get; init; } = false;

    /// <summary>
    /// Maximum degree of parallelism when parallel processing is enabled
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

    /// <summary>
    /// Cancellation token for async operations
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = default;
}

/// <summary>
/// Interface for batch ERB file conversion
/// Feature 634 - AC#3, AC#5, AC#6, AC#7
/// </summary>
public interface IBatchConverter
{
    /// <summary>
    /// Convert all ERB files in a directory recursively
    /// Preserves directory structure in output
    /// Continues processing on individual file failures
    /// </summary>
    /// <param name="inputDirectory">Directory containing ERB files</param>
    /// <param name="outputDirectory">Directory for YAML output files</param>
    /// <param name="options">Batch conversion options</param>
    /// <returns>Batch report with success/failure counts</returns>
    Task<BatchReport> ConvertAsync(string inputDirectory, string outputDirectory, BatchOptions? options = null);
}
