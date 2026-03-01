# Feature 461: Phase 9 System Integration

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

## Type: engine

## Created: 2026-01-11

---

## Summary

Resolve Phase 9 deferred system integration items (GlobalStatic accessor migration, System Commands engine integration, additional system commands).

**Scope** (from architecture.md Phase 12):
1. GlobalStatic accessor migration (3 TODOs in GameInitialization.cs) → **Defer to Phase 21**
2. System Commands engine integration (CharacterManager/StyleManager/GameState stubs → engine implementation via DI)
3. Additional system commands (SETCOLORBYNAME, BEGIN, DRAWLINE) → **OUT OF SCOPE**

**Architecture Note**: Era.Core cannot reference GlobalStatic (engine layer). Integration uses DI:
- Era.Core: Defines interfaces (ICharacterManager, IStyleManager, IGameState) - already done
- Era.Core: Stub implementations remain as fallback
- engine: Creates implementations that call GlobalStatic, registered via DI

**Output**:
- `Era.Core/Common/GameInitialization.cs` - TODOs tracked in 残課題 (Phase 21)
- `engine/Assets/Scripts/Emuera/Services/CharacterManagerImpl.cs` - Engine implementation (NEW)
- `engine/Assets/Scripts/Emuera/Services/StyleManagerImpl.cs` - Engine implementation (NEW)
- `engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` - Engine implementation (NEW)
- (Optional) Additional system command implementations

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9 System Integration** - Phase 12 COM implementation requires engine-side service implementations (CharacterManager, StyleManager, GameState) to be injected via DI pattern, enabling Era.Core commands to call GlobalStatic APIs indirectly. This feature establishes the DI bridge infrastructure that Phase 12 COM commands will depend on.

### Problem (Current Issue)

Phase 9 (F434) deferred system integration items:
- GameInitialization.cs has 3 TODO comments for GlobalStatic accessor migration
- CharacterManager/StyleManager/GameState return stub failures (need engine delegation)
- Additional system commands (SETCOLORBYNAME, BEGIN, DRAWLINE) deferred to Phase 12

**Rationale**: Creating GlobalStatic accessors requires defining complete state accessor pattern (Phase 21 State Systems scope), but Phase 12 COM implementation enables partial integration via engine delegation.

**Note**: Full state accessor pattern is Phase 21 scope. This feature implements minimal integration for Phase 12 COM requirements.

### Goal (What to Achieve)

1. **Defer GameInitialization TODOs** to Phase 21 (track in 残課題)
2. **Implement System Commands engine integration** via DI (create engine implementations calling GlobalStatic)
3. **Additional system commands** - OUT OF SCOPE (tracked in architecture.md)
4. **Verify tests pass** after integration (regression gate)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GameInitialization TODOs tracked in 残課題 | code | Grep | contains | "GameInitialization.cs L319" in feature-461.md | [x] |
| 2 | CharacterManagerImpl created in engine | file | Bash | succeeds | dir engine\Assets\Scripts\Emuera\Services\CharacterManagerImpl.cs | [x] |
| 3 | StyleManagerImpl created in engine | file | Bash | succeeds | dir engine\Assets\Scripts\Emuera\Services\StyleManagerImpl.cs | [x] |
| 4 | GameStateImpl created in engine | file | Bash | succeeds | dir engine\Assets\Scripts\Emuera\Services\GameStateImpl.cs | [x] |
| 5 | Engine implementations use GlobalStatic | code | Grep | contains | "GlobalStatic" in engine\Assets\Scripts\Emuera\Services\ | [x] |
| 6 | GlobalStatic properties added for DI | code | Grep | contains | "ICharacterManager" in GlobalStatic.cs | [x] |
| 7 | Existing SystemCommandTests still pass | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~SystemCommandTests | [x] |
| 8 | Era.Core.Tests isolation (Neg) | code | Grep | not_contains | "uEmuera" in Era.Core.Tests\Era.Core.Tests.csproj | [x] |
| 9 | Era.Core stubs remain as fallback (Neg) | code | Grep | contains | "Result<Unit>.Fail" in Era.Core\Commands\System\CharacterManager.cs | [x] |

