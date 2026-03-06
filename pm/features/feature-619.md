# Feature 619: Feature Creation Workflow with [DRAFT] Status and /fc Command

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **TRACK** - Choose one concrete destination:
>    - Option A: Create new Feature → Add Task to create F{ID}
>    - Option B: Add to existing Feature → Add Task to update F{ID}#T{N}
>    - Option C: Add to architecture.md Phase → Verify Phase exists
> 3. **HANDOFF** - Record in this feature's Handoff section
> 4. **CONTINUE** - Resume this feature's scope
>
> **TBD is FORBIDDEN**. Every discovered issue must have actionable handoff.

## Type: infra

## Background

### Philosophy (思想・上位目標)
Feature 作成はコンテキスト分離と検証可能性を優先すべき。現在の `context:fork` 内での Task() 呼び出しは「動作するが検証不能」というブラックボックス状態を生む。Main コンテキストから直接 Task() を呼ぶことで、途中経過の確認・ログが可能になる。（注：初期実装では resume 機能なし、将来実装）

### Problem (現状の問題)
1. **検証不能**: `context:fork` 内の Task() は agent_id が Main に返らず、resume 不可、ログなし
2. **公式パターン外**: Anthropic 公式ドキュメントに Skills 内 Task() の例なし
3. **TBD 違反リスク**: Handoff で Feature 作成時、即座にファイル作成しないと TBD 違反
4. **セッション分離不可**: 長時間の Feature 生成を別セッションに分離できない

### Goal (このFeatureで達成すること)
1. **[DRAFT] ステータス導入**: Background のみ、AC/Tasks 未生成の状態を表す
2. **/fc コマンド作成**: Main から Task×5 で AC/Tasks を生成（検証可能）
3. **run-workflow 更新**: PHASE-4/8 で Skill(feature-creator) → DRAFT 作成 + /fc 案内
4. **TBD Protocol 更新**: DRAFT file exists で TBD 違反回避

### Session Context
- **Original problem**: Skills (context:fork) 内の Task() が検証不能
- **Considered alternatives**:
  - ❌ 現状維持 - 検証不能のまま
  - ❌ [STUB] ステータス - 技術用語、ユーザーに伝わりにくい
  - ✅ [DRAFT] ステータス - 「下書き→提案→レビュー済み」は自然な流れ
- **Key decisions**:
  - DRAFT 作成 = TBD 違反回避
  - /fc は別セッションで実行（コンテキスト分離）

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | /fc command created | file | Glob | exists | ".claude/commands/fc.md" | [x] |
| 2 | fc.md contains 5-phase Task dispatch | file | Grep | contains | "tech-investigator" | [x] |
| 3a | fc.md reads DRAFT status | file | Grep | contains | "\\[DRAFT\\]" | [x] |
| 3b | fc.md generates AC table | file | Grep | contains | "## Acceptance Criteria" | [x] |
| 3c | fc.md generates Tasks table | file | Grep | contains | "## Tasks" | [x] |
| 4 | index-features.md has [DRAFT] in Status Legend | file | Grep(Game/agents/index-features.md) | contains | "\\[DRAFT\\].*Background のみ" | [x] |
| 5 | CLAUDE.md Deferred Task Protocol updated | file | Grep(CLAUDE.md) | contains | "DRAFT file exists" | [x] |
| 6 | PHASE-4.md updated for DRAFT creation | file | Grep(.claude/skills/run-workflow/PHASE-4.md) | contains | "index-features.*DRAFT" | [x] |
| 7 | PHASE-8.md updated for DRAFT creation | file | Grep(.claude/skills/run-workflow/PHASE-8.md) | contains | "DRAFT.*index-features" | [x] |
| 8 | PHASE-4.md remove Skill(feature-creator) | file | Grep(.claude/skills/run-workflow/PHASE-4.md) | not_contains | "Skill(feature-creator)" | [x] |
| 9 | PHASE-8.md remove Skill(feature-creator) | file | Grep(.claude/skills/run-workflow/PHASE-8.md) | not_contains | "Skill(feature-creator)" | [x] |
| 10 | feature-creator skill deprecated | file | Grep(.claude/skills/feature-creator/SKILL.md) | contains | "DEPRECATED.*Use /fc" | [x] |
| 11 | CLAUDE.md Slash Commands table updated | file | Grep(CLAUDE.md) | contains | "\\| `/fc`" | [x] |
| 12 | CLAUDE.md Skills table updated | file | Grep(CLAUDE.md) | contains | "feature-creator.*DEPRECATED" | [x] |
| 13 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 14 | F620 feature file created | file | Glob(Game/agents/feature-620.md) | exists | "Game/agents/feature-620.md" | [x] |
| 15 | feature-template.md updated | file | Grep(Game/agents/reference/feature-template.md) | not_contains | "Skill(feature-creator)" | [x] |

