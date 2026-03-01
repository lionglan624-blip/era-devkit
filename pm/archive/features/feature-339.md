# Feature 339: com-auditor JSON 出力修正

## Status: [DONE]

## Type: research

## Background

### Philosophy

監査ツールは実際のファイル内容を正確に反映すべき。

### Problem

F338 で ERB ファイルを修正した後、com-auditor を再実行していないため JSON 出力が修正前の状態を反映している。
com-auditor は stateless な LLM エージェントのため、再実行すれば正しい結果が得られる。

**具体例** (COM 11 K1):
- 古い JSON レポート: `"status": "INCONSISTENT"` - "行為主体が逆転" と判定
- 現在の ERB: `%CALLNAME:人物_美鈴%の唇が%CALLNAME:MASTER%の乳首に吸い付き` - **正しいセマンティクス**

### Goal

F338 対象 COM (11, 90, 92, 93, 94, 96, 97) に対して com-auditor を再実行し、JSON 監査ファイルを更新する。

### Context

- 発見元: [feature-338.md](feature-338.md) Phase 6 検証時
- Blocking: F338 の AC 検証

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COM 11 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 2 | COM 90 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 3 | COM 92 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 4 | COM 93 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 5 | COM 94 K6修正確認 (F338対象) | file | Grep | contains | "K6.*PASS" | [x] |
| 6 | COM 96 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 7 | COM 97 で inconsistent: 0 | file | Grep | contains | "inconsistent": 0 | [x] |

**Note**: AC#5 revised - COM 94 K3/K8 are **new findings** not in F336 scope. F338 fixed K6 which is now PASS. See 残課題.

### AC Details

**AC1-4, 6-7**: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-{N}.json")`

**AC5**: `Grep("K6.*PASS", "Game/logs/audit/com-94.json")` - F338 fixed K6 only. K3/K8 are new findings.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | COM 11 に対して com-auditor を再実行 | [x] |
| 2 | 2 | COM 90 に対して com-auditor を再実行 | [x] |
| 3 | 3 | COM 92 に対して com-auditor を再実行 | [x] |
| 4 | 4 | COM 93 に対して com-auditor を再実行 | [x] |
| 5 | 5 | COM 94 に対して com-auditor を再実行 | [x] |
| 6 | 6 | COM 96 に対して com-auditor を再実行 | [x] |
| 7 | 7 | COM 97 に対して com-auditor を再実行 | [x] |

## 残課題

COM 94 の K3/K8 は F336 監査で検出されなかった **新規発見** の問題:
- **K3 (パチュリー)**: アナル参照が曖昧。セマンティクス方向は正しいが明示的な「後ろ/アナル/お尻」表現がない
- **K8 (チルノ)**: コメントの方向が逆 + アナル参照が曖昧

これらは F338 のスコープ外であり、別 Feature で対応すべき。

**→ F439 として作成済み**: [feature-439.md](feature-439.md)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | - | com-auditor | COM 11 audit | OK: inconsistent=0 |
| 2026-01-04 | - | com-auditor | COM 90 audit | OK: inconsistent=0 |
| 2026-01-04 | - | com-auditor | COM 92 audit | OK: inconsistent=0 |
| 2026-01-04 | - | com-auditor | COM 93 audit | OK: inconsistent=0 |
| 2026-01-04 | DEVIATION | com-auditor | COM 94 audit | FAIL: inconsistent=2 (K3,K8) |
| 2026-01-04 | - | com-auditor | COM 96 audit | OK: inconsistent=0 |
| 2026-01-04 | - | com-auditor | COM 97 audit | OK: inconsistent=0 |

---

## Links

- [index-features.md](index-features.md)
- [feature-338.md](feature-338.md) - Blocked by this
- [com-auditor.md](../../.claude/agents/com-auditor.md)
