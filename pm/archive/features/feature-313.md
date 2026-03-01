# Feature 313: kojo 構造的整合性チェック範囲拡張

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo 実装後の品質ゲートとして、構造的整合性を自動検証する

### Problem
F311 で MODIFIER チェックを追加したが、MODIFIER 以外にも kojo 関数の構造的整合性要素が存在する:
- 関数シグネチャ（引数の型・数）
- 変数初期化（LOCAL=0 等）
- RETURN 文の存在と値

これらが欠落・不整合の場合、テスト実行時に予期せぬ失敗が発生する。

### Goal
MODIFIER 以外の構造的整合性要素のチェックを do.md Phase 4 に追加し、早期検出を可能にする

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | do.md Phase 4 に Step 4.3 RETURN 文チェック手順が追加されている | code | contains | "4.3 RETURN Statement Check" | [x] |
| 2 | RETURN 0 パターン検証手順がGrep使用で定義されている | code | contains | 'Grep("RETURN 0"' | [x] |
| 3 | 失敗時の Fix Procedure が定義されている | code | contains | "RETURN Fix Procedure" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | do.md Phase 4 に Step 4.3 RETURN 文整合性チェック手順を追加 | [O] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 16:47 | START | implementer | Task 1 | - |
| 2026-01-02 16:48 | END | implementer | Task 1 | SUCCESS |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-311.md](feature-311.md)
