# Feature 365: SYSTEM.ERB Game Initialization Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB game initialization to C# GameInitialization.cs. This establishes event initialization handlers (@EVENTFIRST, @EVENTLOAD, @QUICK_START_SETUP) and mode flags in C#.

**Context**: Phase 3 Task 1 from full-csharp-architecture.md. Requires F364 Constants.cs completion for variable access.

**Note**: SYSTEM.ERB (242 lines) contains @EVENTFIRST (143 lines with mode selection, character setup, 8 external CALLs), @EVENTLOAD (1 line), and @QUICK_START_SETUP (87 lines with 1 CALL + 3 TRYCALL targets). The main loop already exists in C# engine (uEmuera.Headless, Unity frontend).

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: SYSTEM.ERB contains game initialization handlers - the startup configuration that sets mode flags and initializes game state. Migrating this to C# enables type-safe initialization and prepares for Phase 4+ feature migrations.

### Problem (Current Issue)

SYSTEM.ERB (242 lines verified per F363 analysis) contains three handlers:
- @EVENTFIRST: New game initialization (mode selection, quick start) - 143 lines
- @EVENTLOAD: Save file load handler - 1 line
- @QUICK_START_SETUP: Quick start configuration - 87 lines

External calls (delegated to F367-F371): DEFAULT_OPTION, SHORT_CUT_MODE, VERSION_UP, CUSTOM_CHARAMAKE, etc.

**Key Observation**: This SYSTEM.ERB is MINIMAL compared to typical ERA games (~2000 lines). Main loop logic is already in the C# engine.

**Current State**:
- Event initialization executed in ERB interpreter
- Mode flags set via ERB variables
- Phase 4+ features cannot integrate with C# initialization until migration complete

**Phase 2 Context**:
- F359: MSTest infrastructure ready for GameInitialization testing
- F362: Test migration patterns established
- Test strategy: Create C# tests before removing ERB code

### Goal (What to Achieve)

1. Analyze SYSTEM.ERB structure and identify initialization handlers
2. Create Era.Core/Common/GameInitialization.cs with initialization methods
3. Implement C# methods equivalent to @EVENTFIRST initialization logic including mode flags (FLAG:ゲームモード bits 1-4: futanari, buxom, energetic visitor, NTR all-on; FLAG:ＮＴＲパッチ設定)
4. Create MSTest test cases for initialization verification
5. Verify ERA engine headless mode compatibility
6. Document migration pattern for Phase 4+ features

**Scope Boundaries**:
- **In-scope**: Mode flag bit manipulation (bits 1-4 on FLAG:ゲームモード; bits 0,1,3-8,24-26,31-32 on FLAG:ＮＴＲパッチ設定), quick start state setup, character addition flow
- **Out-of-scope (delegated)**: External CALLs - DEFAULT_OPTION→F367, CUSTOM_CHARAMAKE/CHARA_CUSTUM/VIRGIN_CUSTOM/REVERSEMODE_1→F368, CLOTHES_SETTING→F369, 体詳細初期設定/子宮内体積設定/気温耐性取得→F370, NTR_SET_STAYOUT_MAXIMUM→F371

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SYSTEM.ERB analysis documented | file | Grep | contains | "## SYSTEM.ERB Analysis" | [x] |
| 2 | GameInitialization.cs created | file | Glob | exists | Era.Core/**/GameInitialization.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | GameInitialization tests created | file | Glob | exists | engine.Tests/**/GameInitializationTests.cs | [x] |
| 5 | GameInitialization tests pass | test | dotnet test engine.Tests/ | contains | "GameInitializationTests" | [x] |
| 6 | Headless mode compatibility | test | dotnet test engine.Tests/ | contains | "HeadlessIntegrationTests" | [x] |
| 7 | Migration pattern documented | file | Grep | contains | "## Migration Pattern" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze SYSTEM.ERB: document each handler's behavior (@EVENTFIRST, @EVENTLOAD, @QUICK_START_SETUP), list external CALL/TRYCALL targets with file locations, assess migration feasibility per handler | [x] |
| 2 | 2 | Create Era.Core/Common/GameInitialization.cs with initialization methods, mode flag handling (using Constants.cs), and stub interfaces for external calls (to be implemented by F367-F371) | [x] |
| 3 | 3 | Verify C# build succeeds with GameInitialization.cs | [x] |
| 4 | 4 | Create engine.Tests/Tests/GameInitializationTests.cs using F359/F364 MSTest patterns | [x] |
| 5 | 5 | Verify GameInitialization tests pass | [x] |
| 6 | 6 | Create HeadlessIntegrationTests.cs verifying: (1) GameInitialization methods callable without full ERA engine, (2) Mode flag operations work correctly, (3) Stub interfaces for F367-F371 are defined | [x] |
| 7 | 7 | Document migration pattern for Phase 4+ features (how to hook into initialization events) | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F363 | Phase 3 Planning complete |
| Predecessor | F364 | Constants.cs required for variable access |
| Predecessor | F359 | MSTest structure for GameInitialization testing |
| Successor | F367 | Options & Utilities (implements external calls: DEFAULT_OPTION, SHORT_CUT_MODE, VERSION_UP) |
| Successor | F368 | Character Setup (implements external calls: CUSTOM_CHARAMAKE, CHARA_CUSTUM, etc.) |
| Successor | F369 | Clothing System (implements external calls: CLOTHES_SETTING) |
| Successor | F370 | Body & State (implements external calls: 体詳細初期設定, 子宮内体積設定, 気温耐性取得) |
| Successor | F371 | NTR Initialization (implements external calls: NTR_SET_STAYOUT_MAXIMUM) |

