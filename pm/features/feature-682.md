# Feature 682: Consumer-Side Display Mode Interpretation

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Implement consumer-side display mode interpretation for Era.Core's dialogue pipeline. F676 propagates displayMode metadata through DialogueResult.DialogueLines, but no consumer currently interprets it for rendering behavior. This feature adds display mode interpretation to HeadlessUI (wait prompts, key-wait pauses) and potentially other consumers.

---


## Links

- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Predecessor - added DisplayMode propagation)
- [feature-677.md](feature-677.md) - KojoComparer displayMode awareness (Related)
- [feature-678.md](feature-678.md) - ERB↔YAML DisplayMode Equivalence Comparison (Related)
- [feature-681.md](feature-681.md) - Multi-entry selection and rendering pipeline (Related)
- [feature-683.md](feature-683.md) - DialogueResult.Lines Obsolete Deprecation (Successor)
- [feature-684.md](feature-684.md) - GUI Consumer Display Mode Interpretation (Successor)
- [feature-685.md](feature-685.md) - HeadlessUI Console.SetOut Cleanup Fix (Successor)

---

## Notes

- Created by F676 残課題 (deferred item)
- Scope: HeadlessUI wait prompts, rendering behavior implementation
- Separate from F677 (comparison-only)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: HeadlessUI outputs all dialogue lines identically via `Console.WriteLine($"[Dialogue] {line}")`, ignoring display mode metadata
2. Why: HeadlessUI.OutputDialogue() reads `dialogue.Lines` (text-only `IReadOnlyList<string>`), not `dialogue.DialogueLines` (structured `IReadOnlyList<DialogueLine>` with DisplayMode)
3. Why: HeadlessUI was written before F676 added the DialogueLine/DisplayMode infrastructure -- it predates structured dialogue metadata
4. Why: F676 deliberately excluded consumer-side interpretation from scope ("Display mode interpretation: KojoEngine propagates metadata but does not interpret it. Rendering behavior is consumer responsibility.")
5. Why: The original dialogue pipeline architecture treated all output uniformly as "print each line with newline" (PRINTL equivalent). Display variant semantics (wait, key-wait, display mode) were deferred as consumer responsibility to allow F676 to land without cascading changes

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| HeadlessUI does not pause on Wait/KeyWait display modes | HeadlessUI reads `dialogue.Lines` (text-only) and has no display mode interpretation logic |
| YamlComExecutor flattens dialogue to plain text via `string.Join("\n", dialogue.Lines)` | YamlComExecutor reads text-only Lines property, discarding all display metadata |
| All dialogue lines render identically regardless of DisplayMode value | No consumer in the codebase reads `DialogueResult.DialogueLines` or interprets `DisplayMode` values |

### Conclusion

The root cause is a **consumer adoption gap**: F676 successfully propagated DisplayMode metadata through the pipeline (DialogueEntry → KojoEngine → DialogueResult.DialogueLines), but no consumer reads the structured `DialogueLines` property. All three consumers (HeadlessUI, YamlComExecutor, KojoComparer/YamlRunner) continue to read `dialogue.Lines` or `dialogueResult.Lines`, which is the text-only backward-compatibility property.

The fix requires updating consumer code to:
1. Read `DialogueLines` instead of `Lines`
2. Implement display-mode-specific behavior per DisplayMode value
3. Maintain backward compatibility for consumers that do not need display mode interpretation (YamlComExecutor can stay on text-only Lines)

**Key finding**: HeadlessUI is the primary consumer that needs display mode interpretation. It currently has no wait/pause infrastructure -- it writes directly to `Console.WriteLine` with no input blocking. The `IConsoleOutput` interface already defines `PrintWait()` (wait for input) and `PrintLine()` (newline) semantics, but HeadlessUI does **not** implement `IConsoleOutput`. HeadlessUI is a standalone class with its own `OutputDialogue()`, `OutputState()`, and `ReadInput()` methods. The `IConsoleOutput` interface is used by the ERB runtime print command handlers (`PrintHandler`, `PrintLHandler`, `PrintWHandler`), which are a separate code path from YAML dialogue rendering.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F676 | [DONE] | Predecessor | Added DisplayMode enum, DialogueLine record, and DialogueResult.DialogueLines. F682 was created as F676 残課題. |
| F677 | [PROPOSED] | Related (parallel) | KojoComparer displayMode awareness. F677 extends KojoComparer (comparison consumer), F682 extends HeadlessUI (rendering consumer). Independent scopes. |
| F678 | [DRAFT] | Related (downstream) | ERB↔YAML DisplayMode Equivalence Comparison. Depends on F677. Not directly related to F682. |
| F681 | [DRAFT] | Related | Multi-entry selection and rendering pipeline. If multi-entry is implemented, display mode interpretation must handle per-entry DisplayMode values across multiple entries. |
| F683 | [DRAFT] | Successor | DialogueResult.Lines Obsolete Deprecation. F683 depends on F682 (and F677) migrating consumers to DialogueLines before Lines can be deprecated. |

### Pattern Analysis

This is the **consumer adoption phase** of the incremental metadata propagation pattern identified in F676:
- F671: Added displayMode to YAML schema/converter
- F676: Extended runtime pipeline to propagate displayMode through DialogueResult
- **F682 (this)**: Consumer-side interpretation of displayMode in HeadlessUI
- F677: Consumer-side awareness of displayMode in KojoComparer
- F683: Deprecation of text-only Lines property after consumer migration

Each stage moves displayMode one layer closer to user-visible behavior. F682 is the first feature where displayMode actually affects runtime output.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | HeadlessUI.OutputDialogue() can switch from `dialogue.Lines` to `dialogue.DialogueLines` and branch on `DisplayMode` enum values. All 9 enum values are well-defined with clear ERB equivalents (PRINTDATA/PRINTDATAL/PRINTDATAW/etc.). |
| Scope is realistic | YES | HeadlessUI is 47 lines. Adding display mode interpretation requires ~30-50 lines of switch logic. No architectural changes needed. YamlComExecutor change is optional (can stay on Lines). |
| No blocking constraints | YES | F676 is [DONE]. DialogueResult.DialogueLines is available. DisplayMode enum is defined. HeadlessUI has Console.ReadLine() via ReadInput() for wait semantics. No external blockers. |

