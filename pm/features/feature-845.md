# Feature 845: Full AC pattern catalog scan across all features

## Status: [DRAFT]

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F842 |
| Discovery Phase | Phase 4 (Implementation) |
| Timestamp | 2026-03-06 |

### Observable Symptom
ac-static-verifier parser was built incrementally across F818, F832, F834, F838, F842, each addressing specific use cases. No validation exists that the parser handles the union of all AC pattern syntaxes across 275+ feature files.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | N/A (gap analysis) |
| Exit Code | N/A |
| Error Output | N/A |
| Expected | Parser validated against all existing AC patterns |
| Actual | Each feature only validated its own AC patterns |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Parser code |
| pm/features/ | 275+ feature files with AC definitions |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
Run ac-static-verifier across all 275+ features to discover any remaining pattern parsing gaps. This validates the Philosophy claim that the verifier handles "the full range of regex syntax that AC authors use." Should produce a catalog of all unique AC pattern syntaxes encountered.

## Background

### Philosophy (Mid-term Vision)
ac-static-verifier is the SSOT for automated AC verification. Incremental parser extensions must be validated against the union of all existing AC patterns to prevent regression gaps.

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
