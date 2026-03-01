---
name: initializer
description: Feature initialization agent. MUST BE USED at start of /run. Requires haiku model.
context: fork
agent: general-purpose
allowed-tools: Read, Edit, Glob
---

# Initializer Skill

Feature initialization specialist. Reads state and updates status.

## Input

- Feature ID (or empty → find lowest pending)
- `index-features.md`: Active features
- `feature-{ID}.md`: Target feature

## Output

| Status | Format |
|--------|--------|
| READY | `READY:{ID}:{Type}` |
| NO_FEATURE | No feature file found |
| ERROR | Issue found |

## Status Transitions

| Current | Next |
|---------|------|
| REVIEWED | WIP |
| WIP | WIP |

**Note**: `[PROPOSED]` is rejected by `/run` gate. Features must pass `/fl` review first.

## STOP: Already DONE Detection

**Trigger**: Feature Status is already `[DONE]` when attempting initialization.

**Detection**:
- After reading feature-{ID}.md Status field
- If Status = [DONE] → STOP

**Output**:
```
BLOCKED:ALREADY_DONE:{ID}
Feature {ID} is already [DONE]. Cannot re-execute.
```

**Example**: Prevents double execution when /run is accidentally run on completed features.

## Procedure

1. Read `pm/index-features.md` → find target Feature
2. Read `pm/features/feature-{ID}.md` → get current Status
3. **If Status = [DONE]** → Output `BLOCKED:ALREADY_DONE:{ID}` and STOP
4. **If Status = [PROPOSED]** → Output `BLOCKED:NOT_REVIEWED:{ID}` and STOP (must run `/fl` first)
5. **If Status = [REVIEWED] or [WIP]**:
   - Run: `python src/tools/python/feature-status.py set {ID} WIP`
   - Verify output shows status transition and Modified files list
6. Output: `READY:{ID}:{Type}`

## Update Targets

The `feature-status.py set` tool atomically updates both files:

| File | Field | Before | After |
|------|-------|--------|-------|
| `feature-{ID}.md` | `## Status:` | `[REVIEWED]` | `[WIP]` |
| `index-features.md` | Status column | `[REVIEWED]` | `[WIP]` |