### AC Details

**AC#1**: GameInitialization TODOs tracked in 残課題
- Test: Grep pattern="GameInitialization.cs L319" path="Game/agents/feature-461.md"
- Verifies 残課題 section lists the 3 TODOs (already pre-populated at L257-259)
- Note: This AC is already satisfied - 残課題 section was populated during FL review

**AC#2**: CharacterManagerImpl created in engine
- Test: Bash `dir engine\Assets\Scripts\Emuera\Services\CharacterManagerImpl.cs`
- Note: Uses backslash path for Windows compatibility

**AC#3**: StyleManagerImpl created in engine
- Test: Bash `dir engine\Assets\Scripts\Emuera\Services\StyleManagerImpl.cs`
- Note: Uses backslash path for Windows compatibility

**AC#4**: GameStateImpl created in engine
- Test: Bash `dir engine\Assets\Scripts\Emuera\Services\GameStateImpl.cs`
- Note: Uses backslash path for Windows compatibility

**AC#5**: Engine implementations use GlobalStatic
- Test: Grep pattern="GlobalStatic" path="engine\Assets\Scripts\Emuera\Services\"
- Verifies all 3 engine implementations call GlobalStatic APIs

**AC#6**: GlobalStatic properties added for DI
- Test: Grep pattern="ICharacterManager" path="engine\Assets\Scripts\Emuera\GlobalStatic.cs"
- Verifies GlobalStatic has DI properties for all 3 interfaces (ICharacterManager, IStyleManager, IGameState)
- Note: Era.Core.Tests cannot reference engine, so tests use Era.Core stubs; runtime uses engine implementations via GlobalStatic

**AC#7**: Existing SystemCommandTests still pass
- Test: Bash `dotnet test --filter FullyQualifiedName~SystemCommandTests`
- Note: Existing tests continue to use Era.Core stubs, which return Failure. Tests verify stub wiring, not actual engine behavior.
- **Purpose**: Regression gate only - confirms DI changes don't break existing test infrastructure
- **Out of scope**: Engine implementation correctness verified in F462 (Phase 12 Post-Phase Review) via headless mode

**AC#8**: Era.Core.Tests isolation (Negative)
- Test: Grep pattern="uEmuera" path="Era.Core.Tests/Era.Core.Tests.csproj" with not_contains
- Verifies Era.Core.Tests does NOT reference engine project (architecture constraint)

**AC#9**: Era.Core stubs remain as fallback (Negative)
- Test: Grep pattern="Result<Unit>.Fail" path="Era.Core/Commands/System/CharacterManager.cs"
- Verifies Era.Core stub implementations are preserved (not deleted/modified)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Track GameInitialization TODOs in 残課題 section | [x] |
| 2 | 2 | Create CharacterManagerImpl in engine project | [x] |
| 3 | 3 | Create StyleManagerImpl in engine project | [x] |
| 4 | 4 | Create GameStateImpl in engine project | [x] |
| 5 | 5 | Verify engine implementations use GlobalStatic | [x] |
| 6 | 6 | Add GlobalStatic DI properties for I*Manager interfaces | [x] |
| 7 | 7 | Verify existing SystemCommandTests still pass | [x] |
| 8 | 8 | Verify Era.Core.Tests isolation (negative) | [x] |
| 9 | 9 | Verify Era.Core stubs remain as fallback (negative) | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 9 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### 1. GameInitialization TODOs (Task 1 - defer to Phase 21)

