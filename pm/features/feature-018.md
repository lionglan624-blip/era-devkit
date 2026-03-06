# Feature 018: Config Options Pattern

## Status: [DONE]

## Overview

Refactor static Config class (602 lines) to use the IOptions<T> pattern for dependency injection compatibility and improved testability.

## Problem

The current `Config` class uses static properties throughout, making it difficult to:
- Mock configuration in unit tests
- Support multiple configuration profiles
- Follow DI best practices established in Features 012-017

## Goals

1. Convert static Config class to injectable IOptions<T> pattern
2. Maintain full backward compatibility with existing code
3. Enable configuration mocking in tests

## Acceptance Criteria

- [x] Config properties accessible via IConfigReader interface
- [x] Existing functionality unchanged (static facade maintains backward compatibility)
- [x] Build succeeds (22 warnings, 0 errors)
- [x] Regression tests pass (headless startup OK)
- [x] engine-reference.md updated

## Scope

### In Scope
- Config.cs refactoring to IOptions<T> pattern
- DI registration updates
- Consumer updates to use injected options

### Out of Scope
- Configuration file format changes
- New configuration options
- GlobalStatic migration (separate Feature)

## Effort Estimate

- **Size**: Medium (602 lines)
- **Risk**: 🟡 Medium - Config is widely used
- **Testability**: ★★★☆☆

## Links

- [index-features.md](index-features.md) - Feature tracking
- [feature-013.md](feature-013.md) - FontManager extraction (related pattern)
