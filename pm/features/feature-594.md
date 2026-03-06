# Feature 594: GUI Mode Error Handling

## Status: [DONE]

## Scope Discipline

**明確にIN SCOPE**:
- IErrorDialogService interface for GUI error handling
- Unity-based error dialog implementation
- User-friendly error messages with recovery suggestions
- Integration with Unity application-level exception handling
- Technical details toggle for developer/user modes

**明確にOUT OF SCOPE**:
- ERB script error handling (handled by ProcessErrorHandler → EmueraConsole)
- Headless mode error handling (→ F596)
- Error analytics/telemetry (→ F597)
- Localization support (→ F598)

## Type: engine

## Background

### Philosophy (Mid-term Vision)
GUI applications should provide user-friendly error reporting with clear messaging and recovery options. Error handling must gracefully degrade from detailed technical information for developers to accessible user guidance for end-users.

### Problem (Current Issue)
The current GUI error handling uses basic MessageBox.Show() calls for critical errors, but lacks comprehensive user-friendly error dialogs for runtime exceptions. Engine errors are logged to Unity Console or stderr but don't provide clear user guidance for recovery. When fatal errors occur in GUI mode, users may see cryptic error messages or experience silent failures without understanding what went wrong or how to resolve issues.

Note: This feature handles GUI application-level errors (shown in modal dialogs via IErrorDialogService) while ERB script errors continue to be handled by ProcessErrorHandler (shown in EmueraConsole). The boundary ensures ERB execution errors remain in the console context while system/Unity errors get user-friendly dialog treatment.

### Goal (What to Achieve)
Implement comprehensive GUI error handling with user-friendly error dialogs that provide clear error descriptions, suggested actions, and recovery options. Ensure error information is accessible to both end-users and developers while maintaining system stability.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IErrorDialogService interface exists | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorDialogService.cs) | contains | "interface IErrorDialogService" | [x] |
| 2 | Unity error dialog implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | matches | "class UnityErrorDialog.*IErrorDialogService" | [x] |
| 3 | Error dialog service registration | file | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "public static IErrorDialogService ErrorDialogService" | [x] |
| 4 | Fatal error dialog display | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "ShowFatalError(string title, string message, Exception exception)" | [x] |
| 5 | Runtime error dialog display | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "ShowRuntimeError(string title, string message)" | [x] |
| 6 | Configuration error dialog | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "ShowConfigurationError(string message)" | [x] |
| 7 | Error dialog UI prefab | file | Glob | exists | "engine/Assets/Resources/Prefab/ErrorDialog.prefab" | [x] |
| 8 | Error dialog controller script | file | Grep(engine/Assets/Scripts/ErrorDialogController.cs) | contains | "class ErrorDialogController : MonoBehaviour" | [x] |
| 9 | MainEntry error handler setup | file | Grep(engine/Assets/Scripts/MainEntry.cs) | contains | "Application.logMessageReceived += OnLogMessageReceived" | [x] |
| 10 | Unity exception handling | file | Grep(engine/Assets/Scripts/MainEntry.cs) | contains | "LogType.Exception" | [x] |
| 11 | User-friendly error messages | file | Grep(Era.Core/ErrorHandling/ErrorMessageHelper.cs) | contains | "GetUserFriendlyMessage(Exception exception)" | [x] |
| 12 | Error recovery suggestions | file | Grep(Era.Core/ErrorHandling/RecoverySuggestionHelper.cs) | contains | "GetRecoverySuggestions(string errorType)" | [x] |
| 13 | Technical details toggle | file | Grep(engine/Assets/Scripts/ErrorDialogController.cs) | contains | "ShowTechnicalDetails(bool visible)" | [x] |
| 14 | Error reporting integration | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "ReportError(object errorData, string userComment)" | [x] |
| 15 | No error dialog outside error handling paths | code | Grep(engine/Assets/Scripts/MainEntry.cs) | not_contains | "ShowFatalError.*Start\(\)" | [x] |
| 16 | No error dialog for normal ERB operations | code | Grep(engine/Assets/Scripts/Emuera/Sub) | not_contains | "IErrorDialogService.*ProcessErrorHandler" | [x] |
| 17 | GlobalStatic Reset integration | code | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "_errorDialogService = null" | [x] |
| 18 | Error message helper tests | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter ErrorMessageHelper" | [x] |
| 19 | Recovery suggestion tests | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter RecoverySuggestionHelper" | [x] |
| 20 | Developer mode detection | code | Grep(engine/Assets/Scripts/ErrorDialogController.cs) | matches | "Debug.isDebugBuild.*technical.*details" | [x] |
| 21 | ErrorMessageHelper file exists | file | Glob | exists | "Era.Core/ErrorHandling/ErrorMessageHelper.cs" | [x] |
| 22 | RecoverySuggestionHelper file exists | file | Glob | exists | "Era.Core/ErrorHandling/RecoverySuggestionHelper.cs" | [x] |
| 23 | Error dialog service backing field | code | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "private static IErrorDialogService _errorDialogService" | [x] |
| 24 | No technical debt in UnityErrorDialog | code | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 25 | No technical debt in ErrorDialogController | code | Grep(engine/Assets/Scripts/ErrorDialogController.cs) | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 26 | No technical debt in ErrorMessageHelper | code | Grep(Era.Core/ErrorHandling/ErrorMessageHelper.cs) | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 27 | No technical debt in RecoverySuggestionHelper | code | Grep(Era.Core/ErrorHandling/RecoverySuggestionHelper.cs) | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 28 | F596 integration graceful handling | code | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "Application.isEditor" | [x] |
| 29 | ReportError unit test coverage | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter ReportErrorTests" | [x] |

