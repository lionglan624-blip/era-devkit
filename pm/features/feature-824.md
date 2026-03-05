# Feature 824: Sleep & Menstrual System Migration

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T01:37:46Z -->

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

## Type: erb

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 22 State Systems migration is the SSOT for converting ERB game-logic subsystems to C# within Era.Core, ensuring each subsystem has typed interfaces, equivalence-tested behavior, and zero technical debt. F824 covers the sleep depth and menstrual cycle subsystems (睡眠深度.ERB, 生理機能追加パッチ.ERB).

### Problem (Current Issue)

The sleep depth subsystem (睡眠深度.ERB, 244 lines, 9 functions) and menstrual cycle subsystem (生理機能追加パッチ.ERB, 163 lines, 3 functions) exist only as ERB scripts with no C# equivalents. No ISleepSystem or IMenstrualSystem interfaces exist in Era.Core (`C:\Era\core\src\Era.Core\State\` has no SleepDepth.cs or MenstrualCycle.cs). Because these subsystems use raw CFLAG integer indices (e.g., CFLAG:351 for ovum lifespan, CFLAG:352 for menstrual cycle) without named constants, and because the heartbreak acquisition function (`素質傷心取得`) is defined in 睡眠深度.ERB but called externally from INFO.ERB:712, a naive file-level migration would bury shared logic inside a sleep-specific class and perpetuate magic-number CFLAG access patterns.

### Goal (What to Achieve)

Migrate 睡眠深度.ERB and 生理機能追加パッチ.ERB to C# as `State/SleepDepth.cs` and `State/MenstrualCycle.cs` in Era.Core, with:
1. `素質傷心取得` extracted as a shared `HeartbreakService` (callable from both sleep waking and INFO.ERB contexts)
2. Named CharacterFlagIndex/TalentIndex constants for all referenced CFLAG/TALENT indices
3. Equivalence tests verifying C# output matches ERB baseline for both subsystems
4. Zero-debt implementation (no TODO/FIXME/HACK)
5. IMenstrualCycle provides `ResetCycle` integration surface for F822 Pregnancy data coupling (CFLAG:生理周期 reset contract)

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F824 needed? | Sleep depth and menstrual cycle subsystems exist only as ERB scripts with no C# implementation | `C:\Era\core\src\Era.Core\State\` -- no SleepDepth.cs or MenstrualCycle.cs |
| 2 | Why do they lack C# implementations? | Phase 22 State Systems migration was planned by F814 but sub-features were created as DRAFTs without elaboration | `C:\Era\devkit\pm\features\feature-824.md:3` -- Status: [DRAFT]; commit `0138600` |
| 3 | Why must these two files be migrated together? | They are both small (407 lines total), logically independent, and grouped by the Phase 22 architecture as a single work unit | `C:\Era\devkit\docs\architecture\migration\phase-20-27-game-systems.md:339-340` |
| 4 | Why can't they be migrated as simple file copies? | `素質傷心取得` in 睡眠深度.ERB is called from INFO.ERB:712, creating a cross-system dependency that requires extraction as a shared service; raw CFLAG indices lack named constants | `C:\Era\game\ERB\INFO.ERB:712` -- `CALL 素質傷心取得, ARG` |
| 5 | Why (Root)? | ERB's flat function namespace and raw integer variable access mask structural dependencies that must be made explicit through C# interfaces and typed index constants | `C:\Era\game\ERB\生理機能追加パッチ.ERB:5-6` -- CFLAG:351, CFLAG:352 as raw ints |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F824 is a DRAFT skeleton with stub ACs | Two ERB subsystems (407 lines) have no C# equivalents in Era.Core, with hidden cross-system dependencies via shared functions and raw CFLAG indices |
| Where | `pm/features/feature-824.md` -- incomplete feature file | `C:\Era\game\ERB\睡眠深度.ERB` and `C:\Era\game\ERB\生理機能追加パッチ.ERB` -- unmigrated ERB logic |
| Fix | Fill in stub ACs manually | Migrate both subsystems to typed C# with shared HeartbreakService extraction, named index constants, and equivalence tests |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Predecessor -- Phase 22 Planning created this feature |
| F819 | [DRAFT] | Sibling -- Clothing System (no call dependency) |
| F821 | [DRAFT] | Sibling -- Weather System (no call dependency) |
| F822 | [DRAFT] | Related -- Pregnancy System writes menstrual CFLAGs on conception (data coupling via CFLAG:生理周期, not code CALL) |
| F823 | [DRAFT] | Sibling -- Room & Stain System (no call dependency) |
| F825 | [DRAFT] | Successor -- Relationships & DI Integration depends on F824 |
| F826 | [DRAFT] | Successor -- Post-Phase Review depends on F824 |
| F810 | [DONE] | Related -- IComableUtilities.CanMove basis for SleepDepth injection decision |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| ERB line count within volume limit | FEASIBLE | 407 lines total (244 + 163), well within erb type ~500-line limit |
| Core interfaces available | FEASIBLE | IVariableStore, ICharacterStateVariables, IEngineVariables, IComableUtilities all exist with needed methods |
| Predecessor satisfied | FEASIBLE | F814 is [DONE] |
| No blocking sibling dependencies | FEASIBLE | Zero CALL/JUMP dependencies between F824 ERB files and any sibling feature ERB files (3/3 explorers confirmed) |
| Two subsystems internally independent | FEASIBLE | Sleep and menstrual have zero inter-calls; logically separable into distinct classes |
| Equivalence testing feasible | FEASIBLE | Deterministic logic (cycle math, noise calc) is unit-testable; RAND-dependent paths testable with seeded IRandomProvider |
| Minor gaps manageable | FEASIBLE | IS_DOUTEI (1 call in display function, low impact), CAN_MOVE (exists in IComableUtilities), named constants (routine additions) |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core State namespace | HIGH | Two new classes (SleepDepth.cs, MenstrualCycle.cs) plus shared HeartbreakService.cs |
| Era.Core Type indices | MEDIUM | ~15 new named constants across CharacterFlagIndex, TalentIndex, BaseIndex |
| External ERB callers | MEDIUM | EVENTCOMEND, TRACHECK, AFTERTRA, SHOP_ITEM, SHOP2, INFO.ERB, kojo files -- all unmigrated callers need stub/bridge awareness |
| F822 Pregnancy data coupling | LOW | Pregnancy code resets menstrual CFLAGs (`妊娠処理変更パッチ.ERB:278`); interface contract needed but not blocking |
| DI registration | LOW | New services registered in ServiceCollectionExtensions.cs (F825 scope) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IS_DOUTEI(ARG) not in Era.Core | `生理機能追加パッチ.ERB:118` | Need inline implementation or stub for `簡易追加情報` display function (low impact -- display only) |
| CVARSET CFLAG, 317 bulk reset | `睡眠深度.ERB:168` | Bulk variable reset for ufufu session end; requires character-loop SetCharacterFlag or dedicated bulk-reset method |
| CAN_MOVE location in IComableUtilities | `睡眠深度.ERB:156` | Exists in IComableUtilities.CanMove but not on ILocationService; SleepDepth must inject IComableUtilities or ILocationService gets extended |
| TRYCALL 生理周期DEBUG (debug hook) | `生理機能追加パッチ.ERB:19,79,84,91` | Optional debug hook via TRYCALL (no-op if undefined); replace with ILogger in C# |
| IConsoleOutput for PRINT commands | `睡眠深度.ERB` (15+ PRINT statements) | Sleep waking dialogue functions require IConsoleOutput dependency for text output |
| Missing named CFLAG/TALENT constants | `CharacterFlagIndex.cs`, `TalentIndex.cs` | ~15 constants (睡眠, 睡眠深度, うふふ, 現在位置, 生理周期, 卵子生存日数, ピル使用, 傷心, 親愛, NTR, 危険日, 妊娠, etc.) need adding |
| SELECTCOM global state dependency | `睡眠深度.ERB:4` | Noise calculation depends on current action via IEngineVariables.GetSelectCom() |
| RAND determinism for equivalence tests | Both ERB files use RAND | Must inject IRandomProvider for deterministic testing of probability-based logic |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Shared CFLAG state conflict with F822 Pregnancy | MEDIUM | MEDIUM | Define clear interface contract for menstrual state reset; document that pregnancy code resets menstrual fields (data coupling, not code dependency) |
| CAN_MOVE interface location ambiguity | MEDIUM | MEDIUM | Use existing IComableUtilities.CanMove injection; document decision in Key Decisions |
| Missing CharacterFlagIndex/TalentIndex named constants | HIGH | LOW | Add named constants during implementation; raw `new TalentIndex(N)` works as fallback but violates zero-debt |
| IS_DOUTEI function undefined in Era.Core | MEDIUM | LOW | Simple inline check (TALENT:童貞 AND TALENT:性別 >= 2) or stub interface; display-only usage |
| CVARSET bulk reset semantics unclear in C# | LOW | MEDIUM | Implement as character-loop SetCharacterFlag over CFLAG index 317; document equivalence |
| 素質傷心取得 placement causes coupling if embedded in SleepDepth | MEDIUM | MEDIUM | Extract as shared HeartbreakService from the start; both sleep and INFO.ERB callers use same service |
| Sleep waking dialogue complexity | LOW | MEDIUM | 9 functions with character-specific branching; test each branch path with equivalence tests |

---

## Baseline Measurement
<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Sleep ERB line count | `wc -l < "C:/Era/game/ERB/睡眠深度.ERB"` | 244 | Source file size |
| Menstrual ERB line count | `wc -l < "C:/Era/game/ERB/生理機能追加パッチ.ERB"` | 163 | Source file size |
| Sleep ERB function count | `grep -c "^@" "C:/Era/game/ERB/睡眠深度.ERB"` | 9 | Function definitions |
| Menstrual ERB function count | `grep -c "^@" "C:/Era/game/ERB/生理機能追加パッチ.ERB"` | 3 | Function definitions |
| Existing SleepDepth.cs | `ls "C:/Era/core/src/Era.Core/State/SleepDepth.cs" 2>/dev/null` | (not found) | Does not exist yet |
| Existing MenstrualCycle.cs | `ls "C:/Era/core/src/Era.Core/State/MenstrualCycle.cs" 2>/dev/null` | (not found) | Does not exist yet |

