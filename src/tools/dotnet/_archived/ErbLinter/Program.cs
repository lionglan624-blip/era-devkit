using ErbLinter.Analyzer;
using ErbLinter.Config;
using ErbLinter.Data;
using ErbLinter.Parser;
using ErbLinter.Reporter;

namespace ErbLinter;

class Program
{
    static int Main(string[] args)
    {
        // Check for subcommand
        if (args.Length > 0 && args[0] == "callgraph")
        {
            var options = ParseCallGraphArguments(args.Skip(1).ToArray());
            if (options.ShowHelp)
            {
                PrintCallGraphHelp();
                return 0;
            }
            try
            {
                return RunCallGraph(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        if (args.Length > 0 && args[0] == "impact")
        {
            var options = ParseImpactArguments(args.Skip(1).ToArray());
            if (options.ShowHelp)
            {
                PrintImpactHelp();
                return 0;
            }
            try
            {
                return RunImpact(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        var lintOptions = ParseArguments(args);

        if (lintOptions.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return Run(lintOptions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static int Run(LinterOptions options)
    {
        var scanner = new FileScanner();
        var issues = new List<Issue>();
        var functionIndex = new FunctionIndex();

        // Scan for ERB files
        var files = scanner.Scan(options.Path).ToList();
        Console.Error.WriteLine($"Scanning {files.Count} ERB files...");

        // First pass: build function index and run per-file analysis
        var fileContents = new Dictionary<string, string[]>();
        foreach (var file in files)
        {
            try
            {
                var lines = scanner.ReadFileLines(file);
                fileContents[file] = lines;

                // Build function index
                functionIndex.AddFromFile(file, lines);

                // Run per-file syntax analysis
                var syntaxAnalyzer = new SyntaxAnalyzer();
                issues.AddRange(syntaxAnalyzer.Analyze(file, lines));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }

        Console.Error.WriteLine($"Found {functionIndex.Count} function definitions...");

        // Load CSV definitions if path provided
        var variableRegistry = new VariableRegistry();
        if (!string.IsNullOrEmpty(options.CsvPath))
        {
            variableRegistry.LoadFromDirectory(options.CsvPath);
            Console.Error.WriteLine($"Loaded {variableRegistry.FlagCount} FLAG, {variableRegistry.CFlagCount} CFLAG definitions...");
        }

        // Second pass: function analysis (requires complete index)
        var functionAnalyzer = new FunctionAnalyzer();

        // Check for duplicate functions
        issues.AddRange(functionAnalyzer.AnalyzeIndex(functionIndex));

        // Check CALL targets (optional - can generate many warnings)
        // foreach (var (file, lines) in fileContents)
        // {
        //     issues.AddRange(functionAnalyzer.AnalyzeCalls(file, lines, functionIndex));
        // }

        // Style analysis (kojo naming conventions)
        var styleAnalyzer = new StyleAnalyzer();
        issues.AddRange(styleAnalyzer.Analyze(functionIndex));

        // Variable analysis (requires CSV definitions)
        if (variableRegistry.FlagCount > 0 || variableRegistry.CFlagCount > 0)
        {
            var variableAnalyzer = new VariableAnalyzer(variableRegistry);
            foreach (var (file, lines) in fileContents)
            {
                issues.AddRange(variableAnalyzer.Analyze(file, lines));
            }
        }

        // Dead code analysis (optional)
        if (options.DeadCode)
        {
            Console.Error.WriteLine("Analyzing dead code...");
            var deadCodeAnalyzer = new DeadCodeAnalyzer();
            issues.AddRange(deadCodeAnalyzer.Analyze(functionIndex, fileContents, options.EntryPointsFile));
        }

        // Filter by level
        var filtered = issues.Where(i => i.Level <= options.MinLevel).ToList();

        // Output results
        TextWriter output = options.OutputFile != null
            ? new StreamWriter(options.OutputFile)
            : Console.Out;

        try
        {
            if (options.Format == OutputFormat.Json)
            {
                new JsonReporter().Report(filtered, output);
            }
            else
            {
                new TextReporter(!options.NoColor).Report(filtered, output);
            }
        }
        finally
        {
            if (options.OutputFile != null)
            {
                output.Dispose();
            }
        }

        // Return error code if any errors found
        return filtered.Any(i => i.Level == IssueLevel.Error) ? 1 : 0;
    }

    static LinterOptions ParseArguments(string[] args)
    {
        var options = new LinterOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;

                case "-f":
                case "--format":
                    if (i + 1 < args.Length)
                    {
                        options.Format = args[++i].ToLower() switch
                        {
                            "json" => OutputFormat.Json,
                            _ => OutputFormat.Text
                        };
                    }
                    break;

                case "-l":
                case "--level":
                    if (i + 1 < args.Length)
                    {
                        options.MinLevel = args[++i].ToLower() switch
                        {
                            "error" => IssueLevel.Error,
                            "warning" => IssueLevel.Warning,
                            _ => IssueLevel.Info
                        };
                    }
                    break;

                case "--no-color":
                    options.NoColor = true;
                    break;

                case "-c":
                case "--csv":
                    if (i + 1 < args.Length)
                        options.CsvPath = args[++i];
                    break;

                case "--dead-code":
                    options.DeadCode = true;
                    break;

                case "--entry-points":
                    if (i + 1 < args.Length)
                        options.EntryPointsFile = args[++i];
                    break;

                default:
                    if (!arg.StartsWith("-"))
                        options.Path = arg;
                    break;
            }
        }

        return options;
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"ERB Linter - Static analysis tool for ERB scripts

Usage: erb-linter [options] <path>
       erb-linter callgraph [options] <path>
       erb-linter impact [options] <path>

Options:
  -h, --help              Show this help
  -o, --output <file>     Output to file (default: stdout)
  -f, --format <fmt>      Output format: text (default), json
  -l, --level <level>     Minimum level: error, warning, info (default)
  -c, --csv <path>        CSV directory for variable definitions
  --no-color              Disable colored output
  --dead-code             Enable dead code detection (DEAD001)
  --entry-points <file>   Custom entry points file for dead code detection

Subcommands:
  callgraph               Generate function call graph (DOT format)
                          Use 'erb-linter callgraph --help' for options
  impact                  Analyze change impact for a function
                          Use 'erb-linter impact --help' for options

Examples:
  erb-linter Game/ERB/
  erb-linter --format json -o report.json Game/ERB/
  erb-linter --level error Game/ERB/SYSTEM.ERB
  erb-linter --dead-code Game/ERB/
  erb-linter --dead-code --entry-points custom.txt Game/ERB/
  erb-linter callgraph Game/ERB/
  erb-linter impact --function SHOW_STATUS Game/ERB/
");
    }

    static int RunCallGraph(LinterOptions options)
    {
        var scanner = new FileScanner();
        var functionIndex = new FunctionIndex();

        // Scan for ERB files
        var files = scanner.Scan(options.Path).ToList();
        Console.Error.WriteLine($"Scanning {files.Count} ERB files...");

        // Build function index and collect file contents
        var fileContents = new Dictionary<string, string[]>();
        foreach (var file in files)
        {
            try
            {
                var lines = scanner.ReadFileLines(file);
                fileContents[file] = lines;
                functionIndex.AddFromFile(file, lines);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }

        Console.Error.WriteLine($"Found {functionIndex.Count} function definitions...");

        // Build call graph
        var analyzer = new CallGraphAnalyzer();
        analyzer.BuildGraph(functionIndex, fileContents);

        var (funcCount, edgeCount) = analyzer.GetStats();
        Console.Error.WriteLine($"Built call graph: {funcCount} functions, {edgeCount} edges");

        // Generate DOT output
        var dot = analyzer.GenerateDot(options.CallGraphRoot, options.CallGraphDepth);

        // Output
        TextWriter output = options.OutputFile != null
            ? new StreamWriter(options.OutputFile)
            : Console.Out;

        try
        {
            output.Write(dot);
        }
        finally
        {
            if (options.OutputFile != null)
            {
                output.Dispose();
                Console.Error.WriteLine($"Output written to {options.OutputFile}");
            }
        }

        return 0;
    }

    static LinterOptions ParseCallGraphArguments(string[] args)
    {
        var options = new LinterOptions { CallGraph = true };

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;

                case "--root":
                    if (i + 1 < args.Length)
                        options.CallGraphRoot = args[++i];
                    break;

                case "--depth":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var depth))
                        options.CallGraphDepth = depth;
                    break;

                default:
                    if (!arg.StartsWith("-"))
                        options.Path = arg;
                    break;
            }
        }

        return options;
    }

    static void PrintCallGraphHelp()
    {
        Console.WriteLine(@"ERB Linter - Call Graph Generator

Usage: erb-linter callgraph [options] <path>

Generates a function call graph in DOT format (Graphviz compatible).

Options:
  -h, --help              Show this help
  -o, --output <file>     Output to file (default: stdout)
  --root <function>       Filter to subgraph reachable from function
  --depth <N>             Limit traversal depth from root

Examples:
  erb-linter callgraph Game/ERB/
  erb-linter callgraph -o callgraph.dot Game/ERB/
  erb-linter callgraph --root EVENTFIRST Game/ERB/
  erb-linter callgraph --root SHOW_STATUS --depth 3 Game/ERB/

Output can be rendered with Graphviz:
  dot -Tpng callgraph.dot -o callgraph.png
  dot -Tsvg callgraph.dot -o callgraph.svg
");
    }

    static int RunImpact(LinterOptions options)
    {
        if (string.IsNullOrEmpty(options.ImpactFunction))
        {
            Console.Error.WriteLine("Error: --function is required for impact analysis");
            PrintImpactHelp();
            return 1;
        }

        var scanner = new FileScanner();
        var functionIndex = new FunctionIndex();

        // Scan for ERB files
        var files = scanner.Scan(options.Path).ToList();
        Console.Error.WriteLine($"Scanning {files.Count} ERB files...");

        // Build function index and collect file contents
        var fileContents = new Dictionary<string, string[]>();
        foreach (var file in files)
        {
            try
            {
                var lines = scanner.ReadFileLines(file);
                fileContents[file] = lines;
                functionIndex.AddFromFile(file, lines);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }

        Console.Error.WriteLine($"Found {functionIndex.Count} function definitions...");

        // Build call graph
        var analyzer = new CallGraphAnalyzer();
        analyzer.BuildGraph(functionIndex, fileContents);

        // Check if function exists
        var targetFunc = options.ImpactFunction;
        var funcExists = functionIndex.GetAllFunctions()
            .Any(f => string.Equals(f.Name, targetFunc, StringComparison.OrdinalIgnoreCase));

        if (!funcExists)
        {
            Console.Error.WriteLine($"Warning: Function '{targetFunc}' not found in codebase");
        }

        // Output
        TextWriter output = options.OutputFile != null
            ? new StreamWriter(options.OutputFile)
            : Console.Out;

        try
        {
            if (options.ReverseGraph)
            {
                // DOT format output
                var dot = analyzer.GenerateReverseGraph(targetFunc, options.CallGraphDepth);
                output.Write(dot);
            }
            else
            {
                // Text report
                var impacted = analyzer.GetImpactedFunctions(targetFunc, options.CallGraphDepth);

                output.WriteLine($"=== Impact Analysis: {targetFunc} ===");
                output.WriteLine();

                if (impacted.Count == 0)
                {
                    output.WriteLine("No callers found for this function.");
                }
                else
                {
                    // Group by depth
                    var byDepth = impacted.GroupBy(kv => kv.Value)
                        .OrderBy(g => g.Key);

                    foreach (var group in byDepth)
                    {
                        output.WriteLine($"--- Depth {group.Key} ({group.Count()} functions) ---");
                        foreach (var (func, _) in group.OrderBy(kv => kv.Key))
                        {
                            var file = analyzer.GetFunctionFile(func, functionIndex);
                            var fileInfo = file != null ? $" ({Path.GetFileName(file)})" : "";
                            output.WriteLine($"  {func}{fileInfo}");
                        }
                        output.WriteLine();
                    }

                    // Summary
                    output.WriteLine($"=== Summary ===");
                    output.WriteLine($"Target function: {targetFunc}");
                    output.WriteLine($"Total impacted functions: {impacted.Count}");

                    // Unique files
                    var impactedFiles = impacted.Keys
                        .Select(f => analyzer.GetFunctionFile(f, functionIndex))
                        .Where(f => f != null)
                        .Select(f => f!)
                        .Distinct()
                        .OrderBy(f => f)
                        .ToList();

                    output.WriteLine($"Impacted files: {impactedFiles.Count}");
                    foreach (var file in impactedFiles)
                    {
                        output.WriteLine($"  {file}");
                    }
                }
            }
        }
        finally
        {
            if (options.OutputFile != null)
            {
                output.Dispose();
                Console.Error.WriteLine($"Output written to {options.OutputFile}");
            }
        }

        return 0;
    }

    static LinterOptions ParseImpactArguments(string[] args)
    {
        var options = new LinterOptions { Impact = true };

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;

                case "-f":
                case "--function":
                    if (i + 1 < args.Length)
                        options.ImpactFunction = args[++i];
                    break;

                case "--depth":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var depth))
                        options.CallGraphDepth = depth;
                    break;

                case "--reverse-graph":
                    options.ReverseGraph = true;
                    break;

                default:
                    if (!arg.StartsWith("-"))
                        options.Path = arg;
                    break;
            }
        }

        return options;
    }

    static void PrintImpactHelp()
    {
        Console.WriteLine(@"ERB Linter - Change Impact Analyzer

Usage: erb-linter impact [options] <path>

Analyzes which functions would be impacted by changing a target function.
Shows all callers (direct and indirect) organized by distance from target.

Options:
  -h, --help              Show this help
  -o, --output <file>     Output to file (default: stdout)
  -f, --function <name>   Target function to analyze (required)
  --depth <N>             Limit analysis depth (default: unlimited)
  --reverse-graph         Output DOT format instead of text report

Examples:
  erb-linter impact --function SHOW_STATUS Game/ERB/
  erb-linter impact -f NTR_SEX --depth 3 Game/ERB/
  erb-linter impact -f EVENTFIRST --reverse-graph -o impact.dot Game/ERB/

Output formats:
  Text (default): Lists impacted functions grouped by depth with file info
  DOT (--reverse-graph): Graphviz-compatible graph showing caller chains
");
    }
}
