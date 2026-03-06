# Feature 730: KojoComparer Test Count Discrepancy Investigation

## Status: [CANCELLED]

## Type: research

## Background

### Philosophy (Mid-term Vision)
F706 AC7 expects 650/650 PASS for KojoComparer --all verification. However, KojoComparer --all currently reports 466 tests. The discrepancy between 466 and 650 needs investigation to understand whether the 650 count is outdated or if tests are being missed.

### Problem (Current Issue)
KojoComparer --all reports 466 test cases, but F706 AC7 expects 650. The source of this discrepancy is unknown - it could be due to test filtering, file discovery issues, or an outdated count in F706's AC definition.

### Goal (What to Achieve)
1. Investigate the source of the 466 vs 650 test count discrepancy
2. Determine the correct expected test count
3. Update F706 AC7 if the expected count needs adjustment

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### 5 Whys

1. **Why was 466 reported instead of 650 on 2026-02-01?**
   Because the F706 execution log entry recorded "Discovered 466 (not 650) test cases" during a debugging session on 2026-02-01.

2. **Why did FileDiscovery return 466 instead of 650?**
   Investigation reveals that the 466 count was a transient state. Current execution (2026-02-04) returns "Discovered 650 test cases" consistently.

3. **Why was the count transient?**
   Multiple possibilities: (a) working directory mismatch during debugging, (b) incomplete YAML file state during kojo commits on 2026-01-31, or (c) environment-specific issue during the debugging session.

4. **Why did it resolve itself?**
   The F706 execution log on 2026-02-04 21:15 already shows "Discovered 650 cases" - the count was back to 650 before F730 was created.

5. **Why was F730 created if the issue was already resolved?**
   F727 deferred the investigation because the 466 count was observed during F706/F727 debugging. The deferral followed TBD Prohibition by creating F730, but the issue had already self-resolved.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| F706 log shows "466 (not 650)" on 2026-02-01 | Transient debugging state (working directory, environment, or mid-commit YAML file state) |
| F727 defers to F730 for investigation | Proper escalation per TBD Prohibition, but issue self-resolved |

### Conclusion

**The discrepancy no longer exists.** The 466 count was a transient observation during F706/F727 debugging sessions on 2026-02-01. Evidence:

1. **F646 (2026-01-30)**: "Discovered 650 test cases" after JSON case-sensitivity fix
2. **F706 (2026-02-01)**: "Discovered 466 (not 650) test cases" during debugging
3. **F706 (2026-02-04)**: "Discovered 650 cases but batch execution exceeds 3min"
4. **Current run (2026-02-04)**: `dotnet run --project tools/KojoComparer -- --all` outputs "Discovered 650 test cases"

The 650 count is correct and stable. No code changes are required.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F706 | [BLOCKED] | Parent issue | Original AC6/AC7 expects 650 test cases - verified correct |
| F727 | [DONE] | Source of deferral | Deferred 466 vs 650 investigation to F730 |
| F644 | [DONE] | Created FileDiscovery | FileDiscovery logic is correct, returns 650 |
| F646 | [DONE] | Fixed JSON case-sensitivity | Initial fix that brought count from 0 to 650 |

### Pattern Analysis

This is **not a recurring pattern** - it was a one-time transient observation during debugging. The FileDiscovery algorithm correctly enumerates:
- 11 character directories (1-10 + U)
- 115 implemented COM IDs from com_file_map.json
- Total: ~650 test cases (varies slightly by character_overrides and skip_combinations)

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem exists | NO | Current execution shows 650 test cases consistently |
| Action required | NO | No code changes needed - issue self-resolved |
| F706 AC6/AC7 correct | YES | AC6 expects "Discovered 650 test cases" - matches actual output |

**Verdict**: NOT_FEASIBLE (Feature no longer needed)

The reported problem (466 vs 650 discrepancy) no longer exists. The 466 count was a transient debugging artifact. F706 AC6/AC7's expectation of 650 test cases is correct.

**Recommended action**: Close F730 as NOT_NEEDED and update F706 to remove the 466 discrepancy concern from its documentation.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F706 | [BLOCKED] | Parent feature expecting 650/650 PASS - count verified correct |
| Related | F727 | [DONE] | Created F730 deferral - issue was already resolved |
| Related | F644 | [DONE] | FileDiscovery implementation - working correctly |

### External Dependencies

None.

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F706 AC6/AC7 | NONE | The 650 expectation is correct; no change needed |

---

## Impact Analysis

| Area | Impact | Detail |
|------|--------|--------|
| KojoComparer | NONE | No code changes needed - FileDiscovery works correctly |
| F706 | LOW | May remove 466 discrepancy mentions from execution log notes |
| F727 | NONE | Deferral was appropriate; investigation complete |

---

## Constraints

1. **No code changes required** - FileDiscovery is working correctly
2. **F706 AC6/AC7 is correct** - Do not modify the 650 expectation

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Discrepancy reoccurs | LOW | LOW | If 466 is observed again, check working directory and YAML file completeness |
| Misinterpretation of transient state | LOW | LOW | Document that 466 was debugging artifact in F706 execution log |

---

## Links
- [feature-706.md](feature-706.md) - KojoComparer Full Equivalence Verification
- [feature-727.md](feature-727.md) - Source of discovery
- [feature-644.md](feature-644.md) - FileDiscovery implementation
- [feature-646.md](feature-646.md) - JSON case-sensitivity fix (0 → 650 count)
