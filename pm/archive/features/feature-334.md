# Feature 334: /fl に reference-checker 追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
/fl でレビューする際、形式的な完全性だけでなく、参照の整合性も機械的に検証する。

### Problem (Current Issue)
F329 を /fl でレビューしたが、以下の問題を検出できなかった:
- F323 が Background/Problem で言及されていたが Links に含まれていなかった
- 「F320で90-95に拡張」という主張がF323の存在を見落としていた

現在の feature-reviewer は自由なレビューを行うが、参照の網羅性チェックは機械的に行うべき。

### Goal (What to Achieve)
/fl の最初に reference-checker を実行し、参照漏れを機械的に検出する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | reference-checker.md 作成 | file | Glob | exists | .claude/agents/reference-checker.md | [x] |
| 2 | fl.md に Phase 0 追加 | file | Grep | contains | `Phase 0: Reference Check` | [x] |
| 3 | CLAUDE.md Subagent Strategy に追加 | file | Grep | contains | `reference-checker` | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | reference-checker.md 作成（参照検証ロジック定義） | [x] |
| 2 | 2 | fl.md に Phase 0 として reference-checker 追加 | [x] |
| 3 | 3 | CLAUDE.md Subagent Strategy テーブルに追加 | [x] |

---

## Design Details

### reference-checker の役割

| チェック | 内容 |
|---------|------|
| missing_link | Background/Problem で言及された Feature が Links に含まれるか |
| missing_artifact | 参照された成果物（JSON等）が存在するか |
| unverified_claim | 「Feature X で解決済み」主張の検証（research type のみ、CRITICAL） |

### /fl フロー変更

```
Before: Review → Validate → AC Validation → Feasibility
After:  Reference Check → Review → Validate → AC Validation → Feasibility
```

### 対象範囲

- 全 feature type に適用（research だけでなく）
- 参照漏れは research 以外でも発生しうるため

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | START | Opus | reference-checker.md 作成 | SUCCESS |
| 2026-01-04 | - | Opus | fl.md Phase 0 追加 | SUCCESS |
| 2026-01-04 | END | Opus | CLAUDE.md 更新 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 契機: [feature-329.md](feature-329.md) (F323見落とし問題)
- 関連: [feature-333.md](feature-333.md) (research type 追加)
