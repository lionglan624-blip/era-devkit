# Feature 687: KojoTestRunner DisplayMode Enhanced Reporting

## Status: [DONE]

## Type: engine

## Created: 2026-01-31

---

## Summary

Enhance KojoTestRunner test output to expose DisplayMode metadata from StructuredOutput (captured via F678 DisplayModeCapture). This enables test validation of DisplayMode behavior without requiring DialogueResult integration.

---

## Background

### Philosophy

Test runners should provide visibility into DisplayMode behavior for YAML dialogue validation without requiring architectural changes to the ERB execution path.

### Problem

KojoTestRunner captures DisplayMode metadata via DisplayModeCapture (F678) into `result.StructuredOutput`, but this data is not exposed in test output or validation expectations. Tests cannot validate DisplayMode behavior.

### Goal

Expose StructuredOutput DisplayMode metadata in:
1. JSON test result output (for programmatic validation)
2. Console test output (for human review)
3. Expect validation matchers (for automated testing)

---

## Notes

- F678 already captures DisplayMode per line during execution
- KojoTestRunner lines 279-283 already populate `result.StructuredOutput`
- Need to expose this data in output formatters and add validation matchers

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Tests cannot validate DisplayMode behavior in KojoTestRunner output
2. Why: DisplayMode metadata exists in `result.StructuredOutput` but is not exposed to test validation
3. Why: KojoExpectValidator has no matchers for DisplayMode (only output_contains, var_equals, etc.)
4. Why: F678 was designed for KojoComparer's ERB-YAML equivalence testing, not for kojo test validation
5. Why: The F678 design focused on DiffEngine.Compare() flow, not on extending the test expectation schema

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Test JSON scenarios cannot verify DisplayMode output | KojoExpectValidator lacks displayMode validation matchers |
| DisplayMode metadata is invisible in test reports | KojoTestResultFormatter.WriteCompact/WriteQuiet do not display StructuredOutput |
| Programmatic validation cannot access DisplayMode data | StructuredOutput is serialized to JSON but no expect fields reference it |

### Conclusion

The root cause is a **validation gap**: F678 successfully implemented DisplayMode capture and JSON serialization (`structuredOutput` array in JSON output), but the test validation infrastructure (KojoExpectValidator) was not extended to consume this data. The data flows correctly:

1. `DisplayModeCapture.AddLine()` captures DisplayMode during PRINTDATA execution
2. `KojoTestRunner.RunWithCapture()` populates `result.StructuredOutput` (lines 279-283)
3. `KojoTestResultFormatter.BuildResultObject()` serializes `structuredOutput` to JSON (lines 409-421)

However, the validation path is incomplete:
- `KojoTestExpect` class has no `displayMode_contains`, `displayMode_equals`, etc. fields
- `KojoExpectValidator.Validate()` method does not check DisplayMode expectations
- Test console output (WriteCompact/WriteQuiet) does not display DisplayMode metadata

**Key code analysis**:
- `KojoTestResult.StructuredOutput` (line 78): Already exists with `List<OutputLine>` containing `(Text, DisplayMode)` pairs
- `KojoTestResultFormatter.WriteCompact()` (lines 229-247): Shows branches but not StructuredOutput
- `KojoExpectValidator.Validate()` (lines 57-195): Validates output, stderr, vars, files - no DisplayMode

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F678 | [DONE] | Predecessor | Created DisplayModeCapture infrastructure and StructuredOutput property |
| F684 | [DONE] | Related | DisplayModeConsumer for GUI - different consumer path |
| F686 | [DONE] | Predecessor | Research that identified this feature's integration approach |
| F688 | [DRAFT] | Sibling | InteractiveRunner DisplayMode JSON Response - parallel feature for interactive mode |
| F059 | [DONE] | Related | Original KojoTestRunner implementation - JSON scenario support |
| F174 | [DONE] | Precedent | Added stderr_contains matchers to KojoExpectValidator - same extension pattern |

### Pattern Analysis

