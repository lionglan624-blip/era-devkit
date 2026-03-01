namespace ErbToYaml;

/// <summary>
/// Batch conversion orchestrator for ERB to YAML conversion
/// Feature 634 - AC#3, AC#5, AC#6, AC#7
/// Discovers ERB files recursively, preserves directory structure, continues on error
/// </summary>
public class BatchConverter : IBatchConverter
{
    private readonly IFileConverter _fileConverter;

    public BatchConverter(IFileConverter fileConverter)
    {
        _fileConverter = fileConverter ?? throw new ArgumentNullException(nameof(fileConverter));
    }

    /// <summary>
    /// Convert all ERB files in a directory recursively
    /// AC#3: Recursive discovery with SearchOption.AllDirectories
    /// AC#5: Directory structure preservation using Path.GetRelativePath
    /// AC#6: Summary reporting (counted by ERB files)
    /// AC#7: Continue-on-error behavior (try-catch per file)
    /// </summary>
    /// <param name="inputDirectory">Directory containing ERB files</param>
    /// <param name="outputDirectory">Directory for YAML output files</param>
    /// <param name="options">Batch conversion options</param>
    /// <returns>Batch report with success/failure counts</returns>
    public async Task<BatchReport> ConvertAsync(string inputDirectory, string outputDirectory, BatchOptions? options = null)
    {
        var report = new BatchReport();

        // AC#3: Recursive ERB file discovery
        var erbFiles = Directory.GetFiles(inputDirectory, "*.ERB", SearchOption.AllDirectories);

        report.Total = erbFiles.Length;

        bool enableParallel = options?.EnableParallel ?? false;

        if (!enableParallel)
        {
            foreach (var erbFile in erbFiles)
            {
                // AC#7: Continue-on-error - wrap each file conversion in try-catch
                try
                {
                    // AC#5: Compute output path preserving directory structure
                    var relativePath = Path.GetRelativePath(inputDirectory, erbFile);
                    var relativeDir = Path.GetDirectoryName(relativePath) ?? string.Empty;
                    var outputDir = Path.Combine(outputDirectory, relativeDir);

                    // Convert the file
                    var results = await _fileConverter.ConvertAsync(erbFile, outputDir);

                    // Check if any result failed
                    bool anyFailed = results.Any(r => !r.Success);

                    if (anyFailed)
                    {
                        report.Failed++;
                        // Add first failure to report
                        var failure = results.First(r => !r.Success);
                        report.Failures.Add(failure);
                    }
                    else
                    {
                        report.Success++;
                    }
                }
                catch (Exception ex)
                {
                    // AC#7: File conversion failed - record and continue
                    report.Failed++;
                    report.Failures.Add(new ConversionResult(
                        Success: false,
                        FilePath: erbFile,
                        Error: $"Failed to convert {erbFile}: {ex.Message}"
                    ));
                }
            }
        }
        else
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = options!.MaxDegreeOfParallelism
            };

            await Parallel.ForEachAsync(erbFiles, parallelOptions, async (erbFile, cancellationToken) =>
            {
                // AC#7: Continue-on-error - wrap each file conversion in try-catch
                try
                {
                    // AC#5: Compute output path preserving directory structure
                    var relativePath = Path.GetRelativePath(inputDirectory, erbFile);
                    var relativeDir = Path.GetDirectoryName(relativePath) ?? string.Empty;
                    var outputDir = Path.Combine(outputDirectory, relativeDir);

                    // Convert the file
                    var results = await _fileConverter.ConvertAsync(erbFile, outputDir);

                    // Check if any result failed
                    bool anyFailed = results.Any(r => !r.Success);

                    if (anyFailed)
                    {
                        lock (report)
                        {
                            report.Failed++;
                            // Add first failure to report
                            var failure = results.First(r => !r.Success);
                            report.Failures.Add(failure);
                        }
                    }
                    else
                    {
                        lock (report)
                        {
                            report.Success++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // AC#7: File conversion failed - record and continue
                    lock (report)
                    {
                        report.Failed++;
                        report.Failures.Add(new ConversionResult(
                            Success: false,
                            FilePath: erbFile,
                            Error: $"Failed to convert {erbFile}: {ex.Message}"
                        ));
                    }
                }
            });
        }

        return report;
    }
}
