# Feature 698: YamlComExecutor/BatchProcessor DialogueLines Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Migrate YamlComExecutor and BatchProcessor from DialogueResult.Lines to DialogueLines. This enables F683 (Lines property deprecation) by eliminating the remaining consumers of the backward-compatibility shim.

---

## Background

### Philosophy

F676で導入されたDialogueResult dual-property設計において、Linesは後方互換性のためのshimである。全コンシューマーがDialogueLinesに移行した後、F683でLinesを[Obsolete]マークする計画だが、YamlComExecutorとBatchProcessorが未移行のままである。本Featureでこれらを移行し、F683のブロッカーを解消する。

### Problem

- YamlComExecutor (line 229): `dialogue.Lines`を使用
- BatchProcessor (lines 63, 138): `yamlResult.Lines`を使用

これらが移行されない限り、F683でLinesを廃止予定マークできない。

### Goal

YamlComExecutorとBatchProcessorをDialogueLinesに移行し、F683の前提条件を満たす。

---

## Links

- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Created dual-property design)
- [feature-683.md](feature-683.md) - DialogueResult.Lines Obsolete Deprecation (Successor - blocked by this)

---

## Notes

- Created from F683 tech-investigation findings
- Scope: 2 files, 3 usage sites
- Low complexity migration (string join pattern)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: YamlComExecutor and BatchProcessor still use `dialogue.Lines` / `yamlResult.Lines` instead of DialogueLines
2. Why: These components were written BEFORE F676 introduced the dual-property design (DialogueLines + Lines shim)
3. Why: F676 focused on core pipeline changes (DialogueEntry → DialogueResult propagation) and only migrated the highest-impact consumer (HeadlessUI via F682)
4. Why: F676 used a backward-compatibility shim pattern (Lines computed from DialogueLines) to avoid breaking existing consumers, deferring migration to future features
5. Why: The F676 architectural decision prioritized non-breaking changes over immediate full migration - each consumer migration is a separate feature to maintain atomic change scope

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| F683 is BLOCKED because YamlComExecutor and BatchProcessor still use Lines | These components were not migrated during F676 because backward-compat shim allowed them to continue working |
| YamlComExecutor uses `dialogue.Lines` (line 229) | RenderKojo method was written before DialogueLines existed; string.Join pattern still works via Lines shim |
| BatchProcessor uses `yamlResult.Lines` (lines 63, 138) | BatchProcessor was updated for F677 to use RenderWithMetadata but still extracts text via Lines for comparison output |

### Conclusion

The root cause is **deferred migration from F676's backward-compatibility design**. F676 intentionally created a dual-property pattern (DialogueLines for new consumers, Lines as computed shim for old consumers) to enable incremental adoption. YamlComExecutor and BatchProcessor were left on the Lines API because they still function correctly - they just don't benefit from displayMode metadata. F698 completes the migration chain by converting these remaining consumers to DialogueLines, enabling F683 to mark Lines as [Obsolete].

**Critical analysis of each usage site**:

1. **YamlComExecutor.RenderKojo (line 229)**: `string.Join("\n", dialogue.Lines)`
   - Returns plain text to COM caller
   - displayMode metadata is NOT needed here - COM results are rendered text
   - Migration: Change to `dialogue.DialogueLines.Select(dl => dl.Text)` pattern for consistency

2. **BatchProcessor.ProcessAllAsync (line 63)**: `string.Join("\n", yamlResult.Lines)`
   - Extracts text for comparison with ERB output
   - ALREADY uses `yamlResult.DialogueLines` for displayMode comparison on line 65
   - Migration: Use `.DialogueLines.Select(dl => dl.Text)` for consistency

3. **BatchProcessor.ProcessAsync (line 138)**: `string.Join("\n", yamlResult.Lines)`
   - Same pattern as ProcessAllAsync
   - ALREADY uses `yamlResult.DialogueLines` for displayMode comparison on line 140
   - Migration: Use `.DialogueLines.Select(dl => dl.Text)` for consistency

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F676 | [DONE] | Predecessor | Created DialogueResult dual-property design. F698 completes F676's migration chain. |
| F677 | [DONE] | Related | KojoComparer displayMode awareness. Added RenderWithMetadata() and updated BatchProcessor to use DialogueLines for comparison, but kept Lines for text extraction. |
| F682 | [DONE] | Related | HeadlessUI migrated to DialogueLines. Consumer migration complete for HeadlessUI. |
| F683 | [BLOCKED] | Successor | DialogueResult.Lines Obsolete Deprecation. Blocked by F698 - cannot deprecate Lines until all consumers migrated. |