This follows the **validation matcher extension** pattern established by F174:
- F174 added `stderr_contains`, `stderr_not_contains`, `stderr_matches`, `stderr_equals` to the KojoExpectValidator
- F687 should follow the same pattern: add `displayMode_contains`, `displayMode_count`, etc.
- The extension points are well-defined: `KojoTestExpect` class + `KojoExpectValidator.Validate()` method

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | StructuredOutput data already exists in KojoTestResult (F678). Validation extension pattern proven by F174 (stderr matchers). |
| Scope is realistic | YES | ~5 files to modify. Following established pattern from F174. No architectural changes needed. |
| No blocking constraints | YES | F678 [DONE] provides DisplayModeCapture infrastructure. All required data flows already exist. |

**Verdict**: FEASIBLE

**Implementation approach (recommended)**:
1. Extend `KojoTestExpect` class with DisplayMode validation fields (e.g., `displayMode_contains`, `displayMode_count`)
2. Extend `KojoExpectValidator.Validate()` with DisplayMode checks (following F174 pattern)
3. Update `KojoTestResultFormatter.WriteCompact()` to display StructuredOutput metadata
4. Update JSON output to include DisplayMode summary in human-readable form

**Scope**:
- Phase 1: Console output enhancement (human-readable DisplayMode in test results)
- Phase 2: Expect validation matchers (JSON scenario DisplayMode assertions)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F678 | [DONE] | DisplayModeCapture infrastructure - creates StructuredOutput |
| Predecessor | F686 | [DONE] | Research that identified this integration approach |
| Related | F684 | [DONE] | DisplayModeConsumer - different consumer path (GUI mode) |
| Related | F688 | [DRAFT] | InteractiveRunner DisplayMode JSON Response - sibling feature |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Text.Json | Runtime | Low | Already used for JSON serialization in KojoTestResultFormatter |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| JSON test scenarios | MEDIUM | Will gain new `displayMode_*` expect fields |
| CI/CD pipelines | LOW | Console output format changes (additive) |
| tools/KojoComparer | NONE | Uses ErbRunner/BatchProcessor, not KojoTestRunner validation |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs | Update | Add `displayMode_contains`, `displayMode_count` fields to KojoTestExpect |
| engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs | Update | Add DisplayMode validation methods (CheckDisplayModeContains, CheckDisplayModeCount) |
| engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs | Update | Add DisplayMode helper methods to KojoTestResultFormatter (WriteDisplayModeInfo) |
| engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs | Update | Add extension methods for DisplayMode analysis (GetDisplayModeSummary, DisplayModeEquals) |
| engine.Tests/Tests/DisplayModeValidationTests.cs | Create | Unit tests for DisplayMode validation matchers |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| HEADLESS_MODE preprocessor directive | engine/uEmuera.Headless.csproj | LOW - All changes are in #if HEADLESS_MODE blocks |
| JSON backward compatibility | Existing test scenarios | LOW - New expect fields are optional; existing scenarios unaffected |
| OutputLine class is defined in DisplayModeCapture.cs | F678 implementation | LOW - Can reference existing class directly |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| DisplayMode validation false positives | Low | Medium | Use line-level matching with configurable tolerance |
| Console output bloat with DisplayMode info | Medium | Low | Add verbosity flag or only show on failure |
| Breaking existing expect field parsing | Low | High | Add new fields as optional; validate JSON schema backward compatibility |
| StructuredOutput may be empty for non-PRINTDATA output | Medium | Low | Handle null/empty gracefully; document limitations |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "provide visibility into DisplayMode behavior" | DisplayMode must be observable in test output | AC#3, AC#4 |
| "without requiring architectural changes" | Use existing StructuredOutput path, no new infrastructure | AC#1, AC#2 |
| "for YAML dialogue validation" | Expect validation matchers must verify DisplayMode | AC#5, AC#6, AC#7 |
| "programmatic validation" (Goal 1) | JSON output includes DisplayMode metadata | AC#3 |
| "human review" (Goal 2) | Console output displays DisplayMode | AC#4 |
| "automated testing" (Goal 3) | Expect matchers for DisplayMode assertions | AC#5, AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | KojoTestExpect has displayMode_contains field | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs) | contains | "displayMode_contains" | [x] |
| 2 | KojoTestExpect has displayMode_count field | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs) | contains | "displayMode_count" | [x] |
| 3 | Console output displays DisplayMode summary | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs) | contains | "WriteDisplayModeInfo" | [x] |
| 4 | KojoExpectValidator validates displayMode_contains | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs) | contains | "CheckDisplayModeContains" | [x] |
| 5 | KojoExpectValidator validates displayMode_count | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs) | contains | "CheckDisplayModeCount" | [x] |
| 6 | DisplayMode validation handles empty StructuredOutput gracefully | test | dotnet test engine.Tests --filter "FullyQualifiedName~DisplayModeValidation&FullyQualifiedName~Empty" | succeeds | - | [x] |
| 7 | Unit tests for DisplayMode matchers pass | test | dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeValidation | succeeds | - | [x] |
| 8 | Build succeeds with DisplayMode enhancements | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 9 | No technical debt in modified files | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs,engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs,engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs) | not_matches | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1: KojoTestExpect displayMode_contains field**
- Test: Grep pattern="displayMode_contains" path="engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs"
- Expected: Field exists with JsonPropertyName attribute
- Pattern follows F174 stderr_contains precedent: `[JsonPropertyName("displayMode_contains")] public JsonElement? DisplayModeContains { get; set; }`

