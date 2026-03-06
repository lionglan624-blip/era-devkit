# Feature 721: Extract Shared Headless Path Utilities

## Status: [DONE]

## Scope Discipline

When issues arise that are clearly outside the scope of this Feature, apply the Deferred Task Protocol from CLAUDE.md:

| Option | Destination | Requirement |
|:------:|-------------|-------------|
| A | Create F{ID+1} | Task to create Feature in current Feature |
| B | Add to F{existing} | Task to add content OR direct Edit, only if Status ≠ [DONE] |
| C | Add to architecture.md Phase | Verify Phase exists |

**Then record in Handoff section below AND add corresponding Task.**

## Type: engine

## Background

### Philosophy (Mid-term Vision)

Shared utility code should exist in a single location to prevent maintenance drift when implementation details change (e.g., target framework upgrades).

### Problem (Current Issue)

`GetHeadlessDllPath`, `FindHeadlessProjectPath`, and `QuoteArg` are duplicated between `ProcessLevelParallelRunner.cs` and `KojoBatchRunner.cs` with explicit "(Copied from ProcessLevelParallelRunner)" comments. F720 exposed this when a `net8.0` → `net10.0` TFM change required updating both files independently.

### Goal

Extract shared methods into a common static utility class (e.g., `HeadlessPathHelper`) referenced by both `ProcessLevelParallelRunner` and `KojoBatchRunner`.

## Links

- [feature-720.md](feature-720.md) - Discovered during F720 post-review (PRE-EXISTING)
- [feature-064.md](feature-064.md) - Original ProcessLevelParallelRunner implementation
- [feature-088.md](feature-088.md) - ProcessLevelParallelRunner enhancements
- [feature-169.md](feature-169.md) - KojoBatchRunner introduction (copied methods)
- [feature-176.md](feature-176.md) - Kojo batch execution
- [feature-716.md](feature-716.md) - Related engine tests

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### Duplication Origin

The three utility methods were originally implemented in `ProcessLevelParallelRunner.cs` (Feature 064/088) for spawning worker subprocesses. When `KojoBatchRunner.cs` needed the same subprocess-launching capability for `RunSuiteFileInProcess()` (Feature 169), the methods were copied verbatim rather than extracted to a shared location. The copy comments explicitly acknowledge this: `"(Copied from ProcessLevelParallelRunner)"`.

### Duplicated Methods

Three methods are duplicated between:
- **Source**: `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs` (lines 275-328, 1104-1116)
- **Copy**: `engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs` (lines 717-777)

| Method | ProcessLevelParallelRunner | KojoBatchRunner | Difference |
|--------|---------------------------|-----------------|------------|
| `FindHeadlessProjectPath(gamePath)` | Instance method, searches from `gamePath` parent dir with 4 fallback paths | Static method, searches from `gamePath` with 3 relative paths | Different search strategies but same purpose. KojoBatchRunner uses array-based approach; ProcessLevelParallelRunner uses sequential if-else. |
| `GetHeadlessDllPath(projectPath)` | Instance method, dynamic TFM scan with `net10.0` fallback | Static method, identical logic | Functionally identical after F720 fix |
| `QuoteArg(arg)` | Static method, checks for space and double-quote characters | Static method, checks for space and tab characters | **Minor divergence**: ProcessLevelParallelRunner handles double-quote escaping; KojoBatchRunner handles tab detection. Both perform quote-escaping in the replacement. |

### Third Copy in Tests

`engine.Tests/Tests/ProcessLevelParallelRunnerTests.cs` contains a test-local copy (`GetHeadlessDllPathForTest` and `FindHeadlessProjectPath`), bringing the total to **3 copies** of `GetHeadlessDllPath` logic and **3 copies** of `FindHeadlessProjectPath` logic.

### Maintenance Drift Risk

F720 demonstrated the real cost: when the TFM changed from `net8.0` to `net10.0`, both `ProcessLevelParallelRunner.cs` AND `KojoBatchRunner.cs` required independent updates to the same `GetHeadlessDllPath` method. A future TFM change (e.g., `net12.0`) or search-path change would require updating 3 locations.

## Related Features

| Feature | Relation | Description |
|---------|----------|-------------|
| F064 | Origin | Process-level parallel execution - original implementation of all 3 methods |
| F088 | Origin | Pre-built DLL approach - introduced `GetHeadlessDllPath` |
| F169 | Cause | Suite-in-subprocess mode - copied methods to `KojoBatchRunner` |
| F176 | Related | Engine directory structure - affected `FindHeadlessProjectPath` search paths |
| F720 | Trigger | TFM fix (`net8.0` → dynamic) exposed the duplication cost |
| F716 | Neutral | Unit test coverage - test helper also has a copy of the logic |