**Verdict**: FEASIBLE

**Key considerations**:
- HeadlessUI operates in two modes: interactive (Console.ReadLine) and scripted (ReadScriptedInput with Queue). Wait/KeyWait behavior must account for both modes.
- In scripted/test mode, Wait/KeyWait should likely be no-ops or emit markers (e.g., `[WAIT]`) rather than blocking on Console.ReadLine. Otherwise automated tests would hang.
- The `Display` mode variants (Display, DisplayNewline, DisplayWait) in Emuera refer to "display style" rendering (different font/color). In headless mode, this maps to either a marker or no-op since console has no rich formatting.
- Only `Newline` (PRINTDATAL) is actually used in current kojo files (1,575 occurrences). Other variants have 0 occurrences. However, F682 should support all variants per F676's philosophy: "Every display variant defined in dialogue-schema.json must be propagable through the runtime pipeline."

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F676 | [DONE] | DisplayMode propagation through DialogueResult pipeline. Provides DialogueLine record with DisplayMode property. |
| Related | F677 | [PROPOSED] | KojoComparer displayMode awareness. Separate consumer, independent scope. |
| Related | F681 | [DRAFT] | Multi-entry selection. May affect how per-entry DisplayMode is surfaced. |
| Successor | F683 | [DRAFT] | DialogueResult.Lines deprecation. Depends on F682 completing consumer migration. |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Console | Runtime | Low | Console.ReadLine() for wait/key-wait semantics. Already used by HeadlessUI.ReadInput(). |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core/HeadlessUI.cs | HIGH | Primary target. Currently reads dialogue.Lines (line 19). Must migrate to DialogueLines and interpret DisplayMode. |
| Era.Core/Commands/Com/YamlComExecutor.cs | LOW | Reads dialogue.Lines (line 229) for text output. Display mode interpretation is optional here -- COM execution returns a message string, not structured rendering. Can stay on Lines. |
| tools/KojoComparer/YamlRunner.cs | NONE | F677's responsibility, not F682. YamlRunner is a comparison tool, not a rendering consumer. |
| Era.Core.Tests/HeadlessUITests.cs | MEDIUM | Tests for HeadlessUI.OutputDialogue() must be updated to verify display-mode-specific behavior. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/HeadlessUI.cs | Update | Migrate OutputDialogue() from dialogue.Lines to dialogue.DialogueLines. Add display mode interpretation (switch on DisplayMode enum). |
| Era.Core.Tests/HeadlessUITests.cs | Update | Add test cases for each DisplayMode behavior (Default, Newline, Wait, KeyWait, etc.). Update existing tests for new output format. |
| Era.Core/Commands/Com/YamlComExecutor.cs | Optional Update | Could migrate from dialogue.Lines to dialogue.DialogueLines for structured rendering. Low priority -- current text-only behavior is acceptable for COM execution. |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| HeadlessUI does not implement IConsoleOutput | Era.Core/HeadlessUI.cs | MEDIUM - Cannot reuse IConsoleOutput's Print/PrintLine/PrintWait abstraction. Must implement display mode behavior directly in OutputDialogue(). |
| HeadlessUI has two input modes (interactive Console.ReadLine vs scripted Queue) | Era.Core/HeadlessUI.cs lines 35-46 | HIGH - Wait/KeyWait must not block in scripted mode. Need strategy: no-op, marker output, or configurable behavior. |
| Only Newline (PRINTDATAL) is used in current kojo files | F676 investigation (1,575 occurrences) | LOW - Practical impact limited to Newline behavior, but all 9 enum values must be handled. |
| DisplayMode is per-entry, applied to all lines from same entry | Era.Core/KojoEngine.cs line 89 | LOW - Each DialogueLine in DialogueResult carries its own DisplayMode. Consumer can iterate straightforwardly. |
| Display variants (Display, DisplayNewline, DisplayWait) refer to Emuera display-style rendering | Emuera PRINTDATAD semantics | MEDIUM - Headless console cannot reproduce visual display style. These modes must map to a reasonable console equivalent or be treated as markers. |

## Background

### Philosophy (Mid-term Vision)

Every display variant defined in the dialogue schema must be interpretable by runtime consumers. The headless pipeline should faithfully represent display mode semantics so that automated testing can verify rendering behavior without a GUI, and so that future consumers have a reference implementation for display mode interpretation.

### Problem (Current Issue)

HeadlessUI reads `dialogue.Lines` (text-only `IReadOnlyList<string>`) and outputs all dialogue lines identically via `Console.WriteLine($"[Dialogue] {line}")`. F676 added `DialogueResult.DialogueLines` with per-line `DisplayMode` metadata, but no consumer reads it. Wait/KeyWait/Display semantics are invisible in headless output.

### Goal (What to Achieve)

