using System.Text;
using ErbLinter.Parser;

namespace ErbLinter.Analyzer;

/// <summary>
/// Builds and outputs function call graphs in DOT format
/// </summary>
public class CallGraphAnalyzer
{
    // Function name -> set of functions it calls (callees)
    private readonly Dictionary<string, HashSet<string>> _callees = new(StringComparer.OrdinalIgnoreCase);

    // Function name -> set of functions that call it (callers)
    private readonly Dictionary<string, HashSet<string>> _callers = new(StringComparer.OrdinalIgnoreCase);

    // All known functions (from index)
    private readonly HashSet<string> _knownFunctions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Build call graph from function index and file contents
    /// </summary>
    public void BuildGraph(FunctionIndex functionIndex, Dictionary<string, string[]> fileContents)
    {
        // Initialize all functions
        foreach (var func in functionIndex.GetAllFunctions())
        {
            _knownFunctions.Add(func.Name);
            if (!_callees.ContainsKey(func.Name))
            {
                _callees[func.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            if (!_callers.ContainsKey(func.Name))
            {
                _callers[func.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        // Scan each file for call relationships
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
                    // Only track calls to known functions
                    if (!_knownFunctions.Contains(target))
                        continue;

                    // Add to callees (current -> target)
                    if (_callees.TryGetValue(currentFunction, out var calleeSet))
                    {
                        calleeSet.Add(target);
                    }

                    // Add to callers (target <- current)
                    if (_callers.TryGetValue(target, out var callerSet))
                    {
                        callerSet.Add(currentFunction);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate DOT format output for the entire graph
    /// </summary>
    public string GenerateDot(string? rootFunction = null, int? maxDepth = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph callgraph {");
        sb.AppendLine("  rankdir=LR;");
        sb.AppendLine("  node [shape=box];");

        HashSet<string> includedFunctions;
        if (rootFunction != null)
        {
            includedFunctions = GetReachableFunctions(rootFunction, maxDepth);
        }
        else
        {
            includedFunctions = new HashSet<string>(_knownFunctions, StringComparer.OrdinalIgnoreCase);
        }

        // Output edges
        var outputEdges = new HashSet<string>();
        foreach (var caller in includedFunctions)
        {
            if (_callees.TryGetValue(caller, out var callees))
            {
                foreach (var callee in callees)
                {
                    if (includedFunctions.Contains(callee))
                    {
                        var edge = $"  \"{EscapeDotString(caller)}\" -> \"{EscapeDotString(callee)}\";";
                        if (outputEdges.Add(edge))
                        {
                            sb.AppendLine(edge);
                        }
                    }
                }
            }
        }

        // Output isolated nodes (no edges but in included set)
        foreach (var func in includedFunctions)
        {
            bool hasEdge = false;
            if (_callees.TryGetValue(func, out var callees) && callees.Any(c => includedFunctions.Contains(c)))
                hasEdge = true;
            if (!hasEdge && _callers.TryGetValue(func, out var callers) && callers.Any(c => includedFunctions.Contains(c)))
                hasEdge = true;

            if (!hasEdge)
            {
                sb.AppendLine($"  \"{EscapeDotString(func)}\";");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Get functions reachable from root within depth limit
    /// </summary>
    private HashSet<string> GetReachableFunctions(string root, int? maxDepth)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string func, int depth)>();

        if (!_knownFunctions.Contains(root))
        {
            return result;
        }

        queue.Enqueue((root, 0));
        result.Add(root);

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (maxDepth.HasValue && depth >= maxDepth.Value)
                continue;

            // Add callees (functions called by current)
            if (_callees.TryGetValue(current, out var callees))
            {
                foreach (var callee in callees)
                {
                    if (result.Add(callee))
                    {
                        queue.Enqueue((callee, depth + 1));
                    }
                }
            }

            // Also add callers (functions that call current) for bidirectional traversal
            if (_callers.TryGetValue(current, out var callers))
            {
                foreach (var caller in callers)
                {
                    if (result.Add(caller))
                    {
                        queue.Enqueue((caller, depth + 1));
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get statistics about the call graph
    /// </summary>
    public (int functions, int edges) GetStats()
    {
        int edges = _callees.Values.Sum(set => set.Count);
        return (_knownFunctions.Count, edges);
    }

    /// <summary>
    /// Get callees of a function
    /// </summary>
    public IEnumerable<string> GetCallees(string functionName)
    {
        return _callees.TryGetValue(functionName, out var callees)
            ? callees
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Get callers of a function
    /// </summary>
    public IEnumerable<string> GetCallers(string functionName)
    {
        return _callers.TryGetValue(functionName, out var callers)
            ? callers
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Get all functions that would be impacted by changing the target function.
    /// Returns a dictionary of caller -> depth (distance from target).
    /// </summary>
    public Dictionary<string, int> GetImpactedFunctions(string targetFunction, int? maxDepth = null)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (!_knownFunctions.Contains(targetFunction))
        {
            return result;
        }

        var queue = new Queue<(string func, int depth)>();

        // Start with direct callers at depth 1
        if (_callers.TryGetValue(targetFunction, out var directCallers))
        {
            foreach (var caller in directCallers)
            {
                queue.Enqueue((caller, 1));
                result[caller] = 1;
            }
        }

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (maxDepth.HasValue && depth >= maxDepth.Value)
                continue;

            // Find callers of the current function
            if (_callers.TryGetValue(current, out var callers))
            {
                foreach (var caller in callers)
                {
                    if (!result.ContainsKey(caller))
                    {
                        result[caller] = depth + 1;
                        queue.Enqueue((caller, depth + 1));
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Generate DOT format output for reverse graph (callee -> caller direction)
    /// </summary>
    public string GenerateReverseGraph(string targetFunction, int? maxDepth = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph impact {");
        sb.AppendLine("  rankdir=BT;");
        sb.AppendLine("  node [shape=box];");

        if (!_knownFunctions.Contains(targetFunction))
        {
            sb.AppendLine($"  // Unknown function: {EscapeDotString(targetFunction)}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        // Highlight target function
        sb.AppendLine($"  \"{EscapeDotString(targetFunction)}\" [style=filled, fillcolor=yellow];");

        var impacted = GetImpactedFunctions(targetFunction, maxDepth);
        var outputEdges = new HashSet<string>();

        // Build edges from target to direct callers
        if (_callers.TryGetValue(targetFunction, out var directCallers))
        {
            foreach (var caller in directCallers)
            {
                if (impacted.ContainsKey(caller))
                {
                    var edge = $"  \"{EscapeDotString(targetFunction)}\" -> \"{EscapeDotString(caller)}\";";
                    if (outputEdges.Add(edge))
                    {
                        sb.AppendLine(edge);
                    }
                }
            }
        }

        // Build edges between callers
        foreach (var (func, depth) in impacted)
        {
            if (_callers.TryGetValue(func, out var callers))
            {
                foreach (var caller in callers)
                {
                    if (impacted.ContainsKey(caller))
                    {
                        var edge = $"  \"{EscapeDotString(func)}\" -> \"{EscapeDotString(caller)}\";";
                        if (outputEdges.Add(edge))
                        {
                            sb.AppendLine(edge);
                        }
                    }
                }
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Get the file that contains a function definition
    /// </summary>
    public string? GetFunctionFile(string functionName, FunctionIndex functionIndex)
    {
        var func = functionIndex.GetAllFunctions()
            .FirstOrDefault(f => string.Equals(f.Name, functionName, StringComparison.OrdinalIgnoreCase));
        return func?.FilePath;
    }

    /// <summary>
    /// Escape special characters for DOT format
    /// </summary>
    private static string EscapeDotString(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
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
    /// Extract all call targets from a line
    /// </summary>
    private IEnumerable<string> ExtractCallTargets(string line)
    {
        var targets = new List<string>();

        // Check for various call forms
        var prefixes = new (string prefix, int length)[]
        {
            ("CALL ", 5),
            ("CALLF ", 6),
            ("TRYCALL ", 8),
            ("TRYCALLF ", 9),
            ("JUMP ", 5),
            ("GOTO ", 5),
            ("TRYCCALL ", 9),
            ("TRYCCALLF ", 10),
            ("TRYCGOTO ", 9),
            ("TRYCJUMP ", 9),
            ("CATCH ", 6),
        };

        foreach (var (prefix, length) in prefixes)
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
        targets.AddRange(ExtractExpressionCalls(line));

        return targets;
    }

    /// <summary>
    /// Extract function calls from expressions
    /// </summary>
    private IEnumerable<string> ExtractExpressionCalls(string line)
    {
        var targets = new List<string>();

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
                // Only add if it's a known function
                if (_knownFunctions.Contains(identifier))
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

        // Skip if dynamic (starts with % or ")
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
}
