# Feature 424: Phase 9 Planning

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

## Created: 2026-01-09

---

## Summary

**Feature to create Features**: Create Phase 9 sub-features from full-csharp-architecture.md.

Analyze architecture.md Phase 9 (Command Infrastructure + Mediator Pipeline) and create implementation sub-features.

**Output**: New Feature files (feature-{ID}.md) as primary deliverables.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 8 completion requires Phase 9 planning to maintain momentum:
- Phase 9 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

Phase 9 involves complex command system with 60+ commands + 16 SCOMF special commands + Mediator Pattern introduction requiring careful decomposition into implementable units.

### Goal (What to Achieve)

1. **Analyze Phase 9** requirements from full-csharp-architecture.md
2. **Create implementation sub-features** (F429-F435):
   - F429: CommandDispatcher + Mediator Pipeline (IPipelineBehavior, ICommand, ICommandHandler)
   - F430: Pipeline Behaviors (LoggingBehavior, ValidationBehavior, TransactionBehavior)
   - F431: Print Commands (PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA - 15+ commands)
   - F432: Flow Control Commands (IF/FOR/WHILE/CALL/RETURN - 18+ commands)
   - F433: Variable & Array Commands (LET/VARSET/ARRAYCOPY - 9+ commands)
   - F434: System Commands (Character/Style/System - 16+ commands, includes Task 8.5: GlobalStatic accessor migration)
   - F435: SCOMF Special Commands (16 files migration)
3. **Create transition features** (F436-F437):
   - F436: Phase 9 Post-Phase Review (type: infra)
   - F437: Phase 10 Planning (type: research)
4. **Update index-features.md** with F429-F437
5. **Verify sub-feature quality** (Philosophy, 負債ゼロ AC, 等価性検証 AC)

**Out-of-Scope**: F438 Repository Merge Infrastructure is documented in Pre-Phase Infrastructure section for context only. It is a separate infrastructure feature.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Feature mapping documented | file | Grep | contains | "Phase 9 Feature Mapping:" | [x] |
| 2 | F429 created (CommandDispatcher + Mediator) | file | Glob | exists | feature-429.md | [x] |
| 3 | F430 created (Pipeline Behaviors) | file | Glob | exists | feature-430.md | [x] |
| 4 | F431 created (Print Commands) | file | Glob | exists | feature-431.md | [x] |
| 5 | F432 created (Flow Control) | file | Glob | exists | feature-432.md | [x] |
| 6 | F433 created (Variable & Array) | file | Glob | exists | feature-433.md | [x] |
| 7 | F434 created (System Commands) | file | Glob | exists | feature-434.md | [x] |
| 8 | F435 created (SCOMF Special Commands) | file | Glob | exists | feature-435.md | [x] |
| 9 | F436 created (Post-Phase Review) | file | Glob | exists | feature-436.md | [x] |
| 10 | F437 created (Phase 10 Planning) | file | Glob | exists | feature-437.md | [x] |
| 11 | index-features.md updated | file | Grep | contains | "| 429 |" | [x] |
| 12 | Implementation sub-features (F429-F435) have Philosophy | file | Grep | contains | "Phase 9: Command Infrastructure" | [x] |
| 13 | Sub-features (F429-F435) have 負債ゼロ AC in AC table | file | Grep | contains | "not_contains.*TODO" | [x] |
| 14 | Sub-features (F429-F435) have 等価性検証 AC | file | Grep | contains | "equivalence" | [x] |

### AC Details

**AC#1**: Grep for "Phase 9 Feature Mapping:" in feature-424.md Execution Log

**AC#2-8**: Each implementation sub-feature file exists (F429-F435)
- F429: CommandDispatcher + Mediator Pipeline (architecture foundation)
- F430: Pipeline Behaviors (LoggingBehavior, ValidationBehavior, TransactionBehavior)
- F431: Print Commands (15+ commands: PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA)
- F432: Flow Control Commands (18+ commands: IF/ELSEIF/ELSE/ENDIF, FOR/NEXT, WHILE/WEND, CALL/CALLFORM/RETURN, GOTO/JUMP)
- F433: Variable & Array Commands (9+ commands: LET, VARSET, VARSIZE, ARRAYCOPY, ARRAYREMOVE, ARRAYSHIFT, ARRAYSORT)
- F434: System Commands (16+ commands: Character - ADDCHARA/DELCHARA/PICKUPCHARA, Style - SETCOLOR/SETFONT/ALIGNMENT, System - SAVEGAME/LOADGAME/QUIT)
- F435: SCOMF Special Commands (16 files: SCOMF1.ERB - SCOMF16.ERB special training commands)