**Baseline File**: `_out/tmp/baseline-824.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Noise calculation must match ERB SELECTCASE exactly for all SELECTCOM ranges | `睡眠深度.ERB:4-17` | AC must verify noise values for each SELECTCOM range (20, 21, 40-48, 60-72, 92-94, 97-99, 100-101, default) |
| C2 | Orgasm multiplier tiers must match 6 tier levels | `睡眠深度.ERB:24-38` | AC must test each orgasm count tier boundary (1x/1.35x/1.65x/2.15x/2.75x/3.50x) |
| C3 | Menstrual cycle is a 15-day state machine | `生理機能追加パッチ.ERB:46-76` | AC must verify full cycle: days 1-9 (safe), 10-14 (dangerous), 15 (reset), ovulation probability |
| C4 | Ovulation is probabilistic (30% per day during danger period, forced on day 14) | `生理機能追加パッチ.ERB:64-71` | AC must use seeded IRandomProvider for deterministic testing |
| C5 | Pregnancy skip must be preserved | `生理機能追加パッチ.ERB:42-43` | AC must verify menstrual cycle skips when TALENT:妊娠 is set |
| C6 | Daughter growth gate (NO:149) | `生理機能追加パッチ.ERB:27-31,38-39` | AC must verify daughter characters skip menstruation before growth stage 3 |
| C7 | Male gender guard (gender == 2, excluding futanari) | `生理機能追加パッチ.ERB:23-24` | AC must verify male characters skip menstrual processing |
| C8 | Waking threshold logic (>200/100-200/<100/0) four-value probability | `睡眠深度.ERB:62-74` | AC must verify four-value waking probability logic with seeded random |
| C9 | Waking reactions depend on character NO, TALENT, and hierarchy | `睡眠深度.ERB:106-131` | AC must cover SELECTCASE branches (NO:3, NO:5/6, NO:148/149, default) |
| C10 | Heartbreak function must be independently testable (shared with INFO.ERB) | `INFO.ERB:712`, `睡眠深度.ERB:179` | AC must verify heartbreak as standalone service, not embedded in SleepDepth |
| C11 | Zero-debt requirement | Phase 22 Sub-Feature Requirements | AC must include grep for TODO/FIXME/HACK |
| C12 | CP-2 E2E checkpoint | Phase 22 architecture spec | AC must include integration test |
| C13 | Equivalence tests required | Phase 22 Sub-Feature Requirements | AC must verify ERB-equivalent behavior for both subsystems |
| C14 | IConsoleOutput dependency for dialogue output | Interface Dependency Scan | AC must verify SleepDepth injects IConsoleOutput for PRINTFORM/PRINTW |
| C15 | IComableUtilities.CanMove dependency for eviction | Interface Dependency Scan | AC must verify movement validation injection for sleep expulsion logic |

### Constraint Details

**C1: Noise Calculation SELECTCOM Branching**
- **Source**: `睡眠深度.ERB:4-17` -- SELECTCASE on SELECTCOM with distinct noise values per action range
- **Verification**: Compare C# noise output against ERB SELECTCASE table for each SELECTCOM value
- **AC Impact**: Need per-range test cases covering all SELECTCOM brackets including default case

**C2: Orgasm Multiplier Tiers**
- **Source**: `睡眠深度.ERB:24-38` -- six tiers based on NOWEX orgasm counts
- **Verification**: Feed known orgasm counts and verify multiplier matches ERB thresholds
- **AC Impact**: Need boundary-value tests at each tier transition point

**C3: Menstrual Cycle State Machine**
- **Source**: `生理機能追加パッチ.ERB:46-76` -- 15-day cycle with safe/dangerous/reset transitions
- **Verification**: Step through cycle day-by-day and verify state transitions
- **AC Impact**: Need day-by-day progression test covering full 15-day cycle

**C4: Ovulation Probability**
- **Source**: `生理機能追加パッチ.ERB:64-71` -- 30% chance per day during danger period, forced on day 14
- **Verification**: Use seeded random to verify probability logic
- **AC Impact**: Must inject IRandomProvider; test forced ovulation on day 14 separately

**C5: Pregnancy Skip**
- **Source**: `生理機能追加パッチ.ERB:42-43` — `SIF TALENT:ARG:妊娠 RETURN 0`
- **Verification**: Set TALENT:妊娠, call AdvanceCycle, verify cycle counter unchanged
- **AC Impact**: AC#9 must verify early return when pregnant

**C6: Daughter Growth Gate**
- **Source**: `生理機能追加パッチ.ERB:27-31,38-39` — NO:149 + CFLAG:75 < 3 triggers reset and skip
- **Verification**: Set NO:149 with growth stage < 3, verify menstrual state reset and skip
- **AC Impact**: AC#10 must verify both the reset path and the skip path

**C7: Male Gender Guard**
- **Source**: `生理機能追加パッチ.ERB:23-24` — `SIF TALENT:ARG:性別 == 2 RETURN 0`
- **Verification**: Set TALENT:性別 = 2 (male, non-futanari), verify early return
- **AC Impact**: AC#11 must verify skip for male characters

**C8: Waking Threshold Logic**
- **Source**: `睡眠深度.ERB:62-74` — four values: >200 (no waking check), 100-200 (medium probability), <100 (easy probability), 0 (always wakes)
- **Verification**: Use seeded IRandomProvider to test each threshold boundary
- **AC Impact**: AC#12 must verify four-value threshold probability with deterministic random

**C9: Waking Reactions**
- **Source**: `睡眠深度.ERB:106-131` — SELECTCASE on NO:ARG with character-specific branches
- **Verification**: Test each branch: NO:3 (恋慕→anger, else→indifference), NO:5/6 (anger), NO:148/149 (infant→cry, childish→naive, else→anger), default (rank comparison)
- **AC Impact**: AC#20 must cover all SELECTCASE branches

**C10: HeartbreakService Extraction**
- **Source**: `睡眠深度.ERB:226-243` defines function, `INFO.ERB:712` calls it externally
- **Verification**: Verify HeartbreakService exists as independent class, not nested in SleepDepth
- **AC Impact**: AC must grep for HeartbreakService as separate file/class; verify both sleep and external callers can access it

**C11: Zero-Debt Requirement**
- **Source**: Phase 22 Sub-Feature Requirements (`phase-20-27-game-systems.md:376`)
- **Verification**: Grep all new State files for TODO/FIXME/HACK
- **AC Impact**: AC#14 must verify no debt markers in implementation

**C12: CP-2 E2E Checkpoint**
- **Source**: Phase 22 architecture spec (`phase-20-27-game-systems.md:377`)
- **Verification**: DI resolution test + cross-system flow test
- **AC Impact**: AC#15 must verify services resolve and execute without exceptions

**C13: Equivalence Tests Required**
- **Source**: Phase 22 Sub-Feature Requirements (`phase-20-27-game-systems.md:376`)
- **Verification**: Unit tests comparing C# output to known ERB baseline values
- **AC Impact**: AC#5-12, AC#20-21 must verify ERB-equivalent behavior

**C14: IConsoleOutput Dependency**
- **Source**: Interface Dependency Scan — sleep system has 15+ PRINT statements
- **Verification**: Grep SleepDepth.cs for IConsoleOutput injection
- **AC Impact**: AC#16 verifies constructor injection

**C15: IComableUtilities.CanMove Dependency**
- **Source**: Interface Dependency Scan — `睡眠深度.ERB:156` uses CAN_MOVE
- **Verification**: Grep SleepDepth.cs for IComableUtilities injection
- **AC Impact**: AC#17 verifies constructor injection

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning -- created this feature as DRAFT |
| Related | F819 | [PROPOSED] | Clothing System -- sibling Phase 22 feature (no call dependency) |
| Related | F821 | [PROPOSED] | Weather System -- sibling Phase 22 feature (no call dependency) |
| Related | F822 | [DRAFT] | Pregnancy System -- writes menstrual CFLAGs on conception (`妊娠処理変更パッチ.ERB:278` resets CFLAG:生理周期=0); data coupling, not code dependency |
| Related | F823 | [WIP] | Room & Stain System -- sibling Phase 22 feature (no call dependency) |
| Successor | F825 | [DRAFT] | Relationships & DI Integration -- depends on F824 for service registration |
| Successor | F826 | [DRAFT] | Post-Phase Review -- depends on F824 completion |
| Related | F810 | [DONE] | IComableUtilities.CanMove -- basis for SleepDepth injection decision |

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
| "each subsystem has typed interfaces" | SleepDepth.cs, MenstrualCycle.cs implementations and ISleepDepth.cs, IMenstrualCycle.cs, IHeartbreakService.cs interfaces must exist | AC#1, AC#2, AC#3, AC#23, AC#24, AC#25, AC#26, AC#27, AC#28, AC#33, AC#34, AC#35 |
| "equivalence-tested behavior" | C# output must match ERB baseline for all function paths | AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#20, AC#21, AC#29, AC#31, AC#36, AC#38, AC#39, AC#40 |
| "zero technical debt" | No TODO/FIXME/HACK in new files | AC#14 |

### AC Definition Table

<!-- Deviation: 41 ACs exceed 30-AC soft limit. Justified: F824 migrates 2 independent subsystems (睡眠深度.ERB: 9 functions, 生理機能追加パッチ.ERB: 3 functions) plus shared HeartbreakService extraction, with 15 AC Design Constraints requiring per-constraint verification. Splitting would break the atomic migration unit documented in phase-20-27-game-systems.md. -->

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SleepDepth.cs exists in Era.Core State namespace | file | Glob(C:/Era/core/src/Era.Core/State/SleepDepth.cs) | exists | - | [x] |
| 2 | MenstrualCycle.cs exists in Era.Core State namespace | file | Glob(C:/Era/core/src/Era.Core/State/MenstrualCycle.cs) | exists | - | [x] |
| 3 | HeartbreakService.cs exists as separate class (not in SleepDepth) | file | Glob(C:/Era/core/src/Era.Core/State/HeartbreakService.cs) | exists | - | [x] |
| 4 | Named CFLAG/TALENT/BASE constants added for sleep and menstrual indices | code | Grep(C:/Era/core/src/Era.Core/Types/, pattern="Sleep =\|SleepDepth =\|Ufufu =\|CurrentLocation =\|MenstrualCycleFlag =\|OvumLifespan =\|PillUsed =\|HeartbreakPartner\|Heartbreak =\|DangerDay =\|NTR =\|PublicToilet =\|Anger =\|Pregnancy =\|Gender =\|Fondness =\|Childish =\|CheatingLips =\|CheatingPot =\|CheatingAnal =") | gte | 21 | [x] |
| 5 | Noise calculation matches ERB SELECTCASE for all SELECTCOM ranges (C1) | test | dotnet test --filter "FullyQualifiedName~CalculateNoise" | succeeds | - | [x] |
| 6 | Orgasm multiplier tiers match 6 tier levels (C2) | test | dotnet test --filter "FullyQualifiedName~OrgasmMultiplier" | succeeds | - | [x] |
| 7 | Menstrual cycle 15-day state machine transitions (C3) | test | dotnet test --filter "FullyQualifiedName~AdvanceCycle_Day" | succeeds | - | [x] |
| 8 | Ovulation probability with seeded IRandomProvider (C4) | test | dotnet test --filter "FullyQualifiedName~AdvanceCycle_DangerDay" | succeeds | - | [x] |
| 9 | Pregnancy skip preserved (C5) | test | dotnet test --filter "FullyQualifiedName~AdvanceCycle_WhenPregnant" | succeeds | - | [x] |
| 10 | Daughter growth gate skips menstruation before stage 3 (C6) | test | dotnet test --filter "FullyQualifiedName~AdvanceCycle_Daughter" | succeeds | - | [x] |
| 11 | Male gender guard skips menstrual processing (C7) | test | dotnet test --filter "FullyQualifiedName~AdvanceCycle_Male" | succeeds | - | [x] |
| 12 | Waking threshold four-value probability with seeded random (C8): >200 no-wake, 100-200 medium, <100 easy, 0 always | test | dotnet test --filter "FullyQualifiedName~WakingProbability" | succeeds | - | [x] |
| 13 | HeartbreakService independently testable with affection reduction and TALENT update | test | dotnet test --filter "FullyQualifiedName~AcquireHeartbreak_Reduces\|FullyQualifiedName~AcquireHeartbreak_Sets" | succeeds | - | [x] |
| 14 | Zero debt: no TODO/FIXME/HACK in new, modified, and test files (C11) | code | Grep(C:/Era/core/src/, pattern="TODO\|FIXME\|HACK", glob="**/{SleepDepth,MenstrualCycle,HeartbreakService,ISleepDepth,IMenstrualCycle,IHeartbreakService,CharacterFlagIndex,TalentIndex,BaseIndex,SleepDepthTests,MenstrualCycleTests,HeartbreakServiceTests,ServiceCollectionExtensions}.cs") | not_matches | - | [x] |
| 15 | CP-2 E2E: Sleep and Menstrual services resolve via DI and execute cross-system flow (C12): all 4 tests (Resolve_ISleepDepth, Resolve_IMenstrualCycle, Resolve_IHeartbreakService, SleepMenstrualFlow_ServicesResolveAndExecute) in dedicated E2E test class with `[Trait("Category", "SleepMenstrualE2E")]` | test | dotnet test --filter "Category=SleepMenstrualE2E" | succeeds | - | [x] |
| 16 | SleepDepth injects IConsoleOutput for dialogue output (C14) | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="IConsoleOutput") | matches | IConsoleOutput | [x] |
| 17 | SleepDepth injects IComableUtilities for movement validation (C15) | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="IComableUtilities") | matches | IComableUtilities | [x] |
| 18 | SleepDepth injects IRandomProvider for waking probability (C4/C8) | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="IRandomProvider") | matches | IRandomProvider | [x] |
| 19 | MenstrualCycle injects IRandomProvider for ovulation probability (C4) | code | Grep(C:/Era/core/src/Era.Core/State/MenstrualCycle.cs, pattern="IRandomProvider") | matches | IRandomProvider | [x] |
| 20 | Waking reactions cover all SELECTCASE branches: NO:3 (with 恋慕→anger, without 恋慕→indifference), NO:5/6, NO:148/149, default (C9) | test | dotnet test --filter "FullyQualifiedName~WakingReaction" | succeeds | - | [x] |
| 21 | Ovulation drug function (排卵誘発剤追加処理) migrated in MenstrualCycle | test | dotnet test --filter "FullyQualifiedName~ApplyOvulationDrug" | succeeds | - | [x] |
| 22 | Era.Core builds with zero warnings after adding new files | build | dotnet build C:/Era/core/src/Era.Core/ | succeeds | - | [x] |
| 23 | ISleepDepth.cs interface exists in Era.Core Interfaces namespace | file | Glob(C:/Era/core/src/Era.Core/Interfaces/ISleepDepth.cs) | exists | - | [x] |
| 24 | IMenstrualCycle.cs interface exists in Era.Core Interfaces namespace | file | Glob(C:/Era/core/src/Era.Core/Interfaces/IMenstrualCycle.cs) | exists | - | [x] |
| 25 | IHeartbreakService.cs interface exists in Era.Core Interfaces namespace | file | Glob(C:/Era/core/src/Era.Core/Interfaces/IHeartbreakService.cs) | exists | - | [x] |
| 26 | SleepDepth class implements ISleepDepth interface | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="class SleepDepth.*ISleepDepth") | matches | - | [x] |
| 27 | MenstrualCycle class implements IMenstrualCycle interface | code | Grep(C:/Era/core/src/Era.Core/State/MenstrualCycle.cs, pattern="class MenstrualCycle.*IMenstrualCycle") | matches | - | [x] |
| 28 | HeartbreakService class implements IHeartbreakService interface | code | Grep(C:/Era/core/src/Era.Core/State/HeartbreakService.cs, pattern="class HeartbreakService.*IHeartbreakService") | matches | - | [x] |
| 29 | FormatSimpleStatus displays correct output including IS_DOUTEI inline check | test | dotnet test --filter "FullyQualifiedName~FormatSimpleStatus" | succeeds | - | [x] |
| 30 | MenstrualCycle injects ILogger for debug output replacing TRYCALL hook | code | Grep(C:/Era/core/src/Era.Core/State/MenstrualCycle.cs, pattern="ILogger") | matches | ILogger | [x] |
| 31 | HeartbreakService console output matches ERB dialogue (素質傷心取得 output) | test | dotnet test --filter "FullyQualifiedName~AcquireHeartbreak_OutputsExpectedDialogue" | succeeds | - | [x] |
| 32 | SleepDepth injects ILocationService for MaxRoom eviction logic | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="ILocationService") | matches | ILocationService | [x] |
| 33 | ISleepDepth interface declares all 4 required methods | code | Grep(C:/Era/core/src/Era.Core/Interfaces/ISleepDepth.cs, pattern="CalculateNoise\|UpdateSleepDepth\|HandleWaking\|EvictCharacters") | gte | 4 | [x] |
| 34 | IMenstrualCycle interface declares all 4 required methods | code | Grep(C:/Era/core/src/Era.Core/Interfaces/IMenstrualCycle.cs, pattern="AdvanceCycle\|ApplyOvulationDrug\|FormatSimpleStatus\|ResetCycle") | gte | 4 | [x] |
| 35 | IHeartbreakService interface declares required method | code | Grep(C:/Era/core/src/Era.Core/Interfaces/IHeartbreakService.cs, pattern="AcquireHeartbreak") | matches | - | [x] |
| 36 | EvictCharacters bulk CFLAG reset (CVARSET CFLAG,317 equivalent) zeroes indices 0-316 for all characters | test | dotnet test --filter "FullyQualifiedName~EvictCharacters_BulkReset" | succeeds | - | [x] |
| 37 | ResetCycle zeroes CFLAG:MenstrualCycleFlag for given character (F822 integration contract) | test | dotnet test --filter "FullyQualifiedName~ResetCycle_ZerosMenstrualCycleFlag" | succeeds | - | [x] |
| 38 | UpdateSleepDepth produces ERB-equivalent sleep depth state (@特殊起床 orchestration) | test | dotnet test --filter "FullyQualifiedName~UpdateSleepDepth" | succeeds | - | [x] |
| 39 | `SleepDepthTests.cs`: `HandleWaking_KnownState_ProducesERBEquivalentResult()` — tests full @うふふ中起床口上 orchestration: given character with known sleep depth tier and TALENT state, verifies probability evaluation, correct waking reaction branch via mock IConsoleOutput, and (for 傷心 path) HeartbreakService.AcquireHeartbreak invocation with correct arguments | test | dotnet test --filter "FullyQualifiedName~HandleWaking_KnownState" | succeeds | - | [x] |
| 40 | EvictCharacters eviction logic: characters beyond MaxRoom are evicted (location reset via ILocationService), characters within MaxRoom are unaffected | test | dotnet test --filter "FullyQualifiedName~EvictCharacters_LocationBeyondMaxRoom" | succeeds | - | [x] |
| 41 | SleepDepth injects IHeartbreakService for HandleWaking heartbreak path | code | Grep(C:/Era/core/src/Era.Core/State/SleepDepth.cs, pattern="IHeartbreakService") | matches | IHeartbreakService | [x] |

### AC Details

**AC#4: Named CFLAG/TALENT/BASE constants added for sleep and menstrual indices**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Types/, pattern="Sleep =\|SleepDepth =\|Ufufu =\|CurrentLocation =\|MenstrualCycleFlag =\|OvumLifespan =\|PillUsed =\|HeartbreakPartner\|Heartbreak =\|DangerDay =\|NTR =\|PublicToilet =\|Anger =\|Pregnancy =\|Gender =\|Fondness =\|Childish =\|CheatingLips =\|CheatingPot =\|CheatingAnal =")`
- **Expected**: >= 21 matches across CharacterFlagIndex.cs, TalentIndex.cs, BaseIndex.cs
- **Derivation**: ERB source references 22 distinct named English constants: Sleep (CFLAG), SleepDepth (CFLAG), Ufufu (CFLAG), CurrentLocation (CFLAG), MenstrualCycleFlag (CFLAG:352), OvumLifespan (CFLAG:351), PillUsed (CFLAG), HeartbreakPartner1 (CFLAG), HeartbreakPartner2 (CFLAG), Heartbreak (TalentIndex), DangerDay (TalentIndex), NTR (TalentIndex -- may already exist), PublicToilet (TalentIndex), Pregnancy (TalentIndex -- may already exist), Gender (TalentIndex -- may already exist), Fondness (TalentIndex, 親愛 — 睡眠深度.ERB:101,206,222), Childish (TalentIndex, 幼稚 — 睡眠深度.ERB:119), CheatingLips (TalentIndex, 浮気な唇 — 生理機能追加パッチ.ERB:145), CheatingPot (TalentIndex, 浮気な蜜壷 — 生理機能追加パッチ.ERB:150), CheatingAnal (TalentIndex, 浮気な尻穴 — 生理機能追加パッチ.ERB:155), Anger (BaseIndex). 22 constants with HeartbreakPartner matching both Partner1 and Partner2 = at least 21 unique matches expected.
- **Rationale**: Goal #2 requires named constants for all referenced CFLAG/TALENT indices. Raw integer access violates zero-debt principle.
- **Note**: NTR, Pregnancy, and Gender may already exist in TalentIndex.cs. Pre-existing constants still contribute to the >= 21 count (they match the grep pattern regardless of when they were added), so the threshold remains unchanged. Task#1 must investigate whether these constants already exist; if missing, they must be added in Task#2 to satisfy zero-debt requirement.

