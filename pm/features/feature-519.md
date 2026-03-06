# Feature 519: Convert 8 Subagents to Skills (context:fork implementation)

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
Skills are the SSOT for subagent behavior. Converting agents to Skills with context:fork eliminates lazy loading by guaranteeing knowledge auto-load at dispatch time. Scope: All critical workflow subagents (initializer, finalizer, reference-checker, eratw-reader, dependency-analyzer, goal-setter, philosophy-deriver, task-comparator). Benefit: Subagents execute correctly in one shot without relying on callers to remember loading instructions.

### Problem (Current Issue)
Eight subagents are located in .claude/agents/ directory. While they have YAML frontmatter (name, description, model, tools), this location does not support Skill() tool auto-loading or context:fork isolation. Dispatching requires `Task(prompt: "Read .claude/agents/{agent}.md...")` which relies on callers remembering to include the load instruction. Moving them to .claude/skills/ with proper frontmatter (context: fork, agent: general-purpose, allowed-tools) would enable auto-loading like feature-creator.

### Goal (What to Achieve)
Convert 8 critical subagents to Skills using context:fork to eliminate lazy loading failures. Priority order: initializer (Phase 1), finalizer (Phase 9), reference-checker (/fl workflow), eratw-reader (kojo implementation). Ensure workflow documentation reflects the conversion and maintains backward compatibility.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | initializer skill created | file | Glob | exists | ".claude/skills/initializer/SKILL.md" | [x] |
| 2 | finalizer skill created | file | Glob | exists | ".claude/skills/finalizer/SKILL.md" | [x] |
| 3 | reference-checker skill created | file | Glob | exists | ".claude/skills/reference-checker/SKILL.md" | [x] |
| 4 | eratw-reader skill created | file | Glob | exists | ".claude/skills/eratw-reader/SKILL.md" | [x] |
| 5 | dependency-analyzer skill created | file | Glob | exists | ".claude/skills/dependency-analyzer/SKILL.md" | [x] |
| 6 | goal-setter skill created | file | Glob | exists | ".claude/skills/goal-setter/SKILL.md" | [x] |
| 7 | philosophy-deriver skill created | file | Glob | exists | ".claude/skills/philosophy-deriver/SKILL.md" | [x] |
| 8 | task-comparator skill created | file | Glob | exists | ".claude/skills/task-comparator/SKILL.md" | [x] |
| 9 | context:fork in initializer skill | file | Grep | contains | "context: fork" | [x] |
| 9b | All 8 new skills have context:fork | file | Grep | count_equals | 9 | [x] |
| 10 | initializer skill in CLAUDE.md Skills | file | Grep | contains | "\`initializer\`" | [x] |
| 11 | finalizer skill in CLAUDE.md Skills | file | Grep | contains | "\`finalizer\`" | [x] |
| 12 | reference-checker skill in CLAUDE.md Skills | file | Grep | contains | "\`reference-checker\`" | [x] |
| 13 | eratw-reader skill in CLAUDE.md Skills | file | Grep | contains | "\`eratw-reader\`" | [x] |
| 14 | Agent markdown preserved | file | Glob | exists | ".claude/agents/initializer.md" | [x] |
| 15 | Subagent Strategy reflects conversion | file | Grep | contains | "Skill(initializer)" | [x] |
| 16 | allowed-tools field in initializer skill | file | Grep | contains | "allowed-tools:" | [x] |
| 17 | do.md dispatch updated | file | Grep | contains | "Skill(initializer)" | [x] |
| 18 | Documentation consistency verified | manual | /audit | succeeds | "No issues found" | [x] |
| 19 | All links validated | manual | reference-checker | succeeds | - | [x] |

### AC Details

**AC#1-8**: Skill files created with proper YAML frontmatter
- Test: Glob pattern to verify skill directory structure exists
- Verifies: All 8 target subagents converted to skills

**AC#9**: YAML frontmatter format validation (representative sample)
- Test: Grep for `context: fork` in initializer SKILL.md
- Pattern: `context: fork` verifies proper skill frontmatter
- Path: `.claude/skills/initializer/SKILL.md`
- Verifies: Skill has proper context:fork configuration
- Rationale: Representative sample validates format correctness; AC#9b provides comprehensive count verification

