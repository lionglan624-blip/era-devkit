# Feature 201: /imple Workflow & Documentation Fixes

## Status: [CANCELLED] → Feature 202 に統合

## Type: infra

## Background

### Problem

Feature 192 の実装過程で複数のワークフロー問題、ドキュメント不整合、手戻りの原因が発見された。これらを修正しないと、今後の /imple 実行で同じ問題が繰り返される。

### Goal

/imple が警告・エラー・手戻り・参照間違いなく一発で完了するようにする。

### Context

Feature 192 (COM_10 乳首吸い) 実装時に発見された問題のみを対象とする。

---

## 発見された問題一覧

### 1. kojo-writer.md ERB配置表の不整合

**問題**: COM_10 (乳首吸い) は独立ファイル `KOJO_K{N}_乳首吸い.ERB` だが、kojo-writer.md の ERB Placement テーブルでは `0-9` が「愛撫系」→ `KOJO_K{N}_愛撫.ERB` と記載されており、実際の構成と不一致。

**影響**: kojo-writer が誤ったファイルを編集しようとする可能性。

**修正**:
```diff
| COM | Category | File |
|-----|----------|------|
-| 0-9, 20-21, 40-48 | 愛撫系 | `KOJO_K{N}_愛撫.ERB` |
+| 0, 20-21 | 愛撫系 | `KOJO_K{N}_愛撫.ERB` |
+| 1-9 | 個別系 | `KOJO_K{N}_{COM名}.ERB` (独立ファイル) |
+| 40-48 | 道具系 | `KOJO_K{N}_{COM名}.ERB` (独立ファイル) |
```

または COM→ファイル対応の完全なマッピングを別リファレンスに記載。

---

### 2. regression-tester.md のディレクトリ一括指定が機能しない

**問題**: `--unit tests/ac/kojo/` のようなディレクトリ指定が `[KojoBatch] No files matched` エラーになる。各 feature ディレクトリを個別に実行する必要がある。

**影響**: regression-tester が「全 AC テスト実行」を正しく行えない。

**修正候補**:
1. regression-tester.md に「各 feature-{N}/ を個別実行」と明記
2. または engine 側で再帰検索をサポート

---

### 3. Windows 環境での for ループ構文

**問題**: `for dir in tests/ac/kojo/feature-*/; do ... done` が Windows bash で動作しない。

**影響**: regression-tester の一括実行スクリプトが失敗。

**修正**: PowerShell 構文または Windows 互換コマンドを regression-tester.md に追記:
```powershell
Get-ChildItem -Directory tests/ac/kojo/feature-* | ForEach-Object { ... }
```

---

### 4. Phase 10 完了報告後の問題抽出依頼

**問題**: ユーザーが「全ての問題を抽出できる手順にしたい」と述べているが、現在の Phase 10 完了報告では問題リストが含まれていない。

**影響**: 潜在的な問題が見逃され、ユーザーが手動で確認する必要がある。

**修正**: imple.md Phase 10 の報告テンプレートに「発見された問題 (Issues Found)」セクションを追加:
```
=== Feature {ID} 実装完了 ===
...
**Issues Found (このFeatureスコープ外)**:
- {問題1}
- {問題2}
...
```

---

### 5. Feature 186 の PRE-EXISTING 失敗の明確化

**問題**: 回帰テストで Feature 186 が 48 件失敗しているが、これが PRE-EXISTING であることの判定基準が曖昧。

**影響**: 新規失敗と既存失敗の区別ができず、不要なデバッグや誤った BLOCK が発生。

**修正**: regression-tester.md に PRE-EXISTING 判定フロー追加:
```
1. 失敗した feature の Status を確認
2. [WIP] なら PRE-EXISTING (進行中の Feature)
3. [DONE] なら NEW (既完了 Feature の回帰)
```

---

### 6. ac-tester の {auto} 期待値更新

**問題**: AC テーブルの Expected が `{auto}` のまま。ac-tester は status ファイルから実際の期待値を取得して更新するが、その更新が feature-{ID}.md に反映されない場合がある。

**影響**: AC テーブルが不完全なまま残る。

**修正**: ac-tester.md に「{auto} を実際の値で置換必須」と明記。

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo-writer.md ERB配置表修正 | file | contains | "個別系" or COM→ファイルマッピング | [ ] |
| 2 | regression-tester.md ディレクトリ実行手順修正 | file | contains | "各 feature-{N}/ を個別実行" | [ ] |
| 3 | regression-tester.md Windows 互換コマンド追記 | file | contains | "PowerShell" or "Windows" | [ ] |
| 4 | imple.md Phase 10 問題報告セクション追加 | file | contains | "Issues Found" | [ ] |
| 5 | regression-tester.md PRE-EXISTING 判定フロー追加 | file | contains | "[WIP] なら PRE-EXISTING" | [ ] |
| 6 | ac-tester.md {auto} 更新必須化 | file | contains | "{auto} を実際の値で置換" | [ ] |
| 7 | ビルド成功 | build | succeeds | - | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md 修正 | [ ] |
| 2 | 2-3 | regression-tester.md 修正 | [ ] |
| 3 | 4 | imple.md 修正 | [ ] |
| 4 | 5 | regression-tester.md PRE-EXISTING 判定追加 | [ ] |
| 5 | 6 | ac-tester.md 修正 | [ ] |
| 6 | 7 | ビルド確認 | [ ] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| | | | |

---

## Links

- [index-features.md](index-features.md)
- Source: Feature 192 実装時の発見事項