**AC#33: ISleepDepth interface declares all 4 required methods**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Interfaces/ISleepDepth.cs, pattern="CalculateNoise\|UpdateSleepDepth\|HandleWaking\|EvictCharacters")`
- **Expected**: >= 4 matches
- **Derivation**: ISleepDepth interface design (Interfaces/Data Structures section) specifies 4 methods: `CalculateNoise`, `UpdateSleepDepth`, `HandleWaking`, `EvictCharacters`. Each method name must appear at least once as a method declaration.
- **Rationale**: Verifies interface contract completeness; file-existence AC#23 alone cannot guarantee correct method signatures.

**AC#34: IMenstrualCycle interface declares all 4 required methods**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Interfaces/IMenstrualCycle.cs, pattern="AdvanceCycle\|ApplyOvulationDrug\|FormatSimpleStatus\|ResetCycle")`
- **Expected**: >= 4 matches
- **Derivation**: IMenstrualCycle interface design specifies 4 methods: `AdvanceCycle`, `ApplyOvulationDrug`, `FormatSimpleStatus`, `ResetCycle`. Each method name must appear at least once.
- **Rationale**: Verifies interface contract completeness; file-existence AC#24 alone cannot guarantee correct method signatures.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | 素質傷心取得 extracted as shared HeartbreakService | AC#3, AC#13, AC#31 |
| 2 | Named CharacterFlagIndex/TalentIndex constants for all referenced indices | AC#4 |
| 3 | Equivalence tests verifying C# output matches ERB baseline for both subsystems | AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#20, AC#21, AC#29, AC#31, AC#36, AC#38, AC#39, AC#40 |
| 4 | Zero-debt implementation (no TODO/FIXME/HACK) | AC#14 |
| 6 | SleepDepth.cs and MenstrualCycle.cs created in Era.Core State namespace | AC#1, AC#2 |
| 7 | DI dependencies injected (IConsoleOutput, IComableUtilities, IRandomProvider, IHeartbreakService) | AC#16, AC#17, AC#18, AC#19, AC#30, AC#32, AC#41 |
| 8 | CP-2 E2E checkpoint | AC#15 |
| 9 | Build succeeds with zero warnings | AC#22 |
| 5 | IMenstrualCycle provides ResetCycle integration surface for F822 Pregnancy data coupling | AC#34, AC#37 |
| 10 | Interface files exist and classes implement them | AC#23, AC#24, AC#25, AC#26, AC#27, AC#28, AC#33, AC#35 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Three new files are created in `C:/Era/core/src/Era.Core/State/`: `SleepDepth.cs`, `MenstrualCycle.cs`, and `HeartbreakService.cs`. Each implements a corresponding interface (`ISleepDepth`, `IMenstrualCycle`, `IHeartbreakService`) in `Era.Core.Interfaces`. All DI dependencies (IVariableStore, IEngineVariables, IConsoleOutput, IComableUtilities, ILocationService, IRandomProvider) are constructor-injected. Named constants are added to `CharacterFlagIndex.cs`, `TalentIndex.cs`, and `BaseIndex.cs` before implementing the migration logic. Equivalence tests in `Era.Core.Tests/State/` cover every ERB function path using seeded `IRandomProvider`. CP-2 E2E checkpoint adds DI resolution tests in `DiResolutionTests.cs` and a cross-system flow test in `CrossSystemFlowTests.cs`.

