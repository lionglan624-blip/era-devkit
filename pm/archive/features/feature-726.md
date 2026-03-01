# Feature 726: PilotEquivalence ERB!=YAML Content Mismatch Investigation

## Status: [DONE]

## Scope Discipline

**In Scope**:
- Investigation of 7 PilotEquivalence test failures with ERB!=YAML content differences
- Root cause analysis using 5 Whys methodology
- Documentation of PRINTDATA/DATALIST random selection vs YAML concatenation pattern
- CALLNAME substitution difference analysis (ErbRunner vs YamlRunner)
- Feasibility assessment for resolution strategies
- Creation of follow-up feature for implementation

**Out of Scope**:
- Actual fix implementation (deferred to follow-up feature F746)
- Performance optimization of equivalence testing
- Migration of existing test data

**Scope Changes**:
None. Investigation scope is well-defined and contained.

## Type: infra

## Background

### Philosophy (Investigation Target)
F725 discovered during implementation that PilotEquivalence_* tests show 7 failures where ERB output and YAML output have different content (line count, dialogue text differ). These failures indicate real content mismatches between ERB and YAML implementations, not just formatting differences.

### Problem (Current Issue)
1. **7 PilotEquivalence test failures**: ERB output ≠ YAML output with substantial content differences
2. **Line count differences**: Tests show different number of output lines
3. **Dialogue text differences**: Actual character dialogue content differs between implementations
4. **F706 AC3a blocked**: Cannot pass ErbRunner/equivalence tests until content mismatches resolved
5. **Root cause unknown**: Investigation needed to determine if ERB or YAML implementation is correct

### Goal (What to Achieve)
1. **Investigate 7 failing PilotEquivalence cases** to identify root cause of content differences
2. **Determine correct implementation** (ERB vs YAML) for each mismatch
3. **Create targeted fix features** for identified issues
4. **Unblock F706 AC3a** (ErbRunner/equivalence tests pass)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F706 | [BLOCKED] | KojoComparer batch infrastructure (F726 is blocker FOR F706, not blocked BY F706) |
| Related | F725 | [DONE] | YamlRunner K{N} format support (discovered this issue) |

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### 5 Whys

1. **Why do PilotEquivalence tests fail with content differences?**
   Because ERB output has 6-7 lines while YAML output has 24-27 lines, and the dialogue text differs completely between implementations.

2. **Why does ERB produce fewer lines than YAML?**
   Because ERB uses PRINTDATA/DATALIST which **randomly selects ONE of 4 dialogue patterns** per TALENT branch, while YAML implementation contains ALL 4 patterns concatenated as a single entry's content.

3. **Why does YAML contain all 4 patterns concatenated?**
   Because the YAML file (`tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml`) was created by concatenating all DATALIST blocks from a single TALENT branch, rather than representing the random selection mechanism.

4. **Why wasn't the random selection mechanism preserved in YAML conversion?**
   Because YAML format (`entries:` with single `content:` block) doesn't model the PRINTDATA/DATALIST random selection. ErbToYaml conversion merged all patterns into one entry, losing the 1-of-N random selection semantic.

5. **Why is the YAML file structured this way?**
   The test YAML file predates proper format specification. It was created manually or by an early conversion tool that didn't understand PRINTDATA semantics. Production YAML files should model the random selection mechanism.

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| ERB has 7 lines, YAML has 26 lines (Yearning test) | YAML concatenates 4 DATALIST blocks; ERB selects 1 of 4 randomly |
| Dialogue text completely differs | ERB executed block #4 (last DATALIST); YAML has all 4 blocks concatenated |
| All 4 TALENT branches fail | Same PRINTDATA/DATALIST pattern exists in all branches |
| ERB output contains normalized names ("美鈴", "あなた") | ErbRunner executes actual game with character data loaded |
| YAML output contains placeholders ("%CALLNAME:人物_美鈴%", "%CALLNAME:MASTER%") | YamlRunner uses SimpleCharacterDataService returning placeholders |

### Conclusion

**The root cause is DUAL: test YAML file design AND CALLNAME substitution mismatch.**

**Issue 1 (PRIMARY): YAML Format Doesn't Model Random Selection**

The ERB code uses PRINTDATA/DATALIST to randomly select 1 of 4 dialogue patterns:

```erb
IF TALENT:恋人
    PRINTDATA
        DATALIST  ; Pattern 1: "んっ……そこ、気持ちいい……"
            ...7 lines...
        ENDLIST
        DATALIST  ; Pattern 2: "ふふっ……"
            ...7 lines...
        ENDLIST
        DATALIST  ; Pattern 3: "あっ……やぁ……"
            ...7 lines...
        ENDLIST
        DATALIST  ; Pattern 4: "……ん……"
            ...7 lines...
        ENDLIST
    ENDDATA
```

At runtime, PRINTDATA randomly selects ONE DATALIST block to output. The test YAML file concatenates ALL patterns:

```yaml
- id: "lover"
  content: |
    DATAFORM 「んっ……そこ、気持ちいい……」
    ...Pattern 1 lines...
    DATAFORM 「ふふっ……」
    ...Pattern 2 lines...
    DATAFORM 「あっ……やぁ……」
    ...Pattern 3 lines...
    DATAFORM 「……ん……」
    ...Pattern 4 lines...
```

**Issue 2 (SECONDARY): CALLNAME Substitution Difference**

ERB runs with full game data (character CSV loaded), so CALLNAME substitution works:
- `%CALLNAME:人物_美鈴%` → `美鈴`
- `%CALLNAME:MASTER%` → `あなた`

YAML uses `SimpleCharacterDataService` which returns placeholders:
- `%CALLNAME:人物_美鈴%` → `Character1` (or unreplaced)

**Resolution Strategy**:

1. **For equivalence testing**: Cannot compare random selection (ERB) vs deterministic concatenation (YAML). Either:
   - a. Fix YAML to model random selection (requires format extension)
   - b. Seed random generator to make ERB deterministic for testing
   - c. Check that ERB output is a subset of YAML content (any of 4 patterns)

2. **For CALLNAME**: Either normalize both outputs to placeholders, or inject real character data into YamlRunner.

**Scope Decision**: This Feature is **investigation only**. Fix implementations are deferred to follow-up features.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F706 | [BLOCKED] | Blocked by this | AC3a cannot pass until content mismatch resolved |
| F725 | [DONE] | Discovered issue | F725 created KojoBranchesParser but didn't address PRINTDATA semantic |
| F644 | [DONE] | Infrastructure | Created PilotEquivalenceTests that surface this issue |
| F675 | [DONE] | Format migration | entries: format doesn't model random selection |
| F727 | [DONE] | Character data | Fixed CALLNAME substitution for engine --unit mode, but YamlRunner issue remains |

### Pattern Analysis

This is a **format limitation pattern**: The YAML dialogue format (`entries:` with single `content:`) was designed for simple condition→dialogue mapping. It doesn't model runtime randomization (PRINTDATA/DATALIST). This is a known limitation that affects all COM kojo using PRINTDATA for dialogue variation.

The pattern appears 650+ times across kojo files (every COM uses PRINTDATA). Resolving this requires either:
1. YAML format extension to support randomization
2. Deterministic testing approach (seed-based or subset matching)

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Multiple viable approaches identified (subset matching, seeding, format extension) |
| Scope is realistic | YES | Investigation complete; fix features are well-scoped |
| No blocking constraints | YES | Fix options don't require engine changes |

**Verdict**: FEASIBLE

