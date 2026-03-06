# Feature 649: FileConverter DatalistConverter Interface Refactor

## Status: [DONE]

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

## Type: engine

## Created: 2026-01-28

---

## Summary

Refactor FileConverter.cs DatalistConverter dependency to use a proper interface with accessible properties, eliminating the `_talentLoader = null` workaround pattern.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion tooling quality - eliminate structural workarounds in conversion pipeline.

### Problem (Current Issue)

FileConverter.cs constructor (line 28-30) sets `_talentLoader = null` because DatalistConverter is not an interface with accessible properties. The comment was removed in F639 T12, but the structural workaround remains: when constructed without explicit TalentCsvLoader, FileConverter cannot access the TalentCsvLoader that DatalistConverter internally creates.

### Goal (What to Achieve)

1. Define IDatalistConverter interface with TalentCsvLoader property access
2. Eliminate `_talentLoader = null` workaround in 3-parameter constructor
3. Ensure backward compatibility with existing callers

---

## Links

- [feature-639.md](feature-639.md) - Sakuya Kojo Conversion (origin: Mandatory Handoff T13)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (created FileConverter)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Related Feature)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: FileConverter.cs constructor (line 28) sets `_talentLoader = null` when the 3-parameter constructor is used
2. Why: FileConverter needs TalentCsvLoader for condition parsing (ParseCondition method at lines 337-405), but DatalistConverter creates its own TalentCsvLoader internally (line 30, 53) and does not expose it
3. Why: IDatalistConverter interface only defines `Convert()` and `ValidateYaml()` methods - it has no property to access the internal TalentCsvLoader instance
4. Why: DatalistConverter was designed as a self-contained converter with internal dependencies, and IDatalistConverter was extracted from it only for testability (F634), not for exposing internal state
5. Why: F634 prioritized getting batch conversion working over architectural purity - the workaround was acceptable short-term but creates duplicated TalentCsvLoader instances and breaks encapsulation

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `_talentLoader = null` workaround in FileConverter constructor | IDatalistConverter interface lacks TalentCsvLoader property access |
| Duplicated TalentCsvLoader instances (one in DatalistConverter, optionally one in FileConverter) | No single source of truth for TalentCsvLoader in the converter pipeline |
| FileConverter.ParseCondition() duplicates DatalistConverter.ParseCondition() logic | No shared service for condition parsing between converters |

### Conclusion

The root cause is **interface design incompleteness**: IDatalistConverter was extracted from DatalistConverter for testability but only included the two public methods (`Convert()`, `ValidateYaml()`). It did not expose the internal `TalentCsvLoader` that FileConverter also needs for condition parsing. This forced FileConverter to either:
- Option A: Accept TalentCsvLoader as a separate constructor parameter (current 4-parameter constructor)
- Option B: Set `_talentLoader = null` and lose condition parsing capability (current 3-parameter constructor workaround)

The fix requires extending IDatalistConverter to expose TalentCsvLoader (either directly or via a condition parsing delegate/interface).

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Origin | Batch Conversion Tool - introduced FileConverter with this workaround pattern |
| F639 | [BLOCKED] | Origin | Sakuya Kojo Conversion - created F649 as mandatory handoff (T13) |
| F636-F643 | [DRAFT]/[BLOCKED] | Related | Per-character conversion batches - all use FileConverter via BatchConverter |
| F644 | [DRAFT] | Related | Equivalence Testing Framework - downstream consumer of conversion output |
| F349 | Historical | Foundation | Original DatalistConverter implementation |

### Pattern Analysis

This is a **single-occurrence issue** specific to FileConverter/DatalistConverter integration introduced in F634. The workaround pattern does not recur elsewhere in the codebase. However, it violates the DRY principle because:
1. FileConverter.ParseCondition() (lines 337-405) duplicates DatalistConverter.ParseCondition() (lines 206-271)
2. Both classes independently create TalentConditionParser instances

The duplication exists because FileConverter cannot delegate condition parsing to DatalistConverter without exposing TalentCsvLoader through the interface.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Interface extension is straightforward - add TalentCsvLoader property or condition parsing method to IDatalistConverter |
| Scope is realistic | YES | Changes limited to 3 files: IDatalistConverter.cs, DatalistConverter.cs, FileConverter.cs |
| No blocking constraints | YES | No external dependencies or prerequisites |

**Verdict**: FEASIBLE

