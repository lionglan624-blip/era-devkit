---
name: fl-workflow
description: Progressive Disclosure feature review workflow
allowed-tools: Read, Write, Edit, Glob, Grep, Task, Bash, TaskCreate, TaskUpdate, TaskList
# NOTE: No `requires:` - phases are loaded on-demand via Read() chaining
# to preserve Progressive Disclosure (see CLAUDE.md)
---

# fl-workflow Skill

Progressive Disclosure feature review workflow.

## Zero Debt Upfront (Anti-YAGNI)

**CRITICAL**: This project does NOT follow YAGNI. The following suggestions are **FORBIDDEN**:

| Forbidden Pattern | Why |
|-------------------|-----|
| "Match existing pattern" | Existing may be debt; new design may be correct |
| "Keep it simple/minimal" | Causes future cost increase |
| "YAGNI" / "Not needed now" | Violates Zero Debt Upfront principle |
| "Over-engineering" | Proper design investment is not over-engineering |

**Correct Approach**:
- Research future extension patterns before design
- Reference similar systems / prior implementations
- Choose technically correct implementation from the start

## Execution

**Single ID only**: `/fl {ID}` executes all phases directly for one target.

**Example**: `/fl 529` → Orchestrator calls feature-reviewer, ac-validator, etc. directly.

### Target Resolution

When `/fl {ID}` is invoked:
- If `{ID}` is numeric → `target_type = "feature"`, file = `pm/features/feature-{ID}.md`
- If `{ID}` is a file path → infer from location:
  - `.claude/commands/` → `target_type = "command"`
  - `.claude/agents/` → `target_type = "agent"`
  - `.claude/skills/` → `target_type = "skill"`
  - Otherwise → `target_type = "doc"`

## Overview

| Phase | Name | Skill |
|:-----:|------|-------|
| 1 | Reference Check | Read(.claude/skills/fl-workflow/PHASE-1.md) |
| 2 | Review-Validate-Apply | Read(.claude/skills/fl-workflow/PHASE-2.md) |
| 3 | Maintainability | Read(.claude/skills/fl-workflow/PHASE-3.md) |
| 4 | AC Validation | Read(.claude/skills/fl-workflow/PHASE-4.md) |
| 5 | Feasibility | Read(.claude/skills/fl-workflow/PHASE-5.md) |
| 6 | Planning Validation | Read(.claude/skills/fl-workflow/PHASE-6.md) |
| 7 | Final Reference Check | Read(.claude/skills/fl-workflow/PHASE-7.md) |
| Post | Post-Loop (Status → Index Sync → Report) | Read(.claude/skills/fl-workflow/POST-LOOP.md) |

## Pacing

**急がなくていい。ゆっくり丁寧に。**

- 各Phaseを一つずつ確実に完了する
- 次のPhaseに進む前に、ルーティングテーブルを確認する
- 判断に迷ったら立ち止まる

## Global Rules (Always Applied)

### Workflow Artifact Immutability (F814 Lesson)

**CRITICAL: Orchestrator MUST NOT modify workflow artifacts during /fl execution.**

| Protected | Examples |
|-----------|---------|
| `.claude/skills/**/*.md` | SKILL.md, PHASE-*.md |
| `.claude/commands/*.md` | fc.md, fl.md, run.md |
| `.claude/agents/*.md` | Agent definitions |

Modifying these files to bypass validation gates is **self-hacking** — the orchestrator rewriting its own rules to pass checks that would otherwise fail.

**If a gate blocks progress**: STOP → Report to user. Do not alter the gate.

### Phase Routing Table (MANDATORY)

**ルーティングは機械的に実行。判断しない。**

