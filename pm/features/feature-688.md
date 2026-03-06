# Feature 688: InteractiveRunner DisplayMode JSON Response

## Status: [DONE]

## Type: engine

## Created: 2026-01-31

## Scope Discipline

- **In Scope**: InteractiveRunner DisplayModeCapture integration, InteractiveResponse schema extension, JSON serialization for headless mode clients
- **Out of Scope**: GUI mode DisplayModeConsumer changes, non-headless runners, DisplayModeCapture capture logic modifications
- **Boundaries**: Modifies InteractiveRunner.cs, adds JsonPropertyName attributes to OutputLine class in DisplayModeCapture.cs (serialization-only, no logic change), and adds unit tests. Does not change DisplayModeCapture.cs capture logic or other runners.

---

## Summary

Add StructuredOutput with DisplayMode metadata to InteractiveRunner JSON responses. This enables DisplayMode-aware clients to receive per-line DisplayMode information for YAML dialogue rendering.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F686 | [DONE] | Research feature that identified Option C (leverage DisplayModeCapture) as the integration approach |
| Predecessor | F687 | [DONE] | KojoTestRunner DisplayMode integration - provides reusable OutputLineExtensions (GetDisplayModeSummary, DisplayModeEquals) and validation pattern |
| Predecessor | F678 | [DONE] | DisplayModeCapture infrastructure - provides StartCapture/AddLine/GetLines static API and OutputLine class |
| Related | F684 | [DONE] | DisplayModeConsumer reference implementation for GUI - different consumer path |
| Related | F700 | [DONE] | PRINTDATAW/K/D DisplayMode Variants - enriches DisplayMode capture with variant-specific modes |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Text.Json | Runtime | Low | Already used throughout InteractiveRunner for JSON serialization |
| DisplayModeCapture (F678) | Static API | Low | Thread-static, already stable and tested |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs | LOW | Creates InteractiveRunner instance (line 1400); no direct schema consumption |
| src/tools/node/feature-dashboard (potential) | MEDIUM | Feature Dashboard may consume JSON responses; new field is additive (backward compatible) |
| External interactive clients | MEDIUM | Any client parsing InteractiveResponse JSON will see new optional `structured_output` field |

---

## Background

### Philosophy

DisplayModeCapture (F678) is the single source of truth for per-line display mode metadata. All headless mode runners (KojoTestRunner via F687, InteractiveRunner via F688) should integrate DisplayModeCapture to provide DisplayMode metadata to their consumers, enabling display-style-aware rendering without duplicating capture logic.

### Problem

InteractiveRunner returns plain text output in JSON responses without DisplayMode metadata. Clients cannot distinguish between lines that require wait behavior (DisplayMode.Wait, KeyWait) vs immediate display (DisplayMode.Newline).

### Goal

Extend InteractiveRunner JSON response schema to include StructuredOutput:
1. Add DisplayModeCapture integration to HandleCall/HandleUsercom
2. Include structured_output field in InteractiveResponse
3. Document JSON schema for DisplayMode-aware clients

---

## Notes

- InteractiveRunner should integrate DisplayModeCapture similar to KojoTestRunner
- F687 defines shared infrastructure that F688 can reuse: OutputLineExtensions (GetDisplayModeSummary, DisplayModeEquals) in DisplayModeCapture.cs
- JSON response schema extension needs backward compatibility consideration
- InteractiveResponse already uses `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` pattern for optional fields

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Interactive clients cannot render YAML dialogue with DisplayMode-aware behavior
2. Why: InteractiveRunner JSON responses contain only plain text output without DisplayMode metadata
3. Why: InteractiveRunner's HandleCall/HandleUsercom methods capture output via CaptureOutput() which reads console display lines as plain text strings (line.ToString())
4. Why: InteractiveRunner was designed before F678 DisplayModeCapture existed and was never updated to integrate it
5. Why: F686 research identified the gap and recommended Option C (leverage DisplayModeCapture) but only F687 (KojoTestRunner) was implemented first; F688 (InteractiveRunner) was deferred as a sibling feature

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Interactive clients receive plain text without DisplayMode | InteractiveRunner does not call DisplayModeCapture.StartCapture()/GetLines() |
| JSON responses lack structured_output field | InteractiveResponse class has no property for StructuredOutput |
| Clients cannot distinguish wait vs immediate display lines | HandleCall/HandleUsercom return only Output (string), not per-line DisplayMode metadata |

### Conclusion

The root cause is a **missing integration**: InteractiveRunner's execution path (HandleCall/HandleUsercom) does not use DisplayModeCapture despite the infrastructure being available since F678. The fix is straightforward:

1. **InteractiveResponse needs a `structured_output` field**: Add `List<OutputLine>` or equivalent serializable property with `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` for backward compatibility
2. **HandleCall needs DisplayModeCapture integration**: Call `DisplayModeCapture.StartCapture()` before execution and `DisplayModeCapture.GetLines()` after execution (same pattern as KojoTestRunner lines 257/280)
3. **HandleUsercom needs the same integration**: Parallel change to HandleCall