### Pattern Analysis

This is the **cleanup phase** of the incremental metadata propagation pattern:
- F676: Introduced dual-property design (DialogueLines + Lines shim)
- F677: Consumer awareness in KojoComparer (comparison layer - uses DialogueLines for displayModes)
- F682: Consumer awareness in HeadlessUI (rendering layer - fully migrated)
- **F698 (this)**: Final consumer migration (YamlComExecutor + BatchProcessor text extraction)
- F683: Deprecation of backward compatibility shim (cleanup phase - blocked by F698)

The pattern shows that F677 partially migrated BatchProcessor (uses DialogueLines for displayMode comparison) but kept Lines for text extraction. F698 completes this partial migration.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Simple pattern replacement: `dialogue.Lines` → `dialogue.DialogueLines.Select(dl => dl.Text)` |
| Scope is realistic | YES | 3 usage sites across 2 files. ~5 lines of change per site. Total ~15 lines. |
| No blocking constraints | YES | F676 is [DONE]. DialogueLines property already exists and is populated by KojoEngine and YamlRunner. |

**Verdict**: FEASIBLE

**Scope notes**:
- YamlComExecutor: 1 change site (RenderKojo line 229)
- BatchProcessor: 2 change sites (ProcessAllAsync line 63, ProcessAsync line 138)
- All changes are mechanical: `string.Join("\n", x.Lines)` → `string.Join("\n", x.DialogueLines.Select(dl => dl.Text))`
- No behavioral changes - text output is identical

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F676 | [DONE] | Created DialogueResult dual-property design with DialogueLines |
| Successor | F683 | [BLOCKED] | Waiting for F698 to migrate YamlComExecutor/BatchProcessor before Lines deprecation |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Linq | Runtime | None | `Select()` extension method already available in both files |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| COM execution callers | NONE | YamlComExecutor.Execute returns ComResult.Message (string). No API change. |
| KojoComparer CLI | NONE | BatchProcessor returns BatchReport. No API change. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Commands/Com/YamlComExecutor.cs | Update | Line 229: Change `dialogue.Lines` to `dialogue.DialogueLines.Select(dl => dl.Text)` |
| tools/KojoComparer/BatchProcessor.cs | Update | Lines 63, 138: Change `yamlResult.Lines` to `yamlResult.DialogueLines.Select(dl => dl.Text)` |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| DialogueLines property is IReadOnlyList<DialogueLine> | DialogueResult.cs | LOW - Requires LINQ Select() to extract text, but System.Linq already imported |
| YamlComExecutor returns string, not structured data | IComExecutor interface | NONE - RenderKojo internal method, continues returning string |
| BatchProcessor compares text (string) not structured data | Comparison algorithm | NONE - Text extraction pattern unchanged, just different source property |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Missing System.Linq using statement | Low | Low | Verify imports before edit; BatchProcessor already uses LINQ (line 65: `.Select(dl => dl.DisplayMode)`) |
| Behavioral difference between Lines and DialogueLines.Select(dl => dl.Text) | None | None | Lines is defined as `dialogueLines.Select(dl => dl.Text).ToList()` in DialogueResult.Create() - identical output |
| Test coverage gap | Low | Medium | YamlComExecutor and BatchProcessor have existing tests; change is transparent to test assertions |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "全コンシューマーがDialogueLinesに移行" (All consumers migrated to DialogueLines) | YamlComExecutor must use DialogueLines, not Lines | AC#1, AC#2 |
| "全コンシューマーがDialogueLinesに移行" (All consumers migrated to DialogueLines) | BatchProcessor must use DialogueLines, not Lines | AC#3, AC#4 |
| "F683のブロッカーを解消" (Remove F683 blocker) | Zero remaining Lines usage in migration scope | AC#2, AC#4 |
| "YamlComExecutorとBatchProcessorをDialogueLinesに移行" (Migrate YamlComExecutor and BatchProcessor) | Build succeeds after migration | AC#5 |
| "YamlComExecutorとBatchProcessorをDialogueLinesに移行" (Migrate YamlComExecutor and BatchProcessor) | Existing tests pass (behavioral equivalence) | AC#6, AC#7 |
| Implicit: Clean implementation | Zero technical debt markers | AC#8, AC#9 |