## Feasibility

**Assessment: FEASIBLE**

This is a straightforward Extract Method refactoring with no behavioral changes:

1. **New file**: Create `HeadlessPathHelper.cs` in `engine/Assets/Scripts/Emuera/Headless/` as a `public static class`
2. **Move methods**: Extract the 3 methods as `public static` methods into the new class
3. **Update callers**: Replace direct method calls in `ProcessLevelParallelRunner` and `KojoBatchRunner` with `HeadlessPathHelper.X()` calls
4. **Unify divergence**: Merge the `QuoteArg` implementations (handle both `" "`, `"\t"`, and `"\""`)
5. **Unify `FindHeadlessProjectPath`**: Merge the two search strategies into one comprehensive implementation
6. **Update tests**: Point `ProcessLevelParallelRunnerTests` to use the shared helper

**Risk**: Near-zero. All methods are pure functions (no state, no side effects). The `#if HEADLESS_MODE` preprocessor directive applies to all files in this directory, so the new file follows the same pattern.

**Effort**: Small (< 1 hour implementation). No new dependencies, no architectural changes.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F720 | [DONE] | F720 fixed the TFM resolution logic that will be extracted |

No blocking dependencies. F720 is already [DONE], so the code to extract is in its final form.

## Impact Analysis

### Files Modified

| File | Change |
|------|--------|
| `engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs` | **NEW** - Shared utility class with 3 static methods |
| `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs` | Remove `FindHeadlessProjectPath`, `GetHeadlessDllPath`, `QuoteArg`; call `HeadlessPathHelper` |
| `engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs` | Remove `FindHeadlessProjectPath`, `GetHeadlessDllPath`, `QuoteArg`; call `HeadlessPathHelper` |
| `engine.Tests/Tests/ProcessLevelParallelRunnerTests.cs` | Optionally update `GetHeadlessDllPathForTest` and `FindHeadlessProjectPath` to use `HeadlessPathHelper` |

### Behavioral Impact

- **Near-zero behavioral change**: All method signatures and caller paths remain identical with one exception
- **Exception**: Fallback path return changes from relative to absolute for improved robustness (both originals return `"../engine/uEmuera.Headless.csproj"`, unified returns absolute path)
- **API surface**: Methods change from `private`/`private static` to `public static` on the helper class
- **No downstream impact**: Callers continue to receive valid project paths

## Constraints

1. **`#if HEADLESS_MODE`**: The new file must be wrapped in `#if HEADLESS_MODE` / `#endif` to match all other files in the Headless directory
2. **Unity `.meta` file**: A corresponding `HeadlessPathHelper.cs.meta` file must be created (Unity requires meta files for all assets)
3. **Namespace**: Must remain in `MinorShift.Emuera.Headless` namespace
4. **No new NuGet dependencies**: Pure `System.IO`, `System.Linq`, `System.Collections.Generic`, and `System.Text.RegularExpressions` usage only
5. **TreatWarningsAsErrors**: The new file and modified files must compile cleanly under `Directory.Build.props` settings

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Unity meta file missing causes build error | Medium | Low | Generate meta file or copy from adjacent file and update GUID |
| `QuoteArg` unification changes edge-case behavior | Low | Low | Write unit tests covering space, tab, and quote-in-path scenarios before merging |
| `FindHeadlessProjectPath` search order change | Low | Low | Preserve both sets of search paths in unified implementation; test from both Game/ and repo-root contexts |
| Test helper in `ProcessLevelParallelRunnerTests` diverges again | Low | Low | Update test to call `HeadlessPathHelper` directly, eliminating the test-local copy |

<!-- fc-phase-3-completed -->
<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extract the three duplicated utility methods (`FindHeadlessProjectPath`, `GetHeadlessDllPath`, `QuoteArg`) from `ProcessLevelParallelRunner.cs` and `KojoBatchRunner.cs` into a new shared static class `HeadlessPathHelper.cs` in the `MinorShift.Emuera.Headless` namespace.

**Key Strategy:**

1. **Create new file**: `engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs` with `#if HEADLESS_MODE` guard and Unity `.meta` file
2. **Unify divergences**: Merge the two `QuoteArg` implementations and the two `FindHeadlessProjectPath` search strategies into comprehensive implementations
3. **Update callers**: Replace method calls in `ProcessLevelParallelRunner.cs` and `KojoBatchRunner.cs` with `HeadlessPathHelper.X()` static calls
4. **Update tests**: Modify `ProcessLevelParallelRunnerTests.cs` to call `HeadlessPathHelper` directly, eliminating the test-local copy

