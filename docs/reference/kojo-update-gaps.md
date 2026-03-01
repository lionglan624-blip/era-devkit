# Kojo Workflow Update Gaps

**Status**: Investigation complete (F329)
**Date**: 2026-01-03

---

## Overview

This document records workflow gaps discovered during kojo Feature execution (F319, F324) that caused update omissions, retries, and workflow deviations. These gaps represent systematic issues that will recur unless addressed.

---

## Gap Classification

### 1. Discoverability Gap

**Problem**: kojo_test_gen.py execution directory was documented in KOJO.md but not discoverable during execution.

**Evidence**: F319 Execution Log shows 3 retries (DEVIATION) before discovering correct working directory.

**Root Cause**: Parameter visibility issue - `--output-dir` parameter requirement was in KOJO.md but not referenced in do.md Phase 5 workflow step.

**Impact**: 3 retries, wasted time, workflow interruption

**Current Workaround**: Manual discovery via SKILL.md reading

**Ideal State**: do.md Phase 5 directly references KOJO.md parameter format or includes inline parameter example.

---

### 2. Responsibility Gap (AC/Task Checkboxes)

**Problem**: AC/Task checkbox update timing and responsible agent undefined in workflow.

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

**Current State**: Ad-hoc updates by various agents or orchestrator

**Ideal State**: Explicit responsibility assignment in do.md with timing rules

---

### 3. Configuration Gap (com_file_map.json Extension) - RESOLVED

**Problem**: com_file_map.json extension process undefined when new COM added.

**Evidence**:
- F319: COM_94 not in ranges (90-93 stopped at 93), required ADHOC_FIX
- 7 DEVIATION entries in F319 Execution Log

**Root Cause** (at F319 time):
- F320 philosophy: "将来の COM 追加時の修正コストをゼロにする" (zero cost for future COM additions)
- Reality: Ranges stopped at COM 93, leaving COM 94+ undefined

**Resolution (F320)**:
- F320 extended com_file_map.json to cover **COM 0-699 (all planned COMs)**
- content-roadmap.md defines COM 0-600系 as v1.0 scope
- COM 700+ is undefined in roadmap and not planned
- Therefore, com_file_map.json now covers 100% of planned COMs

**Current State**: ✅ RESOLVED - No workflow change needed

**Verification**:
- com_file_map.json ranges: 0-6, 7-7, 8-9, ... 600-699 (continuous coverage)
- content-roadmap.md: "All 150 COMs" within 0-600系
- COM 700+ not in roadmap → no extension workflow required

---

## Workflow Gap Summary

| Gap Type | Discovered In | Root Cause | Current Impact | Recurrence Risk |
|----------|--------------|------------|----------------|-----------------|
| Discoverability | F319 | Parameter format not visible in do.md Phase 5 | 3 retries, time waste | HIGH - every new kojo Feature |
| Responsibility | F319, F324 | Checkbox ownership/timing undefined | Inconsistent status tracking | MEDIUM - depends on orchestrator behavior |
| Configuration | F319 | com_file_map.json extension process undefined | ~~Manual JSON edits~~ | ✅ RESOLVED (F320 covers COM 0-699) |

---

## Responsibility Matrix Proposal

### AC/Task Checkbox Updates

| Timing | Agent | Responsibility | Checkpoint |
|--------|-------|----------------|------------|
| Phase 4 completion (per Task) | implementer | Mark Task [x] in feature-{ID}.md | Build success |
| Phase 6 completion (per AC) | ac-tester | Mark AC [x] in feature-{ID}.md | Test PASS |
| Phase 7 (post-review) | feature-reviewer | Verify all Tasks [x] and ACs [x] | READY gate |
| Phase 9 (finalize) | finalizer | Final verification, error if incomplete | Commit block |

**Rule**: Checkboxes are updated by the agent that verifies the completion, not by the orchestrator.

