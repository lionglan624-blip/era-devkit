# Feature 538: CsvToYaml Converter Tool

## Status: [BLOCKED]

**Blocked by**: F539 (SchemaValidator with Era.Core.Validation Layer) - ISchemaValidator interface not yet stable
**Unblock condition**: F539 reaches [REVIEWED] status

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
Phase 17: Data Migration - Establish a comprehensive data migration infrastructure that converts CSV definitions to YAML format with schema validation, creating a single source of truth for game data that enables Era.Core to manage all game constants independent of legacy CSV dependencies.

### Problem (Current Issue)
43 CSV files in `Game/CSV/` directory contain game constants that need migration to YAML format for Phase 17 Data Migration. Manual conversion would be error-prone and time-consuming, requiring a reliable batch converter tool.

### Goal (What to Achieve)
Create `tools/CsvToYaml/` converter tool that processes 43 CSV files in one batch operation, generating YAML equivalents with proper schema validation and zero technical debt.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CsvToYaml entry point exists | file | Glob | exists | "tools/CsvToYaml/Program.cs" | [ ] |
| 2 | Project file exists | file | Glob | exists | "tools/CsvToYaml/CsvToYaml.csproj" | [ ] |
| 3 | Main converter class | code | Grep | contains | "class CsvToYamlConverter" | [ ] |
| 4 | CSV parsing interface | code | Grep | contains | "interface ICsvParser" | [ ] |
| 5 | YAML serializer interface | code | Grep | contains | "interface IYamlSerializer" | [ ] |
| 6 | Batch processing capability | code | Grep | contains | "ProcessDirectory.*string inputPath.*string outputPath" | [ ] |
| 7 | CLI entry point | code | Grep | contains | "static.*Main.*string.*args" | [ ] |
| 8 | Help output | output | Bash | contains | "Usage: CsvToYaml <input-directory> --output <output-directory>" | [ ] |
| 9 | CSV count verification | unit | dotnet test | succeeds | "CsvToYamlTests" | [ ] |
| 10 | 43 CSV files processing | integration | dotnet test | succeeds | "ProcessAllCsvFiles" | [ ] |
| 11 | Schema validation integration | code | Grep | contains | "ISchemaValidator" | [ ] |
| 12 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 13 | Build succeeds | build | dotnet test | succeeds | - | [ ] |

### AC Details

**AC#1**: CsvToYaml entry point
- Test: Glob pattern="tools/CsvToYaml/Program.cs"
- Expected: Entry point file exists

**AC#2**: Project file verification
- Test: Glob pattern="tools/CsvToYaml/CsvToYaml.csproj"
- Expected: File exists with proper .NET project configuration

**AC#3**: Main converter implementation
- Test: Grep pattern="class CsvToYamlConverter" path="tools/CsvToYaml/" type=cs
- Expected: Core converter class with batch processing logic

**AC#4**: CSV parsing abstraction
- Test: Grep pattern="interface ICsvParser" path="tools/CsvToYaml/" type=cs
- Expected: Interface for CSV parsing operations

**AC#5**: YAML serialization abstraction
- Test: Grep pattern="interface IYamlSerializer" path="tools/CsvToYaml/" type=cs
- Expected: Interface for YAML generation operations

**AC#6**: Batch directory processing
- Test: Grep pattern="ProcessDirectory.*string inputPath.*string outputPath" path="tools/CsvToYaml/" type=cs
- Expected: Method that processes entire directory of CSV files

**AC#7**: CLI interface
- Test: Grep pattern="static.*Main.*string.*args" path="tools/CsvToYaml/" type=cs
- Expected: Console application entry point (supports both sync and async Main)

**AC#8**: Help documentation
- Test: `dotnet run --project tools/CsvToYaml -- --help`
- Expected: Usage instructions and parameter documentation

**AC#9**: Unit tests for core functionality
- Test: dotnet test --filter "FullyQualifiedName~CsvToYamlTests"
- Expected: Unit tests pass for converter logic

