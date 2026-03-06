# Feature 580: COM Loader Performance Optimization

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
Performance optimization should eliminate repeated expensive operations through intelligent caching without sacrificing functionality. COM loader performance directly impacts game startup time and hot-reload responsiveness. Caching with file modification time invalidation provides the optimal balance between performance and freshness guarantees.

### Problem (Current Issue)
F570 YAML COM Performance Analysis identified YAML parsing as the primary bottleneck, consuming >10% of total execution time during COM loading. The current `YamlComLoader` creates a new `DeserializerBuilder` and parses YAML content for every single file load, even when the same file is loaded multiple times without modifications.

### Goal (What to Achieve)
Implement COM definition caching in `YamlComLoader` and `CustomComLoader` with file modification time invalidation to eliminate redundant YAML parsing operations. Achieve measurable performance improvement in COM loading scenarios while maintaining correctness through proper cache invalidation.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Cache implementation added | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "class.*ComDefinitionCache" | [x] |
| 2 | File modification time tracking | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "LastWriteTime" | [x] |
| 3 | Cache invalidation logic | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "IsStale\\|Invalidate\\|InvalidateCache\\|CacheInvalid" | [x] |
| 4 | Custom COM loader caching | code | Grep(Era.Core/Commands/Com/CustomComLoader.cs) | contains | "ComDefinitionCache" | [x] |
| 5 | Performance timing preserved | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "\\[F580\\].*cache" | [x] |
| 6 | Cache hit logging | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "cache hit" | [x] |
| 7 | Cache miss logging | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "cache miss" | [x] |
| 8 | Deserializer instance reuse | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "static.*Deserializer\\|readonly.*[Dd]eserializer\\|Lazy.*Deserializer" | [x] |
| 9 | Thread-safe cache implementation | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "ConcurrentDictionary\\|lock" | [x] |
| 10 | Unit tests for cache behavior | file | Glob(Era.Core.Tests/**/*ComCache*Tests.cs) | exists | - | [x] |
| 11 | Cache performance test | code | Grep(Era.Core.Tests/**/*.cs) | contains | "Stopwatch\\|ElapsedMilliseconds.*cache\\|ComCachePerformanceTests" | [x] |
| 12 | Hot reload cache integration | code | Grep(Era.Core/Commands/Com/ComHotReload.cs) | contains | "InvalidateCache\\|cache.*Invalidate\\|cache.*Clear" | [x] |
| 13 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 14 | Tests pass | build | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 15 | IFileSystem extended | code | Grep(Era.Core/IO/IFileSystem.cs) | contains | "GetLastWriteTime" | [x] |
| 16 | Injectable cache in YamlComLoader | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "ComDefinitionCache\\|_comDefinitionCache\\|private.*ComDefinitionCache" | [x] |
| 17 | CustomComLoader cache usage | code | Grep(Era.Core/Commands/Com/CustomComLoader.cs) | contains | "ComDefinitionCache.*LoadFromCache\\|cache.*Get\\|TryGetValue" | [x] |
| 18 | IComLoader cache invalidation interface | code | Grep(Era.Core/Data/IComLoader.cs) | contains | "InvalidateCache" | [x] |
| 19 | Cache hit ratio logging | code | Grep(Era.Core/Data/YamlComLoader.cs) | contains | "hit.*ratio\\|ratio.*100\\|operations.*cache" | [x] |

### AC Details

