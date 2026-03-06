# Feature 671: PrintData Variant Metadata Mapping

## Status: [DONE]

## Type: engine

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Created: 2026-01-27

---

## Summary

Map PrintDataNode Variant metadata (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAD, etc.) to display behavior semantics in dialogue-schema.json and YAML rendering.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Extend dialogue-schema.json to capture PrintDataNode display variant semantics in YAML metadata for equivalence testing and future ERB regeneration tools. Scope is metadata preservation, not runtime behavior interpretation.

### Problem (Current Issue)

F634 (Batch Conversion Tool) converts PrintDataNode content to YAML but ignores the Variant field. PrintDataNode has 9 variants:
- PRINTDATA (default)
- PRINTDATAL (newline)
- PRINTDATAW (wait)
- PRINTDATAK (key-wait)
- PRINTDATAKL (key-wait + newline)
- PRINTDATAKW (key-wait + wait)
- PRINTDATAD (display)
- PRINTDATADL (display + newline)
- PRINTDATADW (display + wait)

Each variant affects display behavior (L=newline, W=wait for input, K=key-wait, D=display mode). F634 flattens all variants to identical YAML output, losing display semantics.

### Goal (What to Achieve)

1. Extend dialogue-schema.json to support optional "displayMode" field in branches
2. Map PrintDataNode.Variant to displayMode metadata (e.g., PRINTDATAW → displayMode: "wait")
3. Update FileConverter/PrintDataConverter to preserve Variant metadata in YAML output
4. Preserve displayMode metadata in YAML output for equivalence testing and future ERB regeneration tools (Era.Core renderer integration deferred to F644 or follow-up feature)

---

## Links

- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (Predecessor)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (Predecessor - deferred Variant mapping)
- [feature-675.md](feature-675.md) - YAML Format Unification (Predecessor - schema format change)
- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Downstream - renderer deferred here)
- [feature-651.md](feature-651.md) - KojoComparer KojoEngine API Update (Related)
- [feature-638.md](feature-638.md) - Patchouli Kojo Conversion (Successor)
- [feature-636.md](feature-636.md) - Meiling Kojo Conversion (Consumer)
- [feature-639.md](feature-639.md) - Sakuya Kojo Conversion (Consumer)
- [feature-640.md](feature-640.md) - Remilia Kojo Conversion (Consumer)
- [feature-643.md](feature-643.md) - Generic Kojo Conversion (Consumer)
- [feature-676.md](feature-676.md) - Era.Core renderer DisplayMode integration (Successor - [DRAFT] to be created by Task 9)
- [feature-677.md](feature-677.md) - KojoComparer displayMode awareness (Successor - [DRAFT] to be created by Task 10)
- [feature-349.md](feature-349.md) - Dialogue Schema Foundation
- [feature-361.md](feature-361.md) - YAML Branch Extension
- [dialogue-schema.json](../../tools/YamlSchemaGen/dialogue-schema.json) - Current schema definition

---

## Notes

## Review Notes

- [pending] Phase3-ACValidation iter1: AC#1-4, AC#14-15 pre-implementation validation failures - Schema missing displayMode, converters missing DisplayModeMapper calls, F676/F677 files not exist. Expected for [PROPOSED] feature. Implementation required to pass these ACs.
- [pending] Phase6-FinalRefCheck iter1: F676/F677 reference consistency issues - Links reference non-existent files, AC#14-15 create files but unclear if during F671 (Task 9-10) or separate workflow. Scope boundary clarification needed before FL completion.

- [resolved-applied] Phase1-Uncertain iter1: AC#2 'not_matches' with pattern 'required.*displayMode' depends on JSON formatting - Fixed by changing AC#2 to Read-based verification of JSON structure rather than single-line grep.
- [resolved-acknowledged] Phase2-Maintainability iter4: Critical architecture mismatch discovered - dialogue-schema.json YAML is NOT consumed by KojoEngine (uses different schema). This invalidates AC#7-8 and the Goal statement about Era.Core renderer updates. Feature Philosophy assumes YAML is used for runtime rendering, but investigation shows it's only for KojoComparer equivalence testing. RESOLUTION: F671 scope limited to converter-only (AC#1-6, AC#9-13). AC#7-8 marked [B] and deferred to F644.
- [resolved-acknowledged] Phase2-Maintainability iter4: Philosophy/Goal mismatch - Feature claims YAML rendering preserves ERB behavior but dialogue-schema.json is not consumed by game runtime. Need clarification: Is this for equivalence testing only or preparing for future ERB regeneration tools? RESOLUTION: F671 scope clarified in Technical Design section - metadata preservation for equivalence testing and future ERB regeneration tools. Goal #4 updated to reflect actual scope.
- [resolved-applied] Phase2-Maintainability iter1: CRITICAL stale design - F675 is now [DONE] and changed dialogue-schema.json from branches: to entries: format. F671's entire Technical Design references obsolete branches: format 30+ times. RESOLUTION: Design rewritten to add displayMode to entries: items in schema and propagate through BranchesToEntriesConverter.
- [resolved-applied] Phase2-Maintainability iter1: Converter strategy obsolete - BranchesToEntriesConverter now handles branches→entries conversion. displayMode must be added to branch dictionaries before BranchesToEntriesConverter.Convert() transforms them. RESOLUTION: Updated implementation strategy to add displayMode to branch dict then propagate through BranchesToEntriesConverter.
- [resolved-applied] Phase2-Maintainability iter1: BranchesToEntriesConverter displayMode propagation unclear - Task 4 description incorrectly described FileConverter logic instead of BranchesToEntriesConverter propagation. RESOLUTION: Updated Technical Design Task 4 to clearly specify BranchesToEntriesConverter.Convert() must propagate displayMode from branch dict to entry dict with explicit implementation pattern.
- [resolved-applied] Phase1-Review iter1: F644 status references stale - Feature lists F644 as [DRAFT] in multiple locations but F644 is actually [WIP]. RESOLUTION: Updated all F644 references from [DRAFT] to [WIP] in Dependencies table, Related Features table, Risks table, and comment.
- [resolved-applied] Phase1-Review iter1: F644 status references stale (second update) - Feature lists F644 as [WIP] in multiple locations but F644 is actually [DONE]. RESOLUTION: Updated all F644 references from [WIP] to [DONE]. Updated Risk mitigation to reflect F644 completed without displayMode awareness. Added AC#14 for Task 9 F676 creation verification.
- [resolved-applied] Phase1-Review iter2: Implementation Contract test code API errors - Phase 3 test examples use incorrect PrintDataConverter API (1-param call vs 3-param signature, Dictionary return vs string return, invalid PrintDataNode constructor). RESOLUTION: Updated test examples to use correct 3-parameter Convert() signature, parse returned YAML string for verification, and construct PrintDataNode with property initialization syntax.
- [resolved-applied] Phase1-Review iter3: Test code uses non-existent TextNode and AC#10 matcher issues - Test code references TextNode class (doesn't exist) and AC#10 uses 'matches' matcher which can't verify count. RESOLUTION: Replaced TextNode with DataformNode { Arguments = { "..." } }. Changed AC#10 matcher from 'matches' to 'count_equals' with expected '9'. Fixed Consumers table terminology (branch schema → entry items schema).
- [resolved-applied] Phase1-Review iter4: Implementation Contract inaccuracies and assertion library inconsistency - Code location references slightly inaccurate, AC#5 filter inconsistency, test code uses FluentAssertions while existing tests use xUnit Assert. RESOLUTION: Updated code location references to precise lines (58-64, line 67). Aligned AC#5 Method/Details filters to 'PrintDataL'. Converted all test assertions from FluentAssertions to xUnit Assert syntax.
- [resolved-applied] Phase1-Review iter1: 残課題 Creation Task stale - Creation Task says "When F644 transitions to [REVIEWED]" but F644 is already [WIP] past that state. RESOLUTION: Updated Creation Task to "Contact F644 implementer to add scope" reflecting current F644 status.
- [resolved-applied] Phase1-Review iter1: Implementation Contract Phase headers task number mismatch - Phase 2 header said "Task 1-3" but should be "Task 2-4", Phase 3 header said "Task 4" but should be "Task 5". RESOLUTION: Updated both headers to correct task number ranges.
- [pending] Phase1-Uncertain iter1: Consumers table vs Impact Analysis table redundancy - Reviewer suggests consolidating duplicate entries for PrintDataConverter.cs and FileConverter.cs but tables serve different purposes (reverse dependencies vs change descriptions). Consolidation is optional and debatable.
- [resolved-applied] Phase1-Review iter1: F638 status stale - Dependencies table listed F638 as [PROPOSED] but F638 is actually [DONE]. RESOLUTION: Updated F638 status reference from [PROPOSED] to [DONE].
- [resolved-applied] Phase1-Uncertain iter1: Implementation Contract Phase 2 sub-task numbering - Phase 2 uses internal sub-task numbering (Task 1-4) that differs from main Tasks table numbering (Task 2-4). RESOLUTION: Updated Phase 2 subsections to "Step 1/2/3/4 (Task #)" format to clarify relationship between internal implementation steps and main Tasks table entries.
- [pending] Phase1-Uncertain iter1: AC#10 description clarity - Pattern 'PRINTDATA(L|W|K|KL|KW|D|DL|DW)?' correctly counts 9 variants but reviewer suggests clarifying that we're counting switch case labels. Current description is technically accurate but could be clearer.
- [pending] Phase3-ACValidation iter1: AC#10 pre-implementation validation - tools/ErbToYaml/DisplayModeMapper.cs does not exist yet. AC#10 is valid for post-implementation verification but cannot be pre-validated. AC definition is correct.

