# Feature 536: String Tables Migration - Phase 17

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
Phase 17: Data Migration - Establish YAML as the single source of truth for string table CSV files (Str, CSTR, TSTR, TCVAR), ensuring consistent string handling, simplified maintenance, and zero technical debt legacy migration.

### Problem (Current Issue)
String table CSVs (Str.csv, CSTR.csv, TSTR.csv, TCVAR.csv) contain critical string definitions but remain in legacy CSV format, blocking the completion of Phase 17 data migration and creating inconsistency with the YAML migration pattern established in F528-F535.

### Goal (What to Achieve)
Migrate 4 string table CSV files to YAML format with equivalence verification and zero technical debt, completing Phase 17 string data migration dependencies.

---

## Summary

**Dependencies**: Requires F528 (Critical Config Files Migration) for migration pattern precedent.

**Total Volume**: 4 CSV files (Str.csv, CSTR.csv, TSTR.csv, TCVAR.csv), estimated ~300 lines of migration code.

**Input**:
- `Game/CSV/Str.csv` - Core string definitions
- `Game/CSV/CSTR.csv` - Character string references
- `Game/CSV/TSTR.csv` - Training string references
- `Game/CSV/TCVAR.csv` - Training character variable strings

**Output**:
- YAML files in `Era.Core/Data/Strings/` (Str.yaml, CSTR.yaml, TSTR.yaml, TCVAR.yaml)
- Equivalence tests in `Era.Core.Tests/Data/StringTableEquivalenceTests.cs`

---

## Dependencies

| Dependency | Type | Status | Notes |
|------------|------|--------|-------|
| F528 | Feature | [DONE] | Critical Config Files Migration - provides pattern precedent |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Str.csv YAML conversion | file | Glob | exists | "Era.Core/Data/Strings/Str.yaml" | [ ] |
| 2 | CSTR.csv YAML conversion | file | Glob | exists | "Era.Core/Data/Strings/CSTR.yaml" | [ ] |
| 3 | TSTR.csv YAML conversion | file | Glob | exists | "Era.Core/Data/Strings/TSTR.yaml" | [ ] |
| 4 | TCVAR.csv YAML conversion | file | Glob | exists | "Era.Core/Data/Strings/TCVAR.yaml" | [ ] |
| 5 | Str.csv equivalence test | test | Bash | succeeds | - | [ ] |
| 6 | CSTR.csv equivalence test | test | Bash | succeeds | - | [ ] |
| 7 | TSTR.csv equivalence test | test | Bash | succeeds | - | [ ] |
| 8 | TCVAR.csv equivalence test | test | Bash | succeeds | - | [ ] |
| 9 | Era.Core build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [ ] |
| 10 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [ ] |

### AC Details

**AC#1-4**: File creation verification
- Test: Glob pattern verifies YAML files exist in Era.Core/Data/Strings/
- Expected: All 4 string table YAML files present

**AC#5-8**: Equivalence verification
- Test: Bash command runs specific test methods for each CSV→YAML equivalence
  - AC#5: `dotnet test --filter FullyQualifiedName~StringTableEquivalenceTests.TestStrCsv`
  - AC#6: `dotnet test --filter FullyQualifiedName~StringTableEquivalenceTests.TestCstrCsv`
  - AC#7: `dotnet test --filter FullyQualifiedName~StringTableEquivalenceTests.TestTstrCsv`
  - AC#8: `dotnet test --filter FullyQualifiedName~StringTableEquivalenceTests.TestTcvarCsv`
- Expected: All tests pass, confirming data integrity preservation
- **Minimum**: 3 Assert statements per test method

**AC#9**: Build verification
- Test: Bash command runs Era.Core build
- Expected: Build succeeds without errors

