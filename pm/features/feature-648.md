# Feature 648: PathAnalyzer Pattern Extension

## Status: [CANCELLED]

> **Cancellation Reason (2026-01-28)**: SUPERSEDED by F639 commit 7bd308e.
> F639's investigation phase independently discovered and fixed the PathAnalyzer limitation by adding FallbackPattern.
> This feature was never implemented as a standalone feature - the problem was already solved.

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

## Type: infra

## Created: 2026-01-28

---

## Summary

Extend PathAnalyzer to handle non-KOJO_ prefixed files (NTR口上, SexHara休憩中口上, WC系口上) that currently cause ArgumentException during batch conversion.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format with robust handling of all file naming patterns.

### Problem (Current Issue)

PathAnalyzer in the F634 Batch Conversion Tool expects all kojo files to follow the `KOJO_K{N}_` prefix pattern (e.g., `KOJO_K1_愛撫.ERB`). However, several files across multiple character directories use different naming patterns:
- `NTR口上.ERB`
- `NTR口上_お持ち帰り.ERB`
- `SexHara休憩中口上.ERB`
- `WC系口上.ERB`

When PathAnalyzer.Extract() encounters these files, it throws ArgumentException because the pattern does not match expected format. While F634's continue-on-error behavior handles these gracefully, these files contain convertible PRINTDATA/DATALIST content that should be processed.

**Evidence from F642 execution**:
- 13 PathAnalyzer failures across 4 character directories (7_子悪魔, 8_チルノ, 9_大妖精, 10_魔理沙)
- All failures are non-KOJO_ prefixed files: NTR口上, SexHara休憩中口上, WC系口上
- Files contain valid PRINTDATA/DATALIST blocks but are skipped due to pattern matching failure

### Goal (What to Achieve)

1. Extend PathAnalyzer pattern matching to recognize non-KOJO_ prefixed file patterns
2. Define situation extraction rules for edge-case filenames
3. Process all 13 currently-skipped files successfully in F642 and sibling conversion features
4. Maintain backward compatibility with existing KOJO_ prefix pattern

---

## Links

- [feature-634.md](feature-634.md) - Batch Conversion Tool (PathAnalyzer implementation)
- [feature-642.md](feature-642.md) - Secondary Characters Conversion (origin feature)
- [feature-636.md](feature-636.md) - Meiling Kojo Conversion (potential affected sibling)
- [feature-638.md](feature-638.md) - Patchouli Kojo Conversion (potential affected sibling)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 13 ERB files across 4 character directories (7_子悪魔, 8_チルノ, 9_大妖精, 10_魔理沙) threw ArgumentException during batch conversion
2. Why: PathAnalyzer.Extract() could not parse filenames that lack the `KOJO_` prefix pattern
3. Why: PathAnalyzer was originally designed (F634) with only the `KOJO_` prefix regex pattern
4. Why: F634 focused on the majority case (KOJO_ prefixed files) without handling edge-case naming patterns
5. Why: The kojo file naming convention in the source repository is not uniform - some files use descriptive prefixes (NTR口上, SexHara休憩中口上, WC系口上) instead of the standard KOJO_ prefix

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 13 files failed with "Path does not match expected pattern" error | **WAS**: PathAnalyzer only had KOJO_ prefix pattern. **FIXED in F639 commit 7bd308e** |
| Batch conversion reported 13 failures across 4 directories | **WAS**: F634 design assumed uniform KOJO_ naming. **FIXED in F639 commit 7bd308e** |
| F636, F638, F639, F642 reported PathAnalyzer failures | These features ran BEFORE the fix was implemented. Need to re-run after F639 commit. |

### Conclusion

**CRITICAL FINDING (2026-01-28 09:19 investigation)**:

The root cause WAS that PathAnalyzer.cs only had the KOJO_ prefix pattern. However, this was **ALREADY FIXED** in commit `7bd308e` (2026-01-28 09:05:56) as part of F639:

```
feat(F639): Extend PathAnalyzer and FileConverter for non-KOJO prefix files
```

The fix added a FallbackPattern that matches any filename in a numbered directory:
```csharp
private static readonly Regex FallbackPattern = new Regex(
    @"(?:^|[\\/])(\d+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$",
    RegexOptions.Compiled
);
```

