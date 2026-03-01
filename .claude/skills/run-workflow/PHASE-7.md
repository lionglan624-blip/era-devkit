# Phase 7: Verification

## Goal

Hooks + AC verification + Debug loop (max 3).

---

## Execution Order

1. **Hooks** (Smoke Test) - automatic via PostToolUse
2. **BOM Batch Verification** - manual
3. **AC Verification** - verify all ACs
4. **Debug Loop** (if needed) - max 3 iterations

---

## Step 7.1: Hooks (Automatic)

PostToolUse hooks run after Write/Edit:
- BOM check
- Build check
- Strict warnings
- Smoke test

| Hook Result | Action |
|-------------|--------|
| All OK | Continue |
| Any FAIL | **DEVIATION** → Record → Debug |

---

## Step 7.2: BOM Batch Verification

```bash
python src/tools/python/verify-bom.py --fix Game/ERB/口上
```

| Result | Action |
|--------|--------|
| All OK or Fixed | Continue |
| Fix failed | **DEVIATION** → Record → **STOP** |

---

## Step 7.2.5: AC Structural Lint Gate

**CRITICAL: Run before AC verification. Structural errors cause false FAIL in ac-static-verifier.**

```bash
python src/tools/python/ac_ops.py ac-check {ID}
```

| Result | Action |
|--------|--------|
| Exit 0 | Proceed to Step 7.3 |
| Exit 1 | **STOP** → Report raw output to user verbatim. Do NOT interpret, dismiss, or proceed. Fix AC definition structure first. |

> **Fix tools**: `ac-fix` (update columns), `ac-renumber` (close gaps), `ac-delete` (remove AC). Run `python src/tools/python/ac_ops.py --help` for details.

---

## Step 7.3: AC Verification

**CRITICAL: Dispatch ac-tester subagent. Opus MUST NOT verify ACs directly.**

The ac-tester reads feature-{ID}.md, executes each AC's verification method, and reports results as text. **Orchestrator updates AC/Task statuses.**

```
Task(subagent_type: "ac-tester",
     prompt: "Verify ALL ACs for Feature {ID}.
              Read feature-{ID}.md AC table. For each AC: execute verification.
              Report PASS/FAIL/BLOCKED per AC with evidence. Do NOT edit any files.")
```

**For file/code/build type ACs** (especially in infra type features):

Use `ac-static-verifier.py` for automated verification:

```bash
python src/tools/python/ac-static-verifier.py --feature {ID} --ac-type {code|build|file}
```

This tool generates JSON logs and provides structured verification for static ACs.

### AC Verification Flow (per AC, executed by ac-tester)

```
1. Run verification
2. IF PASS → Report OK:AC{N}
3. IF FAIL → Report ERR with matcher evidence
4. IF BLOCKED → Report BLOCKED with reason
```

**CRITICAL**: ac-tester reports results only (read-only agent). Opus updates AC/Task statuses in feature-{ID}.md and records DEVIATION for any FAIL or BLOCKED before proceeding to Step 7.4.

---

## Step 7.4: Debug Loop (if failures)

Max 3 iterations:

1. Record DEVIATION in Execution Log
2. Dispatch debugger
3. Re-run ALL AC verification (dispatch ac-tester for complete feature) to catch regressions from debugger fix
4. If still failing after 3 attempts → **STOP** → Ask user

---

## DEVIATION Check (CRITICAL)

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

**This phase has highest DEVIATION probability.**

| Event | Is DEVIATION? | Action |
|-------|:-------------:|--------|
| Hook FAIL | Yes | Record immediately |
| AC test FAIL | Yes | Record, then debug |
| Grep count mismatch | Yes | Record, then investigate |
| dotnet test FAIL | Yes | Record, then debug |
| timeout (exit 124) | Yes | Record immediately |
| Manual intervention (taskkill等) | Yes | Record immediately |
| All PASS | No | Continue |

**DEVIATION Recording Format**:
```markdown
| {timestamp} | DEVIATION | {source} | {action} | {detail} |
```

Example:
```markdown
| 2026-01-16 | DEVIATION | Bash | dotnet test | exit code 1 |
```

### Immediate Recording Requirement (F575 Lesson)

**CRITICAL: Next action after DEVIATION must be Edit**

```
Bash exit ≠ 0 occurs
    ↓
[Next tool call] Edit(feature-{ID}.md, add DEVIATION)
    ↓
Then proceed with response (retry, debug, etc.)
```

**Prohibited**: Calling any tool other than Edit after DEVIATION occurs
**Reason**: Attempting to record later leads to forgetting or rationalization

---

## Exit Criteria

- All Hooks PASS
- All ACs marked `[x]`
- Debug loop completed (or not needed)

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
Read(.claude/skills/run-workflow/PHASE-8.md)
```
