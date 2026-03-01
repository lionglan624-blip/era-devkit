using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SaveAnalyzer.Tests")]

namespace SaveAnalyzer;

internal class Program
{
    private static readonly JsonSerializerOptions s_indentedOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        string? filePath = null;
        string? filterVariable = null;
        string? filterCharacter = null;
        bool headerOnly = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;

                case "-f":
                case "--filter":
                    if (i + 1 < args.Length)
                        filterVariable = args[++i];
                    break;

                case "-c":
                case "--character":
                    if (i + 1 < args.Length)
                        filterCharacter = args[++i];
                    break;

                case "--header":
                    headerOnly = true;
                    break;

                default:
                    if (!args[i].StartsWith("-"))
                        filePath = args[i];
                    break;
            }
        }

        if (string.IsNullOrEmpty(filePath))
        {
            Console.Error.WriteLine("Error: No save file specified");
            return 1;
        }

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File not found: {filePath}");
            return 1;
        }

        try
        {
            using var reader = new SaveReader(filePath);
            var data = reader.Read();

            object output;

            if (headerOnly)
            {
                output = new
                {
                    file = Path.GetFileName(filePath),
                    header = data.Header,
                    emueraVersion = data.EmueraVersion
                };
            }
            else
            {
                // Apply filters
                var globals = FilterGlobals(data.GlobalArrays, filterVariable);
                var globalStrings = data.GlobalStringArrays.Count > 0 ? data.GlobalStringArrays : null;
                var characters = FilterCharacters(data.Characters, filterCharacter, filterVariable);

                // When filtering by character only, suppress global data
                bool showGlobals = string.IsNullOrEmpty(filterCharacter) || !string.IsNullOrEmpty(filterVariable);

                output = new
                {
                    file = Path.GetFileName(filePath),
                    header = data.Header,
                    emueraVersion = data.EmueraVersion,
                    globals = showGlobals && globals?.Count > 0 ? globals : null,
                    globalStrings = showGlobals ? globalStrings : null,
                    characters = characters?.Count > 0 ? characters : null,
                    extendedData = showGlobals && data.ExtendedData.Count > 0 ? data.ExtendedData : null
                };
            }

            var json = JsonSerializer.Serialize(output, s_indentedOptions);
            Console.WriteLine(json);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading save file: {ex.Message}");
            return 1;
        }
    }

    internal static Dictionary<string, Dictionary<int, long>>? FilterGlobals(
        Dictionary<string, Dictionary<int, long>> globals,
        string? filterVariable)
    {
        if (string.IsNullOrEmpty(filterVariable))
            return globals;

        // Parse filter: "FLAG" or "FLAG:26"
        var parts = filterVariable.Split(':');
        var varName = parts[0].ToUpperInvariant();
        int? index = parts.Length > 1 && int.TryParse(parts[1], out var idx) ? idx : null;

        var result = new Dictionary<string, Dictionary<int, long>>();

        foreach (var (name, values) in globals)
        {
            if (!name.Equals(varName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (index.HasValue)
            {
                if (values.TryGetValue(index.Value, out var val))
                {
                    result[name] = new Dictionary<int, long> { { index.Value, val } };
                }
            }
            else
            {
                result[name] = values;
            }
        }

        return result.Count > 0 ? result : null;
    }

    internal static List<object>? FilterCharacters(
        List<CharacterData> characters,
        string? filterCharacter,
        string? filterVariable)
    {
        var result = new List<object>();

        foreach (var character in characters)
        {
            // Apply character name filter
            if (!string.IsNullOrEmpty(filterCharacter))
            {
                if (!character.Name.Contains(filterCharacter) &&
                    !character.CallName.Contains(filterCharacter))
                    continue;
            }

            // Apply variable filter
            Dictionary<string, Dictionary<int, long>>? filteredArrays = null;
            if (!string.IsNullOrEmpty(filterVariable))
            {
                var parts = filterVariable.Split(':');
                var varName = parts[0].ToUpperInvariant();
                int? index = parts.Length > 1 && int.TryParse(parts[1], out var idx) ? idx : null;

                filteredArrays = new Dictionary<string, Dictionary<int, long>>();
                foreach (var (name, values) in character.Arrays)
                {
                    if (!name.Equals(varName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (index.HasValue)
                    {
                        if (values.TryGetValue(index.Value, out var val))
                        {
                            filteredArrays[name] = new Dictionary<int, long> { { index.Value, val } };
                        }
                    }
                    else
                    {
                        filteredArrays[name] = values;
                    }
                }

                if (filteredArrays.Count == 0)
                    continue; // Skip character if filter doesn't match
            }

            result.Add(new
            {
                id = character.Id,
                name = character.Name,
                callName = character.CallName,
                isAssi = character.IsAssi,
                no = character.No,
                arrays = filteredArrays ?? (character.Arrays.Count > 0 ? character.Arrays : null)
            });
        }

        return result;
    }

    static void PrintUsage()
    {
        Console.WriteLine(@"SaveAnalyzer - ERA Save File Analyzer

Usage: SaveAnalyzer [options] <save-file>

Options:
  -h, --help              Show this help
  -f, --filter <var>      Filter by variable name (e.g., CFLAG or CFLAG:297)
  -c, --character <name>  Filter by character name
  --header                Show header only

Examples:
  SaveAnalyzer save.sav                          # Full dump
  SaveAnalyzer --header save.sav                 # Header only
  SaveAnalyzer --filter ""FLAG"" save.sav          # All FLAG values
  SaveAnalyzer --filter ""FLAG:26"" save.sav       # FLAG[26] only
  SaveAnalyzer --character ""咲夜"" save.sav       # Filter by character
  SaveAnalyzer -c ""咲夜"" -f ""CFLAG:2"" save.sav   # Combined filters
");
    }
}