- F634 deliberately deferred this to F671 to keep batch conversion focused on content extraction
- Display variant semantics are a separate concern from dialogue content conversion
- This feature requires schema extension (dialogue-schema.json) + converter changes + renderer changes
- 9 variants map to 5 semantic flags: L (newline), W (wait), K (key-wait), D (display), default (none)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: YAML output from F634 batch conversion loses display behavior semantics (all PRINTDATA variants produce identical YAML)
2. Why: PrintDataConverter.Convert() and FileConverter.ProcessConditionalBranch() extract dialogue lines from PrintDataNode but never read the Variant field
3. Why: F634 deliberately scoped Variant handling out to keep the batch conversion focused on content extraction correctness
4. Why: dialogue-schema.json has no field to represent display mode metadata -- only character, situation, entries (with id, content, priority, condition)
5. Why: The original schema (F349/F361) was designed for DATALIST content which has no display variant concept; PRINTDATA variants are a distinct ERB feature that was not part of the original schema design

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| All 9 PRINTDATA variants produce identical YAML output | dialogue-schema.json lacks a displayMode field, and PrintDataConverter/FileConverter have no code path to read or emit PrintDataNode.Variant |
| PRINTDATAL content renders without newline behavior | Era.Core's KojoEngine/PrintDataHandler pipeline has no concept of display variant -- PrintDataCommand only carries SelectedLines, no variant metadata |

### Conclusion

The root cause is a **schema gap**: dialogue-schema.json was designed for DATALIST content which has no display variant concept. When PRINTDATA support was added (F633 parser, F634 converter), the schema was not extended to accommodate the Variant field. The fix requires a three-layer change:
1. **Schema**: Add optional `displayMode` field to entry objects in dialogue-schema.json
2. **Converter**: Read PrintDataNode.Variant in PrintDataConverter and FileConverter, emit displayMode in YAML output
3. **Renderer**: Update Era.Core's PrintDataCommand/PrintDataHandler or KojoEngine to interpret displayMode and emit the appropriate PRINT variant (PRINTL, PRINTW, etc.) (deferred to F676)

**Practical scope consideration**: Codebase analysis reveals that in the 口上 (kojo) ERB files, only **PRINTDATAL** (1,575 occurrences across 30+ files) is actually used among the non-default variants. PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, and PRINTDATADW have **zero occurrences** in the kojo directory. The default PRINTDATA (without suffix) also appears but with the same baseline behavior. This means the practical impact is limited to preserving the "L" (newline) suffix semantics.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F633 | [DONE] | Predecessor (parser) | Added PRINTDATA parsing to ErbParser. PrintDataNode.Variant field is already captured during parsing (line.Split()[0]) |
| F634 | [DONE] | Predecessor (converter) | Batch Conversion Tool. Deliberately deferred Variant handling to F671 (documented in Review Notes and Mandatory Handoffs) |
| F555 | [DONE] | Planning | Phase 19 Planning. Defined kojo conversion scope but did not anticipate Variant metadata needs |
| F636 | [DONE] | Consumer | Meiling Kojo Conversion -- uses F634 output, currently missing Variant metadata |
| F639 | [DONE] | Consumer | Sakuya Kojo Conversion -- most affected (Sakuya files have heavy PRINTDATAL usage) |
| F640 | [DONE] | Consumer | Remilia Kojo Conversion -- affected (WC系口上, NTR口上 have PRINTDATAL) |
| F643 | [DONE] | Consumer | Generic Kojo Conversion -- U_汎用 files have PRINTDATAL in NTR口上 |
| F638 | [DONE] | Consumer | Patchouli Kojo Conversion -- will need re-conversion if F671 changes converter output |
| F644 | [DONE] | Downstream | Equivalence Testing Framework -- must account for displayMode in equivalence comparison |
| F651 | [DONE] | Related | KojoComparer KojoEngine API Update -- KojoEngine currently returns DialogueResult with lines only, no display metadata |

### Pattern Analysis

This is a **schema evolution gap** pattern. The original schema (F349/F361) was designed for DATALIST. When PRINTDATA support was added (F633/F634), the schema was not extended. This is a known pattern in incremental migration projects where each phase reveals new metadata requirements that the initial schema did not anticipate.

The practical impact is mitigated by the fact that only PRINTDATAL is actually used in kojo files. However, implementing all 9 variants ensures forward-compatibility if other ERB files (outside 口上/) are converted later.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | PrintDataNode.Variant is already parsed and stored by ErbParser (F633). Schema extension is straightforward JSON Schema addition. Converter changes are localized to PrintDataConverter and FileConverter |
| Scope is realistic | YES | 3-layer change (schema + converter + renderer) with clear boundaries. ~6-8 files modified. Volume well within engine type ~300 line limit |
| No blocking constraints | YES | F634 is [DONE]. PrintDataNode.Variant already available in AST. dialogue-schema.json is extensible (adding optional field is backward-compatible) |

**Verdict**: FEASIBLE

