# Feature 203: Workflow Infrastructure Audit

## Status: [DONE]

## Type: infra

## Background

### Problem

Feature 186 実装中に虚偽報告・テスト失敗が発覚。F202 で「対応済み」とした項目が機能していない。

**根本原因の仮説**: skills / subagents / hooks の参照・連携が不完全。

### Goal

1. 全 skills の内容と参照状況を監査
2. 全 subagents の Skill tool 使用状況を監査
3. 全 hooks の動作状況を監査
4. kojo_test_gen.py の全 COM 対応状況を監査
5. 本番ログ収集の仕組みを監査 (hook or engine)
6. 発見した問題を Feature 204 に反映

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Skills 監査完了 | doc | exists | Skills Audit セクション | [x] |
| 2 | Subagents 監査完了 | doc | exists | Subagents Audit セクション | [x] |
| 3 | Hooks 監査完了 | doc | exists | Hooks Audit セクション | [x] |
| 4 | kojo_test_gen.py 監査完了 | doc | exists | Python Audit セクション | [x] |
| 5 | ログ収集監査完了 | doc | exists | Log Collection Audit セクション | [x] |
| 6 | Feature 204 更新 | doc | contains | 監査結果反映 | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Skills 監査 | [O] |
| 2 | 2 | Subagents 監査 | [O] |
| 3 | 3 | Hooks 監査 | [O] |
| 4 | 4 | kojo_test_gen.py 監査 | [O] |
| 5 | 5 | ログ収集監査 | [O] |
| 6 | 6 | Feature 204 更新 | [O] |

---

## 修正方針

### 基本原則: Skills に情報集約

```
agent.md    → "何をするか" (ミッション、判断基準)
skill       → "どうやるか" (コマンド、構文、手順)
```

### testing skill 整理案

| File | 内容 | 移動元 |
|------|------|--------|
| testing/SKILL.md | Quick Reference (現状維持) | - |
| testing/KOJO.md | --unit 詳細 (現状維持) | - |
| testing/FLOW.md | --flow 詳細 (**新規**) | regression-tester.md |
| testing/MATCHERS.md | Matcher 一覧 (**新規**) | ac-tester.md (重複解消) |

### agent.md 整理案

| Agent | 変更内容 |
|-------|---------|
| regression-tester.md | How to 情報削除 → `Skill(testing)` 参照のみ |
| ac-tester.md | Matchers 削除 → `Skill(testing)` 参照のみ |

---

## Skills Audit

### 存在する Skills

| Skill | Directory | Files | 用途 | 状態 |
|-------|-----------|-------|------|:----:|
| testing | `.claude/skills/testing/` | SKILL.md, KOJO.md, ERB.md, ENGINE.md | テスト実行方法 | ✅ |
| erb-syntax | `.claude/skills/erb-syntax/` | SKILL.md | ERB構文リファレンス | ✅ |
| kojo-writing | `.claude/skills/kojo-writing/` | SKILL.md, canon-lines.md | 口上執筆ガイド | ✅ |
| engine-dev | `.claude/skills/engine-dev/` | SKILL.md | C#エンジン開発 | ✅ |

### testing skill 内容確認

| File | 内容 | --flow 記載 |
|------|------|:-----------:|
| SKILL.md | Quick Reference, AC Format, Test Types, Pos/Neg, --debug | ⚠️ `flow \| dotnet run ... -- . \| ERB integration` のみ |
| KOJO.md | --unit 詳細、JSON Scenario、Batch Testing | ❌ なし |
| ERB.md | ERB tests | 未確認 |
| ENGINE.md | Engine tests | 未確認 |

**発見した問題**: testing skill に `--flow tests/regression/` の詳細説明がない。regression-tester.md に直接書いてある。

### Skills 参照状況

| Subagent | tools に Skill | Skill 参照記述 | 問題 |
|----------|:--------------:|:--------------:|------|
| regression-tester | ❌ | ❌ | **Skill 使用不可、testing 未参照** |
| ac-tester | ❌ | ❌ | **Skill 使用不可、testing 未参照** |
| implementer | ✅ | - | どの skill を使うか不明 |
| kojo-writer | ✅ | ✅ `kojo-writing` | OK |
| debugger | ✅ | ✅ `testing-reference` | OK (ただし参照名が異なる) |
| ac-validator | ✅ | ✅ `testing skill参照` | OK |
| feature-reviewer | ❌ | `testing skill参照` | **tools なし、参照不可** |
| spec-writer | ❌ | `testing skill参照` | **tools なし、参照不可** |
| initializer | ❌ | ❌ | - |
| finalizer | ❌ | ❌ | - |
| eratw-reader | ❌ | ❌ | - |
| feasibility-checker | ✅ | ❌ | Skill あるが参照指示なし |
| goal-setter | ❌ | ❌ | - |
| dependency-analyzer | ❌ | ❌ | - |
| doc-reviewer | ❌ | ❌ | - |
| ac-task-aligner | ❌ | ❌ | - |

