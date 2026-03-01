using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace KojoComparer;

/// <summary>
/// Discovers ERB-YAML test cases by mapping 117 ERB files to their 443 YAML counterparts.
/// Uses com_file_map.json as SSOT for COM range mappings.
/// Uses com_id metadata in YAML files for direct lookup (Bug 2+4 fix).
/// </summary>
public class FileDiscovery
{
    private readonly string _erbBasePath;
    private readonly string _yamlBasePath;
    private readonly string _mapFilePath;

    /// <summary>
    /// Cache of com_id → YAML file path mappings, keyed by character directory path.
    /// Built lazily per character to avoid re-reading YAML files for every COM lookup.
    /// For standard COMs, the list contains a single YAML file.
    /// For sub-function COMs (e.g., COM_463), the list contains multiple YAML files sorted by filename index.
    /// </summary>
    private readonly Dictionary<string, Dictionary<int, List<string>>> _comIdCache = new();

    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public FileDiscovery(string erbBasePath, string yamlBasePath, string mapFilePath)
    {
        _erbBasePath = erbBasePath;
        _yamlBasePath = yamlBasePath;
        _mapFilePath = mapFilePath;
    }

    /// <summary>
    /// Discovers all ERB-YAML test cases by scanning ERB directory and mapping to YAML files.
    /// </summary>
    /// <returns>List of test cases with ERB file, function name, YAML file, and test state</returns>
    public List<TestCase> DiscoverTestCases()
    {
        var testCases = new List<TestCase>();

        // 1. Load com_file_map.json to get COM→ERB mappings
        var comMap = LoadComFileMap();

        // 2. Scan Game/ERB/口上/ for ERB files
        var erbFiles = Directory.GetFiles(_erbBasePath, "*.ERB", SearchOption.AllDirectories);

        // 3. For each ERB file, determine COM ranges it implements
        foreach (var erbFile in erbFiles)
        {
            var characterId = ExtractCharacterId(erbFile);
            var erbFileName = Path.GetFileNameWithoutExtension(erbFile);

            // Skip non-KOJO files (NTR, WC系, etc.)
            if (!erbFileName.StartsWith("KOJO_K"))
                continue;

            // 4. Find COM ranges that map to this ERB file
            var comEntries = FindComRangesForErb(comMap, erbFileName, characterId);

            // 5. For each COM entry, create test case(s) with corresponding YAML file(s)
            foreach (var entry in comEntries)
            {
                if (entry.SubFunctions != null)
                {
                    // COM with sub-functions: generate one test case per sub-function.
                    // Only generate tests when YAML file count matches sub-function count,
                    // because the positional mapping (subIdx → Nth YAML file) is only valid
                    // when every sub-function has a corresponding YAML file.
                    // When counts differ, some sub-functions were skipped during YAML migration
                    // (e.g., complex conditional branches), so positional mapping would be wrong.
                    var yamlFileCount = GetSubFunctionYamlFileCount(characterId, entry.ComId);
                    if (yamlFileCount != entry.SubFunctions.Length)
                        continue; // Skip: positional mapping unreliable

                    foreach (var subIdx in entry.SubFunctions)
                    {
                        var yamlFile = FindYamlFileForSubFunction(characterId, entry.ComId, subIdx);
                        if (yamlFile != null)
                        {
                            var functionName = GenerateFunctionName(characterId, entry.ComId, subIdx);
                            var state = GetRepresentativeState(entry.ComId);

                            testCases.Add(new TestCase
                            {
                                ErbFile = erbFile,
                                FunctionName = functionName,
                                YamlFile = yamlFile,
                                State = state,
                                ComId = entry.ComId,
                                CharacterId = characterId,
                                SubFunctionIndex = subIdx
                            });
                        }
                    }
                }
                else
                {
                    // Standard COM: one test case
                    var yamlFile = FindYamlFile(characterId, entry.ComId, comMap);
                    if (yamlFile != null)
                    {
                        var functionName = GenerateFunctionName(characterId, entry.ComId);
                        var state = GetRepresentativeState(entry.ComId);

                        testCases.Add(new TestCase
                        {
                            ErbFile = erbFile,
                            FunctionName = functionName,
                            YamlFile = yamlFile,
                            State = state,
                            ComId = entry.ComId,
                            CharacterId = characterId
                        });
                    }
                }
            }
        }

        return testCases;
    }

