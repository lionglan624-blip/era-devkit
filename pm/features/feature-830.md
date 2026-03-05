# Feature 830: Trigger-Gated Shared Utility Extractions (BulkResetCharacterFlags & IsDoutei)

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

## Type: engine

## Background

### Problem (Current Issue)
Two shared utility methods were inlined during Phase 22 migration because only one C# call site existed at the time. Extraction into shared interfaces is deferred until a second C# call site appears, confirming the pattern.

**OB-06 (CVARSET BulkReset)**: IVariableStore.BulkResetCharacterFlags was inlined by F824 (Sleep & Menstrual). Extraction needed when MOVEMENT.ERB migrates (second call site). Source: F824 via F826 Mandatory Handoffs.

**OB-07 (IS_DOUTEI)**: ICharacterUtilities.IsDoutei has 22 ERB call sites across 14 files. F824 inlined 1 call site. Extraction triggered when second C# call site appears. Source: F824 via F826 Mandatory Handoffs.

### Goal (What to Achieve)
Extract BulkResetCharacterFlags into IVariableStore shared method and IsDoutei into ICharacterUtilities shared method when their respective trigger conditions are met.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F824 | [DONE] | Sleep & Menstrual; original source of both obligations |
| Related | F829 | [WIP] | Phase 22 Deferred Obligations Consolidation; routing origin |

## Links
- [Related: F824](feature-824.md) - Sleep & Menstrual (original source)
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (routing origin)
