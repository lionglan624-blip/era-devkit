# Feature 064: Headless口上テスト - Phase 2c Process-level Parallel

## Status: [DONE]

## Background

<!-- Session handoff: Record ALL discussion details here -->
- **Origin**: Split from Feature 062 Phase 2 (originally WBS-062 tasks 13-17)
- **Prerequisite**: Feature 062 Phase 2a (Interactive Mode) complete
- **Purpose**: Memory-stable parallel test execution via process isolation

## Overview

Implement process-level parallel execution for kojo batch tests. Each test runs in a separate process to ensure memory isolation and stability for large test suites.

## Problem

- In-process parallel execution can lead to memory growth
- Long test suites may OOM or slow down over time
- Thread-level parallelism shares game state (potential conflicts)

## Goals

1. Run each test in a separate worker process
2. Automatic worker count based on available memory
3. Memory-stable execution for 100+ test suites
4. Linear speedup proportional to worker count

## Acceptance Criteria

- [ ] `--parallel` spawns separate worker processes
- [ ] `--parallel auto` calculates optimal worker count from memory
- [ ] Memory usage stable over 100+ tests
- [ ] Execution time scales with N/workers
- [ ] `--verbose` logs memory usage per worker
- [ ] Build succeeds (0 errors, 0 warnings)
- [ ] Unit tests pass (85/85+)

## Technical Design

### ProcessLevelParallelRunner

```csharp
public class ProcessLevelParallelRunner
{
    private readonly int workerCount_;
    private readonly long memoryThreshold_;

    public ProcessLevelParallelRunner(int? workers = null)
    {
        workerCount_ = workers ?? CalculateOptimalWorkers();
        memoryThreshold_ = CalculateMemoryThreshold();
    }

    private int CalculateOptimalWorkers()
    {
        // Based on available memory and per-process footprint (~200MB)
        var available = GetAvailableMemory();
        var perProcess = 200 * 1024 * 1024; // 200MB
        return Math.Max(1, Math.Min(Environment.ProcessorCount, (int)(available / perProcess)));
    }

    public BatchResult Run(List<TestScenario> tests)
    {
        return tests.AsParallel()
            .WithDegreeOfParallelism(workerCount_)
            .Select(test => RunInProcess(test))
            .Aggregate(new BatchResult(), MergeResults);
    }

    private TestResult RunInProcess(TestScenario test)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project uEmuera.Headless.csproj -- . " +
                       $"--unit \"{test.File}\" --output-mode json",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return ParseJsonResult(output);
    }
}
```

### Memory Threshold Calculation

```csharp
private long CalculateMemoryThreshold()
{
    // Leave 500MB free for system
    var total = GetTotalPhysicalMemory();
    var reserved = 500 * 1024 * 1024;
    return total - reserved;
}

private void MonitorMemoryUsage()
{
    if (verbose_)
    {
        var used = Process.GetCurrentProcess().WorkingSet64;
        Console.Error.WriteLine($"[Memory] Used: {used / 1024 / 1024}MB");
    }
}
```

### CLI Options

```bash
# Auto worker count (based on memory)
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "tests/kojo/*.json" --parallel

# Fixed worker count
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "tests/kojo/*.json" --parallel 4

# With memory logging
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "tests/kojo/*.json" --parallel --verbose
```

## Test Specifications

| # | Test Name | Input | Expected | Status |
|---|-----------|-------|----------|--------|
| T1 | Parallel_ProcessIsolation | `--parallel 2` + 4 tests | Each in separate process | [ ] |
| T2 | Parallel_AutoCount | `--parallel` (no N) | Calculated from memory | [ ] |
| T3 | Parallel_MemoryStable | 100+ tests | Memory doesn't grow linearly | [ ] |
| T4 | Parallel_SpeedLinear | 50 tests | Time ~= N/workers | [ ] |
| T5 | Parallel_VerboseLog | `--parallel --verbose` | Memory usage logged | [ ] |
| T6 | Parallel_SingleWorker | `--parallel 1` | Sequential but process-isolated | [ ] |

## Performance Targets

| Metric | Target |
|--------|--------|
| Memory per worker | ~200MB |
| Worker startup time | <2s |
| 100 tests (4 workers) | <60s |
| Memory growth | <10% over 100 tests |

## Effort Estimate

- **Size**: Medium-Large
- **Risk**: Medium (process management complexity)
- **Testability**: High (measurable metrics)

## Links

- [feature-062.md](feature-062.md) - Phase 2a (Interactive Mode)
- [feature-063.md](feature-063.md) - Phase 2b (Coverage Report)
- [reference/testing-reference.md](reference/testing-reference.md) - Test documentation
