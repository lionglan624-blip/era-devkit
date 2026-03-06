# Feature 844: _UNESCAPE_RULES catch-all regex refactoring

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
`_UNESCAPE_RULES` in ac-static-verifier.py grows by adding individual entries for each regex character class (e.g., `\\s`->`\s`, `\\S`->`\S`). F842 added 9 rules, bringing total to 17. This per-class enumeration approach does not scale and risks missing new character classes.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | N/A (design observation) |
| Exit Code | N/A |
| Error Output | N/A |
| Expected | Generic regex pattern handles all character class unescaping |
| Actual | 17 individual tuple entries in `_UNESCAPE_RULES` |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/ac-static-verifier.py | `_UNESCAPE_RULES` at lines ~285-310 |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| None | - | - |

### Parent Session Observations
Replace per-class entries with generic regex `r'\\\\([sSdDwWbBAZ])' → r'\\1'` to handle all single-character escape sequences uniformly. Must preserve existing behavior for multi-character escapes like `\\[`, `\\]`, `\\"`.

## Background

### Philosophy (Mid-term Vision)
ac-static-verifier is the SSOT for automated AC verification. Its unescape pipeline should use scalable patterns rather than enumerated rules.

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