**Key code analysis**:
- `InteractiveRunner.HandleCall()` (lines 515-753): Executes ERB function, captures output via `CaptureOutput()`, returns `InteractiveResponse` with `Output` (string), `Lines` (int), `DurationMs` (long). No DisplayModeCapture integration.
- `InteractiveRunner.HandleUsercom()` (lines 1162-1340): Same pattern as HandleCall but for USERCOM commands. Same missing integration.
- `InteractiveRunner.CaptureOutput()` (lines 1345-1374): Reads `console_.GetDisplayLinesForuEmuera(i).ToString()` - plain text only, no DisplayMode metadata.
- `KojoTestRunner.RunWithCapture()` (lines 256-283): Reference implementation showing DisplayModeCapture.StartCapture() before execution and GetLines() after - **this is the pattern F688 should follow**.
- `InteractiveResponse` class (lines 176-208): Current schema has no StructuredOutput field. Needs new property.

**Integration pattern from F687 KojoTestRunner** (to replicate in InteractiveRunner):
```
// Before execution:
DisplayModeCapture.StartCapture();

// After execution:
var structuredOutput = DisplayModeCapture.GetLines();
// Assign to response
```

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F687 | [DONE] | Sibling | KojoTestRunner DisplayMode integration - same infrastructure, different runner |
| F678 | [DONE] | Predecessor | DisplayModeCapture API (StartCapture, AddLine, GetLines, OutputLine class) |
| F684 | [DONE] | Related | DisplayModeConsumer for GUI mode - different consumer path |
| F686 | [DONE] | Predecessor | Research that identified Option C approach for both F687 and F688 |
| F700 | [DONE] | Related | PRINTDATAW/K/D DisplayMode Variants - enriches captured DisplayMode values |
| F062 | [DONE] | Related | Original InteractiveRunner implementation (Feature 062) - JSON-RPC protocol |
| F081 | [DONE] | Related | Interactive mode enhancements (UTF-8, input file, setup command) |
| F082 | [DONE] | Related | Interactive mode error handling fixes |

### Pattern Analysis

F688 follows the **runner integration pattern** established by F687:
- F687 integrated DisplayModeCapture into KojoTestRunner's RunWithCapture() method
- F688 must integrate DisplayModeCapture into InteractiveRunner's HandleCall() and HandleUsercom() methods
- The infrastructure (DisplayModeCapture, OutputLine, OutputLineExtensions) is already complete from F678+F687
- The main difference is serialization: F687 uses KojoTestResult (internal), F688 must serialize to JSON via InteractiveResponse

