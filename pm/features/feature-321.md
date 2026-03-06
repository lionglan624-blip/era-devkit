# Feature 321: FL コマンド Validation 必須化ドキュメント改善

## Status: [DONE]

## Type: infra

## Background

### Problem
FL 320 実行時にオーケストレータが Validation phase を skip し、自己判断で修正を適用した結果、scope 変更 (14件→2件) が Validation なしで実行され、最終的にデータ喪失が発生。

原因:
1. Phase 2 (Validate) の必須性がドキュメントで弱かった
2. 擬似コードが実行フローを曖昧にしていた
3. 「自己判断禁止」が明記されていなかった
4. scope 変更の検出方法が不明確だった

### Goal
FL コマンドのドキュメントを改善し、Validation skip を防止する。

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | オーケストレータ禁止事項が追加 | code | contains | "オーケストレータの禁止事項" | [x] |
| 2 | Iteration チェックリストが追加 | code | contains | "Iteration チェックリスト" | [x] |
| 3 | Phase 2 に必須注記が追加 | code | contains | "Phase 2: Validate (必須 - 省略禁止)" | [x] |
| 4 | Scope 変更の検出セクションが追加 | code | contains | "Scope 変更の検出" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Section 3 に禁止事項を追加 | [x] |
| 2 | 2 | Section 3 にチェックリストを追加 | [x] |
| 3 | 3 | 擬似コード内 Phase 2 に必須注記を追加 | [x] |
| 4 | 4 | Section 2 に Scope 変更検出を追加 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 | COMPLETE | Opus | 4つの改善を fl.md に適用 | DONE |

---

## Links

- [fl.md](../../.claude/commands/fl.md)