**AC#9-10**: Transition features exist
- F436: Phase 9 Post-Phase Review (type: infra)
- F437: Phase 10 Planning (type: research)

**AC#11**: Grep for "| 429 |" in index-features.md to verify Phase 9 features added. F429 entry implies F430-F437 were added atomically in the same update (Task#11).

**AC#12**: Grep for "Phase 9: Command Infrastructure" (exact wording per architecture.md Sub-Feature Requirements) in each of the 7 implementation sub-feature files (F429-F435). All 7 files must contain this pattern in their Philosophy section. Transition features F436-F437 have different Philosophy requirements (infra/research type) and are excluded from this check.

**Verification** (post-Task 2-8): Run `grep -l "Phase 9: Command Infrastructure" feature-429.md feature-430.md feature-431.md feature-432.md feature-433.md feature-434.md feature-435.md | wc -l` and verify output is 7.

**AC#13**: Grep for "not_contains.*TODO" pattern in AC tables of each F429-F435 feature file (per F409 precedent). This is meta-verification: it confirms that each sub-feature's AC table includes a technical debt verification AC. Each engine sub-feature MUST define an AC that uses not_contains matcher for TODO (may also include FIXME/HACK).

**Verification** (post-Task 2-8): Run `grep -l "not_contains.*TODO" feature-429.md feature-430.md feature-431.md feature-432.md feature-433.md feature-434.md feature-435.md | wc -l` and verify output is 7.

**AC#14**: Grep for "equivalence" pattern in AC tables of F429-F435 (per F409 precedent). This verifies that each engine sub-feature includes an AC for legacy implementation equivalence verification per architecture.md Sub-Feature Requirements.

**Verification** (post-Task 2-8): Run `grep -l "equivalence" feature-429.md feature-430.md feature-431.md feature-432.md feature-433.md feature-434.md feature-435.md | wc -l` and verify output is 7.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document "Phase 9 Feature Mapping:" in Execution Log | [x] |
| 2 | 2 | Create F429: CommandDispatcher + Mediator Pipeline | [x] |
| 3 | 3 | Create F430: Pipeline Behaviors (Logging/Validation/Transaction) | [x] |
| 4 | 4 | Create F431: Print Commands (15+ commands) | [x] |
| 5 | 5 | Create F432: Flow Control Commands (IF/FOR/WHILE/CALL/RETURN - 18+ commands) | [x] |
| 6 | 6 | Create F433: Variable & Array Commands (9+ commands) | [x] |
| 7 | 7 | Create F434: System Commands (Character/Style/System - 16+ commands) | [x] |
| 8 | 8 | Create F435: SCOMF Special Commands (16 files) | [x] |
| 9 | 9 | Create F436: Phase 9 Post-Phase Review (type: infra) | [x] |
| 10 | 10 | Create F437: Phase 10 Planning (type: research) | [x] |
| 11 | 11 | Update index-features.md with Phase 9 features | [x] |
| 12 | 12 | Verify all sub-features have "Command Infrastructure" in Philosophy | [x] |
| 13 | 13 | Verify all sub-features have 負債ゼロ AC (TODO/FIXME not_contains) | [x] |
| 14 | 14 | Verify all sub-features have 等価性検証 AC (legacy/ERB equivalence) | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Sub-Feature Decomposition Strategy

**Rationale**: Phase 9 involves 60+ commands + 16 SCOMF files + Mediator Pipeline introduction. Per feature-template.md, engine type features should be ~300 lines with 8-15 ACs. Commands are split by responsibility category and pipeline infrastructure.

