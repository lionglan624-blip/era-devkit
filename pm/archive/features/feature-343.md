# Feature 343: Full C#/Unity Migration Architecture

## Status: [DONE]

## Type: research

## Created: 2026-01-04

---

## Summary

Technical architecture design for complete migration from ERB/uEmuera to C#/Unity with YAML-based kojo content. Eliminates bridge layer approach in favor of direct migration to reduce long-term technical debt.

---

## Background

### Philosophy (Mid-term Vision)

**Zero Technical Debt Architecture**: Build the final target architecture directly, avoiding intermediate compatibility layers that become legacy burden. Prioritize long-term maintainability, extensibility, and single-stack simplicity over short-term migration convenience.

### Problem (Current Issue)

Feature 341 proposed Hybrid Migration (Option D) with ERB↔C# bridge layer:

| Issue | Impact |
|-------|--------|
| Bridge layer (CALLCS/ErbBridge) | Code written for migration, discarded after completion |
| Phase 2 bidirectional sync | Complex state management during transition |
| Two-system maintenance | ERB + C# parallel operation extends maintenance burden |
| Mixed codebase | "Is this ERB or C#?" cognitive overhead |

### Goal (What to Achieve)

Design complete C#/Unity architecture that:
1. Eliminates ERB entirely (no bridge layer)
2. Uses YAML for all kojo content (declarative, validatable)
3. Implements all logic in C# (testable, type-safe)
4. Provides Unity-based UI (extensible, modern)
5. Defines clear migration path from current system

---

## Design Document

**Output**: [designs/full-csharp-architecture.md](designs/full-csharp-architecture.md)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Design document created | file | Glob | exists | designs/full-csharp-architecture.md | [x] |
| 2 | Architecture layers defined | file | Grep | contains | "## Architecture Layers" | [x] |
| 3 | YAML kojo schema defined | file | Grep | contains | "dialogue_entry:" | [x] |
| 4 | C# game engine design | file | Grep | contains | "GameEngine" | [x] |
| 5 | Unity UI layer design | file | Grep | contains | "UnityUI" | [x] |
| 6 | Migration phases defined | file | Grep | contains | "## Migration Path" | [x] |
| 7 | No bridge layer references | file | Grep | not_contains | "CALLCS" | [x] |
| 8 | POC plan defined | file | Grep | contains | "## POC Plan" | [x] |
| 9 | No TODO markers remain | file | Grep | not_contains | "TODO:" | [x] |
| 10 | Document finalized | file | Grep | not_contains | "**Status**: DRAFT" | [x] |

**Target file for AC#2-10**: `designs/full-csharp-architecture.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create design document skeleton | [x] |
| 2 | 2 | Design architecture layers (Content/Logic/UI separation) | [x] |
| 3 | 3 | Design YAML kojo schema (structure, conditions, validation) | [x] |
| 4 | 4 | Design C# game engine (GameLoop, StateManager, EventSystem) | [x] |
| 5 | 5 | Design Unity UI layer (TextRenderer, InputHandler, SceneManager) | [x] |
| 6 | 6 | Define migration phases (ERB→C#, uEmuera→Unity) | [x] |
| 7 | 7 | Verify no bridge layer references in design | [x] |
| 8 | 8 | Plan POC (minimal viable game loop) | [x] |
| 9 | 9 | Remove all TODO markers from design document | [x] |
| 10 | 10 | Finalize document (change Status from DRAFT to FINAL) | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F341 | Research input: Options analysis (supersedes Option D recommendation) |
| Successor | F344+ | Implementation based on this design |

---

## Links

- [feature-341.md](feature-341.md) - ERB System Architecture Research
- [designs/full-csharp-architecture.md](designs/full-csharp-architecture.md) - Design document
- [content-roadmap.md](content-roadmap.md) - Version roadmap

---

## Progress Log

| Date | Phase | Notes |
|------|-------|-------|
| 2026-01-04 | Created | Initial proposal with bridge approach |
| 2026-01-04 | Revised | Changed to full C#/Unity migration (no bridge layer) based on technical debt analysis |
| 2026-01-04 | Completed | Design document finalized with FINAL status |
