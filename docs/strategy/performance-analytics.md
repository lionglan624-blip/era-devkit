# Performance Analytics Architecture

**Created**: 2026-01-24
**Feature**: F607 (Performance Analytics Beyond Error Metrics)
**Status**: Active Implementation

---

## Executive Summary

The performance analytics infrastructure provides comprehensive performance monitoring capabilities beyond error tracking. This system enables data-driven optimization and capacity planning for the ERA game engine through systematic collection of execution timing, memory usage, resource loading metrics, and configurable performance thresholds.

**Key Capabilities**:
- Execution time tracking for operations and methods
- Memory usage monitoring per component
- Resource loading performance measurement
- Configurable performance threshold monitoring
- Integration with existing analytics infrastructure from F597

**Architecture Philosophy**: Platform-agnostic core layer (Era.Core) with engine-layer integration adapters, maintaining clean separation of concerns while enabling unified analytics reporting.

---

## Architecture Overview

### Layer Structure

The performance analytics system follows a three-layer architecture:

```
┌─────────────────────────────────────────────────┐
│ Engine Layer (MinorShift.Emuera.Services)      │
│ - PerformanceMetricsAdapter (Unity-specific)   │
│ - ErrorAnalyticsService integration            │
│ - Logging and telemetry orchestration          │
└─────────────────────────────────────────────────┘
                     ↓ DI
┌─────────────────────────────────────────────────┐
│ Era.Core.Monitoring (Platform-agnostic)        │
│ - IPerformanceMetrics interface                │
│ - PerformanceMetricsService implementation     │
│ - PerformanceAnalyticsService                  │
│ - PerformanceConfig configuration              │
└─────────────────────────────────────────────────┘
                     ↓ Storage
┌─────────────────────────────────────────────────┐
│ Metrics Storage (In-memory dictionaries)       │
│ - ExecutionTimes: Dictionary<string, long>     │
│ - MemoryUsages: Dictionary<string, long>       │
│ - ResourceLoadTimes: Dictionary<string, long>  │
│ - Thresholds: Dictionary<string, double>       │
└─────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Layer | Responsibility |
|-----------|-------|----------------|
| **IPerformanceMetrics** | Era.Core | Interface defining metric collection methods |
| **PerformanceMetricsService** | Era.Core | Platform-agnostic metric collection and storage |
| **PerformanceAnalyticsService** | Era.Core | Metric analysis and threshold violation detection |
| **PerformanceConfig** | Era.Core | Configuration settings for monitoring behavior |
| **PerformanceMetricsAdapter** | Engine | Unity-specific adapter implementing IPerformanceMetrics via DI |
| **ErrorAnalyticsService** | Engine | Unified analytics reporting (integrates performance + error metrics) |

---

## Integration with F597 Analytics Infrastructure

### Integration Strategy

Performance analytics integrates with the existing F597 error analytics infrastructure through a layered adapter pattern:

1. **Engine-layer adapter service** implements IPerformanceMetrics via dependency injection
2. **Adapter forwards calls** to Era.Core monitoring service for storage and processing
3. **PerformanceAnalyticsService bridges** Era.Core metrics with engine-layer ErrorAnalyticsService
4. **Engine layer controls orchestration** while Era.Core maintains metric collection logic

### Data Flow

```
Game Code → PerformanceMetricsAdapter (Engine)
                ↓
        PerformanceMetricsService (Era.Core)
                ↓
        [Store in-memory dictionaries]
                ↓
        PerformanceAnalyticsService.AnalyzeMetrics()
                ↓
        ErrorAnalyticsService (Engine) [via adapter]
                ↓
        Unified Analytics Reporting