### 重複分析

| 項目 | regression-tester.md | testing/SKILL.md | testing/KOJO.md |
|------|:-------------------:|:----------------:|:---------------:|
| `--unit` コマンド | ✅ 詳細 | ✅ 簡易 | ✅ 詳細 |
| `--flow` コマンド | ✅ 詳細 | ❌ なし | ❌ なし |
| `--strict-warnings` | ✅ | ✅ | ❌ |
| TALENT index | ❌ | ❌ | ✅ |
| JSON Scenario | ❌ | ❌ | ✅ |
| テストフェーズ順序 | ✅ | ❌ | ❌ |
| PRE-EXISTING 判定 | ✅ | ❌ | ❌ |

| 項目 | ac-tester.md | testing/SKILL.md |
|------|:-----------:|:----------------:|
| Matchers | ✅ | ✅ **重複** |
| AC Types | ✅ | ❌ |
| Test Commands by Type | ✅ | ✅ 類似 |

---

## Subagents Audit

### 全 Subagent 一覧 (16 agents)

| Agent | model | tools | Skill tool | Skill 参照 | 問題 |
|-------|:-----:|-------|:----------:|:----------:|------|
| ac-task-aligner | haiku | Read, Edit, Glob | ❌ | ❌ | - |
| ac-tester | haiku | Bash, Read, Glob | ❌ | ❌ | **testing 未参照** |
| ac-validator | sonnet | Read, Glob, Grep, Edit, Bash, Skill | ✅ | ✅ | OK |
| debugger | sonnet | Read, Write, Edit, Bash, Glob, Grep, Skill | ✅ | ✅ | OK |
| dependency-analyzer | sonnet | Read, Glob, Grep | ❌ | ❌ | - |
| doc-reviewer | sonnet | Read, Glob, Grep | ❌ | ❌ | - |
| eratw-reader | haiku | Read, Write, Grep | ❌ | ❌ | - |
| feasibility-checker | sonnet | Read, Glob, Grep, Bash, Skill | ✅ | ❌ | 参照指示なし |
| feature-reviewer | opus | Read, Glob, Grep | ❌ | 記述あり | **Skill なしで参照不可** |
| finalizer | haiku | Read, Edit, Bash | ❌ | ❌ | - |
| goal-setter | haiku | Read, Edit, Glob | ❌ | ❌ | - |
| implementer | sonnet | Read, Write, Edit, Bash, Glob, Grep, Skill | ✅ | ❌ | 参照指示なし |
| initializer | haiku | Read, Edit, Glob | ❌ | ❌ | - |
| kojo-writer | opus | Read, Write, Edit, Glob, Grep, Skill | ✅ | ✅ | OK |
| regression-tester | haiku | Bash, Read | ❌ | ❌ | **testing 未参照** |
| spec-writer | sonnet | Read, Write, Edit | ❌ | 記述あり | **Skill なしで参照不可** |

### 問題サマリー

| 問題カテゴリ | 該当 Agent | 修正内容 |
|-------------|-----------|---------|
| Skill tool なし + 参照記述あり | feature-reviewer, spec-writer | tools に Skill 追加 |
| Skill tool なし + testing 必要 | regression-tester, ac-tester | tools に Skill 追加 + 参照指示 |
| Skill tool あり + 参照指示なし | implementer, feasibility-checker | 参照指示追加 |

---

## Hooks Audit

### 存在する Hooks

| Hook | Trigger | Script | 用途 | 状態 |
|------|---------|--------|------|:----:|
| post-code-write | PostToolUse (Write\|Edit) | post-code-write.ps1 | BOM/Build/Strict/C# Test | ✅ |
| pre-ac-write | PreToolUse (Write\|Edit) | pre-ac-write.ps1 | AC ファイル保護 | ✅ |
| pre-bash-ac | PreToolUse (Bash) | pre-bash-ac.ps1 | AC 関連 Bash 保護 | ✅ |
| statusline | StatusLine | statusline.ps1 | ステータス表示 | ✅ |
| Stop | Stop | beep | 完了通知 | ✅ |

### post-code-write.ps1 内容

| Check | 対象 | 動作 |
|-------|------|------|
| 1. BOM | ERB/ERH | UTF-8 BOM なければ追加 |
| 2. Build | 全て | `dotnet build` |
| 3. Strict | ERB のみ | `--strict-warnings` |
| 4. C# Tests | C# のみ | `dotnet test` |

