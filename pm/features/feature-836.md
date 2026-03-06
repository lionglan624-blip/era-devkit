# Feature 836: Enable CA1502 and CA1506 via .editorconfig

## Status: [WIP]
<!-- fl-reviewed: 2026-03-06T00:28:43Z -->

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

CA1502 (cyclomatic complexity) and CA1506 (class coupling) are Microsoft.CodeAnalysis.NetAnalyzers rules that exist in the built-in analyzer set but are set to `severity=none` at `AnalysisLevel=latest-recommended` (`Directory.Build.props:4`). They do not require the Roslynator.Analyzers package. Enabling them requires explicit `.editorconfig` overrides (e.g., `dotnet_diagnostic.CA1502.severity = suggestion`) and a staged remediation plan given `TreatWarningsAsErrors=true` in `Directory.Build.props:3`. This feature was created as a Mandatory Handoff from F831 (Roslynator Analyzers Investigation, Task 5).

### Philosophy (Mid-term Vision)
Static analysis rules are the SSOT for code quality enforcement across the devkit codebase. Complexity and coupling metrics must be visible as analyzer diagnostics so that high-complexity methods are surfaced during development, not discovered ad-hoc during code review. Enablement at `suggestion` severity establishes the baseline for future staged promotion.

### Problem (Current Issue)
CA1502 (cyclomatic complexity) and CA1506 (class coupling) are disabled because `AnalysisLevel=latest-recommended` (`Directory.Build.props:4`) sets them to `severity=none` by default, and the repository's `.editorconfig` (lines 17-78) contains zero `dotnet_diagnostic` severity overrides. This means high-complexity methods like `ErbParser.ParseString` (~370 lines, estimated CC >50 at `ErbParser/ErbParser.cs:28-397`) and high-coupling classes like `KojoComparer/ErbEvaluator.cs` (108 branch constructs across ~870 lines) produce no analyzer diagnostics during build. The `TreatWarningsAsErrors=true` setting (`Directory.Build.props:3`) further constrains the enablement path -- any severity above `suggestion` would cause immediate build failures due to existing violations.

