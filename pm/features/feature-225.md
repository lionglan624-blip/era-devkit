# Feature 225: SSOT Consolidation

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`SSOT`, `testing SKILL`, `do.md`, `consolidation`, `documentation`, `Design Principles`

---

## Background

### Philosophy (Mid-term Vision)

era プロジェクトにおいて、�Eての技術情報は単一の真実�E惁E��溁E(SSOT: Single Source of Truth) に一允E��される。重褁E��報の排除により、サブエージェントが正しい惁E��のみを参照し、ドキュメント間の矛盾による誤動作を防ぐ、E*Skills are the Single Source of Truth for commands. Agents MUST reference Skills, not hardcode commands.**

### Problem (Current Issue)

F200-220 の実裁E��程で、テストコマンド、型ルーチE��ング、ログ形式、ファイルパスなどの技術情報が褁E��のドキュメンチE(do.md, testing SKILL, 吁Eagent.md) に刁E��・重褁E��て記載されてぁE��。これにより以下�E問題が発甁E

| ID | Topic | 競合するソース | 正しい SSOT |
|:--:|-------|--------------|------------|
| S1 | Test commands | do.md, testing SKILL, ac-tester, regression-tester | testing SKILL.md |
| S2 | Type routing | do.md, implementer.md | do.md |
| S3 | Pre-commit scope | do.md, F212, F218 | F218 (context-aware) |
| S4 | Log format | do.md, ac-tester, verify-logs.py | testing SKILL.md |
| S5 | eraTW path | eratw-reader.md (hardcoded) | CLAUDE.md or settings |
| S7 | pre-commit 思想 | SKILL (全検証) vs pre-commit (regression のみ) | testing SKILL.md |
| S8 | F202 思想未反映 | F202 で定義した Phase 7 ループが do.md に未反映 | do.md |
| S9 | IMPLE_FEATURE_ID作�E | do.md L201-204 (bashコマンド直書ぁE | testing SKILL |
| S10 | チE��ト�E力パス | do.md L235, L312 | testing SKILL |
| S11 | engine AC詳細 | do.md L242-248 | testing SKILL |
| S12 | kojo_test_gen.pyコマンチE| do.md L302-304 | testing SKILL |
| S13 | Design Principles | CLAUDE.md (暗黙的) vs 未斁E��匁E| CLAUDE.md (Design Principles section) |
| S14 | file Type Method | 既孁EFeature で不統一 (`-`, `exists`, 未記輁E | testing SKILL.md (明文匁E |

(S6 is resolved by F221)

### Goal (What to Achieve)

全ての SSOT 違反を解消し、情報の一允E��を達成すめE

1. **S1, S4, S9-S12**: testing SKILL.md めESSOT とし、do.md/agents から重褁E��載を削除
2. **S2**: do.md の Type routing めESSOT とし、implementer.md から重褁E��除
3. **S3**: F218 の context-aware pre-commit 思想めESSOT とし、do.md/F212 の古ぁE��載削除
4. **S5**: eraTW path めECLAUDE.md で設定可能にし、eratw-reader.md のハ�Eドコード削除
5. **S7**: testing SKILL.md の pre-commit 思想を�E確匁E
6. **S8**: F202 Phase 7 ループ思想めEdo.md に反映
7. **S13**: Design Principles セクションめECLAUDE.md に追加
8. **S14**: file Type AC の Method 凡例を testing SKILL.md に追加

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Test commands SSOT | file | Grep | contains | "Skill(testing)" | [x] |
| 2 | Type routing SSOT | file | Grep | not_contains | "Type:" | [x] |
| 3 | Pre-commit context-aware doc | file | Grep | contains | "F218" | [x] |
| 4 | eraTW path configurable | file | Grep | contains | "ERATW_PATH" | [x] |
| 5 | Pre-commit philosophy | file | Grep | contains | "regression" | [x] |
| 6 | F202 Phase 7 loop | file | Grep | contains | "F202" | [x] |
| 7 | Design Principles section | file | Grep | contains | "## Design Principles" | [x] |
| 8 | file Type Method凡侁E| file | Grep | contains | "exists.*Glob" | [x] |
| 9 | do.md no hardcoded commands | file | Grep | not_contains | "dotnet run --project" | [x] |
| 10 | ac-tester references SKILL | file | Grep | contains | "Skill(testing)" | [x] |
| 11 | regression-tester references SKILL | file | Grep | contains | "Skill(testing)" | [x] |
| 12 | Build succeeds | build | - | succeeds | - | [x] |

### AC Details

**AC1**: `do.md` should reference `Skill(testing)` instead of hardcoding test commands
**Test**: `Grep("Skill\(testing\)", "c:\Era\era紁E��館protoNTR\.claude\commands\do.md")`
**Expected**: At least 1 match

**AC2**: `implementer.md` should not duplicate Type routing information (SSOT: do.md)
**Test**: `Grep("Type:", "c:\Era\era紁E��館protoNTR\.claude\agents\implementer.md")`
**Expected**: 0 matches (only in do.md)

**AC3**: Pre-commit scope documentation references F218 context-aware approach
**Test**: `Grep("F218", "c:\Era\era紁E��館protoNTR\.claude\commands\do.md")`
**Expected**: At least 1 match

**AC4**: eraTW path is configurable via environment variable or CLAUDE.md
**Test**: `Grep("ERATW_PATH", "c:\Era\era紁E��館protoNTR\.claude\agents\eratw-reader.md")`
**Expected**: At least 1 match

**AC5**: Pre-commit philosophy clarified in testing SKILL.md
**Test**: `Grep("regression", "c:\Era\era紁E��館protoNTR\.claude\skills\testing\SKILL.md")`
**Expected**: Contains clarification of pre-commit scope (regression tests)

**AC6**: F202 Phase 7 loop philosophy reflected in do.md
**Test**: `Grep("F202", "c:\Era\era紁E��館protoNTR\.claude\commands\do.md")`
**Expected**: At least 1 match

**AC7**: Design Principles section exists in CLAUDE.md
**Test**: `Grep("## Design Principles", "c:\Era\era紁E��館protoNTR\CLAUDE.md")`
**Expected**: At least 1 match

**AC8**: file Type Method legend added to testing SKILL.md
**Test**: `Grep("exists.*Glob", "c:\Era\era紁E��館protoNTR\.claude\skills\testing\SKILL.md")`
**Expected**: At least 1 match explaining Method column usage (see Notes S14 for proposed content)

**AC9**: do.md does not hardcode engine test commands (references SKILL instead)
**Test**: `Grep("dotnet run --project", "c:\Era\era紁E��館protoNTR\.claude\commands\do.md")`
**Expected**: 0 matches (should reference Skill(testing))

**AC10**: ac-tester.md references testing SKILL
**Test**: `Grep("Skill\(testing\)", "c:\Era\era紁E��館protoNTR\.claude\agents\ac-tester.md")`
**Expected**: At least 1 match

**AC11**: regression-tester.md references testing SKILL
**Test**: `Grep("Skill\(testing\)", "c:\Era\era紁E��館protoNTR\.claude\agents\regression-tester.md")`
**Expected**: At least 1 match

**AC12**: Build succeeds after documentation changes
**Test**: `dotnet build engine/uEmuera.Headless.csproj`
**Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,9 | Remove hardcoded test commands from do.md, add Skill(testing) references | [x] |
| 2 | 2 | Remove Type routing duplication from implementer.md | [x] |
| 3 | 3,6 | Document F218 context-aware pre-commit and F202 Phase 7 loop in do.md | [x] |
| 4 | 4 | Make eraTW path configurable in CLAUDE.md and eratw-reader.md | [x] |
| 5 | 5 | Clarify pre-commit philosophy (regression only) in testing SKILL.md | [x] |
| 6 | 7 | Add Design Principles section to CLAUDE.md | [x] |
| 7 | 8 | Add file Type Method legend to testing SKILL.md (see Notes S14) | [x] |
| 8 | 10,11 | Add Skill(testing) references to ac-tester.md and regression-tester.md | [x] |
| 9 | 12 | Verify build succeeds | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2025-12-27**: FL review completed. AC#1,2,5,9,10,11 were pre-satisfied (SSOT consolidation already done in prior features). Remaining AC#3,4,6,7,8 are optional enhancements. User decision: mark satisfied ACs as [x], keep Feature for future work on remaining ACs.
- **2025-12-27**: FL iter3 - AC#8 Expected "exists.*Glob" は実裁E���Eパターン。testing SKILL.md に Method legend を追加後に検証可能、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | init | - | Feature created from F223 Task 3 | - |
| 2025-12-27 | fl | feature-reviewer | Review + Validate | 6/12 AC pre-satisfied |
| 2025-12-27 | decision | user | Mark satisfied ACs, keep for remaining | AC#1,2,5,9,10,11 ↁE[x] |
| 2025-12-27 | fl | fl-loop | Task status sync | Task 1,2,5,8,9 ↁE[x] |
| 2025-12-27 | fl | fl-loop | Auto-fix iter2 | Type:code→file, AC#8 regex fix, Task#9 reset |
| 2025-12-27 | fl | user | Pending decisions | AC:Task 1:1→維持E AC#4→維持E Type→file維持E|
| 2025-12-27 | fl | fl-loop | Auto-fix iter4 | AC#8 Table/Details pattern sync |
| 2025-12-27 | fl | user | Pattern decision | AC#8: exists.*Glob (simpler) |
| 2025-12-27 | fl | fl-loop | Auto-fix iter5 | AC#8 pattern, Task#7 S14 ref |
| 2025-12-27 | fl | fl-loop | Complete | 6 iterations, 0 issues |
| 2025-12-27 | do | implementer | Task 3,4,6,7 | SUCCESS |
| 2025-12-27 | do | - | Build verify | SUCCESS |
| 2025-12-27 | finalize | finalizer | Status [WIP]→[DONE], 12/12 AC [x], 9/9 Task [x] | READY_TO_COMMIT |

---

## Dependencies

None

---

## Links

- [F223](feature-223.md) - /do Workflow Comprehensive Audit (parent feature)
- [F221](feature-221.md) - COM file placement consolidation (S6 resolved)
- [F218](feature-218.md) - Context-aware pre-commit scope
- [F212](feature-212.md) - Pre-commit hook initial implementation
- [F202](feature-202.md) - Phase 7 loop philosophy

---

## Notes

### S13: Design Principles Section Proposal

Proposed content for CLAUDE.md Design Principles section:

```markdown
## Design Principles

### Core Principles

| Principle | Description | Implementation |
|-----------|-------------|----------------|
| **SSOT** | 惁E��は1箁E��のみ。SKILLが詳細のtruth | Skills > CLAUDE.md > commands > agents |
| **TDD** | RED→GREEN、テスト�E衁E| do.md Phase 3、Hook保護 |
| **STOP on Ambiguity** | 不�E確なら独自判断せずSTOP | CLAUDE.md Escalation Policy |
| **Separation of Concerns** | Opus=判断、Subagent=実裁E| CLAUDE.md Subagent Strategy |
| **Fail Fast** | 3回失敗でSTOP | do.md Error Handling |
| **Immutable Tests** | AC/RegressionチE��ト編雁E��止 | Hook (pre-ac-write.ps1) |
| **Minimal Context** | OK時�E簡潔、ERR時�Eみ詳細 | Agent output format |
| **AC:Task 1:1** | 1 AC = 1 Test = 1 Task = 1 Dispatch | ac-task-aligner.md |
| **Binary Judgment** | PASS/FAILのみ、曖昧な判定なぁE| testing SKILL |
| **Progressive Disclosure** | 惁E��は段階的に読み込む�E�Enthropic推奨�E�E| skills: YAML frontmatter |
```

### S14: file Type Method Legend

Proposed addition to testing SKILL.md AC Definition Format section:

```markdown
### Method Column Usage (file/code Type)

| AC Type | Matcher | Recommended Method | Example |
|---------|---------|-------------------|---------|
| file | exists/not_exists | Glob | `Glob("pm/features/feature-*.md")` |
| file | contains/matches | Grep | `Grep("## Status", "feature-226.md")` |
| code | contains/matches | Grep | `Grep("Skill\(testing\)", "do.md")` |
| code | not_contains | Grep | `Grep("hardcoded", "do.md")` (expect 0) |

**Note**:
- `exists`/`not_exists` Matcher ↁEMethod = `Glob` (file path pattern)
- `contains`/`matches` Matcher ↁEMethod = `Grep` (content search)
- testing SKILL の「Verification Method: Glob/Grep」�E Type 全体�E説明。個別 AC では Matcher に応じて使ぁE�Eける、E
```

### Implementation Guidance

**Phase 1: Audit** (prerequisite)
- Read all conflicting sources (do.md, testing SKILL.md, agent files)
- Document current state and conflicts

**Phase 2: Update SSOT** (testing SKILL.md, CLAUDE.md)
- Add Design Principles section to CLAUDE.md (S13)
- Add file Type Method legend to testing SKILL.md (S14)
- Clarify pre-commit philosophy in testing SKILL.md (S7)
- Add pre-commit scope section to testing SKILL.md (S9-S12 details)

**Phase 3: Update References** (do.md, agents)
- Replace hardcoded commands with Skill(testing) references in do.md (S1, S9-S12)
- Add F218/F202 references to do.md (S3, S8)
- Update eratw-reader.md to use configurable path (S5)
- Update agent files to reference SKILL (S1, S4)
- Remove duplicate Type routing from implementer.md (S2)

**Phase 4: Verify**
- Run all AC tests
- Verify build succeeds
- Check that no hardcoded commands remain

### eraTW Path Configuration (S5)

Proposed CLAUDE.md addition:

```markdown
## External Dependencies

### eraTW Reference Repository

**Purpose**: Source code reference for kojo dialogue implementation

**Path Configuration**:
1. Environment variable: `ERATW_PATH` (highest priority)
2. CLAUDE.md default: `c:\Era\eraTW`
3. eratw-reader.md fallback: hardcoded path (deprecated)

**Usage**: eratw-reader agent uses this path to extract COM implementations from eraTW source.
```

Corresponding eratw-reader.md update:

```markdown
## eraTW Path

**CRITICAL**: Use configurable path, NOT hardcoded.

```erb
# Priority order:
1. Environment variable: ERATW_PATH
2. CLAUDE.md: External Dependencies > eraTW Reference Repository
3. Fallback: c:\Era\eraTW (deprecated)
```

**Verification**: Before reading, verify path exists. If not found, STOP and report.
```
