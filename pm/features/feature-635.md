# Feature 635: Conversion Parallelization

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

## Created: 2026-01-27

---

## Summary

Add parallel processing capability to batch conversion for improved performance on 117 file conversion.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

Sequential conversion of 117 files may be slow. Parallel processing can significantly reduce total conversion time.

### Goal (What to Achieve)

1. Add `--parallel` flag to batch conversion
2. Implement thread-safe file processing
3. Aggregate results from parallel conversions
4. Handle errors without blocking other conversions

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Line 4305-4320
- [feature-634.md](feature-634.md) - F634 Batch Conversion Tool (Predecessor)
- [feature-633.md](feature-633.md) - F633 PRINTDATA Parser
- [feature-636.md](feature-636.md) - F636-F643 Per-character conversion batches (consumers)
- [feature-644.md](feature-644.md) - F644 Equivalence Testing (quality downstream)
- [feature-671.md](feature-671.md) - F671 PrintData Variant mapping

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Sequential conversion of 117 ERB files may be slow during batch processing
2. Why: BatchConverter.ConvertAsync() processes files in a `foreach` loop, awaiting each FileConverter.ConvertAsync() sequentially
3. Why: F634 was designed for correctness first -- sequential processing is the simplest correct implementation, and parallelization was explicitly deferred to F635
4. Why: Parallel processing introduces thread-safety concerns that require careful analysis of all shared state in the pipeline (ErbParser, DatalistConverter, TalentCsvLoader, YamlDotNet serializers, file I/O)
5. Why: The root cause is that BatchConverter lacks a parallel execution path with thread-safe result aggregation, despite all per-file operations being logically independent

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 117-file batch conversion runs slower than necessary | BatchConverter.ConvertAsync() uses sequential `foreach` with `await` per file, blocking on each file before starting the next |
| No `--parallel` CLI option exists | Program.cs does not parse a `--parallel` flag or configure parallel execution in BatchConverter |

### Conclusion

The root cause is that BatchConverter processes files sequentially in a `foreach` loop despite each file conversion being logically independent (separate input file, separate output directory, separate ErbParser instance). The pipeline components need thread-safety analysis:

- **ErbParser**: Stateless per-call -- `new ErbParser()` is created per file in FileConverter. Each instance uses local state only. **Thread-safe** (new instance per file).
- **TalentCsvLoader**: Read-only after construction. `_talentNameToIndex` Dictionary is populated in constructor and only read via `GetTalentIndex()`. **Thread-safe** for concurrent reads.
- **TalentConditionParser**: Uses `static readonly Regex` (compiled, thread-safe) and no instance state. **Thread-safe**.
- **DatalistConverter**: Contains `_talentLoader` (read-only), `_conditionParser` (thread-safe), `_yamlSerializer` (YamlDotNet ISerializer -- thread-safe after construction per YamlDotNet docs), `_schema` (JsonSchema -- read-only after FromFileAsync). **Thread-safe** for concurrent calls.
- **PrintDataConverter**: Contains `_yamlSerializer` only (thread-safe). **Thread-safe**.
- **PathAnalyzer**: Uses `static readonly Regex` (compiled, thread-safe) and no instance state. **Thread-safe**.
- **BatchReport**: Mutable class with `Total`, `Success`, `Failed` counters and `Failures` list. **NOT thread-safe** -- requires synchronization (lock, Interlocked, or ConcurrentBag).
- **Console.Error.WriteLine**: Thread-safe in .NET (Console output is synchronized).
- **File I/O**: `File.ReadAllTextAsync` and `File.WriteAllTextAsync` on different files are safe. `Directory.CreateDirectory` is idempotent and safe for concurrent calls.

The only thread-safety concern is **BatchReport mutation** during parallel execution. All other components are safe for concurrent use.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F634 | [DONE] | Predecessor (batch infrastructure) | Provides BatchConverter, FileConverter, all interfaces. F635 modifies BatchConverter to add parallel path |
| F636-F643 | [DRAFT] | Consumers | Per-character conversion batches that invoke BatchConverter. Will benefit from parallel mode |
| F644 | [DRAFT] | Downstream quality | Equivalence testing -- unaffected by parallelization (tests output correctness, not execution mode) |
| F633 | [DONE] | Indirect predecessor | PRINTDATA parser. No impact from parallelization |
| F671 | [DRAFT] | Related | PrintData Variant mapping. No impact from parallelization |

### Pattern Analysis