**Verification**: Testing the current PathAnalyzer patterns confirms they work correctly:
- Pure backslash path `4_咲夜\NTR口上_シナリオ8.ERB`: **MATCHES**
- Mixed separator path `Game/ERB/口上/2_小悪魔\NTR口上.ERB`: **MATCHES**
- Pure forward slash path `Game/ERB/口上/2_小悪魔/NTR口上.ERB`: **MATCHES**

**Timeline**:
1. **2026-01-27 21:02**: F634 batch tool completed (KOJO_ only pattern)
2. **2026-01-28 07:08**: F636 ran and failed on 4 non-KOJO files
3. **2026-01-28**: F637, F638, F642 also ran and reported failures
4. **2026-01-28 09:05**: **FIX APPLIED** in commit 7bd308e (F639 implementation)
5. **2026-01-28 09:19**: This investigation confirmed fix works

**Verdict**: F648 is **SUPERSEDED** by F639 commit 7bd308e. The feature objective has been achieved.
- F636, F638, F639, F642 should be re-tested - they may now pass completely
- F648 can be marked as [DONE] or consolidated with F639
- F673 (duplicate feature) should also be marked as superseded

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Original implementation | PathAnalyzer.cs created with KOJO_ only pattern |
| F639 | [DONE]/[WIP] | **FIX APPLIED** | Commit 7bd308e added FallbackPattern for non-KOJO files (2026-01-28 09:05) |
| F642 | [DONE] | Origin discovery | Secondary Characters Conversion that reported 13 "failures" - needs re-run |
| F636 | [BLOCKED] | Affected sibling | Meiling conversion - 4 non-KOJO files, **should unblock after fix** |
| F638 | [BLOCKED] | Affected sibling | Patchouli conversion - 4 non-KOJO files, **should unblock after fix** |
| F640 | [DRAFT] | Affected sibling | Remilia conversion |
| F643 | [DRAFT] | Affected sibling | Generic conversion |
| F673 | [DRAFT] | **DUPLICATE - SUPERSEDED** | Same scope as F648, both superseded by F639 commit 7bd308e |

### Pattern Analysis

**Resolution**: F639 commit 7bd308e (2026-01-28 09:05:56) implemented the FallbackPattern that handles all non-KOJO filenames. The fix was applied during F639 Sakuya conversion work.

**Current state**:
- PathAnalyzer.cs now has TWO patterns:
  1. Primary pattern: `(?:^|[\\/])(\d+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$` (KOJO_ prefix)
  2. FallbackPattern: `(?:^|[\\/])(\d+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$` (ANY filename)
- Tests confirm both patterns work: `Test_Extract_NtrKojoPrefix`, `Test_Extract_SexHaraPrefix`, `Test_Extract_WcKojoPrefix` all pass

**Features that need re-testing**:
- F636 (Meiling) - ran before fix, AC#1 and AC#11 failed
- F638 (Patchouli) - ran before fix, 4 PathAnalyzer failures
- F642 (Secondary) - ran before fix, 13 PathAnalyzer failures

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | **ALREADY SOLVED** | FallbackPattern added in F639 commit 7bd308e (2026-01-28 09:05) |
| Scope is realistic | **SUPERSEDED** | F639 already implemented the required fix |
| No blocking constraints | YES | No further work needed |

**Verdict**: SUPERSEDED

**Investigation Timeline**:
1. Original F642 execution log showed "Path does not match expected pattern" errors for 13 files
2. Initial analysis assumed PathAnalyzer was missing the fallback pattern
3. Deep investigation discovered F639 commit 7bd308e (09:05) added the FallbackPattern
4. Testing confirms the current PathAnalyzer correctly matches all path formats including mixed separators
5. The F636/F638/F642 failures occurred BEFORE the fix was applied

