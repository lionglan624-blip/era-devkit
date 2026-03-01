# Feature 076: Legacy Documentation Consolidation

## Status: [DONE]

## Background

- **Original problem**: Feature 075 created consolidated reference/ documents but left legacy files in pm/ for backward compatibility
- **Current state**: 9 legacy files (3,814 lines) still exist with 60+ cross-references
- **Solution**: Complete consolidation by merging unique content and updating all references

## Overview

Complete the documentation consolidation started in Feature 075 by:
1. Merging unique content from legacy files into reference/ documents
2. Updating all cross-references across the codebase
3. Deleting legacy files

## Problem

Dual documentation creates:
- Maintenance burden (updates needed in 2 places)
- Confusion about authoritative source
- Inconsistent links
- Increased context loading for Claude

## Legacy Files Analysis

| Legacy File | Lines | Target | Consolidation Strategy |
|-------------|------:|--------|------------------------|
| kojo-guidelines.md | 773 | kojo-reference.md | Merge character details, AC sections |
| kojo-index.md | 336 | kojo-reference.md | Merge directory structure, triggers |
| kojo-characters.md | 234 | kojo-reference.md | Merge character stats |
| testing.md | 326 | testing-reference.md | Merge strategy sections |
| testing-scenarios.md | 350 | testing-reference.md | Merge scenario examples |
| kojo-test-reference.md | 540 | testing-reference.md | Merge technical details |
| uEmuera-specification.md | 192 | engine-reference.md | Merge architecture overview |
| uEmuera-interfaces.md | 839 | engine-reference.md | Merge interface docs |
| erb-reference.md | 224 | reference/erb-reference.md | Delete (duplicate) |
| **Total** | **3,814** | - | - |

## Goals

1. Single-source documentation in reference/
2. All references updated to reference/ paths
3. Legacy files deleted
4. Zero broken links

## Scope

### In Scope
- Content comparison and merge for all 9 legacy files
- Reference update across all .md files
- Legacy file deletion
- Link validation

### Out of Scope
- New documentation content
- Tool changes

## Acceptance Criteria

- [x] kojo-reference.md contains all unique content from kojo-*.md files
- [x] testing-reference.md contains all unique content from testing*.md files
- [x] engine-reference.md contains all unique content from uEmuera-*.md files
- [x] All 60+ references updated to reference/ paths
- [x] All 9 legacy files deleted
- [x] /doc-audit reports 0 broken links (verified via grep)
- [x] Build succeeds

## Technical Approach

### Phase 1: Content Audit
For each legacy file:
1. Diff with consolidated version
2. Identify unique sections not in consolidated
3. Mark sections for merge

### Phase 2: Content Merge
1. Add unique sections to consolidated files
2. Maintain consistent formatting
3. Update internal links within consolidated files

### Phase 3: Reference Update
Search and replace patterns:
```
kojo-guidelines.md ↁEreference/kojo-reference.md
kojo-index.md ↁEreference/kojo-reference.md
kojo-characters.md ↁEreference/kojo-reference.md
testing.md ↁEreference/testing-reference.md
testing-scenarios.md ↁEreference/testing-reference.md
kojo-test-reference.md ↁEreference/testing-reference.md
uEmuera-specification.md ↁEreference/engine-reference.md
uEmuera-interfaces.md ↁEreference/engine-reference.md
erb-reference.md ↁEreference/erb-reference.md
```

### Phase 4: Cleanup
1. Delete legacy files
2. Run /doc-audit
3. Verify no broken links

## Reference Update Locations

From audit results, references exist in:
- `.claude/commands/*.md`
- `tools/*/README.md`
- `pm/features/feature-*.md`
- `pm/WBS-*.md`
- `pm/content-roadmap.md`
- `pm/archive/*.md`

## Effort Estimate

- **Size**: Medium (documentation-only, no code)
- **Risk**: Low (can be reverted via git)
- **Sessions**: 1-2

## Dependencies

- Feature 075 [DONE] - Created reference/ structure

## Links

- [feature-075.md](feature-075.md) - Documentation Restructure (predecessor)
- [reference/kojo-reference.md](../reference/kojo-reference.md) - Kojo consolidated
- [reference/testing-reference.md](../reference/testing-reference.md) - Testing consolidated
- [reference/engine-reference.md](../reference/engine-reference.md) - Engine consolidated