**Enforcement**: finalizer MUST check all Tasks [x] and ACs [x] before proceeding to commit. If any unchecked, return BLOCKED status.

---

### com_file_map.json Updates - NOT REQUIRED

**Status**: ✅ No workflow needed

**Reason**: F320 extended com_file_map.json to cover COM 0-699 (all planned COMs per content-roadmap.md). COM 700+ is not in roadmap.

**If COM 700+ is added in future**: Create a Feature to extend com_file_map.json ranges at that time. No preemptive workflow required.

---

### kojo_test_gen.py Parameter Visibility

| Timing | Agent | Responsibility | Action |
|--------|-------|----------------|--------|
| Phase 5 execution | opus orchestrator | Provide complete kojo_test_gen.py command with all parameters | Reference KOJO.md inline or copy parameter format to do.md |
| do.md update | implementer (infra type) | Add kojo_test_gen.py parameter example to Phase 5 | Example: `--feature {ID} --com {COM} --output-dir test/ac/kojo/feature-{ID}/` |

**Rule**: do.md Phase 5 MUST include parameter format inline or direct reference to KOJO.md section (not just "invoke Skill(testing)").

---

## Related Features

| Feature ID | Type | Purpose | Status |
|:----------:|------|---------|:------:|
| F319 | kojo | COM_94 口上 (7 DEVIATIONs discovered) | [DONE] |
| F320 | infra | com_file_map.json SSOT + unified refs | [DONE] |
| F324 | kojo | COM_95 口上 (0 DEVIATIONs after F320 fix) | [DONE] |
| F329 | infra | kojo workflow gap investigation (this doc) | [WIP] |

---

## Recommended Follow-up Features

Based on investigation findings, the following Features are recommended:

### Feature Proposal 1: Checkbox Responsibility Definition

**Type**: infra
**Goal**: Define explicit checkbox update responsibility and timing in do.md
**Scope**:
- Update do.md Phase 4, 6, 7, 9 with checkbox responsibility rules
- Update implementer.md, ac-tester.md, finalizer.md with checkbox update procedures
- Add finalizer blocking check for incomplete checkboxes

### ~~Feature Proposal 2: com_file_map.json Extension Workflow~~ - NOT REQUIRED

**Status**: ✅ Withdrawn

**Reason**: F320 already covers COM 0-699 (all planned COMs). content-roadmap.md does not define COM 700+. No workflow needed.

### Feature Proposal 3: kojo_test_gen.py Parameter Inline Documentation

**Type**: infra
**Goal**: Improve parameter discoverability in do.md Phase 5
**Scope**:
- Add inline parameter format example to do.md Phase 5
- Cross-reference KOJO.md parameter section explicitly
- Add verification step to ensure --output-dir is provided

---

## Notes

- F319 had 7 DEVIATIONs because COM_94 was outside com_file_map.json ranges (stopped at 93)
- F320 extended com_file_map.json to cover **COM 0-699** (not just 90-95 as initially recorded)
- F324 had 0 DEVIATIONs because F320's fix covered all planned COMs
- content-roadmap.md defines v1.0 scope as "All 150 COMs" within COM 0-600系
- COM 700+ is not defined in roadmap → com_file_map.json extension workflow is unnecessary

---

## Links

- [feature-319.md](../feature-319.md) - Source of 7 DEVIATION discovery
- [feature-320.md](../feature-320.md) - com_file_map.json SSOT design
- [feature-324.md](../feature-324.md) - Zero DEVIATION execution (benefited from F320)
- [feature-329.md](../feature-329.md) - This investigation's parent Feature
- [do.md](../../.claude/commands/do.md) - Workflow definition
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - Kojo implementation reference
- [testing KOJO.md](../../.claude/skills/testing/KOJO.md) - kojo_test_gen.py parameters
- [com_file_map.json](../../tools/kojo-mapper/com_file_map.json) - COM→File SSOT
