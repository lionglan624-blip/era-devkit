# Feature 260: Dispatch 最小化 + Subagent 整合

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Subagents は SSOT (Skills/docs) を参照して自律的に正しく実行する。dispatch 側の過剰指定を排除する。

### Problem
F242 実装時に以下の問題が発生:
1. Opus が explorer 結果を kojo-writer prompt にハードコード → SSOT と矛盾
2. do.md の Dispatch Prompt Template が自由度が高すぎ、SSOT 違反を誘発
3. Subagent (agent.md) に実装詳細が混在し、SKILL との二重管理状態

### Goal
1. **do.md の dispatch を最小化**: `{ID} K{N}` 形式のみ
2. **FORBIDDEN セクション追加**: Opus が追加してはいけない内容を明記
3. **Subagent を標準テンプレート化**: Input/Output/STOP のみ、詳細は SKILL 参照

### Context
- 発生箇所: F242 Phase 4
- 関連: F190 (COM→ファイルマッピング), F259 (重複検知)
- 公式準拠: Anthropic Skills Documentation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に Minimal Dispatch Format セクション | file | Grep | contains | "## Minimal Dispatch Format" | [x] |
| 2 | do.md kojo dispatch 形式が最小形式と文書化 | file | Grep | contains | "`{ID} K{N}`" | [x] |
| 3 | do.md に FORBIDDEN セクション | file | Grep | contains | "## FORBIDDEN" | [x] |
| 4 | kojo-writer.md が標準テンプレート (Input セクション) | file | Grep | contains | "## Input" | [x] |
| 5 | kojo-writer.md が標準テンプレート (Output セクション) | file | Grep | contains | "## Output" | [x] |
| 6 | kojo-writer.md から ERB Structure 削除 | file | Grep | not_contains | "## ERB Structure" | [x] |
| 7 | kojo-writer.md から ERB template code 削除 | file | Grep | not_contains | "DATALIST" | [x] |
| 8 | kojo-writer.md に SKILL 参照 | file | Grep | contains | "See Skill:" | [x] |
| 9 | kojo-writing SKILL に ERB Template | file | Grep | contains | "CALLF KOJO_MODIFIER_PRE_COMMON" | [x] |
| 10 | kojo-writing SKILL に Procedure | file | Grep | contains | "## Procedure" | [x] |
| 11 | kojo-writing SKILL に Quality Standards | file | Grep | contains | "## Quality" | [x] |
| 12 | Build | build | - | succeeds | - | [x] |
| 13 | Regression | output | --flow | contains | "passed" | [x] |

### AC Details