### AC Coverage

| AC# | Satisfied By | Design Element |
|:---:|-------------|----------------|
| 1, 2 | File Creation | Create `HeadlessPathHelper.cs` and `.meta` file in Headless directory |
| 3, 4, 5, 16 | Method Extraction | Define 3 `public static` methods in `MinorShift.Emuera.Headless` namespace |
| 6 | Preprocessor Guard | Wrap entire file in `#if HEADLESS_MODE` / `#endif` |
| 7 | Method Removal | Delete `FindHeadlessProjectPath`, `GetHeadlessDllPath`, `QuoteArg` from `ProcessLevelParallelRunner.cs` |
| 8 | Method Removal | Delete all 3 methods from `KojoBatchRunner.cs` |
| 9, 10 | Caller Update | Replace method invocations with `HeadlessPathHelper.X()` calls |
| 11, 12 | Test Update | Update `ProcessLevelParallelRunnerTests.cs` to call `HeadlessPathHelper` |
| 13, 19 | Behavioral Tests | Unit tests verify QuoteArg and FindHeadlessProjectPath edge cases |
| 14, 15 | Verification | Build and test validate zero behavioral change |
| 17 | Code Quality | No TODO/FIXME markers in new file |
| 18 | Robustness | Fallback path uses absolute resolution |

### Key Decisions

#### 1. QuoteArg Unification Strategy

**Current Divergence:**
- `ProcessLevelParallelRunner.QuoteArg`: Checks for `" "` and `"\""`
- `KojoBatchRunner.QuoteArg`: Checks for `" "` and `"\t"`

**Unified Implementation:**
```csharp
public static string QuoteArg(string arg)
{
    if (string.IsNullOrEmpty(arg))
        return "\"\"";

    // Check for any character requiring quoting: space, tab, or quote
    if (arg.Contains(" ") || arg.Contains("\t") || arg.Contains("\""))
    {
        // Escape quotes and wrap in quotes
        return "\"" + arg.Replace("\"", "\\\"") + "\"";
    }

    return arg;
}
```

**Rationale:** The unified version handles all three cases (space, tab, quote), ensuring safe command-line argument passing in all contexts. This is a superset of both implementations with zero behavioral regression.

#### 2. FindHeadlessProjectPath Unification Strategy

**Current Divergence:**
- `ProcessLevelParallelRunner` (lines 275-302): Searches from `gamePath` parent directory with 4 fallback paths (sequential if-else)
- `KojoBatchRunner` (lines 717-738): Searches from `gamePath` with 3 relative paths (array-based iteration)

**Unified Implementation - Strict Superset Approach with Null Safety and Deduplication:**
```csharp
public static string FindHeadlessProjectPath(string gamePath, string projectFileName = "uEmuera.Headless.csproj")
{
    // Start from parent of gamePath (ProcessLevelParallelRunner pattern)
    string baseDir = Path.GetDirectoryName(Path.GetFullPath(gamePath).TrimEnd(Path.DirectorySeparatorChar));
    string gameDir = Path.GetFullPath(gamePath); // KojoBatchRunner pattern

    // Build candidate paths, skipping any that require null parent directories
    var candidates = new List<string>
    {
        // ProcessLevelParallelRunner paths (baseDir-relative)
        Path.Combine(baseDir, "engine", projectFileName),
        Path.Combine(baseDir, "uEmuera", projectFileName),

        // KojoBatchRunner paths (gameDir-relative)
        Path.Combine(gameDir, "..", "engine", projectFileName),
        Path.Combine(gameDir, "..", "uEmuera", projectFileName),
        Path.Combine(gameDir, "..", "..", "engine", projectFileName),
    };

    // Additional baseDir-parent-relative paths (originally CWD-relative in ProcessLevelParallelRunner,
    // resolved to baseDir-parent for deterministic behavior independent of process CWD)
    string baseDirParent = Path.GetDirectoryName(baseDir);
    if (baseDirParent != null)
    {
        candidates.Add(Path.Combine(baseDirParent, "..", "engine", projectFileName));
        candidates.Add(Path.Combine(baseDirParent, "..", "uEmuera", projectFileName));
    }

    // Deduplicate resolved paths to avoid redundant File.Exists checks
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var candidate in candidates)
    {
        string resolved = Path.GetFullPath(candidate);
        if (seen.Add(resolved) && File.Exists(resolved))
            return resolved;
    }

    // Fallback - absolute path for robustness (documented behavioral improvement)
    return Path.GetFullPath(Path.Combine(baseDir, "engine", projectFileName));
}
```