| Current Phase | Condition | Next Phase |
|:-------------:|-----------|:----------:|
| 1 | ref_check.status == OK (feature) | 2 |
| 1 | ref_check.status == OK (non-feature) | POST-LOOP |
| 1 | ref_check.status == NEEDS_REVISION (non-critical fixes applied) | 1 (re-run) |
| 1 | ref_check.status == NEEDS_REVISION (critical only) | 2 (persist_pending) |
| 2 | issues.count == 0 (feature) | 3 |
| 2 | issues.count == 0 (non-feature) | POST-LOOP |
| 2 | applied_fixes > 0 | 2 (re-run) * |
| 2 | applied_fixes == 0 AND all issues pending/invalid (Pending-Gate) | POST-LOOP ** |
| 2 | applied_fixes == 0 | 3 |
| 3 | target_type != "feature" | POST-LOOP |
| 3 | applied_fixes > 0 | 2 * |
| 3 | applied_fixes == 0 | 4 |
| 4 | applied_fixes > 0 | 2 * |
| 4 | applied_fixes == 0 | 5 |
| 5 | applied_fixes > 0 | 2 * |
| 5 | applied_fixes == 0 | 6 |
| 6 | applied_fixes > 0 | 2 * |
| 6 | applied_fixes == 0 | 7 |
| 7 | applied_fixes > 0 | 2 * |
| 7 | applied_fixes == 0 | POST-LOOP |

**\* Loop-back to Phase 2** (Review-Validate-Apply), not Phase 1 (Reference Check). Max Iteration Exception: When iteration == 10, "→ 2" routes become "→ Next Phase" (Forward-Only Mode). See Loop Control section.

**\*\* Pending-Gate**: When Phase 2 finds issues and at least one is user-pending (the rest are pending or validator-invalid), with applied_fixes == 0, skip Phases 3-8 and go directly to POST-LOOP. Phases 3-8 cannot resolve user-decision issues. "All invalid" (zero pending) does NOT trigger Pending-Gate — proceeds normally to Phase 3. See PHASE-2.md Step 2.6.5.

### Entry Check Template (MANDATORY)

各Phase開始時に実行:
```
## Entry Check
- Previous Phase: {N-1}
- Condition met: {condition from routing table}
- Expected Next: Phase {N}
- Actual: Phase {N}
- Match confirmed → proceed
```

### Declare Next Phase Template (MANDATORY)

各Phase終了時に実行:
```
## Declare Next Phase
- Current Phase: {N}
- Condition: {condition}
- Routing Table → Next: Phase {X}
- User Report: **PROHIBITED**

TaskUpdate: "Iteration {I}/10: Phase {X}" → status: "pending"
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
→ あなたの次のアクションは必ずツール呼び出しであること。テキスト出力は禁止。
→ Read()後は即座に次Phase実行。間にテキスト出力を挟んではならない。
```

**Do not judge. Follow the table. Do not report. Read()後は即座に次Phase実行。**

### Loop Control

```
MAX_ITERATIONS: 10
```

**Max Iteration Behavior (Forward-Only Mode)**:

When iteration reaches MAX_ITERATIONS:
1. **All phases execute normally** including Phase 2 (Review → Validate → Apply)
2. Fixes are applied as usual in every phase
3. Routing Table's "→ Phase 2" loop-back rules are **disabled** (forward only)
4. Always proceed to next phase (Phase 2 → 3 → 4 → ... → POST-LOOP)
5. Track `forward_fixes_total` (total fixes applied during Forward-Only Mode)

| Normal Routing | Max Iter Routing |
|----------------|------------------|
| applied_fixes > 0 → Phase 2 | applied_fixes > 0 → **Next Phase** (forward_fixes_total += applied_fixes) |
| applied_fixes == 0 → Next Phase | applied_fixes == 0 → Next Phase |

**Check Timing**: At phase transition (when consulting Routing Table).

**Key**: Forward-Only disables loop-back ONLY. Review, validation, and auto-fix all execute normally in every phase (including Phase 2). The difference is that fixes are not re-verified by Phase 2 before proceeding.

**POST-LOOP Judgment** (forward_fixes_total determines status):