### AC Details

**AC#1**: IErrorDialogService interface exists
- Method: Grep interface definition in engine scripts
- Expected: Interface with ShowFatalError, ShowRuntimeError, ShowConfigurationError methods

**AC#2**: Unity error dialog implementation
- Method: Grep concrete Unity implementation class
- Expected: MonoBehaviour-based implementation of IErrorDialogService

**AC#3**: Error dialog service registration
- Method: Grep GlobalStatic property definition
- Expected: Static property for error dialog service following engine pattern

**AC#4**: Fatal error dialog display
- Method: Grep ShowFatalError method signature
- Expected: Method accepting title, message, and exception parameters

**AC#5**: Runtime error dialog display
- Method: Grep ShowRuntimeError method signature
- Expected: Method for non-fatal runtime errors

**AC#6**: Configuration error dialog
- Method: Grep ShowConfigurationError method
- Expected: Specialized dialog for configuration-related errors

**AC#7**: Error dialog UI prefab
- Method: Glob search for ErrorDialog.prefab
- Expected: Unity prefab file containing error dialog UI

**AC#8**: Error dialog controller script
- Method: Grep ErrorDialogController class definition
- Expected: MonoBehaviour managing error dialog interactions


**AC#9**: MainEntry error handler setup
- Method: Grep Application.logMessageReceived registration
- Expected: New Unity log message handler registration code added to MainEntry startup

**AC#10**: Unity exception handling
- Method: Grep OnLogMessageReceived method for exception handling
- Expected: New handler method for Unity LogType.Exception messages

**AC#11**: User-friendly error messages
- Method: Grep GetUserFriendlyMessage method
- Expected: Method converting technical exceptions to user-friendly text

**AC#12**: Error recovery suggestions
- Method: Grep GetRecoverySuggestions method
- Expected: Method providing context-appropriate recovery suggestions

**AC#13**: Technical details toggle
- Method: Grep ShowTechnicalDetails functionality
- Expected: UI toggle for showing/hiding technical error information

**AC#14**: Error reporting integration
- Method: Grep ReportError functionality
- Expected: Optional user-triggered error reporting mechanism with user comments (manual submit button in dialog vs F597 automatic telemetry/background collection). ReportError creates local report file only; no network transmission, network submission deferred to F597
- Note: ReportError success verified by return value (not file existence). File creation path/format is implementation detail documented in Implementation Contract skeleton

**AC#15**: No error dialog outside error handling paths
- Method: Grep code to verify error dialogs are not called in normal execution paths
- Expected: ShowFatalError not called in initialization methods like Start()

**AC#16**: No error dialog for normal ERB operations
- Method: Grep ProcessErrorHandler integration to verify separation
- Expected: ERB script errors handled by ProcessErrorHandler, not GUI error dialogs


**AC#17**: GlobalStatic Reset integration
- Method: Grep GlobalStatic.cs Reset method for error dialog service reset
- Expected: ErrorDialogService is reset to null in GlobalStatic.Reset() method (MonoBehaviour instances cannot be instantiated with 'new')

**AC#18**: Error message helper tests
- Method: Run C# unit tests for error message formatting logic
- Expected: Error message helper functions are properly unit tested in Era.Core

**AC#19**: Recovery suggestion tests
- Method: Run C# unit tests for recovery suggestion logic
- Expected: Recovery suggestion helper functions are properly unit tested in Era.Core

