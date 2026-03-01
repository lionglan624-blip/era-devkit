using System.Text.RegularExpressions;
using Era.Core;
using Era.Core.Characters;
using Era.Core.Dialogue;
using Era.Core.Dialogue.Evaluation;
using Era.Core.Dialogue.Loading;
using Era.Core.Dialogue.Rendering;
using Era.Core.Dialogue.Selection;
using Era.Core.Functions;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace KojoComparer;

/// <summary>
/// Renders YAML kojo files using Era.Core.KojoEngine.
/// </summary>
public class YamlRunner : IYamlRunner
{
    /// <summary>
    /// Renders a YAML kojo file with given game context.
    /// </summary>
    /// <param name="yamlFilePath">Path to YAML file</param>
    /// <param name="context">Context dictionary with TALENT/ABL/TFLAG state (e.g., {"TALENT": {"16": 1}})</param>
    /// <returns>Rendered dialogue text</returns>
    public virtual string Render(string yamlFilePath, Dictionary<string, object> context)
    {
        var dialogueResult = RenderWithMetadata(yamlFilePath, context);

        // Join lines with newline to reconstruct string format
        return string.Join("\n", dialogueResult.DialogueLines.Select(dl => dl.Text));
    }

    /// <summary>
    /// Renders a YAML kojo file with given game context, returning DialogueResult with metadata.
    /// </summary>
    /// <param name="yamlFilePath">Path to YAML file</param>
    /// <param name="context">Context dictionary with TALENT/ABL/TFLAG state (e.g., {"TALENT": {"16": 1}})</param>
    /// <returns>DialogueResult with lines and displayMode metadata</returns>
    public virtual DialogueResult RenderWithMetadata(string yamlFilePath, Dictionary<string, object> context)
    {
        // Parse CharacterId from file path
        var characterId = ParseCharacterIdFromPath(yamlFilePath);

        // Read file to detect format
        var yamlContent = File.ReadAllText(yamlFilePath);

        // Detect format: "entries:" vs "branches:"
        if (yamlContent.Contains("entries:"))
        {
            // Use existing YamlDialogueLoader for entries format
            return RenderEntriesFormat(yamlFilePath, context, characterId);
        }
        else if (yamlContent.Contains("branches:"))
        {
            // Use new KojoBranchesParser for branches format
            return RenderBranchesFormat(yamlContent, context, characterId);
        }
        else
        {
            throw new InvalidOperationException($"Unknown YAML format. Expected 'entries:' or 'branches:' key in file: {yamlFilePath}");
        }
    }

    /// <summary>
    /// Render entries-format YAML using YamlDialogueLoader.
    /// </summary>
    private DialogueResult RenderEntriesFormat(string yamlFilePath, Dictionary<string, object> context, CharacterId characterId)
    {
        // Create DI components (per-call for thread safety)
        var loader = new YamlDialogueLoader();
        var evaluator = new ConditionEvaluator();
        var selector = new PriorityDialogueSelector(evaluator);
        var renderer = new TemplateDialogueRenderer(new SimpleCharacterDataService());

        // Load file directly (bypass KojoEngine path reconstruction)
        var fileResult = loader.Load(yamlFilePath);
        if (fileResult is Result<DialogueFile>.Failure f1)
            throw new InvalidOperationException($"Failed to load dialogue file: {f1.Error}");

        var file = ((Result<DialogueFile>.Success)fileResult).Value;

        // Create context adapter and wrap with character scope
        var contextAdapter = new ContextAdapter(context);
        var characterContext = new CharacterScopedContext(contextAdapter, characterId);

        // Select entry based on conditions
        var entryResult = selector.Select(file.Entries, characterContext);
        if (entryResult is Result<DialogueEntry>.Failure f2)
            throw new InvalidOperationException($"Failed to select dialogue: {f2.Error}");

        var entry = ((Result<DialogueEntry>.Success)entryResult).Value;

        // Render content
        var renderResult = renderer.Render(entry.Content, characterContext);
        if (renderResult is Result<string>.Failure f3)
            throw new InvalidOperationException($"Failed to render dialogue: {f3.Error}");

        var renderedContent = ((Result<string>.Success)renderResult).Value;
        var lines = renderedContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var dialogueLines = lines
            .Select(text => new DialogueLine(text, entry.DisplayMode))
            .ToList();

        return DialogueResult.Create(dialogueLines);
    }

