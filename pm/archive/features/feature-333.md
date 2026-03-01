# Feature 333: research Type 追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Feature Type によりワークフローを適切に分岐させ、各タイプに必要な検証ステップを確実に実行する。

### Problem (Current Issue)
F329 (kojo workflow gap investigation) で以下の問題が発生:
1. 調査時に F323 の存在を見落とし、誤った結論を導出
2. com_file_map.json の現状を直接確認せず、ドキュメントの記述を信頼
3. 結果として不要な F331 が提案された

根本原因: 調査型 Feature に「成果物現状確認」「関連Feature追跡」ステップが定義されていなかった。
現在の Type (kojo/erb/engine/infra) には調査専用のワークフローがない。

### Goal (What to Achieve)
`research` Type を追加し、調査型 Feature に必須の検証ステップを do.md に定義する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | feature-template.md に research Type 追加 | file | Grep | contains | `research` in Criteria by Type | [x] |
| 2 | do.md Target Selection に research 追加 | file | Grep | contains | `research.*Lowest pending` | [x] |
| 3 | do.md Type Routing に research 追加 | file | Grep | contains | `research.*Artifact confirmation` | [x] |
| 4 | do.md Phase 2 に research 専用ステップ追加 | file | Grep | contains | `For research only` | [x] |
| 5 | do.md Phase 4 に Type: research セクション追加 | file | Grep | contains | `Type: research` | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | feature-template.md Criteria by Type テーブルに research 追加 | [x] |
| 2 | 2 | do.md Target Selection に research 追加 | [x] |
| 3 | 3 | do.md Type Routing テーブルに research 追加 | [x] |
| 4 | 4 | do.md Phase 2 に research 専用ワークフロー追加 | [x] |
| 5 | 5 | do.md Phase 4 に Type: research セクション追加 | [x] |

---

## Design Details

### research Type の特徴

| 項目 | 内容 |
|------|------|
| 目的 | 調査報告書作成 |
| AC数 | 3-5 |
| 検証 | 静的検証 + 成果物確認必須 |

### Phase 2 追加ステップ

1. **Related Feature Discovery**: 参照Feature全リスト + 依存グラフ構築
2. **Artifact Current State Confirmation**: 成果物を直接読み取り、現状スナップショット記録

### Phase 4 追加要件

1. 結論には成果物エビデンス必須
2. 参照Featureは全てリンク
3. 「Feature X で解決済み」の主張は Feature X AND 成果物で検証

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | START | Opus | feature-template.md 編集 | SUCCESS |
| 2026-01-04 | - | Opus | do.md Target Selection 編集 | SUCCESS |
| 2026-01-04 | - | Opus | do.md Type Routing 編集 | SUCCESS |
| 2026-01-04 | - | Opus | do.md Phase 2 research 追加 | SUCCESS |
| 2026-01-04 | END | Opus | do.md Phase 4 research 追加 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 契機: [feature-329.md](feature-329.md) (調査時の見落とし問題)
- 関連: [feature-331.md](feature-331.md) (誤った調査結果に基づく提案 → WITHDRAWN)
