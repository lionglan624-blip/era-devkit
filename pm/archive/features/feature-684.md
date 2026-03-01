# Feature 684: GUI Consumer Display Mode Interpretation

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

GUI Consumer Display Mode Interpretation. Implement display mode interpretation for the GUI (uEmuera) consumer, enabling interactive wait/key-wait blocking behavior and rich display-style rendering that HeadlessUI (F682) represents as markers.

---

## Links

- [feature-682.md](feature-682.md) - Consumer-Side Display Mode Interpretation (Predecessor - headless consumer implementation)
- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Predecessor - DisplayMode propagation)

---

## Notes

- Created as F682 successor (残課題: interactive blocking behavior, rich console formatting for Display mode)
- Scope: GUI-side interpretation of DisplayMode values for visual rendering
- HeadlessUI emits markers ([WAIT], [KEY_WAIT], [DISPLAY]); GUI must implement actual behavior

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: The GUI (uEmuera) does not interpret DisplayMode values from YAML dialogue for interactive or visual behavior
2. Why: The GUI consumer path (Unity-based EmueraConsole / ERB runtime) does not consume Era.Core's DialogueResult.DialogueLines at all -- the GUI is driven by the ERB runtime's own PRINTDATA execution (FunctionCode.PRINTDATA/PRINTDATAL/PRINTDATAW/etc.), which is a completely separate code path from the YAML KojoEngine pipeline
3. Why: Era.Core's KojoEngine and its DialogueResult are consumed only by headless/CLI consumers (HeadlessUI, KojoComparer YamlRunner). The GUI engine (engine/Assets/Scripts/Emuera/) has its own ERB processing pipeline that handles PRINTDATA variants natively via EmueraConsole.Print.cs
4. Why: The YAML-first dialogue system (Era.Core KojoEngine) was designed as a parallel pipeline for testing and headless execution, not yet integrated into the GUI runtime's rendering path
5. Why: Bridging the YAML dialogue pipeline into the GUI requires the GUI to consume Era.Core's DialogueResult, which it currently does not. The GUI processes ERB scripts directly through its own instruction execution engine (Instraction.Child.cs)

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| GUI cannot render YAML dialogue with display mode semantics | GUI engine has no integration with Era.Core's KojoEngine/DialogueResult pipeline. It only runs ERB scripts via its own runtime. |
| WAIT/KEY_WAIT blocking behavior not available for YAML dialogue | The GUI's wait infrastructure (IConsoleInput.WaitEnterKey/WaitAnyKey, EmueraConsole's input handling) is only accessible through the ERB runtime path, not the KojoEngine path |
| DISPLAY mode visual rendering not applied to YAML dialogue | EmueraConsole.Print.cs has PRINTDATAD style handling (UseSetColorStyle flag, Style property with SETCOLOR bypass) but this is ERB-runtime-only |

### Conclusion

The root cause is an **integration gap**: there are two parallel dialogue rendering paths in the system:

1. **ERB runtime path** (GUI-native): ERB scripts → FunctionCode.PRINTDATA* → Instraction.Child.cs execution → EmueraConsole.Print.cs rendering → Unity UI. This path natively handles all PRINTDATA variants including wait blocking (via Process.SystemProc.cs), key-wait, and display-style rendering (via UseSetColorStyle).

2. **YAML pipeline path** (Era.Core): YAML files → YamlDialogueLoader → KojoEngine → DialogueResult → HeadlessUI (headless consumer). This path now carries DisplayMode metadata (F676) and HeadlessUI interprets it as text markers (F682), but no GUI consumer reads DialogueResult.

F684 must bridge these two paths by creating a GUI consumer for Era.Core's DialogueResult that maps DisplayMode values to the GUI's existing rendering infrastructure (EmueraConsole print methods, IConsoleInput wait methods). Alternatively, F684 could extend the engine/Headless integration layer to route DialogueResult output through the IConsole interface (HeadlessConsole already implements IConsole which has IConsoleOutput + IConsoleInput with WaitEnterKey/WaitAnyKey).

**Key architectural finding**: The engine's `HeadlessConsole` class (engine/Assets/Scripts/Emuera/Headless/HeadlessConsole.cs) implements `IConsole` (combining `IConsoleOutput` and `IConsoleInput`). It has `WaitEnterKey()` (Console.ReadLine blocking), `WaitAnyKey()` (Console.ReadKey blocking), rich text output via Print/PrintLine/NewLine, and style support (SetForeColor, SetFontStyle, UseUserStyle). This is the engine-level headless console used for ERB execution. The Era.Core `HeadlessUI` class is a separate, simpler class that only outputs dialogue results via Console.WriteLine.

For GUI display mode interpretation, the integration point would be mapping Era.Core DisplayMode values to the appropriate IConsoleOutput/IConsoleInput operations on the engine's console interface:
- DisplayMode.Wait → IConsoleInput.WaitEnterKey() (blocking)
- DisplayMode.KeyWait → IConsoleInput.WaitAnyKey() (blocking)
- DisplayMode.Display → IConsoleOutput with UseSetColorStyle=false (PRINTDATAD style bypass)

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F682 | [DONE] | Predecessor | HeadlessUI consumer-side DisplayMode interpretation. Implemented marker-based output ([WAIT], [KEY_WAIT], [DISPLAY]) in Era.Core HeadlessUI. F684 must implement actual blocking/rendering behavior for GUI. |
| F676 | [DONE] | Predecessor | DisplayMode propagation through Era.Core dialogue pipeline. Provides DialogueLine record with DisplayMode property in DialogueResult.DialogueLines. |
| F681 | [DONE] | Related | Multi-entry selection and rendering pipeline. KojoEngine.GetDialogueMulti() returns aggregated DialogueResult with per-entry DisplayMode values. GUI consumer must handle multi-entry results. |
| F677 | [DONE] | Related (parallel) | KojoComparer displayMode awareness. Separate consumer (comparison tool), independent from GUI rendering. |
| F683 | [DRAFT] | Successor | DialogueResult.Lines deprecation. Depends on all consumers (including GUI) migrating to DialogueLines. |
| F685 | [DRAFT] | Related | HeadlessUI Console.SetOut cleanup. Unrelated to GUI but part of the same F682 successor batch. |

