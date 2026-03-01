# Phase 8: Post-Review

## Goal

Quality review by feature-reviewer + Doc consistency + SSOT update check.

---

## Three-Step Review (All READY required)

### Step 8.1: Quality Review (mode: post)

```
# feature-reviewer auto-determines mode from Feature Type and Status:
# - research + [DONE] → post  - research + other → pre  - non-research → spec
# Do NOT override mode — let auto-determination handle it.
Task(subagent_type: "feature-reviewer",
     prompt: "Feature {ID}.
              Review implementation quality (post-implementation context).")
```

| Result | Action |
|--------|--------|
| READY | Proceed to Step 8.2 |
| NEEDS_REVISION | **DEVIATION** → Record → Fix → Re-run (AC re-verification deferred to Phase 9 Step 9.2.1) |
| UNREACHABLE | **STOP** → Report to user |
| IRRELEVANT | **STOP** → Report to user |

### Step 8.2: Doc Consistency Check (mode: doc-check)

**Pre-check: Auto-skip for features without new extensibility points.**

**Evidence**: F778 /run session — 8.2 used 38K opus tokens to conclude "no SSOT updates needed" for a feature that only modified existing files. For engine/erb features without new public APIs, the result is trivially predictable.

```
# Auto-skip check: Does this feature add new extensibility points?
new_extensibility = FALSE

# Check Tasks table for new-API keywords
tasks_text = Read feature-{ID}.md Tasks table
IF tasks_text contains any of:
    "Create interface", "New interface", "Add interface",
    "Create class.*public", "New command", "Add command",
    "New agent", "Create agent", "New skill", "Create skill",
    "New src/Era.Core/Types"
THEN new_extensibility = TRUE

# Also check git diff for new files in extensibility paths
new_files = Bash("git diff --name-status HEAD | grep '^A'")
IF new_files contains any of:
    "src/Era.Core/Types/", ".claude/agents/", ".claude/commands/", ".claude/skills/"
THEN new_extensibility = TRUE

IF NOT new_extensibility:
    # Skip 8.2 — doc-check will trivially return OK
    Log: "Step 8.2 skipped — no new extensibility points detected"
    → Proceed to Step 8.3
```

**Full dispatch (when new extensibility points detected)**:

```
Task(subagent_type: "feature-reviewer",
     prompt: "Mode: doc-check. Feature {ID}.")
```

| Result | Action |
|--------|--------|
| READY | Proceed to Step 8.3 |
| NEEDS_REVISION | **DEVIATION** → Record → Fix → Re-run (AC re-verification deferred to Phase 9 Step 9.2.1) |

### Step 8.3: SSOT Update Check

Check `.claude/reference/ssot-update-rules.md`:

| Change Type | Check |
|-------------|-------|
| New `src/Era.Core/Types/*.cs` | engine-dev SKILL updated? |
| New `IVariableStore` methods | engine-dev SKILL updated? |
| New interface | engine-dev SKILL updated? |
| New slash command | CLAUDE.md updated? |
| New agent | CLAUDE.md updated? |

| Result | Action |
|--------|--------|
| All updated (or N/A) | Proceed |
| Missing updates | Update SSOT → Re-verify |

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

| Event | Is DEVIATION? |
|-------|:-------------:|
| NEEDS_REVISION | Yes |
| UNREACHABLE | Yes (STOP) |
| IRRELEVANT | Yes (STOP) |
| All READY | No |

**Loop Limit**: Max 3 NEEDS_REVISION cycles per step. On 4th → **STOP**.

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

```
Read(.claude/skills/run-workflow/PHASE-9.md)
```