**AC#20**: Developer mode detection
- Method: Grep code for developer/user mode switching mechanism
- Expected: Error dialog behavior differs based on Debug.isDebugBuild or configuration flag for technical detail visibility

**AC#21**: ErrorMessageHelper file exists
- Method: Glob search for error message helper class file
- Expected: Helper class file exists in Era.Core for error message formatting logic

**AC#22**: RecoverySuggestionHelper file exists
- Method: Glob search for recovery suggestion helper class file
- Expected: Helper class file exists in Era.Core for recovery suggestion logic

**AC#23**: Error dialog service backing field
- Method: Grep GlobalStatic.cs for private backing field declaration
- Expected: Private backing field for error dialog service following F592 pattern

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-3,17,23 | Create IErrorDialogService interface and Unity implementation with DI registration <!-- Batch waiver: Single interface + DI registration + GlobalStatic Reset in same component --> | [x] |
| 2 | 4-6 | Implement error dialog methods for different error types (fatal, runtime, configuration) <!-- Batch waiver: Related method implementations in same class --> | [x] |
| 3 | 7-8 | Create Unity error dialog UI prefab and controller script <!-- Batch waiver: UI prefab and its controller are tightly coupled --> | [x] |
| 4 | 9-10 | Setup Unity application-level exception handling in MainEntry <!-- Batch waiver: Registration and handler method in same file --> | [x] |
| 5 | 11-12 | Implement user-friendly error message conversion and recovery suggestions <!-- Batch waiver: Related helper methods for error formatting --> | [x] |
| 6 | 13-14,15-16 | Add technical details toggle, optional error reporting, and negative verifications <!-- Batch waiver: UI toggle and validation are complementary features --> | [x] |
| 7 | 18-19,21-22,29 | Create C# unit tests and helper files for testable error handling logic <!-- Batch waiver: Test suite and helper files for Era.Core error handling --> | [x] |
| 8 | 20,28 | Implement developer/user mode switching and F596 integration checks <!-- Single AC for mode detection logic --> | [x] |
| 9 | 24-27 | Technical debt verification for all implementation files <!-- Batch waiver: Technical debt ACs for created files --> | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Task 1-2: Interface and basic dialog methods | IErrorDialogService.cs at engine/Assets/Scripts/Emuera/Services/, UnityErrorDialog.cs at engine/Assets/Scripts/Emuera/Services/, GlobalStatic property registration with private backing field |
| 2 | implementer | sonnet | Task 3: UI components | ErrorDialog.prefab at engine/Assets/Resources/Prefab/, ErrorDialogController.cs at engine/Assets/Scripts/ |
| 3 | implementer | sonnet | Task 4: Unity application-level exception setup | MainEntry.cs OnLogMessageReceived registration in Start() method |
| 4 | implementer | sonnet | Task 5-6: Advanced error handling features | GetUserFriendlyMessage(), GetRecoverySuggestions(), ReportError() methods |
| 5 | implementer | sonnet | Task 7: C# unit tests for helper functions | ErrorMessageHelper.cs and RecoverySuggestionHelper.cs with tests in Era.Core.Tests |

### Unity Prefab Structure (Task 3)
```
ErrorDialog (Canvas)
├── Background (Image)
├── TitleText (Text)
├── MessageText (Text)
├── StackTraceText (Text) - Hidden by default
├── ButtonPanel (GameObject)
│   ├── OKButton (Button)
│   ├── ShowDetailsButton (Toggle)
│   └── ReportButton (Button) - Optional
└── CloseButton (Button)
```

**Required Components**:
- ErrorDialogController script on root Canvas
- Canvas set to Screen Space - Overlay
- Reference existing Options.prefab structure for consistent styling

### MainEntry.cs Integration (Task 4)
```csharp
// Add to MainEntry.cs Start() method
private void Start()
{
    // ... existing initialization ...
    Application.logMessageReceived += OnLogMessageReceived;
}

private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
{
    if (type == LogType.Exception || type == LogType.Error)
    {
        GlobalStatic.ErrorDialogService?.ShowRuntimeError("Application Error", logString);
    }
}
```

### Unity Testing Note

IErrorDialogService requires Unity runtime dependencies (MonoBehaviour, Application.logMessageReceived) that cannot be unit tested in engine.Tests without Unity Test Framework. Error dialog functionality verified via GUI manual testing during implementation. Helper functions for error formatting and recovery suggestions are unit tested in Era.Core.Tests.

