# Feature 219: Kojo Test Infrastructure Hardening

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
TDD integrity must be protected by automation. Test files should never be manually modified to match implementation - implementation must match tests.

### Problem (Current Issue)
1. **AC test edit unprotected**: `tests/ac/` can be edited by debugger, violating TDD principle (test should be immutable after RED phase)
2. **kojo-writer Expected mismatch**: Status file reports arbitrary DATALIST phrase instead of DATALIST[0], causing test/implementation mismatch

### Goal (What to Achieve)
1. Add TDD protection to post-code-write.ps1 (block edits to `tests/ac/` and `tests/regression/`)
2. Remove Expected reporting from kojo-writer.md (kojo_test_gen.py handles this)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TDD protection blocks tests/(ac\|regression)/, allows others | exit_code | script | succeeds | - | [x] |
| 2 | kojo-writer.md has no Expected reporting in status template | file | grep | not_contains | "Expected:" | [x] |

### AC Details

**AC1 Test**: TDD protection unit test (verifies hook path-matching logic in isolation)
```powershell
# Run from project root
# Negative: tests/ac/ should be blocked (exit 1)
$json1 = '{"tool_input":{"file_path":"tests/ac/test.json"}}'
echo $json1 | pwsh -File .claude/hooks/post-code-write.ps1
if ($LASTEXITCODE -ne 1) { throw "FAIL: tests/ac/ not blocked" }

# Negative: tests/regression/ should be blocked (exit 1)
$json2 = '{"tool_input":{"file_path":"tests/regression/scenario.json"}}'
echo $json2 | pwsh -File .claude/hooks/post-code-write.ps1
if ($LASTEXITCODE -ne 1) { throw "FAIL: tests/regression/ not blocked" }

# Positive: Non-test file should pass (exit 0)
$json3 = '{"tool_input":{"file_path":".tmp/test.txt"}}'
echo $json3 | pwsh -File .claude/hooks/post-code-write.ps1
if ($LASTEXITCODE -ne 0) { throw "FAIL: non-test file blocked" }

Write-Host "PASS: TDD protection working correctly"
```

**AC2 Test**: `grep -c "Expected:" .claude/agents/kojo-writer.md` returns 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add TDD protection to post-code-write.ps1 (block tests/ac and tests/regression, exit 1) | [x] |
| 2 | 2 | Remove Expected/key_phrase from kojo-writer.md Status File section | [x] |

---

## Technical Notes

### Task 1: TDD Protection Hook
**Option A (Recommended)**: Integrate into existing `post-code-write.ps1` by adding protection check before other processing.

**Option B**: Create standalone `.claude/hooks/protect-tests.ps1` and register in `.claude/settings.json` as PostToolUse hook.

Implementation (add to post-code-write.ps1 after path validation block `if (-not $path) {...}`):
```powershell
# TDD Protection: Block Edit to tests/ac/ and tests/regression/
if ($path -match "tests[/\\](ac|regression)") {
    Write-Error "BLOCKED: Test file edit forbidden (TDD protection)"
    exit 1
}
```

Note: Insert after the `if (-not $path) { ... exit 0 }` block. Uses $path variable already extracted. Exit code 1 for TDD block (distinct from 2 for build failure).

### Task 2: kojo-writer.md update
Remove Expected and key_phrase from Status File section (lines 81-86). The kojo_test_gen.py script handles DATALIST[0] extraction automatically.

Remove from kojo-writer.md Status File section:
```
Expected: {key_phrase_from_DATALIST}
```

No replacement needed - Expected field should not exist in status output.

<!-- Obsolete: Tasks 5-7 removed. See Discussion Notes section 8 for OOTT decisions. -->

---

## DEVIATION Report (from Feature 187)

| Issue | Root Cause | Impact | Fix |
|-------|-----------|--------|-----|
| K6 Expected mismatch | kojo-writer reported DATALIST[1] | Test had wrong expected value | Task 2 (kojo-writer.md) |
| Duplicate functions K2/K4/K9 | No existing stub check | debugger fix required | Out of scope (F187 issue) |
| Test edited by debugger | No hook protection | TDD violation | Task 1 (hook protection) |
| kojo_test_gen.py failed | Encoding issue | Manual test creation | **RESOLVED** (utf-8-sig already implemented) |

---

## Discussion Notes (Feature 187 Post-Review)