No recurring pattern issues found. This is the final planned consumer of DisplayModeCapture for headless mode runners.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | DisplayModeCapture API is stable (F678 [DONE]). KojoTestRunner integration pattern proven (F687 [DONE]). InteractiveResponse uses System.Text.Json which can serialize OutputLine list. |
| Scope is realistic | YES | ~2 files to modify (InteractiveRunner.cs, possibly a serialization DTO). F687 provides exact code pattern to replicate. Estimated ~50 lines of changes. |
| No blocking constraints | YES | All predecessors [DONE]. DisplayModeCapture is thread-static (safe for InteractiveRunner's single-threaded command loop). No DI changes needed. |

**Verdict**: FEASIBLE

**Key implementation considerations**:
1. **Backward compatibility**: New `structured_output` field must be optional in JSON output. Use `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` (same pattern as existing Output, Error, Vars fields).
2. **JSON serialization**: OutputLine class has `Text` and `DisplayMode` string properties - directly serializable by System.Text.Json. May need `[JsonPropertyName]` attributes for snake_case consistency.
3. **Two response paths**: Both HandleCall (lines 675-681) and HandleUsercom (lines 1272-1278) construct InteractiveResponse and need StructuredOutput. Also the error recovery paths (TargetInvocationException catch blocks at lines 683-721 and 1280-1310) should include StructuredOutput.
4. **DisplayModeCapture lifecycle**: StartCapture before execution, GetLines after execution. GetLines clears the capture automatically.

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs | Update | Add StructuredOutput property to InteractiveResponse; integrate DisplayModeCapture into HandleCall and HandleUsercom |
| engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs | Update | Add JsonPropertyName attributes to OutputLine class for snake_case JSON serialization |
| engine.Tests/Tests/ (new test file) | Create | Unit tests for InteractiveResponse StructuredOutput serialization |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| HEADLESS_MODE preprocessor directive | engine/uEmuera.Headless.csproj | LOW - All InteractiveRunner code is already in #if HEADLESS_MODE block |
| JSON backward compatibility | Existing interactive clients | LOW - New field is optional (WhenWritingNull), existing clients ignore unknown fields |
| DisplayModeCapture is thread-static | F678 design | LOW - InteractiveRunner runs single-threaded command loop, no concurrency issue |
| InteractiveResponse is a simple DTO | F062 design | LOW - Adding a property is non-breaking |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| JSON response size increase | Medium | Low | StructuredOutput only populated for call/usercom; null-ignored otherwise. Typically small (dialogue output is bounded). |
| OutputLine serialization format mismatch | Low | Medium | Use [JsonPropertyName] attributes on OutputLine or create DTO wrapper for consistent snake_case JSON keys |
| DisplayModeCapture state leak between commands | Low | Medium | Call StartCapture at start of each HandleCall/HandleUsercom; GetLines clears state. Reset also clears via DisplayModeCapture.Reset(). |
| Error paths missing StructuredOutput | Medium | Low | Ensure all success/error return paths in HandleCall and HandleUsercom include StructuredOutput from GetLines() |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "should receive DisplayMode metadata" | InteractiveResponse must include structured_output field with per-line DisplayMode | AC#1, AC#2 |
| "proper wait/key-wait behavior and display-style variants" | Each OutputLine must carry Text and DisplayMode properties | AC#2 |
| "DisplayModeCapture integration to HandleCall/HandleUsercom" (Goal 1) | Both HandleCall and HandleUsercom must call StartCapture/GetLines | AC#3, AC#4 |
| "Include structured_output field in InteractiveResponse" (Goal 2) | InteractiveResponse class must have StructuredOutput property | AC#1 |
| "backward compatibility" (Notes, Feasibility) | structured_output must be null-ignored when empty; existing clients unaffected | AC#5 |
| "error recovery paths should include StructuredOutput" (Feasibility) | TargetInvocationException catch blocks must also capture StructuredOutput | AC#6 |
| "Document JSON schema" (Goal 3) | JSON schema documented in feature file or code comments | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | StructuredOutput property has JsonPropertyName attribute | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | contains | "JsonPropertyName(\"structured_output\")" | [x] |
| 2 | StructuredOutput uses List\<OutputLine\> type | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | matches | `List<OutputLine>.*StructuredOutput` | [x] |
| 3 | Both methods integrate DisplayModeCapture.StartCapture | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | count_equals | "DisplayModeCapture.StartCapture()" : 2 | [x] |
| 4 | GetLines results assigned to StructuredOutput property | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | contains | "StructuredOutput = " | [x] |
| 5 | StructuredOutput uses JsonIgnore WhenWritingNull for backward compatibility | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | count_equals | "JsonIgnoreCondition.WhenWritingNull" : 9 | [x] |
| 6 | All execution paths capture StructuredOutput via GetLines | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | count_equals | "DisplayModeCapture.GetLines()" : 8 | [x] |
| 7 | JSON schema documented in code comments | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs) | contains | "JSON format:" | [x] |
| 8 | Unit tests for StructuredOutput serialization pass | test | dotnet test engine.Tests --filter FullyQualifiedName~InteractiveResponseStructuredOutput | succeeds | - | [x] |
| 9 | Build succeeds with DisplayModeCapture integration | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 10 | No technical debt in modified files | code | Grep(engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs,engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs) | not_matches | "TODO|FIXME|HACK" | [x] |
| 11 | OutputLine handles null fields in serialization | test | dotnet test engine.Tests --filter FullyQualifiedName~TestOutputLineWithNullFieldsSerialization | succeeds | - | [x] |

### AC Details

**AC#1: InteractiveResponse has StructuredOutput property**
- Verifies that `InteractiveResponse` class (lines 176-208) has a new `structured_output` JSON property mapped to `StructuredOutput`
- Must use `[JsonPropertyName("structured_output")]` for snake_case JSON consistency with existing fields (output, lines, duration_ms, error, vars)
- This is the core schema extension that Goal 2 requires

**AC#2: StructuredOutput uses List\<OutputLine\> type**
- The property type must be `List<OutputLine>` (from DisplayModeCapture.cs) to carry per-line Text+DisplayMode pairs
- This ensures clients receive the full DisplayMode metadata per line, not just a summary string
- OutputLine class already has `[JsonPropertyName]` attributes from F678 or needs them added for clean serialization

**AC#3: Both methods integrate DisplayModeCapture.StartCapture**
- Both `HandleCall()` and `HandleUsercom()` must call `DisplayModeCapture.StartCapture()` before ERB execution begins
- Placement: after all validation checks, immediately before the execution loop (HandleCall after line 605, HandleUsercom after line 1208)
- Count of 2 StartCapture calls verifies both methods have the integration

**AC#4: GetLines results assigned to StructuredOutput property**
- Verifies that `DisplayModeCapture.GetLines()` result is properly assigned to the `StructuredOutput` property
- This ensures the captured DisplayMode metadata is actually included in the JSON response
- Complements AC#6 which only verifies GetLines is called, not that results are used

**AC#5: Backward compatibility via WhenWritingNull**
- The `StructuredOutput` property must use `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`
- This ensures existing clients that don't expect `structured_output` in JSON will not receive it when null
- Follows the exact same pattern as existing `Output`, `Error`, `ErrorFile`, `Vars` fields in InteractiveResponse
- AC verifies count of 9 WhenWritingNull occurrences (8 existing in entire file + 1 new for StructuredOutput)

