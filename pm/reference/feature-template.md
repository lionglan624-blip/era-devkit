# Feature Template

Template for creating new feature specifications with strict AC definitions.

**MANDATORY**: Before writing ANY feature, read `Skill(feature-quality)` and the type-specific guide (ENGINE.md, KOJO.md, etc.). Skipping this causes preventable FL review iterations.

**Language Policy**: All content in feature files MUST be in English (descriptions, notes, logs). Only in-game dialogue text (inside ERB/YAML) is Japanese.

---

## Feature Granularity Guide

### Principle

**1 Feature = 1 Functional Unit AND Within Volume Limits**

### Criteria by Type

| Type | Functional Unit | Volume Limit | AC Count |
|------|-----------------|--------------|:--------:|
| **kojo** | 1 COM | ~2,500 lines (10 chars) | 12 |
| **erb** | Related processes | ~500 lines | 8-15 |
| **engine** | 1 feature | ~300 lines | 8-15 |
| **infra** | Change set | - | 8-15 |
| **research** | Investigation report | - | 3-5 |

### Type Definitions

| Type | Responsibility | Primary Artifact | Notes |
|------|----------------|------------------|-------|
| kojo | Dialogue implementation | .ERB files | 1 COM = 1 Feature |
| erb | ERB game logic | .ERB files | |
| engine | C# engine implementation | .cs files | |
| infra | Workflow/Review | .md, config files | Post-Phase Review, etc. |
| **research** | **Feature to create Features** | **feature-{ID}.md** | Planning Feature (create sub-features for next Phase) |

**research type characteristics**:
- **Primary artifact is Feature files**: Create new feature-{ID}.md as analysis output
- **Used for Phase Planning**: Create sub-features for next Phase after completing each Phase
- **Output must be explicit**: Specify "Feature to create Features" in Summary

### Decision Flow

1. Define logical "1 feature"
2. Check volume limits
3. Split if exceeded

---

## Agent Selection

**For Feature file creation**: Create [DRAFT] manually or use `/fc` command to complete

| Artifact | Agent/Command |
|----------|---------------|
| designs/{name}.md | spec-writer |
| feature-{ID}.md [DRAFT] | Orchestrator (manual Background creation) |
| feature-{ID}.md [PROPOSED] | `/fc {ID}` command (generates AC/Tasks) |

---

## Section Ownership

Each /fc phase agent writes specific sections. This table is the SSOT for which agent owns which section.

| Section | Owner Agent | Phase | Notes |
|---------|-------------|:-----:|-------|
| Status | orchestrator | - | Set by /fc, /fl, /run |
| Scope Discipline | (static) | DRAFT | Copy verbatim |
| Type | (static) | DRAFT | Set at creation |
| Deviation Context | /run Phase 9 | DRAFT | Optional, delete if N/A |
| Review Context | FL POST-LOOP | DRAFT | Optional, delete if N/A |
| Background | consensus-synthesizer | 1 | Philosophy/Problem/Goal |
| Root Cause Analysis | consensus-synthesizer | 1 | 5 Whys + Symptom vs Root Cause |
| Related Features | consensus-synthesizer | 1 | |
| Feasibility Assessment | consensus-synthesizer | 1 | |
| Impact Analysis | consensus-synthesizer | 1 | |
| Technical Constraints | consensus-synthesizer | 1 | |
| Risks | consensus-synthesizer | 1 | |
| Baseline Measurement | consensus-synthesizer | 1 | DELETE for kojo/research |
| AC Design Constraints | consensus-synthesizer | 1 | |
| Dependencies | consensus-synthesizer | 1 | |
| Acceptance Criteria | ac-designer | 3 | Includes Philosophy Derivation, Goal Coverage |
| Technical Design | tech-designer | 4 | |
| Tasks | wbs-generator | 5 | |
| Implementation Contract | wbs-generator | 5 | |
| Mandatory Handoffs | wbs-generator | 5 | |
| Execution Log | wbs-generator | 5 | |
| Review Notes | FL workflow | - | Managed by orchestrator |
| Links | wbs-generator | 5 | Populated from Related Features + Dependencies |

---

## Template

