# Feature 717: Tool Test Coverage (com-validator, SaveAnalyzer, YamlValidator, kojo-mapper)

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
Test coverage should grow alongside implementation. Tools used in the development pipeline should have tests to prevent silent regressions. Inherits philosophy from F716 (Era.Core Test Coverage Hardening).

### Problem (Current Issue)
4 tools in the `tools/` directory have minimal or zero test coverage:

| Tool | Language | Purpose | Test Files | Note |
|------|----------|---------|:----------:|------|
| com-validator | **Go** | Community YAML validator with Japanese support | 1 | Has `localization_test.go` (4 tests), but no `validator_test.go` |
| SaveAnalyzer | C# | Save file analysis | 0 | |
| YamlValidator | C# | YAML schema validation CLI | 0 | |
| kojo-mapper | Python | Kojo coverage analysis (5 scripts) | 0 | |

These tools are actively used in the development workflow (kojo-mapper for coverage tracking, com-validator for YAML validation, YamlValidator for schema checks). Bugs in these tools could produce incorrect validation results without detection.

### Goal (What to Achieve)
Add basic test coverage for each of the 4 tools. Focus on core functionality and edge cases, not exhaustive coverage.

**Scope**: tools/ test projects only (Python pytest + C# xUnit). Era.Core tests are out of scope (F716).

**Volume estimate**: ~4-8 new test files, ~40-80 test methods.

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 4 tools have minimal or zero test coverage
2. Why: Tests were not created when these tools were originally developed
3. Why: These tools were built as ad-hoc utilities for immediate developer needs, not as production-quality components
4. Why: No test requirements were enforced for tools/ directory (unlike Era.Core which had F499 test strategy)
5. Why: The tools/ directory lacked a formal quality gate — tools were written quickly for specific tasks without lifecycle planning

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| 4 tools have no/minimal tests | Ad-hoc tool creation without quality gates; no test strategy applied to tools/ directory |

### Conclusion

The root cause is that tools/ directory lacked the same quality discipline applied to Era.Core. F499 (Test Strategy) focused on game logic and migration testing but did not establish requirements for developer tools. These tools were created incrementally as utility scripts and grew in complexity without retroactive test addition. F716 identified this gap and created F717/F718 as remediation features.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F716 | [DONE] | Parent initiative | Identified tool test gaps, created F717 as remediation |
| F499 | [DONE] | Related | Test strategy design — focused on game logic, not tools |
| F718 | [DONE] | Sibling | Era.Core Data Loader Test Coverage (split from F716) |
| F706 | [PROPOSED] | Consumer | KojoComparer uses kojo-mapper outputs; better kojo-mapper tests improve confidence |

### Pattern Analysis

This is not a recurring pattern — it is a one-time remediation of accumulated tool test debt. F716 systematically identified all gaps and created targeted features (F717 for tools, F718 for data loaders) to address them. The pattern of gap identification followed by targeted remediation is working correctly.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All 4 tools have testable pure functions that can be unit-tested |
| Scope is realistic | PARTIAL | kojo-mapper (69K lines) is very large; needs scoped approach |
| No blocking constraints | YES | F716 predecessor is [DONE]; all test frameworks available |

**Verdict**: FEASIBLE

**Revisions applied**: Background section corrected (com-validator is Go, not Python; has existing localization_test.go). kojo-mapper scope narrowed to verify_*.py scripts only. All revisions reflected in current Background section.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F716 | [DONE] | Era.Core test coverage hardening (parent initiative) |
| Related | F499 | [DONE] | Test strategy design |
| Related | F718 | [DONE] | Era.Core Data Loader Test Coverage (sibling) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Go 1.23+ | Build-time | Low | Required for `go test` on com-validator; go.mod specifies 1.23 |
| Python 3.13+ | Build-time | Low | Required for pytest on kojo-mapper; `__pycache__` shows cpython-313 |
| pytest | Test-time | Low | Standard Python test framework; not currently in requirements |
| xUnit v3 + Moq | Test-time | Low | Already used in KojoComparer.Tests; pattern available |
| NJsonSchema + YamlDotNet | Runtime | Low | YamlValidator dependencies; needed in test project too |
| gojsonschema | Runtime | Low | com-validator Go dependency; already in go.mod |
| ERA save files (Shift-JIS) | Test data | Medium | SaveAnalyzer reads binary save files; need sample test data |
| ERB file system | Test data | High | kojo-mapper parses real ERB files; needs mocking or fixtures |
| com.schema.json | Test data | Low | Embedded in com-validator binary via go:embed |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Development workflow | MEDIUM | kojo-mapper used for coverage tracking, com-validator for YAML validation |
| CI pipeline | LOW | YamlValidator used in schema validation, not currently in CI |
| F706 KojoComparer | MEDIUM | Uses kojo-mapper outputs for equivalence testing |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/com-validator/validator_test.go | Create | Tests for ValidateFile, mapErrorType, buildErrorDetails, findFieldLocation |
| tools/SaveAnalyzer.Tests/SaveAnalyzer.Tests.csproj | Create | New xUnit test project for SaveAnalyzer |
| tools/SaveAnalyzer.Tests/SaveReaderTests.cs | Create | Tests for SaveReader parsing (header, arrays, characters) |
| tools/SaveAnalyzer.Tests/ProgramTests.cs | Create | Tests for argument parsing, FilterGlobals, FilterCharacters |
| tools/SaveAnalyzer.Tests/TestData/ | Create | Sample save file fixtures (minimal Shift-JIS) |
| tools/YamlValidator.Tests/YamlValidator.Tests.csproj | Create | New xUnit test project for YamlValidator |
| tools/YamlValidator.Tests/ProgramTests.cs | Create | Tests for ParseArguments, ValidateFile with test YAML+schema |
| tools/kojo-mapper/tests/ | Create | Python test directory |
| tools/kojo-mapper/tests/test_verify_range_coverage.py | Create | Tests for verify_range_coverage (pure function, easy to test) |
| tools/kojo-mapper/tests/test_verify_json_references.py | Create | Tests for verify_json_references |
| tools/kojo-mapper/tests/test_verify_com_map.py | Create | Tests for verify_com_map helpers |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| SaveAnalyzer reads Shift-JIS binary files | SaveReader.cs line 67 | MEDIUM - Test data must be actual Shift-JIS encoded files; cannot use plain text |
| SaveAnalyzer/YamlValidator have no library separation | Monolithic Program.cs | MEDIUM - Static methods are testable but tightly coupled to file I/O |
| com-validator uses go:embed for schema | validator.go line 14 | LOW - Tests run in same package, can access embedded schema |
| kojo_mapper.py is 69K lines with deep ERB filesystem dependency | kojo_mapper.py | HIGH - Cannot unit test core functions without extensive mocking or real ERB fixtures |
| kojo_test_gen.py imports from kojo_mapper at module level | kojo_test_gen.py line 14 | MEDIUM - Any test importing kojo_test_gen triggers kojo_mapper parse + JSON load |
| TreatWarningsAsErrors in Directory.Build.props | F708 | LOW - New C# test projects must compile warning-free |
| SaveAnalyzer and YamlValidator have internal Program classes with private methods | Tool architecture | MEDIUM - Test projects need InternalsVisibleTo attribute to access testable methods |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| SaveAnalyzer test data creation complexity | Medium | Medium | Create minimal synthetic save files with known values rather than using real game saves |
| kojo-mapper ERB filesystem coupling makes unit testing hard | High | Medium | Focus on verification scripts (verify_*.py) which have pure functions; defer kojo_mapper.py core testing |
| com-validator validator_test.go needs test YAML files | Low | Low | Create minimal YAML fixtures in test directory or use Go test temp files |
| YamlValidator tests require JSON schema files | Low | Low | Create minimal test schema or reference existing com.schema.json |
| Python test infrastructure not established for kojo-mapper | Medium | Low | Add pytest as dev dependency; follow tools/tests/ pattern used by ac-static-verifier |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Test coverage should grow alongside implementation" | Each tool must have new test files | AC#1, AC#3, AC#5, AC#7, AC#9 |
| "Tools used in the development pipeline should have tests" | Tests must pass for all 4 tools | AC#2, AC#4, AC#6, AC#8, AC#10 |
| "prevent silent regressions" | Tests verify core functionality, not just existence | AC#2, AC#6, AC#8, AC#10 |
| "Focus on core functionality and edge cases" | Tests target testable functions and public APIs, not monolithic scripts | AC#2, AC#4, AC#6, AC#8 (test-pass ACs verify core functionality is exercised) |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | validator_test.go exists in com-validator | file | Glob(tools/com-validator/validator_test.go) | exists | - | [x] |
| 2 | com-validator go test passes | exit_code | go test ./... (in tools/com-validator/) | succeeds | - | [x] |
| 3 | SaveAnalyzer.Tests project exists | file | Glob(tools/SaveAnalyzer.Tests/SaveAnalyzer.Tests.csproj) | exists | - | [x] |
| 4 | SaveAnalyzer.Tests dotnet test passes | test | dotnet test tools/SaveAnalyzer.Tests/ | succeeds | - | [x] |
| 5 | YamlValidator.Tests project exists | file | Glob(tools/YamlValidator.Tests/YamlValidator.Tests.csproj) | exists | - | [x] |
| 6 | YamlValidator.Tests dotnet test passes | test | dotnet test tools/YamlValidator.Tests/ | succeeds | - | [x] |
| 7 | kojo-mapper test files exist | file | Glob(tools/kojo-mapper/tests/test_verify_*.py) | count_equals | 3 | [x] |
| 8 | kojo-mapper pytest passes | exit_code | pytest tools/kojo-mapper/tests/ | succeeds | - | [x] |
| 9 | kojo-mapper tests cover verify scripts only | code | Grep(tools/kojo-mapper/tests/) | not_matches | "import kojo_mapper|from kojo_mapper" | [x] |
| 10 | dotnet build succeeds with no warnings | build | dotnet build | succeeds | - | [x] |
| 11 | SaveAnalyzer.Tests references SaveAnalyzer project | code | Grep(tools/SaveAnalyzer.Tests/SaveAnalyzer.Tests.csproj) | contains | "SaveAnalyzer.csproj" | [x] |
| 12 | YamlValidator.Tests references YamlValidator project | code | Grep(tools/YamlValidator.Tests/YamlValidator.Tests.csproj) | contains | "YamlValidator.csproj" | [x] |

**Note**: 12 ACs is within the infra type range (8-15).

### AC Details

**AC#1: validator_test.go exists in com-validator**
- com-validator already has `localization_test.go` (4 tests) but no tests for `validator.go`
- Verifies: New test file created for core validation logic (ValidateFile, mapErrorType, buildErrorDetails, findFieldLocation)
- Path: `tools/com-validator/validator_test.go`

**AC#2: com-validator go test passes**
- Runs `go test ./...` in `tools/com-validator/` directory
- Verifies: Both existing localization tests and new validator tests pass
- Tests should cover: Validator instantiation (NewValidator), mapErrorType mapping, buildErrorDetails formatting, findFieldLocation heuristic, ValidateFile with valid/invalid YAML

**AC#3: SaveAnalyzer.Tests project exists**
- New xUnit test project following KojoComparer.Tests pattern
- Must reference SaveAnalyzer project and include xunit packages (no Moq needed)
- Path: `tools/SaveAnalyzer.Tests/SaveAnalyzer.Tests.csproj`

**AC#4: SaveAnalyzer.Tests dotnet test passes**
- Runs `dotnet test tools/SaveAnalyzer.Tests/`
- Testable functions: argument parsing (Main args), FilterGlobals, FilterCharacters in Program.cs; SaveReader header/array/character parsing
- Note: SaveReader reads Shift-JIS binary files; tests need minimal synthetic save file fixtures in TestData/

**AC#5: YamlValidator.Tests project exists**
- New xUnit test project following KojoComparer.Tests pattern
- Must reference YamlValidator project and include xunit packages (no Moq needed)
- Path: `tools/YamlValidator.Tests/YamlValidator.Tests.csproj`

**AC#6: YamlValidator.Tests dotnet test passes**
- Runs `dotnet test tools/YamlValidator.Tests/`
- Testable functions: ParseArguments (pure function, easy to test), ValidateFile with test YAML+schema fixtures
- ValidatorOptions class properties are straightforward to verify

**AC#7: kojo-mapper test files exist**
- Expects exactly 3 test files: `test_verify_range_coverage.py`, `test_verify_json_references.py`, `test_verify_com_map.py`
- Located in `tools/kojo-mapper/tests/` directory
- Follows pytest naming convention (test_*.py)

**AC#8: kojo-mapper pytest passes**
- Runs `pytest tools/kojo-mapper/tests/`
- Tests target the 3 verify_*.py scripts:
  - `verify_range_coverage.py`: `verify_range_coverage(json_path)` - gap/overlap detection (pure function)
  - `verify_json_references.py`: `verify_json_references()` - file reference checking (has filesystem coupling, requires test fixtures)
  - `verify_com_map.py`: `load_com_file_map(data)`, `load_skip_combinations(data)` - JSON parsing helpers (pure functions)

**AC#9: kojo-mapper tests cover verify scripts only**
- Ensures no test file imports `kojo_mapper` (the 69K-line module)
- `kojo_mapper.py` has deep ERB filesystem coupling that makes unit testing impractical without extensive mocking
- Scope is explicitly limited to verify_*.py scripts per Feasibility Assessment
- Pattern catches both `import kojo_mapper` and `from kojo_mapper import X` statements using regex `import kojo_mapper|from kojo_mapper`

**AC#10: dotnet build succeeds with no warnings**
- Runs `dotnet build` for the full solution
- Per F708, `TreatWarningsAsErrors=true` in Directory.Build.props
- New test projects must compile warning-free

**AC#11: SaveAnalyzer.Tests references SaveAnalyzer project**
- Verifies the test project has a proper `<ProjectReference>` to SaveAnalyzer
- Pattern: `<ProjectReference Include="..SaveAnalyzer.csproj" />` or similar
- Ensures tests can access SaveAnalyzer internals (or public API)

**AC#12: YamlValidator.Tests references YamlValidator project**
- Verifies the test project has a proper `<ProjectReference>` to YamlValidator
- Pattern: `<ProjectReference Include="..YamlValidator.csproj" />` or similar
- Ensures tests can access YamlValidator types (ValidatorOptions, ParseArguments, etc.)

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature adds basic test coverage for 4 tools using the frameworks and patterns already established in the repository. The design focuses on testing public APIs and testable functions, with targeted mocking for filesystem-coupled components.

**Test Framework Selection:**
- **com-validator (Go)**: Use Go's built-in `testing` package, following the existing `localization_test.go` pattern with table-driven tests
- **SaveAnalyzer (C#)**: Create new xUnit test project following `KojoComparer.Tests` pattern with Moq for dependency injection
- **YamlValidator (C#)**: Create new xUnit test project following `KojoComparer.Tests` pattern (minimal mocking needed)
- **kojo-mapper (Python)**: Use pytest following `tools/tests/` pattern (ac-static-verifier tests)

**Test Data Strategy:**
- **com-validator**: Create minimal YAML fixtures with deliberate errors (type mismatch, missing required fields) in test code using Go temp files
- **SaveAnalyzer**: Create synthetic minimal save files with known Shift-JIS encoded values in `TestData/` directory (1-2 files)
- **YamlValidator**: Create minimal JSON schema + YAML fixtures in test project resources
- **kojo-mapper**: Mixed approach - verify_range_coverage uses inline JSON data, verify_json_references requires filesystem fixtures (mock Path/__file__ or temp directory with test files), verify_com_map helpers use inline JSON data

**Scope Constraints:**
- kojo-mapper tests target ONLY the 3 verify_*.py scripts (pure functions, no ERB filesystem dependency)
- Do NOT test `kojo_mapper.py` (69K lines with deep ERB coupling) or `kojo_test_gen.py` (imports kojo_mapper at module level)
- SaveAnalyzer and YamlValidator test public methods in Program.cs (argument parsing, main logic) and library classes (SaveReader, ValidatorOptions)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `tools/com-validator/validator_test.go` with table-driven tests for ValidateFile (valid/invalid YAML), mapErrorType (all enum cases), buildErrorDetails (formatting), and findFieldLocation (line number extraction) |
| 2 | All tests in validator_test.go and existing localization_test.go pass when running `go test ./...` in com-validator directory |
| 3 | Create `tools/SaveAnalyzer.Tests/SaveAnalyzer.Tests.csproj` with xUnit references and ProjectReference to SaveAnalyzer.csproj; modify SaveAnalyzer Program methods to internal static |
| 4 | Create ProgramTests.cs (argument parsing, FilterGlobals, FilterCharacters) and SaveReaderTests.cs (header parsing, array reading with synthetic Shift-JIS test data in TestData/) |
| 5 | Create `tools/YamlValidator.Tests/YamlValidator.Tests.csproj` with xUnit references and ProjectReference to YamlValidator.csproj; modify YamlValidator Program methods to internal static |
| 6 | Create ProgramTests.cs testing ParseArguments (pure function with various arg combinations) and ValidateFile (with test schema + YAML fixtures in Resources/) |
| 7 | Create 3 test files: `test_verify_range_coverage.py` (gap/overlap detection), `test_verify_json_references.py` (file reference checking with mocked filesystem), `test_verify_com_map.py` (JSON parsing helpers) |
| 8 | All pytest tests pass when running `pytest tools/kojo-mapper/tests/` (verify_range_coverage and verify_com_map use inline JSON data, verify_json_references requires filesystem fixtures via mocking or temp directory) |
| 9 | Grep test files for "import kojo_mapper" — should find zero matches (tests import verify_* modules directly) |
| 10 | dotnet build succeeds with TreatWarningsAsErrors=true enforcement from Directory.Build.props |
| 11 | SaveAnalyzer.Tests.csproj contains `<ProjectReference Include="..\SaveAnalyzer\SaveAnalyzer.csproj" />` |
| 12 | YamlValidator.Tests.csproj contains `<ProjectReference Include="..\YamlValidator\YamlValidator.csproj" />` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Go test structure** | Individual test functions vs table-driven tests | Table-driven tests | Matches existing localization_test.go pattern; more concise for testing mapErrorType enum coverage (8+ error types) |
| **SaveAnalyzer test data** | Real save files vs synthetic files vs embedded resources | Synthetic files in TestData/ | Real save files are large (10KB+) and contain game state; synthetic files allow precise control of test values; embedded resources add complexity without benefit |
| **Shift-JIS test data creation** | Generate at runtime vs pre-create files | Pre-create files | Runtime generation requires correct Shift-JIS encoding logic which duplicates SaveReader; pre-created files are verifiable and stable |
| **SaveAnalyzer testable surface** | Mock StreamReader vs test SaveReader directly vs extract business logic | Test SaveReader + FilterGlobals/FilterCharacters directly | SaveReader constructor requires actual file path; FilterGlobals/FilterCharacters are pure functions; no need to mock I/O for business logic tests |
| **YamlValidator test schema** | Reference existing com.schema.json vs create minimal test schema | Create minimal test schema | com.schema.json is large (400+ lines) and specific to COM validation; minimal schema (required field + type constraint) is sufficient for testing ValidateFile logic |
| **kojo-mapper scope** | Test all 5 scripts vs verify_*.py only | verify_*.py only (3 scripts) | kojo_mapper.py and kojo_test_gen.py have deep ERB/JSON dependencies that require extensive mocking; verify scripts have pure, testable functions |
| **kojo-mapper test fixtures** | Real com_file_map.json vs inline test data | Inline test data | Real JSON is 200+ lines and changes frequently; inline data allows testing gap/overlap detection with minimal, controlled ranges |
| **Python test framework** | unittest vs pytest | pytest | Already used in tools/tests/ for ac-static-verifier; pytest is more concise and has better assertion introspection |
| **C# test project location** | tools/Tests/ (shared) vs tool-specific directories | Tool-specific (SaveAnalyzer.Tests/, YamlValidator.Tests/) | Matches KojoComparer.Tests pattern; keeps test project adjacent to source; allows independent versioning |
| **com-validator test YAML** | Files in testdata/ vs inline strings with temp files | Inline strings with temp files | Go testing package supports temp files natively; inline YAML is more readable in test code; no need for testdata/ directory management |

### Interfaces / Data Structures

**No new public interfaces required.** Tests use existing public APIs:

**Go (com-validator):**
```go
// Test targets (existing functions):
func NewValidator() (*Validator, error)
func (v *Validator) ValidateFile(filePath string) error
func mapErrorType(errType string) string
func buildErrorDetails(err gojsonschema.ResultError) string
func findFieldLocation(field string, yamlNode *yaml.Node, yamlContent string) (int, int)
```

**C# SaveAnalyzer:**
```csharp
// Test targets:
public class SaveReader : IDisposable
{
    public SaveReader(string filePath)
    public SaveData Read()
}

// Program.cs (internal class - methods must be changed to internal for testing):
internal class Program
{
    internal static Dictionary<string, Dictionary<int, long>>? FilterGlobals(
        Dictionary<string, Dictionary<int, long>> globals, string? filter)
    internal static List<object>? FilterCharacters(
        List<CharacterData> characters, string? filterCharacter, string? filterVariable)
}
```

**C# YamlValidator:**
```csharp
// Test targets (internal class - methods must be changed to internal for testing):
internal class Program
{
    internal static ValidatorOptions ParseArguments(string[] args)
    internal static async Task<int> ValidateFile(JsonSchema schema, string yamlPath)
}

// ValidatorOptions is internal class
internal class ValidatorOptions
{
    public string? SchemaPath { get; set; }
    public string? YamlPath { get; set; }
    public string? ValidateAllPath { get; set; }
    public bool ShowHelp { get; set; }
}
```

**Python kojo-mapper:**
```python
# Test targets (existing functions in verify_*.py):
# verify_range_coverage.py
def verify_range_coverage(json_path: Path) -> tuple[bool, list[str]]

# verify_json_references.py
def verify_json_references() -> int

# verify_com_map.py
def load_com_file_map(data: dict) -> dict[int, str]
def load_skip_combinations(data: dict) -> set[tuple[str, str]]
def get_exclusively_unimplemented_files(data: dict) -> set[str]
```

**SaveAnalyzer TestData Structure:**
```
tools/SaveAnalyzer.Tests/
├── SaveAnalyzer.Tests.csproj
├── SaveReaderTests.cs
├── ProgramTests.cs
└── TestData/
    └── minimal_save.sav    # Synthetic Shift-JIS file with known values
```

**YamlValidator Test Resources:**
```
tools/YamlValidator.Tests/
├── YamlValidator.Tests.csproj
├── ProgramTests.cs
└── TestData/
    ├── test_schema.json    # Minimal JSON schema with required field
    ├── valid.yaml          # Valid YAML matching schema
    └── invalid.yaml        # Invalid YAML (missing required field)
```

---

<!-- fc-phase-5-completed -->
## Tasks

| T# | AC# | Description | Status |
|:---:|:---:|-------------|:------:|
| 1 | 1 | Create com-validator validator_test.go with table-driven tests | [x] |
| 2 | 2 | Verify com-validator go test passes | [x] |
| 3 | 3 | Create SaveAnalyzer.Tests xUnit project with references and modify SaveAnalyzer Program methods to internal | [x] |
| 4 | 4 | Create SaveAnalyzer test files and verify dotnet test passes | [x] |
| 5 | 5 | Create YamlValidator.Tests xUnit project with references and modify YamlValidator Program methods to internal | [x] |
| 6 | 6 | Create YamlValidator test files and verify dotnet test passes | [x] |
| 7 | 7 | Create kojo-mapper pytest test files (3 test_verify_*.py) | [x] |
| 8 | 8 | Verify kojo-mapper pytest passes | [x] |
| 9 | 9 | Verify kojo-mapper tests scope constraint (no kojo_mapper import) | [x] |
| 10 | 10 | Verify dotnet build succeeds with no warnings | [x] |
| 11 | 11 | Verify SaveAnalyzer.Tests references SaveAnalyzer project | [x] |
| 12 | 12 | Verify YamlValidator.Tests references YamlValidator project | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | AC Details #1-2 | validator_test.go with table-driven tests for ValidateFile, mapErrorType, buildErrorDetails, findFieldLocation |
| 2 | implementer | sonnet | T3, T11 | AC Details #3, #11 | SaveAnalyzer.Tests.csproj with xUnit packages and ProjectReference; modify SaveAnalyzer Program methods to internal |
| 3 | implementer | sonnet | T4 | AC Details #4 | SaveReaderTests.cs, ProgramTests.cs, TestData/minimal_save.sav (synthetic Shift-JIS file) |
| 4 | implementer | sonnet | T5, T12 | AC Details #5, #12 | YamlValidator.Tests.csproj with xUnit packages and ProjectReference; modify YamlValidator Program methods to internal |
| 5 | implementer | sonnet | T6 | AC Details #6 | ProgramTests.cs, TestData/ (test schema + YAML fixtures) |
| 6 | implementer | sonnet | T7 | AC Details #7 | 3 test files: test_verify_range_coverage.py, test_verify_json_references.py, test_verify_com_map.py with pytest tests using inline JSON data |
| 7 | ac-tester | haiku | T2 | AC#2 | go test ./... passes in com-validator directory |
| 8 | ac-tester | haiku | T8 | AC#8 | pytest tools/kojo-mapper/tests/ passes |
| 9 | ac-tester | haiku | T9 | AC#9 | Grep verification: no "import kojo_mapper" or "from kojo_mapper" patterns in test files |
| 10 | ac-tester | haiku | T10 | AC#10 | dotnet build succeeds with TreatWarningsAsErrors=true |

**Constraints** (from Technical Design):
1. **SaveAnalyzer test data**: Create synthetic minimal save files with known Shift-JIS encoded values (not real game saves)
2. **kojo-mapper scope**: Test ONLY verify_*.py scripts (3 files); do NOT test kojo_mapper.py (69K lines) or kojo_test_gen.py
3. **Go test structure**: Use table-driven tests matching existing localization_test.go pattern
4. **C# test projects**: Follow KojoComparer.Tests pattern (xUnit, tool-specific directories) - no Moq needed for pure functions
5. **Python test framework**: Use pytest following tools/tests/ pattern (ac-static-verifier)
6. **YamlValidator test schema**: Create minimal test schema (not com.schema.json)
7. **com-validator test YAML**: Use inline strings with Go temp files (not testdata/ directory)
8. **TreatWarningsAsErrors**: All new test projects must compile warning-free per F708 policy
9. **Method visibility**: Change SaveAnalyzer and YamlValidator Program class methods from private static to internal static to enable testing; add InternalsVisibleTo to target projects

**Pre-conditions**:
- F716 (Era.Core Test Coverage Hardening) is [DONE]
- Go 1.23+ installed and available in PATH
- Python 3.13+ installed with pytest available
- .NET SDK installed (dotnet test available)
- All 4 tools compile successfully before adding tests

**Success Criteria**:
- All 12 ACs pass verification
- `go test ./...` passes in tools/com-validator/
- `dotnet test` passes for SaveAnalyzer.Tests and YamlValidator.Tests
- `pytest tools/kojo-mapper/tests/` passes with 3 test files
- `dotnet build` succeeds with no warnings (TreatWarningsAsErrors enforcement)
- SaveAnalyzer.Tests and YamlValidator.Tests have proper ProjectReference to their respective tool projects
- kojo-mapper tests do NOT import kojo_mapper.py (scope constraint verified)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. For test failures: Investigate whether tests are incorrect or tools have latent bugs
5. For build failures: Check TreatWarningsAsErrors compatibility with test projects

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| ac-static-verifier does not support count_equals matcher | F722 | DRAFT created. /fc 722 → /fl → /run needed |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-01 12:31 | 1 | T1 completed: Created validator_test.go with table-driven tests for mapErrorType, buildErrorDetails, findFieldLocation, NewValidator, ValidateFile. All tests pass. |
| 2026-02-01 12:34 | 3 | T4 completed: Created SaveReaderTests.cs with 9 tests and extended ProgramTests.cs with 11 additional tests (20 total). Tests cover header parsing, character data, sparse arrays, global arrays, filtering functions with edge cases. All tests pass. |
| 2026-02-01 12:35 | 5 | T6 completed: Created comprehensive ProgramTests.cs with 13 tests (10 ParseArguments tests, 3 ValidateFile tests). Created test data (test_schema.json, valid.yaml, invalid.yaml). All tests pass. Fixed xUnit v3 analyzer warnings by using TestContext.Current.CancellationToken. |
| 2026-02-01 12:48 | 6 | T7 completed: Created 3 kojo-mapper pytest test files (test_verify_range_coverage.py, test_verify_json_references.py, test_verify_com_map.py) with 20 tests total. All tests pass. Tests use inline JSON data and conftest.py for path setup. AC#9 compliant (no kojo_mapper imports). |
| 2026-02-01 13:05 | 9 | AC Verification completed: All 12 ACs PASS. AC#1: validator_test.go exists. AC#2: go test passes (1 package OK). AC#3: SaveAnalyzer.Tests project exists. AC#4: dotnet test passes (20/20 tests). AC#5: YamlValidator.Tests project exists. AC#6: dotnet test passes (13/13 tests). AC#7: 3 test_verify_*.py files found. AC#8: pytest passes (20/20 tests). AC#9: No kojo_mapper imports detected. AC#10: dotnet build succeeds (0 warnings). AC#11: SaveAnalyzer.Tests.csproj contains ProjectReference to SaveAnalyzer.csproj. AC#12: YamlValidator.Tests.csproj contains ProjectReference to YamlValidator.csproj. |
| 2026-02-01 13:10 | DEVIATION | ac-static-verifier | AC#7 count_equals | ac-static-verifier exit code 1: "Unknown matcher: count_equals". Known limitation (documented in testing skill). Manual verification confirmed 3 files exist (PASS). |
| 2026-02-01 13:15 | DEVIATION | verify-logs.py | ERR:1/7 | verify-logs shows ERR because ac-static-verifier recorded AC#7 as FAIL (count_equals unsupported). All 12 ACs manually verified PASS by ac-tester. |

---

## Review Notes
- [resolved-applied] Phase1-Uncertain iter1: Claim 'Focus on core functionality and edge cases' maps to AC#7 and AC#9, but AC#7 only checks file existence (count_equals 3 files) and AC#9 checks scope constraint. Neither verifies that tests actually cover 'core functionality and edge cases'. Consider mapping to AC#2, AC#4, AC#6, AC#8 instead (the test-pass ACs), which verify that tests exercise core functionality.
- [resolved-applied] Phase1-Uncertain iter2: AC#9 not_contains 'kojo_mapper' would also match if test files contain string 'kojo_mapper' in comments or docstrings explaining scope exclusion. Implementer may add comments like '# We don't test kojo_mapper.py because...' which would cause false AC failure. Consider narrowing to 'import kojo_mapper' or 'from kojo_mapper' to specifically catch import statements, or document that test files must not reference kojo_mapper even in comments.
- [resolved-invalid] Phase2-Maintainability iter5: AC#9 uses Type='code' with Matcher='not_matches' and Expected='import kojo_mapper|from kojo_mapper'. The not_matches matcher checks that content does NOT match the regex. However, if the test directory is empty or test files have no Python imports at all, this AC trivially passes without verifying anything meaningful. It does not confirm that tests actually DO import the verify_* modules. Consider adding a companion AC verifying that test files DO import the correct modules.
- [resolved-skipped] Phase2-Maintainability iter5: Claim 'Focus on core functionality and edge cases' maps to AC#2, AC#4, AC#6, AC#8 (test-pass ACs). While test-pass ACs confirm tests run, they don't verify that tests actually cover edge cases. A test file with a single trivial test would pass AC#2/4/6/8. No AC verifies minimum test count or edge case coverage. Consider adding minimum test count ACs per tool if desired. Skipped as 'basic test coverage' goal doesn't require minimum count enforcement.
- [resolved-invalid] Phase3-ACValidation iter7: AC#9 Expected value 'import kojo_mapper|from kojo_mapper' uses regex OR syntax which is correct for not_matches. However, ac-static-verifier has known limitation and may not handle regex OR '|' correctly. Consider splitting into two ACs: AC#9a not_matches 'import kojo_mapper' and AC#9b not_matches 'from kojo_mapper', or verify ac-static-verifier regex support before /run. Verified: ac-static-verifier.py uses Python re.search() which fully supports regex OR '|' syntax.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| ac-static-verifier count_equals unsupported | Tool limitation causes FAIL for valid AC#7 | New Feature | F722 | F722 [DRAFT] created |

---

## Links
- [feature-716.md](feature-716.md) - Parent initiative
- [feature-499.md](feature-499.md) - Test strategy design
- [feature-718.md](feature-718.md) - Era.Core Data Loader Test Coverage (sibling)
- [feature-706.md](feature-706.md) - KojoComparer (consumer)
- [CLAUDE.md Test Coverage Policy](../../CLAUDE.md#test-coverage-policy) - **Link fixed**: Section exists but is "## Test Coverage Policy", not #test-coverage-policy anchor
