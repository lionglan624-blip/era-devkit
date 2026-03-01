# Feature 448: xUnit v3 Migration

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

## Type: engine

## Created: 2026-01-11

---

## Summary

Phase 11 xUnit v3 Migration: Upgrade all 6 active test projects from xUnit v2 (2.9.x) to xUnit v3 (3.2.x) with breaking changes handling. Ensures .NET 10 optimization and MTP v2 compatibility. (ErbLinter.Tests excluded - archived, .NET 8.0)

**Philosophy Inheritance**: Phase 11: xUnit v3 Migration

---

## Background

### Philosophy (Mid-term Vision)

**Phase 11: xUnit v3 Migration** - Establish xUnit v3 as the single source of truth for test infrastructure across all active test projects, ensuring .NET 10 optimization, MTP v2 compatibility, and simplified future maintenance of test dependencies.

### Problem (Current Issue)

Current test infrastructure uses xUnit v2 (2.9.x), which predates .NET 10 optimizations:
- xUnit v3 includes .NET 10-specific enhancements
- API breaking changes require systematic migration
- Runner compatibility needs verification
- MTP v2 support must be validated

### Goal (What to Achieve)

1. **Package Migration** - All 6 active test projects use xUnit v3 packages
2. **API Update** - Handle breaking changes in test code
3. **Verification** - All tests pass with xUnit v3
4. **MTP v2 Compatibility** - Confirm compatibility with Microsoft Test Platform v2

### Impact Analysis

| Target | Change Type | Description |
|--------|-------------|-------------|
| 6 active test projects | Package references | `xunit` → `xunit.v3`, runner updated to 3.x |
| Test code | API updates | Handle breaking changes per migration guide |

**Excluded**: ErbLinter.Tests (tools/_archived/, .NET 8.0) - out of scope

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Era.Core.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 2 | uEmuera.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 3 | ErbParser.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 4 | ErbToYaml.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 5 | KojoComparer.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 6 | YamlSchemaGen.Tests uses xunit.v3 | file | Grep | contains | "PackageReference.*xunit\\.v3" | [x] |
| 7 | Runner upgraded to 3.x | file | Grep | contains | "xunit\\.runner\\.visualstudio.*Version=\\"3\\." | [x] |
| 8 | All projects build | build | dotnet build | succeeds | - | [x] |
| 9 | All tests pass | test | dotnet test | succeeds | - | [x] |
| 10 | Migration documented | file | Grep | contains | "xUnit v3 Migration Notes:" | [x] |

### AC Details

**AC#1**: Era.Core.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="Era.Core.Tests/Era.Core.Tests.csproj"
- Expected: Match found

**AC#2**: uEmuera.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="engine.Tests/uEmuera.Tests.csproj"
- Expected: Match found

**AC#3**: ErbParser.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="tools/ErbParser.Tests/ErbParser.Tests.csproj"
- Expected: Match found

**AC#4**: ErbToYaml.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="tools/ErbToYaml.Tests/ErbToYaml.Tests.csproj"
- Expected: Match found

**AC#5**: KojoComparer.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="tools/KojoComparer.Tests/KojoComparer.Tests.csproj"
- Expected: Match found

**AC#6**: YamlSchemaGen.Tests uses xunit.v3
- Test: Grep pattern="PackageReference.*xunit\\.v3" path="tools/YamlSchemaGen.Tests/YamlSchemaGen.Tests.csproj"
- Expected: Match found

**AC#7**: Runner upgraded to 3.x
- Test: Grep pattern="xunit\\.runner\\.visualstudio.*Version=\"3\\." path="Era.Core.Tests/Era.Core.Tests.csproj"
- Expected: Match found (representative spot check)
- Note: Package name is `xunit.runner.visualstudio` (unchanged from v2), version 3.x indicates v3 compatibility
- Note: All 6 projects updated atomically in Phase 2; spot check on Era.Core.Tests is sufficient verification

**AC#8**: Build succeeds after migration
- Test: `cd C:\Era\era紅魔館protoNTR && dotnet build`
- Expected: Build succeeded. 0 Error(s)
- Verifies package compatibility

**AC#9**: All tests pass with xUnit v3
- Test: `cd C:\Era\era紅魔館protoNTR && dotnet test`
- Expected: All tests pass
- **CRITICAL**: This verifies behavioral equivalence after xUnit v3 migration
- Per architecture.md Sub-Feature Requirements

**AC#10**: Migration notes documented
- Test: Grep pattern="xUnit v3 Migration Notes:" path="Game/agents/feature-448.md"
- Expected: Execution Log contains migration notes section
- Verifies that migration analysis and findings are documented in this feature file

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update Era.Core.Tests package reference | [x] |
| 2 | 2 | Update uEmuera.Tests package reference | [x] |
| 3 | 3 | Update ErbParser.Tests package reference | [x] |
| 4 | 4 | Update ErbToYaml.Tests package reference | [x] |
| 5 | 5 | Update KojoComparer.Tests package reference | [x] |
| 6 | 6 | Update YamlSchemaGen.Tests package reference | [x] |
| 7 | 7 | Update runner to xunit.runner.visualstudio 3.x | [x] |
| 8 | 8 | Verify all projects build | [x] |
| 9 | 9 | Verify all tests pass | [x] |
| 10 | 10 | Document migration findings in Execution Log (includes analysis) | [x] |

