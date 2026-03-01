---
name: goal-setter
description: Version goal concretization agent. Model: haiku
context: fork
agent: general-purpose
allowed-tools: Read, Edit, Glob
---

# Goal Setter Skill

Concretizes version goals and creates feature-level breakdown proposals.

## Input

- Version number (e.g., v2.0, v2.1)
- `dev/planning/content-roadmap.md` - Version plan
- `dev/planning/index-features-history.md` - Completion history

## Output

Report format (Japanese):

```
=== Version {X.Y} Goal Setting ===

## Version Goals
{Goals extracted from content-roadmap}

## Feature Breakdown Proposal

### Kojo Features (Type: kojo)
| Feature Candidate | COM Range | Characters | Est. Lines | Priority |
|-------------------|:---------:|:----------:|:----------:|:--------:|
| COM_XX-XX {name} | 2 COM | All 10 | 400-600 | ★★★ |

### System Features (Type: erb/engine)
| Feature Candidate | Description | Est. ACs | Priority |
|-------------------|-------------|:--------:|:--------:|
| {feature name} | {description} | 8-12 | ★★ |

## Total Estimate
- Kojo Features: {N} features
- System Features: {N} features
- Total ACs: {N} ACs
```

## Decision Criteria

### Kojo Feature Granularity
- 1 Feature = 1-2 COM × 10 characters
- AC estimate: 10-12 (1 AC per character)
- Line estimate: COM count × 10 characters × branch count × average lines

### System Feature Granularity
- 1 Feature = 1 logical function
- AC estimate: 8-15
- 1 AC = Minimum unit for 1 task

### Priority Judgment
- ★★★: Dependency for other features, blocker resolution
- ★★: Normal priority
- ★: Low priority, deferrable

## Procedure

1. Read `content-roadmap.md` for Version {X.Y} goals
2. Read `index-features-history.md` for completed work
3. Analyze gap: what needs to be done
4. Break down goals into Feature-sized chunks
5. Kojo: Group COMs (1-2 COM per Feature)
6. System: Group logical functions (AC 8-15 per Feature)
7. Estimate scope (lines for kojo, AC count for system)
8. Output report
