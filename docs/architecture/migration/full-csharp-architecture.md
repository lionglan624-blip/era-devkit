# Full C# Architecture Design

**Status**: FINAL
**Version**: v1.9-v1.11 (Migration phases) - **Revised 2026-01-12**
**Feature**: [feature-343.md](../feature-343.md)
**Predecessor**: [feature-341.md](../feature-341.md)

> **Revision Note (2026-02-12 #22)**: Document split into phase-grouped files for maintainability.
> - Phase details extracted to `phases/` directory (see Table of Contents below)
> - Design reference (Type Design, C# 14 Patterns, Architecture Layers) extracted to `phases/design-reference.md`
> - This file retained as index/overview with cross-references
>
> **Revision Note (2026-01-20 #21)**: F566 CI Modernization - removed obsolete checks
>
> **Revision Note (2026-01-12 #20)**: Phase 16 C# 14 Style Migration inserted
>
> **Revision Note (2026-01-12 #19)**: Phase 23 NTR Kojo Reference Analysis inserted
>
> **Revision Note (2026-01-10 #18)**: Phase 11 xUnit v3 Migration inserted
>
> **Revision Note (2026-01-10 #17)**: Phase 10 NuGet Package Details added
>
> **Revision Note (2026-01-10 #16)**: Phase 10 Runtime Upgrade inserted
>
> **Revision Note (2026-01-10 #15)**: Phase 27 Directory Structure Refactoring added
>
> **Revision Note (2026-01-10 #14)**: Phase 15 OCP Technical Debt tracking added
>
> **Revision Note (2026-01-09 #13)**: Phase 7 Technical Debt Review - task routing
>
> **Revision Note (2026-01-09 #12)**: F406 Deferred Items tracking added
>
> **Revision Note (2026-01-08 #11)**: Phase 7 (Technical Debt Consolidation) inserted
>
> **Revision Note (2026-01-07 #10)**: Test Infrastructure Transition section added
>
> **Revision Note (2026-01-07 #9)**: Design robustness improvements
>
> **Revision Note (2026-01-06 #8)**: Phase 4 (Architecture Refactoring) inserted
>
> Previous revisions (#1-#7): See git history for details.

---

## Table of Contents

| Section | File | Description |
|---------|------|-------------|
| **Overview** | (this file) | Strategy, dependencies, phase overview, progression rules |
| **Design Reference** | [phases/design-reference.md](phases/design-reference.md) | Type Design, C# 14 Patterns, Architecture Layers, Content/Logic/UI Layer details, Concurrency |
| **Phase 1-4** | [phases/phase-1-4-foundation.md](phases/phase-1-4-foundation.md) | Tools, Test Infrastructure, System Infrastructure, Architecture Refactoring |
| **Phase 5-19** | [phases/phase-5-19-content-migration.md](phases/phase-5-19-content-migration.md) | Variable System through Kojo Conversion |
| **Phase 20-27** | [phases/phase-20-27-game-systems.md](phases/phase-20-27-game-systems.md) | Equipment, Counter, State, NTR Analysis/Design, AI/Visitor, Special Modes, Extensions |
| **Phase 28-34** | [phases/phase-28-34-integration.md](phases/phase-28-34-integration.md) | Domain Events, WPF UI, Integration, Directory, Documentation, Validation, Save Migration |

---

## Migration Strategy

### Full Migration (No Transition Period)

**CRITICAL**: This migration is executed as a **complete replacement**, not an incremental transition.

| Aspect | Approach |
|--------|----------|
| **Execution** | All phases implemented sequentially, then switch over |
| **ERB Files** | Archived after full C# implementation complete |
| **Parallel Running** | NOT supported - old/new systems don't coexist |
| **Rollback** | Git-based only (no runtime fallback) |

**Implications**:
- No ERB loader infrastructure needed in new system
- All game logic must be 100% C# before release
- Comprehensive testing required before switchover
- Save migration tool is **optional** (see Phase 34)

### Feature Progression Protocol

**CRITICAL**: Each phase ends with explicit feature creation tasks. Responsibility and timing are defined below.

#### Phase Planning Feature Creation

| Trigger | Who | What | When |
|---------|-----|------|------|
| Phase N-1 final feature DONE | finalizer agent | Create Phase N planning feature (research type) | Immediately after finalizer marks predecessor DONE |

#### Sub-Feature Creation from Planning

| Trigger | Who | What | When |
|---------|-----|------|------|
| Planning feature Task "Create sub-features" | implementer agent | Create ALL sub-features listed in planning (not just minimum for AC) | During planning feature execution |

**IMPORTANT**: Planning feature AC may require "at least N sub-features" but the Task MUST create ALL sub-features identified in analysis, not just the minimum.

#### Sub-Feature Dependency Analysis (Mandatory)

**Lesson from Phase 21 (F783)**: File-prefix grouping alone produces incorrect dependency graphs. All sub-features received only the planning feature as Predecessor, missing inter-feature call-chain dependencies (e.g., F803→F801, F805→F803/F804, F806-F808→F805, F810→F809, F811→F801/F812).

**Required Steps** (during planning feature execution):

| Step | Action | Output |
|:----:|--------|--------|
| 1 | `Grep("CALL\|TRYCALL\|CALLFORM\|JUMP", file)` for all ERB files in scope | Cross-file call map |
| 2 | Derive call direction: caller → callee per sub-feature boundary | Directed dependency graph |
| 3 | For each sub-feature pair (A calls B): declare B as Predecessor of A if B defines interfaces/functions A must consume | Predecessor rows in Dependencies table |
| 4 | Set index-features.md Depends On column with **bold** for non-[DONE] predecessors | Index consistency |

**Anti-Pattern**: Creating all DRAFT stubs with `Depends On: F{planning}` only. Every sub-feature MUST have its inter-feature predecessors declared at DRAFT creation time.

#### Sub-Feature Execution Order

| Priority | Execution | Condition |
|----------|-----------|-----------|
| CRITICAL | Sequential | Must complete before next CRITICAL starts |
| HIGH | Parallel OK | After all CRITICAL complete |
| Medium | Parallel OK | After all HIGH complete |

#### Feature Planning Guidelines

**Granularity Rules**:

| Metric | Target | Rationale |
|--------|:------:|-----------|
| ACs per Feature | 5-12 | Single session verifiable |
| Tasks per Feature | 3-7 | Parallel development possible |
| AC:Task ratio | ~1.5:1 | Complexity indicator |
| Independent subsystems | 1 per Feature | Single responsibility |

**SOLID Compliance Checklist** (for engine/erb type features):

| Principle | Verification |
|-----------|--------------|
| **S** (Single Responsibility) | Each Feature handles one subsystem |
| **O** (Open/Closed) | Interfaces defined for extension |
| **L** (Liskov Substitution) | Result<T> for error handling |
| **I** (Interface Segregation) | Focused interfaces, not monolithic |
| **D** (Dependency Inversion) | All classes depend on interfaces |

**Feature Splitting Criteria**:

Split a Feature when ANY condition applies:
- AC count > 12
- Task count > 7
- Multiple independent subsystems identified
- Different agents needed (e.g., implementer vs kojo-writer)

**Phase Transition Features (Mandatory)**:

Each Phase completion requires **two separate features** (SRP compliance):

| Feature | Type | Responsibility |
|---------|------|----------------|
| Phase N Post-Phase Review | infra | Review all Phase N features, verify zero technical debt |
| Phase N+1 Planning | research | Analyze next phase, create sub-features |

**Post-Phase Review Redux Pattern (残課題発生時)**:

When Post-Phase Review completes with 残課題:

| Phase | Trigger | Action |
|-------|---------|--------|
| 1. Review Complete | Post-Phase Review [DONE] with 残課題 > 0 | 残課題 fixes as separate features |
| 2. Fixes Complete | All fix features [DONE] | Create Redux Post-Phase Review |
| 3. Redux Complete | Redux [DONE] | Unblock Planning feature |

**Planning Feature Predecessor Rule**:

| Post-Phase Review State | Planning Predecessor |
|-------------------------|---------------------|
| 残課題 == 0 | Original Post-Phase Review |
| 残課題 > 0 (Redux exists) | **Redux** Post-Phase Review |

**Post-Phase Review Feature (infra)**:

| AC# | Description | Verification |
|:---:|-------------|--------------|
| 1-N | Each Phase N feature review passed | feature-reviewer (post-phase) |
| N+1 | Technical debt is zero | Manual verification |
| N+2 | Forward compatibility documented | Execution log |
| **N+3** | **Deferred tasks tracked in next Phase** | Grep architecture.md Phase N+1 |
| **N+4** | **Redux trigger evaluated** | 残課題 > 0 -> Redux DRAFT created, Planning blocked |

**Review Checklist**:

| Check | Question | Action if NO |
|-------|----------|--------------|
| **Philosophy Alignment** | 実装がPhase思想に合致しているか? | Fix in current phase |
| **SOLID Compliance** | SOLID原則に違反していないか? | Refactor in current phase |
| **Forward Compatibility** | 次Phase以降で変更が必要な箇所はないか? | Document for Planning feature |
| **Technical Debt** | 技術負債は残っていないか? | Must be zero to proceed |
| **Deferred Tasks** | 延期タスクが次Phase Tasksに明記されているか? | Add to architecture.md + Planning feature |

**Planning Feature (research)** - Feature を立てる Feature:

**Purpose**: 次 Phase の sub-features を作成する。主成果物は feature-{ID}.md ファイル。

**Summary 必須記載**: 「**Feature を立てる Feature**: Phase N の sub-features を作成する計画 Feature。」

---

## Incremental E2E Test Strategy

**CRITICAL**: 単体テストだけでは統合時の爆発を防げない。Phase完了時にE2Eチェックポイントを段階的に導入し、統合リスクを早期検出する。

### 背景・動機

各Phaseの単体テスト＋等価性テストだけでは以下のリスクが残る：

| リスク | 発生タイミング | 影響 |
|--------|---------------|------|
| DI登録漏れ・循環依存 | 統合時（Phase 30） | 全サービス起動不能 |
| インターフェース契約不整合 | Cross-system呼び出し時 | 実行時例外 |
| イベント伝播の断絶 | Domain Events統合時（Phase 28） | 状態不整合 |
| Headless実行の退行 | Phase 14以降いつでも | CI/テスト基盤崩壊 |

**対策**: Phase完了のPost-Phase Reviewに段階的E2Eチェックポイントを義務化する。

### E2E Checkpoint 一覧

| Checkpoint | Phase | Level | 検証内容 | 実行環境 |
|:----------:|:-----:|-------|----------|----------|
| **CP-1** | 14 | Smoke | Headless起動→DI解決→1コマンド実行→正常終了 | `dotnet run --project uEmuera.Headless` |
| **CP-2** | 22 | Partial | Shop購入→装備反映→Counter更新→State変化 の一連フロー | Headless + seeded scenario |
| **CP-3** | 27 | System | 全サブシステム間連携（NTR trigger含む）、Phase 20-27 全DI解決 | Headless + 複合scenario |
| **CP-4** | 30 | Full | Phase 30 E2E Test Strategy（既存設計）に統合 | Headless + Golden Master |

### Checkpoint 詳細

#### CP-1: Smoke E2E（Phase 14 Post-Phase Review）

**Status**: DONE（Phase 14 完了済み。HeadlessUI実装によりDI解決+コマンド実行は検証済み）

**検証項目**:
- [ ] DIコンテナ起動（循環依存なし）
- [ ] Headless モードで基本コマンド実行
- [ ] 正常終了（例外なし）

#### CP-2: Partial E2E（漸進的構築）

**Status**: TODO

CP-2 は Phase 22 Post-Phase Review で一括実装するのではなく、Phase 完了ごとにスコープを拡張する。各 Step で追加した E2E は退行テストとして以降も常時実行される（削除・無効化禁止）。

**Step 2a — Phase 21 Post-Phase Review (F813)**:
- `src/Era.Core.Tests/E2E/` ディレクトリ + テスト基盤確立
- `AddEraCore()` DI全解決（Phase 5-21 全サービス、例外なし）
- Training->Counter cross-system フロー（seeded 決定的実行）

**Step 2b — Phase 22 各sub-feature 完了時**:
- 当該系統の E2E 追加（服装->State変化、妊娠->日送り 等）
- Step 2a の全 E2E が退行なし

**Step 2c — Phase 22 Post-Phase Review**:
- Phase 20-22 DI統合解決
- Shop購入->Counter更新->State変化 の一連フロー
- Headless シナリオで再現可能（seeded）
- Step 2a-2b の全 E2E が退行なし

**実装方針**:
- `src/Era.Core.Tests/E2E/` に Phase 別テストクラスを配置
- 既存の `IRandomProvider` seeded mock を活用
- 複数サービスを実DI（Mock最小限）で結合

#### CP-3: System E2E（Phase 27 Post-Phase Review）

**Status**: TODO（Phase 27 未着手）

**検証項目**:
- [ ] Phase 20-27 全サブシステムのDI統合解決
- [ ] 訓練→能力成長→口上分岐 の連鎖フロー
- [ ] NTR trigger→Mark進行→口上変化 のフロー（Phase 23-25 成果物）
- [ ] 全システム間のイベント伝播（Phase 28 の前準備）

#### CP-4: Full E2E（Phase 30）

既存の Phase 30 E2E Test Strategy をそのまま適用。CP-1〜CP-3 で段階的に検証済みのため、Phase 30 での爆発リスクは大幅に軽減される。

### Post-Phase Review への統合ルール

| 対象Phase | 追加義務 |
|-----------|----------|
| Phase 14 Review | CP-1 検証結果を Success Criteria に記載 |
| **Phase 21 Review** | **CP-2 Step 2a: E2E基盤構築 + DI全解決 + Training->Counter** |
| Phase 22 Review | CP-2 Step 2c: 全項目達成 + 全既存E2E退行なし |
| Phase 27 Review | CP-3 実装 + 検証結果を Success Criteria に記載 |
| Phase 30 | CP-4（既存設計通り） |

> **原則**: E2E checkpoint で発見された統合不具合は、次Phaseに持ち越さず当該Phase内で修正する。

---

## External Dependencies & Roadmap

| 依存 | 現状 | Phase 10 後 |
|------|------|-------------|
| **.NET** | 8.0 | **10.0** |
| **C#** | 12 | **14** |
| **WPF** | - | .NET 10 (Phase 29) |

### UI プラットフォーム選定

| 候補 | 評価 | 結論 |
|------|------|------|
| **Unity** | .NET Standard 2.1 制約、Era.Core (.NET 10) との不一致、CoreCLR 対応時期不明 | **却下** |
| **Avalonia** | クロスプラットフォーム対応だが、配布先がストア審査不可のためメリット薄い | 将来検討 |
| **WPF** | .NET 10 ネイティブ、テキスト描画(DirectWrite)得意、DI直接統合、ライセンス不要 | **採用** |

**決定根拠**:
- メイン配布: Windows 直接配布(DLSite, Ci-en) -- iOS/Android ストアは成人向けで審査不可
- Era.Core が .NET 10 / C# 14 -> WPF なら直接プロジェクト参照(DLL互換性問題ゼロ)
- テキスト主体のeraゲーム -> WPF の DirectWrite テキスト描画が最適
- 将来 Android APK サイドロード対応が必要になれば Avalonia で薄いUI層を追加する方がコスト低

---

## POC Plan (Completed)

> POC completed during Phase 1. Retained for historical reference.

| Item | Selection | Reason |
|------|-----------|--------|
| Character | K1_Meiling | Simplest, existing kojo |
| Actions | COM_0, COM_1 | Basic aibu commands |
| Features | YAML load, dialogue print, input | Minimal viable loop |

All POC tasks and success criteria completed.

---

## Design Rationale

### Why Full Migration (Not Bridge)?

| Approach | Short-term | Long-term | Final Debt |
|----------|:----------:|:---------:|:----------:|
| Bridge (Option D) | Medium | 2-system maintenance | Bridge layer cleanup |
| **Full Migration** | High | Single stack | **Zero** |

**Decision**: Accept high upfront cost to achieve zero long-term technical debt.

### Key Benefits

| Benefit | Description |
|---------|-------------|
| **Single Language** | C# only, no ERB knowledge required |
| **Full Testability** | Unit tests for all logic, integration tests for flows |
| **Modern Tooling** | Visual Studio, Rider, .NET profiler, diagnostics |
| **Extensibility** | WPF custom controls, .NET ecosystem (audio, localization) |
| **Maintainability** | Standard C# patterns, clear architecture |

---

## Design Reference

For detailed design guidelines, see [phases/design-reference.md](phases/design-reference.md):
- Type Design Guidelines (Strongly Typed IDs, Result<T>, DI)
- C# 14 Patterns (Primary Constructors, Collection Expressions)
- Architecture Layers (Content/Logic/UI)
- Content Layer YAML Schema
- Concurrency Design Guidelines

---

## Migration Path

### Phase Overview

**Total: 35 phases (Phase 0-34)**

| Phase | Goal | Deliverable |
|:-----:|------|-------------|
| 0 | Design | This document (FINAL) |
| 1 | Tools | ERB->YAML converter pipeline (F346-F353) |
| 2 | Test Infrastructure | Era.Core.Tests + 6 TEST*.ERB migration + CI |
| 3 | System Infrastructure | SYSTEM.ERB, COMMON*.ERB, 7 ERH headers - shared foundations |
| **4** | **Architecture Refactoring** | **SRP分割, Strongly Typed IDs, DI導入, Result型** |
| 5 | Variable System | VariableCode, VariableData, scope management |
| 6 | Ability & Training Foundation | ABL.ERB, ABL_UP_DATA.ERB, TRACHECK*.ERB |
| **7** | **Technical Debt Consolidation** | **IVariableStore ISP分割, Callback DI正式化, 統合テスト** |
| 8 | Expression & Function System | ExpressionParser, operators, 100+ built-in functions |
| 9 | Command Infrastructure + **Mediator** | CommandRegistry, 60+ commands, SCOMF, **IPipelineBehavior** |
| **10** | **Runtime Upgrade** | **.NET 10 / C# 14 アップグレード** |
| **11** | **xUnit v3 Migration** | **xUnit v2 -> v3 破壊的変更対応** |
| 12 | COM Implementation | COMF*.ERB 150+ files -> src/Era.Core/Commands/Com/ |
| **13** | **DDD Foundation** | **Aggregate Root, Repository, UnitOfWork patterns** |
| 14 | Era.Core Engine | GameEngine, StateManager, KojoEngine, NtrEngine, ProcessState |
| 15 | Architecture Review | Structure validation, targeted refactoring, design doc update |
| **16** | **C# 14 Style Migration** | **Primary Constructor変換 (50ファイル), Collection Expression適用** |
| 17 | Data Migration | CSV -> YAML with detailed mapping (43 CSV files) |
| **18** | **KojoEngine SRP分割** | **Dialogue Loading/Evaluation/Rendering/Selection separation** |
| 19 | Kojo Conversion | All kojo ERB -> YAML (117 files + 5 utility files) |
| 20 | Equipment & Shop | SHOP*.ERB, 体設定.ERB, アイテム説明.ERB |
| 21 | Counter System | COUNTER_*, TOILET_COUNTER_*, COMABLE*, SOURCE* |
| 22 | State Systems | Clothing, Pregnancy, Stain, Room, Weather, Sleep, Menstrual |
| **23** | **NTR Kojo Reference Analysis** | **咲夜NTR分岐統計, DDD設計入力, Phase 8h/8m/8n Gap分析** |
| **24** | **NTR Bounded Context設計** | **NTR Domain Model, Aggregates, Events, Services** |
| 25 | AI & Visitor Systems | Visitor AI, NTR subsystems (14 files), 訪問者宅拡張 |
| 26 | Special Modes & Messaging | SexHara (9 files), WC_SexHara (7 files), MSG_FUNC, 住人交流 |
| 27 | Extensions | 経歴, 会話(10), 外出, 妖精メイド(16), CORE8666 |
| **28** | **Domain Events統合** | **IDomainEvent, EventPublisher, Cross-cutting Handlers** |
| 29 | WPF UI | WpfGameUI, グラフィック表示 integration |
| 30 | Integration | Full game on new stack, ERB archived |
| **31** | **Directory Structure** | **Zero-base directory refactoring, reference link preservation** |
| 32 | Documentation | Skills, CLAUDE.md, Agents updated |
| 33 | Validation | Parallel run + regression tests |
| 34 | Save Migration | Legacy save converter **(OPTIONAL)** |

### Phase Details

| Phase Group | File |
|-------------|------|
| Phase 1-4 (Foundation) | [phases/phase-1-4-foundation.md](phases/phase-1-4-foundation.md) |
| Phase 5-19 (Variable -> Kojo) | [phases/phase-5-19-content-migration.md](phases/phase-5-19-content-migration.md) |
| Phase 20-27 (Game Systems) | [phases/phase-20-27-game-systems.md](phases/phase-20-27-game-systems.md) |
| Phase 28-34 (Integration) | [phases/phase-28-34-integration.md](phases/phase-28-34-integration.md) |

### Phase Dependencies

```
Phase 1 (Tools)
    |
Phase 2 (Test Infrastructure)
    |
Phase 3 (System Infrastructure) <- CRITICAL: All systems depend on this
    |
Phase 4 (Architecture Refactoring) <- SRP/DI/型設計パターン確立
    |
Phase 5 (Variable System) ----------------------+
    |                                           |
Phase 6 (Ability & Training Foundation) --------+
    |                                           |
Phase 7 (Technical Debt Consolidation) <- ISP/DI正式化/統合テスト
    |                                           |
Phase 8 (Expression & Function) ----------------+
    |                                           |
Phase 9 (Command + Mediator Pipeline) ----------+  <- IPipelineBehavior追加
    |                                           |
Phase 10 (Runtime Upgrade) <- .NET 10/C# 14     |
    |                                           |
Phase 11 (xUnit v3 Migration) <- 破壊的変更対応 |
    |                                           |
Phase 12 (COM Implementation) ------------------+  <- 150+ COMF files
    |                                           |
Phase 13 (DDD Foundation) <---------------------+  <- Aggregate/Repository/UoW
    |
Phase 14 (Era.Core Engine + State Machine)
    |
Phase 15 (Architecture Review) <- CHECKPOINT
    |
Phase 16 (C# 14 Style Migration)
    |
Phase 17 (Data Migration)
    |
Phase 18 (KojoEngine SRP分割)
    |
Phase 19 (Kojo Conversion)
    |
+-----------------------------------------------+
| Parallel Execution Possible (Phase 20-22)     |
| Phase 20 & 21 parallel OK                     |
| Phase 22 must run alone (see concurrency)     |
+-----------------------------------------------+
    |
Phase 23 (NTR Kojo Reference Analysis)
    |
Phase 24 (NTR Bounded Context)
    |
Phase 25 (AI & Visitor + NTR Implementation)
    - F808 Handoff: Implement NTR_NAME(0) in NTR_UTIL migration (INtrUtilityService.GetNtrName currently returns "")
    - F808 Handoff: Implement NTR_RESET_VISITOR_ACTION in NTR_UTIL migration (INtrUtilityService.NtrResetVisitorAction currently no-op)
    |
Phase 26 (Special Modes & Messaging)
    |
Phase 27 (Extensions)
    |
Phase 28 (Domain Events)
    |
Phase 29 (WPF UI)
    |
Phase 30 (Integration)
    |
Phase 31 (Directory Structure)
    |
Phase 32 (Documentation)
    |
Phase 33 (Validation)
    |
Phase 34 (Save Migration) [OPTIONAL]
```

### Phase Progression Rules

| Rule | Description | Example |
|------|-------------|---------|
| **Phase完了時 -> Transition Features** | 各Phaseの実装完了後、Review と Planning を別 Feature として作成 | Phase 6: F397(Review) + F398(Planning) |
| **Review -> Planning 順序** | Review が PASS してから Planning を実行 | F397 DONE -> F398 実行可能 |
| **Planning = Feature を立てる Feature** | Planning feature の主成果物は feature-{ID}.md ファイル | F398 -> Phase 7 sub-features 作成 |
| **Redux Pattern (残課題発生時)** | Review 完了時に残課題 > 0 -> 修正後 Redux Review 作成、Planning は Redux に依存 | Phase 19: F646(Review) -> F649-702(修正) -> F703(Redux) -> F647(Planning) |

**Transition Features 構成** (各 Phase 共通):

| Feature | Type | 責務 |
|---------|------|------|
| Phase N Post-Phase Review | infra | 全 Phase N 実装 Feature のレビュー |
| Phase N+1 Planning | research | **Feature を立てる Feature**: 次 Phase の sub-features 作成 |

**Post-Phase Review 必須タスク** (各 Phase 共通):

| タスク | 内容 | 検証方法 |
|--------|------|----------|
| **Architecture Doc 整合確認** | 本ドキュメントの該当 Phase セクションと実装の整合を検証 | 手動比較 |
| **Success Criteria 更新** | 実装完了後、該当Phase file の `[ ]` を `[x]` に更新 | Edit |
| **差異の文書化** | 設計と実装の差異があれば該当Phase fileを更新 | Edit |
| **Deliverables 検証** | 該当Phase file の Deliverables 表と実際のファイルを照合 | Glob/Grep |
| **Redux 判定** | 残課題 > 0 の場合、Redux Post-Phase Review DRAFT 作成 + Planning を [BLOCKED] に設定 | 上記 Redux Pattern 参照 |
| **cross-phase trigger verification** | 完了した Phase が他の Phase/セクションで定義された義務をトリガーするか確認 | architecture.md 全文検索 |
| **Analyzer NoWarn 負債修正** | `memory/analyzer-nowarn-debt.md` の高優先度ルールを1つ以上選び、手順: (1) `Directory.Build.props` NoWarnから該当CAルールを除去, (2) `dotnet format analyzers erakoumakanNTR.sln --diagnostics CAXXXX --severity error` で自動修正, (3) `dotnet build` + `dotnet test` で確認, (4) 失敗時はNoWarnに戻して次Phaseに繰り越し。修正結果を `memory/analyzer-nowarn-debt.md` に反映（件数更新 or 行削除） | `dotnet format analyzers` |
| **Stryker.NET mutation score** | `cd Era.Core.Tests && dotnet stryker` を実行。killed%, survived%, total mutants をprogress logに記録。前回Post-Phase Reviewの値と比較し、killed%が下がっていれば原因調査。F813が初回baseline | `dotnet stryker` |
| **Dashboard lint/format** | `cd src/tools/node/feature-dashboard && npm run lint` で0 errors + `npm run format:check` でclean。Dashboard未変更のPhaseでもlint実行（他featureの依存更新で壊れる可能性）。warningは許容、errorは修正必須 | `npm run lint && npm run format:check` |
| **Push** | 全commitをリモートにpush | `git push` |

> **CRITICAL**: Post-Phase Review は該当 Phase file のセクションを必ず読み、設計と実装の整合性を確認すること。差異がある場合は該当ファイルを実装に合わせて更新する。

**Planning 必須タスク** (各 Phase 共通):

| タスク | 内容 | 検証方法 |
|--------|------|----------|
| **Push** | 全commitをリモートにpush | `git push` |

---

## Links

- [feature-341.md](../feature-341.md) - Architecture Research
- [feature-343.md](../feature-343.md) - This design's feature
- [content-roadmap.md](../content-roadmap.md) - Version roadmap
- [ntr-core-overview.md](../reference/ntr-core-overview.md) - NTR system reference
