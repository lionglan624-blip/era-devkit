# Feature 677: KojoComparer DisplayMode Awareness

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Update KojoComparer to handle displayMode metadata in dialogue-schema.json YAML files, ensuring equivalence testing accounts for PRINTDATA variant differences.

---

## Background

### Problem (Current Issue)

F671 added displayMode metadata to dialogue-schema.json YAML output. KojoComparer compares ERB vs YAML dialogue output for equivalence testing, but it does not currently recognize or handle the displayMode field. YamlDialogueLoader's DialogueEntryData class has no displayMode property — the field is silently ignored during deserialization. DiffEngine compares normalized text strings only, with no mechanism to report displayMode presence or mismatches.

### Goal (What to Achieve)

1. YAML-side metadata extraction: KojoComparer can read displayMode from YAML files via DialogueResult (leveraging F676's completed Era.Core pipeline)
2. Comparison framework extension: DiffEngine can report displayMode presence/absence between ERB and YAML sides
3. **Out of scope**: Full ERB↔YAML displayMode equivalence comparison (ERB headless mode does not expose PRINTDATA variant metadata) — deferred to F678

---

## Links

- [feature-671.md](feature-671.md) - PrintData Variant Metadata Mapping (Predecessor - added displayMode to schema/converter)
- [feature-676.md](feature-676.md) - Era.Core renderer DisplayMode integration (Related)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Related)
- [feature-651.md](feature-651.md) - KojoComparer KojoEngine API Update (Related)
- [feature-678.md](feature-678.md) - ERB↔YAML DisplayMode Equivalence Comparison (Successor - full equivalence deferred here) [DRAFT]

---

## Notes

- Created by F671 Task 10 as documented in 残課題
- KojoComparer currently uses KojoEngine which returns DialogueResult with lines only, no display metadata
- Forward compatibility gap: YAML files with displayMode should not break existing comparison
- Scope revised per feasibility assessment: YAML-side only. ERB↔YAML full equivalence deferred to F678

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: KojoComparer cannot account for displayMode metadata when comparing ERB vs YAML dialogue output
2. Why: KojoComparer's YamlRunner only reads DialogueResult.Lines (text content), ignoring DialogueResult.DialogueLines which contains displayMode metadata
3. Why: DiffEngine.Compare() only accepts text strings (normalizedA, normalizedB), with no displayMode parameters or comparison logic
4. Why: BatchProcessor extracts yamlOutput as text only via YamlRunner.Render(), not using RenderWithMetadata() to access displayMode data
5. Why: The original equivalence testing design (F644) compared only text output, and KojoComparer consumer code was not updated when F676 added displayMode to the Era.Core pipeline

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| KojoComparer does not compare displayMode metadata between ERB and YAML | KojoComparer's YAML pipeline (YamlDialogueLoader → KojoEngine → DialogueResult) has no mechanism to carry displayMode metadata; the field is silently dropped during YAML deserialization |
| ERB PRINTDATAL output appears to match YAML output even though display behavior differs | DiffEngine compares normalized text strings only; display variant semantics are outside the comparison scope |

### Conclusion

The root cause is a **consumer code gap**: KojoComparer's consumer code (YamlRunner, DiffEngine, BatchProcessor) operates on text-only data despite the Era.Core pipeline now providing displayMode metadata. YamlRunner.Render() extracts only DialogueResult.Lines (text) instead of DialogueLines (text + displayMode). DiffEngine.Compare() accepts only text parameters. BatchProcessor uses Render() instead of accessing full DialogueResult. The ERB side uses headless mode which captures console text output, also losing PRINTDATA variant information.

The fix for F677 (YAML-side only scope) requires:
1. **YAML side**: KojoComparer must extract displayMode metadata from DialogueResult (F676 provides this)
2. **Comparison**: DiffEngine must report displayMode presence/absence (ERB side always null until F678)

Deferred to F678:
3. **ERB side**: The headless mode output or a supplementary mechanism must indicate which PRINTDATA variant was used
4. **Full comparison**: DiffEngine comparison of ERB vs YAML displayMode equivalence