**AC#10**: Technical debt elimination
- Test: Grep pattern="TODO|FIXME|HACK" across feature files
- Path: Era.Core/Data/Strings/
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Convert string table CSVs to YAML format | [ ] |
| 2 | 5,6,7,8 | Implement equivalence verification tests | [ ] |
| 3 | 9 | Update build configuration and validate | [ ] |
| 4 | 10 | Eliminate technical debt and verify clean state | [ ] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### File Structure

| File | Purpose |
|------|---------|
| `Era.Core/Data/Strings/Str.yaml` | Core string definitions (from Str.csv) |
| `Era.Core/Data/Strings/CSTR.yaml` | Character string references (from CSTR.csv) |
| `Era.Core/Data/Strings/TSTR.yaml` | Training string references (from TSTR.csv) |
| `Era.Core/Data/Strings/TCVAR.yaml` | Training character variable strings (from TCVAR.csv) |
| `Era.Core.Tests/Data/StringTableEquivalenceTests.cs` | Equivalence verification tests |

**Path Rationale**: Era.Core/Data/Strings/ is used instead of Game/config/ because string tables are runtime data consumed by Era.Core loaders, not game-specific configuration. This follows the established Era.Core library boundary: data files that require C# processing belong in Era.Core, while game-specific data (ERB scripts, CSV character data) remain in Game/. F528's Game/config/ path is appropriate for config files that may be edited by game developers, while string tables are infrastructure data.

### Migration Pattern

Follow established F528 pattern for CSV→YAML conversion:
1. Manual YAML creation (F538 CsvToYaml tool is [BLOCKED] - not available as dependency)
2. Validate YAML structure against schema (manual schema creation - YamlSchemaGen only supports dialogue schemas)
3. Implement equivalence tests with minimum 3 assertions each
4. Verify build integration

### F528 Pattern Reference

Reference F528 Implementation Contract sections for detailed pattern guidance:
- **Interface Design** (F528 lines 235-266): IDataLoader<T> pattern with Result<T> return type, XML documentation requirements
- **Model Design** (F528 lines 268-294): Strongly typed config models with init properties, namespace conventions
- **DI Registration** (F528 lines 339-344): AddSingleton<IInterface, Implementation> pattern in ServiceCollectionExtensions.cs
- **YAML Structure Examples** (F528 lines 352-387): Inline flow style for arrays, English key mappings, -1 semantic handling
- **Test Naming Convention** (F528 lines 395-403): Test{ConfigType}{TestType} format for filter matching
- **Error Message Format** (F528 line 349): "{LoaderName}: {SpecificError}" pattern

Apply same pattern to string table files with appropriate naming (StringTable instead of CriticalConfig).

**Note**: F558 (Engine Integration Services) referenced above is currently [BLOCKED], not [DONE]. Use F528 pattern only; F558 integration pattern will be available when F558 is unblocked.

### Test Naming Convention

Test methods follow `Test{CsvName}Csv` format (e.g., `TestStrCsv`, `TestCstrCsv`, `TestTstrCsv`, `TestTcvarCsv`). This ensures AC filter patterns match correctly.

### Error Message Format

All validation failures use format: `"String table {CsvName} equivalence verification failed: {specific_error}"` (e.g., `"String table Str equivalence verification failed: Entry count mismatch"`).

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須 -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine integration for string tables | Era.Core loaders need integration with engine/game code that currently reads string tables from CSV. Similar to F528→F558 pattern, identify string table access points in engine and create integration service. | Feature | feature-560.md |

---

## Links

- [index-features.md](index-features.md)
- [feature-528.md](feature-528.md) - Dependency: Critical Config Files Migration
- [feature-516.md](feature-516.md) - Phase 17 Planning
- [feature-535.md](feature-535.md) - F528-F535 Pattern Reference
- [feature-538.md](feature-538.md) - Reference: CsvToYaml Converter Tool (blocked, manual conversion used)
- [feature-558.md](feature-558.md) - Pattern Reference: Engine Integration Services
- [feature-560.md](feature-560.md) - Engine Integration for String Tables