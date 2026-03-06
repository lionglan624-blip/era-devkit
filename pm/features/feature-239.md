# Feature 239: Audit Command Consistency Fix

## Type: infra

## Status: [DONE]
Created: 2025-12-27

## Summary

Quick Win: audit.md と CLAUDE.md の実態との不整合を修正する。

## Scope

- audit.md B6.3: テストディレクトリ構造の更新
- audit.md B8b.1: Hooks一覧の更新
- CLAUDE.md: 存在しない `pre-ac-write.ps1` 参照の修正
- CLAUDE.md: 未登録subagentの追加

## Tasks

| # | Task | Status |
|:-:|------|:------:|
| 1 | audit.md B6.3 テストディレクトリ修正 | [x] |
| 2 | audit.md B8b.1 Hooks修正 | [x] |
| 3 | CLAUDE.md pre-ac-write.ps1 参照修正 | [x] |
| 4 | CLAUDE.md Subagent Strategy 追加 | [x] |

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | B6.3にac/, regression/, scripts/が含まれる | output | contains | "ac/, regression/, scripts/" | [x] |
| 2 | B8b.1にpost-build-shortcut.ps1が含まれる | output | contains | "post-build-shortcut.ps1" | [x] |
| 3 | CLAUDE.mdにpre-ac-write.ps1が存在しない | output | not_contains | "pre-ac-write.ps1" | [x] |
| 4 | CLAUDE.mdにgoal-setterが登録されている | output | contains | "goal-setter" | [x] |

## Progress Log

- 2025-12-27: Feature created as Quick Win
- 2025-12-27: All tasks completed, marked DONE