**Dependency Chain**:
```
F363 (Planning) → F364 (Constants) → F365 (SYSTEM.ERB handlers)
                                            ↓
                                   F367-F371 (external function implementations)
                                            ↓
                                   Full C# initialization stack
```

**Critical Path**: F364 MUST complete before F365. F365 provides stub interfaces, then F367-F371 implement external functions.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 (lines 886-908)
- [feature-363.md](feature-363.md) - Phase 3 Planning (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- [feature-359.md](feature-359.md) - Test Structure (MSTest foundation)
- [feature-362.md](feature-362.md) - Test Migration (patterns reference)
- [feature-367.md](feature-367.md) - Options & Utilities (external dependency)
- [feature-368.md](feature-368.md) - Character Setup (external dependency)
- [feature-369.md](feature-369.md) - Clothing System (external dependency)
- [feature-370.md](feature-370.md) - Body & State Systems (external dependency)
- [feature-371.md](feature-371.md) - NTR Initialization (external dependency)
- Game/ERB/SYSTEM.ERB - Source file to migrate

---

## SYSTEM.ERB Analysis

### Handler Overview

SYSTEM.ERB contains three primary handlers that manage game initialization lifecycle:

1. **@EVENTFIRST** (lines 4-146): New game initialization
2. **@EVENTLOAD** (lines 148-149): Save file load handler
3. **@QUICK_START_SETUP** (lines 155-241): Quick start configuration

### @EVENTFIRST Handler (143 lines)

**Purpose**: Initialize new game with mode selection and character setup

**Flow**:
1. **Initial Setup** (lines 5-10):
   - Initialize TARGET, ASSI to 0
   - Set PBAND to 12
   - Call DEFAULT_OPTION (external)

2. **Mode Selection Loop** (lines 13-60):
   - Display mode options with current state (☆ indicates enabled)
   - Bit 1: Futanari mode
   - Bit 2: Buxom mode
   - Bit 3: Energetic visitor mode
   - Bit 4: NTR all-on mode
   - Option 9: Quick start (skip to SHOP)
   - Options 1-4 toggle FLAG:ゲームモード bits via INVERTBIT
   - Option 9 calls QUICK_START_SETUP and begins SHOP

3. **Character Creation** (lines 62-89):
   - Prompt for role-play mode
   - Option 0: Custom player character (CUSTOM_CHARAMAKE), then add characters 1-13
   - Option 1: Reverse mode (REVERSEMODE_1)
   - Set MAXBASE:MASTER:5 = 1500 if player has penis

4. **Mode Application** (lines 91-117):
   - If futanari mode (bit 1): Set all characters to 性別 = 3
   - If buxom mode (bit 2): Set all characters to バストサイズ = 2
   - If NTR all-on (bit 4): Toggle 12 NTR patch flags (bits 1,3-8,24-26,31-32), set stayout maximum to 3 days

5. **Character Customization** (lines 119-127):
   - Set SAVESTR:10 = "/"
   - Call CHARA_CUSTUM (external)
   - Call VIRGIN_CUSTOM (external)
   - Set MAXBASE:MASTER:5 = 1500 if player has penis

6. **Finalization** (lines 129-146):
   - Call SHORT_CUT_MODE (external)
   - For each character: Set starting position, call CLOTHES_SETTING
   - Set wake time, sleep state, day 1, starting money
   - Begin SHOP

**External Calls**:
- DEFAULT_OPTION (line 10) → F367
- CUSTOM_CHARAMAKE(MASTER) (line 69) → F368
- REVERSEMODE_1 (line 86) → F368
- CHARA_CUSTUM (line 122) → F368
- VIRGIN_CUSTOM (line 127) → F368
- SHORT_CUT_MODE (line 130) → F367
- CLOTHES_SETTING(LOCAL,1) (line 137) → F369
- NTR_SET_STAYOUT_MAXIMUM(LOCAL, 3) (line 115) → F371

### @EVENTLOAD Handler (1 line)

**Purpose**: Execute on save file load

**Flow**:
- Line 149: Call VERSION_UP (external)

**External Calls**:
- VERSION_UP (line 149) → F367

### @QUICK_START_SETUP Handler (87 lines)

**Purpose**: Development quick start - skip all interactive prompts

**Flow**:
1. **Character Addition** (lines 157-169):
   - Add all characters 1-13 with ADDCHARA

2. **Basic Setup** (lines 171-183):
   - Set SAVESTR:10 = "/"
   - For each character: Set position, call CLOTHES_SETTING
   - Set wake time, sleep state, day 1, starting money

3. **NTR Patch Initialization** (lines 186-240):
   - Skip NTR option prompt (set bit 0 on FLAG:ＮＴＲパッチ設定)
   - Stage 0-1: Skip children/position reset (N/A for new game)
   - Stage 2 (lines 192-197): Random menstrual cycle
   - Stage 3 (lines 199-203): Uterus volume (TRYCALL 子宮内体積設定)
   - Stage 4 (lines 205-212): Set relationship to 100
   - Stage 5-6 (lines 215-218): Calendar defaults (暦法月=1, 暦法日=1)
   - Stage 7 (lines 220-224): Pajama defaults
   - Stage 8 (lines 226-228): Temperature tolerance (TRYCALL 気温耐性取得)
   - Stage 9 (lines 230-234): Body details (TRYCALL 体詳細初期設定)
   - Stage 10 (lines 236-237): Skip CLOTHES_EX (already set)
   - Stages 10-16: Will run automatically in SHOW_SHOP

**External Calls**:
- CLOTHES_SETTING(LOCAL,1) (line 176) → F369
- 子宮内体積設定(LOCAL) (line 201) → F370
- 気温耐性取得 (line 227) → F370
- 体詳細初期設定(LOCAL) (line 232) → F370

### Migration Feasibility

**@EVENTFIRST**:
- **Feasible**: Mode flag operations (bits 1-4 on FLAG:ゲームモード, bits 1,3-8,24-26,31-32 on FLAG:ＮＴＲパッチ設定)
- **Delegated**: All external CALLs to F367-F371
- **Strategy**: Create stub methods in C# that will be implemented by successor features

**@EVENTLOAD**:
- **Delegated**: VERSION_UP to F367
- **Strategy**: Simple stub method

**@QUICK_START_SETUP**:
- **Feasible**: NTR patch stage progression logic, menstrual cycle randomization, relationship initialization
- **Delegated**: CLOTHES_SETTING, 子宮内体積設定, 気温耐性取得, 体詳細初期設定 to F369-F370
- **Strategy**: Implement stage control flow, stub external calls

### Summary

Total external dependencies: 12 unique methods
- F367 (Options & Utilities): DEFAULT_OPTION, SHORT_CUT_MODE, VERSION_UP
- F368 (Character Setup): CUSTOM_CHARAMAKE, REVERSEMODE_1, CHARA_CUSTUM, VIRGIN_CUSTOM
- F369 (Clothing System): CLOTHES_SETTING
- F370 (Body & State): 子宮内体積設定, 気温耐性取得, 体詳細初期設定
- F371 (NTR Initialization): NTR_SET_STAYOUT_MAXIMUM

Migration to C# will preserve ERB-style initialization flow while enabling type-safe flag operations and preparing for Phase 4+ feature integration.

---

## Migration Pattern

### Phase 4+ Feature Integration Guide

This section documents how future features (F367-F371 and beyond) should integrate with the GameInitialization framework established in Phase 3.

#### Pattern Overview

The GameInitialization class provides a **stub-based integration pattern** where:
1. Phase 3 (F365) defines interfaces for all external calls as stub methods
2. Phase 4+ features implement these stubs with actual functionality
3. Tests verify both isolated functionality and integration

#### Integration Steps for Future Features

**Step 1: Identify Your Stub Interface**

F365 created stub methods for all external SYSTEM.ERB calls. Find your stub in `Era.Core/Common/GameInitialization.cs`:

```csharp
// Example: F367 implements this stub
public static void DefaultOption()
{
    // Stub - will be implemented in F367
}
```

**Step 2: Create Your Implementation Class**

Create a new class in `Era.Core/Common/` following the naming pattern:
- F367 (Options & Utilities) → `GameOptions.cs`
- F368 (Character Setup) → `CharacterSetup.cs`
- F369 (Clothing System) → `ClothingSystem.cs`
- F370 (Body & State) → `BodyAndStateSystem.cs`
- F371 (NTR Initialization) → `NTRInitialization.cs`

**Step 3: Implement Your Methods**

```csharp
namespace Era.Core.Common
{
    /// <summary>
    /// Game options and utilities (F367).
    /// Implements DEFAULT_OPTION, SHORT_CUT_MODE, VERSION_UP from SYSTEM.ERB.
    /// </summary>
    public static class GameOptions
    {
        /// <summary>
        /// Initialize default game options.
        /// Source: ERB/SYSTEM_OPTION.ERB @DEFAULT_OPTION
        /// </summary>
        public static void DefaultOption()
        {
            // Implementation here
        }
    }
}
```

**Step 4: Update GameInitialization Stub to Call Your Implementation**

Replace the stub with a call to your implementation:

```csharp
// Before (F365 stub):
public static void DefaultOption()
{
    // Stub - will be implemented in F367
}

// After (F367 implementation):
public static void DefaultOption()
{
    GameOptions.DefaultOption();
}
```

**Step 5: Create MSTest Tests**

Follow the F359/F364 MSTest pattern:

```csharp
using Xunit;
using Era.Core.Common;

namespace MinorShift.Emuera.Tests
{
    public class GameOptionsTests
    {
        [Fact]
        public void DefaultOption_SetsExpectedValues()
        {
            // Arrange
            // Act
            GameOptions.DefaultOption();
            // Assert
            // Verify expected state changes
        }
    }
}
```

**Step 6: Verify Tests Pass**

```bash
dotnet test engine.Tests/uEmuera.Tests.csproj --filter "FullyQualifiedName~GameOptionsTests"
```

#### Hook Points for Initialization Events

Features can hook into three initialization lifecycle events:

| Event | Hook Point | Use Case | Example Features |
|-------|------------|----------|------------------|
| **EventFirst** | New game initialization | Character creation, initial state setup | F368 (character customization), F369 (initial clothing) |
| **EventLoad** | Save file load | Version migration, state validation | F367 (VERSION_UP) |
| **QuickStartSetup** | Development quick start | Skip interactive prompts, set defaults | F370 (body initialization), F371 (NTR defaults) |

#### Example Integration: F367 (Options & Utilities)

**Stub Methods to Implement**:
1. `DefaultOption()` - Initialize default game options
2. `ShortCutMode()` - Configure shortcut mode
3. `VersionUp()` - Execute version upgrade migration

**Migration Steps**:
1. Analyze `ERB/SYSTEM_OPTION.ERB` for `@DEFAULT_OPTION`, `@SHORT_CUT_MODE`, `@VERSION_UP`
2. Create `Era.Core/Common/GameOptions.cs`
3. Implement the three methods
4. Update `GameInitialization.cs` stubs to call `GameOptions` methods
5. Create `GameOptionsTests.cs` with MSTest cases
6. Verify build and tests pass
7. Mark F367 complete

#### Example Integration: F368 (Character Setup)

**Stub Methods to Implement**:
1. `CustomCharaMake(int characterId)` - Custom player character creation
2. `CharaCustum()` - Character customization interface
3. `VirginCustom()` - Virgin mode customization
4. `ReverseMode1()` - Reverse mode (player becomes NPC)

**Migration Steps**:
1. Analyze `ERB/キャラメイク.ERB` for character creation handlers
2. Create `Era.Core/Common/CharacterSetup.cs`
3. Implement character creation methods
4. Update `GameInitialization.cs` stubs to call `CharacterSetup` methods
5. Create `CharacterSetupTests.cs` with MSTest cases
6. Verify build and tests pass
7. Mark F368 complete

#### Testing Strategy

**Unit Tests** (Test your implementation class):
```csharp
// Test individual methods in isolation
[Fact]
public void GameOptions_DefaultOption_SetsCorrectFlags()
{
    // Test GameOptions.DefaultOption() directly
}
```

**Integration Tests** (Test GameInitialization hooks):
```csharp
// Test that GameInitialization correctly calls your implementation
[Fact]
public void GameInitialization_EventFirst_CallsDefaultOption()
{
    // Verify GameInitialization.EventFirst() invokes GameOptions.DefaultOption()
}
```

**Headless Compatibility** (Test without ERA engine):
```csharp
// Verify methods work in headless mode (CI/CD requirement)
[Fact]
public void HeadlessMode_DefaultOption_NoEngineRequired()
{
    var exception = Record.Exception(() => GameOptions.DefaultOption());
    Assert.Null(exception);
}
```

#### Migration Checklist

Use this checklist when implementing Phase 4+ features:

- [ ] Identify stub methods in `GameInitialization.cs`
- [ ] Analyze corresponding ERB source files
- [ ] Create implementation class in `Era.Core/Common/`
- [ ] Implement methods with XML documentation
- [ ] Update `GameInitialization.cs` stubs to call implementation
- [ ] Create MSTest test class in `engine.Tests/Tests/`
- [ ] Verify `dotnet build` succeeds
- [ ] Verify `dotnet test` passes
- [ ] Document any new external dependencies
- [ ] Update feature-{ID}.md with completion status

#### Key Design Principles

1. **Separation of Concerns**: GameInitialization orchestrates, implementation classes contain logic
2. **Testability**: All methods must be testable in headless mode (no Unity/ERA engine dependency)
3. **Progressive Migration**: Each feature implements its own stubs, doesn't block others
4. **Documentation**: Every public method has XML comments linking to ERB source
5. **Type Safety**: Use C# types instead of ERB dynamic variables where possible

#### Dependency Chain

```
F365 (SYSTEM.ERB handlers + stubs)
  ├── F367 (Options & Utilities)
  ├── F368 (Character Setup)
  ├── F369 (Clothing System)
  ├── F370 (Body & State Systems)
  └── F371 (NTR Initialization)
      └── Phase 4 complete: Full C# initialization stack
```

Each feature can be implemented independently as long as it implements its own stubs. Features do NOT need to wait for predecessors.

#### Common Patterns

**Bit Flag Operations**:
```csharp
// Use GameInitialization helper methods
long flags = GameInitialization.SetGameModeFlag(0, 1, true);
bool isSet = GameInitialization.GetGameModeFlag(flags, 1);
```

**Character Iteration**:
```csharp
// Follow ERB pattern: FOR LOCAL, 0, CHARANUM
for (int characterId = 0; characterId < Constants.開始時人数; characterId++)
{
    // Process character
}
```

**External Call Delegation**:
```csharp
// ERB: CALL SOME_FUNCTION(ARG1, ARG2)
// C#:  GameInitialization.SomeFunction(arg1, arg2);
//      → Delegates to: YourClass.SomeFunction(arg1, arg2);
```

**TRYCALL Pattern**:
```csharp
// ERB: TRYCALL OPTIONAL_FUNCTION(ARG)
// C#:  Try-catch or null-check pattern
try
{
    YourClass.OptionalFunction(arg);
}
catch
{
    // Silently ignore if not implemented yet
}
```

#### Future Enhancements

Once Phase 4 completes (F367-F371 implemented):
1. Full C# initialization stack will be functional
2. ERB SYSTEM.ERB can be deprecated (keep as reference)
3. Phase 5+ can add new initialization hooks without modifying ERB
4. Test coverage for initialization will be complete

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as Phase 3 CRITICAL main loop migration | PROPOSED |
| 2026-01-06 12:42 | START | implementer | Task 1-7 implementation | - |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 1: SYSTEM.ERB analysis documented | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 2: GameInitialization.cs created | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 3: C# build verified | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 4: Test files verified | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 5: GameInitialization tests pass (18/18) | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 6: HeadlessIntegration tests pass (19/19) | SUCCESS |
| 2026-01-06 12:42 | COMPLETE | implementer | Task 7: Migration pattern documented | SUCCESS |
| 2026-01-06 12:42 | END | implementer | All tasks complete, all ACs verified | SUCCESS |
