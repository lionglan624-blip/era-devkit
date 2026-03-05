# Feature 828: Date Initialization Migration (@日付初期設定)

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

@日付初期設定 (天候.ERB:6-49) uses GOTO + INPUT + PRINTBUTTON interactive loop for date selection. This interactive pattern cannot be directly migrated to C# without UI abstraction. Single caller: 追加パッチverup.ERB:81.

### Goal (What to Achieve)

Migrate @日付初期設定 from 天候.ERB to C# with appropriate UI abstraction for interactive input handling.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F821 | [DONE] | Weather System Migration (parent feature) |

## Links

- [Predecessor: F821](feature-821.md) - Weather System Migration