**AC#2: KojoTestExpect displayMode_count field**
- Test: Grep pattern="displayMode_count" path="engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs"
- Expected: Field exists for counting DisplayMode occurrences
- Format: `{ "displayMode": "MODE_NAME", "count": N }` validation

**AC#3: Console DisplayMode summary method exists and is called**
- Test: Grep pattern="WriteDisplayModeInfo" path="engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs"
- Expected: Method exists and is called from WriteCompact() to display DisplayMode distribution
- Format: `"  DisplayMode: NORMAL(5), SPLIT(2), SKIPPAGE(1)"`
- Note: Verifies implementation exists, not runtime output format

**AC#4: KojoExpectValidator CheckDisplayModeContains**
- Test: Grep pattern="CheckDisplayModeContains" path="engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs"
- Expected: Validation method checking if specified DisplayMode values exist in StructuredOutput
- Follows F174 pattern: CheckStderrContains → CheckDisplayModeContains

**AC#5: KojoExpectValidator CheckDisplayModeCount**
- Test: Grep pattern="CheckDisplayModeCount" path="engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs"
- Expected: Validation method checking if DisplayMode appears exact count times
- Example: `{ "displayMode_count": [{ "mode": "SPLIT", "count": 3 }] }` → verifies exactly 3 SPLIT lines

**AC#6: Empty StructuredOutput handling**
- Test: dotnet test engine.Tests --filter "FullyQualifiedName~DisplayModeValidation&FullyQualifiedName~Empty"
- Expected: Tests verify that displayMode_contains/displayMode_count with empty StructuredOutput:
  - displayMode_contains: Fails (expected mode not found)
  - displayMode_count with count=0: Passes (0 occurrences confirmed)
  - displayMode_count with count>0: Fails (expected N, got 0)

**AC#7: DisplayMode validation unit tests**
- Test: dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeValidation
- Expected: Minimum 3 test methods:
  - TestDisplayModeContains_Found
  - TestDisplayModeContains_NotFound
  - TestDisplayModeCount_ExactMatch
- Test file: engine.Tests/Tests/DisplayModeValidationTests.cs

**AC#8: Build verification**
- Test: dotnet build engine/uEmuera.Headless.csproj
- Expected: Build succeeds with all modifications within #if HEADLESS_MODE blocks

**AC#9: Zero technical debt**
- Test: Grep pattern="TODO\|FIXME\|HACK" path="engine/Assets/Scripts/Emuera/Headless/"
- Expected: 0 matches in feature scope files

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Follow the **validation matcher extension pattern** established by F174 (stderr matchers). The implementation extends KojoTestRunner's existing DisplayMode capture infrastructure (F678) by adding:

