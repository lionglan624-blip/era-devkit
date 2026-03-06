# Feature 557: FL Workflow Progressive Disclosure

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

## Background

### Philosophy (Mid-term Vision)
Zero Debt Upfront principle must be enforced, not just documented. Subagents must see FORBIDDEN patterns prominently when executing FL phases.

### Problem (Current Issue)
1. fl.md is a monolithic 900+ line file
2. Zero Debt Upfront / Anti-YAGNI rules are buried in the middle
3. Subagents don't read or prioritize these rules
4. FL still suggests "match existing pattern" or "YAGNI" despite documentation

### Goal (What to Achieve)
1. Split fl.md into Progressive Disclosure skill (fl-workflow) like run-workflow
2. Place FORBIDDEN patterns at TOP of each relevant phase file
3. Ensure subagents see Anti-YAGNI rules first when dispatched

**Note**: Parallel execution mode (Wave batch processing, wave1 syntax) moved to [Feature 561](feature-561.md).

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/commands/fl.md` | Refactor to thin wrapper | Existing FL functionality preserved via skill |
| `.claude/skills/fl-workflow/` | New directory | 11 new files (SKILL.md + PHASE-0..8 + POST-LOOP.md) |
| `CLAUDE.md` | Skills table update | Add fl-workflow entry |
| Subagents | No change | Read skill phases instead of monolithic command |

### Rollback Plan

If issues arise after deployment:
1. Revert commit via `git revert HEAD`
2. Original fl.md restored from git history
3. Delete `.claude/skills/fl-workflow/` directory
4. Remove CLAUDE.md Skills table entry

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | fl-workflow skill directory exists | file | Glob | exists | `.claude/skills/fl-workflow/` | [x] |
| 2 | SKILL.md has Zero Debt at top | file | head+Grep | contains | "Zero Debt Upfront" (first 50 lines) | [x] |
| 3 | All phase files created | file | Glob | count_equals | 11 | [x] |
| 4 | fl.md is thin wrapper | file | Grep | contains | "Skill(fl-workflow)" | [x] |
| 5 | fl.md line count reduced | file | wc -l | lt | 100 | [x] |
| 6 | PHASE-1 has FORBIDDEN at top | file | Grep | contains | "FORBIDDEN" (first 30 lines) | [x] |
| 7 | PHASE-2 has FORBIDDEN at top | file | Grep | contains | "FORBIDDEN" (first 30 lines) | [x] |
| 8 | PHASE-3 has FORBIDDEN at top | file | Grep | contains | "FORBIDDEN" (first 30 lines) | [x] |
| 9 | All links valid | file | reference-checker | succeeds | - | [x] |
| 10 | CLAUDE.md Skills table updated | file | Grep | contains | "fl-workflow" | [x] |
| 11 | POST-LOOP.md has Philosophy Gate | file | Grep | contains | "Philosophy Gate" | [x] |

### AC Details

**AC#1**: Directory `.claude/skills/fl-workflow/` exists with proper structure

**AC#2**: SKILL.md must have Zero Debt Upfront / Anti-YAGNI FORBIDDEN section within first 50 lines.
**Test**: `head -50 .claude/skills/fl-workflow/SKILL.md | grep "Zero Debt Upfront"` must succeed.

**AC#3**: Files: SKILL.md, PHASE-0.md through PHASE-8.md, POST-LOOP.md (total 11 files).
**Note**: PHASE-0 is used (not PHASE-1) because Phase 0 (Reference Check) is a pre-loop phase distinct from main iteration phases 1-8.

**AC#4-5**: fl.md becomes thin wrapper that calls `Skill(fl-workflow)`, similar to run.md pattern

**AC#6-8**: Phases involving reviewer suggestions (1=Review, 2=Validate, 3=Maintainability) must have FORBIDDEN section within first 30 lines.
**Test**: `head -30 .claude/skills/fl-workflow/PHASE-{1,2,3}.md | grep "FORBIDDEN"` must succeed for each.

**AC#9**: All internal links in new/modified files resolve correctly.
**Verification**: reference-checker agent validates link targets exist. Status "OK" = succeeds, "NEEDS_REVISION" = fails.

**AC#10**: CLAUDE.md Skills table includes fl-workflow entry

**AC#11**: POST-LOOP.md contains Philosophy Gate section (fl.md Section 10), ensuring this post-loop logic is included.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create `.claude/skills/fl-workflow/` directory | [x] |
| 2 | 2 | Create SKILL.md with Zero Debt Upfront at top | [x] |
| 3 | 3 | Extract PHASE-0.md (Reference Check) from fl.md | [x] |
| 4 | 3,6 | Extract PHASE-1.md (Review dispatch) with FORBIDDEN at top | [x] |
| 5 | 3,7 | Extract PHASE-2.md (Validator dispatch) with FORBIDDEN at top | [x] |
| 6 | 3,8 | Extract PHASE-3.md (Maintainability) with FORBIDDEN at top | [x] |
| 7 | 3 | Extract PHASE-4.md (AC Validation) | [x] |
| 8 | 3 | Extract PHASE-5.md (Feasibility) | [x] |
| 9 | 3 | Extract PHASE-6.md (Planning Validation) | [x] |
| 10 | 3 | Extract PHASE-7.md (Final Reference Check) | [x] |
| 11 | 3 | Extract PHASE-8.md (Handoff Validation) | [x] |
| 12 | 3,11 | Extract POST-LOOP.md (pending_user + Report + Status Update + Philosophy Gate) | [x] |
| 13 | 4,5 | Refactor fl.md to thin wrapper | [x] |
| 14 | 9 | Run reference-checker for link validation | [x] |
| 15 | 10 | Update CLAUDE.md Skills table | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | fl.md content | fl-workflow skill files (Tasks 1-12) |
| 2 | implementer | sonnet | fl.md | thin wrapper fl.md (Task 13) |
| 3 | reference-checker | haiku | fl-workflow files | link validation (Task 14) |
| 4 | implementer | sonnet | CLAUDE.md | Skills table update (Task 15) |

**Note**: Phase 2 depends on Phase 1 completion. Task 13 must execute after Tasks 1-12.

---

## Review Notes

- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - Wave Syntax Expansion: Moved to F561 (Parallel Execution).
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC count: Split feature - F557 now has 10 ACs (within guideline), Parallel Execution moved to F561.
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - AC#3 Expected format: Changed to pure count "11", description moved to AC Details.
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - Links section: Comment updated to "Reference for FORBIDDEN patterns" for clarity.
- **2026-01-18 FL iter5**: [resolved] Phase3-Maintainability - Philosophy vs AC gap: For infra-type Features, structural placement enables enforcement. Behavioral testing is integration scope beyond this Feature.
- **2026-01-18 FL iter6**: [resolved] Phase2-Validate - Confirmed: infra-type structural verification is standard. "Enforcement" achieved through placement ensuring visibility.
- **2026-01-18 FL iter7**: [resolved] Phase2-Validate - AC#9 Type 'manual' → 'exit_code'. Method → 'Task(reference-checker)'. AC Details added.
- **2026-01-18 FL iter7**: [resolved] Phase2-Validate - PHASE-0 naming: rationale documented in AC#3 Details (pre-loop phase distinct from main iteration).
- **2026-01-18 FL iter8**: [resolved] Phase2-Validate - AC#3 count-only validation: count_equals with AC Details is valid per SSOT. Robustness covered by AC#11 (Philosophy Gate verification).
- **2026-01-18 FL iter8**: [resolved] Phase2-Validate - Philosophy Gate (fl.md Section 10) goes into POST-LOOP.md. Task 12 updated, AC#11 added.
- **2026-01-18 FL iter10**: [skipped] Phase2-Validate - Impact Analysis notation 'PHASE-0..8' stylistic: no SSOT violation, but could use 'PHASE-0 to PHASE-8' for clarity.
- **2026-01-18 FL iter11**: [skipped] Phase2-Validate - PHASE-0 naming: rationale exists in AC#3 Details, but whether it's 'strong enough' is subjective.
- **2026-01-18 FL iter11**: [skipped] Phase2-Validate - Task 14 wording 'Run reference-checker' is correct but could be 'Verify all links'.
- **2026-01-18 FL iter12**: [skipped] Phase2-Validate - AC#5 Method='wc -l' is non-standard but infra type has flexibility. Severity may be too high.
- **2026-01-18 FL iter13**: [resolved] Phase2-Validate - AC#9 updated to match F451 pattern: Type='file', Method='reference-checker', Matcher='succeeds'.

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Parallel Execution mode | Feature scope split | Feature | F561 |

---

## Links

- [fl.md](../../.claude/commands/fl.md) - Current monolithic command
- [run-workflow/SKILL.md](../../.claude/skills/run-workflow/SKILL.md) - Reference pattern
- [feature-reviewer.md](../../.claude/agents/feature-reviewer.md) - Reference for FORBIDDEN patterns
- [feature-561.md](feature-561.md) - Parallel Execution mode (split from this feature)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-19 01:26 | START | implementer | Task 15 | - |
| 2026-01-19 01:26 | END | implementer | Task 15 | SUCCESS |