**Key architectural insight**: The current YAML files in `Game/YAML/Kojo/` do **not** yet contain displayMode (they were converted before F671). displayMode will only appear after re-conversion with the F671-updated converter. Therefore, the immediate practical impact is forward-compatibility: ensuring KojoComparer doesn't break when displayMode appears, and can compare it when both sides provide the data.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F671 | [DONE] | Predecessor | Added displayMode to dialogue-schema.json and converter output. Created F677 in 残課題 |
| F676 | [DONE] | Related | Era.Core renderer displayMode integration. Separate concern (runtime rendering vs testing) |
| F644 | [DONE] | Related | Equivalence Testing Framework (batch mode). Did not include displayMode awareness |
| F651 | [DONE] | Predecessor | KojoComparer KojoEngine API Update. Established current YamlRunner/KojoEngine pipeline |
| F675 | [DONE] | Predecessor | YAML Format Unification (branches → entries). Established current schema format |

### Pattern Analysis

This is the third instance of a **pipeline metadata gap** pattern in the kojo conversion toolchain:
1. F634 deferred variant handling → F671 added displayMode to converter
2. F671 deferred renderer integration → F676 planned for Era.Core
3. F671 deferred comparison awareness → F677 (this feature) for KojoComparer

The pattern shows incremental metadata propagation: each feature adds metadata at one layer but downstream consumers need separate updates. This is expected in incremental migration but highlights the need for end-to-end verification when adding new metadata fields.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | YamlDialogueLoader can be extended with displayMode property. DiffEngine comparison can include metadata. Changes are localized to KojoComparer and Era.Core |
| Scope is realistic | PARTIAL | YAML-side displayMode extraction is straightforward. ERB-side displayMode extraction is harder -- headless mode captures text output, not PRINTDATA variant info. ERB comparison may need to infer variant from output behavior or be scoped to YAML-only verification |
| No blocking constraints | PARTIAL | DialogueResult record is immutable (single Lines parameter). Extending it requires a breaking change or a new type. ERB headless mode does not expose PRINTDATA variant metadata in its JSON output |

**Verdict**: FEASIBLE (after scope revision)

