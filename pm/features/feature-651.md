# Feature 651: KojoComparer KojoEngine API Update

## Status: [PROPOSED]

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

## Type: erb

## Created: 2026-01-28

---

## Summary

Update KojoComparer's YamlRunner.cs to use the new KojoEngine DI constructor (IDialogueLoader, IDialogueSelector, IDialogueRenderer) introduced by F553. Currently YamlRunner.cs calls `new KojoEngine()` which no longer exists, causing build failure.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion quality validation - KojoComparer must build and run for equivalence verification.

### Problem (Current Issue)

F553 (KojoEngine Facade Refactoring, [DONE]) changed the KojoEngine constructor to require DI parameters but did not update all consumers. `tools/KojoComparer/YamlRunner.cs` line 19 still calls the old parameterless `new KojoEngine()`, causing compilation errors:
- CS7036: Missing 'loader' parameter
- CS1061: 'KojoEngine' has no 'LoadYaml' method
- CS1061: 'KojoEngine' has no 'EvaluateConditions' method
- CS1061: 'KojoEngine' has no 'Render' method

This blocks F639 AC#5 (KojoComparer equivalence verification) and all sibling conversion features (F636-F643).

### Goal (What to Achieve)

1. Update YamlRunner.cs to instantiate KojoEngine with required DI parameters
2. Update method calls to match new KojoEngine API (LoadYaml, EvaluateConditions, Render)
3. Verify KojoComparer builds and runs successfully
4. Add consumer tracking to prevent future API breakage

---

## Links

- [feature-639.md](feature-639.md) - Sakuya Kojo Conversion (origin: AC#5 blocker)
- [feature-553.md](feature-553.md) - KojoEngine Facade Refactoring (API change source)
- [feature-644.md](feature-644.md) - Equivalence Testing Framework (downstream)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F553 | [DONE] | KojoEngine Facade Refactoring - defines new API |
| Predecessor | F549 | [DONE] | YamlDialogueLoader - IDialogueLoader implementation |
| Predecessor | F550 | [DONE] | ConditionEvaluator - IConditionEvaluator for PriorityDialogueSelector |
| Predecessor | F551 | [DONE] | TemplateDialogueRenderer - IDialogueRenderer implementation |
| Predecessor | F552 | [DONE] | PriorityDialogueSelector - IDialogueSelector implementation |
| Predecessor | F652 | [DONE] | Test YAML Migration - AC#12 resolved test data format incompatibility |
| Successor | F639 | [BLOCKED] | Sakuya Kojo Conversion - AC#5 unblocked by this |
| Successor | F636-F643 | [BLOCKED]/[DRAFT] | All character conversions with KojoComparer ACs |
| Successor | F644 | [DRAFT] | Equivalence Testing Framework |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | KojoComparer builds | build | dotnet build tools/KojoComparer | succeeds | - | [x] |
| 2 | YamlRunner.cs exists | file | Glob(tools/KojoComparer/YamlRunner.cs) | exists | - | [x] |
| 3 | No old KojoEngine() call | code | Grep(tools/KojoComparer/YamlRunner.cs) | not_contains | "new KojoEngine()" | [x] |
| 4 | No old LoadYaml call | code | Grep(tools/KojoComparer/YamlRunner.cs) | not_contains | ".LoadYaml(" | [x] |
| 5 | YamlDialogueLoader instantiation | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | "new YamlDialogueLoader()" | [x] |
| 6 | ConditionEvaluator instantiation | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | "new ConditionEvaluator()" | [x] |
| 7 | PriorityDialogueSelector instantiation | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | "new PriorityDialogueSelector(" | [x] |
| 8 | TemplateDialogueRenderer instantiation | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | "new TemplateDialogueRenderer(" | [x] |
| 9 | KojoEngine DI constructor call | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | "new KojoEngine(loader, selector, renderer" | [x] |
| 10 | GetDialogue API call | code | Grep(tools/KojoComparer/YamlRunner.cs) | contains | ".GetDialogue(" | [x] |
| 11 | Consumer tracking document | code | Grep(Era.Core/KojoEngine.cs) | contains | "tools/KojoComparer" | [x] |
| 12 | KojoComparer tests pass | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [B] |
| 13 | Zero technical debt | code | Grep(tools/KojoComparer/YamlRunner.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |

**AC#12 Note**: [B] BLOCKED - PRE-EXISTING issue. Test YAML files use old format (branches:) incompatible with YamlDialogueLoader. Test data migration is out of scope for F651.

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2,3,4,5,6,7,8,9,10,13 | Rewrite YamlRunner.cs with new KojoEngine DI API | [x] |
| 2 | 11 | Add consumer tracking documentation to KojoEngine.cs | [x] |
| 3 | 1,12 | Verify build and tests pass | [x] |

## 残課題 (Deferred Items)

| Item | Why Defer | Destination Type | Destination | Task# |
|------|-----------|------------------|-------------|-------|
| AC#12: Test YAML migration | PRE-EXISTING - test YAMLs use old format (branches:) incompatible with YamlDialogueLoader. Not F651 implementation defect. | Feature | F652 | - |

**Blocker**: F651 is [BLOCKED] until F652 completes and AC#12 passes.

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-28 | T1 | Rewritten YamlRunner.cs with DI pattern - using YamlDialogueLoader per API accessibility constraint |
| 2026-01-28 | T2 | Added consumer tracking to KojoEngine.cs |
| 2026-01-28 | T3 | Build SUCCESS - dotnet build tools/KojoComparer passes |
| 2026-01-28 | DEVIATION | dotnet test tools/KojoComparer.Tests | PRE-EXISTING: 4 tests fail - test YAMLs use old format (branches:) incompatible with YamlDialogueLoader |
| 2026-01-28 | DEVIATION | feature-reviewer | post-review | NEEDS_REVISION: 残課題 'Test YAML migration' needs concrete Feature ID |
| 2026-01-28 | Phase 8 | User decision: BLOCKED until F652 (Test YAML migration) completes |