### Goal (What to Achieve)
Enable CA1502 and CA1506 at `suggestion` severity via `.editorconfig` overrides, establishing the first `dotnet_diagnostic` entries in the repository. The build must remain green (no build failures from the new diagnostics). This is enablement only -- remediation of flagged methods/classes is explicitly out of scope.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are there no cyclomatic complexity or class coupling diagnostics? | CA1502 and CA1506 produce no output during build | `dotnet build` output shows no CA1502/CA1506 diagnostics |
| 2 | Why do these rules produce no output? | Their severity is `none` at the `latest-recommended` analysis level | `Directory.Build.props:4`: `AnalysisLevel=latest-recommended` |
| 3 | Why is severity `none` not overridden? | The `.editorconfig` contains only code style rules, zero `dotnet_diagnostic` severity entries | `.editorconfig:17-78`: no `dotnet_diagnostic` entries |
| 4 | Why has no override been added? | No prior feature addressed these rules; F831 was the first to investigate and identify the gap | `feature-831.md:442`: Mandatory Handoff to F836 |
| 5 | Why (Root)? | Microsoft ships CA1502/CA1506 disabled by default because they are expensive and opinionated; enabling requires explicit opt-in via `.editorconfig` | Microsoft .NET SDK documentation; F831 research findings |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | No cyclomatic complexity or class coupling enforcement during build | CA1502/CA1506 severity is `none` at `AnalysisLevel=latest-recommended` with no `.editorconfig` override |
| Where | Build output (no CA1502/CA1506 diagnostics) | `.editorconfig` (missing `dotnet_diagnostic` entries) |
| Fix | Manually inspect methods for complexity | Add `dotnet_diagnostic.CA1502.severity = suggestion` and `dotnet_diagnostic.CA1506.severity = suggestion` to `.editorconfig` |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F831 | [DONE] | Parent -- Mandatory Handoff created F836 |
| F837 | [DRAFT] | Sibling handoff from F831 (EnforceCodeStyleInBuild) |
| F813 | [DONE] | CA1510 NoWarn debt fix -- related analyzer management |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical viability | FEASIBLE | `.editorconfig` override is a standard .NET SDK mechanism |
| Build safety | FEASIBLE | `suggestion` severity is not promoted to error by TreatWarningsAsErrors |
| Scope containment | FEASIBLE | Single file change (.editorconfig), no csproj modifications needed |
| Risk level | FEASIBLE | Informational diagnostics only, no behavioral change |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Build output | LOW | `suggestion` diagnostics appear in IDE but do not affect build success/failure |
| Developer workflow | LOW | New informational diagnostics visible in IDE; no action required |
| CI pipeline | LOW | `suggestion` severity does not produce warnings or errors in CI |
| Codebase precedent | MEDIUM | First `dotnet_diagnostic` override in `.editorconfig` establishes pattern for future rules |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `TreatWarningsAsErrors=true` | `Directory.Build.props:3` | MUST use `suggestion` severity; `warning` would break build |
| Single `.editorconfig` at repository root | Repository structure | Override applies uniformly to all 18 C# projects in solution |
| CA1502 default threshold = 25 | Microsoft documentation | ErbParser.ParseString far exceeds this; violations expected |
| CA1506 default threshold = 95 (type) / 40 (member) | Microsoft documentation | ErbEvaluator may approach member threshold |
| `_archived/` has own `Directory.Build.props` with `TreatWarningsAsErrors=false` | `_archived/Directory.Build.props:3` | Already excluded; not in solution file |
| No existing `dotnet_diagnostic` precedent | `.editorconfig:17-78` | This is the first diagnostic override entry |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| `suggestion` produces noisy IDE output | HIGH | LOW | Informational only; developers can filter |
| Future promotion to `warning` causes build breaks | MEDIUM | HIGH | Require remediation feature before promotion |
| CA1502 thresholds too aggressive for parser code | HIGH | MEDIUM | Custom threshold or per-file suppression in future feature |
| Developers ignore `suggestion` indefinitely | MEDIUM | MEDIUM | Remediation feature to be created during /run when diagnostics reveal specific violations (targets unknown until enablement) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| CA1502 diagnostics | `dotnet build 2>&1 \| grep CA1502` | 0 | Currently severity=none; no output |
| CA1506 diagnostics | `dotnet build 2>&1 \| grep CA1506` | 0 | Currently severity=none; no output |
| Build result | `dotnet build` | Success | Must remain Success after change |