**Scope revision applied**: Background/Goal revised to YAML-side only scope:
1. Forward compatibility: YamlDialogueLoader preserves displayMode without errors — FEASIBLE
2. YAML-side metadata extraction: KojoComparer can read displayMode from YAML files — FEASIBLE
3. Comparison framework extension: DiffEngine can report displayMode presence/absence — FEASIBLE
4. Full ERB↔YAML displayMode equivalence deferred to F678 (requires headless mode extension)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F671 | [DONE] | Added displayMode to dialogue-schema.json and converter. Created F677 in 残課題 |
| Predecessor | F651 | [DONE] | KojoComparer KojoEngine API Update. Current pipeline must be preserved |
| Predecessor | F675 | [DONE] | YAML Format Unification. Established entries: format used by YamlDialogueLoader |
| Related | F676 | [DONE] | Era.Core renderer displayMode. Separate concern -- runtime vs testing |
| Related | F644 | [DONE] | Equivalence Testing Framework. Batch mode infrastructure already in place |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already referenced by KojoComparer via Era.Core. displayMode deserialization is standard |
| Era.Core DialogueResult | Runtime | Medium | Immutable record with Lines only. Extension requires new property or new type |
| Headless mode JSON output | Runtime | High | Does not expose PRINTDATA variant. ERB-side comparison blocked without headless changes |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/YamlRunner.cs | HIGH | Uses KojoEngine.GetDialogue() which returns DialogueResult(Lines). Must carry displayMode |
| tools/KojoComparer/DiffEngine.cs | HIGH | Compare() takes two strings. Must support metadata comparison |
| tools/KojoComparer/BatchProcessor.cs | MEDIUM | ProcessAllAsync() uses DiffEngine. Must propagate displayMode comparison |
| tools/KojoComparer/Program.cs | LOW | CLI output. May need to display displayMode diff details |
| tools/KojoComparer.Tests/ | MEDIUM | Existing tests must continue passing. New tests for displayMode |
| Era.Core/Types/DialogueResult.cs | MEDIUM | Immutable record. Extension needed to carry displayMode |
| Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | HIGH | DialogueEntryData lacks displayMode property. Must be extended |
| Era.Core/KojoEngine.cs | MEDIUM | GetDialogue() creates DialogueResult from Lines. Must propagate displayMode |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Dialogue/Loading/YamlDialogueLoader.cs | Update | Add displayMode property to DialogueEntryData for deserialization |
| Era.Core/Dialogue/DialogueEntry (or similar) | Update | Add DisplayMode property to domain model |
| Era.Core/Types/DialogueResult.cs | Update | Extend record to include display mode metadata |
| Era.Core/KojoEngine.cs | Update | Propagate displayMode from loaded entry to DialogueResult |
| tools/KojoComparer/YamlRunner.cs | Update | Extract displayMode from DialogueResult and pass to comparison |
| tools/KojoComparer/DiffEngine.cs | Update | Add displayMode to comparison logic |
| tools/KojoComparer.Tests/YamlRunnerTests.cs | Update | Test displayMode extraction |
| tools/KojoComparer.Tests/DiffEngineTests.cs | Update | Test displayMode comparison |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| DiffEngine.Compare() currently accepts only string parameters | tools/KojoComparer/DiffEngine.cs | MEDIUM - Adding optional parameters for displayMode requires signature extension |
| YamlRunner.Render() returns text only, not structured metadata | tools/KojoComparer/YamlRunner.cs | LOW - Adding RenderWithMetadata() is additive, existing callers unaffected |
| ERB headless mode outputs text only, not PRINTDATA variant metadata | engine/uEmuera.Headless | HIGH - Cannot compare ERB displayMode without engine changes |
| KojoEngine loads one DialogueEntry (selected by condition), not all entries | Era.Core/KojoEngine.cs line 77-81 | LOW - displayMode is per-entry, available after selection |
| Existing YAML files in Game/YAML/Kojo/ do not contain displayMode yet | Grep analysis | LOW - Forward-looking change; no data to compare until re-conversion |
| YamlDotNet silently ignores unknown YAML properties during deserialization | YamlDotNet behavior | LOW - Favorable constraint for forward compatibility (existing code won't crash) |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| DiffEngine Compare() signature change may affect callers | Low | Medium | Optional parameters maintain backward compatibility but could cause ambiguity. Check DiffEngine callers (BatchProcessor). |
| ERB-side displayMode extraction infeasible without engine changes | High | Medium | Scope F677 to YAML-only displayMode awareness. Defer ERB equivalence to future feature requiring headless mode extension |
| Existing KojoComparer tests fail due to DialogueResult change | Medium | Medium | DialogueResult extension must be backward-compatible (optional parameter with default) |
| No practical displayMode data to test (YAML files not re-converted) | Medium | Low | Create test fixtures with displayMode in unit tests. Production testing deferred to post-re-conversion |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "KojoComparer can read displayMode from YAML files via DialogueResult" | YamlRunner must extract displayMode from DialogueResult; BatchProcessor must integrate the extraction | AC#1, AC#5, AC#9 |
| "DiffEngine can report displayMode presence/absence between ERB and YAML sides" | DiffEngine comparison must include displayMode metadata in its comparison and reporting | AC#2, AC#3 |
| "Full ERB↔YAML displayMode equivalence comparison deferred to F678" | No ERB-side displayMode extraction in this feature; DiffEngine handles missing ERB displayMode gracefully | AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | YamlRunner extracts displayMode from DialogueResult | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | `RenderWithMetadata` | [x] |
| 2 | DiffEngine ComparisonResult includes displayMode differences | code | Grep(tools/KojoComparer/DiffEngine.cs) | contains | `DisplayModeDifferences` | [x] |
| 3 | DiffEngine reports displayMode mismatch | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~DiffEngine.*DisplayModeMismatch | succeeds | - | [x] |
| 4 | DiffEngine handles missing ERB displayMode gracefully | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~DiffEngine.*MissingDisplayMode | succeeds | - | [x] |
| 5 | YamlRunner displayMode extraction unit test | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~YamlRunner.*DisplayMode | succeeds | - | [x] |
| 6 | Existing KojoComparer tests pass (backward compatibility) | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 7 | Existing Era.Core tests pass (backward compatibility) | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 8 | Zero technical debt | code | Grep(tools/KojoComparer/DiffEngine.cs,tools/KojoComparer/YamlRunner.cs,tools/KojoComparer/BatchProcessor.cs) | not_contains | `TODO\|FIXME\|HACK` | [x] |
| 9 | BatchProcessor integration with displayMode | code | Grep(tools/KojoComparer/BatchProcessor.cs) | contains | `RenderWithMetadata` | [x] |

### AC Details

**AC#1: YamlRunner extracts displayMode from DialogueResult**
- YamlRunner.RenderWithMetadata() must access DialogueResult's displayMode metadata for comparison purposes
- Test: Grep pattern=`DisplayMode` path=tools/KojoComparer/YamlRunner.cs
- YamlRunner must make displayMode available to the comparison pipeline (BatchProcessor/DiffEngine)

**AC#2: DiffEngine ComparisonResult includes displayMode differences**
- ComparisonResult must have a field or collection for displayMode-specific comparison results
- Test: Grep pattern=`DisplayModeDifferences` path=tools/KojoComparer/DiffEngine.cs
- This separates text content differences from display behavior differences in reporting

**AC#3: DiffEngine reports displayMode mismatch**
- Unit test verifying that when ERB side has displayMode X and YAML side has displayMode Y, the DiffEngine reports the mismatch
- Test: dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~DiffEngine.*DisplayMode
- Test should verify mismatch appears in DisplayModeDifferences collection

**AC#4: DiffEngine handles missing ERB displayMode gracefully**
- Since ERB headless mode does not expose PRINTDATA variant metadata (deferred to F678), the ERB side will have null/missing displayMode
- Unit test verifying DiffEngine does not fail when one side has displayMode and the other does not
- Test: dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~DiffEngine.*MissingDisplayMode
- Expected behavior: report as informational (YAML has displayMode, ERB does not) rather than hard failure

**AC#5: YamlRunner displayMode extraction unit test**
- Unit test verifying YamlRunner correctly extracts displayMode from a YAML test fixture containing displayMode metadata
- Test: dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~YamlRunner.*DisplayMode
- Test fixture must include entries with displayMode: "newline" (the only variant used in kojo files per investigation)

**AC#6: Existing KojoComparer tests pass (backward compatibility)**
- All existing KojoComparer tests must continue passing after the changes
- Test: dotnet test tools/KojoComparer.Tests
- DialogueResult extension must be backward-compatible (optional parameter with default or new property alongside Lines)

**AC#7: Existing Era.Core tests pass (backward compatibility)**
- All existing Era.Core tests must continue passing after changes to YamlDialogueLoader, DialogueResult, and KojoEngine
- Test: dotnet test Era.Core.Tests
- Ensures no breaking changes to existing consumers

**AC#8: Zero technical debt**
- No TODO, FIXME, or HACK markers in modified files
- Test: Grep pattern=`TODO|FIXME|HACK` paths=[tools/KojoComparer/DiffEngine.cs, tools/KojoComparer/YamlRunner.cs, tools/KojoComparer/BatchProcessor.cs]
- Expected: 0 matches

**AC#9: BatchProcessor integration with displayMode**
- BatchProcessor.ProcessAllAsync() and ProcessAsync() must call RenderWithMetadata() and pass displayModes to DiffEngine
- Test: Grep pattern=`RenderWithMetadata` path=tools/KojoComparer/BatchProcessor.cs
- Verifies both Compare() call sites are updated for displayMode support

<!-- fc-phase-4-completed -->
## Technical Design

### Overview

F677 extends KojoComparer's equivalence testing framework to recognize and compare displayMode metadata from YAML dialogue files. This feature has a **two-layer dependency structure**:

**Layer 1: Era.Core Pipeline (F676 - Prerequisite)**
- Adds DisplayMode to DialogueEntry, DialogueEntryData, YamlDialogueLoader
- Extends DialogueResult with DialogueLine record carrying displayMode
- Propagates displayMode through KojoEngine

**Layer 2: KojoComparer Consumer (F677 - This Feature)**
- YamlRunner reads DialogueLines instead of Lines to access displayMode
- DiffEngine extended with displayMode comparison logic
- Graceful null handling for ERB side (no displayMode until F678)

**Key architectural constraint**: F677 implementation depends on F676 completing AC#1-4 (Era.Core changes). F677's own scope is limited to KojoComparer consumer code.

### Design Principles

1. **Dependency Boundary Respect**: F677 code touches only tools/KojoComparer/; Era.Core changes are F676's responsibility
2. **YAML-Side Only**: ERB headless mode does not expose PRINTDATA variant metadata. displayMode comparison is YAML→null for now.
3. **Graceful Degradation**: Missing displayMode (ERB side, legacy YAML files) is handled as informational, not error
4. **Backward Compatibility**: Existing comparison logic unchanged; displayMode is an additive check

### Approach

#### YamlRunner Extension

**File**: `tools/KojoComparer/YamlRunner.cs`

**Solution**: Add `RenderWithMetadata()` returning full `DialogueResult` for structured access. Existing `Render()` delegates to it for backward compatibility.

#### DiffEngine Extension

**File**: `tools/KojoComparer/DiffEngine.cs`

**Solution**: Add `DisplayModeDifferences` property to `ComparisonResult`. Extend `Compare()` with optional `displayModesA`/`displayModesB` parameters. Add `CompareDisplayModes()` helper.

#### BatchProcessor Integration

**File**: `tools/KojoComparer/BatchProcessor.cs`

**Solution**: Update both `ProcessAllAsync()` and `ProcessAsync()` to call `RenderWithMetadata()`, extract displayModes, pass to `Compare()`.

### Key Decisions

| Decision | Selected | Rationale |
|----------|----------|-----------|
| YamlRunner API extension | Add new RenderWithMetadata() method | Zero breaking changes; existing callers unaffected |
| DiffEngine Compare() signature | Extend existing with optional parameters | Backward compatible |
| displayMode mismatch handling | Informational only (DisplayModeDifferences list) | ERB side has no displayMode (F678 deferred) |
| ERB displayMode source | Pass null (not available) | Headless mode does not expose PRINTDATA variant metadata |

### File Modification Summary

| File | Change Type | Breaking |
|------|-------------|----------|
| tools/KojoComparer/YamlRunner.cs | Add RenderWithMetadata() | No |
| tools/KojoComparer/DiffEngine.cs | Add DisplayModeDifferences, extend Compare() | No |
| tools/KojoComparer/BatchProcessor.cs | Update to use RenderWithMetadata() | No |
| tools/KojoComparer/IYamlRunner.cs | Add RenderWithMetadata to interface | No |
| tools/KojoComparer.Tests/YamlRunnerDisplayModeTests.cs | New test file | N/A |
| tools/KojoComparer.Tests/DiffEngineDisplayModeTests.cs | New test file | N/A |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,5 | Extend YamlRunner with RenderWithMetadata() method and unit tests | [x] |
| 2 | 2,3,4 | Extend DiffEngine with DisplayModeDifferences property, Compare() overload, CompareDisplayModes() helper, and unit tests | [x] |
| 3 | 9 | Integrate displayMode extraction in BatchProcessor.ProcessAllAsync() | [x] |
| 4 | 6,7,8 | Quality gates: Run all tests (backward compatibility) and verify zero tech debt | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | YamlRunner.cs specification from Technical Design | RenderWithMetadata() implementation + YamlRunnerDisplayModeTests.cs |
| 2 | implementer | sonnet | T2 | DiffEngine.cs specification from Technical Design | DisplayModeDifferences property, Compare() overload, CompareDisplayModes() + DiffEngineDisplayModeTests.cs |
| 3 | implementer | sonnet | T3 | BatchProcessor.cs specification from Technical Design | ProcessAllAsync() integration with displayMode extraction |
| 4 | ac-tester | haiku | T4 | AC#1-9 test commands | Test results |

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| ERB-side displayMode extraction from headless mode JSON output | F678 | Requires engine changes to expose PRINTDATA variant metadata in headless mode |
| Full ERB↔YAML displayMode equivalence comparison | F678 | Blocked on ERB-side extraction |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-30 | Initialization | Feature initialized by initializer agent. Status: [REVIEWED] → [WIP] |
| 2026-01-30 20:30 | Phase 3 TDD RED | Created RED tests: DiffEngineDisplayModeTests.cs (4 tests), YamlRunnerDisplayModeTests.cs (3 tests). Compilation fails as expected. |
| 2026-01-30 21:00 | Phase 4 T1+T2 | Implementer: RenderWithMetadata() added to YamlRunner, DiffEngine extended with DisplayModeDifferences + Compare overload. Test fixtures created. |
| 2026-01-30 21:10 | Phase 4 T3 | Implementer: BatchProcessor updated to use RenderWithMetadata(), IYamlRunner interface updated. |
| 2026-01-30 21:15 | DEVIATION | ac-tester AC#5 FAIL: YamlRunnerDisplayModeTests failed - test fixture paths (displaymode_newline.yaml) don't match YamlRunner.ParseYamlPath() validation |
| 2026-01-30 21:20 | DEVIATION | debugger regression: Fixed path issue but reverted DiffEngine.cs and commented out DiffEngineDisplayModeTests, also overwrote feature-677.md to [DRAFT] |
| 2026-01-30 21:30 | Phase 4 fix | Orchestrator: Re-implemented DiffEngine extension, restored DiffEngineDisplayModeTests, updated BatchProcessorTests mock setup, restored feature-677.md. All 25 tests pass, 8 skipped (pre-existing). Era.Core.Tests: 1443 pass. |
