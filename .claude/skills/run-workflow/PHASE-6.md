# Phase 6: Test Generation (kojo only)

## Goal

Generate tests with kojo_test_gen.py.

---

## Applicability

| Type | Action |
|------|--------|
| kojo | Execute this phase |
| Other | **Skip** → Read(.claude/skills/run-workflow/PHASE-7.md) |

---

## Step 6.1: Run kojo_test_gen.py

```bash
python src/tools/kojo-mapper/kojo_test_gen.py --feature {ID} --com {COM_NUMBER} --output-dir test/ac/kojo/feature-{ID}/
```

**IMPORTANT**: `--output-dir` parameter is required.

**COM→File mapping**: `src/tools/kojo-mapper/com_file_map.json` is SSOT.

---

## Step 6.2: Verify Output

| Returns | Action |
|---------|--------|
| {N} files generated | Continue |
| Missing functions | **STOP** → Report to user |
| Script error | **STOP** → Report to user |

**Expected output**: {N} test JSON files (one per AC character in feature) in `test/ac/kojo/feature-{ID}/`

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

| Event | Is DEVIATION? |
|-------|:-------------:|
| Script error (exit ≠ 0) | Yes |
| Missing functions | Yes (STOP condition) |
| 10 files generated | No |

**If DEVIATION**: Record in Execution Log before STOP.

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
Read(.claude/skills/run-workflow/PHASE-7.md)
```
