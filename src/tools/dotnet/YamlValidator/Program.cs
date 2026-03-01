using System.Runtime.CompilerServices;
using System.Text.Json;
using NJsonSchema;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[assembly: InternalsVisibleTo("YamlValidator.Tests")]

namespace YamlValidator;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var options = ParseArguments(args);

        if (options.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        if (string.IsNullOrEmpty(options.SchemaPath))
        {
            Console.Error.WriteLine("Error: --schema is required");
            PrintHelp();
            return 1;
        }

        if (string.IsNullOrEmpty(options.YamlPath) && string.IsNullOrEmpty(options.ValidateAllPath))
        {
            Console.Error.WriteLine("Error: Either --yaml or --validate-all is required");
            PrintHelp();
            return 1;
        }

        try
        {
            // Load schema
            var schema = await JsonSchema.FromFileAsync(options.SchemaPath);

            if (!string.IsNullOrEmpty(options.ValidateAllPath))
            {
                // Validate all YAML files in directory
                return await ValidateDirectory(schema, options.ValidateAllPath);
            }
            else
            {
                // Validate single file
                return await ValidateFile(schema, options.YamlPath!);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    internal static async Task<int> ValidateFile(JsonSchema schema, string yamlPath)
    {
        var fileName = Path.GetFileName(yamlPath);

        try
        {
            if (!File.Exists(yamlPath))
            {
                Console.WriteLine($"FAIL: {fileName}");
                Console.WriteLine($"Error: File not found");
                return 1;
            }

            // Read YAML file
            var yamlContent = await File.ReadAllTextAsync(yamlPath);

            // Parse YAML to ensure it's valid
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            object? yamlObject;
            try
            {
                yamlObject = deserializer.Deserialize(yamlContent);
            }
            catch (YamlException ex)
            {
                Console.WriteLine($"FAIL: {fileName}");
                Console.WriteLine($"Error at line {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}");
                return 1;
            }

            // Convert YAML to JSON for schema validation
            var jsonCompatible = ConvertToJsonCompatible(yamlObject);
            var jsonContent = JsonSerializer.Serialize(jsonCompatible);

            // Validate against schema
            var errors = schema.Validate(jsonContent);

            if (errors.Count > 0)
            {
                Console.WriteLine($"FAIL: {fileName}");
                foreach (var error in errors)
                {
                    // Extract line number if available (best effort)
                    var errorPath = error.Path ?? "unknown";
                    Console.WriteLine($"Error at {errorPath}: {error.Kind} - {error.Property}");
                }
                return 1;
            }

            Console.WriteLine($"PASS: {fileName} is valid");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: {fileName}");
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    internal static async Task<int> ValidateDirectory(JsonSchema schema, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Console.Error.WriteLine($"Error: Directory not found: {directoryPath}");
            return 1;
        }

        var yamlFiles = Directory.GetFiles(directoryPath, "*.yaml", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(directoryPath, "*.yml", SearchOption.AllDirectories))
            .ToList();

        if (yamlFiles.Count == 0)
        {
            Console.Error.WriteLine($"Warning: No YAML files found in {directoryPath}");
            return 0;
        }

        Console.Error.WriteLine($"Validating {yamlFiles.Count} YAML files...");

        int failCount = 0;
        int passCount = 0;

        foreach (var yamlFile in yamlFiles)
        {
            var result = await ValidateFile(schema, yamlFile);
            if (result == 0)
            {
                passCount++;
            }
            else
            {
                failCount++;
            }
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine($"=== Validation Summary ===");
        Console.Error.WriteLine($"Total files: {yamlFiles.Count}");
        Console.Error.WriteLine($"Passed: {passCount}");
        Console.Error.WriteLine($"Failed: {failCount}");

        return failCount > 0 ? 1 : 0;
    }

    /// <summary>
    /// Converts YamlDotNet's untyped output to System.Text.Json-compatible objects.
    /// YamlDotNet returns Dictionary&lt;object, object&gt; for mappings, which must be
    /// converted to Dictionary&lt;string, object?&gt; for proper JSON serialization.
    /// </summary>
    internal static object? ConvertToJsonCompatible(object? yamlObject)
    {
        return yamlObject switch
        {
            Dictionary<object, object> dict => dict.ToDictionary(
                kvp => kvp.Key.ToString() ?? string.Empty,
                kvp => ConvertToJsonCompatible(kvp.Value)
            ),
            List<object> list => list.Select(ConvertToJsonCompatible).ToList(),
            _ => yamlObject // Scalars pass through (string, int, long, double, bool, null)
        };
    }

    internal static ValidatorOptions ParseArguments(string[] args)
    {
        var options = new ValidatorOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "--schema":
                    if (i + 1 < args.Length)
                        options.SchemaPath = args[++i];
                    break;

                case "--yaml":
                    if (i + 1 < args.Length)
                        options.YamlPath = args[++i];
                    break;

                case "--validate-all":
                    if (i + 1 < args.Length)
                        options.ValidateAllPath = args[++i];
                    break;

                default:
                    if (!arg.StartsWith("-"))
                    {
                        Console.Error.WriteLine($"Warning: Unknown argument '{arg}'");
                    }
                    break;
            }
        }

        return options;
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"YAML Validator - Schema validation tool for YAML dialogue files

Usage: dotnet run --project tools/YamlValidator/ -- [options]

Options:
  -h, --help                  Show this help
  --schema <path>             Path to JSON schema file (required)
  --yaml <path>               Path to YAML file to validate
  --validate-all <dir>        Validate all YAML files in directory (recursive)

Examples:
  # Validate single file
  dotnet run --project tools/YamlValidator/ -- \
    --schema ""tools/YamlSchemaGen/dialogue-schema.json"" \
    --yaml ""Game/YAML/Kojo/COM_K1_0.yaml""

  # Validate all files in directory (CI mode)
  dotnet run --project tools/YamlValidator/ -- \
    --schema ""tools/YamlSchemaGen/dialogue-schema.json"" \
    --validate-all ""Game/YAML/Kojo/""

Exit Codes:
  0 - All files valid
  1 - Validation errors found or execution error
");
    }
}

internal class ValidatorOptions
{
    public bool ShowHelp { get; set; }
    public string? SchemaPath { get; set; }
    public string? YamlPath { get; set; }
    public string? ValidateAllPath { get; set; }
}
