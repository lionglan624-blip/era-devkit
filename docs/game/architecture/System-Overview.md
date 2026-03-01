# System Architecture Overview

High-level overview of the COM YAML infrastructure and runtime architecture for ERA game development.

## Purpose

This document provides a comprehensive architectural overview of the COM YAML system, covering:

- **COM YAML Infrastructure** - Performance optimization, hot reload, and schema validation
- **Runtime Architecture** - Engine integration and runtime execution flow
- **Development Workflow** - Tools for schema generation, validation, and rapid iteration

For detailed modding capabilities, see [COM YAML Modding Guide](../modding/COM-YAML-Guide.md). For data format migration decisions, see [CSV-YAML Mapping](../data-formats/CSV-YAML-Mapping.md).

## COM YAML Infrastructure

The COM YAML system consists of three core infrastructure components that enable efficient development and runtime execution.

### F580: COM Loader Performance Optimization

**Purpose:** Eliminate redundant YAML parsing operations through intelligent caching with file modification time invalidation.

**Key Components:**

- **ComDefinitionCache** - Injectable cache service shared between YamlComLoader and CustomComLoader
- **File Modification Tracking** - Uses `IFileSystem.GetLastWriteTime()` for cache invalidation
- **Thread Safety** - `ConcurrentDictionary<string, CacheEntry>` for concurrent access
- **Performance Monitoring** - Cache hit/miss statistics with F580 timing prefix

**Architecture:**

```
YamlComLoader ──┐
                ├──> ComDefinitionCache (singleton) ──> File System
CustomComLoader ┘                                        (modification time)
```

**Integration Points:**

- `YamlComLoader.Load()` - Checks cache before YAML parsing
- `CustomComLoader.LoadCustomComs()` - Checks cache before deserializing mod files
- `ComHotReload` - Calls `IComLoader.InvalidateCache()` on file change events

**Performance Impact:** Measured >10% reduction in COM loading time (from F570 baseline analysis).

**Reference:** [feature-580.md](../../agents/feature-580.md)

### F581: ComHotReload CI Integration

**Purpose:** Ensure clean test execution and accurate exit codes through proper resource management in background threads.

**Problem Solved:** FileSystemWatcher background threads writing to Console after test framework shutdown caused `exit code 1` despite all tests passing.

**Solution:**

- **ILogger Dependency Injection** - Replace `Console.WriteLine` with `ILogger<ComHotReload>`
- **NullLogger in Tests** - Use `NullLogger<ComHotReload>.Instance` to prevent console output during test execution
- **Disposal Safety** - ILogger is thread-safe and disposal-safe for background thread operations

**Pattern Documentation:**

```csharp
// Pattern: Background thread logging with ILogger disposal safety
// Use ILogger dependency injection instead of Console.WriteLine for FileSystemWatcher events
// to prevent TextWriter disposal exceptions during test framework shutdown
```

**Reference:** [feature-581.md](../../agents/feature-581.md)

### F590: YAML Schema Validation Tools

**Purpose:** Provide comprehensive tooling for YAML schema generation and validation to enable early error detection and IDE integration.

**Tools:**

1. **YamlSchemaGen** - JSON Schema generator for dialogue YAML files
   - Generates `dialogue-schema.json` (JSON Schema Draft-07)
   - Defines structure for character, situation, branches
   - Supports variable types: TALENT, ABL, EXP, FLAG, CFLAG
   - Comparison operators: eq, ne, gt, gte, lt, lte

2. **YamlValidator** - Schema validation CLI
   - Single file validation: `--yaml <path>`
   - Directory validation: `--validate-all <dir>`
   - Exit codes: 0 (success), 1 (failure)
   - Structured error reporting with JSONPath context

**Development Workflow:**

```bash
# 1. Generate schema
dotnet run --project tools/YamlSchemaGen/

# 2. Validate YAML files
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --yaml Game/YAML/Kojo/COM_K1_0.yaml

# 3. Batch validation (CI mode)
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --validate-all Game/YAML/Kojo/
```

**CI Integration:**

- Pre-commit hooks for staged YAML validation
- GitHub Actions workflows for PR validation
- Build pipeline integration before tests

**Schema Versioning:**

- **Breaking changes:** Require YAML file updates (new required properties, type changes)
- **Non-breaking changes:** Backward compatible (optional properties, new variable types)
- Version control: Schema file checked into git with descriptive commit messages

**Reference:** [feature-590.md](../../agents/feature-590.md), [YamlSchemaGen README](../../../tools/YamlSchemaGen/README.md), [YamlValidator README](../../../tools/YamlValidator/README.md)

## Runtime Architecture

The runtime architecture defines how COM YAML files are loaded, executed, and integrated with the game engine.

### F565: COM YAML Runtime Integration

**Purpose:** Enable runtime execution of YAML-defined COM (character dialogue) content within the ERA game engine.

**Key Components:**

1. **YamlComLoader** - Core YAML loader with caching (F580)
   - Deserializes YAML files into `ComDefinition` objects
   - Integrates with `ComDefinitionCache` for performance
   - Handles file modification time tracking

2. **CustomComLoader** - Modding support for custom YAML files
   - Loads mod files from custom directories
   - Uses same cache infrastructure as YamlComLoader
   - Integrates with `IFileSystem` abstraction

3. **IComLoader Interface** - Abstraction for COM loading
   - `Load(string filePath)` - Load COM definition
   - `InvalidateCache(string filePath)` - Cache invalidation hook

