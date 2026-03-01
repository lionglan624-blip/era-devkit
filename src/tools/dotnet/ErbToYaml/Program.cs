using ErbToYaml;

namespace ErbToYaml;

/// <summary>
/// CLI entry point for ErbToYaml batch conversion tool
/// Feature 634 - AC#2, AC#11
/// Supports --batch mode and single-file mode
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point
    /// AC#2: Accepts --batch <directory> argument
    /// AC#11: Returns exit code 1 on invalid arguments
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code: 0 if successful, 1 if failed or invalid arguments</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Check for inject-com-id mode
            if (args.Contains("--inject-com-id"))
            {
                return await RunInjectComIdModeAsync(args);
            }
            // Check for batch mode
            else if (args.Contains("--batch"))
            {
                return await RunBatchModeAsync(args);
            }
            else if (args.Length >= 2)
            {
                // Single-file mode (backward compatible)
                return await RunSingleFileModeAsync(args);
            }
            else
            {
                PrintUsage();
                return 1; // AC#11: Invalid arguments return exit code 1
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Run batch conversion mode
    /// </summary>
    private static async Task<int> RunBatchModeAsync(string[] args)
    {
        // Parse --batch argument
        int batchIndex = Array.IndexOf(args, "--batch");
        if (batchIndex < 0 || batchIndex + 1 >= args.Length)
        {
            Console.Error.WriteLine("Error: --batch requires a directory argument");
            PrintUsage();
            return 1; // AC#11: Missing argument value returns exit code 1
        }

        string inputDirectory = args[batchIndex + 1];

        // Find output directory (next argument after input, or default to input_yaml)
        string outputDirectory;
        if (batchIndex + 2 < args.Length && !args[batchIndex + 2].StartsWith("--"))
        {
            outputDirectory = args[batchIndex + 2];
        }
        else
        {
            outputDirectory = Path.Combine(Path.GetDirectoryName(inputDirectory) ?? ".", Path.GetFileName(inputDirectory) + "_yaml");
        }

        // Find Talent.csv path (--talent argument or default)
        string talentCsvPath = GetArgumentValue(args, "--talent")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "Game", "CSV", "Talent.csv");

        // Find schema path (--schema argument or default)
        string schemaPath = GetArgumentValue(args, "--schema")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

        // Validate input directory exists
        if (!Directory.Exists(inputDirectory))
        {
            Console.Error.WriteLine($"Error: Input directory does not exist: {inputDirectory}");
            return 1;
        }

        Console.WriteLine($"Batch conversion mode:");
        Console.WriteLine($"  Input:  {inputDirectory}");
        Console.WriteLine($"  Output: {outputDirectory}");
        Console.WriteLine($"  Talent CSV: {talentCsvPath}");
        Console.WriteLine($"  Schema: {schemaPath}");
        Console.WriteLine();

        // Parse parallel flag
        bool parallel = args.Contains("--parallel");

        // Auto-discover DIM.ERH for CONST resolution
        var dimErhPath = Path.Combine(inputDirectory, "..", "ERB", "DIM.ERH");
        DimConstResolver? dimConstResolver = null;
        if (File.Exists(dimErhPath))
        {
            dimConstResolver = new DimConstResolver();
            dimConstResolver.LoadFromFile(dimErhPath);
        }

        // Construct dependency graph (DI composition root)
        var datalistConverter = new DatalistConverter(talentCsvPath, schemaPath, dimConstResolver);
        var pathAnalyzer = new PathAnalyzer();
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);
        var batchConverter = new BatchConverter(fileConverter);

        // Create batch options if parallel is enabled
        var batchOptions = parallel ? new BatchOptions { EnableParallel = true } : null;

        // Run batch conversion
        var report = await batchConverter.ConvertAsync(inputDirectory, outputDirectory, batchOptions);

        // AC#6: Print summary report
        Console.WriteLine($"Total: {report.Total}, Success: {report.Success}, Failed: {report.Failed}");

        if (report.Failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failed files:");
            foreach (var failure in report.Failures)
            {
                Console.WriteLine($"  - {failure.FilePath}: {failure.Error}");
            }
        }

        // Return exit code based on results
        return report.Failed > 0 ? 1 : 0;
    }

    /// <summary>
    /// Run single-file conversion mode (backward compatible)
    /// </summary>
    private static async Task<int> RunSingleFileModeAsync(string[] args)
    {
        string erbFilePath = args[0];
        string outputDirectory = Path.GetDirectoryName(args[1]) ?? ".";

        // Find Talent.csv and schema paths
        string talentCsvPath = GetArgumentValue(args, "--talent")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "Game", "CSV", "Talent.csv");

        string schemaPath = GetArgumentValue(args, "--schema")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

        // Auto-discover DIM.ERH for CONST resolution
        var erbDir = Path.Combine(Path.GetDirectoryName(erbFilePath) ?? ".", "..", "ERB");
        var dimErhPath = Path.Combine(erbDir, "DIM.ERH");
        DimConstResolver? dimConstResolver = null;
        if (File.Exists(dimErhPath))
        {
            dimConstResolver = new DimConstResolver();
            dimConstResolver.LoadFromFile(dimErhPath);
        }

        // Construct dependencies
        var datalistConverter = new DatalistConverter(talentCsvPath, schemaPath, dimConstResolver);
        var pathAnalyzer = new PathAnalyzer();
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);

        // Convert single file
        var results = await fileConverter.ConvertAsync(erbFilePath, outputDirectory);

        // Check results
        bool anyFailed = results.Any(r => !r.Success);

        if (anyFailed)
        {
            var failure = results.First(r => !r.Success);
            Console.Error.WriteLine($"Conversion failed: {failure.Error}");
            return 1;
        }

        Console.WriteLine($"Conversion successful: {erbFilePath}");
        return 0;
    }

    /// <summary>
    /// Run inject-com-id mode: injects com_id metadata into existing YAML files
    /// by parsing corresponding ERB files to determine COM ID for each YAML index.
    /// </summary>
    private static async Task<int> RunInjectComIdModeAsync(string[] args)
    {
        // Parse arguments: --inject-com-id <erb_directory> <yaml_directory>
        int flagIndex = Array.IndexOf(args, "--inject-com-id");
        if (flagIndex + 2 >= args.Length)
        {
            Console.Error.WriteLine("Error: --inject-com-id requires <erb_directory> <yaml_directory>");
            PrintUsage();
            return 1;
        }

        string erbDirectory = args[flagIndex + 1];
        string yamlDirectory = args[flagIndex + 2];

        if (!Directory.Exists(erbDirectory))
        {
            Console.Error.WriteLine($"Error: ERB directory does not exist: {erbDirectory}");
            return 1;
        }

        if (!Directory.Exists(yamlDirectory))
        {
            Console.Error.WriteLine($"Error: YAML directory does not exist: {yamlDirectory}");
            return 1;
        }

        Console.WriteLine($"Inject com_id mode:");
        Console.WriteLine($"  ERB source:  {erbDirectory}");
        Console.WriteLine($"  YAML target: {yamlDirectory}");
        Console.WriteLine();

        var pathAnalyzer = new PathAnalyzer();
        var injector = new ComIdInjector(pathAnalyzer);
        var result = await injector.InjectAsync(erbDirectory, yamlDirectory);

        Console.WriteLine($"Injected: {result.Injected}, Skipped (already has com_id): {result.Skipped}");

        if (result.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Errors ({result.Errors.Count}):");
            foreach (var error in result.Errors)
            {
                Console.Error.WriteLine($"  - {error}");
            }
        }

        return result.Errors.Count > 0 ? 1 : 0;
    }

    /// <summary>
    /// Get argument value by flag name
    /// </summary>
    private static string? GetArgumentValue(string[] args, string flag)
    {
        int index = Array.IndexOf(args, flag);
        if (index >= 0 && index + 1 < args.Length)
        {
            return args[index + 1];
        }
        return null;
    }

    /// <summary>
    /// Print usage information
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Batch mode:");
        Console.WriteLine("    ErbToYaml --batch <input_directory> [output_directory] [--talent <path>] [--schema <path>]");
        Console.WriteLine();
        Console.WriteLine("  Single-file mode:");
        Console.WriteLine("    ErbToYaml <erb_file> <output_file> [--talent <path>] [--schema <path>]");
        Console.WriteLine();
        Console.WriteLine("  Inject com_id mode:");
        Console.WriteLine("    ErbToYaml --inject-com-id <erb_directory> <yaml_directory>");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --batch          Enable batch conversion mode (recursive directory processing)");
        Console.WriteLine("  --parallel       Enable parallel processing of ERB files (faster batch conversion)");
        Console.WriteLine("  --inject-com-id  Inject com_id metadata into existing YAML files from ERB function names");
        Console.WriteLine("  --talent         Path to Talent.csv (default: Game/CSV/Talent.csv)");
        Console.WriteLine("  --schema         Path to dialogue-schema.json (default: YamlSchemaGen/dialogue-schema.json)");
    }
}