1. **New expect fields** in `KojoTestExpect` class for DisplayMode validation (`displayMode_contains`, `displayMode_count`)
2. **Validation methods** in `KojoExpectValidator` to check DisplayMode expectations against `result.StructuredOutput`
3. **Console output enhancement** in `KojoTestResultFormatter` to display DisplayMode summary for human review
4. **JSON output preservation** - verify F678's existing `structuredOutput` serialization remains intact

**Data flow** (already exists via F678):
```
PRINTDATA execution
  → DisplayModeCapture.AddLine(text, displayMode)  [F678]
  → KojoTestRunner.RunWithCapture() populates result.StructuredOutput  [F678]
  → KojoTestResultFormatter.BuildResultObject() serializes to JSON  [F678]
  → NEW: KojoExpectValidator validates DisplayMode expectations
  → NEW: KojoTestResultFormatter displays DisplayMode summary in console
```

**Key design decision**: Use `result.StructuredOutput` (already populated by F678) as the single source of truth for DisplayMode validation. This avoids duplicating capture logic and maintains consistency with existing JSON output.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `[JsonPropertyName("displayMode_contains")] public JsonElement? DisplayModeContains { get; set; }` to KojoTestExpect class, following F174 stderr_contains pattern |
| 2 | Add `[JsonPropertyName("displayMode_count")] public JsonElement? DisplayModeCount { get; set; }` to KojoTestExpect class, adjacent to AC#1 field |
| 3 | Add `WriteDisplayModeInfo()` helper method in KojoTestResultFormatter, called from WriteCompact() after WriteBranchInfo() |
| 4 | Add `CheckDisplayModeContains()` method in KojoExpectValidator, following CheckStderrContains pattern |
| 5 | Add `CheckDisplayModeCount()` method in KojoExpectValidator, using count on StructuredOutput |
| 6 | Create DisplayModeValidationTests.cs with test cases for empty StructuredOutput handling |
| 7 | Create DisplayModeValidationTests.cs with minimum 3 test methods using DisplayModeConsumerTests.cs pattern |
| 8 | All code within `#if HEADLESS_MODE`. Build succeeds with `dotnet build engine/uEmuera.Headless.csproj` |
| 9 | Zero TODO/FIXME/HACK comments in modified files |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **DisplayMode validation input** | A) Parse output string for DisplayMode markers<br>B) Use result.StructuredOutput from F678<br>C) Add new capture mechanism | **B) Use result.StructuredOutput** | F678 already provides structured (Text, DisplayMode) pairs. Reusing this avoids duplication and ensures consistency with JSON output. Option A would require string parsing fragility, Option C creates technical debt. |
| **Expect field format for displayMode_count** | A) Single integer (count all modes)<br>B) Dictionary<string, int> (mode→count)<br>C) Array of {mode, count} objects | **C) Array of objects** | Allows validating multiple DisplayMode counts in one test (e.g., `[{"mode": "SPLIT", "count": 3}, {"mode": "NORMAL", "count": 5}]`). More flexible than A/B for comprehensive validation. Follows JSON best practices. |
| **Console output placement** | A) Inline with output text<br>B) Separate section after branches<br>C) Only on failure | **B) Separate section after branches** | Consistent with existing WriteCompact() structure (output → expectation results → branches → **DisplayMode**). Non-intrusive. Option A clutters output, Option C limits human review value. |
| **Empty StructuredOutput handling** | A) Pass silently (no DisplayMode expectations)<br>B) Fail if expectations defined<br>C) Warning only | **B) Fail if expectations defined** | TDD principle: Expectations must be verifiable. If test expects DisplayMode but StructuredOutput is empty, it's a test design error (function doesn't use PRINTDATA) or execution failure. Failing fast prevents false positives. |
| **Helper method vs inline logic** | A) Parse displayMode_count in Validate() method<br>B) Create GetDisplayModeCountList() helper in KojoTestExpect<br>C) Create CheckDisplayModeCount() in validator | **B+C) Helper in both classes** | KojoTestExpect.GetDisplayModeCountList() parses JsonElement array (consistent with GetStderrContainsList() pattern). CheckDisplayModeCount() performs validation logic. Separation of concerns: parsing vs validation. |

