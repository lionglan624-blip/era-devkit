# Feature 610: Feature Creator 5-Phase Orchestrator Redesign

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

Feature creation should be **progressive and resumable** with **context isolation** between phases.

- **Single File SSOT**: `feature-{ID}.md` is the single source of truth
- **Context Isolation**: Each phase runs in separate subagent context, preventing large code reads from polluting main conversation
- **Resumability**: Session interruption should not lose progress; orchestrator can detect completion state from file content
- **Model Optimization**: Different phases benefit from different models (haiku for simple extraction, opus for philosophy derivation)

### Problem (Current Issue)

Current `feature-creator` skill is a monolithic single-skill approach:

1. **Context Pollution**: All code investigation loads into same context window
2. **No Resume**: Session interruption requires starting over
3. **Fixed Model**: Cannot optimize model selection per phase
4. **Tight Coupling**: Philosophy derivation, code investigation, AC design all mixed together

### Goal (What to Achieve)

Transform `feature-creator` into a 5-phase orchestrator with specialized subagents:

| Phase | Agent | Model | Output |
|-------|-------|:-----:|--------|
| 1 | philosophy-definer | haiku | ## Background section (Philosophy/Problem/Goal) |
| 2 | tech-investigator | sonnet | ## Dependencies, ## Impact Analysis |
| 3 | ac-designer | opus | ## Acceptance Criteria, ### AC Details |
| 4 | wbs-generator | haiku | ## Tasks, ## Implementation Contract |
| 5 | validator | haiku | Static validation, loop back if issues |

**Agent Distinction (philosophy-definer vs philosophy-deriver)**:
- **philosophy-definer** (NEW, this feature): Creates Philosophy section FROM user requirements DURING feature creation. Input: user requirements → Output: Philosophy/Problem/Goal.
- **philosophy-deriver** (EXISTING): Validates EXISTING Philosophy coverage DURING FL review. Input: existing Philosophy → Output: derived tasks for comparison.

---

## Impact Analysis

| File/Component | Change Type | Description |
|----------------|-------------|-------------|
| `.claude/skills/feature-creator/SKILL.md` | Rewrite | Convert to orchestrator with phase dispatch |
| `.claude/agents/philosophy-definer.md` | Create | Phase 1 agent definition |
| `.claude/agents/tech-investigator.md` | Create | Phase 2 agent definition |
| `.claude/agents/ac-designer.md` | Create | Phase 3 agent definition |
| `.claude/agents/wbs-generator.md` | Create | Phase 4 agent definition |
| `.claude/agents/feature-validator.md` | Create | Phase 5 agent definition |
| `CLAUDE.md` | Update | Update Subagent Strategy table with 5 new agents (philosophy-definer [NEW - creates Philosophy from user requirements during feature creation vs existing philosophy-deriver validates existing Philosophy coverage during review], tech-investigator, ac-designer, wbs-generator, feature-validator) including Type and Model |
| `CLAUDE.md Context:fork skills` | No change | New agents are Task-dispatched (general-purpose), not context:fork skills |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Orchestrator SKILL.md rewritten | file | Glob | exists | `.claude/skills/feature-creator/SKILL.md` | [x] |
| 2 | philosophy-definer agent exists | file | Glob | exists | `.claude/agents/philosophy-definer.md` | [x] |
| 3 | tech-investigator agent exists | file | Glob | exists | `.claude/agents/tech-investigator.md` | [x] |
| 4 | ac-designer agent exists | file | Glob | exists | `.claude/agents/ac-designer.md` | [x] |
| 5 | wbs-generator agent exists | file | Glob | exists | `.claude/agents/wbs-generator.md` | [x] |
| 6 | feature-validator agent exists | file | Glob | exists | `.claude/agents/feature-validator.md` | [x] |
| 7a | Orchestrator references philosophy-definer | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "philosophy-definer" | [x] |
| 7b | Orchestrator references tech-investigator | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "tech-investigator" | [x] |
| 7c | Orchestrator references ac-designer | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "ac-designer" | [x] |
| 7d | Orchestrator references wbs-generator | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "wbs-generator" | [x] |
| 7e | Orchestrator references feature-validator | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "feature-validator" | [x] |
| 8a | CLAUDE.md philosophy-definer registered | code | Grep(CLAUDE.md) | matches | "philosophy-definer.*haiku" | [x] |
| 8b | CLAUDE.md tech-investigator registered | code | Grep(CLAUDE.md) | matches | "tech-investigator.*sonnet" | [x] |
| 8c | CLAUDE.md ac-designer registered | code | Grep(CLAUDE.md) | matches | "ac-designer.*opus" | [x] |
| 8d | CLAUDE.md wbs-generator registered | code | Grep(CLAUDE.md) | matches | "wbs-generator.*haiku" | [x] |
| 8e | CLAUDE.md feature-validator registered | code | Grep(CLAUDE.md) | matches | "feature-validator.*haiku" | [x] |
| 9 | Resume logic implemented | code | Grep(.claude/skills/feature-creator/SKILL.md) | matches | "determine_resume\|return.*Step" | [x] |
| 10 | ac-designer incorporates philosophy logic | code | Grep(.claude/agents/ac-designer.md) | contains | "derived_tasks\|absolute_claims" | [x] |
| 11 | wbs-generator incorporates alignment logic | code | Grep(.claude/agents/wbs-generator.md) | contains | "AC:Task 1:1\|1 AC = 1" | [x] |
| 12 | All links in new agents valid | tool | /reference-checker | succeeds | - | [x] |
| 13 | All new agents have required format | code | Grep(.claude/agents/philosophy-definer.md) | contains | "## Task" | [x] |
| 14 | Phase 1 uses haiku model | code | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "philosophy-definer.*haiku\|haiku.*philosophy-definer" | [x] |
| 15 | Context isolation via Task dispatch | code | Grep(.claude/skills/feature-creator/SKILL.md) | matches | "Task\\(subagent_type.*philosophy-definer" | [x] |
| 16 | Context isolation effect verified | design | Manual | verified | Task dispatch documented as context isolation mechanism | [x] |