**AC#6: All execution paths capture StructuredOutput via GetLines**
- HandleCall has 4 return paths: success (line 675), TargetInvocationException recovery (line 706), CodeEE recovery (line 737), and timeout (line 615)
- HandleUsercom has 4 return paths: success (line 1272), TargetInvocationException recovery (line 1295), CodeEE recovery (line 1324), and timeout (line 1218)
- Count of 8 GetLines calls ensures all paths provide StructuredOutput, including timeout for partial output visibility
- Pure error returns intentionally skip GetLines(), leaving DisplayModeCapture in capturing state. This is safe because the next StartCapture() call clears all state
- Pure error returns (Status="error") may skip StructuredOutput (null is acceptable for errors)

**AC#7: JSON schema documented**
- Code comments on the StructuredOutput property must describe the JSON schema
- AC verifies XML doc comments contain "JSON format:" text documenting the schema
- Comment must explain the structured_output field format with examples

**AC#8: Unit tests for StructuredOutput serialization**
- Create `engine.Tests/Tests/InteractiveResponseStructuredOutputTests.cs`
- Test cases:
  - Serialization with non-null StructuredOutput includes structured_output in JSON
  - Serialization with null StructuredOutput omits structured_output from JSON (backward compatibility)
  - OutputLine Text and DisplayMode serialize correctly
- Follows DisplayModeConsumerTests.cs pattern from F684

**AC#9: Build verification**
- `dotnet build engine/uEmuera.Headless.csproj` must succeed
- All changes within `#if HEADLESS_MODE` preprocessor blocks
- Verifies no compilation errors from DisplayModeCapture integration

**AC#10: Zero technical debt**
- No TODO, FIXME, or HACK comments in InteractiveRunner.cs or DisplayModeCapture.cs
- Ensures clean implementation without deferred work items across all modified files

**AC#11: OutputLine handles null fields in serialization**
- Unit test verifies OutputLine serialization behavior when Text or DisplayMode properties are null
- Expected behavior: null Text or DisplayMode should serialize as JSON null values (e.g., {"text": null, "display_mode": "NORMAL"})
- This is System.Text.Json default behavior - no special null handling attributes needed on OutputLine properties
- Clients should handle null gracefully for partial/interrupted output scenarios

### Goal Coverage Verification

| Goal# | Goal Item | AC Coverage |
|:-----:|-----------|-------------|
| 1 | Add DisplayModeCapture integration to HandleCall/HandleUsercom | AC#3, AC#4, AC#6 |
| 2 | Include structured_output field in InteractiveResponse | AC#1, AC#2, AC#5 |
| 3 | Document JSON schema for DisplayMode-aware clients | AC#7 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Follow the **DisplayModeCapture integration pattern** established by F687 (KojoTestRunner). The implementation extends InteractiveRunner's JSON response schema by adding DisplayMode metadata to two execution paths (HandleCall and HandleUsercom):

1. **New `structured_output` field** in `InteractiveResponse` class to carry `List<OutputLine>` with per-line DisplayMode metadata
2. **DisplayModeCapture integration** in HandleCall/HandleUsercom execution paths (StartCapture before execution, GetLines after execution)
3. **Backward compatibility** via `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` to preserve existing client behavior
4. **Consistent JSON serialization** using snake_case property names to match existing InteractiveResponse fields

**Data flow** (leveraging F678 DisplayModeCapture infrastructure):
```
Interactive client sends call/usercom command
  → HandleCall/HandleUsercom clears outputBuffer_ (line 530/1169)
  → DisplayModeCapture.StartCapture()  [NEW - mirrors F687 line 257]
  → ERB function execution (PRINTDATA calls DisplayModeCapture.AddLine internally)
  → DisplayModeCapture.GetLines()  [NEW - mirrors F687 line 280]
  → Assign to InteractiveResponse.StructuredOutput  [NEW]
  → JSON serialization includes "structured_output" field (if non-null)
  → Client receives DisplayMode metadata for rendering
```

