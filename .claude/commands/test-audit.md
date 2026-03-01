---
description: Audit test quality (coverage, mutation, assertion density, flaky, dead tests, etc.)
argument-hint: "[target]"
---

**Language**: Thinking in English, respond to user in Japanese.

---

Audit test quality for the specified target. Collects coverage, mutation scores, and quality metrics (assertion density, dead tests, test-to-code ratio, slow tests, flaky detection). Auto-fix low coverage by dispatching subagents to write missing tests.

## Arguments

**TARGET**: `$ARGUMENTS`

| Format | Example | Scope |
|--------|---------|-------|
| (empty) | `/test-audit` | All test projects |
| keyword | `/test-audit dashboard` | Mapped directories (see table below) |
| path | `/test-audit src/tools/dotnet/ErbParser` | Specific directory (auto-detect type) |

### Keyword → Directory Mapping

| Keyword | Directories | Type |
|---------|-------------|------|
| `dashboard` | `src/tools/node/feature-dashboard/backend/`, `src/tools/node/feature-dashboard/frontend/` | JS |
| `core` | `src/Era.Core/` + `src/Era.Core.Tests/` | C# |
| `engine` | `engine/` + `src/engine.Tests/` | C# |
| `erb-parser` | `src/tools/dotnet/ErbParser/` + `src/tools/dotnet/ErbParser.Tests/` | C# |
| `erb-to-yaml` | `src/tools/dotnet/ErbToYaml/` + `src/tools/dotnet/ErbToYaml.Tests/` | C# |
| `kojo-comparer` | `src/tools/dotnet/KojoComparer/` + `src/tools/dotnet/KojoComparer.Tests/` | C# |
| `yaml-validator` | `src/tools/dotnet/YamlValidator/` + `src/tools/dotnet/YamlValidator.Tests/` | C# |
| `yaml-schema-gen` | `src/tools/dotnet/YamlSchemaGen/` + `src/tools/dotnet/YamlSchemaGen.Tests/` | C# |
| `save-analyzer` | `src/tools/dotnet/SaveAnalyzer/` + `src/tools/dotnet/SaveAnalyzer.Tests/` | C# |
| `kojo-quality` | `src/tools/dotnet/KojoQualityValidator/` + `src/tools/dotnet/KojoQualityValidator.Tests/` | C# |
| `entries-migrator` | `src/tools/dotnet/EntriesFormatMigrator/` + `src/tools/dotnet/EntriesFormatMigrator.Tests/` | C# |
| `talent-migrator` | `src/tools/dotnet/YamlTalentMigrator/` + `src/tools/dotnet/YamlTalentMigrator.Tests/` | C# |

