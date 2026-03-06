# Feature 839: Enable EnforceCodeStyleInBuild in core repo (Symmetric with F837)

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T02:31:25Z -->

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

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Build-time enforcement is the SSOT for code style compliance across all repositories. The core repo must have identical enforcement to devkit.

### Problem (Current Issue)
F837 enabled `EnforceCodeStyleInBuild` and added `dotnet_code_quality_unused_parameters` in the devkit repo. The core repo (`C:\Era\core`) shares an identical `.editorconfig` but lacks the same `Directory.Build.props` property. This creates an enforcement gap where devkit enforces IDE-prefix rules at build time but core does not.

### Goal (What to Achieve)
Apply symmetric changes to the core repo:
1. Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to `C:\Era\core\Directory.Build.props`
2. Add `dotnet_code_quality_unused_parameters = all:suggestion` to `C:\Era\core\.editorconfig`
3. Verify build succeeds (existing IDE1006 pragma in `LegacyYamlDialogueLoader.cs:202,209` must be preserved)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are IDE-prefix rules not enforced in core builds? | `EnforceCodeStyleInBuild` property is absent from core's `Directory.Build.props` | `C:\Era\core\Directory.Build.props:1-25` -- no such property |
| 2 | Why is it absent? | .NET SDK defaults `EnforceCodeStyleInBuild` to `false`; explicit opt-in is required | .NET SDK default behavior |
| 3 | Why was it never opted in? | F837 enabled it for devkit only, deferring core as a Mandatory Handoff | `pm/features/feature-837.md:443` -- Mandatory Handoff to F839 |
| 4 | Why was F837 scoped to devkit only? | The 5-repo architecture requires explicit per-repo infrastructure changes | CLAUDE.md 5-repo split architecture |
| 5 | Why (Root)? | Each repo is an independent git repository with its own build configuration; cross-repo changes require separate features | `C:\Era\core` and `C:\Era\devkit` are separate repos with separate `Directory.Build.props` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Core repo does not enforce IDE-prefix style rules at build time | 5-repo architecture requires explicit per-repo opt-in; F837 was correctly scoped to devkit only |
| Where | `C:\Era\core\Directory.Build.props` and `.editorconfig` | Architectural boundary between devkit and core repos |
| Fix | Manually check style compliance | Add `EnforceCodeStyleInBuild` and `dotnet_code_quality_unused_parameters` to core repo config |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F837 | [DONE] | Direct predecessor -- devkit-side enablement; created F839 as Mandatory Handoff |
| F831 | [DONE] | Parent research -- identified the EnforceCodeStyleInBuild gap |
| F836 | [DONE] | Sibling from F831; adds CA1502/CA1506 to devkit .editorconfig (independent scope) |
| F708 | [DONE] | Original TreatWarningsAsErrors=true setup (CA-prefix rules only) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical viability | FEASIBLE | Identical change pattern to F837; single MSBuild property + single editorconfig rule |
| Build safety (warning-level rules) | FEASIBLE | Zero IDE0160 violations; zero IDE0065 violations across all core .cs files |
| Existing pragma coverage | FEASIBLE | IDE1006 pragmas at LegacyYamlDialogueLoader.cs:202-204,209-212 correctly suppress naming violations |
| Suggestion-level noise | FEASIBLE | 28 `this.` usages (IDE0003) + 2 dead private methods (IDE0051) at suggestion severity -- no build failure |
| Editorconfig divergence from devkit | FEASIBLE | F836 adds CA1502/CA1506 to devkit only; tracked and independent |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Core repo build | LOW | Adds IDE-prefix rule enforcement; all existing code is compliant or at suggestion severity |
| Developer experience | LOW | IDE suggestions surface for `this.` usage and dead code but do not break builds |
| Cross-repo consistency | MEDIUM | Closes enforcement gap between devkit and core repos |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `TreatWarningsAsErrors=true` active | `C:\Era\core\Directory.Build.props:3` | Any IDE rule at `:warning` severity becomes a build error |
| Two existing `:warning` rules (IDE0160, IDE0065) | `C:\Era\core\.editorconfig:19,23` | Must have zero violations (confirmed compliant) |
| IDE1006 pragmas must be preserved | `LegacyYamlDialogueLoader.cs:202-204,209-212` | Existing pragmas protect YAML deserialization property names |
| Cross-repo build via WSL | CLAUDE.md WSL section | Core repo build must use WSL dotnet pattern |
| Core repo is separate git repository | 5-repo split architecture | Commit in `C:\Era\core`, not devkit |
| CA1502/CA1506 out of scope | F836 owns those rules | Do NOT add CA1502/CA1506 to core .editorconfig |
| No per-project NoWarn overrides | `Era.Core.csproj`, `Era.Core.Tests.csproj` | No hidden suppressions to account for |
| `_archived` code excluded via Compile Remove | `Era.Core.Tests.csproj:41` | No diagnostic noise from archived code |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Unknown IDE rules producing build errors | LOW | HIGH | Run full `dotnet build` after enablement; all `:warning` rules already verified compliant |
| IDE1006 pragmas accidentally removed | LOW | MEDIUM | Pragmas clearly commented; AC verifies preservation |
| Future editorconfig drift between repos | MEDIUM | MEDIUM | Document synchronization requirement; track divergence |
| Future `:warning`-level rules could break build | LOW | MEDIUM | Verify build after any editorconfig changes |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Core build result | `wsl -- bash -c 'cd /mnt/c/Era/core && dotnet build'` | SUCCESS (0 errors, 0 warnings) | Pre-enablement baseline |
| IDE1006 pragma count | `grep -r "IDE1006" C:\Era\core\src\` | 4 lines in LegacyYamlDialogueLoader.cs | Must remain after enablement |

**Baseline File**: `_out/tmp/baseline-839.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `TreatWarningsAsErrors=true` active in core | `Directory.Build.props:3` | Must verify build succeeds after enablement |
| C2 | Two rules at `:warning` severity (IDE0160, IDE0065) | `.editorconfig:19,23` | Must verify zero violations for these rules |
| C3 | IDE1006 pragmas must be preserved | `LegacyYamlDialogueLoader.cs:202-212` | AC must NOT require removal of existing pragmas |
| C4 | Scope is core repo only | Feature definition | ACs reference `C:\Era\core` paths |
| C5 | Build runs via WSL | CLAUDE.md WSL requirement | Build AC must use WSL dotnet build pattern |
| C6 | CA1502/CA1506 exclusion | F836 owns those rules | ACs must NOT require CA1502/CA1506 in core .editorconfig |
| C7 | `dotnet_code_quality_unused_parameters` must be `:suggestion` severity | F837 precedent | AC must verify severity level is suggestion, not warning |