No recurring pattern found. This is a straightforward performance optimization following the established pattern of F634 delivering sequential correctness first, then F635 adding parallelization. The KojoComparer `BatchProcessor.ProcessAsync()` in `tools/KojoComparer/BatchProcessor.cs` also uses sequential processing (no parallel pattern in codebase to reference). .NET's `Parallel.ForEachAsync` (available in .NET 6+) is the standard library solution for this pattern.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All per-file operations are logically independent. Thread-safety analysis shows only BatchReport requires synchronization. `Parallel.ForEachAsync` is available in .NET 10 (target framework) |
| Scope is realistic | YES | Changes limited to: (1) BatchConverter.cs -- replace foreach with Parallel.ForEachAsync + lock/ConcurrentBag for results, (2) Program.cs -- parse `--parallel` flag, (3) IBatchConverter interface -- add optional parallelism parameter or separate method. ~3 files modified |
| No blocking constraints | YES | F634 has reached [DONE]. All code exists and F635 can proceed |

**Verdict**: FEASIBLE

F634 has reached [DONE] with all ACs completed and implementation finished. F635 can proceed without blocking constraints.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F634 | [DONE] | Batch Conversion Tool -- provides BatchConverter, FileConverter, IBatchConverter, IFileConverter, Program.cs, all infrastructure that F635 modifies |
| Successor | F636-F643 | [DRAFT] | Per-character conversion batches -- will use --parallel flag for faster execution |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Threading.Tasks.Parallel (Parallel.ForEachAsync) | Runtime | Low | Built into .NET 6+, available in target .NET 10 |
| ErbParser (tools/ErbParser/) | Build-time | Low | Already referenced. Thread-safe (new instance per file) |
| YamlDotNet 16.2.1 | Runtime | Low | ISerializer is thread-safe after construction |
| NJsonSchema 11.1.0 | Runtime | Low | JsonSchema.Validate() is thread-safe (read-only schema) |
| TalentCsvLoader | Runtime | Low | Read-only Dictionary after construction -- safe for concurrent reads |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F636-F643 Per-character conversion | MEDIUM | Will use --parallel flag. Parallel mode is additive (opt-in), sequential mode remains default |
| Program.cs CLI | LOW | Needs --parallel flag parsing. Additive change |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml/BatchConverter.cs | Update | Replace sequential foreach with Parallel.ForEachAsync when parallel mode enabled. Add thread-safe result aggregation (lock or ConcurrentBag). Add maxDegreeOfParallelism parameter |
| tools/ErbToYaml/IBatchConverter.cs | Update | Add ConvertParallelAsync method or add optional parallelism parameter to ConvertAsync |
| tools/ErbToYaml/Program.cs | Update | Parse --parallel flag, pass to BatchConverter |
| tools/ErbToYaml/BatchReport.cs | Potentially Update | May need thread-safe variant or keep as-is with external synchronization |
| tools/ErbToYaml.Tests/BatchConverterTests.cs | Update | Add parallel-specific tests (thread safety, result correctness, error handling under parallelism) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| BatchReport is not thread-safe | BatchReport.cs (mutable properties + List) | MEDIUM - Must use lock, ConcurrentBag, or post-aggregation pattern to safely collect results |
| Console.Error.WriteLine in DatalistConverter/FileConverter | DatalistConverter.cs, FileConverter.cs | LOW - Console is thread-safe in .NET, but output may interleave. Acceptable for warning messages |
| File system contention on same output directory | Directory.CreateDirectory called per file | LOW - CreateDirectory is idempotent. No risk of collision since each file writes to unique output path |
| Parallel.ForEachAsync requires .NET 6+ | ErbToYaml.csproj targets net10.0 | LOW - Already satisfied |
| F634 must be [DONE] before F635 can start | Predecessor dependency | RESOLVED - F634 is [DONE] |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Race condition in BatchReport result aggregation | Medium | High | Use lock around BatchReport mutations, or collect results in ConcurrentBag and aggregate after all tasks complete |
| Parallel file I/O saturating disk | Low | Low | Use maxDegreeOfParallelism (e.g., Environment.ProcessorCount) to limit concurrency. SSD handles parallel reads well |
| Non-deterministic test failures from thread timing | Low | Medium | Tests should verify result correctness (counts, contents) not execution order. Use deterministic assertions |
| YamlDotNet serializer thread-safety regression | Very Low | High | YamlDotNet ISerializer is documented as thread-safe. If issues arise, create per-thread serializer instances |
| ErbParser instance state leaking between files | Very Low | High | Current code creates new ErbParser() per file in FileConverter.ConvertAsync(). This pattern must be preserved (not shared) |
| Output file collision if two ERB files produce same YAML filename | Very Low | Medium | PathAnalyzer produces unique filenames per file path. Only risk is two ERB files in same directory with same name (impossible in filesystem) |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Add `--parallel` flag to batch conversion" | CLI must parse --parallel flag and pass to BatchConverter | AC#1, AC#2 |
| "Implement thread-safe file processing" | BatchConverter must use Parallel.ForEachAsync when parallel enabled | AC#3, AC#4 |
| "Only BatchReport requires synchronization" | BatchReport mutation must be synchronized (lock/ConcurrentBag) | AC#5 |
| "Aggregate results from parallel conversions" | Parallel results must match sequential results (counts, failures) | AC#6, AC#7 |
| "Handle errors without blocking other conversions" | Error in one file must not prevent other files from converting in parallel mode | AC#8 |
| "All other components are safe for concurrent use" | No additional synchronization on ErbParser, DatalistConverter, etc. | AC#9 |
| "Sequential mode remains default" | --parallel is opt-in; omitting flag uses sequential foreach | AC#10 |
| "maxDegreeOfParallelism" | Parallel execution must limit concurrency | AC#11 |
| Zero technical debt | No TODO/FIXME/HACK markers in modified files | AC#12a,12b,12c |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --parallel flag parsed in Program.cs | code | Grep(tools/ErbToYaml/Program.cs) | contains | "--parallel" | [x] |
| 2 | --parallel flag printed in usage | code | Grep(tools/ErbToYaml/Program.cs) | contains | "Enable parallel" | [x] |
| 3 | Parallel.ForEachAsync used in BatchConverter | code | Grep(tools/ErbToYaml/BatchConverter.cs) | contains | "Parallel.ForEachAsync" | [x] |
| 4 | IBatchConverter has parallel overload or parameter | code | Grep(tools/ErbToYaml/IBatchConverter.cs) | contains | "BatchOptions" | [x] |
| 5 | Thread-safe result aggregation in BatchConverter | code | Grep(tools/ErbToYaml/BatchConverter.cs) | matches | "lock\|ConcurrentBag\|Interlocked" | [x] |
| 6 | Parallel result correctness test passes | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Correct | succeeds | - | [x] |
| 7 | Parallel report counts match expected | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Count | succeeds | - | [x] |
| 8 | Parallel error isolation test passes | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Error | succeeds | - | [x] |
| 9 | No extra synchronization on thread-safe components | code | Grep(tools/ErbToYaml/FileConverter.cs, pattern="\\block\\s*\\(|Interlocked\\.|ConcurrentBag") | count_equals | 0 | [x] |
| 10 | Default mode remains sequential (no --parallel = foreach) | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Sequential.*Default | succeeds | - | [x] |
| 11 | MaxDegreeOfParallelism configured | code | Grep(tools/ErbToYaml/BatchConverter.cs) | contains | "MaxDegreeOfParallelism" | [x] |
| 12a | Zero technical debt in BatchConverter | code | Grep(tools/ErbToYaml/BatchConverter.cs, pattern="TODO\|FIXME\|HACK") | count_equals | 0 | [x] |
| 12b | Zero technical debt in IBatchConverter | code | Grep(tools/ErbToYaml/IBatchConverter.cs, pattern="TODO\|FIXME\|HACK") | count_equals | 0 | [x] |
| 12c | Zero technical debt in Program | code | Grep(tools/ErbToYaml/Program.cs, pattern="TODO\|FIXME\|HACK") | count_equals | 0 | [x] |

