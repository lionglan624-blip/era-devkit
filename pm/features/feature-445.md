# Feature 445: C# 14 Documentation

## Status: [DONE]

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

## Type: infra

## Created: 2026-01-11

---

## Summary

Create C# 14 skill documentation and update Type Design Guidelines to incorporate C# 14 patterns (primary constructors, extension members, etc.). Add cross-reference from engine-dev skill to csharp-14 skill for feature implementation guidance.

**Output**: New skill file `.claude/skills/csharp-14/SKILL.md` + updated Type Design Guidelines in architecture.md + engine-dev skill reference + CLAUDE.md skills table update.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 10: Runtime Upgrade** - Document C# 14 features as SSOT reference for Phase 12+ implementation. This ensures consistent usage of C# 14 patterns across DDD domain models, COM implementations, and engine features.

### Problem (Current Issue)

Post-F444 runtime upgrade, the codebase lacks documentation for C# 14 features:
- No skill reference for C# 14 syntax and patterns
- Type Design Guidelines still reference C# 12 patterns
- engine-dev skill lacks pointer to C# 14 documentation

### Goal (What to Achieve)

1. **Create csharp-14 skill** - Document C# 14 features relevant to ERA development
2. **Update Type Design Guidelines** - Incorporate C# 14 patterns into architecture.md
3. **Add engine-dev reference** - Link engine-dev skill to csharp-14 skill

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/skills/csharp-14/SKILL.md | New skill file | Agents can reference C# 14 patterns |
| Game/agents/designs/full-csharp-architecture.md | Section added | Type Design Guidelines extended with C# 14 patterns |
| .claude/skills/engine-dev/SKILL.md | Reference added | Engine developers guided to C# 14 skill |
| CLAUDE.md | Skills table updated | New skill available for loading |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | csharp-14/SKILL.md exists | file | Glob | exists | .claude/skills/csharp-14/SKILL.md | [x] |
| 2 | Skill has YAML frontmatter | file | Grep | contains | "name: csharp-14" | [x] |
| 3 | Skill description present | file | Grep | contains | "description:.*C# 14 features" | [x] |
| 4 | Primary constructors documented | file | Grep | contains | "Primary Constructors" | [x] |
| 5 | Extension members documented | file | Grep | contains | "Extension Members" | [x] |
| 6 | Collection expressions documented | file | Grep | contains | "Collection Expressions" | [x] |
| 7 | Type Design Guidelines updated | file | Grep | contains | "C# 14 Patterns" | [x] |
| 8 | Primary constructor pattern shown | file | Grep | contains | "Primary Constructor.*DI" | [x] |
| 9 | engine-dev skill reference added | file | Grep | contains | "csharp-14" | [x] |
| 10 | CLAUDE.md skills table updated | file | Grep | contains | "csharp-14.*C# 14 features" | [x] |
| 11 | Linked files exist | file | Glob | exists | .claude/skills/engine-dev/SKILL.md | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: csharp-14 skill file creation
- Test: Glob pattern=.claude/skills/csharp-14/SKILL.md
- Expected: File exists at specified path

**AC#2**: YAML frontmatter format
- Test: Grep pattern="name: csharp-14" path=".claude/skills/csharp-14/SKILL.md"
- Per skill format requirements: YAML frontmatter with `name` field

**AC#3**: Skill description
- Test: Grep pattern="description:.*C# 14 features" path=".claude/skills/csharp-14/SKILL.md"
- Per skill format requirements: description field (max 1024 chars) includes when to use

**AC#4-6**: C# 14 features documented
- Primary Constructors: Class parameter lists for dependency injection
- Extension Members: Extension methods for existing types
- Collection Expressions: `[..]` syntax for collections
- Test: Grep patterns in csharp-14.md

**AC#7**: Type Design Guidelines updated
- Test: Grep pattern="C# 14 Patterns" path="Game/agents/designs/full-csharp-architecture.md"
- Expected: Section added to architecture.md Type Design Guidelines

**AC#8**: Primary constructor pattern example
- Test: Grep pattern="Primary Constructor.*DI" path="Game/agents/designs/full-csharp-architecture.md"
- Expected: Code example showing primary constructor with DI (dependency injection) pattern

**AC#9**: engine-dev skill reference
- Test: Grep pattern="csharp-14" path=".claude/skills/engine-dev/SKILL.md"
- Expected: Cross-reference to csharp-14 skill added

**AC#10**: CLAUDE.md skills table update
- Test: Grep pattern="csharp-14.*C# 14 features" path="CLAUDE.md"
- Expected: Skills table row with csharp-14 skill and description

**AC#11**: Linked files validation
- Test: Glob pattern=".claude/skills/engine-dev/SKILL.md" (referenced from csharp-14)
- Expected: File exists (validates internal links)

