# Phase 2: Investigation

## Goal

Investigate codebase to understand dependencies and constraints.

---

## Type Routing

| Type | Action |
|------|--------|
| kojo | **Skip Phase 2-3** → Read(.claude/skills/run-workflow/PHASE-4.md) |
| erb/engine/infra | Dispatch explorer |
| research | Related Feature Discovery + Artifact Confirmation |

### Step 2.0: Read Testing Skill (erb/engine/infra only)

**Before investigation, read test commands for your Type**:

```
Skill(testing)
```

Find your Type's section and note:
- Test command format
- Expected output format
- Required parameters

This prevents "I thought I knew the command" errors in Phase 3/6.

---

## For erb/engine/infra

### Step 2.1: Dispatch Explorer

```
Task(subagent_type: "Explore",
     prompt: "Investigate Feature {ID}. Find patterns, files, constraints.")
```

| Returns | Action |
|---------|--------|
| BLOCKED | **STOP** → Report to user |
| READY | Continue |

---

## For research

### Step 2.1: Related Feature Discovery

1. Read feature-{ID}.md Background/Problem
2. Grep for referenced Feature IDs
3. Build dependency graph

### Step 2.2: Artifact Current State

1. Identify artifacts in Problem
2. Read each artifact directly (NOT from docs)
3. Record current state

| Result | Action |
|--------|--------|
| All confirmed | Continue |
| Not found | **STOP** → Report to user |

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

**Before proceeding**: Did any of the following occur?

- Bash exit ≠ 0
- Agent returned BLOCKED
- Unexpected file not found

| Answer | Action |
|--------|--------|
| No | Proceed |
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

| Type | Next Phase |
|------|------------|
| erb/engine | Read(.claude/skills/run-workflow/PHASE-3.md) |
| infra | Read(.claude/skills/run-workflow/PHASE-4.md) |
| research | Read(.claude/skills/run-workflow/PHASE-4.md) |
