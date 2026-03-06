# Feature 683: DialogueResult.Lines Obsolete Deprecation

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Mark DialogueResult.Lines property as [Obsolete] after all consumers migrate to DialogueLines. The backward compatibility shim (Lines computed from DialogueLines) creates permanent maintenance burden. This feature tracks the deprecation timeline after F682/F677 adopt DialogueLines.

---

## Background

F676 added a dual-property design to DialogueResult: structured `DialogueLines` (with display metadata) and backward-compatible `Lines` (text-only, computed from DialogueLines). The Lines property exists solely for backward compatibility with existing consumers (KojoComparer YamlRunner, HeadlessUI, YamlComExecutor). Once these consumers migrate to DialogueLines, Lines should be marked [Obsolete] to guide future consumers toward the structured API.

---

## Links

- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Predecessor - created dual-property design)
- [feature-677.md](feature-677.md) - KojoComparer displayMode awareness (Predecessor - KojoComparer migration)
- [feature-681.md](feature-681.md) - Multi-entry selection. Uses DialogueLines internally.
- [feature-682.md](feature-682.md) - Consumer-Side Display Mode Interpretation (Predecessor - HeadlessUI migration)
- [feature-684.md](feature-684.md) - GUI Consumer Display Mode Interpretation. May need to consider deprecation impact.
- [feature-698.md](feature-698.md) - YamlComExecutor/BatchProcessor DialogueLines Migration (Predecessor - blocking)

---

## Notes

- Created by F676 残課題 (deferred item)
- Cannot proceed until F698 is [DONE] (YamlComExecutor/BatchProcessor migrated)
- Scope: Add [Obsolete("Use DialogueLines instead")] attribute to Lines property

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: DialogueResult.Lines property creates permanent maintenance burden even though consumers should use DialogueLines
2. Why: Lines property exists as a backward-compatibility shim computed from DialogueLines (Line 32: `dialogueLines.Select(dl => dl.Text).ToList()`)
3. Why: F676 introduced the dual-property design to avoid breaking existing consumers during DisplayMode propagation migration
4. Why: Existing consumers (KojoComparer, HeadlessUI, YamlComExecutor, tests) were written before DialogueLines existed and relied on text-only Lines
5. Why: The original DialogueResult was designed as a simple record with `IReadOnlyList<string> Lines` - displayMode metadata was not part of the initial architecture, so the backward compatibility shim was necessary for non-breaking migration

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Lines property will remain in codebase indefinitely | Without explicit deprecation, future consumers have no guidance to use DialogueLines instead of Lines |
| Duplicate data storage (both DialogueLines and Lines computed on every Create() call) | Backward compatibility shim pattern stores both structured and text-only collections |
| Maintenance burden for two API surfaces | No planned deprecation timeline documented - consumers may continue using Lines indefinitely |

### Conclusion

The root cause is a **missing deprecation strategy**: F676 intentionally created the dual-property design for backward compatibility, but without explicit [Obsolete] marking, the Lines property will remain a supported API surface indefinitely. Future consumers have no compile-time guidance to prefer DialogueLines. The fix is straightforward: once all known consumers migrate to DialogueLines, mark Lines as [Obsolete] to emit compiler warnings guiding future development toward the structured API.

**Key finding from investigation**: The consumer migration status is:
- **HeadlessUI**: MIGRATED (F682 [DONE]) - Uses `dialogue.DialogueLines` directly (line 20)
- **KojoComparer/YamlRunner**: PARTIALLY MIGRATED (F677 [DONE]) - Has `RenderWithMetadata()` returning full DialogueResult, but `Render()` still uses `dialogueResult.Lines` (line 31) for backward-compatible string return
- **YamlComExecutor**: MIGRATED (F698 [DONE]) - Uses `dialogue.DialogueLines`
- **BatchProcessor**: MIGRATED (F698 [DONE]) - Uses `yamlResult.DialogueLines`
- **Test files**: 7 usages of `.Lines` in Era.Core.Tests (assertions, backward compat verification)