**FAIL 時**: exit 2、ログは `logs/debug/failed/strict/` に保存

### Hook 動作確認

| テスト | 期待動作 | 確認状況 |
|--------|----------|:--------:|
| ERB 編集 | BOM 追加 + Build + Strict | F202 で確認済み |
| C# 編集 | Build + Test | 未確認 |
| 構文エラー ERB | FAIL + ログ保存 | 未確認 |

**発見した問題**: Hook 動作確認の AC がない。F202 では「環境仕様により間接確認」としたが、明示的なテストがない。

---

## 発見した問題一覧

### Critical (機能不全)

| # | カテゴリ | 問題 | 影響 | 修正対象 |
|:-:|----------|------|------|---------|
| 1 | Subagent | regression-tester に Skill tool なし | testing skill 参照不可 | regression-tester.md |
| 2 | Subagent | ac-tester に Skill tool なし | testing skill 参照不可 | ac-tester.md |
| 3 | Subagent | feature-reviewer に Skill tool なし | 参照記述あるのに使えない | feature-reviewer.md |
| 4 | Subagent | spec-writer に Skill tool なし | 参照記述あるのに使えない | spec-writer.md |

### High (整合性不足)

| # | カテゴリ | 問題 | 影響 | 修正対象 |
|:-:|----------|------|------|---------|
| 5 | Skill/Doc | testing skill と agent.md で情報重複 | 二重メンテナンス | testing/ + agent.md 整理 |
| 6 | Subagent | implementer に skill 参照指示なし | どの skill を使うか不明 | implementer.md |
| 7 | Subagent | feasibility-checker に skill 参照指示なし | Skill tool あるのに使われない | feasibility-checker.md |

### Medium (確認不足)

| # | カテゴリ | 問題 | 影響 | 修正対象 |
|:-:|----------|------|------|---------|
| 8 | Hook | Hook 動作の明示的 AC なし | 動作保証がない | F204 AC 追加 |
| 9 | Skill | testing/ERB.md, ENGINE.md 未確認 | 内容不明 | 確認のみ |

### Low (Python)

| # | カテゴリ | 問題 | 影響 | 修正対象 |
|:-:|----------|------|------|---------|
| 10 | Python | kojo_test_gen.py COM_60 非対応 | Phase 5 実行不可 | kojo_test_gen.py |

---

## Python Audit

### kojo_test_gen.py

| 項目 | 状態 | 問題 |
|------|:----:|------|
| COM_48 (愛撫) | ✅ | 動作確認済み (F182) |
| COM_60 (挿入) | ❌ | `CHAR_MAP` が `_愛撫.ERB` 固定 |
| COM マッピング表 | ❌ | 存在しない |
| --feature オプション | ✅ | 動作する |
| --com オプション | ⚠️ | マッピング表がないので意味がない |

**必要な修正**:
1. COM→ファイルマッピング表追加
2. `CHAR_MAP` を COM に応じて動的に変更

---

## Log Collection Audit

### 現状

| 項目 | 状態 | 実装場所 |
|------|:----:|---------|
| AC テスト結果 | ✅ | `logs/ac/` (エンジン側) |
| Regression テスト結果 | ✅ | `logs/regression/` (エンジン側) |
| Hook でのログ収集 | ❌ | 存在しない |
| FAIL 時ログ保存 | ✅ | `logs/debug/failed/` (Hook) |

### 問題

1. ログ収集はエンジン側 (`TestPathUtils.cs`) で実装済み
2. Hook (`post-code-write.ps1`) は FAIL 時のみ保存
3. **サブエージェントがログを参照していない** → regression-tester.md に記載あるが実際に使われていない

**必要な修正**:
1. regression-tester.md に logs/ 確認手順を強化
2. または Hook でログ集約

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | Feature 203 作成 (調査フェーズ) | - |
| 2025-12-24 | - | 予備調査実施 (未承認) | 10件の問題発見 → PROPOSED に戻し |
| 2025-12-24 | initializer | Feature 初期化 | READY: Background/Tasks/ACs完備。WIP遷移 |
| 2025-12-24 17:17 | implementer | Tasks 1-6: 監査セクション形式化 | SUCCESS: All audit sections formalized |
| 2025-12-24 | finalizer | Finalization: AC verification, status update | SUCCESS: All ACs [x], index-features.md updated, READY_TO_COMMIT |

---

## Links

- [feature-204.md](feature-204.md) - 修正フェーズ (旧 F203)
- [feature-202.md](feature-202.md) - 前回の workflow fix
