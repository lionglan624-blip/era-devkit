# Feature 539: SchemaValidator with Era.Core.Validation Layer

## Status: [PROPOSED]

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
Phase 17: Data Migration - Provide batch validation capabilities for Phase 17 YAML migration with auto-schema detection, enabling comprehensive quality control across all 43 CSV→YAML conversions through a single unified tool. Establish proper layer separation with Era.Core.Validation as infrastructure implementation layer.

### Prior Art
**Existing tools/YamlValidator** provides basic validation but lacks:
- Auto-schema detection (requires explicit `--schema` argument)
- Phase 17 batch directory support (`--phase17-directory`)
- DI integration for runtime use by other tools (e.g., F538 CsvToYaml)
- Error aggregation mode for batch reporting

Reference: `tools/YamlValidator/Program.cs` lines 57-119 (single file validation), lines 122-165 (directory validation)

### Problem (Current Issue)
Phase 17 data migration requires validation of 43 CSV→YAML conversions, but the existing YamlValidator tool lacks Phase 17-specific validation features and batch processing capabilities needed for comprehensive migration verification. Additionally, proper DDD layer separation requires an infrastructure layer for validation implementations.

### Goal (What to Achieve)
Create SchemaValidator with proper layer separation:
1. Define ISchemaValidator interface in Era.Core/Validation/ (domain abstraction)
2. Create Era.Core.Validation project for implementation (infrastructure layer)
3. DI registration in Era.Core.Validation (not Era.Core - layer separation)
4. CLI wrapper in tools/SchemaValidator/ (thin entry point)

This establishes the correct architectural pattern: Domain interfaces in Era.Core, implementations in Era.Core.Validation, CLI tools as thin wrappers.

### Dependencies

| Feature | Relationship | Description |
|---------|--------------|-------------|
| F538 | Successor | F538 (CsvToYaml) references Era.Core.Validation for ISchemaValidator |
| Era.Core | ProjectReference | Era.Core.Validation references Era.Core for interface types |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Era.Core.Validation project exists | file | Glob | exists | "Era.Core.Validation/Era.Core.Validation.csproj" | [ ] |
| 2 | Era.Core.Validation references Era.Core | code | Grep | contains | "ProjectReference.*Era.Core" | [ ] |
| 3 | ISchemaValidator.cs exists in Era.Core | file | Glob | exists | "Era.Core/Validation/ISchemaValidator.cs" | [ ] |
| 4 | ValidationSummary.cs exists in Era.Core | file | Glob | exists | "Era.Core/Validation/ValidationSummary.cs" | [ ] |
| 5 | SchemaValidatorImpl.cs exists in Era.Core.Validation | file | Glob | exists | "Era.Core.Validation/SchemaValidatorImpl.cs" | [ ] |
| 6 | SchemaDetector.cs exists in Era.Core.Validation | file | Glob | exists | "Era.Core.Validation/SchemaDetector.cs" | [ ] |
| 7 | ErrorAggregator.cs exists in Era.Core.Validation | file | Glob | exists | "Era.Core.Validation/ErrorAggregator.cs" | [ ] |
| 8 | DI registration in Era.Core.Validation | code | Grep | contains | "AddSingleton.*ISchemaValidator.*SchemaValidatorImpl" | [ ] |
| 9 | CLI project exists | file | Glob | exists | "tools/SchemaValidator/SchemaValidator.csproj" | [ ] |
| 10 | CLI references Era.Core.Validation | code | Grep | contains | "ProjectReference.*Era.Core.Validation" | [ ] |
| 11 | Program.cs exists | file | Glob | exists | "tools/SchemaValidator/Program.cs" | [ ] |
| 12 | ValidateFile method in interface | code | Grep | contains | "Result.*Unit.*ValidateFile" | [ ] |
| 13 | ValidateDirectory method in interface | code | Grep | contains | "Result.*ValidationSummary.*ValidateDirectory" | [ ] |
| 14 | DetectSchemaType method in interface | code | Grep | contains | "Result.*string.*DetectSchemaType" | [ ] |
| 15 | CLI help shows Phase 17 options | output | Bash | contains | "--phase17-directory" | [ ] |
| 16 | CLI help shows auto-detect option | output | Bash | contains | "--auto-detect-schema" | [ ] |
| 17 | CLI help shows aggregate errors option | output | Bash | contains | "--aggregate-errors" | [ ] |
| 18 | Multi-schema validation test | test | Bash | succeeds | - | [ ] |
| 19 | Batch processing test | test | Bash | succeeds | - | [ ] |
| 20 | DI integration test | test | Bash | succeeds | - | [ ] |
| 21 | Zero technical debt (Era.Core/Validation) | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 22 | Zero technical debt (Era.Core.Validation) | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 23 | Zero technical debt (tools/SchemaValidator) | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 24 | Zero technical debt (tests) | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 25 | Era.Core.Validation build succeeds | build | Bash | succeeds | "dotnet build Era.Core.Validation/" | [ ] |
| 26 | CLI build succeeds | build | Bash | succeeds | "dotnet build tools/SchemaValidator/" | [ ] |

