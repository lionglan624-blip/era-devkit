# Phase 4: Implementation

## Goal

Implement Feature Tasks.

---

## Pre-Check: Goal vs Tasks Consistency

**CRITICAL: Before executing any Task, verify Goal coverage.**

1. Read `### Goal (What to Achieve)` section — list each numbered item
2. Read `## Tasks` table — list each Task
3. For each Goal item, confirm at least one Task covers it
4. If any Goal item has NO covering Task → **STOP** → Report to user

**Special attention**: Goal items requiring user decisions/approvals (e.g., "reach consensus with user") are often missing from Tasks. These must be executed before implementation Tasks.

| Result | Action |
|--------|--------|
| All Goal items covered | Continue to Implementation Contract check |
| Uncovered Goal item found | **STOP** → Report gap to user → Add Task or get waiver |

## Pre-Check: Implementation Contract

1. Does feature spec have `## Implementation Contract`?
2. If yes → **Execute exactly as written** (no modifications)
3. If no → Follow Type routing below

**STOP if**: Contract is impossible or ambiguous → Ask user

---

## Type Routing

### Type: kojo

See `Skill(kojo-writing)` "/run Phase 4 Procedure" section.

Dispatch format: `"{ID} K{N}"` (minimal)

| Returns | Action |
|---------|--------|
| All status files | Continue to Phase 6 |
| Missing functions | Re-dispatch affected K{N} |
| Timeout (1h) | **STOP** → Ask user |

### Type: erb/engine

**Check Task Status and Tag before dispatch:**

```
FOR each Task in Tasks table:
    IF Task.Status == "[x]":
        Log: "Task#{N} already completed, skipping"
        CONTINUE
    IF Task.Status == "[B]":
        Log: "Task#{N} BLOCKED, skipping (Phase 9 handles)"
        CONTINUE

    # Process [ ] and [-] tasks
    IF Task has [I] tag:
        # Mini-TDD: Implement first, then write test
        GOTO "Mini-TDD for [I] Tasks" section below
    ELSE:
        # Normal flow: Test already created in Phase 3
        Task(subagent_type: "implementer",
             prompt: "Task {N}, Feature {ID}. Type: {erb|engine}.")
```

| Returns | Action |
|---------|--------|
| SUCCESS | Continue |
| BUILD_FAIL | Dispatch debugger |
| ERROR | → See ERROR Classification below |

### ERROR Classification

When implementer returns ERROR, classify and act:

| Error Pattern | Action |
|---------------|--------|
| `BLOCKED:CODE_CONFLICT` | **STOP** → Report conflict details to user |
| `BLOCKED:DOC_MISMATCH` | **STOP** → Report mismatch to user for SSOT resolution |
| `BLOCKED:COMPLEXITY_EXCEEDED` | Dispatch `smart-implementer` (opus) with same task |
| `BLOCKED:WRONG_AGENT` | **STOP** → Report correct command to user |
| Other `ERROR` | **STOP** → Report Error Type, Cause, Scope to user |

**Principle**: ERROR types indicate situations requiring human judgment or different agent capabilities. Do NOT retry with same agent.

---

### Mini-TDD for `[I]` Tasks

**For Tasks with `[I]` tag, execute implementation-first TDD within Phase 4:**

