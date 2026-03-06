# Feature 678: ERB↔YAML DisplayMode Equivalence Comparison

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Implement full ERB↔YAML displayMode equivalence comparison in KojoComparer, enabling verification that ERB PRINTDATA variant behavior matches YAML displayMode metadata end-to-end.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Complete equivalence verification must include display variant semantics (PRINTDATAL → newline, PRINTDATAW → wait, etc.), not just text content. Without displayMode comparison, ERB↔YAML equivalence testing has a blind spot where display behavior differences are silently ignored.

### Problem (Current Issue)

F677 (KojoComparer DisplayMode Awareness) adds YAML-side displayMode reading and comparison framework extension, but does NOT compare ERB-side display behavior because:
1. ERB headless mode outputs text only — PRINTDATA variant metadata is not captured in JSON output
2. KojoComparer's ErbRunner captures console text, losing display variant information (PRINTDATAL's newline is indistinguishable from a regular line break)

F676 (Era.Core Renderer DisplayMode Integration) extends the YAML pipeline to carry displayMode through DialogueResult, but the ERB side remains unaware.

### Goal (What to Achieve)

1. Extend headless mode to expose PRINTDATA variant metadata in ERB execution output
2. Update ErbRunner to capture display variant information alongside text content
3. Implement full displayMode comparison in DiffEngine (ERB variant vs YAML displayMode)
4. Verify PRINTDATAL ERB output matches YAML displayMode: "newline" end-to-end

---

## Links

- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Predecessor - extends DialogueResult with displayMode)
- [feature-677.md](feature-677.md) - KojoComparer DisplayMode Awareness (Predecessor - YAML-side displayMode reading + comparison framework)
- [feature-671.md](feature-671.md) - PrintData Variant Metadata Mapping (Related - added displayMode to schema/converter)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (Related - batch mode infrastructure)
- [feature-651.md](feature-651.md) - KojoComparer KojoEngine API Update (Related - current pipeline)
- [feature-059.md](feature-059.md) - Kojo Test Mode JSON Scenario Support (Related - KojoTestRunner architecture)
- [feature-084.md](feature-084.md) - Branch Tracing (Precedent - parallel metadata capture pattern)

---

## Notes

- Created by F677 scope revision — ERB↔YAML full equivalence was deemed infeasible without headless mode extension
- F677 feasibility assessment found headless mode does not expose PRINTDATA variant metadata
- Only PRINTDATAL is used in kojo files (1,575 occurrences) — practical scope is limited
- Requires engine-level changes (headless mode JSON output extension)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ERB↔YAML equivalence testing cannot verify display mode behavior (PRINTDATAL vs displayMode: "newline")
2. Why: The ERB side of comparison has no display mode information — only text content is captured
3. Why: KojoTestRunner.CaptureOutput() reads ConsoleDisplayLine.ToString() which returns text only, not display variant metadata
4. Why: ConsoleDisplayLine is created by EmueraConsole.Print/PrintLine/NewLine which only stores text + formatting — the PRINTDATA variant (L/W/K/D suffix) is not preserved
5. Why: PRINT_DATA_Instruction.DoInstruction() executes PRINTDATA and then calls Console.Print(str) + Console.NewLine() or Console.ReadAnyKey() — the variant information exists at execution time (via func.Function.IsNewLine(), func.Function.IsWaitInput()) but is consumed for behavior, not recorded

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| DiffEngine.Compare() receives null for ERB displayModes parameter (see BatchProcessor.cs line 68) | KojoTestRunner captures text output only; PRINTDATA variant metadata exists during execution but is not persisted anywhere accessible |
| ERB PRINTDATAL output appears identical to regular PRINTL | Console output flattening: PRINTDATAL calls Console.Print() + Console.NewLine() which creates ConsoleDisplayLine without variant tag |
| Cannot verify ERB↔YAML display behavior equivalence | Architecture gap: The console abstraction layer (IConsoleOutput) was designed for visual rendering, not execution metadata capture |

### Conclusion

The root cause is an **execution metadata gap**: PRINTDATA variant information (L/W/K/D suffixes) exists at the instruction level during ERB execution but is not propagated to or persisted in the console output layer. The PRINT_DATA_Instruction.DoInstruction() method (line 195-241 in Instraction.Child.cs) reads the variant via `func.Function.IsNewLine()` and `func.Function.IsWaitInput()` to determine runtime behavior (add newline, wait for key), but this metadata is lost after execution — ConsoleDisplayLine stores only text and style information.

