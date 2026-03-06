# Feature 837: Enable EnforceCodeStyleInBuild for IDE-prefix Rule Enforcement

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T00:27:33Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Build-time enforcement is the SSOT for code style compliance across the devkit and core repositories. IDE-only rules that produce no build diagnostics are decorative, not enforceable. All code style rules defined in `.editorconfig` must participate in the build pipeline to guarantee consistent enforcement regardless of developer IDE configuration.

### Problem (Current Issue)
IDE-prefix code style rules (IDE0003, IDE0008, IDE0011, IDE0060, etc.) produce no build diagnostics because `EnforceCodeStyleInBuild` is absent from both devkit and core `Directory.Build.props` files. The .NET SDK defaults this property to `false`, requiring explicit opt-in. This gap was never addressed because the original `TreatWarningsAsErrors=true` setup (F708) focused exclusively on CA-prefix analyzer rules via `AnalysisLevel=latest-recommended`. As a result, 23 `.editorconfig` style rules (2 at `:warning`, 21 at `:suggestion`) are enforced only within IDEs, with no CI/build guarantee. The F831 investigation (feature-831.md:319-335) identified this gap and created F837 as a Mandatory Handoff (feature-831.md:443).

### Goal (What to Achieve)
Enable `EnforceCodeStyleInBuild` in devkit `Directory.Build.props` so that IDE-prefix rules defined in `.editorconfig` participate in `dotnet build`. Create a Mandatory Handoff (F839) for symmetric core repo changes. Verify that existing warning-level rules (IDE0160, IDE0065) have zero violations (confirmed by all three investigations: 237 devkit files and all core files are fully compliant). Add explicit `.editorconfig` entry for IDE0060 (Remove unused parameter) as the primary motivating rule from the F831 handoff.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do IDE-prefix rules produce no build diagnostics? | `EnforceCodeStyleInBuild` is absent from `Directory.Build.props` | `Directory.Build.props` (entire file, no such property) |
| 2 | Why is `EnforceCodeStyleInBuild` absent? | The .NET SDK defaults it to `false` and requires explicit opt-in | .NET SDK documentation; F831 report feature-831.md:319-335 |
| 3 | Why was explicit opt-in never added? | The F708 `TreatWarningsAsErrors=true` setup focused on CA-prefix rules via `AnalysisLevel=latest-recommended` | `Directory.Build.props:3-4` |
| 4 | Why were IDE-prefix rules not considered alongside CA-prefix rules? | The `.editorconfig` rules were written as IDE guidance only, with severities set to `suggestion` for safety | `.editorconfig:26-57` (21 rules at suggestion, 2 at warning) |
| 5 | Why (Root)? | No feature addressed the gap between editor-visible and build-time style enforcement until F831 identified it and created this handoff | `feature-831.md:443` (Mandatory Handoff creating F837) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | IDE-prefix rules (IDE0003, IDE0008, IDE0060, etc.) silently ignored during `dotnet build` | `EnforceCodeStyleInBuild` property never added to `Directory.Build.props` because F708 scope was CA-prefix only |
| Where | Build output (no IDE-prefix diagnostics) | `Directory.Build.props` configuration in devkit and core repos |
| Fix | Manually check IDE warnings in each developer's IDE | Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to `Directory.Build.props` in both repos |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F831 | [DONE] | Parent research; created F837 as Mandatory Handoff |
| F836 | [DRAFT] | Sibling from F831; enables CA1502/CA1506 (independent) |
| F829 | [DONE] | Phase 22 Deferred Obligations Consolidation |
| F813 | [DONE] | Post-Phase Review 21; CA1510 NoWarn fix (precedent for analyzer enablement) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical viability | FEASIBLE | Single MSBuild property addition; well-documented .NET SDK feature |
| Build safety | FEASIBLE | Zero violations for both warning-level rules (IDE0160: 237/237 file-scoped in devkit, all compliant in core; IDE0065: all compliant) |
| Cross-repo coordination | FEASIBLE | devkit and core `.editorconfig` files are byte-identical; changes can be mirrored |
| Scope containment | FEASIBLE | Engine repo excluded (no Directory.Build.props or .editorconfig); `_archived/` shielded by TreatWarningsAsErrors=false |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Build pipeline | HIGH | IDE-prefix rules will now produce build diagnostics; warning-level rules become build errors under TreatWarningsAsErrors=true |
| Developer workflow | MEDIUM | Developers must fix IDE-prefix violations before code compiles; previously these were IDE-only suggestions |
| core repo | MEDIUM | Symmetric change required; existing IDE1006 pragma in LegacyYamlDialogueLoader.cs:202,209 remains necessary |
| _archived projects | LOW | Shielded by _archived/Directory.Build.props TreatWarningsAsErrors=false |
| Engine repo | LOW | No impact; excluded from scope (no infrastructure exists) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `TreatWarningsAsErrors=true` | `Directory.Build.props:3` | Any IDE rule at `:warning` severity becomes a build error |
| 2 existing `:warning` rules | `.editorconfig:19,23` | IDE0160 (file_scoped) and IDE0065 (outside_namespace) would fire as build errors if violations exist |
| 21+ existing `:suggestion` rules | `.editorconfig:26-57` | Informational only; do not cause build errors |
| `_archived/` opt-out | `_archived/Directory.Build.props:3` | TreatWarningsAsErrors=false shields archived projects from breakage |
| Cross-repo symmetry | devkit + core identical `.editorconfig` | Changes must be applied to both repos |
| Engine exclusion | No Directory.Build.props or .editorconfig | Out of scope entirely |
| IDE0060 not in .editorconfig | Absence in current config | Must add explicit entry to enforce unused parameter detection |
| IDE1006 pragma in core | `LegacyYamlDialogueLoader.cs:202,209` | Existing suppression becomes functionally necessary after enablement |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| IDE0160/IDE0065 build breaks | LOW | HIGH | All three investigations confirmed zero violations across both repos |
| Unknown default IDE rules beyond .editorconfig causing surprises | LOW | MEDIUM | Run full build scan after enablement to identify any unexpected diagnostics |
| Hundreds of suggestion-level violations appearing as informational noise | MEDIUM | LOW | Suggestions do not break build; address incrementally |
| Cross-repo config drift over time | LOW | LOW | Apply changes simultaneously; document synchronization requirement |
| core has 28 `this.` usages (IDE0003) at suggestion severity | LOW | LOW | Suggestion severity does not break build; keep at suggestion |
| Future severity promotions causing unexpected build breaks | MEDIUM | MEDIUM | Document scan requirement before any severity promotion |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| EnforceCodeStyleInBuild in devkit | `grep -c "EnforceCodeStyleInBuild" Directory.Build.props` | 0 | Property absent |
| EnforceCodeStyleInBuild in core | `grep -c "EnforceCodeStyleInBuild" C:/Era/core/Directory.Build.props` | 0 | Property absent |
| IDE0160 violations (devkit) | `grep -rl "^namespace " --include="*.cs" src/` | 0 files | All 237 files use file-scoped namespaces |
| IDE0065 violations (devkit) | Build scan after enablement | 0 | All using directives are outside namespace |