**AC#1-3** (do.md): `Grep "Expected" .claude/commands/do.md`
**AC#4-8** (kojo-writer.md): `Grep "Expected" .claude/agents/kojo-writer.md`
**AC#9-11** (kojo-writing SKILL): `Grep "Expected" .claude/skills/kojo-writing/SKILL.md`
**AC#12**: `dotnet build`
**AC#13**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow tests/regression/`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | - | 全コマンド (do.md, fl.md 等) の dispatch 形式を調査 | [o] |
| 2 | 1,2 | do.md に Minimal Dispatch Format セクション追加 | [o] |
| 3 | 3 | do.md に FORBIDDEN セクション追加 | [o] |
| 4 | 9-11 | kojo-writing SKILL に移行コンテンツ追加 | [o] |
| 5 | 4-8 | kojo-writer.md を標準テンプレートに変更 (Task#4完了後) | [o] |
| 6 | 12 | ビルド確認 | [o] |
| 7 | 13 | 回帰テスト | [o] |

**依存関係**: Task#5 は Task#4 完了後に実行（SKILL にコンテンツ移行後、kojo-writer.md から削除）

---

## Design

### Role Separation (Core Principle)

| Layer | Responsibility | Content | Example |
|-------|----------------|---------|---------|
| **Commands** | ワークフロー起点 | 最小限の dispatch 指示のみ | `{ID} K{N}` |
| **Subagents** | タスク実行 | Input/Output のみ、具体的仕様なし | 何を読み、何を返すか |
| **Skills** | 専門知識 | 具体的仕様・手順・テンプレート | ERBテンプレート、品質基準 |

```
┌─────────────────────────────────────────────────────────────┐
│ Commands (do.md, fl.md, etc.)                              │
│ ・ワークフローの起点                                         │
│ ・Subagentへの最小限dispatch: "{ID} K{N}"                   │
│ ・具体的仕様は書かない                                       │
└─────────────────────────────────────────────────────────────┘
                              ↓ dispatch
┌─────────────────────────────────────────────────────────────┐
│ Subagents (agent.md)                                        │
│ ・Input: 何を読むか                                          │
│ ・Output: 何を返すか                                         │
│ ・STOP conditions: いつ止まるか                              │
│ ・具体的仕様は一切書かない → "See Skill: xxx"                │
└─────────────────────────────────────────────────────────────┘
                              ↓ reference
┌─────────────────────────────────────────────────────────────┐
│ Skills (SKILL.md)                                           │
│ ・専門的知識のみ                                             │
│ ・具体的仕様: テンプレート、品質基準、手順                    │
│ ・How-to: どうやるか (Procedure セクション含む)              │
└─────────────────────────────────────────────────────────────┘
```

**Principle**: Commands は「誰に何を」、Subagents は「何を読み何を返すか」、Skills は「どうやるか」

---

### 1. do.md 変更

**Note**: 現在の do.md Phase 4 は full prompt template を使用している。このFeatureで以下のセクションを追加し、dispatch を最小化する。cache path の参照は引き続き許可（FORBIDDEN セクション参照）。

#### 追加: Minimal Dispatch Format セクション

```markdown
## Minimal Dispatch Format

**Opus は最小限の情報のみを dispatch する。詳細は Subagent が SKILL から取得する。**

| Type | Format | Notes |
|------|--------|-------|
| kojo | `{ID} K{N}` | COM は feature title から取得 |
| erb | `{ID} Task{N}` | Agent が feature-{ID}.md を読む |
| engine | `{ID} Task{N}` | Agent が feature-{ID}.md を読む |
| infra | `{ID} Task{N}` | Agent が feature-{ID}.md を読む |

**Agent の読み込み順序**:
1. `.claude/agents/{agent}.md` → Input/Output/STOP
2. `Skill({skill})` → How-to details (Procedure 含む)
3. `Game/agents/feature-{ID}.md` → Task requirements
```

#### 追加: FORBIDDEN セクション

```markdown
## FORBIDDEN in Dispatch Prompt

**以下の情報を dispatch prompt に含めてはいけない**:

| NG | 理由 |
|----|------|
| 具体的ファイルパス決定 | SKILL の COM→File Placement が SSOT |
| テンプレート内容 | SKILL の ERB Template が SSOT |
| 品質基準の詳細 | SKILL の Quality Standards が SSOT |
| explorer/eratw-reader の結果内容 | Agent が source files を直接読むべき |
| 具体的手順 | SKILL の Procedure が SSOT |

**OK**: Agent が読むべきファイルへの参照パス (e.g., cache path) は許可。

**Rationale**: Agent reads SKILL, not Opus prompt.
```

---

### 2. kojo-writer.md 変更

#### Before (166行)

```markdown
## ERB Structure
@KOJO_MESSAGE_COM_K{N}_{X}
...（45行のテンプレート）

## Constraints
- UTF-8 with BOM required
- eraTW forbidden: ...

## Quality
| Element | Specification |
...
```

#### After (約40行)

```markdown
---
name: kojo-writer
description: Expert kojo dialogue writer. MUST BE USED for character dialogue creation.
model: opus
tools: Read, Write, Edit, Glob, Grep, Skill
skills: kojo-writing, erb-syntax
---

# Kojo Writer Agent

キャラクター口上を作成する。

## Input

- `Game/agents/feature-{ID}.md`: COM 番号、キャラ指定
- `Game/agents/cache/eratw-COM_{X}.md`: eraTW 参照パターン

## Output

| Status | Content |
|--------|---------|
| OK | `OK:K{N}` + status file |
| BLOCKED | `BLOCKED:{CODE}:K{N}` |

## STOP Conditions

### STOP: Existing Stub Detection
**Trigger**: Target file already contains @KOJO_MESSAGE_COM_K{N}_{X}
**Output**: `BLOCKED:EXISTING_STUB:K{N}`

### STOP: File Mismatch Detection
**Trigger**: Target file doesn't match SKILL COM→File mapping (旧: File Not Found を拡張)
**Output**: `BLOCKED:FILE_MISMATCH:K{N}`

### STOP: Wrong Character Code Detection
**Trigger**: K{N} code in content doesn't match dispatch
**Output**: `BLOCKED:WRONG_CHAR:K{N}`

**See Skill: kojo-writing for all implementation details.**
```

**削減**: 166行 → 約40行 (75%削減)

---

### 3. kojo-writing SKILL 変更

#### 現状 (Current State)

kojo-writing/SKILL.md の現在の構成:
- `## Character Reference` - キャラクター定義
- `## Implementation AC` - 品質基準（≒ Quality として再利用可能）
- ERB template の関数命名パターンのみ（完全なテンプレート構造なし）
- `## Procedure` セクションなし

#### 目標 (Target State)

- `## Procedure` 追加 (新規)
- ERB Template 構造追加 (kojo-writer.md から移行)
- `## Quality` に `Implementation AC` を統合/リネーム
- Constraints 追加 (kojo-writer.md から移行)

#### 追加コンテンツ

kojo-writer.md から移行する内容:

| Section | Content | Source |
|---------|---------|--------|
| **## Procedure** | 口上作成手順 (Step 1-6) | 新規作成 |
| **ERB Template** | `@KOJO_MESSAGE_COM_K{N}_{X}` 構造 (45行) | kojo-writer.md から移行 |
| **## Quality** | 品質基準テーブル | kojo-writer.md から移行 |
| **Constraints** | eraTW forbidden/allowed rules | kojo-writer.md から移行 |

#### Procedure セクション (新規)

```markdown
## Procedure

1. Read `Game/agents/feature-{ID}.md` for COM number and character (K{N})
2. Read `Game/agents/cache/eratw-COM_{X}.md` for eraTW patterns
3. Determine target file using COM→File Placement mapping
4. Check for existing stubs using Grep
5. Write dialogue following ERB Template structure
6. Validate output against Quality Standards
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-29 | Feature completed | finalizer | Updated status [WIP]→[DONE], checked all ACs and Tasks | READY_TO_COMMIT |

---

## Links

- [feature-242.md](feature-242.md) (発生契機)
- [feature-261.md](feature-261.md) (スタブ統合 - 関連Feature)
- [feature-190.md](feature-190.md) (COM→ファイルマッピング)
- [feature-259.md](feature-259.md) (重複検知)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)

---

## Anthropic Official Documentation Reference

### Skills の公式定義

> "Skills teach Claude how to complete specific tasks **in a repeatable way**"
> — [GitHub - anthropics/skills](https://github.com/anthropics/skills)

> "The main body of SKILL.md contains **procedural knowledge**: workflows, best practices, and guidance."
> — [Claude Help Center](https://support.claude.com/en/articles/12512198-how-to-create-custom-skills)

### Procedure セクションの公式例

GitHub Actions Debugging Skill より:
```markdown
## Process

1. Use the `list_workflow_runs` tool to look up recent workflow runs
2. Use the `summarize_job_log_failures` tool to get an AI summary
3. If you need more information, use the `get_job_logs` tool
4. Try to reproduce the failure locally
5. Fix the failing build and verify the fix before committing
```

**結論**: Skills に Procedure/Process セクションを含めることは Anthropic 公式設計に準拠。

---

## Root Cause Analysis: Why 50+ Features Haven't Solved Recurring Problems

### Date: 2025-12-28

### Executive Summary

F200-259 の約50件のFeatureで同様の問題を修正してきたが、根本原因を解決していないため再発し続けている。F260 は Dispatch 最小化 + Subagent 整合で構造的問題を解決し、F261 でデータ問題（スタブ）を解決する。

---

### Pattern 1: Information Fragmentation (SSOT Violations)

```
do.md (ワークフロー)
    ↓ 詳細な Dispatch Template
agent.md (役割 + 実装詳細が混在)
    ↓ 重複した情報
SKILL.md (詳細)
    ↓ 参照されない
Opusのprompt (ハードコード)
    ↓ SSOTを上書き
```

**Evidence**:
- F225: 14件のSSOT違反を発見（S1-S14）
- F203: regression-tester, ac-testerにSkill toolがない → Skill参照不可
- F242 Issue#1: 「Opusがexplorer結果をpromptにハードコード → SSOTと矛盾」

---

### Pattern 2: Cyclic Fix Pattern

F202 Background:
```
口上実装 → エラー発見 → infra Feature立てる → 放置 → 次の口上 → 同じエラー
```

**Stub Problem History**:

| Feature | Problem | Fix | Recurrence |
|---------|---------|-----|:----------:|
| F190 | COM_60 duplicates | Manual delete | ✓ |
| F241 | K2/K4/K9 COM_64 duplicates | Manual delete | ✓ |
| F242 | K2/K4/K9 COM_65 misplacement | Manual move | ? |
| F259 | Duplicate detection tool | Post-hoc detection | No prevention |

**Same problem repeated 3 times** → F261 で根本解決。

---

### Pattern 3: Opus Has Too Much Freedom

do.md Dispatch Prompt Template:
```
## Objective
{specific goal for this invocation}

## Output Format
{expected response structure}

## Tool Guidance
{which tools to use, which to avoid}

## Boundaries
{what is in scope, what is out of scope}
```

This allows **Opus to add arbitrary context that overrides SSOT** → F260 で解決。

---

## Follow-up Features

| Feature | Goal | Scope | Depends |
|---------|------|-------|---------|
| **F261** | 全COMスタブ統合 | ERB files | F260 |
| F262 | 全Agent SSOT準拠 | 残り15 agents | F260 |
| F263 | Pre-write File Validation | kojo-writer STOP 強化 | F260 |

---

## Observation Plan

After F260 completion, observe next 3 kojo features (F243-F245):

| Observation | Success Criteria | Failure Indicator |
|-------------|------------------|-------------------|
| Dispatch content | Minimal `{ID} K{N}` only | Extra context in prompt |
| SKILL reference | kojo-writer reads SKILL | kojo-writer uses hardcoded info |
| FORBIDDEN compliance | No file paths in prompt | File paths in dispatch |

If failures occur → Review F260 implementation.