The investigation is complete. Follow-up fix features can proceed with clear understanding of the issue.

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| PRINTDATA runtime | Runtime | N/A | ERA engine feature - cannot modify |
| YamlDotNet | Runtime | Low | Already in use |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer.Tests/PilotEquivalenceTests.cs | HIGH | These tests expose the issue |
| tools/KojoComparer/YamlRunner.cs | MEDIUM | May need modification for subset matching |
| tools/KojoComparer/DiffEngine.cs | MEDIUM | May need modification for subset matching |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml | Review | Test file structure differs from production YAML |
| tools/KojoComparer.Tests/PilotEquivalenceTests.cs | Update (follow-up) | Tests need strategy adjustment |
| tools/KojoComparer/DiffEngine.cs | Update (follow-up) | May need subset matching mode |
| tools/KojoComparer/YamlRunner.cs | Update (follow-up) | SimpleCharacterDataService needs real data |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| PRINTDATA random selection is non-deterministic | ERA engine | HIGH - cannot predict which DATALIST is selected |
| YAML entries: format has single content: block | Era.Core design | MEDIUM - cannot model 1-of-N selection natively |
| Test YAML file is in tools/ErbToYaml.Tests/, not production | Project structure | LOW - test data, not production YAML |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Subset matching may have false positives | MEDIUM | MEDIUM | Require ALL lines of ERB output to exist in YAML, not just some |
| Random seed approach may be engine-invasive | MEDIUM | HIGH | Prefer subset matching approach over engine modification |
| CALLNAME normalization may mask other differences | LOW | LOW | Apply normalization consistently to both outputs |
| 650+ files affected by same pattern | HIGH | HIGH | This is expected; the fix feature will address systematically |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Investigate 7 failing PilotEquivalence cases" | Investigation findings documented with root cause analysis | AC#1, AC#2 |
| "Determine correct implementation (ERB vs YAML)" | Each mismatch analyzed with implementation verdict | AC#3 |
| "Create targeted fix features" | Follow-up features created with clear scope | AC#4, AC#5, AC#6 |
| "Unblock F706 AC3a" | Path to resolution documented and actionable | AC#7, AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 5 Whys analysis documented | file | Grep(Game/agents/feature-726.md) | contains | "5 Whys" | [x] |
| 2 | Root cause table present | file | Grep(Game/agents/feature-726.md) | contains | "Symptom vs Root Cause" | [x] |
| 3 | PRINTDATA/DATALIST random selection identified as root cause | file | Grep(Game/agents/feature-726.md) | contains | "PRINTDATA/DATALIST" | [x] |
| 4 | CALLNAME substitution difference documented | file | Grep(Game/agents/feature-726.md) | contains | "SimpleCharacterDataService" | [x] |
| 5 | Resolution strategies enumerated | file | Grep(Game/agents/feature-726.md) | contains | "subset matching" AND "seeding" AND "format extension" | [x] |
| 6 | Follow-up feature for YAML format/subset matching created | file | Glob(Game/agents/feature-746.md) | exists | F746 (YAML equivalence fix) | [x] |
| 7 | F706 blocker relationship documented | file | Grep(Game/agents/feature-726.md) | matches | "F706.*BLOCKED" | [x] |
| 8 | Feasibility verdict documented | file | Grep(Game/agents/feature-726.md) | matches | "Verdict.*FEASIBLE" | [x] |

**Note**: 8 ACs is within the typical range for infra features (8-15).

### AC Details

**AC#1: 5 Whys analysis documented**
- Verifies systematic root cause analysis was performed
- Method: Grep for "5 Whys" section header in feature file
- Expected: Section exists with numbered why questions

**AC#2: Root cause table present**
- Verifies structured symptom-to-cause mapping
- Method: Grep for "Symptom vs Root Cause" table header
- Expected: Table with symptom and root cause columns

**AC#3: PRINTDATA/DATALIST random selection identified as root cause**
- Verifies the primary root cause (random selection semantic) is documented
- Method: Grep for PRINTDATA/DATALIST mechanism explanation
- Expected: Explanation of how ERB randomly selects 1 of N patterns

**AC#4: CALLNAME substitution difference documented**
- Verifies the secondary root cause is identified
- Method: Grep for SimpleCharacterDataService reference
- Expected: Explanation of why YamlRunner returns placeholders vs real names

**AC#5: Resolution strategies enumerated**
- Verifies concrete fix approaches are proposed
- Method: Grep for all three strategy keywords
- Expected: "subset matching", "seeding", and "format extension" all documented

**AC#6: Follow-up feature for YAML format/subset matching created**
- Verifies investigation leads to actionable fix feature
- Method: Glob for new feature file targeting equivalence fix
- Expected: [DRAFT] or [PROPOSED] feature exists
- Note: This AC will be marked [B] if follow-up feature creation is deferred

**AC#7: F706 blocker relationship documented**
- Verifies the blocking relationship is clearly stated
- Method: Grep for F706 status reference
- Expected: F706 is documented as BLOCKED by this issue