```markdown
# Feature {ID}: {Title}

## Status: [PROPOSED]

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

## Type: {kojo | erb | engine | infra | research}

## Deviation Context
<!-- Optional: Written by /run Phase 9. Raw facts only. Delete if not applicable. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F{parent_ID} |
| Discovery Phase | Phase {N} |
| Timestamp | {YYYY-MM-DD} |

### Observable Symptom
{What happened. Facts only, no interpretation.}

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `{failed command}` |
| Exit Code | {N} |
| Error Output | `{first 200 chars of stderr}` |
| Expected | {expected behavior} |
| Actual | {actual behavior} |

### Files Involved
| File | Relevance |
|------|-----------|
| {path} | {why relevant} |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| {description} | {FAIL/PARTIAL} | {reason} |
| None | - | - |

### Parent Session Observations
{Observations from parent session. Max 200 words.}

## Review Context
<!-- Optional: Written by FL POST-LOOP Step 6.3. Review findings only. Delete if not applicable. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F{parent_ID} |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | {YYYY-MM-DD} |

### Identified Gap
{What the philosophy-deriver identified as missing. Facts only, no interpretation.}

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | {philosophy-deriver / task-comparator} |
| Derived Task | "{derived task description}" |
| Comparison Result | "{task-comparator gap description}" |
| DEFER Reason | "{why ADOPT is technically impossible}" |

### Files Involved
| File | Relevance |
|------|-----------|
| {path} | {why relevant} |

### Parent Review Observations
{Observations from FL review session. Max 200 words.}

## Background

### Philosophy (Mid-term Vision)
{The mid-term goal, policy, or philosophy this Feature belongs to}
{Example: "Regression tests should function as whole-game guarantees"}
{Example: "Subagents should reference Skills and execute correctly in one shot"}
{Note: If inheriting philosophy from prior Features like F095, F105, mention explicitly}

### Problem (Current Issue)
{Specific problem description — must explain WHY at root-cause level, not just WHAT happened}
{Example: "24 scenarios use exit 0 = PASS, but don't guarantee game logic correctness"}
{Note: /fc Phase 1 populates this from investigation when created via Phase 9.8}

### Goal (What to Achieve)
{Specific goal within scope}
{Concrete step toward realizing Philosophy}
{Note: /fc Phase 1 populates this from investigation when created via Phase 9.8}

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why? | {answer} | {file:line} |
| 2 | Why? | {answer} | {file:line} |
| 3 | Why? | {answer} | {file:line} |
| 4 | Why? | {answer} | {file:line} |
| 5 | Why (Root)? | {root cause} | {file:line} |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | {observable symptom} | {underlying cause} |
| Where | {surface location} | {structural origin} |
| Fix | {band-aid fix} | {proper solution} |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F{ID} | {status} | {description} |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| {criterion} | FEASIBLE/NEEDS_REVISION/NOT_FEASIBLE | {evidence} |

**Verdict**: FEASIBLE / NEEDS_REVISION / NOT_FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| {area} | HIGH/MEDIUM/LOW | {description} |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| {constraint} | {source} | {impact} |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| {risk} | HIGH/MEDIUM/LOW | HIGH/MEDIUM/LOW | {mitigation} |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->
<!-- DELETE this section for kojo/research types (no baseline needed). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| {metric} | {command} | {value} | {note} |

**Baseline File**: `.tmp/baseline-{ID}.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | {constraint description} | {file:line or investigation} | {how AC must account for this} |

### Constraint Details

**C{N}: {Constraint Name}**
- **Source**: {how discovered}
- **Verification**: {how to confirm constraint still holds}
- **AC Impact**: {specific guidance for ac-designer}

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| {type} | F{ID} | {status} | {description} |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "{claim from Philosophy}" | {requirement} | AC#{N} |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | {description} | {type} | {method} | {matcher} | {expected} | [ ] |

### AC Details

<!-- Threshold matchers (gte, gt, lt, lte, count_equals): Details block MANDATORY with Derivation.
     Non-threshold matchers (matches, exists, etc.): Details block OPTIONAL. -->

**AC#1: {Description}**  <!-- Required only for threshold matchers -->
- **Test**: `{command}`
- **Expected**: `{output/pattern}`
- **Derivation**: {why this number — e.g., "17 ITEM functions in ERB source, 1:1 migration"}
- **Rationale**: {why this AC is necessary}

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | {goal item from Goal section} | AC#{N} |

---

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

{Selected implementation approach with rationale}
{How this approach satisfies the ACs}

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | {implementation approach for AC#1} |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| {decision point} | {A, B, C} | {selected} | {why} |

### Interfaces / Data Structures

<!-- Optional: Define new interfaces, data structures, or APIs if applicable -->

### Upstream Issues

<!-- Optional: Issues discovered during Technical Design that require upstream changes (AC gaps, constraint gaps, interface API gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed.
     Content may be empty if no upstream issues found. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | {HOW to achieve AC1} | | [ ] |
| 2 | 2 | {Investigation-required task} | [I] | [ ] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

<!--
Use this section when the feature requires specific implementation steps
that must be followed exactly (e.g., subagent orchestration, multi-phase audits).
Delete this section if not needed.
-->

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | {agent} | {model} | {input} | {output} |

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| {issue} | {why defer} | Feature | F{ID} | Task#{N} | [ ] | |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-23 | test | - | unit test | FAIL:63/160 |
| 2025-12-23 | debug | debugger | implementation fix | FIXED |
| 2025-12-23 | test | - | unit test (retry) | PASS:160/160 |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

---

## Links
[Related](feature-XXX.md)
```

---

## AC Definition Guide

**SSOT Reference**: See `Skill(testing)` for details on AC Type, Matcher, and positive/negative requirements.

→ [testing/SKILL.md](../../../.claude/skills/testing/SKILL.md)

**Status**: `[ ]` Not tested | `[x]` PASS | `[-]` FAIL | `[B]` BLOCKED

**Completion Rule**:
- All `[x]` → Feature can reach `[DONE]`
- Any `[-]` → Feature **cannot** reach `[DONE]` until fixed
- Any `[B]` → Feature **cannot** reach `[DONE]` without explicit user waive. `[B]` AC must be tracked in 残課題 with concrete destination

---

## Example

**Basic**:
```markdown
| 1 | Affection output | output | --inject | contains | "最近一緒にいると" | [ ] |
```

**With mock_rand** (for DATALIST):
```markdown
| 2 | COM_311 | output | --unit | contains | "温かいですわ" | [ ] |
```
Test: `dotnet run ... --unit "tests/ac2.json"`
