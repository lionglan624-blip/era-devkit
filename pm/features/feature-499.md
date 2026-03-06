# Feature 499: Test Strategy Design: IRandomProvider and Test Layers

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

## Created: 2026-01-14

---

## Summary

Design test strategy for Era.Core including IRandomProvider abstraction and test layer structure.

**Design Scope**:
- IRandomProvider interface design (random element abstraction)
- Test layer structure (Unit/Integration/E2E responsibilities)
- Test type definitions (AC verification, regression, linter, integration, E2E)
- Random element seeding strategy
- Test data management patterns

**Output**: test-strategy.md sections 1 (Test Layers), 2 (Test Types), 7 (IRandomProvider).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 15: Architecture Review** - Validate Phase 1-14 implementations against Phase 4 design principles (SRP, DIP, Strongly Typed IDs, Result type, DI registration) before large-scale parallel implementation phases (Phase 19-21). Ensures architectural consistency and identifies technical debt requiring resolution.

### Problem (Current Issue)

Test strategy not formalized for Era.Core:
- Random elements (RAND, COM selection) not abstracted
- Test layer responsibilities unclear (Unit vs Integration vs E2E)
- Test type definitions incomplete
- No standardized seeding strategy for deterministic tests
- F498 Testability Assessment identified IRandomProvider requirements

### Goal (What to Achieve)

1. **Design IRandomProvider interface** with methods for all random operations
2. **Define test layer structure** (Unit/Integration/E2E responsibilities)
3. **Document test types** (AC verification, regression, etc.)
4. **Specify seeding strategy** for deterministic testing
5. **Create test-strategy.md sections** (1, 2, 7 per architecture.md Phase 15)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | test-strategy.md exists | file | Glob | exists | Game/agents/designs/test-strategy.md | [x] |
| 2 | Test Layers section | file | Grep(Game/agents/designs/test-strategy.md) | contains | ## 1. Test Layers | [x] |
| 3 | Test Types section | file | Grep(Game/agents/designs/test-strategy.md) | contains | ## 2. Test Types | [x] |
| 4 | IRandomProvider section | file | Grep(Game/agents/designs/test-strategy.md) | contains | ## 7. IRandomProvider | [x] |
| 5 | IRandomProvider interface | file | Grep(Game/agents/designs/test-strategy.md) | contains | interface IRandomProvider | [x] |
| 6 | Test layer responsibilities | file | Grep(Game/agents/designs/test-strategy.md) | contains | Unit Test Guidelines | [x] |
| 7 | Seeding strategy | file | Grep(Game/agents/designs/test-strategy.md) | contains | Seed Control: | [x] |
| 8 | DI registration pattern | file | Grep(Game/agents/designs/test-strategy.md) | contains | AddSingleton<IRandomProvider | [x] |
| 9 | 負債ゼロ | file | Grep(Game/agents/designs/test-strategy.md) | not_contains | // TODO | [x] |

### AC Details

**AC#1**: test-strategy.md exists
- Test: Glob pattern="Game/agents/designs/test-strategy.md"
- Expected: File exists

**AC#2-4**: Required sections exist
- Test: Grep patterns for sections 1, 2, 7 in test-strategy.md
- Expected: All three sections present

**AC#5**: IRandomProvider interface defined
- Test: Grep pattern="interface IRandomProvider" in test-strategy.md
- Expected: Interface code snippet included

**AC#6**: Test layer responsibilities documented
- Test: Grep pattern="Unit.*Integration.*E2E" (multiline) in test-strategy.md section 1
- Expected: Each layer's responsibilities defined (table rows Unit, Integration, E2E in sequence)

**AC#7**: Seeding strategy specified
- Test: Grep pattern="Seed Control:" in test-strategy.md section 7
- Expected: Describes deterministic seeding for tests

**AC#8**: DI registration pattern documented
- Test: Grep pattern="AddSingleton.*IRandomProvider" in test-strategy.md section 7
- Expected: Shows DI registration code snippet

**AC#9**: Zero technical debt in test strategy documentation
- Test: Grep pattern="TODO|FIXME|HACK" path="Game/agents/designs/test-strategy.md"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Create test-strategy.md with required sections (1, 2, 7) | [x] |
| 2 | 5,6,7,8 | Design IRandomProvider interface, test layer structure, and JuelProcessor migration plan | [x] |
| 3 | 9 | Verify zero technical debt in test strategy documentation (負債解消) | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 3 Tasks (batch verification waiver for Tasks 1-2 following F384 precedent for related design sections) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Step 1: Read F498 Output

Before designing, read testability-assessment-15.md to understand IRandom Enhancement Requirements:
- Path: `Game/agents/designs/testability-assessment-15.md`
- Extract: Random element categories, test coverage gaps, hard-to-test patterns

### Step 2: Create test-strategy.md

Per architecture.md Phase 15 requirements, sections 1, 2, 7:

```markdown
# Test Strategy

## 1. Test Layers

| Layer | Responsibilities | Target | Execution |
|-------|------------------|--------|-----------|
| **Unit** | Single class/method isolation | Era.Core classes | Fast, frequent |
| **Integration** | Service interaction, DI wiring | Multiple services | Moderate speed |
| **E2E** | Full game flow, headless mode | Complete scenarios | Slow, comprehensive |

### Unit Test Guidelines:
- Mock all dependencies via DI
- Use IRandomProvider mock for deterministic behavior
- Test single responsibility

### Integration Test Guidelines:
- Real DI container setup
- Seeded IRandomProvider for reproducibility
- Test service interactions

### E2E Test Guidelines:
- Headless mode execution
- Scenario-based (game/tests/*.json)
- Golden Master pattern for output verification

## 2. Test Types

| Type | Purpose | Execution Timing | Tool |
|------|---------|------------------|------|
| **AC Verification** | Verify feature acceptance criteria | /do Phase 6 | dotnet test + verify-logs.py |
| **Regression** | Prevent regressions | pre-commit, CI | dotnet test (all tests) |
| **Linter** | Static analysis | pre-commit | Roslyn Analyzer (ErbLinter deprecated) |
| **Integration** | Service wiring | /do Phase 6 | dotnet test --filter Integration |
| **E2E** | Full scenarios | /do Phase 6, Post-Phase Review | headless mode |

### AC Verification:
- Matcher-based (contains, equals, not_contains, etc.)
- TRX output to Game/logs/prod/ac/engine/feature-{ID}/
- verify-logs.py for result aggregation

### Regression Tests:
- Execute: `dotnet test Era.Core.Tests`
- All tests must PASS before commit
- Immutable (protected by pre-commit hook)

### Linter:
- C# uses Roslyn Analyzer (no separate linter)
- ERB linting deprecated (ErbLinter removed)

## 7. IRandomProvider

### Interface Design

```csharp
// Era.Core/Random/IRandomProvider.cs
namespace Era.Core.Random;

/// <summary>Provides random number generation for game logic</summary>
/// <remarks>
/// Extends existing IRandom (Era.Core.Functions) with additional methods.
/// Uses long types to match ERA's 64-bit integer system.
/// F418's IRandom will be migrated to this interface in F501.
/// </remarks>
public interface IRandomProvider
{
    /// <summary>Get random integer in range [0, max)</summary>
    long Next(long max);

    /// <summary>Get random integer in range [min, max)</summary>
    long Next(long min, long max);

    /// <summary>Select random element from array</summary>
    T NextFromArray<T>(T[] array);

    /// <summary>Get current seed (for debugging/reproducibility)</summary>
    long Seed { get; }
}
```

### Seed Control:
- **Production**: System.Random-based implementation, unseeded (true random)
- **Test**: Seeded implementation for deterministic behavior
- Tests specify seed via mock or test-specific IRandomProvider implementation

### DI Registration

```csharp
// Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
services.AddSingleton<IRandomProvider, SystemRandomProvider>();

// Test code
var provider = new SeededRandomProvider(seed: 12345);
services.AddSingleton<IRandomProvider>(provider);
```

### Usage Pattern

**Before** (hard-coded Random):
```csharp
var random = new Random();
var value = random.Next(100);
```

**After** (IRandomProvider DI):
```csharp
public class MyService
{
    private readonly IRandomProvider _random;

    public MyService(IRandomProvider random)
    {
        _random = random;
    }

    public int GetRandomValue()
    {
        return _random.Next(100);
    }
}
```

### Migration Scope:
- RAND function: Use IRandomProvider.Next()
- RANDDATA function: Use IRandomProvider.NextInRange()
- COM random selection: Use IRandomProvider.NextFromArray()
- Growth calculations: Use IRandomProvider if random
- **JuelProcessor.cs:19**: CRITICAL refactoring target identified by F498 - direct `new Random()` usage must be replaced with DI-injected IRandomProvider (design documented here; implementation in F501 Architecture Refactoring)

## 負債の意図的受け入れ:
(Document any test strategy debt accepted with justification)
```

### Input from F498

Use testability assessment findings:
- Random elements identified → IRandomProvider methods
- Test coverage gaps → Test layer responsibility assignments
- Hard-to-test patterns → Integration test guidelines

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F498 | Testability Assessment provides IRandomProvider requirements |
| Successor | F500 | Test Strategy Design: E2E and /do Integration (completes test-strategy.md) |
| Successor | F501 | Architecture Refactoring: Migrate F418 IRandom → IRandomProvider |
| Related | architecture.md | Test strategy sections must align with Phase 15 requirements |

---

## Links

- [feature-418.md](../index-features-history.md) - F418 IRandom interface (existing implementation context)
- [feature-486.md](feature-486.md) - Phase 15 Planning (parent feature)
- [feature-498.md](feature-498.md) - Testability Assessment (provides requirements)
- [testability-assessment-15.md](designs/testability-assessment-15.md) - F498 output: IRandom Enhancement Requirements (INPUT for this feature)
- [feature-500.md](feature-500.md) - Test Strategy Design: E2E and /do Integration (completes strategy)
- [feature-501.md](feature-501.md) - Architecture Refactoring (IRandom → IRandomProvider migration)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 15 test strategy requirements

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-15 FL iter1**: [resolved] Interface naming: Keep IRandomProvider per architecture.md (SSOT). F418's IRandom deviates from design; migration tracked in F501.
- **2026-01-15 FL iter1**: [resolved] AC#9 redundancy: Removed AC#9 (redundant with AC#2+AC#3+AC#4). Renumbered AC#10→AC#9.
- **2026-01-15 FL iter2**: [resolved] Type mismatch: Fixed to use long types (matches ERA 64-bit integers and existing IRandom).
- **2026-01-15 FL iter2**: [resolved] AC#6 multiline: Kept as-is (already noted in AC Details).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | spec-writer | Created from F486 Phase 15 Planning | PROPOSED |
| 2026-01-15 08:40 | START | implementer | Task 1 | - |
| 2026-01-15 08:40 | END | implementer | Task 1 | SUCCESS |
| 2026-01-15 08:42 | START | implementer | Task 2 | - |
| 2026-01-15 08:42 | END | implementer | Task 2 | SUCCESS |
| 2026-01-15 08:43 | START | implementer | Task 3 | - |
| 2026-01-15 08:43 | END | implementer | Task 3 | SUCCESS |