**AC#9b**: All 8 new skills have context:fork (count-based verification)
- Test: Grep for `context: fork` across all skill directories
- Command: `Grep "context: fork" .claude/skills/*/SKILL.md | count`
- Expected: 9 matches (1 existing feature-creator + 8 new skills)
- Verifies: Uniform field transformation applied to ALL 8 new skills
- Rationale: Complements representative sample (AC#9) with exhaustive count verification
- Baseline: 1 context:fork skill (feature-creator) at review time. If baseline changes before implementation, Expected must be updated accordingly.

**AC#10-13, AC#15**: CLAUDE.md documentation update (atomic operation)
- Test: Grep for skill names and dispatch pattern in CLAUDE.md
- AC#10-13: Verify priority skills with backtick format (e.g., `\`initializer\``) matching existing Skills table format
- AC#15: Verify Skill() dispatch pattern in Subagent Strategy
- Format: CLAUDE.md Skills table uses backtick format: `| \`skill-name\` | description |` (verified against existing entries lines 197-206)
- Rationale: Task#4 covers AC#10-13 and AC#15 as single atomic CLAUDE.md edit operation. Both Skills table and Subagent Strategy updates occur in same file during single edit session. Per feature-quality rules: "Tasks that are same edit operation merged into atomic task".
- Note: Remaining 4 skills' CLAUDE.md Skills table update deferred to F521 per 引継ぎ先指定 (skill files validated by AC#1-8 existence)

**AC#14**: Original agent files preserved (representative sample)
- Test: Glob to verify initializer.md agent file still exists
- Path: `.claude/agents/initializer.md`
- Verifies: Backward compatibility maintained during transition
- Rationale: Representative sample - all 8 agent files are preserved (not deleted during conversion); initializer verifies the preservation pattern applies

**AC#16**: allowed-tools field present (representative sample)
- Test: Grep for `allowed-tools:` in initializer SKILL.md
- Path: `.claude/skills/initializer/SKILL.md`
- Verifies: Field transformation from `tools` to `allowed-tools` applied correctly
- Rationale: Same representative sample approach as AC#9 - uniform transformation across all 8 skills

**AC#17**: do.md dispatch examples updated
- Test: Grep for `Skill(initializer)` in do.md
- Path: `.claude/commands/do.md`
- Verifies: Dispatch examples changed from Task() to Skill() pattern
- Scope: Update dispatch examples in do.md for agents that have explicit examples (initializer, finalizer). Other 6 agents (reference-checker, eratw-reader, dependency-analyzer, goal-setter, philosophy-deriver, task-comparator) are not directly dispatched in do.md - they are dispatched from other documents or workflows.

**AC#18**: Documentation consistency check
- Test: Run /audit slash command to verify SSOT consistency
- Verifies: All documentation cross-references are valid
- Execution: Post-implementation manual verification (Phase 7 in Implementation Contract)
- Note: Type=manual means human-verified, not ac-tester automated. Run after AC#1-17 pass.

**AC#19**: Link validation across all modified files
- Test: Use reference-checker skill (post-conversion) to validate links
- Verifies: No broken links introduced by documentation changes
- Execution: Post-implementation manual verification using newly created skill (Phase 6 → Phase 7)
- Note: reference-checker skill is created by AC#3 (Task#1), then used for AC#19 validation. This is intentional self-test of the converted skill.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-4 | Convert priority subagents to skills (initializer, finalizer, reference-checker, eratw-reader) | [x] |
| 2 | 5-8 | Convert remaining subagents to skills (dependency-analyzer, goal-setter, philosophy-deriver, task-comparator) | [x] |
| 3 | 9, 9b, 16 | Verify frontmatter fields (context:fork count, allowed-tools) | [x] |
| 4 | 10-13, 15 | Update CLAUDE.md Skills table and Subagent Strategy section | [x] |
| 5 | 14 | Verify agent files preserved (backward compatibility) | [x] |
| 6 | 17 | Update do.md dispatch examples to Skill() pattern | [x] |
| 7 | 18-19 | Perform documentation audit and link validation | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Convert initializer, finalizer, reference-checker, eratw-reader to skills. Field mapping: `name → name`, `description → description`, `model → model`, `tools → allowed-tools` (preserve original tool list, change field name only). Add: `context: fork`, `agent: general-purpose`. Reference feature-creator SKILL.md as template. | 4 SKILL.md files |
| 2 | implementer | sonnet | Convert dependency-analyzer, goal-setter, philosophy-deriver, task-comparator to skills. Same field transformation pattern as Phase 1. | 4 SKILL.md files |
| 3 | implementer | sonnet | Update CLAUDE.md Skills table (4 priority skills) and Subagent Strategy section (Skill() dispatch for all 8) | Updated CLAUDE.md |
| 4 | implementer | sonnet | Update do.md dispatch examples from Task() to Skill() pattern for 8 converted agents | Updated do.md |
| 5 | ac-tester | haiku | Verify agent files preserved (Glob .claude/agents/*.md) | Agent preservation verification |
| 6 | reference-checker | haiku | Validate all links in modified documentation | Link validation report |
| 7 | ac-tester | haiku | Run all AC verification tests | AC test results |

### Rollback Plan

If context:fork skills fail to auto-load correctly:
1. Revert .claude/skills/* additions
2. Keep .claude/agents/*.md files unchanged (preserved during conversion)
3. Restore CLAUDE.md to pre-conversion state
4. Document failure mode in Review Notes for future reference

---

## Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/skills/* | Add 8 new skill directories | Skills auto-load via context:fork |
| CLAUDE.md | Update Skills table and Subagent Strategy | All agents see new skills, dispatch changes to Skill() |
| .claude/agents/* | Preserve existing files | Backward compatibility maintained |
| do.md | Update dispatch examples | Change Task() examples to Skill() dispatch for 8 converted agents (lines 128-132 etc.) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-17 FL iter1**: [resolved] Phase2-Validate - 引継ぎ先指定: F520 and F521 referenced but don't exist. User decision: Create F520/F521 now.
- **2026-01-17 FL iter6**: [resolved] Phase2-Validate - AC#15 dispatch pattern: User confirmed Skill() dispatch for all 8 agents. do.md will be updated accordingly.
- **2026-01-17 FL iter7**: [resolved] Phase2-Validate - Task#4 scope: Covers AC#10-13, AC#15 (5 ACs). Acceptable per "same edit operation merged" rule. AC Details now documents rationale: "Task#4 covers AC#10-13 and AC#15 as single atomic CLAUDE.md edit operation."
- **2026-01-17 FL iter8**: [resolved] Phase2-Validate - do.md hardcoded dispatch: User confirmed Skill() dispatch. do.md added to Impact Analysis for update.

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Performance impact testing | Not critical for MVP functionality | Feature | F520 |
| Skill naming convention standardization | Can be addressed after basic conversion | Feature | F521 |
| CLAUDE.md Skills table update for remaining 4 skills | Priority skills first, secondary skills follow | Feature | F521 |
| Stale Task() dispatch in run-workflow/PHASE-1,9.md and fl.md | AC#17 scoped to do.md only, other docs out of scope | Feature | F521 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-17 | START | initializer | Phase 1 init | READY:519:infra |
| 2026-01-17 | START | implementer | Phase 1 - 4 priority skills | SUCCESS |
| 2026-01-17 | START | implementer | Phase 2 - 4 remaining skills | SUCCESS |
| 2026-01-17 | START | implementer | Phase 3 - CLAUDE.md update | SUCCESS |
| 2026-01-17 | START | implementer | Phase 4 - do.md update | SUCCESS |
| 2026-01-17 | END | opus | Task 1-6 verification | PASS |
| 2026-01-17 | START | ac-tester | AC#1-17 verification | PASS |
| 2026-01-17 | DEVIATION | opus | Model ID fix | claude-haiku-4-20250611 → 20250618 (4 files) |
| 2026-01-17 | START | /audit | AC#18 doc consistency | PASS (warn: CLAUDE.md 361 lines) |
| 2026-01-17 | END | reference-checker | AC#19 link validation | PASS |

## Dependencies

| Type | ID | Description |
|------|----|----|
| Reference | feature-creator | Template for context:fork skills |
| Related | do.md | Workflow that dispatches these subagents (implicit change if dispatch method changes) |

---

## Links
- [index-features.md](index-features.md)
- [feature-creator skill](../../.claude/skills/feature-creator/SKILL.md) - Reference implementation
- [feature-520.md](feature-520.md) - Skill Performance Testing (handoff)
- [feature-521.md](feature-521.md) - Skill Documentation Completion (handoff)