### Goal Coverage

| Goal Item | AC Coverage |
|-----------|-------------|
| YamlComExecutorとBatchProcessorをDialogueLinesに移行 | AC#1-4 (migration), AC#2,4 (zero Lines) |
| F683の前提条件を満たす | AC#2,4 (enables F683 to deprecate Lines) |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | YamlComExecutor uses DialogueLines | code | Grep(Era.Core/Commands/Com/YamlComExecutor.cs) | contains | `dialogue.DialogueLines.Select` | [x] |
| 2 | YamlComExecutor no Lines usage | code | Grep(path="Era.Core/Commands/Com/YamlComExecutor.cs", pattern="dialogue\\.Lines[^a-zA-Z]") | count_equals | 0 | [x] |
| 3 | BatchProcessor uses DialogueLines in both methods | code | Grep(path="tools/KojoComparer/BatchProcessor.cs", pattern="yamlResult\\.DialogueLines\\.Select\\(dl => dl\\.Text\\)") | count_equals | 2 | [x] |
| 4 | BatchProcessor no Lines text extraction | code | Grep(path="tools/KojoComparer/BatchProcessor.cs", pattern="yamlResult\\.Lines[^a-zA-Z]") | count_equals | 0 | [x] |
| 5 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 6 | YamlComExecutor tests pass | test | dotnet test Era.Core.Tests --filter YamlComExecutor | succeeds | - | [x] |
| 7 | KojoComparer tests pass | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 8 | YamlComExecutor zero technical debt | code | Grep(path="Era.Core/Commands/Com/YamlComExecutor.cs", pattern="TODO|FIXME|HACK") | count_equals | 0 | [x] |
| 9 | BatchProcessor zero technical debt | code | Grep(path="tools/KojoComparer/BatchProcessor.cs", pattern="TODO|FIXME|HACK") | count_equals | 0 | [x] |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This is a **simple property migration** following the pattern established by F676. The migration changes 3 usage sites from the deprecated `Lines` property to the new `DialogueLines` property with LINQ projection.

**Pattern transformation**:
```csharp
// Before (F676 backward-compat shim)
string.Join("\n", dialogue.Lines)
string.Join("\n", yamlResult.Lines)

// After (F698 DialogueLines migration)
string.Join("\n", dialogue.DialogueLines.Select(dl => dl.Text))
string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text))
```

**Rationale**:
1. **Text-only extraction**: All 3 sites extract plain text (no displayMode metadata needed)
2. **Behavioral equivalence**: `DialogueResult.Lines` is defined as `dialogueLines.Select(dl => dl.Text).ToList()` - the migration produces identical output
3. **LINQ availability**: System.Linq is implicitly available (C# 14 implicit usings). BatchProcessor already uses `.Select()` on line 65 and 140 for displayMode extraction
4. **Consistency with F676 design**: Matches the dual-property pattern intent - new consumers use DialogueLines

**Alternative considered and rejected**:
- Keep `Lines` property: Rejected - blocks F683 deprecation
- Add System.Linq using explicitly: Not needed - implicit usings already active in both projects

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Edit YamlComExecutor.cs line 229: Change `dialogue.Lines` to `dialogue.DialogueLines.Select(dl => dl.Text)` |
| 2 | Same edit as AC#1 - removes the only `dialogue.Lines` usage in file |
| 3 | Edit BatchProcessor.cs lines 63 and 138: Change `yamlResult.Lines` to `yamlResult.DialogueLines.Select(dl => dl.Text)` in both locations |
| 4 | Same edits as AC#3 - removes all `yamlResult.Lines` usage for text extraction |
| 5 | Migration is syntactically correct; build will succeed |
| 6 | Behavioral equivalence (text output identical) ensures tests pass |
| 7 | Behavioral equivalence (text output identical) ensures tests pass |
| 8 | Clean mechanical migration - no workarounds needed |
| 9 | Clean mechanical migration - no workarounds needed |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| LINQ projection pattern | A) `.Select(dl => dl.Text)`<br>B) Add helper method<br>C) Keep Lines property | A | Matches F676 design intent; no helper needed for 3 usage sites; option C blocks F683 |
| System.Linq import | A) Add explicit using<br>B) Rely on implicit usings | B | C# 14 implicit usings already active; BatchProcessor already uses .Select() on lines 65, 140 |
| Migration scope | A) YamlComExecutor + BatchProcessor only<br>B) All Lines consumers<br>C) Add metadata consumption | A | Scoped to unblock F683; other consumers (if any) are separate features; metadata not needed for text-only extraction |