    /// <summary>
    /// Loads com_file_map.json and parses into ComFileMap structure.
    /// </summary>
    private ComFileMap LoadComFileMap()
    {
        var json = File.ReadAllText(_mapFilePath);
        var map = JsonSerializer.Deserialize<ComFileMap>(json, s_caseInsensitiveOptions);
        if (map == null || map.Ranges == null)
        {
            throw new InvalidOperationException($"Failed to load or parse {_mapFilePath}");
        }
        return map;
    }

    /// <summary>
    /// Entry for a discovered COM ID, optionally with sub-function indices.
    /// </summary>
    private record ComEntry(int ComId, int[]? SubFunctions);

    /// <summary>
    /// Finds all COM IDs that map to the given ERB file for the given character.
    /// </summary>
    /// <param name="comMap">Loaded COM file map</param>
    /// <param name="erbFileName">ERB filename without extension (e.g., "KOJO_K1_愛撫")</param>
    /// <param name="characterId">Character ID (e.g., "1", "U")</param>
    /// <returns>List of COM entries with optional sub-function info</returns>
    private List<ComEntry> FindComRangesForErb(ComFileMap comMap, string erbFileName, string characterId)
    {
        var comEntries = new List<ComEntry>();

        // Extract category from ERB filename: KOJO_K1_愛撫 -> _愛撫.ERB
        var match = Regex.Match(erbFileName, @"KOJO_K\d+_(.+)$");
        if (!match.Success)
        {
            match = Regex.Match(erbFileName, @"KOJO_KU_(.+)$");
        }

        if (!match.Success)
            return comEntries;

        var category = match.Groups[1].Value;
        var targetFile = $"_{category}.ERB";

        // Find all COM ranges for this file
        foreach (var range in comMap.Ranges)
        {
            if (range.File != targetFile || !range.Implemented)
                continue;

            // Check for character-specific overrides
            bool skipThisRange = false;
            for (int comId = range.Start; comId <= range.End; comId++)
            {
                // Check skip_combinations
                if (comMap.SkipCombinations?.Any(skip =>
                    skip.Character == $"K{characterId}" && skip.File == targetFile) == true)
                {
                    skipThisRange = true;
                    break;
                }

                // Check character_overrides (specific COM might belong to different file)
                if (comMap.CharacterOverrides?.ContainsKey($"K{characterId}") == true)
                {
                    var overrides = comMap.CharacterOverrides[$"K{characterId}"];
                    if (overrides.ContainsKey(comId.ToString()) &&
                        overrides[comId.ToString()] != targetFile)
                    {
                        continue; // This COM belongs to different file for this character
                    }
                }

                comEntries.Add(new ComEntry(comId, range.SubFunctions));
            }

            if (skipThisRange)
                break;
        }

        return comEntries;
    }

    /// <summary>
    /// Finds the YAML file corresponding to a COM ID using com_id metadata.
    /// Reads com_id field from YAML files and builds a cached lookup dictionary.
    /// Bug 2+4 fix: replaces fragile index-based lookup with direct com_id matching.
    /// For standard COMs, returns the first (and only) YAML file with that com_id.
    /// </summary>
    /// <param name="characterId">Character ID (e.g., "1", "U")</param>
    /// <param name="comId">COM ID</param>
    /// <param name="comMap">Loaded COM file map</param>
    /// <returns>Full path to YAML file, or null if not found</returns>
    private string? FindYamlFile(string characterId, int comId, ComFileMap comMap)
    {
        var comIdMap = GetOrBuildComIdMap(characterId);
        if (comIdMap == null)
            return null;

        // Direct lookup by com_id - return first file for standard COMs
        return comIdMap.TryGetValue(comId, out var yamlPaths) && yamlPaths.Count > 0
            ? yamlPaths[0]
            : null;
    }

