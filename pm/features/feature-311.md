# Feature 311: do.md Phase 4 MODIFIER 整合性チェック追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo 実装後の品質ゲートとして、構造的整合性を自動検証する

### Problem
F291 で K4 の KOJO_MODIFIER_PRE/POST_COMMON 呼び出しが欠落していたが、Phase 7 (Post-Review) の feature-reviewer で初めて検出された。

KOJO_MODIFIER_PRE_COMMON と KOJO_MODIFIER_POST_COMMON は全ての kojo 関数を囲む必須ラッパー呼び出しで、共通の前処理・後処理を担当する。これらが欠落すると、初期化されていない状態でテストが失敗する。

### Goal
do.md Phase 4 完了後に MODIFIER 呼び出しの整合性チェックを追加し、早期検出を可能にする

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に MODIFIER チェック手順記載 | code | Grep(.claude/commands/do.md) | contains | "KOJO_MODIFIER_PRE_COMMON" | [x] |
| 2 | チェック失敗時の対処手順記載 | code | Grep(.claude/commands/do.md) | contains | "MODIFIER 欠落" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | do.md Phase 4 に MODIFIER 整合性チェックステップ追加 | [O] |
| 2 | 2 | MODIFIER 欠落時の修正手順を do.md に追加 | [O] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 16:14 | START | implementer | Task 1 | - |
| 2026-01-02 16:14 | END | implementer | Task 1 | SUCCESS |
| 2026-01-02 16:16 | START | implementer | Task 2 | - |
| 2026-01-02 16:16 | END | implementer | Task 2 | SUCCESS |

## 残課題

- 構造的整合性の範囲拡張: MODIFIER 以外の構造的整合性要素（関数シグネチャ、変数初期化等）のチェック追加 → [Feature 313](feature-313.md)

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-291.md](feature-291.md)
- 対象: [do.md](../../.claude/commands/do.md)
