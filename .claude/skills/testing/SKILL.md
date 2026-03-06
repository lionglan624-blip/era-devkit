---
name: testing
description: Testing reference for ERA games. Use when running tests, writing test scenarios, using --unit, --debug modes, AC verification.
---

# Testing Reference

> **Last Updated:** 2026-02-13
> **Purpose:** Essential testing knowledge in minimal form
> **Design Reference:** [test-strategy.md](../../../docs/strategy/test-strategy.md) - Comprehensive test strategy including /run command integration, log output patterns, pre-commit hook design, AC verification flow, and IRandomProvider abstraction

---

## Quick Reference

> **Execution**: All `dotnet` commands execute via WSL2. See [wsl-dotnet-setup.md](../../../docs/reference/wsl-dotnet-setup.md).
>
> **Agent pattern**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet <command>'`
>
> **Hang prevention**: All `dotnet test` invocations MUST include `--blame-hang-timeout 10s`. This kills any single test hanging beyond 10 seconds.

| Test Type | Command (inside WSL) | Use Case |
|-----------|---------|----------|
| **C# Unit (all)** | `dotnet test devkit.sln --no-build --blame-hang-timeout 10s` | Full regression |
| **C# Unit (Era.Core)** | `dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s` | YAML rendering, KojoEngine |
| **C# Coverage (Era.Core)** | `dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --collect:"XPlat Code Coverage"` | Coverage measurement |
| **C# Unit (tools)** | `dotnet test src/tools/dotnet/ErbParser.Tests/ --blame-hang-timeout 10s` | ERB parser (example; applies to any tool test project) |
| **C# E2E (Era.Core)** | `dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "Category=E2E"` | DI resolution, cross-system flow |
| **Kojo unit** | `--unit path/` or `--unit "path/*.json"` | Kojo function test |
| **debug** | `--debug --char N` | Interactive debug |
| **strict** | `--strict-warnings < /dev/null` | Parser warnings |