| forward_fixes_total | Meaning | POST-LOOP Status |
|:-------------------:|---------|------------------|
| 0 | All phases clean, no unverified fixes | → REVIEWED |
| > 0 | Fixes applied but Phase 2 loop-back skipped | → `/fl` 再実行 |

**Rationale**: Forward-Only continues through all phases to maximize auto-fix coverage in a single pass. But any fix applied without Phase 2 re-verification means the final state is unconfirmed. `forward_fixes_total == 0` guarantees no unverified changes exist.

### Iteration Counter (MANDATORY)

Every TaskUpdate MUST include iteration counter:
```
TaskUpdate(subject: "Iteration {N}/10: Phase {X}", status: "in_progress")
```

### Subagent Dispatch Matrix

| Target Type | Drift Checker | Reviewer | Maintainability | AC Validator | Feasibility | Planning Validator |
|-------------|---------------|----------|-----------------|--------------|-------------|-------------------|
| feature | drift-checker (Phase 1.4.5, if drift_candidates) | feature-reviewer (SEMANTIC=opus, STRUCTURAL=sonnet iter1 only) | feature-reviewer (all types) | ac-validator | feasibility-checker (sonnet) | planning-validator (sonnet, research only) |
| command | N/A | doc-reviewer | N/A | N/A | N/A | N/A |
| agent | N/A | doc-reviewer | N/A | N/A | N/A | N/A |
| skill | N/A | doc-reviewer | N/A | N/A | N/A | N/A |
| doc | N/A | doc-reviewer | N/A | N/A | N/A | N/A |

**Phase 2 Model Summary**: STRUCTURAL=sonnet (iter1 only), SEMANTIC=sonnet→opus escalation (sonnet first; if 0 issues → 1x opus verification), VALIDATOR=sonnet (always). See PHASE-2.md.

### Feature Type Fast-Path Routing

**Purpose**: Optimize FL for feature types with simpler validation requirements.

| Feature Type | Phase Skip | Rationale |
|--------------|------------|-----------|
| kojo | Phase 5 (Feasibility), Phase 6 (Planning) | Kojo features have fixed structure; feasibility is content-only |
| research | Phase 5 (Feasibility) | Research outputs are features, not code |
| erb/engine/infra | (none) | Full validation required |

**Implementation**:

```
IF feature.type == "kojo":
    # Skip Phase 5 and 6
    routing_override = {
        4: 7,  # After AC Validation → Final Reference Check (skip 5, 6)
    }
ELIF feature.type == "research":
    # Skip Phase 5 only
    routing_override = {
        4: 6,  # After AC Validation → Planning Validation (skip 5)
    }
ELSE:
    routing_override = {}  # Use default routing
```

**Fast-Path Conditions**:
- Only applies when `applied_fixes == 0` at phase boundary
- If `applied_fixes > 0`, follow normal routing (return to Phase 2)
- Max Iteration Mode overrides fast-path (all phases execute)

### Reviewer Selection

Dispatch rule: Pass target info (type + path or ID). No additional context.

### Subagent Output Format (MANDATORY)

**全 subagent は JSON のみを返す。**

```json
{"status": "OK"}
```
または
```json
{"status": "NEEDS_REVISION", "issues": [...]}
```

**Orchestrator は JSON のみを処理する。それ以外の出力は無視する。**

### JSON Parse Failure Handling (MANDATORY)

**Subagentの出力がJSONとしてparse不可能な場合**:

```
parse_result = attempt_json_parse(subagent_output)
IF parse_result.failed:
    persist_pending({
        description: "Subagent returned non-JSON output",
        raw_output: subagent_output.first_500_chars,
        phase: current_phase,
        subagent: subagent_type
    }, iteration, "Parse-Failure")
    # Treat as zero issues found - proceed to next phase
    result = {"status": "OK", "issues": []}
```

**Rationale**: Parse失敗は「判断不能」状態。空のissuesとして扱いworkflow継続を保証。POST-LOOPでユーザーに報告。

### Maintainability Review Scope by Feature Type

