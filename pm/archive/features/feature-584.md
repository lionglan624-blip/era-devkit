# Feature 584: Testing SKILL.md AC Method Column Format Standardization

## Status: [CANCELLED]

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

## Background

### Philosophy (Mid-term Vision)
Documentation consistency ensures Skills serve as reliable SSOT for subagents. Method column format ambiguity undermines AC verification accuracy and wastes review cycles on format confusion.

### Problem (Current Issue)
testing SKILL.md shows inconsistent AC Method column formats:
- AC Type Requirements table (line 352) shows `Grep(path)` format
- Method Column Usage examples (lines 56-61) show different format: `Grep("pattern", "file")`

This inconsistency confuses feature writers about proper AC Method column format, leading to FL review iterations on formatting rather than content.

### Goal (What to Achieve)
Standardize testing SKILL.md to use single consistent format for AC Method column throughout the document, eliminating format ambiguity for feature writers.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AC Type Requirements table standardized | file | Grep | contains | "Grep\\(.*pattern.*file" | [ ] |
| 2 | Method Column Usage examples consistent | file | Grep | contains | "Recommended Method.*Grep\\(" | [ ] |
| 3 | No Grep(path) format remaining | file | Grep | not_contains | "Grep\\(path\\)" | [ ] |
| 4 | File existence examples use Glob format | file | Grep | contains | "exists.*Glob\\(" | [ ] |
| 5 | Content search examples use Grep format | file | Grep | contains | "contains.*Grep\\(" | [ ] |
| 6 | All skill references valid | output | Skill | succeeds | - | [ ] |
| 7 | Documentation consistency verified | output | Bash | succeeds | - | [ ] |

### AC Details

**AC#1**: AC Type Requirements table format consistency
- Test: Grep("Grep\\(.*pattern.*file", ".claude/skills/testing/SKILL.md")
- Expected: Table uses detailed format matching Method Column Usage examples

**AC#2**: Method Column Usage examples format verification
- Test: Grep("Recommended Method.*Grep\\(", ".claude/skills/testing/SKILL.md")
- Expected: Examples maintain detailed format with quoted parameters

**AC#3**: Legacy format elimination
- Test: Grep("Grep\\(path\\)", ".claude/skills/testing/SKILL.md") | count
- Expected: 0 matches (no legacy `Grep(path)` format remaining)

**AC#4**: File existence method consistency
- Test: Grep("exists.*Glob\\(", ".claude/skills/testing/SKILL.md")
- Expected: All file existence examples use proper Glob method format

**AC#5**: Content search method consistency
- Test: Grep("contains.*Grep\\(", ".claude/skills/testing/SKILL.md")
- Expected: All content search examples use proper Grep method format

**AC#6**: Reference validation
- Test: Skill(reference-checker) execution
- Expected: All internal links and skill references resolve correctly

**AC#7**: SSOT consistency verification
- Test: Bash(/audit) command execution
- Expected: No SSOT violations or documentation inconsistencies

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3 | Update AC Type Requirements table to match Method Column Usage format | [ ] |
| 2 | 2,4,5 | Verify Method Column Usage examples remain consistent | [ ] |
| 3 | 6,7 | Validate references and documentation consistency | [ ] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Action | Verification |
|-------|--------|-------------|
| 1 | Read current AC Type Requirements table format | Identify specific format inconsistencies |
| 2 | Update table to match Method Column Usage examples | Use detailed format consistently |
| 3 | Verify no `Grep(path)` format remains | Grep verification of format elimination |
| 4 | Run reference-checker and /audit | Ensure no broken links or SSOT violations |

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/skills/testing/SKILL.md | AC Type Requirements table format | All agents referencing testing skill see consistent format |
| Feature creation process | Standardized AC Method format | Reduced FL review iterations on format issues |
| SSOT consistency | Unified documentation format | Improved subagent reliability |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F364 | [DONE] | SSOT update patterns reference |

## Review Notes
### FL Review Conclusion (2026-01-22)
**Decision**: Feature CANCELLED - Problem misdiagnosis confirmed

**Findings from multiple independent reviewers**:
1. AC Type Requirements table (line 352) uses `Grep(path)` as METHOD TYPE notation (what to use)
2. Method Column Usage (lines 56-61) shows ACTUAL CALL SYNTAX examples (how to write)
3. These serve **different documentation purposes** - reference vs how-to
4. Unification would worsen documentation by removing concise type notation
5. No evidence of FL failures caused by alleged "format confusion"

**User Decision**: Option A - Withdraw feature as unnecessary

### Historical Pending Items (resolved by cancellation)
- [resolved-cancelled] Phase1-4: All pending items superseded by feature cancellation

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| (none) | - | - | - |

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links
- [index-features.md](index-features.md)
- [.claude/skills/testing/SKILL.md](../../.claude/skills/testing/SKILL.md) - Target file
- [feature-364.md](feature-364.md) - Related: SSOT update patterns reference