| Feature | Scope | Estimated Size |
|---------|-------|----------------|
| F429 | CommandDispatcher + ICommand/ICommandHandler/IPipelineBehavior interfaces | ~300 lines |
| F430 | LoggingBehavior + ValidationBehavior + TransactionBehavior (per architecture.md Pipeline Behaviors table) | ~250 lines |
| F431 | Print Commands (15+) | ~450 lines |
| F432 | Flow Control (18+: IF/FOR/WHILE/CALL/RETURN) | ~500 lines |
| F433 | Variable & Array Commands (9+) | ~300 lines |
| F434 | System Commands (16+: Character/Style/System) | ~400 lines |
| F435 | SCOMF Special Commands (16 files) | ~800 lines |

**Command Grouping Rationale** (per architecture.md command categories):
- **F429 Architecture**: Core infrastructure (Dispatcher, Mediator, Interfaces)
- **F430 Behaviors**: 横断的関心事 (Logging, Validation, Transaction)
- **F431 Print**: Output responsibility (15+ PRINT variants)
- **F432 Flow**: Control flow responsibility - combines Flow category (IF/ELSEIF/ELSE/ENDIF/FOR/NEXT/WHILE/WEND) + Call category (CALL/CALLFORM/RETURN/GOTO/JUMP) = 18+ commands
- **F433 Variable & Array**: Data manipulation (LET, VARSET, ARRAY operations)
- **F434 System**: Game state operations (Character, Style, System commands)
- **F435 SCOMF**: Special training commands (16 game-specific files)

### Sub-Feature Creation Checklist

Each sub-feature created by F424 MUST include the following per architecture.md Sub-Feature Requirements:

| # | Requirement | Verification |
|:-:|-------------|--------------|
| 1 | **Philosophy inheritance** - "Command Infrastructure + Mediator Pipeline" in Philosophy section | AC#12 (Grep) |
| 2 | **負債解消 Task** - Task to delete TODO/FIXME/HACK comments | Manual check during creation |
| 3 | **等価性検証 AC** - AC verifying legacy implementation equivalence | AC#14 (Grep "equivalence") |
| 4 | **負債ゼロ AC** - AC verifying zero technical debt (not_contains TODO) | AC#13 (Grep "not_contains.*TODO") |

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 9, feature-template.md | F429-F437 feature files |
| 2 | spec-writer | F429-F437 files | index-features.md update |
| 3 | (verify) | F429-F435 files | AC#12-14 PASS |

**Execution Order**:
1. Create all sub-feature files (F429-F437) with all 4 requirements per Sub-Feature Creation Checklist
2. Update index-features.md with all Phase 9 features
3. Verify AC#12-14 pass for all implementation sub-features (F429-F435)
4. Mark Tasks 1-14 complete

---

## Phase 9 Scope Reference

**Snapshot from architecture.md (2026-01-10, Phase 9 section unchanged from 2026-01-09)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for authoritative version.

**Phase 9: Command Infrastructure + Mediator Pipeline**

**Goal**: コマンドシステムの移行（60+ コマンド + 16 SCOMF）+ Mediator Pattern導入

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. CommandDispatcher アーキテクチャ | F429 | Architecture foundation |
| 2. コマンドインターフェース定義 | F429 | ICommand, ICommandHandler |
| 3. IPipelineBehavior定義 | F429 | Mediator pipeline interface |
| 4. LoggingBehavior実装 | F430 | Command logging |
| 5. ValidationBehavior実装 | F430 | Input validation |
| 6. 60+ コマンドハンドラ移行 | F431-F434 | Split by category |
| ├─ Print Commands | F431 | PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA (15+) |
| ├─ Flow Control Commands | F432 | IF/FOR/WHILE/CALL/RETURN (18+) |
| ├─ Variable & Array Commands | F433 | LET, VARSET, ARRAYCOPY (9+) |
| └─ System Commands | F434 | Character/Style/System (16+) |
| 7. 実行コンテキスト管理 | F429 | CommandContext |
| 8. フロー制御（IF/FOR/WHILE/CALL/RETURN）| F432 | See Task 6 row for categorization |
| 8.5. GameInitialization GlobalStatic accessor migration | F434 | 3 TODOs (Phase 7 引き継ぎ) |
| 9. SCOMF*.ERB 16ファイル移行 | F435 | Special training commands |
| 10. Post-Phase Review (type: infra) | F436 | Review F429-F435 |
| 11. Phase 10 Planning (type: research) | F437 | Next phase sub-features |