**Key design decision**: Reuse F687's OutputLineExtensions (GetDisplayModeSummary, DisplayModeEquals) from DisplayModeCapture.cs for consistency. No new helper methods needed - the infrastructure is complete.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `[JsonPropertyName("structured_output")] public List<OutputLine> StructuredOutput { get; set; }` to InteractiveResponse class (line 176), following snake_case pattern of existing fields |
| 2 | Property type is `List<OutputLine>` (F678 class) to carry (Text, DisplayMode) pairs per line |
| 3 | Insert `DisplayModeCapture.StartCapture();` in both HandleCall and HandleUsercom after `outputBuffer_.Clear()`, mirroring F687 line 257 |
| 4 | Insert `StructuredOutput = DisplayModeCapture.GetLines()` in all 8 return paths: HandleCall (success, TargetInvocationException recovery, CodeEE recovery, timeout), HandleUsercom (success, TargetInvocationException recovery, CodeEE recovery, timeout) |
| 5 | Add `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` attribute above StructuredOutput property, matching pattern of Output/Error/Vars fields |
| 6 | Call `DisplayModeCapture.GetLines()` in all 8 return paths to provide StructuredOutput in all cases, including timeout for partial output visibility. Count=8 GetLines calls |
| 7 | Add XML doc comment on StructuredOutput property describing JSON schema: `/// <summary>Per-line DisplayMode metadata. JSON: "structured_output": [{"text": "...", "display_mode": "NORMAL"}]</summary>` |
| 8 | Create `engine.Tests/Tests/InteractiveResponseStructuredOutputTests.cs` with 3 test methods: serialize with non-null StructuredOutput, serialize with null (backward compatibility), OutputLine serialization format |
| 9 | Build with `dotnet build engine/uEmuera.Headless.csproj` - all changes within `#if HEADLESS_MODE` |
| 10 | Zero TODO/FIXME/HACK in InteractiveRunner.cs after implementation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **StructuredOutput property type** | A) `List<OutputLine>` (F678 class)<br>B) Custom DTO with JsonPropertyName attributes<br>C) `List<(string, string)>` tuple | **A) List\<OutputLine\>** | OutputLine already exists (F678) with Text and DisplayMode properties. System.Text.Json can serialize this directly. Reusing existing class avoids DTO duplication. Option B creates unnecessary wrapper, Option C lacks named properties for JSON clarity. |
| **JSON property naming** | A) `structured_output` (snake_case)<br>B) `structuredOutput` (camelCase)<br>C) `StructuredOutput` (PascalCase) | **A) structured_output** | Existing InteractiveResponse fields use snake_case (output, lines, duration_ms, error, vars). Consistency required for client parsing. Use `[JsonPropertyName("structured_output")]` attribute. |
| **Backward compatibility strategy** | A) Always serialize StructuredOutput (empty array if null)<br>B) Omit field when null (WhenWritingNull)<br>C) Add version flag to response | **B) Omit when null** | Follows existing pattern (Output, Error, Vars all use WhenWritingNull). Existing clients ignore unknown fields per JSON spec, but omitting null reduces response size and maintains clean schema evolution. |
| **Where to call GetLines** | A) Only in success paths<br>B) Only after final CaptureOutput()<br>C) All return paths (success + error-recovery + timeout) | **C) All return paths** | AC#6 requires count=8 GetLines calls across 8 paths: HandleCall (success + TargetInvocationException recovery + CodeEE recovery + timeout) + HandleUsercom (success + TargetInvocationException recovery + CodeEE recovery + timeout). Even timeout paths should provide StructuredOutput for partial output visibility. GetLines clears state - safe to call multiple times. |
| **OutputLine JSON serialization** | A) Add [JsonPropertyName] to OutputLine class<br>B) Create DTO wrapper in InteractiveRunner.cs<br>C) Use default System.Text.Json serialization | **A) Add attributes to OutputLine** | OutputLine class (DisplayModeCapture.cs) is shared by F687 (KojoTestRunner) and F688 (InteractiveRunner). Adding `[JsonPropertyName("text")]` and `[JsonPropertyName("display_mode")]` ensures consistent JSON format across consumers. Option B creates duplication, Option C produces PascalCase JSON (inconsistent with InteractiveResponse). |
| **Empty StructuredOutput handling** | A) Return empty list when no PRINTDATA<br>B) Return null when no PRINTDATA<br>C) Always return at least one line | **B) Return null** | DisplayModeCapture.GetLines() returns null when capture was never started or already retrieved. For InteractiveRunner, if no PRINTDATA executes, GetLines returns empty list (not null). However, assign directly without null-coalescing - let WhenWritingNull handle omission. Clients can distinguish "no DisplayMode data" (field absent) from "no output lines" (empty array). |

### Interfaces / Data Structures

**InteractiveResponse schema extension** (InteractiveRunner.cs, line 176):

```csharp
/// <summary>
/// Response from interactive commands.
/// </summary>
public class InteractiveResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Output { get; set; }

    [JsonPropertyName("lines")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Lines { get; set; }

    [JsonPropertyName("duration_ms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long DurationMs { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Error { get; set; }

    [JsonPropertyName("line")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorLine { get; set; }

    [JsonPropertyName("file")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ErrorFile { get; set; }

    [JsonPropertyName("vars")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, long?> Vars { get; set; }

    // NEW: Feature 688 - DisplayMode metadata
    /// <summary>
    /// Per-line DisplayMode metadata for YAML dialogue rendering.
    /// JSON format: "structured_output": [{"text": "line text", "display_mode": "NORMAL"}, ...]
    /// Omitted when no DisplayMode data available (backward compatibility).
    /// </summary>
    [JsonPropertyName("structured_output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OutputLine> StructuredOutput { get; set; }
}
```

**OutputLine class serialization update** (DisplayModeCapture.cs, line 13):