**Why three files instead of two**: `素質傷心取得` is called from both `睡眠深度.ERB:179` and `INFO.ERB:712`. Embedding it in `SleepDepth` would require `InfoState` to take a `SleepDepth` dependency purely for a side-effect function — a SRP violation. `HeartbreakService` as an independent class allows both callers to inject it directly.

**DI injection approach**: Follow the `GeneticsService` pattern (constructor injection, `ArgumentNullException` guards, field assignment). `SleepDepth` injects: `IVariableStore`, `IEngineVariables`, `IConsoleOutput`, `IComableUtilities`, `ILocationService`, `IRandomProvider`, `IHeartbreakService`. `MenstrualCycle` injects: `IVariableStore`, `IRandomProvider`, `ILogger<MenstrualCycle>`. `HeartbreakService` injects: `IVariableStore`, `IConsoleOutput`.

**CVARSET bulk reset**: `CVARSET CFLAG, 317` resets all 317 CFLAG slots (indices 0 through 316 inclusive) to 0 for every character. In C# this is implemented as a character loop calling `_variables.SetCharacterFlag()` for all characters over CFLAG indices 0 through 316 inclusive. This matches ERB semantics exactly: CVARSET resets 317 CFLAG slots per character, not just うふふ-related flags.

**IS_DOUTEI stub**: `簡易追加情報` uses `IS_DOUTEI(ARG)` in a display-only function. Implement as an inline check: `TALENT:童貞 > 0 && TALENT:性別 >= 2`. No new interface needed.

