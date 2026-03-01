# Feature 342: COM 94 K3/K8 アナル参照明示化

## Status: [DONE]

## Type: kojo

## Background

### Philosophy

口上は COM のセマンティクスを明確に反映すべき。

### Problem

F339 の com-auditor 再実行で、COM 94 (Ａ騎乗位する) の K3/K8 に新規問題を発見。
これらは F336 監査で検出されなかった問題で、F338 のスコープ外。

**具体的問題**:
- **K3 (パチュリー)**: アナル参照が曖昧。セマンティクス方向は正しいが明示的な「後ろ/アナル/お尻」表現がない
- **K8 (チルノ)**: コメントの方向が逆 + アナル参照が曖昧

### Goal

COM 94 の K3/K8 を修正し、明示的なアナル参照を追加する。

### Context

- 発見元: [feature-339.md](feature-339.md) com-auditor 再実行
- 監査ファイル: `Game/logs/audit/com-94.json`

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COM 94 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |

### AC Details

**AC1**: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-94.json")`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K3/K8 COM 94 kojo にアナル参照を追加し com-auditor で検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 20:58 | START | implementer | Task 1 | - |
| 2026-01-04 20:58 | END | implementer | Task 1 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-336.md](feature-336.md) - 関連Feature (セマンティクス監査)
- [feature-338.md](feature-338.md) - 関連Feature (セマンティクス修正)
- [feature-339.md](feature-339.md) - 発見元Feature