### 1. TDD違反: テスト期待値修正

**状況**: K6フランのテストで `mock_rand: [0]` を指定しているが、kojo-writer が報告した Expected 値と DATALIST[0] が不一致。debugger がテストを修正した。

**問題**: テスト期待値を実装に合わせて変更するのはTDD違反。Hook で保護すべき。

### 2. 重複関数発生の原因

**発生箇所**: K2, K4, K9 の `口挿入.ERB` にスタブ関数が残存。kojo-writer が `挿入.ERB` に新規追加 → 重複。

**原因**:
- Investigation フェーズで既存スタブ検出したが kojo-writer に伝達されず
- kojo-writer.md の ERB Placement が「挿入.ERB」を指定 → 口挿入.ERB を無視

### 3. COM Map 参照の欠如

**com-map.md**: COM 60-72 = Insertion category として定義あり

**kojo-writer.md**: com-map.md への参照なし、配置先を独自に決定

### 4. mock_rand が外れた原因 (K6)

**kojo-writer 報告**: `Expected: 獣みたい……って思ったけど……` (DATALIST[1])

**実際のDATALIST[0]**: `んっ……あなた、後ろから……？`

**根本原因**: kojo-writer.md の Status File 仕様に「DATALIST のどれを報告するか」の指定がない

### 5. kojo-writer の Expected 報告は必要か？

**do.md Phase 5**: `kojo_test_gen.py` が DATALIST を解析してテスト作成

**結論**: Expected 報告は本来不要（kojo_test_gen.py が自動抽出すべき）。ただし kojo_test_gen.py 失敗時のフォールバックとして現在使用中。

**選択肢**:
- A) kojo_test_gen.py を修正 → Expected 報告を廃止 ← **採用 (2025-12-26)**
- B) Expected 報告を DATALIST[0] に厳密化 → フォールバックとして維持

### 6. feature-057 vs kojo-writer.md の不整合

| Source | COM 60-72 配置先 |
|--------|-----------------|
| feature-057.md (2024-12 リファクタリング) | `_口挿入.ERB` (COM 60-148 全て) |
| kojo-writer.md (現行) | `_挿入.ERB` |
| 実ファイル | 両方存在 (`挿入.ERB` が後から追加された) |

**要決定**: どちらを正とするか。現在は `挿入.ERB` で運用中。

### 7. 未解決の質問

- [x] `挿入.ERB` と `口挿入.ERB` の使い分けルール → **解決**: 現行は `_挿入.ERB` を使用 (kojo_test_gen.py COM_FILE_MAP line 57)。feature-057 の `口挿入.ERB` 記述は歴史的経緯。
- [x] kojo-writer が既存ファイルをチェックする仕組みの追加 → **不要**: SSOT で配置先が一意になったため
- [x] kojo_test_gen.py 修正後、Expected 報告を廃止するか維持するか → **廃止** (選択肢A採用)

### 8. OOTT整理 (2025-12-26)

**決定事項**:
1. `com-map.md` 削除 (OOTT違反 - COMF*.ERB が真の情報源)
2. COM → File 配置ルールを `kojo-writing SKILL.md` に移動 (SSOT)
3. `kojo-writer.md` から配置テーブル削除、Skill 参照に変更
4. 自動生成は Feature 219 (Low Priority) として分離
5. Expected 報告を廃止 (kojo_test_gen.py が DATALIST[0] を自動抽出)
6. 既存ファイルチェック不要 (SSOT で配置先一意)

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-26 | initializer | Feature state initialized (PROPOSED→WIP) | READY |
| 2025-12-26 | implementer | Task 1: TDD protection added to post-code-write.ps1 | SUCCESS |
| 2025-12-26 | implementer | Task 2: Expected removed from kojo-writer.md | SUCCESS |
| 2025-12-26 | - | AC1: TDD protection test (3 cases) | PASS |
| 2025-12-26 | - | AC2: grep Expected: count=0 | PASS |
| 2025-12-26 | - | Build verification | PASS |
| 2025-12-26 | finalizer | Feature 219 finalized (WIP→DONE) | COMPLETE |

---

## Links

- [feature-187.md](feature-187.md) - Triggering issue
- [feature-057.md](feature-057.md) - Original kojo refactoring (COM placement rules)
- [kojo-writer.md](../../.claude/agents/kojo-writer.md)
- [do.md](../../.claude/commands/do.md)
