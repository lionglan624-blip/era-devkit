# Phase 1: Initialize

## Goal

Verify Feature state and change to [WIP] to prepare for implementation.

---

## Step 1.0: Target Resolution

| Argument | Selection |
|----------|-----------|
| `{ID}` | Feature {ID} directly |
| `dev` | Lowest pending erb/engine/infra feature |
| `kojo` | Lowest pending kojo feature |
| (type) | Lowest pending feature of that type |
| (none) | Lowest pending feature (any type) |

**Procedure**:
1. Read `pm/index-features.md`
2. Find matching features
3. Select lowest ID

---

## Step 1.0.5: Status Gate Check

Read `feature-{ID}.md` and check Status:

| Status | Action |
|--------|--------|
| `[REVIEWED]` | Check fl-reviewed marker → If present: Proceed. If absent: **STOP**: "Manual [REVIEWED] detected. Please run `/fl {ID}` first" |
| `[WIP]` | Proceed (resume) |
| `[BLOCKED]` | Check blocker → If resolved AND fl-reviewed marker present: Proceed. If resolved but no marker: **STOP**: "Run `/fl {ID}` first". If unresolved: **STOP**: "Blocker {reason} not resolved" |
| `[PROPOSED]` | **STOP**: "Please run `/fl {ID}` first" |
| `[DONE]` | **STOP**: "Already completed" |

**FL Marker Check**:
```
fl_marker = Grep("<!-- fl-reviewed:", "pm/features/feature-{ID}.md")
IF fl_marker is empty:
    # Manual status change detected - FL not actually run
    STOP: "Manual [REVIEWED] detected. Please run `/fl {ID}` first"
```

---

## Step 1.0.6: Dependency Gate

Read `feature-{ID}.md` Dependencies table.

For each row where Type = "Predecessor":
1. Read `feature-{predecessor_id}.md`
2. Check Status field
3. If Status is not `[DONE]`:
   - **STOP**: "Predecessor {predecessor_id} is not [DONE] (current: {status})"

If all Predecessors are `[DONE]`: Proceed to Step 1.0.7.

---

## Step 1.0.7: Pre-Flight Build Check

```pseudocode
IF feature.type IN ["erb", "engine"]:
    build_result = Bash("dotnet build")
    IF build_result.exit_code != 0:
        Record as PRE-EXISTING DEVIATION
        STOP → Report: "Build is broken before implementation. Fix build first."
# Skip for kojo, infra, research types
```

---

## Step 1.1: Dispatch Initializer

**CRITICAL**: Use Task() with explicit `model: "sonnet"`, NOT Skill(). Skill frontmatter cannot enforce model (Claude Code Issue #14882/#17283). Without explicit model override, initializer inherits session model (opus), wasting ~50K opus tokens on mechanical status updates.

```
Task(subagent_type: "general-purpose",
     model: "sonnet",
     prompt: "Read .claude/skills/initializer/SKILL.md and execute for Feature {ID}.
              OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON is a protocol violation.")
```

| Returns | Action |
|---------|--------|
| NO_FEATURE | Tell user: feature-{ID}.md not found |
| Background empty | Ask user |
| READY | Continue |

---

## Step 1.2: Create Tasks

```
TaskCreate(subject: "Phase 1: Initialize", status: "in_progress")
TaskCreate(subject: "Phase 2-10...", status: "pending")
```

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

**Before proceeding**: Did any Bash command fail (exit ≠ 0)?

| Answer | Action |
|--------|--------|
| No | Proceed to Phase 2 |
| Yes | Edit feature-{ID}.md Execution Log with DEVIATION, then proceed |

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

Mark Phase 1 completed via TaskUpdate, then:

```
Read(.claude/skills/run-workflow/PHASE-2.md)
```