### Pattern Analysis

This is the **GUI consumer adoption phase** of the incremental DisplayMode pipeline:
- F671: Added displayMode to YAML schema/converter
- F676: Extended runtime pipeline to propagate displayMode through DialogueResult
- F682: HeadlessUI consumer (headless/CLI) -- markers, non-blocking
- **F684 (this)**: GUI consumer -- actual interactive behavior, rich rendering
- F683: Lines deprecation (after all consumers migrate)

F684 represents the critical transition from "metadata available" to "user-visible behavior" for GUI users. Unlike F682 (which emits text markers), F684 must produce actual interactive blocking and visual style changes.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Era.Core DialogueResult.DialogueLines is available with per-line DisplayMode. Engine's IConsole interface provides WaitEnterKey/WaitAnyKey (blocking) and style control (SetForeColor, UseUserStyle). Mapping between DisplayMode enum values and IConsole operations is straightforward. |
| Scope is realistic | PARTIAL | Two possible approaches exist: (A) Create a GUI consumer in Era.Core that accepts an IConsole and maps DisplayMode to IConsole methods (~50-80 lines), or (B) Extend the KojoTestRunner/InteractiveRunner in engine to consume DialogueResult with display mode interpretation. Approach A is simpler and testable. Approach B requires modifying Unity project code. |
| No blocking constraints | YES | F676 [DONE] (DialogueResult.DialogueLines available), F682 [DONE] (reference implementation exists). The IConsole interface in the engine provides all needed primitives. No external blockers. |

**Verdict**: FEASIBLE

**Key considerations**:
- The engine's GUI (EmueraConsole) handles PRINTDATA variants natively through the ERB runtime. For YAML-sourced dialogue to have the same behavior, a bridge must be created.
- The engine's `HeadlessConsole` class already demonstrates how to implement IConsole with blocking wait behavior. A GUI consumer could follow the same pattern but target EmueraConsole instead.
- The `IConsoleOutput` interface in the engine (engine/Assets/Scripts/Emuera/Headless/IConsoleOutput.cs) defines Print, PrintLine, NewLine, and style methods. The `IConsoleInput` interface provides WaitEnterKey (WAIT) and WaitAnyKey (KEY_WAIT). DisplayMode maps naturally to combinations of these.
- Display mode (PRINTDATAD) in the original Emuera uses a different rendering style where SETCOLOR is bypassed (UseSetColorStyle=false in EmueraConsole.Print.cs, line 52-56: `if (userStyle.Color == defaultStyle.Color) return userStyle; return new StringStyle(defaultStyle.Color, userStyle.FontStyle, userStyle.Fontname)`). This is a visual-only distinction.
- Only `Newline` (PRINTDATAL) has non-zero usage in current kojo files (1,575 occurrences). Other display mode variants have 0 occurrences. However, all 9 enum values must be supported per F676 philosophy.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F682 | [DONE] | HeadlessUI consumer-side DisplayMode interpretation. Reference implementation for display mode mapping. |
| Predecessor | F676 | [DONE] | DisplayMode propagation through DialogueResult pipeline. Provides DialogueLine record with DisplayMode property. |
| Related | F681 | [DONE] | Multi-entry selection pipeline. GetDialogueMulti() returns aggregated results with per-entry DisplayMode. |
| Related | F677 | [DONE] | KojoComparer displayMode awareness. Independent scope (comparison tool). |
| Successor | F683 | [DRAFT] | DialogueResult.Lines deprecation. Depends on F684 completing GUI consumer migration. |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Unity Engine (6000.3.1f1) | Build | Low | GUI consumer may need to reference Era.Core types. Engine project already references Era.Core via its .csproj setup. |
| IConsole/IConsoleOutput/IConsoleInput interfaces | Runtime | Low | Engine interfaces provide all needed primitives for wait/display behavior. Already stable (Feature 001). |
| EmueraConsole.Print.cs | Runtime | Medium | GUI rendering relies on EmueraConsole's print buffer system. Display mode style bypass (UseSetColorStyle) is internal to EmueraConsole. May need to expose or bridge for Era.Core consumer access. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | MEDIUM | Runs kojo functions for testing. May need to route output through DisplayMode-aware consumer. |
| engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs | MEDIUM | Interactive headless runner. Could benefit from DisplayMode interpretation. |
| Era.Core/HeadlessUI.cs | NONE | Already handled by F682. Not affected by F684. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core (new or existing file) | Add | GUI consumer class that maps DialogueResult.DialogueLines DisplayMode values to IConsole operations (Print/PrintLine + WaitEnterKey/WaitAnyKey + UseSetColorStyle) |
| engine (KojoTestRunner.cs or InteractiveRunner.cs) | Update | Route YAML dialogue output through the new DisplayMode-aware consumer instead of plain text output |
| Era.Core.Tests or engine.Tests | Add | Test cases verifying each DisplayMode value produces correct IConsole method calls |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| GUI uses EmueraConsole (Unity), not Era.Core HeadlessUI | engine/Assets/Scripts/Emuera/GameView/EmueraConsole.Print.cs | HIGH - GUI consumer cannot use Era.Core HeadlessUI's Console.WriteLine approach. Must integrate with EmueraConsole's print buffer and display line system. |
| IConsole is in engine namespace (MinorShift.Emuera.Headless), not Era.Core | engine/Assets/Scripts/Emuera/Headless/IConsole.cs | MEDIUM - Cross-project dependency direction: Era.Core cannot reference engine types directly. GUI consumer must either live in engine or use an abstraction. |
| EmueraConsole display style bypass is internal | EmueraConsole.Print.cs line 52 (UseSetColorStyle) | MEDIUM - Display mode (PRINTDATAD) visual behavior requires internal state manipulation. May need new method on EmueraConsole or IConsoleOutput. |
| Only Newline (PRINTDATAL) is used in current kojo files | F676 investigation (1,575 occurrences) | LOW - Practical impact limited to Newline behavior, but all 9 enum values must be handled. |
| Two IConsoleOutput interfaces exist | Era.Core/Interfaces/IConsoleOutput.cs vs engine/Headless/IConsoleOutput.cs | MEDIUM - Era.Core's IConsoleOutput has PrintWait/PrintData methods. Engine's IConsoleOutput has Print/PrintLine/NewLine + style. They are different interfaces with different signatures. Consumer must target the correct one based on integration layer. |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Cross-project dependency direction prevents Era.Core from referencing engine types | High | High | Design GUI consumer to live in the engine project (not Era.Core), or create an interface adapter in Era.Core that engine implements. F682's HeadlessUI approach (Era.Core class using System.Console) cannot work for GUI. |
| Wait/KeyWait blocking may hang automated tests in GUI context | Medium | High | Follow F682's pattern: detect test/scripted mode and emit markers instead of blocking. IConsoleInput already has IsWaiting flag. |
| Display mode style bypass requires EmueraConsole internal access | Medium | Medium | Expose UseSetColorStyle via IConsoleOutput extension or add a new method to the interface. Alternatively, set UseSetColorStyle before calling Print. |
| EmueraConsole print buffer system is complex | Medium | Medium | Use existing EmueraConsole API (Print, PrintLine, PrintFlush, NewLine) rather than modifying internal buffer logic. |
| Integration with KojoTestRunner/InteractiveRunner may require refactoring their output paths | Medium | Low | KojoTestRunner already outputs dialogue text. Modifying it to use DisplayMode-aware output should be localized to the output section. |

