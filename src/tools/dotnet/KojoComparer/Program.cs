using System.Text.Json;

namespace KojoComparer;

/// <summary>
/// CLI entry point for KojoComparer.
/// Parses arguments and executes comparison.
/// </summary>
class Program
{
    /// <summary>
    /// State profiles for multi-state testing.
    /// Loaded from state-profiles.json at runtime.
    /// </summary>
    private static List<(string Name, Dictionary<string, int> State)>? _multiStateProfiles;

    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("KojoComparer - ERB==YAML equivalence testing\n");

        // Load state profiles from JSON
        LoadStateProfiles();

        // Parse command line arguments
        var arguments = ParseArguments(args);

        // Check for --inject-intro mode
        if (arguments.ContainsKey("inject-intro"))
        {
            return await RunIntroInjectionAsync(arguments);
        }

        // Check for --all mode
        if (arguments.ContainsKey("all"))
        {
            return await RunBatchModeAsync(arguments);
        }

        if (!arguments.ContainsKey("erb") || !arguments.ContainsKey("function") ||
            !arguments.ContainsKey("yaml") || !arguments.ContainsKey("talent"))
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Batch mode:       dotnet run -- --all [--multi-state]");
            Console.WriteLine("  Inject intro:     dotnet run -- --inject-intro --erb <path> --yaml <path>");
            Console.WriteLine("  Batch inject:     dotnet run -- --inject-intro --erb-dir <path> --yaml-dir <path>");
            Console.WriteLine("  Single mode:      dotnet run -- --erb <path> --function <name> --yaml <path> --talent <state>");
            Console.WriteLine("Example: --erb \"Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB\" --function \"@KOJO_MESSAGE_COM_K1_0_1\" --yaml \"tools/ErbToYaml.Tests/TestOutput/meirin_com0.yaml\" --talent \"TALENT:16=1\"");
            Console.WriteLine("\nFlags:");
            Console.WriteLine("  --multi-state     Test all state profiles (default, 恋人, 恋慕, 思慕) instead of just default state");
            return 1;
        }

        var erbPath = arguments["erb"];
        var functionName = arguments["function"];
        var yamlPath = arguments["yaml"];
        var talentStr = arguments["talent"];

        // Parse talent state (e.g., "TALENT:16=1,TALENT:3=0")
        var state = ParseTalentState(talentStr);

        try
        {
            // Initialize components
            var gamePath = Path.GetFullPath("Game");

            Console.WriteLine("Using in-process ErbEvaluator");
            IErbRunner erbRunner = new ErbEvaluator(gamePath);

            var yamlRunner = new YamlRunner();
            var normalizer = new OutputNormalizer();
            var diffEngine = new DiffEngine();

            Console.WriteLine($"Comparing: {functionName} ({talentStr})");
            Console.WriteLine($"ERB:  {erbPath}");
            Console.WriteLine($"YAML: {yamlPath}\n");

            // Execute ERB
            Console.WriteLine("Executing ERB...");
            var (erbOutput, erbDisplayModes) = await erbRunner.ExecuteAsync(erbPath, functionName, state);
            var normalizedErb = normalizer.Normalize(erbOutput);

            // Convert state to context format
            var context = StateConverter.ConvertStateToContext(state);

            // Render YAML
            Console.WriteLine("Rendering YAML...");
            var yamlResult = yamlRunner.RenderWithMetadata(yamlPath, context);
            var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));
            var normalizedYaml = normalizer.Normalize(yamlOutput);
            var yamlDisplayModes = yamlResult.DialogueLines.Select(dl => dl.DisplayMode).ToList();

            // Compare outputs
            Console.WriteLine("Comparing outputs...\n");
            var comparison = diffEngine.Compare(normalizedErb, normalizedYaml, displayModesA: erbDisplayModes, displayModesB: yamlDisplayModes);

            if (comparison.IsMatch)
            {
                Console.WriteLine("Result: PASS");
                Console.WriteLine("ERB output matches YAML output.");
                return 0;
            }
            else
            {
                Console.WriteLine("Result: FAIL");
                Console.WriteLine("ERB output does not match YAML output.\n");
                Console.WriteLine("Differences:");
                foreach (var diff in comparison.Differences)
                {
                    Console.WriteLine(diff);
                }
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    /// <summary>
    /// Runs batch verification mode (--all flag).
    /// Discovers all test cases and processes them with BatchProcessor.
    /// When --multi-state is specified, runs each state profile separately and reports per-profile results.
    /// </summary>
    static async Task<int> RunBatchModeAsync(Dictionary<string, string> arguments)
    {
        var useMultiState = arguments.ContainsKey("multi-state");

        Console.WriteLine(useMultiState
            ? "Running batch verification (--all --multi-state mode)...\n"
            : "Running batch verification (--all mode)...\n");

        var erbBasePath = Path.GetFullPath("Game/ERB/口上");
        var yamlBasePath = Path.GetFullPath("Game/YAML/Kojo");
        var mapFilePath = Path.GetFullPath("src/tools/kojo-mapper/com_file_map.json");

        var discovery = new FileDiscovery(erbBasePath, yamlBasePath, mapFilePath);
        var baseTestCases = discovery.DiscoverTestCases();

        Console.WriteLine($"Discovered {baseTestCases.Count} test cases\n");

        // Initialize components
        var gamePath = Path.GetFullPath("Game");

        Console.WriteLine("Using in-process ErbEvaluator + ErbBatchEvaluator");
        var evaluator = new ErbEvaluator(gamePath);
        IErbRunner erbRunner = evaluator;
        IBatchExecutor batchExecutor = new ErbBatchEvaluator(evaluator);

        var yamlRunner = new YamlRunner();
        var normalizer = new OutputNormalizer();
        var diffEngine = new DiffEngine();

        var batchProcessor = new BatchProcessor(erbRunner, yamlRunner, normalizer, diffEngine, batchExecutor);

        if (!useMultiState)
        {
            // Standard mode: run with empty state only (backward compatible)
            var report = await batchProcessor.ProcessAllAsync(baseTestCases);

            // Print summary
            Console.WriteLine("\n=== SUMMARY ===");
            Console.WriteLine($"{report.PassedTests}/{report.TotalTests} PASS");

            if (report.FailedTests > 0)
            {
                Console.WriteLine($"\n{report.FailedTests} FAILURES:");
                foreach (var failure in report.Failures)
                {
                    Console.WriteLine(failure);
                }
            }

            return report.FailedTests == 0 ? 0 : 1;
        }
        else
        {
            // Multi-state mode: run each state profile separately
            var profileResults = new List<(string ProfileName, int Passed, int Total)>();
            var allFailures = new List<string>();

            if (_multiStateProfiles == null || _multiStateProfiles.Count == 0)
            {
                Console.WriteLine("ERROR: No state profiles loaded.");
                return 1;
            }

            foreach (var (profileName, profileState) in _multiStateProfiles)
            {
                Console.WriteLine($"\n--- Testing state profile: {profileName} ---");

                // Clone test cases with this profile's state
                var testCasesWithState = baseTestCases.Select(tc => new TestCase
                {
                    ErbFile = tc.ErbFile,
                    FunctionName = tc.FunctionName,
                    YamlFile = tc.YamlFile,
                    State = new Dictionary<string, int>(profileState), // Apply profile state
                    ComId = tc.ComId,
                    CharacterId = tc.CharacterId,
                    SubFunctionIndex = tc.SubFunctionIndex
                }).ToList();

                var report = await batchProcessor.ProcessAllAsync(testCasesWithState);

                profileResults.Add((profileName, report.PassedTests, report.TotalTests));

                if (report.FailedTests > 0)
                {
                    allFailures.Add($"\n=== {profileName} profile failures ===");
                    allFailures.AddRange(report.Failures);
                }
            }

            // Print summary for all profiles
            Console.WriteLine("\n\n=== SUMMARY ===");
            foreach (var (profileName, passed, total) in profileResults)
            {
                Console.WriteLine($"{passed}/{total} PASS [{profileName}]");
            }

            var totalPassed = profileResults.Sum(r => r.Passed);
            var totalTests = profileResults.Sum(r => r.Total);
            Console.WriteLine($"{totalPassed}/{totalTests} PASS total");

            if (allFailures.Count > 0)
            {
                Console.WriteLine("\n=== FAILURES ===");
                foreach (var failure in allFailures)
                {
                    Console.WriteLine(failure);
                }
            }

            return totalPassed == totalTests ? 0 : 1;
        }
    }

    /// <summary>
    /// Runs intro line injection mode (--inject-intro flag).
    /// Injects intro lines from ERB files into YAML files.
    /// </summary>
    static async Task<int> RunIntroInjectionAsync(Dictionary<string, string> arguments)
    {
        Console.WriteLine("Running intro line injection...\n");

        var injector = new IntroLineInjector();

        try
        {
            // Check for batch mode (--erb-dir and --yaml-dir)
            if (arguments.ContainsKey("erb-dir") && arguments.ContainsKey("yaml-dir"))
            {
                var erbDir = arguments["erb-dir"];
                var yamlDir = arguments["yaml-dir"];

                var mapFilePath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "..", "..", "..", "..", "..", "..", "kojo-mapper", "com_file_map.json");

                Console.WriteLine($"ERB directory:  {erbDir}");
                Console.WriteLine($"YAML directory: {yamlDir}");
                Console.WriteLine($"Map file:       {mapFilePath}\n");

                await injector.BatchInjectAsync(erbDir, yamlDir, mapFilePath);
                return 0;
            }

            // Single file mode (--erb and --yaml)
            if (arguments.ContainsKey("erb") && arguments.ContainsKey("yaml"))
            {
                var erbPath = arguments["erb"];
                var yamlPath = arguments["yaml"];

                Console.WriteLine($"ERB:  {erbPath}");
                Console.WriteLine($"YAML: {yamlPath}\n");

                await injector.InjectAsync(erbPath, yamlPath);
                return 0;
            }

            // Missing required arguments
            Console.WriteLine("ERROR: --inject-intro requires either:");
            Console.WriteLine("  Single mode: --erb <path> --yaml <path>");
            Console.WriteLine("  Batch mode:  --erb-dir <path> --yaml-dir <path>");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    /// <summary>
    /// Parses command line arguments into a dictionary.
    /// Supports both key-value pairs (--key value) and boolean flags (--flag).
    /// </summary>
    static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i].Substring(2);

                // Check if next argument exists and is not a flag
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    // Key-value pair
                    var value = args[i + 1];
                    result[key] = value;
                    i++; // Skip next argument (it's the value)
                }
                else
                {
                    // Boolean flag (no value)
                    result[key] = "true";
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Parses talent state string into dictionary.
    /// Example: "TALENT:16=1,TALENT:3=0" -> {"TALENT:TARGET:16": 1, "TALENT:TARGET:3": 0}
    /// </summary>
    static Dictionary<string, int> ParseTalentState(string talentStr)
    {
        var state = new Dictionary<string, int>();

        var pairs = talentStr.Split(',');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                if (int.TryParse(parts[1].Trim(), out var value))
                {
                    // Convert "TALENT:16" to "TALENT:TARGET:16"
                    if (!key.Contains("TARGET"))
                    {
                        var keyParts = key.Split(':');
                        if (keyParts.Length == 2)
                        {
                            key = $"{keyParts[0]}:TARGET:{keyParts[1]}";
                        }
                    }
                    state[key] = value;
                }
            }
        }

        return state;
    }

    /// <summary>
    /// Loads state profiles from state-profiles.json.
    /// Falls back to default profile if file not found or invalid.
    /// </summary>
    static void LoadStateProfiles()
    {
        // Default profile (always available as fallback)
        var defaultProfiles = new List<(string Name, Dictionary<string, int> State)>
        {
            ("default", new Dictionary<string, int>())
        };

        // Try to load from JSON file
        string? jsonPath = null;

        // Check in same directory as executable
        var exeDir = Path.GetDirectoryName(AppContext.BaseDirectory);
        if (exeDir != null)
        {
            var candidatePath = Path.Combine(exeDir, "state-profiles.json");
            if (File.Exists(candidatePath))
            {
                jsonPath = candidatePath;
            }
        }

        // Check in project directory (for development)
        if (jsonPath == null)
        {
            var projectPath = Path.GetFullPath("tools/KojoComparer/state-profiles.json");
            if (File.Exists(projectPath))
            {
                jsonPath = projectPath;
            }
        }

        if (jsonPath == null)
        {
            Console.WriteLine("Warning: state-profiles.json not found. Using default profile only.");
            _multiStateProfiles = defaultProfiles;
            return;
        }

        try
        {
            var jsonText = File.ReadAllText(jsonPath);
            var config = JsonSerializer.Deserialize<StateProfileConfig>(jsonText, s_caseInsensitiveOptions);

            if (config?.Profiles == null || config.Profiles.Count == 0)
            {
                Console.WriteLine("Warning: No profiles found in state-profiles.json. Using default profile only.");
                _multiStateProfiles = defaultProfiles;
                return;
            }

            // Convert to internal format
            _multiStateProfiles = config.Profiles
                .Select(p => (p.Name ?? "unnamed", p.State ?? new Dictionary<string, int>()))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load state-profiles.json: {ex.Message}");
            Console.WriteLine("Using default profile only.");
            _multiStateProfiles = defaultProfiles;
        }
    }
}

/// <summary>
/// Configuration for state profiles (deserialized from JSON).
/// </summary>
class StateProfileConfig
{
    public List<StateProfile>? Profiles { get; set; }
}

/// <summary>
/// Individual state profile entry.
/// </summary>
class StateProfile
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, int>? State { get; set; }
}