### Constraint Details

**C1: TreatWarningsAsErrors**
- **Source**: Core repo `Directory.Build.props:3` already has `TreatWarningsAsErrors=true`
- **Verification**: `grep TreatWarningsAsErrors C:\Era\core\Directory.Build.props`
- **AC Impact**: Build verification AC is critical -- any `:warning`+ severity rule with violations will break the build

**C2: Existing Warning-Level Rules**
- **Source**: Core `.editorconfig:19,23` has IDE0160 and IDE0065 at `:warning`
- **Verification**: All core .cs files use file-scoped namespaces and correct using placement (confirmed by all 3 investigations)
- **AC Impact**: Build success AC implicitly covers this; no separate AC needed

**C3: IDE1006 Pragma Preservation**
- **Source**: `LegacyYamlDialogueLoader.cs:202-204,209-212` has pragma disable/restore pairs for YAML property names (`entries`, `condition`, `lines`)
- **Verification**: `grep -n "IDE1006" C:\Era\core\src\Era.Core.Tests\Helpers\LegacyYamlDialogueLoader.cs`
- **AC Impact**: AC should verify pragma lines still exist after implementation

**C4: Core Repo Scope**
- **Source**: Feature definition and 5-repo architecture
- **Verification**: Changes target `C:\Era\core` only
- **AC Impact**: All file paths in ACs must reference core repo

**C5: WSL Build**
- **Source**: CLAUDE.md WSL section -- Smart App Control requires WSL for dotnet
- **Verification**: Standard WSL build pattern
- **AC Impact**: Build verification AC must specify WSL command

**C6: CA1502/CA1506 Exclusion**
- **Source**: F836 owns CA1502/CA1506 rules; [DONE] for devkit only
- **Verification**: Core .editorconfig should NOT contain CA1502/CA1506 after this feature
- **AC Impact**: AC should verify these rules are NOT present (scope boundary)