### Interfaces / Data Structures

**DisplayMode Extension Methods** (added to DisplayModeCapture.cs - same file pattern for types operating on OutputLine):

```csharp
/// <summary>
/// Extension methods for DisplayMode analysis (Feature 687).
/// </summary>
public static class OutputLineExtensions
{
    /// <summary>
    /// Get DisplayMode distribution as formatted string for OutputLine collections.
    /// </summary>
    public static string GetDisplayModeSummary(this List<OutputLine> structuredOutput)
    {
        if (structuredOutput == null || structuredOutput.Count == 0)
            return "Empty StructuredOutput";

        var modeCounts = new Dictionary<string, int>();
        foreach (var line in structuredOutput)
        {
            string mode = line.DisplayMode ?? "none";
            if (!modeCounts.ContainsKey(mode))
                modeCounts[mode] = 0;
            modeCounts[mode]++;
        }

        var parts = new List<string>();
        foreach (var kvp in modeCounts)
        {
            parts.Add($"{kvp.Key}({kvp.Value})");
        }
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Case-insensitive DisplayMode comparison for OutputLine.
    /// </summary>
    public static bool DisplayModeEquals(this OutputLine line, string expectedMode)
    {
        return string.Equals(line.DisplayMode, expectedMode, StringComparison.OrdinalIgnoreCase);
    }
}
```

**New expect fields in KojoTestExpect** (KojoTestScenario.cs):

```csharp
/// <summary>
/// DisplayMode must contain this mode or all modes in array (Feature 687)
/// </summary>
[JsonPropertyName("displayMode_contains")]
public JsonElement? DisplayModeContains { get; set; }

/// <summary>
/// DisplayMode count validation (Feature 687)
/// Format: [{"mode": "SPLIT", "count": 3}]
/// </summary>
[JsonPropertyName("displayMode_count")]
public JsonElement? DisplayModeCount { get; set; }

/// <summary>
/// Get displayMode_contains as a list of strings.
/// </summary>
public List<string> GetDisplayModeContainsList()
{
    return ParseStringOrArray(DisplayModeContains);
}

/// <summary>
/// Get displayMode_count as a list of (mode, expectedCount) tuples.
/// </summary>
public List<(string mode, int count)> GetDisplayModeCountList()
{
    var result = new List<(string, int)>();
    if (!DisplayModeCount.HasValue) return result;

    var el = DisplayModeCount.Value;
    if (el.ValueKind == JsonValueKind.Array)
    {
        foreach (var item in el.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("mode", out var modeEl) &&
                item.TryGetProperty("count", out var countEl) &&
                modeEl.ValueKind == JsonValueKind.String &&
                countEl.ValueKind == JsonValueKind.Number)
            {
                result.Add((modeEl.GetString(), countEl.GetInt32()));
            }
        }
    }
    return result;
}
```

**New validation methods in KojoExpectValidator** (KojoExpectValidator.cs):

```csharp
/// <summary>
/// Check that StructuredOutput contains expected DisplayMode (Feature 687).
/// </summary>
public static ExpectCheckResult CheckDisplayModeContains(List<OutputLine> structuredOutput, string expected)
{
    bool passed = false;
    if (structuredOutput != null)
    {
        foreach (var line in structuredOutput)
        {
            if (line.DisplayModeEquals(expected))
            {
                passed = true;
                break;
            }
        }
    }

    return new ExpectCheckResult
    {
        Passed = passed,
        CheckType = "displayMode_contains",
        Message = passed
            ? $"DisplayMode contains \"{expected}\""
            : $"Expected DisplayMode to contain \"{expected}\"",
        Expected = expected,
        Actual = passed ? null : structuredOutput.GetDisplayModeSummary()
    };
}

/// <summary>
/// Check that DisplayMode appears exact count times (Feature 687).
/// </summary>
public static ExpectCheckResult CheckDisplayModeCount(List<OutputLine> structuredOutput, string mode, int expectedCount)
{
    int actualCount = 0;
    if (structuredOutput != null)
    {
        foreach (var line in structuredOutput)
        {
            if (line.DisplayModeEquals(mode))
                actualCount++;
        }
    }

    bool passed = actualCount == expectedCount;
    return new ExpectCheckResult
    {
        Passed = passed,
        CheckType = "displayMode_count",
        Message = passed
            ? $"DisplayMode \"{mode}\" appears {expectedCount} times"
            : $"Expected DisplayMode \"{mode}\" to appear {expectedCount} times, but got {actualCount}",
        Expected = $"{mode}: {expectedCount}",
        Actual = actualCount == 0 ? structuredOutput.GetDisplayModeSummary() : $"{mode}: {actualCount}"
    };
}

/// <summary>
/// Get DisplayMode distribution summary for error messages (Feature 687).
/// </summary>
private static string GetDisplayModeSummary(List<OutputLine> structuredOutput)
{
    return DisplayModeHelper.GetModeCounts(structuredOutput);
}
```