    /// <summary>
    /// Render branches-format YAML using KojoBranchesParser.
    /// </summary>
    private DialogueResult RenderBranchesFormat(string yamlContent, Dictionary<string, object> context, CharacterId characterId)
    {
        var parser = new KojoBranchesParser();

        // Extract state from context: convert {"TALENT": {"16": 1}} to {"TALENT:16": 1}
        var state = ExtractStateFromContext(context);

        return parser.Parse(yamlContent, state);
    }

    /// <summary>
    /// Extracts flat state dictionary from nested context format.
    /// Converts {"TALENT": {"16": 1}} to {"TALENT:16": 1} for parser.
    /// </summary>
    private Dictionary<string, int> ExtractStateFromContext(Dictionary<string, object> context)
    {
        var state = new Dictionary<string, int>();

        // NOTE: TALENT uses three-part state keys (TALENT:{target}:{index}) to support
        // target-qualified conditions like TALENT:PLAYER:16. Other variable types (ABL, TFLAG)
        // use two-part keys ({TYPE}:{index}) because they are always character-scoped via
        // the context structure. Only TALENT needs compound key parsing because its YAML
        // conditions can contain target:index compound keys (e.g., "PLAYER:16").
        // Extract TALENT state
        if (context.TryGetValue("TALENT", out var talentObj) && talentObj is Dictionary<string, int> talentDict)
        {
            foreach (var (indexStr, value) in talentDict)
            {
                var (target, talentIndex) = TalentKeyParser.ParseTalentYamlKey(indexStr);
                var effectiveTarget = target ?? "TARGET";
                if (talentIndex.HasValue)
                {
                    state[$"TALENT:{effectiveTarget}:{talentIndex.Value}"] = value;
                }
                else if (target != null)
                {
                    // Symbolic key (e.g., "PLAYER" alone) — target-only pattern
                    state[$"TALENT:{target}"] = value;
                }
            }
        }

        // Extract ABL state
        if (context.TryGetValue("ABL", out var ablObj) && ablObj is Dictionary<string, int> ablDict)
        {
            foreach (var (indexStr, value) in ablDict)
            {
                state[$"ABL:{indexStr}"] = value;
            }
        }

        // Extract TFLAG state
        if (context.TryGetValue("TFLAG", out var tflagObj) && tflagObj is Dictionary<string, int> tflagDict)
        {
            foreach (var (indexStr, value) in tflagDict)
            {
                state[$"TFLAG:{indexStr}"] = value;
            }
        }

        return state;
    }

    /// <summary>
    /// Extracts CharacterId from YAML file path.
    /// Supports four formats:
    /// 1. Production COM: .../N_CharacterName/COM_NNN.yaml (e.g., Game/YAML/口上/1_美鈴/COM_311.yaml)
    /// 2. Production K{N}: .../N_CharacterName/K{N}_xxx_N.yaml (e.g., Game/YAML/Kojo/10_魔理沙/K10_会話親密_0.yaml)
    /// 3. Production KU: .../U_汎用/KU_xxx_N.yaml (e.g., Game/YAML/Kojo/U_汎用/KU_日常_0.yaml) → CharacterId(999)
    /// 4. Test: .../charactername_comN.yaml (e.g., tools/KojoComparer.Tests/TestData/meirin_com200.yaml)
    /// </summary>
    private CharacterId ParseCharacterIdFromPath(string yamlFilePath)
    {
        // Try production COM format first: N_CharacterName/COM_NNN.yaml
        var productionComPattern = @"(\d+)_[^/\\]+[/\\]COM_\d+\.yaml$";
        var match = Regex.Match(yamlFilePath, productionComPattern);

        if (match.Success)
        {
            return new CharacterId(int.Parse(match.Groups[1].Value));
        }

        // Try production K{N} format: K{N}_xxx_N.yaml (suffix optional)
        var productionKPattern = @"[/\\]K(\d+)_[^/\\]+?(?:_\d+)?\.yaml$";
        match = Regex.Match(yamlFilePath, productionKPattern);

        if (match.Success)
        {
            return new CharacterId(int.Parse(match.Groups[1].Value));
        }

        // Try production KU (universal) format: KU_xxx_N.yaml → CharacterId(999)
        var productionKuPattern = @"[/\\]KU_[^/\\]+_\d+\.yaml$";
        match = Regex.Match(yamlFilePath, productionKuPattern);

        if (match.Success)
        {
            // KU (universal) paths map to placeholder CharacterId(999)
            return new CharacterId(999);
        }

        // Try test format: meirin_comN.yaml
        var testPattern = @"meirin_com\d+\.yaml$";
        match = Regex.Match(yamlFilePath, testPattern);

        if (match.Success)
        {
            // meirin = character 1 (美鈴)
            return new CharacterId(1);
        }

        throw new ArgumentException($"Invalid YAML path format. Expected: .../N_CharacterName/COM_NNN.yaml, .../K{{N}}_xxx_N.yaml, .../KU_xxx_N.yaml, or .../meirin_comNNN.yaml, got: {yamlFilePath}");
    }