### Test Naming Convention

Test classes follow {HelperName}Tests format (e.g., ErrorMessageHelperTests, RecoverySuggestionHelperTests).

### Interface Definition Pattern
```csharp
// IErrorDialogService.cs
using System;

namespace MinorShift.Emuera.Services;

public interface IErrorDialogService
{
    void ShowFatalError(string title, string message, Exception exception);
    void ShowRuntimeError(string title, string message);
    void ShowConfigurationError(string message);
}

// GlobalStatic.cs registration
// Note: MonoBehaviour-based error dialog requires special handling
private static IErrorDialogService _errorDialogService;
public static IErrorDialogService ErrorDialogService
{
    get => _errorDialogService;
    set => _errorDialogService = value;
}

// UnityErrorDialog.cs implementation skeleton
// NOTE: TODO comments are placeholders to be replaced by implementer agent
using UnityEngine;
using MinorShift.Emuera.Services;

namespace MinorShift.Emuera.UI;

public class UnityErrorDialog : MonoBehaviour, IErrorDialogService
{
    public void ShowFatalError(string title, string message, Exception exception)
    {
        // TODO: Display fatal error dialog with title, message, exception details
    }

    public void ShowRuntimeError(string title, string message)
    {
        // TODO: Display runtime error dialog with title and message
    }

    public void ShowConfigurationError(string message)
    {
        // TODO: Display configuration error dialog with message
    }

    public void ReportError(object errorData, string userComment)
    {
        // TODO: Create local report file with error data and user comments
    }
}

// GlobalStatic.cs Reset() modification
// NOTE: Differs from F592 FatalErrorHandler pattern intentionally
public static void Reset()
{
    // ... existing reset logic ...
    _errorDialogService = null;  // MonoBehaviour cannot be instantiated with 'new'
    // F592 FatalErrorHandler uses: _fatalErrorHandler = new FatalErrorHandler();
    // F594 ErrorDialogService uses: _errorDialogService = null (MonoBehaviour limitation)
}

// ErrorMessageHelper.cs implementation skeleton
using System;

namespace Era.Core.ErrorHandling;

public static class ErrorMessageHelper
{
    public static string GetUserFriendlyMessage(Exception exception)
    {
        // TODO: Convert technical exceptions to user-friendly messages
        return string.Empty;
    }
}

// RecoverySuggestionHelper.cs implementation skeleton
namespace Era.Core.ErrorHandling;

public static class RecoverySuggestionHelper
{
    public static string GetRecoverySuggestions(string errorType)
    {
        // TODO: Provide context-appropriate recovery suggestions
        return string.Empty;
    }
}

### F596 Integration Point

IErrorDialogService methods should handle gracefully when called from non-GUI context (e.g., F596 headless mode). Implementation options:
- Return without action (silent ignore)
- Throw NotSupportedException for incompatible contexts
- Delegate to console-based error handling

F596 integration requires adapter pattern or null-object pattern for headless compatibility.
```

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F592 | [DONE] | Engine Fatal Error Exit Handling - complementary error handling, follows GlobalStatic service registration pattern |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-applied] Phase1-Uncertain iter1-2: F592 dependency relationship - F592 is Related (not Predecessor) per index-features.md rules. Added clarification in Dependencies description about GlobalStatic pattern inheritance.
- [resolved-applied] Phase1-Uncertain iter3: AC#14 'ReportError' local-only vs F597 telemetry distinction clarified in AC Details. ReportError success verification by return value documented.
- [resolved-applied] Phase1-Uncertain iter3: Valid concern per ENGINE.md Issue 35, but severity depends on implementation clarity. Implementation Contract Phase 1 specifies exact paths (now corrected to engine/Assets/Scripts/Emuera/Services/ for both files). AC table updated with specific file paths per Issue 35.