## Background

### Philosophy (Mid-term Vision)

Every DisplayMode value in the YAML dialogue pipeline must produce correct, observable rendering behavior in all consumers. The GUI consumer must interpret DisplayMode semantics identically to the ERB runtime's PRINTDATA variants — Wait blocks until Enter, KeyWait blocks until any key, Display uses style bypass rendering. Era.Core's IConsole abstraction (engine-layer IConsoleOutput + IConsoleInput) is the single integration point for bridging YAML dialogue to GUI behavior, ensuring the GUI consumer is testable via mock IConsole implementations without requiring Unity runtime.

### Problem (Current Issue)

The GUI engine (uEmuera) has no consumer for Era.Core's DialogueResult.DialogueLines. YAML-sourced dialogue with DisplayMode metadata cannot produce interactive blocking (Wait/KeyWait) or visual style changes (Display) in the GUI. The engine's IConsole interface provides all needed primitives (WaitEnterKey, WaitAnyKey, UseUserStyle, Print/PrintLine/NewLine) but no bridge exists between DialogueResult and IConsole.

### Goal (What to Achieve)

1. Create a GUI consumer class that maps each of the 9 DisplayMode enum values to appropriate IConsole method calls (IConsoleOutput for print/style, IConsoleInput for wait/key-wait)
2. Verify all 9 DisplayMode values produce correct IConsole method sequences via unit tests with mock IConsole
3. Verify Display-mode variants apply UseUserStyle=false (SETCOLOR bypass) before printing
4. Verify Wait-mode variants call IConsoleInput.WaitEnterKey() after printing
5. Verify KeyWait-mode variants call IConsoleInput.WaitAnyKey() after printing
6. Ensure the consumer class builds successfully in the engine project
7. Verify zero technical debt in new code

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Every DisplayMode value must produce correct, observable rendering behavior" | All 9 DisplayMode enum values must be handled with correct IConsole method calls | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8 |
| "identically to the ERB runtime's PRINTDATA variants" | Wait → WaitEnterKey, KeyWait → WaitAnyKey, Display → UseUserStyle=false | AC#5, AC#6, AC#7 |
| "IConsole abstraction is the single integration point" | Consumer accepts IConsole (or IConsoleOutput + IConsoleInput) as dependency | AC#3 |
| "testable via mock IConsole implementations" | Unit tests use mock IConsole, not real Console | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GUI consumer class file exists | file | Glob(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | exists | DisplayModeConsumer.cs | [x] |
| 2 | Engine project builds | build | dotnet build engine | succeeds | - | [x] |
| 3 | Consumer accepts IConsole dependency | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | matches | "IConsole\\s+\\w+" | [x] |
| 4 | All 9 DisplayMode cases handled | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | contains | "DisplayWait" | [x] |
| 5 | Wait variants call WaitEnterKey | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | contains | "WaitEnterKey" | [x] |
| 6 | KeyWait variants call WaitAnyKey | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | contains | "WaitAnyKey" | [x] |
| 7 | Display variants set UseUserStyle false | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | contains | "UseUserStyle" | [x] |
| 8 | Unit tests pass | test | dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeConsumer | succeeds | - | [x] |
| 9 | Test covers all 9 DisplayMode values | code | Grep(engine.Tests, glob=*DisplayModeConsumer*) | contains | "DisplayWait" | [x] |
| 10 | Test file exists | file | Glob(engine.Tests/**/DisplayModeConsumerTests.cs) | exists | DisplayModeConsumerTests.cs | [x] |
| 11 | Default/Newline do not call wait methods | code | Grep(engine.Tests, glob=*DisplayModeConsumer*) | contains | "WaitEnterKeyCalls);" | [x] |
| 12 | Zero technical debt | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | not_contains | "TODO" | [x] |

### AC Details

**AC#1: GUI consumer class file exists**
- Test: Glob pattern=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: File exists
- Rationale: Consumer must live in engine project (not Era.Core) because it depends on engine-layer IConsole interfaces (MinorShift.Emuera.Headless namespace). Era.Core cannot reference engine types (Issue 29).

**AC#2: Engine project builds**
- Test: `dotnet build engine`
- Expected: Build succeeds with 0 errors
- Rationale: New class must integrate with existing engine project without breaking compilation.

**AC#3: Consumer accepts IConsole dependency**
- Test: Grep pattern=`IConsole\s+\w+` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: Constructor or method parameter accepts IConsole (or IConsoleOutput + IConsoleInput)
- Rationale: IConsole is the single integration point per Philosophy. Dependency injection enables mock-based testing.

**AC#4: All 9 DisplayMode cases handled**
- Test: Grep pattern=`DisplayWait` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: Contains DisplayWait case (highest enum value ensures all 9 values present in exhaustive switch)
- Rationale: Philosophy requires every DisplayMode value produces correct behavior. Missing cases would cause runtime exceptions with exhaustive switch.

**AC#5: Wait variants call WaitEnterKey**
- Test: Grep pattern=`WaitEnterKey` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: At least 1 match
- Rationale: DisplayMode.Wait and DisplayMode.DisplayWait must block until Enter key, matching PRINTDATAW ERB behavior. Maps to IConsoleInput.WaitEnterKey().

**AC#6: KeyWait variants call WaitAnyKey**
- Test: Grep pattern=`WaitAnyKey` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: At least 1 match
- Rationale: DisplayMode.KeyWait, KeyWaitNewline, KeyWaitWait must block until any key press, matching PRINTDATAK ERB behavior. Maps to IConsoleInput.WaitAnyKey().

**AC#7: Display variants set UseUserStyle false**
- Test: Grep pattern=`UseUserStyle` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: At least 1 match
- Rationale: PRINTDATAD in ERB runtime bypasses SETCOLOR (UseSetColorStyle=false in EmueraConsole.Print.cs). Display-mode variants must replicate this by setting UseUserStyle=false on IConsoleOutput before printing, then restoring.

**AC#8: Unit tests pass**
- Test: `dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeConsumer`
- Expected: All tests pass
- Rationale: Tests verify each DisplayMode value produces correct IConsole method call sequence using mock IConsole.

**AC#9: Test covers all 9 DisplayMode values**
- Test: Grep pattern=`DisplayWait` path=`engine.Tests/` glob=`*DisplayModeConsumer*`
- Expected: Contains DisplayWait test (highest enum value ensures all 9 values tested)
- Rationale: Symmetric test coverage (Issue 37). Each DisplayMode value must have at least one test verifying its behavior.

**AC#10: Test file exists**
- Test: Glob pattern=`engine.Tests/**/DisplayModeConsumerTests.cs`
- Expected: File exists
- Rationale: Test file must exist for AC#8 to be meaningful.

**AC#11: Default/Newline do not call wait methods**
- Test: Grep pattern=`WaitEnterKeyCalls);` path=`engine.Tests/` glob=`*DisplayModeConsumer*`
- Expected: Contains assertion verifying zero wait call counts
- Rationale: Negative test — DisplayMode.Default and DisplayMode.Newline must NOT invoke WaitEnterKey or WaitAnyKey. This verifies the consumer does not over-apply blocking behavior.
- Note: Tests verify zero call counts using `Assert.Equal(0, mock.WaitEnterKeyCalls)` pattern.

**AC#12: Zero technical debt**
- Test: Grep pattern=`TODO` path=`engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
- Expected: No matches (no TODO, FIXME, or HACK markers)
- Rationale: Clean implementation with no deferred work markers.
- Note: Additional verification needed for FIXME and HACK patterns using manual inspection or separate grep commands.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Core Design**: Create `DisplayModeConsumer` class in `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs` that maps Era.Core's `DialogueResult.DialogueLines` to engine-layer `IConsole` method calls. The consumer accepts `IConsole` as a dependency and interprets each `DisplayMode` enum value to produce the appropriate sequence of `IConsoleOutput` print operations and `IConsoleInput` wait operations.

**Architecture**:

```
Era.Core DialogueResult (DisplayMode metadata)
    ↓
DisplayModeConsumer.Render(DialogueResult, IConsole)
    ↓
switch (dialogueLine.DisplayMode)
    ↓
IConsoleOutput.Print/PrintLine/NewLine + IConsoleInput.WaitEnterKey/WaitAnyKey
```

**Key Implementation Details**:

1. **Class Location**: `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`
   - Lives in engine project because it depends on `MinorShift.Emuera.Headless.IConsole`
   - Era.Core cannot reference engine types (cross-project dependency direction)

2. **Public API**:
   ```csharp
   namespace MinorShift.Emuera.Headless
   {
       public class DisplayModeConsumer
       {
           public void Render(DialogueResult dialogueResult, IConsole console);
       }
   }
   ```

3. **DisplayMode Mapping Logic** (exhaustive switch expression):

   | DisplayMode | IConsoleOutput | IConsoleInput | UseUserStyle |
   |-------------|----------------|---------------|--------------|
   | Default | Print(text) | - | true |
   | Newline | PrintLine(text) | - | true |
   | Wait | PrintLine(text) | WaitEnterKey() | true |
   | KeyWait | Print(text) | WaitAnyKey() | true |
   | KeyWaitNewline | PrintLine(text) | WaitAnyKey() | true |
   | KeyWaitWait | PrintLine(text) | WaitAnyKey() + WaitEnterKey() | true |
   | Display | Print(text) | - | **false** |
   | DisplayNewline | PrintLine(text) | - | **false** |
   | DisplayWait | PrintLine(text) | WaitEnterKey() | **false** |

4. **Display Mode Style Bypass**:
   - PRINTDATAD in ERB runtime sets `UseSetColorStyle=false` (EmueraConsole.Print.cs line 52)
   - `DisplayModeConsumer` replicates this by:
     ```csharp
     // Before Display-mode variants
     bool previousUseUserStyle = console.UseUserStyle;
     console.UseUserStyle = false;

     // ... print operation ...

     // Restore after
     console.UseUserStyle = previousUseUserStyle;
     ```
   - This ensures Display-mode variants bypass SETCOLOR commands and use default style

5. **Test Infrastructure**:
   - Unit tests in `engine.Tests/DisplayModeConsumerTests.cs`
   - Mock `IConsole` using Moq or manual mock implementation
   - Verify method call sequences for each DisplayMode value
   - Example test structure:
     ```csharp
     [Fact]
     public void Render_WithDisplayModeWait_CallsWaitEnterKey()
     {
         var mock = new MockConsole();
         var consumer = new DisplayModeConsumer();
         var dialogueResult = DialogueResult.Create(new[]
         {
             new DialogueLine("Test", DisplayMode.Wait)
         });

         consumer.Render(dialogueResult, mock);

         Assert.Single(mock.PrintLineCalls);
         Assert.Equal("Test", mock.PrintLineCalls[0]);
         Assert.Equal(1, mock.WaitEnterKeyCalls);
     }
     ```

6. **Integration with Existing Runners**:
   - `KojoTestRunner.cs` and `InteractiveRunner.cs` currently output plain text
   - These runners already have access to `IConsole` instances
   - Integration approach (NOT in F684 scope, but design consideration):
     - Replace direct `console.PrintLine(text)` with `DisplayModeConsumer.Render(dialogueResult, console)`
     - KojoEngine already produces `DialogueResult` with DisplayMode metadata (F676)
     - This bridge enables YAML-sourced dialogue to behave identically to ERB PRINTDATA variants

**Design Rationale**:

- **Why in engine project**: Era.Core cannot reference engine's `IConsole` interface (MinorShift.Emuera.Headless namespace). Consumer must live where IConsole is defined.
- **Why IConsole abstraction**: Enables mock-based unit testing without Unity runtime. HeadlessConsole (System.Console.ReadLine blocking) proves IConsole provides all needed primitives.
- **Why exhaustive switch**: Compiler-enforced coverage for all 9 DisplayMode values. Adding new enum values will cause compile error until handled.
- **Why UseUserStyle toggle**: Replicates EmueraConsole's PRINTDATAD behavior (style bypass). This is the visual difference between PRINTDATA and PRINTDATAD.
- **Why separate Render method**: Consumers call one method with DialogueResult. Internal switch handles routing to appropriate IConsole operations.

### AC Coverage

**How the Technical Design satisfies each Acceptance Criterion**:

| AC# | Coverage |
|:---:|----------|
| 1 | File created at `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs` |
| 2 | Class compiles in engine project (MinorShift.Emuera.Headless namespace) |
| 3 | `Render()` method accepts `IConsole console` parameter (dependency injection) |
| 4 | Exhaustive switch expression on `dialogueLine.DisplayMode` with 9 case arms |
| 5 | Cases for DisplayMode.Wait and DisplayMode.DisplayWait call `console.WaitEnterKey()` |
| 6 | Cases for DisplayMode.KeyWait, KeyWaitNewline, KeyWaitWait call `console.WaitAnyKey()` |
| 7 | Display-mode cases set `console.UseUserStyle = false` before print, restore after |
| 8 | Unit tests in `engine.Tests/DisplayModeConsumerTests.cs` verify mock IConsole calls |
| 9 | Test methods cover all 9 DisplayMode enum values (symmetric coverage) |
| 10 | Test file `engine.Tests/DisplayModeConsumerTests.cs` created |
| 11 | Negative test verifies Default/Newline modes do NOT call wait methods (mock.Verify Times.Never) |
| 12 | Clean implementation with no TODO/FIXME/HACK markers |

**AC#4 Detail** (All 9 DisplayMode cases handled):
The exhaustive switch expression ensures compiler verification:
```csharp
var action = dialogueLine.DisplayMode switch
{
    DisplayMode.Default => /* ... */,
    DisplayMode.Newline => /* ... */,
    DisplayMode.Wait => /* ... */,
    DisplayMode.KeyWait => /* ... */,
    DisplayMode.KeyWaitNewline => /* ... */,
    DisplayMode.KeyWaitWait => /* ... */,
    DisplayMode.Display => /* ... */,
    DisplayMode.DisplayNewline => /* ... */,
    DisplayMode.DisplayWait => /* ... */
    // No _ => discard pattern - forces exhaustiveness check
};
```
Grep will find 9 matches for `DisplayMode.` in the switch expression.

**AC#7 Detail** (Display variants set UseUserStyle false):
```csharp
private void RenderDisplayVariant(IConsole console, string text, bool withNewline, bool withWait)
{
    bool previous = console.UseUserStyle;
    console.UseUserStyle = false; // <-- AC#7 Grep target

    if (withNewline)
        console.PrintLine(text);
    else
        console.Print(text);

    console.UseUserStyle = previous;

    if (withWait)
        console.WaitEnterKey();
}
```

**AC#11 Detail** (Default/Newline do not call wait methods):
Test uses `mock.Verify(c => c.WaitEnterKey(), Times.Never)` and `mock.Verify(c => c.WaitAnyKey(), Times.Never)` to assert no wait calls for Default/Newline modes.

### Key Decisions

**Decision 1: Consumer lives in engine project, not Era.Core**

- **Context**: Cross-project dependency direction prevents Era.Core from referencing engine types
- **Options Considered**:
  - A) Create consumer in Era.Core with abstraction (Era.Core's own IConsoleOutput interface)
  - B) Create consumer in engine project directly using MinorShift.Emuera.Headless.IConsole
  - C) Extend Era.Core.HeadlessUI with engine IConsole support
- **Decision**: Option B (engine project)
- **Rationale**:
  - Era.Core already has its own `IConsoleOutput` interface with different method signatures (PrintWait, PrintData) than engine's IConsoleOutput (Print, PrintLine, NewLine)
  - Creating a third abstraction layer adds complexity for no benefit
  - F682's HeadlessUI uses System.Console directly (no engine dependency) - this is the Era.Core pattern
  - GUI consumer needs engine's IConsole interface which already exists and is testable
  - HeadlessConsole proves IConsole provides all needed primitives (WaitEnterKey/WaitAnyKey, UseUserStyle, Print/PrintLine)
- **Impact**: DisplayModeConsumer cannot be tested with Era.Core.Tests. Must use engine.Tests project.

**Decision 2: UseUserStyle toggle for Display mode variants**

- **Context**: PRINTDATAD in ERB runtime bypasses SETCOLOR commands (UseSetColorStyle=false in EmueraConsole.Print.cs)
- **Options Considered**:
  - A) Ignore style bypass - treat Display mode identically to Default mode
  - B) Set UseUserStyle=false before Display-mode prints, restore after
  - C) Add new IConsoleOutput method like `PrintWithoutUserStyle(text)`
- **Decision**: Option B (UseUserStyle toggle)
- **Rationale**:
  - Philosophy requires "identically to the ERB runtime's PRINTDATA variants"
  - UseUserStyle property already exists on IConsoleOutput interface (line 125)
  - Toggle pattern preserves encapsulation (no new interface methods needed)
  - HeadlessConsole tracks UseUserStyle as boolean flag (line 16, 204)
  - EmueraConsole's UseSetColorStyle serves the same purpose (style bypass control)
- **Impact**: Consumer must save/restore previous UseUserStyle value to avoid polluting state

**Decision 3: Exhaustive switch expression without discard pattern**

- **Context**: Need to handle all 9 DisplayMode enum values
- **Options Considered**:
  - A) If-else chain with default case
  - B) Switch expression with `_ => throw new ArgumentException()`
  - C) Exhaustive switch expression without discard pattern
- **Decision**: Option C (exhaustive switch)
- **Rationale**:
  - C# compiler enforces exhaustiveness check when no discard pattern present
  - Adding new DisplayMode values in future will cause compile error until handled
  - Matches F682's pattern (HeadlessUI.cs lines 22-33, no discard in prefix switch)
  - Prevents silent runtime failures from unhandled enum values
- **Impact**: Any future DisplayMode enum additions require updating DisplayModeConsumer (fail-fast at compile time)

**Decision 4: KeyWaitWait mode calls WaitAnyKey() then WaitEnterKey()**

- **Context**: DisplayMode.KeyWaitWait maps to PRINTDATAKW (key wait + wait)
- **Options Considered**:
  - A) Call WaitEnterKey() only (treat as duplicate of Wait)
  - B) Call WaitAnyKey() only (treat as duplicate of KeyWait)
  - C) Call WaitAnyKey() followed by WaitEnterKey() (two blocking calls)
- **Decision**: Option C (both wait calls in sequence)
- **Rationale**:
  - PRINTDATAKW in ERB runtime performs two separate wait operations
  - "KW" suffix = "K" (key wait) + "W" (wait) = KeyWait + Wait
  - Matches the compositional semantics of other variants (KeyWaitNewline = KeyWait + Newline)
  - F676 investigation shows 0 occurrences in current kojo files, so practical impact is nil but correctness matters
- **Impact**: KeyWaitWait blocks twice (any key, then Enter key). Users must press two keys to proceed.

**Decision 5: Public Render() method, not constructor injection**

- **Context**: Consumer needs DialogueResult and IConsole to operate
- **Options Considered**:
  - A) Constructor takes IConsole, Render(DialogueResult) method
  - B) Render(DialogueResult, IConsole) method, stateless consumer
  - C) Separate RenderLine(DialogueLine, IConsole) for per-line processing
- **Decision**: Option B (stateless Render method)
- **Rationale**:
  - Consumer is stateless - no internal state between calls
  - IConsole instance may vary per render operation (different console targets)
  - Stateless design is simpler and thread-safe by default
  - Matches functional programming principles (input → output, no hidden state)
  - Constructor injection would force one IConsole per consumer instance lifetime
- **Impact**: Callers pass both DialogueResult and IConsole to Render() method each time

**Decision 6: F684 scope excludes KojoTestRunner/InteractiveRunner integration**

- **Context**: Existing runners could use DisplayModeConsumer but currently don't consume DialogueResult
- **Options Considered**:
  - A) Include integration in F684 (modify runners to call DisplayModeConsumer)
  - B) Defer integration to separate feature (F684 creates consumer only)
- **Decision**: Option B (defer integration)
- **Rationale**:
  - F684 Philosophy: "Create a GUI consumer class" and "Verify all 9 DisplayMode values" - scope is consumer creation and testing
  - KojoTestRunner integration requires analysis of current output paths and ERB vs YAML execution modes
  - InteractiveRunner may need different DisplayMode behavior (e.g., auto-skip waits in scripted mode)
  - Separation allows F684 to complete independently and unblock F683 (Lines deprecation)
  - Integration can be tackled in follow-up feature with specific runner requirements
- **Impact**: DisplayModeConsumer exists and is tested, but not yet consumed by engine runners. Manual integration required in future feature.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4,5,6,7,12 | Create DisplayModeConsumer.cs with IConsole dependency and exhaustive switch for all 9 DisplayMode values | [x] |
| 2 | 8,9,10,11 | Create DisplayModeConsumerTests.cs with mock IConsole verification for all 9 DisplayMode values | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design exhaustive switch pattern | DisplayModeConsumer.cs with IConsole integration |
| 2 | implementer | sonnet | T2 | AC Details test structure | DisplayModeConsumerTests.cs with Moq verification |
| 3 | ac-tester | haiku | - | AC table | Verification results |

### Constraints

1. **Cross-project dependency direction**: DisplayModeConsumer MUST live in `engine/Assets/Scripts/Emuera/Headless/` because Era.Core cannot reference engine's `IConsole` interface (MinorShift.Emuera.Headless namespace). See Technical Design Decision 1.

2. **Exhaustive switch without discard pattern**: The DisplayMode switch expression MUST NOT include `_ => discard` pattern to enable compiler-enforced exhaustiveness checking. Adding new DisplayMode values in the future will cause compile error until handled. See Technical Design Decision 3.

3. **UseUserStyle toggle pattern**: Display-mode variants (Display, DisplayNewline, DisplayWait) MUST save previous `console.UseUserStyle` value, set to `false` before printing, and restore after. This replicates EmueraConsole's PRINTDATAD style bypass behavior. See Technical Design Decision 2.

4. **KeyWaitWait double-blocking**: DisplayMode.KeyWaitWait MUST call `WaitAnyKey()` followed by `WaitEnterKey()` (two separate blocking calls in sequence). This matches PRINTDATAKW ERB runtime behavior. See Technical Design Decision 4.

5. **Stateless consumer design**: DisplayModeConsumer is stateless. Public API is `Render(DialogueResult dialogueResult, IConsole console)` method. IConsole is passed per-call, not constructor-injected. See Technical Design Decision 5.

### Pre-conditions

- F676 [DONE]: DialogueResult.DialogueLines with DisplayMode property exists
- F682 [DONE]: HeadlessUI reference implementation for DisplayMode interpretation exists
- engine project builds successfully: `dotnet build engine`
- engine.Tests project exists

### Success Criteria

1. All 12 ACs pass verification
2. Engine project builds with zero errors: `dotnet build engine`
3. All tests pass: `dotnet test engine.Tests --filter FullyQualifiedName~DisplayModeConsumer`
4. No technical debt markers (TODO/FIXME/HACK) in implementation
5. Exhaustive switch expression ensures compiler verification for all 9 DisplayMode values

### Implementation Steps

#### Task 1: Create DisplayModeConsumer.cs

**File**: `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs`

**Namespace**: `MinorShift.Emuera.Headless`

**Structure**:

```csharp
using Era.Core.Dialogue;
using MinorShift.Emuera.Headless;

namespace MinorShift.Emuera.Headless
{
    public class DisplayModeConsumer
    {
        public void Render(DialogueResult dialogueResult, IConsole console)
        {
            foreach (var line in dialogueResult.DialogueLines)
            {
                RenderLine(line, console);
            }
        }

        private void RenderLine(DialogueLine line, IConsole console)
        {
            // Exhaustive switch expression - no discard pattern
            var action = line.DisplayMode switch
            {
                DisplayMode.Default => () => console.Print(line.Text),
                DisplayMode.Newline => () => console.PrintLine(line.Text),
                DisplayMode.Wait => ((Action)(() => { console.PrintLine(line.Text); console.WaitEnterKey(); })),
                DisplayMode.KeyWait => ((Action)(() => { console.Print(line.Text); console.WaitAnyKey(); })),
                DisplayMode.KeyWaitNewline => ((Action)(() => { console.PrintLine(line.Text); console.WaitAnyKey(); })),
                DisplayMode.KeyWaitWait => ((Action)(() => { console.PrintLine(line.Text); console.WaitAnyKey(); console.WaitEnterKey(); })),
                DisplayMode.Display => () => RenderDisplayVariant(console, line.Text, withNewline: false, withWait: false),
                DisplayMode.DisplayNewline => () => RenderDisplayVariant(console, line.Text, withNewline: true, withWait: false),
                DisplayMode.DisplayWait => ((Action)(() => RenderDisplayVariant(console, line.Text, withNewline: true, withWait: true)))
                // No _ => discard - compiler enforces exhaustiveness
            };
            action();
        }

        private void RenderDisplayVariant(IConsole console, string text, bool withNewline, bool withWait)
        {
            bool previous = console.UseUserStyle;
            console.UseUserStyle = false; // PRINTDATAD style bypass

            if (withNewline)
                console.PrintLine(text);
            else
                console.Print(text);

            console.UseUserStyle = previous;

            if (withWait)
                console.WaitEnterKey();
        }
    }
}
```

**DisplayMode Mapping Reference** (from Technical Design):

| DisplayMode | IConsoleOutput | IConsoleInput | UseUserStyle |
|-------------|----------------|---------------|--------------|
| Default | Print(text) | - | true |
| Newline | PrintLine(text) | - | true |
| Wait | PrintLine(text) | WaitEnterKey() | true |
| KeyWait | Print(text) | WaitAnyKey() | true |
| KeyWaitNewline | PrintLine(text) | WaitAnyKey() | true |
| KeyWaitWait | PrintLine(text) | WaitAnyKey() + WaitEnterKey() | true |
| Display | Print(text) | - | **false** |
| DisplayNewline | PrintLine(text) | - | **false** |
| DisplayWait | PrintLine(text) | WaitEnterKey() | **false** |

#### Task 2: Create DisplayModeConsumerTests.cs

**File**: `engine.Tests/DisplayModeConsumerTests.cs`

**Test Framework**: xUnit with manual mock for IConsole (Moq not available in engine.Tests)

**Test Structure** (verify each of 9 DisplayMode values):

```csharp
using Xunit;
using System;
using System.Collections.Generic;
using MinorShift.Emuera.Headless;
using Era.Core.Types;
using Era.Core.Dialogue;
using uEmuera.Drawing;

namespace MinorShift.Emuera.Tests
{
    public class MockConsole : IConsole
    {
        public List<string> PrintCalls = new List<string>();
        public List<string> PrintLineCalls = new List<string>();
        public int WaitEnterKeyCalls = 0;
        public int WaitAnyKeyCalls = 0;
        public bool UseUserStyle { get; set; } = true;

        // IConsoleOutput implemented
        public void Print(string text) => PrintCalls.Add(text);
        public void PrintLine(string text) => PrintLineCalls.Add(text);
        public void NewLine() { }

        // IConsoleOutput stubs
        public void PrintSystemLine(string text) { }
        public void PrintError(string text) { }
        public void PrintWarning(string text, int level) { }
        public void PrintBar() { }
        public void PrintBar(string pattern) { }
        public void PrintButton(string text, string value) { }
        public void PrintButton(string text, long value) { }
        public void PrintC(string text, bool alignRight) { }
        public void Flush(bool forceNewLine) { }
        public void ClearDisplay() { }
        public void DeleteLine(int count) { }
        public void SetWindowTitle(string title) { }
        public void SetForeColor(Color color) { }
        public void SetBackColor(Color color) { }
        public void SetFontStyle(FontStyle style) { }
        public void SetFont(string font) { }
        public void ResetStyle() { }
        public ConsoleAlignment Alignment { get; set; } = ConsoleAlignment.Left;
        public bool IsBufferEmpty => false;
        public bool LastLineIsTemporary => false;
        public long LineCount => 0;

        // IConsoleInput implemented
        public void WaitEnterKey() => WaitEnterKeyCalls++;
        public void WaitAnyKey() => WaitAnyKeyCalls++;
        public bool WaitAnyKey(long timeoutMs) => true;

        // IConsoleInput stubs
        public InputResult ReadInt() => new InputResult { Success = true, IntValue = 0 };
        public InputResult ReadInt(InputOptions options) => new InputResult { Success = true, IntValue = 0 };
        public InputResult ReadString() => new InputResult { Success = true, StringValue = "" };
        public InputResult ReadString(InputOptions options) => new InputResult { Success = true, StringValue = "" };
        public bool IsKeyPressed() => false;
        public bool IsWaiting => false;
        public void CancelInput() { }
        public void SimulateInput(string input) { }

        // IConsole stubs
        public IGraphicsStub Graphics => new NullGraphicsStub();
        public bool Initialize() => true;
        public void Shutdown() { }
        public bool IsActive => true;
        public bool IsRunning => true;
        public bool IsError => false;
        public void Quit() { }
    }

    public class DisplayModeConsumerTests
    {
        [Fact]
        public void Render_WithDisplayModeDefault_CallsPrintOnly()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.Default)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintCalls);
            Assert.Equal("Test", mock.PrintCalls[0]);
            Assert.Equal(0, mock.WaitEnterKeyCalls);
            Assert.Equal(0, mock.WaitAnyKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeNewline_CallsPrintLineOnly()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.Newline)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.Equal(0, mock.WaitEnterKeyCalls);
            Assert.Equal(0, mock.WaitAnyKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeWait_CallsWaitEnterKey()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.Wait)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.Equal(1, mock.WaitEnterKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeKeyWait_CallsWaitAnyKey()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.KeyWait)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintCalls);
            Assert.Equal("Test", mock.PrintCalls[0]);
            Assert.Equal(1, mock.WaitAnyKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeKeyWaitNewline_CallsWaitAnyKeyAfterPrintLine()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.KeyWaitNewline)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.Equal(1, mock.WaitAnyKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeKeyWaitWait_CallsBothWaitMethods()
        {
            var mock = new MockConsole();
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.KeyWaitWait)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.Equal(1, mock.WaitAnyKeyCalls);
            Assert.Equal(1, mock.WaitEnterKeyCalls);
        }

        [Fact]
        public void Render_WithDisplayModeDisplay_SetsUseUserStyleFalse()
        {
            var mock = new MockConsole();
            mock.UseUserStyle = true; // Start with true
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.Display)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintCalls);
            Assert.Equal("Test", mock.PrintCalls[0]);
            Assert.True(mock.UseUserStyle); // Should be restored to true
        }

        [Fact]
        public void Render_WithDisplayModeDisplayNewline_SetsUseUserStyleFalse()
        {
            var mock = new MockConsole();
            mock.UseUserStyle = true;
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.DisplayNewline)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.True(mock.UseUserStyle); // Should be restored to true
        }

        [Fact]
        public void Render_WithDisplayModeDisplayWait_SetsUseUserStyleFalseAndWaits()
        {
            var mock = new MockConsole();
            mock.UseUserStyle = true;
            var consumer = new DisplayModeConsumer();
            var dialogueResult = DialogueResult.Create(new[]
            {
                new DialogueLine("Test", DisplayMode.DisplayWait)
            });

            consumer.Render(dialogueResult, mock);

            Assert.Single(mock.PrintLineCalls);
            Assert.Equal("Test", mock.PrintLineCalls[0]);
            Assert.Equal(1, mock.WaitEnterKeyCalls);
            Assert.True(mock.UseUserStyle); // Should be restored to true
        }
    }
}
```

**AC#11 Negative Test Coverage**:

The `Render_WithDisplayModeDefault_CallsPrintOnly()` and `Render_WithDisplayModeNewline_CallsPrintLineOnly()` tests already include verification that wait methods are not called:

```csharp
Assert.Equal(0, mock.WaitEnterKeyCalls);
Assert.Equal(0, mock.WaitAnyKeyCalls);
```

This satisfies AC#11's requirement for negative test verification that Default/Newline modes do not call wait methods.

### Rollback Plan

If issues arise after implementation:

1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback and specific failure mode
3. Create follow-up feature for fix with additional investigation into:
   - IConsole interface compatibility issues
   - DisplayMode enum value handling edge cases
   - UseUserStyle property setter behavior differences between HeadlessConsole and EmueraConsole

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| Integration with KojoTestRunner/InteractiveRunner | F686 | DisplayModeConsumer exists and is tested but not yet consumed by engine runners. F686 [DRAFT] で `/fc 686` 実行待ち。 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 05:47 | Phase 3 TDD | Task 2 completed by implementer (sonnet). Created engine.Tests/Tests/DisplayModeConsumerTests.cs with MockConsole and 9 test methods for all DisplayMode values. Build fails (RED state) with CS0246 errors as expected - DisplayModeConsumer class not yet implemented. |
| 2026-01-31 05:51 | Phase 3 TDD | Task 1 completed by implementer (sonnet). Created engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs with exhaustive switch for all 9 DisplayMode values. Build succeeds with warning CS8524 (exhaustive switch without discard pattern). |
| 2026-01-31 | DEVIATION | Build | dotnet build engine | PRE-EXISTING: DisplayModeCapture.cs (F678) missing using System for ThreadStaticAttribute. Fixed by adding using directive. Root cause: F678 incomplete implementation. |