```csharp
using System.Text.Json.Serialization;

/// <summary>
/// Output line with display mode metadata.
/// </summary>
public class OutputLine
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("display_mode")]
    public string DisplayMode { get; set; }
}
```

**HandleCall integration** (InteractiveRunner.cs, lines 515-753):

```csharp
private InteractiveResponse HandleCall(string json, int timeoutMs)
{
    // ... existing code ...

    // Clear output buffer for differential capture
    outputBuffer_.Clear();

    // ... validation checks ...

    // Feature 688: Start DisplayMode capture (after all validation)
    DisplayModeCapture.StartCapture();

    // ... execution loop ...

    // TIMEOUT PATH (line 615):
    if (stopwatch.ElapsedMilliseconds >= timeoutMs)
    {
        var structuredOutput = DisplayModeCapture.GetLines();
        return new InteractiveResponse
        {
            Status = "timeout",
            Output = lastCallOutput_,
            Lines = lineCount,
            DurationMs = stopwatch.ElapsedMilliseconds,
            StructuredOutput = structuredOutput  // NEW
        };
    }

    // SUCCESS PATH (line 675):
    var structuredOutput = DisplayModeCapture.GetLines();
    return new InteractiveResponse
    {
        Status = "ok",
        Output = lastCallOutput_,
        Lines = lineCount,
        DurationMs = stopwatch.ElapsedMilliseconds,
        StructuredOutput = structuredOutput  // NEW
    };

    // ERROR-RECOVERY PATH 1 (TargetInvocationException, line 706):
    catch (TargetInvocationException tie)
    {
        if (inner.Message.Contains("予期しないスクリプト終端です"))
        {
            // ... existing code ...
            var structuredOutput = DisplayModeCapture.GetLines();
            return new InteractiveResponse
            {
                Status = "ok",
                Output = lastCallOutput_,
                Lines = lineCount,
                DurationMs = stopwatch.ElapsedMilliseconds,
                StructuredOutput = structuredOutput  // NEW
            };
        }
        // Pure error - no StructuredOutput
        return new InteractiveResponse
        {
            Status = "error",
            Error = inner.Message,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    // ERROR-RECOVERY PATH 2 (CodeEE exception, line 740):
    catch (Exception ex)
    {
        if (ex.Message.Contains("予期しないスクリプト終端です"))
        {
            // ... existing code ...
            var structuredOutput = DisplayModeCapture.GetLines();
            return new InteractiveResponse
            {
                Status = "ok",
                Output = output.TrimEnd('\r', '\n'),
                Lines = lineCount,
                DurationMs = stopwatch.ElapsedMilliseconds,
                StructuredOutput = structuredOutput  // NEW
            };
        }
        // Pure error - no StructuredOutput
        return new InteractiveResponse
        {
            Status = "error",
            Error = ex.Message,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }
}
```

**HandleUsercom integration** (InteractiveRunner.cs, lines 1162-1340):

Parallel changes to HandleCall:
- Add `DisplayModeCapture.StartCapture()` after `outputBuffer_.Clear()` (line 1169)
- Add `StructuredOutput = DisplayModeCapture.GetLines()` to success path (line 1272)
- Add `StructuredOutput = DisplayModeCapture.GetLines()` to error-recovery paths (lines 1293, 1322)

**Unit test structure** (engine.Tests/Tests/InteractiveResponseStructuredOutputTests.cs):

```csharp
#if HEADLESS_MODE
using Xunit;
using MinorShift.Emuera.Headless;
using System.Collections.Generic;
using System.Text.Json;

namespace engine.Tests.Tests
{
    public class InteractiveResponseStructuredOutputTests
    {
        [Fact]
        public void TestStructuredOutputSerializesWhenNonNull()
        {
            var response = new InteractiveResponse
            {
                Status = "ok",
                Output = "test output",
                StructuredOutput = new List<OutputLine>
                {
                    new OutputLine { Text = "Line 1", DisplayMode = "NORMAL" },
                    new OutputLine { Text = "Line 2", DisplayMode = "SPLIT" }
                }
            };

            string json = JsonSerializer.Serialize(response);

            Assert.Contains("\"structured_output\"", json);
            Assert.Contains("\"text\":\"Line 1\"", json);
            Assert.Contains("\"display_mode\":\"NORMAL\"", json);
        }

        [Fact]
        public void TestStructuredOutputOmittedWhenNull()
        {
            var response = new InteractiveResponse
            {
                Status = "ok",
                Output = "test output",
                StructuredOutput = null
            };

            string json = JsonSerializer.Serialize(response);

            Assert.DoesNotContain("structured_output", json);
        }

        [Fact]
        public void TestOutputLineSerializesWithSnakeCase()
        {
            var line = new OutputLine
            {
                Text = "test text",
                DisplayMode = "NORMAL"
            };

            string json = JsonSerializer.Serialize(line);

            Assert.Contains("\"text\":\"test text\"", json);
            Assert.Contains("\"display_mode\":\"NORMAL\"", json);
            // Verify snake_case (not PascalCase)
            Assert.DoesNotContain("\"Text\"", json);
            Assert.DoesNotContain("\"DisplayMode\"", json);
        }
    }
}
#endif
```

