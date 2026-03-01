# Hook動作検証設計提案

## Status: DRAFT

**Related Features**: F202, F212, F214

## 概要

Git Hook（PostToolUse, pre-commit）の動作を検証する方針を検討する。

## 背景・動機

### 現状の課題

**F202 で発見された問題**:
- Hook出力はEdit/Writeツールの戻り値とは別チャネル（PostToolUse）で表示される
- Claudeが実行したツールの戻り値には Hook の stdout/stderr が含まれない
- AC定義（output/exit_code matcher）で Hook の動作を自動検証できない

**F202 の結論**:
> Hook は別チャネルで動作、AC1-6 は手動確認が必要

**F212 (CI統合) での文脈**:
- pre-commit hook で回帰テスト（--flow + verify-logs.py）を実行
- commit 拒否/許可の動作は exit code で制御
- Hook は実装済み、動作確認済み（手動）

### 問題の本質

| Hook種類 | 用途 | 検証困難性 |
|---------|------|-----------|
| **PostToolUse** (ERB編集後) | BOM付加、ビルド検証 | 出力が別チャネル、ツール戻り値に含まれない |
| **pre-commit** | 回帰テスト実行、commit制御 | Git操作が必要、状態変化を伴う |

**現状**: Hook は正常に設定・動作しているが、明示的なACがなく、動作保証がない。

---

## Options

### Option 1: 手動確認で十分とする（現状維持）

**方針**: Hook の動作検証は Feature 完了時の手動確認とし、AC定義は行わない。

**Pros**:
- 追加実装不要
- Hook は実装済みで動作確認済み
- F212 で pre-commit hook は既に稼働中

**Cons**:
- 回帰時の検出が遅れる可能性
- Hook 動作の保証がドキュメント化されない
- 設定ミス（.githooks 未設定、Hook スクリプト破損）の検出が遅い

**コスト**: なし

---

### Option 2: Hook出力を検証する仕組みを追加

**方針**: Hook の stdout/stderr を一時ファイルに出力し、Bash で読み取って検証する。

**実装案**:

```bash
# PostToolUse Hook での一時ファイル出力
echo "[Hook] BOM added: $path" | tee -a .tmp/hook-output.log

# AC検証時
grep "[Hook] BOM added" .tmp/hook-output.log
```

**AC定義例**:

| AC# | Description | Type | Matcher | Expected |
|:---:|-------------|------|---------|----------|
| 1 | BOMなしERB編集→BOM付加 | file | contains | "[Hook] BOM added" |
| 2 | pre-commit PASS→commit許可 | exit_code | equals | 0 |

**Pros**:
- AC定義による明示的な動作保証
- 回帰検出が早い（自動検証）
- Hook 動作のトレーサビリティ向上

**Cons**:
- Hook スクリプト修正が必要
- .tmp/ ディレクトリ管理（ログローテーション、クリア）
- 一時ファイルの競合リスク（並列 Hook 実行時）

**コスト**: Hook スクリプト修正、AC定義追加、実装 Feature 1件

---

### Option 3: Hook結果をlogに出力して検証

**方針**: Hook の実行結果を `logs/` 配下に保存し、verify-logs.py で検証する。

**実装案**:

```bash
# PostToolUse Hook (post-code-write.ps1)
# BOM付加処理 → 結果を logs/hook/post-code-write.log に出力
echo "$(date +%Y-%m-%d_%H:%M:%S) BOM_ADDED $path" >> logs/hook/post-code-write.log

# pre-commit Hook
# verify-logs.py で logs/hook/ も検証対象に含める
python tools/verify-logs.py --dir _out/logs/prod --hook-logs _out/logs/hook
```

**Pros**:
- 既存の verify-logs.py 検証フローと統合
- logs/ ディレクトリで一元管理
- トレーサビリティ向上（実行履歴保存）

**Cons**:
- verify-logs.py 拡張が必要
- Hook ログのフォーマット定義が必要
- logs/ 肥大化対策（ローテーション）

**コスト**: Hook スクリプト修正、verify-logs.py 拡張、実装 Feature 1件

---

## Analysis

### 検証の必要性

| Hook | 検証優先度 | 理由 |
|------|:--------:|------|
| **PostToolUse (BOM)** | 中 | F085 で BOM 欠落問題が発生したが、Hook 導入後は未発生 |
| **PostToolUse (Build)** | 低 | imple.md Phase 6 Smoke Test で既に dotnet build 実行 |
| **pre-commit (Regression)** | 高 | commit 可否の判断に直結、誤動作時の影響大 |

**Note**: F212 Design Review より、pre-commit は回帰テストのみを実行する設計に変更済み。

### CI統合後の文脈

**F212 の結論**:
- pre-commit hook は「既存を壊していないか」の最終確認のみ
- AC テストは /imple Phase 7 でループ検証済み（FAIL → debugger → 再実行、max 3回）
- dotnet build/test は engine Type の AC 検証で実行済み

**意味**:
- pre-commit hook は軽量（約4秒、回帰テストのみ）
- 失敗頻度は低い（既に全AC検証済み、回帰のみチェック）
- Hook 動作の重要性は「実装時の保証」より「設定確認」

---

## Recommendation

**Option 1（現状維持）を推奨**。

### 理由

1. **現状で動作している**: F202, F212 で Hook は実装・確認済み、問題なし
2. **CI統合で保証範囲が拡大**: pre-commit hook で verify-logs.py が毎回実行される
3. **失敗頻度が低い**: /imple Phase 7 で AC 検証済み、pre-commit は回帰のみ
4. **コストと効果のバランス**: Option 2/3 は実装コストに対して得られる保証が限定的

### 代替案: 設定確認ACのみ追加

Hook の**動作**ではなく、Hook の**設定**を検証する AC を定義する。

**例** (F214 または新規 Feature):

| AC# | Description | Type | Matcher | Expected |
|:---:|-------------|------|---------|----------|
| 1 | PostToolUse Hook 存在 | file | exists | .claude/hooks/post-code-write.ps1 |
| 2 | pre-commit Hook 存在 | file | exists | .githooks/pre-commit |
| 3 | Hook 設定確認 | command | succeeds | `git config core.hooksPath` → `.githooks` |

**メリット**:
- Hook スクリプト破損・削除の検出
- 新環境セットアップ時の設定確認
- コストが低い（既存ファイルの存在確認のみ）

**制限**:
- Hook の**実行内容**は検証しない（手動確認に依存）
- 設定ミスは検出できるが、ロジックバグは検出できない

---

## 未解決事項

1. **Hook 設定AC の定義場所**: F214 に含めるか、別 Feature とするか
2. **手動確認手順の文書化**: Hook 動作確認手順を testing/SKILL.md に記載するか

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-25 | F214 Task 3 で初期作成、F202/F212 文脈を統合 |
