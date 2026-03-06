# Feature 842: ac-static-verifier Pattern Parsing Enhancements

## Status: [DRAFT]

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F841 |
| Discovery Phase | Phase 7 (Verification) |
| Timestamp | 2026-03-06 |

### Observable Symptom
ac-static-verifier fails (exit 1) on 4 valid code-type ACs due to pattern parsing limitations: multiline=true parameter ignored, escaped quotes mangled, pipe alternation not supported with gte matcher.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code` |
| Exit Code | 1 |
| Error Output | `11/15 passed, 0 manual` |
| Expected | 15/15 passed (all patterns match) |
| Actual | AC#1,5,14,16 FAIL due to pattern parsing |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | Verifier code — pattern extraction and matching logic |
| pm/features/feature-841.md | AC definitions that triggered the failures |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| Manual Grep verification | PASS | Confirms implementation correct; verifier limitation |

### Parent Session Observations
Three distinct pattern parsing gaps identified: (1) `multiline=true` parameter in Grep() Method column is ignored — verifier uses single-line matching only, causing `[\s\S]*` patterns to fail. (2) Escaped quotes in pattern strings (e.g., `pattern="\"cd \" in build_command"`) are mangled during AC table parsing, extracting just `\` as the pattern. (3) `\|` pipe escaping used as regex alternation in patterns with gte matcher is treated as literal text, finding 0 matches instead of alternating.

## Background

### Philosophy (Mid-term Vision)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
