# Feature 818: ac-static-verifier Cross-Repo and WSL Support

## Status: [DRAFT]

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

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F813 |
| Discovery Phase | Phase 9 |
| Timestamp | 2026-03-04 |

### Observable Symptom
ac-static-verifier.py fails for cross-repo features: (1) code type ACs with paths in C:\Era\core are rejected as "not in subpath of C:\Era\devkit", (2) file type AC Glob patterns for core repo paths return "not found", (3) build type ACs run native dotnet instead of WSL, failing with NU1301 NuGet source errors.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 813 --ac-type code` |
| Exit Code | 1 |
| Error Output | `Error: 'C:\Era\core\src\Era.Core\Counter\CounterMessage.cs' is not in the subpath of 'C:\Era\devkit'` |
| Expected | Cross-repo Grep paths resolve correctly |
| Actual | Verifier rejects paths outside devkit root |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Main verifier script - path validation too strict |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | PRE-EXISTING limitation discovered during F813 verification |

### Parent Session Observations
F813 is the first cross-repo infra feature to run ac-static-verifier. Previous features were either devkit-only or used manual ac-tester verification. The verifier needs: (1) repo-root override parameter or multi-repo config, (2) WSL dotnet execution for build ACs (matching pre-commit hook pattern), (3) cross-repo Glob/Grep support.

## Background

### Philosophy (Mid-term Vision)
Pipeline Continuity -- ac-static-verifier is part of the AC verification pipeline. Cross-repo features must have the same automated verification coverage as single-repo features.

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
