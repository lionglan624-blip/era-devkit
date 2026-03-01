# Feature 628: Character Data Service for Template Variable Resolution

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

## Created: 2026-01-26

---

## Summary

Implement character data service to resolve CALLNAME and other character-specific template variables with actual character names. This replaces the placeholder "Character{Id}" format currently used in TemplateDialogueRenderer (F551) with actual character names from game data.

**Output**: Era.Core/Characters/ICharacterDataService.cs and implementation

**Scope**: Character name resolution service for template variable substitution in dialogue rendering.

---

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP continuation - Complete the dialogue rendering pipeline with actual character data resolution. TemplateDialogueRenderer (F551) provides extensible infrastructure; this feature provides the data layer integration.

### Problem (Current Issue)

F551 TemplateDialogueRenderer returns placeholder format "Character{Id}" for {CALLNAME} variable because actual character name resolution requires access to game character data which was out of scope for the interface extraction phase.

### Goal (What to Achieve)

Create ICharacterDataService interface and implementation that:
- Resolves CharacterId to character display name (CALLNAME)
- Integrates with existing game data structures (CSV character definitions)
- Can be injected into TemplateDialogueRenderer resolvers

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Complete the dialogue rendering pipeline with actual character data resolution" | Service must provide actual character names (not placeholders) | AC#5, AC#6 |
| "Can be injected into TemplateDialogueRenderer resolvers" | DI integration with proper registration pattern | AC#3, AC#4 |
| "Follows established pattern from F420 (ICharacterDataAccess)" | Interface in Era.Core, stub implementation, DI registration | AC#1, AC#2, AC#3, AC#4 |
| "CharacterId to character display name (CALLNAME)" | GetCallName method with CharacterId parameter returning Result<string> | AC#1, AC#5, AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ICharacterDataService interface exists | file | Glob(Era.Core/Characters/ICharacterDataService.cs) | exists | - | [x] |
| 2 | NullCharacterDataService stub exists | file | Glob(Era.Core/Characters/NullCharacterDataService.cs) | exists | - | [x] |
| 3 | DI registration for ICharacterDataService | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | `AddSingleton<ICharacterDataService, NullCharacterDataService>` | [x] |
| 4 | CallNameResolver uses ICharacterDataService | code | Grep(Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs) | contains | `private readonly ICharacterDataService` | [x] |
| 5 | GetCallName success test passes | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterDataServiceTests.GetCallName_ValidId_ReturnsCallName | succeeds | - | [x] |
| 6 | GetCallName failure test passes | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterDataServiceTests.GetCallName_InvalidId_ReturnsFailure | succeeds | - | [x] |
| 7 | Interface has GetCallName method signature | code | Grep(Era.Core/Characters/ICharacterDataService.cs) | contains | `Result<string> GetCallName(CharacterId characterId)` | [x] |
| 8 | Stub returns placeholder format | code | Grep(Era.Core/Characters/NullCharacterDataService.cs) | contains | `$"Character{characterId.Value}"` | [x] |
| 9 | TestCharacterDataService exists | file | Glob(Era.Core.Tests/Characters/TestCharacterDataService.cs) | exists | - | [x] |
| 10 | Zero technical debt | code | Grep(Era.Core/Characters/*.cs) | not_contains | `TODO\|FIXME\|HACK` | [x] |

### AC Details

**AC#1: ICharacterDataService interface exists**
- Verifies interface file created in Era.Core/Characters/ directory
- New directory following Era.Core/Types/ organization pattern
- Test: Glob pattern="Era.Core/Characters/ICharacterDataService.cs"

**AC#2: NullCharacterDataService stub exists**
- Stub implementation for Era.Core.Tests isolation (same pattern as NullCharacterDataAccess)
- Returns predictable placeholder values for testing
- Test: Glob pattern="Era.Core/Characters/NullCharacterDataService.cs"

**AC#3: DI registration for ICharacterDataService**
- Follows F420 pattern: interface registered with stub implementation
- Enables engine layer to override with actual implementation via GlobalStatic
- Test: Grep pattern=`AddSingleton<ICharacterDataService, NullCharacterDataService>` path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"

**AC#4: CallNameResolver uses ICharacterDataService**
- TemplateDialogueRenderer.CallNameResolver modified to accept ICharacterDataService
- Constructor injection pattern for testability
- Test: Grep pattern=`ICharacterDataService` path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs"

**AC#5: GetCallName success test passes**
- Tests successful character name resolution
- Uses test double (mock/fake) configured to return known value, not NullCharacterDataService
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterDataServiceTests.GetCallName_ValidId_ReturnsCallName

**AC#6: GetCallName failure test passes**
- Tests failure case when character not found
- Uses test double (mock/fake) configured to return Result.Fail, not NullCharacterDataService
- Returns Result.Fail with descriptive error message
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterDataServiceTests.GetCallName_InvalidId_ReturnsFailure

**AC#7: Interface has GetCallName method signature**
- Verifies exact method signature with Result<string> return type
- CharacterId parameter for type safety (not raw int)
- Test: Grep pattern=`Result<string> GetCallName(CharacterId characterId)` path="Era.Core/Characters/ICharacterDataService.cs"

**AC#8: Stub returns placeholder format**
- NullCharacterDataService returns predictable "Character{Id}" format
- Same format as current CallNameResolver for backward compatibility during transition
- Test: Grep pattern=`$"Character{characterId.Value}"` path="Era.Core/Characters/NullCharacterDataService.cs"

**AC#9: TestCharacterDataService exists**
- Test double file exists for AC#5 and AC#6 test execution
- Enables configurable mock behavior for CharacterDataServiceTests
- Test: Glob pattern="Era.Core.Tests/Characters/TestCharacterDataService.cs"

**AC#10: Zero technical debt**
- No TODO/FIXME/HACK markers in feature files
- Clean implementation ready for production
- Test: Grep pattern=`TODO|FIXME|HACK` paths=[Era.Core/Characters/ICharacterDataService.cs, Era.Core/Characters/NullCharacterDataService.cs]
- Expected: 0 matches

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Follow the F420 ICharacterDataAccess pattern: Interface in Era.Core, stub implementation for testing isolation, DI registration for dependency injection. This enables engine-layer implementation to provide actual character data via GlobalStatic override while maintaining Era.Core testability.

**Key architectural pattern**: Era.Core defines interface contract, NullCharacterDataService provides predictable stub for tests, engine layer will provide actual implementation reading from CharacterData.dataString arrays.

This approach satisfies all ACs by:
1. Creating interface and stub in Era.Core/Characters/ (AC#1, AC#2)
2. Registering stub via DI (AC#3)
3. Injecting service into CallNameResolver (AC#4)
4. Providing GetCallName method signature (AC#7)
5. Implementing stub behavior (AC#8)
6. Enabling unit tests with predictable stub values (AC#5, AC#6)
7. Maintaining zero technical debt (AC#9)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Characters/ICharacterDataService.cs` interface file in new Characters directory |
| 2 | Create `Era.Core/Characters/NullCharacterDataService.cs` stub implementation returning predictable placeholder values |
| 3 | Add `services.AddSingleton<ICharacterDataService, NullCharacterDataService>()` to ServiceCollectionExtensions.cs at line ~182 (after ICharacterDataAccess registration) |
| 4 | Modify CallNameResolver in TemplateDialogueRenderer.cs to accept ICharacterDataService via constructor, call service.GetCallName(characterId) instead of returning hardcoded placeholder |
| 5 | Create `Era.Core.Tests/Characters/CharacterDataServiceTests.cs` with test using mock/fake implementation configured to return "Reimu" for CharacterId(0), verify GetCallName returns Result.Ok("Reimu") |
| 6 | Add test in CharacterDataServiceTests.cs using mock/fake implementation configured to return Result.Fail for invalid CharacterId, verify GetCallName returns Result.Fail with message "Character {id} not found" |
| 7 | Define interface method `Result<string> GetCallName(CharacterId characterId)` in ICharacterDataService.cs |
| 8 | Implement NullCharacterDataService.GetCallName returning `Result<string>.Ok($"Character{characterId.Value}")` for backward compatibility during transition |
| 9 | Create `Era.Core.Tests/Characters/TestCharacterDataService.cs` test double file for AC#5 and AC#6 test execution |
| 10 | Verify with Grep that no TODO/FIXME/HACK markers exist in ICharacterDataService.cs or NullCharacterDataService.cs after implementation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Service interface location | Era.Core/Functions (with F420) vs Era.Core/Characters (new dir) | Era.Core/Characters | Separates runtime character data service from CSV template access; follows SRP (ICharacterDataAccess serves function context, ICharacterDataService serves dialogue rendering context) |
| Stub return value format | "Unknown" vs "Character{Id}" vs Fail | "Character{Id}" | Matches current CallNameResolver behavior for backward compatibility; allows gradual migration without breaking existing tests |
| Method signature error handling | Exception vs Result<string> | Result<string> | Consistent with Era.Core Result pattern; enables caller to handle missing character gracefully |
| Constructor injection vs property | Constructor injection vs public property | Constructor injection | Standard DI pattern; ensures service availability at resolver construction time; follows F420 precedent |
| NullCharacterDataService scope | Public vs Internal | Internal sealed | Same as NullCharacterDataAccess (F420); stub is for Era.Core.Tests only, not public API |

### Interfaces / Data Structures

**ICharacterDataService.cs**:
```csharp
using Era.Core.Types;

namespace Era.Core.Characters;

/// <summary>
/// Character data service interface for runtime character data access.
/// Provides access to character display names and other runtime character properties.
/// Feature 628 - Character Data Service for Template Variable Resolution
/// </summary>
public interface ICharacterDataService
{
    /// <summary>
    /// Get character display name (CALLNAME).
    /// Returns the current runtime CALLNAME value which may differ from CSV template default.
    /// </summary>
    /// <param name="characterId">Character ID</param>
    /// <returns>Result containing character display name, or Fail if character not found</returns>
    Result<string> GetCallName(CharacterId characterId);
}
```

**NullCharacterDataService.cs**:
```csharp
using Era.Core.Types;

namespace Era.Core.Characters;

/// <summary>
/// Stub implementation of ICharacterDataService that returns placeholder format.
/// Feature 628 - Temporary implementation for DI resolution and Era.Core.Tests isolation.
/// Real implementation in engine layer will read from CharacterData.dataString[VariableCode.CALLNAME].
/// </summary>
internal sealed class NullCharacterDataService : ICharacterDataService
{
    public Result<string> GetCallName(CharacterId characterId)
    {
        // Return placeholder format matching current CallNameResolver behavior
        return Result<string>.Ok($"Character{characterId.Value}");
    }
}
```

**TemplateDialogueRenderer modification**:
```csharp
public class TemplateDialogueRenderer : IDialogueRenderer
{
    private readonly ICharacterDataService _characterDataService;

    // New constructor for DI injection
    public TemplateDialogueRenderer(ICharacterDataService characterDataService)
    {
        _characterDataService = characterDataService ?? throw new ArgumentNullException(nameof(characterDataService));

        RegisterResolver("CALLNAME", new CallNameResolver(_characterDataService));
        // ... other resolvers
    }

    // Backward compatibility constructor - TRACKED FOR REMOVAL
    // DEBT TRACKING: Remove this constructor via F553 Task "Remove TemplateDialogueRenderer parameterless constructor"
    public TemplateDialogueRenderer() : this(new NullCharacterDataService())
    {
    }
}
```

**Migration Path**: Current TemplateDialogueRenderer has no direct instantiations in codebase. The parameterless constructor delegates to NullCharacterDataService for backward compatibility. F553 (KojoEngine Facade) MUST register TemplateDialogueRenderer via DI container to use the new ICharacterDataService constructor: `services.AddScoped<IDialogueRenderer>(sp => new TemplateDialogueRenderer(sp.GetRequiredService<ICharacterDataService>()))`. The backward compatibility constructor provides fallback for any remaining direct instantiations.

**CallNameResolver modification**:
```csharp
internal class CallNameResolver : ITemplateVariableResolver
{
    private readonly ICharacterDataService _characterDataService;

    public CallNameResolver(ICharacterDataService characterDataService)
    {
        _characterDataService = characterDataService ?? throw new ArgumentNullException(nameof(characterDataService));
    }

    public Result<string> ResolveVariable(IEvaluationContext context, string parameter)
    {
        var characterId = context.CurrentCharacter;
        if (characterId == null)
            return Result<string>.Fail("No current character available");

        return _characterDataService.GetCallName(characterId.Value);
    }
}
```

**DI Registration** (ServiceCollectionExtensions.cs line ~182):
```csharp
// Character Data Service - Feature 628
services.AddSingleton<ICharacterDataService, NullCharacterDataService>();
```

**Note**: TemplateDialogueRenderer DI registration deferred to F553 (KojoEngine Facade) when actual consumer integration occurs.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,7 | Create ICharacterDataService interface with GetCallName method | [x] |
| 2 | 2,8 | Create NullCharacterDataService stub implementation | [x] |
| 3 | 3 | Register ICharacterDataService in DI container | [x] |
| 4 | 4 | Integrate ICharacterDataService into TemplateDialogueRenderer and CallNameResolver with backward compatibility | [x] |
| 5 | 5,6,9 | Create CharacterDataServiceTests with success and failure test cases, including TestCharacterDataService test double | [x] |
| 6 | 10 | Verify zero technical debt in feature files | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-4 | Interface, stub, DI registration, CallNameResolver integration |
| 2 | implementer | sonnet | Task 5 | Unit tests |
| 3 | ac-tester | haiku | Task 6 | Verification of zero technical debt |

**Constraints** (from Technical Design):
1. Era.Core cannot reference engine layer - use DI bridge pattern
2. Follow F420 ICharacterDataAccess pattern for interface + stub + DI
3. NullCharacterDataService must be internal sealed (testing isolation)
4. CallNameResolver constructor injection pattern for testability

**Pre-conditions**:
- F551 TemplateDialogueRenderer exists with CallNameResolver
- Era.Core/DependencyInjection/ServiceCollectionExtensions.cs exists
- Era.Core.Tests project can reference Era.Core

**Success Criteria**:
- All 10 ACs pass verification
- `dotnet build` succeeds with zero errors
- `dotnet test Era.Core.Tests` passes all CharacterDataServiceTests
- No TODO/FIXME/HACK markers in ICharacterDataService.cs or NullCharacterDataService.cs

### File Structure

| File Path | Purpose |
|-----------|---------|
| Era.Core/Characters/ICharacterDataService.cs | Service interface with GetCallName(CharacterId) method |
| Era.Core/Characters/NullCharacterDataService.cs | Stub implementation returning "Character{Id}" placeholder |
| Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs | Update CallNameResolver to use ICharacterDataService |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | Register ICharacterDataService with stub implementation |
| Era.Core.Tests/Characters/CharacterDataServiceTests.cs | Unit tests for GetCallName success and failure paths |
| Era.Core.Tests/Characters/TestCharacterDataService.cs | Test double with configurable behavior for AC#5 and AC#6 tests |

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` at line ~182 (after ICharacterDataAccess registration):

```csharp
// Character Data Service - Feature 628
services.AddSingleton<ICharacterDataService, NullCharacterDataService>();
```

### Test Requirements

**Test Naming Convention**: Test methods follow `{MethodName}_{Scenario}_{ExpectedResult}` format:
- `GetCallName_ValidId_ReturnsCallName`
- `GetCallName_InvalidId_ReturnsFailure`

This ensures AC filter patterns `--filter FullyQualifiedName~CharacterDataServiceTests.GetCallName_ValidId_ReturnsCallName` match correctly.

**Test Double Approach**:
Tests use `TestCharacterDataService` class implementing ICharacterDataService with configurable behavior (NOT the DI-registered NullCharacterDataService).

```csharp
internal class TestCharacterDataService : ICharacterDataService
{
    private readonly Dictionary<CharacterId, string> _names = new();
    private readonly CharacterId? _failureId;

    public TestCharacterDataService(CharacterId? failureId = null)
    {
        _failureId = failureId;
    }

    public void ConfigureCallName(CharacterId characterId, string name)
    {
        _names[characterId] = name;
    }

    public Result<string> GetCallName(CharacterId characterId)
    {
        if (_failureId.HasValue && characterId.Equals(_failureId.Value))
            return Result<string>.Fail($"Character {characterId.Value} not found");

        return _names.TryGetValue(characterId, out var name)
            ? Result<string>.Ok(name)
            : Result<string>.Fail($"Character {characterId.Value} not found");
    }
}
```

**Test Structure**:
1. Success test: Use TestCharacterDataService.ConfigureCallName(CharacterId(0), "Reimu"), verify GetCallName returns Result.Ok("Reimu")
2. Failure test: Use TestCharacterDataService(failureId: CharacterId(999)), verify GetCallName returns Result.Fail with message "Character 999 not found"

### Error Message Format

**GetCallName failure**: `"Character {characterId.Value} not found"` (e.g., "Character 999 not found")

### Implementation Notes

**CallNameResolver Integration**:
- Add ICharacterDataService parameter to CallNameResolver constructor
- Replace hardcoded placeholder logic with `_characterDataService.GetCallName(characterId)`
- Preserve existing null check for CurrentCharacter before calling service

**CharacterId Usage**:
- Use CharacterId.Value property to access underlying integer ID
- Stub format: `$"Character{characterId.Value}"` matches current behavior for backward compatibility

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: F551 TemplateDialogueRenderer returns placeholder "Character{Id}" instead of actual character names
2. Why: CallNameResolver in TemplateDialogueRenderer has no access to character data storage
3. Why: Character names are stored in engine-layer CharacterData class, not accessible from Era.Core
4. Why: Era.Core cannot directly reference engine layer (GlobalStatic, CharacterData) due to architectural separation
5. Why: No service abstraction exists to bridge Era.Core template rendering with engine-layer character data

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| CALLNAME displays as "Character123" placeholder | Missing service abstraction layer between Era.Core dialogue rendering and engine-layer character data storage |

### Conclusion

The root cause is architectural: Era.Core's CallNameResolver cannot access character names because they reside in engine-layer CharacterData.dataString arrays. The existing ICharacterDataAccess interface (F420) provides CSV template data access but not runtime character data (which may differ from CSV defaults after game modifications). A new ICharacterDataService interface is needed to:
1. Define Era.Core-compatible character data access contract
2. Enable engine-layer implementation that reads from CharacterData
3. Allow DI injection into TemplateDialogueRenderer resolvers

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F420 | [DONE] | Similar interface | ICharacterDataAccess provides GetCsvString for CSV template CALLNAME; different from runtime character data |
| F551 | [DONE] | Direct dependency | TemplateDialogueRenderer.CallNameResolver needs this service |
| F553 | [PROPOSED] | Affected by this fix | KojoEngine Facade uses TemplateDialogueRenderer which will benefit from actual CALLNAME resolution |
| F544 | [DONE] | Related interface | IDialogueRenderer interface; this feature enhances its concrete implementation |

### Pattern Analysis

This follows the established pattern from F420 (ICharacterDataAccess) and F558 (service bridge pattern). Era.Core defines interface + stub implementation; engine layer creates actual implementation using GlobalStatic/CharacterData. The stub allows Era.Core.Tests to function independently.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | ICharacterDataAccess (F420) demonstrates the pattern; CharacterData stores CALLNAME at VariableCode.CALLNAME index |
| Scope is realistic | YES | Single interface + implementation + DI registration; ~100 lines total |
| No blocking constraints | YES | F551 is [DONE], CallNameResolver already isolated for replacement |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F551 | [DONE] | TemplateDialogueRenderer must exist before we can inject character data service into its resolvers |
| Related | F420 | [DONE] | ICharacterDataAccess provides similar pattern for CSV template data |
| Successor | F553 | [PROPOSED] | F553 depends on F628's TemplateDialogueRenderer constructor changes; MUST include Task "Remove TemplateDialogueRenderer parameterless constructor" |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| engine/CharacterData.cs | Engine integration | Low | Well-established character data storage; CALLNAME access pattern documented |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs | HIGH | CallNameResolver will use ICharacterDataService to get actual character names |
| Era.Core.Tests/Dialogue/Rendering/TemplateDialogueRendererTests.cs | MEDIUM | Tests will use stub ICharacterDataService for predictable test behavior |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Characters/ICharacterDataService.cs | Create | Interface defining GetCallName(CharacterId) method |
| Era.Core/Characters/NullCharacterDataService.cs | Create | Stub implementation returning placeholder (for Era.Core.Tests) |
| Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs | Update | Inject ICharacterDataService into CallNameResolver |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | Update | Register ICharacterDataService |
| engine/Assets/Scripts/Emuera/Services/CharacterDataServiceImpl.cs | Create (out of scope) | Engine implementation reading from CharacterData (Phase 11 pattern) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Era.Core cannot reference engine | Architecture | MEDIUM - Must use DI bridge pattern (interface in Era.Core, impl in engine) |
| CharacterData uses VariableCode indexing | engine/CharacterData.cs | LOW - Well-documented pattern at line 102 |
| Character names can be modified at runtime | Game design | LOW - Service reads current value, not CSV template default |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| CharacterId not in CharacterList | Medium | Low | Return Result.Fail with descriptive error message |
| Character name is null/empty | Low | Low | Fallback to character template name or return empty string per eraTW pattern |
| Thread safety for character list access | Low | Medium | Use engine-layer locking or copy-on-read pattern |

---

## Review Notes

- [resolved-applied] Phase1-Uncertain iter1: Interface snippet is documentation/illustration, not compilable code. Using statements are typically omitted in documentation snippets for brevity. Whether this is an issue depends on documentation standards. The actual implementation file will need proper using statements regardless.
- [resolved-applied] Phase1-Uncertain iter1: TemplateDialogueRenderer is currently not DI-registered (created directly in KojoEngine). If CallNameResolver needs ICharacterDataService, either TemplateDialogueRenderer gets DI registration OR receives ICharacterDataService in its constructor. The feature's Technical Design shows DI registration for ICharacterDataService, but how TemplateDialogueRenderer obtains it is not fully specified. This may require clarification but is a design detail, not a spec issue.
- [skipped] Phase1-Uncertain iter4: The AC Details clearly specify tests use TestCharacterDataService (mock/fake), not NullCharacterDataService. The test class name 'CharacterDataServiceTests' is a standard naming convention for testing ICharacterDataService interface contract. Whether this requires renaming is subjective.
- [resolved-applied] Phase1-Uncertain iter8: F553 dependency type: F553 is listed as Related but Handoff section requires F553 to include debt tracking Task. Whether this constitutes a Successor dependency (F553 depends on this feature) vs Related (cross-reference) depends on interpretation of hard vs soft dependency.
- [skipped] Phase1-Uncertain iter10: AC#9 Method column already specifies comma-separated paths correctly. Issue claims discrepancy that may not exist in actual verification tool behavior.
- [skipped] Phase1-Uncertain iter10: Result<string> namespace verification is correct (Result<T> is in Era.Core.Types). Using statement is already present. This is suggestion to verify rather than actionable fix.
- [resolved-applied] Phase6-Critical iter10: F628 requires F553 to include task "Remove TemplateDialogueRenderer parameterless constructor (F628 debt)" but F553.md Tasks table does not contain this task. Implementation Gate requires manual F553 update before F628 completion.

---

## Handoff

### Deferred Tasks

| Task | Destination | Note |
|------|-------------|------|
| Remove TemplateDialogueRenderer parameterless constructor | F553 | Zero Debt tracking: Backward compatibility constructor must be removed when KojoEngine Facade integrates DI pattern |

**Handoff Protocol**: F553 implementation MUST include Task "Remove TemplateDialogueRenderer parameterless constructor (F628 debt)" in its Tasks table to satisfy zero debt upfront principle.

**Implementation Gate**: F628 implementation phase must verify F553 Tasks table contains this debt tracking task before proceeding. If missing, add Task 8 to F553: "Remove TemplateDialogueRenderer parameterless constructor (F628 debt tracking)" before F628 completion.

---

## Links

- [index-features.md](index-features.md)
- [F551: TemplateDialogueRenderer Implementation](feature-551.md)
- [feature-template.md](reference/feature-template.md)
