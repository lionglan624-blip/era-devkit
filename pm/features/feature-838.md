# Feature 838: Test Infrastructure Fixes — Cross-Repo Verifier and Engine Test Isolation

## Status: [DRAFT]

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F833 |
| Discovery Phase | Phase 4 (test suite), Phase 9 (static verifier) |
| Timestamp | 2026-03-06 |

### Observable Symptom
Two test infrastructure issues discovered during F833 /run:
1. ac-static-verifier.py fails for cross-repo features (paths relative to engine/core repos not resolvable from devkit root)
2. Engine test suite has 9 PRE-EXISTING test isolation failures when running full suite (ProcessLevelParallelRunnerTests: 5, VariableDataAccessorTests: 4) — all pass in isolation

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python ac-static-verifier.py --feature 833 --ac-type code` |
| Exit Code | 1 |
| Error Output | `File not found: engine/Assets/Scripts/Emuera/Services/EngineVariablesImpl.cs` |
| Expected | Cross-repo paths resolved correctly |
| Actual | Paths resolved relative to devkit root only |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Needs cross-repo path resolution support |
| engine/tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs | Test isolation failures |
| engine/tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs | Test isolation failures (GlobalStatic shared state) |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| --repo-root /c/Era | FAIL | Feature file not found (expects pm/features/ under repo-root) |

### Parent Session Observations
Cross-repo features (engine type modifying engine/ and core/ repos) cannot use ac-static-verifier for automated verification. Manual ac-tester dispatch works but doesn't generate JSON logs for verify-logs.py aggregation. Engine test isolation issues are GlobalStatic shared state between test collections — needs [Collection] attributes or IDisposable cleanup.

## Background

### Philosophy (Mid-term Vision)
Test infrastructure must support cross-repo feature verification without manual workarounds.

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->
- [pending] F833 Phase 9: DEVIATION handoff — test isolation (9 failures) and ac-static-verifier cross-repo path resolution confirmed as F838 obligations

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
