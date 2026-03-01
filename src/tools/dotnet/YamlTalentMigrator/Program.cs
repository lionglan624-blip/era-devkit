using System.Text.Json;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("YamlTalentMigrator.Tests")]

namespace YamlTalentMigrator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        bool dryRun = args.Contains("--dry-run");

        // Allow custom path via --path argument
        string? customPath = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--path")
            {
                customPath = args[i + 1];
                break;
            }
        }

        // Parse --config argument
        string? configPath = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--config")
            {
                configPath = args[i + 1];
                break;
            }
        }

        // Load mapping configuration
        var branchConditions = LoadMappingConfig(configPath);

        string kojoPath;
        if (customPath != null)
        {
            kojoPath = Path.GetFullPath(customPath);
        }
        else
        {
            kojoPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "Game", "YAML", "Kojo");
            kojoPath = Path.GetFullPath(kojoPath);
        }

        if (!Directory.Exists(kojoPath))
        {
            Console.Error.WriteLine($"Error: Kojo directory not found at {kojoPath}");
            return 1;
        }

        Console.WriteLine($"Scanning directory: {kojoPath}");
        Console.WriteLine($"Mode: {(dryRun ? "DRY RUN" : "LIVE")}");
        Console.WriteLine();

        var yamlFiles = Directory.GetFiles(kojoPath, "*.yaml", SearchOption.AllDirectories);
        int filesProcessed = 0;
        int filesModified = 0;
        int branchesUpdated = 0;

        foreach (var file in yamlFiles)
        {
            var result = await ProcessFile(file, dryRun, branchConditions);
            filesProcessed++;

            if (result.Modified)
            {
                filesModified++;
                branchesUpdated += result.BranchesUpdated;

                Console.WriteLine($"[{(dryRun ? "DRY" : "MOD")}] {Path.GetRelativePath(kojoPath, file)}");
                Console.WriteLine($"      Updated {result.BranchesUpdated} branch(es)");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Summary:");
        Console.WriteLine($"  Files scanned: {filesProcessed}");
        Console.WriteLine($"  Files modified: {filesModified}");
        Console.WriteLine($"  Branches updated: {branchesUpdated}");

        return 0;
    }

    internal static Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>> LoadMappingConfig(string? configPath)
    {
        try
        {
            // Try custom path first (if specified)
            if (configPath != null)
            {
                if (!File.Exists(configPath))
                {
                    Console.Error.WriteLine($"Error: Custom config file not found at {configPath}");
                    Environment.Exit(1);
                }

                string json = File.ReadAllText(configPath);
                var config = ParseAndValidateConfig(json, configPath);
                return config;
            }

            // Try default location (talent-mapping.json in tool directory)
            string defaultPath = Path.Combine(AppContext.BaseDirectory, "talent-mapping.json");
            if (File.Exists(defaultPath))
            {
                string json = File.ReadAllText(defaultPath);
                var config = ParseAndValidateConfig(json, defaultPath);
                return config;
            }

            // Fallback to embedded default (F750 mappings)
            return new Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>>
            {
                { 0, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                    {
                        { "TALENT", new Dictionary<int, Dictionary<string, int>>
                            {
                                { 16, new Dictionary<string, int> { { "ne", 0 } } }
                            }
                        }
                    }
                },
                { 1, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                    {
                        { "TALENT", new Dictionary<int, Dictionary<string, int>>
                            {
                                { 3, new Dictionary<string, int> { { "ne", 0 } } }
                            }
                        }
                    }
                },
                { 2, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                    {
                        { "TALENT", new Dictionary<int, Dictionary<string, int>>
                            {
                                { 17, new Dictionary<string, int> { { "ne", 0 } } }
                            }
                        }
                    }
                }
            };
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Error: Malformed JSON in config file: {ex.Message}");
            Environment.Exit(1);
            throw; // Unreachable, but satisfies compiler
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading config: {ex.Message}");
            Environment.Exit(1);
            throw; // Unreachable, but satisfies compiler
        }
    }

    internal static Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>> ParseAndValidateConfig(
        string json,
        string configPath)
    {
        // Parse JSON with string keys
        var configWithStringKeys = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>>(json);

        if (configWithStringKeys == null)
        {
            Console.Error.WriteLine($"Error: Failed to parse config file {configPath}");
            Environment.Exit(1);
        }

        // Validate required keys
        if (!configWithStringKeys.ContainsKey("0") || !configWithStringKeys.ContainsKey("1") || !configWithStringKeys.ContainsKey("2"))
        {
            Console.Error.WriteLine($"Error: Invalid config format in {configPath}. Missing required branch keys 0, 1, or 2");
            Environment.Exit(1);
        }

        // Convert string keys to int keys for C# dictionary
        var config = new Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>>();

        foreach (var kvp in configWithStringKeys)
        {
            if (!int.TryParse(kvp.Key, out var branchIndex))
            {
                Console.Error.WriteLine($"Error: Invalid branch key '{kvp.Key}' in {configPath}. Expected numeric string");
                Environment.Exit(1);
            }

            var talentDict = new Dictionary<string, Dictionary<int, Dictionary<string, int>>>();

            foreach (var talentKvp in kvp.Value)
            {
                var indexDict = new Dictionary<int, Dictionary<string, int>>();

                foreach (var indexKvp in talentKvp.Value)
                {
                    if (!int.TryParse(indexKvp.Key, out var talentIndex))
                    {
                        Console.Error.WriteLine($"Error: Invalid TALENT index '{indexKvp.Key}' in {configPath}. Expected numeric string");
                        Environment.Exit(1);
                    }

                    indexDict[talentIndex] = indexKvp.Value;
                }

                talentDict[talentKvp.Key] = indexDict;
            }

            config[branchIndex] = talentDict;
        }

        return config;
    }

    private static async Task<(bool Modified, int BranchesUpdated)> ProcessFile(
        string filePath,
        bool dryRun,
        Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>> branchConditions)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath);

            var input = new StringReader(content);
            var yaml = new YamlStream();
            yaml.Load(input);

            if (yaml.Documents.Count == 0)
            {
                return (false, 0);
            }

            var root = yaml.Documents[0].RootNode as YamlMappingNode;
            if (root == null)
            {
                return (false, 0);
            }

            // Check if "branches" exists
            if (!root.Children.ContainsKey(new YamlScalarNode("branches")))
            {
                return (false, 0);
            }

            var branches = root.Children[new YamlScalarNode("branches")] as YamlSequenceNode;
            if (branches == null)
            {
                return (false, 0);
            }

            // Check if any branch needs updating
            bool needsUpdate = false;
            int branchIndex = 0;
            foreach (var branchNode in branches.Children)
            {
                var branch = branchNode as YamlMappingNode;
                if (branch == null) continue;

                if (ShouldUpdateBranch(branch, branchIndex))
                {
                    needsUpdate = true;
                    break;
                }
                branchIndex++;
            }

            if (!needsUpdate)
            {
                return (false, 0);
            }

            // Perform updates using YamlDotNet serialization
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            // Parse the file as dynamic object
            var document = deserializer.Deserialize<Dictionary<string, object>>(content);

            if (document.TryGetValue("branches", out var branchesObj) && branchesObj is List<object> branchList)
            {
                int updatedCount = 0;
                for (int i = 0; i < branchList.Count && i < 4; i++)
                {
                    if (branchList[i] is Dictionary<object, object> branch)
                    {
                        if (ShouldUpdateBranchDict(branch, i))
                        {
                            if (i < 3 && branchConditions.ContainsKey(i))
                            {
                                branch["condition"] = branchConditions[i];
                            }
                            else
                            {
                                branch["condition"] = new Dictionary<string, object>();
                            }
                            updatedCount++;
                        }
                    }
                }

                if (updatedCount > 0)
                {
                    if (!dryRun)
                    {
                        string updatedContent = serializer.Serialize(document);
                        await File.WriteAllTextAsync(filePath, updatedContent);
                    }
                    return (true, updatedCount);
                }
            }

            return (false, 0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error processing {filePath}: {ex.Message}");
            return (false, 0);
        }
    }

    private static bool ShouldUpdateBranch(YamlMappingNode branch, int index)
    {
        // Only update branches 0-3
        if (index > 3) return false;

        // Check if condition exists
        var conditionKey = new YamlScalarNode("condition");
        if (!branch.Children.ContainsKey(conditionKey))
        {
            return true; // Missing condition
        }

        var condition = branch.Children[conditionKey];

        // Check if condition is null
        if (condition is YamlScalarNode scalar &&
            (scalar.Value == null || scalar.Value == "null" || scalar.Value == "~"))
        {
            return true;
        }

        // Check if condition is empty mapping
        if (condition is YamlMappingNode mapping && mapping.Children.Count == 0)
        {
            return true;
        }

        return false; // Has non-empty condition
    }

    private static bool ShouldUpdateBranchDict(Dictionary<object, object> branch, int index)
    {
        // Only update branches 0-3
        if (index > 3) return false;

        // Check if condition exists
        if (!branch.ContainsKey("condition"))
        {
            return true;
        }

        var condition = branch["condition"];

        // Check if condition is null
        if (condition == null)
        {
            return true;
        }

        // Check if condition is empty dictionary
        if (condition is Dictionary<object, object> dict && dict.Count == 0)
        {
            return true;
        }

        return false;
    }
}