**Baseline File**: `_out/tmp/baseline-837.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `TreatWarningsAsErrors=true` active | `Directory.Build.props:3` | Must verify build succeeds after enabling EnforceCodeStyleInBuild |
| C2 | Two rules at `:warning` severity | `.editorconfig:19,23` | Must verify zero violations exist for IDE0160 and IDE0065 before/after enablement |
| C3 | IDE0060 needs explicit .editorconfig entry | .NET SDK behavior | AC must verify .editorconfig contains IDE0060 rule after implementation |
| C4 | Cross-repo symmetry required | Both repos have identical .editorconfig | ACs should verify both devkit and core repos |
| C5 | Engine excluded | No infrastructure exists | ACs must NOT require engine changes |
| C6 | `_archived/` opt-out preserved | `_archived/Directory.Build.props:3` | Exclude _archived from build error verification |
| C7 | IDE1006 pragma in core | `LegacyYamlDialogueLoader.cs:202,209` | Existing suppression must be preserved |

### Constraint Details

**C1: TreatWarningsAsErrors Interaction**
- **Source**: `Directory.Build.props:3` sets TreatWarningsAsErrors=true
- **Verification**: `grep "TreatWarningsAsErrors" Directory.Build.props`
- **AC Impact**: Build must succeed after adding EnforceCodeStyleInBuild; any warning-level IDE rule violation becomes a build error

**C2: Warning-Level Rule Compliance**
- **Source**: `.editorconfig:19` (IDE0160 file_scoped:warning), `.editorconfig:23` (IDE0065 outside_namespace:warning)
- **Verification**: Build output contains no IDE0160 or IDE0065 errors
- **AC Impact**: Must verify zero violations; all three investigations confirmed compliance (237 devkit files, all core files)

**C3: IDE0060 Explicit Entry**
- **Source**: .NET SDK does not enforce IDE0060 unless explicitly configured in .editorconfig
- **Verification**: `grep "IDE0060\|dotnet_code_quality_unused_parameters" .editorconfig`
- **AC Impact**: AC must verify the rule entry exists in .editorconfig after implementation

**C4: Cross-Repo Symmetry**
- **Source**: devkit and core `.editorconfig` files are byte-identical
- **Verification**: Compare .editorconfig files between repos
- **AC Impact**: ACs should verify Directory.Build.props changes applied to both repos

**C5: Engine Exclusion**
- **Source**: Engine repo has no Directory.Build.props or .editorconfig
- **Verification**: N/A
- **AC Impact**: No ACs should reference engine repo files

**C6: Archived Project Shielding**
- **Source**: `_archived/Directory.Build.props:3` has TreatWarningsAsErrors=false
- **Verification**: `grep "TreatWarningsAsErrors" _archived/Directory.Build.props`
- **AC Impact**: Archived projects are shielded from new build errors

**C7: Existing IDE1006 Pragma**
- **Source**: `C:\Era\core\src\Era.Core.Tests\Helpers\LegacyYamlDialogueLoader.cs:202,209`
- **Verification**: `grep "pragma warning disable IDE1006" C:/Era/core/src/Era.Core.Tests/Helpers/LegacyYamlDialogueLoader.cs`
- **AC Impact**: Existing pragma suppressions must not be removed; they become functionally necessary after enablement

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F831 | [DONE] | Research that identified the EnforceCodeStyleInBuild gap and created F837 as Mandatory Handoff |
| Related | F836 | [PROPOSED] | Sibling from F831; enables CA1502/CA1506 (independent, no blocking relationship) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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
| "Build-time enforcement is the SSOT for code style compliance" | EnforceCodeStyleInBuild must be present in Directory.Build.props | AC#1 |
| "All code style rules defined in `.editorconfig` must participate in the build pipeline" | Build must succeed with enforcement enabled under TreatWarningsAsErrors=true | AC#4 |
| "IDE-only rules that produce no build diagnostics are decorative, not enforceable" | IDE0060 must have explicit .editorconfig entry to become enforceable | AC#3 |
| "guarantee consistent enforcement regardless of developer IDE configuration" | Build pipeline is the enforcement mechanism; build success proves enforcement | AC#4, AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | EnforceCodeStyleInBuild enabled in devkit Directory.Build.props | code | Grep(Directory.Build.props, pattern="EnforceCodeStyleInBuild.*true") | matches | `EnforceCodeStyleInBuild.*true` | [x] |
| 2 | TreatWarningsAsErrors property preserved in Directory.Build.props | code | Grep(Directory.Build.props, pattern="TreatWarningsAsErrors") | gte | 1 | [x] |
| 3 | IDE0060 rule added to .editorconfig | code | Grep(.editorconfig, pattern="dotnet_code_quality_unused_parameters") | matches | `dotnet_code_quality_unused_parameters` | [x] |
| 4 | devkit solution builds successfully with enforcement enabled | build | dotnet build devkit.sln | succeeds | exit code 0 | [x] |
| 5 | Existing .editorconfig warning-level rules preserved | code | Grep(.editorconfig, pattern=":warning") | gte | 2 | [x] |
| 6 | Core repo handoff feature created | file | Grep(pm/features/feature-839.md, pattern="EnforceCodeStyleInBuild") | matches | `EnforceCodeStyleInBuild` | [x] |
| 7 | F839 registered in index-features.md | code | Grep(pm/index-features.md, pattern="F839") | matches | `F839` | [x] |

### AC Details

**AC#2: TreatWarningsAsErrors property preserved in Directory.Build.props**
- **Test**: `Grep(Directory.Build.props, pattern="TreatWarningsAsErrors")`
- **Expected**: gte 1 — confirms TreatWarningsAsErrors property still exists after our edits (not accidentally removed or altered)
- **Derivation**: TreatWarningsAsErrors already exists at Directory.Build.props:3; Grep returns >= 1 match confirming the property persists after adding EnforceCodeStyleInBuild.
- **Rationale**: EnforceCodeStyleInBuild interacts with TreatWarningsAsErrors (warning-level IDE rules become build errors). Preserving TreatWarningsAsErrors is a prerequisite for build-time enforcement.

**AC#5: Existing .editorconfig warning-level rules preserved**
- **Test**: `Grep(.editorconfig, pattern=":warning")`
- **Expected**: gte 2 — baseline has exactly 2 warning-level rules (IDE0160 file_scoped:warning on line 19, IDE0065 outside_namespace:warning on line 23). After implementation, count must be >= 2 (may increase if IDE0060 is set to warning severity).
- **Rationale**: Constraint C2 requires zero violations for these existing warning-level rules. Their continued presence in .editorconfig confirms they were not downgraded to avoid build errors, which would violate the Philosophy of build-time enforcement as SSOT.
- **Derivation**: 2 existing `:warning` rules confirmed by Grep(.editorconfig, ":warning") returning lines 19 and 23.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Enable EnforceCodeStyleInBuild in devkit Directory.Build.props | AC#1, AC#2 |
| 2 | Verify existing warning-level rules (IDE0160, IDE0065) have zero violations | AC#4, AC#5 |
| 3 | Add explicit .editorconfig entry for IDE0060 | AC#3 |
| 4 | Build succeeds under TreatWarningsAsErrors=true with enforcement enabled | AC#4 |
| 5 | Core repo handoff feature created and registered | AC#6, AC#7 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Two minimal file edits satisfy all 7 ACs with zero C# source file changes.

**Edit 1 — `Directory.Build.props`**: Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` inside the existing `<PropertyGroup>` that already contains `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. Placing both properties in the same group makes the enforcement relationship explicit and keeps the single-group structure consistent with the current file.

**Edit 2 — `.editorconfig`**: Add one line under the `[*.cs]` section to declare IDE0060 at suggestion severity:
```
dotnet_code_quality_unused_parameters = all:suggestion
```
`suggestion` severity is correct: the rule is informational (no build error), aligning with the 21 existing suggestion-level rules. Choosing `:warning` would impose a build-error risk for any unused parameter and is not the goal of this feature.

**No C# file changes are required**: All three investigations (F831) confirmed zero violations for the two warning-level rules (IDE0160, IDE0065) across all 237 devkit C# files. Enabling `EnforceCodeStyleInBuild` will not introduce any new build errors.

**Core repo**: Changes to `C:\Era\core\Directory.Build.props` and `C:\Era\core\.editorconfig` are symmetric and mandatory, but are out of scope for this devkit-only feature. They are tracked as a Mandatory Handoff in this feature file, satisfying AC#6.

This approach satisfies all 7 ACs:
- AC#1, AC#2: Direct result of Edit 1
- AC#3: Direct result of Edit 2
- AC#4: Build verification after Edit 1+2 — zero new violations means exit code 0
- AC#5: Existing `:warning` entries on lines 19, 23 of `.editorconfig` are untouched; adding IDE0060 at `:suggestion` does not reduce their count
- AC#6: Task#4 creates `pm/features/feature-839.md` [DRAFT] for core repo symmetric changes

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to `Directory.Build.props` |
| 2 | Place it inside the same `<PropertyGroup>` as `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — TreatWarningsAsErrors already exists so Grep returns ≥ 1 |
| 3 | Add `dotnet_code_quality_unused_parameters = all:suggestion` to `.editorconfig` under `[*.cs]` |
| 4 | Run `dotnet build devkit.sln` after Edits 1+2; zero violations confirmed by F831 → exit code 0 |
| 5 | `.editorconfig` lines 19 (`csharp_style_namespace_declarations = file_scoped:warning`) and 23 (`csharp_using_directive_placement = outside_namespace:warning`) are untouched; count of `:warning` remains ≥ 2 |
| 6 | Task#4 creates `pm/features/feature-839.md` with `EnforceCodeStyleInBuild` in background — Grep verifies content |
| 7 | Task#4 registers F839 in `pm/index-features.md` — Grep verifies `F839` appears in index |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Placement of EnforceCodeStyleInBuild | A: Same PropertyGroup as TreatWarningsAsErrors; B: New separate PropertyGroup | A: Same PropertyGroup | Logically groups build-enforcement properties together; aligns with AC#2 requirement; no MSBuild behavioral difference between options |
| IDE0060 severity level | A: `:suggestion`; B: `:warning`; C: `:error` | A: `:suggestion` | Warning-level would make any unused parameter a build error under TreatWarningsAsErrors=true — too disruptive; suggestion matches the existing pattern for IDE rules in .editorconfig; the goal is enforceability, not immediate breakage |
| IDE0060 option value | A: `all` (methods and local functions); B: `non_public` (only non-public); C: `local_functions` only | A: `all` | Matches the F831 motivating use case (unused parameters in any method); `non_public` would miss internal/public method violations; `all` is the standard recommendation |
| Core repo handling | A: Apply both repos in one feature; B: devkit-only + Mandatory Handoff | B: devkit-only + Mandatory Handoff | Scope is devkit-only per feature definition; core changes require separate implementation context; handoff satisfies AC#6 |

