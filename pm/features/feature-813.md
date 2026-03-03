# Feature 813: Post-Phase Review Phase 21

## Status: [PROPOSED]

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

## Type: infra

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity -- Post-Phase Review is the SSOT gate ensuring phase closure with zero outstanding obligations, verified DI composition, and reconciled architecture documentation before progressing to the next phase. Scope: Phase 21 Counter System (F783-F812, 13 predecessor features).

### Problem (Current Issue)

Phase 21's per-feature decomposition (F783) divided the Counter System by ERB file boundaries, enabling each feature (F801-F812) to register only its own immediate DI needs for unit testing. Because individual features could not register services depending on sibling interfaces not yet migrated, approximately 17-20 of ~30 Phase 21 Counter services remain unregistered in `AddEraCore()` (`ServiceCollectionExtensions.cs:186-201`). This cumulative DI gap is invisible at the unit test level (which mocks dependencies) but will cause cascading failures at the integration/E2E level required by CP-2 Step 2a. Additionally, 28+ deferred obligations from 8 predecessor features (F803-F808, F810-F811) accumulated cross-cutting concerns that span multiple ERB file boundaries, creating scope pressure that may trigger Redux decomposition. The architecture document (`phase-20-27-game-systems.md:111`) still shows Phase 21 Status as "TODO" despite all sub-features being [DONE].

### Goal (What to Achieve)

1. Register all Phase 21 services in `AddEraCore()` and verify full DI resolution (CP-2 Step 2a)
2. Resolve or properly track all 28+ deferred obligations from F803-F808, F810-F811
3. Establish E2E test foundation with Training-to-Counter cross-system flow
4. Reconcile architecture documentation (Phase 21 Status, Success Criteria)
5. Record Stryker.NET mutation testing baseline
6. Evaluate Redux trigger and create Redux DRAFT if threshold exceeded
7. Verify dashboard lint/format compliance

---

## Deferred Obligations

**N+4 --unit deprecation obligation** (DEFERRED from F782 -> F783 -> F813):
N+4 --unit deprecation: NOT_FEASIBLE -- trigger condition: C# migration functionally complete (kojo no longer requires ERB test runner; kojo testing pipeline dependency resolved). Tracking destination: this feature -- /fc時にdeprecation追跡タスクとして具体化する。

**F810 Mandatory Handoffs** (DEFERRED from F810 -> F813):
1. **IComableUtilities/ICounterUtilities method duplication consolidation**: 4 methods overlap between IComableUtilities (F809/F810) and ICounterUtilities (F801/F804): TimeProgress (int vs int), IsAirMaster (int vs CharacterId), GetTargetNum (no param vs CharacterId), MasterPose (int,int,int vs int,int returning CharacterId). Consolidation to eliminate parallel interfaces with incompatible signatures for the same ERB functions.