Migrate HeadlessUI.OutputDialogue() from `dialogue.Lines` to `dialogue.DialogueLines`, implement a DisplayMode switch that produces observable differences in console output for each mode variant, and ensure Wait/KeyWait modes emit markers in scripted mode (not blocking) while supporting interactive wait in interactive mode.

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Every display variant must be interpretable" | All 9 DisplayMode enum values handled in switch | AC#3, AC#4 |
| "faithfully represent display mode semantics" | Each mode produces distinct, observable output | AC#5, AC#6, AC#7, AC#8 |
| "automated testing can verify rendering behavior" | Scripted mode emits markers instead of blocking | AC#9, AC#10 |
| "reference implementation for display mode interpretation" | Code uses DialogueLines, not Lines | AC#2 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | HeadlessUI.cs compiles successfully | build | dotnet build Era.Core | succeeds | - | [x] |
| 2 | OutputDialogue uses DialogueLines (Pos) | code | Grep(Era.Core/HeadlessUI.cs) | contains | "dialogue.DialogueLines" | [x] |
| 3 | OutputDialogue does not use Lines (Neg) | code | Grep(Era.Core/HeadlessUI.cs) | not_contains | "dialogue.Lines" | [x] |
| 4 | DisplayMode switch references DisplayWait (spot-check supplement to AC#1) | code | Grep(Era.Core/HeadlessUI.cs) | contains | "DisplayMode.DisplayWait" | [x] |
| 5 | Default mode outputs [Dialogue] prefix (Pos) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDefault | succeeds | - | [x] |
| 6 | Newline mode outputs [Dialogue] with newline marker (Pos) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeNewline | succeeds | - | [x] |
| 7 | Wait mode outputs [WAIT] marker (Pos) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeWait | succeeds | - | [x] |
| 8 | KeyWait mode outputs [KEY_WAIT] marker (Pos) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWait | succeeds | - | [x] |
| 9 | Display mode outputs [DISPLAY] marker (Pos) | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplay | succeeds | - | [x] |
| 10 | All HeadlessUI tests pass | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~HeadlessUI | succeeds | - | [x] |
| 11 | Zero technical debt - TODO | code | Grep(Era.Core/HeadlessUI.cs) | not_contains | "TODO" | [x] |
| 12 | F684 DRAFT created | file | File(Game/agents/feature-684.md) | contains | "[DRAFT]" | [x] |
| 13 | F684 registered in index | file | File(Game/agents/index-features.md) | contains | "F684" | [x] |
| 14 | F685 DRAFT created | file | File(Game/agents/feature-685.md) | contains | "[DRAFT]" | [x] |
| 15 | F685 registered in index | file | File(Game/agents/index-features.md) | contains | "F685" | [x] |
| 16 | Zero technical debt - FIXME | code | Grep(Era.Core/HeadlessUI.cs) | not_contains | "FIXME" | [x] |
| 17 | Zero technical debt - HACK | code | Grep(Era.Core/HeadlessUI.cs) | not_contains | "HACK" | [x] |
| 18 | F684 contains GUI summary | file | File(Game/agents/feature-684.md) | contains | "GUI Consumer Display Mode" | [x] |
| 19 | F685 contains cleanup summary | file | File(Game/agents/feature-685.md) | contains | "Console.SetOut Cleanup" | [x] |
| 20 | DisplayWait mode outputs [DISPLAY] with [WAIT] marker | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplayWait | succeeds | - | [x] |
| 21 | KeyWaitNewline mode outputs [KEY_WAIT] marker | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWaitNewline | succeeds | - | [x] |
| 22 | KeyWaitWait mode outputs [KEY_WAIT] marker | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWaitWait | succeeds | - | [x] |
| 23 | DisplayNewline mode outputs [DISPLAY] marker | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplayNewline | succeeds | - | [x] |
| 24 | No discard pattern in switch expression | code | Grep(Era.Core/HeadlessUI.cs) | not_contains | "_ =>" | [x] |

### AC Details

**AC#1: HeadlessUI.cs compiles successfully**
- Test: `dotnet build Era.Core`
- Ensures the DisplayMode switch and DialogueLines migration compile without errors
- Must pass before any other AC

**AC#2: OutputDialogue uses DialogueLines (Pos)**
- Test: Grep pattern=`dialogue.DialogueLines` path=`Era.Core/HeadlessUI.cs`
- Expected: At least 1 match, confirming migration from text-only Lines to structured DialogueLines
- This is the core migration: iterating over `DialogueLine` records instead of plain strings

**AC#3: OutputDialogue does not use Lines (Neg)**
- Test: Grep pattern=`dialogue.Lines` path=`Era.Core/HeadlessUI.cs`
- Expected: 0 matches (not_contains)
- Ensures complete migration away from the text-only `Lines` property
- Pattern catches any access to `dialogue.Lines` including iteration, property access, or LINQ operations

**AC#4: DisplayMode switch covers all 9 values**
- Test: Grep pattern=`DisplayMode.DisplayWait` path=`Era.Core/HeadlessUI.cs`
- Expected: At least 1 match
- DisplayWait is the last enum value. Its presence is a spot-check for switch coverage
- Primary guarantee: AC#1 (build success) + Implementation Contract constraint #2 (exhaustive switch expression) ensures compiler-enforced exhaustiveness. AC#4 is supplementary verification.

**AC#5: Default mode outputs [Dialogue] prefix (Pos)**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDefault`
- DisplayMode.Default (PRINTDATA): Standard line output with `[Dialogue]` prefix, followed by newline
- This is the most common mode and should behave identically to current behavior

**AC#6: Newline mode outputs [Dialogue] with newline marker (Pos)**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeNewline`
- DisplayMode.Newline (PRINTDATAL): Same as Default (both use PRINTL-equivalent in headless)
- In Emuera, PRINTDATAL differs from PRINTDATA only in forced newline behavior. In headless mode (Console.WriteLine), both result in newline. Output should still show `[Dialogue]` prefix.
- **Note**: Output is intentionally identical to AC#5 (Default). The verification goal is code path coverage (Newline switch arm is exercised), not output differentiation. See Technical Design Decision 5.

**AC#7: Wait mode outputs [WAIT] marker (Pos)**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeWait`
- DisplayMode.Wait (PRINTDATAW): After outputting the line, emit `[WAIT]` marker
- In headless mode, this marker signals to test harnesses that a wait-for-input point was encountered
- Must NOT block (no Console.ReadLine) in test context

**AC#8: KeyWait mode outputs [KEY_WAIT] marker (Pos)**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWait`
- DisplayMode.KeyWait (PRINTDATAK): After outputting the line, emit `[KEY_WAIT]` marker
- Covers KeyWait, KeyWaitNewline, KeyWaitWait variants (all produce `[KEY_WAIT]` marker)

**AC#9: Display mode outputs [DISPLAY] marker (Pos)**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplay`
- DisplayMode.Display (PRINTDATAD): Output with `[DISPLAY]` prefix instead of `[Dialogue]`
- Covers Display, DisplayNewline, DisplayWait variants
- In Emuera, "display" mode uses different rendering style. In headless, the prefix change makes this observable

**AC#10: All HeadlessUI tests pass**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~HeadlessUI`
- Ensures existing tests (F479) still pass after migration, plus all new display mode tests
- Regression safety net

**AC#11: Zero technical debt - TODO**
- Test: Grep pattern=`TODO` path=`Era.Core/HeadlessUI.cs`
- Expected: No TODO comments in implementation
- Ensures no deferred work items left in the implementation

**AC#16: Zero technical debt - FIXME**
- Test: Grep pattern=`FIXME` path=`Era.Core/HeadlessUI.cs`
- Expected: No FIXME comments in implementation
- Ensures no known bugs left unaddressed

**AC#17: Zero technical debt - HACK**
- Test: Grep pattern=`HACK` path=`Era.Core/HeadlessUI.cs`
- Expected: No HACK comments in implementation
- Ensures no temporary workarounds left in place

**AC#20: DisplayWait mode outputs [DISPLAY] with [WAIT] marker**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplayWait`
- DisplayMode.DisplayWait (PRINTDATADW): Output with `[DISPLAY]` prefix followed by `[WAIT]` marker
- Verifies the compound behavior: display-style rendering with wait semantics

**AC#21: KeyWaitNewline mode outputs [KEY_WAIT] marker**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWaitNewline`
- DisplayMode.KeyWaitNewline (PRINTDATAKL): After outputting the line with newline, emit `[KEY_WAIT]` marker
- Covers the newline variant of key-wait behavior

**AC#22: KeyWaitWait mode outputs [KEY_WAIT] marker**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeKeyWaitWait`
- DisplayMode.KeyWaitWait (PRINTDATAKW): After outputting the line, emit `[KEY_WAIT]` marker
- Covers the compound key-wait variant with additional wait semantics

**AC#23: DisplayNewline mode outputs [DISPLAY] marker**
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~DisplayModeDisplayNewline`
- DisplayMode.DisplayNewline (PRINTDATADL): Output with `[DISPLAY]` prefix
- Covers the newline variant of display-style rendering

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Wait/KeyWait blocks automated tests | High | High | Detect scripted mode (check if scripted input queue is available). In scripted mode, Wait/KeyWait outputs a marker `[WAIT]`/`[KEY_WAIT]` instead of blocking. |
| Display mode console output breaks existing test assertions | Medium | Medium | Existing HeadlessUITests use dialogue.Lines (text-only). Update tests to use DialogueResult.Create() with appropriate DisplayMode values. |
| YamlComExecutor behavior regression if migrated | Low | Medium | Keep YamlComExecutor on dialogue.Lines (text-only). Display mode interpretation is not needed for COM execution result messages. |
| Display variants (PRINTDATAD/DL/DW) have no console equivalent | Medium | Low | Map to reasonable console behavior: Display → Print (inline), DisplayNewline → PrintLine, DisplayWait → PrintWait. Document the mapping. |
| HeadlessUI.OutputDialogue signature change breaks callers | Low | Medium | Keep method signature `OutputDialogue(DialogueResult dialogue)` unchanged. Internal implementation changes only. |

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2,3,4,24 | Migrate HeadlessUI.OutputDialogue from Lines to DialogueLines with DisplayMode switch | [x] |
| 2 | 5,6,7,8,9,20,21,22,23 | Add 9 DisplayMode test methods to HeadlessUITests.cs | [x] |
| 3 | 1,10,11,16,17 | Verify build success and zero technical debt | [x] |
| 4 | 12,13,18 | Create F684 (GUI Consumer Display Mode Interpretation) DRAFT feature | [x] |
| 5 | 14,15,19 | Create F685 (HeadlessUI Console.SetOut Cleanup Fix) DRAFT feature | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design DisplayMode switch logic | HeadlessUI.OutputDialogue updated |
| 2 | implementer | sonnet | T2 | Technical Design test method specifications | 9 new test methods in HeadlessUITests.cs |
| 3 | ac-tester | haiku | T3 | AC#1,10,11 commands from AC Details | Verification results |

**Constraints** (from Technical Design):
1. HeadlessUI.OutputDialogue signature must remain `OutputDialogue(DialogueResult dialogue)` - internal implementation only
2. Switch expression must be exhaustive with all 9 DisplayMode enum values covered. Must NOT use default/discard pattern (_ =>) to enable compiler warnings on future enum additions.
3. Wait/KeyWait modes must emit markers (`[WAIT]`, `[KEY_WAIT]`) instead of blocking on Console.ReadLine()
4. Display variants use `[DISPLAY]` prefix instead of `[Dialogue]`
5. Console.WriteLine() used for all output - each logical output (line + marker) is a separate Console.WriteLine call (no embedded `\n`)
6. Test methods use StringWriter to capture Console output for assertions
7. All test methods must use `DialogueResult.Create()` with appropriate DisplayMode values
8. Existing test cleanup patterns are NOT modified in F682 scope (TestOutputDialogueValid cleanup remains buggy, deferred to F685)
9. F684 DRAFT must contain 'GUI Consumer Display Mode Interpretation' summary and links back to F682
10. F685 DRAFT must contain 'HeadlessUI Console.SetOut Cleanup' summary and links back to F682

**Pre-conditions**:
- F676 [DONE] - DialogueResult.DialogueLines property exists with DisplayMode metadata
- DisplayMode enum defined with all 9 values (Default, Newline, Wait, KeyWait, KeyWaitNewline, KeyWaitWait, Display, DisplayNewline, DisplayWait)
- HeadlessUI.OutputDialogue() currently reads `dialogue.Lines` (text-only property)
- Era.Core.Tests/HeadlessUITests.cs exists with TestOutputDialogueValid() test

**Success Criteria**:
- HeadlessUI.OutputDialogue() iterates over `dialogue.DialogueLines` (not `dialogue.Lines`)
- Switch expression produces distinct observable output for each DisplayMode value
- All 9 new test methods pass (AC#5-9, AC#20-23)
- Existing HeadlessUITests continue to pass (AC#10)
- No TODO/FIXME/HACK markers in HeadlessUI.cs (AC#11)
- Build succeeds (AC#1)

**Test Naming Convention**: Test methods follow `TestDisplayMode{Variant}` format (e.g., `TestDisplayModeDefault`, `TestDisplayModeNewline`). This ensures AC filter patterns `FullyQualifiedName~DisplayMode{Variant}` match correctly.

**DisplayMode Output Format**:

| DisplayMode | Console Output Pattern |
|-------------|------------------------|
| Default | `[Dialogue] {text}` |
| Newline | `[Dialogue] {text}` |
| Wait | `[Dialogue] {text}` + newline + `[WAIT]` |
| KeyWait | `[Dialogue] {text}` + newline + `[KEY_WAIT]` |
| KeyWaitNewline | `[Dialogue] {text}` + newline + `[KEY_WAIT]` |
| KeyWaitWait | `[Dialogue] {text}` + newline + `[KEY_WAIT]` |
| Display | `[DISPLAY] {text}` |
| DisplayNewline | `[DISPLAY] {text}` |
| DisplayWait | `[DISPLAY] {text}` + newline + `[WAIT]` |

**Test Setup Pattern** (all test methods):
```csharp
var headlessUI = new HeadlessUI();
var dialogue = DialogueResult.Create(new List<DialogueLine>
{
    new("Test line", DisplayMode.{Variant})
});

var originalOut = Console.Out;
var stringWriter = new StringWriter();
Console.SetOut(stringWriter);
headlessUI.OutputDialogue(dialogue);

var output = stringWriter.ToString();
Assert.Contains("{expected pattern}", output);

Console.SetOut(originalOut);
stringWriter.Dispose();
```

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

**Scope Exclusions**:
- YamlComExecutor migration to DialogueLines - Tracked in F683 (F683 handles deprecation after all consumer migration including YamlComExecutor)
- Interactive blocking behavior for Wait/KeyWait - Deferred to F684 (GUI Consumer Display Mode Interpretation)
- Rich console formatting for Display mode - Deferred to F684 (GUI Consumer Display Mode Interpretation)
- Multi-entry display mode handling - Tracked in F681 (Multi-entry selection and rendering pipeline)

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| Fix existing TestOutputDialogueValid Console.SetOut cleanup pattern | F685 | Pre-existing issue: line 51 uses Console.SetOut(Console.Out) which is no-op after SetOut(stringWriter) |

## Review Notes

- [resolved-applied] Phase1-Uncertain iter1: AC#4 uses presence of 'DisplayMode.DisplayWait' as proxy for exhaustive switch coverage of all 9 values. This is a weak proxy - the code could reference DisplayWait without handling all other values. The AC Details section acknowledges compiler exhaustiveness checking as the real guarantee, but the AC itself does not verify this. RESOLUTION: Updated AC#4 Details to clarify that AC#1 (build) + Implementation Contract constraint #2 provide the primary guarantee; AC#4 is supplementary verification.
- [resolved-deferred] Phase1-Uncertain iter7: Pre-existing bug identification (Console.SetOut(Console.Out) on line 51 is a no-op after SetOut(stringWriter)) is correct. However, the fix contradicts Issue 0: Issue 0 recommends removing Task#4 and deferring cleanup to 残課題, while Issue 3 recommends folding Task#4's fix into Task#2 scope with AC verification. These two fixes cannot both be applied as written. The reviewer should consolidate into one coherent recommendation. Additionally, practical impact is uncertain because xUnit runs each test method in isolation and the bug may not cause observable failures in practice. RESOLUTION: Issue 0 was applied - old Task#4 (Console.SetOut fix) was removed and Console.SetOut issue deferred to 残課題 with destination F685. Current Task#4 is repurposed for F684 DRAFT creation. Contradiction resolved.
- [resolved-invalid] Phase1-Uncertain iter9: The F685-has-no-Task part is valid (duplicate of Issue 2). However, the claim that 'F684 is not mentioned in Related Features table' is inaccurate -- F684 does not need to be in Related Features because it is tracked via AC#12-13 and Task#4. Related Features documents cross-feature relationships for existing features, not features-to-be-created. The fix suggestion to add Task#5 for F685 is correct per TBD Prohibition, but adding F684 to Related Features is not required by SSOT. RESOLUTION: F685 Task already added (Task#5). F684 Related Features claim invalid per SSOT.
- [resolved-invalid] Phase1-Uncertain iter9: The observation is factually correct (Console.SetOut(Console.Out) on line 51 is indeed a no-op bug, and the new test pattern correctly captures originalOut). However, the fix recommendation to add an explicit note in Implementation Contract is purely advisory -- the feature already tracks this via 残課題 with destination F685. Adding the note is a 'nice to have' defensive clarification, not a correctness requirement. Whether this constitutes a reviewable issue vs. editorial suggestion is ambiguous. RESOLUTION: Advisory only. Already tracked via 残課題→F685. Implementation Contract constraint #8 already documents this.
- [resolved-applied] Phase0-RefCheck iter1: F684 and F685 do not exist yet but are referenced as acceptance criteria requiring their creation. AC#12 requires F684 [DRAFT] file creation, AC#13 requires F684 registration in index, AC#14 requires F685 [DRAFT] file creation, AC#15 requires F685 registration in index. These are forward-facing ACs dependent on Task#4-5 completion.
- [resolved-applied] Phase0-RefCheck iter1: F684 and F685 DRAFT feature files created and registered in index (file creation prerequisite). Execute Task#4 to create F684.md [DRAFT] with 'GUI Consumer Display Mode Interpretation' summary; Execute Task#5 to create F685.md [DRAFT] with 'Console.SetOut Cleanup' summary; Register both in index-features.md.
- [resolved-applied] Phase0-RefCheck iter1: F684 and F685 DRAFT files created, references now valid. AC#12-15 are forward-facing (dependent on Task#4-5 completion). Create F684.md [DRAFT] and F685.md [DRAFT] files with minimal content before running /run, or mark ACs#12-15 as conditional (dependent on Task#4-5 completion). Currently F684 and F685 are not in index-features.md.
- [resolved-applied] Phase1-Uncertain iter1: F678 appears in Related Features table (line 71) but not in Links section (lines 18-26). Inconsistency between Links and Related Features sections. The inconsistency is factually correct but there is no explicit SSOT rule requiring perfect alignment between these sections. Adding F678 to Links section is a reasonable consistency improvement.
- [resolved-invalid] Phase2-Maintainability iter3: AC#3 not_contains matcher for 'dialogue.Lines' was claimed to false-positive on 'dialogue.DialogueLines'. Validation confirmed this is factually incorrect - 'dialogue.Lines' is NOT a substring of 'dialogue.DialogueLines' (after 'dialogue.' the next char is 'D' not 'L'). AC#3 is correct as-is.
- [resolved-deferred] Phase2-Maintainability iter3: All 9 new test methods lack try/finally for Console.SetOut cleanup. Deferred to F685 for unified fix across all HeadlessUITests (existing + new). F685 Notes updated with handoff.
- [resolved-applied] Phase2-Maintainability iter3: Switch expression changed from embedded '\\n' to separate Console.WriteLine calls for markers. Prefix switch expression + marker switch statement pattern adopted. Implementation Details updated.
- [resolved-applied] Phase2-Maintainability iter3: AC#6 Details updated with note clarifying output equivalence to AC#5 is intentional. Code path coverage is the verification goal.
- [resolved-applied] Phase6-FinalRefCheck iter3: F684 and F685 DRAFT files created during POST-LOOP. These are forward-facing dependencies that require Task#4 and Task#5 completion before /run. Links section contains F684/F685 references but actual files must be created via Tasks.

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-30 20:27 | Phase 3 TDD (Task#2) | Added 9 DisplayMode test methods to HeadlessUITests.cs. Build succeeds, tests FAIL (RED state) as expected. 7 tests fail (Wait/KeyWait/Display modes), 16 tests pass (Default/Newline + existing tests). |
| 2026-01-30 20:31 | Implementation (Task#1) | Migrated HeadlessUI.OutputDialogue from dialogue.Lines to dialogue.DialogueLines. Implemented exhaustive DisplayMode switch expression with prefix selection and marker emission. Era.Core builds successfully. All 17 HeadlessUI tests pass (GREEN). |
| 2026-01-30 | DEVIATION | Bash | git commit | exit code 1: pre-commit hook build failure CS0535 KojoEngine missing GetDialogueMulti. Pre-existing unstaged change in IKojoEngine.cs/KojoEngine.cs (not F682 scope). Resolved by git stash --keep-index. |

---

## Technical Design

### Approach

**Core Strategy**: Migrate HeadlessUI.OutputDialogue() from the text-only `dialogue.Lines` property to the structured `dialogue.DialogueLines` property, implementing a switch statement that interprets each of the 9 `DisplayMode` enum values to produce distinct console output. Wait/KeyWait modes will emit markers (`[WAIT]`, `[KEY_WAIT]`) instead of blocking, enabling test automation without hangs.

**Implementation Steps**:

1. **Migration from Lines to DialogueLines** (AC#2, AC#3)
   - Change iteration target: `foreach (var line in dialogue.Lines)` → `foreach (var dialogueLine in dialogue.DialogueLines)`
   - Access text via `dialogueLine.Text`, display mode via `dialogueLine.DisplayMode`

2. **DisplayMode Switch Implementation** (AC#4)
   - Implement exhaustive switch expression on `dialogueLine.DisplayMode`
   - Handle all 9 enum values: Default, Newline, Wait, KeyWait, KeyWaitNewline, KeyWaitWait, Display, DisplayNewline, DisplayWait
   - Use pattern matching to enforce compile-time exhaustiveness

3. **Mode-Specific Output Formatting** (AC#5-9)
   - **Default/Newline**: Output `[Dialogue] {text}` (standard behavior, AC#5, AC#6)
   - **Wait/KeyWait variants**: Output `[Dialogue] {text}` followed by `[WAIT]` or `[KEY_WAIT]` marker (AC#7, AC#8)
   - **Display variants**: Output `[DISPLAY] {text}` with appropriate wait markers for DisplayWait (AC#9)

4. **Test Coverage** (AC#5-10, AC#20-23)
   - Add 9 new test methods to HeadlessUITests.cs:
     - `TestDisplayModeDefault()` - verifies `[Dialogue]` prefix
     - `TestDisplayModeNewline()` - verifies `[Dialogue]` prefix (same as Default in console)
     - `TestDisplayModeWait()` - verifies `[WAIT]` marker emission
     - `TestDisplayModeKeyWait()` - verifies `[KEY_WAIT]` marker emission
     - `TestDisplayModeDisplay()` - verifies `[DISPLAY]` prefix
   - Update existing test `TestOutputDialogueValid()` to continue using DialogueResult.Create() with DisplayMode.Default

### DisplayMode Mapping

| DisplayMode | ERB Equivalent | Console Output | Rationale |
|-------------|----------------|----------------|-----------|
| Default | PRINTDATA | `[Dialogue] {text}` | Standard line output, newline implicit in Console.WriteLine |
| Newline | PRINTDATAL | `[Dialogue] {text}` | In console, WriteLine always adds newline. Behaves identically to Default. |
| Wait | PRINTDATAW | `[Dialogue] {text}` + `[WAIT]` | Marker indicates wait-for-input point. Non-blocking for test automation. |
| KeyWait | PRINTDATAK | `[Dialogue] {text}` + `[KEY_WAIT]` | Marker indicates key-wait point. Non-blocking for test automation. |
| KeyWaitNewline | PRINTDATAKL | `[Dialogue] {text}` + `[KEY_WAIT]` | Newline implicit in WriteLine. Marker indicates key-wait. |
| KeyWaitWait | PRINTDATAKW | `[Dialogue] {text}` + `[KEY_WAIT]` | Combined key-wait semantics. Single marker for simplicity. |
| Display | PRINTDATAD | `[DISPLAY] {text}` | Display-style rendering indicated by prefix change. |
| DisplayNewline | PRINTDATADL | `[DISPLAY] {text}` | Display-style with newline (implicit in WriteLine). |
| DisplayWait | PRINTDATADW | `[DISPLAY] {text}` + `[WAIT]` | Display-style with wait marker. |

**Key Design Decisions**:
- **Marker-based wait semantics**: Emit `[WAIT]`/`[KEY_WAIT]` markers instead of blocking on Console.ReadLine(). This enables test automation without hangs and provides observable behavior for verification.
- **Prefix-based display mode differentiation**: Use `[DISPLAY]` prefix for Display variants to make display mode observable in console output.
- **Newline handling**: Console.WriteLine() always adds newline. Newline variants (PRINTDATAL, PRINTDATAKL, PRINTDATADL) behave identically to their non-newline counterparts in console context.
- **KeyWait variant unification**: KeyWait, KeyWaitNewline, KeyWaitWait all emit `[KEY_WAIT]` marker. Subtle differences between variants are not meaningful in headless console context.

### AC Coverage Mapping

| AC# | Coverage | Implementation |
|:---:|----------|----------------|
| 1 | Build success | Compile-time validation of switch exhaustiveness via pattern matching. xUnit test method attributes compile. |
| 2 | Uses DialogueLines | Iteration over `dialogue.DialogueLines` in OutputDialogue(). Grep verification: pattern="dialogue.DialogueLines" matches. |
| 3 | Does not use Lines | No references to `dialogue.Lines` in OutputDialogue(). Grep verification: pattern="dialogue.Lines" has 0 matches. |
| 4 | Handles all 9 DisplayMode values | Switch expression covers all enum values. Grep verification: pattern="DisplayMode.DisplayWait" matches (last enum value). |
| 5 | Default mode test | New test `TestDisplayModeDefault()` asserts output contains `[Dialogue]` prefix. Test name filter: `FullyQualifiedName~DisplayModeDefault`. |
| 6 | Newline mode test | New test `TestDisplayModeNewline()` asserts output contains `[Dialogue]` prefix. Test name filter: `FullyQualifiedName~DisplayModeNewline`. |
| 7 | Wait mode test | New test `TestDisplayModeWait()` asserts output contains `[WAIT]` marker. Test name filter: `FullyQualifiedName~DisplayModeWait`. |
| 8 | KeyWait mode test | New test `TestDisplayModeKeyWait()` asserts output contains `[KEY_WAIT]` marker. Test name filter: `FullyQualifiedName~DisplayModeKeyWait`. |
| 9 | Display mode test | New test `TestDisplayModeDisplay()` asserts output contains `[DISPLAY]` prefix. Test name filter: `FullyQualifiedName~DisplayModeDisplay`. |
| 10 | All HeadlessUI tests pass | Existing tests continue to pass (using DialogueResult.Create with DisplayMode.Default). New tests added. Filter: `FullyQualifiedName~HeadlessUI` runs all. |
| 11 | Zero technical debt - TODO | No TODO comments in implementation. Grep verification: pattern="TODO" has 0 matches in HeadlessUI.cs. |
| 16 | Zero technical debt - FIXME | No FIXME comments in implementation. Grep verification: pattern="FIXME" has 0 matches in HeadlessUI.cs. |
| 17 | Zero technical debt - HACK | No HACK comments in implementation. Grep verification: pattern="HACK" has 0 matches in HeadlessUI.cs. |
| 20 | DisplayWait mode test | New test `TestDisplayModeDisplayWait()` asserts output contains both `[DISPLAY]` prefix and `[WAIT]` marker. Test name filter: `FullyQualifiedName~DisplayModeDisplayWait`. |
| 21 | KeyWaitNewline mode test | New test `TestDisplayModeKeyWaitNewline()` asserts output contains `[KEY_WAIT]` marker. Test name filter: `FullyQualifiedName~DisplayModeKeyWaitNewline`. |
| 22 | KeyWaitWait mode test | New test `TestDisplayModeKeyWaitWait()` asserts output contains `[KEY_WAIT]` marker. Test name filter: `FullyQualifiedName~DisplayModeKeyWaitWait`. |
| 23 | DisplayNewline mode test | New test `TestDisplayModeDisplayNewline()` asserts output contains `[DISPLAY]` prefix. Test name filter: `FullyQualifiedName~DisplayModeDisplayNewline`. |

### Key Decisions

**Decision 1: Marker-based wait semantics instead of blocking**
- **Rationale**: HeadlessUI is used in automated test scenarios (F479). Blocking on Console.ReadLine() for Wait/KeyWait modes would hang tests. Markers provide observable behavior for verification while maintaining non-blocking execution.
- **Alternative considered**: Detect scripted mode via a flag or queue check. Rejected because it adds state tracking complexity. Markers are simpler and provide better test observability.
- **Impact**: Wait/KeyWait semantics differ from interactive Emuera behavior (where user must press key). This is acceptable because headless mode is for testing, not user interaction.

**Decision 2: Prefix-based display mode differentiation**
- **Rationale**: Console output has no rich formatting (color, font). Using `[DISPLAY]` prefix makes display mode observable in test assertions while maintaining readability.
- **Alternative considered**: No observable difference (treat Display as Default). Rejected because it violates "faithfully represent display mode semantics" philosophy. Tests could not verify display mode propagation.
- **Impact**: Minimal. Display mode variants have 0 occurrences in current kojo files. Prefix change is future-proof for when Display mode is used.

**Decision 3: Exhaustive switch expression with pattern matching**
- **Rationale**: C# switch expression with pattern matching enforces compile-time exhaustiveness warning. If new DisplayMode enum values are added in the future, the compiler will emit CS8509 warning until the switch is updated.
- **Alternative considered**: if-else chain. Rejected because it lacks compile-time exhaustiveness checking.
- **Impact**: Build emits warning CS8509 if DisplayMode enum is extended without updating HeadlessUI. Warning becomes error if TreatWarningsAsErrors is enabled (fail-fast).

**Decision 4: Keep YamlComExecutor on text-only Lines property**
- **Rationale**: YamlComExecutor returns a plain-text message string for COM execution results. Display mode interpretation is not needed. Migrating YamlComExecutor increases risk without user-visible benefit.
- **Alternative considered**: Migrate YamlComExecutor to DialogueLines for consistency. Rejected because display mode semantics are not meaningful for COM result messages.
- **Impact**: YamlComExecutor continues to use `dialogue.Lines`. This is acceptable -- F683 (Lines deprecation) will address this when all necessary consumers have migrated.

**Decision 5: Newline variants behave identically to non-newline in console**
- **Rationale**: Console.WriteLine() always appends newline. The distinction between PRINTDATA (Default) and PRINTDATAL (Newline) is not observable in console output.
- **Alternative considered**: Emit newline marker (e.g., `[NEWLINE]`). Rejected because it adds noise without conveying meaningful information (newline is always present).
- **Impact**: Newline mode tests will produce identical output to Default mode tests. This is acceptable -- the test verifies that the code path is executed, not that output differs.

### Implementation Details

**File: Era.Core/HeadlessUI.cs**

```csharp
public void OutputDialogue(DialogueResult dialogue)
{
    ArgumentNullException.ThrowIfNull(dialogue);
    foreach (var dialogueLine in dialogue.DialogueLines)
    {
        var prefix = dialogueLine.DisplayMode switch
        {
            DisplayMode.Default => "[Dialogue]",
            DisplayMode.Newline => "[Dialogue]",
            DisplayMode.Wait => "[Dialogue]",
            DisplayMode.KeyWait => "[Dialogue]",
            DisplayMode.KeyWaitNewline => "[Dialogue]",
            DisplayMode.KeyWaitWait => "[Dialogue]",
            DisplayMode.Display => "[DISPLAY]",
            DisplayMode.DisplayNewline => "[DISPLAY]",
            DisplayMode.DisplayWait => "[DISPLAY]"
        };
        Console.WriteLine($"{prefix} {dialogueLine.Text}");

        switch (dialogueLine.DisplayMode)
        {
            case DisplayMode.Wait:
            case DisplayMode.DisplayWait:
                Console.WriteLine("[WAIT]");
                break;
            case DisplayMode.KeyWait:
            case DisplayMode.KeyWaitNewline:
            case DisplayMode.KeyWaitWait:
                Console.WriteLine("[KEY_WAIT]");
                break;
        }
    }
}
```

**Notes**:
- First switch expression determines prefix ([Dialogue] or [DISPLAY]) exhaustively (no default case)
- Compiler warning CS8509 will fire if new enum values are added without updating the prefix switch
- Second switch statement emits wait markers as separate Console.WriteLine calls
- Each output is a distinct Console.WriteLine call — no embedded `\n` in strings

**File: Era.Core.Tests/HeadlessUITests.cs**

Add 9 new test methods in a new region (showing 5 representative methods, remaining 4 follow same pattern):

```csharp
#region AC#5-9 - DisplayMode Interpretation Tests

[Fact]
[Trait("AC", "5")]
public void TestDisplayModeDefault()
{
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(new List<DialogueLine>
    {
        new("Test line", DisplayMode.Default)
    });

    var originalOut = Console.Out;
    var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);
    headlessUI.OutputDialogue(dialogue);

    var output = stringWriter.ToString();
    Assert.Contains("[Dialogue] Test line", output);

    Console.SetOut(originalOut);
    stringWriter.Dispose();
}