### AC Details

**AC#1**: Era.Core.Validation project file existence
- Test: Glob pattern="Era.Core.Validation/Era.Core.Validation.csproj"
- Expected: File exists (new infrastructure layer project)

**AC#2**: Era.Core.Validation references Era.Core
- Test: Grep pattern="ProjectReference.*Era.Core" path="Era.Core.Validation/Era.Core.Validation.csproj"
- Expected: Contains project reference to Era.Core

**AC#3**: ISchemaValidator.cs existence in Era.Core
- Test: Glob pattern="Era.Core/Validation/ISchemaValidator.cs"
- Expected: File exists (interface in domain layer)

**AC#4**: ValidationSummary.cs existence in Era.Core
- Test: Glob pattern="Era.Core/Validation/ValidationSummary.cs"
- Expected: File exists (return type in domain layer)

**AC#5**: SchemaValidatorImpl.cs existence in Era.Core.Validation
- Test: Glob pattern="Era.Core.Validation/SchemaValidatorImpl.cs"
- Expected: File exists (implementation in infrastructure layer)

**AC#6**: SchemaDetector.cs existence in Era.Core.Validation
- Test: Glob pattern="Era.Core.Validation/SchemaDetector.cs"
- Expected: File exists

**AC#7**: ErrorAggregator.cs existence in Era.Core.Validation
- Test: Glob pattern="Era.Core.Validation/ErrorAggregator.cs"
- Expected: File exists

**AC#8**: DI registration in Era.Core.Validation
- Test: Grep pattern="AddSingleton.*ISchemaValidator.*SchemaValidatorImpl" path="Era.Core.Validation/"
- Expected: DI registration in infrastructure layer (NOT Era.Core)

**AC#9**: CLI project file existence
- Test: Glob pattern="tools/SchemaValidator/SchemaValidator.csproj"
- Expected: File exists (thin CLI wrapper)

**AC#10**: CLI references Era.Core.Validation
- Test: Grep pattern="ProjectReference.*Era.Core.Validation" path="tools/SchemaValidator/SchemaValidator.csproj"
- Expected: CLI references infrastructure layer

**AC#11**: Program.cs existence
- Test: Glob pattern="tools/SchemaValidator/Program.cs"
- Expected: File exists (CLI entry point only)

**AC#12**: ValidateFile method in interface
- Test: Grep pattern="Result.*Unit.*ValidateFile" path="Era.Core/Validation/ISchemaValidator.cs"
- Expected: Method signature in interface

**AC#13**: ValidateDirectory method in interface
- Test: Grep pattern="Result.*ValidationSummary.*ValidateDirectory" path="Era.Core/Validation/ISchemaValidator.cs"
- Expected: Method signature in interface

**AC#14**: DetectSchemaType method in interface
- Test: Grep pattern="Result.*string.*DetectSchemaType" path="Era.Core/Validation/ISchemaValidator.cs"
- Expected: Method signature in interface

**Note on AC#12-14 patterns**: Patterns use `.*` instead of angle brackets for flexibility with whitespace variations. This is intentional per ENGINE.md Issue 3.

