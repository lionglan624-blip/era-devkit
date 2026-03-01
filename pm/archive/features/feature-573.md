# Feature 573: COM YAML Community Customization Framework

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

## Type: engine

## Background

### Philosophy (Mid-term Vision)
COM YAML system should enable community customization through extensibility patterns. Community customization support allows modders to add new COMs, override existing behaviors, and create custom game modifications through data-driven configuration.

### Problem (Current Issue)
F565 implements YAML COM runtime but lacks community customization framework. No patterns exist for adding new COMs, overriding existing ones, or creating modular customization packages. Community cannot extend game behavior through YAML modifications.

Placeholder CustomComLoader.cs exists (from F565 prep) with stub LoadCustomComs returning yield break. This feature completes the implementation.

### Goal (What to Achieve)
Implement community customization framework with concrete extensibility patterns:
1. **File-based overrides**: Mod YAML files override core YAML by matching COM ID
2. **Load order**: Core COMs loaded first, then mod COMs in alphabetical directory order
3. **Merge strategy**: Full replacement (mod COM completely replaces core COM with same ID)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Stub implementation replaced | code | Grep | not_contains | yield break | [x] |
| 2 | LoadCustomComs scans mod directories | code | Grep | contains | GetDirectories | [x] |
| 3 | Override/merge logic implemented | code | Grep | contains | MergeWithOverrides | [x] |
| 4 | Mod directory structure exists | file | Glob | exists | Game/mods/README.md | [x] |
| 5 | Mod YAML discoverable | test | dotnet test --filter Category=CustomComLoader | succeeds | - | [x] |
| 6 | Invalid mod structure handled | test | dotnet test --filter Category=InvalidModStructure | succeeds | - | [x] |
| 7 | Invalid YAML handled | test | dotnet test --filter Category=InvalidYaml | succeeds | - | [x] |
| 8 | Missing mods directory handled | test | dotnet test --filter Category=MissingModsDirectory | succeeds | - | [x] |
| 9 | Merge override behavior | test | dotnet test --filter Category=MergeOverrideBehavior | succeeds | - | [x] |
| 10 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 11 | All tests pass | test | dotnet test | succeeds | - | [x] |
| 12 | Zero technical debt (source) | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |
| 13 | Zero technical debt (tests) | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |
| 14 | Uses YamlComValidator | code | Grep | contains | YamlComValidator | [x] |
| 15 | Multi-mod override precedence | test | dotnet test --filter Category=MultiModPrecedence | succeeds | - | [x] |
| 16 | IFileSystem interface created | file | Glob | exists | Era.Core/IO/IFileSystem.cs | [x] |
| 17 | Zero technical debt (IFileSystem) | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |
| 18 | ICustomComLoader interface created | file | Glob | exists | Era.Core/Commands/Com/ICustomComLoader.cs | [x] |
| 19 | CustomComLoader implements ICustomComLoader | code | Grep | matches | CustomComLoader.*:.*ICustomComLoader | [x] |
| 20 | IFileSystem constructor injection | code | Grep | contains | "IFileSystem" | [x] |
| 21 | Zero technical debt (ICustomComLoader) | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |
| 22 | RealFileSystem implements IFileSystem | code | Grep | contains | class RealFileSystem : IFileSystem | [x] |
| 23 | ICustomComLoader DI registration | file | Grep | contains | AddSingleton.*ICustomComLoader.*CustomComLoader | [x] |
| 24 | Invalid file path handling test | test | dotnet test --filter Category=InvalidFilePath | succeeds | - | [x] |
| 25 | File system exception handling test | test | dotnet test --filter Category=FileSystemException | succeeds | - | [x] |
| 26 | Empty mod directory test | test | dotnet test --filter Category=EmptyModDirectory | succeeds | - | [x] |

### AC Details