[Fact]
[Trait("AC", "6")]
public void TestDisplayModeNewline()
{
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(new List<DialogueLine>
    {
        new("Test line", DisplayMode.Newline)
    });

    var originalOut = Console.Out;
    var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);
    headlessUI.OutputDialogue(dialogue);

    var output = stringWriter.ToString();
    Assert.Contains("[Dialogue] Test line", output);

    Console.SetOut(originalOut);
    stringWriter.Dispose();
}

[Fact]
[Trait("AC", "7")]
public void TestDisplayModeWait()
{
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(new List<DialogueLine>
    {
        new("Test line", DisplayMode.Wait)
    });

    var originalOut = Console.Out;
    var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);
    headlessUI.OutputDialogue(dialogue);

    var output = stringWriter.ToString();
    Assert.Contains("[Dialogue] Test line", output);
    Assert.Contains("[WAIT]", output);

    Console.SetOut(originalOut);
    stringWriter.Dispose();
}

[Fact]
[Trait("AC", "8")]
public void TestDisplayModeKeyWait()
{
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(new List<DialogueLine>
    {
        new("Test line", DisplayMode.KeyWait)
    });

    var originalOut = Console.Out;
    var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);
    headlessUI.OutputDialogue(dialogue);

    var output = stringWriter.ToString();
    Assert.Contains("[Dialogue] Test line", output);
    Assert.Contains("[KEY_WAIT]", output);

    Console.SetOut(originalOut);
    stringWriter.Dispose();
}