**AC#15**: CLI help displays Phase 17 options
- Test: `dotnet run --project tools/SchemaValidator/ -- --help`
- Expected: Output contains '--phase17-directory' option

**AC#16**: CLI help shows auto-detect option
- Test: `dotnet run --project tools/SchemaValidator/ -- --help`
- Expected: Output contains '--auto-detect-schema' option

**AC#17**: CLI help shows aggregate errors option
- Test: `dotnet run --project tools/SchemaValidator/ -- --help`
- Expected: Output contains '--aggregate-errors' option

**AC#18**: Multi-schema validation unit test
- Test: `dotnet test Era.Core.Validation.Tests/ --filter "FullyQualifiedName~TestSchemaValidatorMultiSchemaValidation"`
- File: Era.Core.Validation.Tests/SchemaValidatorTests.cs
- Expected: Tests pass
- Scenario: Create test directory with 3+ YAML files of different schema types (dialogue, gamebase, unknown). Use existing schema files from tools/YamlSchemaGen/ and tools/schemas/. Verify each file is validated against its detected schema type. "Unknown" files get YAML syntax check only.
- Test Fixtures: Test project creates temporary YAML files matching known schema patterns for validation.

**AC#19**: Batch processing unit test
- Test: `dotnet test Era.Core.Validation.Tests/ --filter "FullyQualifiedName~TestSchemaValidatorBatchProcessing"`
- File: Era.Core.Validation.Tests/SchemaValidatorTests.cs
- Expected: Tests pass
- Scenario: Create test directory with 10+ YAML files (mix of valid/invalid). Verify ValidateDirectory returns correct TotalFiles, PassedFiles, FailedFiles counts and aggregated Errors list.
- Test Fixtures: Test creates temporary directory with YAML files, including intentionally malformed files to verify error aggregation.

**AC#20**: DI integration test
- Test: `dotnet test Era.Core.Validation.Tests/ --filter "FullyQualifiedName~TestSchemaValidatorDIIntegration"`
- File: Era.Core.Validation.Tests/SchemaValidatorTests.cs
- Expected: Tests pass (verify DI container can resolve ISchemaValidator)
- Scenario: Build ServiceCollection with AddEraValidation(), resolve ISchemaValidator, verify it's SchemaValidatorImpl instance and can execute ValidateFile.
- Test Fixtures: Uses in-memory test YAML content, no external schema files required for DI verification.

**AC#21**: Zero technical debt (Era.Core/Validation)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Validation/" type=cs
- Expected: 0 matches

**AC#22**: Zero technical debt (Era.Core.Validation)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Validation/" type=cs
- Expected: 0 matches

**AC#23**: Zero technical debt (tools/SchemaValidator)
- Test: Grep pattern="TODO|FIXME|HACK" path="tools/SchemaValidator/" type=cs
- Expected: 0 matches

**AC#24**: Zero technical debt (tests)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Validation.Tests/" type=cs
- Expected: 0 matches

**AC#25**: Era.Core.Validation build succeeds
- Test: `dotnet build Era.Core.Validation/`
- Expected: Build succeeds with exit code 0

**AC#26**: CLI build succeeds
- Test: `dotnet build tools/SchemaValidator/`
- Expected: Build succeeds with exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create Era.Core.Validation project with Era.Core reference | [ ] |
| 2 | 3,4,12,13,14 | Create ISchemaValidator interface and ValidationSummary in Era.Core/Validation/ | [ ] |
| 3 | 5,6,7 | Implement SchemaValidatorImpl, SchemaDetector, ErrorAggregator in Era.Core.Validation/ | [ ] |
| 4 | 8 | Create DI registration extension in Era.Core.Validation/ | [ ] |
| 5 | 9,10,11 | Create CLI wrapper in tools/SchemaValidator/ | [ ] |
| 6 | 15,16,17 | Implement CLI interface with Phase 17 options | [ ] |
| 7 | 18,19,20 | Create comprehensive unit tests in Era.Core.Validation.Tests/ | [ ] |
| 8 | 21,22,23,24,25,26 | Code quality verification and build validation | [ ] |

