using System.Text.RegularExpressions;
using ErbLinter.Parser;
using ErbLinter.Reporter;

namespace ErbLinter.Analyzer;

/// <summary>
/// Analyzes ERB codebase for dead (unreachable) functions
/// </summary>
public class DeadCodeAnalyzer
{
    // Function name -> set of caller function names
    private readonly Dictionary<string, HashSet<string>> _callers = new(StringComparer.OrdinalIgnoreCase);

    // Entry points (explicit names)
    private readonly HashSet<string> _entryPoints = new(StringComparer.OrdinalIgnoreCase);

    // Entry point patterns (regex)
    private readonly List<Regex> _entryPatterns = new();

    /// <summary>
    /// Analyze the codebase for dead code
    /// </summary>
    public IEnumerable<Issue> Analyze(
        FunctionIndex functionIndex,
        Dictionary<string, string[]> fileContents,
        string? entryPointsFile = null)
    {
        // Build call graph
        BuildCallGraph(functionIndex, fileContents);

        // Load entry points
        LoadEntryPoints(entryPointsFile);

        // Find dead code
        return FindDeadCode(functionIndex);
    }

    /// <summary>
    /// Build the call graph: for each function, record who calls it
    /// </summary>
    private void BuildCallGraph(FunctionIndex functionIndex, Dictionary<string, string[]> fileContents)
    {
        // Initialize all functions with empty caller sets
        foreach (var func in functionIndex.GetAllFunctions())
        {
            if (!_callers.ContainsKey(func.Name))
            {
                _callers[func.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        // Track current function while scanning each file
        foreach (var (filePath, lines) in fileContents)
        {
            string? currentFunction = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                    continue;

                // Track function definitions
                if (line.StartsWith("@"))
                {
                    currentFunction = ExtractFunctionName(line);
                    continue;
                }

                // Only process if we're inside a function
                if (currentFunction == null)
                    continue;

                // Extract call targets
                var targets = ExtractCallTargets(line);
                foreach (var target in targets)
                {
                    // Add caller to the target's caller set
                    if (_callers.TryGetValue(target, out var callerSet))
                    {
                        callerSet.Add(currentFunction);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Load entry points from file or use defaults
    /// </summary>
    private void LoadEntryPoints(string? entryPointsFile)
    {
        // Default Emuera system entry points
        var defaultEntryPoints = new[]
        {
            // Emuera system callbacks
            "EVENTFIRST", "EVENTEND", "EVENTTURNEND", "EVENTCOMEND",
            "EVENTSHOP", "EVENTTRAIN", "EVENTBUY", "EVENTSELL",
            "EVENTLOAD", "EVENTSAVE", "EVENTLOADEND",
            // Display functions
            "SHOW_STATUS", "SHOW_SHOP", "SHOW_USERCOM", "SHOW_ABLUP_SELECT",
            "SHOW_STAIN", "SHOW_JUEL",
            // User hooks
            "USERSHOP", "USERCOM",
            // System functions
            "SYSTEM_TITLE", "SYSTEM_AUTOSAVE", "SAVEINFO",
        };

        foreach (var entry in defaultEntryPoints)
        {
            _entryPoints.Add(entry);
        }

        // Default patterns
        var defaultPatterns = new[]
        {
            @"^USERCOM\d+$",     // USERCOM0, USERCOM1, etc.
            @"^COM\d+$",         // COM0, COM1, etc.
            @"^COMF\d+$",        // COMF0, COMF1, etc.
            @"^COMABLE\d+$",     // COMABLE0, COMABLE1, etc.
            @"^COM_ABLE\d+$",    // COM_ABLE0, COM_ABLE1, etc.
            @"^KOJO_.*$",        // Kojo dialogue functions
            @"^NTR_KOJO_.*$",    // NTR kojo functions
            @"^K\d+$",           // K0, K1, etc. (kojo short form)
            @"^EVENT.*$",        // All event handlers
            @"^SHOW_.*$",        // All display functions
            @"^JUEL_DEMAND.*$",  // Ability demand functions (called dynamically)
            @"^EXP_DEMAND.*$",   // Experience demand functions (called dynamically)
            @".*口上.*$",        // Kojo dialogue (Japanese naming, may be called dynamically)
            @"^SexHara.*$",      // SexHara functions (called dynamically)
            @"^CALLNAME_K\d+$",  // Character call name functions
            @"^NTR_MESSAGE_.*$", // NTR message functions (called dynamically)
            @"^CHK_CANCEL_.*$",  // Cancel check functions (called dynamically)
            @"^MSG_.*$",         // Message functions (called dynamically)
            @"^COM\d+_.*$",      // Command sub-functions (COM466_21, etc.)
            @"^GETPLACENAME_.*$", // Place name functions (called dynamically)
            @"^CAN_COM\d+$",     // Command availability check functions
            @"^EQUIP_COM\d+$",   // Command equipment functions
            @"^MESSAGE_COM\d+$", // Message COM functions (called via TRYCCALLFORM)
            @"^PUNISHMENT_\d+$", // Punishment functions (called dynamically)
            @"^CHARA_MOVEMENT.*$", // Character movement functions
            @"^SCOM\d+$",        // SCOM functions (called via TRYCALLFORM SCOM{n})
            @"^CAN_SCOM\d+$",    // CAN_SCOM functions
            @"^MESSAGE_SCOM\d+$", // MESSAGE_SCOM functions
            @"^NTR_SEX_\d+$",    // NTR sex scene functions (called via CALLFORM NTR_SEX_{n})
            @"^NTR_A_SEX_\d+$",  // NTR A sex scene functions
            @"^RANDOM_N_GIRL_\d+$", // Random girl name functions
            @"^RANDOM_N_BOY_\d+$", // Random boy name functions
            @"^MAP_PRINT_\d+$",  // Map print functions (called via CALLFORM MAP_PRINT_{n})
            @"^LUNA_OPTION_.*$", // Luna option functions
            @"^アイテム説明_\d+$", // Item description functions (called via CALLFORM アイテム説明_{n})
            @"^WC_PUNISHMENT_\d+$", // Wait counter punishment functions
            @"^LUNA_KOJO_EVENT_.*$", // Luna kojo event functions (called dynamically)
        };

        foreach (var pattern in defaultPatterns)
        {
            _entryPatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        // Load from file if provided
        if (!string.IsNullOrEmpty(entryPointsFile) && File.Exists(entryPointsFile))
        {
            var lines = File.ReadAllLines(entryPointsFile);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip comments and empty lines
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // Check if it's a regex pattern (starts with ^)
                if (trimmed.StartsWith("^"))
                {
                    try
                    {
                        _entryPatterns.Add(new Regex(trimmed, RegexOptions.IgnoreCase | RegexOptions.Compiled));
                    }
                    catch (ArgumentException)
                    {
                        // Invalid regex, skip
                    }
                }
                else
                {
                    _entryPoints.Add(trimmed);
                }
            }
        }
    }

    /// <summary>
    /// Find functions with no callers that are not entry points
    /// </summary>
    private IEnumerable<Issue> FindDeadCode(FunctionIndex functionIndex)
    {
        foreach (var func in functionIndex.GetAllFunctions())
        {
            // Skip if it's an entry point
            if (IsEntryPoint(func.Name))
                continue;

            // Skip if it has callers
            if (_callers.TryGetValue(func.Name, out var callerSet) && callerSet.Count > 0)
                continue;

            yield return new Issue(
                func.FilePath,
                func.Line,
                1,
                IssueLevel.Info,
                "DEAD001",
                $"Function @{func.Name} is never called");
        }
    }

    /// <summary>
    /// Check if a function is an entry point
    /// </summary>
    private bool IsEntryPoint(string functionName)
    {
        // Check explicit entry points
        if (_entryPoints.Contains(functionName))
            return true;

        // Check patterns
        foreach (var pattern in _entryPatterns)
        {
            if (pattern.IsMatch(functionName))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Extract all call targets from a line
    /// </summary>
    private IEnumerable<string> ExtractCallTargets(string line)
    {
        var targets = new List<string>();

        // Check for various call forms
        var prefixes = new[]
        {
            ("CALL ", 5, false),
            ("CALLF ", 6, false),   // CALLF used for function expressions
            ("TRYCALL ", 8, false),
            ("TRYCALLF ", 9, false),
            ("JUMP ", 5, false),
            ("GOTO ", 5, false),
            ("TRYCCALL ", 9, false),
            ("TRYCCALLF ", 10, false),
            ("TRYCGOTO ", 9, false),
            ("TRYCJUMP ", 9, false),
            ("CATCH ", 6, false),
        };

        foreach (var (prefix, length, isDynamic) in prefixes)
        {
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                // Skip CALLFORM, JUMPFORM, etc.
                if (line.StartsWith(prefix.TrimEnd() + "FORM", StringComparison.OrdinalIgnoreCase))
                    break;

                var rest = line.Substring(length).Trim();
                var target = ExtractTarget(rest);
                if (target != null)
                {
                    targets.Add(target);
                }
                break;
            }
        }

        // Also extract function calls used in expressions: FUNCNAME(args)
        // This catches uses like: IF HAS_PENIS(TARGET) or EXP += FUNC(x)
        targets.AddRange(ExtractExpressionCalls(line));

        return targets;
    }

    /// <summary>
    /// Extract function calls from expressions (e.g., HAS_PENIS(TARGET) in IF conditions)
    /// </summary>
    private IEnumerable<string> ExtractExpressionCalls(string line)
    {
        var targets = new List<string>();

        // Pattern: WORD( where WORD is alphanumeric/underscore and known function
        int i = 0;
        while (i < line.Length)
        {
            // Find start of potential identifier
            while (i < line.Length && !IsIdentifierChar(line[i]))
                i++;

            if (i >= line.Length)
                break;

            // Extract identifier
            int start = i;
            while (i < line.Length && IsIdentifierChar(line[i]))
                i++;

            if (i >= line.Length)
                break;

            string identifier = line.Substring(start, i - start);

            // Skip whitespace
            while (i < line.Length && char.IsWhiteSpace(line[i]))
                i++;

            // Check if followed by (
            if (i < line.Length && line[i] == '(')
            {
                // This looks like a function call
                // Only add if it's a known function (exists in our callers dictionary)
                if (_callers.ContainsKey(identifier))
                {
                    targets.Add(identifier);
                }
            }
        }

        return targets;
    }

    private static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    /// <summary>
    /// Extract target function name from the rest of a call line
    /// </summary>
    private static string? ExtractTarget(string rest)
    {
        if (string.IsNullOrEmpty(rest))
            return null;

        // Skip if dynamic (starts with %)
        if (rest.StartsWith("%") || rest.StartsWith("\""))
            return null;

        // Find end of function name
        var endIndex = rest.Length;
        for (int i = 0; i < rest.Length; i++)
        {
            var c = rest[i];
            if (c == ' ' || c == ',' || c == '(' || c == ';' || c == '\t')
            {
                endIndex = i;
                break;
            }
        }

        var target = rest.Substring(0, endIndex).Trim();
        return string.IsNullOrEmpty(target) ? null : target;
    }

    /// <summary>
    /// Extract function name from a definition line starting with @
    /// </summary>
    private static string ExtractFunctionName(string line)
    {
        var rest = line.Substring(1);
        var endIndex = rest.Length;
        for (int i = 0; i < rest.Length; i++)
        {
            var c = rest[i];
            if (c == ' ' || c == '(' || c == ',' || c == ';' || c == '\t')
            {
                endIndex = i;
                break;
            }
        }
        return rest.Substring(0, endIndex).Trim();
    }

    /// <summary>
    /// Get caller information for a function (for debugging/reporting)
    /// </summary>
    public IEnumerable<string> GetCallers(string functionName)
    {
        return _callers.TryGetValue(functionName, out var callers)
            ? callers
            : Enumerable.Empty<string>();
    }
}
