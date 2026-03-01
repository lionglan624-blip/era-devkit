# Phase 5: Refactoring (TDD)

## Goal

Refactor implementation while maintaining GREEN. Improve code quality without changing behavior.

---

## Type Routing

| Type | Action |
|------|--------|
| infra | **Skip** → Read(.claude/skills/run-workflow/PHASE-7.md) |
| research | **Skip** → Read(.claude/skills/run-workflow/PHASE-7.md) |
| erb | Refactor ERB code → GREEN confirmation |
| engine | Refactor C# code → GREEN confirmation |

> **Note**: kojo type does not reach Phase 5 (skips from Phase 4 → Phase 6 directly)

---

## Step 5.0: GREEN Confirmation (Pre-Refactor)

**Before refactoring, confirm tests pass (GREEN from Phase 4).**

### For erb

```bash
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit {test-file}
```

### For engine

```bash
dotnet test --blame-hang-timeout 10s --filter "FullyQualifiedName~{TestClass}"
```

| Result | Action |
|--------|--------|
| PASS (exit 0) | Proceed to Step 5.1 |
| FAIL (exit ≠ 0) | **STOP** → Return to Phase 4 (implementation incomplete) |

---

## Step 5.1: Dispatch Implementer (Refactor Mode)

```
Task(subagent_type: "implementer",
     prompt: "Phase 5 Refactoring, Feature {ID}. Type: {erb|engine}.
              Refactor the implementation from Phase 4. Rules:
              - Do NOT change behavior (tests must still pass)
              - Eliminate duplication
              - Improve naming and readability
              - Simplify complex logic
              - If no refactoring needed, report SKIP.")
```

| Returns | Action |
|---------|--------|
| SKIP | Proceed to Phase 7 (no changes needed) |
| SUCCESS | Proceed to Step 5.2 |
| ERROR | Evaluate → Dispatch debugger if needed |

---

## Step 5.2: GREEN Re-confirmation (Post-Refactor)

**CRITICAL: Refactoring must not break tests.**

### For erb

```bash
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit {test-file}
```

### For engine

```bash
dotnet test --blame-hang-timeout 10s --filter "FullyQualifiedName~{TestClass}"
```

| Result | Action |
|--------|--------|
| PASS (exit 0) | Refactoring successful → Proceed |
| FAIL (exit ≠ 0) | **DEVIATION** → Revert refactoring → Record → Re-run Step 5.1 |

**Revert procedure** (if GREEN fails):
1. Show impact: `git diff -- {modified files}`
2. Ask user for confirmation: "Revert refactoring changes in {files}? (y/n)"
3. Only execute `git checkout -- {files}` after user confirms
4. Re-run GREEN confirmation to ensure clean state
5. Record DEVIATION
6. Re-dispatch implementer with narrower refactoring scope, or SKIP

**Note**: Per CLAUDE.md Git Safety: per-file git checkout requires user confirmation.

**Max retry**: 2 attempts. On 3rd failure → Skip refactoring, proceed to Phase 7.

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

| Event | Is DEVIATION? |
|-------|:-------------:|
| Step 5.0 FAIL | Yes (Phase 4 incomplete) |
| Step 5.2 FAIL | Yes (refactoring broke tests) |
| Agent ERR | Yes |
| SKIP (no refactoring needed) | No |
| All PASS | No |

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
| erb/engine | Read(.claude/skills/run-workflow/PHASE-7.md) (skip Phase 6) |

> **Note**: Only erb/engine types reach Phase 5. kojo skips from Phase 4 → Phase 6. infra/research skip from Phase 4 → Phase 7.
