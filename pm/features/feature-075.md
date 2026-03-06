# Feature 075: Documentation Restructure

## Status: [DONE]

## Background

- **Original problem**: Documentation responsibility unclear, agents.md overloaded, kojo docs scattered across 3 files
- **Solution**: Task-oriented rules/, consolidated reference docs, archive old features
- **Trigger**: /doc-audit analysis

## Overview

Restructure documentation following single responsibility principle:
- rules/ = "What to do now" (task-oriented)
- reference/ = "Deep dive when needed"
- archive/ = "Historical records"

## Scope

### In Scope
1. Archive old feature/WBS files (001-050)
2. Consolidate kojo docs → kojo-reference.md
3. Consolidate testing docs → testing-reference.md
4. Consolidate engine docs → engine-reference.md
5. Lighten agents.md (workflow → rules/)
6. Reorganize rules/ for task-oriented structure
7. Update CLAUDE.md

### Out of Scope
- Content changes (only reorganization)
- New documentation creation

## Acceptance Criteria

- [x] archive/ directory created with feature-001~050, WBS-001~050
- [x] reference/ directory created
- [x] kojo-reference.md consolidates kojo-index + guidelines + characters
- [x] testing-reference.md consolidates testing + scenarios
- [x] engine-reference.md consolidates specification + interfaces
- [x] agents.md lightened (links to rules/ and reference/)
- [x] rules/ restructured for task-oriented access
- [x] CLAUDE.md Document Map updated
- [x] All internal links updated

## New Structure

```
Game/agents/
├── agents.md              # Lightweight: principles + links
├── index-features.md      # Active only
├── index-features-history.md
├── reference/
│   ├── erb-reference.md
│   ├── kojo-reference.md
│   ├── testing-reference.md
│   └── engine-reference.md
├── active/                # Current work (optional, may keep flat)
└── archive/
    ├── feature-001~050.md
    └── WBS-001~050.md

.claude/rules/
├── erb-syntax.md
├── kojo-implement.md
├── engine-modify.md
├── feature-workflow.md
├── testing-run.md
└── docs-edit.md
```

## Links

- [agents.md](agents.md)
- [CLAUDE.md](../../CLAUDE.md)
