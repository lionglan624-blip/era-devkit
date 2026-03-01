namespace ErbLinter.Parser;

/// <summary>
/// Information about a function definition
/// </summary>
public record FunctionInfo(string Name, string FilePath, int Line);

/// <summary>
/// Indexes all function definitions across ERB files
/// </summary>
public class FunctionIndex
{
    private readonly Dictionary<string, List<FunctionInfo>> _functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _files = new();

    /// <summary>
    /// All indexed function names
    /// </summary>
    public IEnumerable<string> FunctionNames => _functions.Keys;

    /// <summary>
    /// All indexed file paths
    /// </summary>
    public IEnumerable<string> FilePaths => _files;

    /// <summary>
    /// Total number of unique functions
    /// </summary>
    public int Count => _functions.Count;

    /// <summary>
    /// Add functions from a file
    /// </summary>
    public void AddFromFile(string filePath, string[] lines)
    {
        _files.Add(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            // Function definition starts with @
            if (line.StartsWith("@"))
            {
                var funcName = ExtractFunctionName(line);
                if (!string.IsNullOrEmpty(funcName))
                {
                    var info = new FunctionInfo(funcName, filePath, i + 1);

                    if (!_functions.TryGetValue(funcName, out var list))
                    {
                        list = new List<FunctionInfo>();
                        _functions[funcName] = list;
                    }
                    list.Add(info);
                }
            }
        }
    }

    /// <summary>
    /// Check if a function exists
    /// </summary>
    public bool HasFunction(string name)
    {
        return _functions.ContainsKey(name);
    }

    /// <summary>
    /// Get function info by name
    /// </summary>
    public IEnumerable<FunctionInfo> GetFunction(string name)
    {
        return _functions.TryGetValue(name, out var list) ? list : Enumerable.Empty<FunctionInfo>();
    }

    /// <summary>
    /// Get functions with multiple definitions (potential issues)
    /// </summary>
    public IEnumerable<(string Name, List<FunctionInfo> Definitions)> GetDuplicateFunctions()
    {
        return _functions
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp => (kvp.Key, kvp.Value));
    }

    /// <summary>
    /// Get all function definitions (first definition for each name)
    /// </summary>
    public IEnumerable<FunctionInfo> GetAllFunctions()
    {
        return _functions.Values.Select(list => list[0]);
    }

    /// <summary>
    /// Extract function name from a definition line
    /// </summary>
    private static string ExtractFunctionName(string line)
    {
        // Remove @ prefix
        var rest = line.Substring(1);

        // Find end of function name (space, comma, or open paren means parameters)
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
}
