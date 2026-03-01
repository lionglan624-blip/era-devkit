# Feature 017: ParserMediator Instance-Based

## Status: [DONE]

## Overview

Convert ParserMediator from a static class to an injectable service, enabling proper dependency injection and testability.

## Problem

ParserMediator is currently a static class with 175 lines of code containing parsing utilities and warning collection. This creates:
- Global state that's difficult to test in isolation
- Tight coupling between components
- Inability to mock/substitute the parser mediator in unit tests
- Conflicts with modern DI patterns established in recent features

Feature 016 (LexicalAnalyzer Split) has been completed, which was a prerequisite for this refactoring.

## Goals

1. Convert ParserMediator from static class to instance-based service
2. Extract interface (IParserMediator) for dependency injection
3. Maintain backward compatibility during transition
4. Improve testability of parsing-related code

## Acceptance Criteria

- [x] IParserMediator interface extracted with all public methods
- [x] ParserMediator converted to instance-based implementation
- [x] DI registration added to service configuration (GlobalStatic.ParserMediatorInstance)
- [x] All call sites updated to use injected instance (via static facade)
- [x] Build succeeds (0 errors)
- [x] Regression tests pass (58 unit tests, headless startup OK)
- [x] engine-reference.md updated with new interface

## Scope

### In Scope
- ParserMediator.cs conversion to instance-based
- IParserMediator interface creation
- DI container registration
- Update call sites (analyze impact first)
- Documentation update

### Out of Scope
- Other static class conversions (Config, GlobalStatic)
- Additional refactoring beyond ParserMediator
- New functionality additions

## Effort Estimate

- **Size**: Medium (175 lines source, call site updates)
- **Risk**: 🟡 Medium - Core parsing component, requires careful testing
- **Testability**: ★★★☆☆

## Technical Notes

- Depends on: Feature 016 (LexicalAnalyzer Split) ✓ Complete
- WarningCollector service (Feature 012) may have overlap - review integration
- Consider thread safety requirements

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [feature-016.md](feature-016.md) - Prerequisite: LexicalAnalyzer Split
- [feature-012.md](feature-012.md) - Related: WarningCollector Service
- [engine-reference.md](../reference/engine-reference.md) - Architecture documentation
