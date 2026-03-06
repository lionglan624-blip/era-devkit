# Feature 371: NTR Initialization Functions Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB external dependency: NTR_SET_STAYOUT_MAXIMUM function from NTR_TAKEOUT.ERB to C#.

**Context**: Phase 3 from full-csharp-architecture.md (line 1149). Supports F365 (SYSTEM.ERB) migration.

**Note**: NTR_TAKEOUT.ERB is 1,792 lines, but only NTR_SET_STAYOUT_MAXIMUM (partial migration) is called by SYSTEM.ERB.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Initialization Stack**: F365 migrates SYSTEM.ERB handlers, but they depend on external functions. F371 migrates NTR initialization function.

### Problem (Current Issue)

SYSTEM.ERB calls:
- NTR_SET_STAYOUT_MAXIMUM (NTR_TAKEOUT.ERB:1634) - sets NTR stayout limits

**Scope Limitation**: This feature migrates ONLY NTR_SET_STAYOUT_MAXIMUM, not the entire NTR_TAKEOUT.ERB. Full NTR system migration is Phase 17.

### Goal (What to Achieve)

1. Analyze NTR_SET_STAYOUT_MAXIMUM function and its dependencies
2. Create Era.Core/State/NtrInitialization.cs with stayout limit function (using getCflag/setCflag accessor pattern for testability, following F370 State/ convention)
3. Create MSTest test cases
4. Integrate with F365 GameInitialization.cs

---

## Function Analysis

*Preliminary analysis completed. Task 1 will verify and finalize the documentation below.*

**Target Function**: NTR_SET_STAYOUT_MAXIMUM (NTR_TAKEOUT.ERB:1634)

**Preliminary Analysis Notes**:
- CFLAG index mapping: 長期滞在最大日数 = CFLAG index 22 (per Game/CSV/CFLAG.csv)
- Bit manipulation: bits 4-7 (mask 0b11110000) = maximum days, bits 0-3 (mask 0b00001111) = current days. Note: ERB uses 1-based bit numbering (bits 5-8 in comments); C# uses 0-based (bits 4-7).
- Both fields store 4-bit values (implementation range 0-7 per ERB comments, though 4-bit mask allows 0-15)
- Read-modify-write pattern: getCflag to read current value, clear HIGH nibble (bits 4-7), set new max days while preserving LOW nibble (bits 0-3 = current days), setCflag to write back
- C# method signature with accessor parameters (getCflag, setCflag)

**Related Functions** (NOT migrated in F371, for Phase 17 reference):
- NTR_GET_STAYOUT_CURRENT (line 1609) - read current stayout days
- NTR_GET_STAYOUT_MAXIMUM (line 1617) - read maximum stayout days
- NTR_SET_STAYOUT_CURRENT (line 1624) - write current stayout days

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Function analysis documented | file | Grep | contains | "## Function Analysis" in feature-371.md | [x] |
| 2 | NtrInitialization.cs created | file | Glob | exists | Era.Core/State/NtrInitialization.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | Unit tests created | file | Glob | exists | engine.Tests/Tests/NtrInitializationTests.cs | [x] |
| 5 | Unit tests pass | test | dotnet test | succeeds | - | [x] |
| 6 | GameInitialization calls NtrInitialization | file | Grep | contains | "NtrInitialization." in GameInitialization.cs | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze NTR_SET_STAYOUT_MAXIMUM: document bit manipulation behavior (high nibble storage), CFLAG:22 dependency | [x] |
| 2 | 2 | Create NtrInitialization.cs with SetStayoutMaximum using read-modify-write pattern: getCflag(characterId, 22) → bit manipulation → setCflag(characterId, 22, newValue) | [x] |
| 3 | 3,4,5 | Create unit tests, verify build and tests pass | [x] |
| 4 | 6 | Update GameInitialization.NTRSetStayoutMaximum stub to call NtrInitialization | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Constants.cs required for NTR constants |
| Predecessor | F365 | GameInitialization.cs structure to integrate with |
| Successor | F365 | Enables full SYSTEM.ERB migration |
| Related | Phase 17 | Full NTR system migration (NTR_TAKEOUT.ERB complete) |

**Dependency Chain**:
```
F364 (Constants) → F365 (SYSTEM.ERB) + F371 (NTR Init) → Full C# initialization
```

**Scope Note**: F371 is partial migration of NTR_TAKEOUT.ERB. Phase 17 completes full NTR system.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 (line 1149)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- [feature-366.md](feature-366.md) - COMMON.ERB (accessor pattern reference)
- [feature-370.md](feature-370.md) - Body Initialization (pattern reference)
- Game/ERB/NTR/NTR_TAKEOUT.ERB - Source file (1,792 lines, partial migration)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as F365 external dependency (NTR partial) | PROPOSED |
| 2026-01-06 16:46 | START | implementer | Task 2 | - |
| 2026-01-06 16:46 | END | implementer | Task 2: Created NtrInitialization.cs, Updated GameInitialization stub | SUCCESS |