| File | Line | Current Status | Action |
|------|:----:|----------------|--------|
| `Era.Core\Common\GameInitialization.cs` | 319 | TODO: Replace with GlobalStatic | Keep TODO, track in 残課題 |
| `Era.Core\Common\GameInitialization.cs` | 339 | TODO: Replace with GlobalStatic | Keep TODO, track in 残課題 |
| `Era.Core\Common\GameInitialization.cs` | 358 | TODO: Replace with GlobalStatic | Keep TODO, track in 残課題 |

**Rationale**: Era.Core cannot reference GlobalStatic (architecture constraint). Full accessor pattern requires Phase 21 State Systems design. Current stub lambdas returning defaults are acceptable for Phase 12.

**Action**: Update 残課題 section with all 3 TODOs, referencing Phase 21.

### 2. System Commands Engine Integration (Tasks 2-5 - from F434)

**Architecture**: Era.Core cannot call GlobalStatic. Create engine-side implementations via DI.

| Service | Era.Core Interface | Engine Implementation | Location |
|---------|-------------------|----------------------|----------|
| CharacterManager | ICharacterManager | CharacterManagerImpl | engine/Assets/Scripts/Emuera/Services/ |
| StyleManager | IStyleManager | StyleManagerImpl | engine/Assets/Scripts/Emuera/Services/ |
| GameState | IGameState | GameStateImpl | engine/Assets/Scripts/Emuera/Services/ |

**GlobalStatic APIs to use**:
- `GlobalStatic.Process` - game process control (quit, restart)
- `GlobalStatic.Console` - style management (color, font, alignment)
- `GlobalStatic.VEvaluator` - character/save management

**Type Conversion**: Engine APIs use Int64 for character identifiers. Implementations must convert:
- `CharacterId` → `Int64` via `.Value` property (follows Phase 4 Strongly Typed ID pattern)
- Example: `GlobalStatic.VEvaluator.AddCharacter(id.Value)` where `id.Value` is Int64
- `AlignmentType` → `DisplayLineAlignment` enum mapping (Left→LEFT, Center→CENTER, Right→RIGHT)

**API Limitations**:
- `IGameState.Restart()`: No engine API exists. Implement as `Result<Unit>.Fail("Restart not supported via API")`

**Pattern**:
```csharp
// engine/Assets/Scripts/Emuera/Services/CharacterManagerImpl.cs
using Era.Core.Interfaces;
using Era.Core.Types;
using MinorShift.Emuera;

public class CharacterManagerImpl : ICharacterManager
{
    public Result<Unit> AddChara(CharacterId id)
    {
        GlobalStatic.VEvaluator.AddCharacter(id.Value);
        return Result<Unit>.Ok(Unit.Value);
    }
    // ... other methods
}
```

**DI Registration** (via GlobalStatic static properties, following existing pattern):
```csharp
// In GlobalStatic.cs - add properties like existing IVariableEvaluator pattern (lines 159-164)
private static ICharacterManager _characterManager;
public static ICharacterManager CharacterManagerInstance
{
    get => _characterManager ?? new CharacterManager(); // Fallback to Era.Core stub
    set => _characterManager = value;
}

// Register in engine startup (after GlobalStatic.Reset() completes):
// Location: engine/Assets/Scripts/Emuera/Sub/Process.cs initialization or
//           engine/Assets/Scripts/Emuera/Headless/Main.cs for headless mode
GlobalStatic.CharacterManagerInstance = new CharacterManagerImpl();
GlobalStatic.StyleManagerInstance = new StyleManagerImpl();
GlobalStatic.GameStateInstance = new GameStateImpl();
```

**Registration Timing**: Must register after `GlobalStatic.Reset()` (line 190-222 in GlobalStatic.cs) but before first usage. Follows existing pattern for `_fileSystem` registration (line 204).

### 3. Additional System Commands (from F434 scope reduction)

**Status**: OUT OF SCOPE for F461. Deferred to follow-up feature.

| Command | Category | Description | Priority | Decision |
|---------|----------|-------------|:--------:|:--------:|
| SETCOLORBYNAME | Style | Set color by name lookup | Low | Defer |
| BEGIN | System | Begin block command | Low | Defer |
| DRAWLINE | System | Draw horizontal line | Low | Defer |

