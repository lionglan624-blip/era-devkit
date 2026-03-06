# Feature 151: Skills全面展開

## Status: [DONE]

## Type: infra

## Background

Feature 150 の実験結果に基づく Skills 適用拡大。

**Feature 150 の結論:**
- ❌ `skills:` フィールド - subagent に自動注入されない（メタデータのみ）
- ✅ `tools: Skill` - subagent が Skill ツールを使える

**対象 Skills:**
| Skill | 用途 |
|-------|------|
| erb-syntax | ERB 構文リファレンス |
| kojo-writing | 口上作成ガイド |
| testing | テストリファレンス |
| engine-dev | C# エンジン開発 |

**対象 Agents 分析:**
| Agent | Model | 現在の tools | Skill 必要性 | 理由 |
|-------|-------|-------------|:------------:|------|
| implementer | sonnet | ✅ Skill あり | - | 150 で追加済み |
| kojo-writer | opus | Skill なし | **CRITICAL** | kojo-writing 必須 |
| debugger | sonnet | Skill なし | **HIGH** | 多種 Skills 参照 |
| ac-validator | sonnet | Skill なし | MEDIUM | testing 参照可能 |
| feasibility-checker | sonnet | Skill なし | MEDIUM | erb/engine 参照可能 |
| eratw-reader | haiku | Skill なし | SKIP | 単純抽出タスク |
| ac-tester | haiku | Skill なし | LOW | 単純実行タスク |
| smoke-tester | haiku | Skill なし | SKIP | 単純実行タスク |
| regression-tester | haiku | Skill なし | SKIP | 単純実行タスク |
| doc-reviewer | sonnet | Skill なし | SKIP | ドメイン知識不要 |
| initializer | haiku | Skill なし | SKIP | 状態更新タスク |
| finalizer | haiku | Skill なし | SKIP | 状態更新タスク |
| ac-task-aligner | haiku | Skill なし | SKIP | メタデータ整合 |

**スコープ決定:**
- **必須**: kojo-writer, debugger（高価値）
- **任意**: ac-validator, feasibility-checker（中価値）
- **除外**: haiku 系 / 単純タスク系（低価値、コスト増）

---

## Overview

Feature 150 で確立した `tools: Skill` パターンを高価値 subagent に展開。

## Goals

1. kojo-writer に `tools: Skill` 追加（kojo-writing スキル利用可能化）
2. debugger に `tools: Skill` 追加（多種スキル利用可能化）
3. 中価値 agent への展開（ac-validator, feasibility-checker）
4. 実効性検証（Skill 実際使用確認）

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo-writer に Skill ツール追加 | file | contains | "Skill" | [x] |
| 2 | debugger に Skill ツール追加 | file | contains | "Skill" | [x] |
| 3 | ac-validator に Skill ツール追加 | file | contains | "Skill" | [x] |
| 4 | feasibility-checker に Skill ツール追加 | file | contains | "Skill" | [x] |
| 5 | ビルド成功 | build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md の tools に Skill 追加 | [O] |
| 2 | 2 | debugger.md の tools に Skill 追加 | [O] |
| 3 | 3 | ac-validator.md の tools に Skill 追加 | [O] |
| 4 | 4 | feasibility-checker.md の tools に Skill 追加 | [O] |
| 5 | 5 | dotnet build 実行確認 | [O] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Execution Log

- **2025-12-20 00:00**: Initialized by initializer agent. Feature 151 now WIP.
- **2025-12-20 completion**: All ACs verified and tasks completed. Skills added to kojo-writer, debugger, ac-validator, feasibility-checker. Build succeeds. Status updated to DONE by finalizer agent.

---

## Links

- [feature-150.md](feature-150.md) - Skills 導入実験（依存）
- [index-features.md](index-features.md) - Feature tracking