    /// <summary>
    /// Finds the YAML file for a specific sub-function of a COM ID.
    /// Sub-function YAML files share the same com_id but are distinguished by filename index
    /// (e.g., K1_日常_0.yaml through K1_日常_5.yaml for COM_463 sub-functions 0-5).
    /// </summary>
    /// <param name="characterId">Character ID (e.g., "1", "U")</param>
    /// <param name="comId">COM ID</param>
    /// <param name="subFunctionIndex">Sub-function index (0-based)</param>
    /// <returns>Full path to YAML file, or null if not found</returns>
    private string? FindYamlFileForSubFunction(string characterId, int comId, int subFunctionIndex)
    {
        var comIdMap = GetOrBuildComIdMap(characterId);
        if (comIdMap == null)
            return null;

        // Lookup all YAML files with this com_id, sorted by filename index
        if (!comIdMap.TryGetValue(comId, out var yamlPaths))
            return null;

        // The sub-function index maps to the Nth YAML file (sorted by filename numeric suffix)
        return subFunctionIndex < yamlPaths.Count ? yamlPaths[subFunctionIndex] : null;
    }

    /// <summary>
    /// Returns the number of YAML files for a given COM ID and character.
    /// Used to verify that the positional mapping between sub-function indices and YAML files is valid.
    /// </summary>
    private int GetSubFunctionYamlFileCount(string characterId, int comId)
    {
        var comIdMap = GetOrBuildComIdMap(characterId);
        if (comIdMap == null)
            return 0;

        return comIdMap.TryGetValue(comId, out var yamlPaths) ? yamlPaths.Count : 0;
    }

    /// <summary>
    /// Gets or builds the com_id → YAML files mapping for a character.
    /// </summary>
    private Dictionary<int, List<string>>? GetOrBuildComIdMap(string characterId)
    {
        var characterDir = Directory.GetDirectories(_yamlBasePath, $"{characterId}_*").FirstOrDefault();
        if (characterDir == null)
            return null;

        if (!_comIdCache.TryGetValue(characterDir, out var comIdMap))
        {
            comIdMap = BuildComIdMap(characterDir);
            _comIdCache[characterDir] = comIdMap;
        }

        return comIdMap;
    }

    /// <summary>
    /// Regex to match K-pattern YAML filenames: K{N}_xxx or KU_xxx (standard kojo files).
    /// Used to filter out NTR口上, WC系, SexHara, KOJO_MODIFIER, and other non-K-pattern files
    /// that may also contain com_id fields but are not standard COM test targets.
    /// </summary>
    private static readonly Regex KPatternFilenameRegex = new(@"^(?:K\d+_|KU_)", RegexOptions.Compiled);