**Rationale:** This is a strict superset preserving ALL search paths from both original implementations without dropping any. ProcessLevelParallelRunner's baseDir-relative and CWD-relative paths are preserved alongside KojoBatchRunner's gameDir-relative paths including the two-levels-up search. The only intentional behavioral change is the fallback returning an absolute path for improved robustness in cross-process scenarios.

#### 3. GetHeadlessDllPath - Parameterized DLL Name with Version-Aware Sort

**Current State:** Both implementations are identical after F720 fix (dynamic TFM scan). However, both use `OrderByDescending(d => d)` which sorts lexicographically, causing `net9.0` to be preferred over `net10.0` (since `'9' > '1'`). The unified implementation fixes this pre-existing bug.

**Implementation:** Extract as `public static` method with configurable DLL name and version-aware TFM sorting:
```csharp
public static string GetHeadlessDllPath(string projectPath, string dllName = "uEmuera.Headless.dll")
{
    string projectDir = Path.GetDirectoryName(Path.GetFullPath(projectPath));
    string debugDir = Path.Combine(projectDir, "bin", "Debug");

    if (Directory.Exists(debugDir))
    {
        // Version-aware sort: parse numeric version from TFM directory names (e.g., "net10.0" → 10.0)
        // Fixes pre-existing bug where lexicographic sort preferred net9.0 over net10.0
        var tfmDirs = Directory.GetDirectories(debugDir)
            .Select(d => new { Path = d, Name = System.IO.Path.GetFileName(d) })
            .OrderByDescending(d =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(d.Name, @"net(\d+(?:\.\d+)?)");
                return match.Success ? double.Parse(match.Groups[1].Value) : 0.0;
            });

        foreach (var tfmDir in tfmDirs)
        {
            string dllPath = Path.Combine(tfmDir.Path, dllName);
            if (File.Exists(dllPath))
                return dllPath;
        }
    }

    // Fallback: hardcoded net10.0
    return Path.Combine(projectDir, "bin", "Debug", "net10.0", dllName);
}
```

#### 4. Method Visibility Change

**Current:** All methods are `private` or `private static` in their original classes
**New:** All methods become `public static` in `HeadlessPathHelper`

**Rationale:** Making them public allows callers (`ProcessLevelParallelRunner`, `KojoBatchRunner`, `ProcessLevelParallelRunnerTests`) to invoke them. Since these are pure utility functions (no state, no side effects), there's no encapsulation risk.

#### 5. Test Update Strategy

**Current:** `ProcessLevelParallelRunnerTests.cs` has local copies:
- `FindHeadlessProjectPath()` (lines 489-515)
- `GetHeadlessDllPathForTest()` (lines 522-540)

**Update:** The test's `FindHeadlessProjectPath()` uses upward traversal from test execution directory (`Directory.GetCurrentDirectory()`) which is fundamentally different from the production method's `gamePath`-based search. Two approaches:

**Selected Strategy - Pure Delegation:** Replace test-local implementations with simple delegation to HeadlessPathHelper:
```csharp
private string FindHeadlessProjectPath()
{
    // Compute gamePath from test assembly base directory
    // Tests run from: engine.Tests/bin/Debug/net10.0/
    string testBaseDir = AppContext.BaseDirectory;
    string repoRoot = Path.GetFullPath(Path.Combine(testBaseDir, "..", "..", "..", ".."));
    string gamePath = Path.Combine(repoRoot, "Game");

    // Delegate entirely to shared helper - no test-specific search logic
    return HeadlessPathHelper.FindHeadlessProjectPath(gamePath);
}

private string GetHeadlessDllPathForTest(string projectPath)
{
    return HeadlessPathHelper.GetHeadlessDllPath(projectPath);
}
```

**Rationale:** Pure delegation eliminates all duplication. If the standard 4-level navigation fails, the test fails explicitly (rather than silently finding an alternate path), surfacing build structure issues that need investigation. HeadlessPathHelper.FindHeadlessProjectPath already has comprehensive search paths including null-safe parent directory fallbacks - duplicating that logic in test code would defeat the purpose of extraction.

#### 6. Unity .meta File Generation

**Approach:** Copy an adjacent `.meta` file (e.g., `ProcessLevelParallelRunner.cs.meta`) and update the GUID to a new random GUID.

