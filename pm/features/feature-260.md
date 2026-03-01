# Feature 260: Dispatch 最小化 + Subagent 整吁E

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Subagents は SSOT (Skills/docs) を参照して自律的に正しく実行する。dispatch 側の過剰持E��を排除する、E

### Problem
F242 実裁E��に以下�E問題が発甁E
1. Opus ぁEexplorer 結果めEkojo-writer prompt にハ�EドコーチEↁESSOT と矛盾
2. do.md の Dispatch Prompt Template が�E由度が高すぎ、SSOT 違反を誘発
3. Subagent (agent.md) に実裁E��細が混在し、SKILL との二重管琁E��慁E

### Goal
1. **do.md の dispatch を最小化**: `{ID} K{N}` 形式�Eみ
2. **FORBIDDEN セクション追加**: Opus が追加してはぁE��なぁE�E容を�E訁E
3. **Subagent を標準テンプレート化**: Input/Output/STOP のみ、詳細は SKILL 参�E

### Context
- 発生箁E��: F242 Phase 4
- 関連: F190 (COM→ファイルマッピング), F259 (重褁E��知)
- 公式準拠: Anthropic Skills Documentation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に Minimal Dispatch Format セクション | file | Grep | contains | "## Minimal Dispatch Format" | [x] |
| 2 | do.md kojo dispatch 形式が最小形式と斁E��匁E| file | Grep | contains | "`{ID} K{N}`" | [x] |
| 3 | do.md に FORBIDDEN セクション | file | Grep | contains | "## FORBIDDEN" | [x] |
| 4 | kojo-writer.md が標準テンプレーチE(Input セクション) | file | Grep | contains | "## Input" | [x] |
| 5 | kojo-writer.md が標準テンプレーチE(Output セクション) | file | Grep | contains | "## Output" | [x] |
| 6 | kojo-writer.md から ERB Structure 削除 | file | Grep | not_contains | "## ERB Structure" | [x] |
| 7 | kojo-writer.md から ERB template code 削除 | file | Grep | not_contains | "DATALIST" | [x] |
| 8 | kojo-writer.md に SKILL 参�E | file | Grep | contains | "See Skill:" | [x] |
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
| 1 | - | 全コマンチE(do.md, fl.md 筁E の dispatch 形式を調査 | [o] |
| 2 | 1,2 | do.md に Minimal Dispatch Format セクション追加 | [o] |
| 3 | 3 | do.md に FORBIDDEN セクション追加 | [o] |
| 4 | 9-11 | kojo-writing SKILL に移行コンチE��チE��加 | [o] |
| 5 | 4-8 | kojo-writer.md を標準テンプレートに変更 (Task#4完亁E��E | [o] |
| 6 | 12 | ビルド確誁E| [o] |
| 7 | 13 | 回帰チE��チE| [o] |

**依存関俁E*: Task#5 は Task#4 完亁E��に実行！EKILL にコンチE��チE��行後、kojo-writer.md から削除�E�E

---

## Design

### Role Separation (Core Principle)

| Layer | Responsibility | Content | Example |
|-------|----------------|---------|---------|
| **Commands** | ワークフロー起点 | 最小限の dispatch 持E��のみ | `{ID} K{N}` |
| **Subagents** | タスク実衁E| Input/Output のみ、�E体的仕様なぁE| 何を読み、何を返すぁE|
| **Skills** | 専門知譁E| 具体的仕様�E手頁E�EチE��プレーチE| ERBチE��プレート、品質基溁E|

```
┌─────────────────────────────────────────────────────────────━E
━ECommands (do.md, fl.md, etc.)                              ━E
━E・ワークフローの起点                                         ━E
━E・Subagentへの最小限dispatch: "{ID} K{N}"                   ━E
━E・具体的仕様�E書かなぁE                                      ━E
└─────────────────────────────────────────────────────────────━E
                              ↁEdispatch
┌─────────────────────────────────────────────────────────────━E
━ESubagents (agent.md)                                        ━E
━E・Input: 何を読むぁE                                         ━E
━E・Output: 何を返すぁE                                        ━E
━E・STOP conditions: ぁE��止まるか                              ━E
━E・具体的仕様�E一刁E��かなぁEↁE"See Skill: xxx"                ━E
└─────────────────────────────────────────────────────────────━E
                              ↁEreference
┌─────────────────────────────────────────────────────────────━E
━ESkills (SKILL.md)                                           ━E
━E・専門皁E��識�Eみ                                             ━E
━E・具体的仕槁E チE��プレート、品質基準、手頁E                   ━E
━E・How-to: どぁE��るか (Procedure セクション含む)              ━E
└─────────────────────────────────────────────────────────────━E
```

**Principle**: Commands は「誰に何を」、Subagents は「何を読み何を返すか」、Skills は「どぁE��るか、E

---

### 1. do.md 変更

**Note**: 現在の do.md Phase 4 は full prompt template を使用してぁE��。このFeatureで以下�Eセクションを追加し、dispatch を最小化する。cache path の参�Eは引き続き許可�E�EORBIDDEN セクション参�E�E�、E

#### 追加: Minimal Dispatch Format セクション

```markdown
## Minimal Dispatch Format

**Opus は最小限の惁E��のみめEdispatch する。詳細は Subagent ぁESKILL から取得する、E*

| Type | Format | Notes |
|------|--------|-------|
| kojo | `{ID} K{N}` | COM は feature title から取征E|
| erb | `{ID} Task{N}` | Agent ぁEfeature-{ID}.md を読む |
| engine | `{ID} Task{N}` | Agent ぁEfeature-{ID}.md を読む |
| infra | `{ID} Task{N}` | Agent ぁEfeature-{ID}.md を読む |

**Agent の読み込み頁E��E*:
1. `.claude/agents/{agent}.md` ↁEInput/Output/STOP
2. `Skill({skill})` ↁEHow-to details (Procedure 含む)
3. `pm/features/feature-{ID}.md` ↁETask requirements
```

#### 追加: FORBIDDEN セクション

```markdown
## FORBIDDEN in Dispatch Prompt

**以下�E惁E��めEdispatch prompt に含めてはぁE��なぁE*:

| NG | 琁E�� |
|----|------|
| 具体的ファイルパス決宁E| SKILL の COM→File Placement ぁESSOT |
| チE��プレート�E容 | SKILL の ERB Template ぁESSOT |
| 品質基準�E詳細 | SKILL の Quality Standards ぁESSOT |
| explorer/eratw-reader の結果冁E�� | Agent ぁEsource files を直接読むべぁE|
| 具体的手頁E| SKILL の Procedure ぁESSOT |

**OK**: Agent が読むべきファイルへの参�Eパス (e.g., cache path) は許可、E

**Rationale**: Agent reads SKILL, not Opus prompt.
```

---

### 2. kojo-writer.md 変更

#### Before (166衁E

```markdown
## ERB Structure
@KOJO_MESSAGE_COM_K{N}_{X}
...�E�E5行�EチE��プレート！E

## Constraints
- UTF-8 with BOM required
- eraTW forbidden: ...

## Quality
| Element | Specification |
...
```

#### After (紁E0衁E

```markdown
---
name: kojo-writer
description: Expert kojo dialogue writer. MUST BE USED for character dialogue creation.
model: opus
tools: Read, Write, Edit, Glob, Grep, Skill
skills: kojo-writing, erb-syntax
---

# Kojo Writer Agent

キャラクター口上を作�Eする、E

## Input

- `pm/features/feature-{ID}.md`: COM 番号、キャラ持E��E
- `pm/cache/eratw-COM_{X}.md`: eraTW 参�Eパターン

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

**削渁E*: 166衁EↁE紁E0衁E(75%削渁E

---

### 3. kojo-writing SKILL 変更

#### 現状 (Current State)

kojo-writing/SKILL.md の現在の構�E:
- `## Character Reference` - キャラクター定義
- `## Implementation AC` - 品質基準（≒ Quality として再利用可能�E�E
- ERB template の関数命名パターンのみ�E�完�EなチE��プレート構造なし！E
- `## Procedure` セクションなぁE

#### 目樁E(Target State)

- `## Procedure` 追加 (新要E
- ERB Template 構造追加 (kojo-writer.md から移衁E
- `## Quality` に `Implementation AC` を統吁Eリネ�Eム
- Constraints 追加 (kojo-writer.md から移衁E

#### 追加コンチE��チE

kojo-writer.md から移行する�E容:

| Section | Content | Source |
|---------|---------|--------|
| **## Procedure** | 口上作�E手頁E(Step 1-6) | 新規作�E |
| **ERB Template** | `@KOJO_MESSAGE_COM_K{N}_{X}` 構造 (45衁E | kojo-writer.md から移衁E|
| **## Quality** | 品質基準テーブル | kojo-writer.md から移衁E|
| **Constraints** | eraTW forbidden/allowed rules | kojo-writer.md から移衁E|

#### Procedure セクション (新要E

```markdown
## Procedure

1. Read `pm/features/feature-{ID}.md` for COM number and character (K{N})
2. Read `pm/cache/eratw-COM_{X}.md` for eraTW patterns
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

- [feature-242.md](feature-242.md) (発生契橁E
- [feature-261.md](feature-261.md) (スタブ統吁E- 関連Feature)
- [feature-190.md](feature-190.md) (COM→ファイルマッピング)
- [feature-259.md](feature-259.md) (重褁E��知)
- [kojo-writing SKILL](../../../archive/claude_legacy_20251230/skills/kojo-writing/SKILL.md)

---

## Anthropic Official Documentation Reference

### Skills の公式定義

> "Skills teach Claude how to complete specific tasks **in a repeatable way**"
>  E[GitHub - anthropics/skills](https://github.com/anthropics/skills)

> "The main body of SKILL.md contains **procedural knowledge**: workflows, best practices, and guidance."
>  E[Claude Help Center](https://support.claude.com/en/articles/12512198-how-to-create-custom-skills)

### Procedure セクションの公式侁E

GitHub Actions Debugging Skill より:
```markdown
## Process

1. Use the `list_workflow_runs` tool to look up recent workflow runs
2. Use the `summarize_job_log_failures` tool to get an AI summary
3. If you need more information, use the `get_job_logs` tool
4. Try to reproduce the failure locally
5. Fix the failing build and verify the fix before committing
```

**結諁E*: Skills に Procedure/Process セクションを含めることは Anthropic 公式設計に準拠、E

---

## Root Cause Analysis: Why 50+ Features Haven't Solved Recurring Problems

### Date: 2025-12-28

### Executive Summary

F200-259 の紁E0件のFeatureで同様�E問題を修正してきたが、根本原因を解決してぁE��ぁE��め�E発し続けてぁE��、E260 は Dispatch 最小化 + Subagent 整合で構造皁E��題を解決し、F261 でチE�Eタ問題（スタブ）を解決する、E

---

### Pattern 1: Information Fragmentation (SSOT Violations)

```
do.md (ワークフロー)
    ↁE詳細な Dispatch Template
agent.md (役割 + 実裁E��細が混在)
    ↁE重褁E��た情報
SKILL.md (詳細)
    ↁE参�EされなぁE
Opusのprompt (ハ�EドコーチE
    ↁESSOTを上書ぁE
```

**Evidence**:
- F225: 14件のSSOT違反を発見！E1-S14�E�E
- F203: regression-tester, ac-testerにSkill toolがなぁEↁESkill参�E不可
- F242 Issue#1: 「Opusがexplorer結果をpromptにハ�EドコーチEↁESSOTと矛盾、E

---

### Pattern 2: Cyclic Fix Pattern

F202 Background:
```
口上実裁EↁEエラー発要EↁEinfra Feature立てめEↁE放置 ↁE次の口丁EↁE同じエラー
```

**Stub Problem History**:

| Feature | Problem | Fix | Recurrence |
|---------|---------|-----|:----------:|
| F190 | COM_60 duplicates | Manual delete | ✁E|
| F241 | K2/K4/K9 COM_64 duplicates | Manual delete | ✁E|
| F242 | K2/K4/K9 COM_65 misplacement | Manual move | ? |
| F259 | Duplicate detection tool | Post-hoc detection | No prevention |

**Same problem repeated 3 times** ↁEF261 で根本解決、E

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

This allows **Opus to add arbitrary context that overrides SSOT** ↁEF260 で解決、E

---

## Follow-up Features

| Feature | Goal | Scope | Depends |
|---------|------|-------|---------|
| **F261** | 全COMスタブ統吁E| ERB files | F260 |
| F262 | 全Agent SSOT準拠 | 残り15 agents | F260 |
| F263 | Pre-write File Validation | kojo-writer STOP 強匁E| F260 |

---

## Observation Plan

After F260 completion, observe next 3 kojo features (F243-F245):

| Observation | Success Criteria | Failure Indicator |
|-------------|------------------|-------------------|
| Dispatch content | Minimal `{ID} K{N}` only | Extra context in prompt |
| SKILL reference | kojo-writer reads SKILL | kojo-writer uses hardcoded info |
| FORBIDDEN compliance | No file paths in prompt | File paths in dispatch |

If failures occur ↁEReview F260 implementation.
