# Feature 275: kojo-init AC テンプレート修正

## Status: [DONE]

## Type: infra

## Background

### Problem
kojo-init が生成する feature-{ID}.md の AC テーブルで:
- Expected 列が `"{auto}"` プレースホルダーのまま
- Method 列にテストパスがない

テストパスは固定パターン `tests/ac/kojo/feature-{ID}/test-{ID}-K{N}.json` なので事前に生成可能。

### Goal
kojo-init テンプレートを修正し、正しいテストパスと Expected 値を生成。

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo-init.md AC テーブル Method 列にテストパス | code | contains | "tests/ac/kojo/feature-{ID}" | [x] |
| 2 | kojo-init.md AC テーブル Expected 列修正 | code | not_contains | "{auto}" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | kojo-init.md AC テンプレート修正 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | - | - | kojo-init.md 更新 | OK |

## Links
- [index-features.md](index-features.md)
- 発生元: [feature-244.md](feature-244.md) Issues Found
