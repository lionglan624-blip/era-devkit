# Feature 556: Add Dependency Gate to /fl, /do, /run

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

## Type: infra

## Created: 2026-01-18

---

## Summary

Add Dependency Gate to /fl, /do, /run commands to prevent execution when Predecessor features are not [DONE].

**Problem discovered**: F541 was executed despite F540 (Predecessor) being [PROPOSED]. Current workflow only checks the feature's own Status, not its Dependencies.

**Solution**: Add uniform Predecessor check to all three commands:
- /fl: Set feature to [BLOCKED] if Predecessor not [DONE]
- /do: STOP if Predecessor not [DONE]
- /run: STOP if Predecessor not [DONE]

**No exceptions**: All types (kojo, erb, engine, infra, research) follow the same rule.

---

## Background

### Philosophy (Mid-term Vision)

**Fail Fast** - Detect dependency violations at the earliest possible point. Prevent wasted effort on features that cannot be completed due to unmet dependencies.

**SSOT**: Dependencies table in feature-{ID}.md is the source of truth for blocking relationships. Workflow commands (fl.md, do.md, PHASE-1.md) are the SSOT for execution gates.

**Scope**: This feature establishes the Dependency Gate pattern across all execution commands (/fl, /do, /run), ensuring uniform enforcement for all feature types.

### Problem (Current Issue)

Current workflow has no Dependency Gate:
- Status Gate Check (PHASE-1.md Step 1.0.5) only checks feature's own Status
- No check for Predecessor Status in Dependencies table
- F541 was executed with F540 [PROPOSED] - a dependency violation

### Goal (What to Achieve)

1. Add Dependency Gate to /fl (set [BLOCKED])
2. Add Dependency Gate to /do (STOP)
3. Add Dependency Gate to /run PHASE-1.md (STOP)
4. Uniform rule: Predecessor must be [DONE] for all types

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| fl.md | Add Step 1.5: Dependency Gate | Features with unresolved predecessors get [BLOCKED] status |
| do.md | Add Step 1.0.6: Dependency Gate | /do STOPS before execution if Predecessor not [DONE] |
| PHASE-1.md | Add Step 1.0.6: Dependency Gate | /run STOPS at Phase 1 if Predecessor not [DONE] |
| index-features.md | Status updates via fl.md | Features may change to [BLOCKED] automatically |

### Rollback Plan

If issues arise after implementation:
1. `git revert` the commit that added Dependency Gate
2. Notify user of the rollback
3. Create follow-up feature to address the issue

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | fl.md has Dependency Gate | file | Grep | contains | "Dependency Gate" | [x] |
| 2 | fl.md sets BLOCKED | file | Grep | contains | "\\[BLOCKED\\].*Predecessor" | [x] |
| 3 | do.md has Dependency Gate | file | Grep | contains | "Dependency Gate" | [x] |
| 4 | do.md STOPs on dependency | file | Grep | contains | "STOP.*Predecessor.*not.*DONE" | [x] |
| 5 | PHASE-1.md has Dependency Gate | file | Grep | contains | "Dependency Gate" | [x] |
| 6 | PHASE-1.md STOPs on dependency | file | Grep | contains | "STOP.*Predecessor.*not.*DONE" | [x] |
| 7a | No exceptions in fl.md | file | Grep(.claude/commands/fl.md) | not_contains | "Exception.*research\\|research.*Exception" | [x] |
| 7b | No exceptions in do.md | file | Grep(.claude/commands/do.md) | not_contains | "Exception.*research\\|research.*Exception" | [x] |
| 7c | No exceptions in PHASE-1.md | file | Grep(.claude/skills/run-workflow/PHASE-1.md) | not_contains | "Exception.*research\\|research.*Exception" | [x] |
| 8 | No debt in fl.md | file | Grep(.claude/commands/fl.md) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 9 | No debt in do.md | file | Grep(.claude/commands/do.md) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 10 | No debt in PHASE-1.md | file | Grep(.claude/skills/run-workflow/PHASE-1.md) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 11 | fl.md updates index-features.md | file | Grep(.claude/commands/fl.md) | contains | "index-features.md.*\\[BLOCKED\\]" | [x] |

### AC Details