**F805 Mandatory Handoffs** (DEFERRED from F805 -> F813):
1. **DatuiMessage DRY extraction**: ERB DATUI_MESSAGE is a shared global function (Game/ERB/COUNTER_MESSAGE.ERB:484) used by both main counter and WC counter. F802 migrated it as `private void DatuiMessage` in CounterMessage.cs. F805 re-implements privately in WcCounterMessage.cs (unavoidable: F802's method is private and sealed). Post-phase, extract to shared `IDatuiMessageService` to eliminate duplication.

**F803 Mandatory Handoffs** (DEFERRED from F803 -> F813):
1. **ICharacterStringVariables VariableStore implementation**: CSTR is not stored in VariableStore today; connecting the interface to the engine VariableStore is outside F803 scope. F803 provides interface definition + stub only; runtime VariableStore extension requires engine-layer changes.
2. **EXP_UP logic duplication**: CheckExpUp exists as private in AbilityGrowthProcessor; F803 adds public method to ICounterUtilities interface, creating duplication. Extraction to shared implementation is a refactoring concern.
3. **ICounterSourceHandler ISP violation**: single interface exposes 3 responsibilities (dispatch via HandleCounterSource, undressing via DatUI helpers, pain check via PainCheckVMaster). F803 scope cannot extract IUndressingHandler without additional interface churn; ISP compliance requires separate interfaces per responsibility.
   - **F805 DI impact**: When ISP extracts IUndressingHandler from ICounterSourceHandler, F805 (Toilet Counter Source) must update its DI injection -- F805 consumes DatUI helpers via ICounterSourceHandler.
4. **CFlagIndex typed struct**: CFLAG dispatch branches use raw int private constants (CFLAG:MASTER:ローターA挿入, CFLAG:ARG:ぱんつ確認, etc.) instead of typed CFlagIndex. Cross-class reuse not yet needed; deferred per Key Decision.
5. **EquipIndex typed struct**: DATUI helper methods use raw int EQUIP constants (装備:17 レオタード, etc.) instead of typed EquipIndex. Cross-class reuse not yet needed; deferred per Key Decision.
6. **IShrinkageSystem runtime implementation**: F803 creates IShrinkageSystem interface (Era.Core/Counter/IShrinkageSystem.cs) with no engine-layer implementation; production calls use stub/no-op until implemented. Runtime 締り具合変動 logic requires engine-layer integration.

**F807 Mandatory Handoffs** (DEFERRED from F807 -> F813):
1. **WcCounterMessageTease行動テストカバレッジ**: F807 ACは全て静的コードチェック（Grepパターン、ビルド成功）。ハンドラ分岐、INPUTループパス、NTR revelation、EQUIP mutationの行動等価性テストが未検証。F807 Philosophyは「structural migration equivalence」に限定済み；行動等価性はF813スコープ内で検証予定だったが、F813タスク数超過（15タスク/19AC）のためF814に再延期。Mandatory Handoffs参照。
2. **IWcCounterMessageTeaseインターフェース抽出検討**: WcCounterMessageTeaseは具象型で注入（Key Decision Row 1）。WcCounterMessage.Dispatch()の単体テストに全11依存の実体が必要。選択肢: (A) IWcCounterMessageTease作成、(B) 恒久的技術的負債として許容。
3. **キャラクターID定数統合**: MESSAGE27の13個のprivate const（キャラクターID）がF806 SEXと重複の可能性。共有CharacterConstantsクラスが存在しない。統合追跡が必要。
4. **WcCounterMessageコンストラクタ肥大化対策**: F807後11パラメータ、F806追加で12パラメータ。パラメータオブジェクト化またはハンドラディスパッチサービスへの統合を検討。
5. **CFlag/Cflag命名規則正規化**: WcCounterMessage.cs（F805）はCFlag prefix、WcCounterMessageItem/Ntr（F808）はCflag prefix。F807はF808パターンに従うが、WcCounterMessage.cs内の既存CFlag定数は未修正。命名規則統一が必要。
6. **AC#34ローカル関数施行ギャップ追跡**: AC#34のgrepパターン `private (static )?int \w+\(` はC#ローカル関数（`private`キーワードなし）を検出不可。Technical Designはローカル関数を明示的に禁止するが、機械的AC強制が存在しない。WcCounterMessageTeaseでのローカル関数使用は未検証。F813でレビュー時に静的解析ツールまたは追加grepパターンでローカル関数ゼロを確認する必要がある。

**F806 Mandatory Handoffs** (DEFERRED from F806 -> F813):
1. **IEngineVariables GetTime/SetTime NuGet packaging**: Default interface methods added in F806 are source-only; if Era.Core NuGet is re-published, package version must be bumped.
2. **IEngineVariables GetTime/SetTime behavioral override**: Default bodies (=> 0 / {}) are no-ops; engine implementor must override for TIME access correctness. Without override, all 4 TIME += operations silently fail.
3. **WC_VA_FITTING documentation**: 9 kojo ERB files call WC_VA_FITTING; after C# migration, external callers must know to use IWcCounterMessageSex.VaFitting.
4. **KOJO 3-param overload verification**: F806 uses 2-param KojoMessageWcCounter for all 28 TRYCALLFORM patterns. Verify no pattern requires a 3rd parameter.
5. **WcCounterMessageSex responsibility review (16 deps)**: Evaluate if handler groups should be split into separate classes.
6. **WcCounterMessageSex duplicate constant names**: TalentVirginity2/TalentGender2 duplicate TalentVirginity/TalentGender; consolidation needed.
7. **Dispatch() dual offender convention unification**: SEX handlers (F806) use explicit `CharacterId offender` parameter; ITEM/NTR (F808) and TEASE (F807) use implicit `_engine.GetTarget()`. Dual convention persists in WcCounterMessage.Dispatch(). Unify to one pattern.
8. **NOITEM photography bug (ERB原文バグ)**: TOILET_COUNTER_MESSAGE_SEX.ERB line 411 uses `NOITEM != 0 &&` instead of `NOITEM != 0 ||`. Photography scene is dead code in normal gameplay (NOITEM==0). C# migration at WcCounterMessageSex.cs:1922 faithfully preserved bug. Fix requires changing guard clause logic.

**F808 Mandatory Handoffs** (DEFERRED from F808 -> F813):
1. **WcCounterMessageNtr責務分割**: ERBファイル `TOILET_COUNTER_MESSAGE_NTR.ERB` は「ペッティングとNTR関連」の寄せ集めだった。6メソッド中4つがNTR無関係（RotorOut=デバイス操作、OshikkoLooking=観察、WithFirstSex/WithPetting=性行為）。9依存コンストラクタはNTR管理クラスタ(2メソッド)のみが`_kojoMessage`,`_counterUtilities`,`_ntrUtility`を使用し、残り4メソッドは3-5依存で済む。分割推奨: IWcCounterMessageNtrObservation + IWcCounterMessageNtrRevelation。
2. **NtrReversalSource/NtrAgreeSource計算式乖離**: ERBは除算ベース動的スケーリング `(LOCAL:21/10)+10` 、C#は固定定数 `subReduction=50` 。等価性テストで検証が必要。

**F808 /fc教訓（プロセス改善）**:
Phase 21の分解（F783）はERBファイル単位で行い、/fcはその境界を無検証で踏襲した。結果、ERBの「残り物寄せ集めファイル」がそのままC#クラス境界になった。今後の/fcでは「ERBファイル境界 ≠ ドメイン境界」の検証ステップが必要。

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does CP-2 Step 2a (DI full resolution) face failure risk? | Only ~10 of ~30 Phase 21 Counter services are registered in AddEraCore(); ~17-20 registrations are missing | `ServiceCollectionExtensions.cs:186-201` -- only F805/F806/F807/F808/F812 labels present |
| 2 | Why are so many services unregistered? | Each Phase 21 feature (F801-F812) registered only its own immediate DI needs for unit test isolation | DI comments show per-feature labels; F801/F802/F803/F804/F809/F810/F811 registrations absent |
| 3 | Why did features register only their own needs? | Individual features could not register services depending on sibling interfaces not yet migrated (circular dependency during parallel development) | Stubs created as test doubles (IShrinkageSystem, ICounterUtilities, IComableUtilities) instead of DI-registerable services |
| 4 | Why was there no consolidated DI registration task? | F783 planning decomposed Phase 21 by ERB file boundaries, not by domain/DI boundaries | F783 scope definition used file-prefix grouping without call-chain analysis |
| 5 | Why (Root)? | Phase 21's exceptional size (25,862 lines, 6x Phase 20, 30 files) required per-feature DI silos for parallel development, but no cross-system integration plan was established to consolidate them post-completion | `phase-20-27-game-systems.md:111` -- Phase 21 Status still "TODO"; no integration checkpoint defined |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | ~17-20 Phase 21 Counter interfaces unregistered in AddEraCore(); 28+ deferred obligations accumulated | Per-feature DI pattern created isolated silos without cross-system integration plan; F783 decomposed by ERB file boundaries not domain boundaries |
| Where | `ServiceCollectionExtensions.cs:186-201` (missing registrations); predecessor Mandatory Handoffs sections | F783 Phase 21 Planning scope definition; per-feature unit test isolation strategy |
| Fix | Register each missing service individually | Systematic DI resolution with E2E validation, obligation triage with Redux evaluation, architecture doc reconciliation |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning -- decomposition origin, Mandatory Handoff source |
| F801 | [DONE] | Main Counter Core -- DI registrations missing |
| F802 | [DONE] | Main Counter Output -- DatuiMessage duplication source |
| F803 | [DONE] | Main Counter Source -- ISP violation, ITouchSet, IShrinkageSystem |
| F804 | [DONE] | WC Counter Core -- DI registrations missing |
| F805 | [DONE] | WC Counter Source + Message Core -- DatuiMessage duplication, CFlag naming |
| F806 | [DONE] | WC Counter Message SEX -- 8 deferred handoffs, 16 deps review |
| F807 | [DONE] | WC Counter Message TEASE -- 6 deferred handoffs, constructor bloat |
| F808 | [DONE] | WC Counter Message ITEM + NTR -- NtrReversal calc divergence, responsibility split |
| F809 | [DONE] | COMABLE Core -- IComableUtilities overlap |
| F810 | [DONE] | COMABLE Extended -- MasterPose duplication handoff |
| F811 | [DONE] | SOURCE Entry System -- ITouchStateManager canonical, MasterPose adapter |
| F812 | [DONE] | SOURCE1 Extended -- DI registrations present |
| F814 | [DRAFT] | Phase 22 Planning -- successor, BLOCKED on F813 |
| F782 | [DONE] | Phase 20 Planning -- N+4 --unit deprecation origin (F782->F783->F813 chain) |
| F815 | [DONE] | Golden Test Design -- E2E fallback if DI resolution fails |
| F816 | [DONE] | StubVariableStore -- related test infrastructure |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| DI registration gap resolvable | FEASIBLE | ~17-20 services need registration; mechanical but well-understood task |
| E2E test foundation buildable | FEASIBLE | CP-2 Step 2a design exists; F815 fallback documented for failure case |
| Deferred obligations tractable | FEASIBLE | 28+ items categorized; Redux pattern available for overflow |
| Architecture doc reconciliation | FEASIBLE | Phase 21 Status update is a documentation task |
| IShrinkageSystem no-implementation risk | FEASIBLE | Stub/no-op registration acceptable for DI resolution; runtime implementation is engine-layer scope |
| Scope within single feature | NEEDS_REVISION | 13 Tasks and 28+ obligations exceed typical infra feature range; Redux likely needed |

**Verdict**: FEASIBLE -- Feature is achievable but scope pressure is high. Redux evaluation (architecture doc threshold) is a mandatory task within this feature. If obligations exceed threshold, Redux DRAFT creation is part of the deliverable.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| DI Composition Root | HIGH | ~17-20 new registrations in ServiceCollectionExtensions.cs; cascading resolution required |
| E2E Test Infrastructure | HIGH | New E2E directory and test foundation; first cross-system integration tests |
| Architecture Documentation | MEDIUM | Phase 21 Status update, Success Criteria reconciliation |
| Interface Consolidation | MEDIUM | MasterPose 3-way merge, ITouchSet/ITouchStateManager consolidation, DatuiMessage extraction |
| Code Quality | MEDIUM | CFlag/Cflag naming normalization, duplicate constant removal, NoWarn debt reduction |
| Phase 22 Pipeline | HIGH | F814 is BLOCKED until F813 completes; pipeline continuity depends on this feature |
| NTR Subsystem | MEDIUM | WcCounterMessageNtr responsibility split; NtrReversalSource calculation verification |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| ~17-20 Counter interfaces unregistered in AddEraCore() | `ServiceCollectionExtensions.cs:186-201` | CP-2 Step 2a will fail until all services registered |
| MasterPose 3-way signature divergence | `ICounterUtilities.cs:42`, `IComableUtilities.cs:15`, `ITouchStateManager.cs:36` | Consolidation requires adapter pattern to maintain backward compatibility |
| ITouchSet/ITouchStateManager dual interfaces coexist | 26 references across codebase | Mechanical but large migration with parameter mapping |
| IShrinkageSystem has no concrete implementation | `IShrinkageSystem.cs:5-12` | Must register stub/no-op for DI resolution; runtime implementation is engine scope |
| NtrReversalSource uses fixed constants vs ERB dynamic scaling | `WcCounterMessageNtr.cs:520-555` | Must verify against ERB source; may require calculation fix |
| WcCounterMessageSex has 16 constructor dependencies | `WcCounterMessageSex.cs:201-217` | Responsibility review needed to evaluate split |
| WcCounterMessage has 12 constructor parameters | `WcCounterMessage.cs:41-53` | Parameter object or handler dispatch consolidation needed |
| N+4 --unit deprecation NOT_FEASIBLE | `feature-813.md` Deferred Obligations | Must explicitly re-defer with concrete tracking |
| Cross-repo changes required | Era.Core (core repo) + devkit | Two-repo coordination for DI and test changes |
| IEngineVariables GetTime/SetTime are no-ops | Default interface methods | TIME operations silently fail without engine override |
| NullEngineVariables GetTime/SetTime no-ops | `IEngineVariables` default bodies | E2E tests must account for TIME operation silent failures |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| CP-2 DI resolution cascading failures | MEDIUM | HIGH | Register incrementally; DI-resolve-all test first; F815 Golden Test fallback |
| Scope exceeds single-feature capacity (Redux trigger) | HIGH | MEDIUM | Pre-categorize obligations; Redux DRAFT creation is a planned deliverable |
| MasterPose consolidation breaks existing tests | HIGH | HIGH | Adapter pattern maintains backward compatibility across all 3 signatures |
| NtrReversalSource divergence is intentional simplification vs bug | MEDIUM | MEDIUM | ERB source comparison required before modifying |
| IShrinkageSystem stub registration masks missing implementation | LOW | MEDIUM | Document as engine-layer scope; track in successor feature |
| ITouchSet consolidation breaks F803 consumers | MEDIUM | HIGH | Parameter mapping from F811 Mandatory Handoffs |
| NoWarn CA1510 removal triggers unexpected warnings | LOW | HIGH | Auto-fix with dotnet format; revert if build fails |
| ICounterSourceHandler ISP split breaks F805 DI injection | MEDIUM | MEDIUM | Plan adapter; coordinate with F805 DI changes |
| Task count (13+) causes scope creep during implementation | HIGH | MEDIUM | Group related tasks; strict scope discipline |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| DI registered services (Phase 21) | Grep count in ServiceCollectionExtensions.cs | ~10 | Pre-resolution count; target is ~30 |
| E2E test count | Glob `Era.Core.Tests/E2E/**/*Tests.cs` | 0 | Directory does not exist yet |
| NoWarn suppressed rules | Grep count in Directory.Build.props | ~21-24 | CA1510 is target for removal |
| Stryker mutation score | `dotnet stryker` in Era.Core.Tests | Not yet recorded | First baseline; no regression comparison |
| Dashboard lint errors | `npm run lint` in feature-dashboard | Not yet recorded | Target: 0 errors |

**Baseline File**: `_out/tmp/baseline-813.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | DI must cover Phase 5-21 ALL services without exception | CP-2 Step 2a, `full-csharp-architecture.md` | AC must verify BuildServiceProvider() resolves all registered services |
| C2 | Phase 21 architecture doc must be reconciled | `phase-20-27-game-systems.md:111` | AC must verify Status updated from "TODO" to completion state |
| C3 | MasterPose consolidation must maintain backward compatibility | F811 Mandatory Handoffs, 3 interface files | AC must verify all 3 original call patterns still work via adapters |
| C4 | Zero remaining untracked technical debt | architecture doc, Scope Discipline | AC must grep TODO/FIXME/HACK and verify count or track in Redux |
| C5 | Redux trigger evaluation is mandatory | `full-csharp-architecture.md` Redux Pattern | AC must verify obligation count evaluation and Redux DRAFT if threshold exceeded |
| C6 | N+4 --unit NOT_FEASIBLE must be explicitly tracked | Deferred Obligations chain F782->F783->F813 | AC must verify NOT_FEASIBLE documentation with concrete re-deferral destination |
| C7 | Stryker baseline is FIRST measurement (no regression comparison) | architecture doc CP-2 | Record mutation score only; no pass/fail threshold |
| C8 | E2E must use seeded IRandomProvider for determinism | architecture doc CP-2 Step 2a | Deterministic execution verification in cross-system flow |
| C9 | Dashboard lint must have zero errors | Post-Phase Review checklist | npm run lint exit 0; warnings permitted |
| C10 | Stubs acceptable for DI registration where no implementation exists | IShrinkageSystem evidence | DI resolution test must pass even with stub/no-op implementations |
| C11 | Cross-repo scope: core repo changes via NuGet | Architecture, Era.Core NuGet | ACs must specify which repo (core vs devkit) for file-level checks |
| C12 | E2E tests are permanent (immutable test policy) | CLAUDE.md design principles | E2E test files must not be deleted after creation |

### Constraint Details

**C1: DI Full Resolution**
- **Source**: CP-2 Step 2a in `full-csharp-architecture.md`; investigation confirmed ~17-20 missing registrations in `ServiceCollectionExtensions.cs:186-201`
- **Verification**: `BuildServiceProvider()` call in test must resolve all Phase 5-21 services without throwing
- **AC Impact**: AC must include DI resolution test that instantiates all registered services; failure triggers F815 fallback path

**C2: Architecture Doc Reconciliation**
- **Source**: `phase-20-27-game-systems.md:111` shows Phase 21 Status "TODO" despite all F801-F812 being [DONE]
- **Verification**: Grep Phase 21 section for updated Status value
- **AC Impact**: AC must verify both Status field and Success Criteria checkboxes are updated

**C3: MasterPose Backward Compatibility**
- **Source**: 3-way signature divergence across `ICounterUtilities.cs:42` (int,int)->CharacterId, `IComableUtilities.cs:15` (int,int,int)->int, `ITouchStateManager.cs:36` (int,int,bool)->int
- **Verification**: All existing call sites continue to compile and pass tests
- **AC Impact**: AC must verify adapter wiring and existing test pass-through

**C5: Redux Trigger Evaluation**
- **Source**: `full-csharp-architecture.md` Redux Pattern definition
- **Verification**: Count unresolved obligations; compare to Redux threshold
- **AC Impact**: AC must verify evaluation was performed and result documented (either "below threshold" or Redux DRAFT created)

**C7: Stryker Baseline**
- **Source**: CP-2 Step 2a baseline measurement requirement
- **Verification**: Stryker output recorded in progress log
- **AC Impact**: AC must verify mutation score is recorded but NOT set a pass/fail threshold (first measurement)

**C10: Stub DI Registration**
- **Source**: IShrinkageSystem.cs has interface-only definition with no concrete implementation anywhere in codebase
- **Verification**: DI resolution test passes with stub/no-op registered for IShrinkageSystem
- **AC Impact**: AC must allow stub registrations; runtime implementation is engine-layer scope tracked in successor

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (Mandatory Handoff origin, decomposition source) |
| Predecessor | F801 | [DONE] | Main Counter Core -- DI registrations missing |
| Predecessor | F802 | [DONE] | Main Counter Output -- DatuiMessage duplication source |
| Predecessor | F803 | [DONE] | Main Counter Source -- ISP violation, ITouchSet, IShrinkageSystem |
| Predecessor | F804 | [DONE] | WC Counter Core -- DI registrations missing |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- DatuiMessage, CFlag naming |
| Predecessor | F806 | [DONE] | WC Counter Message SEX -- 8 deferred handoffs, 16 deps |
| Predecessor | F807 | [DONE] | WC Counter Message TEASE -- 6 deferred handoffs, constructor bloat |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR -- NtrReversal divergence, responsibility split |
| Predecessor | F809 | [DONE] | COMABLE Core -- IComableUtilities overlap with ICounterUtilities |
| Predecessor | F810 | [DONE] | COMABLE Extended -- MasterPose duplication handoff |
| Predecessor | F811 | [DONE] | SOURCE Entry System -- ITouchStateManager canonical, MasterPose adapter specs |
| Predecessor | F812 | [DONE] | SOURCE1 Extended |
| Successor | F814 | [DRAFT] | Phase 22 Planning -- BLOCKED on F813 completion |
| Related | F816 | [DONE] | StubVariableStore -- test infrastructure reference |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Post-Phase Review is the SSOT gate ensuring phase closure with zero outstanding obligations" | All deferred obligations must be resolved or explicitly tracked with concrete destination | AC#5, AC#6, AC#9, AC#11, AC#12, AC#17, AC#20, AC#26, AC#28 |
| "verified DI composition" | All Phase 5-21 services must resolve via BuildServiceProvider() without exceptions | AC#1, AC#25, AC#29, AC#30 |
| "reconciled architecture documentation before progressing to the next phase" | Phase 21 Status and Success Criteria must be updated from TODO | AC#7, AC#16 |
| "Pipeline Continuity" | E2E foundation, Stryker baseline, and dashboard compliance must be verified before phase progression | AC#3, AC#4, AC#8, AC#10, AC#21, AC#23 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DI all-resolve test exists and passes | test | dotnet test --filter "FullyQualifiedName~DiResolutionTests" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 2 | ICounterUtilities MasterPose removed after consolidation | code | Grep(path="C:/Era/core/src/Era.Core/") | not_contains | "MasterPose(int poseType, int poseSlot)" | [ ] |
| 3 | E2E test directory created | file | Glob("C:/Era/core/src/Era.Core.Tests/E2E/*Tests.cs") | exists | - | [ ] |
| 4 | Dashboard lint zero errors | exit_code | npm run lint --prefix src/tools/node/feature-dashboard | succeeds | - | [ ] |
| 5 | N+4 --unit NOT_FEASIBLE re-deferred with concrete destination | code | Grep(path="pm/features/feature-813.md") | matches | "NOT_FEASIBLE.*destination.*F\\d+" | [ ] |
| 6 | Redux trigger evaluation result documented in Execution Log | code | Grep(path="pm/features/feature-813.md") | matches | "Redux evaluation result: (below threshold|Redux DRAFT created as F\d+)" | [ ] |
| 7 | Phase 21 architecture status reconciled (at least 2 phases DONE) | code | Grep(path="docs/architecture/migration/phase-20-27-game-systems.md", pattern="Phase Status.*DONE") | gte | 2 | [ ] |
| 8 | Stryker baseline recorded in execution log | code | Grep(path="pm/features/feature-813.md") | matches | "mutation score.*\\d+\\.\\d+%" | [ ] |
| 9 | ITouchSet interface removed after consolidation | code | Grep(path="C:/Era/core/src/Era.Core/") | not_contains | "interface ITouchSet" | [ ] |
| 10 | Build succeeds after all changes | build | dotnet build | succeeds | - | [ ] |
| 11 | DatuiMessage extracted to shared service | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "IDatuiMessageService" | [ ] |
| 12 | WcCounterMessageNtr split into observation and revelation | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "IWcCounterMessageNtrObservation" | [ ] |
| 13 | NtrReversalSource equivalence verification result documented | code | Grep(path="pm/features/feature-813.md") | matches | "NtrReversalSource verification result: (INTENTIONAL|REGRESSION)" | [ ] |
| 14 | IComableUtilities MasterPose removed after consolidation | code | Grep(path="C:/Era/core/src/Era.Core/") | not_contains | "MasterPose(int pose, int arg1, int arg2)" | [ ] |
| 15 | NtrAgreeSource equivalence verification result documented | code | Grep(path="pm/features/feature-813.md") | matches | "NtrAgreeSource verification result: (INTENTIONAL|REGRESSION)" | [ ] |
| 16 | Phase 21 CP-2 Step 2a Success Criterion checked | code | Grep(path="docs/architecture/migration/phase-20-27-game-systems.md") | matches | "\\[x\\].*CP-2 Step 2a PASS" | [ ] |
| 17 | WcCounterMessageNtr Revelation interface created | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "IWcCounterMessageNtrRevelation" | [ ] |
| 18 | E2E test class contains at least one test method | code | Grep(path="C:/Era/core/src/Era.Core.Tests/E2E/") | contains | "[Fact]" | [ ] |
| 19 | E2E test uses seeded IRandomProvider for determinism | code | Grep(path="C:/Era/core/src/Era.Core.Tests/E2E/") | contains | "IRandomProvider" | [ ] |
| 20 | Original IWcCounterMessageNtr interface removed after split | code | Grep(path="C:/Era/core/src/Era.Core/") | not_matches | "interface IWcCounterMessageNtr[^A-Z]" | [ ] |
| 21 | Dashboard format check clean | exit_code | npm run format:check --prefix src/tools/node/feature-dashboard | succeeds | - | [ ] |
| 22 | Canonical MasterPose preserved in ITouchStateManager | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "MasterPose(int targetPart, int masterPart" | [ ] |
| 23 | Cross-system flow E2E test passes | test | dotnet test --filter "FullyQualifiedName~CrossSystemFlow" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 24 | All existing Era.Core unit tests pass after refactoring | test | dotnet test C:/Era/core/src/Era.Core.Tests --blame-hang-timeout 10s | succeeds | - | [ ] |
| 25 | Phase 5-21 DI registrations present in AddEraCore | code | Grep(path="C:/Era/core/src/Era.Core/", pattern="services\\.Add(Singleton|Transient|Scoped)") | gte | 25 | [ ] |
| 26 | TODO/FIXME/HACK audit in Phase 21 code documented | code | Grep(path="pm/features/feature-813.md") | matches | "TODO/FIXME/HACK audit: \\d+ found, 0 untracked" | [ ] |
| 27 | CA1510 NoWarn removal outcome documented | code | Grep(path="pm/features/feature-813.md") | matches | "CA1510 removal: (REMOVED|DEFERRED to F\\d+)" | [ ] |
| 28 | Deferred obligation transfer count documented | code | Grep(path="pm/features/feature-813.md") | matches | "Transferred \\d+ obligations to F814" | [ ] |
| 29 | CounterCombinationAdapter exists for DI adapter wiring | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "CounterCombinationAdapter" | [ ] |
| 30 | WcCounterCombinationAdapter exists for DI adapter wiring | code | Grep(path="C:/Era/core/src/Era.Core/") | contains | "WcCounterCombinationAdapter" | [ ] |

### AC Details

**AC#7: Phase 21 architecture status reconciled (at least 2 phases DONE)**
- **Test**: `Grep(path="docs/architecture/migration/phase-20-27-game-systems.md", pattern="Phase Status.*DONE")`
- **Expected**: `>= 2`
- **Derivation**: Pre-implementation baseline has 1 phase DONE (Phase 20). After Phase 21 reconciliation, at least 2 phases must show DONE status.
- **Rationale**: Phase 21 Status is currently "TODO" despite all sub-features F801-F812 being [DONE]. Architecture doc must reflect completion.
- **Complementary**: AC#16 provides Phase-21-specific verification (`[x].*CP-2 Step 2a PASS` is unique to Phase 21 section). AC#7 (count) + AC#16 (Phase-21-specific) together satisfy C2 constraint.

**AC#6: Redux trigger evaluation (conditional path)**
- **Conditional**: If the documented result is `Redux DRAFT created as F{N}`, the implementer MUST verify: (a) `pm/features/feature-{N}.md` exists with `[DRAFT]` status, AND (b) `pm/index-features.md` includes the new feature entry. The AC#6 matcher only verifies the Execution Log pattern; DRAFT file existence is verified procedurally during Task 7.
- **Rationale**: Goal #6 requires "create Redux DRAFT if threshold exceeded." The AC only verifies documentation; file creation verification is an Implementation Contract obligation.

**AC#13/AC#15: NtrReversalSource/NtrAgreeSource verification (conditional path)**
- **Conditional**: If the documented result is `REGRESSION`, the implementer MUST either: (a) apply the fix in the same Task 14 session (AC#24 verifies all existing tests pass after fix), OR (b) document the deferral reason in the Mandatory Handoffs row "NtrReversalSource/NtrAgreeSource REGRESSION fix if found" Result column with concrete rationale.
- **Rationale**: Technical Design Work Area 6 states "If it is a regression bug, apply fix." AC#13/AC#15 alone only verify documentation of the finding, not resolution. The conditional path ensures REGRESSION findings are not silently ignored.

**AC#28: Deferred obligation transfer count**
- **Expected minimum**: 20 (Mandatory Handoffs table has 22+ rows with Destination=F814 at spec completion)
- **Verification**: The implementer MUST count Mandatory Handoffs rows with Destination=F814 and write the exact count in the Execution Log entry. The `\d+` matcher verifies format; the Implementation Contract "Redux evaluation mandatory" row + AC#28 description enforce count accuracy.
- **Rationale**: Defense against documenting an incorrect/incomplete count that would satisfy the regex pattern.

**AC#25: Phase 5-21 DI registrations present in AddEraCore**
- **Test**: `Grep(path="C:/Era/core/src/Era.Core/", pattern="services\\.Add(Singleton|Transient|Scoped)")`
- **Expected**: `>= 25`
- **Derivation**: Baseline is ~10 registrations (Phase 5-20 only). After Task 5 adds ~21 Phase 21 registrations, total should be ~30. Threshold of 25 accounts for adapter pattern variations.
- **Rationale**: Defense-in-depth alongside AC#1 (DiResolutionTests). Prevents vacuous test pass if DiResolutionTests covers only a subset of services.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Register all Phase 21 services in AddEraCore() and verify full DI resolution | AC#1, AC#25, AC#29, AC#30 |
| 2 | Resolve or properly track all 28+ deferred obligations | AC#2, AC#5, AC#6, AC#9, AC#10, AC#11, AC#12, AC#13, AC#14, AC#15, AC#17, AC#20, AC#22, AC#24, AC#26, AC#27, AC#28 |
| 3 | Establish E2E test foundation with Training-to-Counter cross-system flow | AC#3, AC#18, AC#19, AC#23 |
| 4 | Reconcile architecture documentation | AC#7, AC#16 |
| 5 | Record Stryker.NET mutation testing baseline | AC#8 |
| 6 | Evaluate Redux trigger and create Redux DRAFT if threshold exceeded | AC#6 |
| 7 | Verify dashboard lint/format compliance | AC#4, AC#21 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Work Area 1: MasterPose Consolidation (AC#2, Task 1)**

Three interfaces each declare a divergent `MasterPose` signature:
- `ICounterUtilities.MasterPose(int poseType, int poseSlot) -> CharacterId` (F804)
- `IComableUtilities.MasterPose(int pose, int arg1, int arg2) -> int` (F809)
- `ITouchStateManager.MasterPose(int targetPart, int masterPart, bool prevTurn = false) -> int` (F811, canonical)

Strategy: Remove `MasterPose` from `ICounterUtilities` and `IComableUtilities`, keeping only `ITouchStateManager.MasterPose` as the canonical. All call sites in `CounterReaction.cs`, `WcCounterReaction.cs`, and `StubComableUtilities.cs` must redirect to `ITouchStateManager.MasterPose`. Two not_contains ACs verify removal: AC#2 checks that `MasterPose(int poseType, int poseSlot)` (ICounterUtilities 2-param signature) is absent; AC#14 checks that `MasterPose(int pose, int arg1, int arg2)` (IComableUtilities 3-param signature) is absent. `ITouchStateManager.MasterPose(int targetPart, int masterPart, bool prevTurn = false)` does not match either banned pattern so it passes the `not_contains` ACs.

**Work Area 2: ITouchSet/ITouchStateManager Consolidation (AC#9, Tasks 3-4)**

`ITouchSet` (3-param: `void TouchSet(int mode, int type, CharacterId target)`) and `ITouchStateManager` (4-param: `void TouchSet(int targetPart, int masterPart, int character, bool reset = false)`) coexist. Only `CounterSourceHandler.cs` still consumes `ITouchSet`. Migration: update `CounterSourceHandler.cs` to inject `ITouchStateManager` instead of `ITouchSet`; map parameters `mode->targetPart, type->masterPart, target.Value->character, reset=false`. After migration, delete `ITouchSet.cs`. AC verifies `"interface ITouchSet"` is absent from the codebase.

**Work Area 3: DI All-Resolve (AC#1, AC#3, Task 2, Task 5, Task 13)**

Identified 21 Counter interfaces missing from `ServiceCollectionExtensions.cs`. Registration strategy:
- **Has concrete impl**: `ICounterSystem` (ActionSelector), `IWcCounterSystem` (WcActionSelector), `ICounterSourceHandler` (CounterSourceHandler), `ICounterOutputHandler` (CounterOutputHandler), `IActionSelector` (ActionSelector), `IActionValidator` (ActionValidator), `IComableUtilities` (StubComableUtilities), `ISourceSystem` (SourceEntrySystem), `ITouchStateManager` (TouchStateManager), `IComAvailabilityChecker` (ComableChecker or GlobalComableFilter)
- **Needs stub class**: `ICounterUtilities`, `IWcSexHaraService`, `INtrUtilityService`, `IShrinkageSystem`, `ITrainingCheckService`, `IKnickersSystem`, `IEjaculationProcessor`, `IKojoMessageService`
- **Factory/keyed pattern**: `IComHandler` (strategy pattern — no single concrete class; register NullComHandler as default per Upstream Issues)
- **Adapter wiring per F811 spec**: `ICombinationCounter -> CounterCombination`, `IWcCombinationCounter -> WcCounterCombination` (Task 2 from the Tasks table)

For interfaces with no concrete implementation (`IShrinkageSystem`, `ICounterUtilities`, `IWcSexHaraService`, `INtrUtilityService`, `ITrainingCheckService`, `IKnickersSystem`, `IEjaculationProcessor`, `IKojoMessageService`): create `Null`-prefixed or `Stub`-prefixed no-op classes in Era.Core (following the `StubComableUtilities` pattern). These are registered in `AddEraCore()` so `BuildServiceProvider().GetRequiredService<T>()` does not throw.

E2E test: create `src/Era.Core.Tests/E2E/` directory. Add `DiResolutionTests.cs` which calls `services.AddEraCore()` then `provider.GetRequiredService<T>()` for each Phase 5-21 interface. This test verifies AC#1 (succeeds) and AC#3 (file exists). The F813 Tasks table Task 13 also specifies a Training-to-Counter cross-system flow test using seeded `IRandomProvider`. If DI resolution succeeds, add a single cross-system smoke test. If it fails mid-way, create F815 Golden Test Design feature per the fallback instruction.

**Work Area 4: DatuiMessage Extraction (AC#11, Task 12)**

`DatuiMessage` is private in both `CounterMessage.cs` (F802) and `WcCounterMessage.cs` (F805). Strategy: create `IDatuiMessageService` interface in `Era.Core/Counter/` with a single method `void SendDatuiMessage(CharacterId offender)`. Create `DatuiMessageService.cs` concrete class. Update `CounterMessage.cs` and `WcCounterMessage.cs` to inject `IDatuiMessageService` and delegate their private `DatuiMessage` calls to it. Register in `AddEraCore()`. AC verifies `"IDatuiMessageService"` exists in the codebase.

**Work Area 5: WcCounterMessageNtr Split (AC#12, AC#17, AC#20, Task 13)**

`IWcCounterMessageNtr` has 6 methods across 2 responsibility domains:
- **Observation group** (4 methods): `RotorOut`, `OshikkoLooking`, `WithFirstSex`, `WithPetting` — no `_kojoMessage`, `_counterUtilities`, `_ntrUtility` dependencies
- **Revelation group** (2 methods): `NtrRevelation`, `NtrRevelationAttack` — full NTR dependency cluster

Create `IWcCounterMessageNtrObservation` (4 methods) and `IWcCounterMessageNtrRevelation` (2 methods). The existing `WcCounterMessageNtr` class can implement both interfaces (or split into two classes). Update callers (`WcCounterMessage.Dispatch()` etc.) to inject by new interface. Register both in DI. Remove the original `IWcCounterMessageNtr` interface after splitting into Observation and Revelation. Update all callers to inject `IWcCounterMessageNtrObservation` or `IWcCounterMessageNtrRevelation` as appropriate. AC#20 verifies removal. AC verifies `"IWcCounterMessageNtrObservation"` exists in codebase.

**Work Area 6: NtrReversalSource/NtrAgreeSource Verification (AC#13, AC#15, Task 14 [I])**

Compare `WcCounterMessageNtr.NtrReversalSource()` (C#: fixed constants 50/100/200/300/500) against `TOILET_COUNTER_MESSAGE_NTR.ERB:960-1034` (ERB: dynamic scaling `(delta/10)+10`). Investigation task: read both implementations, determine if the divergence is intentional simplification or regression bug. Document the finding as `"NtrReversalSource verification result: INTENTIONAL"` or `"NtrReversalSource verification result: REGRESSION"` in the Execution Log. If it is a regression bug, apply fix. AC#13 matches `"NtrReversalSource verification result: (INTENTIONAL|REGRESSION)"` in the feature file.

Similarly, compare `WcCounterMessageNtr.NtrAgreeSource()` (C#) against `TOILET_COUNTER_MESSAGE_NTR.ERB` (ERB). The NtrAgreeSource method is in the same vicinity as NtrReversalSource in both C# (`WcCounterMessageNtr.cs`) and ERB (`TOILET_COUNTER_MESSAGE_NTR.ERB`). Apply the same INTENTIONAL/REGRESSION determination. Document as `"NtrAgreeSource verification result: INTENTIONAL"` or `"NtrAgreeSource verification result: REGRESSION"`. AC#15 matches in the feature file.

**Work Area 7: Deferred Obligation Tracking (AC#5, AC#6)**

N+4 --unit NOT_FEASIBLE: add a text block to this feature file's Execution Log stating the NOT_FEASIBLE status and its concrete destination feature ID (e.g., F{N}). This satisfies `"NOT_FEASIBLE.*destination.*F\d+"`. Redux trigger evaluation: after all obligations are categorized (resolved/re-deferred/tracked), document `"Redux evaluation result: [below threshold / Redux DRAFT created as F{N}]"` in the Execution Log.

**Work Area 8: Architecture Doc Reconciliation (AC#7)**

Update `docs/architecture/migration/phase-20-27-game-systems.md`: change Phase 21 Status from `"TODO"` to `"DONE"`. Phase 20 is already `"DONE"` (confirmed by grep). After update, at least 2 phases show `"Phase Status: DONE"` in the file. AC uses `gte 2` matcher on `Grep(pattern="Phase Status.*DONE")`.

**Work Area 9: Stryker Baseline (AC#8, Task 9 [I])**

Run `dotnet stryker` in `src/Era.Core.Tests/` (WSL). Record the mutation score (killed%, survived%, total mutants) in the Execution Log as `"mutation score: XX.XX% (killed N/total)"`. AC matches `"mutation score.*\d+\.\d+%"`.

**Work Area 10: Dashboard Lint (AC#4, Task 9)**

Run `npm run lint --prefix src/tools/node/feature-dashboard`. Exit code 0 = PASS. Fix any errors found (warnings are acceptable). AC verifies the command succeeds.

**Work Area 11: Build Verification (AC#10, Task 15)**

After all C# changes: run `dotnet build devkit.sln` (via WSL). TreatWarningsAsErrors=true is active. Also attempt NoWarn CA1510 removal per Task 8; revert if build fails and defer to next phase.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core.Tests/E2E/DiResolutionTests.cs` that calls `AddEraCore()` and resolves all Phase 5-21 services via `GetRequiredService<T>()`. Test must pass with `dotnet test --filter "FullyQualifiedName~E2E"`. |
| 2 | Remove `MasterPose(int pose, int arg1, int arg2)` from `IComableUtilities` and `MasterPose(int poseType, int poseSlot)` from `ICounterUtilities`. Grep of core/src/Era.Core/ must find zero matches for `"MasterPose(int poseType, int poseSlot)"` (the ICounterUtilities signature to be removed). |
| 3 | Create `src/Era.Core.Tests/E2E/` directory with at least one `*Tests.cs` file. Glob must find the file. |
| 4 | Run `npm run lint --prefix src/tools/node/feature-dashboard`. Fix any errors so exit code = 0. |
| 5 | Add text to Execution Log: `"N+4 --unit NOT_FEASIBLE: destination F{ID}"` (concrete feature ID). Grep of feature-813.md matches `"NOT_FEASIBLE.*destination.*F\d+"`. |
| 6 | Add text to Execution Log: `"Redux evaluation result: below threshold"` or `"Redux evaluation result: Redux DRAFT created as F{ID}"`. Grep of feature-813.md matches `"Redux evaluation result: (below threshold|Redux DRAFT created as F\d+)"`. |
| 7 | Update `phase-20-27-game-systems.md` Phase 21 Status from `TODO` to `DONE`. After update, at least 2 occurrences of `"Phase Status.*DONE"` exist in the file (Phase 20 + Phase 21). |
| 8 | Run `dotnet stryker` in `src/Era.Core.Tests/`. Record `"mutation score: XX.XX%"` in Execution Log. Grep of feature-813.md matches `"mutation score.*\d+\.\d+%"`. |
| 9 | Migrate `CounterSourceHandler.cs` from `ITouchSet` to `ITouchStateManager`. Delete `ITouchSet.cs`. Grep of core/src/Era.Core/ must find zero matches for `"interface ITouchSet"`. |
| 10 | After all C# changes, `dotnet build devkit.sln` (WSL) must exit 0 with zero errors. |
| 11 | Create `IDatuiMessageService.cs` interface and `DatuiMessageService.cs` implementation in `Era.Core/Counter/`. Update `CounterMessage.cs` and `WcCounterMessage.cs` to use it. Register in DI. Grep finds `"IDatuiMessageService"`. |
| 12 | Create `IWcCounterMessageNtrObservation.cs` interface in `Era.Core/Counter/`. Update `WcCounterMessageNtr.cs` to implement it. Register in DI. Grep finds `"IWcCounterMessageNtrObservation"`. |
| 13 | Compare `NtrReversalSource()` C# vs ERB. Write result as `"NtrReversalSource verification result: INTENTIONAL"` or `"NtrReversalSource verification result: REGRESSION"` in Execution Log. Grep of feature-813.md matches `"NtrReversalSource verification result: (INTENTIONAL\|REGRESSION)"`. |
| 14 | Grep of core/src/Era.Core/ must find zero matches for `"MasterPose(int pose, int arg1, int arg2)"` (the IComableUtilities signature to be removed). Verified alongside AC#2. |
| 15 | Compare `NtrAgreeSource()` C# vs ERB. Write result as `"NtrAgreeSource verification result: INTENTIONAL"` or `"NtrAgreeSource verification result: REGRESSION"` in Execution Log. Grep of feature-813.md matches `"NtrAgreeSource verification result: (INTENTIONAL\|REGRESSION)"`. |
| 16 | After Phase 21 architecture doc reconciliation, the CP-2 Step 2a Success Criterion checkbox must be checked. Grep of phase-20-27-game-systems.md must match `"\\[x\\].*CP-2 Step 2a PASS"`. This pattern is unique to Phase 21. |
| 17 | Create `IWcCounterMessageNtrRevelation` interface in `Era.Core/Counter/`. Update `WcCounterMessageNtr.cs` to implement it. Grep finds `"IWcCounterMessageNtrRevelation"`. |
| 18 | E2E test file must contain at least one `[Fact]` attribute to prevent AC#1 vacuous pass. Grep of E2E directory must match `"\\[Fact\\]"`. |
| 19 | E2E test must reference `IRandomProvider` to satisfy C8 determinism constraint. Grep of E2E directory must match `"IRandomProvider"`. |
| 20 | After splitting IWcCounterMessageNtr into Observation + Revelation interfaces, remove the original IWcCounterMessageNtr. Grep of Era.Core/ must find zero matches for `"interface IWcCounterMessageNtr[^A-Z]"` (excludes the two new suffixed interfaces). |
| 21 | Run `npm run format:check --prefix src/tools/node/feature-dashboard`. Exit code 0 = clean formatting. Fix any issues found. |
| 22 | After removing MasterPose from ICounterUtilities and IComableUtilities, verify ITouchStateManager.MasterPose canonical signature `(int targetPart, int masterPart, ...)` still exists. Grep of Era.Core/ must match. |
| 23 | Create cross-system flow test (Training→Counter) in E2E directory. Test must pass with `dotnet test --filter "FullyQualifiedName~CrossSystemFlow"`. Uses seeded IRandomProvider for determinism. |
| 24 | After all refactoring changes (MasterPose consolidation, ITouchSet migration, DatuiMessage extraction, NTR split), run full Era.Core unit test suite. All existing tests must pass, verifying adapter wiring and backward compatibility (C3 constraint). |
| 25 | After registering ~21 Phase 21 services in AddEraCore() (Task 5), verify that the total DI registration count in ServiceCollectionExtensions.cs reaches >= 25 (baseline ~10). Defense-in-depth alongside AC#1 DiResolutionTests. |
| 26 | Run `Grep(path="C:/Era/core/src/Era.Core/", pattern="TODO\|FIXME\|HACK")`. Document count and tracking status in Execution Log: `"TODO/FIXME/HACK audit: N found, 0 untracked"`. All found items must be tracked in Mandatory Handoffs or Redux. Satisfies C4 constraint. |
| 27 | After Task 8 CA1510 removal attempt, document outcome in Execution Log: `"CA1510 removal: REMOVED"` if successful, or `"CA1510 removal: DEFERRED to F{ID}"` if reverted. Satisfies Task 8 conditional verification. |
| 28 | After all obligation categorization (Task 7), document total transferred count in Execution Log: `"Transferred N obligations to F814"`. Count must match Mandatory Handoffs table row count with Destination=F814. |
| 29 | Create `CounterCombinationAdapter : ICombinationCounter` and `WcCounterCombinationAdapter : IWcCombinationCounter` in Era.Core. Register in AddEraCore(). Grep must find `"CounterCombinationAdapter"`. |
| 30 | Create `WcCounterCombinationAdapter : IWcCombinationCounter` in Era.Core. Register in AddEraCore(). Grep must find `"WcCounterCombinationAdapter"` independently of AC#29. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| MasterPose canonical location | Keep in ICounterUtilities / Keep in IComableUtilities / Keep in ITouchStateManager only | ITouchStateManager only | F811 designates ITouchStateManager as canonical owner; it has the most complete signature with `prevTurn` flag |
| ITouchSet migration | Keep parallel / Migrate call sites then delete | Migrate then delete | ITouchSet (3-param) is only consumed by CounterSourceHandler (1 file); migration is low-risk and completes the consolidation. ITouchStateManager is the canonical (F811) |
| Stub class pattern for unimplemented interfaces | Anonymous lambda registration / Internal stub class / No-op default methods | Internal stub class following StubComableUtilities pattern | Consistent with existing codebase pattern (StubComableUtilities); easier to find and update; TreatWarningsAsErrors requires implementations to exist |
| IWcCounterMessageNtr split implementation | Single class implementing both interfaces / Two separate classes | Single class implementing both interfaces | Minimal file churn; the responsibility split is at the interface level for DI seam; internal class structure can be refactored separately |
| ICombinationCounter DI registration | Adapter wrapper / Direct registration of CounterCombination | Direct registration with interface adaptation | CounterCombination has `AccumulateCombinations()` returning `int[]` while ICombinationCounter takes `CharacterId` — must create adapter or adjust interface. Use adapter class per F811 spec. |
| DatuiMessage extraction scope | Extract to shared class only / Also update all callers | Extract and update both callers (CounterMessage + WcCounterMessage) | Both callers must use the shared service to eliminate duplication; partial extraction leaves debt |
| E2E test scope on DI failure | Continue to cross-system test / Stop and create F815 | Stop and create F815 if DI resolution fails | Feature spec explicitly mandates F815 fallback path to prevent half-built E2E infrastructure |

### Interfaces / Data Structures

**New interfaces**:

```csharp
// Era.Core/Counter/IDatuiMessageService.cs
public interface IDatuiMessageService
{
    void SendDatuiMessage(CharacterId offender);
}

// Era.Core/Counter/IWcCounterMessageNtrObservation.cs
public interface IWcCounterMessageNtrObservation
{
    void RotorOut(CharacterId offender);
    void OshikkoLooking(CharacterId offender);
    void WithFirstSex(CharacterId offender, int painLevel);
    void WithPetting(CharacterId offender);
}

// Era.Core/Counter/IWcCounterMessageNtrRevelation.cs
public interface IWcCounterMessageNtrRevelation
{
    void NtrRevelation(CharacterId target, int actionType);
    void NtrRevelationAttack(CharacterId attacker);
}
```

**Stub classes to create** (pattern: internal sealed class, all methods no-op or return safe default):

```csharp
// NullCounterUtilities, NullWcSexHaraService, NullNtrUtilityService,
// NullShrinkageSystem, NullTrainingCheckService, NullKnickersSystem,
// NullEjaculationProcessor, NullKojoMessageService
// Each in the relevant Counter subdirectory namespace
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `ICombinationCounter.AccumulateCombinations(CharacterId)` signature does not match `CounterCombination.AccumulateCombinations()` (no parameter). Adapter class required. | AC#1 (DI resolution), Task 2 | Create `CounterCombinationAdapter : ICombinationCounter` that wraps CounterCombination and discards the CharacterId parameter. Same for WcCounterCombination. |
| `IComHandler` uses strategy pattern — no single concrete class. `BuildServiceProvider().GetRequiredService<IComHandler>()` will fail without special handling. | AC#1 (DI resolution) | Register a no-op `NullComHandler` as default, or use keyed services. Investigate how ComCaller resolves IComHandler before choosing registration approach. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2,14,22 | Unify MasterPose SSOT: consolidate ICounterUtilities.MasterPose (F803, (int,int)→CharacterId), IComableUtilities.MasterPose (F809, (int,int,int)→int), and ITouchStateManager.MasterPose (F811, (int,int,bool)→int canonical) into single canonical implementation. Adapter specs in F811 Mandatory Handoffs. | | [ ] |
| 2 | 1,29,30 | Register DI adapters: ICombinationCounter→F802 CounterCombination, IWcCombinationCounter→F802 WcCounterCombination (F811 stub replacement handoff). Create CounterCombinationAdapter and WcCounterCombinationAdapter classes wrapping the concrete classes to match ICombinationCounter/IWcCombinationCounter signatures. | | [ ] |
| 3 | 9 | Migrate F803 ITouchSet→ITouchStateManager (26 references). Parameter mapping: mode→targetPart, type→masterPart, target.Value→character, reset=false (F811 Mandatory Handoffs) | | [ ] |
| 4 | 9 | Delete ITouchSet.cs after migration (Task 3). Verify Grep finds zero matches for "interface ITouchSet" in Era.Core/. | | [ ] |
| 5 | 1,25 | Register all ~21 missing Phase 21 Counter interfaces in AddEraCore(). Create Null-prefixed stub classes for unimplemented interfaces (ICounterUtilities, IWcSexHaraService, INtrUtilityService, IShrinkageSystem, ITrainingCheckService, IKnickersSystem, IEjaculationProcessor, IKojoMessageService). Register IComHandler as NullComHandler default. Follow StubComableUtilities pattern. | | [ ] |
| 6 | 7,16 | Reconcile Phase 21 architecture documentation: update phase-20-27-game-systems.md Phase 21 Status from "TODO" to "DONE" and mark Success Criteria checkboxes as completed. | | [ ] |
| 7 | 5,6,26,28 | Document deferred obligation tracking in Execution Log: (1) N+4 --unit NOT_FEASIBLE re-deferral with concrete destination feature ID, (2) Redux trigger evaluation result after all obligations categorized. If Redux DRAFT created, verify: (a) pm/features/feature-{N}.md exists with [DRAFT] status, (b) pm/index-features.md includes the new entry. Note: The 22+ Mandatory Handoffs table entries (all Destination=F814) serve as the structural tracking record; this Task documents the summary Execution Log entries only. | | [ ] |
| 8 | 27 | Analyzer NoWarn debt一括修正: (1) NoWarnからCA1510を除去, (2) `dotnet format analyzers devkit.sln --diagnostics CA1510 --severity error` で自動修正, (3) `dotnet build` で0 errors確認, (4) `dotnet test` で全テスト通過確認。失敗時はNoWarnに戻して次Phaseに繰り越し。対象ルールの優先順位と手順は `memory/analyzer-nowarn-debt.md` 参照 | [I] | [ ] |
| 9 | 8 | Stryker.NET baseline計測: `cd src/Era.Core.Tests && dotnet stryker` を実行し、mutation score (killed%, survived%, total mutants) をExecution Logに記録。これが以降のPost-Phase Reviewの比較baselineとなる | [I] | [ ] |
| 10 | 4,21 | Dashboard lint/format verification: `cd src/tools/node/feature-dashboard && npm run lint` で0 errors確認 + `npm run format:check` でclean確認。warningは許容するがerrorは修正必須 | | [ ] |
| 11 | - | Push all commits to remote (core repo + devkit repo) | | [ ] |
| 12 | 11 | DatuiMessage DRY extraction: create IDatuiMessageService interface and DatuiMessageService concrete class in Era.Core/Counter/. Update CounterMessage.cs and WcCounterMessage.cs to inject and delegate to IDatuiMessageService. Register in AddEraCore(). (F805 Mandatory Handoff) | | [ ] |
| 13 | 12,17,20 | F808 WcCounterMessageNtr責務分割: IWcCounterMessageNtrを2インターフェースに分割。(1) IWcCounterMessageNtrObservation (RotorOut, OshikkoLooking, WithFirstSex, WithPetting — 5依存) (2) IWcCounterMessageNtrRevelation (NtrRevelation, NtrRevelationAttack + private helpers — 7依存)。現状9依存コンストラクタはERBファイル境界の盲目的踏襲が原因。RotorOutのIWcCounterMessageItem移動も検討（デバイスライフサイクルの同一ドメイン）。F806/F807のインジェクション変更が必要 | | [ ] |
| 14 | 13,15 | F808 NtrReversalSource/NtrAgreeSource計算式等価性検証: ERBは動的スケーリング（除算ベース: (delta/10)+10等）、C#は固定定数（50,200,500等）。意図的簡略化か回帰バグか判定し、ERB等価が必要なら修正。対象: WcCounterMessageNtr.cs:512-602 vs TOILET_COUNTER_MESSAGE_NTR.ERB:960-1034 | [I] | [ ] |
| 15 | 1,3,10,18,19,23,24 | CP-2 Step 2a E2E基盤構築: (1) `src/Era.Core.Tests/E2E/` ディレクトリ作成 (2) `AddEraCore()` DI全解決テスト（Phase 5-21 全サービス登録、例外なし） (3) Training→Counter cross-system フロー（seeded IRandomProvider 決定的実行）。設計根拠: `docs/architecture/migration/full-csharp-architecture.md` CP-2 Step 2a。**E2E失敗時の対応**: DI解決またはcross-systemフローが失敗し、障害箇所の切り分けが困難な場合、**実装を中断して F815 Golden Test Design を作成する**。理由: 150+ COMF × COMABLE × Counter が統合された状態で障害点を特定するには、関数単位の等価性検証（ERB出力 vs C#出力のゴールデンテスト）が障害分離レイヤーとして必要。ゴールデンテスト基盤を先に整備し、個別関数の等価性を確認した上でE2Eに再挑戦する。 | | [ ] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP -> Ask user for guidance.

<!-- Note: Infra-type features use Constraint|Rule|Source format instead of template's Phase|Agent|Model|Input|Output. Constraints document implementation-time invariants rather than sequential agent dispatch steps. -->

| Constraint | Rule | Source |
|------------|------|--------|
| TreatWarningsAsErrors | All C# changes must produce zero build warnings. Build via WSL: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build devkit.sln'` | Directory.Build.props |
| E2E tests via WSL | All `dotnet test` commands must use WSL and include `--blame-hang-timeout 10s` | CLAUDE.md WSL section |
| Cross-repo coordination | C# code changes (Era.Core interfaces, stub classes, DI registrations, E2E tests) go in `C:\Era\core`. Architecture docs, PM files, and Execution Log updates go in `C:\Era\devkit`. | Architecture: 5-repo split |
| Redux evaluation mandatory | After all obligations are categorized, count unresolved items vs Redux threshold. Document result before Task 11 (Push). | C5 constraint, AC#6 |
| E2E fallback protocol | If DI resolution or cross-system flow fails and fault isolation is difficult, STOP implementation and create F815 Golden Test Design feature. Do NOT continue building broken E2E infrastructure. | Technical Design Work Area 3 |
| Stub class pattern | Null-prefixed no-op classes must follow the StubComableUtilities pattern (internal sealed, all methods return safe defaults). TreatWarningsAsErrors requires full interface implementation. | Key Decisions |
| Immutable E2E tests | E2E test files created under `Era.Core.Tests/E2E/` must not be deleted after creation (immutable test policy). | C12 constraint, CLAUDE.md |
| Execution Log entries | AC#5, AC#6, AC#8, AC#13, AC#15, AC#26, AC#27, AC#28 are verified by Grep on the Execution Log in this file. These entries MUST use the exact patterns specified in the AC Expected column. | AC table rows 5, 6, 8, 13, 15, 26, 27, 28 |
| Task 14 REGRESSION conditional | If Task 14 finds REGRESSION for NtrReversalSource or NtrAgreeSource: (1) apply fix and verify via AC#24 (all tests pass), OR (2) fill Mandatory Handoffs "NtrReversalSource/NtrAgreeSource REGRESSION fix" row Result column with concrete deferral rationale. REGRESSION without either action is a protocol violation. | AC Details AC#13/AC#15 |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| N+4 --unit deprecation NOT_FEASIBLE re-deferral | NOT_FEASIBLE tracking chain: F782->F783->F813->next. Must document concrete successor destination in Execution Log. | Feature | F814 | Task 7 | [ ] | |
| IShrinkageSystem runtime implementation | No concrete implementation exists; engine-layer change required. Stub registered in DI is acceptable for now. | Feature (engine-layer) | F814 | Task 7 (track) | [ ] | |
| IEngineVariables GetTime/SetTime behavioral override | Default bodies are no-ops; engine implementor must override for TIME operations. Silent failure risk in production. | Feature (engine-layer) | F814 | Task 7 (track) | [ ] | |
| WcCounterMessageSex responsibility review (16 deps) | 16 constructor dependencies may warrant class split. Deferred from F806. | Feature | F814 | Task 7 (track) | [ ] | |
| CFlag/Cflag naming normalization | WcCounterMessage.cs uses CFlag prefix; WcCounterMessageItem/Ntr use Cflag prefix. Inconsistency deferred from F807. | Feature | F814 | Task 7 (track) | [ ] | |
| WC_VA_FITTING caller documentation | 9 kojo ERB files call WC_VA_FITTING; after C# migration external callers must use IWcCounterMessageSex.VaFitting. Deferred from F806. | Feature | F814 | Task 7 (track) | [ ] | |
| IComHandler DI registration strategy | IComHandler uses strategy pattern -- no single concrete class. NullComHandler registered as default is a known gap. | Feature | F814 | Task 7 (track) | [ ] | |
| WcCounterMessage constructor bloat (12 params) | Parameter object or handler dispatch consolidation needed. Deferred from F807. | Feature | F814 | Task 7 (track) | [ ] | |
| WcCounterMessageTease behavioral test coverage | F807 deferred; structural migration equivalence only -- behavioral equivalence (handler branching, INPUT loop paths, NTR revelation, EQUIP mutation) unverified | Feature | F814 | Task 7 (track) | [ ] | |
| NOITEM photography bug (ERB原文バグ) | F806 deferred; TOILET_COUNTER_MESSAGE_SEX.ERB line 411 uses `NOITEM != 0 &&` instead of `||`. C# faithfully preserved bug at WcCounterMessageSex.cs:1922. Fix requires guard clause logic change. | Feature | F814 | Task 7 (track) | [ ] | |
| KOJO 3-param overload verification | F806 deferred; F806 uses 2-param KojoMessageWcCounter for all 28 TRYCALLFORM patterns. Verify no pattern requires a 3rd parameter. | Feature | F814 | Task 7 (track) | [ ] | |
| WcCounterMessageSex duplicate constant names | F806 deferred; TalentVirginity2/TalentGender2 duplicate TalentVirginity/TalentGender. Consolidation needed. | Feature | F814 | Task 7 (track) | [ ] | |
| Dispatch() dual offender convention unification | F806 deferred; SEX handlers use explicit `CharacterId offender` param; ITEM/NTR/TEASE use implicit `_engine.GetTarget()`. Dual convention in WcCounterMessage.Dispatch(). | Feature | F814 | Task 7 (track) | [ ] | |
| IWcCounterMessageTease interface extraction | F807 deferred; WcCounterMessageTease is concrete type injection. Decision: (A) create IWcCounterMessageTease or (B) accept as permanent technical debt. | Feature | F814 | Task 7 (track) | [ ] | |
| Character ID constant consolidation | F807 deferred; MESSAGE27 has 13 private const character IDs that may duplicate F806 SEX. Shared CharacterConstants class does not exist. | Feature | F814 | Task 7 (track) | [ ] | |
| AC#34 local function enforcement gap | F807 deferred; AC#34 grep pattern cannot detect C# local functions (no `private` keyword). WcCounterMessageTease local function usage unverified. Static analysis or additional grep needed. | Feature | F814 | Task 7 (track) | [ ] | |
| ICharacterStringVariables VariableStore implementation | F803 deferred; CSTR not stored in VariableStore. Interface definition + stub only; runtime extension requires engine-layer changes. | Feature (engine-layer) | F814 | Task 7 (track) | [ ] | |
| EXP_UP logic duplication | F803 deferred; CheckExpUp exists as private in AbilityGrowthProcessor and public in ICounterUtilities. Extraction to shared implementation needed. | Feature | F814 | Task 7 (track) | [ ] | |
| ICounterSourceHandler ISP violation | F803 deferred; single interface exposes 3 responsibilities (dispatch, undressing, pain check). F805 DI impact when extracting IUndressingHandler. | Feature | F814 | Task 7 (track) | [ ] | |
| CFlagIndex typed struct | F803 deferred; CFLAG dispatch uses raw int constants instead of typed CFlagIndex. Cross-class reuse not yet needed. | Feature | F814 | Task 7 (track) | [ ] | |
| EquipIndex typed struct | F803 deferred; DATUI helpers use raw int EQUIP constants instead of typed EquipIndex. Cross-class reuse not yet needed. | Feature | F814 | Task 7 (track) | [ ] | |
| IComableUtilities/ICounterUtilities TimeProgress/IsAirMaster/GetTargetNum consolidation | F810 Mandatory Handoff; 3 of 4 overlapping methods remain after MasterPose consolidation (Task 1). Need interface unification or removal of duplicates. | Feature | F814 | Task 7 (track) | [ ] | |
| NtrReversalSource/NtrAgreeSource REGRESSION fix if found | Task 14 [I] investigation may find REGRESSION. If REGRESSION found but fix is not feasible within F813 scope, track here with concrete fix plan. | Feature | F814 | Task 14 | [ ] | |
| Null-prefixed stub classes concrete implementation | 7 of 8 DI stub classes (NullCounterUtilities, NullWcSexHaraService, NullNtrUtilityService, NullTrainingCheckService, NullKnickersSystem, NullEjaculationProcessor, NullKojoMessageService) need replacement with real implementations. IShrinkageSystem tracked separately. | Feature | F814 | Task 7 (track) | [ ] | |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| - | START | - | F813 Phase 21 Post-Phase Review begins | - |
| - | reconciliation | implementer | Phase 21 architecture doc update | - |
| - | redux-eval | implementer | Redux evaluation result: | - |
| - | stryker | implementer | mutation score: | - |
| - | ntr-verify | implementer | NtrReversalSource verification result: | - |
| - | ntr-verify | implementer | NtrAgreeSource verification result: | - |
| - | n4-defer | implementer | N+4 --unit NOT_FEASIBLE: destination | - |
| - | todo-audit | implementer | TODO/FIXME/HACK audit: | - |
| - | ca1510 | implementer | CA1510 removal: | - |
| - | transfer | implementer | Transferred N obligations to F814 | - |
| - | di-resolve | implementer | CP-2 Step 2a E2E DI resolution test | - |
| - | END | - | F813 complete | - |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: Related Features table | F782 referenced in body but missing from Related Features table
- [fix] Phase1-RefCheck iter1: Related Features table | F815 referenced in body but missing from Related Features table
- [pending] Phase2-Pending iter1: Task 11 has AC# = '-' (orphan task). Task 'Push all commits to remote' has no verifying AC. Remove from Tasks table or add AC.
- [fix] Phase2-Review iter1: Mandatory Handoffs table | All 8 TBD Destination IDs replaced with F814 (Phase 22 Planning successor)
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#14 (IComableUtilities MasterPose not_contains) for Task 1 coverage gap
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#15 (NtrAgreeSource verification result) for Task 14 coverage gap
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#16 (Phase 21 Success Criteria checkboxes) for C2 constraint coverage
- [fix] Phase2-Review iter1: AC Coverage row 6 | AC#6 pattern aligned with AC Definition Table regex
- [fix] Phase2-Review iter1: Mandatory Handoffs table | Added F807 WcCounterMessageTease behavioral test coverage handoff to F814
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#17 (IWcCounterMessageNtrRevelation contains) for Task 13 coverage gap
- [fix] Phase2-Review iter2: Technical Design Work Area 3 | Removed IComHandler 'skip if DI test allows abstract' escape hatch contradicting C1
- [fix] Phase2-Review iter2: AC#16 | Revised regex from 'Phase 21.*\[ \]' to contains '\\[x\\].*CP-2 Step 2a PASS' (unique to Phase 21 section)
- [fix] Phase2-Review iter2: Goal Coverage table | Moved AC#2, AC#14 from Goal #1 to Goal #2 (MasterPose is deferred obligation, not DI registration)
- [fix] Phase2-Review iter3: Mandatory Handoffs table | Added 12 missing deferred obligation entries from F806 (4), F807 (3), F803 (5) all with Destination=F814
- [fix] Phase2-Review iter4: Technical Design Work Area 4 | Fixed stale annotation from '(AC#11, Task not in current Tasks table)' to '(AC#11, Task 12)'
- [fix] Phase2-Uncertain iter4: AC Definition Table | Added AC#18 ([Fact] exists in E2E) to prevent AC#1 vacuous pass
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#19 (IRandomProvider in E2E) to verify C8 constraint
- [fix] Phase2-Review iter5: Deferred Obligations F807 item 1 | Updated text to acknowledge re-deferral from F813 to F814
- [fix] Phase2-Review iter5: Technical Design Work Area 11 | Fixed stale task reference from 'Task 7' to 'Task 8'
- [fix] Phase2-Review iter5: Tasks table Task 15 | Added AC#10 to ensure build verification survives Task 8 deferral
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#20 (IWcCounterMessageNtr original interface removed after split) for Task 13
- [fix] Phase2-Review iter7: Mandatory Handoffs table | Added TimeProgress/IsAirMaster/GetTargetNum consolidation entry (F810 handoff, only MasterPose was covered)
- [fix] Phase2-Review iter7: Technical Design Work Area 5 | Resolved 'keep or deprecate' ambiguity — now states removal intent aligned with AC#20
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#21 (dashboard format:check) for Task 10 coverage gap
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#22 (canonical MasterPose preserved in ITouchStateManager) for Task 1 positive verification
- [fix] Phase2-Uncertain iter8: Goal Coverage table | Added AC#6 to Goal #2 coverage (Redux eval is obligation tracking mechanism)
- [fix] Phase2-Review iter9: Technical Design Work Area 5 | Fixed stale task reference from 'Task 11' to 'Task 13' (with AC#17, AC#20)
- [fix] Phase2-Review iter9: Technical Design Work Area 6 | Fixed stale task reference from 'Task 12' to 'Task 14' (with AC#15)
- [fix] Phase2-Review iter9: Technical Design Work Area 9 | Fixed stale task reference from 'Task 8' to 'Task 9'
- [fix] Phase2-Review iter9: Technical Design Work Area 6 | Added NtrAgreeSource investigation guidance (matching AC#15 and Task 14)
- [fix] Phase2-Review iter9: Tasks table Task 7 | Added clarification that Mandatory Handoffs table is the structural tracking record
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs table | Added NtrReversalSource/NtrAgreeSource REGRESSION fix tracking (conditional on Task 14 result)
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs table | Added consolidated entry for 7 Null-prefixed stub classes needing real implementations
- [fix] Phase4-ACValidation iter10: AC#16 | Changed matcher from 'contains' to 'matches' (regex pattern in Expected)
- [fix] Phase4-ACValidation iter10: AC#18 | Changed Expected from '\\[Fact\\]' to '[Fact]' (literal string for contains matcher)
- [fix] Phase4-ACValidation iter10: AC#20 | Changed matcher from 'not_contains' to 'not_matches' (regex character class in Expected)
- [fix] Phase1-RefCheck iter1: Links section | F782 referenced in body but missing from Links section
- [fix] Phase1-RefCheck iter1: Links section | F815 referenced in body but missing from Links section
- [fix] Phase2-Review iter1: Philosophy | Corrected '12 predecessor features' to '13 predecessor features' (F783 + F801-F812 = 13)
- [fix] Phase2-Review iter1: Implementation Contract | Added AC#15 to Execution Log Grep pattern list
- [fix] Phase2-Review iter1: Structure | Removed non-template ## Summary section (content already in Goal)
- [fix] Phase2-Review iter1: Implementation Contract | Added deviation comment for infra Constraint|Rule|Source format
- [fix] Phase2-Review iter1: Structure | Moved ### Deferred Obligations from Background subsection to top-level ## section
- [fix] Phase2-Uncertain iter1: AC Definition Table | Split AC#1 filter from 'E2E' to 'DiResolutionTests'; added AC#23 for cross-system flow
- [fix] Phase2-Review iter2: AC#7 AC Details | Added Complementary note clarifying AC#16 provides Phase-21-specific verification
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#24 (all unit tests pass after refactoring) for C3 adapter wiring verification
- [fix] Phase2-Uncertain iter3: AC Definition Table | Added AC#25 (DI registration count gte 25) for Goal #1 defense-in-depth
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#26 (TODO/FIXME/HACK audit) for C4 constraint coverage
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#27 (CA1510 removal outcome) for Task 8 conditional verification
- [fix] Phase2-Uncertain iter4: AC Definition Table | Added AC#28 (obligation transfer count) for obligation audit completeness
- [fix] Phase2-Review iter4: Tasks table Task 8 | Tagged as [I] (outcome depends on build result)
- [fix] Phase2-Review iter5: Goal Coverage | Moved AC#27 from Goal #1 (DI) to Goal #2 (obligation tracking) — CA1510 is code quality, not DI
- [fix] Phase2-Uncertain iter5: Philosophy Derivation | Added AC#23 to Pipeline Continuity row (cross-system flow E2E)
- [fix] Phase2-Review iter6: AC#2 description | Changed from 'MasterPose consolidated to single canonical interface' to 'ICounterUtilities MasterPose removed after consolidation'
- [fix] Phase2-Review iter6: AC Details | Added AC#13/AC#15 conditional path note (REGRESSION requires fix or documented deferral)
- [fix] Phase2-Review iter7: AC#26 | Broadened audit path from Era.Core/Counter/ to Era.Core/ (C4 full scope)
- [fix] Phase2-Review iter7: Goal Coverage | Removed AC#10 from Goal #1 (build success ≠ DI resolution)
- [fix] Phase2-Review iter7: Tasks table Task 8 | Removed AC#10 dual assignment (keep only AC#27)
- [fix] Phase2-Review iter8: Implementation Contract | Added Task 14 REGRESSION conditional enforcement row
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#29 (CounterCombinationAdapter exists) for Task 2 adapter verification
- [fix] Phase2-Review iter9: Technical Design Work Area 1 | Fixed stale MasterPose signature (int mode, int type, int target) → correct AC#2/AC#14 patterns
- [fix] Phase2-Uncertain iter9: AC Details | Added AC#28 expected minimum count (20) and verification guidance
- [fix] Phase2-Review iter10: AC Details | Added AC#6 conditional path for Redux DRAFT file existence verification
- [fix] Phase2-Review iter10: Tasks table | Moved AC#24 from Task 1 to Task 15 (post-refactoring timing)
- [fix] Phase2-Review iter1: Tasks table Task 7 | Added Redux DRAFT file verification sub-step (AC#6 conditional path obligation)
- [fix] Phase2-Review iter1: Goal Coverage Goal #3 | Removed AC#1 (DiResolutionTests ≠ cross-system flow)
- [fix] Phase2-Review iter2: Philosophy Derivation | Added AC#16 to 'reconciled architecture documentation' row (C2 requires both Status + Success Criteria)
- [fix] Phase2-Review iter3: Technical Design Work Area 11 | Fixed header (AC#10, Task 8) → (AC#10, Task 15) and body 'Task 7' → 'Task 8' for CA1510
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#30 (WcCounterCombinationAdapter exists) for Task 2 second adapter verification
- [fix] Phase2-Review iter4: Goal Coverage | Moved AC#10 from Goal #3 (E2E) to Goal #2 (obligation resolution build risk)
- [fix] Phase3-Maintainability iter5: Philosophy Derivation | Fixed 'verified DI composition' row: removed AC#2, added AC#25, AC#29, AC#30 (aligned with Goal Coverage Goal #1)
- [fix] Phase2-Review iter6: Philosophy Derivation | Added AC#21 to 'Pipeline Continuity' row (dashboard compliance includes format:check)
- [pending] Phase2-Pending iter7: AC#10 Goal Coverage placement loop — moved from Goal #1 (iter7-prev) → Goal #3 → Goal #2 (iter4), reviewer says remove from Goal #2. No clear correct Goal. Philosophy Derivation has AC#10 in Pipeline Continuity.

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning (decomposition origin, Mandatory Handoff source)
- [Predecessor: F801](feature-801.md) - Main Counter Core
- [Predecessor: F802](feature-802.md) - Main Counter Output (DatuiMessage duplication source)
- [Predecessor: F803](feature-803.md) - Main Counter Source (ISP violation, ITouchSet, IShrinkageSystem)
- [Predecessor: F804](feature-804.md) - WC Counter Core
- [Predecessor: F805](feature-805.md) - WC Counter Source + Message Core (DatuiMessage, CFlag naming)
- [Predecessor: F806](feature-806.md) - WC Counter Message SEX (8 deferred handoffs, 16 deps)
- [Predecessor: F807](feature-807.md) - WC Counter Message TEASE (6 deferred handoffs, constructor bloat)
- [Predecessor: F808](feature-808.md) - WC Counter Message ITEM + NTR (NtrReversal divergence, responsibility split)
- [Predecessor: F809](feature-809.md) - COMABLE Core (IComableUtilities overlap)
- [Predecessor: F810](feature-810.md) - COMABLE Extended (MasterPose duplication handoff)
- [Predecessor: F811](feature-811.md) - SOURCE Entry System (ITouchStateManager canonical, MasterPose adapter specs)
- [Predecessor: F812](feature-812.md) - SOURCE1 Extended
- [Successor: F814](feature-814.md) - Phase 22 Planning (BLOCKED on F813 completion)
- [Related: F782](feature-782.md) - Phase 20 Planning (N+4 --unit deprecation origin)
- [Related: F815](feature-815.md) - Golden Test Design (E2E fallback if DI resolution fails)
- [Related: F816](feature-816.md) - StubVariableStore (test infrastructure reference)
