# Feature 138: Feature 129 COM_9 自慰 口上テスト失敗修正

## Status: [DONE]
## Type: erb
## Priority: MEDIUM

## Background

Feature 133 の AC verification 中に発見された問題。Feature 129 の COM_9 自慰口上で 16 テスト中 8 テストが失敗している。

### 発見経緯

Feature 133 (KojoBatch tests配列処理) の検証時に、`kojo-129-K1.json` を実行した結果：
- 16/16 テスト実行: OK (Feature 133 で修正)
- 8/16 テスト PASS
- 8/16 テスト FAIL

### 問題の詳細

失敗しているのは「思慕」「なし」関係状態でのテストケース：
- 思慕_pattern0-3: FAIL
- なし_pattern0-3: FAIL

### Root Cause (確定)

Test JSON ファイルの TALENT index が incorrect:
- 現在: TALENT index 18 を使用 (=== TALENT:18 == Identifiable but wrong)
- 修正: TALENT index 17 に変更 (=== TALENT:17 == 思慕)
- 追加: TALENT:3=0 を追加して恋慕状態をクリア

ERB コードは正しい - テストデータの修正のみ必要。

### 影響範囲

- K1 (美鈴) のみ確認済み
- K2-K10 でも同様の問題が発生している可能性あり

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 思慕状態で拒否台詞 | output | contains | "もっと仲良くなってから" | [x] |
| 2 | なし状態で拒否台詞 | output | contains | "お引き取りください" | [x] |
| 3 | kojo-129-K1.json 16/16 PASS | output | contains | "16/16" | [x] |
| 4 | Build成功 | build | succeeds | - | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Test JSON TALENT index 修正 (思慕テスト) | [x] |
| 2 | 2 | Test JSON TALENT index 修正 (なしテスト) | [x] |
| 3 | 3 | kojo-129-K1.json 全テスト PASS 確認 | [x] |
| 4 | 4 | ビルド実行と成功確認 | [x] |

## Technical Notes

### 関連ファイル

- `Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` - COM_9 自慰口上実装
- `Game/tests/kojo-129-K1.json` - テストケース定義

### テスト実行コマンド

```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --unit "tests/kojo-129-K1.json"
```

### 参照

- Feature 129: COM_9 自慰 口上 (完了済み、テスト定義含む)
- Feature 133: KojoBatch tests配列処理 (テスト基盤修正)

## Execution State

**Initialized**: 2025-12-19 10:15 JST
**Current Phase**: Test Verification
**Progress**: 50% - Tasks 1-2 complete (JSON fix), pending test verification
**Model**: Haiku 4.5
**Dispatcher**: Initializer Agent

### Work Distribution

| Task | Agent | Model | Status |
|------|-------|-------|--------|
| 1-2: JSON修正 | implementer | opus | done |
| 3: テスト確認 | regression-tester | haiku | pending |
| 4: ビルド検証 | regression-tester | haiku | pending |

### Key Context

- **Root Cause**: TALENT index错误 (18 → 17) in test JSON
- **Affected**: K1-K10 COM_9 自慰口上テスト
- **Scope**: Test data fix (ERB code is correct)
- **Dependency**: Feature 133 (KojoBatch engine fix) ✅ DONE
- **Blocker**: None

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-19 10:15 | START | initializer | Feature state extraction | - |
| 2025-12-19 10:16 | END | initializer | Status → [WIP], Execution State added | SUCCESS |
| 2025-12-19 18:51 | START | implementer | Task 1-2 (Test JSON TALENT index fix) | - |
| 2025-12-19 18:59 | END | implementer | Task 1-2 | SUCCESS (8min) |
| 2025-12-19 19:00 | START | unit-tester | Task 3 (kojo-129-K1.json test verification) | - |
| 2025-12-19 19:00 | END | unit-tester | Task 3 (16/16 PASS verified) | SUCCESS (22.45s) |
| 2025-12-19 19:15 | START | debugger | K5/K7 JSON format fix | - |
| 2025-12-19 19:16 | END | debugger | K5/K7 JSON fixed, build verified | SUCCESS (1min) |
| 2025-12-19 19:17 | START | finalizer | Feature 138 completion | - |
| 2025-12-19 19:17 | END | finalizer | Status → [DONE], all ACs verified | SUCCESS (1min) |

## Discovered Issues

### Issue 1: K5 JSON Format Error
**File**: `Game/tests/kojo-129-K5.json`
**Type**: JSON structure + function name
**Status**: FIXED

**Problems**:
1. Missing wrapper object - file was raw array `[...]` instead of `{"name": "...", "tests": [...]}`
2. Incorrect function call suffix - used `KOJO_MESSAGE_COM_K5_9_1` instead of `KOJO_MESSAGE_COM_K5_9`

**Fix Applied**:
- Added wrapper object with correct name field
- Changed all 16 test calls from `_1` suffix to correct function name
- Verified against reference format in `kojo-129-K6.json`

### Issue 2: K7 JSON Format Error
**File**: `Game/tests/kojo-129-K7.json`
**Type**: JSON structure + field naming
**Status**: FIXED

**Problems**:
1. Used `function` field instead of `call` field
2. Had unnecessary `defaults` block that served no purpose
3. Missing explicit `character` field in individual tests (relied on defaults)

**Fix Applied**:
- Removed `defaults` block
- Changed all 16 tests from `"function": "KOJO_K7_COM009"` to `"call": "KOJO_MESSAGE_COM_K7_9"`
- Added explicit `"character": "7"` to all tests
- Verified against reference format in `kojo-129-K6.json`

**Root Cause**: Inconsistent test file generation pattern in Feature 129 kojo-writer output

## References

- `Game/agents/feature-129.md` - 元の COM_9 口上 feature
- `Game/agents/feature-133.md` - 発見元 feature