**Notes on scope refinement**: The Background lists 9 variants but codebase analysis shows only PRINTDATAL is used in kojo files. Implementation should support all 9 variants for correctness but testing priority should focus on PRINTDATAL. The renderer layer (Goal #4) may be deferred if KojoEngine's current pipeline (DialogueResult with lines list) requires significant restructuring to carry display metadata.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool -- PrintDataConverter and FileConverter exist, Variant handling deferred to F671 |
| Predecessor | F633 | [DONE] | PRINTDATA Parser Extension -- PrintDataNode.Variant field is parsed and populated |
| Predecessor | F675 | [DONE] | YAML Format Unification -- replaced branches: format with entries: format; displayMode must be added to entries: schema instead of branches: |
| Related | F644 | [DONE] | Equivalence Testing Framework -- must account for displayMode field in equivalence comparison |
| Related | F651 | [DONE] | KojoComparer KojoEngine API Update -- KojoEngine API may need extension for display metadata |
| Successor | F638 | [DONE] | Patchouli Kojo Conversion -- will benefit from Variant-aware conversion |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet 16.2.1 | Runtime | Low | Already referenced in ErbToYaml.csproj. Serialization of new displayMode field is standard |
| NJsonSchema 11.1.0 | Runtime | Low | Already referenced. Schema validation will automatically validate new displayMode field once schema is updated |
| dialogue-schema.json | Design-time | Medium | Schema extension must be backward-compatible (new field must be optional). Existing YAML files without displayMode must remain valid |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/ErbToYaml/PrintDataConverter.cs | HIGH | Must emit displayMode based on PrintDataNode.Variant |
| tools/ErbToYaml/FileConverter.cs | HIGH | ProcessConditionalBranch must propagate Variant from PrintDataNode to YAML branch |
| tools/YamlSchemaGen/dialogue-schema.json | HIGH | Must add optional displayMode property to entry items schema |
| Era.Core/KojoEngine.cs | MEDIUM | Currently returns DialogueResult(lines). May need to carry display metadata |
| Era.Core/Commands/Print/PrintCommand.cs | MEDIUM | PrintDataCommand only has SelectedLines. May need variant info |
| Era.Core/Commands/Print/PrintHandler.cs | MEDIUM | PrintDataHandler calls _console.PrintData(). May need variant-specific dispatch |
| Era.Core/Interfaces/IConsoleOutput.cs | LOW | Already has Print, PrintLine, PrintWait methods for different display modes |
| tools/KojoComparer/ | LOW | Uses KojoEngine to compare ERB vs YAML output. DisplayMode differences may affect comparison |
| tools/ErbToYaml.Tests/PrintDataConverterTests.cs | HIGH | Must add tests for Variant-aware conversion |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/YamlSchemaGen/dialogue-schema.json | Update | Add optional "displayMode" property to branch object schema |
| tools/ErbToYaml/PrintDataConverter.cs | Update | Read PrintDataNode.Variant, include displayMode in YAML output |
| tools/ErbToYaml/FileConverter.cs | Update | Propagate PrintDataNode.Variant through ProcessConditionalBranch to YAML branches |
| tools/ErbToYaml/IPrintDataConverter.cs | No Change | Interface unchanged; Variant is already accessible via PrintDataNode parameter |
| tools/ErbToYaml.Tests/PrintDataConverterTests.cs | Update | Add tests for all Variant mappings (PRINTDATAL -> displayMode: "newline", etc.) |
| Era.Core/Dialogue/Loading/ | Possible Update | YAML loader may need to deserialize displayMode field from YAML |
| Era.Core/KojoEngine.cs | Possible Update | DialogueResult may need display metadata |
| Era.Core/Commands/Print/PrintCommand.cs | Possible Update | PrintDataCommand may need DisplayMode property |
| Era.Core/Commands/Print/PrintHandler.cs | Possible Update | May need variant-specific dispatch to PrintLine/PrintWait |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| dialogue-schema.json must remain backward-compatible | Existing YAML files (F636/F639/F640/F643 outputs) | HIGH - displayMode field must be optional; existing files without it must still validate |
| PrintDataNode.Variant is a raw string (e.g., "PRINTDATAL") | ErbParser.Ast.PrintDataNode | LOW - Simple string-to-enum mapping needed |
| Era.Core PrintDataCommand only carries SelectedLines | Era.Core/Commands/Print/PrintCommand.cs | MEDIUM - Adding DisplayMode to command record requires updating handler and command dispatcher |
| KojoEngine returns DialogueResult with lines only | Era.Core/KojoEngine.cs (line 88) | MEDIUM - Carrying display metadata through the pipeline requires DialogueResult extension |
| IConsoleOutput already has separate Print/PrintLine/PrintWait methods | Era.Core/Interfaces/IConsoleOutput.cs | LOW - Favorable constraint: display mode dispatch can map to existing methods |
| Only PRINTDATAL is used in kojo files (1,575 occurrences) | Grep analysis of Game/ERB/口上/ | LOW - Other variants have 0 occurrences but should be implemented for correctness |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Schema change breaks existing YAML validation | Low | High | Make displayMode optional in schema. Run validation on all existing YAML files after schema update |
| Re-conversion needed for already-converted files | Medium | Medium | F636/F639/F640/F643 YAML outputs lack displayMode. Document re-conversion procedure. May need batch re-run |
| KojoEngine pipeline restructuring too large for F671 scope | Medium | Medium | Scope F671 to schema + converter only. Defer renderer changes to a follow-up feature if needed |
| DisplayMode semantics differ between PRINTDATA and inline PRINT variants | Low | Low | PRINTDATAL maps to "newline" = same as PRINTL suffix. Semantics are consistent |
| Equivalence testing (F644) must account for displayMode | Low | Medium | F644 is [DONE] and was completed without displayMode awareness. Will require follow-up patch to support displayMode comparison |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Implementation Overview

This feature extends the dialogue YAML schema and conversion pipeline to preserve PrintDataNode display variant semantics. The implementation spans three layers: schema definition, converter, and renderer.

**Key Insight from Investigation**: Only PRINTDATAL (1,575 occurrences) is actually used in kojo files. The other 8 variants have zero occurrences. However, we will implement all 9 variants for forward-compatibility and correctness.

**Critical Design Decision**: The renderer layer (AC#7-AC#8) requires careful scoping. Era.Core's current architecture has KojoEngine return DialogueResult with only a `List<string>` of lines (no display metadata). Extending this to carry displayMode would require propagating metadata through multiple layers (DialogueEntry → DialogueResult → command creation). Given that the practical use case is limited to PRINTDATAL, and the feature scope is already substantial at the schema + converter level, **we will defer renderer implementation to a follow-up feature** if the converter-only solution proves insufficient for YAML→ERB equivalence testing.

### Architecture Changes

#### Layer 1: Schema Extension (dialogue-schema.json)

**File**: `tools/YamlSchemaGen/dialogue-schema.json`

**Change**: Add optional `displayMode` property to entry items schema.

**Location**: Inside `entries` array items properties, alongside `id`, `content`, `priority`, and `condition`.

**Schema addition**:
```json
"displayMode": {
  "type": "string",
  "description": "Display variant mode (PRINTDATA suffix semantics)",
  "enum": [
    "newline",
    "wait",
    "keyWait",
    "keyWaitNewline",
    "keyWaitWait",
    "display",
    "displayNewline",
    "displayWait"
  ]
}
```

**Note**: The `default` variant (PRINTDATA with no suffix) is represented by **absence of the displayMode field** (not `displayMode: "default"`). This keeps existing YAML files valid without requiring updates.

**Required array**: `displayMode` MUST NOT be added to the `required` array. Only `id` and `content` remain required. This ensures backward compatibility with existing F636/F639/F640/F643 YAML files.

#### Layer 2: Converter Changes

**2.1 PrintDataConverter.cs**

**File**: `tools/ErbToYaml/PrintDataConverter.cs`

**Current behavior**: `Convert()` method builds a dictionary with `character`, `situation`, and `branches` (containing a single branch with `lines`). The PrintDataNode.Variant field is never read.

**New behavior**:
1. Use the shared static utility method `DisplayModeMapper.MapVariant(string variant)` that maps PRINTDATA variant strings to displayMode values:
   - "PRINTDATA" → null (no displayMode field)
   - "PRINTDATAL" → "newline"
   - "PRINTDATAW" → "wait"
   - "PRINTDATAK" → "keyWait"
   - "PRINTDATAKL" → "keyWaitNewline"
   - "PRINTDATAKW" → "keyWaitWait"
   - "PRINTDATAD" → "display"
   - "PRINTDATADL" → "displayNewline"
   - "PRINTDATADW" → "displayWait"

2. In `Convert()` method (around line 63-67 where the branch dictionary is built), call `DisplayModeMapper.MapVariant(printData.Variant)` and conditionally add displayMode to the branch dictionary if result is non-null. The branch dictionary is then passed to BranchesToEntriesConverter.Convert() which must be updated to propagate displayMode to the entry dictionary.

**Code location**: Lines 58-64 (branches list construction) and line 67 (BranchesToEntriesConverter.Convert call).

**Pattern**:
```csharp
var branch = new Dictionary<string, object>
{
    { "lines", lines }
};

var displayMode = DisplayModeMapper.MapVariant(printData.Variant);
if (displayMode != null)
{
    branch["displayMode"] = displayMode;
}
```

**2.2 FileConverter.cs**

**File**: `tools/ErbToYaml/FileConverter.cs`

**Current behavior**: `ProcessConditionalBranch()` (lines 263-327) extracts lines from PrintDataNode/DatalistNode and builds a branch dictionary. The PrintDataNode.Variant field is never read.

**New behavior**:
1. Use the shared static utility method `DisplayModeMapper.MapVariant(string variant)` (same mapping as PrintDataConverter).

2. In `ProcessConditionalBranch()`, when processing a PrintDataNode (lines 274-288), capture the Variant and add displayMode to the branch dictionary if non-null.

**Code location**: Lines 311-326 (branch dictionary construction).

**Challenge**: The current code iterates over `body` nodes without tracking which PrintDataNode a line came from. If a branch body contains multiple PrintDataNode instances with different Variants, we need to determine which Variant to use.

**Resolution strategy**:
- **Assumption**: A single conditional branch should not contain multiple PrintDataNode instances with different Variants (this would be unusual ERB authoring).
- **Implementation**: Track the first non-default Variant encountered in the branch body. If multiple different Variants are found, use the first encountered (and log a warning comment in code).
- **Pre-implementation validation required**: Before implementing Task 3, grep kojo ERB files for IF blocks containing multiple different PRINTDATA variants. Document the result in Review Notes. If violations found, change strategy to per-line displayMode (which would require entries-level rather than branch-level displayMode). If no violations found, document the grep evidence to validate the lossy design assumption.

**Pattern**:
```csharp
string? variantToUse = null;
foreach (var node in body)
{
    if (node is PrintDataNode printData)
    {
        if (variantToUse == null && printData.Variant != "PRINTDATA")
        {
            variantToUse = printData.Variant;
        }
        // Extract lines as before...
    }
}

var branch = new Dictionary<string, object> { { "lines", lines } };

if (variantToUse != null)
{
    var displayMode = DisplayModeMapper.MapVariant(variantToUse);
    if (displayMode != null)
    {
        branch["displayMode"] = displayMode;
    }
}
```

**2.3 IPrintDataConverter.cs**

**File**: `tools/ErbToYaml/IPrintDataConverter.cs`

**Change**: No interface signature change needed. The `Convert()` method already receives `PrintDataNode printData`, which contains the Variant field. The change is internal to the implementation.

#### Layer 3: Renderer Changes (DEFERRED)

**Files affected**:
- `Era.Core/Commands/Print/PrintCommand.cs` (AC#7)
- `Era.Core/Commands/Print/PrintHandler.cs` (AC#8)
- `Era.Core/KojoEngine.cs` (line 88 - DialogueResult creation)
- `Era.Core/Dialogue/DialogueFile.cs` (DialogueEntry record)
- `Era.Core/Dialogue/Loading/YamlDialogueLoader.cs` (deserialization)

**Current architecture**:
1. KojoEngine loads YAML file → deserializes to DialogueFile (list of DialogueEntry with Id, Content, Priority, Condition)
2. DialogueSelector selects matching DialogueEntry
3. DialogueRenderer renders Content string (template variable substitution)
4. KojoEngine splits rendered string into lines and returns DialogueResult(lines)

**Problem**: DialogueEntry only has `Content: string`. There is no field to carry displayMode metadata. The YAML schema structure (character + situation + branches with condition + lines + displayMode) does not map directly to this architecture, which expects a flat list of entries.

**Analysis**: The F634 converter produces YAML with the schema's branch structure (for TALENT-based conditional dialogue), but Era.Core's YamlDialogueLoader expects a different schema (flat entries list with conditions). These are **two different YAML schemas**:
- **dialogue-schema.json** (F634 output): Used for PRINTDATA/DATALIST conversion from ERB kojo files
- **Dialogue system schema** (Era.Core): Used for new simplified dialogue format (mentioned in YamlDialogueLoader.cs line 43-44)

**Conclusion**: F634's YAML output (using dialogue-schema.json) is **not consumed by Era.Core's KojoEngine**. The KojoEngine uses a different YAML format. Therefore, **AC#7 and AC#8 (renderer layer) are not applicable to this feature's scope**.

**Revised scope**: F671 focuses on **preserving Variant metadata in dialogue-schema.json YAML output for equivalence testing and future ERB regeneration**. The YAML files are consumed by KojoComparer (equivalence testing tool), not by the game runtime.

**Implication for AC#7-AC#8**: These ACs should be marked as **out of scope** or converted to verification ACs that check the YAML output structure rather than runtime behavior. The actual renderer integration would be a separate feature (F644 or later).

### Implementation Plan

#### Phase 1: Schema Extension
1. Edit `tools/YamlSchemaGen/dialogue-schema.json`
2. Add `displayMode` property to branch items schema (as specified above)
3. Verify schema is valid JSON

#### Phase 2: Converter Updates
1. Create `tools/ErbToYaml/DisplayModeMapper.cs`:
   - Implement `MapVariant()` static utility method
2. Update `tools/ErbToYaml/PrintDataConverter.cs`:
   - Use `DisplayModeMapper.MapVariant()`
   - Modify `Convert()` to emit displayMode
3. Update `tools/ErbToYaml/FileConverter.cs`:
   - Use `DisplayModeMapper.MapVariant()`
   - Modify `ProcessConditionalBranch()` to emit displayMode

#### Phase 3: Test Implementation
1. Add test cases to `tools/ErbToYaml.Tests/PrintDataConverterTests.cs`:
   - Test PRINTDATAL → displayMode: "newline" (AC#5)
   - Test all 9 variant mappings (AC#6)
   - Test backward compatibility (default PRINTDATA produces no displayMode field) (AC#9)
2. Run tests and verify coverage

#### Phase 4: Schema Validation
1. Verify existing YAML files (F636/F639/F640/F643 outputs) still validate against updated schema
2. Run YamlValidator CLI on sample files

### Variant Mapping Reference

| ERB Variant | Suffix Semantics | displayMode Value |
|-------------|------------------|-------------------|
| PRINTDATA | (none) | (omitted) |
| PRINTDATAL | L = newline | "newline" |
| PRINTDATAW | W = wait | "wait" |
| PRINTDATAK | K = key-wait | "keyWait" |
| PRINTDATAKL | K + L | "keyWaitNewline" |
| PRINTDATAKW | K + W | "keyWaitWait" |
| PRINTDATAD | D = display mode | "display" |
| PRINTDATADL | D + L | "displayNewline" |
| PRINTDATADW | D + W | "displayWait" |

### Testing Strategy

**Unit tests** (tools/ErbToYaml.Tests/PrintDataConverterTests.cs):
- Test fixture for all 9 variants
- Verify YAML output contains correct displayMode value
- Verify YAML validates against updated schema
- Verify backward compatibility (PRINTDATA with no suffix)

**Integration tests** (via existing F634 test infrastructure):
- Run batch conversion on kojo files containing PRINTDATAL
- Verify YAML output includes displayMode: "newline"
- Verify existing YAML files still validate

**AC verification** (via AC tester):
- AC#1-4: Grep-based code verification
- AC#5-6, AC#9, AC#13: Unit test execution
- AC#10: Grep pattern count verification
- AC#11: Technical debt check
- AC#12: Build verification

### File Change Summary

| File | Lines Changed (Est.) | Change Type |
|------|---------------------|-------------|
| dialogue-schema.json | +10 | Schema extension |
| PrintDataConverter.cs | +25 | New method + emit logic |
| FileConverter.cs | +30 | New method + emit logic |
| PrintDataConverterTests.cs | +150 | New test cases |
| PrintCommand.cs | 0 (deferred) | - |
| PrintHandler.cs | 0 (deferred) | - |

**Total estimated changes**: ~215 lines (well within engine type scope limits)

### Dependencies

**Build-time dependencies**:
- YamlDotNet 16.2.1 (already referenced in ErbToYaml.csproj)
- NJsonSchema 11.1.0 (already referenced for schema validation)
- ErbParser (provides PrintDataNode.Variant field)

**Test-time dependencies**:
- xUnit (existing test framework)

### Risks and Mitigations

**Risk 1**: Multiple PrintDataNode instances in a single conditional branch with different Variants
- **Mitigation**: Implement "first non-default Variant wins" strategy. Add validation grep to check if this pattern exists in kojo files.

**Risk 2**: Existing YAML files from F636/F639/F640/F643 become invalid after schema update
- **Mitigation**: Make displayMode optional (not required). Test backward compatibility explicitly (AC#9).

**Risk 3**: AC#7-AC#8 (renderer layer) may be infeasible due to architecture mismatch
- **Mitigation**: Defer renderer implementation. Focus F671 on converter layer. Document renderer needs in a follow-up feature.

### Success Criteria

1. All 13 ACs pass (with AC#7-AC#8 potentially rescoped as deferred)
2. PRINTDATAL conversion produces YAML with `displayMode: "newline"`
3. Existing YAML files still validate against updated schema
4. No technical debt (TODO/FIXME/HACK) in modified files
5. All unit tests pass
6. Build succeeds

### Follow-up Work

**If renderer layer is needed** (depends on equivalence testing requirements in F644):
- Create new feature to extend PrintDataCommand with DisplayMode property
- Update PrintDataHandler to dispatch based on DisplayMode
- Extend DialogueEntry/DialogueResult to carry display metadata
- Update YamlDialogueLoader to deserialize displayMode field

**For now**: F671 focuses on **preserving displayMode metadata in YAML output**. Consumption of that metadata is deferred to future work if needed.

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Schema has displayMode property | code | Grep(tools/YamlSchemaGen/dialogue-schema.json) | contains | "displayMode" | [x] |
| 2 | Schema displayMode is optional (not in required) | code | Read(tools/YamlSchemaGen/dialogue-schema.json) | not_contains | "displayMode" in entries required array | [x] |
| 3 | PrintDataConverter emits displayMode | code | Grep(tools/ErbToYaml/PrintDataConverter.cs) | contains | "DisplayModeMapper.MapVariant" | [x] |
| 4 | FileConverter propagates displayMode from PrintDataNode | code | Grep(tools/ErbToYaml/FileConverter.cs) | contains | "DisplayModeMapper.MapVariant" | [x] |
| 5 | PRINTDATAL produces displayMode newline in YAML | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PrintDataL | succeeds | - | [x] |
| 6 | All 9 variant mappings implemented | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~AllVariantsMapping | succeeds | - | [x] |
| 7 | Era.Core renderer interprets displayMode | deferred | - | - | Deferred to F676 (renderer layer) | [B] |
| 8 | Print dispatching based on displayMode | deferred | - | - | Deferred to F676 (renderer layer) | [B] |
| 9 | Backward compatibility: existing schema validation | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~BackwardCompatibility | succeeds | - | [x] |
| 10 | Variant-to-displayMode mapping covers all 9 variants | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~AllVariantsMapping | succeeds | - | [x] |
| 11 | Zero technical debt | code | Grep(tools/ErbToYaml/DisplayModeMapper.cs,tools/ErbToYaml/PrintDataConverter.cs,tools/ErbToYaml/FileConverter.cs,tools/ErbToYaml.Tests/PrintDataConverterTests.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 12 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 13 | All unit tests pass | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 14 | F676 feature file created for renderer integration | code | exists | Game/agents/feature-676.md | - | [x] |
| 15 | F677 feature file created for KojoComparer integration | code | exists | Game/agents/feature-677.md | - | [x] |

### AC Details

**AC#1: Schema has displayMode property**
- Test: Grep pattern="displayMode" path="tools/YamlSchemaGen/dialogue-schema.json"
- Expected: displayMode property exists as a string enum field within entry object schema
- The displayMode field captures PRINTDATA variant semantics: "newline", "wait", "keyWait", "keyWaitNewline", "keyWaitWait", "display", "displayNewline", "displayWait"
- Must be added inside the entry items/properties alongside existing "id", "content", "priority", and "condition"

**AC#2: Schema displayMode is optional**
- Test: Read tools/YamlSchemaGen/dialogue-schema.json and verify entry-level required array does not contain "displayMode"
- Expected: The entry schema required array contains only ["id", "content"], not ["id", "content", "displayMode"]
- Rationale: Existing YAML files from F636/F639/F640/F643 have no displayMode field. Making it required would break validation of all existing files.

**AC#3: PrintDataConverter emits displayMode**
- Test: Grep pattern="DisplayModeMapper.MapVariant" path="tools/ErbToYaml/PrintDataConverter.cs" type=cs
- Expected: PrintDataConverter.Convert() uses DisplayModeMapper.MapVariant() to convert PrintDataNode.Variant to displayMode value in YAML output
- The mapping function converts raw variant strings (e.g., "PRINTDATAL") to semantic display mode values (e.g., "newline")

**AC#4: FileConverter propagates displayMode**
- Test: Grep pattern="DisplayModeMapper.MapVariant" path="tools/ErbToYaml/FileConverter.cs" type=cs
- Expected: FileConverter uses DisplayModeMapper.MapVariant() to convert PrintDataNode.Variant and emit displayMode in YAML branch dictionary
- Both code paths (simple PrintDataNode and IF-wrapped PrintDataNode) must call this mapping function

**AC#5: PRINTDATAL produces displayMode newline in YAML**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~PrintDataL
- Expected: Test creates PrintDataNode with Variant="PRINTDATAL", converts to YAML, and verifies output contains displayMode: newline
- This is the primary practical case (1,575 occurrences in kojo files)

**AC#6: All 9 variant mappings implemented**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~VariantMapping
- Expected: Dedicated test method(s) verify all 9 PRINTDATA variant strings map to correct displayMode values:
  - PRINTDATA → (no displayMode or "default")
  - PRINTDATAL → "newline"
  - PRINTDATAW → "wait"
  - PRINTDATAK → "keyWait"
  - PRINTDATAKL → "keyWaitNewline"
  - PRINTDATAKW → "keyWaitWait"
  - PRINTDATAD → "display"
  - PRINTDATADL → "displayNewline"
  - PRINTDATADW → "displayWait"


**AC#9: Backward compatibility test**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~BackwardCompatibility
- Expected: Test verifies that YAML output without displayMode field (i.e., from default PRINTDATA variant) still passes schema validation
- Ensures existing F636/F639/F640/F643 YAML files remain valid

**AC#10: Variant-to-displayMode mapping covers all 9 variants**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~AllVariantsMapping
- Expected: Unit test that calls DisplayModeMapper.MapVariant() for all 9 variants and asserts correct output
- This tests the DisplayModeMapper utility class directly (unit test of mapper logic) while AC#6 tests end-to-end converter output (integration test)

**AC#11: Zero technical debt**
- Test: Grep pattern="TODO|FIXME|HACK" paths=[tools/ErbToYaml/DisplayModeMapper.cs, tools/ErbToYaml/PrintDataConverter.cs, tools/ErbToYaml/FileConverter.cs, tools/ErbToYaml.Tests/PrintDataConverterTests.cs]
- Expected: 0 matches across all modified files

**AC#12: Build succeeds**
- Test: dotnet build (solution-level)
- Expected: Exit code 0, no build errors

**AC#13: All unit tests pass**
- Test: dotnet test tools/ErbToYaml.Tests
- Expected: All existing F634 tests continue to pass alongside new F671 tests
- Validates that displayMode changes don't break existing converter functionality

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Extend dialogue-schema.json entries items properties with optional displayMode field | [x] |
| 2 | 3 | Create DisplayModeMapper.cs with MapVariant helper and update PrintDataConverter to emit displayMode | [x] |
| 3 | 4 | Update FileConverter to use DisplayModeMapper.MapVariant and propagate displayMode in ProcessConditionalBranch | [x] |
| 4 | 3,4 | Update BranchesToEntriesConverter.Convert() to propagate displayMode from branch dict to entry dict | [x] |
| 5 | 5,6,9,13 | Create unit tests for PRINTDATAL conversion and all 9 variant mappings | [x] |
| 6 | 10 | Verify variant mapping completeness (all 9 variants covered) | [x] |
| 7 | 11 | Verify zero technical debt in modified converter files | [x] |
| 8 | 12 | Verify solution builds successfully | [x] |
| 9 | 14 | Create F676: Era.Core renderer DisplayMode integration (dependent on F671 completion) | [x] |
| 10 | 15 | Create F677: KojoComparer displayMode awareness (dependent on F671 completion) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

**Note on AC#7-8 (Renderer Layer)**: These ACs verify Era.Core PrintCommand/PrintHandler changes. However, Technical Design investigation revealed that dialogue-schema.json YAML files are NOT consumed by Era.Core's KojoEngine (which uses a different YAML schema). The F634 YAML output is consumed by KojoComparer for equivalence testing, not by the game runtime. Therefore, **AC#7-8 are OUT OF SCOPE** for F671. The renderer integration (if needed for equivalence testing) will be handled in a follow-up feature (F644 or later).

**AC:Task Coverage Summary**:
- Task 1 verifies schema extension (AC#1: displayMode property exists, AC#2: optional not required)
- Task 2 verifies PrintDataConverter implementation (AC#3: emits displayMode based on Variant)
- Task 3 verifies FileConverter implementation (AC#4: propagates displayMode)
- Task 4 verifies BranchesToEntriesConverter implementation (AC#3,4: propagates displayMode from branch to entry)
- Task 5 verifies comprehensive unit test coverage (AC#5: PRINTDATAL case, AC#6: all 9 variants, AC#9: backward compatibility, AC#13: all tests pass)
- Task 6 verifies mapping completeness via code inspection (AC#10: count of variant mappings)
- Task 7 verifies technical debt absence (AC#11: no TODO/FIXME/HACK)
- Task 8 verifies build success (AC#12: dotnet build succeeds)

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Scope Clarification

**Critical Architecture Finding**: F634's YAML output (using dialogue-schema.json) is **not consumed by Era.Core's KojoEngine**. The KojoEngine uses a different YAML format (flat entries list). F634 YAML files are consumed by:
1. **KojoComparer** (equivalence testing tool) - compares ERB vs YAML dialogue output
2. **Future ERB regeneration tools** (not yet implemented)

Therefore, **F671 scope is limited to preserving displayMode metadata in YAML output**. Renderer integration (AC#7-8) is out of scope.

### Implementation Phases

| Phase | Agent | Model | Input | Output | AC# |
|-------|-------|-------|-------|--------|-----|
| 1 | implementer | sonnet | Task 1: Schema extension | Updated dialogue-schema.json | 1,2 |
| 2 | implementer | sonnet | Task 2-4: Converter updates | PrintDataConverter.cs, FileConverter.cs, BranchesToEntriesConverter.cs with displayMode emission | 3,4 |
| 3 | implementer | sonnet | Task 5: Unit tests | PrintDataConverterTests.cs with 9-variant test coverage | 5,6,9,13 |
| 4 | ac-tester | haiku | Verify all ACs (excluding AC#7-8 which are out of scope) | AC verification report | All (except 7,8) |

### Pre-conditions

1. F633 ([DONE]) - PrintDataNode.Variant field is already parsed and available in AST
2. F634 ([DONE]) - PrintDataConverter and FileConverter exist with content extraction logic
3. dialogue-schema.json exists at `tools/YamlSchemaGen/dialogue-schema.json`
4. ErbToYaml.Tests project exists for unit test additions
5. F676 does not yet exist; Task 9 will create it during implementation

### Phase 1: Schema Extension (Task 1)

**File**: `tools/YamlSchemaGen/dialogue-schema.json`

**Action**: Add optional `displayMode` property to entry items schema.

**Location**: Inside the `entries` array items properties object, alongside existing `id`, `content`, `priority`, and `condition` properties.

**Exact addition**:
```json
"displayMode": {
  "type": "string",
  "description": "Display variant mode (PRINTDATA suffix semantics)",
  "enum": [
    "newline",
    "wait",
    "keyWait",
    "keyWaitNewline",
    "keyWaitWait",
    "display",
    "displayNewline",
    "displayWait"
  ]
}
```

**Critical requirements**:
- displayMode MUST be added to the `properties` object (not `required` array)
- Only `id` and `content` remain in the `required` array
- This ensures backward compatibility with existing F636/F639/F640/F643 YAML files

**Verification**: AC#1 (field exists), AC#2 (not required)

### Phase 2: Converter Updates (Task 2-4)

**CRITICAL ORDERING**: Task 4 (BranchesToEntriesConverter propagation) MUST be implemented before or simultaneously with Tasks 2-3 to prevent silent data loss. If Tasks 2-3 add displayMode to branch dict but Task 4 is not implemented, displayMode will be silently dropped at the BranchesToEntriesConverter boundary.

#### Step 1: DisplayModeMapper.cs (Task 2)

**Action**: Create shared utility class with static mapping method:

```csharp
namespace ErbToYaml
{
    public static class DisplayModeMapper
    {
        public static string? MapVariant(string variant)
        {
            return variant switch
            {
                "PRINTDATA" => null,  // Default variant - no displayMode field
                "PRINTDATAL" => "newline",
                "PRINTDATAW" => "wait",
                "PRINTDATAK" => "keyWait",
                "PRINTDATAKL" => "keyWaitNewline",
                "PRINTDATAKW" => "keyWaitWait",
                "PRINTDATAD" => "display",
                "PRINTDATADL" => "displayNewline",
                "PRINTDATADW" => "displayWait",
                _ => null  // Unknown variant - treat as default
            };
        }
    }
}
```

#### Step 2: PrintDataConverter.cs (Task 2)

**File**: `tools/ErbToYaml/PrintDataConverter.cs`

**Action**: Update `Convert()` method (lines 58-64 for branch dict, line 67 for Convert call) to emit displayMode:

**Current code pattern** (inner dictionary at lines 60-63):
```csharp
var branch = new Dictionary<string, object>
{
    { "lines", lines }
};
```

**New code pattern**:
```csharp
var branch = new Dictionary<string, object>
{
    { "lines", lines }
};

var displayMode = DisplayModeMapper.MapVariant(printData.Variant);
if (displayMode != null)
{
    branch["displayMode"] = displayMode;
    // NOTE: displayMode propagation requires Task 4 (BranchesToEntriesConverter update) to be complete
}
```

**Verification**: AC#3 (emits displayMode based on Variant)

#### Step 3: FileConverter.cs (Task 3)

**File**: `tools/ErbToYaml/FileConverter.cs`

**Action 1**: Use shared `DisplayModeMapper.MapVariant()` utility method.

**Action 2**: Update `ProcessConditionalBranch()` method (lines 263-327) to propagate displayMode.

**Implementation pattern** (based on Technical Design "first non-default Variant wins" strategy):
```csharp
string? variantToUse = null;
foreach (var node in body)
{
    if (node is PrintDataNode printData)
    {
        if (variantToUse == null && printData.Variant != "PRINTDATA")
        {
            variantToUse = printData.Variant;
        }
        // Extract lines as before...
    }
}

var branch = new Dictionary<string, object> { { "lines", lines } };

if (variantToUse != null)
{
    var displayMode = DisplayModeMapper.MapVariant(variantToUse);
    if (displayMode != null)
    {
        branch["displayMode"] = displayMode;
    }
}
```

#### Step 4: BranchesToEntriesConverter.cs (Task 4)

**File**: `tools/ErbToYaml/BranchesToEntriesConverter.cs`

**Action**: Update `Convert()` method to propagate `displayMode` from branch dictionary to entry dictionary.

**Context**: BranchesToEntriesConverter.Convert() receives branch dictionaries that may contain a `displayMode` field (added by PrintDataConverter and FileConverter). This field must be propagated to the resulting entry dictionaries.

**Current behavior**: BranchesToEntriesConverter.Convert() (lines 18-61) only propagates `lines`, `condition`, and generates `id`, `content`, `priority`. The `displayMode` field is not propagated.

**Implementation pattern** (modify Convert() method after line 55 - after condition propagation block):

```csharp
// In the loop that creates entry dictionaries from branch data:
var entry = new Dictionary<string, object>
{
    {"id", entryId},
    {"content", content},
    {"priority", priority}
};

// Existing condition propagation
if (branch.ContainsKey("condition"))
{
    entry["condition"] = branch["condition"];
}

// NEW: displayMode propagation
if (branch.ContainsKey("displayMode"))
{
    entry["displayMode"] = branch["displayMode"];
}
```

**Verification**: AC#3, AC#4 (displayMode flows from PrintDataConverter/FileConverter → BranchesToEntriesConverter → YAML output)

### Phase 3: Unit Tests (Task 5)

**File**: `tools/ErbToYaml.Tests/PrintDataConverterTests.cs`

**Action**: Add comprehensive test methods for variant mapping.

**Test method structure**:

```csharp
[Fact]
public void Convert_PrintDataL_ProducesDisplayModeNewline()
{
    // Arrange
    var printData = new PrintDataNode
    {
        Variant = "PRINTDATAL"
    };
    var dataform = new DataformNode();
    dataform.Arguments.Add("Test line");
    printData.Content.Add(dataform);

    var converter = new PrintDataConverter();

    // Act
    var yamlResult = converter.Convert(printData, "@TestChara", "@TestSituation");

    // Parse YAML to verify displayMode
    var deserializer = new YamlDotNet.Serialization.Deserializer();
    var result = deserializer.Deserialize<Dictionary<string, object>>(yamlResult);

    // Assert
    Assert.True(result.ContainsKey("entries"));
    var entries = result["entries"] as List<object>;
    Assert.Single(entries);
    var entry = entries[0] as Dictionary<object, object>;
    Assert.True(entry.ContainsKey("displayMode"));
    Assert.Equal("newline", entry["displayMode"]);
}

[Theory]
[InlineData("PRINTDATA", null)]  // Default - no displayMode
[InlineData("PRINTDATAL", "newline")]
[InlineData("PRINTDATAW", "wait")]
[InlineData("PRINTDATAK", "keyWait")]
[InlineData("PRINTDATAKL", "keyWaitNewline")]
[InlineData("PRINTDATAKW", "keyWaitWait")]
[InlineData("PRINTDATAD", "display")]
[InlineData("PRINTDATADL", "displayNewline")]
[InlineData("PRINTDATADW", "displayWait")]
public void Convert_AllVariantsMapping_ProduceCorrectDisplayMode(string variant, string? expectedDisplayMode)
{
    // Arrange
    var printData = new PrintDataNode
    {
        Variant = variant,
        Content = { new DataformNode { Arguments = { "Test line" } } }
    };

    var converter = new PrintDataConverter();

    // Act
    var yamlResult = converter.Convert(printData, "@TestChara", "@TestSituation");

    // Parse YAML to verify displayMode
    var deserializer = new YamlDotNet.Serialization.Deserializer();
    var result = deserializer.Deserialize<Dictionary<string, object>>(yamlResult);

    // Assert
    var entries = result["entries"] as List<object>;
    var entry = entries[0] as Dictionary<object, object>;

    if (expectedDisplayMode == null)
    {
        Assert.False(entry.ContainsKey("displayMode"));
    }
    else
    {
        Assert.True(entry.ContainsKey("displayMode"));
        Assert.Equal(expectedDisplayMode, entry["displayMode"]);
    }
}

[Fact]
public void Convert_DefaultPrintData_OmitsDisplayMode_BackwardCompatibility()
{
    // Arrange
    var printData = new PrintDataNode
    {
        Variant = "PRINTDATA",  // Default variant
        Content = { new DataformNode { Arguments = { "Test line" } } }
    };

    var converter = new PrintDataConverter();

    // Act
    var yamlResult = converter.Convert(printData, "@TestChara", "@TestSituation");

    // Parse YAML to verify displayMode absence
    var deserializer = new YamlDotNet.Serialization.Deserializer();
    var result = deserializer.Deserialize<Dictionary<string, object>>(yamlResult);

    // Assert - displayMode should NOT be present
    var entries = result["entries"] as List<object>;
    var entry = entries[0] as Dictionary<object, object>;
    Assert.False(entry.ContainsKey("displayMode"));

    // Verify YAML structure is valid without displayMode
    // (This ensures existing F636/F639/F640/F643 files remain valid)
}
```

**Test naming convention**: Test methods follow `Convert_{Variant}_{ExpectedBehavior}` format to match AC#5-6 filter patterns.

**Verification**: AC#5 (PRINTDATAL test), AC#6 (all 9 variants test), AC#9 (backward compatibility test), AC#13 (all tests pass)

### Phase 4: AC Verification (ac-tester)

**Scope**: Verify AC#1-6, AC#9-13 (excluding AC#7-8 which are out of scope)

**Method**:
1. AC#1-2: Grep dialogue-schema.json for displayMode field and required array
2. AC#3: Grep PrintDataConverter.cs for displayMode emission logic
3. AC#4: Grep FileConverter.cs for displayMode propagation logic
4. AC#5-6, AC#9, AC#13: Run dotnet test with appropriate filters
5. AC#10: Grep PrintDataConverter.cs and count PRINTDATA variant references (should be 9)
6. AC#11: Grep for TODO/FIXME/HACK in modified files
7. AC#12: Run dotnet build

**AC#7-8 Handling**: Mark as `[B]` (BLOCKED) with note that renderer integration is deferred to F644 or follow-up feature. Add to 残課題 section.

### Success Criteria

1. All in-scope ACs pass (AC#1-6, AC#9-13)
2. AC#7-8 marked as `[B]` and tracked in 残課題
3. PRINTDATAL conversion produces YAML with `displayMode: "newline"`
4. Existing YAML files still validate against updated schema (backward compatibility verified)
5. No technical debt (TODO/FIXME/HACK) in modified files
6. All unit tests pass
7. Build succeeds

### Out-of-Scope Items

**AC#7-8 (Renderer Layer)**: Era.Core PrintCommand/PrintHandler integration is out of scope because:
1. F634 YAML output (dialogue-schema.json) is NOT consumed by KojoEngine
2. KojoEngine uses a different YAML schema (flat entries list)
3. F634 YAML is consumed by KojoComparer (equivalence testing tool)
4. Renderer integration (if needed) will be addressed in F644 or follow-up feature

**Tracking**: AC#7-8 will be marked `[B]` and added to 残課題 section with destination feature reference.

### Variant Mapping Reference (SSOT)

| ERB Variant | Suffix Semantics | displayMode Value | Usage in Kojo |
|-------------|------------------|-------------------|---------------|
| PRINTDATA | (none) | (omitted) | Common |
| PRINTDATAL | L = newline | "newline" | 1,575 occurrences |
| PRINTDATAW | W = wait | "wait" | 0 occurrences |
| PRINTDATAK | K = key-wait | "keyWait" | 0 occurrences |
| PRINTDATAKL | K + L | "keyWaitNewline" | 0 occurrences |
| PRINTDATAKW | K + W | "keyWaitWait" | 0 occurrences |
| PRINTDATAD | D = display mode | "display" | 0 occurrences |
| PRINTDATADL | D + L | "displayNewline" | 0 occurrences |
| PRINTDATADW | D + W | "displayWait" | 0 occurrences |

**Note**: Only PRINTDATAL is actually used in kojo files, but all 9 variants are implemented for correctness and forward-compatibility.

### File Change Summary

| File | Lines Changed (Est.) | Change Type | Task# |
|------|---------------------|-------------|-------|
| dialogue-schema.json | +12 | Schema extension | 1 |
| DisplayModeMapper.cs | +15 | New utility class | 2 |
| PrintDataConverter.cs | +10 | Emit logic only | 2 |
| FileConverter.cs | +20 | Emit logic + variant tracking | 3 |
| BranchesToEntriesConverter.cs | +10 | Propagation logic | 4 |
| PrintDataConverterTests.cs | +150 | New test cases | 5 |

**Total estimated changes**: ~207 lines (within engine type scope limits ~300)

---

## 残課題

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| AC#7-8: Era.Core renderer integration (PrintCommand DisplayMode, PrintHandler dispatch) | REMOVED: dialogue-schema.json is not consumed by Era.Core. Renderer integration requires separate architecture decision (different YAML schema). | New feature | F676 | Create F676: Era.Core renderer DisplayMode integration (dependent on F671 completion) |
| dialogue-schema.json vs Era.Core schema incompatibility | F634 output uses CamelCaseNamingConvention branches structure; KojoEngine expects flat DialogueEntry list with UnderscoredNamingConvention. These are fundamentally different YAML formats. | New feature | F676 | Create F676: Era.Core renderer DisplayMode integration (dependent on F671 completion) |
| KojoComparer displayMode consumption compatibility | Philosophy promises 'metadata preservation for equivalence testing' but no AC verifies KojoComparer can read YAML files containing displayMode field without errors. Forward compatibility gap. | New feature | F677 | Create F677: KojoComparer displayMode awareness (dependent on F671 completion) |

<!-- TBD Prohibition: F644 is [DONE]. Renderer integration must be tracked via F676 (new feature) rather than F644 scope addition. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-28 | fc-phase-5 | wbs-generator | Generate Tasks + Implementation Contract | Tasks table created, AC#7-8 scoped out to 残課題 |
| 2026-01-30 11:04 | START | implementer | Tasks 1-4 | - |
| 2026-01-30 11:04 | END | implementer | Tasks 1-4 | SUCCESS |
| 2026-01-30 11:07 | AC Verification | ac-tester | Verify AC#1-15 (excluding AC#7-8 [B]) | 11/15 PASS, 2/15 FAIL (AC#14-15 file creation), 2/15 BLOCKED (AC#7-8) |
| 2026-01-30 11:09 | Tasks 9-10 | orchestrator | Create F676.md, F677.md [DRAFT] | AC#14-15 now PASS |
| 2026-01-30 | BLOCKED | orchestrator | User chose Option A: wait for F676 | Status [WIP] → [BLOCKED]. AC#7-8 deferred to F676. F676/F677 registered in index-features.md |
| 2026-01-30 | DONE | orchestrator | Circular dependency detected: F671↔F676 deadlock. User waived AC#7-8 (out of scope by design) | Status [BLOCKED] → [DONE]. AC#7-8 tracked in 残課題 → F676 |

---

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Extend dialogue-schema.json to capture PrintDataNode display variant semantics" | Schema must have optional displayMode field in entry objects | AC#1, AC#2 |
| "YAML metadata preservation for equivalence testing and future ERB regeneration" | Converter must read PrintDataNode.Variant and emit displayMode in YAML output | AC#3, AC#4, AC#5 |
| "Map PrintDataNode Variant metadata to display behavior semantics" | All 9 variants must map to correct displayMode values | AC#6 |
| "metadata preservation for equivalence testing" | YAML output contains displayMode metadata for testing consumption | AC#5, AC#6, AC#9 (runtime behavior deferred to F676) |
| "backward-compatible" (from Technical Constraints) | Existing YAML files without displayMode must remain valid against updated schema | AC#9 |
| "All 9 variants" (from Background) | Variant mapping must be complete for all 9 PRINTDATA variants | AC#6, AC#10 |
| "Zero technical debt" (engine standard) | No TODO/FIXME/HACK markers in new/modified files | AC#11 |
| "Build succeeds" (engine standard) | Solution builds without errors after changes | AC#12 |
| "Unit tests pass" (engine standard) | All existing and new unit tests pass | AC#13 |
