using Era.Core.Dialogue;
using Era.Core.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlMemberAttribute = YamlDotNet.Serialization.YamlMemberAttribute;

namespace KojoComparer;

/// <summary>
/// Parser for "branches:" format YAML kojo files.
/// This format uses a simpler structure with branches containing lines and conditions.
/// </summary>
public class KojoBranchesParser
{
    private readonly IDeserializer _deserializer;
    private readonly Func<string, string[], bool>? _functionEvaluator;

    private static readonly string[] VariablePrefixes =
    [
        "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN",
        "MARK", "EXP", "NOWEX", "ABL", "FLAG", "TFLAG", "TEQUIP", "PALAM",
        "ARG", "LOCAL"
    ];

    public KojoBranchesParser(Func<string, string[], bool>? functionEvaluator = null)
    {
        _functionEvaluator = functionEvaluator;
        // Using UnderscoredNamingConvention to match production YAML format
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Parse a branches-format YAML file and return dialogue result.
    /// Selects the first branch whose condition evaluates to true, or the last branch with empty condition (ELSE fallback).
    /// </summary>
    /// <param name="yamlContent">YAML content to parse</param>
    /// <param name="state">Game state dictionary in format {"TALENT:16": 1, "TALENT:3": 0}</param>
    public DialogueResult Parse(string yamlContent, Dictionary<string, int>? state = null)
    {
        var fileData = _deserializer.Deserialize<BranchesFileData>(yamlContent);

        if (fileData?.Branches == null || fileData.Branches.Count == 0)
        {
            throw new InvalidOperationException("No branches found in YAML file");
        }

        // Select FIRST branch where condition evaluates to TRUE
        // If no conditions match, select LAST branch with empty condition (ELSE fallback)
        // ERB structure: IF TALENT:恋人 / ELSEIF TALENT:恋慕 / ELSEIF TALENT:思慕 / ELSE
        // YAML structure: Branches have proper TALENT conditions, or condition: {} for ELSE
        var selectedBranch = fileData.Branches
            .FirstOrDefault(b => EvaluateCondition(b.Condition, state))
            ?? fileData.Branches.LastOrDefault(b => IsEmptyCondition(b.Condition));

        if (selectedBranch == null)
        {
            throw new InvalidOperationException("No branch with empty condition found");
        }

        // Convert lines to DialogueLine
        // Support both simple string lines and lines_with_metadata format
        List<DialogueLine> dialogueLines;

        if (selectedBranch.LinesWithMetadata != null && selectedBranch.LinesWithMetadata.Count > 0)
        {
            // Use lines_with_metadata when available (for per-line displayMode)
            dialogueLines = selectedBranch.LinesWithMetadata
                .Select(lineData => new DialogueLine(
                    lineData.Text,
                    ParseDisplayMode(lineData.DisplayMode)))
                .ToList();
        }
        else
        {
            // Fall back to simple lines format
            dialogueLines = selectedBranch.Lines
                .Select(line => new DialogueLine(line, DisplayMode.Default))
                .ToList();
        }

        return DialogueResult.Create(dialogueLines);
    }

    private bool IsEmptyCondition(Dictionary<string, object>? condition)
    {
        return condition == null || condition.Count == 0;
    }

    /// <summary>
    /// Evaluates a YAML condition against game state.
    /// Supports single TALENT conditions and compound conditions (AND, OR, NOT).
    /// </summary>
    /// <param name="condition">YAML condition in format { "TALENT": { "16": { "ne": 0 } } } or compound format</param>
    /// <param name="state">Game state in format { "TALENT:16": 1, "TALENT:3": 0 }</param>
    /// <param name="depth">Current recursion depth for compound conditions (default 0)</param>
    /// <returns>True if condition matches state, false otherwise</returns>
    private bool EvaluateCondition(Dictionary<string, object>? condition, Dictionary<string, int>? state, int depth = 0)
    {
        // Empty condition is only for ELSE fallback
        if (condition == null || condition.Count == 0)
            return false;

        // No state provided → only match empty conditions
        if (state == null)
            return false;

        // Check for compound operator keys BEFORE TALENT key (T1)
        if (condition.ContainsKey("AND") || condition.ContainsKey("OR") || condition.ContainsKey("NOT"))
            return EvaluateCompoundCondition(condition, state, depth);

        // NOTE: TALENT uses dedicated EvaluateTalentCondition logic because compound key format (e.g., "PLAYER:16") requires TalentKeyParser parsing that generic EvaluateVariableCondition cannot provide. See Maintenance Note #4 in feature-760.md.
        // Example condition: { "TALENT": { "3": { "ne": 0 } } }
        // Parse TALENT conditions
        if (condition.TryGetValue("TALENT", out var talentObj) && talentObj is Dictionary<object, object> talentDict)
        {
            foreach (var kvp in talentDict)
            {
                var keyStr = kvp.Key?.ToString();
                if (string.IsNullOrEmpty(keyStr))
                    continue;

                var (target, talentIndex) = TalentKeyParser.ParseTalentYamlKey(keyStr);

                // Build target-qualified state key
                var effectiveTarget = target ?? "TARGET";

                string stateKey;
                if (talentIndex.HasValue)
                {
                    stateKey = $"TALENT:{effectiveTarget}:{talentIndex.Value}";
                }
                else
                {
                    // NOTE: Target-only symbolic reference (e.g., TALENT:PLAYER with no index).
                    // State key: TALENT:PLAYER (two-part, no index dimension).
                    // Evaluates to default-0 until F769 provides runtime state injection.
                    stateKey = $"TALENT:{effectiveTarget}";
                }

                var stateValue = state.GetValueOrDefault(stateKey, 0);

                // Parse operator dictionary: { "ne": 0 } or { "bitwise_and_cmp": {...} }
                if (kvp.Value is Dictionary<object, object> opDict)
                {
                    if (!EvaluateOpDict(stateValue, opDict))
                        return false;
                }
            }

            return true; // All TALENT conditions matched
        }

        // Parse variable conditions using shared method
        foreach (var prefix in VariablePrefixes)
        {
            if (condition.TryGetValue(prefix, out var obj) && obj is Dictionary<object, object> dict)
            {
                return EvaluateVariableCondition(prefix, dict, state);
            }
        }

        // Parse FUNCTION conditions with delegate evaluation
        if (condition.TryGetValue("FUNCTION", out var functionObj) && functionObj is Dictionary<object, object> functionDict)
        {
            if (functionDict.TryGetValue("name", out var nameObj) && nameObj is string name)
            {
                var args = functionDict.TryGetValue("args", out var argsObj) && argsObj is List<object> argsList
                    ? argsList.Select(a => a?.ToString() ?? string.Empty).ToArray()
                    : Array.Empty<string>();
                return _functionEvaluator?.Invoke(name, args) ?? false;
            }
            return false;
        }

        return false;
    }

    /// <summary>
    /// Recursively evaluates compound conditions (AND, OR, NOT).
    /// Enforces TALENT-only scope and maximum nesting depth.
    /// </summary>
    /// <param name="condition">Compound condition dict containing AND/OR/NOT key</param>
    /// <param name="state">Game state</param>
    /// <param name="depth">Current recursion depth (default 0)</param>
    /// <returns>True if compound condition evaluates to true</returns>
    /// <exception cref="InvalidOperationException">Thrown when depth exceeds 5, or non-TALENT key found in sub-condition</exception>
    private bool EvaluateCompoundCondition(Dictionary<string, object> condition, Dictionary<string, int>? state, int depth = 0)
    {
        // Depth limit enforcement (T5, AC#14)
        if (depth > 5)
            throw new InvalidOperationException($"Compound condition nesting exceeds maximum depth (depth: {depth}, max: 5)");

        // AND operator: all sub-conditions must be true (T2, AC#1, AC#2, AC#15)
        if (condition.TryGetValue("AND", out var andObj) && andObj is List<object> andList)
        {
            // Empty AND array returns true (vacuous truth) (AC#15)
            if (andList.Count == 0)
                return true;

            foreach (var subCondObj in andList)
            {
                if (subCondObj is not Dictionary<object, object> subCondDict)
                    continue;

                // Convert Dictionary<object, object> to Dictionary<string, object> for EvaluateCondition
                var subCond = subCondDict.ToDictionary(
                    kvp => kvp.Key?.ToString() ?? string.Empty,
                    kvp => kvp.Value);

                // Scope validation (T6, AC#7): only allow TALENT compound conditions
                ValidateConditionScope(subCond);

                // Recursive evaluation with depth increment
                if (!EvaluateCondition(subCond, state, depth + 1))
                    return false; // Short-circuit on first false
            }
            return true; // All sub-conditions passed
        }

        // OR operator: any sub-condition must be true (T3, AC#3, AC#4, AC#16)
        if (condition.TryGetValue("OR", out var orObj) && orObj is List<object> orList)
        {
            // Empty OR array returns false (AC#16)
            if (orList.Count == 0)
                return false;

            foreach (var subCondObj in orList)
            {
                if (subCondObj is not Dictionary<object, object> subCondDict)
                    continue;

                var subCond = subCondDict.ToDictionary(
                    kvp => kvp.Key?.ToString() ?? string.Empty,
                    kvp => kvp.Value);

                // Scope validation
                ValidateConditionScope(subCond);

                // Recursive evaluation
                if (EvaluateCondition(subCond, state, depth + 1))
                    return true; // Short-circuit on first true
            }
            return false; // All sub-conditions failed
        }

        // NOT operator: negate sub-condition result (T4, AC#5)
        if (condition.TryGetValue("NOT", out var notObj) && notObj is Dictionary<object, object> notDict)
        {
            var subCond = notDict.ToDictionary(
                kvp => kvp.Key?.ToString() ?? string.Empty,
                kvp => kvp.Value);

            // Scope validation
            ValidateConditionScope(subCond);

            // Recursive evaluation and negation
            return !EvaluateCondition(subCond, state, depth + 1);
        }

        // If no compound operator recognized, return false
        return false;
    }

    /// <summary>
    /// Validates that sub-condition contains only allowed keys.
    /// Supports TALENT, CFLAG, TCVAR, and compound operators (AND, OR, NOT).
    /// </summary>
    /// <param name="subCondition">Sub-condition dictionary to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when non-allowlisted keys are found</exception>
    private void ValidateConditionScope(Dictionary<string, object> subCondition)
    {
        var allowedKeys = new HashSet<string>(VariablePrefixes) { "TALENT", "FUNCTION", "AND", "OR", "NOT" };
        var invalidKeys = subCondition.Keys.Where(key => !allowedKeys.Contains(key)).ToList();

        if (invalidKeys.Any())
        {
            throw new InvalidOperationException($"Compound conditions with unsupported keys (found: {string.Join(", ", invalidKeys)}). Supported keys: {string.Join(", ", allowedKeys)}.");
        }
    }

    /// <summary>
    /// Evaluates a bitwise_and_cmp operator: (stateValue &amp; mask) op expectedValue
    /// Returns true if condition matches, false otherwise.
    /// Returns null if the operator value is not a valid bitwise_and_cmp dictionary.
    /// F759: Two-stage evaluation for compound bitwise-comparison
    /// </summary>
    private static bool? EvaluateBitwiseAndCmp(int stateValue, object? opValue)
    {
        if (opValue is not Dictionary<object, object> bitwiseDict)
            return null;

        var maskStr = bitwiseDict.GetValueOrDefault("mask", "0")?.ToString() ?? "0";
        var compOp = bitwiseDict.GetValueOrDefault("op", "eq")?.ToString() ?? "eq";
        var valueStr = bitwiseDict.GetValueOrDefault("value", "0")?.ToString() ?? "0";

        if (!int.TryParse(maskStr, out var mask) || !int.TryParse(valueStr, out var expectedValue))
            return null;

        var bitwiseResult = stateValue & mask;
        return EvaluateOperator(bitwiseResult, compOp, expectedValue);
    }

    /// <summary>
    /// Evaluates all operators in an opDict against a state value.
    /// Shared between TALENT inline block and EvaluateVariableCondition.
    /// Handles both simple operators (eq/ne/gt/etc.) and compound bitwise_and_cmp.
    /// Returns false if any operator condition fails (AND logic).
    /// F759: Eliminates loop duplication across both evaluation paths
    /// </summary>
    private static bool EvaluateOpDict(int stateValue, Dictionary<object, object> opDict)
    {
        foreach (var opKvp in opDict)
        {
            var op = opKvp.Key?.ToString();
            if (string.IsNullOrEmpty(op))
                continue;

            if (op == "bitwise_and_cmp")
            {
                var bitwiseResult = EvaluateBitwiseAndCmp(stateValue, opKvp.Value);
                if (bitwiseResult != true)
                    return false;
                continue;
            }

            if (!int.TryParse(opKvp.Value?.ToString(), out var expected))
                continue;
            if (!EvaluateOperator(stateValue, op, expected))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Evaluates operator against state value and expected value.
    /// Supports comparison operators (eq, ne, gt, gte, lt, lte) and bitwise_and.
    /// </summary>
    private static bool EvaluateOperator(int stateValue, string op, int expected) => op switch
    {
        "eq" => stateValue == expected,
        "ne" => stateValue != expected,
        "gt" => stateValue > expected,
        "gte" => stateValue >= expected,
        "lt" => stateValue < expected,
        "lte" => stateValue <= expected,
        "bitwise_and" => (stateValue & expected) != 0,
        _ => false
    };

    /// <summary>
    /// Evaluates variable conditions for any variable type (CFLAG, TCVAR, future EQUIP/STAIN/ITEM).
    /// Handles the common pattern of iterating through variable indices and operator conditions.
    /// Note: Accepts any string index without int.TryParse validation (unlike existing TALENT evaluation
    /// which requires numeric indices). This is correct for CFLAG/TCVAR which use string-based indices.
    /// </summary>
    /// <param name="variableType">Variable type name (e.g., "CFLAG", "TCVAR")</param>
    /// <param name="varDict">Variable condition dictionary</param>
    /// <param name="state">Game state dictionary</param>
    /// <returns>True if all variable conditions match state, false otherwise</returns>
    private static bool EvaluateVariableCondition(string variableType, Dictionary<object, object> varDict, Dictionary<string, int> state)
    {
        foreach (var kvp in varDict)
        {
            var indexStr = kvp.Key?.ToString();
            if (string.IsNullOrEmpty(indexStr))
                continue;

            // Construct state key: "{variableType}:{index}"
            var stateKey = $"{variableType}:{indexStr}";
            var stateValue = state.GetValueOrDefault(stateKey, 0);

            // Parse operator dictionary: { "ne": 0 } or { "bitwise_and_cmp": {...} }
            if (kvp.Value is Dictionary<object, object> opDict)
            {
                if (!EvaluateOpDict(stateValue, opDict))
                    return false;
            }
        }

        return true; // All variable conditions matched
    }

    private DisplayMode ParseDisplayMode(string? displayModeStr)
    {
        if (string.IsNullOrEmpty(displayModeStr))
            return DisplayMode.Default;

        return displayModeStr.ToLowerInvariant() switch
        {
            "default" => DisplayMode.Default,
            "newline" => DisplayMode.Newline,
            "wait" => DisplayMode.Wait,
            "keywait" => DisplayMode.KeyWait,
            "keywaitnewline" => DisplayMode.KeyWaitNewline,
            "keywaitwait" => DisplayMode.KeyWaitWait,
            "display" => DisplayMode.Display,
            "displaynewline" => DisplayMode.DisplayNewline,
            "displaywait" => DisplayMode.DisplayWait,
            _ => throw new ArgumentException($"Unknown displayMode: {displayModeStr}")
        };
    }
}

/// <summary>
/// Data structure for deserializing branches-format YAML files.
/// </summary>
internal class BranchesFileData
{
    public string Character { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public int? ComId { get; set; }
    public List<BranchData> Branches { get; set; } = new();
}

/// <summary>
/// Data structure for a single branch in branches-format YAML.
/// </summary>
internal class BranchData
{
    public List<string> Lines { get; set; } = new();

    [YamlMember(Alias = "lines_with_metadata")]
    public List<LineData>? LinesWithMetadata { get; set; }

    public Dictionary<string, object>? Condition { get; set; }
}

/// <summary>
/// Data structure for a line with metadata (displayMode).
/// Supports test scenarios where per-line displayMode is required.
/// </summary>
internal class LineData
{
    public string Text { get; set; } = string.Empty;

    [YamlMember(Alias = "display_mode")]
    public string? DisplayMode { get; set; }
}