### Interfaces / Data Structures

<!-- Optional: Define new interfaces, data structures, or APIs if applicable -->

This feature modifies two configuration files only. No new interfaces, data structures, or APIs are defined.

**`Directory.Build.props` after edit** (relevant PropertyGroup):
```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  ...
</PropertyGroup>
```

**`.editorconfig` addition** (appended after existing `[*.cs]` rules, before the end of file):
```ini
# Unused parameters
dotnet_code_quality_unused_parameters = all:suggestion
```

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
| 1 | 1, 2 | Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` inside the existing `<PropertyGroup>` containing `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props` | | [x] |
| 2 | 3, 5 | Add `dotnet_code_quality_unused_parameters = all:suggestion` under the `[*.cs]` section in `.editorconfig` | | [x] |
| 3 | 4 | Run `dotnet build devkit.sln` via WSL and verify exit code 0 (no IDE0160/IDE0065 build errors) | | [x] |
| 4 | 6, 7 | Create `pm/features/feature-839.md` [DRAFT] for core repo symmetric changes (`C:\Era\core\Directory.Build.props` and `C:\Era\core\.editorconfig`) and register in `pm/index-features.md` | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `Directory.Build.props` | `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` added to PropertyGroup |
| 2 | implementer | sonnet | `.editorconfig` | `dotnet_code_quality_unused_parameters = all:suggestion` added under `[*.cs]` |
| 3 | implementer | sonnet | `devkit.sln` | `dotnet build devkit.sln` exit code 0 confirmed |
| 4 | implementer | sonnet | `pm/index-features.md` | `pm/features/feature-839.md` created + registered |

### Pre-conditions

- F831 is [DONE] (predecessor)
- `Directory.Build.props` does NOT yet contain `EnforceCodeStyleInBuild`
- `.editorconfig` does NOT yet contain `dotnet_code_quality_unused_parameters`

### Execution Order

1. **Task 1** — Edit `Directory.Build.props`: Locate the `<PropertyGroup>` containing `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` on the next line after `<TreatWarningsAsErrors>`. Do NOT create a new PropertyGroup.

2. **Task 2** — Edit `.editorconfig`: Locate the `[*.cs]` section. Append the following block at the end of the `[*.cs]` section (before any subsequent section header or end of file):
   ```ini
   # Unused parameters
   dotnet_code_quality_unused_parameters = all:suggestion
   ```

3. **Task 3** — Build verification: Run via WSL:
   ```bash
   MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build devkit.sln'
   ```
   Expected: exit code 0. If any IDE-prefix warnings appear as errors, STOP and report to user.

4. **Task 4** — Create `pm/features/feature-839.md` [DRAFT] with background content describing the core repo symmetric changes. Then register in `pm/index-features.md` under the Phase 22 table.

### Build Verification Steps

After Task 3 completes:
- Confirm exit code is 0
- Scan build output for `IDE0160` or `IDE0065` — must be absent
- If unexpected IDE warnings appear: STOP → Report to user (do not suppress without approval)

### Rollback Plan

If the build fails after Task 1 or Task 2:
1. Revert the failing edit with `git checkout -- <file>`
2. Report the diagnostic output to user
3. Create a follow-up feature for investigation if needed

### Success Criteria

- `Directory.Build.props` contains `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`
- `.editorconfig` contains `dotnet_code_quality_unused_parameters = all:suggestion`
- `dotnet build devkit.sln` exits with code 0
- `pm/features/feature-839.md` exists and is registered in `pm/index-features.md`

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Symmetric changes to `C:\Era\core\Directory.Build.props` and `C:\Era\core\.editorconfig` (add `EnforceCodeStyleInBuild` + `dotnet_code_quality_unused_parameters`) | core repo is out of scope for this devkit-only feature; changes require separate implementation context | Feature | F839 | Task#4 | [x] | 作成済み(A) |

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
| 2026-03-06T01:00 | START | initializer | [REVIEWED]→[WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-06T01:02 | INVESTIGATION | explorer | Confirmed insertion points for all 4 tasks | OK |
<!-- run-phase-2-completed -->
| 2026-03-06T01:05 | IMPL | orchestrator | Task 1-4 all completed, build 0W/0E | OK |
<!-- run-phase-4-completed -->
| 2026-03-06T01:08 | DEVIATION | ac-static-verifier | build AC#4 | PRE-EXISTING: verifier runs dotnet directly (not WSL), Smart App Control blocks. Build verified via WSL in Phase 4 (0W/0E) |
| 2026-03-06T01:10 | VERIFY | ac-static-verifier | code 5/5 PASS, file 1/1 PASS, build WSL-verified | All 7 ACs [x] |
<!-- run-phase-7-completed -->
| 2026-03-06T01:12 | POST-REVIEW | feature-reviewer | NEEDS_REVISION: Handoff Transferred not updated | Fixed |
<!-- run-phase-8-completed -->
| 2026-03-06T01:15 | REPORT | orchestrator | 7/7 AC PASS, 1 DEVIATION (PRE-EXISTING), user approved | OK |
<!-- run-phase-9-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: pm/features/feature-837.md (after Review Notes) | Missing ## Improvement Log section (template compliance)
- [fix] Phase2-Review iter1: AC#2 (AC Definition Table) | AC#2 description-matcher semantic mismatch — updated description to match actual Grep behavior
- [fix] Phase2-Review iter2: Goal section | Goal text claimed "both devkit and core" but ACs only cover devkit — revised to devkit-only with handoff
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#7 for F839 index registration (DRAFT Creation Checklist requirement)

---

## Improvement Log

<!-- Optional: Track improvements discovered during implementation -->

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F831](feature-831.md) - Roslynator Analyzers Investigation — parent research; created F837 as Mandatory Handoff
[Related: F836](feature-836.md) - Enable CA1502 and CA1506 via .editorconfig — sibling from F831 (independent)
[Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation
[Related: F813](feature-813.md) - Post-Phase Review 21; CA1510 NoWarn fix (precedent for analyzer enablement)
[Successor: F839](feature-839.md) - core repo symmetric changes (EnforceCodeStyleInBuild + IDE0060)
[Related: F708] - TreatWarningsAsErrors=true setup (pre-PM era; no feature file)