<!-- Batch verification waivers:
- Task 2 (AC 3,4,12,13,14): Interface and return type are cohesive domain abstractions
- Task 3 (AC 5,6,7): Implementation classes form cohesive infrastructure unit
- Task 5 (AC 9,10,11): CLI project structure created together
- Task 6 (AC 15,16,17): CLI options are single feature
- Task 7 (AC 18,19,20): Test suite creation for cohesive test categories
- Task 8 (AC 21-26): Quality gates run together
-->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Architecture Overview

```
Era.Core/                           # Domain Layer
├── Validation/
│   ├── ISchemaValidator.cs         # Interface (domain abstraction)
│   └── ValidationSummary.cs        # Return type (domain model)

Era.Core.Validation/                # Infrastructure Layer (NEW PROJECT)
├── Era.Core.Validation.csproj      # References Era.Core
├── SchemaValidatorImpl.cs          # ISchemaValidator implementation
├── SchemaDetector.cs               # Schema type detection
├── ErrorAggregator.cs              # Error collection
└── ServiceCollectionExtensions.cs  # DI registration

Era.Core.Validation.Tests/          # Test Project
├── Era.Core.Validation.Tests.csproj
└── SchemaValidatorTests.cs

tools/SchemaValidator/              # CLI Wrapper (thin)
├── SchemaValidator.csproj          # References Era.Core.Validation
└── Program.cs                      # Entry point only
```

### Reference Direction (Critical)

```
Era.Core (interface)
    ↑
Era.Core.Validation (implementation) ←── tools/SchemaValidator (CLI)
    ↑
    └─────────────────────────────────────────
                      ↑
              tools/CsvToYaml (F538)
```

**Era.Core references nothing** (top-level domain layer)

### File Structure

**Domain Layer (Era.Core/Validation/)**:

| Path | Purpose |
|------|---------|
| `Era.Core/Validation/ISchemaValidator.cs` | Schema validation interface |
| `Era.Core/Validation/ValidationSummary.cs` | Batch validation result record |

**Infrastructure Layer (Era.Core.Validation/)**:

| Path | Purpose |
|------|---------|
| `Era.Core.Validation/Era.Core.Validation.csproj` | Project file with Era.Core reference |
| `Era.Core.Validation/SchemaValidatorImpl.cs` | ISchemaValidator implementation |
| `Era.Core.Validation/SchemaDetector.cs` | Schema type detection logic |
| `Era.Core.Validation/ErrorAggregator.cs` | Error collection and formatting |
| `Era.Core.Validation/ServiceCollectionExtensions.cs` | DI registration extension |

**CLI Layer (tools/SchemaValidator/)**:

| Path | Purpose |
|------|---------|
| `tools/SchemaValidator/SchemaValidator.csproj` | CLI project (references Era.Core.Validation) |
| `tools/SchemaValidator/Program.cs` | Entry point only |

**Test Layer**:

| Path | Purpose |
|------|---------|
| `Era.Core.Validation.Tests/Era.Core.Validation.Tests.csproj` | Test project |
| `Era.Core.Validation.Tests/SchemaValidatorTests.cs` | Unit tests |

### Interface Definition

**Era.Core/Validation/ISchemaValidator.cs** (Domain Layer):
```csharp
using Era.Core.Types;

namespace Era.Core.Validation;

// Note: Era.Core.Types provides Result<T> and Unit types

/// <summary>Schema validation abstraction for YAML files</summary>
public interface ISchemaValidator
{
    /// <summary>Validate single YAML file against detected schema</summary>
    /// <param name="yamlPath">Path to YAML file</param>
    /// <returns>Success or validation error</returns>
    Result<Unit> ValidateFile(string yamlPath);

    /// <summary>Validate directory of YAML files with batch processing</summary>
    /// <param name="directoryPath">Path to directory</param>
    /// <returns>Validation summary with results</returns>
    Result<ValidationSummary> ValidateDirectory(string directoryPath);

    /// <summary>Detect appropriate schema for given YAML file</summary>
    /// <param name="yamlPath">Path to YAML file</param>
    /// <returns>Schema type name or error</returns>
    Result<string> DetectSchemaType(string yamlPath);
}
```