**C7: Suggestion Severity**
- **Source**: F837 established `:suggestion` as the severity for `dotnet_code_quality_unused_parameters`
- **Verification**: `grep dotnet_code_quality_unused_parameters C:\Era\core\.editorconfig`
- **AC Impact**: AC must verify the exact severity string `:suggestion`

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F837 | [DONE] | Devkit-side EnforceCodeStyleInBuild enablement completed |
| Related | F831 | [DONE] | Parent research -- identified the EnforceCodeStyleInBuild gap |
| Related | F836 | [DONE] | Sibling -- adds CA1502/CA1506 to devkit .editorconfig (independent scope) |
| Related | F708 | [DONE] | Original TreatWarningsAsErrors=true setup |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Build-time enforcement is the SSOT for code style compliance" | EnforceCodeStyleInBuild must be enabled in core repo | AC#1 |
| "The core repo must have identical enforcement to devkit" | dotnet_code_quality_unused_parameters added with same severity | AC#2 |
| "The core repo must have identical enforcement to devkit" | Build must succeed after enablement (no regressions) | AC#3 |
| "Build-time enforcement is the SSOT for code style compliance" | Existing IDE1006 pragmas preserved (no false enforcement) | AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | EnforceCodeStyleInBuild enabled in core Directory.Build.props | code | Grep(C:\Era\core\Directory.Build.props) | matches | `EnforceCodeStyleInBuild.*true` | [x] |
| 2 | dotnet_code_quality_unused_parameters added with suggestion severity | code | Grep(C:\Era\core\.editorconfig) | matches | `dotnet_code_quality_unused_parameters.*all:suggestion` | [x] |
| 3 | Core repo build succeeds via WSL | build | `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build Era.Core.sln'` | succeeds | 0 errors | [x] |
| 4 | IDE1006 pragmas preserved in LegacyYamlDialogueLoader.cs | code | Grep(C:\Era\core\src\Era.Core.Tests\Helpers\LegacyYamlDialogueLoader.cs, pattern="IDE1006") | count_equals | 4 | [x] |
| 5 | EnforceCodeStyleInBuild inside PropertyGroup element | code | Grep(C:\Era\core\Directory.Build.props) | matches | `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` | [x] |
| 6 | dotnet_code_quality_unused_parameters in [*.cs] section | code | Grep(C:\Era\core\.editorconfig) | matches | `dotnet_code_quality_unused_parameters = all:suggestion` | [x] |
| 7 | Core test suite passes via WSL | build | `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test Era.Core.sln --blame-hang-timeout 10s'` | succeeds | 0 errors | [x] |

### AC Details

**AC#4: IDE1006 pragmas preserved in LegacyYamlDialogueLoader.cs**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\Helpers\LegacyYamlDialogueLoader.cs, pattern="IDE1006")`
- **Expected**: count_equals 4 (2 pragma disable + 2 pragma restore at lines 202-204 and 209-212)
- **Derivation**: 4 IDE1006 pragma lines confirmed by baseline grep of `LegacyYamlDialogueLoader.cs` (2 pragma disable + 2 pragma restore); count is deterministic from YAML deserialization structure requiring 2 lowercase property groups
- **Rationale**: Constraint C3 requires IDE1006 pragmas for YAML deserialization property names (`entries`, `condition`, `lines`) to remain intact.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add EnforceCodeStyleInBuild to core Directory.Build.props | AC#1, AC#5 |
| 2 | Add dotnet_code_quality_unused_parameters = all:suggestion to core .editorconfig | AC#2, AC#6 |
| 3 | Verify build succeeds (IDE1006 pragmas preserved) | AC#3, AC#4, AC#7 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature applies two config-file-only changes to `C:\Era\core`, mirroring the F837 pattern applied to devkit. No C# source files are modified.

**Change 1 — `C:\Era\core\Directory.Build.props`**: Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` as the third `<PropertyGroup>` child, immediately after `<AnalysisLevel>latest-recommended</AnalysisLevel>`. This matches the devkit layout (`devkit/Directory.Build.props:5`) exactly.

**Change 2 — `C:\Era\core\.editorconfig`**: Append a `# Unused parameters` subsection at the end of the `[*.cs]` block (after `csharp_preserve_single_line_blocks = true`), with the single line `dotnet_code_quality_unused_parameters = all:suggestion`. This matches the devkit layout (`devkit/.editorconfig:80-81`) exactly. Note: CA1502/CA1506 lines present in devkit's editorconfig are NOT added here — those belong to F836 and are out of scope per constraint C6.