| AC# | Verification Command | Notes |
|:---:|---------------------|-------|
| 1 | `rg "yield break" Era.Core/Commands/Com/CustomComLoader.cs` | Verifies stub replaced with real implementation |
| 2 | `rg "GetDirectories" Era.Core/Commands/Com/CustomComLoader.cs` | Verifies actual directory scanning logic |
| 3 | `rg "MergeWithOverrides" Era.Core/Commands/Com/CustomComLoader.cs` | Override resolution: mod COMs replace core COMs by ID |
| 4 | Glob pattern=Game/mods/README.md | README documents mod structure for community |
| 5 | `dotnet test --filter Category=CustomComLoader` | Uses [Trait("Category", "CustomComLoader")] |
| 6 | `dotnet test --filter Category=InvalidModStructure` | Uses [Trait("Category", "InvalidModStructure")] |
| 7 | `dotnet test --filter Category=InvalidYaml` | Verifies YAML files failing YamlComValidator.ValidateSchema() are skipped and loader continues to next file |
| 8 | `dotnet test --filter Category=MissingModsDirectory` | Uses [Trait("Category", "MissingModsDirectory")] |
| 9 | `dotnet test --filter Category=MergeOverrideBehavior` | Verifies mod COM with ID X replaces core COM with ID X |
| 10 | `dotnet build` | No build errors |
| 11 | `dotnet test` | All tests pass including new CustomComLoader tests |
| 12 | `rg "TODO\|FIXME\|HACK" Era.Core/Commands/Com/CustomComLoader.cs` | No technical debt markers in source |
| 13 | `rg "TODO\|FIXME\|HACK" Era.Core.Tests/Commands/Com/CustomComLoaderTests.cs` | No technical debt markers in tests |
| 14 | `rg "YamlComValidator" Era.Core/Commands/Com/CustomComLoader.cs` | CustomComLoader uses YamlComValidator from F565 for schema validation |
| 15 | `dotnet test --filter Category=MultiModPrecedence` | Verifies alphabetically later mod wins when same COM ID in multiple mods |
| 16 | Glob pattern=Era.Core/IO/IFileSystem.cs | IFileSystem interface exists at expected location |
| 17 | `rg "TODO\|FIXME\|HACK" Era.Core/IO/IFileSystem.cs` | No technical debt markers in IFileSystem interface |
| 18 | Glob pattern=Era.Core/Commands/Com/ICustomComLoader.cs | ICustomComLoader interface exists at expected location |
| 19 | `rg "CustomComLoader.*:.*ICustomComLoader" Era.Core/Commands/Com/CustomComLoader.cs` | CustomComLoader implements interface for DI |
| 20 | `rg "IFileSystem" Era.Core/Commands/Com/CustomComLoader.cs` | Verifies IFileSystem dependency injection in constructor |
| 21 | `rg "TODO\|FIXME\|HACK" Era.Core/Commands/Com/ICustomComLoader.cs` | No technical debt markers in ICustomComLoader interface |
| 22 | `rg "class RealFileSystem : IFileSystem" Era.Core/Commands/Com/CustomComLoader.cs` | RealFileSystem internal class implements IFileSystem for production use |
| 23 | `rg "AddSingleton.*ICustomComLoader.*CustomComLoader" Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` | Verifies DI registration for ICustomComLoader interface |
| 24 | `dotnet test --filter Category=InvalidFilePath` | Tests CustomComLoader behavior with invalid file paths in mod directory |
| 25 | `dotnet test --filter Category=FileSystemException` | Tests CustomComLoader graceful handling of file system exceptions (DirectoryNotFoundException, etc.) |
| 26 | `dotnet test --filter Category=EmptyModDirectory` | Tests CustomComLoader behavior when mod directory exists but is empty |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Replace stub yield break with real implementation | [x] |
| 2 | 2 | Implement LoadCustomComs to scan mod directories | [x] |
| 3 | 3 | Implement override/merge logic (MergeWithOverrides) | [x] |
| 4 | 4 | Create Game/mods/ directory with README (usage guide for modders) | [x] |
| 5 | 5 | Write CustomComLoader unit tests with [Trait("Category", "CustomComLoader")] | [x] |
| 6 | 6 | Write InvalidModStructure test with [Trait("Category", "InvalidModStructure")] | [x] |
| 7 | 7 | Write InvalidYaml test with [Trait("Category", "InvalidYaml")] | [x] |
| 8 | 8 | Write MissingModsDirectory test with [Trait("Category", "MissingModsDirectory")] | [x] |
| 9 | 9 | Write MergeOverrideBehavior test with [Trait("Category", "MergeOverrideBehavior")] | [x] |
| 10 | 10 | Verify build succeeds | [x] |
| 11 | 11 | Verify all tests pass | [x] |
| 12 | 12 | Remove TODO/FIXME/HACK from CustomComLoader.cs | [x] |
| 13 | 13 | Ensure no TODO/FIXME/HACK in test files | [x] |
| 14 | 14 | Integrate YamlComValidator for schema validation | [x] |
| 15 | 15 | Write MultiModPrecedence test with [Trait("Category", "MultiModPrecedence")] | [x] |
| 16 | 16 | Create Era.Core/IO/IFileSystem.cs with interface definition | [x] |
| 17 | 17 | Ensure no TODO/FIXME/HACK in IFileSystem interface | [x] |
| 18 | 18 | Create Era.Core/Commands/Com/ICustomComLoader.cs interface file | [x] |
| 19 | 19 | Implement CustomComLoader : ICustomComLoader class declaration | [x] |
| 20 | 20 | Refactor CustomComLoader to accept IFileSystem in constructor with null=real file system default | [x] |
| 21 | 21 | Ensure no TODO/FIXME/HACK in ICustomComLoader interface | [x] |
| 22 | 22 | Implement RealFileSystem : IFileSystem as internal class in CustomComLoader.cs | [x] |
| 23 | 23 | Register ICustomComLoader in ServiceCollectionExtensions.cs | [x] |
| 24 | 24 | Write InvalidFilePath negative test with [Trait("Category", "InvalidFilePath")] | [x] |
| 25 | 25 | Write FileSystemException negative test with [Trait("Category", "FileSystemException")] | [x] |
| 26 | 26 | Write EmptyModDirectory negative test with [Trait("Category", "EmptyModDirectory")] | [x] |