**Recommended actions**:
1. Mark F648 as [DONE] - objective achieved via F639
2. Mark F673 as [DONE] or SUPERSEDED - duplicate of F648
3. Unblock F636, F638 - the blocker (PathAnalyzer limitation) is resolved
4. Re-run F636, F638, F642 batch conversions to verify fix

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool - PathAnalyzer original implementation |
| **Resolution** | F639 | [DONE] | **Fixed the issue** - commit 7bd308e added FallbackPattern |
| Duplicate | F673 | [DRAFT] | Same scope - should be marked SUPERSEDED |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| tools/ErbToYaml/PathAnalyzer.cs | Code | **RESOLVED** | FallbackPattern added in commit 7bd308e |
| tools/ErbToYaml.Tests/PathAnalyzerTests.cs | Test | None | Tests at lines 327-373 confirm NTR, SexHara, WC patterns work |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Action Needed |
|----------|--------------|---------------|
| F636 Meiling Kojo Conversion | HIGH | **UNBLOCK** - PathAnalyzer issue resolved |
| F638 Patchouli Kojo Conversion | HIGH | **UNBLOCK** - PathAnalyzer issue resolved |
| F639 Sakuya Kojo Conversion | MEDIUM | Already applied the fix |
| F642 Secondary Characters | LOW | Already [DONE] - consider re-run to verify all files now pass |
| F640, F641, F643 | MEDIUM | Should proceed without PathAnalyzer issues |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/PathAnalyzer.cs | **ALREADY CHANGED** | FallbackPattern added in commit 7bd308e (F639) |
| tools/ErbToYaml.Tests/PathAnalyzerTests.cs | **ALREADY CHANGED** | Tests for non-KOJO patterns added and passing |
| Game/agents/feature-636.md | **NEEDS UPDATE** | Remove F648 from blockers, update to [REVIEWED] status |
| Game/agents/feature-638.md | **NEEDS UPDATE** | Remove F648 from blockers, update to [REVIEWED] status |
| Game/agents/feature-673.md | **NEEDS UPDATE** | Mark as SUPERSEDED by F639 commit 7bd308e |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PathAnalyzer FallbackPattern requires numbered directory prefix | Regex pattern line 21 | **ACCEPTED** - All character directories follow N_CharacterName format |
| Non-KOJO files may have no PRINTDATA/DATALIST content | ERB file structure | **NOT PATHANALYZER** - Separate concern; files are correctly parsed but may have no convertible content |
| Mixed path separators (Windows runtime) | Directory.GetFiles on Windows | **HANDLED** - FallbackPattern regex supports both / and \ separators |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Re-run of F636/F638/F642 reveals other issues | Low | Medium | The PathAnalyzer fix is verified; other issues would be separate concerns |
| Some non-KOJO files have no convertible content | Certain | Low | Expected behavior - these files are function-based ERB, not dialogue data |
| F673 duplicate creates confusion | High | Low | Mark F673 as SUPERSEDED in this investigation |

---

## Acceptance Criteria

<!-- To be completed via /fc command after root cause verification -->

---

## Tasks

<!-- To be completed via /fc command after root cause verification -->

---

## Review Notes

- [2026-01-28 09:19] tech-investigator: Deep investigation completed. **FINDING: Issue was ALREADY FIXED in F639 commit 7bd308e (09:05:56)**. The FallbackPattern was added to PathAnalyzer.cs as part of F639 Sakuya conversion. F636/F638/F642 failures occurred BEFORE this fix was applied.
- [2026-01-28 09:19] tech-investigator: Verified current PathAnalyzer patterns correctly match all path formats including mixed separators (`Game/ERB/口上/2_小悪魔\NTR口上.ERB`). 19 PathAnalyzer tests pass.
- [2026-01-28 09:19] tech-investigator: **RECOMMENDATION**: Mark F648 as [DONE] (objective achieved via F639). Mark F673 as SUPERSEDED. Unblock F636, F638. Re-run F636/F638/F642 to verify complete conversion success.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-28 | CREATE | implementer | Feature file creation | [DRAFT] status |
| 2026-01-28 09:19 | INVESTIGATE | tech-investigator | Deep investigation of PathAnalyzer regex patterns | FallbackPattern exists and works correctly |
| 2026-01-28 09:19 | INVESTIGATE | tech-investigator | Traced git history: 7bd308e added FallbackPattern | Fix applied at 09:05:56 as part of F639 |
| 2026-01-28 09:19 | VERIFY | tech-investigator | Tested regex patterns with mixed separators | All paths match correctly: backslash, forward slash, and mixed |
| 2026-01-28 09:19 | CONCLUDE | tech-investigator | F648 objective achieved via F639 commit | SUPERSEDED - recommend [DONE] status |
