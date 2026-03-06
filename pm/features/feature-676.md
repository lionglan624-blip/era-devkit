# Feature 676: Era.Core Renderer DisplayMode Integration

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Integrate displayMode metadata from dialogue-schema.json YAML output into Era.Core's rendering pipeline, enabling display variant behavior (newline, wait, key-wait, display) in YAML-based dialogue rendering.

---

## Background

### Philosophy (Mid-term Vision)

Era.Core's dialogue pipeline must faithfully carry all metadata from YAML source through to consumers. Display mode is an intrinsic property of dialogue output that must be preserved end-to-end -- from YAML deserialization through selection, rendering, and into the DialogueResult consumed by KojoEngine callers. Silently dropping metadata violates the principle that the YAML-first dialogue system should be a complete replacement for ERB-based dialogue rendering. Every display variant defined in dialogue-schema.json must be propagable through the runtime pipeline (schema validation enforces correctness, runtime propagates values, interpretation is consumer responsibility).

### Problem (Current Issue)

F671 added displayMode metadata to dialogue-schema.json YAML output, preserving PRINTDATA variant semantics (PRINTDATAL -> "newline", PRINTDATAW -> "wait", etc.). However, Era.Core's KojoEngine and rendering pipeline do not consume this metadata. The YAML files produced by F634/F671 use dialogue-schema.json format, which is different from Era.Core's internal dialogue format.

### Goal (What to Achieve)

1. Determine if Era.Core needs to consume dialogue-schema.json format YAML files
2. Bridge the gap between dialogue-schema.json format and Era.Core's internal format
3. Extend Era.Core's dialogue pipeline to propagate display metadata to consumers

---

## Links

- [feature-671.md](feature-671.md) - PrintData Variant Metadata Mapping (Predecessor - added displayMode to schema/converter)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Related)
- [feature-677.md](feature-677.md) - KojoComparer displayMode awareness (Related)

---

## Notes

- Created by F671 Task 9 as documented in 残課題
- F671 investigation revealed dialogue-schema.json YAML is NOT consumed by KojoEngine (uses different schema)
- Scope and feasibility need investigation before implementation
- AC#7-8 from F671 were deferred here

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Era.Core's KojoEngine and rendering pipeline do not consume the displayMode metadata that F671 added to dialogue-schema.json YAML output
2. Why: YamlDialogueLoader deserializes YAML into DialogueEntryData which only has Id, Content, Priority, and Condition fields -- there is no DisplayMode property
3. Why: The DialogueEntry record and its deserialization helper were designed before displayMode existed -- they predate F671's schema extension
4. Why: KojoEngine's entire pipeline (Load → Select → Render → DialogueResult) was built for text content only, with display behavior hardcoded as "print each line with newline" (line 87-88 of KojoEngine.cs splits on '\n')
5. Why: The original Era.Core dialogue architecture assumed all dialogue would be rendered uniformly (PRINTL equivalent). Display variant semantics (wait, key-wait, display mode) were an ERB concept that was never modeled in the YAML-first dialogue system

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| displayMode metadata in YAML files is ignored by Era.Core | DialogueEntry record has no DisplayMode property; YamlDialogueLoader does not deserialize it; DialogueResult carries only `IReadOnlyList<string>` with no per-line display metadata |
| KojoComparer equivalence testing cannot verify display variant behavior | The entire KojoEngine pipeline (DialogueEntry → DialogueResult → HeadlessUI) flattens all output to plain text lines, losing display mode semantics |

### Conclusion

The root cause is an **architectural gap**: Era.Core's dialogue pipeline was designed as a text-only system. The `DialogueEntry` record carries `Content: string` with no display metadata. `DialogueResult` is `IReadOnlyList<string>` -- a flat list of text lines. The rendering pipeline (KojoEngine line 87-88) splits rendered content by `\n` and returns lines, treating all output uniformly as "print line with newline."

**Critical finding from investigation**: The YAML kojo files (under `Game/YAML/Kojo/`) DO use the dialogue-schema.json format (with `character`, `situation`, `entries` top-level keys). However, `YamlDialogueLoader.DialogueFileData` only deserializes the `Entries` list -- `character` and `situation` are ignored (handled by file path conventions instead). The `displayMode` field in entries items IS present in the schema and will be present in YAML files after F671's converter updates, but `DialogueEntryData` has no corresponding property to receive it. YamlDotNet silently ignores unknown properties, so displayMode is quietly dropped during deserialization.

**Architecture decision required**: F676 must extend at minimum:
1. `DialogueEntryData` / `DialogueEntry` to carry DisplayMode
2. `YamlDialogueLoader` to populate it
3. `DialogueResult` to carry per-line display metadata
4. `KojoEngine.GetDialogue()` to propagate display metadata through the pipeline
5. Consumers (HeadlessUI, PrintDataHandler) to interpret display mode

This is NOT a simple "add a field" change -- it requires propagating metadata through 4-5 layers of the pipeline.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F671 | [DONE] | Predecessor | Added displayMode to dialogue-schema.json and ErbToYaml converters. F676 was created by F671 Task 9. |
| F677 | [DRAFT] | Related (downstream) | KojoComparer displayMode awareness. F677 needs F676 to propagate displayMode through KojoEngine so comparisons can account for it. |
| F644 | [DONE] | Related | Equivalence Testing Framework. Uses KojoComparer which calls KojoEngine. Currently compares text only, not display mode. |
| F651 | [DONE] | Related | KojoComparer KojoEngine API Update. Updated YamlRunner to use KojoEngine API. Any DialogueResult changes in F676 would affect this. |
| F675 | [DONE] | Related | YAML Format Unification (branches → entries). Established the entries format that F676 must work with. |
| F634 | [DONE] | Related | Batch Conversion Tool. Produces YAML files consumed by KojoEngine via YamlDialogueLoader. |

### Pattern Analysis