<!-- AC:Task alignment: 10 ACs, 10 Tasks. ErbLinter.Tests excluded (archived). -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Strategy

**Phase 1: Investigation**
1. Read xUnit v3 migration guide
2. Identify breaking changes affecting current test code
3. Plan API update strategy

**Phase 2: Package Updates**
1. Update 6 active test projects atomically (one commit):
   - Change `xunit` → `xunit.v3` (3.2.x)
   - Update `xunit.runner.visualstudio` 2.x → 3.x (package name unchanged per xUnit docs)
   - Excluded: ErbLinter.Tests (tools/_archived/, .NET 8.0)
2. Run `dotnet build` to verify package resolution

**Phase 3: API Updates**
1. Fix any compilation errors from API changes
2. Update test code as needed per migration guide
3. Known potential changes: Assert.Equal<T> signature, Assert.Throws return type, IAsyncLifetime interface. If new API issues discovered, document in Execution Log.

**Phase 4: Verification**
1. Run `dotnet build` - must succeed
2. Run `dotnet test` - all tests must pass
3. Document migration notes in Execution Log

### Breaking Changes Reference

From architecture.md Phase 11:

| 変更点 | v2 | v3 | 対応 |
|--------|----|----|------|
| パッケージ名 | `xunit` | `xunit.v3` | 参照変更 |
| Runner | `xunit.runner.visualstudio` 2.x | `xunit.runner.visualstudio` 3.x | バージョン変更 |
| Assert API | 一部変更 | 新 API | コード修正 |
| Theory data | `MemberData` 等 | 拡張 | 確認・修正 |

**References**:
- [xUnit v3 Release Notes](https://xunit.net/releases/v3/3.2.0)
- [xUnit v3 Migration Guide](https://xunit.net/docs/getting-started/v3/migration)

### Rollback Plan

If migration fails:
1. Revert all .csproj changes: `git checkout -- "**/*.csproj"`
2. Revert test code changes: `git checkout -- "**/*.Tests/**"`
3. Notify user with failure description
4. Create follow-up feature for investigation

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F446 | Phase 10 Post-Phase Review must complete first |
| Predecessor | F447 | Phase 11 Planning must complete first |
| Successor | F449 | Phase 11 Post-Phase Review - reviews this feature after completion |

---

## Links

- [feature-446.md](feature-446.md) - Phase 10 Post-Phase Review (predecessor)
- [feature-447.md](feature-447.md) - Phase 11 Planning (parent)
- [feature-449.md](feature-449.md) - Phase 11 Post-Phase Review (successor)
- [feature-451.md](feature-451.md) - do.md Gate Improvement (残課題)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 11 definition

---

## Out-of-Scope Note

| Item | Description | Tracked At |
|------|-------------|------------|
| xUnit1051 警告対応 | `TestContext.Current.CancellationToken` 使用推奨。コード品質改善。 | architecture.md Phase 11 Task 9 |
| do.md gate 改善 | 異常の明示的定義と Gate セクション追加 | F451 |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-11 FL iter1**: [applied] Phase2-Validate - AC#5/Summary/Impact Analysis: ErbLinter.Tests excluded. Updated to '6 test projects'.
- **2026-01-11 FL iter2**: [applied] Phase2-Validate - architecture.md Phase 11 scope updated (7→6 projects, ErbLinter.Tests excluded).
- **2026-01-11 FL iter2**: [resolved] Phase2-Validate - AC#7 'representative spot check' - accepted with clarification note added.
- **2026-01-11 FL iter4**: [applied] Phase3-Maintainability - F449 Implementation Contract template updated (6 projects, ErbLinter.Tests excluded).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 | create | spec-writer | Created from F447 Phase 11 Planning | PROPOSED |
| 2026-01-11 14:23 | START | implementer | Tasks 1-10 | - |
| 2026-01-11 14:23 | END | implementer | Tasks 1-10 | SUCCESS |
| 2026-01-11 14:25 | DEVIATION | do | AC#7 pattern fix | Spec error: xunit.v3.runner.visualstudio package doesn't exist. Corrected to xunit.runner.visualstudio Version 3.x |

### xUnit v3 Migration Notes:

**Package Changes Applied**:
- `xunit` 2.9.2 → `xunit.v3` 3.2.1
- `xunit.runner.visualstudio` 2.8.2 → `xunit.runner.visualstudio` 3.1.0

**Critical Finding**: Runner package name remains `xunit.runner.visualstudio` (NOT `xunit.v3.runner.visualstudio`). The v3 runner uses the same package name with v3.x version numbers.

**Migration Results**:
- **Build**: All 6 projects build successfully
- **Tests**: All 1,246 tests pass (773+382+65+10+12+4)
  - Era.Core.Tests: 773 passed
  - uEmuera.Tests: 382 passed
  - ErbParser.Tests: 65 passed
  - ErbToYaml.Tests: 10 passed
  - KojoComparer.Tests: 12 passed, 8 skipped (pre-existing)
  - YamlSchemaGen.Tests: 4 passed
- **API Changes**: No breaking API changes affecting current test code
- **New Warnings**: xUnit v3 analyzer warnings (xUnit1051) recommend using `TestContext.Current.CancellationToken` for cancellation support. These are best-practice recommendations, not errors. Out of scope for this migration.

**Behavioral Equivalence**: Confirmed. All tests pass with identical behavior under xUnit v3.