**Verification**: After both edits, run `dotnet build Era.Core.sln` via WSL to confirm 0 errors. Then run `dotnet test Era.Core.sln` to confirm test suite passes. `LegacyYamlDialogueLoader.cs` is not touched; the 4 IDE1006 pragma lines remain intact by construction.

All ACs are satisfied by these two targeted edits plus build/test verification.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` inside the existing `<PropertyGroup>` in `C:\Era\core\Directory.Build.props` |
| 2 | Append `dotnet_code_quality_unused_parameters = all:suggestion` under `[*.cs]` in `C:\Era\core\.editorconfig` |
| 3 | Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build Era.Core.sln'`; expect 0 errors |
| 4 | `LegacyYamlDialogueLoader.cs` is not modified; 4 IDE1006 pragma lines remain by construction — verify with Grep count_equals 4 |
| 5 | The element added in AC#1 uses full XML tag form `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` satisfying the exact-match check |
| 6 | The appended line uses exact spacing `dotnet_code_quality_unused_parameters = all:suggestion` matching the devkit pattern |
| 7 | Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test Era.Core.sln --blame-hang-timeout 10s'`; expect 0 errors |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Placement of `EnforceCodeStyleInBuild` in Directory.Build.props | A: After TreatWarningsAsErrors, B: After AnalysisLevel, C: Before closing `</PropertyGroup>` | B: After AnalysisLevel | Mirrors devkit layout exactly (devkit:5 = line 3 of PropertyGroup after TreatWarningsAsErrors + AnalysisLevel); symmetric layout is the explicit goal |
| Placement of `dotnet_code_quality_unused_parameters` in .editorconfig | A: After last existing `[*.cs]` rule (append), B: Inline among other dotnet_ rules, C: New section at file end | A: Append after `csharp_preserve_single_line_blocks` with `# Unused parameters` comment | Mirrors devkit .editorconfig:80-81 exactly; no new `[*.cs]` section needed since core .editorconfig already ends the `[*.cs]` block there |
| Include CA1502/CA1506 in core .editorconfig | A: Add (symmetric with devkit), B: Omit (follow F836 scope boundary) | B: Omit | Constraint C6 is explicit: CA1502/CA1506 belong to F836 which targets devkit only; adding them to core here would expand scope without a corresponding feature |
| Severity of dotnet_code_quality_unused_parameters | A: `:warning`, B: `:suggestion`, C: `:error` | B: `:suggestion` | Constraint C7 and F837 precedent; suggestion-level does not fail build under TreatWarningsAsErrors |

### Interfaces / Data Structures

<!-- Optional: Define new interfaces, data structures, or APIs if applicable -->

No new interfaces or data structures. This feature modifies two MSBuild/editorconfig configuration files only.

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 5 | Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to `C:\Era\core\Directory.Build.props` inside the existing `<PropertyGroup>`, after `<AnalysisLevel>latest-recommended</AnalysisLevel>` | | [x] |
| 2 | 2, 6 | Append `# Unused parameters` subsection with `dotnet_code_quality_unused_parameters = all:suggestion` to the `[*.cs]` block in `C:\Era\core\.editorconfig`; do NOT add CA1502/CA1506 | | [x] |
| 3 | 3, 4, 7 | Run `dotnet build Era.Core.sln` via WSL (verify 0 errors), run `dotnet test Era.Core.sln` via WSL (verify 0 errors), and grep-verify that 4 IDE1006 pragma lines remain in LegacyYamlDialogueLoader.cs | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-839.md Tasks + Technical Design | Modified `C:\Era\core\Directory.Build.props` |
| 2 | implementer | sonnet | feature-839.md Tasks + Technical Design | Modified `C:\Era\core\.editorconfig` |
| 3 | implementer | sonnet | WSL build/test commands | Build and test pass confirmation; pragma count verified |

### Pre-conditions

- `C:\Era\core` repo is accessible (separate git repository from devkit)
- WSL2 Ubuntu 24.04 with `/home/siihe/.dotnet/dotnet` available
- Baseline: core build currently succeeds with 0 errors, 0 warnings (pre-enablement)

### Execution Order