**Era.Core/Validation/ValidationSummary.cs**:
```csharp
namespace Era.Core.Validation;

/// <summary>Summary of batch validation results</summary>
public record ValidationSummary(
    int TotalFiles,
    int PassedFiles,
    int FailedFiles,
    IReadOnlyList<string> Errors);
```

### Project Dependencies

**Era.Core.Validation/Era.Core.Validation.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Era.Core\Era.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="NJsonSchema" Version="11.*" />
    <PackageReference Include="YamlDotNet" Version="16.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.*" />
  </ItemGroup>
</Project>
```

**tools/SchemaValidator/SchemaValidator.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Era.Core.Validation\Era.Core.Validation.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="System.CommandLine" Version="2.*" />
  </ItemGroup>
</Project>
```

### SchemaDetector Specification

**Era.Core.Validation/SchemaDetector.cs**:
```csharp
namespace Era.Core.Validation;

/// <summary>Detects schema type based on YAML file characteristics</summary>
public class SchemaDetector
{
    /// <summary>
    /// Detection algorithm (priority order):
    /// 1. Filename pattern matching:
    ///    - "*_talent*.yaml" → "talent"
    ///    - "*_dialogue*.yaml" or "*_kojo*.yaml" → "dialogue"
    ///    - "*_config*.yaml" → "config"
    /// 2. Root key inspection (if filename doesn't match):
    ///    - Has "dialogue_groups" key → "dialogue"
    ///    - Has "talents" or "talent_list" key → "talent"
    ///    - Has "settings" or "config" key → "config"
    /// 3. Fallback: return "unknown" (validation will use generic schema)
    /// </summary>
    public string Detect(string yamlPath) { ... }
}
```

**Design Rationale**: Returns `string` instead of enum for extensibility. New schema types can be added without modifying existing code.

**Schema File Resolution**:
- Schema name maps to existing schema locations:
  - `"dialogue"` → `tools/YamlSchemaGen/dialogue-schema.json`
  - `"gamebase"` → `tools/schemas/GameBase.schema.json`
  - `"variablesize"` → `tools/schemas/VariableSize.schema.json`
- Future schema types will follow pattern: `tools/schemas/{schemaType}.schema.json`

**Unknown Schema Handling**:
- When DetectSchemaType returns `"unknown"`:
  1. ValidateFile logs warning: `"No schema detected for {filename}, performing basic YAML syntax check only"`
  2. Performs YAML parse validation only (no JSON Schema validation)
  3. Returns `Result<Unit>.Success()` if YAML parses correctly
- This allows Phase 17 migration files to be batch-processed even before all schema types are defined

### ErrorAggregator Specification

**Era.Core.Validation/ErrorAggregator.cs**:
```csharp
namespace Era.Core.Validation;

/// <summary>Collects and formats validation errors across batch operations</summary>
public class ErrorAggregator
{
    private readonly List<ValidationError> _errors = new();

    /// <summary>Add error for a specific file</summary>
    public void Add(string filePath, string message, int? line = null);

    /// <summary>Get all collected errors</summary>
    public IReadOnlyList<string> GetFormattedErrors();

    /// <summary>Clear all collected errors</summary>
    public void Clear();

    /// <summary>Check if any errors were collected</summary>
    public bool HasErrors { get; }
}

/// <summary>Single validation error</summary>
internal record ValidationError(string FilePath, string Message, int? Line);
```

**Aggregation Behavior**:
- Per-file errors are collected during ValidateDirectory
- GetFormattedErrors() returns list in format: `"{FilePath}:{Line}: {Message}"` or `"{FilePath}: {Message}"` if no line
- Used when `--aggregate-errors` CLI option is specified

### DI Registration

**Era.Core.Validation/ServiceCollectionExtensions.cs** (Infrastructure Layer):
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Era.Core.Validation;