**Rationale**: F461 focus is DI bridge infrastructure. Additional commands are separate scope and tracked in architecture.md Phase 12 residual items.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F434 | System Commands foundation |
| Predecessor | F460 | Phase 7 Technical Debt Resolution |
| Successor | F462 | Phase 12 Post-Phase Review |

---

## Links

- [feature-434.md](feature-434.md) - System Commands foundation (Task 8.5 deferred)
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-460.md](feature-460.md) - Phase 7 Technical Debt Resolution
- [feature-462.md](feature-462.md) - Phase 12 Post-Phase Review
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 technical debt section

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - Implementation Contract already notes "Actual engine APIs must be verified during implementation"
- **2026-01-12 FL iter3**: [resolved] Architecture violation fixed - Era.Core cannot call GlobalStatic; redesigned to use DI with engine-side implementations
- **2026-01-12 FL iter4**: [resolved] Phase2-Validate - DI registration uses GlobalStatic static property pattern (hybrid with MS DI)
- **2026-01-12 FL iter4**: [resolved] Phase2-Validate - Test filter uses FullyQualifiedName~SystemCommandTests
- **2026-01-12 FL iter5**: [resolved] Architecture refined - Era.Core.Tests cannot reference engine; tests use Era.Core stubs; runtime uses engine implementations via GlobalStatic
- **2026-01-12 FL iter5**: [resolved] Phase2-Validate - Test filter verified working via feasibility check
- **2026-01-12 FL iter5**: [resolved] Phase2-Validate - GlobalStatic API signatures documented with type conversion
- **2026-01-12 FL iter6**: [resolved] Phase5-Feasibility - IGameState.Restart() API missing. Documented in Implementation Contract as "Restart not supported via API"

---

## 残課題

| Item | Description | Location | Target Phase |
|------|-------------|----------|:------------:|
| BodyDetailInit GlobalStatic accessor | getCflag/setCflag/getTalent accessor pattern | GameInitialization.cs L319 | Phase 21 |
| UterusVolumeInit GlobalStatic accessor | getCflag/setCflag/getTalent accessor pattern | GameInitialization.cs L339 | Phase 21 |
| TemperatureToleranceInit GlobalStatic accessor | getCflag/setCflag accessor pattern | GameInitialization.cs L358 | Phase 21 |

**Note**: These TODOs remain in code with stub lambdas returning defaults. Full accessor pattern requires Phase 21 State Systems design.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 17:43 | START | implementer | Task 2-9 | - |
| 2026-01-12 17:47 | END | implementer | Task 2-9 | SUCCESS |
| 2026-01-12 17:55 | START | ac-tester | All 9 ACs | - |
| 2026-01-12 17:55 | END | ac-tester | All 9 ACs | PASS |
| 2026-01-12 17:56 | START | feature-reviewer | Mode: post | - |
| 2026-01-12 17:56 | DEVIATION | feature-reviewer | Reset() missing DI resets | NEEDS_REVISION |
| 2026-01-12 17:57 | END | opus | Fixed GlobalStatic.Reset() | SUCCESS |
| 2026-01-12 18:01 | START | feature-reviewer | Mode: post (re-run) | - |
| 2026-01-12 18:01 | END | feature-reviewer | Mode: post | READY |
| 2026-01-12 18:02 | DEVIATION | feature-reviewer | Mode: doc-check | NEEDS_REVISION |
| 2026-01-12 18:02 | END | opus | Updated engine-dev SKILL.md | SUCCESS |
| 2026-01-12 18:03 | END | feature-reviewer | Mode: doc-check (re-run) | READY |
| 2026-01-12 18:03 | END | opus | SSOT Update Check | READY |
| 2026-01-12 18:04 | COMPLETION | finalizer | All ACs [x], all Tasks [○] verified | READY_TO_COMMIT |
