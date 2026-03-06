# Feature 673: PathAnalyzer Enhancement for Non-KOJO Prefix Files

## Status: [CANCELLED]

> **Cancellation Reason (2026-01-28)**: SUPERSEDED by F639 commit 7bd308e.
> F639's investigation phase independently discovered and fixed the PathAnalyzer limitation by adding FallbackPattern.
> This feature was never implemented as a standalone feature - the problem was already solved before /fc completion.
> See also: F648 (duplicate, also superseded).

## Type: erb

## Created: 2026-01-28

---

## Summary

Enhance PathAnalyzer in tools/ErbToYaml to support ERB files without the KOJO_ prefix (e.g., NTR口上.ERB, SexHara休憩中口上.ERB, WC系口上.ERB), enabling batch conversion of all kojo files including non-standard naming patterns.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

F636 pilot conversion revealed that PathAnalyzer requires `KOJO_` prefix in filenames (pattern: `N_CharacterName/KOJO_Situation.ERB`). 4 files in 1_美鈴/ failed conversion:
- NTR口上.ERB
- NTR口上_お持ち帰り.ERB
- SexHara休憩中口上.ERB
- WC系口上.ERB

This naming convention limitation affects all character directories (F636-F643), not just 美鈴.

### Goal (What to Achieve)

1. Extend PathAnalyzer to handle non-KOJO_ prefix ERB files
2. Re-convert the 4 failed files from F636
3. Ensure F637-F643 sibling batches can convert all files without naming-related failures

---

## Links

- [feature-636.md](feature-636.md) - Meiling Kojo Conversion (source of this follow-up)
- [feature-634.md](feature-634.md) - Batch Conversion Tool (PathAnalyzer implementation)
- [feature-637.md](feature-637.md) - F637-F643 sibling batches (consumers)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: F636 pilot conversion failed on 4 files (NTR口上.ERB, NTR口上_お持ち帰り.ERB, SexHara休憩中口上.ERB, WC系口上.ERB)
2. Why: PathAnalyzer.Extract() threw ArgumentException because the files did not match the KOJO_ prefix pattern
3. Why: F634 PathAnalyzer implementation only included the pattern `N_CharacterName/KOJO_Situation.ERB` (PathPattern regex)
4. Why: F634 was designed based on the most common naming convention; the non-KOJO files were not considered in the original scope
5. Why: The 117 kojo files have heterogeneous naming conventions, but only the KOJO_ pattern was analyzed during F634 design

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 4 files failed F636 conversion with "Path does not match expected pattern" | PathAnalyzer only had KOJO_ prefix regex pattern in F634 |
| F636 status was [BLOCKED] waiting on F673 | Feature dependency tracking assumes F673 is needed to fix PathAnalyzer |

### Conclusion

**CRITICAL FINDING: The issue has ALREADY been fixed.**

Investigation reveals that commit `7bd308e` (2026-01-28, feat(F639)) **already extended PathAnalyzer** with a fallback pattern for non-KOJO prefix files:

```csharp
// Fallback pattern for non-KOJO files: N_CharacterName/Filename.ERB
private static readonly Regex FallbackPattern = new Regex(
    @"(?:^|[\\/])(\d+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$",
    RegexOptions.Compiled
);
```

**Evidence**:
1. All 19 PathAnalyzer tests pass (including 4 tests specifically for NTR口上, SexHara, WC系 patterns)
2. Batch conversion on `Game/ERB/口上/1_美鈴/` now succeeds: `Total: 11, Success: 11, Failed: 0`
3. All 11 ERB files (including the previously-failing 4) convert to 80 YAML files

**Timeline**:
- 2026-01-27: F634 created PathAnalyzer with KOJO_ pattern only
- 2026-01-27: F636 ran and failed on 4 non-KOJO files
- 2026-01-28: F639 investigation added fallback pattern to PathAnalyzer
- 2026-01-28: F673 created as follow-up (now obsolete)

**Recommendation**: F673 should be **CANCELLED** as the problem is already solved. F636 and F639 blockers on F673 should be removed.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor | Original PathAnalyzer with KOJO_ pattern only |
| F636 | [BLOCKED] | Consumer | Meiling conversion - blocked on F673 but fix already exists |
| F639 | [BLOCKED] | Fix provider | Sakuya conversion - **this feature already implemented the fix** via commit 7bd308e |
| F637-F643 | [DRAFT]/[BLOCKED] | Consumers | Sibling character conversion batches - all unblocked by F639's fix |

### Pattern Analysis

This is a **documentation lag issue**: F673 was created to track the PathAnalyzer limitation, but F639's investigation phase independently discovered and fixed the same issue. The feature tracking system was not updated to reflect this.

**Prevention**: When a feature investigation discovers and fixes a predecessor issue, update related feature statuses immediately.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | N/A | Problem ALREADY SOLVED in F639 |
| Scope is realistic | N/A | No implementation needed |
| No blocking constraints | N/A | Fallback pattern exists and works |

**Verdict**: NOT_FEASIBLE (as a standalone feature) - **RECOMMEND CANCELLATION**

The work described in F673's Goals has already been completed:
1. ✅ PathAnalyzer extended to handle non-KOJO_ prefix ERB files (FallbackPattern regex)
2. ✅ Conversion of previously-failed files now succeeds
3. ✅ F637-F643 can convert all files without naming-related failures (verified via batch test)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool containing PathAnalyzer |
| Related | F636 | [BLOCKED] | Pilot conversion that discovered this limitation - **blocker should be removed** |
| Related | F639 | [BLOCKED] | Sakuya conversion - **already implemented the fix** |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| PathAnalyzer.cs | Source | N/A | Already updated with FallbackPattern in F639 |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F636 Meiling Kojo Conversion | HIGH | Blocked on F673 - should be unblocked |
| F637-F643 character conversions | MEDIUM | Some may be blocked on F673 - should be unblocked |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/PathAnalyzer.cs | Already Modified | FallbackPattern added by F639 (commit 7bd308e) |
| tools/ErbToYaml.Tests/PathAnalyzerTests.cs | Already Modified | 4 tests added for NTR, SexHara, WC patterns |
| Game/agents/feature-673.md | Update | Mark as CANCELLED, document already-fixed status |
| Game/agents/feature-636.md | Update | Remove F673 from blockers |
| Game/agents/index-features.md | Update | Update F636 status and dependencies |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| None | - | Problem already solved |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Documentation drift if F673 not cancelled | High | Medium | Cancel F673 and update blockers immediately |
| F636/F639 remain blocked unnecessarily | High | High | Remove F673 from their Dependencies tables |
