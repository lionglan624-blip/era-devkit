# Feature 597: Error Analytics and Telemetry

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
Error monitoring and analytics provide valuable insights for improving system reliability and user experience. Telemetry data should be collected with user consent and privacy protection, enabling developers to identify common error patterns and prioritize fixes.

### Problem (Current Issue)
Current error handling (F594) provides user-friendly error reporting but lacks systematic collection of error metrics, frequency analysis, and aggregated reporting. Without analytics, it's difficult to identify which errors are most common, which recovery suggestions are effective, and how error patterns change over time.

### Goal (What to Achieve)
Implement optional error analytics and telemetry system that collects anonymized error data with user consent, providing insights into error frequency, error patterns, recovery success rates, and system reliability metrics.

---

## Scope Discipline

**明確にIN SCOPE**:
- IErrorAnalyticsService interface for telemetry collection
- Automatic background error data collection (distinct from F594 manual user-triggered ReportError)
- Error data collection and anonymization logic
- User consent management for analytics
- Local metrics in-memory aggregation (file persistence deferred to F604)
- Integration with existing GUI error handling (F594)

**明確にOUT OF SCOPE**:
- Local metrics file persistence (→ F604)
- Remote analytics transmission (→ Future feature)
- Real-time dashboard visualization (→ Future feature)
- Analytics for ERB script errors (→ Future consideration)
- Performance analytics beyond error metrics (→ Future feature)

---

## Implementation Contract

### IErrorAnalyticsService Interface
```csharp
using System.Collections.Generic;

namespace MinorShift.Emuera.Services;

public interface IErrorAnalyticsService
{
    /// <summary>Gets whether user has granted analytics consent</summary>
    bool HasAnalyticsConsent { get; }

    /// <summary>Sets user consent for analytics data collection</summary>
    void SetAnalyticsConsent(bool consent);

    /// <summary>Collects error data for analytics if consent granted</summary>
    void CollectErrorData(string errorType, string message, string stackTrace);


    /// <summary>Anonymizes sensitive data in error messages</summary>
    string AnonymizeMessage(string message);

    /// <summary>Returns collected error metrics as read-only collection</summary>
    IEnumerable<ErrorMetric> GetCollectedMetrics();
}
```

### Service Implementation
```csharp
using System;
using System.Collections.Generic;

namespace MinorShift.Emuera.Services;

public class ErrorAnalyticsService : IErrorAnalyticsService
{
    private bool _hasConsent = false;
    private List<ErrorMetric> _errorLog = new();

    public bool HasAnalyticsConsent => _hasConsent;

    public void SetAnalyticsConsent(bool consent)
    {
        _hasConsent = consent;
    }

    public void CollectErrorData(string errorType, string message, string stackTrace)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (!_hasConsent) return;

        var metric = new ErrorMetric
        {
            Type = errorType,
            Message = AnonymizeMessage(message),
            StackTrace = stackTrace,
            Timestamp = DateTime.UtcNow
        };
        _errorLog.Add(metric);
    }


    public string AnonymizeMessage(string message)
    {
        // Remove file paths (C:\Users\*), IP addresses, and user names
        var result = message.Replace(Environment.UserName, "{anonymous}")
                           .Replace(Environment.UserDomainName, "{domain}");

        // Remove IP addresses
        result = System.Text.RegularExpressions.Regex.Replace(result,
            @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "{IP}");

        // Remove Windows user file paths
        result = System.Text.RegularExpressions.Regex.Replace(result,
            @"C:\\Users\\[^\\]+", @"C:\Users\{anonymous}");

        return result;
    }

    public IEnumerable<ErrorMetric> GetCollectedMetrics()
    {
        return _errorLog.AsReadOnly();
    }
}

public class ErrorMetric
{
    public string Type { get; init; }
    public string Message { get; init; }
    public string StackTrace { get; init; }
    public DateTime Timestamp { get; init; }
}
```

### Test Naming Convention
- Test class: `ErrorAnalyticsServiceTests`
- Test file: `engine.Tests/Tests/ErrorAnalyticsServiceTests.cs`
- Method pattern: `Test{MethodName}_{Condition}`
- Example: `TestCollectErrorData_WhenConsentGranted()`

