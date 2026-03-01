using System.Text;
using System.Text.RegularExpressions;
using ErbParser;
using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Injects com_id metadata into existing YAML files by parsing corresponding ERB files
/// to determine the COM ID for each YAML file index.
/// Bug 2+4 fix: enables direct COM ID lookup in YAML files instead of fragile index-based mapping.
/// </summary>
public class ComIdInjector
{
    private readonly IPathAnalyzer _pathAnalyzer;
    private readonly ILocalGateResolver? _localGateResolver;

    public ComIdInjector(IPathAnalyzer pathAnalyzer, ILocalGateResolver? localGateResolver = null)
    {
        _pathAnalyzer = pathAnalyzer ?? throw new ArgumentNullException(nameof(pathAnalyzer));
        _localGateResolver = localGateResolver;
    }

    /// <summary>
    /// Inject com_id into all YAML files under yamlBasePath that correspond to ERB files under erbBasePath.
    /// </summary>
    /// <param name="erbBasePath">Root directory containing ERB files (e.g., Game/ERB/口上)</param>
    /// <param name="yamlBasePath">Root directory containing YAML files (e.g., Game/YAML/Kojo)</param>
    /// <returns>Summary of injection results</returns>
    public async Task<InjectResult> InjectAsync(string erbBasePath, string yamlBasePath)
    {
        var result = new InjectResult();

        // Discover all ERB files (not just KOJO_K* — also NTR, SexHara, WC, KOJO_MODIFIER)
        var erbFiles = Directory.GetFiles(erbBasePath, "*.ERB", SearchOption.AllDirectories)
            .ToList();

        foreach (var erbFile in erbFiles)
        {
            try
            {
                await InjectForErbFileAsync(erbFile, yamlBasePath, result);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process {erbFile}: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Process a single ERB file: parse it, determine COM IDs for each convertible node,
    /// and inject com_id into the corresponding YAML files.
    /// </summary>
    private async Task InjectForErbFileAsync(string erbFilePath, string yamlBasePath, InjectResult result)
    {
        // 1. Extract character/situation from path
        string character, situation;
        try
        {
            (character, situation) = _pathAnalyzer.Extract(erbFilePath);
        }
        catch (ArgumentException)
        {
            // Skip files that don't match expected path patterns (e.g., .ERH files in wrong location)
            return;
        }

        // 2. Parse ERB file
        var content = await File.ReadAllTextAsync(erbFilePath, Encoding.UTF8);
        var parser = new ErbParser.ErbParser();
        var astNodes = parser.ParseString(content, erbFilePath);

        // Apply LOCAL gate resolution
        if (_localGateResolver != null)
        {
            astNodes = _localGateResolver.Resolve(astNodes);
        }

        // 3. Replicate FileConverter's convertible node extraction with COM ID tracking
        var comIdsForYaml = new List<int?>(); // One entry per YAML file that would be generated

        foreach (var node in astNodes)
        {
            if (node is DatalistNode || node is PrintDataNode)
            {
                comIdsForYaml.Add(null); // Top-level node, no function context
            }
            else if (node is SelectCaseNode)
            {
                comIdsForYaml.Add(null);
            }
            else if (node is IfNode ifNode && ContainsConvertibleContent(ifNode))
            {
                comIdsForYaml.Add(null);
            }
            else if (node is FunctionDefNode functionNode)
            {
                var resolvedBody = _localGateResolver != null
                    ? _localGateResolver.Resolve(functionNode.Body)
                    : functionNode.Body;

                var comId = ExtractComId(functionNode.FunctionName);

                foreach (var bodyNode in resolvedBody)
                {
                    if (bodyNode is DatalistNode || bodyNode is PrintDataNode)
                    {
                        comIdsForYaml.Add(comId);
                    }
                    else if (bodyNode is SelectCaseNode)
                    {
                        comIdsForYaml.Add(comId);
                    }
                    else if (bodyNode is IfNode bodyIfNode && ContainsConvertibleContent(bodyIfNode))
                    {
                        comIdsForYaml.Add(comId);
                    }
                }

                // EVENT functions don't produce regular indexed YAML files (they have their own naming)
                // so we don't track them here
            }
        }

        if (comIdsForYaml.Count == 0)
            return;

        // 4. Find the YAML directory for this character
        // Path pattern: yamlBasePath/{charId}_{charName}/
        // Extract charId from situation (e.g., "K1_愛撫" → "1", "NTR口上" → from ERB dir name)
        var charIdMatch = Regex.Match(situation, @"^K(\d+|U)_");
        string? charId = null;
        string? category = null;

        if (charIdMatch.Success)
        {
            charId = charIdMatch.Groups[1].Value;
            // Extract category from situation (e.g., "K1_愛撫" → "愛撫")
            var categoryMatch = Regex.Match(situation, @"^K\w+_(.+)$");
            if (categoryMatch.Success)
                category = categoryMatch.Groups[1].Value;
        }
        else
        {
            // Non-K situations (NTR口上, SexHara, WC系, etc.)
            // Extract charId from the parent directory name (e.g., "1_美鈴" → "1")
            var parentDir = Path.GetFileName(Path.GetDirectoryName(erbFilePath));
            if (parentDir != null)
            {
                var dirMatch = Regex.Match(parentDir, @"^(\d+|U)_");
                if (dirMatch.Success)
                {
                    charId = dirMatch.Groups[1].Value;
                    category = situation; // Use full situation as category (e.g., "NTR口上")
                }
            }
        }

        if (charId == null || category == null)
            return;

        var yamlCharDirs = Directory.GetDirectories(yamlBasePath, $"{charId}_*");
        if (yamlCharDirs.Length == 0)
            return;

        var yamlCharDir = yamlCharDirs[0];

        // 5. Find YAML files matching pattern, sorted by numeric index
        // Search for both indexed files (K1_愛撫_0.yaml) and single files (K4_乳首責め.yaml)
        var yamlPrefix = charIdMatch.Success ? $"K{charId}_{category}" : category;

        // Collect YAML files from all locations (character dir, root, single-file variants)
        var searchDirs = new List<string> { yamlCharDir };

        // Also search yamlBasePath root for duplicate files (e.g., K10_会話親密_0.yaml at root)
        if (yamlCharDir != yamlBasePath)
            searchDirs.Add(yamlBasePath);

        foreach (var searchDir in searchDirs)
        {
            var indexedPattern = $"{yamlPrefix}_*.yaml";
            var yamlFiles = Directory.GetFiles(searchDir, indexedPattern)
                .Select(f => new
                {
                    Path = f,
                    Index = int.TryParse(
                        Regex.Match(Path.GetFileNameWithoutExtension(f), @"_(\d+)$").Groups[1].Value,
                        out var idx) ? idx : -1
                })
                .Where(f => f.Index >= 0)
                .OrderBy(f => f.Index)
                .ToList();

            // Also check for non-indexed single files: K4_乳首責め.yaml or KOJO_KU_愛撫.yaml
            if (yamlFiles.Count == 0 && searchDir == yamlCharDir)
            {
                var singleFileCandidates = new[]
                {
                    Path.Combine(yamlCharDir, $"{yamlPrefix}.yaml"),
                    Path.Combine(yamlCharDir, $"KOJO_{yamlPrefix}.yaml"), // KOJO_ prefix variant
                };
                foreach (var candidate in singleFileCandidates)
                {
                    if (File.Exists(candidate))
                    {
                        yamlFiles = [new { Path = candidate, Index = 0 }];
                        break;
                    }
                }
            }

            // 6. Inject com_id into each YAML file
            for (int i = 0; i < Math.Min(comIdsForYaml.Count, yamlFiles.Count); i++)
            {
                var comId = comIdsForYaml[i];
                if (!comId.HasValue)
                    continue;

                var yamlFilePath = yamlFiles[i].Path;

                try
                {
                    await InjectComIdIntoFileAsync(yamlFilePath, comId.Value, result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to inject com_id into {yamlFilePath}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Inject com_id into a single YAML file if not already present.
    /// </summary>
    private static async Task InjectComIdIntoFileAsync(string yamlFilePath, int comId, InjectResult result)
    {
        var yamlContent = await File.ReadAllTextAsync(yamlFilePath, Encoding.UTF8);

        // Check if com_id already exists
        if (Regex.IsMatch(yamlContent, @"^com_id:\s*\d+", RegexOptions.Multiline))
        {
            result.Skipped++;
            return;
        }

        // Inject com_id after the situation line
        var situationLinePattern = new Regex(@"^(situation: .+)$", RegexOptions.Multiline);
        var updatedContent = situationLinePattern.Replace(yamlContent, $"$1\ncom_id: {comId}", 1);

        if (updatedContent == yamlContent)
        {
            result.Errors.Add($"Could not find situation line in {yamlFilePath}");
            return;
        }

        await File.WriteAllTextAsync(yamlFilePath, updatedContent, Encoding.UTF8);
        result.Injected++;
    }

    /// <summary>
    /// Extract COM ID from function name.
    /// Patterns (all → comId):
    ///   KOJO_MESSAGE_COM_K{charId}_{comId}[_{subIndex}]     — standard COM functions
    ///   KOJO_MESSAGE_COUNTER_K{charId}_{comId}[_{subIndex}]  — EVENT counter functions
    ///   NTR_KOJO_MESSAGE_COM_K{charId}_{comId}[_{subIndex}]  — NTR COM functions
    ///   NTR_KOJO_K{charId}_{comId}[_{subIndex}]              — NTR dialogue functions
    /// </summary>
    internal static int? ExtractComId(string functionName)
    {
        // Pattern 1: COM_K or COUNTER_K (covers standard COM, EVENT counter, and NTR COM)
        // COM_K{charId}_{comId} or COUNTER_K{charId}_{comId} with optional _{subIndex}
        var match = Regex.Match(functionName, @"(?:COM|COUNTER)_K(?:\d+|U)_(\d+)(?:_\d+)?$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var comId))
        {
            return comId;
        }

        // Pattern 2: NTR_KOJO_K{charId}_{comId}[_{subIndex}] — NTR dialogue functions
        // Also matches NTR_KOJO_KW{charId}_{comId}[_{subIndex}] (W = 見せつけ variant)
        match = Regex.Match(functionName, @"^NTR_KOJO_KW?(\d+)_(\d+)(?:_\d+)?$");
        if (match.Success && int.TryParse(match.Groups[2].Value, out var ntrComId))
        {
            return ntrComId;
        }

        return null;
    }

    /// <summary>
    /// Check if IfNode contains PRINTDATA or DATALIST content (mirrors FileConverter logic)
    /// </summary>
    private static bool ContainsConvertibleContent(IfNode ifNode)
    {
        return HasConvertibleContentRecursive(ifNode.Body)
            || ifNode.ElseIfBranches.Any(e => HasConvertibleContentRecursive(e.Body))
            || (ifNode.ElseBranch != null && HasConvertibleContentRecursive(ifNode.ElseBranch.Body));
    }

    private static bool HasConvertibleContentRecursive(List<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is PrintDataNode || node is DatalistNode)
                return true;
            if (node is IfNode nested && ContainsConvertibleContent(nested))
                return true;
        }
        return false;
    }
}

/// <summary>
/// Result of com_id injection operation.
/// </summary>
public class InjectResult
{
    public int Injected { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; } = new();
}
