namespace ErbLinter.Data;

/// <summary>
/// Registry of variable definitions loaded from CSV files
/// </summary>
public class VariableRegistry
{
    private readonly Dictionary<int, string> _flags = new();
    private readonly Dictionary<int, string> _cflags = new();
    private readonly Dictionary<int, string> _abilities = new();
    private readonly Dictionary<int, string> _talents = new();

    // Reverse lookup: name to index
    private readonly Dictionary<string, int> _flagsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _cflagsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _abilitiesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _talentsByName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Number of FLAG definitions
    /// </summary>
    public int FlagCount => _flags.Count;

    /// <summary>
    /// Number of CFLAG definitions
    /// </summary>
    public int CFlagCount => _cflags.Count;

    /// <summary>
    /// Number of ability definitions
    /// </summary>
    public int AbilityCount => _abilities.Count;

    /// <summary>
    /// Number of talent definitions
    /// </summary>
    public int TalentCount => _talents.Count;

    /// <summary>
    /// Load all CSV files from a directory
    /// </summary>
    public void LoadFromDirectory(string csvDir)
    {
        var loader = new CsvLoader();

        // Load FLAG.CSV
        var flagPath = Path.Combine(csvDir, "FLAG.CSV");
        if (!File.Exists(flagPath))
            flagPath = Path.Combine(csvDir, "Flag.csv");
        LoadInto(_flags, _flagsByName, loader.LoadCsv(flagPath));

        // Load CFLAG.csv
        var cflagPath = Path.Combine(csvDir, "CFLAG.csv");
        if (!File.Exists(cflagPath))
            cflagPath = Path.Combine(csvDir, "Cflag.csv");
        LoadInto(_cflags, _cflagsByName, loader.LoadCsv(cflagPath));

        // Load Abl.csv
        var ablPath = Path.Combine(csvDir, "Abl.csv");
        if (!File.Exists(ablPath))
            ablPath = Path.Combine(csvDir, "ABL.CSV");
        LoadInto(_abilities, _abilitiesByName, loader.LoadCsv(ablPath));

        // Load Talent.csv
        var talentPath = Path.Combine(csvDir, "Talent.csv");
        if (!File.Exists(talentPath))
            talentPath = Path.Combine(csvDir, "TALENT.CSV");
        LoadInto(_talents, _talentsByName, loader.LoadCsv(talentPath));
    }

    private static void LoadInto(
        Dictionary<int, string> byIndex,
        Dictionary<string, int> byName,
        Dictionary<int, string> source)
    {
        foreach (var (index, name) in source)
        {
            byIndex[index] = name;
            byName[name] = index;
        }
    }

    /// <summary>
    /// Check if a FLAG index is defined
    /// </summary>
    public bool HasFlag(int index) => _flags.ContainsKey(index);

    /// <summary>
    /// Check if a FLAG name is defined
    /// </summary>
    public bool HasFlag(string name) => _flagsByName.ContainsKey(name);

    /// <summary>
    /// Get FLAG name by index
    /// </summary>
    public string? GetFlagName(int index) => _flags.GetValueOrDefault(index);

    /// <summary>
    /// Check if a CFLAG index is defined
    /// </summary>
    public bool HasCFlag(int index) => _cflags.ContainsKey(index);

    /// <summary>
    /// Check if a CFLAG name is defined
    /// </summary>
    public bool HasCFlag(string name) => _cflagsByName.ContainsKey(name);

    /// <summary>
    /// Get CFLAG name by index
    /// </summary>
    public string? GetCFlagName(int index) => _cflags.GetValueOrDefault(index);

    /// <summary>
    /// Check if an ability index is defined
    /// </summary>
    public bool HasAbility(int index) => _abilities.ContainsKey(index);

    /// <summary>
    /// Check if a talent index is defined
    /// </summary>
    public bool HasTalent(int index) => _talents.ContainsKey(index);

    /// <summary>
    /// Get all FLAG definitions
    /// </summary>
    public IReadOnlyDictionary<int, string> Flags => _flags;

    /// <summary>
    /// Get all CFLAG definitions
    /// </summary>
    public IReadOnlyDictionary<int, string> CFlags => _cflags;
}