### F597 Integration Handoff (2026-01-23)
F597 (Error Analytics and Telemetry) requires automatic integration with UnityErrorDialog for error data collection. The integration will add ErrorAnalyticsService.CollectErrorData() calls to ShowFatalError() and ShowRuntimeError() methods to enable seamless analytics collection without requiring manual integration at each error site. This follows the automatic integration pattern decided in F597 FL review.
- [resolved-acknowledged] Phase1-Uncertain iter4: Related dependency and AC#7 path structure confirmed valid for [PROPOSED] feature.
- [resolved-acknowledged] Phase1-Uncertain iter5: Test ACs acceptable at PROPOSED stage per F592 precedent. AC#18-19 verify C# unit tests for helper functions.
- [resolved-applied] Phase1-Uncertain iter7: The distinction between F594 AC#14 'ReportError' (user-triggered manual submit) and F597 'Error analytics/telemetry' (automatic collection) is documented in F594's AC Details. Added clarification that file creation path/format is implementation detail (no file path AC needed).
- [resolved-applied] Phase1-Uncertain iter8: UnityErrorDialog listed as output but no code snippet in Implementation Contract. Added UnityErrorDialog MonoBehaviour implementation skeleton to Implementation Contract per ENGINE.md Issue 25.
- [resolved-applied] Phase1-Uncertain iter8: AC#14 ReportError scope clarification - added explicit 'no network transmission' to AC Details per user decision.
- [resolved-acknowledged] Phase1-Uncertain iter8: Path references in Implementation Contract corrected to engine/Assets/Scripts/Emuera/Services/ per user decision.
- [resolved-acknowledged] Phase1-Uncertain iter10: F592 pattern inheritance - current AC#3+AC#17+AC#23 combination adequately captures pattern with Implementation Contract documenting MonoBehaviour vs regular class difference.
- [resolved-acknowledged] Phase1-Uncertain iter10: AC#15-16 negative test paths maintained at current specificity level per user decision.
- [resolved-applied] Phase1-Uncertain iter10: ErrorMessageHelper.cs and RecoverySuggestionHelper.cs existence ACs added (AC#21-22). Implementation Contract skeletons now included.
- [resolved-applied] Phase1-Uncertain iter11: UnityErrorDialog MonoBehaviour implementation skeleton added to Implementation Contract per ENGINE.md Issue 25.
- [resolved-applied] Phase1-Uncertain iter11: AC#14 ReportError file verification - added AC#29 unit test coverage for ReportError functionality.
- [resolved-acknowledged] Phase1-Uncertain iter11: Review Notes consolidation completed per user decision for improved documentation clarity.
- [resolved-acknowledged] Phase1-Uncertain iter12: MainEntry.cs new methods - current AC design appropriate for [PROPOSED] feature per user decision.
- [resolved] Volume waiver: 29 ACs required for comprehensive GUI error handling (interface + Unity implementation + UI + helpers + tests + technical debt verification). Cohesive scope per F594 Philosophy.
- [resolved-applied] Phase1-Uncertain iter13: Review Notes pending items resolved through user consultation and consolidated for clarity.
- [resolved-applied] Phase1-Uncertain iter14: AC#14 ReportError file creation verification. Added AC#29 unit test coverage for ReportError functionality.
- [resolved-acknowledged] Phase1-Uncertain iter14: Review Notes [pending] items are discussion history (optional per template).
- [resolved-acknowledged] Phase1-Uncertain iter15: Implementation Contract TODO comment note placement maintained at line 264 per user decision.

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Headless mode compatibility | Focus on GUI mode first | Feature | F596 |
| Error analytics/telemetry | Privacy considerations | Feature | F597 |
| Localization support | Single language implementation first | Feature | F598 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-22 19:59 | START | implementer | Task 1-2 | - |
| 2026-01-22 19:59 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-22 20:04 | START | implementer | Task 3 | - |
| 2026-01-22 20:04 | END | implementer | Task 3 | SUCCESS |
| 2026-01-22 20:06 | START | implementer | Task 4 | - |
| 2026-01-22 20:06 | END | implementer | Task 4 | SUCCESS |
| 2026-01-22 20:09 | START | implementer | Task 5-6 | - |
| 2026-01-22 20:09 | END | implementer | Task 5-6 | SUCCESS |
| 2026-01-22 20:13 | START | implementer | Task 8 | - |
| 2026-01-22 20:13 | END | implementer | Task 8 | SUCCESS |
| 2026-01-22 20:20 | START | ac-tester | Phase 6 Verification | - |
| 2026-01-22 20:20 | END | ac-tester | Phase 6 Verification | 29/29 AC PASS |
| 2026-01-22 20:25 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: engine-dev SKILL missing IErrorDialogService |
| 2026-01-22 20:26 | FIX | opus | engine-dev SKILL.md | Added IErrorDialogService to GlobalStatic table and Core Interfaces |

## Links
- [index-features.md](index-features.md)
- [feature-592.md](feature-592.md) - Engine Fatal Error Exit Handling
- [feature-596.md](feature-596.md) - Headless Mode Error Handling
- [feature-597.md](feature-597.md) - Error Analytics/Telemetry
- [feature-598.md](feature-598.md) - Localization Support