**AC#1**: Cache implementation added
- Test: `Grep("class.*ComDefinitionCache", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache class is implemented within YamlComLoader

**AC#2**: File modification time tracking
- Test: `Grep("LastWriteTime", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: File timestamps are captured for invalidation

**AC#3**: Cache invalidation logic
- Test: `Grep("IsStale\\|Invalidate\\|InvalidateCache\\|CacheInvalid", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache staleness check with specific method naming patterns

**AC#4**: Custom COM loader caching
- Test: `Grep("ComDefinitionCache", "Era.Core/Commands/Com/CustomComLoader.cs")`
- Verifies: CustomComLoader uses the same cache mechanism

**AC#5**: Performance timing preserved
- Test: `Grep("\\[F580\\].*cache", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache timing measurements with literal [F580] prefix added in code (preserving F570 pattern). Code presence required, not runtime output.

**AC#6**: Cache hit logging
- Test: `Grep("cache hit", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache hits are logged for performance monitoring

**AC#7**: Cache miss logging
- Test: `Grep("cache miss", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache misses are logged for performance monitoring

**AC#8**: Deserializer instance reuse
- Test: `Grep("static.*Deserializer\\|readonly.*[Dd]eserializer\\|Lazy.*Deserializer", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Deserializer creation overhead eliminated with flexible implementation patterns

**AC#9**: Thread-safe cache implementation
- Test: `Grep("ConcurrentDictionary\\|lock", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache handles concurrent access safely

**AC#10**: Unit tests for cache behavior
- Test: `Glob("Era.Core.Tests/**/*ComCache*Tests.cs")`
- Verifies: Cache functionality has comprehensive test coverage

**AC#11**: Cache performance test
- Test: `Grep("Stopwatch\\|ElapsedMilliseconds.*cache\\|ComCachePerformanceTests", "Era.Core.Tests/**/*Com*Tests.cs")`
- Verifies: Performance test exists with timing measurement or dedicated performance test class in COM test files

**AC#12**: Hot reload cache integration
- Test: `Grep("InvalidateCache\\|cache.*Invalidate\\|cache.*Clear", "Era.Core/Commands/Com/ComHotReload.cs")`
- Verifies: ComHotReload calls IComLoader.InvalidateCache on file change events using dependency injection pattern

**AC#15**: IFileSystem extended
- Test: `Grep("GetLastWriteTime", "Era.Core/IO/IFileSystem.cs")`
- Verifies: IFileSystem interface includes GetLastWriteTime method for CustomComLoader cache integration

**AC#16**: Injectable cache in YamlComLoader
- Test: `Grep("ComDefinitionCache.*inject\\|private.*cache\\|_cache", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: YamlComLoader receives ComDefinitionCache via dependency injection

**AC#18**: IComLoader cache invalidation interface
- Test: `Grep("InvalidateCache", "Era.Core/Commands/Com/IComLoader.cs")`
- Verifies: IComLoader interface extended with InvalidateCache method for ComHotReload integration

**AC#19**: Cache hit ratio logging
- Test: `Grep("hit.*ratio\\|ratio.*100\\|operations.*cache", "Era.Core/Data/YamlComLoader.cs")`
- Verifies: Cache hit ratio statistics are logged for performance monitoring

**AC#17**: CustomComLoader cache usage
- Test: `Grep("ComDefinitionCache.*LoadFromCache\\|cache.*Get\\|TryGetValue", "Era.Core/Commands/Com/CustomComLoader.cs")`
- Verifies: CustomComLoader actually uses cache for loading operations, not just references the class

**AC#13**: Build succeeds
- Test: `dotnet build Era.Core`
- Expected: Clean build with no errors

**AC#14**: Tests pass
- Test: `dotnet test Era.Core.Tests`
- Expected: All tests pass including new cache tests

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 8 | Extract deserializer to static/readonly field to eliminate recreation overhead | [x] |
| 2 | 1 | Implement ComDefinitionCache injectable class service | [x] |
| 3 | 2 | Add file modification time tracking | [x] |
| 4 | 3 | Implement cache invalidation logic | [x] |
| 5 | 9 | Implement thread-safe cache using ConcurrentDictionary or lock mechanism | [x] |
| 6 | 5 | Add F580 timing prefix to cache measurements | [x] |
| 7 | 6 | Add cache hit logging | [x] |
| 8 | 7 | Add cache miss logging | [x] |
| 9 | 19 | Add cache hit ratio logging for performance monitoring | [x] |
| 10 | 18 | Extend IComLoader interface with InvalidateCache method | [x] |
| 11 | 15 | Extend IFileSystem interface with GetLastWriteTime method | [x] |
| 12 | 16 | Inject ComDefinitionCache into YamlComLoader constructor | [x] |
| 13 | 4 | Add ComDefinitionCache reference to CustomComLoader | [x] |
| 14 | 17 | Implement cache lookup in CustomComLoader.LoadCustomComs() | [x] |
| 15 | 12 | Modify ComHotReload to use IComLoader.InvalidateCache on file change events | [x] |
| 16 | 10 | Create ComDefinitionCacheTests.cs for cache behavior unit tests | [x] |
| 17 | 11 | Add performance test with Stopwatch measurement for cache improvement | [x] |
| 18 | 13,14 | Verify build and test execution | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Cache Design Requirements

1. **Cache Key**: Use absolute file path as cache key
2. **Cache Value**: Store `(ComDefinition, DateTime lastModified)` tuple
3. **Invalidation**: Compare `File.GetLastWriteTime()` with cached timestamp
4. **Thread Safety**: Use `ConcurrentDictionary<string, CacheEntry>` or equivalent
5. **Logging**: Include cache hit/miss statistics with F580 prefix
6. **Integration Points**:
   - `YamlComLoader.Load()` method
   - `CustomComLoader.LoadCustomComs()` method
   - `ComHotReload` file change notifications

### ComHotReload Cache Integration Architecture

ComHotReload will use **instance-based cache with dependency injection**:
- ComDefinitionCache implemented as injectable instance service
- Extend IComLoader interface with `InvalidateCache(string filePath)` method
- ComHotReload calls `_comLoader.InvalidateCache(filePath)` on FileSystemWatcher events
- YamlComLoader implements InvalidateCache to clear cached entry
- Pattern: `InvalidateCache(` or `cache.*Invalidate` or `cache.*Clear`

### Architectural Decision: CustomComLoader Cache Integration

CustomComLoader currently creates its own DeserializerBuilder instance (lines 51-54). To achieve AC#4 (CustomComLoader caching), the cache will be implemented as a **shared injectable service** following dependency injection principles:

- Extract `ComDefinitionCache` as a separate injectable class service
- Both YamlComLoader and CustomComLoader receive cache via constructor dependency injection
- CustomComLoader modifies its LoadCustomComs() method to check cache before deserializing
- Cache handles both standard YAML files (YamlComLoader) and custom mod files (CustomComLoader)
- Follows existing DI patterns (IFileSystem in CustomComLoader, IComLoader in ComHotReload)

### Dependency Injection Implementation

- **YamlComLoader Constructor**: Add `ComDefinitionCache cache` parameter to constructor
- **ComHotReload Update**: Modify ComHotReload to pass cache instance: `new YamlComLoader(ComDefinitionCache.Instance)` or use singleton pattern
- **CustomComLoader Constructor**: Add `ComDefinitionCache cache` parameter to constructor
- **Pattern**: Constructor injection with singleton cache instance across all components

### IFileSystem Integration for Cache

CustomComLoader uses IFileSystem abstraction, not direct File.GetLastWriteTime(). To support cache invalidation in CustomComLoader:

- Extend IFileSystem interface with `GetLastWriteTime(string path)` method
- Modify RealFileSystem class in Era.Core/Commands/Com/CustomComLoader.cs to implement GetLastWriteTime method
- Cache uses IFileSystem.GetLastWriteTime() for timestamp comparison in CustomComLoader context
- YamlComLoader can use direct File.GetLastWriteTime() as it doesn't use IFileSystem abstraction

### Performance Measurement

- Preserve existing F570 parsing time measurements
- Add F580 cache lookup time measurements
- Log cache hit/miss statistics with F580 prefix (AC#6, AC#7)
- Include cache statistics in performance reports

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F570 | [DONE] | F570 identified YAML parsing bottleneck and added timing infrastructure |

<!-- Dependency Types: Predecessor (BLOCKING), Successor (informational), Related (reference) -->

---

## Review Notes
- [resolved iter6] Phase1 iter1: AC#3 matcher 'IsStale.*modification' may be overly prescriptive, coupling to specific method naming. Consider more flexible pattern.
- [resolved iter6] Phase1 iter1: AC#8 alternation pattern may need verification for regex mode and consideration of Lazy<IDeserializer> patterns.
- [resolved iter6] Phase1 iter1: AC#12 ComHotReload cache integration architectural appropriateness - explicit cache invalidation vs timestamp-based self-invalidation.
- [dismissed: merged with iter1] Phase1 iter2: AC#3 'IsStale.*modification' prescriptive but aligns with documented GetLastWriteTime() design. Could be more behavior-focused.
- [dismissed: merged with iter1] Phase1 iter2: AC#12 explicit cache invalidation vs timestamp-based self-invalidation - architectural design question per Implementation Contract integration points.
- [resolved iter6] Phase1 iter2: Task ordering suggestion - deserializer extraction (Task#4) could come after cache implementation for logical flow.
- [resolved iter6] Phase1 iter3: AC#8 pattern could include Lazy.*Deserializer for comprehensive coverage per Review Notes.
- [dismissed: acceptable] Phase1 iter3: 14 ACs at upper bound of infra range (8-15) - consolidation of AC#6+AC#7 logging ACs suggested but they verify distinct behaviors.
- [dismissed: correct observation] Phase1 iter4: AC#8 alternation pattern suggestion for Lazy.*Deserializer valid per Review Notes but based on incorrect Grep alternation concern.

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Memory usage optimization | Cache size limits and LRU eviction out of scope | Feature | F581 |

<!-- Note: "TBD" removed from example per CLAUDE.md TBD Prohibition -->
<!-- Tracking destination options (CLAUDE.md Deferred Task Protocol):
- Feature: Create new Feature → Destination ID = F{ID}
- Task: Add to existing Feature Tasks → Destination ID = F{ID}#T{N}
- Phase: Add to architecture.md Phase Tasks → Destination ID = Phase {N}
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 13:16 | START | implementer | Tasks 1-5 | - |
| 2026-01-21 13:16 | END | implementer | Tasks 1-5 | SUCCESS |
| 2026-01-21 13:20 | START | implementer | Tasks 6-9 | - |
| 2026-01-21 13:20 | END | implementer | Tasks 6-9 | SUCCESS |
| 2026-01-21 13:21 | START | implementer | Tasks 10-14 | - |
| 2026-01-21 13:21 | END | implementer | Tasks 10-14 | SUCCESS |
| 2026-01-21 13:23 | START | implementer | Tasks 15-18 | - |
| 2026-01-21 13:27 | END | implementer | Tasks 15-18 | SUCCESS |
| 2026-01-21 13:30 | VERIFY | opus | Phase 6 AC Verification | All 19 ACs PASS |
| 2026-01-21 13:32 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: IFileSystem.GetLastWriteTime not documented in engine-dev SKILL.md |
| 2026-01-21 13:34 | FIX | opus | doc-check fix | Added GetLastWriteTime to engine-dev SKILL.md IFileSystem section |

## Links
- [feature-570.md](feature-570.md) - YAML COM Performance Analysis (predecessor)
- [index-features.md](index-features.md)