```

### Unified Analytics Model

Performance metrics and error metrics share common analytics infrastructure:

| Metric Type | Source | Destination | Integration Point |
|-------------|--------|-------------|------------------|
| Execution Time | IPerformanceMetrics | ErrorAnalyticsService | Adapter.RecordExecutionTime() |
| Memory Usage | IPerformanceMetrics | ErrorAnalyticsService | Adapter.RecordMemoryUsage() |
| Resource Load Time | IPerformanceMetrics | ErrorAnalyticsService | Adapter.RecordResourceLoadTime() |
| Threshold Violations | PerformanceAnalyticsService | ErrorAnalyticsService | Adapter.AnalyzeMetrics() |
| Error Events | ErrorAnalyticsService | ErrorAnalyticsService | Direct (F597) |

### Separation of Concerns

**Era.Core responsibilities** (platform-agnostic):
- Define metric collection interface
- Implement metric storage and retrieval
- Provide basic metric analysis (threshold checks, pattern detection)
- Maintain configuration model

**Engine layer responsibilities** (Unity-specific):
- Provide DI integration for IPerformanceMetrics
- Forward metrics to platform-agnostic storage
- Integrate with ErrorAnalyticsService for unified reporting
- Handle Unity-specific logging and telemetry

---

## Available Metrics

### Execution Time Metrics

**Purpose**: Track operation and method execution duration for performance bottleneck identification.

**API**:
```csharp
void RecordExecutionTime(string operation, long milliseconds);
```

**Parameters**:
- `operation`: Operation identifier (e.g., "LoadCSV", "ProcessTraining", "CalculateMark")
- `milliseconds`: Execution time in milliseconds (non-negative)

**Storage**: `Dictionary<string, long>` mapping operation names to last recorded execution time

**Error Handling**:
- Throws `ArgumentNullException` if operation is null or empty
- Throws `ArgumentOutOfRangeException` if milliseconds is negative

**Usage Pattern**:
```csharp
var stopwatch = Stopwatch.StartNew();
// ... operation code ...
stopwatch.Stop();
performanceMetrics.RecordExecutionTime("LoadCSV", stopwatch.ElapsedMilliseconds);
```

---

### Memory Usage Metrics

**Purpose**: Monitor memory consumption per component for capacity planning and memory leak detection.

**API**:
```csharp
void RecordMemoryUsage(string component, long bytes);
```

**Parameters**:
- `component`: Component identifier (e.g., "CharacterCache", "VariableStore", "MarkSystem")
- `bytes`: Memory usage in bytes (non-negative)

**Storage**: `Dictionary<string, long>` mapping component names to last recorded memory usage

**Error Handling**:
- Throws `ArgumentNullException` if component is null or empty
- Throws `ArgumentOutOfRangeException` if bytes is negative

**Usage Pattern**:
```csharp
var memoryBefore = GC.GetTotalMemory(false);
// ... component initialization ...
var memoryAfter = GC.GetTotalMemory(false);
performanceMetrics.RecordMemoryUsage("CharacterCache", memoryAfter - memoryBefore);
```

---

### Resource Loading Metrics

**Purpose**: Measure file and resource loading performance for I/O optimization.

**API**:
```csharp
void RecordResourceLoadTime(string resource, long milliseconds);
```

**Parameters**:
- `resource`: Resource identifier (e.g., "character.csv", "kojo/meiling.erb", "saves/save001.sav")
- `milliseconds`: Load time in milliseconds (non-negative)

**Storage**: `Dictionary<string, long>` mapping resource identifiers to last recorded load time

**Error Handling**:
- Throws `ArgumentNullException` if resource is null or empty
- Throws `ArgumentOutOfRangeException` if milliseconds is negative

**Usage Pattern**:
```csharp
var stopwatch = Stopwatch.StartNew();
var data = await File.ReadAllTextAsync(resourcePath);
stopwatch.Stop();
performanceMetrics.RecordResourceLoadTime(resourcePath, stopwatch.ElapsedMilliseconds);
```

---

### Performance Thresholds

**Purpose**: Configure performance thresholds for automated monitoring and alerting.

**API**:
```csharp
void SetPerformanceThreshold(string metric, double threshold);
```

**Parameters**:
- `metric`: Metric identifier (e.g., "ExecutionTime.LoadCSV", "MemoryUsage.CharacterCache")
- `threshold`: Threshold value (metric-specific units)

**Storage**: `Dictionary<string, double>` mapping metric identifiers to threshold values

**Error Handling**:
- Throws `ArgumentNullException` if metric is null or empty
- No validation on threshold value (allows negative thresholds for specialized use cases)

**Usage Pattern**:
```csharp
// Set execution time threshold: flag operations over 1 second
performanceMetrics.SetPerformanceThreshold("ExecutionTime.LoadCSV", 1000.0);

// Set memory threshold: flag components over 100MB
performanceMetrics.SetPerformanceThreshold("MemoryUsage.CharacterCache", 104857600.0);
```

---

### Metrics Snapshot

**Purpose**: Retrieve current state of all performance metrics for analysis and reporting.

**API**:
```csharp
PerformanceMetricsSnapshot GetCurrentMetrics();
```

**Return Type**:
```csharp
public record PerformanceMetricsSnapshot(
    Dictionary<string, long> ExecutionTimes,
    Dictionary<string, long> MemoryUsages,
    Dictionary<string, long> ResourceLoadTimes,
    Dictionary<string, double> Thresholds
);
```

**Behavior**: Returns defensive copies of internal dictionaries to prevent external mutation.

**Usage Pattern**:
```csharp
var snapshot = performanceMetrics.GetCurrentMetrics();

// Analyze execution times
foreach (var kvp in snapshot.ExecutionTimes)
{
    if (kvp.Value > 1000)
        Console.WriteLine($"Slow operation: {kvp.Key} took {kvp.Value}ms");
}