    /// <summary>
    /// Builds a com_id → YAML file paths dictionary for all YAML files in a character directory.
    /// Reads the first ~10 lines of each YAML file to extract the com_id field.
    /// Only includes K-pattern files (K{N}_xxx or KU_xxx); skips NTR/WC/SexHara files.
    /// For COM IDs with multiple YAML files (e.g., COM_463 sub-functions), collects all files
    /// sorted by the numeric suffix in the filename (e.g., K1_日常_0 → 0, K1_日常_1 → 1).
    /// </summary>
    private static Dictionary<int, List<string>> BuildComIdMap(string characterDir)
    {
        var comIdRegex = new Regex(@"^com_id:\s*(\d+)", RegexOptions.Compiled);
        var filenameSuffixRegex = new Regex(@"_(\d+)\.yaml$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var map = new Dictionary<int, List<string>>();

        var yamlFiles = Directory.GetFiles(characterDir, "*.yaml");
        foreach (var yamlFile in yamlFiles)
        {
            // Only include K-pattern files (K{N}_xxx or KU_xxx) in the com_id map.
            // Skip NTR口上, WC系, SexHara, KOJO_MODIFIER, and other non-K-pattern files
            // that may have com_id fields but are not standard COM test targets.
            var fileName = Path.GetFileName(yamlFile);
            if (!KPatternFilenameRegex.IsMatch(fileName))
                continue;

            // Read first ~10 lines to find com_id field (it's near the top)
            using var reader = new StreamReader(yamlFile, System.Text.Encoding.UTF8);
            for (int lineNum = 0; lineNum < 10; lineNum++)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                var match = comIdRegex.Match(line);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var parsedComId))
                {
                    if (!map.TryGetValue(parsedComId, out var fileList))
                    {
                        fileList = [];
                        map[parsedComId] = fileList;
                    }
                    fileList.Add(yamlFile);
                    break;
                }
            }
        }

        // Sort each file list by the numeric suffix in filename
        // e.g., K1_日常_0.yaml → 0, K1_日常_1.yaml → 1, etc.
        foreach (var kvp in map)
        {
            if (kvp.Value.Count > 1)
            {
                kvp.Value.Sort((a, b) =>
                {
                    var matchA = filenameSuffixRegex.Match(Path.GetFileName(a));
                    var matchB = filenameSuffixRegex.Match(Path.GetFileName(b));
                    var indexA = matchA.Success ? int.Parse(matchA.Groups[1].Value) : 0;
                    var indexB = matchB.Success ? int.Parse(matchB.Groups[1].Value) : 0;
                    return indexA.CompareTo(indexB);
                });
            }
        }

        return map;
    }

    /// <summary>
    /// Extracts character ID from ERB file path.
    /// Example: .../1_美鈴/KOJO_K1_愛撫.ERB -> "1"
    /// Example: .../U_汎用/KOJO_KU_日常.ERB -> "U"
    /// </summary>
    private string ExtractCharacterId(string erbFilePath)
    {
        var directory = Path.GetFileName(Path.GetDirectoryName(erbFilePath));
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException($"Cannot extract directory from {erbFilePath}");

        var match = Regex.Match(directory, @"^(\d+|U)_");
        if (!match.Success)
            throw new ArgumentException($"Cannot extract character ID from {erbFilePath}");

        return match.Groups[1].Value;
    }

    /// <summary>
    /// Generates ERB function name from character ID and COM ID.
    /// Example: characterId="1", comId=0 -> "@KOJO_MESSAGE_COM_K1_0"
    /// Example: characterId="10", comId=301 -> "@KOJO_MESSAGE_COM_K10_301"
    /// </summary>
    private string GenerateFunctionName(string characterId, int comId)
    {
        // Function name uses full COM ID, not split hundreds/remainder
        return $"@KOJO_MESSAGE_COM_K{characterId}_{comId}";
    }

    /// <summary>
    /// Generates ERB function name for a sub-function of a COM.
    /// Example: characterId="1", comId=463, subFunctionIndex=0 -> "@KOJO_MESSAGE_COM_K1_463_0"
    /// Example: characterId="9", comId=463, subFunctionIndex=5 -> "@KOJO_MESSAGE_COM_K9_463_5"
    /// </summary>
    private string GenerateFunctionName(string characterId, int comId, int subFunctionIndex)
    {
        return $"@KOJO_MESSAGE_COM_K{characterId}_{comId}_{subFunctionIndex}";
    }

    /// <summary>
    /// Returns a representative state for testing a given COM ID.
    /// Uses empty state to ensure both ERB and YAML use default/fallback branch.
    /// KojoBranchesParser selects first empty-condition branch, so ERB must also
    /// fall through to ELSE branch for equivalence testing to work.
    /// Comprehensive branch-specific testing is deferred to F709.
    /// </summary>
    private Dictionary<string, int> GetRepresentativeState(int comId)
    {
        // Empty state: both ERB (ELSE branch) and YAML (empty condition branch) use default
        return new Dictionary<string, int>();
    }
}

/// <summary>
/// Test case data structure.
/// </summary>
public class TestCase
{
    public string ErbFile { get; set; } = "";
    public string FunctionName { get; set; } = "";
    public string YamlFile { get; set; } = "";
    public Dictionary<string, int> State { get; set; } = new();
    public int ComId { get; set; }
    public string CharacterId { get; set; } = "";
    /// <summary>
    /// Sub-function index for COMs with sub-functions (e.g., COM_463_0 through COM_463_5).
    /// Null for standard COMs that use a single function.
    /// </summary>
    public int? SubFunctionIndex { get; set; }
}

/// <summary>
/// COM file map JSON structure.
/// </summary>
internal class ComFileMap
{
    public List<ComRange> Ranges { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>>? CharacterOverrides { get; set; }
    public List<SkipCombination>? SkipCombinations { get; set; }
}

internal class ComRange
{
    public int Start { get; set; }
    public int End { get; set; }
    public string File { get; set; } = "";
    public bool Implemented { get; set; }
    [JsonPropertyName("sub_functions")]
    public int[]? SubFunctions { get; set; }
}

internal class SkipCombination
{
    public string Character { get; set; } = "";
    public string File { get; set; } = "";
}
