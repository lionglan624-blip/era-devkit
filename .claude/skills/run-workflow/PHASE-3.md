# Phase 3: Test Creation (TDD)

## Goal

Create executable tests from ACs and confirm RED (failure).

---

## Investigation Tag `[I]` Pre-Check

**Before creating tests, identify `[I]` tagged Tasks:**

```
FOR each Task in Tasks table:
    IF Task has [I] tag:
        Log: "Task#{N} [I]: Deferred to Phase 4 mini-TDD"
        deferred_tasks.append(Task#)
    ELSE:
        normal_tasks.append(Task#)
```

**`[I]` Tasks are SKIPPED in Phase 3** - their tests are created in Phase 4 after implementation.

---

## Type Routing

| Type | Action |
|------|--------|
| kojo | **Skip** → Read(.claude/skills/run-workflow/PHASE-4.md) |
| infra | **Skip** → Read(.claude/skills/run-workflow/PHASE-4.md) |
| erb | Generate test scenario JSON (for non-`[I]` Tasks only) |
| engine | Create C# unit test + AC test (for non-`[I]` Tasks only) |

---

## For erb

### Step 3.1: Dispatch Implementer

```
Task(subagent_type: "implementer",
     prompt: "Phase 3 TDD, Feature {ID}. Type: erb.")
```

### Step 3.2: Confirm RED

```bash
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit {test-file}
```

**Expected**: FAIL (exit ≠ 0)

**IMPORTANT**: Phase 3 exit ≠ 0 is NOT a DEVIATION (RED confirmation is the goal)

---

## For engine

### Step 3.1: Dispatch Implementer

```
Task(subagent_type: "implementer",
     prompt: "Phase 3 TDD, Feature {ID}. Type: engine.")
```

### Step 3.2: Confirm RED

```bash
dotnet test --blame-hang-timeout 10s --filter "FullyQualifiedName~{TestClass}"
```

**Expected**: FAIL (exit ≠ 0) - this is normal

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

**Phase 3 分類ルール**: Phase 3ではexit ≠ 0の原因を分類する:

| Exit ≠ 0 の原因 | 分類 | 記録 |
|-----------------|------|------|
| TDD RED（テスト実行失敗 = Step 3.2 期待結果） | **正常** | 記録不要 |
| テスト作成自体の失敗 | DEVIATION | 記録必須 |
| Build error | DEVIATION | 記録必須 |
| Agent ERR | DEVIATION | 記録必須 |

**判断基準**: Step 3.2 の "Expected: FAIL (exit ≠ 0)" で実行したテストの失敗のみ正常。それ以外のexit ≠ 0はDEVIATION。

| Answer | Action |
|--------|--------|
| Only TDD RED failures (Step 3.2) | Proceed (no DEVIATION) |
| Any other failure | Edit feature-{ID}.md Execution Log with DEVIATION |

---

## Phase終了チェック

```bash
# 1. deviation-log確認（hook記録）
cat _out/tmp/deviation-log.txt 2>/dev/null || echo "No deviation log"

# 2. feature-{ID}.mdのDEVIATION数確認
grep -c DEVIATION pm/features/feature-{ID}.md
```

→ ログにあるのにfeature-{ID}.mdにないなら**記録漏れ** → 追記してから次へ
→ **TDD RED例外**: Phase 3のテストコマンド実行（`--unit`, `dotnet test --filter`）による exit != 0 は
  期待通りの結果なのでDEVIATIONとして記録**不要**。deviation-log.txtに残るが無視してよい。
→ ただし**ビルドエラー**やAgent ERRは例外に該当**しない**（DEVIATIONとして記録）
→ Hook log > Feature MD 差分に TDD RED 分を含む場合、差分は正常

---

## Next

```
Read(.claude/skills/run-workflow/PHASE-4.md)
```
