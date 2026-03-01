namespace ErbToYaml;

public class DimConstResolver : IDimConstResolver
{
    private readonly Dictionary<string, int> _constants = new();

    public void LoadFromFile(string dimErhPath)
    {
        foreach (var line in File.ReadAllLines(dimErhPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#DIM CONST "))
            {
                var parts = trimmed.Substring("#DIM CONST ".Length).Split('=', 2);
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var value))
                {
                    _constants[parts[0].Trim()] = value;
                }
            }
        }
    }

    public int? Resolve(string name) =>
        _constants.TryGetValue(name, out var value) ? value : null;

    public string ResolveToString(string value)
    {
        if (int.TryParse(value, out _))
            return value; // Already numeric
        return _constants.TryGetValue(value, out var numericValue)
            ? numericValue.ToString()
            : value;
    }
}
