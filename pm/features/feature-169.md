# Feature 169: Hooks & Background Process Issues

## Status: [DONE]

## Type: engine

## Background

### Problem

Feature 155 実装中に発覚した2つの問題：

| # | 問題 | 影響 |
|---|------|------|
| 1 | PreToolUse hooks 未登録 | AC Protection が動作せず、テストシナリオを自由に変更できた |
| 2 | バックグラウンドプロセス残留 | headless が 16GB メモリ消費、終了しない |

### Goal

1. PreToolUse hooks の登録と動作検証
2. バックグラウンドプロセス問題の原因特定と対策

### Context

- Feature 163 で hooks 設計・ファイル作成済み、settings.json 登録漏れ
- CLI 移行時に設定がリセットされた可能性
- `--unit` でディレクトリ指定時にプロセスが終了しない現象

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | PreToolUse hooks 登録済み | output | contains | "PreToolUse" | [x] |
| 2 | hooks Write 既存 AC 編集ブロック | code | contains | "[BLOCKED] Cannot modify existing AC/regression file" | [x] |
| 3 | hooks Bash 既存 AC 削除ブロック | code | contains | "[BLOCKED] Destructive operation on ac/regression" | [x] |
| 4 | hooks 新規 AC 作成は正常動作 | code | contains | "[Hook] New AC/regression file allowed" | [x] |
| 5 | --unit ディレクトリ指定でプロセス終了 | exit_code | - | プロセス終了確認 | [x] |
| 6 | engine ビルド成功 | build | succeeds | - | [x] |

### AC Details

**AC 1**: `grep PreToolUse .claude/settings.json`
**AC 2-4**: 手動検証（CLI再起動後にhooks動作確認）
- AC 2: Edit existing AC file via CLI → expect "[BLOCKED] Cannot modify existing AC/regression file"
- AC 3: Bash rm existing AC file via CLI → expect "[BLOCKED] Destructive operation on ac/regression"
- AC 4: Write new AC file via CLI → expect "[Hook] New AC/regression file allowed"
**AC 5**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-155/`
**AC 6**: `dotnet build engine/uEmuera.Headless.csproj`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | settings.json に PreToolUse hooks 登録確認 | [x] |
| 2 | 2 | hooks Write 既存 AC 編集ブロック検証（手動） | [x] |
| 3 | 3 | hooks Bash 既存 AC 削除ブロック検証（手動） | [x] |
| 4 | 4 | hooks 新規 AC 作成許可検証（手動） | [x] |
| 5 | 5 | --unit ディレクトリ指定問題調査・修正 | [x] |
| 6 | 6 | engine ビルド確認 | [x] |

---

## Investigation Notes

### 問題1: PreToolUse hooks

- `.claude/hooks/pre-ac-write.ps1` - 存在する
- `.claude/hooks/pre-bash-ac.ps1` - 存在する
- `.claude/settings.json` - PreToolUse 未登録だった → 2025-12-21 追加済み

### 問題2: バックグラウンドプロセス

発生条件:
- `--unit tests/ac/kojo/feature-155/` (ディレクトリ指定)
- 10ファイル、各1テスト

観測:
- メモリ 16GB 消費
- プロセス終了しない
- 個別ファイル指定では正常終了

仮説:
- ディレクトリ内ファイル列挙後のループ問題
- テスト間のリソース解放問題
- 無限ループ（条件判定バグ）

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | ISSUE | - | Feature 155 中に発覚 | 2問題特定 |
| 2025-12-21 | FIX | - | PreToolUse hooks 追加 | settings.json 更新 |
| 2025-12-21 | IMPL | implementer | KojoBatchRunner プロセス分離実装 | SUCCESS |
| 2025-12-21 | TEST | - | ディレクトリモード終了確認 | PASS |
| 2025-12-21 | DONE | - | Feature 169 完了 | All AC PASS |

---

## Links

- [feature-163.md](feature-163.md) - AC/Regression Protection Hooks（元Feature）
- [feature-155.md](feature-155.md) - 問題発覚時のFeature