    /// <summary>
    /// Async wrapper for compatibility with existing tests.
    /// </summary>
    [Obsolete("Use Render() instead - no async operations needed")]
    public virtual Task<string> RenderAsync(string yamlFilePath, Dictionary<string, object> context)
    {
        return Task.FromResult(Render(yamlFilePath, context));
    }

    /// <summary>
    /// Adapter to convert Dictionary<string,object> context to IEvaluationContext.
    /// Used by YamlRunner to bridge KojoComparer's context format with KojoEngine API.
    /// </summary>
    private class ContextAdapter : IEvaluationContext
    {
        private readonly IVariableStore _variables;

        public ContextAdapter(Dictionary<string, object> context)
        {
            _variables = new VariableStoreAdapter(context);
        }

        public int ArgCount => 0;
        public CharacterId? CurrentCharacter => null; // Set by CharacterScopedContext wrapper
        public IVariableStore Variables => _variables;

        public T GetArg<T>(int index) => throw new NotSupportedException("YamlRunner context does not support GetArg");
        public T[] GetArgs<T>() => throw new NotSupportedException("YamlRunner context does not support GetArgs");
    }

    /// <summary>
    /// Context wrapper that ensures CurrentCharacter is set for condition evaluation.
    /// Delegates all other members to the wrapped context.
    /// </summary>
    private sealed class CharacterScopedContext(IEvaluationContext inner, CharacterId character) : IEvaluationContext
    {
        public int ArgCount => inner.ArgCount;
        public CharacterId? CurrentCharacter => character;
        public IVariableStore Variables => inner.Variables;
        public T GetArg<T>(int index) => inner.GetArg<T>(index);
        public T[] GetArgs<T>() => inner.GetArgs<T>();
    }

    /// <summary>
    /// Adapter to convert Dictionary<string,object> to IVariableStore.
    /// Supports TALENT/ABL/TFLAG lookups from nested dictionary format.
    /// Context format: {"TALENT": {"16": 1}, "ABL": {"10": 50}}
    /// </summary>
    private class VariableStoreAdapter : IVariableStore
    {
        private readonly Dictionary<string, object> _context;

        public VariableStoreAdapter(Dictionary<string, object> context)
        {
            _context = context ?? new Dictionary<string, object>();
        }

        public Result<int> GetTalent(CharacterId character, TalentIndex talent)
        {
            if (_context.TryGetValue("TALENT", out var talentObj) &&
                talentObj is Dictionary<string, int> talentDict &&
                talentDict.TryGetValue(talent.Value.ToString(), out var value))
            {
                return Result<int>.Ok(value);
            }
            return Result<int>.Ok(0); // Default to 0 if not set
        }

        public void SetTalent(CharacterId character, TalentIndex talent, int value) { }

        public Result<int> GetAbility(CharacterId character, AbilityIndex ability)
        {
            if (_context.TryGetValue("ABL", out var ablObj) &&
                ablObj is Dictionary<string, int> ablDict &&
                ablDict.TryGetValue(ability.Value.ToString(), out var value))
            {
                return Result<int>.Ok(value);
            }
            return Result<int>.Ok(0);
        }

