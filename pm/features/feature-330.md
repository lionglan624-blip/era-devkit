# Feature 330: Checkbox Responsibility Definition

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ワークフローの欠陥は発見時に報告し、調査→修正の流れで恒久対策する

### Problem (Current Issue)
AC/Task checkbox update timing and responsible agent undefined in workflow.

**Evidence**:
- F319: Manual checkbox updates occurred at various times during execution
- F324: All checkboxes updated in batch at end
- No consistent pattern documented

**Root Cause**: do.md does not specify:
- Which agent updates checkboxes (implementer? ac-tester? finalizer? opus orchestrator?)
- When checkboxes should be updated (after each task? after AC verification? at finalize?)
- What happens if checkboxes are not updated (does finalizer block? does commit proceed?)

**Impact**:
- Inconsistent feature status tracking
- Unclear completion signals for workflow orchestration
- Potential for partial completion to be mistaken as full completion

**Note**: ac-tester.md already has partial implementation (lines 120-131 "Task ステータス更新" section). This feature consolidates and extends existing rules to all relevant agents.

### Goal (What to Achieve)
Define explicit checkbox update responsibility and timing in do.md to ensure consistent status tracking across all Features.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md updated with checkbox responsibility | file | Grep(do.md) | matches | checkbox.*(responsibility\|implementer\|ac-tester\|finalizer) | [x] |
| 2 | implementer.md updated with Task checkbox procedure | file | Grep(implementer.md) | matches | Mark Task.*\[x\].*build | [x] |
| 3 | ac-tester.md has AC PASS checkbox update | file | Grep(ac-tester.md) | matches | AC PASS.*\[x\]\|Mark.*AC.*PASS | [x] |
| 4 | finalizer.md updated with blocking check | file | Grep(finalizer.md) | matches | NOT_READY.*(incomplete\|unchecked)\|(incomplete\|unchecked).*NOT_READY | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update do.md with Checkbox Responsibility section: Phase 4→implementer marks Task[x], Phase 6→ac-tester marks AC[x], Phase 7→feature-reviewer verifies, Phase 9→finalizer blocks if incomplete | [x] |
| 2 | 2 | Update implementer.md: Add "Mark Task [x] after build success" procedure | [x] |
| 3 | 3 | Verify ac-tester.md "Task ステータス更新" section meets AC3 criteria; add missing text if not present | [x] |
| 4 | 4 | Update finalizer.md: Add blocking check - return NOT_READY if any Task/AC unchecked | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 08:05 | START | implementer | Task 1 | - |
| 2026-01-04 08:05 | END | implementer | Task 1 | SUCCESS |
| 2026-01-04 08:05 | START | implementer | Task 2 | - |
| 2026-01-04 08:05 | END | implementer | Task 2 | SUCCESS |
| 2026-01-04 08:05 | START | implementer | Task 3 | - |
| 2026-01-04 08:05 | END | implementer | Task 3 | SUCCESS |
| 2026-01-04 08:05 | START | implementer | Task 4 | - |
| 2026-01-04 08:05 | END | implementer | Task 4 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-329.md](feature-329.md) - Parent investigation
- [kojo-update-gaps.md](reference/kojo-update-gaps.md) - Workflow gap analysis