### Implementation Details

**File 1: Era.Core/Commands/Com/YamlComExecutor.cs**
- Location: Line 229 in `RenderKojo` method
- Current: `onSuccess: dialogue => string.Join("\n", dialogue.Lines),`
- Updated: `onSuccess: dialogue => string.Join("\n", dialogue.DialogueLines.Select(dl => dl.Text)),`
- Context: Result.Match lambda returning string for COM output

**File 2: tools/KojoComparer/BatchProcessor.cs**
- Location 1: Line 63 in `ProcessAllAsync` method
  - Current: `var yamlOutput = string.Join("\n", yamlResult.Lines);`
  - Updated: `var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));`
  - Context: Text extraction for ERB-YAML comparison
- Location 2: Line 138 in `ProcessAsync` method
  - Current: `var yamlOutput = string.Join("\n", yamlResult.Lines);`
  - Updated: `var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));`
  - Context: Same pattern as ProcessAllAsync (single-file version)

**Edge cases verified**:
- Empty DialogueLines: `Select()` produces empty enumerable → `string.Join()` returns empty string (same as Lines behavior)
- Single line: `Select()` produces single element → `string.Join()` returns single string (same as Lines behavior)
- Null check: DialogueResult.DialogueLines is never null (initialized in constructor as empty list if null input)

### AC Details

**AC#1: YamlComExecutor uses DialogueLines**
- Verifies YamlComExecutor.RenderKojo (line 229) migrated from `dialogue.Lines` to `dialogue.DialogueLines.Select(dl => dl.Text)`
- Pattern: `dialogue.DialogueLines.Select` (partial match sufficient - exact LINQ syntax may vary)
- File: Era.Core/Commands/Com/YamlComExecutor.cs

**AC#2: YamlComExecutor no Lines usage**
- Ensures no remaining `dialogue.Lines` usage (the deprecated property)
- Regex `dialogue\.Lines[^a-zA-Z]` matches `dialogue.Lines)` or `dialogue.Lines,` but not `dialogue.DialogueLines`
- Must be zero matches for migration completeness

**AC#3: BatchProcessor uses DialogueLines in both methods**
- Verifies BatchProcessor.ProcessAllAsync (line 63) and ProcessAsync (line 138) both migrated to DialogueLines pattern
- count_equals 2 because both ProcessAllAsync and ProcessAsync should use this pattern
- Pattern: `yamlResult.DialogueLines.Select(dl => dl.Text)` for text extraction
- File: tools/KojoComparer/BatchProcessor.cs

**AC#4: BatchProcessor no Lines text extraction**
- Ensures no remaining `yamlResult.Lines` usage for text extraction
- Regex `yamlResult\.Lines[^a-zA-Z]` excludes `yamlResult.DialogueLines`
- Must be zero matches

**AC#5: Build succeeds**
- Verifies migration doesn't break compilation
- Era.Core build includes YamlComExecutor

**AC#6: YamlComExecutor tests pass**
- Verifies behavioral equivalence - existing tests should pass unchanged
- Filter: YamlComExecutor tests in Era.Core.Tests

**AC#7: KojoComparer tests pass**
- Verifies BatchProcessor behavioral equivalence
- Full test suite for tools/KojoComparer.Tests