### DI Registration
```csharp
// In GlobalStatic.cs - Insert after _errorDialogService property (around line 125) for chronological feature ordering
private static IErrorAnalyticsService _errorAnalyticsService;
public static IErrorAnalyticsService ErrorAnalyticsService
{
    get => _errorAnalyticsService;
    set => _errorAnalyticsService = value;
}

// In Application.Start() or similar initialization code
private void Start()
{
    ErrorAnalyticsService = new ErrorAnalyticsService();
}

// In Reset() method
static void Reset()
{
    _errorAnalyticsService = new ErrorAnalyticsService();
    // NOTE: Uses 'new' pattern because ErrorAnalyticsService is a regular class. F594 uses '= null' because UnityErrorDialog is a MonoBehaviour (cannot instantiate with 'new').
}
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Analytics service interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | contains | "interface IErrorAnalyticsService" | [x] |
| 2 | Error data collection method | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "CollectErrorData.*errorType.*message.*stackTrace" | [x] |
| 3 | User consent property | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | contains | "bool HasAnalyticsConsent" | [x] |
| 4 | Anonymization method | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "AnonymizeMessage.*message" | [x] |
| 5 | User consent method | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | contains | "SetAnalyticsConsent" | [x] |
| 6 | Service DI registration | file | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "public static IErrorAnalyticsService ErrorAnalyticsService" | [x] |
| 7 | Service file existence | file | Glob | exists | "engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs" | [x] |
| 8 | Interface file existence | file | Glob | exists | "engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs" | [x] |
| 9 | Unit tests execution | build | Bash | succeeds | "dotnet test engine.Tests --filter ErrorAnalyticsServiceTests" | [x] |
| 10 | No technical debt in interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | not_contains | "TODO" | [x] |
| 11 | No technical debt in service | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | not_contains | "TODO" | [x] |
| 12 | Analytics disabled by default | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "_hasConsent = false" | [x] |
| 13 | No data collection without consent | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "if (!_hasConsent) return" | [x] |
| 14 | GlobalStatic Reset integration | file | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "_errorAnalyticsService = new ErrorAnalyticsService" | [x] |
| 15 | Anonymization removes user data | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "Environment.UserName.*anonymous" | [x] |
| 16 | IP address anonymization | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "{IP}" | [x] |
| 17 | File path anonymization | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "{anonymous}" | [x] |
| 18 | GetCollectedMetrics method | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | contains | "GetCollectedMetrics" | [x] |
| 19 | GetCollectedMetrics implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "return _errorLog.AsReadOnly()" | [x] |
| 20 | ErrorMetric class definition | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "class ErrorMetric" | [x] |
| 21 | F594 integration - collect on error | file | Grep(engine/Assets/Scripts/Emuera/Services/UnityErrorDialog.cs) | contains | "ErrorAnalyticsService?.CollectErrorData" | [x] |
| 22 | No collection without consent (negative) | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "if (!_hasConsent) return" | [x] |
| 23 | Null message handling (negative) | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "if (string.IsNullOrEmpty(message))" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,8 | Create analytics service interface | [x] |
| 2 | 2,7,10,11,20 | Implement error data collection service with ErrorMetric class | [x] |
| 3 | 3,5 | Add consent management methods to interface | [x] |
| 4 | 4,12,13,15,16,17 | Implement data anonymization, consent checks, and verify anonymization patterns | [x] |
| 5 | 6,14 | Register service in GlobalStatic DI container and Reset method | [x] |
<!-- Batch waiver: DI property and Reset integration in same GlobalStatic file -->
| 6 | 9 | Create unit tests for analytics service | [x] |
| 7 | 18,19 | Add GetCollectedMetrics method to interface (AC#18) and implement in service with AsReadOnly pattern (AC#19) | [x] |
| 8 | 21 | Integrate with F594 ErrorDialogService for automatic error collection | [x] |
| 9 | 22,23 | Implement error condition handling (negative test cases) | [x] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F594 | [DONE] | GUI Mode Error Handling - base error handling system |

---

## Mandatory Handoffs

| Component | Destination | Description |
|-----------|-------------|-------------|
| SaveErrorMetrics implementation | F604 (create) | Complete SaveErrorMetrics method with file format design and local metrics storage |
| Remote analytics transmission | F605 (create) | HTTP/HTTPS transmission of anonymized metrics to remote server |
| Real-time dashboard visualization | F606 (create) | Web-based dashboard for analytics visualization |
| Performance analytics beyond error metrics | F607 (create) | CPU, memory, rendering performance analytics |

---

## Review Notes
- [resolved-applied] Phase1-Uncertain iter2: AC#9 test filter - moved to engine.Tests with explicit class name 'ErrorAnalyticsServiceTests'
- [resolved-applied] Phase1-Uncertain iter2: AC#4 AnonymizeErrorData - interface changed to AnonymizeMessage to match implementation
- [resolved-applied] Phase1-Uncertain iter5: AC#16 NotImplementedException - accepted as valid tracking pattern per 'Track What You Skip' principle
- [resolved-applied] Phase1-Uncertain iter6: AC#17-18 regex patterns - simplified AC#16 to literal string matching
- [resolved-applied] Phase1-Uncertain iter8: AC#14 Task mapping - added AC#14 to Task 5 with batch waiver
- [resolved-applied] Phase1-Uncertain iter7: AC#17-18 patterns already use literal strings '{IP}' and '{anonymous}' which match code
- [resolved-applied] Phase1-Uncertain iter7: Task 7-8 removed - only Tasks 1-6 exist in current Task table
- [resolved-applied] Phase1-Uncertain iter2: Mandatory Handoffs F604-F607 destinations - '(create)' notation is sufficient per Deferred Task Protocol Option B (concrete destination)
- [resolved-invalid] Phase1-Uncertain iter3: Integration pattern documentation - Implementation Contract already shows service interface, integration point will be documented in Implementation phase
- [resolved-invalid] Phase1-Uncertain iter3: Task 6 batch waiver - batch waiver comment already exists, AC#9 (unit tests) and AC#16 (stub tracking) relationship is clear (tests verify stub behavior)
- [resolved-invalid] Phase1-Uncertain iter3: Namespace/path consistency - file paths already specified in AC table, Implementation Contract code is clear
- [resolved-invalid] Phase1-Uncertain iter4: Namespace/path comments - duplicate removed as noted
- [resolved-invalid] Phase1-Uncertain iter7: Test file existence AC - test execution AC#9 will fail if test file doesn't exist, separate existence check is redundant
- [resolved-invalid] Phase1-Uncertain iter8: Test file existence AC defense - test execution AC provides sufficient verification, separate existence AC is redundant
- [resolved-invalid] Phase1-Uncertain iter9: Task 7 AC grouping - grouping interface method (AC#19) with implementation (AC#20) in same task is appropriate design
- [resolved-invalid] Phase1-Uncertain iter10: AC#2 pattern precision - user confirmed current pattern is functionally correct
- [resolved-invalid] Phase1-Uncertain iter10: Reset comment style - user confirmed current placement is sufficient
- [resolved-applied] Phase2-Maintainability iter10: SaveErrorMetrics NotImplementedException - removed from interface, deferred to F604 per user decision
- [resolved-applied] Phase2-Maintainability iter10: Integration verification - added AC#21 and Task 8 for F594 automatic integration per user decision
- [resolved-applied] Phase2-Maintainability iter10: F604-F607 handoffs - created stub features F604-F607 as [PROPOSED]/[BLOCKED] per user decision
- [resolved-applied] Phase3-ACValidation iter10: Negative test coverage - added AC#22,23 and Task 9 for error condition handling per user decision
- [resolved-applied] Phase4-Feasibility iter10: Feasibility check completed - null handling added to Implementation Contract

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-23 | DEVIATION | ac-static-verifier | AC#21 file verification | FAIL: pattern "ErrorAnalyticsService.CollectErrorData" not found |
| 2026-01-23 | DEVIATION | ac-static-verifier | AC#9 test execution | FAIL: TestAnonymizeMessage_NullInput_ReturnsEmpty |
| 2026-01-23 | FIX | debugger | AnonymizeMessage null check | Added null guard |
| 2026-01-23 | FIX | Opus | AC#21 pattern | Updated pattern to include null-conditional operator |
| 2026-01-23 | RETRY | ac-static-verifier | All ACs | PASS: 23/23 |
| 2026-01-23 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: engine-dev SKILL.md missing IErrorAnalyticsService |
| 2026-01-23 | FIX | Opus | engine-dev SKILL.md | Added IErrorAnalyticsService to GlobalStatic table and Core Interfaces |

## Links
- [index-features.md](index-features.md)
- [feature-594.md](feature-594.md)