**AC#1-2**: /fl Dependency Gate
- Location: `.claude/commands/fl.md`
- After Phase 1 Load, check Dependencies table
- If Predecessor not [DONE]: Set feature to [BLOCKED], report to user

**AC#3-4**: /do Dependency Gate
- Location: `.claude/commands/do.md`
- After Status Gate Check
- If Predecessor not [DONE]: **STOP** with message

**AC#5-6**: /run Dependency Gate
- Location: `.claude/skills/run-workflow/PHASE-1.md`
- After Step 1.0.5 Status Gate Check
- If Predecessor not [DONE]: **STOP** with message

**AC#7a-7c**: No type exceptions
- All types (kojo, erb, engine, infra, research) follow same rule
- No "research can skip" exception
- Split per-file for standard Grep syntax

**AC#8-10**: No technical debt markers in modified files (split per-file for precise verification)

**AC#11**: fl.md updates index-features.md
- Verifies that fl.md's Dependency Gate updates index-features.md status to [BLOCKED]

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,11 | Add Dependency Gate to fl.md | [x] |
| 2 | 3,4 | Add Dependency Gate to do.md | [x] |
| 3 | 5,6 | Add Dependency Gate to PHASE-1.md | [x] |
| 4 | 7a,7b,7c,8,9,10 | Verify no exceptions and no debt | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Dependency Gate Logic (shared)

```markdown
## Step X: Dependency Gate

1. Read feature-{ID}.md Dependencies table
2. For each row where Type = "Predecessor":
   a. Read feature-{PredID}.md
   b. Check Status field
   c. If Status ≠ [DONE]:
      - /fl: Edit feature-{ID}.md Status → [BLOCKED]
             Edit index-features.md Status → [BLOCKED]
             Report: "Feature {ID} blocked: Predecessor {PredID} is {Status}"
      - /do, /run: **STOP**: "Predecessor {PredID} is not [DONE] (current: {Status})"
3. If all Predecessors [DONE]: Proceed
```

### Insertion Points

| File | Insert After | New Step |
|------|--------------|----------|
| fl.md | Section 1. Initialize (after target resolution) | Step 1.5: Dependency Gate |
| do.md | Step 1.0.5 Status Gate | Step 1.0.6: Dependency Gate |
| PHASE-1.md | Step 1.0.5 Status Gate | Step 1.0.6: Dependency Gate |

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| - | - | No dependencies | - |

---

## Links

- [index-features.md](index-features.md)
- [fl.md](../../.claude/commands/fl.md)
- [do.md](../../.claude/commands/do.md)
- [PHASE-1.md](../../.claude/skills/run-workflow/PHASE-1.md)
- [F540](feature-540.md) - Predecessor that F541 violated (background context)
- [F541](feature-541.md) - Issue discovered during execution
- [F559](feature-559.md) - Follow-up: [BLOCKED] unblocking mechanism

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| [BLOCKED] unblocking mechanism | F556 scope is adding gate, not managing status transitions | Feature | F559 |

---

## Review Notes

- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#7a-7c: Keep as regression guard to prevent future exception additions.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - AC#2: Keep pattern, implementation to match.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Task#1: Follows AC#2 resolution.
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - AC#8-10: Keep per-file split for debugging ease.
- **2026-01-18 FL iter4**: [resolved] Phase3-Maintainability - Insertion Points: Use Section 1.5 in implementation.
- **2026-01-18 FL iter4**: [resolved] Phase3-Maintainability - AC#2 pattern: Implementation to match pattern.
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - Structural dependencies: Step 1.0.5 verified to exist.
- **2026-01-18 FL iter7**: [resolved] Phase2-Validate - AC#11: Implementation to match pattern.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | Opus | Feature creation | PROPOSED |
| 2026-01-18 | init | initializer | Status → [WIP] | READY |
| 2026-01-18 | impl | implementer | Tasks 1-3 | SUCCESS |
| 2026-01-18 | verify | Opus | AC verification | All PASS |
| 2026-01-18 | DEVIATION | feature-reviewer | F557 ref incorrect | Fixed → F559 |
| 2026-01-18 | DEVIATION | feature-reviewer | CLAUDE.md missing [BLOCKED] | Fixed |
