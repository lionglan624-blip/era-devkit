# Feature 299: do.md kojo 品質チェック手順追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo Feature は Phase 8d 品質基準 (4-8行 + 感情/場面描写) を満たす実装のみを完了とみなす

### Problem (Current Issue)
現在の `/do kojo` ワークフローでは:
1. `erb-duplicate-check.py` で関数存在をチェック
2. `--check-stub` でスタブか実装済みか判定
3. スタブ → 削除して新規作成
4. **実装済み → そのままスキップ** ← 品質確認なし

問題点:
- 実装済みでも Phase 8d 品質基準を満たしていない可能性
- 古い Phase 8c 品質 (4行のみ) のまま残っている可能性
- Feature 完了時に全キャラが品質基準を満たしているか不明

### Goal (What to Achieve)
do.md の Phase 4.0 Pre-Implementation Check に品質確認ステップを追加し、実装済みでも品質不足の場合は再作成を提案する

### Dependencies
- F300: 既存口上品質監査 (必須: kojo_mapper.py --quality オプションを提供。本 Feature はその機能を do.md ワークフローに統合する)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に品質チェック手順が記載されている | code | Grep | matches | `(quality|品質)` | [x] |
| 2 | do.md に kojo_mapper.py --quality 呼び出し手順がある | code | Grep | contains | `kojo_mapper.py --quality` | [x] |
| 3 | 品質不足時にユーザ確認を行う手順がある | code | Grep | contains | `LOW_QUALITY` | [x] |

### AC Details

**Prerequisites**: F300 must be complete (kojo_mapper.py --quality implemented)

**AC1 Test**: `rg "(quality|品質)" .claude/commands/do.md`
**AC2 Test**: `rg "kojo_mapper.py --quality" .claude/commands/do.md`
**AC3 Test**: `rg "LOW_QUALITY" .claude/commands/do.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | do.md Phase 4.0 に品質チェックセクションを追加 | [O] |
| 2 | 2 | do.md に kojo_mapper.py --quality 呼び出し手順を追加 | [O] |
| 3 | 3 | do.md に LOW_QUALITY 時のユーザ確認分岐を追加 | [O] |

---

## Design Notes

### 品質チェック基準

品質基準の詳細定義は F300 (kojo_mapper.py --quality) を参照。本 Feature は do.md ワークフローへの統合に専念する。

**Output Format Coupling**: AC3 の `LOW_QUALITY` は F300 で定義される出力形式に依存。F300 が出力形式を変更した場合、AC3 の Expected も更新が必要。

### ワークフロー案

**Note**: 本ワークフローは F300 完了を前提とする。F299 は do.md へのワークフロー統合のみを担当し、kojo_mapper.py --quality の実装は F300 の scope。

```
既存実装発見 (--check-stub = IMPLEMENTED)
  ↓
品質チェック (kojo_mapper.py --quality)  ← F300 が提供
  ↓
├─ GOOD_QUALITY → スキップ
└─ LOW_QUALITY → ユーザ確認
     ↓
   ├─ 再作成 → kojo-writer dispatch
   └─ スキップ → 続行
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 11:28 | START | implementer | Task 1-3 | - |
| 2026-01-02 11:28 | END | implementer | Task 1 | SUCCESS |
| 2026-01-02 11:28 | END | implementer | Task 2 | SUCCESS |
| 2026-01-02 11:28 | END | implementer | Task 3 | SUCCESS |
| 2026-01-02 11:30 | DEVIATION | Grep | AC検証 (count mode) | 曖昧結果: カウントとサマリーが矛盾 |
| 2026-01-02 11:30 | VERIFY | Grep | AC検証 (content mode) | PASS - 再検証で全AC確認 |
| 2026-01-02 11:35 | FIX | do.md | A: Grep曖昧結果記録追加, B: content mode標準化 | SUCCESS |

---

## 残課題

Philosophy Gate により検出された将来課題 (本 Feature scope 外):

| 課題 | Scope | 対応方針 |
|------|-------|----------|
| 品質状態の可視化 | F300 | kojo_mapper.py --quality の出力形式で対応 |
| Feature完了条件への品質チェック組み込み | 将来 infra Feature | complete-feature コマンドへの統合を検討 |

---

## Links

- [index-features.md](index-features.md)
- [do.md](../../.claude/commands/do.md)
- [erb-duplicate-check.py](../../tools/erb-duplicate-check.py)
- Related: [feature-300.md](feature-300.md) - 既存口上品質監査