### AC Details

**AC#1: --parallel flag parsed in Program.cs**
- Program.cs must detect `--parallel` in args and pass parallelism configuration to BatchConverter
- Test: Grep pattern=`"--parallel"` path=`tools/ErbToYaml/Program.cs`
- Expected: At least one match where --parallel is parsed from args

**AC#2: --parallel flag printed in usage**
- PrintUsage() must document the --parallel option for CLI discoverability
- Test: Grep pattern=`"--parallel"` path=`tools/ErbToYaml/Program.cs`
- Expected: Match in PrintUsage method context (combined with AC#1, minimum 2 matches)
- Note: AC#1 and AC#2 verify different aspects (parsing vs documentation) but both use same pattern on same file

**AC#3: Parallel.ForEachAsync used in BatchConverter**
- BatchConverter must use `Parallel.ForEachAsync` (not `Task.WhenAll` or manual thread pool) as the standard .NET 6+ parallel async iteration API
- Test: Grep pattern=`Parallel.ForEachAsync` path=`tools/ErbToYaml/BatchConverter.cs`
- Expected: At least one match in the parallel execution path
- Edge case: Must be inside a conditional branch (only when parallel mode is enabled)

**AC#4: IBatchConverter has parallel overload or parameter**
- IBatchConverter interface must expose parallelism capability, either via:
  - (a) Optional `bool parallel = false` parameter on ConvertAsync, or
  - (b) Separate `ConvertParallelAsync` method, or
  - (c) `ParallelOptions` or `int maxDegreeOfParallelism` parameter
- Test: Grep pattern=`parallel` (case-insensitive) path=`tools/ErbToYaml/IBatchConverter.cs`
- Expected: At least one match indicating parallel support in interface

**AC#5: Thread-safe result aggregation in BatchConverter**
- BatchReport has mutable `Total`, `Success`, `Failed` (int) and `Failures` (List) properties
- Parallel access to these must be synchronized using one of: `lock`, `ConcurrentBag`, or `Interlocked`
- Test: Grep pattern=`lock|ConcurrentBag|Interlocked` path=`tools/ErbToYaml/BatchConverter.cs`
- Expected: At least one synchronization mechanism present
- Preferred pattern: Collect results in ConcurrentBag<ConversionResult> then aggregate after all tasks complete

**AC#6: Parallel result correctness test passes**
- Unit test verifies that parallel conversion of multiple files produces correct output files
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Correct
- Expected: Test passes, all files converted correctly

**AC#7: Parallel report counts match expected**
- Unit test verifies BatchReport.Total, Success, Failed counts are correct under parallel execution
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Count
- Expected: Counts match expected values (e.g., 3 total, 3 success, 0 failed for valid files)

**AC#8: Parallel error isolation test passes**
- Unit test verifies that one file's error does not block or corrupt other file conversions in parallel mode
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Parallel.*Error
- Expected: Valid files succeed, only invalid file fails, report accurately reflects both

**AC#9: No extra synchronization on thread-safe components**
- FileConverter.cs must NOT add lock/Interlocked/ConcurrentBag since it creates new ErbParser per file and all injected dependencies are thread-safe
- Test: Grep pattern=`lock|Interlocked|ConcurrentBag` path=`tools/ErbToYaml/FileConverter.cs`
- Expected: 0 matches -- no unnecessary synchronization
- Rationale: Adding locks to already-thread-safe code degrades parallel performance

**AC#10: Default mode remains sequential**
- When --parallel is NOT specified, BatchConverter must use the existing sequential `foreach` loop (no behavioral change for existing callers)
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~Sequential.*Default
- Expected: Test passes, confirming sequential path still works correctly

**AC#11: MaxDegreeOfParallelism configured**
- Parallel.ForEachAsync must use `ParallelOptions { MaxDegreeOfParallelism = N }` to limit concurrency
- Test: Grep pattern=`MaxDegreeOfParallelism` path=`tools/ErbToYaml/BatchConverter.cs`
- Expected: At least one match setting MaxDegreeOfParallelism (e.g., `Environment.ProcessorCount`)
- Rationale: Unbounded parallelism can saturate disk I/O and exhaust thread pool

**AC#12a,12b,12c: Zero technical debt**
- No TODO, FIXME, or HACK markers in files modified by F635
- Test: Grep pattern=`TODO|FIXME|HACK` for each file: BatchConverter.cs, IBatchConverter.cs, Program.cs
- Expected: 0 matches per file

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Conditional Execution Path with Parallel.ForEachAsync**

The design adds a parallel execution path to BatchConverter while preserving the existing sequential behavior as the default. The implementation uses .NET's `Parallel.ForEachAsync` API (available since .NET 6) with `ParallelOptions` to control concurrency. Thread-safety is achieved through **lock-based result aggregation** on the existing BatchReport class rather than introducing a new ConcurrentBag-based collection, minimizing API surface changes.

**Key Design Decisions**:
1. **Interface Design**: Add optional `BatchOptions options = null` parameter to existing `IBatchConverter.ConvertAsync()` method with BatchOptions class containing EnableParallel, MaxDegreeOfParallelism, CancellationToken for future extensibility
2. **Synchronization Strategy**: Use `lock (report)` to protect BatchReport mutations (Total, Success, Failed, Failures.Add)
3. **Concurrency Control**: Set `MaxDegreeOfParallelism = Environment.ProcessorCount` to prevent thread pool exhaustion
4. **CLI Integration**: Add `--parallel` flag to Program.cs with single-pass argument parsing

### AC Coverage

| AC# | Design Decision |
|:---:|-----------------|
| 1 | Program.cs: Add `args.Contains("--parallel")` check in `RunBatchModeAsync()`, pass `new BatchOptions { EnableParallel = true }` to `batchConverter.ConvertAsync()` |
| 2 | Program.cs: Add `--parallel` option to `PrintUsage()` method with description "Enable parallel processing of ERB files" |
| 3 | BatchConverter.cs: Wrap existing `foreach` in `if (!enableParallel)` branch, add `else` branch with `await Parallel.ForEachAsync(erbFiles, parallelOptions, async (erbFile, ct) => { ... })` |
| 4 | IBatchConverter.cs: Add `BatchOptions options = null` parameter to `ConvertAsync()` signature. Add `BatchOptions` class with `EnableParallel`, `MaxDegreeOfParallelism`, `CancellationToken` properties |
| 5 | BatchConverter.cs: Wrap all `report.Success++`, `report.Failed++`, `report.Failures.Add()` calls in parallel branch with `lock (report) { ... }` |
| 6 | BatchConverterTests.cs: Add `Test_Parallel_CorrectResults()` - convert 3 valid ERB files in parallel mode, verify all 3 YAML files created with correct content |
| 7 | BatchConverterTests.cs: Add `Test_Parallel_ReportCountsCorrect()` - verify `report.Total == 3, report.Success == 3, report.Failed == 0` after parallel conversion |
| 8 | BatchConverterTests.cs: Add `Test_Parallel_ErrorIsolation()` - convert 3 files (2 valid, 1 invalid) in parallel, verify 2 succeed, 1 fails, report accurate |
| 9 | FileConverter.cs: No changes - already thread-safe (new ErbParser per call, read-only dependencies). Grep verifies no lock/Interlocked/ConcurrentBag added |
| 10 | BatchConverterTests.cs: Add `Test_Sequential_DefaultBehavior()` - call `ConvertAsync()` without `enableParallel` parameter, verify sequential execution (no change in behavior) |
| 11 | BatchConverter.cs: Create `var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };` and pass to `Parallel.ForEachAsync()` |
| 12 | Implementation completes without TODO/FIXME/HACK markers - verified by Grep at completion |

### Key Decisions

| Decision | Rationale | Alternative Considered |
|----------|-----------|----------------------|
| **Lock-based aggregation** (not ConcurrentBag) | Minimizes API changes. BatchReport already exists and is used in 5 places (Program.cs, BatchConverter.cs, tests). Lock contention is low (occurs only on success/failure, not during file I/O which dominates execution time). | ConcurrentBag pattern (collect results in bag, aggregate after Parallel.ForEachAsync completes) would require larger refactor and more complex test setup. Lock is simpler and sufficient for this workload. |
| **BatchOptions parameter** (not separate method) | Preserves backward compatibility. All existing callers (F634 tests, single-file mode) work without changes (null options defaults to sequential). Parallel mode is opt-in via `new BatchOptions { EnableParallel = true }`. | Separate `ConvertParallelAsync()` method would duplicate logic and create two code paths to maintain. Interface bloat with no benefit. |
| **MaxDegreeOfParallelism = Environment.ProcessorCount** | Balances parallelism with system resources. 117 files / 8 cores = ~15 files per core, reducing context switching. Prevents thread pool exhaustion on high file counts. | Unbounded parallelism (`MaxDegreeOfParallelism = -1`) risks saturating disk I/O and exhausting thread pool on large batches. Fixed value (e.g., 4) ignores system capabilities. |
| **Parallel.ForEachAsync** (not Task.WhenAll) | Standard .NET API for parallel async iteration. Handles partitioning, throttling, and exception aggregation automatically. Available in .NET 6+ (target framework is .NET 10). | `Task.WhenAll(erbFiles.Select(async file => { ... }))` creates all tasks upfront (no throttling), risking memory pressure with 117 tasks. Manual partitioning with `Partitioner.Create()` adds complexity without benefit. |
| **Single-pass CLI parsing** (not CommandLineParser library) | Program.cs already uses manual `Array.IndexOf()` parsing for `--batch`, `--talent`, `--schema`. Adding `args.Contains("--parallel")` maintains consistency and avoids external dependency for a boolean flag. | CommandLineParser library (NuGet package) adds 500KB+ dependency for a single flag. Overkill for simple CLI. |
| **No changes to FileConverter** | Thread-safety analysis confirms all components safe (ErbParser instances, read-only dependencies, separate file I/O). Adding unnecessary locks degrades parallel performance. | Adding locks to FileConverter would serialize file processing despite parallel ForEach, defeating the purpose. |
| **Preserved sequential path** | Zero behavioral change for existing callers. All F634 tests pass without modification. Parallel mode is additive, reducing risk. | Removing sequential path would force all callers to handle parallel mode, breaking backward compatibility and requiring F634 retesting. |

### File Changes

| File | Change Description |
|------|-------------------|
| **tools/ErbToYaml/IBatchConverter.cs** | Add `BatchOptions options = null` optional parameter to `ConvertAsync()` signature. Add `BatchOptions` class with `EnableParallel`, `MaxDegreeOfParallelism`, `CancellationToken` properties. Update XML documentation to describe parallel mode behavior. |
| **tools/ErbToYaml/BatchConverter.cs** | Add `BatchOptions options = null` parameter to implementation. Split execution into two branches: (1) `if (!enableParallel)` - existing sequential `foreach` loop, (2) `else` - new `Parallel.ForEachAsync()` with `lock (report)` around all `report` mutations. Extract file processing logic into shared private method `ProcessFileAsync()` to avoid duplication. |
| **tools/ErbToYaml/Program.cs** | Add `bool parallel = args.Contains("--parallel");` in `RunBatchModeAsync()`. Pass `enableParallel: parallel` to `batchConverter.ConvertAsync()`. Add `--parallel` option to `PrintUsage()` with description. |
| **tools/ErbToYaml.Tests/BatchConverterTests.cs** | Add 4 new test methods: `Test_Parallel_CorrectResults()`, `Test_Parallel_ReportCountsCorrect()`, `Test_Parallel_ErrorIsolation()`, `Test_Sequential_DefaultBehavior()`. Each test creates temp directories with 3 ERB files, invokes BatchConverter with/without parallel mode, verifies output correctness and report accuracy. |

### Implementation Details

#### BatchConverter.cs - Parallel Branch

```csharp
// In ConvertAsync() method
// NOTE: report.Total = erbFiles.Length is set BEFORE this conditional (shared between both paths)
bool enableParallel = options?.EnableParallel ?? false;
if (!enableParallel)
{
    // Existing sequential foreach loop (lines 36-74)
    // No changes to preserve backward compatibility
}
else
{
    // New parallel branch
    var parallelOptions = new ParallelOptions
    {
        MaxDegreeOfParallelism = options?.MaxDegreeOfParallelism ?? Environment.ProcessorCount
    };

    await Parallel.ForEachAsync(erbFiles, parallelOptions, async (erbFile, cancellationToken) =>
    {
        try
        {
            // Compute output path (same as sequential)
            var relativePath = Path.GetRelativePath(inputDirectory, erbFile);
            var relativeDir = Path.GetDirectoryName(relativePath) ?? string.Empty;
            var outputDir = Path.Combine(outputDirectory, relativeDir);

            // Convert file
            var results = await _fileConverter.ConvertAsync(erbFile, outputDir);

            // Check results and update report (synchronized)
            bool anyFailed = results.Any(r => !r.Success);
            lock (report)
            {
                if (anyFailed)
                {
                    report.Failed++;
                    var failure = results.First(r => !r.Success);
                    report.Failures.Add(failure);
                }
                else
                {
                    report.Success++;
                }
            }
        }
        catch (Exception ex)
        {
            // Error handling (synchronized)
            lock (report)
            {
                report.Failed++;
                report.Failures.Add(new ConversionResult(
                    Success: false,
                    FilePath: erbFile,
                    Error: $"Failed to convert {erbFile}: {ex.Message}"
                ));
            }
        }
    });
}
```

#### Program.cs - CLI Parsing

```csharp
// In RunBatchModeAsync() after line 69
bool parallel = args.Contains("--parallel");
var batchOptions = parallel ? new BatchOptions { EnableParallel = true } : null;

// In batchConverter.ConvertAsync() call (line 101)
var report = await batchConverter.ConvertAsync(inputDirectory, outputDirectory, batchOptions);

// In PrintUsage() after line 186
Console.WriteLine("  --parallel   Enable parallel processing of ERB files (faster batch conversion)");
```

#### Test Structure

```csharp
[Fact]
public async Task Test_Parallel_CorrectResults()
{
    // Arrange: 3 valid ERB files in subdirectories
    // Act: ConvertAsync(inputDir, outputDir, new BatchOptions { EnableParallel = true })
    // Assert: 3 YAML files exist with correct content
}

[Fact]
public async Task Test_Parallel_ReportCountsCorrect()
{
    // Arrange: 3 valid ERB files
    // Act: ConvertAsync with parallel mode
    // Assert: report.Total == 3, Success == 3, Failed == 0
}

[Fact]
public async Task Test_Parallel_ErrorIsolation()
{
    // Arrange: 2 valid ERB + 1 invalid ERB
    // Act: ConvertAsync with parallel mode
    // Assert: Total == 3, Success == 2, Failed == 1, Failures.Count == 1
}

[Fact]
public async Task Test_Sequential_DefaultBehavior()
{
    // Arrange: 2 valid ERB files
    // Act: ConvertAsync WITHOUT BatchOptions parameter
    // Assert: Success == 2, verify sequential execution (same as F634)
}
```

### Risk Mitigation

| Risk | Mitigation Strategy |
|------|-------------------|
| Lock contention bottleneck | Lock is held only during counter increments and list append (microseconds). File I/O and parsing dominate execution time (milliseconds), so lock contention is negligible. Profiling with 117 files will validate. |
| Parallel test flakiness | Tests verify outcome correctness (file counts, content, report accuracy), not execution order. No timing-dependent assertions. All file paths are unique (per-file temp directories), eliminating race conditions. |
| Disk I/O saturation | MaxDegreeOfParallelism limits concurrent operations to CPU count (typically 8-16). Modern SSDs handle 500+ IOPS, far exceeding this workload. Each file operation is independent (separate read/write), minimizing contention. |
| YamlDotNet thread-safety regression | YamlDotNet ISerializer is documented as thread-safe after construction. DatalistConverter creates serializer once in constructor (line 94 in F634). If issue arises, fallback is to use ThreadLocal<ISerializer>, but this is unlikely. |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Add --parallel flag parsing and usage documentation to Program.cs | [x] |
| 2 | 4 | Update IBatchConverter interface to support BatchOptions parameter | [x] |
| 3 | 3,5,11 | Implement parallel execution path in BatchConverter with thread-safe result aggregation | [x] |
| 4 | 9 | Verify FileConverter remains unchanged (no unnecessary synchronization) | [x] |
| 5 | 6,7,8,10 | Add parallel mode unit tests to BatchConverterTests | [x] |
| 6 | 12 | Verify zero technical debt in modified files | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | CLI parsing pattern from Program.cs (lines 69-90) | --parallel flag parsing, usage documentation |
| 2 | implementer | sonnet | T2 | Current IBatchConverter.ConvertAsync signature | Updated interface with `BatchOptions options = null` parameter |
| 3 | implementer | sonnet | T3 | Technical Design parallel branch implementation | Parallel.ForEachAsync with lock-based aggregation, MaxDegreeOfParallelism |
| 4 | ac-tester | haiku | T4 | FileConverter.cs analysis | Verification that no locks added (already thread-safe) |
| 5 | implementer | sonnet | T5 | Test structure from Technical Design | 4 test methods covering correctness, counts, error isolation, default behavior |
| 6 | ac-tester | haiku | T6 | Grep pattern from AC#12a,12b,12c | Technical debt verification across 3 modified files |

**Constraints** (from Technical Design):

1. **Lock-based aggregation**: Use `lock (report)` to protect BatchReport mutations. Lock must wrap all `report.Success++`, `report.Failed++`, `report.Failures.Add()` calls in parallel branch.
2. **BatchOptions parameter**: Add `BatchOptions options = null` to existing `IBatchConverter.ConvertAsync()` method (backward compatible). BatchOptions contains EnableParallel, MaxDegreeOfParallelism, CancellationToken for future extensibility.
3. **MaxDegreeOfParallelism**: Must be set to `Environment.ProcessorCount` to limit concurrency.
4. **Parallel.ForEachAsync**: Use standard .NET API (not Task.WhenAll). Available in .NET 10 target framework.
5. **Preserved sequential path**: Wrap existing `foreach` in `if (!enableParallel)` branch. Add `else` branch for parallel path. No changes to sequential logic.
6. **No FileConverter changes**: FileConverter.cs must remain unchanged (already thread-safe with per-file ErbParser instances).

**Pre-conditions**:
- F634 must be [DONE] (BatchConverter, FileConverter, IBatchConverter, Program.cs, all infrastructure exist)
- tools/ErbToYaml project builds successfully
- tools/ErbToYaml.Tests project has at least 1 existing test (to follow test pattern)

**Success Criteria**:
- All 12 ACs pass verification (Grep for code structure, dotnet test for behavior)
- `dotnet build tools/ErbToYaml` succeeds
- `dotnet test tools/ErbToYaml.Tests` succeeds (including new parallel tests)
- Parallel mode (`--parallel`) converts 3 files faster than sequential (manually verified during F636-F643 execution)
- Zero TODO/FIXME/HACK markers in modified files

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback with specific failure mode (e.g., "Parallel mode causes race condition in report aggregation")
3. Create follow-up feature for fix with additional investigation:
   - If lock contention: Measure with profiler, consider ConcurrentBag pattern
   - If test flakiness: Add deterministic timing controls, review Parallel.ForEachAsync guarantees
   - If disk I/O saturation: Lower MaxDegreeOfParallelism to Environment.ProcessorCount / 2

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Review Notes
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} {iter}: {description} -->
- [resolved-skipped] Phase1-Uncertain iter2: T5 maps to AC#6,7,8,10 (4 ACs in one task). Rationale: Test method names in Technical Design map clearly to individual ACs (AC#6→CorrectResults, AC#7→CountsCorrect, AC#8→ErrorIsolation, AC#10→DefaultBehavior). Each test method corresponds to exactly one AC. The grouping is for implementation convenience, not AC conflation.
- [resolved-skipped] Phase2-Maintainability iter6: Lock-based aggregation was chosen over ConcurrentBag 'to minimize API changes'. This creates future debt when BatchReport evolves. ConcurrentBag with post-aggregation is inherently safe for future field additions without lock maintenance. Rationale: Defer to future needs. Current lock approach is simple and performs well for this use case.
- [resolved-applied] Phase2-Maintainability iter6: Interface Design: Optional parameter 'bool enableParallel = false' violates Interface Segregation. Adding maxDegreeOfParallelism, CancellationToken later requires signature changes. Should use BatchOptions parameter object.
- [resolved-skipped] Phase2-Maintainability iter6: T5 maps to AC#6,7,8,10 (4 ACs). Review Notes resolution doesn't override AC:Task 1:1 principle. Should split into T5a-T5d. Rationale: All 4 ACs involve writing test methods to same file (BatchConverterTests.cs) which qualifies as atomic implementation unit per wbs-generator.md line 96.
- [resolved-skipped] Phase2-Maintainability iter6: Philosophy states 'Aggregate results from parallel conversions' but no AC verifies Failures content correctness under parallel execution. AC#8 checks count but not Failures details. Rationale: Philosophy claim concerns aggregation correctness (counts), not individual failure record content which is F634 responsibility.
- [resolved-skipped] Phase2-Maintainability iter6: Parallel branch duplicates sequential logic without shared extraction. Implementation Details should show ProcessFileAsync() method. Rationale: Code duplication is minimal and extraction adds complexity without clear benefit for 2-branch conditional.
- [resolved-skipped] Phase2-Maintainability iter6: T4 dispatches implementer for verification-only task. Should be ac-tester since it's Grep check, no code changes. Rationale: Implementation Contract already assigns ac-tester to T4 Phase 4.
- [resolved-skipped] Phase3-ACValidation iter6: AC#9, AC#12a,12b,12c Method column contains pattern parameter non-standard syntax. Testing skill specifies Method=Grep(path) but count_equals needs pattern specification somewhere. Rationale: Testing skill doesn't specify count_equals pattern location. Current format is functional.
- [resolved-invalid] Phase3-ACValidation iter6: AC#5 uses matches with regex 'lock|ConcurrentBag|Interlocked' - consider clearer approach.
- [resolved-invalid] Phase1-Review iter1: AC#5 matcher 'matches' with regex creates false positives. Invalid: AC#5 targets BatchConverter.cs which will contain 'lock (report)' after implementation. False positive risk is negligible in this small focused file.
- [resolved-skipped] Phase1-Uncertain iter1: Method column format 'Grep(path, pattern=...)' is non-standard vs '{method}'. Feature-template.md doesn't explicitly forbid this format. Style preference rather than rule violation.
- [resolved-applied] Phase1-Review iter2: Technical Design interface contradiction between Approach/Key Decisions/File Changes. Applied: Updated all sections to use BatchOptions consistently.
- [resolved-applied] Phase1-Review iter2: 8 pending review notes unresolved. Applied: Resolved all 8 pending review notes with clear rationale.
- [resolved-invalid] Phase1-Review iter2: T4 agent assignment shows 'implementer' should be 'ac-tester'. Invalid: Implementation Contract already assigns ac-tester to T4 Phase 4. Reviewer misread the file.
- [resolved-skipped] Phase1-Uncertain iter2: AC:Task 1:1 violation T5 maps 4 ACs. Uncertain: While all are test methods in same file (atomic unit), conflicting review notes need resolution. Rationale: wbs-generator.md line 96 permits grouping when ACs target same file (BatchConverterTests.cs). Each AC maps to distinct test method name, maintaining traceability.
- [resolved-skipped] Phase1-Uncertain iter2: Method column format non-standard with embedded patterns. Uncertain: SSOT doesn't define count_equals pattern location. Current format functional but style concern. Rationale: AC Definition Table Method column uses Grep(path) for code-type ACs. Pattern parameter for count_equals matchers is documented in AC Details. Format is functional and consistent across all ACs.
- [resolved-invalid] Phase1-Review iter1: T5 violates AC:Task 1:1 principle by mapping 4 ACs to one task. Invalid: wbs-generator.md line 96 permits grouping when ACs represent same atomic implementation unit (test methods in same file).
- [resolved-invalid] Phase1-Review iter1: Philosophy missing AC for Failures content verification. Invalid: Philosophy 'Aggregate results' concerns count correctness, not individual failure record content which is tested by F634.

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none) | - | - | - | - |

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-27 | /fc | Feature completion: AC/Tasks/Design generated |
| 2026-01-28 06:14 | T2 | START: implementer - Update IBatchConverter interface |
| 2026-01-28 06:14 | T2 | END: SUCCESS - BatchOptions class added with EnableParallel, MaxDegreeOfParallelism, CancellationToken properties. ConvertAsync signature updated. Build passes. AC#4 verified. |
| 2026-01-28 06:16 | T1 | START: implementer - Add --parallel flag parsing and usage documentation |
| 2026-01-28 06:16 | T1 | END: SUCCESS - Program.cs parsing and usage updated. Build passes. AC#1,2 verified. No technical debt. |
| 2026-01-28 | T3 | START/END: SUCCESS - Parallel.ForEachAsync with lock-based aggregation implemented |
| 2026-01-28 | Phase 6 | All 52 tests pass. All 14 ACs verified PASS |
| 2026-01-28 | DEVIATION | ac-static-verifier | AC#5,9 verification | exit code 1 - regex parsing error in tool (matches/count_equals with pipe chars). ACs manually verified PASS |