**Baseline File**: `_out/tmp/baseline-836.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `suggestion` severity MUST be used initially | `Directory.Build.props:3` (TreatWarningsAsErrors) | AC must verify severity is `suggestion`, not `warning` or `error` |
| C2 | Changes are `.editorconfig` only | F831 research, single config point | No csproj modifications; AC must verify no csproj changes |
| C3 | Build must remain green | TreatWarningsAsErrors + suggestion interaction | AC must verify `dotnet build` succeeds |
| C4 | Both CA1502 and CA1506 must be enabled | F831 Mandatory Handoff scope | Both rules must have severity overrides |
| C5 | NoWarn must NOT include CA1502/CA1506 | Feature purpose (enablement) | AC must verify absence from NoWarn in Directory.Build.props |
| C6 | Scope is enablement only, not remediation | Feasibility constraint | No code refactoring tasks; remediation feature created during /run when specific violations are identified |
| C7 | First `dotnet_diagnostic` override establishes precedent | No existing entries in .editorconfig | Override must be in correct `[*.cs]` section |

### Constraint Details

**C1: Suggestion Severity Requirement**
- **Source**: `Directory.Build.props:3` sets `TreatWarningsAsErrors=true`; any severity above `suggestion` causes build failure
- **Verification**: Grep `.editorconfig` for `CA1502.severity = suggestion` and `CA1506.severity = suggestion`
- **AC Impact**: AC#1 and AC#2 verify exact severity value is `suggestion`

**C2: .editorconfig-Only Changes**
- **Source**: F831 research confirmed rules are built-in; only `.editorconfig` override needed
- **Verification**: Implementation Contract constraint: "Only `.editorconfig` is modified"
- **AC Impact**: No dedicated AC; implementation constraint enforced by Implementation Contract. AC#1 and AC#2 verify .editorconfig content; AC#3 verifies build green.

**C3: Build Green Requirement**
- **Source**: `TreatWarningsAsErrors=true` promotes warnings to errors; `suggestion` is exempt from this promotion
- **Verification**: Run `dotnet build` and confirm exit code 0
- **AC Impact**: AC#3 verifies build success after .editorconfig change

**C4: Both Rules Must Be Enabled**
- **Source**: F831 Mandatory Handoff scope specifies both CA1502 and CA1506
- **Verification**: Grep `.editorconfig` for both `CA1502` and `CA1506` severity entries
- **AC Impact**: AC#1 and AC#2 each verify one rule; AC#4 verifies count ≥ 2

**C5: No NoWarn Suppression**
- **Source**: Feature purpose is enablement; suppressing via NoWarn defeats the goal
- **Verification**: Implementation Contract pre-condition: "Do NOT touch Directory.Build.props"
- **AC Impact**: No dedicated AC; implicit coverage via AC#3 (build green confirms suggestion severity is active, not suppressed). NoWarn absence is an implementation constraint, not a post-condition.

**C6: Enablement Only, Not Remediation**
- **Source**: Feasibility assessment; remediation deferred to separate feature
- **Verification**: No code refactoring tasks in Task table
- **AC Impact**: Task table contains only configuration and build verification tasks

**C7: First Diagnostic Override Precedent**
- **Source**: `.editorconfig:17-78` contains zero `dotnet_diagnostic` entries; this feature adds the first
- **Verification**: Grep `.editorconfig` for `dotnet_diagnostic` entries
- **AC Impact**: AC#4 verifies at least 2 `dotnet_diagnostic` entries exist in `.editorconfig`

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F831 | [DONE] | Research investigation that identified CA1502/CA1506 gap and created Mandatory Handoff |
| Related | F837 | [PROPOSED] | Sibling handoff from F831 (EnforceCodeStyleInBuild); independent implementation |
| Related | F813 | [DONE] | CA1510 NoWarn debt fix -- related analyzer management |

<!-- fc-phase-2-completed -->

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Static analysis rules are the SSOT for code quality enforcement" | CA1502 and CA1506 must be explicitly enabled as analyzer diagnostics | AC#1, AC#2 |
| "Complexity and coupling metrics must be visible as analyzer diagnostics" | Both rules must produce diagnostics at suggestion severity | AC#1, AC#2, AC#4, AC#5 |
| "Enablement at suggestion severity establishes the baseline" | Severity must be exactly `suggestion`; build must remain green | AC#1, AC#2, AC#3 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CA1502 severity override exists in .editorconfig | code | Grep(.editorconfig) | matches | `dotnet_diagnostic\.CA1502\.severity = suggestion` | [x] |
| 2 | CA1506 severity override exists in .editorconfig | code | Grep(.editorconfig) | matches | `dotnet_diagnostic\.CA1506\.severity = suggestion` | [x] |
| 3 | Build succeeds after enablement | build | dotnet build devkit.sln | succeeds | exit code 0 | [x] |
| 4 | dotnet_diagnostic entries count in .editorconfig | code | Grep(.editorconfig, pattern="dotnet_diagnostic") | gte | 2 | [x] |
| 5 | CA1502 or CA1506 diagnostics appear in build output | build | dotnet build devkit.sln 2>&1 \| grep CA150 | gte | 1 | [-] |

### AC Details

**AC#4: dotnet_diagnostic entries count in .editorconfig**
- **Test**: `Grep(.editorconfig, pattern="dotnet_diagnostic")`
- **Expected**: `gte 2` (at least 2 lines: one for CA1502, one for CA1506)
- **Rationale**: C4+C7 constraints -- both CA1502 and CA1506 must be enabled (C4), and this establishes the first dotnet_diagnostic entries in the repository (C7). Derivation: 2 rules = minimum 2 dotnet_diagnostic entries.

**AC#5: CA1502 or CA1506 diagnostics appear in build output**
- **Test**: `dotnet build devkit.sln 2>&1 | grep CA150`
- **Expected**: `gte 1` (at least 1 line matching CA1502 or CA1506)
- **Rationale**: Philosophy claims metrics "must be visible as analyzer diagnostics". AC#1-4 verify configuration exists; AC#5 verifies the enabled rules actually produce diagnostic output. Known high-complexity targets (e.g., `ErbParser.ParseString` CC >50) should trigger CA1502 at the default threshold of 25.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Enable CA1502 at suggestion severity via .editorconfig | AC#1 |
| 2 | Enable CA1506 at suggestion severity via .editorconfig | AC#2 |
| 3 | Establish first dotnet_diagnostic entries in the repository | AC#4 |
| 4 | Build must remain green (no build failures) | AC#3 |
| 5 | Enablement only -- no remediation (scope containment) | AC#1, AC#2 |
| 6 | Diagnostics are actually visible after enablement | AC#5 |

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

<!-- fc-phase-4-completed -->

## Technical Design

### Approach

Add two `dotnet_diagnostic` severity override entries to `.editorconfig` within the existing `[*.cs]` section. The entries are appended at the end of the `[*.cs]` block under a new `# Analyzer severity overrides` comment, establishing the first `dotnet_diagnostic` precedent in the repository.

