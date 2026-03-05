# Feature 823: Room & Stain System Migration

## Status: [DONE]
<!-- fl-reviewed: 2026-03-04T23:55:56Z -->

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

## Background

### Philosophy (Mid-term Vision)

Phase 22 State Systems is the SSOT for runtime state management migration. Each ERB subsystem (room smell, stain, clothing, weather, pregnancy, sleep) becomes a dedicated C# service with typed interfaces, replacing raw FLAG/CFLAG/STAIN array manipulation with domain-specific APIs. F823 covers the room smell and stain runtime behavior domain.

### Problem (Current Issue)

ROOM_SMELL.ERB (1140 lines) and STAIN.ERB (300 lines) implement runtime stain mutation and room odor state tracking using packed integer encoding and raw variable array manipulation. The DRAFT Goal incorrectly references "implementing IStainLoader," but IStainLoader already exists at `Era.Core\Data\IStainLoader.cs:7-13` as a YAML configuration loader (loading StainConfig metadata: index, name, description). No C# service exists for the actual runtime behavior: stain cross-contamination (STAIN_MOVE_BY_PENIS), semen application (STAIN_ADD_SEMEN_*), room smell creation/decay (ROOM_SMELL_DAY/MORNING), or smell query functions (ROOM_SMELL_BIT/VALUE/WHOSE/KINDS/CHARA_MOST). Additionally, ROOM_SMELL uses a decimal packed integer encoding (`WHOSE * 10000000 + VALUE * 100000 + BIT` at ROOM_SMELL.ERB:941) that requires a typed value object. The DRAFT also incorrectly claims "no external NTR CALL is required," but SexHara.ERB and NTR_MASTER_SEX.ERB directly CALL ROOM_SMELL functions (ROOM_SMELL_NTR_PET/ORAL/SEX and ROOM_SMELL_CHARA_SUB).

### Goal (What to Achieve)

Migrate ROOM_SMELL.ERB and STAIN.ERB to C# by creating IRoomSmellService (room smell state machine: creation, decay, ventilation, query) and IStainService (runtime stain mutation: cross-contamination, semen application, duration tracking). Implement SmellData value type for packed integer encoding with encode/decode roundtrip safety. Achieve zero-debt implementation with unit tests verifying behavioral spec derived from ERB source analysis. DA/TA variable access for ROOM_SMELL_WHOSE_SAMEN is explicitly deferred unless a minimal accessor is added to IVariableStore during this feature.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does the DRAFT Goal reference IStainLoader? | Because the DRAFT was auto-generated from ERB file names without checking existing Era.Core interfaces | `feature-823.md:31` |
| 2 | Why does IStainLoader not fit? | Because IStainLoader is a YAML configuration loader returning StainConfig (index/name/description), not a runtime behavior service | `Era.Core\Data\IStainLoader.cs:7-13`, `Era.Core\Data\Models\StainConfig.cs:4-9` |
| 3 | Why are runtime services missing? | Because no C# equivalent exists for STAIN.ERB's stain mutation functions or ROOM_SMELL.ERB's smell state machine -- only the data-loading layer was built | `Era.Core\Data\YamlStainLoader.cs:8-39` (config only) |
| 4 | Why is the packed integer encoding a problem? | Because ROOM_SMELL stores smell data as `WHOSE * 10000000 + VALUE * 100000 + BIT` in FLAG/CFLAG arrays, requiring domain modeling as a value type for type safety | `ROOM_SMELL.ERB:781-794`, `ROOM_SMELL.ERB:941` |
| 5 | Why (Root)? | Because the ERB runtime behavior layer (stain mutation + smell state) was never migrated to C# services -- only the static configuration layer (StainConfig/StainIndex) exists, leaving a gap between data types and runtime operations | `IVariableStore.cs:93-96` (GetStain/SetStain exist but no service orchestrates them) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | DRAFT Goal says "implementing IStainLoader" | IStainLoader already exists for YAML config; the real gap is runtime service interfaces (IRoomSmellService, IStainService) |
| Where | Feature-823.md Goal section | Era.Core architecture: data layer complete (StainConfig, StainIndex, GetStain/SetStain) but service/orchestration layer absent |
| Fix | Rename IStainLoader reference | Create IRoomSmellService and IStainService as runtime behavior services, with SmellData value type for packed integer encoding |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Predecessor: Phase 22 Planning |
| F803 | [DONE] | Pattern source: CounterSourceHandler migration, CrossContaminateStain pattern |
| F811 | [DONE] | Pattern source: ShootingSystem ejaculation processing |
| F819 | [PROPOSED] | Sibling: Clothing System (no cross-calls) |
| F821 | [PROPOSED] | Sibling: Weather System (no cross-calls) |
| F822 | [DRAFT] | Sibling: Pregnancy System (no cross-calls) |
| F824 | [PROPOSED] | Sibling: Sleep & Menstrual (no cross-calls) |
| F825 | [DRAFT] | Successor: Relationships & DI Integration (depends on F823) |
| F826 | [DRAFT] | Successor: Post-Phase Review (depends on F823) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Era.Core stain infrastructure | FEASIBLE | StainIndex, GetStain/SetStain, StainConfig all exist |
| Era.Core common functions | FEASIBLE | HasPenis/HasVagina (CommonFunctions), IsBathroom (LocationSystem), GetSelectCom (IEngineVariables) all available |
| Packed integer encoding | FEASIBLE | 3 decoders are simple arithmetic; SmellData value type straightforward |
| ROOM_SMELL_DAY decomposition | FEASIBLE | 612-line function can be split into sub-methods by action type (bath, smell creation, command dispatch, transfer) |
| DA/TA variable access for WHOSE_SAMEN | NEEDS_REVISION | I3DArrayVariables has GetTa but no DA access; ROOM_SMELL_WHOSE_SAMEN (line 1108-1140) requires decision: add accessor or defer |
| External ERB caller compatibility | FEASIBLE | Public API must be exposed for SexHara.ERB, NTR_MASTER_SEX.ERB, COMF405.ERB, INFO.ERB; ERB functions remain until callers are migrated |
| Goal interface naming | NEEDS_REVISION | IStainLoader name is taken; Goal must reference IRoomSmellService and IStainService |
| Self-containment within Phase 22 | FEASIBLE | No cross-calls to sibling ERB files (F819-F824); fully independent |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core interfaces | HIGH | Two new service interfaces (IRoomSmellService, IStainService) plus SmellData value type added to core |
| DI composition root | MEDIUM | ServiceCollectionExtensions.cs must register new services |
| External ERB callers | MEDIUM | SexHara.ERB, NTR_MASTER_SEX.ERB, COMF405.ERB, INFO.ERB call ROOM_SMELL functions; must remain callable via bridge |
| ComableChecker | LOW | ComableChecker.Range4x.cs already reads FlagRoomSmellInit=500; must remain compatible |
| Existing stain infrastructure | LOW | StainIndex, IStainLoader, YamlStainLoader unchanged; new IStainService is additive only |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IStainLoader name already taken by YAML config loader | `Era.Core\Data\IStainLoader.cs:7-13` | New runtime service must use IStainService, not IStainLoader |
| Packed integer encoding must be preserved or bridged | `ROOM_SMELL.ERB:781-794` | External ERB callers (COMF405, INFO.ERB) still use packed ints via FLAG/CFLAG; C# SmellData must support encode/decode |
| FLAG index ranges 500-549 (primary), 550-599 (secondary), 600+ (visitor) | `DIM.ERH:487-489` | C# must map to identical FLAG indices via IVariableStore |
| CFLAG indices 800-806+ for per-character smell | `DIM.ERH:491-498` | C# must map to identical CFLAG indices |
| DA variable has no C# interface | `I3DArrayVariables.cs` -- no GetDa/SetDa | ROOM_SMELL_WHOSE_SAMEN (line 1108-1140) cannot be migrated without adding DA access or deferring |
| STAIN variables are bitfields with SETBIT/GETBIT | `DIM.ERH:304-310` | C# stain operations must use bitwise OR/AND matching ERB semantics |
| Value clamped 0-99, kind bits clamped 0-65535 | `ROOM_SMELL.ERB:957-968` | SmellData must enforce same bounds |
| Room index capped at 30 | `ROOM_SMELL.ERB:1015-1016` | Room > 30 results in no-op |
| COMF405 calls ROOM_SMELL_* as FUNCTION/FUNCTIONS (RETURNF) | `COMF405.ERB:9-86` | Must expose query functions as public methods |
| NTR_MASTER_SEX calls CHARA_SUB directly (5 call sites) | `NTR_MASTER_SEX.ERB:608,682,711,740,1275` | AddCharacterSmell must be public API |
| TreatWarningsAsErrors=true | `Directory.Build.props` (F708) | All new code must compile with zero warnings |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| DA/TA variable interface gap blocks WHOSE_SAMEN migration | HIGH | MEDIUM | Explicitly defer WHOSE_SAMEN to follow-up feature, or add minimal read-only DA accessor in this feature |
| Packed integer encoding errors in C# translation | MEDIUM | HIGH | Comprehensive unit tests with extracted ERB baseline values; SmellData value type with encode/decode roundtrip verification |
| ROOM_SMELL_DAY 612-line mega-function creates test coverage gaps | MEDIUM | MEDIUM | Decompose into sub-methods per interaction type (bath, smell creation, command dispatch, transfer); test each independently |
| External ERB callers (COMF405, SexHara, NTR files) depend on ROOM_SMELL functions | LOW | HIGH | Functions must remain callable from ERB via bridge; do not remove ERB functions until all callers are migrated |
| Goal mismatch leads to incorrect AC generation if uncorrected | HIGH | HIGH | Goal corrected in this synthesis to reference IRoomSmellService/IStainService |
| Interface naming confusion between IStainLoader (config) and IStainService (runtime) | MEDIUM | MEDIUM | Clear naming convention and XML doc comments distinguishing config loading from runtime mutation |
| Stain duration timer (120 ticks) edge cases | LOW | MEDIUM | Equivalence tests with duration boundary values (0, 119, 120, 121) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ROOM_SMELL.ERB line count | `wc -l` | 1140 | Full file including comments |
| STAIN.ERB line count | `wc -l` | ~300 | Full file including comments |
| ROOM_SMELL function count | ERB @-function headers | ~25 | Includes DAY, MORNING, BIT, VALUE, WHOSE, KINDS, CHARA_MOST, CHARA_SUB, NTR_*, ROOM_SUB, etc. |
| STAIN function count | ERB @-function headers | ~7 | MOVE_BY_PENIS, ADD_SEMEN_HAND/B/Sumata/M/V/A |
| External callers of ROOM_SMELL | grep CALL ROOM_SMELL | 6+ files | COMF405, INFO, SexHara, NTR_MASTER_SEX, etc. |
| External callers of STAIN | grep CALL STAIN | 10+ files | EJACULATION, BEFORETRAIN, NTR_SEX, NTR_ACTION, etc. |

