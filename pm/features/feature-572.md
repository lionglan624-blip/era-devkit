# Feature 572: COM YAML Rapid Iteration Tooling

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

## Type: infra

## Background

### Philosophy (Mid-term Vision)
COM YAML system should support rapid iteration through validation cycle tooling and development feedback mechanisms. Instant validation feedback enables content creators to detect errors immediately on save, without needing to run the game and trigger the affected COM scenario.

### Problem (Current Issue)
F565 implements YAML COM runtime where changes already take effect on next Execute call (no caching). However, content creators lack immediate validation feedback - they must run the game and trigger specific scenarios to discover YAML errors. No validation cycle tooling exists for quick error detection during development.

### Goal (What to Achieve)
Implement validation cycle tooling for YAML COM files: automatic validation on file save with immediate error feedback, enabling content creators to catch errors without running the game.

### Impact Analysis

| Component | Change Type | Description |
|-----------|-------------|-------------|
| Era.Core/Commands/Com/ComHotReload.cs | New file | Validation watcher implementation |
| Era.Core/Data/YamlComLoader.cs | Dependency | Used for YAML file parsing |
| Era.Core/Commands/Com/YamlComValidator.cs | Dependency | Used for schema validation (static) |
| Game/data/coms/ | Watched directory | FileSystemWatcher target |
| Era.Core.Tests/ | New file | Unit tests for validation watcher |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Validation watcher implementation | file | Glob | exists | Era.Core/Commands/Com/ComHotReload.cs | [x] |
| 2 | File watcher integration (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | FileSystemWatcher | [x] |
| 3 | Validation on file change (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | ValidateSchema | [x] |
| 4 | Thread safety (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | lock | [x] |
| 5 | Error recovery callback (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | OnValidationError | [x] |
| 6 | Debounce implementation (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | Timer | [x] |
| 7 | DI constructor pattern (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | IComLoader | [x] |
| 8 | IDisposable pattern (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | IDisposable | [x] |
| 9 | File access retry verification (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | retry | [x] |
| 10 | Enable/disable configuration (ComHotReload.cs) | code | Grep "Era.Core/Commands/Com/ComHotReload.cs" | contains | HotReloadEnabled | [x] |
| 11 | Validation watcher unit test | file | Glob | exists | Era.Core.Tests/Commands/Com/ComHotReloadTests.cs | [x] |
| 12 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 13 | All tests pass | test | dotnet test | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,7 | Create ComHotReload.cs with IComLoader constructor parameter | [x] |
| 2 | 2 | Implement FileSystemWatcher setup for YAML COM files | [x] |
| 3 | 3,9 | Implement validation pipeline (YamlComLoader + YamlComValidator) with retry strategy | [x] |
| 4 | 4 | Add thread synchronization (lock) around watcher callbacks | [x] |
| 5 | 5 | Implement OnValidationError callback for validation failure handling | [x] |
| 6 | 6 | Implement debounce timer for batching rapid changes | [x] |
| 7 | 8 | Implement IDisposable pattern for resource cleanup | [x] |
| 8 | 10 | Implement HotReloadEnabled configuration property | [x] |
| 9 | 11 | Create unit tests for validation watcher functionality | [x] |
| 10 | 12 | Verify build success | [x] |
| 11 | 13 | Verify all tests pass | [x] |

---

## Dependencies

| Type | Feature | Status | Relationship | Notes |
|------|---------|--------|--------------|-------|
| Predecessor | F565 | [DONE] | Predecessor | COM YAML Runtime Integration |

---

## Links

- [index-features.md](index-features.md)
- [feature-565.md](feature-565.md) - COM YAML Runtime Integration (Predecessor)

---

## Implementation Contract

### Dependency Injection
- ComHotReload constructor takes `IComLoader` parameter. Use constructor overloading: parameterless constructor calls primary constructor with new YamlComLoader()
- Required using: `using Era.Core.Data;` for YamlComLoader and IComLoader
- Enables testing with mock loader and future loader implementations
- Note: YamlComValidator.ValidateSchema is called statically (not injected) as it is a stateless utility class

### Configuration
- HotReloadEnabled property (default: true) controls whether watcher is active
- When false: FileSystemWatcher.EnableRaisingEvents = false (no events fired)
- Constructor parameter or property setter - both acceptable patterns for infra features

### Resource Disposal
- ComHotReload : IDisposable
- Dispose() sequence: (1) set disposed flag under lock, (2) disable watcher (EnableRaisingEvents=false), (3) dispose Timer, (4) dispose FileSystemWatcher
- Implement standard dispose pattern with disposed flag to prevent double-dispose
- Callbacks check disposed flag before proceeding to prevent race conditions

### FileSystemWatcher Configuration
- Filter: `*.yaml` files in `Game/data/coms/`
- Watch subdirectories: Yes (recursive - nested structure: daily/, training/bondage/, etc.)
- Events: Changed, Created, Renamed, Deleted
- NotifyFilters: LastWrite, FileName, DirectoryName
- Set EnableRaisingEvents = true after configuration to start watching
- Note: Deleted events log removal but skip validation (no file to validate)
- Note: Only .yaml files are watched. Project convention requires COM YAML files to use .yaml extension (not .yml). This is intentional and consistent with Game/data/coms/ file naming.

### Debounce Strategy
- Delay: 100ms after last change event (configurable via constructor parameter)
- Rationale: 100ms handles most editor auto-save patterns; configurable for environments with different save behaviors
- Purpose: Batch rapid consecutive changes (e.g., editor auto-save)
- Implementation: Timer reset on each event, fire validation when timer expires

### Validation Pipeline
1. FileSystemWatcher detects change
2. Debounce timer expires
3. Parse YAML file using injected `_comLoader.Load(filePath)` - ComHotReload wraps Load() call with retry strategy (exponential backoff with 3 retries over 500ms to handle editor file locks)
   - On `Result.Fail`: Invoke OnValidationError callback with parse error, skip to step 6
   - On `Result.Ok`: Proceed to step 4 with ComDefinition
4. Call `YamlComValidator.ValidateSchema(comDefinition)` (static) which returns `Result<bool>`
   - On `Result.Fail`: Invoke OnValidationError callback with validation error, skip to step 6
   - On `Result.Ok`: Proceed to step 5
5. On success: Log validation success (YAML changes already take effect on next Execute call - no cache to invalidate)
6. On failure: Call OnValidationError callback (log error with file path and error details)

### Thread Synchronization
- FileSystemWatcher callbacks run on ThreadPool threads
- Debounce timer callback also runs on ThreadPool
- Lock object: `private readonly object _syncRoot = new object();`
- Use single lock scope per callback invocation (watcher callback locks once around timer reset + state check; timer callback locks once around validation + error callback)
- Prevent race conditions between multiple rapid file changes and timer interactions

### Rollback Plan
1. Disable hot-reload via config flag (`HotReloadEnabled = false`)
2. Revert commit if issues found in production
3. Create follow-up feature for fixes if needed

---

## Review Notes

- [resolved] Phase0-RefCheck iter1: AC#1 format fixed - changed to Glob/exists
- [resolved] Phase0-RefCheck iter1: AC#3 path corrected to tools/YamlValidator/
- [resolved] Phase1 iter1: AC#3 fixed - now verifies ValidateSchema call in hot-reload code instead of referencing existing CLI tool
- [resolved] Phase1 iter1: Tasks clarified with concrete deliverables (Task#1-7)
- [resolved] Phase1 iter1: AC count expanded to 9 (thread safety, error recovery, cache invalidation, unit test added)
- [resolved] Phase1 iter1: Implementation Contract added with design details (FileSystemWatcher config, debounce, validation pipeline, thread sync)
- [resolved] Phase1 iter2: Implementation Contract path fixed from 'Game/YAML/Kojo/COM_*.yaml' to 'Game/data/coms/' with recursive watching
- [resolved] Phase1 iter2: AC#2-6 Grep paths fixed to specific filename 'ComHotReload.cs'
- [resolved] Phase1 iter2: Rollback Plan added to Implementation Contract
- [resolved] Phase1 iter2: Impact Analysis table added to Background section
- [resolved] Phase1 iter3: Task#1-2 scope overlap resolved - Task#1 (FileSystemWatcher setup + event emission) and Task#2 (validation handler logic on change events) are logically distinct implementation steps
- [resolved] Phase1 iter3: Validation Pipeline updated to reference existing YamlComLoader.Load() for YAML parsing
- [resolved] Phase1 iter3: Dependencies table Status column added showing F565 [DONE]
- [resolved] Phase1 iter4: Cache invalidation scope removed - YamlComExecutor has no cache (files re-read on each Execute call)
- [resolved] Phase1 iter4: AC#6 (InvalidateCache) and Task#5 removed, ACs renumbered (now 8 total)
- [resolved] Phase1 iter4: Impact Analysis updated - removed YamlComExecutor.cs modification row
- [resolved] Phase1 iter4: Philosophy/Problem/Goal reframed as 'validation cycle tooling' instead of 'hot-reload for instant changes' (changes already work without restart)
- [resolved] Phase1 iter4: Implementation Contract step 5 updated - no cache to invalidate, just log success
- [resolved] Phase1 iter5: AC#1 Glob pattern fixed - changed from wildcard *HotReload*.cs to explicit ComHotReload.cs (Windows forward slash issue)
- [resolved] Phase1 iter6: AC#6 Glob pattern fixed to explicit path 'Era.Core.Tests/Commands/Com/ComHotReloadTests.cs' (matches existing test structure)
- [resolved] Phase1 iter6: Task#1 updated to include YAML deserialization via YamlComLoader
- [resolved] Phase1 iter6: NotifyFilters added to FileSystemWatcher Configuration
- [resolved] Phase1 iter7: Validation Pipeline step 3 updated for Result<ComDefinition> handling (Result.Fail → OnValidationError)
- [resolved] Phase1 iter7: Impact Analysis updated with YamlComLoader.cs dependency
- [resolved] Phase1 iter7: Thread Synchronization updated to include debounce timer callback and timer reset/fire operations
- [resolved] Phase1 iter7: Debounce Strategy rationale added and marked as configurable
- [resolved] Phase1 iter8: Tasks split into atomic units (1 AC = 1 Task) per feature-template.md
- [resolved] Phase1 iter8: AC#6 added for debounce Timer implementation verification
- [resolved] Phase1 iter8: Dependency Injection section added to Implementation Contract (IComLoader parameter)
- [resolved] Phase1 iter8: _syncRoot initialization guidance added to Thread Synchronization
- [resolved] Phase1 iter8: Validation Pipeline step 4 updated for Result<bool> handling
- [resolved] Phase1 iter11: Test coverage for specific behaviors (debounce, thread safety) verified via AC#8 unit test file existence - detailed test method coverage is implementation responsibility, not AC requirement for infra type
- [resolved] Phase1 iter9: AC#7 added for IComLoader DI constructor pattern verification
- [resolved] Phase1 iter9: FileSystemWatcher Events updated to include Deleted (with note: skip validation for deleted files)
- [resolved] Phase1 iter9: Namespace clarification added to Dependency Injection section (Era.Core.Data)
- [resolved] Phase1 iter9: AC#2 test path confirmed consistent with existing test structure (no change needed)
- [resolved] Phase1 iter11: AC Type column verified against testing SKILL - all types (file, code, build, test) are valid per AC Types table
- [resolved] Phase1 iter10: Impact Analysis updated with YamlComValidator.cs dependency (static utility)
- [resolved] Phase1 iter10: Dependency Injection section clarified - YamlComValidator is stateless static class, not injected
- [resolved] Phase2 iter10: Validation Pipeline updated to use injected _comLoader instead of direct YamlComLoader
- [resolved] Phase2 iter10: Class name 'ComHotReload' kept for consistency with feature scope - class performs hot validation (immediate feedback) which aligns with "hot-reload" naming pattern
- [resolved] Phase2 iter10: Deleted event behavior covered by implementation specification (logs removal, skips validation) - no additional AC required as this is error handling, not core functionality
- [resolved] Phase2 iter10: Default constructor parameter verification covered by AC#7 (IComLoader DI constructor pattern) - validates constructor signature includes IComLoader parameter
- [resolved] Phase3 iter10: AC table restructured to standard format - added Method column per feature-template.md requirement
- [resolved] Phase1 iter12: Thread Synchronization clarified - single lock scope per callback invocation specified to prevent deadlock
- [resolved] Phase1 iter12: FileSystemWatcher Configuration updated - EnableRaisingEvents = true requirement added to start watching
- [resolved] Phase2 iter13: IDisposable pattern added - AC#8 and Task#7 for proper FileSystemWatcher and Timer cleanup
- [resolved] Phase2 iter13: Validation Pipeline updated with file access retry strategy - exponential backoff with 3 retries over 500ms to handle editor file locks
- [resolved] Phase2 iter13: Task#8 split into Task#8-9 for clearer AC:Task traceability (build vs test verification)
- [resolved] Phase1 iter14: AC#8-9 added for IDisposable pattern and file access retry verification to enforce Zero Debt Upfront principle
- [resolved] Phase1 iter14: AC count updated to 12 (within infra range 8-15) and Task mapping updated to maintain 1:1 AC:Task alignment
- [resolved] Phase1 iter15: AC#2-9 Method column format fixed - changed from 'Grep' to 'Grep "Era.Core/Commands/Com/ComHotReload.cs"' per testing SKILL requirements for code type ACs
- [resolved] Phase1 iter16: FileSystemWatcher Configuration updated - clarified .yaml only filter (not .yml) per project convention, consistent with Game/data/coms/ naming
- [resolved] Phase2 iter17: Validation Pipeline updated - clarified ComHotReload wraps Load() call with retry logic (not YamlComLoader internal retry)
- [resolved] Phase2 iter17: Dependency Injection updated - changed from default parameter to constructor overloading pattern for C# compatibility
- [resolved] Philosophy Gate iter18: AC#10 and Task#8 added - HotReloadEnabled configuration capability per Philosophy requirement for "development feedback mechanisms" (configurable tools)

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 10:50 | START | implementer | Task 1-11 | - |
| 2026-01-21 10:50 | END | implementer | Task 1-11 | SUCCESS |

---