**Runtime Flow:**

```
YAML File ──> YamlComLoader ──> ComDefinitionCache ──> ComDefinition
                                        ↓
                                   Era.Core
                                        ↓
                                  Engine Rendering
```

### Engine Integration Points

**Era.Core Role:**

- **YAML Rendering** - Converts ComDefinition objects to executable dialogue
- **Game Constants** - Provides DIM.ERH constants (F364) and SYSTEM.ERB initialization (F365)
- **State Management** - Integrates with Body/Pregnancy/Weather state (F370) and NTR initialization (F371)
- **Runtime Validation** - Ensures YAML content matches game state requirements

**Engine Execution:**

- **uEmuera.Headless.csproj** - Headless mode for development and testing
- **uEmuera.exe** - Unity-based GUI for user verification
- **Build Process** - Unity CLI build with working directory set to `Game/`

### Hot Reload for Rapid Iteration

**ComHotReload** enables real-time YAML file updates during development:

1. **FileSystemWatcher** - Monitors `Game/data/coms/` directory
2. **Debounce Timer** - 100ms delay to handle multiple rapid changes
3. **Cache Invalidation** - Calls `IComLoader.InvalidateCache()` on file change
4. **Automatic Reload** - Reloads COM definitions without restarting engine

**Event Flow:**

```
File Change ──> FileSystemWatcher ──> Debounce Timer ──> InvalidateCache ──> Reload
```

**Reference:** [feature-565.md](../../agents/feature-565.md) (if exists), [COM YAML Guide](../modding/COM-YAML-Guide.md)

## Development Workflow

The development workflow integrates schema generation, validation, and rapid iteration for efficient YAML content creation.

### Schema Generation (YamlSchemaGen)

**When to Generate:**

- Adding new variable types (MARK, PALAM)
- Modifying dialogue structure (new required fields)
- Adding comparison operators
- Changing validation rules

**Workflow:**

1. Modify `tools/YamlSchemaGen/Program.cs` (`GenerateDialogueSchema()` method)
2. Add unit tests in `tools/YamlSchemaGen.Tests/SchemaValidationTests.cs`
3. Regenerate schema: `dotnet run --project tools/YamlSchemaGen/`
4. Verify changes: `git diff tools/YamlSchemaGen/dialogue-schema.json`
5. Test validation: Run YamlValidator against existing files

**Output:** `tools/YamlSchemaGen/dialogue-schema.json` (JSON Schema Draft-07)

**Reference:** [YamlSchemaGen README](../../../tools/YamlSchemaGen/README.md)

### Validation (YamlValidator, com-validator)

**YamlValidator** - CLI validation tool:

- **Single file:** `--yaml <path>` for targeted validation
- **Directory:** `--validate-all <dir>` for CI/batch mode
- **Exit codes:** 0 (success), 1 (failure) for CI integration
- **Error reporting:** JSONPath context for precise error location

**com-validator** - Community YAML validator with Japanese support:

- Enhanced error messages for Japanese content creators
- Additional validation rules for community modding
- Integration with community tooling ecosystem

**CI Integration Examples:**

```bash
# Pre-commit hook
dotnet run --project tools/YamlSchemaGen/ && \
  dotnet run --project tools/YamlValidator/ -- \
    --schema tools/YamlSchemaGen/dialogue-schema.json \
    --yaml "$staged_file"

# GitHub Actions
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --validate-all Game/YAML/Kojo/
```

**Reference:** [YamlValidator README](../../../tools/YamlValidator/README.md), [src/tools/go/com-validator/](../../../src/tools/go/com-validator/)

### Hot Reload for Rapid Iteration

**Development Cycle:**

1. **Edit YAML file** in `Game/data/coms/` or `Game/YAML/Kojo/`
2. **Automatic detection** via FileSystemWatcher (100ms debounce)
3. **Cache invalidation** clears stale ComDefinition
4. **Immediate reload** in running engine instance
5. **Verify changes** without restart

**Performance Benefits:**

- **Zero restart time** - Changes apply within milliseconds
- **Preserved game state** - No need to replay scenarios
- **Rapid experimentation** - Iterate on dialogue without interruption

**Error Handling:**

- Validation errors logged via ILogger
- Failed reloads preserve previous valid state
- Clear error messages with file path context

**Reference:** [feature-572.md](../../agents/feature-572.md) (if exists)

## Cross-References

### Modding and Content Creation

- [COM YAML Modding Guide](../modding/COM-YAML-Guide.md) - Tier 1+2 moddability capabilities
- [CSV-YAML Mapping](../data-formats/CSV-YAML-Mapping.md) - Migration decisions and Tier 3 rationale

### Feature Documentation

- [Feature Dependencies](Feature-Dependencies.md) - F563-F593 dependency graph
- [feature-580.md](../../agents/feature-580.md) - COM Loader Performance Optimization
- [feature-581.md](../../agents/feature-581.md) - ComHotReload CI Integration
- [feature-590.md](../../agents/feature-590.md) - YAML Schema Validation Tools

### Tool Documentation

- [YamlSchemaGen README](../../../tools/YamlSchemaGen/README.md) - Schema generation tool
- [YamlValidator README](../../../tools/YamlValidator/README.md) - Schema validation CLI
- [com-validator](../../../src/tools/go/com-validator/) - Community YAML validator

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-25 | Initial creation (F564) - COM YAML infrastructure documentation |
