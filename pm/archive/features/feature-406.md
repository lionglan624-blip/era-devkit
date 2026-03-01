# Feature 406: Equipment/OrgasmProcessor Completion

## Status: [DONE]

## Phase: 7 (Technical Debt Consolidation)

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

## Created: 2026-01-08

---

## Summary

Complete 100% implementation of Equipment and Orgasm processors by filling in all TODO-marked stubs. F402 established StateChange type safety; this feature completes the processor business logic.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立。

OrgasmProcessor/EquipmentProcessor がスケルトン状態のまま残存。本Featureは Processor 100%実装により技術負債ゼロ原則に貢献する。

### Problem (Current Issue)

Equipment and Orgasm processors currently have skeleton implementations with extensive TODO stubs:

| File | Issue | Lines |
|------|-------|-------|
| EquipmentProcessor.cs | ProcessEquipment returns empty result | 26-72 |
| OrgasmProcessor.cs | CalculateOrgasmCandidates stub | 105-113 |
| OrgasmProcessor.cs | CalculateOrgasmIntensities stub | 160-183 |
| OrgasmProcessor.cs | CalculatePleasureMarkStrength missing MARK check | 190-212 |
| OrgasmProcessor.cs | UpdateNowexFlags missing NOWEX writes | 335-362 |
| OrgasmProcessor.cs | UpdateOrgasmCounters missing EX writes | 368-381 |
| OrgasmProcessor.cs | UpdateOrgasmExperience commented out | 387-410 |

F402 completed StateChange type safety migration but explicitly deferred processor logic completion to this feature.

### Goal (What to Achieve)