The `suggestion` severity level is specifically chosen because `TreatWarningsAsErrors=true` in `Directory.Build.props` promotes `warning`-severity diagnostics to build errors. The .NET SDK does NOT promote `suggestion` to error, so the build remains green regardless of how many violations exist in the codebase.

No csproj modifications are needed. No NoWarn entries are added. The single `.editorconfig` file change satisfies all 5 ACs.

The two lines to add at the end of the `[*.cs]` section in `.editorconfig`:

```
# Analyzer severity overrides
dotnet_diagnostic.CA1502.severity = suggestion
dotnet_diagnostic.CA1506.severity = suggestion
```

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Append `dotnet_diagnostic.CA1502.severity = suggestion` inside `[*.cs]` section of `.editorconfig` |
| 2 | Append `dotnet_diagnostic.CA1506.severity = suggestion` inside `[*.cs]` section of `.editorconfig` |
| 3 | `suggestion` severity is not promoted by `TreatWarningsAsErrors=true`; build succeeds after the `.editorconfig` change |
| 4 | Two `dotnet_diagnostic` lines added satisfies `gte 2`; AC counts all `dotnet_diagnostic` occurrences in `.editorconfig` |
| 5 | Build output is captured and grepped for CA1502/CA1506; known high-complexity methods (e.g., `ErbParser.ParseString`) should trigger CA1502 at default threshold 25 |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Severity level for CA1502/CA1506 | `suggestion`, `warning`, `error` | `suggestion` | `TreatWarningsAsErrors=true` promotes `warning`→error causing build failure; `suggestion` is the only level that keeps the build green (C1 constraint) |
| Placement within `.editorconfig` | End of `[*.cs]` section, separate `[*.cs]` block, new section | End of existing `[*.cs]` section with `# Analyzer severity overrides` comment | Single `[*.cs]` block is simpler and avoids section duplication; comment creates a clear sub-group for future diagnostic overrides (C7 precedent constraint) |
| Number of entries | One combined comment block vs. individual lines | Two individual lines with shared comment header | Matches `.editorconfig` style (one entry per line); makes future addition of additional rules trivial |

### Interfaces / Data Structures

Not applicable. This feature modifies a configuration file only; no new interfaces, data structures, or APIs are introduced.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. N ACs : 1 Task allowed. No orphan Tasks. -->

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 4 | Add `# Analyzer severity overrides` comment and two `dotnet_diagnostic` severity lines to the `[*.cs]` section of `.editorconfig` | | [x] |
| 2 | 3, 5 | Run `dotnet build devkit.sln` and verify exit code 0 (build remains green after .editorconfig change) and confirm CA1502/CA1506 diagnostics appear in output | | [-] |

### Task Tags

- `[I]` — Investigation required before implementation (read source, measure, decide)

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `.editorconfig` `[*.cs]` section | Two `dotnet_diagnostic` lines appended |
| 2 | implementer | sonnet | `devkit.sln` | Build output confirming exit code 0 |

### Pre-conditions

- `.editorconfig` exists at repository root with a `[*.cs]` section (confirmed: lines 17-78)
- `Directory.Build.props` has `TreatWarningsAsErrors=true` (line 3) and `AnalysisLevel=latest-recommended` (line 4)
- No existing `dotnet_diagnostic` entries in `.editorconfig` (baseline: 0 occurrences)

### Execution Steps

**Step 1 — Edit `.editorconfig`** (Task 1)

