# Feature 312: CLAUDE.md ERATW_PATH 環境変数設定手順追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy
eratw-reader が安定して動作するために、環境設定が明文化されている必要がある

### Problem
F291 セッションで eratw-reader の初回実行が失敗した。原因:
1. ERATW_PATH 環境変数が未設定
2. CLAUDE.md にデフォルトパスの記載はあるが、設定手順がない
3. ユーザーが事前にパス設定と確認を行う手順が文書化されていなかった

### Goal
CLAUDE.md に ERATW_PATH 環境変数の設定手順を追加し、初回セットアップを明確化する

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ERATW_PATH 設定手順記載 | code | Grep(CLAUDE.md) | contains | setx ERATW_PATH | [x] |
| 2 | デフォルトパス存在確認手順記載 | code | Grep(CLAUDE.md) | contains | Test-Path | [x] |

### AC Details

**Verification**: `python tools/ac-static-verifier.py --feature 312 --ac-type code`

**AC1**: CLAUDE.md に `setx ERATW_PATH` が記載されている
**AC2**: CLAUDE.md に `Test-Path` が記載されている

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | CLAUDE.md External Dependencies > eraTW に `setx ERATW_PATH "path"` コマンド追加 | [x] |
| 2 | 2 | CLAUDE.md External Dependencies > eraTW に `Test-Path $env:ERATW_PATH` 確認手順追加 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 15:51 | START | implementer | Task 1 | - |
| 2026-01-02 15:51 | START | implementer | Task 2 | - |
| 2026-01-02 15:51 | END | implementer | Task 1 & 2 | SUCCESS |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-291.md](feature-291.md)
- 対象: [CLAUDE.md](../../CLAUDE.md)