1. Complete EquipmentProcessor.ProcessEquipment with all equipment handlers (18 handlers + CLOTHE_EFFECT + 2 insertion handlers)
2. Complete OrgasmProcessor calculation methods (CalculateOrgasmCandidates, CalculateOrgasmIntensities)
3. Integrate variable access (CUP, PALAM, PALAMLV, TEQUIP, MARK, NOWEX, EX, EXP) via IVariableStore
4. Remove all TODO stubs from both processors
5. Ensure 100% business logic coverage matching TRACHECK_EQUIP.ERB and TRACHECK_ORGASM.ERB

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | EquipmentProcessor ProcessEquipment implements equipment checks | code | Grep EquipmentProcessor.cs | contains | if (GetEquipmentFlag | [x] |
| 2 | EquipmentProcessor implements CLOTHE_EFFECT call | code | Grep EquipmentProcessor.cs | contains | ProcessClotheEffect | [x] |
| 3 | EquipmentProcessor implements V/A insertion checks | code | Grep EquipmentProcessor.cs | contains | ProcessVInsertion | [x] |
| 4 | No TODO stubs in EquipmentProcessor | code | Grep EquipmentProcessor.cs | not_contains | TODO: Full implementation | [x] |
| 5a | OrgasmProcessor CalculateOrgasmCandidates uses CUP | code | Grep OrgasmProcessor.cs | contains | GetCup(target | [x] |
| 5b | OrgasmProcessor CalculateOrgasmCandidates uses PALAM | code | Grep OrgasmProcessor.cs | contains | GetPalam(target | [x] |
| 6 | OrgasmProcessor CalculateOrgasmIntensities uses PALAMLV thresholds | code | Grep OrgasmProcessor.cs | contains | GetPalamLv | [x] |
| 7 | OrgasmProcessor CalculateOrgasmIntensities writes CDOWN | code | Grep OrgasmProcessor.cs | contains | SetCDown | [x] |
| 8 | OrgasmProcessor CalculatePleasureMarkStrength checks MARK | code | Grep OrgasmProcessor.cs | contains | GetMark(target | [x] |
| 9 | OrgasmProcessor UpdateNowexFlags writes NOWEX via StateChange | code | Grep OrgasmProcessor.cs | contains | NowExChange | [x] |
| 10 | OrgasmProcessor UpdateOrgasmCounters writes EX via StateChange | code | Grep OrgasmProcessor.cs | contains | ExChange | [x] |
| 11 | OrgasmProcessor UpdateOrgasmExperience writes EXP via StateChange | code | Grep OrgasmProcessor.cs | contains | ExpChange | [x] |
| 12 | No TODO stubs in OrgasmProcessor | code | Grep OrgasmProcessor.cs | not_contains | TODO: Implement when | [x] |
| 13 | No FIXME/HACK comments in processors | code | Grep Era.Core/Training/ | not_contains | FIXME\|HACK | [x] |
| 14 | ERB equivalence test file exists | file | Glob | exists | Era.Core.Tests/**/OrgasmProcessorEquivalenceTests.cs | [x] |
| 15 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 16 | All Training tests pass | test | dotnet | succeeds | Category=Training | [x] |

### AC Details

**AC#1**: EquipmentProcessor implements equipment flag checks:
```csharp
// Example pattern (from TRACHECK_EQUIP.ERB lines 7-8):
// SIF TEQUIP:ARG:クリキャップ
//     CALL EQUIP_COM42(ARG)

if (GetEquipmentFlag(target, EquipmentType.ClitCap))
{
    ProcessClitCap(target, result);
}
```

Expected: Lines 26-72 TODO block replaced with actual equipment checks for all 18 equipment types.

**AC#2**: EquipmentProcessor calls CLOTHE_EFFECT handler:
```csharp
ProcessClotheEffect(target, result);
```

Expected: Line 82 equivalent implemented (based on TRACHECK_EQUIP.ERB line 82).

**AC#3**: EquipmentProcessor implements V/A insertion processing:
```csharp
// Lines 87-88: SIF TEQUIP:ARG:Ｖセックス >= 0 && !SOURCE:ARG:快Ｖ
var vInsertPartner = GetVInsertionPartner(target);
if (vInsertPartner >= 0 && GetSource(target, SourceIndex.PleasureV) == 0)
{
    ProcessVInsertion(target, vInsertPartner, result);
}
```

Expected: Lines 86-92 equivalent implemented with ProcessVInsertion and ProcessAInsertion handlers.

**AC#4**: EquipmentProcessor TODO removal verification:
```bash
grep -c "TODO: Full implementation" Era.Core/Training/EquipmentProcessor.cs
# Expected: 0
```

**AC#5**: OrgasmProcessor uses CUP and PALAM for candidate calculation:
```csharp
// Lines 105-110: 絶頂候補 += ((CUP:奴隷:快Ｃ + PALAM:奴隷:快Ｃ) >= PALAMLV:4)
// Note: OrgasmProcessor uses ITrainingVariables for GetCup, IVariableStore for GetPalam (Task 0)
// Note: PalamIndex lacks well-known constants. Use explicit constructor: new PalamIndex(0) for PleasureC
var cupC = _trainingVariables.GetCup(target, CupIndex.PleasureC).ValueOr(0);
var palamC = _variableStore.GetPalam(target, new PalamIndex(0)).ValueOr(0);  // PleasureC = 0
candidates += ((cupC + palamC) >= _juelVariables.GetPalamLv(4).ValueOr(0)) ? 1 : 0;
```

Expected: Lines 105-113 TODO block replaced with actual CUP+PALAM checks for C/V/A/B sites.

**AC#6**: OrgasmProcessor uses PALAMLV thresholds:
```csharp
// Lines 47-63: Check pleasure thresholds for orgasm intensity (2x/1x/acting)
// Note: OrgasmProcessor constructor updated to take IJuelVariables (Task 0)
var threshold = _juelVariables.GetPalamLv(4).ValueOr(0);
var doubleThreshold = threshold * 2;
```

Expected: Lines 160-183 TODO block replaced with PALAMLV threshold checks.

**AC#7**: OrgasmProcessor writes CDOWN to reduce post-orgasm pleasure:
```csharp
// Lines 47-63: Set CDOWN to reduce pleasure
// Note: OrgasmProcessor constructor updated to take ICharacterStateVariables (Task 0)
_characterStateVariables.SetCDown(target, PalamIndex.PleasureC, reductionAmount);
```

Expected: Lines 160-183 implement CDOWN writes matching TRACHECK_ORGASM.ERB lines 47-114.

**AC#8**: OrgasmProcessor checks MARK level for pleasure mark strength:
```csharp
// Lines 118-132: IF LOCAL >= 3 && MARK:奴隷:快楽刻印 < 3
// Note: OrgasmProcessor constructor updated to take ICharacterStateVariables (Task 0)
var currentMark = _characterStateVariables.GetMark(target, MarkIndex.Pleasure).ValueOr(0);
if (total >= 3 && currentMark < 3)
{
    strengthResult = 3;
}
```

Expected: Lines 190-212 implement MARK checks matching TRACHECK_ORGASM.ERB lines 118-132.

**AC#9**: OrgasmProcessor writes NOWEX flags via StateChange pattern:
```csharp
// Lines 216-220: NOWEX:奴隷:Ｃ絶頂 = Ｃ絶頂強度
result.AddChange(new NowExChange(NowExIndex.OrgasmC, c));
```

Expected: Lines 335-362 already use NowExChange pattern. Remove TODO comment only.

**AC#10**: OrgasmProcessor writes EX counters via StateChange pattern:
```csharp
// Lines 221-225: EX:奴隷:Ｃ絶頂 += Ｃ絶頂強度
if (c > 0) result.AddChange(new ExChange(ExIndex.OrgasmC, c));
```

Expected: Lines 368-381 already use ExChange pattern. Remove TODO comment only.

**AC#11**: OrgasmProcessor writes EXP counters via StateChange pattern:
```csharp
// Lines 226-231: EXP:奴隷:絶頂経験 += Ｃ絶頂強度 + Ｖ絶頂強度 + Ａ絶頂強度 + Ｂ絶頂強度
int totalExp = c + v + a + b;
if (totalExp > 0)
{
    result.AddChange(new ExpChange(ExpIndex.OrgasmExperience, totalExp));
}
```

Expected: Lines 387-410 already use ExpChange pattern. Verify implementation is complete.

**AC#12**: OrgasmProcessor TODO removal verification:
```bash
grep -c "TODO: Implement when" Era.Core/Training/OrgasmProcessor.cs
# Expected: 0
```

**AC#13**: Build verification:
```bash
dotnet build Era.Core/Era.Core.csproj
# Expected: Build succeeded
```

**AC#14**: Training test suite verification:
```bash
dotnet test Era.Core.Tests --filter "Category=Training"
# Expected: All tests pass
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Update processor constructors with required interface DI (IVariableStore, ITrainingVariables, IJuelVariables, ITEquipVariables, ICharacterStateVariables) | [x] |
| 1 | 1 | Create GetEquipmentFlag, GetVInsertionPartner, GetAInsertionPartner helper methods | [x] |
| 2 | 1-4 | Complete EquipmentProcessor implementation (18 handlers + TODO removal) | [x] |
| 3 | 5a,5b | Implement CalculateOrgasmCandidates with CUP+PALAM | [x] |
| 4 | 6-7 | Implement CalculateOrgasmIntensities with thresholds and CDOWN | [x] |
| 5 | 8 | Implement CalculatePleasureMarkStrength with MARK checks | [x] |
| 6 | 9-11 | Remove TODO comments from Update methods (StateChange patterns already implemented) | [x] |
| 7 | 12 | Remove all TODO stubs from OrgasmProcessor | [x] |
| 8 | 13 | Remove all FIXME/HACK comments from Era.Core/Training/ | [x] |
| 9 | 14 | Create OrgasmProcessorEquivalenceTests.cs verifying ERB parity | [x] |
| 10 | 15 | Verify C# build succeeds | [x] |
| 11 | 16 | Verify Training tests pass | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Note: T1-8 implement, T9-10 technical debt cleanup, T11-12 verify -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Dependency Requirements

**PREVIOUSLY BLOCKED BY**: F412 (TEQUIP/CDOWN Variable Accessor Addition) for GetTEquip/SetTEquip, GetCDown/SetCDown, GetEx/SetEx methods - RESOLVED.

**PREVIOUSLY BLOCKED BY**: F403 (Character namespace StateChange migration) for ExpChange usage - RESOLVED.

**REQUIRES**: IVariableStore must expose the following methods for processor completion:
- GetCup/SetCup (CupIndex: PleasureC/V/A/B)
- GetPalam/SetPalam (PalamIndex: PleasureC/V/A/B)
- GetPalamLv (level threshold access)
- GetTEquip/SetTEquip (equipment flag access)
- GetMark/SetMark (MarkIndex: PleasureMark)
- GetNowEx/SetNowEx (NowExIndex: OrgasmC/V/A/B, DoubleOrgasm, TripleOrgasm, QuadOrgasm)
- GetEx/SetEx (ExIndex: OrgasmC/V/A/B)
- GetExp/SetExp (ExpIndex: OrgasmExperience, OrgasmExperienceC/V/A/B)
- GetCDown/SetCDown (pleasure reduction after orgasm)
- GetSource (SourceIndex: PleasureV/A for insertion checks)

If any methods are missing from IVariableStore, this feature is BLOCKED and must report to user.

### Implementation Steps

| Step | Component | Action |
|:----:|-----------|--------|
| 0 | Constructor DI | Update processor constructors to inject required interfaces (IVariableStore for GetPalam, ITrainingVariables for GetCup, IJuelVariables for GetPalamLv, ITEquipVariables for GetTEquip, ICharacterStateVariables for GetMark/SetCDown/SetNowEx/SetEx/SetExp) |
| 1 | EquipmentProcessor | Add equipment flag helper methods (GetEquipmentFlag, GetVInsertionPartner, GetAInsertionPartner) wrapping ITEquipVariables/ICharacterStateVariables |
| 2 | EquipmentProcessor | Implement 18 equipment handlers (ProcessClitCap, ProcessOnahole, ..., ProcessNewlywedPlay) |
| 3 | EquipmentProcessor | Implement ProcessClotheEffect, ProcessVInsertion, ProcessAInsertion |
| 4 | EquipmentProcessor | Replace lines 26-72 TODO with equipment checks calling handlers |
| 5 | OrgasmProcessor | Implement CalculateOrgasmCandidates using CUP+PALAM access via ITrainingVariables |
| 6 | OrgasmProcessor | Implement CalculateOrgasmIntensities using PALAMLV thresholds via IJuelVariables and CDOWN writes via ICharacterStateVariables.SetCDown |
| 7 | OrgasmProcessor | Update CalculatePleasureMarkStrength to check MARK level via ICharacterStateVariables.GetMark |
| 8 | OrgasmProcessor | Remove TODO comments from Update methods (NowExChange/ExChange/ExpChange patterns already implemented) |
| 9 | Verification | Build and test with Training test suite |

**Note on AC#9-11**: The current implementation already uses StateChange pattern (NowExChange, ExChange, ExpChange). Task is to remove TODO comments, not to replace with direct variable writes.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-08 FL**: NOT FEASIBLE - IVariableStore lacks GetTEquip/SetTEquip, GetCDown/SetCDown, GetEx/SetEx. Created F412 as prerequisite. Status changed to [BLOCKED].
- **2026-01-09 FL**: F412 completed. Blocker resolved. Feature unblocked and re-reviewed.

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 09:30 | START | implementer | Task 0 | - |
| 2026-01-09 09:30 | END | implementer | Task 0 | SUCCESS |
| 2026-01-09 09:33 | START | implementer | Task 1 | - |
| 2026-01-09 09:33 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 09:34 | START | implementer | Task 2 | - |
| 2026-01-09 09:37 | END | implementer | Task 2 | SUCCESS |
| 2026-01-09 09:38 | START | implementer | Task 3 | - |
| 2026-01-09 09:42 | END | implementer | Task 3 | SUCCESS |
| 2026-01-09 09:44 | START | implementer | Task 4 | - |
| 2026-01-09 09:44 | END | implementer | Task 4 | SUCCESS |
| 2026-01-09 09:46 | START | implementer | Task 5 | - |
| 2026-01-09 09:46 | END | implementer | Task 5 | SUCCESS |
| 2026-01-09 09:48 | START | implementer | Tasks 6-8 | - |
| 2026-01-09 09:48 | END | implementer | Tasks 6-8 | SUCCESS |
| 2026-01-09 09:50 | START | implementer | Task 9 (TDD) | - |
| 2026-01-09 09:50 | END | implementer | Task 9 (TDD) | SUCCESS |
| 2026-01-09 09:51 | START | - | Task 10-11 (Verify) | - |
| 2026-01-09 09:51 | END | - | Task 10-11 (Verify) | SUCCESS - Build OK, 115 tests PASS |
| 2026-01-09 09:55 | DEVIATION | Bash | dotnet test | ArchitectureTests.AC7 FAIL (expected 6, actual 7) |
| 2026-01-09 09:56 | START | debugger | Fix AC7 test | - |
| 2026-01-09 09:56 | END | debugger | Fix AC7 test | SUCCESS - Updated expected count from 6 to 7 (EquipmentIndex added) |
| 2026-01-09 09:57 | START | - | Phase 6 Verification | - |
| 2026-01-09 09:57 | END | - | Phase 6 Verification | SUCCESS - 336 tests PASS, all AC verified |
| 2026-01-09 10:00 | START | feature-reviewer | post | - |
| 2026-01-09 10:00 | END | feature-reviewer | post | READY |
| 2026-01-09 10:02 | START | feature-reviewer | doc-check | - |
| 2026-01-09 10:02 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION - EquipmentIndex.cs not in engine-dev SKILL.md |
| 2026-01-09 10:03 | END | - | doc-check fix | SUCCESS - Added EquipmentIndex to engine-dev SKILL.md |
| 2026-01-09 10:15 | START | - | Deferred items handoff | - |
| 2026-01-09 10:15 | END | - | Deferred items handoff | SUCCESS - Added to full-csharp-architecture.md (Phase 10, 20) |

## Links
- [F412: TEQUIP/CDOWN Variable Accessor Addition](feature-412.md) - **BLOCKER**: Provides required accessor methods
- [F402: StateChange Equipment/Orgasm Migration](feature-402.md) - Dependency: StateChange type safety
- [F403: Character Namespace StateChange Migration](feature-403.md) - Dependency: ExpChange usage
- [F398: Phase 7 Planning](feature-398.md) - Parent planning feature
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 design reference (lines 1806, 1913-1914, 1929)
