---
name: reference-checker
description: Feature reference and link validator. Requires sonnet model.
context: fork
agent: general-purpose
allowed-tools: Read, Glob, Grep
---

# Reference Checker Skill

Validate feature references, links, and artifact existence.

## Task

Mechanically verify that all references in a feature spec are valid and complete.

## Input

Feature ID (e.g., "329")

## Procedure

### 1. Read Feature Spec

Read `pm/features/feature-{ID}.md` fully.

### 2. Extract References

Extract all references from the feature spec:

| Source | Pattern | Example |
|--------|---------|---------|
| Links section | `[feature-{ID}.md]` | feature-319.md |
| Background/Problem | `F{ID}`, `Feature {ID}`, `feature-{ID}` | F319, Feature 319 |
| Artifact paths | File paths (`.json`, `.md`, `.ERB`, `.py`) | com_file_map.json |

### 3. Validate Feature References

For each referenced Feature ID:

1. Check Links section contains the reference
2. Verify `pm/features/feature-{ID}.md` exists (Glob)
3. If referenced in Background/Problem but missing from Links → Issue

### 4. Validate Artifact References

For each artifact path mentioned:

1. Verify file exists (Glob)
2. If file doesn't exist → Issue

### 5. Cross-Reference Check (research type only)

If feature Type is `research`:

1. For each "resolved by Feature X" or "fixed in Feature X" claim:
   - Read Feature X
   - Verify Feature X actually addresses the claimed resolution
   - If claim is unverified → Issue (CRITICAL)

## Output

```json
{
  "status": "OK | NEEDS_REVISION",
  "issues": [
    {
      "severity": "critical | major | minor",
      "type": "missing_link | missing_artifact | unverified_claim | orphan_reference",
      "location": "Links section | Background | Problem",
      "issue": "Feature F323 referenced in Problem but not in Links",
      "fix": "Add [feature-323.md](feature-323.md) to Links section"
    }
  ],
  "summary": {
    "features_referenced": 3,
    "features_in_links": 2,
    "artifacts_referenced": 1,
    "artifacts_verified": 1
  }
}
```

## Issue Severity

| Type | Severity | Description |
|------|----------|-------------|
| unverified_claim | critical | "resolved by X" claim not verified |
| missing_link | major | Feature referenced but not in Links |
| missing_artifact | major | Artifact path doesn't exist |
| orphan_reference | minor | Link exists but not referenced in body |

## STOP Conditions

- Feature file doesn't exist → STOP, report "Feature {ID} not found"