### Implementation Notes

1. **Pattern consistency**: All changes mirror F687 (KojoTestRunner) DisplayModeCapture integration for code maintainability
2. **Error path handling**: Both normal-completion error-recovery paths (TargetInvocationException and CodeEE exceptions with "予期しないスクリプト終端です") include StructuredOutput. Pure error returns (unexpected exceptions) omit StructuredOutput (null is acceptable)
3. **GetLines placement**: Call GetLines immediately before constructing InteractiveResponse in each return path. This ensures capture state is cleared even if response construction fails
4. **Backward compatibility**: Existing clients that don't expect `structured_output` will ignore it (JSON spec allows unknown fields). WhenWritingNull ensures the field is absent when null, maintaining clean JSON for clients that validate against strict schemas
5. **HEADLESS_MODE isolation**: All changes within preprocessor blocks. DisplayModeCapture and OutputLine are already headless-only (F678)
6. **No timeout impact**: DisplayModeCapture.StartCapture/GetLines are O(1) operations with negligible overhead (thread-static list manipulation). No performance regression expected
7. **OutputLine JsonPropertyName attributes**: Adding [JsonPropertyName] attributes to OutputLine class (F678 file) is a **shared infrastructure change** that benefits both F687 (KojoTestRunner JSON output) and F688 (InteractiveRunner JSON output). This ensures consistent snake_case JSON format across all headless mode consumers

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,5 | Add StructuredOutput property to InteractiveResponse class with JsonPropertyName and WhenWritingNull attributes | [x] |
| 2 | 1,2 | Add JsonPropertyName attributes to OutputLine class for snake_case serialization | [x] |
| 3 | 3,4,6 | Integrate DisplayModeCapture into HandleCall (StartCapture before execution, GetLines in all 4 return paths including timeout) | [x] |
| 4 | 3,4,6 | Integrate DisplayModeCapture into HandleUsercom (StartCapture before execution, GetLines in all 4 return paths including timeout) | [x] |
| 5 | 7 | Add XML doc comment to StructuredOutput property documenting JSON schema | [x] |
| 6 | 8,11 | Create InteractiveResponseStructuredOutputTests.cs with serialization tests | [x] |
| 7 | 9 | Build engine with DisplayModeCapture integration | [x] |
| 8 | 10 | Verify zero technical debt in InteractiveRunner.cs | [x] |
| 9 | 1-11 | AC verification and completion | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T5 | Technical Design specifications for InteractiveResponse schema and DisplayModeCapture integration | InteractiveResponse.StructuredOutput property, OutputLine JsonPropertyName attributes, HandleCall/HandleUsercom integration code, XML doc comments |
| 2 | implementer | sonnet | T6 | Technical Design test specifications | InteractiveResponseStructuredOutputTests.cs |
| 3 | ac-tester | haiku | T7-T8 | Build and verification commands from AC Details | Build success confirmation, zero debt verification |

**Constraints** (from Technical Design):

1. All code must be within `#if HEADLESS_MODE` preprocessor directives
2. Follow F687 pattern: StartCapture before execution, GetLines after execution (same as KojoTestRunner lines 257/280)
3. InteractiveResponse.StructuredOutput uses `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` for backward compatibility
4. HandleCall has 4 return paths requiring GetLines: success (line 675), TargetInvocationException error-recovery (line 706), CodeEE error-recovery (line 740), timeout (line 615)
5. HandleUsercom has 4 return paths requiring GetLines: success (line 1272), TargetInvocationException error-recovery (line 1295), CodeEE error-recovery (line 1324), timeout (line 1218)
6. Total 8 GetLines calls verified by AC#6 count_equals matcher
7. OutputLine class receives `[JsonPropertyName("text")]` and `[JsonPropertyName("display_mode")]` attributes for consistent JSON serialization, requiring `using System.Text.Json.Serialization;` import
8. Pure error paths (Status="error") omit StructuredOutput (null is acceptable)

**Pre-conditions**:

- F678 DisplayModeCapture infrastructure is complete ([DONE])
- F687 KojoTestRunner integration provides reusable OutputLineExtensions pattern ([DONE])
- F686 research identified Option C (leverage DisplayModeCapture) approach ([DONE])
- engine/uEmuera.Headless.csproj builds successfully
- InteractiveRunner.cs exists with HandleCall (lines 515-753) and HandleUsercom (lines 1162-1340)

**Success Criteria**:

1. All 11 ACs marked [x] (PASS)
2. `dotnet build engine/uEmuera.Headless.csproj` succeeds
3. `dotnet test engine.Tests --filter FullyQualifiedName~InteractiveResponseStructuredOutput` succeeds with minimum 4 test methods passing
4. InteractiveResponse JSON serialization includes `"structured_output"` field when non-null, omits when null
5. OutputLine JSON serialization uses snake_case: `{"text": "...", "display_mode": "NORMAL"}`
6. DisplayModeCapture.GetLines() called exactly 8 times (4 in HandleCall, 4 in HandleUsercom)

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert`
2. Notify user of rollback with specific failure symptoms
3. Create follow-up feature for fix with additional investigation into:
   - JSON serialization compatibility with existing interactive clients
   - DisplayModeCapture lifecycle edge cases (GetLines timing, state leaks)
   - Error-recovery path StructuredOutput completeness

**JSON Property Naming Convention**:

InteractiveResponse uses snake_case for all JSON properties:
- `status` (existing)
- `output` (existing)
- `lines` (existing)
- `duration_ms` (existing)
- `error` (existing)
- `vars` (existing)
- `structured_output` (new - F688)

Use `[JsonPropertyName("structured_output")]` attribute on StructuredOutput property for consistency.

**Error Path Classification**:

| Path Type | Example | Include StructuredOutput? |
|-----------|---------|:-------------------------:|
| Success | Normal execution completion | YES |
| Error-recovery | TargetInvocationException with "予期しないスクリプト終端です" message | YES |
| Pure error | Unexpected exceptions, validation failures | NO (null) |

**Test Naming Convention**:

Test methods in InteractiveResponseStructuredOutputTests.cs follow `Test{Feature}{Scenario}` format:
- `TestStructuredOutputSerializesWhenNonNull`
- `TestStructuredOutputOmittedWhenNull`
- `TestOutputLineSerializesWithSnakeCase`
- `TestStructuredOutputEmptyListSerializesAsEmptyArray`
- `TestOutputLineWithNullFieldsSerialization`

This ensures AC#8 filter pattern matches correctly. Includes both positive and negative test cases per ENGINE.md requirements.

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | /fc Phases 2-5 | Root Cause Analysis, AC Design, Technical Design, WBS generated |
| 2026-01-31 11:24 | Implementation Phase 1 | Tasks T1-T5 completed: StructuredOutput property added, OutputLine JsonPropertyName attributes added, DisplayModeCapture integrated into HandleCall and HandleUsercom (all 8 return paths) |
| 2026-01-31 11:24 | Implementation Phase 2 | Task T6 completed: InteractiveResponseStructuredOutputTests.cs created with 5 test methods, all tests passed |
| 2026-01-31 11:24 | Verification Phase 3 | Tasks T7-T8 completed: Build succeeded, zero technical debt verified |

---

## Review Notes

- [resolved-applied] Phase1 iter2: Timeout return paths exist in both HandleCall (line 615) and HandleUsercom (line 1218) that return InteractiveResponse without calling DisplayModeCapture.GetLines(). Fixed by including timeout paths in AC#6 (count=8) and updating all related documentation to include timeout paths for partial output visibility.
- [resolved-applied] Phase1 iter2: AC#3 (HandleCall StartCapture), AC#4 (HandleUsercom GetLines), and AC#11 (HandleUsercom StartCapture) all use 'contains' matcher on the entire InteractiveRunner.cs file. Fixed by consolidating AC#3/AC#11 into single count_equals matcher for StartCapture (count=2) and changing AC#4 to verify assignment to StructuredOutput property.
- [resolved-applied] Phase1 iter4: AC#10 uses matcher 'not_contains' with Expected 'TODO\|FIXME\|HACK'. Fixed by changing to 'not_matches' matcher with 'TODO|FIXME|HACK' pattern per ENGINE.md Issue 39.
- [resolved-applied] Phase1 iter5: AC#5 uses `matches` matcher with pattern `WhenWritingNull.*\n.*StructuredOutput` which requires cross-line regex matching. Fixed by changing to 'contains' matcher with 'JsonIgnoreCondition.WhenWritingNull' to verify the attribute exists.
- [resolved-applied] Phase1 iter5: AC#7 verifies `structured_output` appears in InteractiveRunner.cs using `contains` matcher, but this also matches the `JsonPropertyName("structured_output")` attribute (verified by AC#1). Fixed by changing Expected to 'JSON format:' to uniquely identify the XML doc comment.
- [resolved-applied] Phase1 iter6: ENGINE.md requires positive AND negative tests, but AC#8 test cases only included positive scenarios. Fixed by adding TestOutputLineWithNullFieldsSerialization negative test case and corresponding AC#11.

---

## Links

- [F687: KojoTestRunner DisplayMode Enhanced Reporting](feature-687.md)
- [F678: DisplayModeCapture](feature-678.md)
- [F684: DisplayModeConsumer](feature-684.md)
- [F686: Research - DisplayMode Integration](feature-686.md)
- [F700: PRINTDATAW/K/D DisplayMode Variants](feature-700.md)
- [F062: Original InteractiveRunner implementation](feature-062.md)
- [F081: Interactive mode enhancements](feature-081.md)
- [F082: Interactive mode error handling fixes](feature-082.md)
- [InteractiveRunner source](../../engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs)
- [DisplayModeCapture source](../../engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs)
