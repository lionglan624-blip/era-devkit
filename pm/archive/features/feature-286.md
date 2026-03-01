# Feature 286: Subagent 書き込みファイルの BOM 検証

## Status: [DONE]

## Type: infra

## Background

### Philosophy
サブエージェントが作成したファイルも本体と同じ品質基準を満たすことを保証する

### Problem
Feature 282 で kojo-writer (subagent) が作成した K3/K6 の ERB ファイルに UTF-8 BOM が欠落していた。

**原因**: 既存の PostToolUse hook (.claude/hooks/post-code-write.ps1) は ERB ファイルに BOM を自動追加するが、Claude Code の hooks (PreToolUse/PostToolUse) は subagent の Tool 呼び出しでは発火しない (F282 で観測)。kojo-writer subagent が作成したファイルはこの保護をバイパスする。

### Goal
Subagent (kojo-writer) が作成した口上 ERB ファイルの BOM 欠落を自動検出・修正する仕組みを導入

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | BOM 検証スクリプト作成 | exit_code | python tools/verify-bom.py Game/ERB/口上 | succeeds | - | [x] |
| 2 | /do Phase 6 に BOM 一括検証ステップ追加 | code | Grep .claude/commands/do.md | contains | verify-bom.py | [x] |
| 3 | 欠落時の自動修正 | exit_code | python tools/verify-bom.py --fix Game/ERB/口上 | succeeds | - | [x] |
| 4 | BOM 欠落ファイル検知 | exit_code | python tools/verify-bom.py .tmp/test-no-bom.erb | fails | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | verify-bom.py 作成 (Game/ERB/口上 対象の BOM 検証) | [O] |
| 2 | 2 | do.md Phase 6 に BOM 検証ステップ追加 | [O] |
| 3 | 3 | verify-bom.py に --fix オプション追加 | [O] |
| 4 | 4 | BOM 欠落検知テスト (テストファイル作成 + 検証) | [O] |

---

## Design Notes

### 採用: Phase 6 一括検証
- Phase 6 の Hooks 直後、Manual Linter Check の前に `Game/ERB/口上/*.ERB` 全体を BOM チェック
- シンプルで確実
- 欠点: BOM 問題は Phase 6 まで検出されない。ただし --fix フラグで自動修正可能なため許容範囲
- スコープ: 口上ファイル (kojo) のみ対象。他の ERB は既存 PostToolUse hook でカバー済み

**Tool Location**: `tools/verify-bom.py` (infrastructure tooling, alongside verify-logs.py, erb-duplicate-check.py)

**既存 PostToolUse hook との関係**:
- PostToolUse hook (.claude/hooks/post-code-write.ps1): メインエージェントによる Write/Edit 時に BOM を自動追加
- verify-bom.py: サブエージェント完了後に Phase 6 で一括検証・修正
- 両者は補完関係。hook はリアルタイム、verify-bom.py はバッチ処理

### 不採用: SubagentStop hook
- SubagentStop は matcher を持たない (全サブエージェント完了時に発火)
- 特定のファイル変更を検出できず、毎回実行される冗長性
- Phase 6 一括検証の方がシンプルで確実

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 10:32 | START | implementer | Task 1-4 | - |
| 2026-01-01 10:32 | END | implementer | Task 1 | SUCCESS |
| 2026-01-01 10:32 | END | implementer | Task 2 | SUCCESS |
| 2026-01-01 10:32 | END | implementer | Task 3 | SUCCESS |
| 2026-01-01 10:32 | END | implementer | Task 4 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- [Anthropic hooks docs](https://docs.anthropic.com/claude-code/hooks)
- 親Feature: [feature-282.md](feature-282.md)
