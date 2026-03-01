# Feature 680: xUnit v3 WDAC Compatibility Fix

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: infra

## Created: 2026-01-30

---

## Background

### Philosophy (Mid-term Vision)

Ensure CI reliability - guarantee that pre-commit hook tests are actually executed.

### Problem (Current Issue)

Investigate, evaluate, and fix the issue where xUnit v3's Out-of-Process test execution is blocked by Windows WDAC (Windows Defender Application Control), preventing Era.Core.Tests from running.

xUnit v3 (3.2.1) launches `Era.Core.Tests.exe` as a separate process (Out-of-Process mode) during test execution. However, Windows WDAC policy (Enforcement status=2) blocks execution of this .exe.

Result:
- `dotnet test` waits ~50 seconds then outputs `Catastrophic failure: This file was blocked by application control policy`
- **0 tests executed but returns exit code 0** → pre-commit hook passes but no tests run
- CI gate is effectively disabled

### Goal (What to Achieve)

1. Investigate xUnit v3 feature usage and determine if v3 is necessary
2. Report investigation results to user and reach consensus on approach (v2 rollback / WDAC exception / other)
3. Implement the agreed approach and verify `dotnet test` actually executes tests
4. Confirm pre-commit hook operates normally

---

## Links

- [feature-646.md](feature-646.md) - 発見元（Post-Phase Review Phase 19 の pre-commit hook 実行時）
- [feature-679.md](feature-679.md) - Related infra (Phase 19 Tool Test Fixes)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (tests run through same xUnit infrastructure)
- [feature-696.md](feature-696.md) - Future xUnit v3 re-migration tracking (created by Task 7)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Pre-commit hook passes without actually running tests (0 tests executed, exit code 0)
2. Why: `dotnet test` with xUnit v3 returns exit code 0 even when no tests are discovered/executed
3. Why: xUnit v3 launches `Era.Core.Tests.exe` as a separate process (out-of-process execution model), and WDAC blocks that executable silently from the test runner's perspective
4. Why: Windows WDAC (Enforcement status=2) blocks unsigned/untrusted executables, including locally-built test assemblies
5. Why: xUnit v3 changed its execution model from in-process (v2) to out-of-process, requiring test projects to produce executable binaries — a design choice that conflicts with strict application control policies

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|-----------|
| Pre-commit hook passes with 0 tests executed | xUnit v3's out-of-process execution model is incompatible with WDAC-enforced environments, and `dotnet test` does not fail when 0 tests are discovered |
| ~50 second hang during commit | xUnit v3 runner waiting for the blocked `Era.Core.Tests.exe` process to start |
| "Catastrophic failure: アプリケーション制御ポリシーによってこのファイルがブロックされました" | WDAC blocking the unsigned out-of-process test executable |

### Conclusion

The root cause is a **compatibility conflict between xUnit v3's out-of-process execution model and Windows WDAC enforcement**. xUnit v3 requires launching the test assembly as a standalone executable (`Era.Core.Tests.exe`), which WDAC blocks. Compounding this, `dotnet test` treats 0-tests-executed as success (exit code 0), creating a false positive CI gate.

The project does **NOT use any xUnit v3-specific APIs** (see Investigation Evidence below), making v2 rollback the lowest-risk option.

### Investigation Evidence

**v3-specific API scan results** (all negative):
- `Assert.Multiple` — not used
- `TestContext.Current` — not used
- `[Fact(Timeout=...)]` — not used
- `[assembly: CaptureConsole]` / `[assembly: CaptureTrace]` — not used
- `IAsyncLifetime` (new pattern) — not used
- `[Theory(DisableDiscoveryEnumeration=...)]` — not used

**`Xunit.Sdk` namespace usage**: Found in `Era.Core.Tests/Assertions/ResultAssertTests.cs` and `Era.Core.Tests/TestHelpers.cs` — these use `Xunit.Sdk.XunitException` which exists in both xUnit v2 and v3. No migration needed.

**Affected test projects** (all using xUnit v3 3.2.1):
- `Era.Core.Tests` (pre-commit hook target)
- `engine.Tests`
- `tools/ErbParser.Tests`
- `tools/ErbToYaml.Tests`
- `tools/YamlSchemaGen.Tests`
- `tools/KojoComparer.Tests`