**Baseline File**: `_out/tmp/baseline-823.txt` (generated during /run Phase 1)

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | IStainLoader already exists and must not be duplicated or reused for runtime behavior | `Era.Core\Data\IStainLoader.cs:7-13` | ACs must target IRoomSmellService and IStainService, never IStainLoader |
| C2 | SmellData packed integer encoding roundtrip must be verified | `ROOM_SMELL.ERB:781-794, :941` | ACs must verify encode(decode(x)) == x for SmellData (WHOSE*10M + VALUE*100K + BIT) |
| C3 | Value strength clamped to [0, 99], kind bits clamped to [0, 65535] | `ROOM_SMELL.ERB:957-968` | ACs must include boundary value tests for SmellData |
| C4 | External ERB callers must remain functional | `COMF405.ERB:9-86`, `SexHara.ERB:129-502`, `NTR_MASTER_SEX.ERB:608-1275` | NTR_PET/ORAL/SEX bridge compatibility is ensured by the original ERB files remaining unmodified (this feature does not delete or alter them); no separate AC or Task verification needed (see Key Decisions) |
| C5 | STAIN_MOVE_BY_PENIS has visitor-specific and character-specific branching | `STAIN.ERB:11-69` | ACs must cover visitor/character/client branching separately |
| C6 | Visitor stain uses FLAG storage, not STAIN array | `STAIN.ERB:14-18`, `DIM.ERH:489` | ACs must verify separate visitor storage path |
| C7 | DA variable gap for WHOSE_SAMEN | `ROOM_SMELL.ERB:1108-1140`, `I3DArrayVariables.cs` | AC for WHOSE_SAMEN must be conditioned on DA interface availability or explicitly deferred |
| C8 | STAIN_ADD_SEMEN_* sets stain duration timer to 120 | `STAIN.ERB:122,148,243,298` | ACs must verify duration timer value |
| C9 | Bath location resets character smell | `ROOM_SMELL.ERB:132-141` | ACs must verify BATHROOM zeroes smell |
| C10 | ROOM_SMELL_DAY duplicated code blocks require decomposition | `ROOM_SMELL.ERB:153-240` (~10 identical lubrication/stain check blocks) | ACs should verify decomposed sub-methods rather than monolithic function |
| C11 | Room index capped at 30 | `ROOM_SMELL.ERB:1015-1016` | ACs must verify room > 30 is no-op |
| C12 | CrossContaminateStain pattern already exists | `CounterSourceHandler.cs:1543-1548` | StainService should follow established pattern |
| C13 | ComableChecker reads FlagRoomSmellInit = 500 | `ComableChecker.Range4x.cs:17,197` | Must remain compatible with existing consumer |
| C14 | Behavioral spec baseline required from ERB source analysis | Feature Goal | Extract baseline behavioral rules from ERB source code for unit test assertions |

### Constraint Details

**C1: IStainLoader Naming Collision**
- **Source**: Investigation found IStainLoader.cs with `Result<StainConfig> Load(string path)` -- a YAML config loader
- **Verification**: `grep -r "IStainLoader" Era.Core/` confirms existing usage
- **AC Impact**: ac-designer must use IStainService for runtime stain mutation interface; IStainLoader is off-limits

**C2: SmellData Packed Integer Encoding**
- **Source**: ROOM_SMELL.ERB:781-794 defines three decoders; :941 defines encoder
- **Verification**: Extract sample FLAG values from headless game; verify encode/decode roundtrip
- **AC Impact**: SmellData must have Encode() and static Decode() methods; roundtrip must be verified with boundary values

**C3: SmellData Clamping**
- **Source**: ROOM_SMELL.ERB:957-964 clamps value to 0-99, kind to 0-65535
- **Verification**: ERB source shows explicit `MAX(0, MIN(99, value))` equivalent
- **AC Impact**: Constructor/factory must enforce bounds; boundary tests at 0, 99, 100, 65535, 65536

**C4: External ERB Caller Compatibility**
- **Source**: grep found 6+ ERB files calling ROOM_SMELL functions, 10+ calling STAIN functions
- **Verification**: grep for `CALL ROOM_SMELL` and `CALL STAIN` in game repo
- **AC Impact**: NTR_PET/ORAL/SEX bridge compatibility is ensured by the original ERB files remaining unmodified — this feature does not delete or modify them. Per Key Decisions, NTR_PET/ORAL/SEX are optional implementation-level delegation within RoomSmellService and require no AC or Task verification. Public API methods (IRoomSmellService, IStainService) must match ERB function signatures for the migrated callers only.

**C5: STAIN_MOVE_BY_PENIS Branching**
- **Source**: STAIN.ERB:11-69 uses SELECTCASE on SELECTCOM for 4 visitor/character/client paths
- **Verification**: Read STAIN.ERB:6-70 to confirm branch structure
- **AC Impact**: Separate test cases per branch (visitor, character, client, default)

**C6: Visitor Stain Storage**
- **Source**: STAIN.ERB:14-18 stores visitor stain in FLAG (not STAIN array)
- **Verification**: DIM.ERH:489 confirms FLAG:600 range for visitor
- **AC Impact**: StainService must detect visitor vs character and use correct storage path

**C7: DA Variable Gap**
- **Source**: I3DArrayVariables.cs has GetTa/SetTa but no GetDa/SetDa
- **Verification**: grep for GetDa in Era.Core Interfaces returns no matches
- **AC Impact**: WHOSE_SAMEN function must be explicitly deferred or a minimal DA accessor added; AC must state which path is taken

**C8: Stain Duration Timer**
- **Source**: STAIN.ERB:122,148,243,298 all set duration to 120
- **Verification**: grep for 120 in STAIN.ERB context lines
- **AC Impact**: Test that AddSemen* methods set duration timer to exactly 120

**C9: Bath Smell Reset**
- **Source**: ROOM_SMELL.ERB:132-141 checks IsBathroom and zeroes smell
- **Verification**: LocationSystem.cs:35-44 confirms IsBathroom exists
- **AC Impact**: Test that bath location zeroes all character smell values

**C10: ROOM_SMELL_DAY Decomposition**
- **Source**: ~10 identical lubrication/penis stain check blocks in lines 153-240
- **Verification**: Read ROOM_SMELL.ERB:153-240 to count duplicated blocks
- **AC Impact**: Decomposed helper method should be tested independently; monolithic test is insufficient

**C11: Room Index Cap**
- **Source**: ROOM_SMELL.ERB:1015-1016 checks room index against max (30)
- **Verification**: Read line 1015-1016 for boundary check
- **AC Impact**: Test room index 30 (last valid) and 31 (no-op)

**C12: CrossContaminateStain Pattern**
- **Source**: CounterSourceHandler.cs:1543-1548 already implements stain transfer
- **Verification**: Read the method to confirm pattern
- **AC Impact**: StainService should follow or delegate to existing pattern; do not duplicate logic

**C13: ComableChecker Compatibility**
- **Source**: ComableChecker.Range4x.cs:17,197 reads FlagRoomSmellInit = 500
- **Verification**: grep for FlagRoomSmellInit in Era.Core
- **AC Impact**: FLAG indices used by RoomSmellService must be compatible with existing ComableChecker constants