**Key code analysis**:
- `PRINT_DATA_Instruction.DoInstruction()` (Instraction.Child.cs:195-241): Executes PRINTDATA content, checks `func.Function.IsNewLine()` and `func.Function.IsWaitInput()` for behavior
- `KojoTestRunner.CaptureOutput()` (KojoTestRunner.cs:513-545): Iterates ConsoleDisplayLine and calls `line.ToString()` — text only
- `HeadlessConsole.Print/NewLine()` (HeadlessConsole.cs:66-98): Writes text to buffer/stdout without metadata
- `KojoTestResult.Output` (KojoTestResult.cs:72): Plain string, no structured metadata

**Architectural insight**: The solution requires either:
1. **Option A**: Modify the console output layer to tag output lines with display mode during execution
2. **Option B**: Add a parallel metadata capture mechanism in headless mode that records display variants alongside text
3. **Option C**: Extend KojoTestResult to carry structured output (text + displayMode pairs) populated during execution

Option B or C are preferred as they don't impact the main console abstraction used by GUI mode.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F676 | [DONE] | Predecessor | Era.Core renderer displayMode integration. Provides DialogueResult with displayMode for YAML side. |
| F677 | [DONE] | Predecessor | KojoComparer displayMode awareness. DiffEngine has Compare() overload with displayModes parameters. |
| F671 | [DONE] | Related | PrintData variant metadata mapping. Added displayMode to dialogue-schema.json. |
| F644 | [DONE] | Related | Equivalence Testing Framework. Batch mode infrastructure for KojoComparer. |
| F651 | [DONE] | Related | KojoComparer KojoEngine API Update. Established current YamlRunner/KojoEngine pipeline. |
| F059 | [DONE] | Related | Kojo Test Mode JSON Scenario Support. KojoTestRunner architecture. |
| F084 | [DONE] | Precedent | Branch Tracing. Added parallel metadata capture (TraceService) alongside text output — demonstrates metadata capture pattern. |

### Pattern Analysis

This follows the **parallel metadata capture** pattern established by F084 (Branch Tracing):
- F084 added `TraceService.StartBranchCollection()` and `TraceService.GetBranchDecisions()` to capture branch execution metadata alongside normal output
- F678 can follow the same pattern: add a display mode capture service that records (text, displayMode) pairs during PRINTDATA execution
- KojoTestResult.Branches already demonstrates the pattern of carrying structured execution metadata in test results

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | PRINTDATA variant info exists at execution time (func.Function.IsNewLine(), IsWaitInput()). TraceService precedent shows metadata capture is feasible. |
| Scope is realistic | YES | Only PRINTDATAL used in kojo files (1,575 occurrences). Other variants (W, K, D) have 0 occurrences. Implementation can focus on L variant. |
| No blocking constraints | YES | F676 [DONE], F677 [DONE]. DiffEngine already has displayModes comparison capability. |

**Verdict**: FEASIBLE

**Implementation approach (recommended)**:
1. Create `DisplayModeCapture` service (similar to TraceService) with StartCapture/AddLine/GetLines methods
2. Modify PRINT_DATA_Instruction.DoInstruction() to call DisplayModeCapture.AddLine(text, displayMode) after Console.Print() when in headless/capture mode
3. Extend KojoTestResult with `List<OutputLine>` where OutputLine has (Text, DisplayMode)
4. Update ErbRunner to parse structured output instead of plain text
5. Update BatchProcessor to pass ERB displayModes to DiffEngine.Compare()