| Feature Type | Responsibility Clarity | Philosophy Coverage | Task Coverage | Technical Debt | Maintainability | Extensibility |
|--------------|:----------------------:|:-------------------:|:-------------:|:--------------:|:---------------:|:-------------:|
| engine | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| erb | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| infra | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| kojo | ✅ | ✅ | ✅ | - | - | - |
| research | ✅ | ✅ | ✅ | - | - | - |

### Planning Validator Mode Detection

Detect mode by status:
- **pre**: Status != `[DONE]` → validate design before/during `/run`
- **post**: Status == `[DONE]` → validate coverage after `/run` (no status change)

### Orchestrator Prohibitions

- Judging reviewer issues as valid/invalid by yourself
- Applying fixes without dispatching Validation subagent
- Reading SSOT yourself and judging as "correct"
- **Proceeding to next Phase without updating TaskUpdate**
- **Managing pending_user in memory only** - must write to feature.md Review Notes with `[pending]` tag
- **Reporting to user before POST-LOOP** (no mid-loop summaries)
- **Evaluating or judging route decisions** (execute route logic mechanically)

### Mid-Loop Report Prohibition (MANDATORY)

**POST-LOOP以外でのユーザー報告は禁止。**

以下の状況でも報告禁止:
- 重要な変更が適用された場合（新しい依存性追加など）
- MAX_ITERATIONS到達時
- Phase遷移時
- 「確認したほうがよい」と感じた時 ← **この感覚自体が違反の兆候**

**唯一の例外**:
- ユーザーが明示的に "stop FL" / "cancel FL"
- CLAUDE.md Escalation Policy条件（3連続失敗、フレームワークバグ）

**Phase遷移時チェック**:
```
□ ユーザーに報告しようとしている? → 禁止
□ 確認を求めようとしている? → 禁止
□ 「重要なので説明したい」? → 禁止
→ 次のPhaseに即座に進む
```

### Loop Detection Functions

**`is_loop(issue)`**: Returns true if `issue.location + issue.issue` appeared in a previous fix. Detection: Read feature.md Review Notes `[fix]` entries and check `fix_entries.any(entry => entry.location == issue.location && entry.issue == issue.issue)`. Parse format: `- [fix] {phase} iter{N}: {location} | {issue_text}` — split on ` | ` after the first `: ` to extract location and issue_text.

**`is_orchestrator_decidable(issue)`**: Returns true if fix is **additive** and **derivable from existing content or established project patterns**, OR if fix is a **documentation correction verifiable against source code or SSOT**. Returns false if fix requires genuinely novel design decisions with no applicable precedent.

| Decidable (auto-apply) | NOT Decidable (user-pending) |
|------------------------|------------------------------|
| AC/Task Definition Table row addition | AC/Task removal (reduce count) |
| AC Coverage row addition (for existing ACs) | Constraint modifications that alter verified design boundaries |
| AC Details block addition (for threshold-matcher ACs) | Genuinely novel architectural choice with no project precedent |
| Implementation Contract subsection addition (for existing Tasks) | |
| **Design decision with established project precedent** (e.g., F802 delegation pattern, fail-loud stub pattern) | |
| **DI pattern decidable from MS DI conventions** (e.g., null sentinel for nullable interface, AddSingleton for new class) | |
| **Interface signature extension** where single obvious option exists (e.g., add parameter to match call sites) | |
| **Doc fix: feature.md content correction verifiable against source code** | |
| **Doc fix: AC matcher improvement (fragile → robust)** | |
| **Doc fix: template compliance (missing required section)** | |

Detection: Check if `issue.fix` proposes (a) adding content that is fully determined by existing sections (e.g., AC Coverage derives from AC Details, Implementation Contract derives from Tasks/AC tables), (b) correcting feature.md content where the correct value is **mechanically verifiable** against source code, SSOT, or template, OR (c) a design choice where the same pattern was already applied in a predecessor/sibling feature. Key patterns: "add AC", "add Task", "add row", "add subsection", "追加" with additive content, "fix signature to match source", "split AC for robustness", "add missing template section", "follow F{NNN} pattern".