```
# Step 1: Implement first (discover actual output)
Task(subagent_type: "implementer",
     prompt: "Task {N}, Feature {ID}. Type: {erb|engine}.
     NOTE: This is an [I] (Investigation) task. Focus on implementation.
     After implementation, report the ACTUAL output/values produced.")

# Step 2: Extract actual value from implementer output
Parse implementer response for:
    ACTUAL_OUTPUT_TYPE: {type}
    ACTUAL_OUTPUT_VALUE: {value}

IF not found:
    Re-prompt implementer: "What is the concrete output value produced by this implementation?"

    IF still not found after re-prompt:
        # Attempt 3: Direct extraction from implementation artifacts
        modified_files = git diff --name-only
        FOR file in modified_files:
            content = Read(file)
            # Search for output patterns: PRINT/PRINTL (ERB), return values, assertions
            output_candidates = extract_output_patterns(content)
            IF output_candidates is not empty:
                ACTUAL_OUTPUT_VALUE = output_candidates[0]
                Log: "Task#{N} [I] extracted via artifact scan"
                → GOTO Step 3 (Update AC Expected)

        # All 3 extraction attempts failed
        # 1. Record in Execution Log (NOT as DEVIATION)
        Edit feature-{ID}.md Execution Log:
            | {timestamp} | MiniTDD | orchestrator | Task#{N} [I] extract | NO_OUTPUT |

        # 2. Mark Task and AC as BLOCKED
        Edit feature-{ID}.md Tasks table:
            Task#{N} Status: [ ] → [B]
        Edit feature-{ID}.md AC table:
            AC#{M} Status: [ ] → [B]

        # 3. Record in Mandatory Handoffs (format matches F750 pattern)
        Edit feature-{ID}.md Mandatory Handoffs:
            | Issue | Reason | Destination | Destination ID | Creation Task |
            |-------|--------|-------------|----------------|---------------|
            | Task#{N} AC#{M}: [I] NO_OUTPUT | BLOCKED | - | - | Phase 9 Completion Gate で決定 |

        # Note: [B] status triggers Phase 9 Completion Gate (Step 9.6)
        # Phase 9.7 will execute A/B/C decision logic and update Destination
        # Do NOT use "TBD" - use "-" for pending Phase 9 resolution

        # 4. Skip and continue
        SKIP Steps 3-5 for this Task
        CONTINUE to next [I] Task (if any)

# Note: [B] for NO_OUTPUT is appropriate after 3 failed extraction attempts because
# without a concrete Expected value, the AC cannot be verified by test.
# Phase 9.6 Completion Gate handles [B] ACs with user choices (wait/waive/fix).

# Step 3: Update AC Expected with concrete value
IF AC Expected was placeholder (TBD, ???, [PLACEHOLDER]):
    Edit feature-{ID}.md: Replace placeholder with {value} from Step 2
    Log: "AC#{M} Expected updated: {placeholder} → {value}"

# Step 4: Create test with concrete Expected
Task(subagent_type: "implementer",
     prompt: "Phase 3 TDD (deferred), Feature {ID}.
     Create test for AC#{M} with Expected: {value}")

# Step 5: Verify GREEN
Run test
IF PASS:
    Log: "Task#{N} [I] mini-TDD complete: GREEN"
ELSE:
    Dispatch debugger
```

**Key Principle**: Test ADDITION only. Existing tests from Phase 3 are never modified.

| Step | Action | Artifact |
|:----:|--------|----------|
| 1 | Implement | Code changes |
| 2 | Extract | Parse ACTUAL_OUTPUT_VALUE |
| 3 | Update AC | feature-{ID}.md (Expected field) |
| 4 | Create test | Test file |
| 5 | Verify | GREEN confirmation |

### Type: infra/research

```
Task(subagent_type: "implementer",
     prompt: "Task {N}, Feature {ID}. Type: {infra|research}.")
```

For research creating sub-features:

### ID Allocation Protocol (MANDATORY)

1. **Read**: `grep "Next Feature number" pm/index-features.md` → Get current N
2. **Reserve**: IDs for all DRAFTs to create (N, N+1, N+2, ...)
3. **Update First**: Edit index-features.md `Next Feature number: {N} → {N + count}`
4. **Then Create**: feature-{N}.md, feature-{N+1}.md, ... with Background only
5. **Register**: Add rows to index-features.md Active Features as [DRAFT]
6. **Record in Handoff**: "F{N} [DRAFT] - 別セッションで /fc {N} を実行"
7. Continue current feature (do not wait)

**CRITICAL**: Step 3 (Update) MUST complete before Step 4 (Create) to prevent ID collision.

---

## DEVIATION Check (CRITICAL)

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

**After EACH Task dispatch, check**:

| Event | Is DEVIATION? |
|-------|:-------------:|
| Bash exit ≠ 0 | Yes |
| Agent ERR:* | Yes |
| BUILD_FAIL | Yes |
| SUCCESS | No |

**If DEVIATION occurred**:
1. **Immediately** Edit feature-{ID}.md Execution Log
2. Record: `| {timestamp} | DEVIATION | {agent} | {action} | {detail} |`
3. Then continue with recovery (debugger, retry, etc.)

**Do NOT proceed to next Task without recording DEVIATION.**

---

## Phase終了チェック

```bash
# 1. deviation-log確認（hook記録）
cat _out/tmp/deviation-log.txt 2>/dev/null || echo "No deviation log"

# 2. feature-{ID}.mdのDEVIATION数確認
grep -c DEVIATION pm/features/feature-{ID}.md
```

→ ログにあるのにfeature-{ID}.mdにないなら**記録漏れ** → 追記してから次へ

---

## Next

| Type | Next Phase |
|------|------------|
| erb/engine | Read(.claude/skills/run-workflow/PHASE-7.md) (skip Phase 5-6) |
| kojo | Read(.claude/skills/run-workflow/PHASE-6.md) (skip Phase 5) |
| infra/research | Read(.claude/skills/run-workflow/PHASE-7.md) (skip Phase 5-6) |