**Format:**
```yaml
fileFormatVersion: 2
guid: <new-random-guid>
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

### Implementation Order

1. **Create `HeadlessPathHelper.cs`** with all 3 unified methods
2. **Generate `.meta` file** with new GUID
3. **Update `ProcessLevelParallelRunner.cs`**:
   - Remove lines 275-328 (`FindHeadlessProjectPath`, instance method)
   - Remove lines 1104-1116 (`QuoteArg`, static method)
   - Remove lines 309-328 (`GetHeadlessDllPath`, instance method)
   - Change field initialization: `headlessProjectPath_ = HeadlessPathHelper.FindHeadlessProjectPath(gamePath);`
   - Change field initialization: `headlessDllPath_ = HeadlessPathHelper.GetHeadlessDllPath(headlessProjectPath_);`
   - Change all `QuoteArg(x)` calls to `HeadlessPathHelper.QuoteArg(x)`
4. **Update `KojoBatchRunner.cs`**:
   - Remove lines 717-777 (all 3 static methods)
   - Change all call sites to `HeadlessPathHelper.X()`
5. **Update `ProcessLevelParallelRunnerTests.cs`**:
   - Change `FindHeadlessProjectPath()` to delegate to `HeadlessPathHelper.FindHeadlessProjectPath(repoRoot + "/Game")`
   - Change `GetHeadlessDllPathForTest()` to delegate to `HeadlessPathHelper.GetHeadlessDllPath()`
6. **Build and test** to verify zero behavioral change

#### 7. Static Class Design Trade-off

**Decision:** HeadlessPathHelper is a static class with public static methods.

**Trade-off (OCP):** Static classes cannot be extended or overridden. If future requirements need different search strategies (e.g., container-aware path resolution), the class itself must be modified.

**Justification:**
- These are pure utility methods with no state - static is the natural choice
- Default parameters (projectFileName, dllName) provide basic customization without inheritance
- The Headless directory already uses static utility patterns (see ProcessLevelParallelRunner static methods)
- Testability is preserved through direct method calls; no mocking needed for pure path operations
- If polymorphism is ever needed, refactor to interface+implementation is straightforward (low debt)

### Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Unity meta file missing | Generate from template with new GUID before build |
| `QuoteArg` unification changes behavior | Cover space, tab, and quote-in-path test cases; unified impl is superset |
| `FindHeadlessProjectPath` search order regression | Unified impl preserves both search strategies; test from both Game/ and repo-root contexts |
| Test helper diverges again | Test helpers delegate to shared helper, eliminating independent implementation |

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "single location" | All 3 methods must exist in exactly one shared class | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6 |
| "single location" | Duplicated methods must be removed from original files | AC#7, AC#8 |
| "single location" | Shared class must have correct namespace for single location | AC#16 |
| "prevent maintenance drift" | Callers must reference the shared helper, not local copies | AC#9, AC#10, AC#11, AC#12 |
| "prevent maintenance drift" | Existing functionality must be preserved (build + tests pass) | AC#14, AC#15 |
| "prevent maintenance drift" | Unified methods must handle all original edge cases correctly | AC#13, AC#19 |
| "prevent maintenance drift" | Behavioral improvements must be consistently applied | AC#18 |
| "prevent maintenance drift" | New file must be clean of technical debt markers | AC#17 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | HeadlessPathHelper.cs exists | file | Glob(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | exists | engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs | [x] |
| 2 | HeadlessPathHelper.cs.meta exists | file | Glob(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs.meta) | exists | engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs.meta | [x] |
| 3 | HeadlessPathHelper contains FindHeadlessProjectPath | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "public static string FindHeadlessProjectPath\\(" | [x] |
| 4 | HeadlessPathHelper contains GetHeadlessDllPath | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "public static string GetHeadlessDllPath\\(" | [x] |
| 5 | HeadlessPathHelper contains QuoteArg | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "public static string QuoteArg\\(" | [x] |
| 6 | HeadlessPathHelper uses HEADLESS_MODE guard | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "#if HEADLESS_MODE" | [x] |
| 7 | ProcessLevelParallelRunner no longer defines duplicated methods | code | Grep(engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs) | not_matches | "string\\s+(FindHeadlessProjectPath\|GetHeadlessDllPath\|QuoteArg)\\s*\\(" | [x] |
| 8 | KojoBatchRunner no longer defines duplicated methods | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs) | not_matches | "static.*(?:FindHeadlessProjectPath\|GetHeadlessDllPath\|QuoteArg)\\s*\\(" | [x] |
| 9 | ProcessLevelParallelRunner calls HeadlessPathHelper | code | Grep(engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs) | contains | "HeadlessPathHelper." | [x] |
| 10 | KojoBatchRunner calls HeadlessPathHelper | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs) | contains | "HeadlessPathHelper." | [x] |
| 11 | Test file uses HeadlessPathHelper for FindHeadlessProjectPath | code | Grep(engine.Tests/Tests/ProcessLevelParallelRunnerTests.cs) | contains | "HeadlessPathHelper.FindHeadlessProjectPath" | [x] |
| 12 | Test file uses HeadlessPathHelper for GetHeadlessDllPath | code | Grep(engine.Tests/Tests/ProcessLevelParallelRunnerTests.cs) | contains | "HeadlessPathHelper.GetHeadlessDllPath" | [x] |
| 13 | QuoteArg unit tests pass (space, tab, quote handling) | test | dotnet test engine.Tests --filter QuoteArg | succeeds | - | [x] |
| 14 | Engine build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 15 | Engine tests pass | test | dotnet test engine.Tests | succeeds | - | [x] |
| 16 | HeadlessPathHelper namespace correct | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "namespace MinorShift.Emuera.Headless" | [x] |
| 17 | Zero technical debt in new file | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |
| 18 | Fallback path is absolute | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs) | contains | "Path\\.GetFullPath\\(Path\\.Combine\\(" | [x] |
| 19 | FindHeadlessProjectPath unit tests pass (search path coverage) | test | dotnet test engine.Tests --filter FindHeadlessProjectPath | succeeds | - | [x] |

### AC Details

**AC#1: HeadlessPathHelper.cs exists**
- The new shared utility class file must be created in the Headless directory alongside the callers
- Test: Glob pattern matches `engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs`

**AC#2: HeadlessPathHelper.cs.meta exists**
- Unity requires a `.meta` file for every asset file. Missing meta causes Unity build errors
- Test: Glob pattern matches the `.meta` companion file

**AC#3: HeadlessPathHelper contains FindHeadlessProjectPath**
- The unified `FindHeadlessProjectPath` method must be defined as `public static string` and cover search paths from both original implementations
- Test: Grep for `public static string FindHeadlessProjectPath(` in HeadlessPathHelper.cs

**AC#4: HeadlessPathHelper contains GetHeadlessDllPath**
- The unified `GetHeadlessDllPath` method must be defined as `public static string` with version-aware TFM sorting
- Test: Grep for `public static string GetHeadlessDllPath(` in HeadlessPathHelper.cs

**AC#5: HeadlessPathHelper contains QuoteArg**
- The unified `QuoteArg` method must be defined as `public static string` and handle spaces, tabs, AND double-quote characters (merging both implementations)
- Test: Grep for `public static string QuoteArg(` in HeadlessPathHelper.cs

**AC#6: HeadlessPathHelper uses HEADLESS_MODE guard**
- All files in the Headless directory are wrapped in `#if HEADLESS_MODE` / `#endif` preprocessor directives
- The new file must follow this established convention
- Test: Grep for `#if HEADLESS_MODE` in the new file

**AC#7: ProcessLevelParallelRunner no longer defines duplicated methods**
- All three local method definitions must be removed from ProcessLevelParallelRunner.cs: `FindHeadlessProjectPath`, `GetHeadlessDllPath`, and `QuoteArg`
- Test: Grep with regex for method signatures (`string` return type + method name + opening paren); must not match

**AC#8: KojoBatchRunner no longer defines duplicated methods**
- All three local method definitions must be removed from KojoBatchRunner.cs: `FindHeadlessProjectPath`, `GetHeadlessDllPath`, and `QuoteArg`
- Test: Grep for any of the three static method signatures; must not match

**AC#9: ProcessLevelParallelRunner calls HeadlessPathHelper**
- After removing local methods, ProcessLevelParallelRunner must call `HeadlessPathHelper.FindHeadlessProjectPath()`, `HeadlessPathHelper.GetHeadlessDllPath()`, and/or `HeadlessPathHelper.QuoteArg()` as needed
- Test: Grep for `HeadlessPathHelper.` usage

**AC#10: KojoBatchRunner calls HeadlessPathHelper**
- After removing local methods, KojoBatchRunner must call the shared helper
- Test: Grep for `HeadlessPathHelper.` usage

**AC#11: Test file uses HeadlessPathHelper for FindHeadlessProjectPath**
- `ProcessLevelParallelRunnerTests.cs` contains test-local copy of `FindHeadlessProjectPath`. After refactoring, the test should delegate to `HeadlessPathHelper.FindHeadlessProjectPath()` rather than implementing its own search logic
- Test: Grep for `HeadlessPathHelper.FindHeadlessProjectPath` in ProcessLevelParallelRunnerTests.cs confirms delegation to shared helper

**AC#12: Test file uses HeadlessPathHelper for GetHeadlessDllPath**
- `ProcessLevelParallelRunnerTests.cs` contains test-local copy of `GetHeadlessDllPathForTest`. After refactoring, the test should delegate to `HeadlessPathHelper.GetHeadlessDllPath()` to eliminate duplication
- Test: Grep for `HeadlessPathHelper.GetHeadlessDllPath` in ProcessLevelParallelRunnerTests.cs confirms delegation to shared helper

**AC#13: QuoteArg unit tests pass (space, tab, quote handling)**
- Unit tests for the unified `QuoteArg` method must verify correct handling of all edge cases:
  - Space: `"path with spaces"` → `"\"path with spaces\""`
  - Tab: `"path\twith\ttabs"` → `"\"path\twith\ttabs\""`
  - Quote: `"path\"with\"quotes"` → `"\"path\\\"with\\\"quotes\""`
  - Combined (tab AND quote): `"path\t\"mixed\""` → `"\"path\t\\\"mixed\\\"\""`
  - Empty/null: graceful handling without crash
- Test: `dotnet test engine.Tests --filter QuoteArg` exits with code 0

**AC#14: Engine build succeeds**
- The headless engine project must compile cleanly with `TreatWarningsAsErrors=true`
- Test: `dotnet build engine/uEmuera.Headless.csproj` exits with code 0

**AC#15: Engine tests pass**
- All existing engine tests must continue to pass, confirming no behavioral regression
- Test: `dotnet test engine.Tests` exits with code 0

**AC#16: HeadlessPathHelper namespace correct**
- Must use `MinorShift.Emuera.Headless` namespace to match all other files in the directory
- Test: Grep for namespace declaration

**AC#17: Zero technical debt in new file**
- The new file must not contain TODO, FIXME, or HACK markers
- Test: Grep for debt markers in HeadlessPathHelper.cs; must not match

**AC#18: Fallback path is absolute**
- The unified `FindHeadlessProjectPath` must return an absolute path as fallback (via `Path.GetFullPath(Path.Combine(...))`) instead of the original relative `../engine/uEmuera.Headless.csproj`. This ensures robustness in cross-process scenarios where CWD may differ
- Test: Grep for `Path.GetFullPath(Path.Combine(` in HeadlessPathHelper.cs

**AC#19: FindHeadlessProjectPath unit tests pass (search path coverage)**
- Unit tests for the unified `FindHeadlessProjectPath` method must verify correct search path resolution from both Game/ and non-standard directory contexts, null-safe parent directory handling, and deduplication behavior
- Test: `dotnet test engine.Tests --filter FindHeadlessProjectPath` exits with code 0

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3,4,5,6,16,17,18 | Create HeadlessPathHelper.cs in engine/Assets/Scripts/Emuera/Headless/ with all 3 unified methods (FindHeadlessProjectPath, GetHeadlessDllPath, QuoteArg) as public static, wrapped in #if HEADLESS_MODE, in MinorShift.Emuera.Headless namespace, zero debt markers, with absolute fallback path | [x] |
| 2 | 2 | Create HeadlessPathHelper.cs.meta file with new GUID for Unity asset management | [x] |
| 3 | 7,9 | Remove FindHeadlessProjectPath, GetHeadlessDllPath, and QuoteArg from ProcessLevelParallelRunner.cs and update all call sites to use HeadlessPathHelper.X() | [x] |
| 4 | 8,10 | Remove FindHeadlessProjectPath, GetHeadlessDllPath, and QuoteArg from KojoBatchRunner.cs and update all call sites to use HeadlessPathHelper.X() | [x] |
| 5 | 11,12 | Update ProcessLevelParallelRunnerTests.cs to delegate FindHeadlessProjectPath() and GetHeadlessDllPathForTest() to HeadlessPathHelper | [x] |
| 6 | 13 | Write QuoteArg unit tests covering space, tab, quote, empty/null input, and combined scenarios | [x] |
| 7 | 19 | Write FindHeadlessProjectPath unit tests covering search path resolution, null-safe parent, and deduplication | [x] |
| 8 | 14,15 | Build engine/uEmuera.Headless.csproj and run dotnet test engine.Tests to verify zero behavioral change | [x] |

## Implementation Contract

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Technical Design (unified QuoteArg, FindHeadlessProjectPath, GetHeadlessDllPath implementations), Unity .meta file template | HeadlessPathHelper.cs and .meta file with 3 public static methods |
| 2 | implementer | sonnet | ProcessLevelParallelRunner.cs, line ranges to remove, HeadlessPathHelper call patterns | Updated ProcessLevelParallelRunner.cs with method removal and HeadlessPathHelper calls |
| 3 | implementer | sonnet | KojoBatchRunner.cs, line ranges to remove, HeadlessPathHelper call patterns | Updated KojoBatchRunner.cs with method removal and HeadlessPathHelper calls |
| 4 | implementer | sonnet | ProcessLevelParallelRunnerTests.cs, test helper delegation pattern | Updated test file with HeadlessPathHelper delegation |
| 5 | implementer | sonnet | QuoteArg behavioral test cases (space, tab, quote, empty/null, combined) | HeadlessPathHelperTests.cs with QuoteArg unit tests |
| 6 | implementer | sonnet | FindHeadlessProjectPath test cases (search paths, null parent, deduplication) | HeadlessPathHelperTests.cs with FindHeadlessProjectPath unit tests |
| 7 | ac-tester | haiku | All ACs (file existence, method count, build/test success) | AC verification results (binary pass/fail) |

## Review Notes

- [resolved-applied] Phase1-Uncertain iter1: 19 ACs exceeds the engine type recommended range of 8-15. Several ACs are granular implementation detail checks (AC#11-14 checking individual Contains/Replace calls in QuoteArg) that could be consolidated. → Consolidated AC#11-14 into single behavioral test AC#11.
- [resolved-applied] Phase2-Maintainability iter1: 19 ACs exceeds the engine type recommended range of 8-15. AC#11-14 are granular implementation detail checks (individual Contains/Replace calls in QuoteArg) that verify HOW the code is written rather than WHAT it achieves. → Same consolidation fix.
- [resolved-applied] Phase2-Maintainability iter1: FindHeadlessProjectPath null-safety and path deduplication. → Added null-check for Path.GetDirectoryName(baseDir) and HashSet-based deduplication to Technical Design.
- [resolved-applied] Phase2-Maintainability iter1: Test hardcoded 4-level navigation fragility. → Added upward traversal fallback to test FindHeadlessProjectPath wrapper in Technical Design.
- [resolved-applied] Phase2-Maintainability iter1: HeadlessPathHelper hardcoded file names. → Added default-value parameters (projectFileName, dllName) to FindHeadlessProjectPath and GetHeadlessDllPath.
- [resolved-applied] Phase2-Maintainability iter1: AC#5 fragile count_equals pattern. → Changed to not_matches with precise `string` return type pattern, consistent with AC#6.
- [resolved-applied] Phase2-Maintainability iter1: Missing unit tests for unified methods. → Added AC#11 (QuoteArg tests) and AC#17 (FindHeadlessProjectPath tests) with corresponding Tasks.
- [resolved-applied] Phase3-ACValidation iter1: Missing behavioral tests for unified methods. → Same fix: AC#11 and AC#17 add behavioral/negative test coverage.
- [resolved-applied] Phase3-ACValidation iter1: count_equals Known Limitation. → AC#5 changed to not_matches. AC#3 split into AC#3,4,5 with contains matcher (fully automatable).
- [resolved-applied] Phase3-ACValidation iter1: These ACs test implementation syntax (specific Contains/Replace calls) rather than behavioral outcomes. → Same consolidation fix as above.
- [resolved-applied] Phase1-Holistic iter5: AC#3 used Type=code with Matcher=count_equals but ac-static-verifier only supports count_equals for Type=file. → Split AC#3 into 3 separate ACs (AC#3,4,5) each using contains matcher for individual method signatures. Renumbered all subsequent ACs (total now 19).

## Mandatory Handoffs

No deferred items. All scope is covered by the 8 Tasks defined above.

## Execution Log

| Timestamp | Type | Source | Action | Detail |
|-----------|------|--------|--------|--------|
| 2026-02-01 14:35 | DEVIATION | test | FindHeadlessProjectPath_NullParentDirectory_DoesNotCrash | ArgumentNullException when gamePath is root - fixed in debugger attempt 1 |

## Debug Log

### Attempt 1 (2026-02-01, debugger, sonnet)

**Error Type**: TEST_FAIL

**Test**: `FindHeadlessProjectPath_NullParentDirectory_DoesNotCrash`

**Root Cause**: When `gamePath` is a filesystem root (e.g., `C:\`), `Path.GetDirectoryName(...)` returns `null`. The code at line 48-52 attempted to use `baseDir` in `Path.Combine(baseDir, "engine", projectFileName)` without null checking, causing `ArgumentNullException`.

**Fix Applied**: Added null check before using `baseDir` in candidate path construction:
- Lines 48-52: Wrapped baseDir-relative paths in `if (baseDir != null)` block
- Line 62: Added null check before calling `Path.GetDirectoryName(baseDir)`
- Line 79: Changed fallback to use `baseDir ?? gameDir` to handle null baseDir case

**Files Modified**:
- `engine/Assets/Scripts/Emuera/Headless/HeadlessPathHelper.cs`

**Result**: FIXED - All 14 HeadlessPathHelper tests now pass.

**Next Step**: RETRY_TEST - Run full engine test suite to verify no regressions.