**Base command** (execute from Game directory via WSL):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/game && /home/siihe/.dotnet/dotnet run --project ../engine/uEmuera.Headless.csproj -- . [OPTIONS]'
```

---

## AC Definition Format

```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Meiling affection gained | output | --unit | contains | "最近一緒にいると" | [ ] |
```

### AC Types

| Type | Purpose | Verification Method |
|------|---------|---------------------|
| `output` | Screen output verification | --unit |
| `variable` | Variable value verification | --unit (dump) |
| `build` | Build success | dotnet build |
| `exit_code` | Exit code | Script execution |
| `file` | File existence/contents | Glob/Grep (static) |
| `code` | Code content verification | Grep (static) |
| `test` | C# unit test | dotnet test |

### Method Column Usage (file/code Type)

| AC Type | Matcher | Recommended Method | Example |
|---------|---------|-------------------|---------|
| file | exists/not_exists | Glob | `Glob("pm/features/feature-*.md")` |
| file | contains/matches | Grep | `Grep("## Status", "feature-226.md")` |
| code | contains/matches | Grep | `Grep("Skill\(testing\)", "run.md")` |
| code | not_contains | Grep | `Grep("hardcoded", "run.md")` (expect 0) |
| code | contains/matches | Grep (complex) | `Grep(path="src/tools/python/*.py", pattern="def classify", type=cs)` |

**Complex Method Format (F632)**: Named parameters `Grep(path="...", pattern="...", type=...)` are supported. When `pattern` is specified, it overrides the Expected column for matching. The `type` parameter is optional.

**Note**:
- `exists`/`not_exists` Matcher → Method = `Glob` (file path pattern)
- `contains`/`matches` Matcher → Method = `Grep` (content search)
- testing SKILL's "Verification Method: Glob/Grep" describes the Type overall. Individual ACs use appropriate method based on Matcher.

### Matchers

| Matcher | Judgment |
|---------|----------|
| `equals` | Exact match (trimmed) |
| `contains` | Contains substring |
| `not_contains` | Does not contain substring |
| `matches` | Regex match |
| `not_matches` | Regex pattern NOT found |
| `succeeds` | exit code 0 (or slash command success) |
| `fails` | exit code ≠ 0 |
| `gt/gte/lt/lte` | Numeric comparison |
| `count_equals` | Count matches equals expected number |
| `exists` | File exists |
| `not_exists` | File does not exist |

**Slash Command Exception**: Slash commands (e.g., `/audit`) are not shell commands and don't return exit codes. Use `file | /command | succeeds` pattern per INFRA.md Issue 19.

### Matcher Selection Guide

| Feature Type | Recommended Matcher | Reason |
|--------------|---------------------|--------|
| kojo | `contains` × multiple lines | Order/whitespace may vary |
| erb/engine | `equals` | Strict match ideal |
| infra (MD) | `contains` | Whitespace/newline may vary |

**Status**: `[ ]` Not tested | `[x]` PASS | `[-]` FAIL | `[B]` BLOCKED
**Judgment**: Binary yes/no only.

**Completion Rule**: Any `[-]` or `[B]` blocks `[DONE]`. `[B]` requires explicit user waive with Mandatory Handoffs tracking.

---

## Test Types by Feature

| Feature Type | AC Type(s) | Required Tests | Pos/Neg | Details |
|--------------|------------|----------------|:-------:|---------|
| kojo | output | unit (--unit) | Pos only | [KOJO.md](KOJO.md) |
| engine | output, variable, exit_code | C# Unit | **Both** | [ENGINE.md](ENGINE.md) |
| hook | output, exit_code | AC test | **Both** | Prevent malfunction |
| subagent | output | AC test | **Both** | Verify intervention logic |
| infra | build, file, test | dotnet build, E2E | Pos only | - |
<!-- erb type archived (2026-01-10) - ERB editing disabled during migration.
     feature-quality/ERB.md retained for reviewing legacy ERB features only. -->

---

## Positive/Negative Testing

To flush out bugs, program-related features should test both positive (happy path) and negative (error cases/boundary conditions).

### Pos/Neg Requirements

| Type | Pos | Neg | Reason |
|------|:---:|:---:|--------|
| engine | ◎ | ◎ | Unit tests mandatory. Verify boundary conditions & error cases |
| erb | ◎ | ◎ | Cover branching & boundary conditions |
| hook | ◎ | ◎ | Preventing malfunction is critical. Verify both FAIL/PASS |
| subagent | ◎ | ◎ | Verify normal operation + non-intervention cases |
| kojo | ○ | - | Output verification only |
| infra | ○ | - | Existence verification only |

---

## PRE-EXISTING Judgment

| Feature Status | Failure Class | Action |
|----------------|---------------|--------|
| [WIP] | PRE-EXISTING | Note, proceed |
| [DONE] | NEW | debugger |

**If [WIP] then PRE-EXISTING** - Can proceed without inquiry.

---

### AC Naming Convention

```markdown
| 9 | Save on FAIL (Pos) | output | contains | "saved" | [ ] |
| 10 | No save on PASS (Neg) | output | not_contains | "saved" | [ ] |
```

### Examples

| Feature | Positive | Negative |
|---------|----------|----------|
| Save log on FAIL | FAIL → saved | PASS → not saved |
| Path conversion | ac/ → conversion success | debug/ → returns null |
| debugger intervention | On error → execute fix | Normal → no intervention |

---

## Loading Warning Detection (F101)

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/game && /home/siihe/.dotnet/dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings < /dev/null 2>&1'
```

**Output**: `[WARNING] file:line: message` → Exit code 1 when warnings exist.
**Integration**: NEW warnings block Feature completion.

---

## debug Mode (`--debug`)

**File input (recommended)** - Avoids UTF-8 encoding issues:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/game && /home/siihe/.dotnet/dotnet run --project ../engine/uEmuera.Headless.csproj -- . --debug --char 1 --input-file tests/test.txt'
```

### JSON Protocol Commands

| Command | Example |
|---------|---------|
| `setup` | `{"cmd":"setup","char":"1"}` |
| `call` | `{"cmd":"call","func":"COM352"}` |
| `call` + args | `{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[4]}` |
| `set` | `{"cmd":"set","var":"FLAG:0","value":12345}` |
| `set-after-wakeup` | `{"cmd":"set-after-wakeup","var":"CFLAG:4:300","value":5}` |
| `dump` | `{"cmd":"dump","vars":["FLAG:0"]}` |
| `assert_contains` | `{"cmd":"assert_contains","text":"expected text"}` |
| `assert_equals` | `{"cmd":"assert_equals","var":"FLAG:0","value":42}` |
| `exit` | `{"cmd":"exit"}` |

### Supported Variables

**Supported**: `TALENT`, `CFLAG`, `ABL`, `BASE`, `TARGET`, `FLAG`, `TFLAG`, `TCVAR`
**Not Supported**: `PALAM`, `MARK`

**Important**: CsvNo ≠ CharacterIndex. `--char 4` adds Chara4.csv but gets index 1.

### Assert Commands (F081)

```json
{"cmd":"setup","char":"1"}
{"cmd":"call","func":"KOJO_MESSAGE_思慕獲得_KU","args":[1]}
{"cmd":"assert_contains","text":"最近一緒"}
{"cmd":"exit"}
```

**Response**: `{"status":"pass",...}` or `{"status":"fail",...}`

---

## debug vs Production Tests

| Path | Purpose | Edit |
|------|---------|:----:|
| tests/ac/ | AC verification (production) | **Forbidden** |
| src/Era.Core.Tests/ | C# unit tests (production) | **Forbidden** |
| tests/debug/ | Debugging | Allowed |

When test fails:
1. Create debug test in tests/debug/
2. Investigate cause
3. Fix implementation
4. Re-run production test

---

## Log Directory Structure

```
logs/
├── prod/                    # Production test results (verify-logs.py target)
│   ├── ac/kojo/feature-{N}/ # AC test results
│   └── ac/engine/           # C# Unit Test results (.trx)
└── debug/                   # Debug use (not verified)
    ├── failed/              # FAIL history
    └── scratch/             # Temporary debug runs