**Resolved**: F698 migrated YamlComExecutor and BatchProcessor to DialogueLines. F683 is no longer blocked.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F676 | [DONE] | Predecessor | Created dual-property design. F683 was created in F676 残課題. |
| F677 | [DONE] | Predecessor | KojoComparer displayMode awareness. Added RenderWithMetadata() but Render() still uses Lines. |
| F682 | [DONE] | Predecessor | HeadlessUI migrated to DialogueLines. Consumer migration complete for HeadlessUI. |
| F684 | [DONE] | Related | GUI Consumer Display Mode Interpretation. May need to consider deprecation impact. |
| F681 | [DONE] | Related | Multi-entry selection. Uses DialogueLines internally. |
| F698 | [DONE] | Predecessor | YamlComExecutor/BatchProcessor migration. Unblocked F683. |

### Pattern Analysis

This is the **cleanup phase** of the incremental metadata propagation pattern:
- F671: Added displayMode to YAML schema/converter (data layer)
- F676: Extended runtime pipeline to propagate displayMode (infrastructure layer)
- F677: Consumer awareness in KojoComparer (consumer layer - comparison)
- F682: Consumer awareness in HeadlessUI (consumer layer - rendering)
- **F683 (this)**: Deprecation of backward compatibility shim (cleanup phase)

The pattern shows systematic cleanup following incremental migration. F683 is the "close the door" feature that signals the migration is complete.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Adding [Obsolete] attribute is a simple, well-understood C# language feature |
| Scope is realistic | PARTIAL | Scope is small (~5 lines) but blocked on consumer migration. YamlComExecutor (line 229) and BatchProcessor (lines 63, 138) still use Lines. |
| No blocking constraints | NO | YamlComExecutor and BatchProcessor have NOT migrated to DialogueLines. Marking Lines as [Obsolete] now would emit warnings in production code. |

**Verdict**: NEEDS_REVISION → **RESOLVED** (F698 created)

**Resolution**: Option B selected - F698 created for YamlComExecutor/BatchProcessor migration. F683 is now [BLOCKED] until F698 completes.

**Original findings** (for reference):
1. F677 added RenderWithMetadata() but kept Render() using Lines for backward-compatible string return (acceptable - Render() is a convenience wrapper)
2. F682 migrated HeadlessUI - complete
3. YamlComExecutor uses Lines (line 229) → **Addressed by F698**
4. BatchProcessor uses Lines (lines 63, 138) → **Addressed by F698**