[Fact]
[Trait("AC", "9")]
public void TestDisplayModeDisplay()
{
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(new List<DialogueLine>
    {
        new("Test line", DisplayMode.Display)
    });

    var originalOut = Console.Out;
    var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);
    headlessUI.OutputDialogue(dialogue);

    var output = stringWriter.ToString();
    Assert.Contains("[DISPLAY] Test line", output);

    Console.SetOut(originalOut);
    stringWriter.Dispose();
}

#endregion
```

### Risk Mitigation Summary

| Risk | Mitigation Strategy |
|------|---------------------|
| Wait/KeyWait blocks automated tests | Use marker-based semantics. No Console.ReadLine() blocking. Markers provide observable behavior for tests. |
| Display mode output breaks existing test assertions | Existing test `TestOutputDialogueValid()` already uses `DialogueResult.Create()` with `DisplayMode.Default`. Output format remains `[Dialogue] {text}`. No breaking change. |
| YamlComExecutor behavior regression | Do not migrate YamlComExecutor in F682 scope. It continues using `dialogue.Lines` (text-only). F683 will address deprecation after all necessary consumers migrate. |
| Display variants have no console equivalent | Use prefix-based differentiation (`[DISPLAY]` prefix). Provides observable behavior for tests while respecting console limitations. |
| HeadlessUI.OutputDialogue signature change | Method signature remains `OutputDialogue(DialogueResult dialogue)`. Only internal implementation changes. No breaking change to callers. |

### Scope Boundaries

**In Scope**:
- HeadlessUI.OutputDialogue() migration from Lines to DialogueLines
- DisplayMode switch implementation for all 9 enum values
- Marker-based wait semantics ([WAIT], [KEY_WAIT])
- Display mode differentiation ([DISPLAY] prefix)
- 9 new HeadlessUITests test methods (AC#5-9, AC#20-23)

**Out of Scope** (Tracked Destinations):
- YamlComExecutor migration to DialogueLines - Defer to F683 (DialogueResult.Lines Deprecation)
- Interactive blocking behavior for Wait/KeyWait - Defer to F684 (GUI Consumer Display Mode Interpretation)
- Rich console formatting for Display mode - Defer to F684 (GUI Consumer Display Mode Interpretation)
- Multi-entry display mode handling - Tracked in F681 (Multi-entry selection and rendering pipeline)

### Dependencies and Execution Order

**Prerequisites**:
- F676 [DONE] - DisplayMode propagation through DialogueResult pipeline

**No Blocking Dependencies**: F682 can execute immediately.

**Downstream Consumers**:
- F683 [DRAFT] - DialogueResult.Lines Deprecation (depends on F682 completing consumer migration)

---