**Note**: 16 ACs (with 10 sub-ACs in AC#7 and AC#8, totaling 24 verification points) for infra type. Count justified by 5 new agent definitions plus orchestrator redesign plus philosophy verification plus comprehensive testing. Individual agent and registration verification prevents further consolidation.

### AC Details

**AC#1-6**: File existence verification
- Verify all required files created via Glob
- **AC#1 scope**: File existence only. Content verification (phase dispatch, resume logic) covered by separate ACs (7a-7e, 9, 14, 15)
- Single File SSOT enforced by design: All agents append to same feature-{ID}.md file, no intermediate state files created

**AC#7a-7e**: Phase dispatch verification
- Orchestrator references all 5 phase agents individually
- Verifies each agent name appears in orchestrator SKILL.md

**AC#8a-8e**: CLAUDE.md Subagent Strategy update
- All new agents must be registered in CLAUDE.md table with correct models
- Individual verification of each agent registration (philosophy-definer/wbs-generator/feature-validator: haiku, tech-investigator: sonnet, ac-designer: opus)

**AC#9**: Resume capability
- Orchestrator must contain logic to detect current state from file content
- Resume from appropriate step based on sections present

**AC#10**: ac-designer incorporates philosophy derivation patterns
- Verify ac-designer.md contains philosophy derivation logic keywords (absolute_claims, derived tasks)
- Ensures agent reuses existing philosophy-deriver analysis patterns

**AC#11**: wbs-generator incorporates AC:Task alignment patterns
- Verify wbs-generator.md contains AC:Task alignment logic keywords (AC.*Task, 1:1 mapping)
- Ensures agent reuses existing ac-task-aligner validation patterns

**AC#12**: All links in new agents valid
- Execute reference-checker against all 5 new agent files to verify cross-references are valid
- Prevents broken links in agent definitions affecting workflow reliability

**AC#13**: Agent format verification
- Verify philosophy-definer.md has required ## Task section (representative check)
- Representative sampling rationale: All 5 agents are created from same template with identical structure - verifying one confirms all others follow pattern
- Ensures consistency with existing agent structure patterns

**AC#14**: Phase 1 model parameter verification
- Verify orchestrator SKILL.md specifies haiku model for philosophy-definer dispatch
- Ensures correct model selection for phase execution

**AC#15**: Context isolation verification
- Verify Task() dispatch pattern used for philosophy-definer (representative of all phases)
- Ensures separate subagent context execution as stated in Philosophy

**AC#16**: Context isolation effect verification
- Manual design review confirms Task dispatch achieves context isolation
- Philosophy claims context isolation benefit; AC#15 verifies dispatch pattern exists, AC#16 confirms design intent is documented
- Effect: Main orchestrator context window does not grow with phase agent code reads

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2 | Create philosophy-definer.md agent definition | [x] |
| 2 | 3 | Create tech-investigator.md agent definition | [x] |
| 3 | 4,10 | Create ac-designer.md with philosophy derivation patterns | [x] |
| 4 | 5,11 | Create wbs-generator.md with AC:Task alignment patterns | [x] |
| 5 | 6 | Create feature-validator.md agent definition | [x] |
| 6 | 1 | Create feature-creator/SKILL.md orchestrator skeleton | [x] |
| 7 | 7a-7e | Implement phase dispatch for all 5 agents | [x] |
| 8 | 8a-8e | Update CLAUDE.md Subagent Strategy table | [x] |
| 9 | 9 | Implement resume logic detection | [x] |
| 10 | 12 | Validate all links in new agent files using reference-checker | [x] |
| 11 | 13 | Verify all new agents have required ## Task section | [x] |
| 12 | 14 | Verify Phase 1 model parameter in dispatch code | [x] |
| 13 | 15 | Use Task() dispatch pattern for context isolation | [x] |
| 14 | 16 | Document context isolation design intent in SKILL.md | [x] |

<!-- AC:Task 1:1 Rule: Multiple ACs merged for same implementation unit -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Phase Execution Flow

```
Orchestrator (SKILL.md, sonnet)
│
├─→ Step 0: Initialize
│     Create feature-{ID}.md skeleton with Status: [DRAFT]
│
├─→ Step 1: Philosophy Phase
│     Task(subagent_type: "general-purpose", model: "haiku", prompt: "Read .claude/agents/philosophy-definer.md...")
│     Append: ## Background section
│
├─→ Step 2: Investigation Phase
│     Task(subagent_type: "general-purpose", model: "sonnet", prompt: "Read .claude/agents/tech-investigator.md...")
│     Append: ## Dependencies, ## Impact Analysis
│
├─→ Step 3: Design Phase
│     Task(subagent_type: "general-purpose", model: "opus", prompt: "Read .claude/agents/ac-designer.md...")
│     Append: ## Acceptance Criteria, ### AC Details
│
├─→ Step 4: WBS Phase
│     Task(subagent_type: "general-purpose", model: "haiku", prompt: "Read .claude/agents/wbs-generator.md...")
│     Append: ## Tasks, ## Implementation Contract
│
└─→ Step 5: Validation & Finalize
      Task(subagent_type: "general-purpose", model: "haiku", prompt: "Read .claude/agents/feature-validator.md...")
      IF issues AND retry_count < 3: GOTO relevant Step, increment retry_count
      ELSE IF retry_count >= 3: STOP with ERROR and log issues
      ELSE: Status → [PROPOSED], update index

**Note**: Phase 5 feature-validator outputs validation result JSON. Orchestrator interprets result and decides GOTO or finalize. feature-validator has no retry authority.

**UPSERT Pattern for Retry**: When orchestrator retries a phase (e.g., Step 3 after validation failure), the phase agent MUST use UPSERT semantics - replace existing section if present, append if not. This prevents content duplication when ## Acceptance Criteria already exists.
```

### Resume Logic

```python
def determine_resume_point(feature_path):
    content = Read(feature_path)

    if "## Tasks" in content:
        return "Step 5"  # Validation only
    elif "## Acceptance Criteria" in content:
        return "Step 4"  # WBS from here
    elif "## Dependencies" in content:
        return "Step 3"  # Design from here
    elif "## Background" in content:
        return "Step 2"  # Investigation from here
    else:
        return "Step 1"  # Philosophy from here
```

### Agent Responsibilities

| Agent | Input | Output | Incorporates |
|-------|-------|--------|--------------|
| philosophy-definer | User requirements | Philosophy/Problem/Goal JSON | - |
| tech-investigator | Feature file | Dependencies/Impact JSON | - |
| ac-designer | Feature file | AC table/details JSON | philosophy-deriver logic |
| wbs-generator | Feature file | Tasks/Contract JSON | ac-task-aligner logic |
| feature-validator | Feature file | Validation result JSON | - |

**Note**: "Incorporates" means **pattern reference** - the new agent's definition includes copy-pasted logic patterns from the referenced existing agents (philosophy-deriver, ac-task-aligner). This is NOT a runtime dependency; the new agents do not dispatch the existing agents. The purpose is to reuse validated analysis patterns for consistency.

### Rollback Plan

If issues arise after deployment:
1. Revert to previous feature-creator/SKILL.md from git
2. Delete new agent files (.claude/agents/*-definer.md, etc.)
3. Revert CLAUDE.md changes
4. Create follow-up feature for fix

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | 612 | [DONE] | Extensible Orchestrator Design - required to prevent hardcoded 5-phase sequence becoming technical debt |

<!-- Dependency Types: Predecessor (BLOCKING), Successor (informational), Related (reference) -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-applied] Phase0-RefCheck iter1: Referenced Feature 612 created. F610 remains [BLOCKED] until F612 is [DONE].
- [resolved-applied] Phase1-Uncertain iter1: The distinction between philosophy-definer and philosophy-deriver IS documented in Impact Analysis (line 63) though parenthetically. → Added explicit explanation to Goal section.
- [wontfix] Phase1-Uncertain iter1: Feature-610.md already documents sampling rationale in AC Details. User confirmed representative sampling maintained.
- [resolved-applied] Phase2-Maintainability iter1: Philosophy claims 'Context Isolation' but no AC verified effect. → Added AC#16 for context isolation effect verification.
- [wontfix] Phase2-Maintainability iter1: AC#13 representative sampling - user confirmed representative sampling maintained.
- [resolved-applied] Phase2-Maintainability iter1: Task#6 atomic unit unclear. → User chose: AC#1 verifies file existence only, separate Tasks verify content.
- [resolved-applied] Phase2-Maintainability iter1: Retry content duplication problem. → Added UPSERT pattern documentation to Implementation Contract.
- [resolved-applied] Phase2-Maintainability iter1: 'Incorporates' meaning vague. → Clarified as "pattern reference" (copy-paste logic, not runtime dependency).
- [resolved-applied] Phase2-Maintainability iter1: AC count note. → Updated to "16 ACs (with 10 sub-ACs, totaling 24 verification points)".
- [resolved-applied] Phase3-ACValidation iter1: AC#1-6 format - Method 'Glob' was already correct.
- [resolved-applied] Phase3-ACValidation iter1: AC#7a-7e Type changed from 'file' to 'code' for content search.
- [resolved-applied] Phase3-ACValidation iter1: AC#8a-8e Method=Grep(CLAUDE.md) was already specified in table.
- [resolved-applied] Phase3-ACValidation iter1: AC#9-15 Method columns were already specified in table.
- [resolved-applied] Phase3-ACValidation iter1: AC#12 Method changed to '/reference-checker' (tool type).
- [resolved-applied] Phase3-ACValidation iter1: AC table column consistency - all ACs now use consistent 7-column format.
- [wontfix] Phase1-Uncertain iter1: AC#9 resume logic pattern - user confirmed current flexible pattern maintained.
- [resolved-applied] Phase1-Uncertain iter1: AC#13 states '(representative check)' but doesn't explain why philosophy-definer is representative - suggestion to explain sampling rationale is reasonable but may be pedantic for structural verification
- [resolved-applied] Phase1-Uncertain iter2: Task#6 covers 4 ACs (AC#1,7,9,15) violating AC:Task 1:1 principle. feature-quality allows merging 'same edit operation' tasks, but ac-task-aligner says '1:1 is mandatory'. 4:1 ratio acceptable but all ACs modify the same file. Principle interpretation unclear
- [resolved-applied] Phase1-Uncertain iter3: AC#15 pattern 'Task\\(subagent_type.*philosophy-definer' may be too specific to dispatch syntax variations. Pattern can match documented Implementation Contract syntax but more flexible pattern could be beneficial
- [wontfix] Phase1-Uncertain iter4: AC#13 representative check approach - user confirmed representative sampling as project policy.
- [resolved-applied] Phase1-Uncertain iter4: Review Notes pending items resolved via POST-LOOP user confirmation.
- [wontfix] Phase1-Uncertain iter5: Review Notes pending items workflow - whether pending items should be resolved before FL review vs during FL review is process judgment call, not correctness issue
- [wontfix] Phase1-Uncertain iter8: AC#13 representative sampling concern re-raised but already documented as policy decision in iter6. Project maintains representative sampling approach with documented rationale
- [wontfix] Phase1-Uncertain iter6: AC#9 pattern 'determine_resume|return.*Step' can match documented pseudocode. Alternative patterns suggested but original pattern is not incorrect - optimization suggestion, not correctness issue
- [wontfix] Phase1-Uncertain iter6: AC#13 representative sampling is documented design decision with rationale. Whether to verify all 5 agents vs one representative is policy question, not correctness issue
- [resolved-applied] Phase1-Uncertain iter1: Implementation Contract - Agent Responsibilities table shows 'Incorporates' column with 'philosophy-deriver' and 'ac-task-aligner' for certain agents, but these existing agents should be clearly referenced or the relationship should be explained.
- [resolved-applied] Phase1-Uncertain iter2: AC table AC#13-17 could use more specific patterns to ensure agents are registered in correct table with model verification (e.g., 'philosophy-definer.*haiku')
- [resolved-applied] Phase1-Uncertain iter2: Implementation Contract Phase Execution Flow could specify explicit model parameter in each Task dispatch for clarity
- [resolved-applied] Phase1-Uncertain iter2: Impact Analysis should clarify whether Context:fork skills list in CLAUDE.md needs updating for new agent invocation patterns
- [resolved-applied] Phase1-Uncertain iter3: Impact Analysis Context:fork consideration - new agents are Task-dispatched (general-purpose), not context:fork skills, but clarification would improve documentation
- [resolved-applied] Phase1-Uncertain iter4: Impact Analysis documentation improvement to clarify that new agents are Task-dispatched (general-purpose), not context:fork skills
- [wontfix] Phase1-Uncertain iter8: AC#12 pattern 'determine_resume|return.*Step' should match implementation but clarification in AC Details about flexible resume logic detection would be beneficial - Pattern is flexible enough to catch resume logic variations
- [wontfix] Phase1-Uncertain iter8: index-features.md updates typically handled by finalizer agent, but explicit AC/Task could be added for clarity - Finalizer agent handles this automatically per CLAUDE.md Subagent Strategy

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| F612 example config outdated | F612 shows opus/sonnet for phases 1,4,5 but F610 uses haiku | 残課題 - low priority | F612 design is pattern reference, not binding |
| F612 broken links | Relative paths incorrect | 残課題 - low priority | F612 is [DONE], minor doc issue |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-24 | DEVIATION | Bash | ac-static-verifier --ac-type file | exit code 1, 0/6 passed (backtick escaping issue in Expected column) |
| 2026-01-24 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit code 1, 1/6 passed (pattern escaping issue) |
| 2026-01-24 | RESOLVED | Opus | Manual verification | All 16 ACs verified PASS via direct Glob/Grep |
| 2026-01-24 | FIX | Opus | Model alignment | Fixed Phase 1,4,5 models in SKILL.md to match F610 Implementation Contract |
| 2026-01-24 | DEVIATION | feature-reviewer | Post-review | NEEDS_REVISION: model docs inconsistency |
| 2026-01-24 | RESOLVED | Opus | Fix | Updated Phase-specific models section in SKILL.md |
| 2026-01-24 | NOTE | feature-reviewer | Doc-check | F612 design doc has outdated examples - OUT OF SCOPE |

## Links

- [feature-612.md](feature-612.md)
- [index-features.md](index-features.md)
- [feature-creator SKILL.md](../../.claude/skills/feature-creator/SKILL.md) - Current implementation
- [feature-template.md](reference/feature-template.md) - Template reference