### AC Details

**AC#1-3c**: /fc command structure
- AC#1: /fc command file 作成
- AC#2: 5-phase Task dispatch 実装
- AC#3a-c: DRAFT status 読み取り、AC/Tasks 生成検証

**AC#4**: Status Legend 更新
- [DRAFT] = Background のみ、AC/Tasks 未生成
- [PROPOSED] = AC/Tasks あり、FL 待ち
- 遷移: [DRAFT] → /fc → [PROPOSED]

**AC#5**: TBD Protocol 更新
- Option A Validation: "DRAFT file exists OR Creation Task exists"
- DRAFT 作成 = TBD 違反回避

**AC#6-9**: run-workflow 更新
- AC#6: PHASE-4 DRAFT 作成 workflow 追加
- AC#7: PHASE-8 DRAFT 作成 workflow 追加
- AC#8: PHASE-4 Skill(feature-creator) 参照削除
- AC#9: PHASE-8 Skill(feature-creator) 参照削除

**AC#10**: feature-creator skill deprecated
- DEPRECATED 注記追加
- /fc command への誘導

**AC#11**: CLAUDE.md Slash Commands 更新
- `/fc` コマンドを Slash Commands 表に追加

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-3c | Create /fc command with Main context Task dispatch | [x] |
| 2 | 4 | Update index-features.md Status Legend with [DRAFT] | [x] |
| 3 | 5 | Update CLAUDE.md Deferred Task Protocol | [x] |
| 4 | 6,8 | Update PHASE-4.md for DRAFT creation workflow | [x] |
| 5 | 7,9 | Update PHASE-8.md for DRAFT creation workflow | [x] |
| 6 | 10 | Deprecate feature-creator skill | [x] |
| 7 | 11 | Update CLAUDE.md Slash Commands table | [x] |
| 8 | 12 | Update CLAUDE.md Skills table - mark feature-creator as DEPRECATED | [x] |
| 9 | 13 | Verify build succeeds | [x] |
| 10 | 14 | Create F620 - /fc Resume Capability | [x] |
| 11 | 15 | Update feature-template.md Agent Selection | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Step 1: Create /fc Command (Task 1)

Create `.claude/commands/fc.md`:

```markdown
# /fc Command - Feature Completion

Complete a [DRAFT] feature by generating AC/Tasks via Task dispatch.

## Usage
/fc {ID}

## Precondition
- feature-{ID}.md exists with [DRAFT] status
- Background section (Philosophy, Problem, Goal) is complete

## Procedure
1. Read feature-{ID}.md, verify [DRAFT] status
2. Task(tech-investigator) → Dependencies, Impact Analysis
3. Task(ac-designer) → AC table
4. Task(wbs-generator) → Tasks table
5. Task(feature-validator) → Validation
6. Update status [DRAFT] → [PROPOSED]
7. Report completion
```

### Step 2: Update index-features.md (Task 2)

