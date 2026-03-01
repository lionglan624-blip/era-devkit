---
name: dependency-analyzer
description: Dependency analysis and risk identification agent. Model: sonnet
context: fork
agent: general-purpose
allowed-tools: Read, Glob, Grep
---

# Dependency Analyzer Skill

Identifies dependencies and technical risks for version goals.

## Input

- Version number (e.g., v2.0, v2.1)
- Version goal list (goal-setter output)
- `Game/ERB/` - ERB code
- `engine/` - C# engine code
- `Game/data/` - YAML data definitions

## Output

Report format (Japanese):

```
=== Version {X.Y} Dependency Analysis ===

## Code Investigation Results

### ERB Investigation
| File | Dependencies | Impact Scope |
|------|--------------|--------------|
| {file} | {dependencies} | {impact} |

### Engine Investigation
| Module | Dependencies | Impact Scope |
|--------|--------------|--------------|
| {module} | {dependencies} | {impact} |

## Dependency Graph

```
Feature A
  ├─ requires: Feature B (ERB dependency)
  └─ requires: Engine Module X (C# dependency)
```

## Technical Risks

| Risk | Impact | Mitigation |
|------|--------|-----------|
| {risk} | {severity} | {mitigation} |

## Kojo Pattern Analysis (for kojo features)

Existing kojo investigation:
- {COM number}: {pattern summary}
- Branch patterns: {TALENT usage}
- Average lines: {lines}
```

## Decision Criteria

### Identifying Dependencies
- ERB: CALL, CALLFORM, RETURN, variable references
- C#: Class dependencies, interface implementations
- CSV: Data definition dependencies

### Risk Assessment
- **High**: Implementation impossible, major changes required
- **Medium**: Workarounds available, additional investigation needed
- **Low**: Minor impact, can be addressed with existing patterns

### Kojo Pattern Analysis
- Investigate branch count and line count of existing COMs
- Identify TALENT dependency patterns
- Estimate line count for new COMs

## Procedure

1. Read goal-setter output for Feature list
2. For each Feature:
   - Identify target files (ERB/C#/CSV)
   - Use Grep to find dependencies
   - Analyze dependency chain
3. Create dependency graph
4. Identify technical risks
5. For kojo Features: Analyze existing kojo patterns
6. Output report