**Precedent-Based Decidability** (F805 lesson — all 12 user-pending items were auto-resolvable):

When a design question has an established pattern in predecessor/sibling features, the orchestrator SHOULD auto-apply the pattern instead of escalating to user-pending. Examples:
- Delegation vs reimplementation → follow predecessor's delegation pattern (F802)
- Sealed class testability → real instances with mocked deps (F802 pattern)
- DI nullable resolution → null sentinel registration (MS DI convention)
- Interface method default → `throw NotImplementedException` (fail-loud convention)
- Interface signature gap → add parameter matching call sites (single obvious option)

**Escalation to user-pending is ONLY for**:
1. Genuinely novel architectural choice with no project precedent
2. Scope reduction (AC/Task removal)
3. Constraint modifications that alter verified design boundaries

**Documentation Fix Criteria** (all must be true):
1. Target is feature.md content (not source code)
2. Correct value is verifiable against source code, SSOT, or template (not opinion-based)
3. Fix does not change the feature's functional scope or design intent

**`is_pending(issue)`**: Evaluation order matters — **decidability overrides severity**:

```python
def is_pending(issue):
    if is_orchestrator_decidable(issue):
        return False  # Priority 1: mechanical additions always auto-apply
    if is_loop(issue):
        return True   # Priority 2: loop detection
    if issue.severity == "critical":
        return True   # Priority 3: critical non-decidable issues
    return False
```

Pending issues cannot be auto-fixed and are persisted via `persist_pending()`.

**Note**: `is_loop()` reads from feature.md (file-based), not in-memory. This survives context compression and session crashes.

### Counter Scoping

**`applied_fixes`**: Per-phase local counter. Each phase initializes `applied_fixes = 0` at entry and increments within its own fix loop. Routing at phase end checks this phase-local count.

**`forward_fixes_total`**: Cross-phase accumulator used ONLY in Forward-Only Mode. Each phase adds `forward_fixes_total += applied_fixes` when in Forward-Only Mode. Used by POST-LOOP to determine final status.

**`iteration`**: Cross-phase loop counter. Incremented at Phase 2 entry (the loop-back target). MAX_ITERATIONS = 10.

### persist_pending Usage Guidance

**persist_pending Definition**: See section "persist_pending Definition" below. Writes to **feature.md Review Notes section** with `[pending]` tag. Do NOT create separate files like `pending_user.txt`.

This function is used across multiple Phase files. Always read this SKILL.md before proceeding to Phase files.

### Orchestrator-Decidable Issues (NOT persist_pending)

The following issues are **NOT user-pending**. Orchestrator autonomously applies fixes:

| Issue Type | Agent | Action |
|------------|-------|--------|
| AC:Task 1:1 alignment | ac-task-aligner | Apply fix directly (unless BLOCKED) |
| AC/Task Definition Table row addition | Any reviewer/validator | Apply fix directly |
| AC Coverage row addition (existing ACs) | Any reviewer/validator | Apply fix directly |
| AC Details block addition (threshold-matcher ACs) | Any reviewer/validator | Apply fix directly |
| Implementation Contract subsection addition (existing Tasks) | Any reviewer/validator | Apply fix directly |
| Doc fix: feature.md content correction (verifiable against source) | Any reviewer/validator | Apply fix directly |
| Doc fix: AC matcher improvement (fragile → robust) | Any reviewer/validator | Apply fix directly |
| Doc fix: template compliance (missing required section) | Any reviewer/validator | Apply fix directly |

**Judgment Criteria**:

| Criterion | orchestrator_decidable | user_pending |
|-----------|:-----:|:-----:|
| Fix is 100% derivable from existing content | Yes | - |
| Fix corrects feature.md content verifiable against source/SSOT | Yes | - |
| Fix improves AC matcher robustness without changing intent | Yes | - |
| Fix adds missing template-required section | Yes | - |
| Fix requires new design decisions not in existing sources | - | Yes |
| Fix removes content (reduce AC/Task count) | - | Yes |
| Fix alters verified design boundary (Constraint modification) | - | Yes |

**User-pending required** (scope reduction / genuinely novel decisions only):

| Issue Type | Reason |
|------------|--------|
| AC removal (reduce AC count) | Reduces user-approved scope |
| Task removal (reduce Task count) | Reduces user-approved scope |
| Constraint modification altering design boundary | Alters verified design boundary |
| Genuinely novel architectural choice with no project precedent | Not derivable from any existing source or pattern |

**NOT user-pending** (auto-apply even if they look like "design decisions"):

| Issue Type | Reason |
|------------|--------|
| Design choice with predecessor/sibling feature precedent | Derivable from established project pattern |
| DI convention (null sentinel, registration, factory) | Derivable from MS DI standard conventions |
| Interface signature extension with single obvious option | Derivable from call site analysis |
| Stub default value (fail-loud NotImplementedException) | Derivable from established project convention |

**Rationale**: Additions derivable from existing content are mechanical. Design decisions with established project precedents are also mechanical — the pattern is already decided, only the application is new. Scope reductions and genuinely novel choices (no applicable precedent anywhere in the project) require user confirmation. See `is_orchestrator_decidable()` in Loop Detection Functions.

**F805 Lesson**: 12/12 user-pending items were auto-resolvable from F802 patterns and DI conventions. Over-escalation to user-pending wastes POST-LOOP cycles and blocks FL progress.

### persist_pending Definition

`persist_pending(issue, iteration, phase)` writes to **feature.md Review Notes section**:

```markdown
## Review Notes
- [pending] {phase} iter{iteration}: {issue.description}
```

Do NOT create separate files like `pending_user.txt`. All pending issues are tracked in feature.md.

### resolve_pending Definition

`resolve_pending(issue, iteration, status)` updates existing `[pending]` tags in **feature.md Review Notes section**:

```markdown
## Review Notes
- [resolved-applied] {phase} iter{iteration}: {issue.description}
- [resolved-invalid] {phase} iter{iteration}: {issue.description}
- [resolved-skipped] {phase} iter{iteration}: {issue.description}
```

**Status values**:
- `"resolved-applied"`: Fix was successfully applied in Step C.1 ELSE branch
- `"resolved-invalid"`: Issue was invalidated by validation in Step C.1 validation.invalid branch
- `"resolved-skipped"`: User explicitly chose to skip this issue in POST-LOOP Step 2

This function finds the corresponding [pending] entry and updates the tag to [resolved-applied], [resolved-invalid], or [resolved-skipped].

**Usage examples**:
- `resolve_pending(issue, iteration, "resolved-applied")` - Mark issue as resolved with fix applied
- `resolve_pending(issue, iteration, "resolved-invalid")` - Mark issue as resolved but invalid
- `resolve_pending(issue, iteration, "resolved-skipped")` - Mark issue as skipped by user decision

**CRITICAL**: `resolved-skipped` is ONLY produced by POST-LOOP user decision (action == "skip"). Orchestrator MUST NOT use `resolved-skipped` autonomously. Loop-detected issues (`is_loop`) MUST go through `persist_pending` → `[pending]` → POST-LOOP user decision flow. Bypassing this flow by directly writing `[resolved-skipped]` is a protocol violation.

### persist_fix Definition

`persist_fix(issue, iteration, phase)` writes to **feature.md Review Notes section**:

```markdown
## Review Notes
- [fix] {phase} iter{iteration}: {issue.location} | {issue.issue}
```

Appends a new `[fix]` entry. This records that a fix was applied, enabling `is_loop()` to detect repeated fixes across sessions and after context compression.

`[fix]` entries are **immutable** — unlike `[pending]` which transitions to `[resolved-*]`, `[fix]` entries remain permanently as audit history.

