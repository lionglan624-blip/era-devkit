# Feature 335: DEVIATION 記録とシステム修正の強制

## Status: [DONE]

## Type: infra

## Background

### Philosophy
Fix the system, not just the instance.

### Problem
Feature 325 実行時に3件の DEVIATION が発生したが、Phase 8 まで記録されなかった。
- eratw-reader ERR:section_not_found → 「想定内」として記録せず
- Grep 曖昧結果 → 「解決した」として記録せず
- NEEDS_REVISION → 「正常フロー」として記録せず

根本原因:
1. 「解決/想定内/正常 = 記録不要」という誤認
2. Ad-hoc 対応で終わり、システム修正に至らない

### Goal
1. DEVIATION の定義と即時記録ルールを明文化
2. Ad-hoc ではなくシステム修正を優先する原則を明文化
3. eratw-reader のフォールバック手順を明文化

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Deviation Resolution Principle が do.md に存在 | code | contains | "Deviation Resolution Principle" | [x] |
| 2 | DEVIATION Detection Patterns が do.md に存在 | code | contains | "DEVIATION Detection Patterns" | [x] |
| 3 | 即時記録ルールが明文化 | code | contains | "同じターンで Execution Log に記録" | [x] |
| 4 | Caller Fallback Procedure が eratw-reader.md に存在 | code | contains | "Caller Fallback Procedure" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | do.md に Deviation Resolution Principle 追加 | [x] |
| 2 | 2-3 | do.md に DEVIATION Detection Patterns 追加 | [x] |
| 3 | 4 | eratw-reader.md に Caller Fallback Procedure 追加 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | Implementation | Opus | do.md Deviation Resolution Principle 追加 | SUCCESS |
| 2026-01-04 | Implementation | Opus | do.md DEVIATION Detection Patterns 追加 | SUCCESS |
| 2026-01-04 | Implementation | Opus | eratw-reader.md Caller Fallback Procedure 追加 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 親Feature: [feature-325.md](feature-325.md) (この Feature で問題発覚)