**Success Criteria**:
- [ ] 60+ コマンドが実装済み
- [ ] 16 SCOMF が実装済み
- [ ] CommandRegistry が DI 登録済み
- [ ] Mediator Pipeline が機能
- [ ] コマンド実行が legacy と等価

**Design Requirements**: See [F377 Design Principles](feature-377.md#design-principles) (static class禁止, Strongly Typed ID, Result型)

**Technical Debt from Phase 7**: GameInitialization GlobalStatic accessor migration (3 TODOs) - assigned to F434 (System Commands) per Task 8.5 mapping above

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F423 | Phase 8 Post-Phase Review must pass first (includes F428 engine-dependent functions) |
| Related | F377 | Design Principles (static class禁止, Strongly Typed ID, Result型) - mandatory for Phase 9 implementation |
| Successor | F429-F435 | Implementation sub-features (created by this feature) |
| Successor | F436 | Phase 9 Post-Phase Review (created by this feature) |
| Successor | F437 | Phase 10 Planning (created by this feature) |
| Related | F438 | Repository Merge Infrastructure (documented in Pre-Phase Infrastructure section, out-of-scope for F424) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 definition
- [feature-423.md](feature-423.md) - Phase 8 Post-Phase Review (dependency)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (reference)
- [feature-428.md](feature-428.md) - F428 Engine-Dependent Functions (Phase 8, predecessor via F423)
- [feature-409.md](feature-409.md) - Phase 8 Planning (predecessor pattern)
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-429.md](feature-429.md) - F429 CommandDispatcher + Mediator (to be created)
- [feature-430.md](feature-430.md) - F430 Pipeline Behaviors (to be created)
- [feature-431.md](feature-431.md) - F431 Print Commands (to be created)
- [feature-432.md](feature-432.md) - F432 Flow Control (to be created)
- [feature-433.md](feature-433.md) - F433 Variable & Array (to be created)
- [feature-434.md](feature-434.md) - F434 System Commands (to be created)
- [feature-435.md](feature-435.md) - F435 SCOMF Special Commands (to be created)
- [feature-436.md](feature-436.md) - F436 Phase 9 Post-Phase Review (to be created)
- [feature-437.md](feature-437.md) - F437 Phase 10 Planning (to be created)
- [feature-438.md](feature-438.md) - F438 Repository Merge Infrastructure (out-of-scope, context only)

---

## Pre-Phase Infrastructure: Repository Merge

> **CONTEXT ONLY** - This section documents infrastructure decisions for Phase 9 context. Implementation is tracked in separate Feature F438 (out-of-scope for F424).

**Decision Date**: 2026-01-10

### Background

歴史的経緯により `engine/` は submodule として別リポジトリで管理されていた。しかし：
- ロジック ERB → C# (Era.Core) への移行により、engine との境界が曖昧に
- Submodule 管理のオーバーヘッド
- Era.Core と engine 間の参照が複雑

**決定**: Option A - リポジトリ統合（engine を吸収）

### License Compliance

| ファイル | ライセンス | 権利者 | 対応 |
|----------|-----------|--------|------|
| `engine/LICENSE` | Apache 2.0 | xerysherry (uEmuera) | 保持必須 |
| `engine/Assets/StreamingAssets/license@emuera` | zlib風 | MinorShift (Emuera) | 保持必須 |

**Apache 2.0 Section 4(b)**: 変更したファイルに変更表示が必要 → Git履歴で追跡可能にする

### Implementation Steps

```bash
# 1. 現在の submodule 状態を確認
git submodule status

# 2. submodule 設定を削除（.gitmodules から engine エントリ削除）
git rm --cached engine
rm -rf .gitmodules  # または engine エントリのみ削除

# 3. engine リポジトリを remote として追加
git remote add engine-origin <engine-repo-path>
git fetch engine-origin

# 4. subtree merge で履歴ごと取り込み
git merge -s ours --no-commit --allow-unrelated-histories engine-origin/main
git read-tree --prefix=engine/ -u engine-origin/main
git commit -m "feat(infra): Merge engine repository into main repo

Absorb engine submodule with full git history for license compliance.
- Apache 2.0 requires modification notices (tracked via git log)
- All LICENSE files preserved
- Future development in unified repository"

# 5. engine 内の .git 参照を削除
rm -rf engine/.git

# 6. remote を削除
git remote remove engine-origin

# 7. 動作確認
dotnet build
dotnet test
```