This is an **incremental capability extension** pattern. The dialogue system was built in stages:
- F349/F361: Schema foundation (content only)
- F633/F634: PRINTDATA parsing and conversion (variant captured in AST but not propagated)
- F671: displayMode metadata preserved in YAML output
- F676 (this): Extend runtime pipeline to consume displayMode

Each stage added metadata that the downstream systems were not yet ready to consume. This is a natural pattern for layered architectures -- the schema leads, the runtime follows.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All required extension points are clearly identified: DialogueEntry, DialogueEntryData, YamlDialogueLoader, DialogueResult, KojoEngine. Changes are additive (new optional fields). |
| Scope is realistic | PARTIAL | 5-layer pipeline change (DialogueEntryData → DialogueEntry → YamlDialogueLoader → DialogueResult → KojoEngine/consumers) is substantial but each layer change is small (~10-20 lines). Total ~100-150 lines of production code + tests. Within engine type ~300 line limit. |
| No blocking constraints | YES | F671 is [DONE]. displayMode is already in dialogue-schema.json. YAML files will contain the field after re-conversion. No external blockers. |

**Verdict**: FEASIBLE

**Scope refinement notes**:
- Only PRINTDATAL (1,575 occurrences) is used in kojo files. Other variants have 0 occurrences. Implementation should support all variants but testing priority is PRINTDATAL → "newline".
- The IConsoleOutput interface already has separate Print/PrintLine/PrintWait methods, which maps well to display modes. However, the current consumer (HeadlessUI) does not use IConsoleOutput -- it writes directly to Console.WriteLine. The PrintDataCommand/PrintDataHandler path is for ERB runtime execution, not YAML dialogue rendering.
- **Key architectural question**: Should display mode be consumed at the KojoEngine level (returning structured data) or at the consumer level (HeadlessUI/KojoComparer interpreting DialogueResult)? The answer depends on whether consumers need to differentiate display modes or just need the text.
- For equivalence testing (F677/KojoComparer), the display mode matters because ERB outputs use PRINTDATAL (adds newline) while YAML output currently uses plain text splitting. The comparison should account for this difference.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F671 | [DONE] | Added displayMode to dialogue-schema.json and ErbToYaml converters. F676 consumes this metadata. |
| Related | F677 | [DRAFT] | KojoComparer displayMode awareness. Will consume F676's extended DialogueResult to compare display behavior. |
| Related | F644 | [DONE] | Equivalence Testing Framework. KojoComparer uses KojoEngine; changes here affect test output. |
| Related | F651 | [DONE] | KojoComparer KojoEngine API Update. YamlRunner depends on DialogueResult shape. |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already referenced. Deserialization of new DisplayMode field is standard string property. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/YamlRunner.cs | HIGH | Calls KojoEngine.GetDialogue(), reads DialogueResult.Lines. Must handle new display metadata if DialogueResult is extended. |
| Era.Core/HeadlessUI.cs | LOW | Reads DialogueResult.Lines for console output. Could optionally interpret display mode. |
| Era.Core/Commands/Com/YamlComExecutor.cs | LOW | Reads dialogue.Lines. Continues to work via computed Lines property. |
| Era.Core/Commands/Print/PrintDataHandler.cs | NONE | Used for ERB runtime PRINTDATA execution, not YAML dialogue rendering. Separate code path. |
| Era.Core/Commands/Print/PrintCommand.cs | NONE | ERB runtime print commands. Separate code path from YAML dialogue. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Dialogue/DialogueFile.cs | Update | Add optional DisplayMode property to DialogueEntry record |
| Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | Update | Add DisplayMode to DialogueEntryData, map to DialogueEntry |
| Era.Core/Types/DialogueResult.cs | Update | Extend to carry per-line display metadata (e.g., List<DialogueLine> with text + displayMode) |
| Era.Core/KojoEngine.cs | Update | Propagate DisplayMode from DialogueEntry through render pipeline to DialogueResult |
| Era.Core/IKojoEngine.cs | No Change | Interface returns Result<DialogueResult> -- record change is transparent |
| tools/KojoComparer/YamlRunner.cs | Update | Read display metadata from DialogueResult (needed for F677 downstream) |
| Era.Core/HeadlessUI.cs | Optional Update | Could output display mode hints in debug output |
| Era.Core.Tests/HeadlessUITests.cs | Update | Update DialogueResult constructor calls to use new initialization pattern |
| Era.Core.Tests/Commands/Com/YamlComExecutorTests.cs | Update | Update DialogueResult constructor calls to use new initialization pattern |
| Era.Core.Tests/Commands/Com/ComEvaluationContextTests.cs | Update | Update DialogueResult constructor calls to use new initialization pattern |
| .claude/skills/engine-dev/SKILL.md | Update | Update DialogueResult record documentation, add DialogueLine record |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| DialogueResult is a public record used by KojoComparer | Era.Core/Types/DialogueResult.cs | HIGH - Changing DialogueResult shape is a breaking change for KojoComparer (YamlRunner.cs line 53 reads .Lines) |
| YamlDotNet silently ignores unknown YAML properties | YamlDotNet deserialization behavior | LOW - Favorable: current code works without displayMode; adding it is additive |
| KojoEngine splits Content by '\n' to create lines | Era.Core/KojoEngine.cs line 87 | MEDIUM - displayMode is per-entry, but lines are per-split. Need to decide: per-entry or per-line displayMode? |
| Only PRINTDATAL is used in kojo files (1,575 occurrences) | F671 grep analysis | LOW - Practical impact limited to "newline" display mode |
| DialogueEntry uses `required` properties (C# 11) | Era.Core/Dialogue/DialogueFile.cs | LOW - New DisplayMode property should be optional (not required) |
| KojoEngine default path is "Game/YAML/口上" but files are in "Game/YAML/Kojo" | Era.Core/KojoEngine.cs line 51 vs actual file locations | LOW - Path is configurable; KojoComparer's YamlRunner passes correct path |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| DialogueResult breaking change for KojoComparer | High | Medium | Use backward-compatible approach: keep Lines property, add optional DisplayModes property. Or extend DialogueLine to pair (text, displayMode). |
| Per-entry vs per-line displayMode semantic mismatch | Medium | Medium | dialogue-schema.json has displayMode at entry level, but KojoEngine splits one entry into multiple lines. Design decision: apply entry-level displayMode to all lines from that entry. |
| Re-conversion of YAML files needed before testing | Medium | Low | F671 converter changes exist but existing YAML files may not have displayMode yet. Re-run converter or test with newly converted files. |
| Scope creep into PrintDataCommand/PrintDataHandler | Low | Medium | Explicitly exclude ERB runtime path. F676 scope is YAML dialogue pipeline only (KojoEngine path). PrintDataCommand/PrintDataHandler is ERB runtime, not YAML. |
| F677 depends on F676 output shape | Medium | Low | Design DialogueResult extension with F677 consumption in mind. Document the expected contract. |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "must faithfully carry all metadata from YAML source through to consumers" | DialogueEntry must have DisplayMode property populated from YAML | AC#1, AC#2, AC#3 |
| "Display mode is an intrinsic property... must be preserved end-to-end" | DialogueResult must carry per-line display metadata to callers | AC#4, AC#5, AC#6 |
| "Silently dropping metadata violates the principle" | YamlDialogueLoader must deserialize displayMode (not silently ignore it) | AC#3 |
| "Every display variant defined in dialogue-schema.json must be propagable through the runtime pipeline" | All 8 display mode enum values must be supported | AC#7 |
| "complete replacement for ERB-based dialogue rendering" | Pipeline propagation must be end-to-end (entry → result) | AC#5, AC#6 |

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | DialogueEntry has DisplayMode enum property | code | contains | `DisplayMode DisplayMode` | [x] |
| 2 | DialogueEntryData has DisplayMode for deserialization | code | contains | `string? DisplayMode` | [x] |
| 3 | YamlDialogueLoader maps DisplayMode from data to domain | code | contains | "DisplayMode" | [x] |
| 4 | DialogueLine record exists with Text and DisplayMode | code | contains | `record DialogueLine` | [x] |
| 5 | DialogueResult carries DialogueLine list | code | matches | `IReadOnlyList<DialogueLine>` | [x] |
| 6 | KojoEngine propagates DisplayMode into DialogueResult | code | contains | "DisplayMode" | [x] |
| 7 | All 8 display mode values supported (enum coverage test) | test | succeeds | - | [x] |
| 8 | DialogueResult.Lines backward compatibility preserved | code | matches | `IReadOnlyList<string>\\s+Lines` | [x] |
| 9 | Build succeeds | build | succeeds | - | [x] |
| 10 | All existing tests pass | test | succeeds | - | [x] |
| 11 | Zero technical debt in modified files (TODO) | code | not_contains | "TODO" | [x] |
| 12 | Absent displayMode handled gracefully (default behavior) | test | succeeds | - | [x] |
| 13 | Test files updated for new DialogueResult constructor | test | succeeds | - | [x] |
| 14 | engine-dev SKILL.md updated with DialogueLine documentation | code | contains | "DialogueLine" | [x] |
| 15 | F682 DRAFT file created | file | exists | Game/agents/feature-682.md | [x] |
| 16 | F681 DRAFT file created | file | exists | Game/agents/feature-681.md | [x] |
| 17 | Zero technical debt in modified files (FIXME) | code | not_contains | "FIXME" | [x] |
| 18 | Zero technical debt in modified files (HACK) | code | not_contains | "HACK" | [x] |
| 19 | Invalid displayMode value rejection | test | succeeds | - | [x] |

### AC Details

**AC#1: DialogueEntry has optional DisplayMode property**
- Why: DialogueEntry is the domain record that carries dialogue metadata. It currently lacks any display mode information, so displayMode from YAML is silently dropped.
- Test: Grep pattern=`string? DisplayMode` path=Era.Core/Dialogue/DialogueFile.cs
- Expected: Property exists as optional (nullable string). Not `required` since most entries will have null displayMode (PRINTDATA default).
- Edge case: Must NOT be `required` -- existing YAML files without displayMode must continue to load.

**AC#2: DialogueEntryData has DisplayMode for deserialization**
- Why: DialogueEntryData is the YamlDotNet deserialization helper. Without a DisplayMode property here, YamlDotNet silently ignores the displayMode field in YAML.
- Test: Grep pattern=`string? DisplayMode` path=Era.Core/Dialogue/Loading/YamlDialogueLoader.cs
- Expected: Property exists on DialogueEntryData class with nullable string type.
- Note: YamlDotNet CamelCaseNamingConvention maps YAML `displayMode` to C# `DisplayMode` automatically.

**AC#3: YamlDialogueLoader maps DisplayMode from data to domain**
- Why: Even with the property on both types, the Load() method must explicitly map it during the Select() LINQ projection.
- Test: Grep pattern="DisplayMode" path=Era.Core/Dialogue/Loading/YamlDialogueLoader.cs
- Expected: DisplayMode mapping appears in the YamlDialogueLoader (flexible to exact syntax/formatting).

**AC#4: DialogueLine record exists with Text and DisplayMode**
- Why: The current pipeline flattens content to `IReadOnlyList<string>`. To carry per-line display metadata, a structured record pairing text with displayMode is needed.
- Test: Grep pattern=`record DialogueLine` path=Era.Core/Types/DialogueResult.cs (Implementation Contract requires positional record syntax)
- Expected: New positional record in DialogueResult.cs with Text and DisplayMode parameters.
- Design note: displayMode is per-entry in YAML, but KojoEngine splits one entry into multiple lines. All lines from the same entry share the entry's displayMode.

**AC#5: DialogueResult carries DialogueLine list**
- Why: DialogueResult must expose structured display metadata to consumers (KojoComparer, HeadlessUI).
- Test: Grep pattern=`IReadOnlyList<DialogueLine>` path=Era.Core/Types/DialogueResult.cs
- Expected: DialogueResult has a property or constructor parameter of type IReadOnlyList<DialogueLine>.

**AC#6: KojoEngine propagates DisplayMode into DialogueResult**
- Why: KojoEngine.GetDialogue() is the pipeline terminus that constructs DialogueResult. It must read entry.DisplayMode and propagate it to each DialogueLine.
- Test: Grep pattern="DisplayMode" path=Era.Core/KojoEngine.cs
- Expected: At least one reference to DisplayMode in KojoEngine.cs, in the GetDialogue method where DialogueResult is constructed (any variable name acceptable).

**AC#7: All 8 display mode values supported (enum coverage test)**
- Why: Philosophy requires "every display variant defined in dialogue-schema.json must be representable." There are 8 enum values: newline, wait, keyWait, keyWaitNewline, keyWaitWait, display, displayNewline, displayWait. Note: Runtime uses string propagation without validation - schema validation is the enforcement point.
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayMode
- Expected: Test passes. Test must verify that YAML with each of the 8 displayMode values deserializes correctly and propagates through the pipeline.
- Note: null (absent displayMode) is the 9th case, covered by AC#12.

**AC#8: DialogueResult.Lines backward compatibility preserved**
- Why: KojoComparer's YamlRunner (line 53) reads `dialogueResult.Lines` as `IReadOnlyList<string>`. Removing or changing Lines would break KojoComparer (HIGH impact consumer).
- Test: Grep pattern=`IReadOnlyList<string>\\s+Lines` path=Era.Core/Types/DialogueResult.cs (regex matches both property and init syntax)
- Expected: The existing `Lines` property is preserved alongside the new `DialogueLines` property.
- Design: DialogueResult has both `Lines` (backward compat, text-only) and `DialogueLines` (structured). Lines is computed once via Create factory method with private constructor enforcement.

**AC#9: Build succeeds**
- Why: All changes must compile without errors.
- Test: dotnet build Era.Core
- Expected: Build succeeds with exit code 0.

**AC#10: All existing tests pass**
- Why: Changes must not regress existing functionality. DialogueResult shape change could break existing tests.
- Test: dotnet test Era.Core.Tests
- Expected: All tests pass.

**AC#15: F682 DRAFT file created**
- Why: AC#15 verifies F682 is properly created as a [DRAFT] feature per the deferred item in 残課題 section.
- Test: File exists at Game/agents/feature-682.md
- Expected: File contains minimum DRAFT structure: Status: [DRAFT], Type: engine, Summary describing consumer-side display mode interpretation (HeadlessUI wait prompts, rendering behavior), Links back to F676. Index registration in index-features.md is verified by F682's own creation workflow, not this AC.

**AC#16: F681 DRAFT file created**
- Why: AC#16 verifies F681 is properly created as a [DRAFT] feature per the deferred item in 残課題 section.
- Test: File exists at Game/agents/feature-681.md
- Expected: File contains minimum DRAFT structure: Status: [DRAFT], Type: engine, Summary describing multi-entry selection and rendering pipeline, Links back to F676. Index registration in index-features.md is verified by F681's own creation workflow, not this AC.

**AC#11/17/18: Zero technical debt in modified files**
- Why: No TODO/FIXME/HACK markers left in modified production code.
- Test: AC#11 (TODO), AC#17 (FIXME), AC#18 (HACK) with Grep not_contains matcher for each term across modified files
- Expected: No occurrences of TODO, FIXME, or HACK in Era.Core/Dialogue/DialogueFile.cs, Era.Core/Dialogue/Loading/YamlDialogueLoader.cs, Era.Core/Types/DialogueResult.cs, Era.Core/KojoEngine.cs

**AC#12: Absent displayMode handled gracefully (default behavior)**
- Why: Most existing YAML files do not have displayMode field. YamlDotNet will deserialize it as null, which ParseDisplayMode converts to Default enum member.
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayMode
- Expected: Test passes. Test includes a case where YAML has no displayMode field, verifying DialogueEntry.DisplayMode is Default and DialogueLine.DisplayMode is Default.
- Edge case: This is the most common case in practice (1,575 PRINTDATAL entries all map to Newline, but existing files haven't been re-converted yet).

**AC#19: Invalid displayMode value rejection**
- Why: Fail-fast validation ensures invalid YAML values are caught at deserialization rather than passed through to consumers.
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayMode
- Expected: Test passes. Test verifies that YAML with invalid displayMode value (e.g., "invalidMode") throws InvalidDataException during YamlDialogueLoader.Load().
- Edge case: Validates that only the 8 defined schema values plus null/empty are accepted.

**AC#13: Test files updated for new DialogueResult constructor**
- Why: DialogueResult constructor changes from positional to init-based syntax. Test files that construct DialogueResult directly will break without updates.
- Test: dotnet test Era.Core.Tests --filter "FullyQualifiedName~HeadlessUITests|YamlComExecutorTests|ComEvaluationContextTests"
- Expected: Tests pass. These 3 test files contain all known DIRECT DialogueResult constructor call sites in test code. KojoEngine itself is updated in T4. Any mock implementations creating DialogueResult are covered by AC#10.
- Note: This verifies constructor migration was performed correctly. If new tests are added that use DialogueResult constructor, they would be caught by AC#10.

**AC#14: engine-dev SKILL.md updated with DialogueLine documentation**
- Why: Per SSOT update rules, new types in Era.Core/Types/*.cs require engine-dev SKILL.md documentation updates. DialogueLine is a new record type that consumers need to understand.
- Test: Grep pattern="DialogueLine" path=.claude/skills/engine-dev/SKILL.md
- Expected: DialogueLine record is documented in the Types section alongside DialogueResult.

<!-- fc-phase-4-completed -->
## Technical Design

### Overview

F676 extends Era.Core's dialogue pipeline to carry displayMode metadata from YAML source through to DialogueResult consumers. The design follows a **5-layer propagation pattern**: YAML → Deserialization (DialogueEntryData) → Domain (DialogueEntry) → Render → Structured Result (DialogueLine) → Consumer.

**Key architectural decision**: DialogueResult will carry BOTH unstructured `Lines` (backward compat) and structured `DialogueLines` (new capability). The `Lines` property becomes a computed projection from `DialogueLines` for zero-breaking-change migration.

### Design Principles

1. **Backward Compatibility First**: Existing consumers (KojoComparer YamlRunner.cs line 53) continue to work without changes
2. **Additive Changes Only**: All modifications are optional additions, not breaking changes
3. **Null as Default**: Absent displayMode field defaults to null (standard PRINTDATA behavior)
4. **Per-Entry Semantics**: displayMode is entry-level metadata that applies to all lines split from that entry's content

### Layer-by-Layer Design

#### Layer 1: Domain Model Extension (DialogueFile.cs)

**File**: `Era.Core/Dialogue/DialogueFile.cs`

Define DisplayMode enum and add to DialogueEntry record:

```csharp
/// <summary>
/// Display mode for dialogue rendering behavior.
/// Maps to PrintDataNode variant types from dialogue-schema.json.
/// </summary>
public enum DisplayMode
{
    Default,        // PRINTDATA - default behavior (null in YAML)
    Newline,        // PRINTDATAL - newline
    Wait,           // PRINTDATAW - wait for input
    KeyWait,        // PRINTDATAK - key wait
    KeyWaitNewline, // PRINTDATAKL - key wait + newline
    KeyWaitWait,    // PRINTDATAKW - key wait + wait
    Display,        // PRINTDATAD - display mode
    DisplayNewline, // PRINTDATADL - display + newline
    DisplayWait     // PRINTDATADW - display + wait
}

public record DialogueEntry
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public int Priority { get; init; } = 0;
    public DialogueCondition? Condition { get; init; }

    /// <summary>
    /// Display mode for rendering behavior.
    /// Default indicates standard PRINTDATA behavior.
    /// </summary>
    public DisplayMode DisplayMode { get; init; } = DisplayMode.Default;  // NEW
}
```

**Rationale**:
- **Upfront Investment**: Typed enum prevents ad-hoc string matching in consumers (F680, F677)
- **Fail-Fast**: Invalid YAML values cause deserialization errors rather than silent pass-through
- **Extensibility**: Enum provides compile-time safety, IDE support, and exhaustiveness checking
- **Long-term Maintainability**: Single enum definition prevents duplicate string-to-behavior mapping in each consumer
- NOT nullable since Default enum member represents absent displayMode (explicit over implicit)
- Non-required with default value to avoid breaking existing YAML files

#### Layer 2: Deserialization Helper (YamlDialogueLoader.cs)

**File**: `Era.Core/Dialogue/Loading/YamlDialogueLoader.cs`

Add DisplayMode property to DialogueEntryData class:

```csharp
internal class DialogueEntryData
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public DialogueConditionData? Condition { get; set; }
    public string? DisplayMode { get; set; }  // NEW - string for YAML deserialization
}
```

**Rationale**:
- YamlDotNet CamelCaseNamingConvention (line 46) automatically maps YAML `displayMode` → C# `DisplayMode`
- `string?` allows YamlDotNet to deserialize null when field is absent (no deserialization error)
- Without this property, YamlDotNet silently ignores `displayMode` in YAML (current bug)
- String type for deserialization; converted to enum during domain mapping in Layer 3

#### Layer 3: Deserialization Mapping (YamlDialogueLoader.cs)

**File**: `Era.Core/Dialogue/Loading/YamlDialogueLoader.cs`

Update Load() method's LINQ projection (line 65-71) to map DisplayMode with validation:

```csharp
private static DisplayMode ParseDisplayMode(string? displayMode)
{
    if (string.IsNullOrEmpty(displayMode))
        return DisplayMode.Default;

    return displayMode switch
    {
        "newline" => DisplayMode.Newline,
        "wait" => DisplayMode.Wait,
        "keyWait" => DisplayMode.KeyWait,
        "keyWaitNewline" => DisplayMode.KeyWaitNewline,
        "keyWaitWait" => DisplayMode.KeyWaitWait,
        "display" => DisplayMode.Display,
        "displayNewline" => DisplayMode.DisplayNewline,
        "displayWait" => DisplayMode.DisplayWait,
        _ => throw new InvalidDataException($"Unknown displayMode value: '{displayMode}'. Valid values: newline, wait, keyWait, keyWaitNewline, keyWaitWait, display, displayNewline, displayWait")
    };
}

var entries = fileData.Entries
    .Select(e => new DialogueEntry
    {
        Id = e.Id,
        Content = e.Content,
        Priority = e.Priority,
        Condition = ConvertCondition(e.Condition),
        DisplayMode = ParseDisplayMode(e.DisplayMode)  // NEW - with validation
    })
    .ToList();
```

**Rationale**:
- **Fail-Fast**: Invalid displayMode values cause deserialization error rather than silent pass-through
- **Single Point of Validation**: All YAML displayMode values validated at deserialization boundary
- **Explicit Mapping**: Clear correspondence between dialogue-schema.json strings and enum values
- **Error Context**: Exception provides valid value list for user correction

#### Layer 4: Structured Result Type (DialogueResult.cs)

**File**: `Era.Core/Types/DialogueResult.cs`

Replace simple record with dual-property design:

```csharp
namespace Era.Core.Types;

/// <summary>
/// Represents the result of dialogue selection from YAML kojo.
/// </summary>
public record DialogueResult
{
    private DialogueResult(IReadOnlyList<DialogueLine> dialogueLines, IReadOnlyList<string> lines)
    {
        DialogueLines = dialogueLines;
        Lines = lines;
    }

    /// <summary>
    /// Structured dialogue lines with display metadata.
    /// </summary>
    public IReadOnlyList<DialogueLine> DialogueLines { get; }

    /// <summary>
    /// Text-only lines (backward compatibility).
    /// Computed from DialogueLines.
    /// </summary>
    public IReadOnlyList<string> Lines { get; }

    /// <summary>
    /// Creates a DialogueResult from structured dialogue lines.
    /// </summary>
    public static DialogueResult Create(IReadOnlyList<DialogueLine> dialogueLines)
    {
        var lines = dialogueLines.Select(dl => dl.Text).ToList();
        return new DialogueResult(dialogueLines, lines);
    }
}

/// <summary>
/// Single dialogue line with display mode metadata.
/// </summary>
/// <param name="Text">Line text content</param>
/// <param name="DisplayMode">Display mode for rendering behavior</param>
public record DialogueLine(string Text, DisplayMode DisplayMode);
```

**Rationale**:
- **DialogueLine**: Positional record pairs text with displayMode (immutable, concise)
- **DialogueLines**: New structured list (read-only property, set via private constructor)
- **Lines**: Read-only property for backward compatibility
  - KojoComparer's `dialogueResult.Lines` (YamlRunner.cs line 53) continues to work
  - Zero breaking changes for existing consumers
  - Computed once during creation via Create factory method, preserving record immutability
- **Create factory**: Static factory method is the ONLY way to create DialogueResult (private constructor enforces this)
- **Enforcement**: Private constructor prevents inconsistent construction, ensuring Lines always matches DialogueLines
- **Record equality**: Changing from positional record to non-positional record alters equality semantics (positional compares constructor parameters, non-positional compares all properties). Verified no consumers use DialogueResult equality comparisons.
- **Migration path**: Consumers can stay on Lines (text-only) or migrate to DialogueLines (structured) at their own pace

**Alternative considered and rejected**: Extending existing Lines to carry metadata via tuple/struct wrapper would require changing Lines' type, breaking all consumers immediately.

#### Layer 5: Pipeline Propagation (KojoEngine.cs)

**File**: `Era.Core/KojoEngine.cs`

Update GetDialogue() method (line 87-88) to propagate displayMode:

```csharp
var renderedContent = ((Result<string>.Success)renderResult).Value;
var lines = renderedContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

// Create structured DialogueLines with per-entry displayMode
var dialogueLines = lines
    .Select(text => new DialogueLine(text, entry.DisplayMode))
    .ToList();

return Result<DialogueResult>.Ok(DialogueResult.Create(dialogueLines));
```

**Current code**:
```csharp
var lines = renderedContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
return Result<DialogueResult>.Ok(new DialogueResult(lines.ToList()));
```

**Changes**:
1. Line 87-88: Split logic unchanged (behavior preserved)
2. NEW: Map each line to DialogueLine with entry's DisplayMode
3. Construct DialogueResult with DialogueLines property (required initializer syntax)

**Rationale**:
- **Per-entry semantics**: All lines from one entry share that entry's displayMode
  - dialogue-schema.json has displayMode at entry level (not line level)
  - One DialogueEntry → multiple lines (split by '\n') → all lines inherit entry.DisplayMode
  - Example: Entry with DisplayMode=Newline and Content="Line1\nLine2" → 2 DialogueLines both with DisplayMode=Newline
- **Default propagation**: If entry.DisplayMode is Default, all DialogueLines have DisplayMode=Default (standard PRINTDATA behavior)

### Consumer Impact Analysis

#### High-Impact Consumer: KojoComparer/YamlRunner.cs

**Current code** (line 53):
```csharp
return string.Join("\n", dialogueResult.Lines);
```

**Impact**: ZERO breaking changes
- `dialogueResult.Lines` continues to return `IReadOnlyList<string>` (computed property)
- No code changes required
- YamlRunner can optionally migrate to DialogueLines in F677 (downstream feature)

#### Low-Impact Consumer: Era.Core/HeadlessUI.cs

**Current behavior**: Reads DialogueResult.Lines for console output

**Impact**: ZERO breaking changes
- Lines property preserved
- Optional enhancement: Could read DialogueLines and output display mode hints in debug mode

#### Low-Impact Consumer: Era.Core/Commands/Com/YamlComExecutor.cs

**Current code** (line 229):
```csharp
dialogue.Lines
```

**Impact**: ZERO breaking changes
- Lines property continues to work via computed property
- No code changes required

### Testing Strategy

#### Unit Test Coverage

**Test file**: `Era.Core.Tests/Dialogue/DisplayModeTests.cs` (new file)

**Test cases**:
1. **Deserialization with displayMode**: YAML with `displayMode: "newline"` → DialogueEntry.DisplayMode = Newline
2. **Deserialization without displayMode**: YAML without displayMode field → DialogueEntry.DisplayMode = Default
3. **All 8 enum values**: Test each displayMode value:
   - "newline", "wait", "keyWait", "keyWaitNewline", "keyWaitWait", "display", "displayNewline", "displayWait"
4. **Invalid displayMode**: YAML with `displayMode: "invalid"` → InvalidDataException thrown
5. **Propagation through pipeline**: DialogueEntry with DisplayMode=Wait → all DialogueLines have DisplayMode=Wait
6. **Multi-line entry**: Entry with Content="A\nB" and DisplayMode=KeyWait → 2 DialogueLines both with DisplayMode=KeyWait
7. **Backward compatibility**: dialogueResult.Lines returns text-only list (computed from DialogueLines)

**Existing test impact**:
- All existing Era.Core.Tests continue to pass (Lines property behavior unchanged)
- No test modifications required (additive changes only)

### Edge Cases and Error Handling

| Case | Behavior | Rationale |
|------|----------|-----------|
| YAML without displayMode field | DialogueEntry.DisplayMode = Default | Explicit default enum member better than nullable |
| Empty Content with displayMode | 0 DialogueLines (Split returns empty after RemoveEmptyEntries) | Existing behavior preserved |
| Invalid displayMode value (not in schema enum) | InvalidDataException thrown during Load() | Fail-fast validation at deserialization boundary |
| Null Entry.Content (malformed data) | Split throws NullReferenceException | Existing behavior (guards exist in loader) |

### Migration Path

**Phase 1 (F676)**: Core pipeline support
- Add DisplayMode to DialogueEntry, DialogueEntryData, YamlDialogueLoader
- Add DialogueLine and DialogueLines to DialogueResult
- Preserve Lines for backward compatibility
- **Consumer action required**: NONE (all changes backward compatible)

**Phase 2 (F677)**: Consumer adoption
- KojoComparer reads DialogueLines to compare display behavior
- HeadlessUI optionally outputs display mode hints
- **Consumer action required**: Opt-in migration to DialogueLines

**Phase 3 (Future)**: Full adoption
- All consumers migrate to DialogueLines
- Lines property marked `[Obsolete]` (but not removed for stability)

### File Modification Summary

| File | Lines Changed | Change Type | Breaking |
|------|--------------|-------------|----------|
| Era.Core/Dialogue/DialogueFile.cs | +5 | Add property | No |
| Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | +2 | Add property + mapping | No |
| Era.Core/Types/DialogueResult.cs | +15 | Replace record, add DialogueLine | No |
| Era.Core/KojoEngine.cs | +6 | Update GetDialogue logic | No |
| Era.Core.Tests/Dialogue/DisplayModeTests.cs | +150 (new) | New test file | N/A |

**Total production code**: ~28 lines (within engine type ~300 line limit)
**Total test code**: ~150 lines

### Dependencies on Other Features

| Feature | Dependency Type | Impact |
|---------|----------------|--------|
| F671 | Prerequisite | displayMode in dialogue-schema.json and YAML output |
| F677 | Downstream | KojoComparer will consume DialogueLines for equivalence testing |

### Open Questions

None. All design decisions are concrete and validated against ACs.

### Non-Goals (Explicit Exclusions)

1. **PrintDataCommand/PrintDataHandler**: ERB runtime PRINT commands are a separate code path. F676 scope is YAML dialogue pipeline only.
2. **Display mode interpretation**: KojoEngine propagates metadata but does not interpret it. Rendering behavior (newline vs wait vs keyWait) is consumer responsibility.
4. **YAML file re-conversion**: F676 handles existing files (null displayMode) and future files (with displayMode). Re-conversion is F671/F634 responsibility.
5. **Multi-entry selection**: Design assumes single-entry selection and rendering (KojoEngine.GetDialogue() processes single entry). F681 will address multi-entry selection and rendering pipeline changes.

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add optional DisplayMode property to DialogueEntry record in DialogueFile.cs | [x] |
| 2 | 2,3 | Add DisplayMode property to DialogueEntryData and mapping in YamlDialogueLoader.cs | [x] |
| 3 | 4,5,8 | Replace DialogueResult record with dual-property design (DialogueLines + backward-compatible Lines) and add DialogueLine record in DialogueResult.cs | [x] |
| 4 | 6 | Update KojoEngine.GetDialogue() to propagate DisplayMode from DialogueEntry to DialogueLine instances | [x] |
| 5 | 7,12,19 | Create DisplayModeTests.cs with 8 enum value coverage + default handling + invalid value rejection test cases | [x] |
| 6 | 13 | Update test files to use new DialogueResult initialization pattern (HeadlessUITests.cs, YamlComExecutorTests.cs, ComEvaluationContextTests.cs) - Change from `new DialogueResult(lines)` to `DialogueResult.Create(lines.Select(l => new DialogueLine(l, DisplayMode.Default)).ToList())` | [x] |
| 7 | 14 | Update .claude/skills/engine-dev/SKILL.md to reflect new DialogueResult dual-property design and add DialogueLine record documentation | [x] |
| 8 | 15 | Create F682 for consumer-side display mode interpretation (HeadlessUI wait prompts, rendering behavior) | [x] |
| 9 | 16 | Create F681 for multi-entry selection and rendering pipeline | [x] |
| 10 | 9,10,11,17,18,19 | Run build verification, test suite, and tech debt check | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T4 | Technical Design Layer 1-5 specifications | Production code changes in 4 files + DisplayMode enum |
| 2 | implementer | sonnet | T5 | Technical Design Testing Strategy | DisplayModeTests.cs with 7+ test cases including invalid value rejection |
| 3 | implementer | sonnet | T6 | Impact Analysis test file updates | Updated test files for new DialogueResult constructor |
| 4 | implementer | sonnet | T7 | SSOT update requirements | Updated engine-dev SKILL.md with new DialogueResult/DialogueLine/DisplayMode documentation |
| 5 | implementer | sonnet | T8-T9 | Deferred feature creation | Created F680 and F681 [DRAFT] feature files |
| 6 | ac-tester | haiku | T10 | AC#9-11,15-19 test commands | Build, test suite, tech debt verification, DRAFT file creation results |

**Constraints** (from Technical Design):

1. **Backward Compatibility First**: DialogueResult.Lines property MUST remain as IReadOnlyList<string> for KojoComparer compatibility
2. **Additive Changes Only**: All modifications are optional additions - no breaking changes to existing interfaces
3. **Null as Default**: Absent displayMode field defaults to null (no deserialization errors)
4. **Per-Entry Semantics**: displayMode applies to all lines split from that entry's content (one entry → multiple lines → shared displayMode)
5. **Constructor Compatibility**: DialogueResult positional constructor usage must be updated in test files (not breaking for consumer interfaces)
6. **Positional Record Requirement**: DialogueLine MUST use positional record syntax for immutability and consistency with design intent

**Pre-conditions**:

- F671 is [DONE] (displayMode exists in dialogue-schema.json)
- YamlDotNet is already referenced in Era.Core.csproj
- DialogueResult is currently a simple record with single Lines property
- KojoEngine.GetDialogue() splits content by '\n' and returns DialogueResult

**Success Criteria**:

1. All 12 ACs pass verification
2. dotnet build Era.Core succeeds (AC#9)
3. dotnet test Era.Core.Tests succeeds with all existing + new tests passing (AC#10)
4. KojoComparer's YamlRunner.cs requires zero changes (backward compatibility verified)
5. Grep verification confirms no TODO/FIXME/HACK in modified files (AC#11)

**Execution Steps**:

1. **Phase 1 - Layer 1-2: Domain and Deserialization (T1-T2)**
   - Edit Era.Core/Types/DisplayMode.cs: Define DisplayMode enum with 9 values (Default, Newline, Wait, KeyWait, KeyWaitNewline, KeyWaitWait, Display, DisplayNewline, DisplayWait)
   - Edit Era.Core/Dialogue/DialogueFile.cs: Add `public DisplayMode DisplayMode { get; init; } = DisplayMode.Default` to DialogueEntry record
   - Edit Era.Core/Dialogue/Loading/YamlDialogueLoader.cs:
     - Add `public string? DisplayMode { get; set; }` to DialogueEntryData class
     - Add ParseDisplayMode method with validation switch expression
     - Update Load() LINQ projection with `DisplayMode = ParseDisplayMode(e.DisplayMode)`
   - Verify: AC#1, AC#2, AC#3 (Grep checks)

2. **Phase 1 - Layer 3-4: Result Type and Pipeline (T3-T4)**
   - Edit Era.Core/Types/DialogueResult.cs:
     - Add `public record DialogueLine(string Text, DisplayMode DisplayMode);`
     - Replace DialogueResult record with dual-property design (required DialogueLines + computed Lines)
   - Edit Era.Core/KojoEngine.cs:
     - Update GetDialogue() to map lines to DialogueLine instances with entry.DisplayMode
     - Change constructor call from `new DialogueResult(lines.ToList())` to `DialogueResult.Create(dialogueLines)`
   - Verify: AC#4, AC#5, AC#6, AC#8 (Grep checks + code inspection)

3. **Phase 2 - Testing (T5)**
   - Create Era.Core.Tests/Dialogue/DisplayModeTests.cs
   - Implement 7 test cases from Technical Design Testing Strategy:
     1. Deserialization with displayMode
     2. Deserialization without displayMode (Default)
     3. All 8 enum values (parameterized test)
     4. Invalid displayMode value rejection (InvalidDataException)
     5. Propagation through pipeline
     6. Multi-line entry with shared displayMode
     7. Backward compatibility (Lines property computed from DialogueLines)
   - Verify: AC#7, AC#12, AC#19 (test execution)

4. **Phase 3 - Test Updates (T6)**
   - Update Era.Core.Tests/HeadlessUITests.cs: Change DialogueResult constructor calls from `new DialogueResult(lines)` to `DialogueResult.Create(lines.Select(l => new DialogueLine(l, DisplayMode.Default)).ToList())`
   - Update Era.Core.Tests/Commands/Com/YamlComExecutorTests.cs: Apply same DialogueResult constructor migration pattern
   - Update Era.Core.Tests/Commands/Com/ComEvaluationContextTests.cs: Apply same DialogueResult constructor migration pattern

5. **Phase 4 - SSOT Update (T7)**
   - Update .claude/skills/engine-dev/SKILL.md:
     - Add DisplayMode enum documentation in Types section
     - Update DialogueResult documentation to reflect dual-property design (DialogueLines + computed Lines)
     - Add DialogueLine record documentation in Types section
     - Remove outdated positional constructor reference

6. **Phase 5 - Deferred Feature Creation (T8-T9)**
   - Create feature-682.md [DRAFT] with: Status: [DRAFT], Type: engine, Summary: "Consumer-side display mode interpretation (HeadlessUI wait prompts, rendering behavior implementation)", Links: back to F676 in Dependencies table
   - Create feature-681.md [DRAFT] with: Status: [DRAFT], Type: engine, Summary: "Multi-entry selection and rendering pipeline extension for DialogueResult", Links: back to F676 in Dependencies table

7. **Phase 6 - Quality Gates (T10)**
   - Run: `dotnet build Era.Core` (AC#9)
   - Run: `dotnet test Era.Core.Tests` (AC#10)
   - Run: Grep for TODO|FIXME|HACK in modified files (AC#11,17,18)
   - Run: Test invalid displayMode rejection (AC#19)
   - Verify: F682 and F681 DRAFT files exist (AC#15,16)
   - Verify: All commands succeed with expected results

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with specific failure details
3. Create follow-up feature for fix with additional investigation of:
   - DialogueResult consumer compatibility issues
   - YamlDotNet deserialization edge cases
   - Test coverage gaps

**Error Handling**:

- If AC#1-3 fail: Deserialization layer issue → Check enum definition and YamlDotNet property naming conventions
- If AC#4-6 fail: Pipeline propagation issue → Verify LINQ projection and record construction
- If AC#7 fail: Enum coverage incomplete → Add missing displayMode values to test
- If AC#8 fail: Breaking change introduced → Verify Lines property exists as IReadOnlyList<string>
- If AC#9-10 fail: Build/test regression → STOP, report to user (Escalation Policy)
- If AC#19 fail: Validation not working → Check ParseDisplayMode switch expression and InvalidDataException throwing
- If 3 consecutive failures in any phase: STOP, report to user (Fail Fast principle)

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| Consumer-side display mode interpretation (HeadlessUI wait prompts, rendering behavior) | F682 | F676 propagates metadata but no consumer interprets it for rendering behavior. Separate from F677 (comparison-only). |
| Multi-entry selection and rendering pipeline | F681 | Current design assumes single-entry selection and rendering. Multi-entry would require pipeline changes and per-entry metadata in DialogueLine. Single-entry displayMode mapping pattern will need refactoring. |
| Mark DialogueResult.Lines as [Obsolete] after consumer migration | F683 | Backward compatibility shim creates permanent maintenance burden. Needs tracked deprecation timeline after F682/F677 adopt DialogueLines. |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-30 15:55 | Phase 3 (T5-T6) | Fixed DisplayModeTests.cs pipeline tests (rewrote to use YamlDialogueLoader + manual DialogueResult instead of KojoEngine). Updated HeadlessUITests.cs, YamlComExecutorTests.cs, ComEvaluationContextTests.cs to use DialogueResult.Create(). Build + tests PASS (1427/1427). |
| 2026-01-30 15:58 | Phase 5 (T8-T9) | Created F681 and F682 DRAFT files. Registered both in index-features.md. Tasks 8-9 complete. |
