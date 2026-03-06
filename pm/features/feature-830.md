# Feature 830: Trigger-Gated Shared Utility Extractions (BulkResetCharacterFlags & IsDoutei)

## Status: [CANCELLED]
<!-- fl-reviewed: 2026-03-06T00:54:50Z -->

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

## Type: engine

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->
## Background

### Philosophy (Mid-term Vision)
Shared utility methods in Era.Core must be extracted into reusable interfaces/extensions when a second C# call site confirms the usage pattern, ensuring the codebase follows the SSOT principle for common game logic (virginity checks, bulk flag operations) rather than duplicating inline implementations across migration features.

### Problem (Current Issue)
Two shared utility methods were inlined during F824 (Phase 22 Sleep & Menstrual migration) because only one C# call site existed at the time. The inlined implementations are: (1) `ResetUfufu()` in `Era.Core/State/SleepDepth.cs:340-351`, a private method implementing `CVARSET CFLAG,317` as a loop zeroing indices 0-316 for all characters, called only from `HandleWaking()` at line 270-272; and (2) `isDoutei` in `Era.Core/State/MenstrualCycle.cs:200-201`, an inline boolean expression `maleVirginity > 0 && gender >= 2` with a single call site. Neither extraction can proceed because the trigger conditions are unmet: MOVEMENT.ERB (the second `CVARSET CFLAG,317` call site) is scheduled for Phase 30 (`docs/architecture/migration/phase-28-34-integration.md:662`), and no second C# call site for IS_DOUTEI exists (grep for `MaleVirginity.*>.*0` in Era.Core found only MenstrualCycle.cs:201). The feature was created as a placeholder by F829 Task 3 to track OB-06 and OB-07 from F826 Mandatory Handoffs.