**AC#12**: Technical debt check
- Test: Grep pattern="TODO|FIXME|HACK" (unescaped alternation for ripgrep) paths=".claude/skills/csharp-14/SKILL.md, Game/agents/designs/full-csharp-architecture.md (C# 14 section only), .claude/skills/engine-dev/SKILL.md (updated section only)"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Create csharp-14/SKILL.md with YAML frontmatter | [x] |
| 2 | 4,5,6 | Document C# 14 features (Primary Constructors, Extension Members, Collection Expressions) | [x] |
| 3 | 7,8 | Update Type Design Guidelines in architecture.md with C# 14 patterns | [x] |
| 4 | 9 | Add csharp-14 reference to engine-dev skill | [x] |
| 5 | 10 | Update CLAUDE.md skills table with csharp-14 entry | [x] |
| 6 | 11 | Verify linked files exist | [x] |
| 7 | 12 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 12 ACs = 7 Tasks (skill creation batched, see waiver below) -->

**Batch Task Waiver (Task 1)**: Following F384 precedent for related file creation (frontmatter + basic structure in same edit).

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### csharp-14/SKILL.md Skill Structure

```markdown
---
name: csharp-14
description: C# 14 features reference. Use when implementing engine features with .NET 10, writing DDD domain models, or creating COM implementations that leverage modern C# patterns.
---

# C# 14 Features Reference

> **Purpose**: Document C# 14 features relevant to ERA game development
> **Runtime**: .NET 10+
> **Last Updated**: 2026-01-11

## When to Use This Skill

- Implementing engine features (Era.Core, uEmuera.Headless)
- Writing DDD domain models (Phase 12+)
- Creating COM implementations
- Refactoring legacy code to modern patterns

---

## Primary Constructors

[Documentation content here]

---

## Extension Members

[Documentation content here]

---

## Collection Expressions

[Documentation content here]

---

## Type Design Guidelines Integration

See [full-csharp-architecture.md](../../../Game/agents/designs/full-csharp-architecture.md) for integration with existing patterns.

---

## Links

- [engine-dev SKILL](../engine-dev/SKILL.md) - Engine development reference
- [full-csharp-architecture.md](../../../Game/agents/designs/full-csharp-architecture.md) - Architecture design
```

### Type Design Guidelines Update Location

**File**: `Game/agents/designs/full-csharp-architecture.md`

**Section**: Add new "C# 14 Patterns" section after existing Type Design Guidelines

**Content**: Examples of primary constructors with Result<T>, extension members for domain models, collection expressions for initialization

### engine-dev Skill Update

**File**: `.claude/skills/engine-dev/SKILL.md`

**Location**: Add to "When to Use This Skill" or "Links" section

**Content**:
```markdown
## C# Language Features

For C# 14 specific patterns (primary constructors, extension members, collection expressions), see:
- [csharp-14 skill](../csharp-14/SKILL.md)
```

### CLAUDE.md Update

**File**: `CLAUDE.md`

**Location**: Skills table

**Content**: Add row for csharp-14 skill:
```markdown
| `csharp-14` | C# 14 features reference (primary constructors, extension members, collection expressions) |
```

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | implementer | csharp-14 skill structure | csharp-14/SKILL.md created |
| 2 | implementer | C# 14 feature docs | csharp-14/SKILL.md content |
| 3 | implementer | architecture.md Type Design Guidelines | C# 14 patterns added |
| 4 | implementer | engine-dev SKILL.md | csharp-14 reference added |
| 5 | implementer | CLAUDE.md Skills table | csharp-14 row added |
| 6 | doc-reviewer | All modified docs | SSOT + link validation |

### Rollback Plan

If issues arise after deployment:
1. Delete `.claude/skills/csharp-14/SKILL.md`
2. Revert C# 14 section in `Game/agents/designs/full-csharp-architecture.md`
3. Remove csharp-14 reference from `.claude/skills/engine-dev/SKILL.md`
4. Remove csharp-14 row from `CLAUDE.md` Skills table
5. Create follow-up feature for fix

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F444 | .NET 10 / C# 14 Core Upgrade provides runtime |
| Successor | F446 | Phase 10 Post-Phase Review (verifies this feature) |

---

## Links

- [feature-437.md](feature-437.md) - Phase 10 Planning (parent feature)
- [feature-444.md](feature-444.md) - .NET 10 / C# 14 Core Upgrade
- [feature-446.md](feature-446.md) - Phase 10 Post-Phase Review
- [feature-384.md](feature-384.md) - Batch Task Waiver precedent
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 10 definition
- [csharp-14/SKILL.md](../../.claude/skills/csharp-14/SKILL.md) - C# 14 skill (to be created)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-11 FL iter2**: [resolved] Phase2-Validate - AC#1 Glob pattern: Leading dot in path works (verified in iter6)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 | create | implementer | Created from F437 Phase 10 Planning | PROPOSED |
| 2026-01-11 | START | initializer | Initialize Feature 445 | READY |
| 2026-01-11 | END | implementer | Tasks 1-5 implementation | SUCCESS |
| 2026-01-11 | END | ac-tester | AC 1-12 verification | PASS:12/12 |
| 2026-01-11 | END | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-11 | END | feature-reviewer | Doc-check (mode: doc-check) | READY |