// Check threshold violations
var analysisService = new PerformanceAnalyticsService();
analysisService.AnalyzeMetrics(snapshot);
```

---

## Configuration Options

### PerformanceConfig Record

Performance monitoring behavior is configured via immutable `PerformanceConfig` record:

```csharp
public sealed record PerformanceConfig(
    bool IsEnabled = false,
    long ExecutionTimeThresholdMs = 1000,
    long MemoryUsageThresholdBytes = 104857600,
    long ResourceLoadTimeThresholdMs = 500,
    int MaxMetricsRetention = 1000,
    bool AutoAnalyze = true,
    bool IncludeStackTraces = false
);
```

### Configuration Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **IsEnabled** | bool | false | Whether performance monitoring is enabled. Opt-in for performance overhead concerns. |
| **ExecutionTimeThresholdMs** | long | 1000 | Execution time threshold in milliseconds. Operations exceeding this are flagged. |
| **MemoryUsageThresholdBytes** | long | 104857600 | Memory usage threshold in bytes (100 MB). Components exceeding this are flagged. |
| **ResourceLoadTimeThresholdMs** | long | 500 | Resource load time threshold in milliseconds. Loading operations exceeding this are flagged. |
| **MaxMetricsRetention** | int | 1000 | Maximum number of metrics to retain in memory. FIFO eviction when limit reached. |
| **AutoAnalyze** | bool | true | Whether to automatically analyze metrics on collection. Enables real-time monitoring. |
| **IncludeStackTraces** | bool | false | Whether to include stack traces in performance metrics. Useful for debugging but increases overhead. |

### Usage Patterns

**Development Configuration** (detailed monitoring):
```csharp
var devConfig = new PerformanceConfig(
    IsEnabled: true,
    ExecutionTimeThresholdMs: 100,  // Flag operations over 100ms
    MemoryUsageThresholdBytes: 10485760,  // Flag components over 10MB
    ResourceLoadTimeThresholdMs: 100,
    MaxMetricsRetention: 5000,
    AutoAnalyze: true,
    IncludeStackTraces: true  // Enable for debugging
);
```

**Production Configuration** (minimal overhead):
```csharp
var prodConfig = new PerformanceConfig(
    IsEnabled: true,
    ExecutionTimeThresholdMs: 5000,  // Only flag very slow operations
    MemoryUsageThresholdBytes: 524288000,  // Flag components over 500MB
    ResourceLoadTimeThresholdMs: 2000,
    MaxMetricsRetention: 500,
    AutoAnalyze: false,  // Manual analysis to reduce overhead
    IncludeStackTraces: false
);
```

**Disabled Configuration** (testing/benchmarking):
```csharp
var disabledConfig = new PerformanceConfig(IsEnabled: false);
```

---

## Performance Analytics Service

### Purpose

`PerformanceAnalyticsService` provides platform-agnostic analysis of performance metrics, identifying performance issues and threshold violations.

### Architecture

**Layer**: Era.Core.Monitoring (platform-agnostic)

**Integration Point**: Engine-layer adapter forwards analysis results to ErrorAnalyticsService for unified analytics reporting.

### API

```csharp
public interface IPerformanceAnalyticsService
{
    void AnalyzeMetrics(PerformanceMetricsSnapshot metrics);
}
```

### Analysis Capabilities

**Execution Time Analysis**:
- Identifies slow operations exceeding configured thresholds
- Detects execution time patterns (e.g., degradation over time)
- Marks operations for attention (engine layer handles logging)

**Memory Usage Analysis**:
- Identifies high-memory components
- Detects memory growth patterns
- Flags potential memory leaks

**Resource Loading Analysis**:
- Identifies slow-loading resources
- Detects I/O bottlenecks
- Prioritizes optimization targets

**Threshold Violation Detection**:
- Checks all metrics against configured thresholds
- Reports violations to engine layer for alerting

### Usage Pattern

```csharp
// Collect metrics
var snapshot = performanceMetrics.GetCurrentMetrics();

// Analyze metrics
var analyticsService = new PerformanceAnalyticsService();
analyticsService.AnalyzeMetrics(snapshot);