**AC#8: Feasibility verdict documented**
- Verifies investigation concludes with clear actionability assessment
- Method: Grep for feasibility verdict
- Expected: "FEASIBLE" or "NOT FEASIBLE" conclusion with evidence

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This is an investigation-only feature. The technical approach focuses on **documenting findings and creating actionable follow-up features**, not implementing fixes.

**Design Philosophy**: Progressive disclosure of investigation results through structured documentation. Each AC validates a specific aspect of the investigation has been completed.

The investigation has identified two root causes:
1. **PRIMARY**: YAML format doesn't model PRINTDATA/DATALIST random selection (1-of-N semantic)
2. **SECONDARY**: CALLNAME substitution difference between ErbRunner (full game data) and YamlRunner (SimpleCharacterDataService placeholders)

Three resolution strategies are documented:
- **Subset matching**: ERB output must be subset of YAML content
- **Deterministic seeding**: Seed random generator for predictable DATALIST selection
- **Format extension**: Extend YAML schema to model random selection

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | **5 Whys analysis** - Already completed in "Root Cause Analysis" section. Verification: Grep for "5 Whys" header |
| 2 | **Root cause table** - Already completed in "Symptom vs Root Cause" table. Verification: Grep for table header |
| 3 | **PRINTDATA/DATALIST identified** - Already documented in "Issue 1 (PRIMARY)" section. Verification: Grep for "PRINTDATA/DATALIST" |
| 4 | **CALLNAME difference** - Already documented in "Issue 2 (SECONDARY)" section. Verification: Grep for "SimpleCharacterDataService" |
| 5 | **Resolution strategies** - Already documented in "Resolution Strategy" section. Verification: Grep with regex for all three strategies |
| 6 | **Follow-up feature created** - Create feature-746.md for YAML equivalence fix. Verification: Glob for new feature file |
| 7 | **F706 blocker documented** - Already documented in "Related Features" table and Dependencies section. Verification: Grep for "F706.*BLOCKED" |
| 8 | **Feasibility verdict** - Already documented in "Feasibility Assessment" section. Verification: Grep for "Verdict.*FEASIBLE" |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Investigation depth | Surface analysis vs 5 Whys | 5 Whys analysis | Ensures root cause identification, not just symptom description |
| Fix scope | Include fixes in F726 vs defer | Defer to follow-up feature | Investigation features should not mix analysis with implementation |
| Resolution strategy | Single approach vs enumerate all | Enumerate all three | Allows follow-up feature to select best approach with full context |
| Follow-up feature creation | Manual vs automated | Manual (AC#6) | Feature creation requires context and judgment, not automatable |

### Implementation Steps for AC#6

**AC#6 requires creating a follow-up feature file.** This is the only AC requiring new work (others validate existing content).

**Follow-up Feature Scope** (F746):
- **Type**: infra
- **Philosophy**: Resolve PilotEquivalence ERB!=YAML content mismatch using subset matching approach
- **Problem**: 7 PilotEquivalenceTests fail because ERB output (1 random DATALIST) != YAML content (all 4 DATALIST concatenated)
- **Goal**: Make equivalence tests pass by implementing subset matching: verify ERB output is subset of YAML content
- **Acceptance Criteria** (draft):
  - AC#1: Subset matching algorithm implemented in DiffEngine
  - AC#2: CALLNAME normalization applied to both ERB and YAML outputs
  - AC#3: All 7 PilotEquivalence tests pass with subset matching
  - AC#4: No false positives (ERB lines not in YAML content should still fail)

**Dependencies**:
- F726 must be [DONE] (investigation complete)
- F706 remains [BLOCKED] until F746 is [DONE]

### Data Structures

No new data structures required. This feature is documentation-only.

For the follow-up feature (F746), potential data structures:
```csharp
// DiffEngine.cs - Subset matching mode
public class SubsetMatchResult
{
    public bool IsSubset { get; set; }
    public List<string> UnmatchedLines { get; set; }
    public MatchStrategy Strategy { get; set; } // Exact, Subset, Normalized
}

public enum MatchStrategy
{
    Exact,        // Line-by-line exact match (current)
    Subset,       // ERB lines must exist in YAML (proposed)
    Normalized    // After CALLNAME normalization
}
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4,5,7,8 | Verify investigation documentation completeness (all sections present) | [x] |
| 2 | 6 | Create follow-up feature F746 for YAML equivalence fix implementation | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | ac-tester | haiku | T1 | AC table (verification commands) | All documentation ACs verified |
| 2 | philosophy-definer | haiku | T2 | F726 investigation findings | F746 feature file (DRAFT) |

**Constraints** (from Technical Design):
1. No code implementation required - investigation only
2. All documentation sections already completed by tech-investigator
3. Follow-up feature must reference F726 investigation findings
4. Follow-up feature scope limited to subset matching approach (not format extension or seeding)

**Pre-conditions**:
- F726 investigation sections exist (Root Cause Analysis, Feasibility Assessment, Technical Design)
- Investigation findings are documented and verified
- Next available feature ID determined (F728 or later)

**Success Criteria**:
- All 8 ACs pass verification
- F746 feature file exists with complete specification
- F746 properly references F726 as predecessor
- F706 blocker relationship documented in F746

**Rollback Plan**:

If issues arise during verification:
1. Report verification failures to user with specific AC numbers
2. Do NOT attempt to fix investigation findings (investigation is complete)
3. If AC#6 fails (follow-up feature creation), report to user for guidance on feature ID assignment

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| CALLNAME normalization for YamlRunner | Part of subset matching implementation | Follow-up feature for YAML equivalence fix | F746 (assigned by Task#2) | Task#2: Create follow-up feature |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Review Notes

- [resolved-applied] Phase1-Uncertain iter4: AC#6 references F729 for follow-up feature but F729 is [DONE] with different scope. Task#2 should determine correct next available feature ID per index-features.md "Next Feature number: 740"
- [resolved-applied] Phase1-Uncertain iter5: Feature ID conflict resolved - user selected F741 as new ID for YAML equivalence fix
- [resolved-applied] Phase3-ACValidation iter5: Feature ID conflict resolved - user selected F741 as new ID for YAML equivalence fix
- [resolved-applied] Phase6-FinalRefCheck iter5: F706 Blocker table corrected to accurately describe F726's scope (PRINTDATA/DATALIST content mismatch, not TALENT:恋人 undefined)

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-04 | Phase 4 | F746 作成（YAML Equivalence Subset Matching Fix）。F741はSession Extractor用に使用済みのため、新IDを割り当て |
| 2025-02-04 | FL Review | /fl 726 完了。[PROPOSED]→[REVIEWED]。Follow-up Feature IDをF741に確定 |
| 2025-02-04 | FL Review | F706 Blocker表を正確なスコープに修正（PRINTDATA/DATALIST content mismatch） |
| 2025-02-04 | Investigation | 「TALENT:恋人 undefined」調査実施。結論: 誤診。詳細は下記 |

### TALENT:恋人 調査結果 (2025-02-04)

**調査経緯**: F706 Blocker表の元記載「TALENT:恋人 undefined constant」が実際の問題か確認

**調査結果**:
- TALENT:恋人はTalent.csvに未定義（事実）。定義されているのは`恋慕`（index 3）のみ
- `恋人`・`思慕`もCSV未定義。ERBでは4段階分岐（恋人→恋慕→思慕→なし）で611箇所使用
- **しかしこれはF706のブロッカーではない** - F726調査で実際の原因はPRINTDATA/DATALIST random selectionと判明
- 元の記載は誤診。F706デバッグ中にテスト失敗原因を誤って帰属

**結論**:
- 別Feature作成: **不要**（誤診であり、実際のブロッカーはF726で追跡済み）
- 恋人/思慕のCSV追加: ゲームコンテンツ上必要であれば将来別Featureで検討可能（KojoComparer等価性テストとは無関係）

---

## Links
- [feature-644.md](feature-644.md) - PilotEquivalenceTests infrastructure (surfaced this issue)
- [feature-706.md](feature-706.md) - KojoComparer Full Equivalence Verification (blocked by this)
- [feature-725.md](feature-725.md) - YamlRunner K{N} format and branches parser support (discovered this issue)
- [feature-727.md](feature-727.md) - Character YAML data and CALLNAME substitution
- [feature-675.md](feature-675.md) - YAML Format Unification
- [feature-746.md](feature-746.md) - YAML Equivalence Subset Matching Fix (follow-up implementation)