# Feature 272: ErbLinter ワークフロー統合

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
静的解析ツールは作成するだけでなく、ワークフローに統合して自動実行されなければ価値がない。
技術的負債の早期発見と防止には、CI/ワークフローへの組み込みが必須。

### Problem (Current Issue)
- ErbLinter (`tools/ErbLinter/`) は機能豊富（デッドコード検出、重複関数、構文チェック）
- しかし `/do` ワークフローに統合されていない
- pre-commit hook でも実行されていない
- 結果: ツールは存在するが完全に「死んでいる」状態
- 現在の検出結果: 重複関数1件（このFeatureで修正）、デッドコード24+件（別Feature）

### Goal (What to Achieve)
1. /do Phase 6 で ErbLinter を自動実行
2. pre-commit で warning 以上をブロック
3. 既存の重複関数警告（EVENTTRAIN）を解消

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に ErbLinter 参照追加 | code | Grep(.claude/commands/do.md) | contains | "ErbLinter" | [ ] |
| 2 | pre-commit に ErbLinter 実行追加 | code | Grep(.githooks/pre-commit) | contains | "ErbLinter" | [ ] |
| 3 | ErbLinter warning 0件（EVENTTRAIN重複解消含む） | exit_code | cd tools/ErbLinter && dotnet run -- -l warning | succeeds | - | [ ] |

### AC Details

**AC1 Test**: `Grep("ErbLinter", ".claude/commands/do.md")`
**Expected**: ErbLinter への参照が存在

**AC2 Test**: `Grep("ErbLinter", ".githooks/pre-commit")`
**Expected**: ErbLinter 実行コマンドが存在

**AC3 Test**:
```bash
cd tools/ErbLinter && dotnet run -- -l warning ../../Game/ERB/
```
**Expected**: Exit code 0 (warning なし、FUNC004 含む全警告が解消されている)
**Verification**: 手動実行で exit code 確認

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | do.md Phase 6 に ErbLinter チェック追加 | [ ] |
| 2 | 2 | pre-commit hook に ErbLinter 実行追加 | [ ] |
| 3 | 3 | EVENTTRAIN 重複解消: (1) BEFORETRAIN.ERB の @EVENTTRAIN 内 FOR ループ (line 24 `CSTR:キャラ番号:11 = /` の後) に `CSTR:キャラ番号:1 =` を追加, (2) COMMON.ERB lines 109-112 (@EVENTTRAIN, FOR, CSTR:LOCAL:1=, NEXT の4行) を削除 | [ ] |

---

## Technical Notes

### ErbLinter 現状

```bash
# 実行コマンド
cd tools/ErbLinter
dotnet run -- -l warning ../../Game/ERB/

# デッドコード検出
dotnet run -- --dead-code ../../Game/ERB/ -f json
```

### 検出済み問題

| 種別 | 件数 | 対応 |
|------|-----:|------|
| 重複関数 (FUNC004) | 1 | **このFeatureで修正** |
| デッドコード (DEAD001) | 24+ | 別Feature（削除は破壊的変更） |
| スタイル警告 (STYLE002) | 2 | 意図的なので無視 |

### EVENTTRAIN 重複の詳細

- `BEFORETRAIN.ERB:3` - 正規実装（#PRI あり、完全な初期化処理）
  - CSTR:*:10, CSTR:*:11 をリセット
  - **CSTR:*:1 はリセットしていない**
- `COMMON.ERB:109` - CSTR:*:1 リセットのみ
  - @ONCE/@FIRSTTIME 関数が使用する初回判定フラグ

**修正手順**:
1. BEFORETRAIN.ERB の @EVENTTRAIN ループ内に `CSTR:キャラ番号:1 =` を追加（line 24 付近）
2. COMMON.ERB の @EVENTTRAIN (lines 109-112) を削除

**重要**: 単純削除は @ONCE/@FIRSTTIME を壊すため、必ずマージ後に削除すること。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links
- [index-features.md](index-features.md)
- [ErbLinter README](../../tools/ErbLinter/README.md)
- [do.md](../../.claude/commands/do.md)
- [pre-commit](../../.githooks/pre-commit)