Three solution approaches identified:
1. **Property exposure**: Add `TalentCsvLoader TalentLoader { get; }` to IDatalistConverter - simple but exposes implementation detail
2. **Method delegation**: Add `Dictionary<string, object>? ParseCondition(string condition)` to IDatalistConverter - cleaner API, hides TalentCsvLoader
3. **Constructor injection**: Inject TalentCsvLoader into both converters from composition root - maintains interface purity but changes DI pattern

Recommended: Option 2 (Method delegation) - exposes capability without implementation detail.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F634 | [DONE] | Batch Conversion Tool - introduced the pattern being refactored |
| Related | F639 | [BLOCKED] | Sakuya Kojo Conversion - origin of this handoff |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| tools/ErbToYaml/IDatalistConverter.cs | Build-time | Low | Interface definition to extend |
| tools/ErbToYaml/DatalistConverter.cs | Build-time | Low | Implementation to update |
| tools/ErbToYaml/FileConverter.cs | Build-time | Low | Consumer to simplify |
| tools/ErbToYaml.Tests/FileConverterTests.cs | Build-time | Low | Tests use 4-parameter constructor with TalentCsvLoader |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/ErbToYaml/FileConverter.cs | HIGH | Primary consumer - workaround exists here |
| tools/ErbToYaml/Program.cs (lines 99-102, 144-147) | MEDIUM | Composition root creates DatalistConverter and FileConverter |
| tools/ErbToYaml.Tests/FileConverterTests.cs | MEDIUM | All tests use 4-parameter constructor with explicit TalentCsvLoader |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/IDatalistConverter.cs | Update | Add ParseCondition() method or TalentLoader property |
| tools/ErbToYaml/DatalistConverter.cs | Update | Implement new interface member (already has ParseCondition method, just needs public exposure) |
| tools/ErbToYaml/FileConverter.cs | Rewrite | Remove _talentLoader field, remove ParseCondition() method, delegate to IDatalistConverter |
| tools/ErbToYaml.Tests/FileConverterTests.cs | Update | Simplify test setup - remove TalentCsvLoader construction where not needed |
| tools/ErbToYaml/Program.cs | Update (minor) | May need to adjust composition root (remove separate TalentCsvLoader creation if Option 2) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| DatalistConverter.ParseCondition() is private | DatalistConverter.cs line 206 | MEDIUM - must change visibility to implement interface method |
| IDatalistConverter already has 2 methods | IDatalistConverter.cs | LOW - adding a third method is additive, no breaking changes |
| FileConverterTests use 4-parameter constructor | FileConverterTests.cs | LOW - tests still work, can be simplified later |
| Program.cs composition root uses concrete types | Program.cs lines 99-102, 144-147 | LOW - interface change is transparent to composition root |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking change to IDatalistConverter consumers | Low | Medium | Interface addition is non-breaking; existing implementations just add new method |
| FileConverter test regressions | Low | Low | 4-parameter constructor can remain for backward compatibility; tests gradually simplified |
| DatalistConverter ParseCondition visibility change exposes internal logic | Low | Low | ParseCondition returns schema-compliant structure, safe to expose |
| Composition root changes introduce wiring errors | Low | Medium | Build and test verification after changes |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "eliminate structural workarounds" (Philosophy) | `_talentLoader = null` workaround must be removed from FileConverter | AC#4, AC#5 |
| "IDatalistConverter interface with TalentCsvLoader property access" (Goal 1) | Interface must expose condition parsing capability | AC#1, AC#2 |
| "Eliminate `_talentLoader = null` workaround" (Goal 2) | FileConverter 3-parameter constructor must not set null | AC#4, AC#5 |
| "Ensure backward compatibility" (Goal 3) | Existing callers must work without modification | AC#8 |
| "ParseCondition method to IDatalistConverter interface" (Context) | Method delegation pattern implemented | AC#1, AC#2, AC#3 |
| "single source of truth for condition parsing" (Root Cause) | FileConverter must delegate to DatalistConverter, not duplicate | AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IDatalistConverter has ParseCondition method | code | Grep(tools/ErbToYaml/IDatalistConverter.cs) | contains | `Dictionary<string, object>? ParseCondition(string condition)` | [x] |
| 2 | DatalistConverter.ParseCondition is public | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | matches | `public Dictionary<string, object>\\? ParseCondition` | [x] |
| 3 | DatalistConverter implements IDatalistConverter.ParseCondition | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | matches | `public Dictionary<string, object>\\? ParseCondition\\(string condition\\)` | [x] |
| 4 | FileConverter has no _talentLoader field | code | Grep(tools/ErbToYaml/FileConverter.cs) | not_contains | `_talentLoader` | [x] |
| 5 | FileConverter 3-parameter constructor has no null workaround | code | Grep(tools/ErbToYaml/FileConverter.cs) | not_contains | `_talentLoader = null;` | [x] |
| 6 | FileConverter has no ParseCondition method | code | Grep(tools/ErbToYaml/FileConverter.cs) | not_contains | `private Dictionary<string, object>? ParseCondition` | [x] |
| 7 | FileConverter delegates condition parsing to IDatalistConverter | code | Grep(tools/ErbToYaml/FileConverter.cs) | contains | `_datalistConverter.ParseCondition` | [x] |
| 8 | Build succeeds | build | dotnet build tools/ErbToYaml | succeeds | - | [x] |
| 9 | Unit tests pass | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 10 | Zero technical debt | code | Grep(tools/ErbToYaml/*.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |

### AC Details

**AC#1: IDatalistConverter has ParseCondition method**
- Verifies interface extension per Goal 1
- Method signature: `Dictionary<string, object>? ParseCondition(string condition)`
- Returns nullable dictionary for graceful handling of unparseable conditions
- Path: tools/ErbToYaml/IDatalistConverter.cs

**AC#2: DatalistConverter.ParseCondition is public**
- DatalistConverter.ParseCondition (currently line 206) must change from `private` to `public`
- Uses regex matcher to handle optional whitespace variations
- Path: tools/ErbToYaml/DatalistConverter.cs

**AC#3: DatalistConverter implements IDatalistConverter.ParseCondition**
- Verifies DatalistConverter implements new ParseCondition method with correct public signature
- Uses regex matcher to verify exact method signature and public visibility
- Path: tools/ErbToYaml/DatalistConverter.cs

**AC#4: FileConverter has no _talentLoader field**
- Verifies elimination of workaround pattern per Goal 2
- Field `_talentLoader` (currently line 17) must be removed entirely
- Path: tools/ErbToYaml/FileConverter.cs

**AC#5: FileConverter 3-parameter constructor has no null workaround**
- Verifies `= null;` pattern (currently line 28) is removed
- Constructor should no longer need any TalentCsvLoader-related logic
- Path: tools/ErbToYaml/FileConverter.cs

**AC#6: FileConverter has no ParseCondition method**
- Verifies duplicate ParseCondition method (currently lines 337-405) is removed
- Single source of truth for condition parsing is now DatalistConverter
- Path: tools/ErbToYaml/FileConverter.cs

**AC#7: FileConverter delegates condition parsing to IDatalistConverter**
- ProcessConditionalBranch method must call `_datalistConverter.ParseCondition(condition)`
- Delegation replaces duplicated logic
- Path: tools/ErbToYaml/FileConverter.cs

**AC#8: Build succeeds**
- Verifies backward compatibility - all consumers compile without modification
- Interface addition is non-breaking; existing IDatalistConverter consumers just need to implement new method
- Command: `dotnet build tools/ErbToYaml`

**AC#9: Unit tests pass**
- Verifies existing test coverage still passes
- FileConverterTests may simplify (4-parameter constructor still works but 3-parameter now fully functional)
- Command: `dotnet test tools/ErbToYaml.Tests`

**AC#10: Zero technical debt**
- Verifies no TODO/FIXME/HACK markers in modified files
- Uses comprehensive pattern per feature-quality ENGINE guide Issue 39
- Uses glob pattern to cover all C# files in tools/ErbToYaml directory

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Method Delegation Pattern**: Extend IDatalistConverter interface with `ParseCondition()` method and make FileConverter delegate condition parsing to DatalistConverter. This eliminates duplication and the `_talentLoader` workaround while maintaining backward compatibility.

**Rationale**:
- **Single Source of Truth**: DatalistConverter becomes the sole owner of condition parsing logic (lines 206-271), eliminating the 69-line duplication in FileConverter (lines 337-405)
- **Interface Purity**: Exposes capability (condition parsing) without exposing implementation details (TalentCsvLoader)
- **Minimal Changes**: Only 3 files modified, no composition root changes required
- **Backward Compatible**: IDatalistConverter extension is additive (non-breaking), FileConverter constructors remain unchanged
- **Zero Configuration**: FileConverter.ProcessConditionalBranch (line 323) simply replaces `ParseCondition(condition)` with `_datalistConverter.ParseCondition(condition)`

**How This Satisfies ACs**:
- AC#1-3: Interface extension and implementation visibility change
- AC#4-5: Remove `_talentLoader` field and null assignment workaround
- AC#6-7: Delete FileConverter.ParseCondition() and delegate to IDatalistConverter
- AC#8-9: Build and test verification ensure backward compatibility
- AC#10: Clean refactor with no debt markers

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `Dictionary<string, object>? ParseCondition(string condition);` to IDatalistConverter interface (after ValidateYaml method) |
| 2 | Change DatalistConverter.ParseCondition (line 206) visibility from `private` to `public` |
| 3 | Verify DatalistConverter.ParseCondition has correct public method signature matching IDatalistConverter interface |
| 4 | Delete `private readonly TalentCsvLoader? _talentLoader;` field declaration (line 17) |
| 5 | Delete `_talentLoader = null;` assignment (line 28) from 3-parameter constructor |
| 6 | Delete entire FileConverter.ParseCondition method (lines 333-405, 73 lines including XML doc) |
| 7 | In FileConverter.ProcessConditionalBranch (line 323), replace `ParseCondition(condition)` with `_datalistConverter.ParseCondition(condition)` |
| 8 | Run `dotnet build tools/ErbToYaml` - should succeed without errors |
| 9 | Run `dotnet test tools/ErbToYaml.Tests` - FileConverterTests use 4-parameter constructor, so all tests remain valid |
| 10 | Grep for `TODO\|FIXME\|HACK` in 3 modified files - should find zero matches |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| How to expose condition parsing | A) Add `TalentCsvLoader TalentLoader { get; }` property<br>B) Add `ParseCondition()` method<br>C) Constructor injection from composition root | B | Method delegation hides implementation detail (TalentCsvLoader), exposes only capability. Option A violates encapsulation. Option C requires composition root changes and breaks existing test setup. |
| What to do with 4-parameter constructor | A) Delete it (breaking change)<br>B) Keep it (backward compatibility)<br>C) Deprecate it with [Obsolete] | B | FileConverterTests.cs uses 4-parameter constructor extensively. Keeping it maintains backward compatibility with zero test changes. Constructor parameter is now unused but harmless. |
| Whether to simplify 3-parameter constructor | A) Keep chained constructor pattern<br>B) Inline initialization | A | Chained constructor (line 39) `: this(pathAnalyzer, printDataConverter, datalistConverter)` remains unchanged. Simply delete `_talentLoader = talentLoader;` from 4-parameter body. |
| ParseCondition return type | A) `Dictionary<string, object>?` (nullable)<br>B) `Dictionary<string, object>` (non-null with empty dict on error) | A | Matches existing DatalistConverter.ParseCondition signature (line 206). Nullable allows distinction between "empty condition" (empty dict) and "parsing failed" (null), though current implementation returns empty dict on error. |

### Interfaces / Data Structures

**IDatalistConverter Interface Extension**:

```csharp
public interface IDatalistConverter
{
    string Convert(DatalistNode datalist, string character, string situation);
    void ValidateYaml(string yaml);

    /// <summary>
    /// Parse condition string and convert to dialogue-schema.json format
    /// Feature 649 - eliminate FileConverter condition parsing duplication
    /// </summary>
    /// <param name="condition">Condition string (e.g., "TALENT:恋慕")</param>
    /// <returns>Condition object matching dialogue-schema.json structure, or empty dict if parsing fails</returns>
    Dictionary<string, object>? ParseCondition(string condition);
}
```

**DatalistConverter Implementation**:

```csharp
// Line 206: Change visibility only
public Dictionary<string, object>? ParseCondition(string condition)
{
    // ... existing implementation unchanged (lines 206-271)
}
```

**FileConverter Changes**:

```csharp
// DELETE line 17: private readonly TalentCsvLoader? _talentLoader;

public FileConverter(
    IPathAnalyzer pathAnalyzer,
    IPrintDataConverter printDataConverter,
    IDatalistConverter datalistConverter)
{
    _pathAnalyzer = pathAnalyzer ?? throw new ArgumentNullException(nameof(pathAnalyzer));
    _printDataConverter = printDataConverter ?? throw new ArgumentNullException(nameof(printDataConverter));
    _datalistConverter = datalistConverter ?? throw new ArgumentNullException(nameof(datalistConverter));
    // DELETE line 28: _talentLoader = null;
}

// 4-parameter constructor: Keep for backward compatibility, but delete TalentCsvLoader assignment
public FileConverter(
    IPathAnalyzer pathAnalyzer,
    IPrintDataConverter printDataConverter,
    IDatalistConverter datalistConverter,
    TalentCsvLoader talentLoader)  // Parameter kept but unused
    : this(pathAnalyzer, printDataConverter, datalistConverter)
{
    // DELETE line 41: _talentLoader = talentLoader;
    // Constructor body now empty - keep for backward compatibility
}

// ProcessConditionalBranch (line 323): Delegate to DatalistConverter
private ConditionalBranch ProcessConditionalBranch(IfNode ifNode, ...)
{
    // ...
    var conditionObj = _datalistConverter.ParseCondition(condition); // Changed from ParseCondition(condition)
    // ...
}

// DELETE lines 333-405: FileConverter.ParseCondition method (entire method removed)
```

**Edge Cases Verified**:
- Empty condition string: TalentConditionParser.ParseTalentCondition returns null → ParseCondition returns empty dict (line 214)
- Talent not in CSV: TalentCsvLoader.GetTalentIndex returns null → ParseCondition returns empty dict (line 224)
- Null condition: Not applicable - ProcessConditionalBranch (line 317-318) constructs condition from ifNode properties, never passes null
- Invalid operator: Default case in switch (line 260) maps to `ne: 0`

**Downstream Impact**:
- ProcessConditionalBranch receives same return type (Dictionary<string, object>?) from delegated call
- YAML serialization (YamlDotNet) handles empty dict `{}` as valid condition per dialogue-schema.json
- FileConverterTests continue to work without modification (4-parameter constructor still exists)

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Extend IDatalistConverter interface with ParseCondition method and implement in DatalistConverter | [x] |
| 2 | 4,5,6,7 | Refactor FileConverter to delegate condition parsing to IDatalistConverter | [x] |
| 3 | 8,9,10 | Verify build, tests, and zero technical debt | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Interface extension spec from Technical Design | IDatalistConverter.cs, DatalistConverter.cs updated |
| 2 | implementer | sonnet | T2 | FileConverter refactor spec from Technical Design | FileConverter.cs refactored |
| 3 | ac-tester | haiku | T3 | Test commands from ACs | Build/test verification |

**Constraints** (from Technical Design):
1. DatalistConverter.ParseCondition visibility must change from private to public (line 206)
2. IDatalistConverter interface extension is additive (non-breaking change)
3. FileConverter 4-parameter constructor retained for backward compatibility
4. ProcessConditionalBranch delegation (line 323) replaces direct ParseCondition call
5. Implementation must not introduce TODO/FIXME/HACK markers

**Pre-conditions**:
- tools/ErbToYaml project builds successfully
- tools/ErbToYaml.Tests tests pass
- DatalistConverter.ParseCondition method exists at line 206 (private)
- FileConverter._talentLoader field exists at line 17
- FileConverter.ParseCondition method exists at lines 333-405

**Success Criteria**:
- All 10 ACs pass verification
- `dotnet build tools/ErbToYaml` succeeds
- `dotnet test tools/ErbToYaml.Tests` succeeds with all tests passing
- FileConverter 3-parameter constructor no longer has `_talentLoader = null` workaround
- FileConverter delegates condition parsing to IDatalistConverter.ParseCondition
- Zero TODO/FIXME/HACK markers in modified files

**Rollback Plan**:

If issues arise after implementation:
1. Revert commit with `git revert`
2. Notify user of rollback with issue description
3. Create follow-up feature for fix with additional root cause investigation

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| Identify all structural workarounds in conversion pipeline (comprehensive audit) | Phase 20 | Philosophy Gate gap: broader scope than F649's specific FileConverter workaround fix |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 09:58 | Task 1 - START | implementer - Extend IDatalistConverter interface with ParseCondition method |
| 2026-01-28 09:58 | Task 1 - END | implementer - SUCCESS |
| 2026-01-28 10:00 | Task 2 - START | implementer - Refactor FileConverter to delegate condition parsing to IDatalistConverter |
| 2026-01-28 10:00 | Task 2 - END | implementer - SUCCESS |