---

## Implementation Contract

### Interface Definitions

```csharp
using Era.Core.Data.Models;

namespace Era.Core.Commands.Com;

/// <summary>
/// Interface for custom COM loading operations. Enables DI and mocking for tests.
/// </summary>
public interface ICustomComLoader
{
    IEnumerable<ComDefinition> LoadCustomComs(string modPath);
    IEnumerable<ComDefinition> MergeWithOverrides(
        IEnumerable<ComDefinition> coreComs,
        IEnumerable<ComDefinition> modComs);
}

using System;

namespace Era.Core.IO;

/// <summary>
/// Abstraction for file system operations. Enables unit testing without real file system.
/// Error handling: Methods throw standard System.IO exceptions (DirectoryNotFoundException,
/// FileNotFoundException, etc.) following .NET conventions.
/// </summary>
public interface IFileSystem
{
    string[] GetDirectories(string path);
    string[] GetFiles(string path, string searchPattern);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    string ReadAllText(string path);
}
```

### Class Signatures

**Note**: Stub only has LoadCustomComs (yield break). MergeWithOverrides is a new method to be added.

```csharp
/// <summary>
/// Loads custom COM definitions from mod directories.
/// </summary>
public class CustomComLoader : ICustomComLoader
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Creates CustomComLoader with optional file system abstraction.
    /// When used with DI, IFileSystem will be injected. When created manually, null defaults to RealFileSystem.
    /// </summary>
    /// <param name="fileSystem">File system abstraction (null = real file system)</param>
    public CustomComLoader(IFileSystem? fileSystem = null);

    /// <summary>
    /// Scans mod directories and returns all custom COM definitions.
    /// </summary>
    /// <param name="modPath">Root path to mods directory (Game/mods/)</param>
    /// <returns>Enumerable of COM definitions from all discovered mod YAML files</returns>
    public IEnumerable<ComDefinition> LoadCustomComs(string modPath);

    /// <summary>
    /// Merges mod COMs with core COMs, applying override logic.
    /// Mod COMs with matching IDs replace core COMs entirely.
    /// </summary>
    /// <param name="coreComs">Base COM definitions from core YAML</param>
    /// <param name="modComs">Custom COM definitions from mods</param>
    /// <returns>Merged collection with overrides applied</returns>
    public IEnumerable<ComDefinition> MergeWithOverrides(
        IEnumerable<ComDefinition> coreComs,
        IEnumerable<ComDefinition> modComs);
}

/// <summary>
/// Production file system implementation wrapping System.IO operations.
/// Used when no mock IFileSystem is provided to CustomComLoader constructor.
/// Note: This class will be nested inside CustomComLoader.cs as internal implementation.
/// </summary>
internal class RealFileSystem : IFileSystem
{
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);
    public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public bool FileExists(string path) => File.Exists(path);
    public string ReadAllText(string path) => File.ReadAllText(path);
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// COM Community Customization - Feature 573
services.AddSingleton<ICustomComLoader, CustomComLoader>();
```

**Note**: IFileSystem is not registered in DI as RealFileSystem is an internal implementation detail. CustomComLoader constructor handles IFileSystem dependency injection when available or creates RealFileSystem internally when null.

### Mod Directory Structure

```
Game/mods/
├── README.md           # Community documentation
├── {mod-name}/         # One directory per mod
│   ├── mod.yaml        # Mod metadata (name, author, version)
│   └── coms/           # COM YAML files
│       └── *.yaml      # Custom/override COM definitions
```

### mod.yaml Schema

```yaml
# mod.yaml - Mod metadata file
name: string        # Required: Mod display name
author: string      # Optional: Mod author name
version: string     # Optional: Semver format (e.g., "1.0.0")
```

### README.md Content Requirements

README.md must document:
1. Mod directory structure example
2. COM YAML format reference (link to F565 docs)
3. Override behavior explanation (how mod COMs replace core COMs)
4. Load order rules (alphabetical by mod directory name)

### Override Resolution Algorithm

1. Load all core COMs from `Game/CSV/COM/`
2. Scan `Game/mods/` for mod directories (sorted by `StringComparer.OrdinalIgnoreCase`)
3. For each mod, load all YAML files from `{mod}/coms/`
4. Build merged collection: if mod COM ID matches core COM ID, replace entirely
5. Return merged collection