**Note on test files**: Test files using `.Lines` (7 usages in Era.Core.Tests) are acceptable because:
- DisplayModeTests line 227-233: Explicitly tests backward compatibility (AC#8 from F676)
- Other test files: May use Lines for assertion convenience; [Obsolete] warning in tests is informational, not blocking

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F676 | [DONE] | Created DialogueResult dual-property design with Lines backward compat |
| Predecessor | F677 | [DONE] | KojoComparer displayMode awareness - added RenderWithMetadata() |
| Predecessor | F682 | [DONE] | HeadlessUI migrated to DialogueLines |
| Predecessor | F698 | [DONE] | YamlComExecutor/BatchProcessor DialogueLines migration (blocking) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| C# [Obsolete] attribute | Language | None | Standard .NET feature, well-supported |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Current Usage | Migration Status |
|----------|--------------|---------------|------------------|
| Era.Core/HeadlessUI.cs | NONE | Uses `dialogue.DialogueLines` | MIGRATED (F682) |
| tools/KojoComparer/YamlRunner.cs | LOW | `Render()` uses `dialogueResult.Lines` for string return | ACCEPTABLE (convenience wrapper) |
| Era.Core/Commands/Com/YamlComExecutor.cs | NONE | Uses `dialogue.DialogueLines` | MIGRATED (F698) |
| tools/KojoComparer/BatchProcessor.cs | NONE | Uses `yamlResult.DialogueLines` | MIGRATED (F698) |
| Era.Core.Tests/*.cs | LOW | 7 usages for assertions | ACCEPTABLE (test code) |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Types/DialogueResult.cs | Update | Add `[Obsolete("Use DialogueLines instead. Lines will be removed in a future version.")]` attribute to Lines property (line 25) |
| Era.Core/Commands/Com/YamlComExecutor.cs | **Prerequisite** | Must migrate from `dialogue.Lines` to `dialogue.DialogueLines` before F683 |
| tools/KojoComparer/BatchProcessor.cs | **Prerequisite** | Must migrate from `yamlResult.Lines` to `yamlResult.DialogueLines` before F683 |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| [Obsolete] generates CS0618 warning at all usage sites | C# compiler | HIGH - Cannot deprecate until all production code migrates away from Lines |
| TreatWarningsAsErrors may be enabled | Build configuration | MEDIUM - Deprecation would cause build failures if TreatWarningsAsErrors is set |
| Test code using Lines will emit warnings | Era.Core.Tests | LOW - Test warnings are acceptable; tests verify backward compatibility behavior |
| YamlComExecutor.RenderKojo returns string, needs DialogueLines interpretation | Era.Core/Commands/Com/YamlComExecutor.cs | MEDIUM - Migration requires design decision on how to handle displayMode in COM results |
| BatchProcessor comparison output format | tools/KojoComparer/BatchProcessor.cs | LOW - String join format unchanged; migration is straightforward |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Premature deprecation causes build failures | Low (if prerequisites enforced) | High | Enforce F683 cannot start until YamlComExecutor/BatchProcessor migrate |
| Future consumers ignore [Obsolete] warning | Medium | Low | Use ObsoleteAttribute with error=true in future version to enforce |
| YamlComExecutor displayMode interpretation unclear | Medium | Medium | Keep RenderKojo returning plain text; displayMode interpretation is out of YamlComExecutor scope |
| Removing Lines in future version breaks binary compatibility | Low | Medium | Use semantic versioning; schedule removal for major version bump only |

---

## Philosophy

**API Lifecycle Management** - Mark deprecated APIs with clear guidance to establish migration path for future consumers. The [Obsolete] attribute serves as compile-time documentation that Lines is a backward-compatibility shim, not the preferred API. All production consumers must use DialogueLines; Lines exists only for legacy compatibility verification.

---

## Goal (What to Achieve)

1. Add [Obsolete] attribute to DialogueResult.Lines property with migration guidance message
2. Verify build succeeds without errors (warnings acceptable in test code)
3. Verify all production consumers have migrated away from Lines (no non-test .Lines usage)
4. Document deprecation in code comments for future maintainers

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "All production consumers must use DialogueLines" | No production code (non-test) uses .Lines | AC#3 |
| "Lines exists only for legacy compatibility verification" | Test code may use Lines (acceptable warnings) | AC#5 |
| "[Obsolete] attribute serves as compile-time documentation" | Obsolete attribute present with message | AC#1, AC#2 |
| "Mark deprecated APIs with clear guidance" | XML documentation explains deprecation | AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Lines property has [Obsolete] attribute | code | Grep(Era.Core/Types/DialogueResult.cs) | contains | `[Obsolete` | [x] |
| 2 | Obsolete message guides to DialogueLines | code | Grep(Era.Core/Types/DialogueResult.cs) | contains | `Use DialogueLines instead` | [x] |
| 3 | No consumer .Lines property access in Era.Core | code | Grep(Era.Core/) | not_contains | `dialogue\.Lines\|result\.Lines` | [x] |
| 4 | Era.Core build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 5 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 6 | XML doc mentions deprecation | code | Grep(Era.Core/Types/DialogueResult.cs) | contains | `backward compatibility` | [x] |
| 7 | Zero technical debt | code | Grep(Era.Core/Types/DialogueResult.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |

### AC Details

**AC#1: Lines property has [Obsolete] attribute**
- Verification: Grep pattern=`[Obsolete` path=Era.Core/Types/DialogueResult.cs
- Expected: Pattern found (attribute present on Lines property)
- Note: The [Obsolete] attribute must be on line 24-25 area (before Lines property declaration)

**AC#2: Obsolete message guides to DialogueLines**
- Verification: Grep pattern=`Use DialogueLines instead` path=Era.Core/Types/DialogueResult.cs
- Expected: Pattern found
- Note: Message should clearly indicate the preferred API (DialogueLines)

**AC#3: No consumer .Lines property access in Era.Core**
- Verification: Grep pattern=`dialogue\.Lines|result\.Lines` path=Era.Core/ type=cs
- Expected: 0 matches (no consumer code accessing .Lines property)
- Note: Pattern targets consumer access patterns (dialogue.Lines, result.Lines), not property declaration
- Rationale: F698 migrated YamlComExecutor; HeadlessUI migrated in F682. All consumers now use DialogueLines.

**AC#4: Era.Core build succeeds**
- Verification: dotnet build Era.Core
- Expected: Exit code 0
- Note: Build must succeed. Deprecation warnings in this project are acceptable but should not exist (property declaration site).

**AC#5: Era.Core.Tests build succeeds**
- Verification: dotnet build Era.Core.Tests
- Expected: Exit code 0
- Note: Test code using .Lines will emit CS0618 warnings, but build must succeed (no TreatWarningsAsErrors for deprecation).

**AC#6: XML doc mentions deprecation**
- Verification: Grep pattern=`backward compatibility` path=Era.Core/Types/DialogueResult.cs
- Expected: Pattern found in XML documentation
- Note: Existing comment "Text-only lines (backward compatibility)" already exists at line 22-23. This AC verifies documentation is preserved/enhanced.

**AC#7: Zero technical debt**
- Verification: Grep pattern=`TODO|FIXME|HACK` path=Era.Core/Types/DialogueResult.cs
- Expected: 0 matches
- Note: Implementation should not leave TODOs for future deprecation stages

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,6 | Add [Obsolete] attribute and enhance XML documentation for Lines property | [x] |
| 2 | 3 | Verify no production code uses .Lines property | [x] |
| 3 | 4,5 | Verify build succeeds for Era.Core and Era.Core.Tests | [x] |
| 4 | 7 | Verify zero technical debt in DialogueResult.cs | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Technical Design attribute/documentation specs | DialogueResult.cs updated with [Obsolete] and enhanced docs |
| 2 | ac-tester | haiku | T2-T4 | AC verification commands | Verification results |

**Constraints** (from Technical Design):

1. [Obsolete] attribute must use message: `"Use DialogueLines instead. Lines will be removed in a future version."`
2. XML documentation enhancement must preserve existing backward compatibility note
3. No functional changes - only metadata (attribute + documentation)
4. Test code warnings (CS0618) are expected and acceptable

**Pre-conditions**:

- F698 [DONE] status confirmed - YamlComExecutor and BatchProcessor migrated to DialogueLines
- F682 [DONE] status confirmed - HeadlessUI migrated to DialogueLines
- Era.Core/Types/DialogueResult.cs exists at expected location

**Success Criteria**:

1. `[Obsolete("Use DialogueLines instead. Lines will be removed in a future version.")]` attribute present on Lines property (line 24)
2. XML documentation on lines 21-23 updated to mention "DEPRECATED" and "Use DialogueLines for structured access"
3. `dotnet build Era.Core` exits with code 0
4. `dotnet build Era.Core.Tests` exits with code 0 (CS0618 warnings acceptable)
5. Grep for `dialogue\.Lines|result\.Lines` in Era.Core/ returns 0 matches (excluding DialogueResult.cs declaration)
6. Grep for `TODO|FIXME|HACK` in DialogueResult.cs returns 0 matches

**Implementation Details**:

**File**: `Era.Core/Types/DialogueResult.cs`

**Change location**: Lines 21-25

**Before**:
```csharp
    /// <summary>
    /// Text-only lines (backward compatibility).
    /// Computed from DialogueLines.
    /// </summary>
    public IReadOnlyList<string> Lines { get; }
```

**After**:
```csharp
    /// <summary>
    /// Text-only lines (backward compatibility - DEPRECATED).
    /// Computed from DialogueLines. Use DialogueLines for structured access.
    /// </summary>
    [Obsolete("Use DialogueLines instead. Lines will be removed in a future version.")]
    public IReadOnlyList<string> Lines { get; }
```

**Expected Compiler Behavior**:
- Property declaration site: No warning (attribute is metadata)
- Consumer sites using `.Lines`: CS0618 warning emitted
- Expected warnings: Era.Core.Tests (7 test usages) will emit warnings - acceptable for backward compatibility verification tests

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with reason
3. Create follow-up feature for fix with additional investigation (e.g., if TreatWarningsAsErrors causes build failures)

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| ac-static-verifier Japanese filename utf-8 decode error | F702 | PRE-EXISTING - DRAFT created |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | DEVIATION | ac-static-verifier.py --ac-type code exit 1 (utf-8 decode error in Japanese filenames) |
| 2026-01-31 | DEVIATION | doc-check NEEDS_REVISION: engine-dev/SKILL.md Lines property not marked deprecated |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Add the `[Obsolete]` attribute with a clear migration message to the `Lines` property in `DialogueResult.cs`. This is a minimal code change (single attribute addition) that provides compile-time guidance for future consumers while maintaining full backward compatibility.

**Implementation strategy**:
1. Add `[Obsolete("Use DialogueLines instead. Lines will be removed in a future version.")]` attribute on line 24 (before the Lines property declaration)
2. Enhance existing XML documentation to explicitly mention deprecation rationale
3. Verify no regressions by running existing tests (which may emit warnings but must pass)

**Why this approach satisfies the ACs**:
- AC#1, AC#2: Direct attribute addition with migration guidance message
- AC#3: F698 prerequisite ensures no production usage exists before deprecation
- AC#4, AC#5: Minimal change reduces risk; existing tests verify backward compatibility still works
- AC#6: Enhanced XML documentation provides context for future maintainers
- AC#7: No workarounds or TODOs needed - straightforward deprecation pattern

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `[Obsolete(...)]` attribute on line 24, immediately before `public IReadOnlyList<string> Lines { get; }` |
| 2 | Attribute message: `"Use DialogueLines instead. Lines will be removed in a future version."` |
| 3 | Prerequisite verification: F698 [DONE] status ensures YamlComExecutor migrated. HeadlessUI already migrated (F682). Grep verification confirms zero production usage. |
| 4 | Run `dotnet build Era.Core` - should succeed. Property declaration site emits no warning (deprecation applies to consumers). |
| 5 | Run `dotnet build Era.Core.Tests` - should succeed. Test code using `.Lines` will emit CS0618 warnings (expected and acceptable). |
| 6 | Update XML documentation on line 21-23 to: `/// <summary>\n/// Text-only lines (backward compatibility - DEPRECATED).\n/// Computed from DialogueLines. Use DialogueLines for structured access.\n/// </summary>` |
| 7 | Grep `TODO\|FIXME\|HACK` in DialogueResult.cs - should return 0 matches. Implementation requires no deferred work. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Obsolete message content | A) "Deprecated, use DialogueLines"<br>B) "Use DialogueLines instead. Lines will be removed in a future version."<br>C) "This property is obsolete." | B | Provides clear migration path ("Use DialogueLines") and signals future removal timeline without committing to specific version |
| ObsoleteAttribute error flag | A) `[Obsolete(message, error: false)]` (warning)<br>B) `[Obsolete(message, error: true)]` (compile error) | A (implicit default) | Warning-level deprecation allows gradual migration; error-level would break test code unnecessarily |
| XML documentation update scope | A) Minimal edit (add "DEPRECATED" keyword)<br>B) Comprehensive rewrite with migration examples | A | Concise deprecation notice matches existing documentation style. Attribute message provides migration guidance. |
| Prerequisite enforcement | A) Add [Obsolete] now, accept warnings in YamlComExecutor<br>B) Wait for F698 completion | B | Zero-warning principle: production code should not emit deprecation warnings. F698 migration must complete first. |

### Implementation Details

**File**: `Era.Core/Types/DialogueResult.cs`

**Change location**: Lines 21-25

**Before**:
```csharp
    /// <summary>
    /// Text-only lines (backward compatibility).
    /// Computed from DialogueLines.
    /// </summary>
    public IReadOnlyList<string> Lines { get; }
```

**After**:
```csharp
    /// <summary>
    /// Text-only lines (backward compatibility - DEPRECATED).
    /// Computed from DialogueLines. Use DialogueLines for structured access.
    /// </summary>
    [Obsolete("Use DialogueLines instead. Lines will be removed in a future version.")]
    public IReadOnlyList<string> Lines { get; }
```

**Compiler behavior**:
- Property declaration site: No warning (attribute is metadata)
- Consumer sites using `.Lines`: CS0618 warning emitted
- Expected warnings: Era.Core.Tests (7 test usages) will emit warnings - acceptable for backward compatibility verification tests

**Migration timeline** (for future reference):
1. Current feature (F683): Mark as [Obsolete] with `error: false` (warning)
2. Future major version: Change to `error: true` (compile error)
3. Later major version: Remove Lines property entirely

---