**Path fallback**: If `$ARGUMENTS` is not a keyword, treat as directory path. Auto-detect type by checking for `*.csproj` (C#) or `package.json` (JS).

---

## Phase 1: Target Resolution

1. Parse `$ARGUMENTS` to determine targets
2. If empty, build full target list:
   - **C#**: `src/Era.Core.Tests/`, all `src/tools/dotnet/*Tests/` directories (Glob for `src/tools/dotnet/*Tests/*.csproj`)
   - **JS**: `src/tools/node/feature-dashboard/backend/`, `src/tools/node/feature-dashboard/frontend/`
3. If keyword, use mapping table above
4. If path, verify directory exists, detect type
5. Report resolved targets before proceeding

---

## Phase 2: Coverage Collection

### C# Projects

For each C# test project:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet test "{project.csproj}" --no-build --results-directory "_out/tmp/test-audit" /p:CollectCoverage=true /p:CoverageOutputFormat=cobertura /p:CoverageOutput="_out/tmp/test-audit/{project-name}.cobertura.xml"'
```

**Pre-step**: Run `dotnet build` via WSL once for all targets before collecting coverage.

**Parse cobertura XML**: Read the generated XML files. Extract per-file `line-rate` and `branch-rate` attributes. Calculate project-level averages.

### JS Projects

For each JS project directory:

```bash
# Backend
cd src/tools/node/feature-dashboard/backend && npx vitest run --coverage 2>&1

# Frontend
cd src/tools/node/feature-dashboard/frontend && npx vitest run --coverage 2>&1
```

**Parse output**: Vitest prints a coverage table to stdout. Extract per-file line/branch/function percentages from the text output.

---

## Phase 3: Mutation Testing

### JS Backend (Stryker configured)

```bash
cd src/tools/node/feature-dashboard/backend && npx stryker run 2>&1
```

**Parse output**: Extract mutation score from Stryker clear-text report. Note killed/survived/timeout counts.

### C# Projects

Check if `dotnet-stryker` is installed:
```bash
dotnet tool list -g | grep -i stryker
```

- **If installed**: Run `dotnet stryker --project "{source-project.csproj}" --test-project "{test-project.csproj}"` for each target
- **If not installed**: Report `[INFO] C# mutation testing not available. Install: dotnet tool install -g dotnet-stryker`

### JS Frontend

Report `[INFO] Frontend mutation testing not configured. Add @stryker-mutator/core to devDependencies.`

---

## Phase 4: Quality Metrics

Collect test quality indicators beyond coverage and mutation. All metrics are computed via static analysis (Grep/Read) and test execution output.

### 4a: Assertion Density

Count assertions per test method. Detects "assertionless tests" (tests that exercise code but verify nothing).

**C# (xUnit)**:
```bash
# Count test methods
grep -rcE '\[(Fact|Theory)' {test_project_dir}/ --include="*.cs" | awk -F: '{sum+=$2} END {print sum}'

# Count assertions
grep -rcE 'Assert\.' {test_project_dir}/ --include="*.cs" | awk -F: '{sum+=$2} END {print sum}'

# Density = assertion_count / test_method_count
```

**JS (Vitest)**:
```bash
# Count test functions
grep -rcE '\b(test|it)\(' {project_dir}/src/ --include="*.test.*" | awk -F: '{sum+=$2} END {print sum}'

# Count expectations
grep -rcE 'expect\(' {project_dir}/src/ --include="*.test.*" | awk -F: '{sum+=$2} END {print sum}'
```

**Assertionless Test Detection** (critical — these are false-positive tests):
```bash
# C#: Find [Fact]/[Theory] methods with no Assert.* call in the method body
# Parse each test file: for each [Fact]/[Theory], scan until next method/class boundary
# Flag methods where Assert count == 0
```

Report each assertionless test by file:line.

### 4b: Dead Test Detection

Count tests that are skipped, disabled, or conditionally excluded.

**C# (xUnit)**:
```bash
# Skipped tests
grep -rnE '\[(Fact|Theory)\(Skip\s*=' {test_project_dir}/ --include="*.cs"

# Pragma-disabled (extremely rare but check)
grep -rnE '#if\s+false' {test_project_dir}/ --include="*.cs"
```

**JS (Vitest)**:
```bash
# Skipped tests
grep -rnE '\b(test|it|describe)\.skip\(' {project_dir}/src/ --include="*.test.*"

# TODO tests
grep -rnE '\b(test|it)\.todo\(' {project_dir}/src/ --include="*.test.*"
```

Calculate: `dead_rate = skipped_count / total_test_count * 100`

### 4c: Test-to-Code Ratio

Compare lines of code between source and test projects.

**C# Projects**:
```bash
# Source LOC (excluding blank lines and comments)
find {source_dir}/ -name "*.cs" | xargs wc -l | tail -1

# Test LOC
find {test_dir}/ -name "*.cs" | xargs wc -l | tail -1

# Ratio = test_LOC / source_LOC
```

**JS Projects**:
```bash
# Source LOC (*.js/*.ts, exclude node_modules, *.test.*, *.spec.*)
find {project_dir}/src/ -name "*.js" -o -name "*.ts" | grep -v test | grep -v spec | xargs wc -l | tail -1

# Test LOC
find {project_dir}/src/ -name "*.test.*" -o -name "*.spec.*" | xargs wc -l | tail -1
```

### 4d: Test Execution Time

Identify slow tests that degrade CI/development feedback loops.

**C# (xUnit)**:
```bash
# Run with TRX logging (duration per test is in TRX XML)
dotnet test "{project.csproj}" --no-build --logger "trx;LogFileName=timing.trx" --results-directory "_out/tmp/test-audit"
```

Parse TRX XML: extract `duration` attribute from each `<UnitTestResult>`. Flag tests > 1000ms as SLOW, > 5000ms as VERY_SLOW.

**JS (Vitest)**:
```bash
# Vitest prints per-test timing in verbose mode
cd {project_dir} && npx vitest run --reporter=verbose 2>&1
```

Parse stdout for timing info. Flag tests > 1000ms.

### 4e: Flaky Test Detection (optional)

Detects non-deterministic tests by running the suite multiple times.

**Trigger**: Only run when `$ARGUMENTS` includes `--flaky` flag, or when full audit (`/test-audit` with no args).

**Procedure**:
```bash
# Run tests 3 times, collect results
for i in 1 2 3; do
  dotnet test "{project.csproj}" --no-build --logger "trx;LogFileName=run-${i}.trx" --results-directory "_out/tmp/test-audit/flaky"
done
```

Compare TRX results across runs. Any test that changes outcome (PASS↔FAIL) between runs is flagged as FLAKY.

**JS**: `npx vitest run --retry 3 --reporter=verbose 2>&1` — Vitest natively reports retried tests.

**Performance note**: Triples test execution time. Skip for targeted audits unless explicitly requested.

### 4f: Boundary Value Coverage (heuristic)

Heuristic check: do tests reference boundary values?

```bash
# C#: Check for common boundary patterns in test files
grep -rcE '(int\.MaxValue|int\.MinValue|long\.MaxValue|\.Empty|null|""|\b0\b|\b-1\b|\.Length\s*-\s*1|byte\.MaxValue)' {test_project_dir}/ --include="*.cs"
```

Report count per project. Low count relative to test count suggests boundary testing gaps. This is a **heuristic only** — not a definitive metric.

### 4g: Test Independence Check (optional)

Verify tests don't depend on execution order.

**C# (xUnit v3)**: xUnit v3 randomizes test order by default. Check for test failures when running with explicit randomization:
```bash
dotnet test "{project.csproj}" --no-build -- xUnit.Execution.DisableParallelization=false
```

**Trigger**: Same as flaky detection — only on `--independence` flag or full audit.

Compare results with default (parallel) vs sequential (`-- xUnit.Execution.DisableParallelization=true`). Differences indicate order-dependent tests.

---

## Phase 5: Report

Display results in structured format:

```
╔═══════════════════════════════════════════════════════════════════╗
║                      Test Audit Report                            ║
╚═══════════════════════════════════════════════════════════════════╝

═══ Coverage Summary ═══

| Project | Line % | Branch % | Mutation % | Status |
|---------|-------:|---------:|-----------:|--------|
| Era.Core | 72.3% | 58.1% | - | MODERATE |
| engine | 45.2% | 33.0% | - | LOW |
| dashboard/backend | 81.5% | 74.2% | 68.3% | GOOD |
| dashboard/frontend | 55.0% | 42.1% | - | LOW |
| ErbParser | 88.0% | 81.2% | - | GOOD |

═══ Quality Metrics ═══

| Project | Assert Density | Dead Tests | T:C Ratio | Slow (>1s) | Assertionless | Status |
|---------|---------------:|-----------:|----------:|-----------:|--------------:|--------|
| Era.Core | 2.4 | 0 (0.0%) | 1.2 | 1 | 0 | GOOD |
| engine | 1.1 | 3 (4.2%) | 0.4 | 5 | 2 | LOW |
| dashboard/backend | 3.1 | 0 (0.0%) | 0.8 | 0 | 0 | GOOD |
| dashboard/frontend | 1.8 | 1 (2.0%) | 0.6 | 0 | 1 | MODERATE |
| ErbParser | 2.0 | 0 (0.0%) | 1.5 | 0 | 0 | GOOD |

═══ Low Coverage Files (< 60%) ═══

| File | Line % | Branch % |
|------|-------:|---------:|
| src/Era.Core/Commands/Foo.cs | 23.1% | 10.0% |
| engine/Sub/Bar.cs | 41.0% | 25.0% |

═══ Assertionless Tests (false-positive risk) ═══

| File:Line | Test Method | Framework |
|-----------|-------------|-----------|
| src/engine.Tests/FooTests.cs:42 | Should_Process_Input | xUnit |
| src/engine.Tests/BarTests.cs:88 | Constructor_Works | xUnit |

═══ Dead Tests (skipped/disabled) ═══

| File:Line | Test Method | Reason |
|-----------|-------------|--------|
| src/engine.Tests/OldTests.cs:15 | Legacy_Format_Parse | Skip="Pending migration" |
| src/engine.Tests/OldTests.cs:30 | Legacy_Encoding | Skip="Pending migration" |

═══ Slow Tests (> 1000ms) ═══

| File | Test Method | Duration |
|------|-------------|----------|
| src/Era.Core.Tests/IntegrationTests.cs:55 | FullPipeline_Renders | 2340ms |

═══ Flaky Tests (if --flaky) ═══

| File | Test Method | Run 1 | Run 2 | Run 3 |
|------|-------------|:-----:|:-----:|:-----:|
| (none detected) | | | | |

═══ Mutation Report ═══

| Project | Killed | Survived | Timeout | Score | Status |
|---------|-------:|---------:|--------:|------:|--------|
| dashboard/backend | 45 | 12 | 3 | 75.0% | MODERATE |

═══════════════════════════════════════════════════════════════════
Summary: X projects, coverage Y LOW, quality Z LOW, W assertionless, V dead
═══════════════════════════════════════════════════════════════════
```

### Coverage Thresholds

| Range | Status | Action |
|-------|--------|--------|
| >= 80% | GOOD | No action needed |
| 60-79% | MODERATE | Warning only |
| < 60% | LOW | Auto-fix triggered (Phase 6) |

### Quality Metric Thresholds

| Metric | GOOD | MODERATE | LOW |
|--------|------|----------|-----|
| **Assertion Density** | >= 2.0 | 1.0-1.9 | < 1.0 |
| **Dead Test Rate** | 0% | 1-5% | > 5% |
| **Test-to-Code Ratio** | >= 1.0 | 0.5-0.9 | < 0.5 |
| **Slow Tests** | 0 | 1-5 | > 5 |
| **Assertionless Tests** | 0 | 1-3 | > 3 |
| **Flaky Tests** | 0 | - | >= 1 (any flaky = LOW) |

### Overall Quality Status

Per-project status is determined by the **worst** metric:
- All GOOD → GOOD
- Any MODERATE (none LOW) → MODERATE
- Any LOW → LOW

---

## Phase 6: Auto-Fix

Auto-fix addresses two categories: **low coverage** and **quality issues**.

### 6a: Coverage Fix

**Trigger**: Any file with line coverage < 60%.

**Scope limit**: Fix at most **5 files** per run (ordered by lowest coverage first).

**Skip conditions**:
- Files in `src/tools/dotnet/_archived/` — skip, report as `[SKIP] archived`
- Generated files (TestData/, migrations) — skip
- Files with 0 lines of coverable code — skip

**Fix Procedure** — For each low-coverage file:

1. **Read** the source file to understand its logic
2. **Read** existing tests (if any) in the corresponding test project
3. **Dispatch** subagent to write missing tests:

```
Task(subagent_type: "general-purpose", model: "sonnet", prompt: "
You are a test writer. Write unit tests for the following source file to increase its coverage.

Source file: {path}
Content:
{file_content}

Existing tests (if any): {existing_test_content}

Current coverage: {line_pct}% line, {branch_pct}% branch

Requirements:
- Use the same test framework as existing tests (xUnit for C#, Vitest for JS)
- Follow existing test patterns in this project
- Focus on untested branches and edge cases
- Write the tests to {test_file_path}
- Tests MUST compile and pass
- Do NOT modify the source file — only write/edit test files

After writing tests, run them:
- C#: dotnet test {test_project.csproj}
- JS: cd {project_dir} && npx vitest run

If tests fail, fix them. STOP after 3 consecutive failures.
")
```

4. **Verify** new tests pass after subagent completes
5. **Report** coverage improvement: `[FIX] {file} → {old}% → {new}%`

### 6b: Assertionless Test Fix

**Trigger**: Any test method with zero assertions detected in Phase 4a.

**Scope limit**: Fix at most **10 assertionless tests** per run.

**Fix Procedure** — For each assertionless test:

1. **Read** the test file and identify the assertionless method
2. **Read** the source file being tested to understand expected behavior
3. **Dispatch** subagent:

```
Task(subagent_type: "general-purpose", model: "sonnet", prompt: "
You are a test quality fixer. The following test method has NO assertions and is a false-positive risk.

Test file: {test_file_path}
Test method: {method_name} (line {line_number})
Source under test: {source_file_content}

Add meaningful assertions to this test method. The test should verify:
- Return values
- State changes
- Expected exceptions (if applicable)

Requirements:
- Add Assert calls that verify the actual behavior
- Do NOT add trivial assertions (Assert.True(true))
- Keep the existing test setup/arrange logic
- Tests MUST compile and pass after modification

Run tests after fixing: dotnet test {test_project.csproj}
")
```

### 6c: Dead Test Cleanup (report only)

Dead tests are **not auto-deleted**. Instead, report recommendations:

```
[DEAD] src/engine.Tests/OldTests.cs:15 → Skip="Pending migration" (since 2025-11-20)
  Recommendation: Remove or re-enable. Skipped > 90 days.
[DEAD] src/engine.Tests/OldTests.cs:30 → Skip="Pending migration" (since 2025-11-20)
  Recommendation: Remove or re-enable. Skipped > 90 days.
```

### Fix Failure Handling

- If subagent test fails 3 times → `[FAIL] Could not fix {file}. Manual attention needed.`
- Do NOT modify source code to make tests pass
- Do NOT skip test verification

---

## Output Summary

After all phases complete, print final summary:

```
═══ Auto-Fix Results ═══

── Coverage ──
[FIX] src/Era.Core/Foo.cs → 23.1% → 67.2% (+44.1%)
[FIX] engine/Bar.cs → 41.0% → 72.0% (+31.0%)
[SKIP] src/tools/dotnet/_archived/Baz.cs (archived)
[FAIL] src/Era.Core/Qux.cs (test compilation error)

── Assertionless ──
[FIX] src/engine.Tests/FooTests.cs:42 → Added 2 assertions
[FIX] src/engine.Tests/BarTests.cs:88 → Added 1 assertion

── Dead Tests ──
[DEAD] src/engine.Tests/OldTests.cs:15 → Remove or re-enable (skipped > 90 days)
[DEAD] src/engine.Tests/OldTests.cs:30 → Remove or re-enable (skipped > 90 days)

── Flaky ──
(not detected / not run)

═══════════════════════════════════════════════════════════════════
Total: X audited, Y coverage-fixed, Z assertion-fixed, W dead, V flaky
═══════════════════════════════════════════════════════════════════
```