**AC#10**: Integration test for all 43 files
- Test: dotnet test --filter "FullyQualifiedName~ProcessAllCsvFiles"
- Expected: Integration test verifies all Game/CSV/*.csv files are processed correctly

**AC#11**: Schema validation integration
- Test: Grep pattern="ISchemaValidator" path="tools/CsvToYaml/" type=cs
- Expected: Integration with ISchemaValidator interface (created in F539)
- Note: F538 must execute after F539 to have ISchemaValidator available

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="tools/CsvToYaml/" type=cs AND Grep pattern="TODO|FIXME|HACK" path="tools/CsvToYaml.Tests/" type=cs
- Expected: 0 matches in both directories - production-ready code

**AC#13**: Build verification
- Test: `dotnet build tools/CsvToYaml/ && dotnet test tools/CsvToYaml.Tests/`
- Expected: Clean build and all tests pass

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create project structure and csproj file | [ ] |
| 2 | 3,4,5 | Implement core interfaces and converter class | [ ] |
| 3 | 6,7 | Implement batch processing and CLI interface | [ ] |
| 4 | 8 | Add comprehensive help and usage documentation | [ ] |
| 5 | 9,10 | Create unit and integration test suites | [ ] |
| 6 | 11 | Integrate schema validation | [ ] |
| 7 | 12,13 | Verify zero technical debt and successful build | [ ] |

<!-- **Batch verification waivers**:
- Task 1 (AC 1,2): Project structure and csproj are co-created in single operation
- Task 2 (AC 3,4,5): Interface definitions and implementation class form cohesive unit
- Task 3 (AC 6,7): Batch processing and CLI entry point are tightly coupled
- Task 5 (AC 9,10): Unit and integration tests are created together for test suite
- Task 7 (AC 12,13): Tech debt check and build verification are final quality gates
-->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Project Structure

**Project Reference for Schema Validation** (CsvToYaml.csproj):
```xml
<ItemGroup>
  <ProjectReference Include="../../Era.Core.Validation/Era.Core.Validation.csproj" />
</ItemGroup>
```

```
tools/CsvToYaml/
├── CsvToYaml.csproj          # Console application project (includes Era.Core.Validation reference)
├── Program.cs                # CLI entry point with argument parsing
├── Services/
│   ├── ICsvParser.cs         # CSV parsing interface
│   ├── CsvParser.cs          # CSV parsing implementation
│   ├── IYamlSerializer.cs    # YAML serialization interface
│   ├── YamlSerializer.cs     # YAML serialization implementation
│   └── CsvToYamlConverter.cs # Main conversion logic
└── Models/
    └── ConversionResult.cs   # Result type for conversion operations

tools/CsvToYaml.Tests/
├── CsvToYaml.Tests.csproj    # Test project
├── UnitTests/
│   ├── CsvParserTests.cs     # Unit tests for CSV parsing
│   ├── YamlSerializerTests.cs # Unit tests for YAML serialization
│   └── ConverterTests.cs     # Unit tests for conversion logic
└── IntegrationTests/
    └── BatchProcessingTests.cs # Integration tests for full workflow
```

### Interface Definitions

```csharp
// tools/CsvToYaml/Services/ICsvParser.cs
using Era.Core.Types;

namespace CsvToYaml.Services;

public interface ICsvParser
{
    // Returns generic Dictionary for flexibility - each CSV has different column structure.
    // Strongly-typed models are responsibility of downstream consumers (migration features).
    Result<IEnumerable<Dictionary<string, string>>> Parse(string filePath);
}
```

```csharp
// tools/CsvToYaml/Services/IYamlSerializer.cs
using Era.Core.Types;

namespace CsvToYaml.Services;

public interface IYamlSerializer
{
    Result<string> Serialize<T>(T data);
}
```

```csharp
// tools/CsvToYaml/Services/YamlSerializer.cs (implementation)
using Era.Core.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CsvToYaml.Services;

public class YamlSerializer : IYamlSerializer
{
    private readonly ISerializer _serializer;

    public YamlSerializer()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .DisableAliases()
            .Build();
    }

    public Result<string> Serialize<T>(T data)
    {
        try
        {
            var yaml = _serializer.Serialize(data);
            return Result<string>.Success(yaml);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"YAML serialization failed: {ex.Message}");
        }
    }
}
```

### Schema Validation Integration

```csharp
// tools/CsvToYaml/Services/CsvToYamlConverter.cs (ISchemaValidator usage)
using Era.Core.Types;  // For Result<T>, Unit
using Era.Core.Validation;  // ISchemaValidator from Era.Core.Validation (F539)

namespace CsvToYaml.Services;

public class CsvToYamlConverter
{
    private readonly ICsvParser _csvParser;
    private readonly IYamlSerializer _yamlSerializer;
    private readonly ISchemaValidator? _schemaValidator;  // Optional, from F539

    public CsvToYamlConverter(
        ICsvParser csvParser,
        IYamlSerializer yamlSerializer,
        ISchemaValidator? schemaValidator = null)  // Optional validation
    {
        _csvParser = csvParser;
        _yamlSerializer = yamlSerializer;
        _schemaValidator = schemaValidator;
    }

    public Result<Unit> ConvertWithValidation(string inputPath, string outputPath)
    {
        // ... conversion logic ...

        // Optional validation if validator provided and --validate flag set
        if (_schemaValidator != null)
        {
            var validationResult = _schemaValidator.ValidateFile(outputPath);
            // Result<T> uses discriminated union pattern matching (ENGINE.md Issue 2)
            if (validationResult is Result<Unit>.Failure f)
                return Result<Unit>.Fail(f.Error);
        }

        return Result<Unit>.Ok(Unit.Value);
    }
}
```

### CLI Interface Contract

```bash
# Basic usage
dotnet run --project tools/CsvToYaml -- Game/CSV/ --output Game/content/

# Help documentation
dotnet run --project tools/CsvToYaml -- --help
```

**Help Output Format**:
```
Usage: CsvToYaml <input-directory> --output <output-directory>

Arguments:
  input-directory     Path to directory containing CSV files

Options:
  --output <path>     Output directory for generated YAML files
  --validate          Enable schema validation (requires SchemaValidator)
  --help              Show this help message

Examples:
  CsvToYaml Game/CSV/ --output Game/content/
  CsvToYaml Game/CSV/ --output Game/content/ --validate
```

### Error Handling Contract

**Error Message Format**: Use `Result<T>` pattern from Era.Core for all operations.

| Error Scenario | Message Format |
|----------------|----------------|
| Input directory not found | `"Input directory '{path}' does not exist"` |
| Output directory creation failed | `"Failed to create output directory '{path}'"` |
| CSV parsing error | `"Failed to parse CSV file '{filename}': {error}"` |
| YAML generation error | `"Failed to generate YAML for '{filename}': {error}"` |
| Schema validation error | `"Schema validation failed for '{filename}': {error}"` |

### CSV File Count Verification

**Actual Count**: 43 files (verified by `Glob Game/CSV/*.csv`)

**Test Contract**: Integration test must verify all 43 existing CSV files are processed:

```csharp
[Test]
public void ProcessAllCsvFiles_ShouldHandle43Files()
{
    // Arrange
    var csvFiles = Directory.GetFiles("Game/CSV", "*.csv");
    Assert.That(csvFiles.Length, Is.EqualTo(43), "CSV file count verification");

    // Act & Assert - process all files successfully
}
```

### Architecture Integration

**Dependencies**:

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F516 | Phase 17 Planning (parent feature) |
| Predecessor | F539 | SchemaValidator with Era.Core.Validation Layer - ISchemaValidator interface in Era.Core.Validation |
| ProjectReference | Era.Core.Validation | ISchemaValidator implementation (DI registration via AddEraValidation()) |
| Package | Era.Core.Types | Result<T> discriminated union pattern |
| Package | YamlDotNet | YAML serialization |

**Note**: F539 must complete FL review and reach [REVIEWED] status before F538 implementation. F538 references Era.Core.Validation (not tools/SchemaValidator) for ISchemaValidator.

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須 -->

None identified. All CsvToYaml tool scope is covered within this feature.

---

## Review Notes
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC table Method column: Validator confirmed Method column is used consistently (testing/SKILL.md shows standard).
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#12 grep pattern: Fixed in iter2 - changed "TODO\\|FIXME\\|HACK" to "TODO|FIXME|HACK" per ripgrep standard.
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#6 grep pattern: Validator confirmed parameter names in pattern are intentional specification for consistency, pattern already uses .* for flexibility.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Relative path in test: Relative paths in test code are common pattern. Tests are run from repository root per dotnet conventions. SSOT does not prohibit.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Dependencies format: feature-template.md does not mandate table format. Prose format is acceptable when dependencies are clearly listed.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Result<T> details: ENGINE.md provides canonical reference. Feature correctly uses Era.Core.Types without duplicating pattern details.
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - AC#1 directory check: Changed from directory path 'tools/CsvToYaml/' to file path 'tools/CsvToYaml/Program.cs' - Glob reliably matches files.
- **2026-01-18 FL iter4**: [resolved] Phase2-Validate - F348 reference: Corrected in iter4 - F348 was incorrect reference. ISchemaValidator is created in F539. Updated Dependencies table with F539 as predecessor.
- **2026-01-18 FL iter4**: [resolved] Phase2-Validate - Interface snippets: Added interface code snippets with namespace per ENGINE.md Issue 21.
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - ISchemaValidator integration: Added project reference to SchemaValidator and usage snippet showing CsvToYamlConverter with optional ISchemaValidator dependency.
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - F539 dependency note: Added note to Dependencies that F539 must reach [REVIEWED] before F538 implementation.
- **2026-01-18 FL iter6**: [resolved] Phase2-Validate - YamlSerializer implementation: Added YamlDotNet serializer implementation snippet per ENGINE.md Issue 21.
- **2026-01-18 FL iter6**: [resolved] Phase2-Validate - F539 pending issues: F539 has unresolved [pending] issues, but F538 spec correctly documents this as blocked dependency. Execution order is documented; spec is complete.
- **2026-01-18 FL iter6**: [resolved] Phase2-Validate - AC#9/AC#10 Expected format: Validator confirmed filter patterns in Expected column is informative and not prohibited. Pattern consistent within feature.
- **2026-01-18 FL iter7**: [resolved] Phase2-Validate - AC#7 async Main: Updated pattern from "static void Main" to "static.*Main" to support both sync and async entry points.
- **2026-01-18 FL iter8**: [resolved] Phase3-Maintainability - Task batch waivers: Tasks 1,2,3,5,7 batch multiple ACs per ENGINE.md Issue 7 established pattern (project structure, interfaces, CLI, tests, quality gates). Waiver documentation moved from HTML comment to Review Notes for visibility.
- **2026-01-18 F539 arch**: [resolved] Updated ProjectReference from tools/SchemaValidator to Era.Core.Validation per F539 architecture redesign. F538 now references Era.Core.Validation for ISchemaValidator.
- **2026-01-18 Result<T>**: [resolved] Fixed ConvertWithValidation code snippet to use discriminated union pattern matching (Result<T>.Failure f) instead of non-existent .IsFailure/.Error properties. Used Result<Unit>.Ok/Fail per Era.Core.Types API.

---

## Links
- [index-features.md](index-features.md)
- [Feature 516: Phase 17 Planning](feature-516.md) - Parent planning feature
- [Feature 528: Critical Config Files Migration](feature-528.md) - Reference for YAML output format (Game/config/*.yaml establishes conversion rules)
- [Feature 539: SchemaValidator with Era.Core.Validation Layer](feature-539.md) - ISchemaValidator interface provider via Era.Core.Validation (predecessor)
- [Full C# Architecture](designs/full-csharp-architecture.md) - Lines 3843-3847 tool specifications