        public void SetAbility(CharacterId character, AbilityIndex ability, int value) { }

        public int GetTFlag(FlagIndex index)
        {
            if (_context.TryGetValue("TFLAG", out var tflagObj) &&
                tflagObj is Dictionary<string, int> tflagDict &&
                tflagDict.TryGetValue(index.Value.ToString(), out var value))
            {
                return value;
            }
            return 0;
        }

        public void SetTFlag(FlagIndex index, int value) { }

        // Other IVariableStore methods return default values (0)
        // since KojoComparer only uses TALENT/ABL/TFLAG
        public int GetFlag(FlagIndex index) => 0;
        public void SetFlag(FlagIndex index, int value) { }
        public Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag) => Result<int>.Ok(0);
        public void SetCharacterFlag(CharacterId character, CharacterFlagIndex flag, int value) { }
        public Result<int> GetPalam(CharacterId character, PalamIndex index) => Result<int>.Ok(0);
        public void SetPalam(CharacterId character, PalamIndex index, int value) { }
        public Result<int> GetExp(CharacterId character, ExpIndex index) => Result<int>.Ok(0);
        public void SetExp(CharacterId character, ExpIndex index, int value) { }
        public Result<int> GetBase(CharacterId character, BaseIndex index) => Result<int>.Ok(0);
        public void SetBase(CharacterId character, BaseIndex index, int value) { }
        public Result<int> GetTCVar(CharacterId character, TCVarIndex index) => Result<int>.Ok(0);
        public void SetTCVar(CharacterId character, TCVarIndex index, int value) { }
        public Result<int> GetSource(CharacterId character, SourceIndex index) => Result<int>.Ok(0);
        public void SetSource(CharacterId character, SourceIndex index, int value) { }
        public Result<int> GetMark(CharacterId character, MarkIndex index) => Result<int>.Ok(0);
        public void SetMark(CharacterId character, MarkIndex index, int value) { }
        public Result<int> GetNowEx(CharacterId character, NowExIndex index) => Result<int>.Ok(0);
        public void SetNowEx(CharacterId character, NowExIndex index, int value) { }
        public Result<int> GetMaxBase(CharacterId character, MaxBaseIndex index) => Result<int>.Ok(0);
        public void SetMaxBase(CharacterId character, MaxBaseIndex index, int value) { }
        public Result<int> GetCup(CharacterId character, CupIndex index) => Result<int>.Ok(0);
        public void SetCup(CharacterId character, CupIndex index, int value) { }
        public Result<int> GetJuel(CharacterId character, int index) => Result<int>.Ok(0);
        public void SetJuel(CharacterId character, int index, int value) { }
        public Result<int> GetGotJuel(CharacterId character, int index) => Result<int>.Ok(0);
        public void SetGotJuel(CharacterId character, int index, int value) { }
        public Result<int> GetPalamLv(int index) => Result<int>.Ok(0);
        public void SetPalamLv(int index, int value) { }
        public Result<int> GetStain(CharacterId character, StainIndex index) => Result<int>.Ok(0);
        public void SetStain(CharacterId character, StainIndex index, int value) { }
        public Result<int> GetDownbase(CharacterId character, DownbaseIndex index) => Result<int>.Ok(0);
        public void SetDownbase(CharacterId character, DownbaseIndex index, int value) { }
        public Result<int> GetEquip(CharacterId character, int index) => Result<int>.Ok(0);
        public void SetEquip(CharacterId character, int index, int value) { }
        public Result<string> GetCharacterString(CharacterId character, CstrIndex index) => Result<string>.Ok(string.Empty);
        public Result<int> GetExpLv(int level) => Result<int>.Ok(0);
        public void SetExpLv(int index, int value) { }
        public int GetNoItem() => 0;
    }

    /// <summary>
    /// Simple ICharacterDataService implementation for KojoComparer.
    /// Returns placeholder character names in format "CharacterN".
    /// </summary>
    private class SimpleCharacterDataService : ICharacterDataService
    {
        public Result<string> GetCallName(CharacterId characterId)
        {
            return Result<string>.Ok($"Character{characterId.Value}");
        }
    }
}