/// <summary>DI registration for validation services</summary>
public static class ValidationServiceCollectionExtensions
{
    /// <summary>Add Era validation services to DI container</summary>
    public static IServiceCollection AddEraValidation(this IServiceCollection services)
    {
        services.AddSingleton<ISchemaValidator, SchemaValidatorImpl>();
        return services;
    }
}
```

**Usage in consumers**:
```csharp
// tools/SchemaValidator/Program.cs or tools/CsvToYaml or future runtime
var services = new ServiceCollection();
services.AddEraValidation();
var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ISchemaValidator>();
```

### CLI Interface

**Command Format**:
```bash
dotnet run --project tools/SchemaValidator/ -- [options]
```

**Phase 17 Options**:
- `--phase17-directory <path>` - Validate all Phase 17 YAML files
- `--auto-detect-schema` - Auto-detect appropriate schema for each file
- `--aggregate-errors` - Collect all errors before reporting

**CLI Parsing Specification**:
Use `System.CommandLine` library (same pattern as existing tools/ErbLinter). This provides:
- Automatic `--help` generation
- Type-safe argument parsing
- Consistent error messages

```csharp
// tools/SchemaValidator/Program.cs
using System.CommandLine;

var phase17DirOption = new Option<DirectoryInfo?>(
    "--phase17-directory",
    "Validate all YAML files in Phase 17 directory");

var autoDetectOption = new Option<bool>(
    "--auto-detect-schema",
    "Auto-detect schema type for each file");

var aggregateOption = new Option<bool>(
    "--aggregate-errors",
    "Collect all errors before reporting");

var rootCommand = new RootCommand("SchemaValidator - YAML schema validation for Era")
{
    phase17DirOption,
    autoDetectOption,
    aggregateOption
};

rootCommand.SetHandler((phase17Dir, autoDetect, aggregate) => { ... },
    phase17DirOption, autoDetectOption, aggregateOption);

return await rootCommand.InvokeAsync(args);
```

### Test Naming Convention

Test methods follow `Test{ClassName}{Scenario}` format:
- `TestSchemaValidatorMultiSchemaValidation`
- `TestSchemaValidatorBatchProcessing`
- `TestSchemaValidatorDIIntegration`

### Error Message Format

For validation failures: `"SchemaValidator: {FileName} failed validation - {ErrorDetails}"` with Japanese error details where user-facing.

---

## Review Notes
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-18 FL iter1-5**: [resolved] Multiple iterations fixed minor issues (AC paths, Philosophy, etc.)
- **2026-01-18 FL iter6**: [resolved] Architecture redesign - Era.Core.Validation project created to resolve layer separation issue. DI registration moved from Era.Core to Era.Core.Validation. CLI becomes thin wrapper.
- **2026-01-18 FL iter7**: [resolved] Fixed AC#12-14 patterns to use flexible regex without angle brackets. Added test file locations to AC#18-20. Fixed ServiceCollectionExtensions namespace redundancy. Added Task batch waivers.
- **2026-01-18 FL iter8**: [resolved] Fixed 引継ぎ先指定 to reference F540 (Post-Phase Review) instead of F541 (KojoEngine). Added pattern flexibility note for AC#12-14. Clarified Unit type origin in interface snippet.
- **2026-01-19 FL iter9**: [resolved] Maintainability review - Added Prior Art section (existing YamlValidator reference), SchemaDetector/ErrorAggregator specifications, project dependencies (NJsonSchema, YamlDotNet, System.CommandLine), CLI parsing specification, test scenario details for AC#18-20, string return type rationale.
- **2026-01-19 FL iter10**: [resolved] Spec review - Added Schema File Resolution mapping to existing schema locations, Unknown Schema Handling specification (YAML syntax check fallback), test fixtures clarification for AC#18-20, fixed interface comment style.

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Integration with CI/CD pipeline | Phase 17 focus, CI integration is Phase 18 scope | Feature | F540 (Post-Phase Review tracks Phase 17→18 handoffs) |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links
[index-features.md](index-features.md) | [feature-516.md](feature-516.md) | [feature-538.md](feature-538.md) | [feature-540.md](feature-540.md)