### Verification

| Check | Command | Expected |
|-------|---------|----------|
| 履歴統合 | `git log -- engine/` | engine の過去コミットが表示される |
| LICENSE 存在 | `ls engine/LICENSE` | ファイル存在 |
| license@emuera 存在 | `ls engine/Assets/StreamingAssets/license@emuera` | ファイル存在 |
| ビルド成功 | `dotnet build` | 成功 |
| テスト成功 | `dotnet test` | 成功 |

### Result

```
統合後:
era紅魔館protoNTR/
├── Era.Core/           # C# ゲームロジック
├── Game/               # ERB コンテンツ
├── engine/             # 統合されたエンジン（履歴付き）
│   ├── Assets/
│   ├── LICENSE         # Apache 2.0 (uEmuera)
│   └── Assets/StreamingAssets/
│       └── license@emuera  # zlib (Emuera)
└── .git                # 両リポジトリの履歴を含む
```

### Timing

Phase 9 開始前（F429 実装前）に実施。独立した作業のため、他の Feature と並行可能。

**Note**: This Repository Merge section documents infrastructure context for Phase 9. Implementation should be tracked as a separate infrastructure task (e.g., via /next) before F429 implementation begins. F424 does not create this feature; it only documents the decision and context.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2026-01-10 FL iter1**: AC count (14) exceeds research type guideline (3-5). Justified breakdown: 9 deliverable existence ACs (F429-F437 files) + 1 mapping documentation AC + 1 index update AC + 3 quality verification ACs (philosophy, debt-zero, equivalence). Each AC maps 1:1 to verifiable output. Consolidation would compromise traceability for /do execution. Precedent: F409 (Phase 8 Planning) has identical structure (14 ACs for 9 sub-features).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created as Phase 9 planning feature per mandatory transition | PROPOSED |
| 2026-01-10 | START | initializer | Phase 1: Initialize | READY:424:research |
| 2026-01-10 | START | Opus | Phase 2: Investigation | - |
| 2026-01-10 | END | Opus | Phase 2: Investigation | Artifacts confirmed |
| 2026-01-10 | START | Opus | Phase 4: Implementation | - |
| 2026-01-10 | - | - | Phase 9 Feature Mapping: | F429-F437 from architecture.md |
| 2026-01-10 | - | - | F429: CommandDispatcher + Mediator | ICommand/Handler/Pipeline |
| 2026-01-10 | - | - | F430: Pipeline Behaviors | Logging/Validation/Transaction |
| 2026-01-10 | - | - | F431: Print Commands | 15+ commands |
| 2026-01-10 | - | - | F432: Flow Control | 18+ commands (IF/FOR/WHILE/CALL) |
| 2026-01-10 | - | - | F433: Variable & Array | 9+ commands |
| 2026-01-10 | - | - | F434: System Commands | 16+ commands + GlobalStatic TODOs |
| 2026-01-10 | - | - | F435: SCOMF Special | 16 files |
| 2026-01-10 | - | - | F436: Post-Phase Review | type: infra |
| 2026-01-10 | - | - | F437: Phase 10 Planning | type: research |
| 2026-01-10 | END | implementer | Phase 4: Implementation | Tasks 1-11 [x] |
| 2026-01-10 | START | ac-tester | Phase 6: Verification | AC#1-14 |
| 2026-01-10 | END | ac-tester | Phase 6: Verification | AC#1-14 PASS |
| 2026-01-10 | START | feature-reviewer | Phase 7: Post-Review (post) | - |
| 2026-01-10 | END | feature-reviewer | Phase 7: Post-Review (post) | READY |
| 2026-01-10 | START | feature-reviewer | Phase 7: Doc-Check | - |
| 2026-01-10 | END | feature-reviewer | Phase 7: Doc-Check | READY |
| 2026-01-10 | - | - | Phase 7: SSOT Update Check | No updates needed (research type) |