**AC#8: YamlComExecutor zero technical debt**
- No TODO/FIXME/HACK markers in YamlComExecutor.cs
- Pattern: `TODO|FIXME|HACK`
- File: Era.Core/Commands/Com/YamlComExecutor.cs

**AC#9: BatchProcessor zero technical debt**
- No TODO/FIXME/HACK markers in BatchProcessor.cs
- Pattern: `TODO|FIXME|HACK`
- File: tools/KojoComparer/BatchProcessor.cs

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate YamlComExecutor.cs from Lines to DialogueLines | [x] |
| 2 | 3,4 | Migrate BatchProcessor.cs from Lines to DialogueLines | [x] |
| 3 | 5,6,7,8,9 | Verify build, tests, and zero technical debt | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Migration pattern from Technical Design | Updated source files |
| 2 | ac-tester | haiku | T3 | AC verification commands | Test results |

**Constraints** (from Technical Design):

1. Use LINQ `.Select(dl => dl.Text)` pattern for text extraction
2. Behavioral equivalence required - text output must be identical
3. Zero edits to DialogueResult.cs (property definition unchanged)

**Migration Pattern**:

```csharp
// Before (F676 backward-compat shim)
string.Join("\n", dialogue.Lines)
string.Join("\n", yamlResult.Lines)

// After (F698 DialogueLines migration)
string.Join("\n", dialogue.DialogueLines.Select(dl => dl.Text))
string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text))
```

**File Change Sites**:

1. **Era.Core/Commands/Com/YamlComExecutor.cs** (Line 229)
   - Current: `onSuccess: dialogue => string.Join("\n", dialogue.Lines),`
   - Updated: `onSuccess: dialogue => string.Join("\n", dialogue.DialogueLines.Select(dl => dl.Text)),`

2. **tools/KojoComparer/BatchProcessor.cs** (Line 63)
   - Current: `var yamlOutput = string.Join("\n", yamlResult.Lines);`
   - Updated: `var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));`

3. **tools/KojoComparer/BatchProcessor.cs** (Line 138)
   - Current: `var yamlOutput = string.Join("\n", yamlResult.Lines);`
   - Updated: `var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));`

**Pre-conditions**:

- F676 is [DONE] - DialogueResult.DialogueLines property exists
- System.Linq is implicitly available (C# 14 implicit usings active in both projects)
- Both files already import necessary dependencies for .Select() extension method

**Success Criteria**:

1. All 3 change sites migrated from `.Lines` to `.DialogueLines.Select(dl => dl.Text)`
2. Build succeeds (AC#5)
3. YamlComExecutor tests pass unchanged (AC#6)
4. KojoComparer tests pass unchanged (AC#7)
5. Zero remaining `.Lines` usage in scope files (AC#2,4)
6. Zero technical debt markers (AC#8,9)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

**LINQ Availability Verification**:

- Both Era.Core and tools/KojoComparer use C# 14 with implicit usings
- BatchProcessor.cs already uses `.Select()` on lines 65 and 140 for displayMode extraction
- No explicit `using System.Linq;` statement needed

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

## Review Notes
- [resolved-applied] Phase1-Uncertain iter2: count_equals matcher requires pattern specification but AC#4 only references the pattern implicitly from AC#3. Expected column should contain the pattern to count, or Method should use complex format Grep(path=..., pattern=...).
- [resolved-invalid] Phase1-Validation iter6: AC#5 duplicate - wrong location identified. AC Definition Table is correctly numbered.

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | Phase 1 | Initialized: [REVIEWED] → [WIP] |
| 2026-01-31 | Phase 2 | Investigation: All prerequisites verified |
| 2026-01-31 | Phase 3 | Skipped (behavioral equivalence migration) |
| 2026-01-31 | Phase 4 | Implementation: 3 migration sites completed |
| 2026-01-31 | Phase 6 | Verification: 9/9 ACs PASS |
| 2026-01-31 | Phase 7 | Post-Review: Quality OK, doc fix (F683 status) |
| 2026-01-31 | Phase 8 | Report: DEVIATION=0, All PASS |