Do NOT create separate files. All fix history is tracked in feature.md Review Notes.

### apply_fix Definition

`apply_fix(issue)` applies the fix described in `issue.fix` to the target file.

**Scope**: FL workflow apply_fix operates on feature-{ID}.md content (documentation fixes). It does NOT modify source code.

**Execution**:
```
apply_fix(issue):
    IF issue.fix is a direct text replacement (simple Edit):
        Edit(target_file, issue.location, issue.fix)  # Orchestrator applies directly
    ELSE:
        Task(subagent_type: "general-purpose", model: "sonnet",
             prompt: "Edit the file to apply this fix.
             File: {target_path}
             Location: {issue.location}
             Issue: {issue.issue}
             Fix: {issue.fix}
             Apply the fix exactly. Output JSON: {\"status\": \"OK\"} or {\"status\": \"ERROR\", \"reason\": \"...\"}
             No other output.")
```

**On failure**: If the edit fails or subagent returns ERROR:
```
persist_pending(issue, iteration, "{current_phase}-ApplyFailed")
```

**Note**: The caller is responsible for calling `persist_fix()` after successful `apply_fix()`.

### Orchestrator Decision Criteria (Zero Debt Upfront)

**CRITICAL**: When making ANY recommendation or judgment, apply this self-check:

```
BEFORE recommending:
  □ Am I recommending this because it's SIMPLER NOW? → WRONG
  □ Am I recommending this because it's CHEAPER NOW? → WRONG
  □ Am I recommending this because "existing code does X"? → WRONG
  □ Am I recommending DEFER/SKIP to keep scope small? → WRONG

If ANY checkbox is YES → Re-evaluate from FUTURE perspective
```

**FORBIDDEN words in recommendations**:
- "keep scope"
- "already covered"
- "cost"
- "keep simple"
- "not needed now"

**Default stance**: ADOPT (add to this feature). Only DEFER when **technically impossible** in this feature.

### Root Cause Resolution (Anti-Workaround)

**CRITICAL**: When fixing issues, always target the root cause, not symptoms.

```
BEFORE applying fix:
  □ Does this fix address WHY the problem occurred? → Root cause
  □ Does this fix only address WHAT went wrong? → Symptom (WRONG)
  □ Will this same problem recur in similar situations? → Incomplete fix (WRONG)
  □ Am I adding special-case handling instead of fixing the source? → Workaround (WRONG)

If fix is symptom/workaround → Find and fix root cause instead
```

**Workaround patterns to REJECT**:
- Adding exception handling for a case that shouldn't occur
- Documenting "known issue" instead of fixing
- Adding retry logic for flaky behavior
- Special-casing one scenario instead of generalizing

**Root cause patterns to ADOPT**:
- Fixing the source of incorrect data
- Updating the design that allows invalid states
- Removing the condition that causes the edge case
- Generalizing the solution to cover all similar cases

**Example**:
| Symptom Fix (WRONG) | Root Cause Fix (CORRECT) |
|---------------------|--------------------------|
| "Skip this test when X" | Fix why X causes test failure |
| "Add null check here" | Fix why null is passed |
| "Retry on timeout" | Fix why timeout occurs |
| "Document as limitation" | Remove the limitation |

### Early Termination Prohibition

FL loop MUST NOT terminate early except for these conditions:
1. Phase 2-7 ALL have zero issues (Review-Validate-Apply + Maintainability + AC Validation + Feasibility + Planning Validation + Final Reference Check)
2. MAX_ITERATIONS reached
3. User explicitly instructs "stop FL" or "cancel FL"

**Ambiguous user responses** (e.g., "DONE", "OK", "proceed"):
- Do NOT interpret as termination
- Ask clarification: "Do you want to stop FL and mark target as DONE?"
- Never terminate without explicit confirmation

## Start

**Now read Phase 1**:
```
Read(.claude/skills/fl-workflow/PHASE-1.md)
```