### Goal (What to Achieve)
When trigger conditions are met, extract BulkResetCharacterFlags into an `IVariableStoreExtensions` extension method (not interface method, to avoid forcing all IVariableStore implementations to change) and extract IsDoutei into `ICommonFunctions` (which already hosts gender utility methods, unlike the non-existent ICharacterUtilities referenced in the original DRAFT). The two extractions are independently triggerable: OB-06 fires when MOVEMENT.ERB migration creates a second BulkReset call site; OB-07 fires when any ERB-to-C# migration produces a second IS_DOUTEI C# call site (22 ERB call sites across 14 files are candidates).

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are BulkReset and IsDoutei inlined? | F824 migration inlined them because only one C# call site existed for each | `Era.Core/State/SleepDepth.cs:340-351`, `Era.Core/State/MenstrualCycle.cs:200-201` |
| 2 | Why was only one call site created? | Phase 22 migrated only Sleep & Menstrual systems; other ERB callers remain unmigrated | `pm/features/feature-824.md` |
| 3 | Why can't extraction proceed now? | Project design principle requires a second C# call site to confirm the pattern before extraction (YAGNI) | `pm/features/feature-829.md:437` |
| 4 | Why is the second call site so far away? | MOVEMENT.ERB (BulkReset's second caller) is scheduled for Phase 30, 8 phases from current Phase 22 | `docs/architecture/migration/phase-28-34-integration.md:662` |
| 5 | Why (Root)? | The migration roadmap orders phases by functional domain, placing MOVEMENT.ERB in the distant "Miscellaneous Systems" phase, making cross-domain utility extraction a future concern by design | `docs/architecture/migration/full-csharp-architecture.md` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Two utility methods are duplicated inline instead of shared | Migration phase ordering places the second call sites in distant future phases, preventing extraction trigger |
| Where | `SleepDepth.cs:340-351` and `MenstrualCycle.cs:200-201` | Phase roadmap: MOVEMENT.ERB at Phase 30; IS_DOUTEI callers scattered across Phases 23-34 |
| Fix | Inline the methods now (band-aid; violates YAGNI) | Wait for trigger, then extract to extension method / ICommonFunctions |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F824 | [DONE] | Original source -- inlined both BulkReset and IS_DOUTEI during Phase 22 migration |
| F826 | [DONE] | Post-Phase Review that generated Mandatory Handoffs containing OB-06/OB-07 |
| F829 | [DONE] | Deferred Obligations Consolidation -- routed OB-06/OB-07 to F830 |
| F827 | [DRAFT] | Phase 23 Planning -- next phase, but MOVEMENT.ERB is Phase 30 |
| F831 | [DONE] | Roslynator Investigation -- sibling from F829, no code dependency |
| F833 | [WIP] | IEngineVariables Indexed Methods Stubs -- sibling from F829, no code dependency |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| OB-06 trigger (MOVEMENT.ERB second call site) | NOT MET | MOVEMENT.ERB is Phase 30; currently at Phase 22 (`phase-28-34-integration.md:662`) |
| OB-07 trigger (second IS_DOUTEI C# call site) | NOT MET | Grep `MaleVirginity.*>.*0` in Era.Core: only 1 hit at MenstrualCycle.cs:201 |
| IVariableStore exists for BulkReset hosting | FEASIBLE | `Era.Core/Interfaces/IVariableStore.cs` exists; extension method pattern established in `IVariableStoreExtensions.cs` |
| ICommonFunctions exists for IsDoutei hosting | FEASIBLE | `Era.Core/Interfaces/ICommonFunctions.cs` exists with gender utilities |
| Implementation complexity (when triggered) | LOW | Simple extraction of existing inline code |

**Verdict**: FEASIBLE (trigger-gated) -- Both trigger conditions are currently unmet. Feature is [PROPOSED] (spec-ready for review); will enter /run when at least one trigger fires. Implementation complexity is LOW when triggered.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core interfaces | MEDIUM | BulkReset as extension method avoids interface change; IsDoutei on ICommonFunctions adds one method |
| Era.Core.Tests | LOW | Existing `SleepDepthTests.cs:760` validates bulk reset; test updates needed for extraction |
| Cross-repo coordination | MEDIUM | All changes are in core repo (`C:\Era\core`), not devkit; feature must coordinate |
| Migration roadmap | LOW | No impact on other phases; extractions happen opportunistically |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Core repo changes only | F830 targets `C:\Era\core\` | devkit feature must coordinate cross-repo changes |
| BulkReset uses extension methods pattern | `IVariableStoreExtensions.cs` (F782) | Extension method preferred over interface method to avoid forcing all implementations |
| `ResetUfufu()` is private | `SleepDepth.cs:340` | Extraction requires changing visibility or delegating to shared method |
| CVARSET zeros indices 0-316 via raw index loop | `SleepDepth.cs:348` | Bulk operation needs efficient implementation preserving exact semantics |
| ICharacterUtilities does not exist | Grep: zero matches in Era.Core | Must use ICommonFunctions (has gender utilities) instead |
| IsDoutei needs both ICommonFunctions and IVariableStore access | MenstrualCycle.cs:200-201 | ICommonFunctions is stateless; IsDoutei parameters must include variable store or talent value |
| IVariableStore has 25+ implementations/mocks | Grep across Era.Core | Extension method avoids forcing changes to all implementations |
| ICommonFunctions has ~15 sealed test stubs + ~5 Moq mocks in Era.Core.Tests | Grep `: ICommonFunctions` across Era.Core.Tests | Interface member chosen (semantic fit); Moq mocks auto-handle; ~15 sealed stubs need `IsDoutei` method added |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Feature remains [DRAFT] indefinitely | HIGH | LOW | Acceptable by design; trigger-gated obligations are expected to be long-lived |
| ICommonFunctions placement adds dependency complexity | MEDIUM | MEDIUM | IsDoutei method signature takes primitive parameters (virginity value, gender) to avoid IVariableStore coupling |
| BulkReset extension method on IVariableStore may not have access to CharaNum | LOW | MEDIUM | Extension method takes count parameter; caller provides IEngineVariables.GetCharaNum() |
| IS_DOUTEI inline may contain semantic inversion bug (1/3 explorers flagged) | MEDIUM | HIGH | Out of scope for F830; if confirmed, track as separate F824 bug fix feature |
| OB-07 triggers before OB-06 (IS_DOUTEI has more ERB call sites) | MEDIUM | LOW | Both extractions are independent; either can proceed alone |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BulkReset call sites | `Grep(Era.Core/, pattern="CVARSET\|BulkReset\|ResetUfufu")` | 1 (SleepDepth.cs) | Trigger fires when count >= 2 |
| IS_DOUTEI C# call sites | `Grep(Era.Core/, pattern="MaleVirginity.*>.*0\|IsDoutei")` | 1 (MenstrualCycle.cs) | Trigger fires when count >= 2 |

**Baseline File**: `_out/tmp/baseline-830.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Both extractions are trigger-gated; neither trigger is currently met | F829 OB-06/OB-07 | ACs must verify trigger condition as precondition before extraction proceeds |
| C2 | BulkReset must preserve CVARSET CFLAG,317 semantics (zero indices 0-316 for all characters) | `SleepDepth.cs:342-350` | Equivalence test required comparing before/after bulk reset behavior |
| C3 | IS_DOUTEI ERB definition: `HAS_PENIS(ARG) && !TALENT:ARG:童貞` (reference) | `COMMON.ERB:208-215`, `MenstrualCycle.cs:200-201` | Extraction must preserve existing C# inline semantics without behavioral change; ERB semantic discrepancy deferred to F840 per Risk row 4 |
| C4 | IVariableStore has 25+ implementations/mocks | Grep across Era.Core | Extension method on IVariableStoreExtensions preferred over interface method |
| C5 | ICharacterUtilities does not exist; ICommonFunctions is correct host | Grep: zero matches for ICharacterUtilities | ACs must reference ICommonFunctions, not ICharacterUtilities |
| C6 | Existing test validates bulk reset behavior | `SleepDepthTests.cs:760` | Regression test must be maintained or updated during extraction |
| C7 | Era.Core NuGet package version bump required | Cross-repo dependency | AC must verify package version update after extraction |
| C8 | ICommonFunctions has ~15 sealed test stubs + ~5 Moq mocks | Grep `: ICommonFunctions` in Era.Core.Tests | Sealed stubs must add `IsDoutei` method (Moq mocks auto-handle); interface member chosen for semantic fit over extension method |

### Constraint Details

**C1: Trigger-Gated Execution**
- **Source**: F829 routing analysis of OB-06/OB-07 from F826 Mandatory Handoffs
- **Verification**: Grep for second call site before proceeding with extraction
- **AC Impact**: Feature cannot produce ACs until at least one trigger fires; when triggered, ACs must verify the trigger condition was met

**C2: BulkReset Semantic Equivalence**
- **Source**: `SleepDepth.cs:342-350` implements CVARSET CFLAG,317 as loop zeroing indices 0-316
- **Verification**: Compare output of private ResetUfufu() with extracted extension method
- **AC Impact**: Equivalence test must cover all-characters scope and exact index range

**C3: IS_DOUTEI ERB Reference**
- **Source**: `ERB/COMMON.ERB:208-215` defines IS_DOUTEI as `HAS_PENIS(ARG) && !TALENT:ARG:童貞` with warning "間違えやすいけど、TALENT:童貞が 0 だと童貞"
- **Verification**: Confirm extraction preserves existing C# inline semantics (maleVirginity > 0 && gender >= 2) without behavioral change
- **AC Impact**: Extraction must preserve existing C# inline semantics; ERB semantic discrepancy (TALENT:童貞=0 means doutei) deferred to F840 per Risk row 4

**C4: Extension Method Pattern**
- **Source**: `IVariableStoreExtensions.cs` establishes pattern (F782); 25+ IVariableStore implementations would need changes if interface method used
- **Verification**: Verify extension method compiles and is callable from SleepDepth
- **AC Impact**: AC must verify BulkReset is an extension method, not an interface member

**C5: ICommonFunctions Host**
- **Source**: ICharacterUtilities does not exist anywhere in Era.Core (grep: zero matches); ICommonFunctions already has gender utility methods
- **Verification**: Grep for ICommonFunctions in Era.Core confirms existence
- **AC Impact**: ACs referencing IsDoutei must target ICommonFunctions, not ICharacterUtilities

**C6: Existing Regression Test**
- **Source**: `SleepDepthTests.cs:760` test `EvictCharacters_BulkReset_ZeroesCflag0to316_ForAllCharacters` validates bulk reset via HandleWaking
- **Verification**: Run test after extraction; must pass unchanged
- **AC Impact**: AC#11 verifies existing test still passes after BulkReset extraction

**C7: NuGet Package Version Bump**
- **Source**: Era.Core is consumed as a NuGet package (`Era.Core 1.0.0`); interface changes require version increment
- **Verification**: Grep `<Version>` in Era.Core.csproj confirms version tag present and updated
- **AC Impact**: AC#13 verifies version bump after extraction

**C8: ICommonFunctions Test Stub Impact**
- **Source**: Grep `: ICommonFunctions` in Era.Core.Tests found ~15 sealed test stubs and ~5 Moq mocks
- **Verification**: Sealed stubs will fail to compile until `IsDoutei` method is added; Moq mocks auto-handle new interface members
- **AC Impact**: Task#6 (IsDoutei interface addition) includes updating all ~15 sealed test stubs; AC#16 (MenstrualCycle regression) validates tests pass after changes

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F824 | [DONE] | Original source -- inlined both BulkReset and IS_DOUTEI during Phase 22 migration |
| Related | F826 | [DONE] | Post-Phase Review that generated Mandatory Handoffs containing OB-06/OB-07 |
| Related | F829 | [DONE] | Deferred Obligations Consolidation -- routed OB-06/OB-07 to F830 |
| Related | F827 | [DRAFT] | Phase 23 Planning -- next phase, but MOVEMENT.ERB is Phase 30 |
| Related | F831 | [DONE] | Roslynator Investigation -- sibling from F829, no code dependency |
| Related | F833 | [WIP] | IEngineVariables Indexed Methods Stubs -- sibling from F829, no code dependency |

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
| "must be extracted into reusable interfaces/extensions when a second C# call site confirms the usage pattern" | BulkResetCharacterFlags must exist as extension method on IVariableStoreExtensions when trigger fires | AC#2, AC#3 |
| "must be extracted into reusable interfaces/extensions when a second C# call site confirms the usage pattern" | IsDoutei must exist as method on ICommonFunctions when trigger fires | AC#6, AC#7 |
| "ensuring the codebase follows the SSOT principle for common game logic" | Private ResetUfufu removed from SleepDepth after extraction (no duplication) | AC#4 |
| "ensuring the codebase follows the SSOT principle for common game logic" | Inline isDoutei expression removed from MenstrualCycle after extraction (no duplication) | AC#9 |
| "rather than duplicating inline implementations across migration features" | Second call site uses extracted method, not a new inline copy | AC#5, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | OB-06 trigger: second bulk CFLAG reset call site exists | code | Grep(Era.Core/, pattern="ResetUfufu\|CVARSET.*CFLAG\|BulkResetCharacterFlags") | gte | 2 | [ ] |
| 2 | BulkResetCharacterFlags is extension method on IVariableStoreExtensions | code | Grep(Era.Core/Interfaces/IVariableStoreExtensions.cs, pattern="BulkResetCharacterFlags.*this IVariableStore") | matches | - | [ ] |
| 3 | BulkResetCharacterFlags zeros CFLAG 0-316 for all characters | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~BulkResetCharacterFlags" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 4 | Private ResetUfufu removed from SleepDepth | code | Grep(Era.Core/State/SleepDepth.cs, pattern="private.*ResetUfufu") | not_matches | `private.*ResetUfufu` | [ ] |
| 5 | SleepDepth.HandleWaking calls BulkResetCharacterFlags extension | code | Grep(Era.Core/State/SleepDepth.cs, pattern="BulkResetCharacterFlags") | matches | `BulkResetCharacterFlags` | [ ] |
| 6 | IsDoutei method declared on ICommonFunctions | code | Grep(Era.Core/Interfaces/ICommonFunctions.cs, pattern="IsDoutei") | matches | `IsDoutei` | [ ] |
| 7 | IsDoutei preserves C# inline semantics (maleVirginity > 0 && gender >= 2) | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~IsDoutei" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 8 | OB-07 trigger: second IS_DOUTEI C# call site exists | code | Grep(Era.Core/, pattern="MaleVirginity.*>.*0") | gte | 2 | [ ] |
| 9 | Inline isDoutei expression removed from MenstrualCycle | code | Grep(Era.Core/State/MenstrualCycle.cs, pattern="maleVirginity > 0 && gender >= 2") | not_matches | `maleVirginity > 0 && gender >= 2` | [ ] |
| 10 | MenstrualCycle calls ICommonFunctions.IsDoutei instead of inline | code | Grep(Era.Core/State/MenstrualCycle.cs, pattern="IsDoutei") | matches | `IsDoutei` | [ ] |
| 11 | Existing SleepDepth bulk reset test passes after extraction | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~EvictCharacters_BulkReset_ZeroesCflag0to316" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 12 | Era.Core builds without errors after extraction | build | dotnet build Era.Core/ --no-restore | succeeds | - | [ ] |
| 13 | Era.Core NuGet package version bumped (not 1.0.0) | code | Grep(Era.Core/Era.Core.csproj, pattern="<Version>1\.0\.0</Version>") | not_matches | `<Version>1.0.0</Version>` | [ ] |
| 14 | IS_DOUTEI semantic investigation feature F840 created as [DRAFT] | file | Grep(pm/features/feature-840.md, pattern="IS_DOUTEI\|IsDoutei\|semantic.*inversion") | matches | `IS_DOUTEI\|IsDoutei\|semantic.*inversion` | [ ] |
| 15 | F840 registered in index-features.md Active Features table | code | Grep(pm/index-features.md, pattern="F840") | matches | `F840` | [ ] |
| 16 | Existing MenstrualCycle tests pass after IsDoutei extraction | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~MenstrualCycle" --blame-hang-timeout 10s | succeeds | - | [ ] |
| 17 | BulkResetCharacterFlags handles zero charCount without error | test | dotnet test Era.Core.Tests --filter "FullyQualifiedName~BulkResetCharacterFlags_ZeroCharCount" --blame-hang-timeout 10s | succeeds | - | [ ] |

### AC Details

**AC#1: OB-06 trigger: second bulk CFLAG reset call site exists**
- **Test**: `Grep(Era.Core/, pattern="ResetUfufu|CVARSET.*CFLAG|BulkResetCharacterFlags")`
- **Expected**: >= 2 matches (baseline: 1 match for `ResetUfufu` in SleepDepth.cs; trigger fires when a second match appears from another feature's bulk CFLAG reset implementation, or after extraction when `BulkResetCharacterFlags` replaces inline code)
- **Derivation**: OB-06 fires when MOVEMENT.ERB migration (Phase 30) or another feature creates a second C# call site for bulk CFLAG reset. The grep uses the underlying pattern names (`ResetUfufu`, `CVARSET.*CFLAG`) rather than the extracted method name, because the trigger must be detectable BEFORE extraction creates `BulkResetCharacterFlags`.
- **Rationale**: Trigger precondition per C1 -- extraction must not proceed without confirmed second call site.

**AC#8: OB-07 trigger: second IS_DOUTEI C# call site exists**
- **Test**: `Grep(Era.Core/, pattern="MaleVirginity.*>.*0")`
- **Expected**: >= 2 matches (baseline: 1 match for `MaleVirginity > 0` in MenstrualCycle.cs; trigger fires when a second match appears from another feature's IS_DOUTEI implementation)
- **Derivation**: OB-07 fires when any ERB-to-C# migration produces a second IS_DOUTEI C# call site. The grep uses the underlying pattern (`MaleVirginity.*>.*0`) rather than only the extracted method name, because the trigger must be detectable BEFORE extraction creates `IsDoutei`.
- **Rationale**: Trigger precondition per C1 -- extraction must not proceed without confirmed second call site.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract BulkResetCharacterFlags into IVariableStoreExtensions extension method | AC#1, AC#2, AC#3, AC#4, AC#5 |
| 2 | Extract IsDoutei into ICommonFunctions | AC#6, AC#7, AC#8, AC#9, AC#10 |
| 3 | Two extractions are independently triggerable | AC#1 (OB-06 trigger), AC#8 (OB-07 trigger) |
| 4 | Existing tests maintained / regression-safe | AC#11, AC#12, AC#16, AC#17 |
| 5 | Era.Core NuGet package version bump | AC#13 |

> **Note**: AC#14, AC#15 (F840 creation and index registration for IS_DOUTEI semantic investigation) derive from "Track What You Skip" project principle and Mandatory Handoffs protocol, not from the Goal section.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature uses a trigger-gated extraction approach: each obligation (OB-06 and OB-07) is
independently dormant until its trigger condition fires. Neither extraction produces any code
change until a second C# call site is confirmed by grep. When triggered, each extraction is a
mechanical refactoring: move the existing inline code to the designated host (IVariableStoreExtensions
for BulkReset, ICommonFunctions for IsDoutei), update the original call site to delegate to the
new shared method, and remove the now-redundant inline.

**OB-06 (BulkResetCharacterFlags)**: When MOVEMENT.ERB migration (or any other feature) creates a
second C# call site for bulk CFLAG reset, the private `ResetUfufu()` method in SleepDepth.cs is
extracted as a public static extension method `BulkResetCharacterFlags(this IVariableStore, int charCount)`
on the existing `IVariableStoreExtensions` class. The `charCount` parameter is provided by the
caller (SleepDepth has `_engine.GetCharaNum()`; the second call site will supply its own count
the same way). The body loops `charId` from 0 to `charCount-1` and `flagIdx` from 0 to 316
inclusive, calling `SetCFlag`. The private `ResetUfufu()` is replaced with a single-line
delegation to the extension. The existing test at SleepDepthTests.cs:760 exercises this code
path through `HandleWaking()` and will continue to pass without modification after extraction.

**OB-07 (IsDoutei)**: When any migration creates a second C# call site for `IS_DOUTEI`, the
inline boolean in MenstrualCycle.cs:201 is extracted as `IsDoutei(int maleVirginity, int gender)`
declared on `ICommonFunctions` and implemented in `CommonFunctions`. The signature uses primitives
to avoid IVariableStore coupling (consistent with all other ICommonFunctions members: `HasPenis`,
`HasVagina`, `IsFemale`). The inline `bool isDoutei = maleVirginity > 0 && gender >= 2` in
MenstrualCycle is replaced with `_common.IsDoutei(maleVirginity, gender)`. MenstrualCycle must
gain an `ICommonFunctions _common` field + constructor injection (currently not present).

Both extractions complete with a NuGet version bump in Era.Core.csproj.

The 17 ACs are ordered so that ACs#1-5 cover OB-06 and ACs#6-10 cover OB-07; ACs#11-13 cover
regression safety and package versioning; ACs#14-15 cover the IS_DOUTEI semantic investigation
handoff (F840); AC#16 covers MenstrualCycle regression; AC#17 covers BulkReset boundary case
(charCount=0). Each extraction can be verified and committed independently.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | When OB-06 trigger fires: grep confirms >= 2 hits for `ResetUfufu\|CVARSET.*CFLAG\|BulkResetCharacterFlags` in Era.Core (baseline 1 in SleepDepth + second call site from trigger feature) |
| 2 | Add `public static void BulkResetCharacterFlags(this IVariableStore variables, int charCount)` to IVariableStoreExtensions.cs; grep confirms `BulkResetCharacterFlags.*this IVariableStore` present |
| 3 | Write xUnit tests: `BulkResetCharacterFlags_ZeroesCflag0to316_ForAllCharacters` (mock IVariableStore, charCount=2, assert all CFLAG 0-316 zeroed) and `BulkResetCharacterFlags_ZeroCharCount_NoOp` (charCount=0, verify no SetCFlag calls) |
| 4 | Remove private `ResetUfufu()` method body and declaration from SleepDepth.cs; grep confirms `private.*ResetUfufu` no longer present |
| 5 | Replace `ResetUfufu()` call at SleepDepth.cs:271 with `_variables.BulkResetCharacterFlags(_engine.GetCharaNum())`; grep confirms `BulkResetCharacterFlags` present in SleepDepth.cs |
| 6 | When OB-07 trigger fires: add `bool IsDoutei(int maleVirginity, int gender);` to ICommonFunctions.cs; grep confirms `IsDoutei` present |
| 7 | Implement `IsDoutei` in CommonFunctions.cs with body `return maleVirginity > 0 && gender >= 2;`; write xUnit test `IsDoutei_PreservesCSharpInlineSemantics` covering male-virgin (true), female (false), non-virgin (false) cases |
| 8 | When OB-07 trigger fires: grep confirms >= 2 hits for `MaleVirginity.*>.*0` in Era.Core (baseline 1 in MenstrualCycle + second call site from trigger feature) |
| 9 | Remove inline `bool isDoutei = maleVirginity > 0 && gender >= 2;` from MenstrualCycle.cs:201; grep confirms `maleVirginity > 0 && gender >= 2` no longer present |
| 10 | Replace inline with `_common.IsDoutei(maleVirginity, gender)` in MenstrualCycle; inject `ICommonFunctions` via constructor; grep confirms `IsDoutei` present in MenstrualCycle.cs |
| 11 | Run `dotnet test Era.Core.Tests --filter "FullyQualifiedName~EvictCharacters_BulkReset_ZeroesCflag0to316"` after OB-06 extraction; test must pass unchanged |
| 12 | Run `dotnet build Era.Core/` after all changes; output must contain `Build succeeded` |
| 13 | Bump `<Version>` in Era.Core.csproj (e.g., 1.0.0 → 1.1.0) after extraction; grep confirms version is no longer 1.0.0 |
| 14 | Create feature-840.md [DRAFT] with Background describing IS_DOUTEI semantic inversion (ERB TALENT:童貞=0 vs C# maleVirginity > 0) |
| 15 | Register F840 in index-features.md Active Features table with [DRAFT] status |
| 16 | Run `dotnet test Era.Core.Tests --filter "FullyQualifiedName~MenstrualCycle"` after OB-07 extraction; existing MenstrualCycle tests must pass after constructor injection change |
| 17 | Write xUnit test `BulkResetCharacterFlags_ZeroCharCount_NoOp` (charCount=0, verify no SetCFlag calls); covered by Task#3 test suite |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|--------------------|----------|-----------|
| BulkReset host: interface method vs extension method | A: `IVariableStore` interface method, B: `IVariableStoreExtensions` static extension | B: Extension method | C4: 25+ IVariableStore implementations would require changes if interface method added; extension method pattern already established in F782 |
| BulkReset signature: pass charCount vs IEngineVariables | A: `(this IVariableStore, IEngineVariables)`, B: `(this IVariableStore, int charCount)` | B: primitive int | Extension methods should not depend on other interfaces; caller supplies `_engine.GetCharaNum()` directly — keeps extension stateless |
| IsDoutei host: ICommonFunctions vs new ICharacterUtilities | A: `ICommonFunctions`, B: new `ICharacterUtilities` | A: ICommonFunctions | C5: ICharacterUtilities does not exist; ICommonFunctions already has gender utilities (HasPenis, HasVagina, IsFemale); adding one more gender-keyed predicate fits the existing domain |
| IsDoutei signature: primitives vs CharacterId+IVariableStore | A: `(int maleVirginity, int gender)`, B: `(CharacterId, IVariableStore)` | A: primitives | ICommonFunctions is stateless (no store access); all existing methods take primitive int; call site already has the resolved values available |
| IsDoutei semantic bug fix: fix now vs scope-defer | A: Fix inline to `!= 0` per ERB spec TALENT:童貞=0 means doutei, B: Preserve inline semantics | B: Preserve inline | Risk row 4 explicitly defers semantic inversion bug to a separate feature; F830 extracts without behavioral change |
| MenstrualCycle ICommonFunctions injection: field vs method param | A: constructor-injected `ICommonFunctions _common` field, B: pass as method parameter | A: constructor injection | Consistent with SleepDepth pattern; avoids leaking interface dependency into public method signatures |
| Trigger independence: implement both together vs independently | A: Wait for both triggers before implementing, B: Implement each extraction independently when its trigger fires | B: Independent | OB-07 has 22 ERB call sites (higher likelihood of early trigger); OB-06 is Phase 30; coupling them would block OB-07 indefinitely |

### Interfaces / Data Structures

**OB-06: Extension method addition to IVariableStoreExtensions**

```csharp
// Era.Core/Interfaces/IVariableStoreExtensions.cs
// Added when OB-06 trigger fires (second BulkReset call site confirmed)

/// <summary>
/// Zeroes CFLAG indices 0-316 for all characters (charCount characters).
/// Equivalent to ERB: CVARSET CFLAG, 317 — extracted from SleepDepth.ResetUfufu() (F830).
/// </summary>
/// <param name="charCount">Number of characters to reset (caller provides IEngineVariables.GetCharaNum())</param>
public static void BulkResetCharacterFlags(this IVariableStore variables, int charCount)
{
    // ERB: CVARSET CFLAG,317 — count=317 means indices 0-316
    const int CvarsetResetCount = 317;

    for (int charId = 0; charId < charCount; charId++)
    {
        for (int flagIdx = 0; flagIdx < CvarsetResetCount; flagIdx++)
        {
            variables.SetCFlag(charId, new CharacterFlagIndex(flagIdx), 0);
        }
    }
}
```

**OB-07: Interface method addition to ICommonFunctions**

```csharp
// Era.Core/Interfaces/ICommonFunctions.cs
// Added when OB-07 trigger fires (second IsDoutei call site confirmed)

/// <summary>
/// Determines if a character is doutei (male virgin).
/// Preserves C# inline semantics from MenstrualCycle.cs:201 (maleVirginity > 0 && gender >= 2).
/// WARNING: May not match ERB IS_DOUTEI semantics (COMMON.ERB:208-215 uses TALENT:童貞=0 means doutei).
/// See F840 for semantic inversion investigation. Bug tracking deferred per F830 Risk row 4.
/// </summary>
/// <param name="maleVirginity">Value of TALENT:童貞 (MaleVirginity talent)</param>
/// <param name="gender">Gender value (1=female, 2=male, 3=futanari)</param>
bool IsDoutei(int maleVirginity, int gender);
```

```csharp
// Era.Core/Common/CommonFunctions.cs
public bool IsDoutei(int maleVirginity, int gender)
{
    return maleVirginity > 0 && gender >= 2;
}
```

**OB-07: MenstrualCycle constructor injection change**

MenstrualCycle currently has constructor `(IVariableStore, IEngineVariables, IRandomProvider, ILogger<MenstrualCycle>)`. After extraction, add `ICommonFunctions common` parameter and store as `private readonly ICommonFunctions _common`. Injection registration in the composition root must also be verified (CommonFunctions is likely already registered since it is used elsewhere).

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#7 description says "HasPenis && MaleVirginity > 0" but ERB spec C3 is `HAS_PENIS(ARG) && !TALENT:ARG:童貞` (TALENT:童貞=0 means doutei). The C# inline and AC#7 preserve a semantic inversion vs ERB. | AC Design Constraints C3, AC Definition Table AC#7 | No fix required for F830 (Risk row 4 defers the bug); document in AC#7 Details that "IsDoutei preserves C# inline semantics, not ERB TALENT:童貞=0 convention — bug fix is out of scope" |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Verify OB-06 trigger: grep Era.Core/ for `ResetUfufu\|CVARSET.*CFLAG\|BulkResetCharacterFlags` and confirm >= 2 matches (second bulk CFLAG reset call site exists) | | [ ] |
| 2 | 2 | Add `BulkResetCharacterFlags(this IVariableStore variables, int charCount)` static extension method to `Era.Core/Interfaces/IVariableStoreExtensions.cs` | | [ ] |
| 3 | 3, 17 | Write xUnit tests in Era.Core.Tests: `BulkResetCharacterFlags_ZeroesCflag0to316_ForAllCharacters` (charCount=2, assert all CFLAG 0-316 zeroed) and `BulkResetCharacterFlags_ZeroCharCount_NoOp` (charCount=0, no calls to SetCFlag) | | [ ] |
| 4 | 4, 5 | In `Era.Core/State/SleepDepth.cs`: replace private `ResetUfufu()` body with a single-line delegation to `_variables.BulkResetCharacterFlags(_engine.GetCharaNum())` and remove the private method declaration | | [ ] |
| 5 | 8 | Verify OB-07 trigger: grep Era.Core/ for `MaleVirginity.*>.*0\|IsDoutei` and confirm >= 2 matches (second IS_DOUTEI call site exists) | | [ ] |
| 6 | 6 | Add `bool IsDoutei(int maleVirginity, int gender);` declaration to `Era.Core/Interfaces/ICommonFunctions.cs`, implement in `Era.Core/Common/CommonFunctions.cs` with body `return maleVirginity > 0 && gender >= 2;`, and update all ~15 sealed ICommonFunctions test stubs in Era.Core.Tests to implement IsDoutei (return default `false`) per C8 | | [ ] |
| 7 | 7 | Write xUnit test `IsDoutei_PreservesCSharpInlineSemantics` in Era.Core.Tests covering: male-virgin (true), female (false), non-virgin (false) cases | | [ ] |
| 8 | 9, 10 | In `Era.Core/State/MenstrualCycle.cs`: add `ICommonFunctions _common` field + constructor injection, replace inline `bool isDoutei = maleVirginity > 0 && gender >= 2` with `_common.IsDoutei(maleVirginity, gender)` | | [ ] |
| 9 | 11, 12, 16 | Run `dotnet test Era.Core.Tests --filter "FullyQualifiedName~EvictCharacters_BulkReset_ZeroesCflag0to316" --blame-hang-timeout 10s`, `dotnet test Era.Core.Tests --filter "FullyQualifiedName~MenstrualCycle" --blame-hang-timeout 10s`, and `dotnet build Era.Core/ --no-restore`; all must pass | | [ ] |
| 10 | 13 | Bump `<Version>` in `Era.Core/Era.Core.csproj` (e.g., 1.0.0 → 1.1.0) after extractions complete | | [ ] |
| 11 | 14, 15 | Create feature-840.md [DRAFT] for IS_DOUTEI semantic inversion investigation (ERB TALENT:童貞=0 vs C# maleVirginity > 0) and register in index-features.md | | [ ] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-830.md Tasks#1; grep Era.Core/ for ResetUfufu\|CVARSET.*CFLAG\|BulkResetCharacterFlags | Trigger confirmed (grep output logged) OR feature halted if trigger not met |
| 2 | implementer | sonnet | feature-830.md Tasks#2; IVariableStoreExtensions.cs pattern from Technical Design | BulkResetCharacterFlags extension method added to IVariableStoreExtensions.cs |
| 3 | implementer | sonnet | feature-830.md Tasks#3; IVariableStoreExtensions.cs (written in Phase 2) | xUnit test BulkResetCharacterFlags_ZeroesCflag0to316_ForAllCharacters in Era.Core.Tests |
| 4 | implementer | sonnet | feature-830.md Tasks#4; Era.Core/State/SleepDepth.cs lines 270-272, 340-351 | ResetUfufu removed, HandleWaking delegates to BulkResetCharacterFlags |
| 5 | implementer | sonnet | feature-830.md Tasks#5; grep Era.Core/ for MaleVirginity.*>.*0\|IsDoutei | Trigger confirmed (grep output logged) OR OB-07 skipped if trigger not met |
| 6 | implementer | sonnet | feature-830.md Tasks#6; ICommonFunctions.cs; CommonFunctions.cs; Technical Design interfaces; C8 sealed stubs list | IsDoutei declared on ICommonFunctions, implemented in CommonFunctions, ~15 sealed test stubs updated |
| 7 | implementer | sonnet | feature-830.md Tasks#7; CommonFunctions.cs (written in Phase 6) | xUnit test IsDoutei_PreservesCSharpInlineSemantics in Era.Core.Tests |
| 8 | implementer | sonnet | feature-830.md Tasks#8; Era.Core/State/MenstrualCycle.cs; ICommonFunctions.cs | MenstrualCycle updated with _common injection and IsDoutei delegation |
| 9 | implementer | sonnet | feature-830.md Tasks#9; Era.Core.Tests; Era.Core/ | SleepDepth regression PASS, MenstrualCycle regression PASS, dotnet build PASS logged in Execution Log |
| 10 | implementer | sonnet | feature-830.md Tasks#10; Era.Core/Era.Core.csproj | <Version> bumped; NuGet package version updated |
| 11 | implementer | sonnet | feature-830.md Tasks#11; Mandatory Handoffs row for IS_DOUTEI | feature-840.md [DRAFT] created + index-features.md row added |

### Pre-conditions

- Both OB-06 and OB-07 trigger conditions are confirmed unmet at feature creation (Feasibility Assessment).
- **OB-06 gate**: Task#1 MUST confirm >= 2 grep hits for `ResetUfufu|CVARSET.*CFLAG|BulkResetCharacterFlags` before Tasks#2-4 execute. If trigger not met, Tasks#2-4 are skipped; feature cannot reach [DONE] without at least one triggered extraction.
- **OB-07 gate**: Task#5 MUST confirm >= 2 grep hits for `MaleVirginity.*>.*0|IsDoutei` before Tasks#6-8 execute. If trigger not met, Tasks#6-8 are skipped.
- Either extraction may proceed independently; both gates failing = feature stays at current status (re-run when trigger fires).
- All changes target `C:\Era\core` (core repo), NOT devkit.
- Verify composition root registers `CommonFunctions` before Task#8 (ICommonFunctions must already be resolvable).

### Execution Order

```
Task#1 (OB-06 trigger check)
  → PASS: Task#2 → Task#3 → Task#4
  → FAIL: Skip Tasks#2-4

Task#5 (OB-07 trigger check)
  → PASS: Task#6 → Task#7 → Task#8
  → FAIL: Skip Tasks#6-8

Task#9 (regression + build) — run after all triggered extraction tasks complete
Task#10 (version bump) — run last, after Task#9 PASS
Task#11 (IS_DOUTEI handoff) — run unconditionally (not trigger-gated)
```

### Build Verification Steps

After Task#4 (OB-06 path):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build Era.Core/ --no-restore'
```

After Task#8 (OB-07 path):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build Era.Core/ --no-restore'
```

Final regression test (Task#9):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~EvictCharacters_BulkReset_ZeroesCflag0to316" --blame-hang-timeout 10s --results-directory /mnt/c/Era/devkit/_out/test-results'
```

### Success Criteria

- OB-06 triggered: AC#1, AC#2, AC#3, AC#4, AC#5 all pass
- OB-07 triggered: AC#6, AC#7, AC#8, AC#9, AC#10 all pass
- Neither extraction triggered: No extraction code changes; Task#11 (F840 handoff) still executes unconditionally (AC#14, AC#15 must pass); feature stays at current status for extraction tasks (re-run when trigger fires)
- OB-06 triggered (additional): AC#11, AC#12, AC#13, AC#17 must also pass
- OB-07 triggered (additional): AC#12, AC#13, AC#16 must also pass
- Unconditional: AC#14, AC#15 must pass regardless of trigger state

### Error Handling

| Error | Action |
|-------|--------|
| Trigger check finds 0 or 1 hits | STOP — do not proceed with extraction tasks; log result; skip triggered group |
| Build fails after extraction | STOP → report to user; do NOT continue to next task |
| Regression test fails (AC#11) | STOP → report to user; extraction may have broken SleepDepth behavior |
| Composition root missing ICommonFunctions registration | STOP → report to user; Task#8 cannot proceed without DI registration |
| MenstrualCycle constructor injection breaks existing tests | STOP → report to user |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| IS_DOUTEI semantic inversion vs ERB TALENT:童貞=0 convention (Risk row 4, 1/3 explorers flagged; C# inline uses `maleVirginity > 0` but ERB `TALENT:童貞=0` means doutei) | Out of scope for extraction-only F830; extraction preserves C# inline semantics per Key Decision row 5 | New Feature | F840 | Task#11 | [ ] | |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

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
| 2026-03-06T01:10:00Z | START | orchestrator | Phase 1 Initialize | F830 [REVIEWED]->[WIP] |
<!-- run-phase-1-completed -->
| 2026-03-06T02:30:00Z | CANCELLED | orchestrator | Both triggers unmet (OB-06: Phase 30, OB-07: no 2nd call site). Obligations handed off to phase-28-34-integration.md Phase 30 section. F840 creation deferred to Phase 30 planning. | F830 [WIP]->[CANCELLED] |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: AC#1+AC#8+Task#1+Task#5 | Circular trigger detection — grep patterns used extracted method names that don't exist pre-extraction; changed to baseline patterns (ResetUfufu/CVARSET/MaleVirginity)
- [fix] Phase2-Review iter1: AC#7+Task#7 | AC#7 falsely claimed ERB spec compliance; changed to 'preserves C# inline semantics'
- [fix] Phase2-Review iter1: AC#13 | Version bump AC only checked tag existence, not increment; changed to not_matches against baseline 1.0.0
- [fix] Phase2-Trivial iter1: Improvement Log section | Missing template-required section added
- [fix] Phase2-Review iter2: Mandatory Handoffs + Task#11 + AC#14 | IS_DOUTEI semantic inversion bug tracked via Mandatory Handoff to F840 (Track What You Skip)
- [fix] Phase3-Maintainability iter3: AC#14+Task#11+Mandatory Handoff | F834 already occupied by F832's handoff; changed destination to F840; AC#14 matcher strengthened to IS_DOUTEI-specific content
- [fix] Phase2-Review iter4: C3 AC Implication | 'Boolean logic must match ERB spec exactly' contradicted AC#7/Key Decision row 5; revised to 'preserve C# inline semantics'
- [fix] Phase2-Review iter4: Goal Coverage Item 6 | Fabricated goal item removed; AC#14 noted as deriving from Track What You Skip principle
- [fix] Phase3-Maintainability iter5: AC#14+Task#11+Mandatory Handoff | ID collision fix (F834→F835→F836 all taken)
- [fix] Phase2-Review iter6: AC#14+Task#11+Mandatory Handoff | F840 allocated (F834-F837 all occupied by sibling feature handoffs); definitive scan confirmed F840 available
- [fix] Phase2-Review iter7: Dependencies+Related Features | Synced F831 [DRAFT]→[WIP], F833 [DRAFT]→[PROPOSED] via feature-status.py
- [fix] Phase4-ACValidation iter8: AC#12 | Matcher changed from matches/Build succeeded to succeeds/- (build type convention)
- [fix] Phase4-ACValidation iter8: AC#14 | Type changed from code to file (PM document, not source code)
- [fix] Phase2-Review iter9: AC#15 added | DRAFT Creation Checklist requires both file existence AND index registration; AC#14 only verified file content
- [info] Phase1-DriftChecked: F831 (Related)
- [fix] Phase1-DriftSync iter10: Related Features table | F831 [WIP]→[DONE], F833 [PROPOSED]→[WIP] synced
- [fix] Phase3-Maintainability iter11: IsDoutei doc comment | 'Mirrors COMMON.ERB' claim removed; changed to 'Preserves C# inline semantics' with WARNING about ERB discrepancy
- [fix] Phase3-Maintainability iter11: BulkResetCharacterFlags body | Added TODO comment about extracting magic number 316 to named constant during implementation
- [fix] Phase7-FinalRefCheck iter11: Links section | Added F840 link (Mandatory Handoff destination referenced throughout spec but missing from Links)
- [fix] Phase3-Maintainability iter1: BulkResetCharacterFlags code snippet | Replaced TODO+magic number 316 with named constant `CvarsetResetCount = 317` (Zero Debt Upfront)
- [fix] Phase2-Review iter2: Feasibility Assessment verdict | Updated 'remains [DRAFT] by design' to 'FEASIBLE (trigger-gated)' — feature is [PROPOSED] per /fc completion
- [fix] Phase3-Maintainability iter3: IsDoutei interface declaration | Changed DIM (default body with NotImplementedException) to abstract member — Fail Fast at compile time
- [fix] Phase3-Maintainability iter4: AC#14+AC#15+Task#11+Mandatory Handoff+Links | F838→F840 ID collision fix (F838 now occupied by Test Infrastructure Fixes)
- [fix] Phase2-Review iter5: Pre-conditions+Success Criteria+Error Handling | Replaced 'feature remains/stays [DRAFT]' with correct lifecycle language (feature doesn't revert to [DRAFT] after /fc)
- [fix] Phase3-Maintainability iter6: AC#16+Task#9+Goal Coverage+Success Criteria | Added MenstrualCycle regression AC (constructor injection risk); included AC#14-16 in Success Criteria
- [fix] Phase2-Review iter7: Success Criteria | Clarified 'Neither triggered' case to acknowledge unconditional Task#11 execution (F840 handoff)
- [fix] Phase2-Review iter8: Technical Design Approach | Updated stale '13 ACs' to '16 ACs' with AC#14-16 group descriptions
- [fix] Phase1-RefCheck iter1: C3 Details | Path ERA/COMMON.ERB corrected to ERB/COMMON.ERB
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#17 row
- [fix] Phase2-Review iter1: AC#8 pattern | Removed post-extraction 'IsDoutei' from trigger grep (self-fulfilling pattern; AC Details already specified pre-extraction pattern only)
- [fix] Phase2-Uncertain iter1: Success Criteria | Split blanket 'Either triggered' into per-trigger AC sets (OB-06: AC#11,12,13,17; OB-07: AC#12,13,16; Unconditional: AC#14,15)
- [fix] Phase3-Maintainability iter2: All F839 refs | F839→F840 ID collision fix (F839 now occupied by Enable EnforceCodeStyleInBuild)
- [fix] Phase3-Maintainability iter9: Technical Constraints + AC Design Constraints | Added C8 documenting ICommonFunctions ~15 sealed test stubs impact (investigated via grep)
- [fix] Phase2-Review iter10: Task#6+Implementation Contract Phase 6+C8 Details | Expanded Task#6 to include sealed stub updates; fixed C8 Details to reference Task#6 (not Task#8)
- [fix] Phase4-ACValidation iter10: AC#2 Expected | Removed redundant Expected (pattern already in Method)
- [fix] Phase4-ACValidation iter10: AC#17+Task#3 | Added BulkReset boundary test (charCount=0) for engine-type negative coverage

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 830 (2026-03-06)
- [applied] C37: Mandatory Handoff Destination ID collision check → `.claude/agents/quality-fixer.md`
- [applied] Step 8e: ERB/C# Semantic Consistency Check → `.claude/agents/tech-designer.md`
- [revised] Self-fulfilling grep pattern detection (AC#8のみ該当、AC#1は正しく設計済み) → `.claude/agents/ac-validator.md`
- [revised] id-collision + semantic-consistency分類カテゴリ追加 (cross-reference-staleは既存stale-referenceでカバー済みのため削除) → `src/tools/python/imp-analyzer.py`
- [rejected] predecessor_context をSTRUCTURAL reviewに拡張 — STRUCTURAL reviewはformat/template準拠のみでpredecessor context不要。最適化はSEMANTIC/Phase 3/Phase 7に既に実装済み

---

<!-- fc-phase-6-completed -->
## Links
- [Related: F824](feature-824.md) - Sleep & Menstrual (original source)
- [Related: F826](feature-826.md) - Post-Phase Review (OB-06/OB-07 origin)
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (routing origin)
- [Related: F827](feature-827.md) - Phase 23 Planning
- [Related: F831](feature-831.md) - Roslynator Investigation (sibling)
- [Related: F833](feature-833.md) - IEngineVariables Indexed Stubs (sibling)
- [Related: F782](feature-782.md) - IVariableStoreExtensions pattern origin
- [Related: F840](feature-840.md) - IS_DOUTEI Semantic Inversion Investigation (Mandatory Handoff destination)
