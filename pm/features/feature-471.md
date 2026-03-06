# Feature 471: Phase 14 Planning

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: research

## Created: 2026-01-12

---

## Summary

**Feature を立てる Feature**: Phase 14 Planning

Create sub-features for Phase 14 Era.Core Engine (F474-F486):
- Implementation sub-features from Phase 14 tasks (GameEngine, StateManager, KojoEngine, etc.)
- F484: SCOMF prerequisite checks (IsScenarioAvailable TALENT/ABL/FLAG conditions, deferred from F473)
- F485: Post-Phase Review Phase 14 (type: infra)
- F486: Phase 15 Planning (type: research)

**Note**: F472 (Character Aggregate Extended Value Objects) and F473 (SCOMF Full Implementation) were created during Phase 13 execution, so Phase 14 starts at F474.

**Note**: Actual allocation adjusted from original expectation (F486-F488) to minimize ID gaps (F484-F486).

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 14 implements core engine components (GameEngine, StateManager, KojoEngine, CommandProcessor, NtrEngine, HeadlessUI) plus process state machine and character setup migrations. Requires careful decomposition into manageable features following granularity rules (8-15 ACs for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points.

### Problem (Current Issue)

Phase 13 completion requires Phase 14 planning to maintain momentum:
- Phase 14 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Era.Core Engine is the integration phase (core game loop + headless execution)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 14** requirements from full-csharp-architecture.md
2. **Decompose Era.Core Engine** into manageable sub-features
3. **Create implementation sub-features** from Phase 14 tasks
4. **Create transition features** (Post-Phase Review + Phase 15 Planning)
5. **Update index-features.md** with Phase 14 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 14 analysis documented | file | Grep | contains | "Phase 14 Feature Mapping:" | [x] |
| 2 | Engine component categorization complete | file | Grep | contains | "Phase 14 Engine Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "GameEngine\\|StateManager\\|KojoEngine\\|CommandProcessor\\|NtrEngine" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 14" | [x] |
| 5 | Phase 15 Planning in index | file | Grep | contains | "Phase 15 Planning" | [x] |
| 6 | index-features.md updated | file | Grep | contains | "\\| 47[4-9] \\||\\| 48[0-9] \\|" | [x] |
| 7 | Implementation sub-feature has Philosophy | file | Grep | contains | "Phase 14: Era\\.Core Engine" | [x] |
| 8 | Minimum sub-feature coverage (10+) | file | Grep | count_gte | 10 | [x] |
| 9 | Next Feature number updated | file | Grep | matches | "Next Feature number: 4[89][0-9]" | [x] |

### AC Details

**AC#1**: Phase 14 analysis documented in feature-471.md Execution Log
- Test: Grep pattern="Phase 14 Feature Mapping:" in feature-471.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: Engine component categorization documented
- Test: Grep pattern="Phase 14 Engine Analysis:" in feature-471.md Execution Log
- Must contain explicit Feature ID allocation table (e.g., "F474: GameEngine + Main Loop")
- Shows how 13 architecture.md tasks grouped into 10+ implementation features
- Documents decomposition rationale (engine component responsibility, granularity compliance)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="GameEngine|StateManager|KojoEngine|CommandProcessor|NtrEngine" path="Game/agents/index-features.md"
- Verifies at least one Phase 14 engine component is registered in index

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 14" in index-features.md
- Type: infra, follows F470 pattern

**AC#5**: Phase 15 Planning in index
- Test: Grep pattern="Phase 15 Planning" in index-features.md
- Type: research, follows F471 pattern

**AC#6**: index-features.md updated with Phase 14 features
- Test: Grep pattern="\\| 47[4-9] \\||\\| 48[0-9] \\|" in index-features.md
- Verifies at least one sub-feature ID in 474-489 range registered
- Pattern covers expected 10-15 sub-features (starting from F474)

**AC#7**: Implementation sub-feature has Philosophy
- Test: Grep pattern="Phase 14: Era.Core Engine" path="Game/agents/feature-47[4-9].md" (or feature-48*.md)
- Verifies Philosophy section contains "Phase 14: Era.Core Engine" in created sub-feature files
- Per architecture.md Sub-Feature Requirements

**AC#8**: Minimum sub-feature coverage (10+)
- Test: Grep pattern="\\| 47[4-9] \\||\\| 48[0-9] \\|" path="Game/agents/index-features.md", count >= 10
- Verifies at least 10 Phase 14 sub-features created (F474+)
- Note: Phase 14 has 13 tasks, expecting 10-15 sub-features based on grouping

**AC#9**: Next Feature number updated
- Test: Grep pattern="Next Feature number: 4[89][0-9]" path="Game/agents/index-features.md"
- Verifies Next Feature number incremented to 480-499 range after sub-feature creation

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 14 requirements and document "Phase 14 Feature Mapping:" | [x] |
| 2 | 2 | Document engine component categorization as "Phase 14 Engine Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features per Analysis Method step 2 (8-15 ACs per feature, grouped by engine component responsibility, with 負債解消 tasks, 負債ゼロ ACs, and 等価性検証 per Sub-Feature Requirements #4-6) | [x] |
| 4 | 4 | Create Phase 14 Post-Phase Review feature (type: infra) | [x] |
| 5 | 5 | Create Phase 15 Planning feature (type: research) | [x] |
| 6 | 6 | Update index-features.md with all Phase 14 features | [x] |
| 7 | 7 | Verify Philosophy inheritance in implementation sub-features | [x] |
| 8 | 8 | Verify minimum sub-feature coverage (10+ features created) | [x] |
| 9 | 9 | Update Next Feature number in index-features.md after sub-feature creation | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 9 Tasks (Task#3 creates multiple sub-features as batch deliverable per Planning feature pattern) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 14 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 14 tasks and scope
   - Note engine component requirements (GameEngine, StateManager, KojoEngine, etc.)
   - Review Phase 14 design requirements (IGameEngine, IKojoEngine, INtrEngine interfaces)

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group engine components by responsibility
   - Assign explicit Feature IDs (F474+ for engine components - F472/F473 already used by Phase 13)
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

   **Actual Feature ID Allocation** (adjusted during execution):
   - **F474-F478**: Core engine components (GameEngine, StateManager, KojoEngine, CommandProcessor, NtrEngine)
   - **F479**: HeadlessUI
   - **F480**: ProcessState execution state machine
   - **F481**: InputHandler input processing
   - **F482-F483**: Character setup migrations (CHARA_SET.ERB, MANSETTTING.ERB)
   - **F484**: SCOMF prerequisite checks (Task 13: IsScenarioAvailable detail conditions, deferred from F473)
   - **F485**: Phase 14 Post-Phase Review (Task 11: type: infra)
   - **F486**: Phase 15 Planning (Task 12: type: research)

   **Decomposition Rationale**:
   - Group by engine component responsibility (F474-F478: Core engines, F479: UI, F480-F481: State/Input, F482-F483: Character setup)
   - Each feature targets 8-15 ACs per granularity rules
   - HeadlessUI (F479) separated from core engines: presentation layer concern for console-based testing, not core engine logic
   - Process state and input handling separated due to distinct scope (execution state vs user input)
   - Character setup migrations separated by file (CHARA_SET.ERB vs MANSETTTING.ERB)
   - Transition features follow standard pattern (F485: Review, F486: Planning)
   - F472 (Character Aggregate Extended Value Objects) and F473 (SCOMF Full Implementation) created during Phase 13 execution, so Phase 14 starts at F474

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 14: Era.Core Engine" per architecture.md
   - Include test PASS verification AC (per Sub-Feature Requirements)
   - Reference engine interfaces (IGameEngine, IKojoEngine, etc.)

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 15 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 14 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 14, feature-template.md | Phase 14 sub-feature files |
| 2 | spec-writer | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC#7 PASS |

**Execution Order**:
1. Analyze Phase 14 scope and create engine component categorization
2. Create all sub-feature files with Sub-Feature Requirements
3. Update index-features.md with all Phase 14 features
4. Verify AC#7 passes for implementation sub-features
5. Mark Tasks 1-8 complete

### Sub-Feature Requirements

Per architecture.md Phase 14, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 14: Era.Core Engine" in Philosophy section | All implementation features | AC#7 Grep |
| 2 | **AC: Test verification** - AC verifying tests pass after engine implementation | Each implementation feature | Manual inspection |
| 3 | **Engine interfaces** - Reference to IGameEngine/IKojoEngine/INtrEngine in Background | All implementation features | Manual inspection |
| 4 | **Tasks: 負債解消** - TODO/FIXME/HACK コメント削除タスクを含む | Each implementation feature | AC に not_contains |
| 5 | **AC: 負債ゼロ** - 技術負債ゼロを検証する AC を含む | Each implementation feature | AC 一覧確認 |
| 6 | **Tasks: 等価性検証** - legacy 実装との等価性テストを含む (Character setup migration features CHARA_SET.ERB/MANSETTTING.ERB only; other Phase 14 features are new implementations without legacy equivalents) | Character setup migrations | AC にテスト存在確認 |

---

## Phase 14 Scope Reference

**Partial snapshot from architecture.md (2026-01-12)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for complete task list (13 tasks total).

**Phase 14: Era.Core Engine**

**Goal**: Pure C# game engine (headless実行可能)

**Prerequisites**: Phase 3-13 完了（System, Architecture, Variable, Ability, Expression, Command, COM, DDD Foundation）

**Tasks (summary - see architecture.md for full list)**:
1. Implement GameEngine (main loop)
2. Implement StateManager (save/load JSON)
3. Implement KojoEngine (YAML parsing, condition evaluation)
4. Implement CommandProcessor (COM execution)
5. Implement NtrEngine (parameter calculations)
6. Implement HeadlessUI (console-based testing)
7. Implement ProcessState (実行状態機械)
8. Implement InputHandler (入力待ち処理)
9. Migrate CHARA_SET.ERB (キャラクターセットアップ)
10. Migrate MANSETTTING.ERB (男性キャラ設定)
11. Create Phase 14 Post-Phase Review feature (type: infra)
12. Create Phase 15 Planning feature (type: research)
13. Implement SCOMF prerequisite checks (IsScenarioAvailable 詳細条件、F473 からの延期)
    - Note: F473 (DONE) implemented SCOMF core logic (SOURCE/STAIN/EXP/TCVAR handlers). Task 13 addresses the deferred IsScenarioAvailable prerequisite checks (TALENT/ABL/FLAG-based execution availability) - a separate concern.

**Success Criteria**:
- [ ] Era.Core が headless 実行可能
- [ ] ProcessState 状態機械が確立
- [ ] CharacterSetup が C# 実装
- [ ] 全テスト PASS

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F470 | Phase 13 Post-Phase Review must pass first |
| Successor | F474-F484 | Phase 14 implementation sub-features (created by this feature) |
| Successor | F485 | Post-Phase Review Phase 14 (created by this feature) |
| Successor | F486 | Phase 15 Planning (created by this feature) |

---

## Links

- [feature-463.md](feature-463.md) - Phase 13 Planning (precedent feature)
- [feature-470.md](feature-470.md) - Phase 13 Post-Phase Review (dependency)
- [feature-472.md](feature-472.md) - Character Aggregate Extended Value Objects (Phase 13, predecessor)
- [feature-473.md](feature-473.md) - SCOMF Full Implementation (Phase 13, predecessor for F484)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines

---

## 残課題 (Deferred Tasks)

| Task | Reason | Target Phase | Target Feature |
|------|--------|:------------:|:--------------:|
| Fix architecture.md typo: "Phase 3-11" → "Phase 3-13" at line 3320 | Out of scope for this feature (F471 is correctly using Phase 3-13) | Phase 14 | F485 (Post-Phase Review) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

- **2026-01-13 FL**: AC count (9) exceeds typical research type guideline (3-5). Justification: Phase 14 has 13 tasks requiring detailed coverage verification across implementation sub-features, transition features, and Philosophy inheritance. This exceeds typical research scope but follows F463 (Phase 13 Planning) precedent.
- **2026-01-13 FL**: Sub-Feature Requirements #6 (等価性検証) restricted to character setup migrations. Rationale: architecture.md says "legacy 実装との等価性テストを含む" which implies a legacy implementation exists. Only CHARA_SET.ERB and MANSETTTING.ERB migrations have legacy ERB equivalents; other Phase 14 tasks are new implementations without legacy counterparts.
- **2026-01-13 FL**: architecture.md has typo "Phase 3-11" but should be "Phase 3-13" (DDD Foundation is Phase 13). Feature-471.md correctly uses "Phase 3-13". architecture.md typo tracked for separate fix.
- **2026-01-13 FL Phase 6**: Planning Validator suggested stricter AC pattern (individual file existence ACs). Rejected: F463 precedent uses identical pattern ("At least one implementation sub-feature created") and was successfully completed. AC structure follows established project convention.
- **2026-01-13 FL Phase 6**: Planning Validator questioned 等価性検証 restriction. Maintained: New C# engine components (GameEngine, ProcessState, etc.) are NOT migrations from existing ERB files - they're new implementations replacing Emuera engine logic. Only CHARA_SET.ERB and MANSETTTING.ERB are actual ERB-to-C# migrations requiring equivalence tests.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-12 | create | feature-builder | Created from F463 Phase 13 Planning | PROPOSED |
| 2026-01-13 | START | initializer | Initialize Feature 471 | [WIP] |
| 2026-01-13 | START | explorer | Phase 14 scope analysis | READY |
| 2026-01-13 | START | opus | Task 1: Document Phase 14 Feature Mapping | SUCCESS |
| 2026-01-13 | END | opus | Task 2: Document Phase 14 Engine Analysis | SUCCESS |
| 2026-01-13 | START | spec-writer (x3) | Tasks 3-5: Create sub-features F474-F486 (parallel) | - |
| 2026-01-13 | END | spec-writer | F474-F479 Core Engines created | SUCCESS |
| 2026-01-13 | END | spec-writer | F480-F484 State/Input/Migration created | SUCCESS |
| 2026-01-13 | END | spec-writer | F485-F486 Transition features created | SUCCESS |
| 2026-01-13 | END | opus | Task 6: Update index-features.md | SUCCESS |
| 2026-01-13 | END | opus | Task 7: Verify Philosophy (11/11 files) | SUCCESS |
| 2026-01-13 | END | opus | Task 8: Verify coverage (13 features) | SUCCESS |
| 2026-01-13 | END | opus | Task 9: Update Next Feature number (487) | SUCCESS |
| 2026-01-13 | END | ac-tester | Verify all 9 ACs | PASS:9/9 |
| 2026-01-13 | END | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-13 | DEVIATION | feature-reviewer | Doc-check (mode: doc-check) | NEEDS_REVISION |
| 2026-01-13 | END | opus | Fix 8 doc consistency issues (F486-F488 → F484-F486) | SUCCESS |

---

## Phase 14 Feature Mapping:

**Source**: architecture.md Phase 14 (13 tasks) → Sub-features (F474-F486, 13 features)

| Arch Task# | Task Name | Sub-Feature ID | Type | Rationale |
|:----------:|-----------|:--------------:|:----:|-----------|
| 1 | GameEngine main loop | F474 | engine | Core engine orchestration |
| 2 | StateManager JSON | F475 | engine | Persistence layer |
| 3 | KojoEngine YAML | F476 | engine | Dialogue system |
| 4 | CommandProcessor | F477 | engine | COM execution |
| 5 | NtrEngine | F478 | engine | NTR calculations |
| 6 | HeadlessUI | F479 | engine | Console testing |
| 7 | ProcessState | F480 | engine | Execution state machine |
| 8 | InputHandler | F481 | engine | Input processing |
| 9 | CHARA_SET.ERB | F482 | engine | Character setup migration |
| 10 | MANSETTTING.ERB | F483 | engine | Male character migration |
| 13 | SCOMF prerequisites | F484 | engine | IsScenarioAvailable conditions |
| 11 | Post-Phase Review | F485 | infra | Phase 14 review |
| 12 | Phase 15 Planning | F486 | research | Phase 15 planning |

**Note**: Adjusted from expected allocation (F486/F487/F488) to minimize ID gaps:
- Implementation features: F474-F484 (11 features)
- Transition features: F485-F486 (2 features)
- Total: 13 sub-features (vs expected 15 - reduced by consolidating related tasks)

---

## Phase 14 Engine Analysis:

### Decomposition Rationale

| Group | Feature IDs | Components | AC Target | Dependency |
|-------|:-----------:|------------|:---------:|------------|
| **Core Engines** | F474-F478 | GameEngine, StateManager, KojoEngine, CommandProcessor, NtrEngine | 10-12 each | Sequential (F474 first) |
| **UI Layer** | F479 | HeadlessUI | 8-12 | After F474 |
| **Process State** | F480 | ProcessState + CallStack + ExecutionContext | 10-12 | F474 prerequisite |
| **Input System** | F481 | InputHandler + InputRequest + InputValidator | 8-12 | After F474 |
| **Character Setup** | F482-F483 | CHARA_SET, MANSETTTING migrations | 8-12 each | Independent |
| **SCOMF Extension** | F484 | IsScenarioAvailable prerequisites | 8-12 | F473 (DONE) |
| **Transition** | F485-F486 | Post-Phase Review, Phase 15 Planning | 8-15, 3-5 | After all impl |

### Sub-Feature Quality Requirements (per architecture.md)

| # | Requirement | Applies To |
|:-:|-------------|------------|
| 1 | Philosophy: "Phase 14: Era.Core Engine" | F474-F484 |
| 2 | AC: Test PASS verification | F474-F484 |
| 3 | Background: IGameEngine/IKojoEngine/etc reference | F474-F484 |
| 4 | Task: 負債解消 (TODO/FIXME/HACK deletion) | F474-F484 |
| 5 | AC: 負債ゼロ verification | F474-F484 |
| 6 | Task: 等価性検証 (legacy equivalence) | F482-F483 only |

### Interface Requirements (Phase 4 Design)

```
IGameEngine → GameEngine (F474)
IStateManager → StateManager (F475)
IKojoEngine → KojoEngine (F476)
ICommandProcessor → CommandProcessor (F477)
INtrEngine → NtrEngine (F478)
IProcessState → ProcessState (F480)
IInputHandler → InputHandler (F481)
```

### DI Integration Point

Phase 14 is the DI consolidation phase. ServiceCollectionExtensions.cs must register all Phase 5-14 interfaces.