**Step 1 — Modify `C:\Era\core\Directory.Build.props` (Task 1)**

Read the current file to confirm structure. Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` as a new line immediately after `<AnalysisLevel>latest-recommended</AnalysisLevel>`, inside the existing `<PropertyGroup>`. This mirrors the devkit layout (`devkit/Directory.Build.props`).

**Step 2 — Modify `C:\Era\core\.editorconfig` (Task 2)**

Read the current file to confirm the `[*.cs]` section ends with `csharp_preserve_single_line_blocks = true`. Append the following two lines at the end of the `[*.cs]` block:

```
# Unused parameters
dotnet_code_quality_unused_parameters = all:suggestion
```

Do NOT add CA1502 or CA1506 — those belong to F836 (devkit-only scope).

**Step 3 — Build and test verification (Task 3)**

Run build:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build Era.Core.sln'
```
Expected: 0 errors.

Run tests:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test Era.Core.sln --blame-hang-timeout 10s'
```
Expected: 0 errors.

Verify pragma preservation:
```bash
grep -n "IDE1006" "C:\Era\core\src\Era.Core.Tests\Helpers\LegacyYamlDialogueLoader.cs"
```
Expected: exactly 4 matching lines.

### Rollback Plan

If build fails after Step 1 or Step 2:
1. STOP — do not proceed further
2. Revert the modified file(s) using `git checkout` in the `C:\Era\core` repo
3. Report exact error output to user for guidance
4. Do not create a follow-up feature without user approval

### Success Criteria

- All 7 ACs pass (AC#1–7)
- Core repo build: 0 errors after enablement
- Core repo tests: 0 errors after enablement
- IDE1006 pragma count in LegacyYamlDialogueLoader.cs: exactly 4 (unchanged)
- CA1502/CA1506 absent from core `.editorconfig`

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T02:45 | START | initializer | Status [REVIEWED] → [WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-06T02:47 | PHASE2 | explorer | Investigation: core repo files verified | OK |
<!-- run-phase-2-completed -->
| 2026-03-06T02:50 | PHASE4 | implementer | Task 1-3: Edit props+editorconfig, build+test | SUCCESS |
<!-- run-phase-4-completed -->
| 2026-03-06T02:55 | PHASE7 | ac-tester | AC#1-7 verified (code 5/5 static, build 2/2 manual WSL) | ALL PASS |
<!-- run-phase-7-completed -->
| 2026-03-06T02:58 | PHASE8 | feature-reviewer | Quality review (spec mode) 5/5 | READY |
<!-- run-phase-8-completed -->
| 2026-03-06T03:00 | DEVIATION | Bash | ac-static-verifier --ac-type build | exit 1: WinError 2 — tool cannot execute WSL-wrapped commands natively |
| 2026-03-06T03:05 | PHASE9 | orchestrator | Report & Approval | Approved |
<!-- run-phase-9-completed -->
| 2026-03-06T03:05 | PHASE10 | finalizer | Status [WIP] → [DONE] | READY_TO_COMMIT |
| 2026-03-06T03:06 | CodeRabbit | 0 findings | core repo review (base-commit HEAD~1) | - |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-DriftCheck iter1: AC Design Constraints C6 detail | F836 status "currently WIP" stale → updated to "[DONE]"
- [info] Phase1-DriftChecked: F836 (Related)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 839 (2026-03-06)
- [rejected] subagentプロンプトに大ファイルRead時の offset/limit 義務化 — 119回Readはmulti-agentパイプライン累計で各agent1回の正当Read。offset/limit強制は情報欠損を招く
- [rejected] subagentテンプレートにlimit指示追加 — 提案Aと同根。異なるagentの正当Readであり個別制限は的外れ
- [proposed] post-code-write.ps1, pre-bash-ac.ps1, post-bash-deviation.ps1 の ConvertFrom-Json を regex抽出に置換（pre-tdd-protection.ps1 既存パターン適用） → `.claude/hooks/` 内3ファイル

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F837](feature-837.md) - Enable EnforceCodeStyleInBuild for IDE-prefix Rule Enforcement (devkit)
[Related: F831](feature-831.md) - Roslynator Analyzers Investigation (parent research)
[Related: F836](feature-836.md) - CA1502/CA1506 severity overrides (devkit)
[Related: F708] - TreatWarningsAsErrors setup (pre-PM era; no feature file)
