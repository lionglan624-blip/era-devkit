# Engine Testing (C# Unit)

C# engine testing-specific reference. See [SKILL.md](SKILL.md) for common information.

> **Last Updated**: 2026-02-12

---

## C# Unit Tests

```bash
dotnet test src/engine.Tests/
```

**Specific test**: `dotnet test src/engine.Tests/ --filter "FullyQualifiedName~CommandDispatcher"`

---

## Test Patterns

### Collection Fixture (Shared State)

Tests using shared state like GlobalStatic require Collection definition:

```csharp
[Collection("GlobalStatic")]
public class MyTests
{
    [Fact]
    public void Test_Something()
    {
        GlobalStatic.Reset();  // Reset before test
        // ... test ...
        GlobalStatic.Reset();  // Cleanup
    }
}
```

### Encoding Fixture (Shift-JIS)

Tests using Shift-JIS (932) must use Encoding Collection:

```csharp
[Collection("Encoding")]
public class EncodingTests { }
```

### Mock Pattern

Mock interfaces to verify DI:

```csharp
private class MockCommandDispatcher : ICommandDispatcher
{
    public bool InitializeCalled { get; private set; }
    public void InitializeCommands() => InitializeCalled = true;
    // ... implement interface ...
}

[Fact]
public void GlobalStatic_CanBeOverridden()
{
    GlobalStatic.Reset();
    GlobalStatic.CommandDispatcher = new MockCommandDispatcher();
    // ... assert ...
    GlobalStatic.Reset();  // Cleanup
}
```

---

## Test Files

| File | Purpose |
|------|---------|
| `TestFixture.cs` | EncodingFixture (Shift-JIS) |
| `GlobalStaticCollection.cs` | GlobalStatic Collection定義 |
| `*Tests.cs` | 各機能のテスト |

---

## Test Workflow

```
C# Unit → Integration (dotnet test) → Regression
```

**When adding new engine features**:
1. Define interface (GlobalStatic DI)
2. Create unit tests (using mocks) - **Both positive/negative**
3. Implementation
4. Integration verification with dotnet test

---

## Positive/Negative Testing

Engine requires both positive/negative. See [SKILL.md](SKILL.md#positivenegative-testing) for details.

| Test Target | Positive Example | Negative Example |
|-------------|------------------|------------------|
| Path conversion | ac/ → success | debug/ → null |
| Input validation | Valid value → continue | Invalid value → error |
| Boundary conditions | N=max → success | N=max+1 → failure |

---

## xUnit v3 Features (Phase 11)

Migrated from xUnit v2 → v3 in Phase 11. Following new features available.

### Package Reference

```xml
<!-- csproj -->
<PackageReference Include="xunit.v3" Version="3.2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.*" />
```

### TestContext.Current

Access context information during test execution:

```csharp
[Fact]
public async Task LongRunningOperation_CanBeCancelled()
{
    // CancellationToken for cooperative cancellation
    var token = TestContext.Current.CancellationToken;

    await SomeLongOperationAsync(token);

    // Diagnostic messages (appears in test output)
    TestContext.Current.SendDiagnosticMessage("Operation completed");
}

[Fact]
public void Test_WithWarning()
{
    // Add warnings to test results
    TestContext.Current.AddWarning("This test uses deprecated API");

    // Key-value storage for cross-stage communication
    TestContext.Current.KeyValueStorage["setup_time"] = DateTime.Now;
}
```

### CancellationToken Pattern (xUnit1051 compliance)

Use `TestContext.Current.CancellationToken` in async tests:

```csharp
// NG: xUnit1051 warning - test method does not pass cancellation token
[Fact]
public async Task Test_WithoutToken()
{
    await Task.Delay(100);  // Warning: no cancellation support
}

// OK: Proper cancellation token usage
[Fact]
public async Task Test_WithToken()
{
    await Task.Delay(100, TestContext.Current.CancellationToken);
}

// OK: Pass to downstream methods
[Fact]
public async Task ProcessTraining_Async()
{
    var ct = TestContext.Current.CancellationToken;
    var result = await processor.ProcessAsync(target, ct);
    Assert.True(result.Success);
}
```

### Matrix Theory Data

Combinatorial parameter tests:

```csharp
// Traditional MemberData
public static IEnumerable<object[]> GetTestData()
{
    yield return [CharacterId.Meiling, CommandId.Kiss, 10];
    yield return [CharacterId.Sakuya, CommandId.Caress, 5];
}

[Theory]
[MemberData(nameof(GetTestData))]
public void Training_CalculatesCorrectGrowth(CharacterId target, CommandId cmd, int expected)
{
    var result = processor.Process(target, cmd);
    Assert.Equal(expected, result.Value.Growth);
}

// v3 Matrix style (all combinations)
[Theory]
[CombinatorialData]
public void AllCombinations_Work(
    [CombinatorialValues(1, 2, 3)] int a,
    [CombinatorialValues("x", "y")] string b)
{
    // Tests: (1,x), (1,y), (2,x), (2,y), (3,x), (3,y)
}
```

### Explicit Tests

Tests run only under specific conditions:

```csharp
// Skip in normal runs, run only when explicitly requested
[Fact(Explicit = true)]
public void SlowIntegrationTest()
{
    // Long-running test excluded from default runs
}

// Run with: dotnet test --filter "Explicit=true"
```

### Assembly-Level Fixture

Fixture shared across entire assembly:

```csharp
// Define fixture
public class DatabaseFixture : IAsyncLifetime
{
    public IDbConnection Connection { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Connection = await CreateConnectionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}

// Register at assembly level
[assembly: AssemblyFixture(typeof(DatabaseFixture))]

// Use in tests
public class DatabaseTests(DatabaseFixture fixture)
{
    [Fact]
    public void Query_ReturnsData()
    {
        var result = fixture.Connection.Query("SELECT 1");
        Assert.NotEmpty(result);
    }
}
```

### Migration Notes

| v2 | v3 | Notes |
|----|----|----|
| `xunit` package | `xunit.v3` | Package name change |
| `xunit.runner.visualstudio` 2.x | 3.x | Runner version |
| No TestContext | `TestContext.Current` | New API |
| Assert (mostly same) | Assert + new overloads | Superset |

### Recommended Patterns

| Scenario | Pattern |
|----------|---------|
| Async tests | Always pass `TestContext.Current.CancellationToken` |
| Shared state | Use Collection or Assembly-Level Fixture |
| Slow tests | Mark with `Explicit = true` |
| Debug output | Use `TestContext.Current.SendDiagnosticMessage()` |
| Test data | Use `[MemberData]` or `[CombinatorialData]` |

### References

- [xUnit v3 What's New](https://xunit.net/docs/getting-started/v3/whats-new)
- [xUnit v3 Migration Guide](https://xunit.net/docs/getting-started/v3/migration)