Locate the `[*.cs]` section in `.editorconfig`. Append the following block at the end of the `[*.cs]` section, before any subsequent section header (or at end of file if `[*.cs]` is the last section):

```
# Analyzer severity overrides
dotnet_diagnostic.CA1502.severity = suggestion
dotnet_diagnostic.CA1506.severity = suggestion
```

Constraints:
- Only `.editorconfig` is modified. Do NOT touch `Directory.Build.props` or any `.csproj` file.
- Severity must be exactly `suggestion` (not `warning`, not `error`).
- Entries must be inside the `[*.cs]` section scope.

**Step 2 — Verify build** (Task 2)

Run from WSL:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build devkit.sln --blame-hang-timeout 10s'
```

Expected: exit code 0, `Build succeeded` in output. If build fails, STOP and report to user — do NOT modify severity or add suppressions autonomously.

### Rollback Plan

If the build fails after the `.editorconfig` change:
1. Revert the `.editorconfig` edit with `git checkout -- .editorconfig`
2. STOP and report the build failure details to the user
3. Do NOT promote severity, add NoWarn entries, or modify `.csproj` files as workarounds
4. Create a follow-up feature to address the root cause before re-attempting

### Error Handling

| Error | Action |
|-------|--------|
| Build fails with CA1502/CA1506 diagnostic errors | Impossible at `suggestion` severity — investigate if TreatWarningsAsErrors behavior has changed; STOP and report |
| `.editorconfig` has no `[*.cs]` section | STOP — report to user; do not create a new section without user approval |
| Existing `dotnet_diagnostic` entries found | STOP — report existing entries to user; they may conflict with this feature's precedent role |

---

## Mandatory Handoffs

<!-- CRITICAL: Mandatory Handoffs are for issues discovered DURING implementation that cannot be resolved within this feature's scope. -->
<!-- Option A: Create new feature immediately. Option B: Defer to existing planned feature. Option C: Track as Review Note [pending]. -->
<!-- Validation: Destination ID must be a concrete F{ID} — no TBD, no "future feature", no vague references. -->
<!-- DRAFT Creation: Handoff features are created as [DRAFT] immediately when handoff is identified. -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T01:00:00Z | START | initializer | Status [REVIEWED] → [WIP] | OK |
<!-- run-phase-1-completed -->
| 2026-03-06T01:02:00Z | INVESTIGATE | explorer | .editorconfig [*.cs] L17-82, no dotnet_diagnostic entries, Directory.Build.props confirmed | OK |
<!-- run-phase-2-completed -->
| 2026-03-06T01:04:00Z | IMPLEMENT | implementer | Task#1: Added dotnet_diagnostic CA1502+CA1506 to .editorconfig | OK |
| 2026-03-06T01:05:00Z | IMPLEMENT | implementer | Task#2: dotnet build devkit.sln — Build succeeded, 0 warnings, 0 errors | OK |
| 2026-03-06T01:06:00Z | DEVIATION | orchestrator | grep CA150 in build output | exit 1: suggestion-severity diagnostics not shown in dotnet build output (IDE-only) |
<!-- run-phase-4-completed -->
| 2026-03-06T01:08:00Z | VERIFY | ac-static-verifier | code ACs (AC#1,2,4) | PASS 3/3 |
| 2026-03-06T01:09:00Z | DEVIATION | ac-static-verifier | build ACs (AC#3,5) | exit 1: AC#3 dotnet not on Windows PATH (WSL required), AC#5 gte matcher unsupported for build type |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Related Features table (line 63) | F837 status listed as [DRAFT] but actual status is [PROPOSED]
- [fix] Phase3-Maintainability iter1: Risks table + C6 | Leak Prevention: remediation follow-up referenced as 'separate feature' without concrete ID; replaced with explicit deferral justification (targets unknown until enablement)
- [fix] Phase3-Maintainability iter2: Philosophy / AC Table | Philosophy 'must be visible' claim had no AC verifying diagnostics are produced; added AC#5 (build output grep for CA1502/CA1506)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F831](feature-831.md) - Roslynator Analyzers Investigation — parent Mandatory Handoff that created F836
- [Related: F837](feature-837.md) - Sibling handoff from F831 (EnforceCodeStyleInBuild); independent implementation
- [Related: F813](feature-813.md) - CA1510 NoWarn debt fix — related analyzer management
