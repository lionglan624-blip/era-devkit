# Feature 308: Post-Review Doc-Check Mode

## Status: [DONE]

## Type: infra

## Background

### Problem
- コード変更時にドキュメント (skills/commands/agents) の更新漏れが発生
- F300 実装時に kojo_mapper.py を変更したが testing skill 更新を忘れた
- 既存の post-review は品質チェックのみで、ドキュメント整合性はチェックしていなかった

### Goal
feature-reviewer に doc-check mode を追加し、変更ファイルに対応するドキュメント更新を強制する

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | feature-reviewer.md に Mode: doc-check セクション存在 | code | Grep | contains | "Mode: doc-check" | [x] |
| 2 | do.md Phase 7 に Step 7.2 存在 | code | Grep | contains | "Step 7.2" | [x] |
| 3 | do.md Phase 7 に doc-check dispatch 記載 | code | Grep | contains | "mode: doc-check" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | feature-reviewer.md に Mode: doc-check セクション追加 | [x] |
| 2 | 2,3 | do.md Phase 7 を 2-step 構成に変更 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 09:10 | CREATE | opus | F300 実行中に問題発覚、Feature 作成 | - |
| 2026-01-02 09:12 | CODE | opus | feature-reviewer.md 更新 | SUCCESS |
| 2026-01-02 09:13 | CODE | opus | do.md Phase 7 更新 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 発生源: [feature-300.md](feature-300.md)