**ApplyOrgasmMultiplier visibility**: `ApplyOrgasmMultiplier` is a public method on `SleepDepth` class (not on `ISleepDepth` interface). It is a helper for the orgasm tier calculation used within `UpdateSleepDepth`. Public visibility allows direct unit testing (AC#6) without requiring invocation through the interface method.

**TRYCALL 生理周期DEBUG**: The debug hook appears 4 times in `生理機能追加パッチ.ERB`. Replace with `ILogger<MenstrualCycle>` optional debug logging at the same call sites. No behavior change.

**Named constants strategy**: Add 9 new constants to `CharacterFlagIndex`: `Sleep` (index from C:/Era/game/data/CFLAG.yaml), `SleepDepth`, `Ufufu`, `CurrentLocation`, `MenstrualCycleFlag` (352), `OvumLifespan` (351), `PillUsed`, `HeartbreakPartner1`, `HeartbreakPartner2`. Add 9 new constants to `TalentIndex`: `Heartbreak`, `DangerDay`, `NTR`, `PublicToilet`, `Fondness` (親愛), `Childish` (幼稚), `CheatingLips` (浮気な唇), `CheatingPot` (浮気な蜜壷), `CheatingAnal` (浮気な尻穴). Add 1 constant to `BaseIndex`: `Anger`. Verify actual CFLAG/TALENT integer values from C:/Era/game/data/CFLAG.yaml, C:/Era/game/CSV/Talent.csv, C:/Era/game/data/BASE.yaml before writing.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `C:/Era/core/src/Era.Core/State/SleepDepth.cs` |
| 2 | Create `C:/Era/core/src/Era.Core/State/MenstrualCycle.cs` |
| 3 | Create `C:/Era/core/src/Era.Core/State/HeartbreakService.cs` as a separate class (not nested) |
| 4 | Add named constants to `CharacterFlagIndex.cs` (Sleep, SleepDepth, Ufufu, CurrentLocation, MenstrualCycleFlag, OvumLifespan, PillUsed, HeartbreakPartner1, HeartbreakPartner2), `TalentIndex.cs` (Heartbreak, DangerDay, NTR, PublicToilet, Fondness, Childish, CheatingLips, CheatingPot, CheatingAnal, Pregnancy, Gender), `BaseIndex.cs` (Anger); Grep pattern matches English constant names >= 21 matches across the Types directory |
| 5 | `SleepDepthTests.cs`: `CalculateNoise_SelectComCase20_Returns10()`, `CalculateNoise_SelectComCase21_Returns0()`, `CalculateNoise_SelectComRange40to48_Returns25()`, `CalculateNoise_SelectComRange60to72_Returns15()`, `CalculateNoise_SelectComRange92to94_ReturnsExpectedValue()`, `CalculateNoise_SelectComRange97to99_ReturnsExpectedValue()`, `CalculateNoise_SelectComRange100to101_Returns100()`, `CalculateNoise_Default_Returns10()` — verify all SELECTCASE branches |
| 6 | `SleepDepthTests.cs`: `ApplyOrgasmMultiplier_*` — verify each tier boundary (>=8→3.50x, >=6→2.75x, >=4→2.15x, >=2→1.65x, >=1→1.35x, =0→1x); boundary values tested at each threshold |
| 7 | `MenstrualCycleTests.cs`: `AdvanceCycle_Days1to9_SafePeriod()`, `AdvanceCycle_Day10_SetsDangerDay()`, `AdvanceCycle_Day14_ForcedOvulation()`, `AdvanceCycle_Day15_ResetsCycle()` — covers full 15-day state machine |
| 8 | `MenstrualCycleTests.cs`: `AdvanceCycle_DangerDay_30PercentOvulationWithSeededRandom()` — uses `SeededRandomProvider` to verify 30% probability path and forced ovulation on day 14 |
| 9 | `MenstrualCycleTests.cs`: `AdvanceCycle_WhenPregnant_SkipsMenstrualProcessing()` — TALENT:妊娠 set, verify cycle counter unchanged |
| 10 | `MenstrualCycleTests.cs`: `AdvanceCycle_DaughterBeforeGrowthStage3_SkipsMenstruation()` — NO:149 + CFLAG:成長度 < 3, also verifies reset of state when growth flags found active |
| 11 | `MenstrualCycleTests.cs`: `AdvanceCycle_MaleCharacter_SkipsMenstrualProcessing()` — TALENT:性別 == 2 guard |
| 12 | `SleepDepthTests.cs`: `WakingProbability_SleepDepthAbove200_NoWake()`, `WakingProbability_SleepDepthBetween100And200_TierMediumWithSeededRandom()`, `WakingProbability_SleepDepthBelow100_TierLowWithSeededRandom()`, `WakingProbability_SleepDepthZero_AlwaysWakes()` — four-value threshold probability |
| 13 | `HeartbreakServiceTests.cs`: `AcquireHeartbreak_ReducesAffectionByTenth()`, `AcquireHeartbreak_SetsHeartbreakPartner1_WhenEmpty()`, `AcquireHeartbreak_SetsHeartbreakPartner2_WhenPartner1Filled()`, `AcquireHeartbreak_SetsHeartbreakTalentPlus1000()` |
| 14 | Zero TODO/FIXME/HACK: all new State and Interface files written with completed implementation; verified by Grep of all six new files |
| 15 | Add `Resolve_ISleepDepth()`, `Resolve_IMenstrualCycle()`, `Resolve_IHeartbreakService()` to `DiResolutionTests.cs`; add `SleepMenstrualFlow_ServicesResolveAndExecute()` to `CrossSystemFlowTests.cs`: resolves all 3 services from DI container, calls UpdateSleepDepth with a sleeping character mock, and verifies HeartbreakService is callable from SleepDepth's waking path without exceptions |
| 16 | SleepDepth constructor takes `IConsoleOutput` parameter; Grep confirms field and constructor |
| 17 | SleepDepth constructor takes `IComableUtilities` parameter; Grep confirms field and constructor |
| 18 | SleepDepth constructor takes `IRandomProvider` parameter; Grep confirms field and constructor |
| 19 | MenstrualCycle constructor takes `IRandomProvider` parameter; Grep confirms field and constructor |
| 20 | `SleepDepthTests.cs`: `WakingReaction_CharNo3_WithAffection_CallsAnger()`, `WakingReaction_CharNo3_WithoutAffection_CallsIndifference()`, `WakingReaction_CharNo5or6_CallsAnger()`, `WakingReaction_CharNo148or149_Infant_Cries()`, `WakingReaction_CharNo149_Childish_CallsNaive()`, `WakingReaction_Default_HighRank_CallsAnger()`, `WakingReaction_Default_LowRank_CallsConfused()` |
| 21 | `MenstrualCycleTests.cs`: `ApplyOvulationDrug_SetsDangerDayAndOvulationAndCycleDay12()` |
| 22 | `dotnet build C:/Era/core/src/Era.Core/` succeeds; TreatWarningsAsErrors=true enforces zero warnings |
| 23 | Create `C:/Era/core/src/Era.Core/Interfaces/ISleepDepth.cs` (covered by Task 3) |
| 24 | Create `C:/Era/core/src/Era.Core/Interfaces/IMenstrualCycle.cs` (covered by Task 3) |
| 25 | Create `C:/Era/core/src/Era.Core/Interfaces/IHeartbreakService.cs` (covered by Task 3) |
| 26 | SleepDepth.cs declares `class SleepDepth : ISleepDepth` (verified in Task 6 implementation) |
| 27 | MenstrualCycle.cs declares `class MenstrualCycle : IMenstrualCycle` (verified in Task 8 implementation) |
| 28 | HeartbreakService.cs declares `class HeartbreakService : IHeartbreakService` (verified in Task 4 implementation) |
| 29 | `MenstrualCycleTests.cs`: `FormatSimpleStatus_MaleCharacter_ReturnsEmpty()`, `FormatSimpleStatus_WithDoutei_ShowsIndicator()`, `FormatSimpleStatus_FemaleWithActiveCycle_ReturnsExpectedString()` — covers display-only function: male guard, IS_DOUTEI inline check, and normal female positive path |
| 30 | MenstrualCycle.cs constructor takes `ILogger<MenstrualCycle>` parameter; replaces TRYCALL 生理周期DEBUG at 4 call sites |
| 31 | `HeartbreakServiceTests.cs`: `AcquireHeartbreak_OutputsExpectedDialogue()` — verifies IConsoleOutput receives ERB-equivalent PRINT content from 睡眠深度.ERB:226-243 |
| 32 | SleepDepth.cs constructor takes `ILocationService` parameter for MaxRoom() in EvictCharacters |
| 33 | ISleepDepth.cs contains method declarations for all 4 interface methods (covered by Task 3) |
| 34 | IMenstrualCycle.cs contains method declarations for all 4 interface methods including ResetCycle (covered by Task 3) |
| 35 | IHeartbreakService.cs contains AcquireHeartbreak method declaration (covered by Task 3) |
| 36 | `SleepDepthTests.cs`: `EvictCharacters_BulkReset_ZeroesCflag0to316_ForAllCharacters()` — verifies CVARSET CFLAG,317 equivalent: after EvictCharacters call, all CFLAG indices 0-316 are zeroed for every character via mock IVariableStore |
| 37 | `MenstrualCycleTests.cs`: `ResetCycle_ZerosMenstrualCycleFlag()` — verifies ResetCycle zeroes CFLAG:MenstrualCycleFlag for the given character via mock IVariableStore |
| 38 | `SleepDepthTests.cs`: `UpdateSleepDepth_KnownState_ProducesERBEquivalentResult()` — tests full @特殊起床 orchestration: character filtering by location/sleep state, RAND increment (seeded), noise application, CFLAG write-back |
| 39 | `SleepDepthTests.cs`: `HandleWaking_KnownState_ProducesERBEquivalentResult()` — tests full @うふふ中起床口上 orchestration: probability evaluation, reaction branch selection via mock IConsoleOutput, and HeartbreakService.AcquireHeartbreak invocation for 傷心 path |
| 40 | `SleepDepthTests.cs`: `EvictCharacters_LocationBeyondMaxRoom_EvictsCharacters()` — verifies characters at locations > MaxRoom are evicted (location reset via mock ILocationService), characters within MaxRoom are unaffected |
| 41 | SleepDepth.cs constructor takes `IHeartbreakService` parameter for HandleWaking heartbreak invocation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| HeartbreakService placement | A: Nested in SleepDepth, B: Separate class in State/ | B: Separate `HeartbreakService.cs` | INFO.ERB:712 calls it externally; embedding it in SleepDepth would force cross-service dependency on SleepDepth from InfoState |
| IComableUtilities injection namespace | A: Add CanMove to ILocationService, B: Inject `Era.Core.Counter.Comable.IComableUtilities` directly | B: Inject existing IComableUtilities | CanMove already exists in IComableUtilities (F810); extending ILocationService would be scope creep; cross-namespace injection is acceptable |
| CVARSET bulk reset implementation | A: dedicated BulkResetCharacterFlags() method on IVariableStore, B: character loop in SleepDepth | B: inline character loop | Only 1 C# call site exists (SleepDepth.EvictCharacters); MOVEMENT.ERB:81 uses the same ERB CVARSET command but remains unmigrated ERB code that cannot consume IVariableStore. Extraction to IVariableStore.BulkResetCharacterFlags is tracked unconditionally in Mandatory Handoffs for when MOVEMENT.ERB migrates to C# within Phase 22 |
| IS_DOUTEI implementation | A: new IBiologicalState interface method, B: inline TALENT check | B: inline check | 1 call site in display-only function; interface overhead not justified |
| Debug hook replacement | A: Remove TRYCALL 生理周期DEBUG entirely, B: replace with ILogger debug logging | B: ILogger | Preserves debugging capability; ILogger is already in the DI container; no behavioral change |
| Interface naming for new services | A: ISleepSystem / IMenstrualSystem (matching feature file language), B: ISleepDepth / IMenstrualCycle | B: ISleepDepth / IMenstrualCycle | Matches the ERB function file names and the migration target class names; consistent with existing naming (IInfoState mirrors InfoState) |
| MAXROOM / MaxRoom for eviction loop | A: inject ILocationService for MaxRoom(), B: pass max rooms as constructor parameter | A: inject ILocationService | ILocationService already has MaxRoom(); consistent with how other services get location data |
| ApplyOrgasmMultiplier interface placement | A: Declare on ISleepDepth for testability via interface, B: Public concrete method not on interface | B: Public on SleepDepth class only | ApplyOrgasmMultiplier is a calculation helper used within UpdateSleepDepth; adding it to ISleepDepth would expose an implementation detail in the interface contract. Unit tests use the concrete SleepDepth type directly |

### Interfaces / Data Structures

New interfaces in `C:/Era/core/src/Era.Core/Interfaces/`:

```csharp
// ISleepDepth.cs
public interface ISleepDepth
{
    int CalculateNoise(int selectCom, int masterId);
    void UpdateSleepDepth(int masterId, int charCount);   // @特殊起床 loop
    void HandleWaking(int masterId, int wakingCharId);     // @うふふ中起床口上
    void EvictCharacters(int masterId, int charCount);     // expulsion logic
}

// IMenstrualCycle.cs
public interface IMenstrualCycle
{
    void AdvanceCycle(int characterId);          // @生理周期
    void ApplyOvulationDrug(int characterId);    // @排卵誘発剤追加処理
    string FormatSimpleStatus(int characterId);  // @簡易追加情報 (display portion)
    void ResetCycle(int characterId);        // Reset menstrual state (for F822 Pregnancy integration)
}

// IHeartbreakService.cs
public interface IHeartbreakService
{
    void AcquireHeartbreak(int slaveId, int acquisitionMethod, int cheatingPartnerId);
    // acquisitionMethod: 0 = witnessed cheating scene
}
```

New named constants (verify integer values from C:/Era/game/data/CFLAG.yaml, C:/Era/game/CSV/Talent.csv, C:/Era/game/data/BASE.yaml before finalizing):

```csharp
// CharacterFlagIndex.cs additions (F824)
public static readonly CharacterFlagIndex Sleep = new(N);               // 睡眠
public static readonly CharacterFlagIndex SleepDepth = new(N);          // 睡眠深度
public static readonly CharacterFlagIndex Ufufu = new(N);               // うふふ
public static readonly CharacterFlagIndex CurrentLocation = new(N);     // 現在位置
public static readonly CharacterFlagIndex MenstrualCycleFlag = new(352); // 生理周期
public static readonly CharacterFlagIndex OvumLifespan = new(351);       // 卵子生存日数
public static readonly CharacterFlagIndex PillUsed = new(N);             // ピル使用
public static readonly CharacterFlagIndex HeartbreakPartner1 = new(N);   // 傷心相手1
public static readonly CharacterFlagIndex HeartbreakPartner2 = new(N);   // 傷心相手2

// TalentIndex.cs additions (F824)
public static readonly TalentIndex Heartbreak = new(N);    // 傷心
public static readonly TalentIndex DangerDay = new(N);     // 危険日
public static readonly TalentIndex NTR = new(N);           // NTR
public static readonly TalentIndex PublicToilet = new(N);  // 公衆便所
public static readonly TalentIndex Fondness = new(N);      // 親愛
public static readonly TalentIndex Childish = new(N);      // 幼稚
public static readonly TalentIndex CheatingLips = new(N);  // 浮気な唇
public static readonly TalentIndex CheatingPot = new(N);   // 浮気な蜜壷
public static readonly TalentIndex CheatingAnal = new(N);  // 浮気な尻穴

// BaseIndex.cs additions (F824)
public static readonly BaseIndex Anger = new(N);          // 怒り
```

**Note**: All `N` values must be verified from `C:/Era/game/data/CFLAG.yaml`, `C:/Era/game/CSV/Talent.csv`, `C:/Era/game/data/BASE.yaml` before writing. The implementer agent must read these files first.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| IComableUtilities is in `Era.Core.Counter.Comable` namespace, not `Era.Core.Interfaces` | Technical Constraints (C15) | SleepDepth.cs must add `using Era.Core.Counter.Comable;` and inject `IComableUtilities` from that namespace; DI registration must also reference this namespace |
| CFLAG/TALENT integer values for new constants not verified in this design | AC#4 | Implementer must read C:/Era/game/data/CFLAG.yaml, C:/Era/game/CSV/Talent.csv, C:/Era/game/data/BASE.yaml to populate actual integer values before writing CharacterFlagIndex/TalentIndex/BaseIndex additions |
| ISleepDepth, IMenstrualCycle, IHeartbreakService interfaces do not exist yet | AC#1, AC#2, AC#3, AC#5-13, AC#15-21 | Create interfaces in Era.Core.Interfaces/ as part of Task 3 before implementing service classes |
| DI registration (ServiceCollectionExtensions) must register 3 new services | AC#15 (E2E DiResolution) | Add `services.AddSingleton<ISleepDepth, SleepDepth>()`, `IMenstrualCycle`, `IHeartbreakService` in ServiceCollectionExtensions.cs; this is in scope for F824 (F825 handles broader DI integration for the full Phase 22 batch) |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 4 | Read C:/Era/game/data/CFLAG.yaml, C:/Era/game/CSV/Talent.csv, C:/Era/game/data/BASE.yaml and record actual integer indices for all new constants (Sleep, SleepDepth, Ufufu, CurrentLocation, MenstrualCycleFlag, OvumLifespan, PillUsed, HeartbreakPartner1, HeartbreakPartner2, Heartbreak, DangerDay, NTR, PublicToilet, Anger, Pregnancy, Gender, Fondness/親愛, Childish/幼稚, CheatingLips/浮気な唇, CheatingPot/浮気な蜜壷, CheatingAnal/浮気な尻穴) | [I] | [x] |
| 2 | 4 | Add named constants to CharacterFlagIndex.cs (9 constants), TalentIndex.cs (9-11 constants: Heartbreak, DangerDay, NTR, PublicToilet, Fondness, Childish, CheatingLips, CheatingPot, CheatingAnal + Pregnancy/Gender if Task#1 finds them missing), and BaseIndex.cs (1 constant) using integer values from Task 1 | | [x] |
| 3 | 23, 24, 25, 33, 34, 35 | Create ISleepDepth.cs, IMenstrualCycle.cs, and IHeartbreakService.cs interfaces in C:/Era/core/src/Era.Core/Interfaces/ | | [x] |
| 4 | 3, 13, 14, 28, 31 | Implement HeartbreakService.cs in C:/Era/core/src/Era.Core/State/ with constructor injection of IVariableStore and IConsoleOutput; implement AcquireHeartbreak method (affection reduction by tenth, HeartbreakPartner1/2 assignment, Heartbreak talent update, and ERB-equivalent dialogue output via IConsoleOutput from 睡眠深度.ERB:226-243 PRINT lines) | | [x] |
| 5 | 13, 31 | Write HeartbreakServiceTests.cs in C:/Era/core/src/Era.Core.Tests/State/ covering AcquireHeartbreak_ReducesAffectionByTenth, AcquireHeartbreak_SetsHeartbreakPartner1_WhenEmpty, AcquireHeartbreak_SetsHeartbreakPartner2_WhenPartner1Filled, AcquireHeartbreak_SetsHeartbreakTalentPlus1000, AcquireHeartbreak_OutputsExpectedDialogue (ERB-equivalent PRINT content from 睡眠深度.ERB:226-243 via mock IConsoleOutput) | | [x] |
| 6 | 1, 5, 6, 12, 14, 16, 17, 18, 20, 26, 32, 36, 38, 39, 40, 41 | Implement SleepDepth.cs in C:/Era/core/src/Era.Core/State/ with constructor injection of IVariableStore, IEngineVariables, IConsoleOutput, IComableUtilities, ILocationService, IRandomProvider, IHeartbreakService; implement CalculateNoise (SELECTCOM branching), ApplyOrgasmMultiplier (6-tier logic), UpdateSleepDepth, HandleWaking (four-value threshold probability: >200 no-wake, 100-200 medium, <100 easy, 0 always-wake + character-branch reactions), EvictCharacters (CVARSET bulk reset via loop) | | [x] |
| 7 | 5, 6, 12, 20, 36, 38, 39, 40 | Write SleepDepthTests.cs in C:/Era/core/src/Era.Core.Tests/State/ covering all CalculateNoise SELECTCOM branches (8 ranges including 92-94 and 97-99), all ApplyOrgasmMultiplier tier boundaries, four-value WakingProbability with SeededRandomProvider (>200 no-wake, 100-200 medium, <100 easy, 0 always), all WakingReaction character branches (NO:3 with 恋慕→anger + without 恋慕→indifference, NO:5/6, NO:148/149 infant→cry + childish→naive + else→anger, default high-rank, default low-rank), EvictCharacters_BulkReset (verifies CFLAG indices 0-316 zeroed for all characters after CVARSET equivalent), EvictCharacters_LocationBeyondMaxRoom (verifies characters at locations > MaxRoom are evicted via ILocationService while characters within MaxRoom are unaffected), UpdateSleepDepth_KnownState_ProducesERBEquivalentResult (tests full @特殊起床 orchestration: character filtering by location/sleep, RAND increment, noise application, CFLAG write-back), and HandleWaking_KnownState_ProducesERBEquivalentResult (tests @うふふ中起床口上 orchestration: probability → reaction branch → heartbreak invocation) | | [x] |
| 8 | 2, 7, 8, 9, 10, 11, 14, 19, 21, 27, 29, 30, 37 | Implement MenstrualCycle.cs in C:/Era/core/src/Era.Core/State/ with constructor injection of IVariableStore and IRandomProvider; implement AdvanceCycle (15-day state machine with male-guard, pregnancy-skip, daughter-growth-gate, 30%-ovulation probability, forced day-14 ovulation, day-15 reset), ApplyOvulationDrug, implement ResetCycle (zeroes CFLAG:生理周期 for given character, for F822 Pregnancy integration); replace TRYCALL debug hook with ILogger<MenstrualCycle> debug logging; implement FormatSimpleStatus with inline IS_DOUTEI check (TALENT:童貞 > 0 && TALENT:性別 >= 2) | | [x] |
| 9 | 7, 8, 9, 10, 11, 21, 29, 37 | Write MenstrualCycleTests.cs in C:/Era/core/src/Era.Core.Tests/State/ covering AdvanceCycle_Days1to9_SafePeriod, AdvanceCycle_Day10_SetsDangerDay, AdvanceCycle_Day14_ForcedOvulation, AdvanceCycle_Day15_ResetsCycle, AdvanceCycle_DangerDay_30PercentOvulationWithSeededRandom, AdvanceCycle_WhenPregnant_SkipsMenstrualProcessing, AdvanceCycle_DaughterBeforeGrowthStage3_SkipsMenstruation, AdvanceCycle_MaleCharacter_SkipsMenstrualProcessing, ApplyOvulationDrug_SetsDangerDayAndOvulationAndCycleDay12, ResetCycle_ZerosMenstrualCycleFlag, FormatSimpleStatus_MaleCharacter_ReturnsEmpty, FormatSimpleStatus_WithDoutei_ShowsIndicator, FormatSimpleStatus_FemaleWithActiveCycle_ReturnsExpectedString | | [x] |
| 10 | 15 | Register ISleepDepth/SleepDepth, IMenstrualCycle/MenstrualCycle, IHeartbreakService/HeartbreakService as Singleton in ServiceCollectionExtensions.cs (AddEraCore); add Resolve_ISleepDepth, Resolve_IMenstrualCycle, Resolve_IHeartbreakService tests and SleepMenstrualFlow_ServicesResolveAndExecute: resolves all 3 services, calls UpdateSleepDepth with sleeping character, verifies HeartbreakService callable from waking path to E2E test class with `[Trait("Category", "SleepMenstrualE2E")]` | | [x] |
| 11 | 14, 22 | Run dotnet build C:/Era/core/src/Era.Core/ and verify zero warnings (TreatWarningsAsErrors=true); verify no TODO/FIXME/HACK in SleepDepth.cs, MenstrualCycle.cs, HeartbreakService.cs | | [x] |

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
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-824.md Task#1: C:/Era/game/data/CFLAG.yaml, C:/Era/game/CSV/Talent.csv, C:/Era/game/data/BASE.yaml | Actual integer values for all 21 constants (9 CharacterFlagIndex + 9 TalentIndex + Pregnancy/Gender investigation + 1 BaseIndex) (ACTUAL_OUTPUT_VALUE) |
| 2 | implementer | sonnet | feature-824.md Task#2 + Task#1 output | Updated CharacterFlagIndex.cs, TalentIndex.cs, BaseIndex.cs |
| 3 | implementer | sonnet | feature-824.md Task#3 | ISleepDepth.cs, IMenstrualCycle.cs, IHeartbreakService.cs in Era.Core/Interfaces/ (ISleepDepth.cs, IMenstrualCycle.cs, IHeartbreakService.cs in Era.Core/Interfaces/) |
| 4 | implementer | sonnet | feature-824.md Task#4 | HeartbreakService.cs in Era.Core/State/ |
| 5 | implementer | sonnet | feature-824.md Task#5 | HeartbreakServiceTests.cs in Era.Core.Tests/State/ (5 unit tests) |
| 6 | implementer | sonnet | feature-824.md Task#6 | SleepDepth.cs in Era.Core/State/ |
| 7 | implementer | sonnet | feature-824.md Task#7 | SleepDepthTests.cs in Era.Core.Tests/State/ (multiple unit tests including HandleWaking_KnownState_ProducesERBEquivalentResult for @うふふ中起床口上 orchestration) |
| 8 | implementer | sonnet | feature-824.md Task#8 | MenstrualCycle.cs in Era.Core/State/ |
| 9 | implementer | sonnet | feature-824.md Task#9 | MenstrualCycleTests.cs in Era.Core.Tests/State/ (10 unit tests) |
| 10 | implementer | sonnet | feature-824.md Task#10 | Updated ServiceCollectionExtensions.cs, DiResolutionTests.cs, CrossSystemFlowTests.cs |
| 11 | implementer | sonnet | feature-824.md Task#11 | Build verification output + zero-debt grep results |

### Pre-conditions

- F814 is [DONE] (confirmed in Dependencies)
- C:/Era/core/ repo is accessible
- Era.Core.Tests project exists at C:/Era/core/src/Era.Core.Tests/
- GAME_PATH set to C:/Era/game for CSV file access

### Execution Order

Tasks MUST be executed in order (1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10 → 11):

- Task 1 is [I]: implementer reads CSV files and reports concrete integer values; these values are inputs to Task 2
- Task 2 depends on Task 1 output (integer values)
- Task 3 is independent of Task 2 (interfaces use int parameters, not typed index types); can proceed after Task 1 completes
- Tasks 4 and 8 depend on Task 3 (implement interfaces)
- Tasks 5 and 9 depend on Tasks 4 and 8 respectively (test implementations)
- Task 10 depends on Tasks 4, 6, and 8 (services must exist before DI registration)
- Task 11 is always last (build + zero-debt verification)

### Build Verification Steps

After each Task (4, 6, 8, 10), run:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/ --no-incremental'
```

After Tasks 5, 7, 9:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "FullyQualifiedName~SleepDepth|FullyQualifiedName~MenstrualCycle|FullyQualifiedName~Heartbreak"'
```

After Task 10 (E2E):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "Category=SleepMenstrualE2E"'
```

Task 11 (final build):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/ --no-incremental'
```

### Success Criteria

- All 41 ACs have status [x]
- dotnet build exits 0 with zero warnings
- No TODO/FIXME/HACK in SleepDepth.cs, MenstrualCycle.cs, HeartbreakService.cs
- All E2E tests pass

### Error Handling

- If CFLAG.yaml integer value for a constant conflicts with an existing CharacterFlagIndex entry → STOP, report to user
- If IComableUtilities namespace cannot be resolved in SleepDepth.cs → add `using Era.Core.Counter.Comable;` (documented in Upstream Issues)
- If build fails with TreatWarningsAsErrors → fix warning before proceeding to next task (never suppress)

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| DI registration of SleepDepth/MenstrualCycle/HeartbreakService in broader Phase 22 DI integration | F824 registers only own services; F825 owns the full Phase 22 DI integration batch including relationship services | Feature | F825 | - | [x] | 追記済み |
| F822 Pregnancy data coupling: menstrual CFLAG reset contract | 妊娠処理変更パッチ.ERB:278 resets CFLAG:生理周期=0; interface contract for menstrual state reset must be documented for F822 migration | Feature | F822 | - | [x] | 追記済み |
| CVARSET CFLAG,317 bulk reset pattern also used in MOVEMENT.ERB:81 | MOVEMENT.ERB migration must extract BulkResetCharacterFlags as shared IVariableStore method to eliminate duplication (currently 1 C# call site in SleepDepth; MOVEMENT.ERB is unmigrated ERB — extraction applies when MOVEMENT.ERB migrates to C#) | Feature | F826 | - | [x] | 追記済み |
| IS_DOUTEI shared utility extraction needed when additional ERB callers migrate to C# | IS_DOUTEI is defined in COMMON.ERB and called from 14 ERB files (22 call sites). F824 inlines it in MenstrualCycle.FormatSimpleStatus (1 call site). When second C# call site appears, extract to shared utility (e.g., ICharacterUtilities.IsDoutei) | Feature | F826 | - | [x] | 追記済み |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-05 | PHASE_START | orchestrator | Phase 1 Initialize | F824 [REVIEWED] → [WIP] |
<!-- run-phase-1-completed -->
| 2026-03-05 | PHASE_START | orchestrator | Phase 2 Investigation | Codebase verified: all targets clean, deps exist |
<!-- run-phase-2-completed -->
| 2026-03-05 | PHASE_START | orchestrator | Phase 3-4 Implementation | Tasks 1-11 executed |
| 2026-03-05 | TASK_DONE | implementer | Task 1 [I] | CFLAG values: 生理周期=900, 卵子生存日数=1102, 傷心相手2=1199(new) |
| 2026-03-05 | TASK_DONE | implementer | Task 2 | 11 CFLAG + 10 TALENT + 1 BASE constants added; BaseIndex.Satisfaction PRE-EXISTING fix 12→25 |
| 2026-03-05 | TASK_DONE | implementer | Task 3 | ISleepDepth, IMenstrualCycle, IHeartbreakService created |
| 2026-03-05 | TASK_DONE | implementer | Task 4 | HeartbreakService.cs (IEngineVariables added for CALLNAME) |
| 2026-03-05 | TASK_DONE | implementer | Task 5 | HeartbreakServiceTests.cs — 5/5 pass |
| 2026-03-05 | TASK_DONE | implementer | Task 6 | SleepDepth.cs (314 lines, FlagIndex.ActNoise added) |
| 2026-03-05 | TASK_DONE | implementer | Task 7 | SleepDepthTests.cs — 29/29 pass |
| 2026-03-05 | TASK_DONE | implementer | Task 8 | MenstrualCycle.cs (IEngineVariables added for GetCharacterNo) |
| 2026-03-05 | TASK_DONE | implementer | Task 9 | MenstrualCycleTests.cs — 13/13 pass |
| 2026-03-05 | TASK_DONE | implementer | Task 10 | DI registration + 4 E2E tests pass |
| 2026-03-05 | TASK_DONE | implementer | Task 11 | Build 0W/0E, zero debt confirmed |
| 2026-03-05 | ALL_TESTS | orchestrator | 50/50 tests pass | Duration 44ms |
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
| 2026-03-05 | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed — new code, clean patterns) |
<!-- run-phase-5-completed -->
| 2026-03-05 | PHASE_START | orchestrator | Phase 7 Verification | AC lint clean, 41/41 ACs PASS |
<!-- run-phase-7-completed -->
| 2026-03-05 | PHASE_START | orchestrator | Phase 8 Post-Review | READY — quality OK, INTERFACES.md updated |
<!-- run-phase-8-completed -->
| 2026-03-05 | PHASE_START | orchestrator | Phase 9 Report | 0 DEVIATION, 41/41 AC PASS, handoffs transferred |
<!-- run-phase-9-completed -->
| 2026-03-05 | CodeRabbit | 2 Minor (修正済み) | &amp; in comments → fixed (eb2ffb2) |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
- [fix] Phase2-Review iter1: [AC-005] Philosophy Derivation table | Missing interface file-existence ACs and Philosophy Derivation row for 'typed interfaces' claim — added AC#23/24/25, updated Philosophy Derivation and Goal Coverage
- [fix] Phase2-Review iter1: [AC-002] AC#5,6,7,8,9,10,11,12,20,21 Method column | Test filters indistinguishable (shared class-level filter) — specialized per-AC filters for independent verification
- [fix] Phase2-Review iter1: [TSK-002] Philosophy Derivation 'equivalence-tested behavior' row | Missing AC#20, AC#21 from coverage — added to AC Coverage column
- [fix] Phase2-Uncertain iter1: [AC-002] AC#4 grep pattern | Japanese-only pattern fragile if constants use English names — changed to English constant name patterns
- [fix] Phase2-Review iter2: [AC-001] Success Criteria | AC count said 22 but 29 ACs exist — updated to 29
- [fix] Phase2-Uncertain iter2: [AC-005] AC Definition Table | No AC verifies classes implement interfaces — added AC#26/27/28 with Grep for class declaration
- [fix] Phase2-Review iter2: [AC-005] AC Definition Table + Tasks | FormatSimpleStatus untested despite philosophy requiring equivalence — added AC#29 and test cases
- [fix] Phase2-Review iter2: [INV-003] Implementation Contract Execution Order | Task 3 dependency rationale incorrect (interfaces use int, not typed indices) — corrected
- [fix] Phase2-Review iter3: [TSK-002] Tasks table Task 3 AC# | Task 3 claimed AC#1/AC#2 (implementation files) but creates interfaces — changed to 3,23,24,25
- [fix] Phase2-Review iter3: [INV-003] Technical Design CVARSET | Contradictory 'only uふふ-related flags' vs reset all 317 — clarified to match ERB semantics
- [fix] Phase2-Review iter3: [AC-005] Technical Design DI + AC | ILogger<MenstrualCycle> missing from injection list and no AC — added to design and AC#30
- [fix] Phase2-Uncertain iter3: [AC-005] AC#13 + HeartbreakService | No equivalence test for console output — added AC#31
- [fix] Phase2-Review iter4: [AC-002] AC#31 Method | Shared filter with AC#13 (FullyQualifiedName~Heartbreak) — specialized to FullyQualifiedName~AcquireHeartbreak_OutputsExpectedDialogue
- [fix] Phase2-Review iter4: [TSK-002] Task#5 Description | Missing AcquireHeartbreak_OutputsExpectedDialogue test for AC#31 — added to test list
- [fix] Phase2-Review iter4: [AC-002] AC#4 grep pattern | Missing Sleep = constant (1 of 9 CharacterFlagIndex) — added to pattern, updated expected to >=14
- [fix] Phase2-Review iter5: [AC-001] AC Coverage AC#4 | Threshold contradiction (13 vs 14) — updated to >= 14 matching AC table
- [fix] Phase2-Review iter5: [TSK-002] Task 8 AC# | Missing AC#29 (FormatSimpleStatus implementation) — added
- [resolved-invalid] Phase2-Uncertain iter5: [TSK-002] Task 6/8 AC#14 | Implementation tasks legitimately contribute to zero-debt AC#14 by writing debt-free code. Task#11 provides the formal grep verification. Validator iter2 rationale accepted: AC# column indicates contribution, not exclusive ownership.
- [fix] Phase2-Review iter6: [INV-003] Task#6 description | IS_DOUTEI check misplaced in SleepDepth (belongs in MenstrualCycle FormatSimpleStatus) — moved to Task#8
- [fix] Phase2-Review iter6: [AC-002] AC#4 + Task#1 | NTR duplication risk + missing Pregnancy/Gender investigation — added to Task#1 scope, clarified AC#4 note
- [fix] Phase2-Review iter6: [AC-005] AC Definition Table | Missing ILocationService injection AC for SleepDepth — added AC#32
- [fix] Phase2-Review iter7: [CON-002] AC#5 Detail + Task#7 | Missing SELECTCOM ranges 92-94 and 97-99 from C1 constraint — added 2 test methods
- [fix] Phase2-Review iter7: [AC-005] AC Definition Table | No AC verifies interface method signatures — added AC#33/34/35
- [fix] Phase2-Review iter7: [INV-003] Technical Design naming | MenstrualCycle vs MenstrualCycleFlag inconsistency — unified to MenstrualCycleFlag
- [fix] Phase2-Review iter8: [AC-001] C2 + AC#6 | '5 thresholds' vs 6 tiers inconsistency — updated to '6 tier levels'
- [fix] Phase2-Review iter9: [TSK-002] Task#3 AC# | AC#3 (HeartbreakService.cs in State/) misaligned with Task#3 (creates interfaces) — removed, kept in Task#4
- [fix] Phase2-Review iter9: [AC-002] AC#14 glob | Pipe syntax invalid in glob — changed to brace expansion {SleepDepth,MenstrualCycle,HeartbreakService}.cs
- [fix] Phase1-RefCheck iter0: [FMT-003] Links section | F810 referenced in Design Decisions but not in Links/Dependencies/Related Features — added to all three sections
- [fix] Phase2-Review iter1: [FMT-002] Mandatory Handoffs row 3 | Destination ID `(future MOVEMENT migration)` not a valid F-number — changed Destination to Phase, ID to Phase 22 (Game Systems Migration)
- [fix] Phase2-Uncertain iter1: [AC-002] AC#14 glob pattern | Interface files excluded from zero-debt grep — expanded to cover ISleepDepth/IMenstrualCycle/IHeartbreakService.cs
- [fix] Phase2-Review iter1: [AC-005] EvictCharacters bulk reset | No equivalence test for CVARSET CFLAG,317 translation — added AC#36, updated Task#6/7 and Goal Coverage
- [fix] Phase2-Review iter1: [AC-002] AC#15 Method filter | Category=E2E too broad (runs all E2E) — scoped to F824-specific test names
- [fix] Phase2-Review iter2: [AC-005] Goal / Philosophy Derivation | ResetCycle not traceable to Goal or Philosophy — added Goal item 5 and Philosophy Derivation row
- [fix] Phase2-Review iter2: [AC-001] Technical Design CVARSET | Ambiguous 'from 0 to 317' phrasing (could mean 318 slots) — clarified to '317 slots (indices 0 through 316 inclusive)'
- [fix] Phase2-Uncertain iter2: [AC-002] AC#15 filter syntax | Backslash-pipe in markdown table ambiguous for VSTest — changed to Category=SleepMenstrualE2E trait filter
- [fix] Phase2-Review iter3: [AC-005] AC#37 + Goal/Philosophy | ResetCycle has no behavioral test — added AC#37, updated Task#9, Goal Coverage row 5, Philosophy Derivation
- [fix] Phase2-Review iter3: [AC-002] AC#4 pattern + threshold | Pregnancy/Gender constants not verified — added to grep pattern, threshold >=16
- [fix] Phase2-Review iter3: [AC-005] AC#20 + Task#7 | NO:3 else-branch (indifference) untested — added to AC#20 description and Task#7 test list
- [fix] Phase2-Review iter4: [TSK-002] Task#4 AC#/description | AC#31 missing from Task#4, IConsoleOutput dialogue not in description — added AC#31 and dialogue output description
- [fix] Phase2-Review iter4: [AC-001] AC#12/C8/Task#7 | 100-200 medium tier untested, 'three-tier' should be 'four-value' — added 4th test, updated C8/AC#12 descriptions
- [fix] Phase2-Review iter4: [AC-001] Implementation Contract Phase 1 | '14 new constants' vs 16 listed in Task#1 — updated to 16
- [fix] Phase2-Review iter4: [AC-001] Task#2 TalentIndex count | Fixed '4 constants' to '4-6 constants' depending on Task#1 Pregnancy/Gender findings
- [fix] Phase2-Review iter5: [TSK-002] Task#8 AC# | AC#37 missing from Task#8 (implements ResetCycle) — added
- [fix] Phase2-Review iter5: [AC-005] AC#29 coverage | Normal female positive path untested — added FormatSimpleStatus_FemaleWithActiveCycle test
- [fix] Phase2-Review iter6: [AC-005] UpdateSleepDepth | No end-to-end equivalence test for @特殊起床 orchestration — added AC#38, updated Task#6/7 and Goal Coverage
- [fix] Phase2-Review iter6: [AC-001] ApplyOrgasmMultiplier visibility | Unclear if public/private — documented as public non-interface helper on SleepDepth class
- [fix] Phase2-Review iter7: [AC-002] AC#14 glob | Zero-debt grep misses modified type files — expanded to include CharacterFlagIndex/TalentIndex/BaseIndex.cs
- [fix] Phase2-Review iter7: [INV-003] Philosophy Derivation | 'Interface contracts include integration surfaces' not from Philosophy text — removed row (covered by Goal Coverage row 5)
- [fix] Phase2-Review iter7: [FMT-001] Key Decisions | ApplyOrgasmMultiplier public non-interface placement undocumented — added Key Decisions entry
- [fix] Phase2-Review iter8: [TSK-002] Philosophy Derivation equivalence row | AC#36/AC#38 missing — added
- [fix] Phase2-Review iter8: [AC-002] AC#13 filter | Superset of AC#31 filter — changed to exclusion filter (AcquireHeartbreak&!~OutputsExpected)
- [fix] Phase2-Uncertain iter8: [TSK-002] Goal Coverage row 1 | AC#31 (dialogue equivalence) absent — added
- [fix] Phase2-Review iter8: [AC-002] AC#14 glob | Test files uncovered by zero-debt grep — expanded to include SleepDepthTests/MenstrualCycleTests/HeartbreakServiceTests.cs
- [fix] Phase2-Review iter10: [AC-001] AC#15 + Task#10 | Cross-system flow test semantically undefined — added concrete scenario (resolve 3 services, call UpdateSleepDepth, verify HeartbreakService callable from waking path)
- [fix] Phase2-Review iter10: [AC-005] AC Definition Table + Philosophy/Goal Coverage | HandleWaking (@うふふ中起床口上) has no end-to-end orchestration test — added AC#39, updated Task#7, Philosophy Derivation, Goal Coverage
- [fix] Phase3-Maintainability iter10: [CON-002] Key Decisions CVARSET row + Mandatory Handoffs row 3 | Conditional tracking ('if a third call site') with unclear rationale — made unconditional, clarified 1 C# call site vs unmigrated ERB, tracked for MOVEMENT.ERB migration
- [resolved-applied] Phase4-ACCount iter10: [AC-001] AC Definition Table | AC count (39) exceeds soft limit (30) — added deviation comment with justification (2 subsystems + shared service + 15 constraints)
- [fix] Phase5-Feasibility iter10: [FMT-001] AC Definition Table | AC count deviation comment missing — added justification for 39 ACs (2 subsystems + shared service + 15 constraints)
- [fix] Phase2-Review iter1: [AC-002] AC#13 Method column | VSTest filter uses undocumented `!~` operator — replaced with explicit `FullyQualifiedName~AcquireHeartbreak_Reduces|FullyQualifiedName~AcquireHeartbreak_Sets`
- [fix] Phase2-Review iter1: [AC-001] AC#15 Description | DI resolution tests not explicitly enumerated — listed all 4 tests with trait requirement
- [fix] Phase2-Review iter2: [TSK-002] Task#6 AC# + Description | AC#39 missing from Task#6 AC# column, 'three-tier' should be 'four-value' — added AC#39 and corrected probability description
- [fix] Phase2-Review iter2: [TSK-002] Task#7 Description | NO:148/149 else→anger test path missing — added explicit sub-branch enumeration (infant→cry + childish→naive + else→anger)
- [fix] Phase2-Review iter3: [INV-003] Philosophy Derivation table | CP-2 E2E checkpoint row not from Philosophy text — removed (already in Goal Coverage row)
- [fix] Phase2-Review iter4: [AC-005] AC#4 + Named Constants Strategy + Task#1/2 | 5 TALENTs referenced in ERB missing from constants (Fondness/親愛, Childish/幼稚, CheatingLips/浮気な唇, CheatingPot/浮気な蜜壷, CheatingAnal/浮気な尻穴) — added to strategy, AC#4 pattern, Task#1/2, threshold >=16→>=21
- [fix] Phase2-Review iter5: [AC-001] AC#4 Note | Contradictory >=16 fallback vs >=21 threshold — removed fallback, clarified pre-existing constants contribute to >=21
- [fix] Phase2-Review iter5: [AC-002] AC#14 glob | ServiceCollectionExtensions.cs excluded from zero-debt grep despite Task#10 modifying it — added
- [fix] Phase2-Review iter5: [INV-003] Philosophy Derivation table | HeartbreakService extraction row from Goal not Philosophy — removed (covered by Goal Coverage)
- [fix] Phase2-Review iter6: [FMT-001] TalentIndex code block | Missing 5 constants (Fondness, Childish, CheatingLips, CheatingPot, CheatingAnal) from Interfaces/Data Structures — added
- [fix] Phase2-Review iter6: [AC-001] Implementation Contract Phase 1 | Stale '16 constants' count — updated to 21
- [fix] Phase2-Review iter7: [AC-005] AC Definition Table + Task#6/7 | EvictCharacters eviction logic (MaxRoom, location reset) untested — added AC#40, updated Philosophy Derivation/Goal Coverage/Task#6/7
- [fix] Phase2-Review iter8: [AC-001] Implementation Contract + Success Criteria | Stale counts: Phase 5 (4→5 tests), Phase 9 (9→10 tests), AC total (39→40) — updated all
- [fix] Phase3-Maintainability iter9: [AC-005] DI injection + AC Definition Table | SleepDepth missing IHeartbreakService injection for HandleWaking heartbreak path — added to DI list, added AC#41, updated Task#6
- [fix] Phase3-Maintainability iter9: [INV-003] Philosophy Derivation table | Named constants row from Goal not Philosophy — removed (covered by Goal Coverage row 2)
- [fix] Phase2-Review iter1: [FMT-002] Goal Coverage Verification | (implicit) Goal Item values instead of numeric identifiers — assigned sequential numbers 6-11
- [fix] Phase2-Review iter1: [FMT-002] Review Notes | All 50+ [fix] entries missing [{category-code}] per template format — added category codes to all entries
- [fix] Phase3-Maintainability iter2: [SCP-004] Mandatory Handoffs | IS_DOUTEI inline implementation has no tracking for future extraction (14 ERB files, 22 call sites) — added Mandatory Handoffs row for Phase 22
- [fix] Phase2-Review iter3: [TSK-002] Task#9 Description | FormatSimpleStatus test methods missing from description despite AC#29 in AC# column — added 3 test methods
- [fix] Phase5-Feasibility iter4: [INV-003] Task#1/Design/Contract | CFLAG.csv and BASE.csv references incorrect — corrected file extensions to .yaml
- [fix] Phase5-Feasibility iter5: [INV-003] Task#1/Design/Contract | File paths incorrect — CFLAG.yaml and BASE.yaml are in C:/Era/game/data/ not C:/Era/game/CSV/, Talent.csv is capital T — corrected all paths to actual filesystem locations
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning — created this feature as DRAFT
- [Related: F819](feature-819.md) - Clothing System — sibling Phase 22 feature (no call dependency)
- [Related: F821](feature-821.md) - Weather System — sibling Phase 22 feature (no call dependency)
- [Related: F822](feature-822.md) - Pregnancy System — data coupling via CFLAG:生理周期 reset on conception
- [Related: F823](feature-823.md) - Room & Stain System — sibling Phase 22 feature (no call dependency)
- [Successor: F825](feature-825.md) - Relationships & DI Integration — depends on F824 for service registration
- [Successor: F826](feature-826.md) - Post-Phase Review — depends on F824 completion
- [Related: F810](feature-810.md) - IComableUtilities.CanMove — basis for SleepDepth injection decision