// Results forwarded to ErrorAnalyticsService via adapter
```

### Platform-Agnostic Design

`PerformanceAnalyticsService` intentionally does **not** perform logging, telemetry, or alerting:

- **Era.Core responsibility**: Metric analysis and pattern detection
- **Engine layer responsibility**: Logging, telemetry, alerting (via adapter)

This separation maintains platform independence and testability.

---

## Error Handling Approach

### Exception-Based Validation

Performance metrics APIs use exception-based validation for preconditions:

| Method | Null Parameter | Negative Value |
|--------|----------------|----------------|
| RecordExecutionTime | ArgumentNullException | ArgumentOutOfRangeException |
| RecordMemoryUsage | ArgumentNullException | ArgumentOutOfRangeException |
| RecordResourceLoadTime | ArgumentNullException | ArgumentOutOfRangeException |
| SetPerformanceThreshold | ArgumentNullException | (none) |

**Rationale**: Performance monitoring is an internal instrumentation concern, not a user-facing operation. Exceptions for invalid inputs are appropriate (programmer errors, not runtime errors).

### No Result<T> Pattern

Performance metrics methods return `void` (not `Result<T>`) because:

1. **Internal instrumentation**: Not user-facing operations with recoverable errors
2. **Fail-fast validation**: Invalid inputs are programmer errors (should be caught during development)
3. **Monitoring semantics**: Metric collection failure should not propagate to business logic
4. **Consistency**: Matches logging and telemetry patterns (void methods with validation exceptions)

### Null Safety

`PerformanceAnalyticsService.AnalyzeMetrics()` validates snapshot parameter:
- Throws `ArgumentNullException` if metrics snapshot is null
- Defensive copies prevent external mutation of internal state

---

## Implementation Notes

### Directory Structure

```
src/Era.Core/
└── Monitoring/
    ├── IPerformanceMetrics.cs
    ├── PerformanceMetricsService.cs
    ├── IPerformanceAnalyticsService.cs
    ├── PerformanceAnalyticsService.cs
    └── PerformanceConfig.cs

src/Era.Core.Tests/
└── Monitoring/
    ├── PerformanceMetricsServiceTests.cs
    └── PerformanceAnalyticsServiceTests.cs
```

### Storage Implementation

Metrics are stored in-memory using `Dictionary<string, T>`:

```csharp
private readonly Dictionary<string, long> _executionTimes = new();
private readonly Dictionary<string, long> _memoryUsages = new();
private readonly Dictionary<string, long> _resourceLoadTimes = new();
private readonly Dictionary<string, double> _thresholds = new();
```

**Characteristics**:
- Latest-value semantics (new recordings overwrite previous values)
- Thread-unsafe (requires external synchronization for concurrent access)
- No persistence (in-memory only, cleared on restart)

**Future Enhancement Opportunities** (out of scope for F607):
- Retention policy with FIFO eviction (MaxMetricsRetention config)
- Thread-safe concurrent collections
- Persistence to disk or remote analytics service
- Historical data aggregation and trending

### Testing Strategy

**Unit tests verify**:
- Null parameter validation (ArgumentNullException)
- Negative value validation (ArgumentOutOfRangeException)
- Valid input handling (correct storage and retrieval)
- Snapshot defensive copying
- Analytics service integration

**Test coverage**:
- `PerformanceMetricsServiceTests.cs`: IPerformanceMetrics implementation
- `PerformanceAnalyticsServiceTests.cs`: Analysis logic

---

## Future Enhancements

**Out of scope for F607** but documented for roadmap planning:

| Enhancement | Feature | Description |
|-------------|---------|-------------|
| **Performance Visualization** | F613 (create) | Dashboard components for performance data visualization |
| **Real-time Monitoring** | F617 (create) | Real-time performance monitoring infrastructure |
| **Historical Data Storage** | TBD | Persist metrics beyond current session for trend analysis |
| **Automated Optimization** | TBD | Performance optimization recommendations based on analytics |
| **Performance Alerting** | TBD | Notification system for threshold violations |

---

## References

- [F597: Error Analytics and Telemetry](../feature-597.md) - Analytics infrastructure foundation
- [F604: SaveErrorMetrics Implementation](../feature-604.md) - Shared analytics infrastructure
- [F605: Remote Analytics Transmission](../feature-605.md) - Performance data transmission
- [F606: Real-time Dashboard Visualization](../feature-606.md) - Error metrics visualization
- [F607: Performance Analytics Beyond Error Metrics](../feature-607.md) - This feature
- [F613: Performance Metrics Visualization](../feature-613.md) - Performance-specific visualization (create)
- [F617: Real-time Performance Monitoring](../feature-617.md) - Real-time monitoring infrastructure (create)

---

## Glossary

| Term | Definition |
|------|------------|
| **Metric** | Quantitative measurement of system behavior (execution time, memory usage, etc.) |
| **Snapshot** | Point-in-time capture of all performance metrics |
| **Threshold** | Configured limit for acceptable metric values; violations trigger analysis |
| **Analytics** | Automated analysis of metrics to identify patterns and issues |
| **Adapter** | Engine-layer component bridging platform-agnostic Era.Core with Unity-specific services |
| **Platform-agnostic** | Code independent of Unity or other platform-specific APIs |