**C14: Behavioral Spec Baseline**
- **Source**: Feature Goal specifies "unit tests verifying behavioral spec derived from ERB source analysis"
- **Verification**: Extract behavioral rules from ERB source code (clamping, branching, timer values)
- **AC Impact**: ERB source analysis required before writing behavioral unit test assertions

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Successor | F825 | [DRAFT] | Relationships & DI Integration depends on F823 services |
| Successor | F826 | [DRAFT] | Post-Phase Review depends on F823 completion |
| Related | F803 | [DONE] | CounterSourceHandler migration pattern; CrossContaminateStain reference |
| Related | F811 | [DONE] | ShootingSystem ejaculation processing pattern |
| Related | F819 | [PROPOSED] | Clothing System (no cross-calls) |
| Related | F821 | [PROPOSED] | Weather System (no cross-calls) |
| Related | F822 | [DRAFT] | Pregnancy System (no cross-calls) |
| Related | F824 | [PROPOSED] | Sleep & Menstrual (no cross-calls) |

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
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Each ERB subsystem becomes a dedicated C# service with typed interfaces" | IRoomSmellService interface must exist with smell state machine methods | AC#1, AC#2, AC#21 |
| "Each ERB subsystem becomes a dedicated C# service with typed interfaces" | IStainService interface must exist with runtime stain mutation methods | AC#3, AC#4, AC#22, AC#23 |
| "replacing raw FLAG/CFLAG/STAIN array manipulation with domain-specific APIs" | SmellData value type must encapsulate packed integer encoding | AC#5, AC#6, AC#7 |
| "replacing raw FLAG/CFLAG/STAIN array manipulation with domain-specific APIs" | IStainService must replace raw STAIN bitfield operations with typed mutation methods; visitor stain uses FLAG not STAIN array | AC#3, AC#4, AC#9, AC#10, AC#16 |
| "smell state machine: creation, decay, ventilation, query" | ProcessDayPhase creates smell (creation, AC#26), ProcessMorningPhase decays/ventilates (AC#25), query functions decode SmellData | AC#17, AC#25, AC#26, AC#15 |
| "zero-debt implementation" | No TODO/FIXME/HACK markers in new code | AC#14 |
| "unit tests verifying behavioral spec derived from ERB source analysis" | Unit tests must pass verifying C# matches ERB behavioral spec | AC#15 |
| "DA/TA variable access for ROOM_SMELL_WHOSE_SAMEN is explicitly deferred" | WHOSE_SAMEN must NOT be implemented in this feature's service code | AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IRoomSmellService interface exists with smell state machine methods | file | Glob(src/Era.Core/State/IRoomSmellService.cs) | exists | `-` | [x] |
| 2 | IRoomSmellService declares smell query and mutation methods | code | Grep(src/Era.Core/State/IRoomSmellService.cs, pattern="ProcessDayPhase|ProcessMorningPhase|GetSmellBit|GetSmellValue|GetSmellWhose|GetSmellKinds|GetCharaMostSmell|AddCharacterSmell") | gte | 8 | [x] |
| 3 | IStainService interface exists with runtime stain mutation methods | file | Glob(src/Era.Core/State/IStainService.cs) | exists | `-` | [x] |
| 4 | IStainService declares stain transfer and semen application methods | code | Grep(src/Era.Core/State/IStainService.cs, pattern="MoveByPenis|AddSemenHand|AddSemenB|AddSemenSumata|AddSemenM|AddSemenV|AddSemenA") | gte | 7 | [x] |
| 5 | SmellData value type exists with Encode and Decode methods | code | Grep(src/Era.Core/State/SmellData.cs, pattern="Encode|Decode") | gte | 2 | [x] |
| 6 | SmellData roundtrip unit test exists (encode then decode preserves values) | code | Grep(src/Era.Core.Tests/State/SmellDataTests.cs, pattern="Encode.*Decode|Decode.*Encode|roundtrip|Roundtrip") | matches | `Encode.*Decode|Decode.*Encode|roundtrip|Roundtrip` | [x] |
| 7 | SmellData clamps value to [0,99] and kind bits to [0,65535] | test | dotnet test --filter "FullyQualifiedName~SmellData" | succeeds | pass | [x] |
| 8 | RoomSmellService bath location resets character smell to zero | test | dotnet test --filter "FullyQualifiedName~RoomSmell" | succeeds | pass | [x] |
| 9 | StainService MoveByPenis handles visitor, character, and client branches | test | dotnet test --filter "FullyQualifiedName~StainService" | succeeds | pass | [x] |
| 10 | StainService AddSemen methods set duration timer to 120 — verified by unit test | test | dotnet test --filter "FullyQualifiedName~StainService" | succeeds | pass | [x] |
| 11 | RoomSmellService room index > 30 is no-op (boundary test) — verified by unit test | test | dotnet test --filter "FullyQualifiedName~RoomSmell" | succeeds | pass | [x] |
| 12 | DI registration of IRoomSmellService and IStainService in ServiceCollectionExtensions | code | Grep(src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="IRoomSmellService|IStainService") | gte | 2 | [x] |
| 13 | WHOSE_SAMEN explicitly deferred (not implemented in this feature) | code | Grep(path="src/Era.Core/State/", pattern="WhoseSamen|WHOSE_SAMEN|GetWhoseSamen") | not_matches | `WhoseSamen|WHOSE_SAMEN|GetWhoseSamen` | [x] |
| 14 | No TODO/FIXME/HACK in new Room & Stain service files | code | Grep(path="src/Era.Core/State/", pattern="TODO|FIXME|HACK") | not_matches | `TODO|FIXME|HACK` | [x] |
| 15 | All Room & Stain unit tests pass | test | dotnet test --filter "FullyQualifiedName~RoomSmell\|FullyQualifiedName~StainService\|FullyQualifiedName~SmellData" | succeeds | pass | [x] |
| 16 | Visitor stain uses FLAG 600+ range storage (not STAIN array) | code | Grep(src/Era.Core/State/StainService.cs, pattern="600|FlagVisitor|VisitorStainFlag") | gte | 1 | [x] |
| 17 | ROOM_SMELL_DAY decomposed into named sub-methods (not monolithic) | code | Grep(src/Era.Core/State/RoomSmellService.cs, pattern="ProcessBathReset|ProcessSmellCreation|ProcessStainTransfer") | gte | 3 | [x] |
| 18 | SmellData file exists as value type | file | Glob(src/Era.Core/State/SmellData.cs) | exists | `-` | [x] |
| 19 | Build succeeds with zero warnings | build | dotnet build src/Era.Core/ --warnaserror | succeeds | pass | [x] |
| 20 | E2E DI resolution test includes RoomSmellService and StainService | code | Grep(src/Era.Core.Tests/E2E/, pattern="IRoomSmellService|IStainService") | gte | 2 | [x] |
| 21 | RoomSmellService constructor injects IVariableStore, ILocationService, ICommonFunctions | code | Grep(src/Era.Core/State/RoomSmellService.cs, pattern="IVariableStore|ILocationService|ICommonFunctions") | gte | 3 | [x] |
| 22 | StainService constructor injects IVariableStore, IEngineVariables, ICommonFunctions | code | Grep(src/Era.Core/State/StainService.cs, pattern="IVariableStore|IEngineVariables|ICommonFunctions") | gte | 3 | [x] |
| 23 | StainService MoveByPenis dispatch has visitor/character/client branches | code | Grep(src/Era.Core/State/StainService.cs, pattern="[Vv]isitor|[Cc]haracter|[Cc]lient") | gte | 3 | [x] |
| 24 | RoomSmellService references FLAG 500 base constant for ComableChecker compatibility | code | Grep(src/Era.Core/State/RoomSmellService.cs, pattern="500|FlagRoomSmellInit|RoomSmellFlagBase") | gte | 1 | [x] |
| 25 | RoomSmellService ProcessMorningPhase decay and ventilation behavior — verified by unit test | test | dotnet test --filter "FullyQualifiedName~RoomSmell" | succeeds | pass | [x] |
| 26 | RoomSmellService ProcessDayPhase smell creation behavior (stained character → room FLAG updated) — verified by unit test | test | dotnet test --filter "FullyQualifiedName~RoomSmell" | succeeds | pass | [x] |

### AC Details

**AC#2: IRoomSmellService declares smell query and mutation methods**
- **Test**: `Grep(src/Era.Core/State/IRoomSmellService.cs, pattern="ProcessDayPhase|ProcessMorningPhase|GetSmellBit|GetSmellValue|GetSmellWhose|GetSmellKinds|GetCharaMostSmell|AddCharacterSmell")`
- **Expected**: `>= 8`
- **Derivation**: 8 core public methods from ROOM_SMELL.ERB: ROOM_SMELL_DAY (ProcessDayPhase), ROOM_SMELL_MORNING (ProcessMorningPhase), ROOM_SMELL_BIT (GetSmellBit), ROOM_SMELL_VALUE (GetSmellValue), ROOM_SMELL_WHOSE (GetSmellWhose), ROOM_SMELL_KINDS (GetSmellKinds), ROOM_SMELL_CHARA_MOST (GetCharaMostSmell), ROOM_SMELL_CHARA_SUB (AddCharacterSmell). NTR_PET/ORAL/SEX are thin wrappers calling AddCharacterSmell.
- **Rationale**: Ensures 1:1 migration of ERB public API surface. Constraint C4 (external ERB callers) requires these methods be publicly accessible.

**AC#4: IStainService declares stain transfer and semen application methods**
- **Test**: `Grep(src/Era.Core/State/IStainService.cs, pattern="MoveByPenis|AddSemenHand|AddSemenB|AddSemenSumata|AddSemenM|AddSemenV|AddSemenA")`
- **Expected**: `>= 7`
- **Derivation**: 7 public functions from STAIN.ERB: STAIN_MOVE_BY_PENIS (MoveByPenis), STAIN_ADD_SEMEN_HAND (AddSemenHand), STAIN_ADD_SEMEN_B (AddSemenB), STAIN_ADD_SEMEN_Sumata (AddSemenSumata), STAIN_ADD_SEMEN_M (AddSemenM), STAIN_ADD_SEMEN_V (AddSemenV), STAIN_ADD_SEMEN_A (AddSemenA).
- **Rationale**: Ensures all STAIN.ERB public functions are migrated. Constraint C12 (CrossContaminateStain pattern) informs internal implementation, not public API count.

**AC#5: SmellData value type exists with Encode and Decode methods**
- **Test**: `Grep(src/Era.Core/State/SmellData.cs, pattern="Encode|Decode")`
- **Expected**: `>= 2`
- **Derivation**: SmellData requires exactly 1 Encode method and 1 static Decode method for packed integer conversion (WHOSE*10000000 + VALUE*100000 + BIT). Constraint C2 mandates roundtrip verification.
- **Rationale**: Packed integer encoding (C2) must have typed encode/decode to replace raw arithmetic in ERB.

**AC#12: DI registration of IRoomSmellService and IStainService**
- **Test**: `Grep(src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="IRoomSmellService|IStainService")`
- **Expected**: `>= 2`
- **Derivation**: 2 new service interfaces (IRoomSmellService, IStainService) each require 1 AddSingleton registration line. Constraint C1 confirms IStainLoader is separate and unchanged.
- **Rationale**: DI composition root must register both new services for E2E resolution (AC#20).

**AC#16: Visitor stain uses FLAG 600+ range storage**
- **Test**: `Grep(src/Era.Core/State/StainService.cs, pattern="600|FlagVisitor|VisitorStainFlag")`
- **Expected**: `>= 1`
- **Derivation**: Constraint C6 (STAIN.ERB:14-18, DIM.ERH:489) specifies visitor stain stored in FLAG at 600+ range. StainService.cs must reference this constant (e.g., `private const int VisitorStainFlagBase = 600;` or similar). Floor count 1 ensures at least one reference exists.
- **Rationale**: Verifies source code contains the FLAG 600+ constant for visitor storage path, complementing AC#9's behavioral test.

**AC#17: ROOM_SMELL_DAY decomposed into named sub-methods**
- **Test**: `Grep(src/Era.Core/State/RoomSmellService.cs, pattern="ProcessBathReset|ProcessSmellCreation|ProcessStainTransfer")`
- **Expected**: `>= 3`
- **Derivation**: ROOM_SMELL_DAY is 612 lines with ~10 duplicated lubrication/stain check blocks (Constraint C10). Decomposition requires exactly 3 named private helpers: (1) ProcessBathReset, (2) ProcessSmellCreation, (3) ProcessStainTransfer. Pattern matches specific method names rather than any private method.
- **Rationale**: Previous pattern (`private.*void|private.*int|...`) was too broad — matched any private method. Named pattern ensures actual ROOM_SMELL_DAY decomposition, not coincidental private method count.

**AC#20: E2E DI resolution test includes RoomSmellService and StainService**
- **Test**: `Grep(src/Era.Core.Tests/E2E/, pattern="IRoomSmellService|IStainService")`
- **Expected**: `>= 2`
- **Derivation**: 2 new service interfaces must each be verified in E2E DI resolution test (1 GetRequiredService call per interface).
- **Rationale**: F813 E2E pattern requires all new services to be resolvable from the DI container.

**AC#21: RoomSmellService constructor injection**
- **Test**: `Grep(src/Era.Core/State/RoomSmellService.cs, pattern="IVariableStore|ILocationService|ICommonFunctions")`
- **Expected**: `>= 3`
- **Derivation**: 3 constructor parameters from Technical Design: IVariableStore (FLAG/CFLAG/STAIN access), ILocationService (IsBathroom for bath reset), ICommonFunctions (HasPenis/HasVagina for smell type).
- **Rationale**: V2 checklist requires every constructor parameter to have an injection verification AC.

**AC#22: StainService constructor injection**
- **Test**: `Grep(src/Era.Core/State/StainService.cs, pattern="IVariableStore|IEngineVariables|ICommonFunctions")`
- **Expected**: `>= 3`
- **Derivation**: 3 constructor parameters from Technical Design: IVariableStore (STAIN/FLAG access), IEngineVariables (GetMaster/GetTarget), ICommonFunctions (HasPenis).
- **Rationale**: V2 checklist requires every constructor parameter to have an injection verification AC.

**AC#23: StainService MoveByPenis dispatch branches**
- **Test**: `Grep(src/Era.Core/State/StainService.cs, pattern="[Vv]isitor|[Cc]haracter|[Cc]lient")`
- **Expected**: `>= 3`
- **Derivation**: STAIN_MOVE_BY_PENIS has 4 SELECTCASE branches (visitor, character, client/villager, default). Pattern targets domain-specific keywords rather than generic `case/switch`. Floor count 3 requires at least visitor + character + client to appear in the code.
- **Rationale**: Previous pattern (`case|switch|visitor|...`) included generic C# keywords; tightened to domain-specific identifiers. Complements AC#9 behavioral verification.

**AC#24: RoomSmellService references FLAG 500 base constant**
- **Test**: `Grep(src/Era.Core/State/RoomSmellService.cs, pattern="500|FlagRoomSmellInit|RoomSmellFlagBase")`
- **Expected**: `>= 1`
- **Derivation**: Constraint C13 requires ComableChecker.Range4x.cs FlagRoomSmellInit=500 to remain compatible. RoomSmellService must reference the FLAG 500 base index (as a constant or literal) to ensure its implementation aligns with the pre-existing ComableChecker constant.
- **Rationale**: Targets RoomSmellService.cs directly (not Era.Core/ broadly) to verify the new implementation actually references the FLAG 500 base index, preventing false-positive pass from pre-existing ComableChecker constant.

**AC#25: RoomSmellService ProcessMorningPhase decay and ventilation behavior**
- **Test**: `dotnet test --filter "FullyQualifiedName~RoomSmell"`
- **Expected**: pass
- **Derivation**: Philosophy says IRoomSmellService is a "smell state machine: creation, decay, ventilation, query". ProcessMorningPhase implements decay (smell strength reduction) and ventilation (ventilation timestamp update). ERB source analysis (ROOM_SMELL.ERB:34-113) shows `ROOM_SMELL_MORNING` stores `TIME` into `FLAG:(部屋のにおい_初期FLAG_2)` = `FLAG:550` (the secondary FLAG base, index 0) at line 40 (`FLAG:(LOCAL:1) = TIME`), and then calls `ROOM_SMELL_ROOM_MINUS` (via ROOM_SMELL_VENTILATION, lines 734-742) which subtracts the decrement amount (10) from each room's smell VALUE component stored in FLAG:500-549 (primary) and FLAG:550-599 (secondary). "Ventilation state" is NOT a separate boolean FLAG — it is the last-ventilation **timestamp** stored at `FLAG:550` (secondaryFlagBase index 0), readable as `IVariableStore.GetFlag(550)`. After `ProcessMorningPhase`, this timestamp equals the current TIME value. Unit tests must verify: (1) a non-zero smell VALUE component (extracted by SmellData.Decode) at a room's FLAG:500+N slot is reduced by the decrement amount after ProcessMorningPhase (decay), and (2) `IVariableStore.GetFlag(550)` equals the injected current TIME value after ProcessMorningPhase (ventilation timestamp updated).
- **Rationale**: Philosophy "decay, ventilation" requires concrete testable assertions referencing specific FLAG indices. ERB source (ROOM_SMELL.ERB:40 and ROOM_SMELL_VENTILATION at line 734) confirms: ventilation state = timestamp at FLAG:550, decay = VALUE subtraction via ROOM_SMELL_ROOM_MINUS. AC#2 only verifies method existence; this AC verifies the behavioral spec with concrete index references.
- **Disambiguation**: AC#8, AC#11, AC#25, AC#26 share the same test filter (`FullyQualifiedName~RoomSmell`) but target distinct test methods: AC#8 → `ProcessDayPhase_Bathroom*`, AC#11 → `*_RoomIndex*`, AC#25 → `ProcessMorningPhase_*`, AC#26 → `ProcessDayPhase_SmellCreation*`. Each behavior has its own `[Fact]` method; the shared filter runs all, but individual test failure isolates the specific AC.

**AC#26: RoomSmellService ProcessDayPhase smell creation behavior**
- **Test**: `dotnet test --filter "FullyQualifiedName~RoomSmell"`
- **Expected**: pass
- **Derivation**: Philosophy says IRoomSmellService handles "creation" as part of the smell state machine. ProcessDayPhase creates room smell when a stained character is present — updating room FLAG at the correct index. Task 7 explicitly includes this test case ("ProcessDayPhase with stained character → room smell FLAG updated").
- **Rationale**: AC#17 only verifies that ProcessSmellCreation method name exists (structural), but creation *behavior* (stained character → room FLAG updated) requires a behavioral test. This AC ensures the Philosophy "creation" claim is verified by actual test execution, not just code structure.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate ROOM_SMELL.ERB to C# by creating IRoomSmellService (smell state machine: creation, decay, ventilation, query) | AC#1, AC#2, AC#8, AC#11, AC#17, AC#21, AC#24, AC#25, AC#26 |
| 2 | Migrate STAIN.ERB to C# by creating IStainService (runtime stain mutation: cross-contamination, semen application, duration tracking) | AC#3, AC#4, AC#9, AC#10, AC#16, AC#22, AC#23 |
| 3 | Implement SmellData value type for packed integer encoding with encode/decode roundtrip safety | AC#5, AC#6, AC#7, AC#18 |
| 4 | Achieve zero-debt implementation | AC#14, AC#19 |
| 5 | Unit tests verifying behavioral spec derived from ERB source analysis | AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#15, AC#25, AC#26 |
| 6 | DA/TA variable access for ROOM_SMELL_WHOSE_SAMEN is explicitly deferred | AC#13 |
| 7 | DI registration and E2E integration | AC#12, AC#20 |

---

<!-- fc-phase-4-completed -->

## Technical Design

### Approach

Five new files in `Era.Core\State\` implement the runtime behavior layer that was never migrated:

1. **`SmellData.cs`** — readonly record struct wrapping the packed integer encoding `WHOSE * 10000000 + VALUE * 100000 + BIT`. Static `Decode(long raw)` factory method and `Encode()` instance method, with clamping enforced in the constructor (Value [0,99], Kind [0,65535]).

2. **`IRoomSmellService.cs`** — interface declaring 8 public methods matching the ERB public API surface: `ProcessDayPhase`, `ProcessMorningPhase`, `GetSmellBit`, `GetSmellValue`, `GetSmellWhose`, `GetSmellKinds`, `GetCharaMostSmell`, `AddCharacterSmell`.

3. **`RoomSmellService.cs`** — concrete implementation. Dependencies injected via constructor: `IVariableStore`, `ILocationService`, `ICommonFunctions`. State stored in `IVariableStore.GetFlag/SetFlag` at FLAG indices 500-599 (primary, secondary) and CFLAG indices 800-806+ via `IVariableStore.GetCharacterFlag/SetCharacterFlag`. ROOM_SMELL_DAY decomposed into private helpers: `ProcessBathReset`, `ProcessSmellCreation`, `ProcessStainTransfer`, and per-interaction-type helpers (at minimum 3 private sub-methods) to replace the ~10 duplicated lubrication/stain check blocks. Room index guard: `if (roomIndex > 30) return;` matching ERB:1015-1016. NTR wrapper methods (`AddNtrPetSmell`, `AddNtrOralSmell`, `AddNtrSexSmell`) delegate to `AddCharacterSmell` — they are internal pass-throughs, not separate interface methods. `WHOSE_SAMEN` function is NOT implemented (deferred per AC#13).

   **Ventilation definition (ERB source: ROOM_SMELL.ERB:34-42, 734-742)**: "Ventilation" in the smell state machine is NOT a separate enabled/disabled boolean flag. It is a **time-based decay process** controlled by the last-processed timestamp stored at `FLAG:550` (the secondary FLAG base index 0, named `部屋のにおい_初期FLAG_2` in ERB). `ROOM_SMELL_MORNING` sets `FLAG:(LOCAL:1) = TIME` (line 40) — i.e., `IVariableStore.SetFlag(550, currentTime)` — recording when the morning phase last ran. The `ROOM_SMELL_VENTILATION` function (lines 734-742) updates the same timestamp slot and then calls `ROOM_SMELL_ROOM_MINUS` to subtract the decrement amount (10) from each room's smell VALUE component. `ProcessMorningPhase` must: (a) set `IVariableStore.SetFlag(550, currentTime)` to update the ventilation timestamp, and (b) subtract the decay amount from the VALUE component of each room's packed smell data in FLAG:500-549 (primary) and FLAG:550-599 (secondary) via SmellData decode→adjust→encode→store. These are the two concrete testable assertions for AC#25.

4. **`IStainService.cs`** — interface declaring 7 public methods: `MoveByPenis`, `AddSemenHand`, `AddSemenB`, `AddSemenSumata`, `AddSemenM`, `AddSemenV`, `AddSemenA`.

5. **`StainService.cs`** — concrete implementation. Dependencies injected: `IVariableStore`, `IEngineVariables`, `ICommonFunctions`. `MoveByPenis` uses `IEngineVariables.GetSelectCom()` for SELECTCOM dispatch, branching on visitor/character/client paths (STAIN.ERB:11-69). **GetSelectCom coupling rationale**: ERB STAIN_MOVE_BY_PENIS uses `SELECTCOM` to determine the sexual act context (which party is involved); this is the only mechanism to distinguish visitor/character/client stain transfer paths — the coupling is an inherent ERB semantic preserved in the C# migration, not an architectural concern. Visitor path stores stain in `IVariableStore.GetFlag/SetFlag` at FLAG:600+ range (not STAIN array) per constraint C6. Character/client paths use `IVariableStore.GetStain/SetStain` with bitwise OR semantics matching CrossContaminateStain pattern from CounterSourceHandler. Duration timer set to 120 by `AddSemen*` methods via `IVariableStore.SetStain(character, durationIndex, 120)`.

**DI Registration**: Both services registered as `AddSingleton` in `ServiceCollectionExtensions.cs` under "State Systems (Phase 22) - Feature 823" comment block.

**Tests in `Era.Core.Tests\State\`**: `SmellDataTests.cs`, `RoomSmellServiceTests.cs`, `StainServiceTests.cs`. Each uses mock implementations of injected interfaces (matching the MockVariableStore pattern from BodySettingsTests). No headless game execution required for unit tests.

**E2E Test**: Two new `[Fact]` methods added to `DiResolutionTests.cs` — `Resolve_IRoomSmellService()` and `Resolve_IStainService()` — following the exact pattern of existing resolution tests.

This approach satisfies all ACs: interfaces and implementations are placed in the verified `Era.Core\State\` path (AC#1,3,18), method declarations match ERB function counts (AC#2,4,5), boundary clamping and roundtrip is unit-tested (AC#6,7), behaviour tests cover bath reset/room cap/stain branches/decay (AC#8,9,11,25), duration timer and visitor FLAG path are verified in tests (AC#10,16), ROOM_SMELL_DAY is decomposed (AC#17), DI registration enables E2E resolution (AC#12,20), WHOSE_SAMEN is absent (AC#13), no debt markers (AC#14), all tests pass (AC#15), build succeeds (AC#19), constructor injection is verified for both services (AC#21,22), MoveByPenis dispatch branch count is verified (AC#23), and ComableChecker FlagRoomSmellInit=500 compatibility is preserved (AC#24).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core\State\IRoomSmellService.cs` with file-scoped namespace `Era.Core.State` |
| 2 | Declare all 8 methods in `IRoomSmellService`: `ProcessDayPhase`, `ProcessMorningPhase`, `GetSmellBit`, `GetSmellValue`, `GetSmellWhose`, `GetSmellKinds`, `GetCharaMostSmell`, `AddCharacterSmell` |
| 3 | Create `Era.Core\State\IStainService.cs` with file-scoped namespace `Era.Core.State` |
| 4 | Declare all 7 methods in `IStainService`: `MoveByPenis`, `AddSemenHand`, `AddSemenB`, `AddSemenSumata`, `AddSemenM`, `AddSemenV`, `AddSemenA` |
| 5 | Create `Era.Core\State\SmellData.cs` as `readonly record struct` with `Encode()` instance method and `static Decode(long raw)` factory |
| 6 | Write roundtrip test in `SmellDataTests.cs` asserting `SmellData.Decode(original.Encode()) == original` for boundary values |
| 7 | Unit tests in `SmellDataTests.cs` verify: constructor clamps Whose to >= 0 (test -1 → 0), clamps Value to [0,99] (test 100 → 99, -1 → 0), clamps Kind to [0,65535] (test 65536 → 65535); all pass via `dotnet test --filter "FullyQualifiedName~SmellData"` |
| 8 | Unit test in `RoomSmellServiceTests.cs`: mock `ILocationService.IsBathroom` returns true → `ProcessDayPhase` zeroes all CFLAG 800-806 smell entries for the character |
| 9 | Unit tests in `StainServiceTests.cs` cover 4 `MoveByPenis` branches: visitor (GetSelectCom returns visitor value → FLAG storage), character (STAIN bitwise OR), client (STAIN bitwise OR), default no-op |
| 10 | Unit tests in `StainServiceTests.cs` assert `GetStain(character, durationIndex)` equals 120 after each `AddSemen*` call |
| 11 | Unit test in `RoomSmellServiceTests.cs`: call with `roomIndex = 31`, assert no FLAG mutation; call with `roomIndex = 30`, assert FLAG mutation occurs |
| 12 | Add two lines to `ServiceCollectionExtensions.cs`: `services.AddSingleton<IRoomSmellService, RoomSmellService>();` and `services.AddSingleton<IStainService, StainService>();` |
| 13 | `IRoomSmellService.cs` and `RoomSmellService.cs` contain no `WhoseSamen`, `WHOSE_SAMEN`, or `GetWhoseSamen` identifiers — verified by Grep not_matches |
| 14 | All 5 new `.cs` files in `Era.Core\State\` contain no `TODO`, `FIXME`, or `HACK` markers |
| 15 | All tests in `SmellDataTests.cs`, `RoomSmellServiceTests.cs`, `StainServiceTests.cs` pass via `dotnet test --filter "FullyQualifiedName~RoomSmell\|FullyQualifiedName~StainService\|FullyQualifiedName~SmellData"` |
| 16 | `StainService.cs` references FLAG 600+ range constant for visitor stain storage — verified by Grep for `600|FlagVisitor|VisitorStainFlag` in StainService.cs (gte 1). Distinct from AC#9 (behavioral test) — this verifies the constant exists in source code |
| 17 | `RoomSmellService.cs` contains 3+ private helper methods extracted from `ProcessDayPhase`: `ProcessBathReset`, `ProcessSmellCreation`, `ProcessStainTransfer` (minimum 3 — may add more per interaction type) |
| 18 | Create `Era.Core\State\SmellData.cs` — file existence verified by Glob |
| 19 | `dotnet build src/Era.Core/ --warnaserror` succeeds; `TreatWarningsAsErrors=true` is inherited from `Directory.Build.props`; all new code uses `var` only where type is apparent, no unnecessary qualifiers |
| 20 | Add `Resolve_IRoomSmellService()` and `Resolve_IStainService()` `[Fact]` methods to `DiResolutionTests.cs` using `_provider.GetRequiredService<IRoomSmellService/IStainService>()` pattern |
| 21 | `RoomSmellService` constructor parameters include `IVariableStore`, `ILocationService`, `ICommonFunctions` — verified by Grep for these interface types in `RoomSmellService.cs` |
| 22 | `StainService` constructor parameters include `IVariableStore`, `IEngineVariables`, `ICommonFunctions` — verified by Grep for these interface types in `StainService.cs` |
| 23 | `StainService.MoveByPenis` contains dispatch branches for visitor/character/client cases — verified by Grep for domain-specific identifiers `[Vv]isitor|[Cc]haracter|[Cc]lient` (gte 3) |
| 24 | Grep for `500|FlagRoomSmellInit|RoomSmellFlagBase` in `RoomSmellService.cs` confirms FLAG 500 base index is referenced (gte 1) — verifies C13 ComableChecker compatibility in the new implementation |
| 25 | Unit test in `RoomSmellServiceTests.cs`: (a) `IVariableStore.GetFlag(550)` equals injected currentTime after ProcessMorningPhase — verifies ventilation timestamp update at FLAG:550 (secondaryFlagBase, `部屋のにおい_初期FLAG_2`, ERB:40); (b) SmellData.Decode(FLAG:500+N).Value is reduced by decay decrement for a room with pre-set non-zero smell — verifies decay via ROOM_SMELL_ROOM_MINUS (ERB:745-778); both pass → ensures Philosophy "decay, ventilation" claim is AC-verified with concrete FLAG:550 index assertion |
| 26 | Unit test in `RoomSmellServiceTests.cs`: ProcessDayPhase with stained character → room smell FLAG updated at correct index (creation behavior); ensures Philosophy "creation" claim is AC-verified |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| SmellData value type vs raw int | Raw int arithmetic in service, typed value object, separate encoder class | `readonly record struct SmellData` | Constraint C2/C3 require encode/decode roundtrip verification; value type ensures bounds enforcement at construction boundary; matches era.Core pattern for StainIndex |
| WHOSE_SAMEN handling | Add minimal DA accessor to I3DArrayVariables, defer to follow-up feature | Defer (not implemented) | C7 constraint confirms no GetDa/SetDa in I3DArrayVariables; adding DA interface is a cross-cutting concern requiring its own feature; AC#13 explicitly verifies absence |
| NTR wrapper methods (NTR_PET/ORAL/SEX) | Expose as separate interface methods, internal delegates to AddCharacterSmell | Internal private delegates; not in IRoomSmellService interface | These 3 ERB functions are thin wrappers (ROOM_SMELL.ERB:~600-640); external ERB callers (SexHara.ERB, NTR_MASTER_SEX.ERB) use ERB bridge which calls AddCharacterSmell(public) directly with appropriate smellKind parameters — NTR wrappers are optional implementation-level delegation within RoomSmellService, not required by any AC; no AC or Task verification needed |
| Visitor stain storage | STAIN array with special index, FLAG array at 600+ range, separate flag store | FLAG array at 600+ range | Constraint C6: STAIN.ERB:14-18 and DIM.ERH:489 confirm FLAG:600 range; ComableChecker uses FlagRoomSmellInit=500 confirming FLAG index space; `IVariableStore.GetFlag/SetFlag` already available |
| Stain duration index | Raw integer constant in service, StainIndex typed constant, inline comment | Raw integer constant with XML doc comment (e.g., `private const int StainDurationIndex = 8;`) | Duration index is a raw ERB array index not covered by existing StainIndex constants; follows ShootingSystem pattern of raw integer constants with CSV-verified comments |
| ROOM_SMELL_DAY decomposition granularity | 1 large private method, 3 sub-methods by type, 10+ micro-methods per duplicated block | 3+ private sub-methods minimum (`ProcessBathReset`, `ProcessSmellCreation`, `ProcessStainTransfer`) | AC#17 requires >= 3 private methods; Constraint C10 identifies ~10 duplicated blocks that may be consolidated into typed helpers; 3 is the floor, implementer may add more |
| Service lifetime in DI | Transient, Scoped, Singleton | Singleton | Both services depend only on `IVariableStore` and `ILocationService` which are themselves Singleton; no per-request state; follows established pattern for all Phase 21/22 services in ServiceCollectionExtensions.cs |
| Test mock approach | NSubstitute, Moq, hand-rolled mocks | Hand-rolled mock inner classes | Existing tests (VisitorSettingsTests, BodySettingsTests) use hand-rolled mocks; no Moq/NSubstitute references found in Era.Core.Tests project |

### Interfaces / Data Structures

```csharp
// Era.Core/State/SmellData.cs
namespace Era.Core.State;

/// <summary>
/// Packed integer encoding for room smell data.
/// Encoding: WHOSE * 10_000_000 + VALUE * 100_000 + BIT
/// Source: ROOM_SMELL.ERB:781-794 (decode), :941 (encode)
/// Feature 823 - Room and Stain System Migration
/// </summary>
public readonly record struct SmellData
{
    /// <summary>Character who caused the smell (WHOSE component, >= 0)</summary>
    public int Whose { get; }

    /// <summary>Smell strength [0, 99] (VALUE component)</summary>
    public int Value { get; }

    /// <summary>Smell kind bitfield [0, 65535] (BIT component)</summary>
    public int Kind { get; }

    public SmellData(int whose, int value, int kind)
    {
        Whose = Math.Max(0, whose);
        Value = Math.Max(0, Math.Min(99, value));
        Kind = Math.Max(0, Math.Min(65535, kind));
    }

    /// <summary>Encode to packed integer (WHOSE*10M + VALUE*100K + BIT)</summary>
    public long Encode() => (long)Whose * 10_000_000L + (long)Value * 100_000L + Kind;

    /// <summary>Decode packed integer to SmellData</summary>
    public static SmellData Decode(long raw)
    {
        int whose = (int)(raw / 10_000_000L);
        int value = (int)((raw % 10_000_000L) / 100_000L);
        int kind = (int)(raw % 100_000L);
        return new SmellData(whose, value, kind);
    }
}

// Era.Core/State/IRoomSmellService.cs
namespace Era.Core.State;

/// <summary>
/// Runtime room smell state machine: creation, decay, ventilation, query.
/// Migrates ROOM_SMELL.ERB (~25 functions) to C#.
/// Distinct from IStainLoader (Era.Core.Data) which loads YAML config.
/// Feature 823 - Room and Stain System Migration
/// </summary>
public interface IRoomSmellService
{
    void ProcessDayPhase(int locationId);        // ROOM_SMELL_DAY
    void ProcessMorningPhase(int locationId);    // ROOM_SMELL_MORNING
    int GetSmellBit(int roomIndex);              // ROOM_SMELL_BIT
    int GetSmellValue(int roomIndex);            // ROOM_SMELL_VALUE
    int GetSmellWhose(int roomIndex);            // ROOM_SMELL_WHOSE
    int GetSmellKinds(int roomIndex);            // ROOM_SMELL_KINDS
    int GetCharaMostSmell(CharacterId charId);   // ROOM_SMELL_CHARA_MOST
    void AddCharacterSmell(CharacterId charId, int smellKind, int value); // ROOM_SMELL_CHARA_SUB
}

// Era.Core/State/IStainService.cs
namespace Era.Core.State;

/// <summary>
/// Runtime stain mutation: cross-contamination, semen application, duration tracking.
/// Migrates STAIN.ERB (7 public functions) to C#.
/// Distinct from IStainLoader (Era.Core.Data) which loads StainConfig YAML.
/// Feature 823 - Room and Stain System Migration
/// </summary>
public interface IStainService
{
    void MoveByPenis(CharacterId targetId);      // STAIN_MOVE_BY_PENIS
    void AddSemenHand(CharacterId targetId);     // STAIN_ADD_SEMEN_HAND
    void AddSemenB(CharacterId targetId);        // STAIN_ADD_SEMEN_B
    void AddSemenSumata(CharacterId targetId);   // STAIN_ADD_SEMEN_Sumata
    void AddSemenM(CharacterId targetId);        // STAIN_ADD_SEMEN_M
    void AddSemenV(CharacterId targetId);        // STAIN_ADD_SEMEN_V
    void AddSemenA(CharacterId targetId);        // STAIN_ADD_SEMEN_A
}
```

**Raw index constants** (to be defined as `private const int` in `RoomSmellService.cs` and `StainService.cs`, following ShootingSystem pattern):

- FLAG 500-549: primary room smell slots (ROOM_SMELL.ERB via DIM.ERH:487-489)
- FLAG 550-599: secondary room smell slots; **FLAG:550 (index 0 of secondaryFlagBase `部屋のにおい_初期FLAG_2`) is the ventilation timestamp** — set to current TIME by both `ROOM_SMELL_MORNING` (ERB:40) and `ROOM_SMELL_VENTILATION` (ERB:737). This is the "ventilation state" that AC#25 tests: `IVariableStore.GetFlag(550) == currentTime` after ProcessMorningPhase.
- FLAG 600+: visitor stain (STAIN.ERB:14-18 via DIM.ERH:489)
- CFLAG 800-806+: per-character smell (DIM.ERH:491-498)
- StainDurationIndex: raw integer index for stain duration timer (set to 120 by AddSemen*)

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `IRoomSmellService.AddCharacterSmell` signature uses `CharacterId` but `ProcessDayPhase`/`ProcessMorningPhase` take `int locationId` — the ERB functions take `ARG` (room index) not a typed CharacterId for room-level calls; only CHARA_SUB uses a character ID | AC#2, Interfaces / Data Structures | Verify that `GetCharaMostSmell` and `AddCharacterSmell` use `CharacterId`, while `ProcessDayPhase`, `ProcessMorningPhase`, `GetSmellBit/Value/Whose/Kinds` use `int roomIndex` — this is already the design above, no AC change needed |
| No existing `CharacterId` using ns confirmed in test files | (verification note) | `CharacterId` type used in CounterSourceHandler and VisitorSettings — confirmed available in `Era.Core.Types` namespace |

---

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 5, 18 | Create `Era.Core/State/SmellData.cs` as `readonly record struct` with `Whose`, `Value` (clamped [0,99]), `Kind` (clamped [0,65535]) properties; `Encode()` instance method and `static Decode(long raw)` factory; enforce clamping in constructor | | [x] |
| 2 | 1, 2 | Create `Era.Core/State/IRoomSmellService.cs` interface with file-scoped namespace `Era.Core.State`; declare all 8 methods: `ProcessDayPhase`, `ProcessMorningPhase`, `GetSmellBit`, `GetSmellValue`, `GetSmellWhose`, `GetSmellKinds`, `GetCharaMostSmell`, `AddCharacterSmell` | | [x] |
| 3 | 3, 4 | Create `Era.Core/State/IStainService.cs` interface with file-scoped namespace `Era.Core.State`; declare all 7 methods: `MoveByPenis`, `AddSemenHand`, `AddSemenB`, `AddSemenSumata`, `AddSemenM`, `AddSemenV`, `AddSemenA` | | [x] |
| 4 | 8, 11, 13, 17, 21, 24, 25, 26 | Create `Era.Core/State/RoomSmellService.cs` implementing `IRoomSmellService`; inject `IVariableStore`, `ILocationService`, `ICommonFunctions`; use FLAG 500-599 and CFLAG 800-806+ for state; decompose `ProcessDayPhase` into private helpers `ProcessBathReset`, `ProcessSmellCreation`, `ProcessStainTransfer` (minimum 3); apply room index > 30 guard; do NOT implement `WhoseSamen`/`WHOSE_SAMEN`/`GetWhoseSamen`; verify FlagRoomSmellInit=500 compatibility with ComableChecker; implement AC#25 ventilation behavior in `ProcessMorningPhase`: (1) call `IVariableStore.SetFlag(550, currentTime)` to update the ventilation timestamp at FLAG:550 (`部屋のにおい_初期FLAG_2` secondaryFlagBase), and (2) subtract decay amount (10) from SmellData.Value at FLAG:500+N (primary) and FLAG:550+N (secondary) for each room via decode→subtract→clamp→encode→store (matches ERB ROOM_SMELL_ROOM_MINUS:745-778) | | [x] |
| 5 | 9, 10, 16, 22, 23 | Create `Era.Core/State/StainService.cs` implementing `IStainService`; inject `IVariableStore`, `IEngineVariables`, `ICommonFunctions`; `MoveByPenis` dispatches on `GetSelectCom()` value for visitor/character/client/default branches; visitor branch uses `GetFlag/SetFlag` at FLAG 600+ range; character/client branches use `GetStain/SetStain` with bitwise OR; `AddSemen*` methods set duration timer to 120 | | [x] |
| 6 | 6, 7 | Write `Era.Core.Tests/State/SmellDataTests.cs`; include roundtrip test asserting `SmellData.Decode(original.Encode()) == original` for boundary values; include clamping tests: Whose -1 → 0, Value 100 → 99, Value -1 → 0, Kind 65536 → 65535 | | [x] |
| 7 | 8, 11, 15, 25, 26 | Write `Era.Core.Tests/State/RoomSmellServiceTests.cs`; include bath reset test (mock `ILocationService.IsBathroom` returns true → CFLAG 800-806+ zeroed); include room cap test (roomIndex 31 → no FLAG mutation, roomIndex 30 → FLAG mutation occurs); include decay and ventilation test for AC#25 — two assertions: (a) `IVariableStore.GetFlag(550)` equals injected currentTime after ProcessMorningPhase (ventilation timestamp — ERB ROOM_SMELL.ERB:40 `FLAG:(LOCAL:1) = TIME`, where LOCAL:1 = `部屋のにおい_初期FLAG_2` = FLAG:550), and (b) SmellData.Decode(FLAG:500+N).Value is reduced by the decay decrement amount after ProcessMorningPhase for a room with pre-set non-zero smell (decay via ROOM_SMELL_ROOM_MINUS, ERB:745-778); include smell creation test (ProcessDayPhase with stained character → room smell FLAG updated); include stain transfer test (ProcessDayPhase transfers character stain to room smell data); include query function behavioral tests (GetSmellBit/GetSmellValue/GetSmellWhose/GetSmellKinds/GetCharaMostSmell return correct decoded values from FLAG/CFLAG state) — per C10, decomposed sub-methods must be tested independently | | [x] |
| 8 | 9, 10, 15 | Write `Era.Core.Tests/State/StainServiceTests.cs`; include 4 `MoveByPenis` branch tests (visitor FLAG path, character STAIN bitwise OR, client STAIN bitwise OR, default no-op); include duration timer tests asserting each `AddSemen*` call sets duration index to 120 | | [x] |
| 9 | 12 | Register both services in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`: add `services.AddSingleton<IRoomSmellService, RoomSmellService>()` and `services.AddSingleton<IStainService, StainService>()` under "State Systems (Phase 22) - Feature 823" comment block | | [x] |
| 10 | 20 | Add `Resolve_IRoomSmellService()` and `Resolve_IStainService()` `[Fact]` methods to `Era.Core.Tests/E2E/DiResolutionTests.cs` using `_provider.GetRequiredService<>()` pattern matching existing resolution tests | | [x] |
| 11 | 14, 15, 19 | Run `dotnet test --filter "FullyQualifiedName~RoomSmell|FullyQualifiedName~StainService|FullyQualifiedName~SmellData"` and verify all pass; run `dotnet build src/Era.Core/ --warnaserror` and confirm zero warnings; verify no TODO/FIXME/HACK markers in the 5 new `.cs` files in `Era.Core/State/` | | [x] |

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

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `Era.Core/State/` directory, `SmellData.cs` design from Technical Design section | `SmellData.cs` (Tasks 1) |
| 2 | implementer | sonnet | `IRoomSmellService` and `IStainService` designs from Technical Design section | `IRoomSmellService.cs`, `IStainService.cs` (Tasks 2-3) |
| 3 | implementer | sonnet | `RoomSmellService.cs` design: IVariableStore injection, FLAG/CFLAG indices, decomposition pattern, room index guard, WHOSE_SAMEN deferral | `RoomSmellService.cs` (Task 4) |
| 4 | implementer | sonnet | `StainService.cs` design: IVariableStore injection, SELECTCOM dispatch, visitor FLAG path, bitwise OR pattern, duration timer 120 | `StainService.cs` (Task 5) |
| 5 | implementer | sonnet | `SmellDataTests.cs` spec: roundtrip test, clamping boundary tests | `SmellDataTests.cs` (Task 6) |
| 6 | implementer | sonnet | `RoomSmellServiceTests.cs` spec: bath reset, room cap boundary (30/31) | `RoomSmellServiceTests.cs` (Task 7) |
| 7 | implementer | sonnet | `StainServiceTests.cs` spec: 4 MoveByPenis branches, 6 AddSemen* duration tests | `StainServiceTests.cs` (Task 8) |
| 8 | implementer | sonnet | `ServiceCollectionExtensions.cs` current content, service registrations spec | Updated `ServiceCollectionExtensions.cs` (Task 9) |
| 9 | implementer | sonnet | `DiResolutionTests.cs` existing pattern, 2 new facts spec | Updated `DiResolutionTests.cs` (Task 10) |
| 10 | tester | sonnet | All 5 new `.cs` files in `Era.Core/State/`, test files, `ServiceCollectionExtensions.cs`, `DiResolutionTests.cs` | Test run PASS, build zero warnings, no debt markers confirmed (Task 11) |

### Pre-conditions

- F814 is [DONE] (Phase 22 Planning predecessor)
- `Era.Core/State/` directory exists (verify before Task 1)
- `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` exists (verify before Task 9)
- `Era.Core.Tests/E2E/DiResolutionTests.cs` exists (verify before Task 10)
- `IVariableStore`, `ILocationService`, `IEngineVariables`, `ICommonFunctions`, `CharacterId` types available in Era.Core (confirmed by Technical Design)

### Execution Order

Tasks 1 → 2 → 3 must complete before Tasks 4 and 5 (implementations depend on value type and interfaces).
Tasks 4 and 5 must complete before Tasks 7 and 8 (tests depend on concrete implementations).
Task 6 may be written in parallel with Tasks 4 and 5 (SmellData is independent).
Task 9 depends on Tasks 4 and 5 (DI registration requires concrete types to exist).
Task 10 depends on Task 9 (E2E test requires DI registration to be in place).
Task 11 must be the final task (verifies all prior work).

### Build Verification Steps

After each implementation task, verify: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build src/Era.Core/ --warnaserror'`

After all test tasks, run: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "FullyQualifiedName~RoomSmell|FullyQualifiedName~StainService|FullyQualifiedName~SmellData"'`

### Success Criteria

- All 11 Tasks completed with `[x]` status
- All 26 ACs verified as `[x]`
- Zero build warnings (`TreatWarningsAsErrors=true` enforced)
- No `WhoseSamen`/`WHOSE_SAMEN`/`GetWhoseSamen` in any new file
- No `TODO`/`FIXME`/`HACK` in any new file

### Error Handling

- If `CharacterId` type is not found: STOP → Report to user (namespace may differ from `Era.Core.Types`)
- If `ILocationService.IsBathroom` signature does not match RoomSmellService design: STOP → Report to user
- If `ServiceCollectionExtensions.cs` uses a different registration pattern than `AddSingleton`: STOP → Report to user
- If `DiResolutionTests.cs` does not exist or uses a different pattern than `GetRequiredService`: STOP → Report to user
- If build fails with TreatWarningsAsErrors after 2 fix attempts: STOP → Report to user

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| ROOM_SMELL_WHOSE_SAMEN function not migrated — DA variable interface gap (no GetDa/SetDa in I3DArrayVariables) | DA access is a cross-cutting concern; adding DA interface requires its own feature; AC#13 explicitly verifies absence | Feature | F825 | — (existing deferral, no creation task needed) | [x] | 確認済み |

<!-- Transferred + Result columns:
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-05T16:00:00Z | INIT | initializer | Status [REVIEWED] → [WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-05T16:05:00Z | INVESTIGATE | explorer | Codebase investigation: IVariableStore, ILocationService, ICommonFunctions, IEngineVariables, CharacterId, StainIndex, ServiceCollectionExtensions, DiResolutionTests, CounterSourceHandler patterns confirmed | All prerequisites exist |
<!-- run-phase-2-completed -->
| 2026-03-05T16:20:00Z | TDD_RED | implementer | Created stubs (RoomSmellService, StainService) + test files (SmellDataTests, RoomSmellServiceTests, StainServiceTests) | 22 FAIL, 10 PASS (RED confirmed) |
<!-- run-phase-3-completed -->
| 2026-03-05T16:35:00Z | IMPLEMENT | implementer | Task 4: RoomSmellService (12/12 tests PASS) | GREEN |
| 2026-03-05T16:40:00Z | IMPLEMENT | implementer | Task 5: StainService (10/10 tests PASS) | GREEN |
| 2026-03-05T16:45:00Z | IMPLEMENT | implementer | Tasks 9+10: DI registration + E2E tests (34/34 PASS) | GREEN |
| 2026-03-05T16:46:00Z | VERIFY | orchestrator | Task 11: Build 0 warnings, 34/34 tests PASS, no TODO/FIXME/HACK | PASS |
<!-- run-phase-4-completed -->
| 2026-03-05T16:50:00Z | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed) |
<!-- run-phase-5-completed -->
| 2026-03-05T16:55:00Z | AC_VERIFY | ac-tester | All 26 ACs verified | OK:26/26 |
<!-- run-phase-7-completed -->
| 2026-03-05T17:00:00Z | DEVIATION | feature-reviewer | Phase 8.1 Post-Review | NEEDS_REVISION: (1) ProcessMorningPhase missing secondary FLAG 550-599 decay, (2) Environment.TickCount non-injectable vs IEngineVariables.GetTime(), (3) long→int narrowing cast unguarded |
| 2026-03-05T17:10:00Z | FIX | debugger | Fix 3 review issues | All fixed: secondary decay loop added, IEngineVariables injected, checked() casts added. 34/34 PASS |
| 2026-03-05T17:11:00Z | Phase 8 | orchestrator | Step 8.2 skipped (new interfaces are Era.Core internal, not engine extensibility). Step 8.3 SSOT check: no updates needed | READY |
<!-- run-phase-8-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: Goal/Philosophy/C14/GoalCoverage | Revised "equivalence tests against ERB baseline" to "unit tests verifying behavioral spec derived from ERB source analysis" — aligned with Technical Design's explicit "No headless game execution" decision
- [fix] Phase2-Review iter1: AC#16 | Changed from code/Grep matcher to test-based verification — original Grep too broad (matches any GetFlag call, not specifically visitor branch)
- [fix] Phase2-Review iter2: AC#16 | Changed from duplicate test-based AC (identical to AC#9) to code-level Grep for FLAG 600+ constant in StainService.cs — now verifies source code reference, distinct from AC#9 behavioral test
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#24 (ComableChecker FlagRoomSmellInit=500 compatibility) — C13 constraint had documented AC Impact but no covering AC
- [fix] Phase2-Review iter2: AC#10 | Changed from fragile code/Grep (120.*duration pattern) to test-based verification — grep pattern wouldn't match actual implementation (SetStain with duration index)
- [fix] Phase2-Review iter2: AC Definition Table + Goal Coverage + Task 7 | Added AC#25 (ProcessMorningPhase decay behavior) — Philosophy "decay, ventilation" claim had no AC coverage
- [fix] Phase2-Review iter2: AC#11 | Changed from fragile code/Grep (bare "31" pattern) to test-based verification — pattern too broad, would match any number 31 in test files
- [fix] Phase2-Review iter3: Success Criteria | Changed "All 23 ACs" to "All 25 ACs" — AC count was stale after adding AC#24 and AC#25
- [fix] Phase2-Review iter4: AC#24 | Narrowed Grep target from Era.Core/ to RoomSmellService.cs — previous target was false-positive (FlagRoomSmellInit already exists in ComableChecker before implementation)
- [fix] Phase2-Review iter4: Task 7 | Extended to include smell creation and stain transfer tests — C10 requires decomposed sub-methods to be tested independently, not just counted (AC#17)
- [fix] Phase2-Review iter5: Task 7 | Extended to include query function behavioral tests (GetSmellBit/Value/Whose/Kinds/CharaMost) — AC#2 only verifies method names exist, query correctness needed behavioral coverage via AC#15
- [resolved-applied] Phase3-Maintainability iter6: Mandatory Handoffs F825 does not mention WHOSE_SAMEN or DA variable — destination feature lacks searchable reference to deferred item
- [fix] Phase3-Maintainability iter6: SmellData design | Added Whose >= 0 clamping (Math.Max(0, whose)) — negative Whose produces incorrect packed encoding; follows existing Value/Kind clamping pattern
- [fix] Phase3-Maintainability iter6: Key Decisions NTR wrappers | Clarified as optional implementation-level delegation, ERB bridge calls AddCharacterSmell directly — no AC/Task verification needed
- [fix] Phase3-Maintainability iter6: Technical Design StainService | Added GetSelectCom coupling rationale — ERB SELECTCOM is the only mechanism to distinguish visitor/character/client context
- [fix] Phase3-Maintainability iter6: AC#17 | Tightened pattern from generic private method count to specific named methods (ProcessBathReset|ProcessSmellCreation|ProcessStainTransfer)
- [fix] Phase2-Review iter7: AC#7 Details + Task 6 | Added Whose -1 → 0 boundary test — iter6 added Whose clamping to SmellData but AC/Task spec was not updated
- [fix] Phase2-Review iter8: Philosophy Derivation | Added IStainService row for "replacing raw STAIN manipulation" claim — was only mapped to SmellData ACs
- [fix] Phase2-Review iter8: AC#23 | Tightened Grep pattern from generic case/switch to domain-specific [Vv]isitor|[Cc]haracter|[Cc]lient — previous pattern matched any switch statement
- [fix] Phase2-Review iter8: Task 4 AC column | Added AC#25 — Task 4 implements ProcessMorningPhase but was missing AC#25 reference
- [fix] Phase2-Review iter9: AC#6 | Narrowed Grep path from Era.Core.Tests/ to Era.Core.Tests/State/SmellDataTests.cs — directory-level grep was too broad
- [fix] Phase4-ACValidation iter10: AC#13 + AC#14 | Fixed Method column format — comma-separated file paths replaced with directory path (path="src/Era.Core/State/") for ac-verifier parsability
- [fix] Phase1-RefCheck iter10: Links section | Added F813 (E2E DI resolution test pattern reference) — referenced in AC#20 rationale but missing from Links
- [fix] Phase1-RefCheck iter10: Baseline File | Annotated as "(generated during /run Phase 1)" — clarifies artifact is not pre-existing
- [fix] Phase2-Uncertain iter10: Philosophy Derivation | Added "smell state machine: creation, decay, ventilation, query" row mapping to AC#17, AC#25, AC#15 — creation/ventilation claims lacked explicit Philosophy traceability
- [fix] Phase2-Review iter11: AC#25 Details | Added Disambiguation note — AC#8/AC#11/AC#25 share test filter but target distinct [Fact] methods; individual test failure isolates specific AC
- [fix] Phase3-Maintainability iter11: F825 Problem section | Added WHOSE_SAMEN/DA variable gap deferred note — resolved [pending] from iter6
- [fix] Phase3-Maintainability iter11: AC#25 | Expanded to explicitly include ventilation behavior assertion alongside decay — Philosophy "ventilation" claim now has dedicated test coverage
- [fix] Phase3-Maintainability iter11: AC Definition Table + Details + Philosophy Derivation + Goal Coverage + Task 4/7 | Added AC#26 (ProcessDayPhase smell creation behavior) — Philosophy "creation" claim was only verified structurally (AC#17 method name) not behaviorally
- [fix] Phase2-Review iter1: AC#25 Details / Technical Design | Defined "ventilation" concretely — FLAG:550 timestamp, ERB ROOM_SMELL.ERB:40 and ROOM_SMELL_VENTILATION:734-742; updated AC#25 Details, Task 4/7 implementation contracts, Technical Design raw index constants
- [fix] Phase2-Uncertain iter1: C4 AC Implication | Updated to align with Key Decisions — NTR_PET/ORAL/SEX bridge compatibility ensured by original ERB files remaining unmodified, no separate AC needed

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Related: F803](feature-803.md) - CounterSourceHandler migration, CrossContaminateStain pattern (reference implementation)
- [Related: F811](feature-811.md) - ShootingSystem ejaculation processing pattern (reference implementation)
- [Related: F819](feature-819.md) - Sibling: Clothing System (no cross-calls)
- [Related: F821](feature-821.md) - Sibling: Weather System (no cross-calls)
- [Related: F822](feature-822.md) - Sibling: Pregnancy System (no cross-calls)
- [Related: F824](feature-824.md) - Sibling: Sleep & Menstrual (no cross-calls)
- [Successor: F825](feature-825.md) - Relationships & DI Integration (depends on F823 services)
- [Related: F813](feature-813.md) - E2E DI resolution test pattern reference
- [Successor: F826](feature-826.md) - Post-Phase Review (depends on F823 completion)
