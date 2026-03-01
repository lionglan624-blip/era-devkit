using System.CommandLine;
using System.Diagnostics;

namespace KojoQualityValidator;

internal class Program
{
    internal static int Main(string[] args)
    {
        var filesOption = new Option<string?>(
            "--files",
            description: "File or pattern to validate");

        var diffOption = new Option<string?>(
            "--diff",
            description: "Validate files changed since commit (e.g., HEAD~1)");

        var minEntriesOption = new Option<int?>(
            "--min-entries",
            getDefaultValue: () => null,
            description: "Minimum number of entries required");

        var minLinesOption = new Option<int?>(
            "--min-lines",
            getDefaultValue: () => null,
            description: "Minimum lines per entry required");

        var rootCommand = new RootCommand("Kojo quality validator")
        {
            filesOption,
            diffOption,
            minEntriesOption,
            minLinesOption
        };

        rootCommand.SetHandler(
            (files, diff, minEntries, minLines) =>
            {
                Environment.ExitCode = HandleValidation(files, diff, minEntries, minLines);
            },
            filesOption,
            diffOption,
            minEntriesOption,
            minLinesOption);

        rootCommand.Invoke(args);
        return Environment.ExitCode;
    }

    static int HandleValidation(string? files, string? diff, int? minEntries, int? minLines)
    {
        try
        {
            List<string> filesToValidate;

            if (!string.IsNullOrEmpty(diff))
            {
                filesToValidate = GetChangedFiles(diff);
            }
            else if (!string.IsNullOrEmpty(files))
            {
                filesToValidate = GetFilesFromPattern(files);
            }
            else
            {
                Console.Error.WriteLine("Error: Either --files or --diff must be specified");
                return 1;
            }

            if (filesToValidate.Count == 0)
            {
                Console.WriteLine("Validating 0 files...");
                Console.WriteLine("\nResult: 0/0 PASS, 0/0 FAIL");
                return 0;
            }

            var validator = new QualityValidator();
            // If neither flag is specified, use defaults (4, 4)
            // If only one is specified, set the other to 0 (skip validation)
            int effectiveMinEntries;
            int effectiveMinLines;

            if (minEntries.HasValue && minLines.HasValue)
            {
                // Both specified
                effectiveMinEntries = minEntries.Value;
                effectiveMinLines = minLines.Value;
            }
            else if (minEntries.HasValue)
            {
                // Only entries specified - skip line validation
                effectiveMinEntries = minEntries.Value;
                effectiveMinLines = 0;
            }
            else if (minLines.HasValue)
            {
                // Only lines specified - skip entry validation
                effectiveMinEntries = 0;
                effectiveMinLines = minLines.Value;
            }
            else
            {
                // Neither specified - use defaults
                effectiveMinEntries = 4;
                effectiveMinLines = 4;
            }

            var rule = new QualityRule(MinEntries: effectiveMinEntries, MinLinesPerEntry: effectiveMinLines);
            var results = new List<ValidationResult>();

            foreach (var file in filesToValidate)
            {
                var result = validator.Validate(file, rule);
                results.Add(result);
            }

            PrintResults(results);

            return results.All(r => r.IsValid) ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static List<string> GetChangedFiles(string commit)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"diff --name-only {commit} -- *.yaml",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start git process");

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git diff failed with exit code {process.ExitCode}");

        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(f => f.EndsWith(".yaml"))
            .Select(f => Path.GetFullPath(f))
            .ToList();
    }

    static List<string> GetFilesFromPattern(string pattern)
    {
        var fullPath = Path.GetFullPath(pattern);

        if (File.Exists(fullPath))
        {
            return new List<string> { fullPath };
        }

        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        var searchPattern = Path.GetFileName(fullPath);

        if (searchPattern.Contains('*') || searchPattern.Contains('?'))
        {
            return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories).ToList();
        }

        if (Directory.Exists(directory))
        {
            return Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories).ToList();
        }

        return new List<string>();
    }

    static void PrintResults(List<ValidationResult> results)
    {
        Console.WriteLine($"Validating {results.Count} files...\n");

        var passCount = 0;
        foreach (var result in results)
        {
            var icon = result.IsValid ? "✓" : "✗";
            var filename = Path.GetFileName(result.FilePath);

            if (result.IsValid)
            {
                Console.WriteLine($"{icon} {filename,-30} {result.EntryCount} entries × 4+ lines");
                passCount++;
            }
            else
            {
                Console.WriteLine($"{icon} {filename,-30} {string.Join(", ", result.Errors)}");
            }
        }

        Console.WriteLine($"\nResult: {passCount}/{results.Count} PASS, {results.Count - passCount}/{results.Count} FAIL");
    }
}