**Multiple mods overriding same COM ID**: Later mod (alphabetically) wins. Return type IEnumerable preserves final merged order.

### Error Handling

- **Invalid mod structure**: Missing `coms/` subdirectory or missing `mod.yaml`. Skip mod, continue loading others.
- **Invalid YAML**: Parse failure OR schema validation failure (uses YamlComValidator from F565). Skip file, continue.
- **Missing mods directory**: Return core COMs only (no error)

**Note**: AC#6 InvalidModStructure test covers both missing mod.yaml and missing coms/ subdirectory scenarios in a single test file.

**Validation Integration**: CustomComLoader.LoadCustomComs calls YamlComValidator.ValidateSchema(comDef) for each parsed COM. Invalid COMs are skipped (consistent with Error Handling strategy).

**Error Strategy Rationale**: Unlike YamlComLoader.LoadAll which aggregates errors via Result.Fail, CustomComLoader silently skips invalid mods/files. This is intentional: mods are user-contributed content with varying quality. Failing on one broken mod would prevent all mods from loading. Tests verify skip behavior via error handling ACs.

**Note**: Logging behavior is implementation detail, not verified by AC. Tests verify graceful continuation.

### Scope Boundary

**In Scope**: CustomComLoader class implementation with LoadCustomComs and MergeWithOverrides methods, interface definitions, and DI registration.

**Out of Scope**:
- Integration into YamlComLoader/YamlComExecutor loading pipeline. Integration requires new follow-up feature (to be created after F573 completion).

---

## Dependencies

| Type | Feature | Relationship | Notes |
|------|---------|--------------|-------|
| Predecessor | F565 | Successor | COM YAML Runtime Integration |

---

## Links

- [index-features.md](index-features.md)
- [feature-565.md](feature-565.md) - COM YAML Runtime Integration
- [feature-576.md](feature-576.md) - Integration with YamlComLoader loading pipeline

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 11:58 | START | implementer | Phase 3 TDD - Create real tests for Tasks 5-9, 15, 24-26 | - |
| 2026-01-21 11:58 | END | implementer | Phase 3 TDD - Tests created and verified (RED phase) | SUCCESS |

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Integration with YamlComLoader loading pipeline | Requires API changes to YamlComLoader for mod COM integration | Create follow-up feature | F576 |

---

## Review Notes
- [resolved] iter4: Added AC#9 MergeOverrideBehavior test for merge behavior verification
- [resolved] iter4: Specified StringComparer.OrdinalIgnoreCase for alphabetical sorting
- [resolved] iter4: Clarified logging is implementation detail, tests verify graceful continuation
- [deferred→NEW] iter9: Integration with ComYamlLoader out-of-scope, requires new feature to be created after F573
- [resolved] iter7: AC Method column format standardized (Method=Grep, pattern in Expected)
- [resolved] iter10: Added mod.yaml schema specification (name/author/version)
- [resolved] iter10: Added README content requirements specification
- [resolved] iter10: Added AC#14 for YamlComValidator integration verification
- [applied] POST-LOOP: Added ICustomComLoader interface for testability and DI
- [applied] POST-LOOP: Added IFileSystem abstraction for unit testing without file system
- [applied] POST-LOOP: Added AC#15 MultiModPrecedence test for multi-mod override verification
- [resolved] Phase1 iter1: YamlComValidator integration path already clear in Implementation Contract section (Validation Integration)
- [resolved] Phase1 iter2: Task#16/AC#16 correctly verify IFileSystem.cs file creation and zero-debt
- [resolved] Phase1 iter4: AC#1 follows standard not_contains matcher format per testing SKILL
- [resolved] Phase1 iter6: AC#7 InvalidYaml test verifies test passes - AC Details already specify expected validation failure skip behavior
- [resolved] Phase1 iter1: AC#19 pattern uses double quotes inside Expected column - removed quotes from Expected column in iter3
- [resolved] Phase1 iter3: DI registration not specified - clarified in Scope Boundary that DI registration is out-of-scope
- [resolved] Phase1 iter4: AC#19 Expected pattern exact whitespace may vary - changed to matches matcher with regex pattern for robustness
- [resolved] Phase1 iter5: AC#12/AC#13 pattern escaping TODO\\|FIXME\\|HACK may not be parsed correctly by verification tool - pattern is SSOT-compliant per SKILL.md
- [resolved] Phase1 iter6: AC#4 marked [x] in [PROPOSED] feature - pre-work completed before review, marking acceptable
- [resolved] Phase1 iter6: Review Notes pending cleanup - iter5 escaping concern was invalid, cleanup completed
- [resolved] Phase1 iter7: Minor review notes cleanup - pending notes were stale tracking items, now resolved
- [skipped] Phase1 iter8: AC#4 Status [x] in [REVIEWED] feature workflow - maintained as [x] since file exists (user decision)