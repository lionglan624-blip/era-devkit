# Feature 019: GlobalStatic → DI

## Status: [DONE]

## Overview

Migrate GlobalStatic class from static state holder to dependency-injected services. This is a high-impact refactoring that touches 35 files with 188 usage sites.

## Problem

`GlobalStatic.cs` (61 lines) holds global mutable state accessed statically throughout the codebase:
- Creates tight coupling between components
- Makes unit testing extremely difficult (state bleeds between tests)
- Prevents parallel test execution
- Hidden dependencies obscure component requirements

## Goals

1. Convert GlobalStatic fields to injectable services
2. Enable unit testing with isolated state
3. Maintain backward compatibility during migration
4. Improve code maintainability and testability

## Acceptance Criteria

- [x] GlobalStatic state accessible via DI (IProcess extended with GetScaningLine, scaningLine, SkipPrint, MethodStack)
- [x] Existing code continues to work (phased migration - 20 usages migrated)
- [x] At least one consuming class migrated to DI (7 files migrated)
- [x] Build succeeds
- [x] Regression tests pass
- [x] Documentation updated (engine-reference.md)

## Scope

### In Scope
- Analysis of GlobalStatic dependencies (35 files, 188 sites)
- Design of replacement service interfaces
- Phased migration strategy
- Initial implementation (first phase only)

### Out of Scope
- Complete migration of all 188 usage sites (future phases)
- Changes to ERB game logic
- Save format changes

## Effort Estimate

- **Size**: Large (61行 source, 188箇所 usage across 35 files)
- **Risk**: 🔴 Very High - Core state management, requires careful phased approach
- **Testability**: ★★★★★ - Maximum impact on testability

## Technical Notes

From Wishlist analysis:
- Phased migration required (cannot change all 188 sites at once)
- Need facade/adapter pattern during transition
- Consider service locator as intermediate step

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