**Validation integration in KojoExpectValidator.Validate()** (add after stderr validation, before no_errors check):

```csharp
// Check displayMode_contains (Feature 687)
var displayModeContainsList = expect.GetDisplayModeContainsList();
foreach (var expected in displayModeContainsList)
{
    // Need to pass StructuredOutput from KojoTestResult - requires signature change
    var result = CheckDisplayModeContains(structuredOutput, expected);
    results.Add(result);
}

// Check displayMode_count (Feature 687)
var displayModeCountList = expect.GetDisplayModeCountList();
foreach (var (mode, expectedCount) in displayModeCountList)
{
    var result = CheckDisplayModeCount(structuredOutput, mode, expectedCount);
    results.Add(result);
}
```

**CRITICAL**: `KojoExpectValidator.Validate()` signature must be extended to accept `List<OutputLine> structuredOutput` parameter:

```csharp
public static List<ExpectCheckResult> Validate(
    KojoTestExpect expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable,
    List<OutputLine> structuredOutput)  // NEW parameter
```

**Console output helper in KojoTestResultFormatter** (KojoTestResult.cs):

```csharp
/// <summary>
/// Display DisplayMode distribution summary (Feature 687).
/// </summary>
private static void WriteDisplayModeInfo(KojoTestResult result, TextWriter output)
{
    if (result.StructuredOutput == null || result.StructuredOutput.Count == 0)
        return;

    string summary = result.StructuredOutput.GetDisplayModeSummary();
    output.WriteLine($"  DisplayMode: {summary}");
}
```

Call from WriteCompact() after WriteBranchInfo() (line 247):

```csharp
// Show branch info if available
WriteBranchInfo(result, output);

// Show DisplayMode summary (Feature 687)
WriteDisplayModeInfo(result, output);
```

**Unit test structure** (engine.Tests/Tests/DisplayModeValidationTests.cs):

