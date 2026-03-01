# Feature 332: kojo_test_gen.py Parameter Inline Documentation

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ワークフローの欠陥は発見時に報告し、調査→修正の流れで恒久対策する

### Problem (Current Issue)
kojo_test_gen.py execution directory was documented in KOJO.md but not discoverable during execution.

**Evidence**: F319 Execution Log shows 3 retries (DEVIATION) before discovering correct working directory.

**Root Cause**: Parameter visibility issue - `--output-dir` parameter requirement was in KOJO.md but not referenced in do.md Phase 5 workflow step.

**Impact**: 3 retries, wasted time, workflow interruption

**Current Workaround**: Manual discovery via SKILL.md reading

### Goal (What to Achieve)
Add inline parameter documentation to do.md Phase 5 to improve discoverability.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md Phase 5 includes inline parameter format | code | Grep(.claude/commands/do.md) | contains | python tools/kojo-mapper/kojo_test_gen.py --feature {ID} --com {COM_NUMBER} --output-dir Game/tests/ac/kojo/feature-{ID}/ | [x] |
| 2 | do.md Phase 5 cross-references KOJO.md explicitly | code | Grep(.claude/commands/do.md) | contains | Skill(testing) to get kojo_test_gen.py command format from KOJO.md | [x] |
| 3 | do.md Phase 5 includes --output-dir verification step | code | Grep(.claude/commands/do.md) | contains | --output-dir parameter is required | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add inline parameter format example `--feature {ID} --com {COM_NUMBER} --output-dir` to do.md Phase 5 | [x] |
| 2 | 2 | Verify KOJO.md cross-reference exists (already present at line 554) | [x] |
| 3 | 3 | Add `--output-dir parameter is required` note to do.md Phase 5 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | COMPLETION_VERIFICATION | finalizer | Verify all ACs and tasks | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-329.md](feature-329.md) - Parent investigation
- [feature-319.md](feature-319.md) - 3 retry discovery source
- [kojo-update-gaps.md](reference/kojo-update-gaps.md) - Workflow gap analysis
- [testing/KOJO.md](../../.claude/skills/testing/KOJO.md) - kojo_test_gen.py parameters