**Projects already on xUnit v2** (unaffected):
- `tools/_archived/ErbLinter.Tests` (xunit 2.6.2)
- `tools/KojoQualityValidator.Tests` (xunit 2.5.3)

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F646 | DONE | Discovery context | Problem discovered during F646 Post-Phase Review Phase 19 pre-commit hook execution |
| F644 | DONE | Affected by fix | Equivalence Testing Framework — tests run through same xUnit infrastructure |
| F679 | DRAFT | Related infra | Phase 19 Tool Test Fixes — may also use xUnit v3→v2 changes applied by F680 |

### Pattern Analysis

This is a **first occurrence** — not a recurring pattern. The issue was introduced when the project migrated from xUnit v2 to v3 (likely during a package update). The migration was silent because the API surface used is backward-compatible, and the execution model change (in-process → out-of-process) was not apparent until WDAC enforcement blocked the executable.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Approach A (v2 rollback) confirmed feasible — no v3-specific APIs found in any test project |
| Scope is realistic | YES | Package reference changes in 6 .csproj files + pre-commit hook guard |
| No blocking constraints | YES | No WDAC admin access needed for Approach A; no external dependencies |

**Verdict**: FEASIBLE

**Approach assessment**:

| Approach | Feasibility | Recommendation |
|----------|:-----------:|:--------------:|
| A. xUnit v2 rollback | HIGH | Recommended — zero v3 API usage found, minimal change |
| B. Smart App Control off | LOW | Not recommended — irreversible, security degradation |
| C. WDAC policy exception | MEDIUM | Viable but requires admin policy management, adds environment-specific config |
| D. In-process execution | LOW | `InProcessTestProcessLauncher` is internal API (NCrunch-only), not a supported configuration |

**Additional mitigation (regardless of approach)**: The pre-commit hook should validate that test count > 0 to prevent false-positive passes in the future. `dotnet test` can output results in a parseable format.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F646 | DONE | Discovery context; no blocking dependency |
| Related | F679 | DRAFT | Tool test fixes may need same xUnit version change |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| xunit (v2 package) | Build-time | Low | Well-established, stable package (xunit 2.9.3 is latest v2) |
| xunit.runner.visualstudio (v2) | Build-time | Low | Required for `dotnet test` integration |
| Microsoft.NET.Test.Sdk | Build-time | Low | Already present (18.0.1), compatible with both v2 and v3 |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.githooks/pre-commit` | HIGH | Runs `dotnet test Era.Core.Tests` on every commit — currently broken (false positive) |
| Developer workflow | HIGH | Manual `dotnet test` execution for local validation |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `Era.Core.Tests/Era.Core.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `engine.Tests/uEmuera.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `tools/ErbParser.Tests/ErbParser.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `tools/ErbToYaml.Tests/ErbToYaml.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `tools/YamlSchemaGen.Tests/YamlSchemaGen.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `tools/KojoComparer.Tests/KojoComparer.Tests.csproj` | Update | Change xunit.v3 → xunit (v2) package references |
| `.githooks/pre-commit` | Update | Add test-count validation guard (0 tests = fail) |
| `engine.Tests/Tests/xunit.runner.json` | Review | v2 runner.json schema may differ slightly |

## Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| WDAC enforcement cannot be disabled | Windows security policy (irreversible Smart App Control) | HIGH — must work around, not disable |
| `Xunit.Sdk.XunitException` must remain available | Test assertion helpers (ResultAssert, TestHelpers) | LOW — available in both v2 and v3 |
| Pre-commit hook uses `set -e` for exit code checking | `.githooks/pre-commit` | MEDIUM — `dotnet test` exit code 0 on 0 tests bypasses this |
| 6 test projects share same xUnit version | Project consistency | LOW — all should be updated together |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| v2 rollback breaks compilation due to subtle API differences | Low | Medium | No v3-specific APIs found; `Xunit.Sdk` namespace exists in both versions |
| Future xUnit v3 re-migration needed | Medium | Low | Track as known debt; v3 compatibility requires WDAC resolution first |
| Test count guard gives false negative (legitimate 0 tests) | Low | Low | Only Era.Core.Tests has 90+ test files; 0 tests would indicate a real problem |
| Other test projects (engine.Tests, tools/*) also silently failing | Medium | Medium | Rollback all 6 projects together; verify each runs tests post-change |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "CI reliability guarantee - pre-commit hook tests are actually executed" | Tests must actually run (non-zero test count) | AC#4, AC#5 |
| "xUnit v3 out-of-process execution blocked by WDAC" | xUnit v2 packages must be referenced instead of v3 | AC#1, AC#2 |
| "dotnet test returns exit code 0 with 0 tests = false positive CI gate" | Pre-commit hook must detect and reject 0-test scenario | AC#7, AC#8 |
| "All 6 test projects share same xUnit version" | All 6 .csproj files updated consistently | AC#2, AC#3 |
| "No v3-specific APIs found" | Existing tests pass without modification after rollback | AC#4, AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Era.Core.Tests uses xUnit v2 package | file | Grep(Era.Core.Tests/Era.Core.Tests.csproj) | contains | "Include=\"xunit\" Version=\"2.9.3\"" | [x] |
| 2 | No xunit.v3 references in any test project | file | Grep(*.csproj) | not_contains | "xunit.v3" | [x] |
| 3 | All 6 test projects build successfully | build | dotnet build | succeeds | - | [x] |
| 4 | Era.Core.Tests passes | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 5 | All 6 test projects pass tests | test | dotnet test (all projects) | succeeds | - | [-] |
| 6 | Pre-commit hook validates test count | file | Grep(.githooks/pre-commit) | contains | "Total:[[:space:]]+0([^0-9]|$)" | [x] |
| 7 | Pre-commit hook contains missing-summary error message | file | Grep(.githooks/pre-commit) | contains | "No test summary found" | [x] |
| 8 | Pre-commit hook contains 0-test rejection logic | file | Grep(.githooks/pre-commit) | contains | "0 tests executed" | [x] |
| 9 | xunit.runner.visualstudio is v2-compatible version | file | Grep(Era.Core.Tests/Era.Core.Tests.csproj) | not_contains | "xunit.runner.visualstudio.*Version=\"3" | [x] |
| 10 | Clean restore succeeds after package migration | build | dotnet restore --force | succeeds | - | [x] |
| 11 | DRAFT feature-696 created for xUnit v3 re-migration | file | Grep(Game/agents/feature-696.md) | contains | "[DRAFT]" | [x] |
| 12 | feature-696 registered in index-features.md | file | Grep(Game/agents/index-features.md) | contains | "feature-696" | [x] |
| 13 | Era.Core.Tests executes >0 tests at runtime | output | dotnet test Era.Core.Tests | not_contains | "Total:    0" | [x] |

**Note**: 13 ACs is within the 8-15 range for infra type.

### AC Details

**AC#1: Era.Core.Tests uses xUnit v2 package**
- Verifies the primary test project (pre-commit hook target) references xunit v2, not xunit.v3
- The `xunit` package (without `.v3` suffix) is the v2 line
- Grep target: `Era.Core.Tests/Era.Core.Tests.csproj`

**AC#2: No xunit.v3 references in any test project**
- Ensures all 6 affected .csproj files have been updated
- Grep across all .csproj files for the string "xunit.v3" which should not appear
- Affected projects: Era.Core.Tests, engine.Tests, ErbParser.Tests, ErbToYaml.Tests, YamlSchemaGen.Tests, KojoComparer.Tests

**AC#3: All 6 test projects build successfully**
- `dotnet build` must succeed for each of the 6 test projects after package reference changes
- Validates no compilation errors from v3→v2 API differences
- Projects: Era.Core.Tests, engine.Tests (uEmuera.Tests), ErbParser.Tests, ErbToYaml.Tests, YamlSchemaGen.Tests, KojoComparer.Tests

**AC#4: Era.Core.Tests passes**
- Core verification: `dotnet test Era.Core.Tests` must execute tests and report success
- This is the project blocked by WDAC in the v3 configuration
- Relies on v2 in-process execution to avoid the original 0-test scenario (v2 won't be blocked by WDAC)
- Non-zero count guarantee is provided by the pre-commit hook guard (AC#6-8), not this AC

**AC#5: All 6 test projects pass tests**
- Comprehensive verification that rollback does not break any test project
- Run `dotnet test` for each of the 6 projects and verify all pass
- This catches any subtle v3-only API usage that the static scan may have missed

**AC#6: Pre-commit hook validates test count**
- The pre-commit hook must include logic to parse test output and reject 0-test runs
- Grep for "Total: 0" handling in the hook script (the guard checks for this condition)
- This is the "defense in depth" mitigation regardless of xUnit version

**AC#7: Pre-commit hook contains missing-summary error message**
- The pre-commit hook must include logic to detect when test output contains no summary
- Grep for "No test summary found" error message in the hook script
- This prevents false positives when dotnet test crashes without producing output

**AC#8: Pre-commit hook contains 0-test rejection logic**
- The pre-commit hook must contain the error message for 0-test detection
- Grep verifies the hook includes "0 tests executed" error message handling
- This ensures the specific 0-test rejection logic is implemented in the hook file

**AC#9: xunit.runner.visualstudio is v2-compatible version**
- xUnit v3 shipped its own runner (`xunit.runner.visualstudio` 3.x)
- After rollback, runner must be v2-compatible (2.x line)
- Grep verifies no Version="3... references for this package

**AC#10: Clean restore succeeds after package migration**
- `dotnet restore --force` must succeed cleanly after package reference changes
- Validates that the updated dependency graph is coherent and resolvable
- Forces a clean restore to verify the package migration is structurally sound

**AC#11: DRAFT feature-696 created for xUnit v3 re-migration**
- Task#7 creates feature-696.md with [DRAFT] status for future xUnit v3 re-migration tracking
- Grep verifies the file exists and contains [DRAFT] status marker after T7 execution
- This AC validates post-T7 execution state (T7 creates the target file)

**AC#12: feature-696 registered in index-features.md**
- Task#7 creates feature-696.md and registers it in index-features.md per DRAFT Creation Checklist
- Grep verifies the feature appears in the index after T7 execution
- This AC validates post-T7 execution state ensuring proper tracking and preventing orphaned features

**AC#13: Era.Core.Tests executes >0 tests at runtime**
- Verifies that after xUnit v2 rollback, Era.Core.Tests actually executes non-zero tests (not 0)
- This validates the core Philosophy guarantee: tests are "actually executed"
- Output type AC that fails if dotnet test output shows "Total:    0" (4 spaces as dotnet formats)

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Selected approach: xUnit v2 Rollback with Pre-commit Hook Test Count Guard**

This design combines two complementary solutions:

1. **Package Reference Migration (xUnit v3 → v2)**: Replace `xunit.v3` 3.2.1 with `xunit` 2.9.3 and `xunit.runner.visualstudio` 3.1.0 → 2.8.2 in all 6 affected test projects. This eliminates the root cause (out-of-process execution blocked by WDAC) while preserving all existing test functionality (zero v3-specific API usage confirmed).

2. **Pre-commit Hook Test Count Validation**: Add a guard to `.githooks/pre-commit` that parses `dotnet test` output and fails if test count is 0. This provides defense-in-depth against future false-positive CI passes regardless of xUnit version.

**Why this approach satisfies the ACs**:
- AC#1-3: Package references migration ensures all projects use xUnit v2
- AC#4-6: xUnit v2's in-process execution model bypasses WDAC, allowing tests to run
- AC#7-8: Pre-commit hook guard prevents 0-test false positives permanently
- AC#9-10: Runner compatibility and clean restore validate the migration is complete

**Rationale**: This is the lowest-risk option because:
- No v3-specific APIs are in use (confirmed by Root Cause investigation)
- xUnit v2 is stable and actively maintained (2.9.3 released 2024-12-15)
- In-process execution is compatible with WDAC (no executable launch required)
- Test count guard prevents regression even if packages change in the future

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Replace `<PackageReference Include="xunit.v3" Version="3.2.1" />` with `<PackageReference Include="xunit" Version="2.9.3" />` in `Era.Core.Tests/Era.Core.Tests.csproj` |
| 2 | Perform global search for "xunit.v3" across all .csproj files after replacement to verify no residual references |
| 3 | Run `dotnet build` for each of the 6 test projects: Era.Core.Tests, engine.Tests, ErbParser.Tests, ErbToYaml.Tests, YamlSchemaGen.Tests, KojoComparer.Tests |
| 4 | Execute `dotnet test Era.Core.Tests/Era.Core.Tests.csproj` and verify exit code 0 with test execution (not just discovery timeout) |
| 5 | Execute `dotnet test` for each of the 6 test projects and verify all return exit code 0 with tests passing |
| 6 | Add bash script logic to parse `dotnet test` output for "Total: X" pattern and exit 1 if X=0 |
| 7 | Add bash script logic to detect missing test summary and exit 1 if no "Total:" line found |
| 8 | Verify the pre-commit hook contains "0 tests executed" error message |
| 9 | Replace `<PackageReference Include="xunit.runner.visualstudio" Version="3.1.0">` with `<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">` in all 6 test projects, then grep to verify no Version="3 references remain |
| 10 | Execute `dotnet restore --force` at solution root to verify clean restore with updated package references |
| 11 | Create DRAFT feature-696.md file to track future xUnit v3 re-migration work item |
| 12 | Register feature-696 in index-features.md per DRAFT Creation Checklist |
| 13 | Execute dotnet test Era.Core.Tests and verify 0 tests are not executed |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **xUnit version strategy** | A. Rollback to v2<br>B. Add WDAC exception<br>C. Disable Smart App Control<br>D. In-process launcher config | **A. Rollback to v2** | Zero v3 API usage found; WDAC exception requires admin policy management (environment-specific); Smart App Control disable is irreversible; in-process launcher is internal NCrunch API (unsupported) |
| **xUnit v2 version** | 2.6.2 (ErbLinter.Tests)<br>2.9.3 (latest stable v2) | **2.9.3** | Standardize on latest stable release for security patches and compatibility; 2.9.3 released 2024-12-15 is actively maintained |
| **Runner version** | 2.5.4 (ErbLinter.Tests)<br>2.8.2 (latest v2) | **2.8.2** | Match latest runner with latest xunit core; ensures compatibility with Microsoft.NET.Test.Sdk 18.0.1 |
| **Hook test count validation** | Exit immediately if 0 tests<br>Skip validation (rely on xUnit fix) | **Exit immediately if 0 tests** | Defense-in-depth: prevents future false positives even if xUnit version changes or other issues cause 0-test runs |
| **Hook output parsing** | Regex on "Total: X"<br>JSON output (`--logger trx`) | **Regex on "Total: X"** | Simpler implementation; `dotnet test` default output format is stable; TRX requires XML parsing overhead |
| **xunit.runner.json migration** | Update schema URL to v2<br>Keep existing config | **Keep existing config** | v2 schema is backward-compatible; `parallelizeAssembly` and `parallelizeTestCollections` exist in both versions; no migration needed |
| **Test project update order** | Sequential (one at a time)<br>All 6 simultaneously | **All 6 simultaneously** | Ensures version consistency; prevents mixed-version state; single atomic commit |

### Package Reference Changes

**Before (xUnit v3)**:
```xml
<PackageReference Include="xunit.v3" Version="3.2.1" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

**After (xUnit v2)**:
```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

**Note**: `<IncludeAssets>` and `<PrivateAssets>` attributes remain unchanged (identical structure in both versions).

### Pre-commit Hook Enhancement

**Current implementation** (`.githooks/pre-commit` lines 17-18):
```bash
echo "[2/3] dotnet test Era.Core.Tests..."
dotnet test Era.Core.Tests/Era.Core.Tests.csproj --nologo -v q
```

**Enhanced implementation** (with test count validation):
```bash
echo "[2/3] dotnet test Era.Core.Tests..."
TEST_OUTPUT=$(dotnet test Era.Core.Tests/Era.Core.Tests.csproj --nologo -v m 2>&1)
TEST_EXIT_CODE=$?
echo "$TEST_OUTPUT"

# Check dotnet test exit code first (test failures take precedence)
if [ $TEST_EXIT_CODE -ne 0 ]; then
  exit $TEST_EXIT_CODE
fi

# Validate test count > 0 to prevent false positive (xUnit v3 WDAC issue)
if echo "$TEST_OUTPUT" | grep -qE "Total:[[:space:]]+0([^0-9]|$)"; then
  echo "ERROR: 0 tests executed. This indicates a test discovery or execution failure."
  exit 1
fi

# Validate test summary exists to prevent crashes/missing output
if echo "$TEST_OUTPUT" | grep -qE 'Total:[[:space:]]+[0-9]+'; then
  : # Test summary found, continue
else
  echo "ERROR: No test summary found in output. This indicates a test runner crash."
  exit 1
fi
```

**Design rationale**:
1. Capture `dotnet test` output to `TEST_OUTPUT` variable (both stdout and stderr via `2>&1`)
2. Use `-v m` (minimal verbosity) to ensure test result summary is included in output
3. Preserve exit code in `TEST_EXIT_CODE` before running other commands
4. Echo output to maintain visible feedback (same UX as current hook)
5. Parse output for "Total: 0" pattern (matches "Failed: X, Passed: Y, Skipped: Z, Total: 0")
6. Exit 1 with clear error message if 0 tests detected
7. Exit with original `dotnet test` exit code if non-zero (test failures)

**Edge cases handled**:
- `dotnet test` succeeds with 0 tests → Hook fails (AC#8)
- `dotnet test` fails with >0 tests → Hook fails with original exit code (checked first)
- `dotnet test` succeeds with >0 tests → Hook continues to completion (CI PASSED)
- Grep pattern uses flexible spacing: "Total:[[:space:]]+0([^0-9]|$)" matches dotnet test's variable-width output format ("Total:    0" with multiple spaces)

**Scope Limitation**: The hook enhancement covers Era.Core.Tests (pre-commit hook target) with test-count validation. The other 5 test projects (engine.Tests, ErbParser.Tests, ErbToYaml.Tests, YamlSchemaGen.Tests, KojoComparer.Tests) are migrated to xUnit v2 but rely on manual verification for 0-test scenarios.

**Rationale**: The pre-commit hook only runs Era.Core.Tests (line 18), making it the only automated CI target. The other 5 projects are development-time tools run manually. The Philosophy "CI reliability guarantee" applies specifically to automated execution (pre-commit hook), not manual developer testing.

### xunit.runner.json Compatibility

**Current configuration** (`engine.Tests/Tests/xunit.runner.json`):
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": true
}
```

**Migration assessment**: No changes required. Both `parallelizeAssembly` and `parallelizeTestCollections` are supported in xUnit v2 (introduced in v2.0). The `$schema` URL points to `/schema/current/` which auto-redirects to the appropriate version based on the xunit package version detected in the project.

**Verification**: After rollback, the schema URL will resolve to the v2 schema, and the configuration will remain valid.

### File Impact Summary

| File | Lines Changed | Change Type | Description |
|------|:-------------:|:-----------:|-------------|
| `Era.Core.Tests/Era.Core.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `engine.Tests/uEmuera.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `tools/ErbParser.Tests/ErbParser.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `tools/ErbToYaml.Tests/ErbToYaml.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `tools/YamlSchemaGen.Tests/YamlSchemaGen.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `tools/KojoComparer.Tests/KojoComparer.Tests.csproj` | 2 | Edit | Replace xunit.v3 3.2.1 → xunit 2.9.3, runner 3.1.0 → 2.8.2 |
| `.githooks/pre-commit` | +10 | Edit | Add test count validation logic after line 17; change verbosity -v q → -v m |
| `Game/agents/feature-696.md` | 1 | New | DRAFT feature for xUnit v3 re-migration tracking |
| `Game/agents/index-features.md` | 1 | Edit | Register feature-696 in index |
| `engine.Tests/Tests/xunit.runner.json` | 0 | No change | v2-compatible (both settings exist in v2) |

**Total**: 9 files modified, 1 new file, 0 deletions. 24 lines changed (12 package references + 10 hook lines + 2 tracking files).

### Implementation Sequence

1. **Package reference updates** (parallel for all 6 projects):
   - Update xunit.v3 → xunit in .csproj files
   - Update xunit.runner.visualstudio versions in .csproj files

2. **Clean restore**:
   - Execute `dotnet restore --force` to clear v3 packages from cache

3. **Build validation**:
   - Build each test project individually to verify no compilation errors

4. **Test execution validation**:
   - Run `dotnet test` for each project to verify tests execute and pass

5. **Pre-commit hook enhancement**:
   - Add test count validation logic
   - Test the hook with a simulated 0-test scenario

6. **Full CI validation**:
   - Run complete pre-commit hook to verify end-to-end workflow

**Atomic commit strategy**: All changes committed together to ensure the repository never has a mixed v2/v3 state.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,9 | Update package references in all 6 test projects (xunit.v3 → xunit 2.9.3, runner 3.1.0 → 2.8.2) | [x] |
| 2 | 10 | Execute dotnet restore --force to clear v3 package cache | [x] |
| 3 | 3 | Build all 6 test projects to verify no compilation errors | [x] |
| 4 | 4,13 | Run Era.Core.Tests and verify test execution with non-zero count | [x] |
| 5 | 5 | Run all 6 test projects and verify all tests pass | [-] |
| 6 | 6,7,8 | Add test count validation logic to pre-commit hook | [x] |
| 7 | 11,12 | Create DRAFT feature-696 for xUnit v3 re-migration tracking and register in index-features.md (verify F696 ID available) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T3 | Package reference changes from Technical Design | Updated .csproj files, clean build |
| 2 | implementer | sonnet | T4-T5 | Test execution requirements from Technical Design | Test execution verification |
| 3 | implementer | sonnet | T6 | Pre-commit hook enhancement from Technical Design | Enhanced hook with test count guard |
| 4 | implementer | sonnet | T7 | DRAFT Creation Checklist from feature-template.md | feature-696.md created and registered in index-features.md |

**Constraints** (from Technical Design):

1. All 6 test projects must be updated simultaneously to maintain version consistency
2. Package reference attributes (`<IncludeAssets>`, `<PrivateAssets>`) must be preserved unchanged
3. Pre-commit hook test count validation must parse "Total: 0" pattern explicitly
4. xunit.runner.json configuration files require no changes (v2-compatible)

**Pre-conditions**:

- No unstaged changes in affected files (.csproj, .githooks/pre-commit)
- `.githooks` path configured as git hooks directory
- `dotnet` CLI available (verified by existing pre-commit hook)
- All 6 test projects currently reference xunit.v3 3.2.1 and xunit.runner.visualstudio 3.1.0

**Success Criteria**:

- All 13 ACs marked `[x]` in AC table
- `dotnet test Era.Core.Tests` executes >0 tests and returns exit code 0
- Pre-commit hook validates test count and rejects 0-test scenarios
- No xunit.v3 references remain in any .csproj file
- All test projects build and pass tests

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert`
2. Restore xUnit v3 package references to original versions (xunit.v3 3.2.1, runner 3.1.0)
3. Execute `dotnet restore --force` to restore v3 packages
4. Notify user of rollback reason (e.g., test failures, unexpected API incompatibility)
5. Create follow-up feature for investigation with additional evidence from rollback

**Alternative if xUnit v2 proves incompatible**:

1. Abandon xUnit v2 rollback approach
2. Create new feature to implement Approach C (WDAC policy exception) from Feasibility Assessment
3. Document v2 incompatibility as Handoff item

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| xUnit v3 re-migration when WDAC issue is resolved | Future requirement tracked as risk | New Feature (DRAFT) | F696 | T7 |

<!-- Handoff Protocol: Track concrete destination (F{ID}, T{N}, Phase N) with actionable creation task -->

---

## Review Notes

- [resolved-applied] Phase6-FinalRefCheck iter10: AC#12 restructured from pre-condition file existence check to post-T7 execution verification (Grep for [DRAFT] status in feature-696.md).
- [resolved-invalid] Phase0-RefCheck iter1: Reference-checker flagged AC#12/T7/F696 forward reference as invalid, but this follows valid pattern: T7 creates F696, AC#12 verifies post-execution, Handoff table correctly references T7 as creation task. Pattern is legitimate for implementation workflow.
- [resolved-applied] Phase1-Uncertain iter5: Scope Discipline section format updated to match feature-template.md "Out-of-Scope Issue Protocol" STOP/REPORT/TRACK/LINK format.
- [resolved-applied] Phase6-FinalRefCheck iter9: Original F694 was reserved by F647 (Phase 20 Planning, F686-F695 range). Changed to F696 and created DRAFT feature-696.md during FL POST-LOOP per user decision.
- [resolved-applied] Phase0-RefCheck iter1: AC#12 and AC#13 forward reference issue resolved by Phase6-FinalRefCheck iter10 restructuring AC#12 to post-T7 verification.

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-30 19:41 | START | Task 1-3 (implementer) |
| 2026-01-30 19:41 | END | Task 1-3: SUCCESS - All 6 test projects updated, restored, and built successfully |
| 2026-01-30 19:42 | START | Task 4-5 (implementer) |
| 2026-01-30 19:42 | DEVIATION | implementer | dotnet test YamlSchemaGen.Tests | PRE-EXISTING: SchemaValidationTests.Schema_ValidatesSampleDialogue fails (PropertyRequired: #/entries). Not caused by F680. 5/6 projects pass. |
| 2026-01-30 19:42 | END | Task 4: SUCCESS (1427 tests). Task 5: 5/6 PASS, 1 PRE-EXISTING fail |
| 2026-01-30 19:43 | START | Task 6 (implementer) |
| 2026-01-30 19:43 | END | Task 6: SUCCESS - Pre-commit hook enhanced with test count validation |
| 2026-01-30 19:43 | NOTE | Task 7 was completed during FL POST-LOOP (pre-/run). feature-696.md created and registered. |
| 2026-01-30 19:44 | DEVIATION | ac-static-verifier | file verification | exit code 1: 4/8 passed. Tool limitations: regex pattern [[:space:]] and [DRAFT] brackets misinterpreted, *.csproj glob failed on Windows |
| 2026-01-30 19:44 | DEVIATION | ac-static-verifier | build verification | exit code 1: 0/2 passed. Tool CWD issue: dotnet build/restore ran without project path specification |
| 2026-01-30 19:45 | NOTE | F697 DRAFT created for YamlSchemaGen test failure (user decision: AC#5 waive as PRE-EXISTING) |
| 2026-01-30 19:45 | NOTE | Workflow fix: ac-designer, wbs-generator, PHASE-4.md に Goal Coverage Verification 追加 (Goal#2 skip 再発防止) |
| 2026-01-30 19:46 | DEVIATION | pre-commit hook | commit failed | exit code 1: "No test summary found" — dotnet test output uses Japanese locale (合計:) not English (Total:). Hook regex only matched English. |
| 2026-01-30 19:46 | FIX | pre-commit hook | Added Japanese locale support: (Total|合計) pattern in grep |

---

## Notes

### Discovery Context

During F646 Phase 9 commit, the pre-commit hook (`dotnet test Era.Core.Tests`) appeared to hang for over 10 minutes. Investigation revealed:
- Tests timed out after ~50 seconds
- xUnit v3 tried to launch `Era.Core.Tests.exe` but was blocked by WDAC
- 0 tests executed, exit code 0 (false positive pass)

### WDAC Status

```
CodeIntegrityPolicyEnforcementStatus: 2 (Enforced)
UsermodeCodeIntegrityPolicyEnforcementStatus: 2 (Enforced)
```

### Current Packages

```xml
<PackageReference Include="xunit.v3" Version="3.2.1" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.0">
```

### Approach Options

| Approach | Content | Advantages | Disadvantages |
|----------|---------|------------|---------------|
| A. xUnit v2 Rollback | Revert v3 → v2 | Simple, avoids WDAC issue | v3 migration work wasted. Rewrite needed if v3 features used |
| B. Smart App Control Off | Disable Windows setting | Root cause resolution | **Irreversible** (cannot re-enable). Security degradation |
| C. WDAC Policy Exception | Register test build output path as trusted | Maintain WDAC + enable test execution | Admin rights required, policy management complexity |
| D. dotnet test --in-process | xUnit v3 In-Process execution | Avoid WDAC + maintain v3 | No such setting in xunit.runner.json. `InProcessTestProcessLauncher` is internal API (NCrunch only) |

### Investigation Items (Completed - see Root Cause Analysis)

1. Check if Era.Core.Tests uses xUnit v3-specific features
   - `[Fact(Timeout = ...)]` timeout specification
   - `TestContext.Current` usage
   - `[assembly: CaptureConsole]` / `[assembly: CaptureTrace]`
   - New `IAsyncLifetime` patterns
   - `Assert.Multiple` and other v3 new APIs
   - v3-specific attributes (`[Theory(DisableDiscoveryEnumeration = ...)]`, etc.)
2. Feasibility of adding WDAC policy exceptions
3. Change volume for v2 rollback scenario

---