```csharp
#if HEADLESS_MODE
using Xunit;
using MinorShift.Emuera.Headless;
using System.Collections.Generic;

namespace engine.Tests.Tests
{
    public class DisplayModeValidationTests
    {
        [Fact]
        public void TestDisplayModeContains_Found()
        {
            var structuredOutput = new List<OutputLine>
            {
                new OutputLine { Text = "Line 1", DisplayMode = "NORMAL" },
                new OutputLine { Text = "Line 2", DisplayMode = "SPLIT" }
            };

            var result = KojoExpectValidator.CheckDisplayModeContains(structuredOutput, "SPLIT");
            Assert.True(result.Passed);
            Assert.Equal("displayMode_contains", result.CheckType);
        }

        [Fact]
        public void TestDisplayModeContains_NotFound()
        {
            var structuredOutput = new List<OutputLine>
            {
                new OutputLine { Text = "Line 1", DisplayMode = "NORMAL" }
            };

            var result = KojoExpectValidator.CheckDisplayModeContains(structuredOutput, "SKIPPAGE");
            Assert.False(result.Passed);
            Assert.Contains("Expected DisplayMode to contain", result.Message);
        }

        [Fact]
        public void TestDisplayModeCount_ExactMatch()
        {
            var structuredOutput = new List<OutputLine>
            {
                new OutputLine { Text = "L1", DisplayMode = "SPLIT" },
                new OutputLine { Text = "L2", DisplayMode = "NORMAL" },
                new OutputLine { Text = "L3", DisplayMode = "SPLIT" },
                new OutputLine { Text = "L4", DisplayMode = "SPLIT" }
            };

            var result = KojoExpectValidator.CheckDisplayModeCount(structuredOutput, "SPLIT", 3);
            Assert.True(result.Passed);
        }

        [Fact]
        public void TestDisplayModeContains_EmptyStructuredOutput()
        {
            var result = KojoExpectValidator.CheckDisplayModeContains(null, "SPLIT");
            Assert.False(result.Passed);
            Assert.Equal("Empty StructuredOutput", result.Actual);
        }

        [Fact]
        public void TestDisplayModeCount_ZeroCount()
        {
            var structuredOutput = new List<OutputLine>
            {
                new OutputLine { Text = "L1", DisplayMode = "NORMAL" }
            };

            var result = KojoExpectValidator.CheckDisplayModeCount(structuredOutput, "SPLIT", 0);
            Assert.True(result.Passed);
        }

        [Fact]
        public void TestDisplayModeCount_ExpectedButEmpty()
        {
            var result = KojoExpectValidator.CheckDisplayModeCount(null, "SPLIT", 3);
            Assert.False(result.Passed);
            Assert.Contains("but got 0", result.Message);
        }
    }
}
#endif
```

### Implementation Notes

1. **Pattern consistency**: All new code follows F174 (stderr validation) pattern for consistency with existing codebase
2. **Edge case handling**: Empty/null StructuredOutput returns "Empty StructuredOutput" summary instead of throwing exceptions
3. **Backward compatibility**: New expect fields are optional (JsonElement? nullable) - existing tests unaffected
4. **HEADLESS_MODE isolation**: All changes within preprocessor blocks to avoid GUI build contamination
5. **Validation signature change**: Adding `structuredOutput` parameter to Validate() is a **breaking change** - all call sites in KojoTestRunner must be updated to pass `result.StructuredOutput`

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Add displayMode_contains and displayMode_count fields to KojoTestExpected class | [x] |
| 2 | 3,4,5 | Add extension methods to DisplayModeCapture.cs (GetDisplayModeSummary, DisplayModeEquals) | [x] |
| 3 | 4,5 | Add CheckDisplayModeContains and CheckDisplayModeCount validation methods to KojoExpectValidator | [x] |
| 4 | 4,5 | Update KojoExpectValidator.Validate() signature and integrate DisplayMode validation | [x] |
| 5 | 3 | Add WriteDisplayModeInfo helper method to KojoTestResultFormatter | [x] |
| 6 | 6,7 | Create DisplayModeValidationTests.cs with unit tests | [x] |
| 7 | 8 | Build engine with DisplayMode enhancements | [x] |
| 8 | 9 | Verify zero technical debt in modified files | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T5 | Technical Design specifications | KojoTestExpect fields, extension methods, KojoExpectValidator methods, KojoTestResultFormatter helper |
| 2 | implementer | sonnet | T6 | Technical Design test specifications | DisplayModeValidationTests.cs |
| 3 | ac-tester | haiku | T7-T8 | Build and verification commands | Build success, zero debt confirmation |

**Constraints** (from Technical Design):

1. All code must be within `#if HEADLESS_MODE` preprocessor directives
2. Follow F174 stderr validation pattern for consistency (KojoExpectValidator lines 562-575)
3. New expect fields must use JsonElement? nullable type for backward compatibility
4. KojoExpectValidator.Validate() signature change requires updating all call sites in KojoTestRunner
5. Unit tests must follow DisplayModeConsumerTests.cs pattern (F684 reference)

**Pre-conditions**:

- F678 DisplayModeCapture infrastructure is complete ([DONE])
- F686 research identified integration approach ([DONE])
- engine/uEmuera.Headless.csproj builds successfully
- KojoTestRunner.cs lines 279-283 already populate result.StructuredOutput

**Success Criteria**:

1. All 9 ACs marked [x] (PASS)
2. `dotnet build engine/uEmuera.Headless.csproj` succeeds
3. `dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeValidation` succeeds with minimum 6 test methods passing
4. Console output displays DisplayMode summary: `"  DisplayMode: NORMAL(5), SPLIT(2)"`
5. JSON test scenarios can use new expect fields: `"displayMode_contains": "SPLIT"`, `"displayMode_count": [{"mode": "SPLIT", "count": 3}]`

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert`
2. Notify user of rollback with specific failure symptoms
3. Create follow-up feature for fix with additional investigation into:
   - KojoExpectValidator.Validate() call site compatibility
   - DisplayMode null handling edge cases
   - JSON schema backward compatibility verification

**Test Naming Convention**:

Test methods in DisplayModeValidationTests.cs follow `Test{Matcher}{Scenario}` format:
- `TestDisplayModeContains_Found`
- `TestDisplayModeContains_NotFound`
- `TestDisplayModeContains_EmptyStructuredOutput`
- `TestDisplayModeCount_ExactMatch`
- `TestDisplayModeCount_ZeroCount`
- `TestDisplayModeCount_ExpectedButEmpty`

This ensures AC#7 and AC#8 filter patterns match correctly.

**KojoExpectValidator.Validate() Signature Change**:

**Current signature** (before F687):
```csharp
public static List<ExpectCheckResult> Validate(
    KojoTestExpect expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable)
```

**New signature** (F687):
```csharp
public static List<ExpectCheckResult> Validate(
    KojoTestExpect expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable,
    List<OutputLine> structuredOutput)  // NEW parameter
```

**Backward Compatibility**:

The existing 6-parameter overload in KojoExpectValidator (lines 57-65) will be updated to delegate to the new 7-parameter signature with structuredOutput=null:

```csharp
public static List<ExpectCheckResult> Validate(
    KojoTestExpect expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable)
{
    return Validate(expect, output, stderr, errors, warnings, getVariable, null);
}
```

**Call site update** (KojoTestRunner.cs, approximate line 300+):

Find and update all calls to `KojoExpectValidator.Validate()` to pass `result.StructuredOutput` as the 7th argument.

Before:
```csharp
var results = KojoExpectValidator.Validate(
    expect,
    result.Output,
    result.Stderr,
    result.Errors,
    result.Warnings,
    getVariable);
```

After:
```csharp
var results = KojoExpectValidator.Validate(
    expect,
    result.Output,
    result.Stderr,
    result.Errors,
    result.Warnings,
    getVariable,
    result.StructuredOutput);  // NEW argument
```

**Console Output Format**:

WriteDisplayModeInfo() called after WriteBranchInfo() in WriteCompact() produces:
```
  DisplayMode: NORMAL(5), SPLIT(2), SKIPPAGE(1)
```

Only displayed when `result.StructuredOutput` is non-empty. No output for empty StructuredOutput (graceful degradation).

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| ac-static-verifier `not_matches` matcher unsupported (PRE-EXISTING) | A: F704 | ac-static-verifier に not_matches 実装で根本解決 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | /fc | AC/Tasks/Design generated |
| 2026-01-31 | Phase 6 | All 9 ACs PASS, all 8 Tasks PASS |
| 2026-01-31 | DEVIATION | Bash | ac-static-verifier exit 1 | AC#9 not_matches unsupported by tool, manually verified PASS |
| 2026-01-31 | DEVIATION | feature-reviewer | NEEDS_REVISION | 2 minor: duplicate Dependencies, AC Coverage numbering → fixed |

---

## Links

- [F678: DisplayModeCapture](feature-678.md)
- [F684: DisplayModeConsumer](feature-684.md)
- [F686: Research - DisplayMode Integration](feature-686.md)
- [F688: InteractiveRunner DisplayMode](feature-688.md)
- [KojoTestRunner source](../../engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs)
- [KojoExpectValidator source](../../engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs)
- [KojoTestScenario source](../../engine/Assets/Scripts/Emuera/Headless/KojoTestScenario.cs)
- [DisplayModeConsumerTests reference](../../engine.Tests/Tests/DisplayModeConsumerTests.cs)
