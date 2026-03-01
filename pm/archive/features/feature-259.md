# Feature 259: ERB関数重複検知の/doワークフロー統合

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
1. **実装前に問題を検知し、デバッグコストを削減する**
2. **F190 リファクタリング基準に基づき、誤分割されたファイルを集約する**
   - F190 で COM→ファイルマッピングが確定 (COM 60-72 → `_挿入.ERB`, COM 80-85 → `_口挿入.ERB`)
   - このルールに反するスタブが残存していると重複エラーの原因となる
   - 重複検知ツールは、F190 ルール違反の発見にも活用可能

### Problem
F241実装時、K2/K4/K9の`口挿入.ERB`に既存スタブが残っており、新実装の`挿入.ERB`の関数がロード順で上書きされた。結果、AC検証で48/160テストが失敗し、デバッグが必要になった。

**根本原因**: Emueraはファイルをアルファベット順にロードするため、同名関数が複数ファイルに存在すると後からロードされた方が有効になる。

### Goal
1. /do Phase 4（実装前）で重複関数を検知し、kojo-writer dispatch前に警告・修正を行う
2. **F241 で発生したスタブ重複問題の再発防止**
   - K2/K4/K9 の `口挿入.ERB` に残存した COM_64 スタブ → 48/160 テスト失敗
   - 同様の問題を COM_65-68 実装前に検知・解決

### Context
- 発生箇所: F241 Phase 6 (デバッグで2回修正)
- 影響: 開発時間ロス + context消費
- 対象: kojo Feature (COM実装時)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md Phase 4.0に重複チェック手順追加 | file | Grep | contains | "Duplicate Check" | [x] |
| 2 | 重複検知スクリプト作成 | file | Glob | exists | tools/erb-duplicate-check.py | [x] |
| 3 | 検知テスト: 重複あり | output | python | contains | "DUPLICATE" | [x] |
| 4 | 検知テスト: 重複なし | output | python | contains | "OK" | [x] |
| 5 | スキャンモードテスト | exit_code | python | succeeds | - | [x] |
| 6 | Build | build | dotnet build | succeeds | - | [x] |

### AC Details

**AC1**: `Grep("Duplicate Check", ".claude/commands/do.md")`

**AC2**: `Glob("tools/erb-duplicate-check.py")`

**AC3**: 重複が存在するケースでスクリプトを実行し、出力に "DUPLICATE" が含まれることを確認
```bash
# テストデータ: テスト実行時に一時的なスタブファイルを作成
# 1. テスト用ERBファイルを作成 (同名関数を2ファイルに定義)
#    場所: .tmp/f259/ (CLAUDE.md temporary file guidelines)
# 2. スクリプト実行
# 3. テスト用ファイルを削除
python tools/erb-duplicate-check.py --function "KOJO_TEST_DUPLICATE" --path ".tmp/f259"
```
**Setup**: Task 2 実行時に `.tmp/f259/` 配下に一時的なテスト用スタブを作成・削除

**AC4**: 重複がないケースでスクリプトを実行し、出力に "OK" が含まれることを確認
```bash
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K1_64" --path "Game/ERB/口上/1_美鈴"
```
**Precondition**: `KOJO_MESSAGE_COM_K1_64` は `Game/ERB/口上/1_美鈴/KOJO_K1_挿入.ERB` に1つだけ存在（F241完了により確認済み）

**AC5**: スキャンモード (--function なし) でディレクトリ内の全@関数をスキャン。正常終了 (exit code 0) を確認
```bash
python tools/erb-duplicate-check.py --path "Game/ERB/口上/1_美鈴"
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2 | erb-duplicate-check.py スクリプト作成 (スキャンモード含む) | [x] |
| 2 | 3 | 検知テスト: 重複あり (テストデータ作成・削除含む) | [x] |
| 3 | 4 | 検知テスト: 重複なし | [x] |
| 4 | 5 | スキャンモードテスト | [x] |
| 5 | 1 | do.md Phase 4.0 更新 | [x] |
| 6 | 6 | ビルド確認 | [x] |

---

## Design

### 依存関係

- **Python**: 3.8+ (既存ツール kojo_mapper.py と同様)

### スクリプト仕様

#### 単一関数チェック (--function 指定)
```bash
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K2_64" --path "Game/ERB/口上"
```

**出力**:
```
# 重複あり
DUPLICATE: KOJO_MESSAGE_COM_K2_64
  - Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB:751
  - Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB:27

# 重複なし
OK: KOJO_MESSAGE_COM_K2_64 (1 definition)
  - Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB:751
```

#### スキャンモード (--function なし)
```bash
python tools/erb-duplicate-check.py --path "Game/ERB/口上/2_小悪魔"
```

**動作**: 指定パス内の全ERBファイルから `@` で始まる関数定義を抽出し、重複をチェック

**出力**:
```
# 重複あり
DUPLICATE: KOJO_MESSAGE_COM_K2_64
  - Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB:751
  - Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB:27

# 重複なし (すべてユニーク)
OK: All 42 functions are unique
```

### do.md Phase 4.0 変更

既存の `4.0 eraTW Cache` を拡張し、Duplicate Check を追加 (kojo type のみ):

```markdown
**4.0 Pre-Implementation Check** (kojo only):

**Duplicate Check**: For each target character K{N}:
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K{N}_{COM}" --path "Game/ERB/口上"

| Result | Action |
|--------|--------|
| OK | Continue to eraTW Cache |
| DUPLICATE | Remove stub first, then continue |

**eraTW Cache**: Dispatch eratw-reader.
```

**Note**: Fail-fast principle により Duplicate Check を eraTW Cache の前に配置。重複があれば早期に発見・解決。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Phase 1 | initializer | Feature init | READY |
| 2025-12-28 | Phase 2 | explorer | Investigate context | Complete |
| 2025-12-28 | Phase 4 | implementer | Tasks 1-5 | SUCCESS |
| 2025-12-28 | Phase 6 | - | AC verification | 6/6 PASS |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review | NEEDS_REVISION |
| 2025-12-28 | Phase 7 | - | Fix feature doc | Complete |
| 2025-12-28 | Phase 7 | feature-reviewer | Re-verify | READY |
| 2025-12-28 | Phase 8 | finalizer | Status update to [DONE] | Complete |

---

## Links

- [index-features.md](index-features.md)
- [do.md](../../.claude/commands/do.md)
- Related:
  - F241 (発見契機)
  - [F190](feature-190.md) (COM→ファイルマッピング確定)
  - [F221](feature-221.md) (_挿入.ERB vs _口挿入.ERB 整理)
  - [F260](feature-260.md) (SSOT準拠提案 - 本Featureの議論から派生)