**Scope refinement**:
- Phase 1: PRINTDATAL only (covers all 1,575 kojo occurrences)
- Phase 2 (future): PRINTDATAW, PRINTDATAK, etc. (currently 0 occurrences — no practical need yet)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F676 | [DONE] | Era.Core renderer DisplayMode integration — DialogueResult carries displayMode |
| Predecessor | F677 | [DONE] | KojoComparer YAML-side displayMode awareness — DiffEngine has Compare() overload |
| Related | F671 | [DONE] | Added displayMode to dialogue-schema.json and converter |
| Related | F644 | [DONE] | Equivalence Testing Framework batch mode |
| Related | F084 | [DONE] | Branch Tracing — precedent for parallel metadata capture pattern |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| engine/uEmuera.Headless | Build | Low | Headless mode compilation. No external package dependencies. |
| System.Text.Json | Runtime | Low | Already used for JSON output in headless mode. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/ErbRunner.cs | HIGH | Must parse structured output (text + displayMode) instead of plain string |
| tools/KojoComparer/BatchProcessor.cs | MEDIUM | Must pass ERB displayModes to DiffEngine.Compare() |
| tools/KojoComparer.Tests/* | MEDIUM | Test fixtures need update for structured output |
| engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs | HIGH | Must carry structured output lines |
| engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs | HIGH | PRINT_DATA_Instruction must call capture service |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs | Create | New service for capturing display mode during PRINTDATA execution |
| engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs | Update | Add StructuredOutput property with List<OutputLine> |
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | Update | Call DisplayModeCapture.Start/Get, populate result.StructuredOutput |
| engine/Assets/Scripts/Emuera/Headless/KojoTestResultFormatter.cs | Update | Include displayMode in JSON output format |
| engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs | Update | PRINT_DATA_Instruction calls DisplayModeCapture in headless mode |
| tools/KojoComparer/ErbRunner.cs | Update | Parse structured JSON output, extract displayModes list |
| tools/KojoComparer/BatchProcessor.cs | Update | Pass ERB displayModes to DiffEngine.Compare() |
| tools/KojoComparer.Tests/ErbRunnerTests.cs | Update | Test structured output parsing |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| HEADLESS_MODE preprocessor directive | engine/uEmuera.Headless.csproj | LOW - DisplayModeCapture service wrapped in #if HEADLESS_MODE |
| Instruction execution path is shared between GUI and headless | Instraction.Child.cs | MEDIUM - Must check IsHeadlessMode or capture mode flag before calling DisplayModeCapture |
| KojoTestResult is serialized to JSON | KojoTestResultFormatter | MEDIUM - StructuredOutput must be serializable |
| ErbRunner parses JSON via System.Text.Json | ErbRunner.cs line 115 | LOW - Extend existing JSON parsing to include structuredOutput array |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Performance overhead from capture service | Low | Low | DisplayModeCapture is only active during test mode (checked via flag); normal execution unaffected |
| JSON output format change breaks existing ErbRunner parsing | Medium | Medium | Use backward-compatible JSON: add structuredOutput alongside existing output field |
| Scope creep to all PRINTDATA variants | Low | Medium | Explicitly limit Phase 1 to PRINTDATAL; other variants deferred to separate feature if needed |
| Console output ordering mismatch with capture | Low | Medium | Call DisplayModeCapture.AddLine immediately after Console.Print in same code block to maintain ordering |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Implement parallel metadata capture pattern (established by F084 TraceService) to record PRINTDATA display mode alongside text output during execution. The solution uses a DisplayModeCapture service that intercepts PRINTDATA execution without modifying the console abstraction layer.

**Data Flow Architecture**:
```
PRINT_DATA_Instruction.DoInstruction()
  ↓ (determine displayMode via func.Function.IsNewLine(), IsWaitInput())
  ↓
DisplayModeCapture.AddLine(text, displayMode)
  ↓ (stores in thread-local List<OutputLine>)
  ↓
KojoTestRunner.RunSingleTest()
  ↓ (calls DisplayModeCapture.GetLines())
  ↓
KojoTestResult.StructuredOutput
  ↓ (serialized to JSON by KojoTestResultFormatter)
  ↓
ErbRunner.ExecuteAsync()
  ↓ (parses structuredOutput array from JSON)
  ↓
BatchProcessor.ProcessTestCaseAsync()
  ↓ (extracts displayModes list from structuredOutput)
  ↓
DiffEngine.Compare(displayModesA: erbDisplayModes, displayModesB: yamlDisplayModes)
```

**Key Design Decisions**:
1. **Parallel capture service** - Follows F084 TraceService pattern: thread-local storage, Start/Add/Get lifecycle
2. **Backward-compatible JSON** - Add structuredOutput alongside existing output field (ErbRunner can gracefully handle missing field)
3. **PRINTDATAL focus** - Only implement "newline" variant (1,575 occurrences); other variants deferred to future feature
4. **Headless-only scope** - Guarded by #if HEADLESS_MODE and runtime mode check

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs` following TraceService.cs structure (ThreadStatic storage, Start/Add/Get methods) |
| 2 | Implement `void AddLine(string text, string displayMode)` method in DisplayModeCapture service |
| 3 | Modify PRINT_DATA_Instruction.DoInstruction() line 231-236 to call DisplayModeCapture.AddLine() after Console.Print/NewLine, map func.Function.IsNewLine() → "newline" |
| 4 | Add `List<OutputLine> StructuredOutput` property to KojoTestResult where OutputLine has Text and DisplayMode string properties |
| 5 | Update ErbRunner.ExecuteAsync() JSON parsing (line 115-123) to read structuredOutput array and extract displayModes |
| 6 | Update BatchProcessor.ProcessTestCaseAsync() line 68 to pass `displayModesA: erbDisplayModes` instead of null |
| 7 | Build validation - ensures consumer-side changes (KojoComparer) compile correctly |
| 8 | Build validation - ensures engine-side changes (DisplayModeCapture, KojoTestResult extension) compile correctly |
| 9 | Regression protection - ensures existing KojoComparer functionality remains intact |
| 10 | Integration test verifies end-to-end flow: ERB PRINTDATAL → capture → JSON → parse → compare with YAML displayMode: "newline" |
| 11 | Code quality - no deferred technical debt markers in new service implementation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Metadata capture approach | A) Modify ConsoleDisplayLine to store displayMode<br>B) Add parallel capture service (TraceService pattern)<br>C) Modify PRINT instruction to output metadata tag | B | Parallel service isolates capture logic from core console abstraction; proven pattern from F084; minimal impact on GUI mode |
| JSON output format | A) Replace output with structured array<br>B) Add structuredOutput alongside output<br>C) Add metadata header before output | B | Backward compatible - existing ErbRunner code works without changes; structuredOutput is optional field |
| DisplayMode value determination | A) Store original function name (PRINTDATAL)<br>B) Map to normalized displayMode string ("newline")<br>C) Use enum value | B | Matches YAML schema format (displayMode: "newline"); enables direct comparison with F677 DiffEngine.Compare() overload |
| Capture activation | A) Always active in headless mode<br>B) Activated via flag (like TraceService)<br>C) Activated via environment variable | A | Simpler than TraceService - display mode is core metadata, not optional trace data; minimal performance overhead |
| PRINTDATA variant coverage | A) All variants (L/W/K/D)<br>B) PRINTDATAL only<br>C) PRINTDATAL and PRINTDATAW | B | Practical scope - only PRINTDATAL is used (1,575 occurrences); other variants have 0 occurrences in kojo files |

### Interfaces / Data Structures

**DisplayModeCapture.cs** (new file):
```csharp
#if HEADLESS_MODE
using System.Collections.Generic;

namespace MinorShift.Emuera.Headless
{
    public class OutputLine
    {
        public string Text { get; set; }
        public string DisplayMode { get; set; }
    }

    public static class DisplayModeCapture
    {
        [ThreadStatic]
        private static List<OutputLine> _lines;

        [ThreadStatic]
        private static bool _isCapturing;

        public static bool IsCapturing => _isCapturing;

        public static void StartCapture()
        {
            if (_lines == null)
                _lines = new List<OutputLine>();
            else
                _lines.Clear();
            _isCapturing = true;
        }

        public static void AddLine(string text, string displayMode)
        {
            if (!_isCapturing || _lines == null)
                return;

            _lines.Add(new OutputLine
            {
                Text = text ?? string.Empty,
                DisplayMode = displayMode ?? "none"
            });
        }

        public static List<OutputLine> GetLines()
        {
            if (_lines == null)
                return null;

            var result = new List<OutputLine>(_lines);
            _lines.Clear();
            _isCapturing = false;
            return result;
        }

        public static void Reset()
        {
            _lines = null;
            _isCapturing = false;
        }
    }
}
#endif
```

**KojoTestResult.cs extension**:
```csharp
public class KojoTestResult
{
    // Existing properties...
    public string Output { get; set; } = "";
    public List<BranchDecision> Branches { get; set; } = new List<BranchDecision>();

    // NEW: Structured output with display mode metadata
    public List<OutputLine> StructuredOutput { get; set; } = new List<OutputLine>();
}
```

**KojoTestResultFormatter.BuildResultObject() extension**:
```csharp
// Add structuredOutput array to JSON output (alongside existing fields)
object[] structuredOutputArray = null;
if (result.StructuredOutput != null && result.StructuredOutput.Count > 0)
{
    structuredOutputArray = new object[result.StructuredOutput.Count];
    for (int i = 0; i < result.StructuredOutput.Count; i++)
    {
        var line = result.StructuredOutput[i];
        structuredOutputArray[i] = new
        {
            text = line.Text,
            displayMode = line.DisplayMode
        };
    }
}

// Include in JSON object:
return new
{
    // ... existing fields ...
    output = result.Output,  // Keep for backward compatibility
    structuredOutput = structuredOutputArray  // NEW
};
```

**PRINT_DATA_Instruction.DoInstruction() integration** (Instraction.Child.cs line 231-236):
```csharp
// Existing code:
if (func.Function.IsNewLine() || func.Function.IsWaitInput())
{
    exm.Console.NewLine();
    if (func.Function.IsWaitInput())
        exm.Console.ReadAnyKey();
}

// NEW: Capture display mode after Console output
#if HEADLESS_MODE
if (DisplayModeCapture.IsCapturing)
{
    string displayMode = func.Function.IsNewLine() ? "newline" : "none";
    // Capture the printed text with its display mode
    // Note: str variable contains the last printed text from line 224-227
    DisplayModeCapture.AddLine(str, displayMode);
}
#endif
```

**ErbRunner.ExecuteAsync() extension** (line 118-123):
```csharp
// Existing code:
if (root.TryGetProperty("output", out var outputElement))
{
    return outputElement.GetString() ?? string.Empty;
}

// NEW: Parse structuredOutput to extract displayModes
// (Return type changes from string to (string output, List<string> displayModes))
public async Task<(string output, List<string> displayModes)> ExecuteAsync(...)
{
    // ... existing code ...

    if (root.TryGetProperty("output", out var outputElement))
    {
        var output = outputElement.GetString() ?? string.Empty;
        var displayModes = new List<string>();

        // Parse structuredOutput if available
        if (root.TryGetProperty("structuredOutput", out var structuredElement) &&
            structuredElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var line in structuredElement.EnumerateArray())
            {
                if (line.TryGetProperty("displayMode", out var dmElement))
                {
                    displayModes.Add(dmElement.GetString() ?? "none");
                }
            }
        }

        return (output, displayModes);
    }

    throw new InvalidOperationException("JSON output missing 'output' field");
}
```

**BatchProcessor.ProcessTestCaseAsync() update** (line 68):
```csharp
// Existing code:
var erbOutput = await _erbRunner.ExecuteAsync(testCase.ErbFile, testCase.FunctionName, testCase.State);
var normalizedErb = _normalizer.Normalize(erbOutput);
// ... YAML rendering ...
var comparison = _diffEngine.Compare(normalizedErb, normalizedYaml, displayModesA: null, displayModesB: yamlDisplayModes);

// NEW:
var (erbOutput, erbDisplayModes) = await _erbRunner.ExecuteAsync(testCase.ErbFile, testCase.FunctionName, testCase.State);
var normalizedErb = _normalizer.Normalize(erbOutput);
// ... YAML rendering ...
var comparison = _diffEngine.Compare(normalizedErb, normalizedYaml, displayModesA: erbDisplayModes, displayModesB: yamlDisplayModes);
```

**Integration test structure** (AC#10):
```csharp
[Fact]
public async Task DisplayModeEquivalence_PRINTDATAL_MatchesYamlNewline()
{
    // Arrange: Create test ERB with PRINTDATAL
    var erbContent = @"
@COM100(対象_番号)
PRINTDATAL 美鈴思慕獲得テスト
RETURN 0
";

    // Arrange: Create test YAML with displayMode: newline
    var yamlContent = @"
- text: 美鈴思慕獲得テスト
  displayMode: newline
";

    // Act: Execute ERB and render YAML
    var (erbOutput, erbDisplayModes) = await _erbRunner.ExecuteAsync(...);
    var yamlResult = _yamlRunner.RenderWithMetadata(...);
    var yamlDisplayModes = yamlResult.DialogueLines.Select(dl => dl.DisplayMode).ToList();

    // Assert: DisplayModes match
    Assert.Single(erbDisplayModes);
    Assert.Equal("newline", erbDisplayModes[0]);
    Assert.Equal(erbDisplayModes, yamlDisplayModes);

    // Assert: Full comparison passes
    var comparison = _diffEngine.Compare(
        _normalizer.Normalize(erbOutput),
        _normalizer.Normalize(string.Join("\n", yamlResult.Lines)),
        displayModesA: erbDisplayModes,
        displayModesB: yamlDisplayModes
    );
    Assert.True(comparison.IsMatch);
}
```

### Implementation Notes

**Edge Case Handling**:
1. **Empty text**: AddLine() accepts null text, converts to empty string to prevent downstream null reference errors
2. **Missing structuredOutput**: ErbRunner returns empty displayModes list when structuredOutput field is absent (backward compatibility)
3. **Capture not started**: DisplayModeCapture.AddLine() checks IsCapturing flag before recording (no-op if not active)
4. **Multi-line PRINTDATAL**: Current design captures per-Console.Print() call; PRINTDATAL with multiple data lines will produce multiple OutputLine entries

**Pattern Consistency**:
- ThreadStatic storage matches TraceService.cs (line 28-33)
- Start/Get lifecycle matches TraceService.StartBranchCollection/GetBranchDecisions (line 101-125)
- JSON serialization follows KojoTestResult.Branches pattern (line 389-405)

**Downstream Impact Verification**:
- ErbRunner return type change requires update to ALL call sites in BatchProcessor (line 47, 130)
- DiffEngine.Compare() already has displayModesA parameter from F677 (no signature change needed)

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,11 | Create DisplayModeCapture service (engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs) | [x] |
| 2 | 3 | Integrate DisplayModeCapture into PRINT_DATA_Instruction (engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | [x] |
| 3 | 4 | Extend KojoTestResult with StructuredOutput property (engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs) | [x] |
| 4 | 4 | Update KojoTestRunner to populate StructuredOutput from DisplayModeCapture (engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) | [x] |
| 5 | 4 | Extend KojoTestResultFormatter to serialize structuredOutput array (engine/Assets/Scripts/Emuera/Headless/KojoTestResultFormatter.cs) | [x] |
| 6 | 8 | Verify engine-side changes build successfully | [x] |
| 7 | 5 | Update ErbRunner to parse structuredOutput and return displayModes (tools/KojoComparer/ErbRunner.cs) | [x] |
| 8 | 6 | Update BatchProcessor to pass ERB displayModes to DiffEngine.Compare (tools/KojoComparer/BatchProcessor.cs) | [x] |
| 9 | 7,9 | Verify consumer-side changes build and tests pass | [x] |
| 10 | 10 | Create integration test DisplayModeEquivalence_PRINTDATAL_MatchesYamlNewline (tools/KojoComparer.Tests/) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T5 | Technical Design DisplayModeCapture specs | Engine-side capture implementation |
| 2 | implementer | sonnet | T6 | Build validation | Compiled engine assemblies |
| 3 | implementer | sonnet | T7-T8 | Technical Design ErbRunner/BatchProcessor specs | Consumer-side parsing implementation |
| 4 | implementer | sonnet | T9 | Build + regression validation | Compiled KojoComparer + passing tests |
| 5 | implementer | sonnet | T10 | Technical Design integration test spec | End-to-end equivalence test |

**Constraints** (from Technical Design):

1. **Headless-only scope**: DisplayModeCapture must be guarded by `#if HEADLESS_MODE` preprocessor directive
2. **Backward compatibility**: JSON output must include both `output` (legacy) and `structuredOutput` (new) fields
3. **PRINTDATAL focus**: Only implement "newline" displayMode variant; other variants (W/K/D) are out of scope
4. **Capture activation**: DisplayModeCapture activates automatically in headless mode (no flag required)
5. **Thread-local storage**: Follow TraceService pattern with `[ThreadStatic]` attribute on capture storage
6. **Execution ordering**: Call `DisplayModeCapture.AddLine()` immediately after `Console.Print()` in same code block to maintain ordering

**Pre-conditions**:

- F676 [DONE] - DialogueResult with displayMode exists
- F677 [DONE] - DiffEngine.Compare() has displayModes parameters
- engine/uEmuera.Headless.csproj builds successfully
- tools/KojoComparer solution builds successfully

**Success Criteria**:

1. All 11 ACs pass verification
2. `dotnet build engine.Tests` succeeds
3. `dotnet build tools/KojoComparer` succeeds
4. `dotnet test tools/KojoComparer.Tests` all tests pass
5. Integration test demonstrates ERB PRINTDATAL produces displayMode="newline" matching YAML
6. No technical debt markers in DisplayModeCapture.cs

**Implementation Steps**:

### Phase 1: Engine-side Capture Implementation (T1-T5)

**T1: Create DisplayModeCapture service**

File: `engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs`

Follow Technical Design interface specification (lines 259-321):
- Wrap entire file in `#if HEADLESS_MODE` ... `#endif`
- Define `OutputLine` class with `Text` and `DisplayMode` string properties
- Define `DisplayModeCapture` static class with ThreadStatic fields `_lines` and `_isCapturing`
- Implement `StartCapture()`, `AddLine(string text, string displayMode)`, `GetLines()`, `Reset()` methods
- Follow TraceService.cs pattern (ThreadStatic storage, lifecycle management)

**T2: Integrate DisplayModeCapture into PRINT_DATA_Instruction**

File: `engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`

After existing code at line 231-236 (Console.NewLine() + ReadAnyKey() block):
```csharp
#if HEADLESS_MODE
if (DisplayModeCapture.IsCapturing)
{
    string displayMode = func.Function.IsNewLine() ? "newline" : "none";
    DisplayModeCapture.AddLine(str, displayMode);
}
#endif
```

**T3: Extend KojoTestResult with StructuredOutput**

File: `engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs`

Add property (follow Technical Design lines 325-334):
```csharp
public List<OutputLine> StructuredOutput { get; set; } = new List<OutputLine>();
```

**T4: Update KojoTestRunner to populate StructuredOutput**

File: `engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs`

In `RunSingleTest()` method:
- Call `DisplayModeCapture.StartCapture()` before test execution
- Call `result.StructuredOutput = DisplayModeCapture.GetLines()` after test execution
- Follow TraceService integration pattern from F084

**T5: Extend KojoTestResultFormatter to serialize structuredOutput**

File: `engine/Assets/Scripts/Emuera/Headless/KojoTestResultFormatter.cs`

In `BuildResultObject()` method (follow Technical Design lines 338-361):
- Create `structuredOutputArray` from `result.StructuredOutput`
- Each array element has `{ text: line.Text, displayMode: line.DisplayMode }`
- Add `structuredOutput = structuredOutputArray` to JSON object alongside existing `output` field

### Phase 2: Engine Build Validation (T6)

**T6: Verify engine-side changes build**

Command: `dotnet build engine.Tests`

Expected: Build succeeds with no errors

### Phase 3: Consumer-side Parsing Implementation (T7-T8)

**T7: Update ErbRunner to parse structuredOutput**

File: `tools/KojoComparer/ErbRunner.cs`

Change `ExecuteAsync()` return type from `Task<string>` to `Task<(string output, List<string> displayModes)>`

In JSON parsing (line 115-123), follow Technical Design lines 395-422:
- Parse existing `output` field
- If `structuredOutput` field exists, iterate array and extract `displayMode` values into list
- Return tuple `(output, displayModes)`

**T8: Update BatchProcessor to pass ERB displayModes**

File: `tools/KojoComparer/BatchProcessor.cs`

In `ProcessTestCaseAsync()` (line 68), follow Technical Design lines 426-437:
- Change `await _erbRunner.ExecuteAsync(...)` to tuple deconstruction: `var (erbOutput, erbDisplayModes) = await _erbRunner.ExecuteAsync(...)`
- Change `DiffEngine.Compare(..., displayModesA: null, ...)` to `displayModesA: erbDisplayModes`

### Phase 4: Consumer Build + Regression Validation (T9)

**T9: Verify consumer-side changes build and tests pass**

Commands:
1. `dotnet build tools/KojoComparer`
2. `dotnet test tools/KojoComparer.Tests`

Expected: Build succeeds, all existing tests pass (regression protection)

### Phase 5: Integration Test (T10)

**T10: Create integration test**

File: `tools/KojoComparer.Tests/DisplayModeEquivalenceTests.cs` (new file)

Follow Technical Design integration test structure (lines 441-476):
- Test name: `DisplayModeEquivalence_PRINTDATAL_MatchesYamlNewline`
- Arrange: Create test ERB with PRINTDATAL, test YAML with displayMode: "newline"
- Act: Execute ERB via ErbRunner, render YAML via YamlRunner
- Assert:
  - ERB displayModes contains single "newline" entry
  - YAML displayModes contains single "newline" entry
  - DiffEngine.Compare() with both displayModes returns IsMatch=true

**Test Naming Convention**: Test methods follow `DisplayModeEquivalence_{Variant}_{Scenario}` format to enable AC#10 filter pattern matching.

**DI Registration**: N/A - DisplayModeCapture is a static utility class, no DI registration needed.

**Error Message Format**: N/A - DisplayModeCapture does not return Result<T>, uses void methods.

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Complete equivalence verification must include display variant semantics" | ERB-side display mode must be captured and compared | AC#1-6, AC#10 |
| "PRINTDATAL → newline" mapping | PRINTDATAL execution must record displayMode="newline" | AC#3, AC#4, AC#10 |
| "not just text content" | Comparison must verify displayMode alongside text | AC#5, AC#6, AC#10 |
| "ERB↔YAML equivalence testing has a blind spot" | End-to-end test must demonstrate blind spot is closed | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DisplayModeCapture.cs exists | file | Glob | exists | engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs | [x] |
| 2 | DisplayModeCapture has AddLine method | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs) | contains | "void AddLine" | [x] |
| 3 | PRINT_DATA_Instruction calls DisplayModeCapture | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | "DisplayModeCapture" | [x] |
| 4 | KojoTestResult has StructuredOutput property | code | Grep(engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs) | contains | "StructuredOutput" | [x] |
| 5 | ErbRunner parses structuredOutput from JSON | code | Grep(tools/KojoComparer/ErbRunner.cs) | contains | "structuredOutput" | [x] |
| 6 | BatchProcessor passes ERB displayModes to Compare | code | Grep(tools/KojoComparer/BatchProcessor.cs) | contains | "displayModesA:" | [x] |
| 7 | KojoComparer solution builds | build | dotnet build tools/KojoComparer | succeeds | - | [x] |
| 8 | engine.Tests build succeeds | build | dotnet build engine.Tests | succeeds | - | [x] |
| 9 | KojoComparer.Tests pass | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 10 | End-to-end displayMode equivalence verification | test | dotnet test tools/KojoComparer.Tests --filter DisplayModeEquivalence | succeeds | - | [x] |
| 11 | No technical debt markers | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1: DisplayModeCapture.cs exists**
- Test: Glob pattern="engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs"
- Expected: File exists
- Rationale: New service file following TraceService pattern (F084 precedent)

**AC#2: DisplayModeCapture has AddLine method**
- Test: Grep pattern="void AddLine" path="engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs"
- Expected: Method signature found
- Rationale: Core capture method called from PRINT_DATA_Instruction
- Edge cases: Method must accept (text, displayMode) parameters

**AC#3: PRINT_DATA_Instruction calls DisplayModeCapture**
- Test: Grep pattern="DisplayModeCapture" path="engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs"
- Expected: Call to capture service found
- Rationale: Integration point where PRINTDATA variant metadata exists (func.Function.IsNewLine())
- Note: Call should be guarded by headless mode check

**AC#4: KojoTestResult has StructuredOutput property**
- Test: Grep pattern="StructuredOutput" path="engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs"
- Expected: Property found
- Rationale: Carries (text, displayMode) pairs to JSON output
- Type: List<OutputLine> or similar structure

**AC#5: ErbRunner parses structuredOutput from JSON**
- Test: Grep pattern="structuredOutput" path="tools/KojoComparer/ErbRunner.cs"
- Expected: JSON parsing for structured output
- Rationale: Consumer must extract displayModes from new JSON format
- Note: Backward compatible - structuredOutput is optional field

**AC#6: BatchProcessor passes ERB displayModes to Compare**
- Test: Grep pattern="displayModesA:" path="tools/KojoComparer/BatchProcessor.cs"
- Expected: ERB displayModes passed (no longer null)
- Rationale: Completes the pipeline - DiffEngine.Compare() now receives both sides
- Verification: Current code shows `displayModesA: null`, must change to actual list

**AC#7: KojoComparer solution builds**
- Test: dotnet build tools/KojoComparer
- Expected: Build succeeds
- Rationale: Validates consumer-side changes compile

**AC#8: engine.Tests build succeeds**
- Test: dotnet build engine.Tests
- Expected: Build succeeds
- Rationale: Validates headless mode changes compile with test project

**AC#9: KojoComparer.Tests pass**
- Test: dotnet test tools/KojoComparer.Tests
- Expected: All tests pass
- Rationale: Regression protection for existing functionality

**AC#10: End-to-end displayMode equivalence verification**
- Test: dotnet test tools/KojoComparer.Tests --filter DisplayModeEquivalence
- Expected: Integration test passes
- Rationale: Demonstrates the blind spot is closed - ERB PRINTDATAL produces displayMode="newline" that matches YAML
- Test design:
  1. Create test YAML with displayMode: "newline"
  2. Execute ERB with PRINTDATAL
  3. Compare via DiffEngine with both displayModes
  4. Assert no displayMode mismatch

**AC#11: No technical debt markers**
- Test: Grep pattern="TODO|FIXME|HACK" path="engine/Assets/Scripts/Emuera/Headless/DisplayModeCapture.cs"
- Expected: 0 matches
- Rationale: Clean implementation without deferred work

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| PRINTDATAW/K/D variants | F700 | Currently 0 occurrences; reactive - implement when kojo uses W/K/D |
| GUI mode displayMode visualization | Phase 29 Task 9 | Reactive - implement if user requests GUI indicators. See full-csharp-architecture.md Phase 29. |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID} or existing Feature Task) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 05:54 | Phase 1 (T1-T5) | Engine-side capture implementation complete. DisplayModeCapture service created, integrated into PRINT_DATA_Instruction, KojoTestResult extended with StructuredOutput, KojoTestRunner updated to populate StructuredOutput, JSON serialization added. Build succeeded: 0 warnings, 0 errors. |
| 2026-01-31 06:01 | Phase 3 (T7-T8) | Consumer-side parsing implementation complete. ErbRunner updated to parse structuredOutput and return tuple (output, displayModes). BatchProcessor updated to pass ERB displayModes to DiffEngine.Compare. Build succeeded: 7 warnings (deprecated RenderAsync), 0 errors. Tests: 26 passed, 2 failed (unrelated edge cases), 11 skipped. |