Add to Status Legend:
```markdown
| `[DRAFT]` | Background のみ、AC/Tasks 未生成 | オーケストレーターがスタブ作成 |
```

Update Status Transition Rules:
```markdown
| [DRAFT] | [PROPOSED] | `/fc` 完了 | AC/Tasks 生成済み |
```

### Step 3: Update CLAUDE.md (Task 3)

Update Deferred Task Protocol Option A:
```markdown
| A | `F{ID}` (new) | **DRAFT file exists** OR Creation Task exists in Tasks table |
```

### Step 4: Update PHASE-4.md (Task 4)

Replace:
```markdown
For research creating sub-features, use Skill(feature-creator) instead.
```

With:
```markdown
For research creating sub-features:
1. Orchestrator creates DRAFT: feature-{ID}.md with Background only
2. Register in index-features.md as [DRAFT]
3. Record in Handoff: "F{ID} [DRAFT] - 別セッションで /fc {ID} を実行"
4. Continue current feature (do not wait)
```

### Step 5: Update PHASE-8.md (Task 5)

Replace:
```markdown
- A: Create Feature now → use `Skill(feature-creator)`
```

With:
```markdown
- A: Create DRAFT now → Orchestrator creates feature-{ID}.md [DRAFT], registers in index-features.md
  - Record: "F{ID} [DRAFT] - 別セッションで /fc {ID} を実行"
```

### Step 6: Deprecate feature-creator (Task 6)

Add to top of `.claude/skills/feature-creator/SKILL.md`:
```markdown
> **DEPRECATED**: This skill is deprecated. Use `/fc` command instead.
> See F619 for migration details.
```

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F610 | [DONE] | Feature Creator 5-Phase Orchestrator Redesign |

## Impact Analysis

| File/Component | Change Type | Description |
|----------------|-------------|-------------|
| `.claude/commands/fc.md` | Create | New /fc command |
| `Game/agents/index-features.md` | Update | Add [DRAFT] to Status Legend |
| `CLAUDE.md` | Update | Deferred Task Protocol Option A |
| `.claude/skills/run-workflow/PHASE-4.md` | Update | DRAFT creation workflow |
| `.claude/skills/run-workflow/PHASE-8.md` | Update | DRAFT creation workflow |
| `.claude/skills/feature-creator/SKILL.md` | Update | DEPRECATED notice |
| `Game/agents/reference/feature-template.md` | Update | Remove Skill(feature-creator) reference |

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Resume capability for /fc command | Initial implementation scope limited | Feature | F620 | Task#10 |

## Review Notes
- [resolved-applied] Phase1-Uncertain iter3-7: AC#11 backtick pattern validated - works correctly
- [resolved-applied] Phase1-Uncertain iter4: CLAUDE.md Skills table update - added AC#12
- [resolved-applied] Phase1-Uncertain iter6: AC#2 philosophy-definer mismatch - fixed to tech-investigator
- [resolved-applied] Phase1-Uncertain iter7: Task 10 AC:Task 1:1 conflict - will add AC#14

## Links
- [index-features.md](index-features.md)
- [F610: Feature Creator 5-Phase Orchestrator Redesign](feature-610.md)

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-25 10:38 | START | implementer | Task 4 | - |
| 2026-01-25 10:38 | END | implementer | Task 4 | SUCCESS |
| 2026-01-25 10:40 | START | implementer | Task 5 | - |
| 2026-01-25 10:40 | END | implementer | Task 5 | SUCCESS |
| 2026-01-25 10:43 | START | implementer | Task 8 | - |
| 2026-01-25 10:43 | END | implementer | Task 8 | SUCCESS |
| 2026-01-25 10:47 | START | implementer | Task 11 | - |
| 2026-01-25 10:47 | END | implementer | Task 11 | SUCCESS |
| 2026-01-25 11:01 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: additional files reference deprecated Skill(feature-creator) |
