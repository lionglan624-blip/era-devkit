# Feature 310: kojo-writing SKILL 既存パターン参照ルール追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo-writer は SKILL テンプレートだけでなく、既存コードのパターンも参照すべき

### Problem
F291 で K4 が KOJO_MODIFIER 呼び出しを欠落した。原因は kojo-writer が:
1. SKILL テンプレートのみを参照
2. 同一ファイル内の他 COM 実装 (COM_65 等) を参照しなかった
3. 他キャラの同 COM 実装 (K1 の COM_85) を参照しなかった

### Goal
SKILL.md に「既存パターン参照」ルールを追加し、kojo-writer が既存コードと整合性のある実装を行うようにする

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 既存パターン参照ルール記載 | code | Grep .claude/skills/kojo-writing/SKILL.md | contains | "既存パターン参照" | [x] |
| 2 | 参照優先順位の明記 | code | Grep .claude/skills/kojo-writing/SKILL.md | contains | "1. **同一ファイル内" | [x] |
| 3 | 参照パターン抽出方法の記載 | code | Grep .claude/skills/kojo-writing/SKILL.md | contains | "Grep" | [x] |
| 4 | 参照パターン種類の列挙 | code | Grep .claude/skills/kojo-writing/SKILL.md | contains | "KOJO_MODIFIER" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | SKILL.md に既存パターン参照ルール追加 | [x] |
| 2 | 2 | SKILL.md に参照優先順位を明記 | [x] |
| 3 | 3 | SKILL.md に既存パターン抽出方法 (Grep手順) を追加 | [x] |
| 4 | 4 | SKILL.md に参照すべきパターン種類 (KOJO_MODIFIER, DATALIST等) を列挙 | [x] |

## 残課題

| 項目 | 内容 | 対応方針 |
|------|------|----------|
| 矛盾解決方法 | SKILLテンプレートと既存パターンの矛盾時解決ルール | Phase 28 Task 8 |

**→ Phase 28 Task 8 として追跡**: [full-csharp-architecture.md](designs/full-csharp-architecture.md) Phase 28: Documentation

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 15:59 | START | implementer | Task 1-4 | - |
| 2026-01-02 15:59 | END | implementer | Task 1-4 | SUCCESS |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-291.md](feature-291.md)
- 対象: [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