```

**FAIL Log Preservation**: When a test in `tests/ac/` fails, the output is automatically copied to `logs/debug/failed/{test-name}.txt` for investigation history.

**PASS behavior**: Passing tests do NOT write to `logs/debug/failed/`.

---

## Log Output Commands

| Test | Command |
|------|---------|
| C# Unit | `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet test devkit.sln --no-build --blame-hang-timeout 10s --logger "trx;LogFileName=test-result.trx" --results-directory _out/logs/prod/ac/'` |
| AC (kojo) | `--unit tests/ac/kojo/feature-{N}/` |
<!-- Regression tests archived (2026-01-10) -->

---

## Log Verification

```bash
python src/tools/python/verify-logs.py --dir _out/logs/prod [--scope all|feature:{ID}|engine]
```

**Scope parameter** (default: `all`):
| Scope | Behavior |
|-------|----------|
| `all` | Filters by active features from `index-features.md` (lifecycle-aware, F787) |
| `feature:{ID}` | Verifies logs for a specific feature only (used by PHASE-9) |
| `engine` | Scans all engine TRX files without filtering |

| Path | Format | Pass Check |
|------|--------|------------|
| `logs/prod/ac/engine/**/*.trx` | XML | `outcome="Passed"` for all tests (scope-filtered) |
| `logs/prod/ac/**/*-result.json` | JSON | `summary.failed == 0` (scope-filtered) |
<!-- regression/*-result.json archived (2026-01-10) - regression tests removed during migration -->

---

## Static Verification (ac-static-verifier.py)

**Purpose**: Verifies code/build/file type ACs through static analysis, generating JSON logs for verification.

**What it does**:
- Validates code type ACs using grep pattern matching
- Validates build type ACs by executing build commands
- Validates file type ACs by checking file existence (supports glob patterns and directory paths) or content matching (Grep method)
- Generates standardized JSON logs compatible with verify-logs.py

**CLI Usage**:

```bash
python src/tools/python/ac-static-verifier.py --feature {ID} --ac-type {code|build|file}
```

**Arguments**:
- `--feature {ID}`: Feature number (e.g., 268)
- `--ac-type {type}`: AC type to verify (code, build, or file)
- `--repo-root {path}`: Repository root directory (default: current directory)

**Output Location**:

```
_out/logs/prod/ac/{type}/feature-{ID}/{type}-result.json
```

**Examples**:

```bash
# Verify code type ACs for feature 268
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type code

# Verify build type ACs
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type build

# Verify file type ACs
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type file
```

**Output Format**:

```json
{
  "feature": 268,
  "type": "code",
  "results": [
    {
      "ac_number": 9,
      "result": "PASS",
      "details": {
        "pattern": "AC Type=build/file/code",
        "file_path": ".claude/commands/run.md",
        "matcher": "not_contains",
        "pattern_found": false,
        "matched_files": []
      }
    }
  ],
  "summary": {
    "total": 1,
    "passed": 1,
    "manual": 0,
    "failed": 0
  }
}
```

**Result Status Values**:
- `PASS`: AC verification succeeded
- `FAIL`: AC verification failed
- `MANUAL`: Slash command AC requiring manual verification (see [manual verification guide](../../src/tools/python/ac-static-verifier-manual-verification.md))

**Integration with verify-logs.py**:

The static verifier generates logs in the same format as other AC verifiers. Use `verify-logs.py` to aggregate results:

```bash
python src/tools/python/verify-logs.py --scope feature:268
# Output: Feature-268: OK:12/12
```

**Exit Codes**:
- `0`: All ACs passed
- `1`: One or more ACs failed or no matching ACs found

**AC Type Requirements**:

| AC Type | Method Column | Matcher | Expected Column |
|---------|---------------|---------|-----------------|
| code | Grep(path or dir/) | contains/not_contains/matches/not_matches | Pattern to search |
| build | (build command) | succeeds/fails | dotnet build |
| build | `<command>` | succeeds/fails | `-` (F626: uses Method) |
| file | Glob | exists/not_exists | File path or pattern |
| file | `Glob(pattern)` | exists/not_exists | `-` (F626: uses Method) |
| file | Grep(path) | contains/not_contains/matches/not_matches | Pattern to search |

**Expected Column Escaping**: Expected values support backslash-escaped quotes (`\"` → `"`) for AC values containing literal double quotes. Example: `"contains \"escaped\" text"` matches content containing `contains "escaped" text`.

---

## CI (pre-commit)

<!-- Regression tests archived (2026-01-10) - ERB flow tests不要 (migration中) -->

Pre-commit hook runs **C# test gate** via WSL2 before each commit.

```bash
[0/7] Schema synchronization check
[1/7] Variable size validation
[2/7] Dashboard lint check (skip if no dashboard files)
[3/7] wsl_dotnet build devkit.sln              # WSL2 経由
[4/7] wsl_dotnet format (per-project, staged files only) # WSL2 経由
[5/7] wsl_dotnet test devkit.sln --no-build    # WSL2 経由
```

**Design philosophy**: Ensures C# code compiles and tests pass before commit. Uses WSL2 to bypass Smart App Control.

Setup: `git config core.hooksPath .githooks`

Bypass (dev only): `git commit --no-verify`

---

## Quality Audit (F300)

**Purpose**: Batch audit of existing kojo implementations against Phase 8d quality standards.

```bash
python src/tools/kojo-mapper/kojo_mapper.py Game/ERB/口上 --quality
```

**Output**:
- Console: Quality summary + LOW_QUALITY list
- File: `pm/audit/kojo-quality-YYYY-MM-DD.md`

**Phase 8d Criteria**:
| Item | Check | Status |
|------|-------|--------|
| TALENT branching | 4 branches (lover/affection/admiration/none) | TALENT_4 |
| Line count | 4-8 lines per branch | LOW (<4) / OK / EXCESS (>8) |
| Pattern count | 1+ per branch | LOW (0) / OK (1+) |

**Quality Levels**:
- `STUB`: No DATAFORM content
- `IMPLEMENTED`: Has content
- `LOW_QUALITY`: Implemented but fails Phase 8d
- `Phase 8d PASS`: Meets all criteria

---

## Known Limitations

| Limitation | Workaround |
|------------|------------|
| **PALAM unsupported** | Use indirect parameters |
| **MARK unsupported** | Use other variables |
| **WAIT blocks** | Use call unit test instead |
| **ac-static-verifier: equals matcher** | Use `contains` with unique substring (F608 audit) |
| **ac-static-verifier: count_equals/pipe escape resolved (F792)** | count_equals, gt, gte, lt, lte now work for code/file+Grep types; `\|` pipe escaping now supported in AC Expected values |
| **.NET analyzer suggestion severity: CLI非表示 (F836)** | `suggestion` severity の diagnostics は `dotnet build` CLI stdout に表示されない（IDE専用）。検証には SARIF 出力を使用: `dotnet build Project.csproj /p:ErrorLog=output.sarif` → `grep CA1502 output.sarif` |

---

## Error Message Language Convention

**CRITICAL**: Error messages in C# code (Era.Core, Era.Core.Tests) use **English**.

| Component | Language | Example |
|-----------|----------|---------|
| Production error messages | English | `"No dialogue entries match the current context"` |
| Test assertions | English | `Assert.Contains("No dialogue entries", ...)` |
| In-game user-facing text | Japanese | `"Kojo file not found"` (example) |
| YAML content | Japanese | kojo dialogue text |

**Why**: Era.Core is a library that may be used in different contexts. English error messages are standard for libraries and make debugging easier.

**Common Mistake (F629)**: Test expects Japanese error message but code returns English:
```csharp
// NG: Mismatch
Assert.Contains("No kojo entries matching the condition were found", failure.Error);

// OK: Match actual English message
Assert.Contains("No dialogue entries match the current context", failure.Error);
```

**Rule**: When writing test assertions for error messages, check the actual error message in the source code first.

---

## Era.Core.Tests Infrastructure (F359)

Test utilities for `src/Era.Core.Tests/`:

| File | Purpose |
|------|---------|
| `Assertions/ResultAssert.cs` | Result<T> type assertions - AssertSuccess/AssertFailure (F491) |
| `BaseTestClass.cs` | Common setup/teardown, path resolution, assertion helpers |
| `TestHelpers.cs` | YAML loading, string normalization, context builders |
| `Mocks/MockGameContext.cs` | Fluent API for TALENT/CFLAG/ABL/EXP mock state |

**Usage Example**:
```csharp
// Using MockGameContext
var context = new MockGameContext()
    .WithTalent(50, 1)    // TALENT:50 = 1
    .WithAbl(2, 5)        // ABL:2 = 5
    .Build();

// Using TestHelpers
var yaml = TestHelpers.LoadYamlFile("sample-dialogue.yaml");
var context = TestHelpers.CreateContext(talent: talentDict, abl: ablDict);
```

**Coverage Baseline** (F359): 70.1% line / 55.76% branch. Target: 95%.

---

## E2E Tests (F813)

End-to-end tests that verify full DI container resolution and cross-system flows using the real `AddEraCore()` registration (not mocked).

| File | Purpose |
|------|---------|
| `E2E/DiResolutionTests.cs` | Verifies all Phase 5-21 services resolve via `GetRequiredService<T>()` |
| `E2E/CrossSystemFlowTests.cs` | Training→Counter cross-system flow with seeded `IRandomProvider` |

**Directory**: `src/Era.Core.Tests/E2E/`
**Trait**: `[Trait("Category", "E2E")]`
**Run**: `dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "Category=E2E"`

**Key patterns**:
- Full DI container via `services.AddEraCore()` + `BuildServiceProvider()` (not mocked)
- `SeededRandomProvider(42)` for deterministic cross-system tests
- Null-prefixed stubs provide safe defaults for unimplemented interfaces

**Mutation Baseline** (F813): 99.87% mutation score (Stryker.NET, 29754/29794 killed).

---

## Infinite Loop / Hang Detection

> **Lesson (2026-02-26)**: `CounterPunishment.Execute()` had a `while(continueLoop)` loop with no iteration limit. A CFLAG constant mismatch (test: 1131, impl: 206) caused the guard to always fail, creating an infinite loop that froze the entire test suite for 3+ minutes.

### Prevention Rules

| Rule | Description |
|------|-------------|
| **`--blame-hang-timeout 10s`** | ALL `dotnet test` invocations MUST include this flag. Kills any single test exceeding 10 seconds, preventing zombie processes |
| **Loop guard** | All `while` loops in production code MUST have a `maxIterations` guard (e.g., `for (int i = 0; i < 100 && condition; i++)`) |
| **Constant alignment** | TDD RED tests MUST use the same magic numbers as the implementation. Mismatched constants cause silent behavioral divergence |
| **Filtered test runs** | Use `--filter` to isolate slow/hanging tests: `dotnet test --no-build --filter "FullyQualifiedName~ClassName"` |

### Diagnosing Test Hangs

```bash
# 1. Quick full run with timeout (detect hang)
timeout 15 dotnet test src/Era.Core.Tests/ --no-build --blame-hang-timeout 10s --nologo -v q

# 2. Binary search: exclude suspect group
dotnet test --no-build --blame-hang-timeout 10s --filter "FullyQualifiedName!~SuspectClass"

# 3. Isolate single class
dotnet test --no-build --blame-hang-timeout 10s --filter "FullyQualifiedName~ExactClassName"

# 4. Pair test: run two classes together to detect parallel deadlock
dotnet test --no-build --blame-hang-timeout 10s --filter "FullyQualifiedName~ClassA|FullyQualifiedName~ClassB"
```

### Test Suite Performance Baseline (2026-02-26)

| Metric | Value |
|--------|-------|
| Total tests | ~2,700 |
| Full suite (--no-build) | **~2 seconds** |
| Build + test | **~8 seconds** |

If test time exceeds 10 seconds, investigate for hangs before assuming slowness.

---

## ⚠️ Common Mistakes

The following patterns do NOT work. Agents MUST check this section.

| NG Pattern | Correct Pattern | Reason |
|------------|-----------------|--------|
| `< NUL` | `< /dev/null` | Git Bash environment (NUL is interpreted as a file) |
| `../uEmuera/` | `../engine/` | Old path (pre-2024) |
| `uEmuera.Tests/` | `src/engine.Tests/` | Old directory name |

**Note (F220)**: Directory paths now work for `--unit`. Example: `--unit tests/ac/kojo/` expands to `*.json` automatically.
<!-- --flow tests/regression/ archived (2026-01-10) - flow tests removed during migration -->
