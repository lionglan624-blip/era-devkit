# Feature 566: Pre-commit CI Modernization

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

## Background

### Philosophy (Mid-term Vision)

CI checks must provide value. Dead checks waste commit time and create false confidence.
Per architecture.md "Test Infrastructure Transition": Phase 16 completion triggers CI modernization.

### Problem (Current Issue)

Current pre-commit contains obsolete checks:
1. `verify_com_map.py` - ERB-based COM mapping verification, but F562/F563 moved to YAML COM
2. `verify-logs.py` - Feature AC log verification, but logs are not generated (headless tests archived)
3. Comments reference outdated Phase numbers (need update per architecture.md)

### Goal (What to Achieve)

1. Remove obsolete checks that provide no value
2. Update comments to reflect current architecture
3. Add `dotnet test` integration (early implementation before Phase 15, justified by F565 COM YAML Runtime Integration completion)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | verify_com_map.py removed from pre-commit | code | Grep | not_contains | "verify_com_map.py" | [x] |
| 2 | verify-logs.py removed from pre-commit | code | Grep | not_contains | "verify-logs.py" | [x] |
| 3 | Update outdated Phase comments in pre-commit | code | Grep | not_contains | "Phase 12 (COM Implementation) + Phase 28 (Integration)" | [x] |
| 3b | Updated Phase reference present | code | Grep | contains | "Phase 15.*Kojo" | [x] |
| 4 | dotnet test Era.Core.Tests command present | code | Grep | contains | "dotnet test Era.Core.Tests" | [x] |
| 5 | pre-commit still executable | exit_code | `bash .githooks/pre-commit` | succeeds | - | [x] |
| 6 | pre-commit blocks on syntax error (Neg) | exit_code | `bash .githooks/pre-commit` | fails | - | [x] |
| 7 | architecture.md revision notes updated | code | Grep | matches | "F566.*CI Modernization" | [x] |

### AC Details

**AC#1-3**: Obsolete code and comments removed
- File: `.githooks/pre-commit`
- Verification: Grep for removed patterns
- Note: verify_com_map.py removed without YAML schema validation replacement (will be added in Phase 16 per architecture.md)

**AC#4**: dotnet test integration
- Add actual dotnet test command since F565 is complete
- Command: `dotnet test Era.Core.Tests` (per architecture.md line 4491)
- Placement: After dotnet build check

**AC#5**: Positive test - pre-commit runs successfully on clean state

**AC#6**: Negative test - pre-commit fails on dotnet test failure
- Create temporary test file with failing assertion: Era.Core.Tests/TempFailTest.cs with [Fact] Assert.True(false)
- Run `bash .githooks/pre-commit`
- Verify non-zero exit code due to test failure
- Remove temporary test file to fix state

**AC#7**: Architecture documentation updated
- File: `Game/agents/designs/full-csharp-architecture.md`
- Format: Blockquote revision note entry
- Add entry: `> **Revision Note (YYYY-MM-DD #N)**: F566 CI Modernization - removed obsolete checks`


---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove verify_com_map.py from pre-commit | [x] |
| 2 | 2 | Remove verify-logs.py from pre-commit | [x] |
| 3 | 3,3b | Update outdated Phase comments with current references | [x] |
| 4 | 4 | Add dotnet test command to pre-commit | [x] |
| 5 | 5 | Test pre-commit positive case | [x] |
| 6 | 6 | Test pre-commit negative case | [x] |
| 7 | 7 | Update architecture.md Revision Notes | [x] |

---

## Implementation Contract

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.githooks/pre-commit` | Remove obsolete checks, add dotnet test | Faster commits, proper test integration |
| `architecture.md` | Add F566 note | Documentation accuracy |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Pre-commit restored to previous state
3. Create follow-up feature for fix

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F565 | [DONE] | COM YAML Runtime (F566 prepares for dotnet test after F565) |

---

## Review Notes

- [applied] Phase2-Maintainability iter8: Philosophy Coverage Gap resolved. User approved early dotnet test implementation based on F565 completion (C# test infrastructure ready). Early CI integration preferred over waiting for Phase 15.

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| YAML schema validation replacement for verify_com_map.py | architecture.md Test Infrastructure Transition requirement | architecture.md Phase 16 Post-Phase Review Tasks | architecture.md Task list (line 4487-4498) |
| IMPLE_FEATURE_ID cleanup | pre-commit no longer uses this file; workflow docs reference dead code | Feature | F577 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-20 21:25 | START | implementer | Task 1-4 | - |
| 2026-01-20 21:25 | END | implementer | Task 1-4 | SUCCESS |
| 2026-01-20 21:26 | VERIFY | opus | Task 5 (Pos test) | PASS |
| 2026-01-20 21:26 | VERIFY | opus | Task 6 (Neg test) | PASS |
| 2026-01-20 21:27 | VERIFY | opus | Task 7 | PASS |
| 2026-01-20 21:27 | VERIFY | opus | AC#1-7 | ALL PASS |
| 2026-01-20 21:28 | DEVIATION | feature-reviewer | doc-check | testing/SKILL.md stale |
| 2026-01-20 21:28 | FIX | opus | testing/SKILL.md | Updated pre-commit section |
| 2026-01-20 21:29 | DEVIATION | feature-reviewer | doc-check | IMPLE_FEATURE_ID now dead code |
| 2026-01-20 21:29 | DEFER | opus | IMPLE_FEATURE_ID cleanup | Out of scope → F577 handoff |

---

## Links

- [index-features.md](index-features.md)
- [feature-562.md](feature-562.md) - Referenced in Background
- [feature-563.md](feature-563.md) - Referenced in Background
- [feature-565.md](feature-565.md) - Predecessor: COM YAML Runtime
- [feature-564.md](feature-564.md) - Blocked by this feature
- [feature-567.md](feature-567.md) - Related: Claude Code Hooks Cleanup
- [feature-577.md](feature-577.md) - Successor: Workflow Dead Code Cleanup
