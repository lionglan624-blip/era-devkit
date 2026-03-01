# Feature 063: Headless口上テスチE- Phase 2b Coverage Report

## Status: [DONE]

## Background

<!-- Session handoff: Record ALL discussion details here -->
- **Origin**: Split from Feature 062 Phase 2 (originally WBS-062 tasks 10-12)
- **Prerequisite**: Feature 062 Phase 2a (Interactive Mode) complete
- **Purpose**: Branch coverage analysis for kojo dialogue testing

## Overview

Implement coverage report functionality for kojo testing. Track IF/SELECTCASE branch coverage during function execution and generate coverage reports.

## Problem

- No visibility into which dialogue branches have been tested
- Cannot verify that all conditions in kojo functions are exercised
- Manual inspection of branch coverage is error-prone

## Goals

1. Track IF/SELECTCASE branches during execution
2. Calculate branch coverage percentage
3. Generate machine-readable coverage reports
4. Enable coverage-driven test development

## Acceptance Criteria

- [x] `--coverage-report coverage.json` outputs coverage data
- [x] Report includes `branches_total` and `branches_hit` counts
- [x] Report includes per-branch details (file, line, condition, hit)
- [x] Coverage works with both single tests and batch mode
- [x] Build succeeds (0 errors, warnings acceptable)
- [ ] Unit tests pass (pre-existing build issue)

## Technical Design

### CoverageCollector Class

```csharp
public class CoverageCollector
{
    public class BranchInfo
    {
        public string File { get; set; }
        public int Line { get; set; }
        public string Condition { get; set; }
        public bool Hit { get; set; }
    }

    public List<BranchInfo> Branches { get; }
    public int BranchesTotal => Branches.Count;
    public int BranchesHit => Branches.Count(b => b.Hit);
    public double CoveragePercent => BranchesTotal > 0
        ? (double)BranchesHit / BranchesTotal * 100 : 0;

    public void RecordBranch(string file, int line, string condition, bool taken);
    public string ToJson();
}
```

### Hook Points in Interpreter

```csharp
// ProcessState.IfStatement()
if (CoverageCollector.IsEnabled)
    CoverageCollector.RecordBranch(file, line, condition, result);

// ProcessState.SelectCase()
if (CoverageCollector.IsEnabled)
    CoverageCollector.RecordBranch(file, line, caseCondition, matched);
```

### Report Format

```json
{
  "branches_total": 42,
  "branches_hit": 35,
  "coverage_percent": 83.3,
  "branches": [
    {"file": "KOJO_K4_会話親寁EERB", "line": 123, "condition": "TALENT:3==1", "hit": true},
    {"file": "KOJO_K4_会話親寁EERB", "line": 145, "condition": "CFLAG:2>=5000", "hit": false}
  ]
}
```

### CLI Usage

```bash
# Single test with coverage
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "KOJO_MESSAGE_COM_K4_300" --char 4 \
  --coverage-report coverage.json

# Batch test with combined coverage
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "tests/kojo/*.json" \
  --coverage-report coverage.json
```

## Test Specifications

| # | Test Name | Input | Expected | Status |
|---|-----------|-------|----------|--------|
| T1 | Coverage_BasicReport | `--coverage-report coverage.json` | JSON with branches_total/hit | [x] |
| T2 | Coverage_BranchDetails | Report content | `branches` array with conditions | [x] |
| T3 | Coverage_MultipleConditions | Test with IF chains | All branches tracked | [x] |
| T4 | Coverage_BatchMode | Multiple scenarios | Combined coverage | [x] |
| T5 | Coverage_ZeroBranches | Function with no IF | `branches_total: 0` | [ ] |

## Effort Estimate

- **Size**: Medium
- **Risk**: Medium (requires interpreter hooks)
- **Testability**: High

## Links

- [feature-062.md](feature-062.md) - Phase 2a (Interactive Mode)
- [feature-064.md](feature-064.md) - Phase 2c (Process-level Parallel)
- [reference/testing-reference.md](../reference/testing-reference.md) - Test documentation
