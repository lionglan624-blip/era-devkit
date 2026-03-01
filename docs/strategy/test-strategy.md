# Test Strategy

**Date**: 2026-01-15
**Feature**: F499
**Scope**: Test strategy for Era.Core including IRandomProvider abstraction and test layer structure.

---

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
- Focus on isolated business logic without external dependencies
- Manual mocking approach (no Moq/NSubstitute framework)

### Integration Test Guidelines:
- Real DI container setup
- Seeded IRandomProvider for reproducibility
- Test service interactions
- Verify DI registration correctness
- Validate interface contracts between services

### E2E Test Guidelines:
- Headless mode execution
- Scenario-based (game/tests/*.json)
- Golden Master pattern for output verification
- Full game flow validation
- **Incremental checkpoints** at Phase 14/22/27/30 (see Section 8)

---

## 2. Test Types

| Type | Purpose | Execution Timing | Tool |
|------|---------|------------------|------|
| **AC Verification** | Verify feature acceptance criteria | /do Phase 6 | dotnet test + verify-logs.py |
| **Regression** | Prevent regressions | pre-commit, CI | dotnet test via WSL (all tests) |
| **Linter** | Static analysis | pre-commit | Roslyn Analyzer (ErbLinter deprecated) |
| **Integration** | Service wiring | /do Phase 6 | dotnet test --filter Integration |
| **E2E** | Full scenarios | /do Phase 6, Post-Phase Review | headless mode |

### AC Verification:
- Matcher-based (contains, equals, not_contains, etc.)
- TRX output to _out/logs/prod/ac/engine/feature-{ID}/
- verify-logs.py for result aggregation
- Binary PASS/FAIL judgment only

### Regression Tests:
- Execute: `dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s`
- All tests must PASS before commit
- Immutable (protected by pre-commit hook)
- 1019 tests across 89 test files (as of Phase 15)

### Linter / Static Analysis:
- C# uses Roslyn Analyzer (`AnalysisLevel=latest-recommended` in `Directory.Build.props`)
- Dashboard (JS/JSX) uses ESLint v9 (flat config) + Prettier
- ERB linting deprecated (ErbLinter removed)
- Build warnings are treated as errors in CI (`TreatWarningsAsErrors=true`)
- Pre-commit hook checks staged dashboard files (ESLint + Prettier)
- NoWarn suppressed rules tracked in `memory/analyzer-nowarn-debt.md`

### Mutation Testing:
- Dashboard backend: Stryker (JS) — `cd src/tools/node/feature-dashboard/backend && npm run test:mutation`
- Era.Core: Stryker.NET — `cd Era.Core.Tests && dotnet stryker`
- Executed at Post-Phase Review (not per-commit — high cost)
- Scores recorded in Post-Phase Review progress log for trend tracking

---

## 3. /do Command Integration

### Phase 3: TDD (Test-Driven Development)

**/do command responsibilities**:
1. Create C# test methods based on ACs
2. Execute tests to confirm RED (tests fail initially)
3. Implement feature code
4. Re-execute tests to confirm GREEN (tests pass)

**Workflow**:
```bash
# Phase 3: Create tests (implementer agent)
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=feature-{ID}-red.trx"
# Expected: Tests FAIL (RED confirmation)

# Phase 3: Implement feature code
# ... implementation ...

# Phase 3: Re-execute tests
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=feature-{ID}-green.trx"
# Expected: Tests PASS (GREEN confirmation)
```

### Phase 6: Verification (AC検証)

**/do command responsibilities**:
1. Execute all AC verification tests
2. Output logs to `_out/logs/prod/ac/engine/feature-{ID}/`
3. Run verify-logs.py for result aggregation
4. Update feature-{ID}.md AC Status column

**Workflow**:
```bash
# Phase 6: Execute AC tests (ac-tester agent)
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=ac-{AC#}.trx"

# Phase 6: Aggregate results
python tools/verify-logs.py --scope feature:{ID}

# Phase 6: Update feature file
# Edit feature-{ID}.md: [ ] → [x] or [-] based on results
```

### Scope Specification

**Feature scope**:
```bash
--filter "FullyQualifiedName~Feature{ID}"
```

**AC scope**:
```bash
--filter "FullyQualifiedName~Feature{ID}AC{N}"
```

## 4. Log Output

### TRX Output (C# Tests)

**Path**: `_out/logs/prod/ac/engine/feature-{ID}/`

**Naming Convention**:
- RED confirmation: `feature-{ID}-red.trx`
- GREEN confirmation: `feature-{ID}-green.trx`
- AC verification: `ac-{AC#}.trx`

**Format**: MSTest TRX (XML)

### JSON Output (AC Verification)

**Path**: `_out/logs/prod/ac/feature-{ID}.json`

**Format**:
```json
{
  "feature_id": "F{ID}",
  "total_acs": N,
  "passed": N,
  "failed": N,
  "acs": [
    {
      "ac_number": 1,
      "description": "...",
      "status": "PASS|FAIL",
      "matcher": "contains",
      "expected": "...",
      "actual": "..."
    }
  ]
}
```

### verify-logs.py Integration

**Usage**:
```bash
# Scope: Single feature
python tools/verify-logs.py --scope feature:{ID}

# Scope: All tests
python tools/verify-logs.py --scope all

# Output: Console report with PASS/FAIL counts
```

**Result Aggregation** (execute from repository root):
- Reads JSON result files matching `*-result.json` pattern in ac/ subdirectories
- Always checks regression logs (regression is default scope)
- Outputs console summary (PASS/FAIL counts per category)

## 5. Pre-commit Hook

**Note**: This section documents the DESIGNED pre-commit hook behavior for C# development. Current hook (.githooks/pre-commit) runs verify_com_map.py and conditionally verify-logs.py. The C# test execution described here is for FUTURE phases when C# migration is complete.

### Execution Target (Future State)

**Command**: `wsl_dotnet test devkit.sln --no-build --blame-hang-timeout 10s` (via WSL2)

**Scope**: All Era.Core tests (unit + integration)

### Execution Conditions

**Always execute**:
- Pre-commit hook runs full test suite
- All tests must PASS before commit allowed

**Feature-specific scope** (during feature implementation):
- Add feature scope filter: `--filter "FullyQualifiedName~Feature{ID}"`
- Faster feedback during development

### Linter Integration

**C# Static Analysis**: Roslyn Analyzer
- Configured in Era.Core.csproj
- Runs automatically during `dotnet build`
- Warnings treated as errors (WarningsAsErrors=true)

**ErbLinter**: Deprecated (current state)
- ErbLinter tool deprecated - no ERB static analysis currently
- ERB code files remain in Game/ERB/ for execution but new development is C#
- C# code is linted by Roslyn Analyzer (separate concern from ERB)

### Hook Configuration

```bash
# .githooks/pre-commit (WSL2 経由)
wsl_dotnet build devkit.sln --nologo -v q
wsl_dotnet test devkit.sln --nologo --no-build --blame-hang-timeout 10s -v m
# Exit code: 0 (PASS) → Allow commit, Non-zero (FAIL) → Block commit
```

## 6. AC Verification Flow

### AC Type別検証方法

| AC Type | Method | Matcher | Verification Tool |
|---------|--------|---------|-------------------|
| **test** | dotnet test | succeeds/fails | dotnet test + TRX |
| **code** | Grep | contains/not_contains/matches | Grep tool |
| **file** | Glob/Grep | exists/contains | Glob/Grep tools |
| **build** | dotnet build | succeeds | dotnet build |
| **output** | headless mode | contains/matches | Bash + Grep |
| **variable** | headless mode | equals | Bash + Grep |

### Matcher Definitions

| Matcher | Behavior | Example |
|---------|----------|---------|
| **equals** | Exact match | Expected: "100", Actual: "100" |
| **contains** | Substring match | Expected: "Error", Actual: "RuntimeError occurred" |
| **not_contains** | Absence check | Expected: "TODO", Actual: "// Clean code" |
| **matches** | Regex match | Expected: "\\d{3}", Actual: "123" |
| **succeeds** | Exit code 0 | Command exits 0 |
| **fails** | Exit code non-zero | Command exits 1 |
| **exists** | File exists | Glob finds file |
| **not_exists** | File absent | Glob finds no file |
| **count_equals** | Count match | Expected: 5, Actual: 5 instances |
| **gt/gte/lt/lte** | Numeric comparison | Expected: ">= 10", Actual: 15 |

### Log Format

**PASS Example**:
```
[PASS] AC#1: CharacterId interface exists
  Type: file
  Method: Glob
  Matcher: exists
  Expected: src/Era.Core/Types/CharacterId.cs
  Actual: File found
```

**FAIL Example**:
```
[FAIL] AC#2: Zero technical debt
  Type: file
  Method: Grep
  Matcher: not_contains
  Expected: TODO|FIXME|HACK
  Actual: Found 3 matches in src/Era.Core/Functions/Foo.cs:42, 58, 91
```

### Result Judgment

**Binary**: PASS/FAIL only (no "Confidence" levels)

**Criteria**:
- Matcher satisfied -> PASS
- Matcher not satisfied -> FAIL
- Tool error (file not found, command failed) -> FAIL with ERROR annotation

---

## 7. IRandomProvider

### Interface Design

```csharp
// src/Era.Core/Random/IRandomProvider.cs
namespace Era.Core.Random;

/// <summary>Provides random number generation for game logic</summary>
/// <remarks>
/// Creates a new interface parallel to IRandom (Era.Core.Functions).
/// IRandomProvider does NOT extend IRandom; they are separate interfaces.
/// Uses long types to match ERA's 64-bit integer system.
/// See F501 for migration details (JuelProcessor only in F501 scope).
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
- Seeded providers enable reproducible test scenarios for debugging

### DI Registration

```csharp
// src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
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

    public long GetRandomValue()
    {
        return _random.Next(100);
    }
}
```

### Migration Scope:
- RAND function: Use IRandomProvider.Next()
- RANDDATA function: Use IRandomProvider.Next(min, max)
- COM random selection: Use IRandomProvider.NextFromArray()
- Growth calculations: Use IRandomProvider if random
- **JuelProcessor.cs:19**: CRITICAL refactoring target identified by F498 - direct `new Random()` usage must be replaced with DI-injected IRandomProvider (design documented here; implementation in F501 Architecture Refactoring)

### Testability Benefits:
- Mock injection enables deterministic testing of random-dependent logic
- Seed control allows reproducing specific test scenarios
- Eliminates untestable direct Random() instantiation (F498 High Severity issue)
- Enables unit testing of JUEL expression evaluation with predictable random values

### F498 Critical Finding Resolution:
F498 identified JuelProcessor.cs line 19 direct Random() instantiation as a High Severity testability blocker. This design specifies the IRandomProvider interface as the solution. Implementation tracking:
- **F499** (this feature): Design IRandomProvider interface
- **F501**: Refactor JuelProcessor to inject IRandomProvider via constructor
- **F501**: Update all JuelProcessor instantiation sites
- **F501**: Verify DI container registration in AddEraCore()
- **F501**: Migrate existing JuelProcessor tests to inject mock IRandomProvider

---

## 8. Incremental E2E Checkpoints

**SSOT**: [full-csharp-architecture.md](full-csharp-architecture.md#incremental-e2e-test-strategy)

Phase 30 まで E2E を先送りすると、統合時に DI 登録漏れ・インターフェース不整合・イベント伝播断絶が大量発生し、原因特定が困難になる。段階的に統合検証を行い、リスクを早期検出する。

| CP | Phase | Level | 検証内容 |
|:--:|:-----:|-------|----------|
| 1 | 14 | Smoke | DI解決 + Headless起動 + 1コマンド実行 |
| 2 | 22 | Partial | Shop->Counter->State 一連フロー |
| 3 | 27 | System | 全サブシステム連携 + NTR + 拡張モジュール |
| 4 | 30 | Full | 既存Phase 30 E2E設計に統合 |

**原則**: CP で発見された統合不具合は次Phase に持ち越さず、当該Phase 内で修正する。

**実装先**: `src/Era.Core.Tests/E2E/` ディレクトリ配下に Phase 別テストクラスを配置。
