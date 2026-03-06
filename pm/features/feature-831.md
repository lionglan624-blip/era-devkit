# Feature 831: Roslynator Analyzers Investigation

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T23:09:24Z -->

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

## Type: research

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Static analysis rules are the SSOT for code quality enforcement across the devkit codebase. Rule enablement decisions must be evidence-based, distinguishing between built-in .NET SDK analyzers and third-party packages, to prevent unnecessary dependency adoption and NoWarn debt accumulation.

### Problem (Current Issue)
Phase 22 Task 12 (`docs/architecture/migration/phase-20-27-game-systems.md:303`) prescribed "Roslynator.Analyzers package (500+ rules)" investigation targeting CA1502 (cyclomatic complexity), CA1506 (class coupling), and IDE0060 (unused parameters). However, all three rules are built-in .NET/Roslyn analyzers -- CA1502 and CA1506 are Microsoft.CodeAnalysis.NetAnalyzers rules (available via the existing `AnalysisLevel=latest-recommended` in `Directory.Build.props:4`), and IDE0060 is a Roslyn compiler diagnostic. The obligation chain (F814 -> F819 -> F829 -> F831) routed the task through three features without anyone verifying whether Roslynator is actually needed for these rules, because the original Task 12 conflated Roslynator-specific rules (RCS prefix) with standard .NET analyzer rules (CA/IDE prefix).

### Goal (What to Achieve)
Produce a concrete adoption recommendation for the Roslynator.Analyzers package by: (1) evaluating the three originally-requested rules (CA1502, CA1506, IDE0060) as built-in rules requiring only .editorconfig configuration, (2) assessing Roslynator's unique RCS-prefix rules for incremental value, (3) identifying the EnforceCodeStyleInBuild gap that prevents IDE-prefix rules from being enforced during `dotnet build`, (4) ensuring all recommendations cite specific evidence for traceability, and (5) tracking follow-up implementation for actionable findings.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is Roslynator investigation needed? | Phase 22 Task 12 prescribed a general "Roslynator Analyzers 導入調査" (500+ rules); downstream interpretation (MEMORY.md, F829) associated it with CA1502, CA1506, IDE0060 | `docs/architecture/migration/phase-20-27-game-systems.md:303`; `pm/features/feature-829.md` OB-11 |
| 2 | Why were these specific rules linked to Roslynator? | Downstream interpretation assumed built-in CA/IDE-prefix rules required the Roslynator package, without verifying rule ownership | MEMORY.md: "Roslyn Analyzers (CA1502, CA1506, IDE0060) planned as zero-token replacement -- links to Phase 22 Task 12 (Roslynator)" |
| 3 | Why was this conflation not caught earlier? | The obligation chain (F814->F819->F829) focused on routing the task, not investigating the question | F814 routed to F819; F819 declared out-of-scope; F829 routed to F831 |
| 4 | Why are CA1502/CA1506 not already active? | They exist in the built-in analyzer set but are set to severity=none at the `latest-recommended` analysis level | `Directory.Build.props:4` sets AnalysisLevel=latest-recommended |
| 5 | Why is IDE0060 not enforced during build? | `EnforceCodeStyleInBuild` is not set in any repo, so IDE-prefix rules are ignored by `dotnet build` | Absence confirmed across all repos |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Roslynator investigation obligation unresolved after 3 features | The three target rules (CA1502, CA1506, IDE0060) do not require Roslynator -- they are built-in .NET/Roslyn analyzers |
| Where | Phase 22 Task 12 text | Rule prefix ownership: CA = Microsoft.CodeAnalysis.NetAnalyzers, IDE = Roslyn built-in, RCS = Roslynator-specific |
| Fix | Install Roslynator package and configure | Enable built-in rules via .editorconfig; evaluate Roslynator RCS rules separately on cost/benefit |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Phase 22 Planning; created Task 12 which originated the Roslynator obligation |
| F819 | [DONE] | Clothing System; correctly declared Roslynator out-of-scope for its feature |
| F829 | [DONE] | Phase 22 Deferred Obligations Consolidation; routed obligation to F831 |
| F813 | [DONE] | Post-Phase Review 21; performed CA1510 NoWarn debt fix (related NoWarn management) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical feasibility | FEASIBLE | Three target rules are built-in; .editorconfig changes are trivial |
| Scope clarity | NEEDS_REVISION | Original goal assumed Roslynator needed; revised goal separates built-in from RCS evaluation |
| Risk level | FEASIBLE | Research type -- no code changes, only documentation/recommendations |
| Dependencies | FEASIBLE | No blocking dependencies; all prerequisites are [DONE] |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Build configuration | MEDIUM | Recommendation may lead to .editorconfig changes in follow-up feature |
| NoWarn debt | LOW | Research output informs debt management strategy; no direct changes |
| Cross-repo consistency | MEDIUM | Findings apply to devkit, core, and engine repos symmetrically |
| Developer experience | LOW | IDE-only rules (IDE0060) already visible in editors; build enforcement is the gap |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TreatWarningsAsErrors=true | `Directory.Build.props:3` | Any newly-enabled rule at warning severity causes immediate build break |
| AnalysisLevel=latest-recommended | `Directory.Build.props:4` | CA1502/CA1506 exist but are severity=none at this level; requires explicit .editorconfig override |
| EnforceCodeStyleInBuild not set | Absence in all repos | IDE-prefix rules (including IDE0060) are silently ignored during `dotnet build` |
| 25 existing NoWarn rules | `Directory.Build.props:23` | Adding Roslynator would compound existing suppression debt |
| 5-repo split | Architecture | Directory.Build.props/.editorconfig changes affect only the repo where applied |
| Cross-repo config symmetry | devkit + core share identical config | Recommendations must account for symmetric application |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Roslynator adoption causes hundreds of new warnings/build breaks | HIGH | HIGH | Recommend REJECT unless RCS rules provide compelling value |
| CA1502 flags many methods in parser code (ErbParser.cs: 1416 lines, 131 branches) | HIGH | MEDIUM | Recommend starting at suggestion severity, promote after remediation |
| CA1506 flags tightly-coupled tool classes | MEDIUM | MEDIUM | Set generous threshold or start at suggestion severity |
| IDE0060 false positives on interface implementations | LOW | LOW | IDE0060 already excludes interface implementations by default |
| RCS rules conflict with .editorconfig style preferences | MEDIUM | MEDIUM | If adopted, use roslynator_analyzers.enabled_by_default=false |
| Cross-repo configuration drift after applying recommendations | MEDIUM | LOW | Document symmetric change requirements in recommendation |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Research type produces no code changes | Feature type: research | ACs verify documentation/recommendation artifacts only |
| C2 | CA1502/CA1506/IDE0060 are NOT Roslynator rules | .NET SDK documentation; 3/3 investigator consensus | ACs must not conflate built-in rules with Roslynator; recommendation must address them separately |
| C3 | Existing 25-rule NoWarn debt | `Directory.Build.props:23` | Recommendation must account for existing debt; cost/benefit must include NoWarn impact |
| C4 | EnforceCodeStyleInBuild gap | Absence in all repos | Recommendation must address this gap for IDE-prefix rule enforcement |
| C5 | Must address Roslynator RCS rules specifically | Task 12 scope (Phase 22) | Cannot redirect entirely without evaluating Roslynator's unique RCS-prefix value |

### Constraint Details

**C1: Research Type -- No Code Changes**
- **Source**: Feature type: research (feature-template.md granularity guide)
- **Verification**: Type field = research
- **AC Impact**: ACs verify report completeness and recommendation quality, not code changes

**C2: Built-in vs Roslynator Rule Distinction**
- **Source**: 3/3 investigators confirmed CA/IDE prefix = built-in, RCS prefix = Roslynator-specific
- **Verification**: Microsoft documentation for CA1502, CA1506; Roslyn source for IDE0060
- **AC Impact**: Report must contain per-rule evaluation with correct ownership attribution

**C3: NoWarn Debt Context**
- **Source**: `Directory.Build.props:23` contains 25 suppressed CA rules
- **Verification**: Count NoWarn entries in Directory.Build.props
- **AC Impact**: Adoption recommendation must include NoWarn impact assessment

**C4: EnforceCodeStyleInBuild Gap**
- **Source**: Explorer 3 identified absence of EnforceCodeStyleInBuild across all repos
- **Verification**: Grep for EnforceCodeStyleInBuild in all Directory.Build.props files
- **AC Impact**: Recommendation must address whether to enable this property for IDE rule enforcement

**C5: Roslynator RCS-Prefix Evaluation**
- **Source**: Phase 22 Task 12 explicitly calls for Roslynator.Analyzers package investigation
- **Verification**: Task 12 text in `docs/architecture/migration/phase-20-27-game-systems.md:303`
- **AC Impact**: Report must evaluate RCS-prefix rules for incremental value beyond built-in analyzers

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F814 | [DONE] | Phase 22 Planning; created Task 12 which originated the Roslynator obligation |
| Related | F819 | [DONE] | Clothing System; declared Roslynator out-of-scope |
| Related | F829 | [DONE] | Phase 22 Deferred Obligations Consolidation; routing origin |
| Related | F813 | [DONE] | Post-Phase Review 21; CA1510 NoWarn debt fix (related NoWarn management) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Static analysis rules are the SSOT for code quality enforcement" | Report must establish authoritative rule ownership (CA/IDE/RCS prefix mapping) | AC#1 |
| "Rule enablement decisions must be evidence-based" | Each recommendation must cite specific evidence (rule counts, NoWarn impact, build behavior) | AC#5 |
| "distinguishing between built-in .NET SDK analyzers and third-party packages" | Report must evaluate built-in rules and Roslynator RCS rules in separate sections | AC#1, AC#2 |
| "prevent unnecessary dependency adoption and NoWarn debt accumulation" | Adoption recommendation must account for NoWarn debt impact | AC#4 |
| "prevent unnecessary dependency adoption and NoWarn debt accumulation" | Research output must feed tracked follow-up pipeline | AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Per-rule ownership attribution for CA1502, CA1506, IDE0060 | file | Grep(feature-831.md, pattern="^### Rule Ownership Attribution") | matches | `^### Rule Ownership Attribution` | [x] |
| 2 | RCS-prefix incremental value assessment | file | Grep(feature-831.md, pattern="^### RCS-Prefix Incremental Value") | matches | `^### RCS-Prefix Incremental Value` | [x] |
| 3 | EnforceCodeStyleInBuild recommendation | file | Grep(feature-831.md, pattern="^### EnforceCodeStyleInBuild Gap Analysis") | matches | `^### EnforceCodeStyleInBuild Gap Analysis` | [x] |
| 4 | Roslynator adoption recommendation with NoWarn impact | file | Grep(feature-831.md, pattern="^Verdict:.*(ADOPT|REJECT|CONDITIONAL)") | matches | `^Verdict:.*(ADOPT|REJECT|CONDITIONAL)` | [x] |
| 5 | Evidence-based traceability per recommendation | file | Grep(feature-831.md, pattern="^- Evidence:") | gte | 3 | [x] |
| 6 | Follow-up implementation tracking via Mandatory Handoffs | file | Grep(feature-831.md, pattern="^\| .+ \| .+ \| Feature \|") | gte | 1 | [x] |

### AC Details

**AC#6: Follow-up implementation tracking via Mandatory Handoffs**
- **Test**: `Grep(pm/features/feature-831.md, pattern="^\| .+ \| .+ \| Feature \|")`
- **Expected**: `gte 1`
- **Derivation**: Philosophy requires "prevent unnecessary dependency adoption and NoWarn debt accumulation" — recommendations without tracked follow-up risk being forgotten. At least one Mandatory Handoff entry must exist after the verdict is written. Pattern `^\| .+ \| .+ \| Feature \|` specifically matches the 7-column Mandatory Handoffs table structure (Issue | Reason | Destination | ...) where Destination = "Feature", avoiding false positives from other tables.
- **Rationale**: Research features that produce actionable recommendations must track follow-up implementation to prevent leak.

**AC#5: Evidence-based traceability per recommendation**
- **Test**: `Grep(pm/features/feature-831.md, pattern="^- Evidence:")`
- **Expected**: `gte 3`
- **Derivation**: 3 evidence entries minimum: one for built-in rule evaluation (CA1502/CA1506/IDE0060), one for RCS-prefix assessment, one for EnforceCodeStyleInBuild gap. Each of the 3 Goal sub-items requires its own evidence citation to satisfy Philosophy's "must be evidence-based" requirement.
- **Rationale**: Philosophy requires "evidence-based" decisions. Each recommendation must trace to specific findings, not assertions.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Evaluate CA1502, CA1506, IDE0060 as built-in rules requiring only .editorconfig configuration | AC#1 |
| 2 | Assess Roslynator's unique RCS-prefix rules for incremental value | AC#2 |
| 3 | Identify the EnforceCodeStyleInBuild gap for IDE-prefix rule enforcement | AC#3 |
| 4 | Produce a concrete adoption recommendation for Roslynator.Analyzers package | AC#4 |
| 5 | Evidence-based traceability across all recommendations | AC#5 |
| 6 | Follow-up implementation tracking ensures recommendations are acted upon | AC#6 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature produces a self-contained investigation report by writing research sections directly into `feature-831.md`. Because the feature type is `research` and all 6 ACs use `Grep(feature-831.md, ...)` to verify content in the file itself, the implementation method is: during `/run`, write the required report sections into this file under a new `## Research Report` top-level section.

The three report sub-sections (Rule Ownership Attribution, RCS-Prefix Incremental Value, EnforceCodeStyleInBuild Gap Analysis) map 1:1 to AC#1, AC#2, and AC#3. The adoption verdict (AC#4) is written as the closing line of the report. Each sub-section includes at least one `- Evidence:` line (AC#5, minimum 3 total).

This approach satisfies all ACs without creating external files. The research content lives entirely inside `feature-831.md`, which is appropriate for a self-contained investigation-report variant of the research type (as opposed to a planning-type research feature that produces sub-feature files).

The implementation requires two primary activities during `/run`:
1. Investigation: Read `Directory.Build.props`, `.editorconfig`, and cross-check Microsoft documentation for rule prefix ownership and EnforceCodeStyleInBuild behavior.
2. Authoring: Write the three `###` sub-sections plus the `Verdict:` line into a `## Research Report` section.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Write `### Rule Ownership Attribution` section inside `## Research Report`. Section must contain per-rule evaluation: CA1502 = Microsoft.CodeAnalysis.NetAnalyzers (built-in), CA1506 = Microsoft.CodeAnalysis.NetAnalyzers (built-in), IDE0060 = Roslyn compiler diagnostic (built-in). Includes `- Evidence:` line citing Directory.Build.props AnalysisLevel and .NET SDK documentation. |
| 2 | Write `### RCS-Prefix Incremental Value` section inside `## Research Report`. Section must evaluate Roslynator RCS-prefix rules on cost/benefit axis given 25-rule existing NoWarn debt. Includes `- Evidence:` line citing Roslynator rule count, NoWarn debt count, and build-break risk. |
| 3 | Write `### EnforceCodeStyleInBuild Gap Analysis` section inside `## Research Report`. Section must confirm EnforceCodeStyleInBuild absence across repos and assess whether setting it would cause IDE0060 enforcement. Includes `- Evidence:` line citing Grep result of EnforceCodeStyleInBuild across all Directory.Build.props files. |
| 4 | Write `Verdict: ADOPT`, `Verdict: REJECT`, or `Verdict: CONDITIONAL` as a standalone line at the end of the report (or inside the report after the three sections). The regex `^Verdict:.*(ADOPT\|REJECT\|CONDITIONAL)` must match at line start — no leading spaces or markdown markers. |
| 5 | Each of the three `###` sections contains at least one `^- Evidence:` line at line start (no leading spaces). Three sections = minimum 3 evidence lines, satisfying `gte 3`. |
| 6 | Task 5 populates the Mandatory Handoffs table with at least one entry. The Grep pattern `^\| .+ \| .+ \| Feature \|` matches the 7-column Mandatory Handoffs table rows where Destination = "Feature" (3rd column), avoiding false positives from other tables with fewer columns or different column content. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Report section placement | A: New top-level `## Research Report` section; B: Inline within existing Background/Goal sections | A: New `## Research Report` section | ACs use Grep on the whole file — a dedicated section prevents pattern collision with planning text and keeps the report findable. AC#1-3 patterns (`^### Rule Ownership Attribution`, etc.) are H3 anchors that work at any nesting depth, but a dedicated H2 parent provides clear separation from fc-generated sections. |
| Verdict line format | A: `Verdict: REJECT` standalone line; B: `**Verdict**: REJECT` (bold markdown); C: `- Verdict: REJECT` (list item) | A: `Verdict: REJECT` standalone line | AC#4 pattern is `^Verdict:.*` — must start the line. Bold markdown `**Verdict**:` and list prefix `- Verdict:` both fail the regex. Plain text with no leading characters is the only safe choice. |
| Evidence line format | A: `- Evidence: {text}` at line start; B: `  - Evidence:` (indented); C: prose paragraph | A: `- Evidence:` at line start (no indent) | AC#5 pattern is `^- Evidence:` — must match at column 0. Indented list items fail. Unindented bullet is the correct format. |
| Roslynator adoption recommendation | A: ADOPT (install package); B: REJECT (rely on built-in rules); C: CONDITIONAL (adopt with scoped config) | Determined during investigation, not pre-decided | Risk table in feature-831.md rates Roslynator adoption causing hundreds of new warnings as HIGH/HIGH. Preliminary lean toward CONDITIONAL (roslynator_analyzers.enabled_by_default=false, adopt only specific RCS rules) or REJECT. Final verdict requires reading actual RCS rule inventory. |
| EnforceCodeStyleInBuild recommendation | A: Enable immediately; B: Defer to separate feature; C: Document gap without recommending change | Determined during investigation | C3 constraint (TreatWarningsAsErrors=true) means enabling EnforceCodeStyleInBuild risks immediate build breaks. Gap analysis section documents the risk; enabling is a separate follow-on feature if recommended. |

### Interfaces / Data Structures

<!-- Optional: Define new interfaces, data structures, or APIs if applicable -->

This is a research feature. No interfaces or data structures are defined. The only "data structure" is the report section format:

The report uses H3 sections matching the AC patterns (Rule Ownership Attribution, RCS-Prefix Incremental Value, EnforceCodeStyleInBuild Gap Analysis) plus a `Verdict:` line. See Implementation Contract for execution details.

The `Verdict:` line must appear at column 0, with no indentation, bold markers, or list prefix.

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#5 self-referential risk: `^- Evidence:` pattern will scan the entire feature-831.md including this Technical Design section. The phrase `- Evidence:` appears in the Interfaces / Data Structures code block above as a comment placeholder. This is inside a fenced code block — ripgrep does NOT skip code blocks, so the pattern `^- Evidence:` will match the line `- Evidence: {citation}` in the code snippet, causing AC#5 to over-count. | AC Definition Table (AC#5) | Update the code block example in Technical Design Interfaces section to use a non-matching placeholder (e.g., `- Evidence: TBD` matches but will be counted, which is acceptable since gte 3 is the threshold and code block lines are collateral matches that only inflate the count, not reduce it). Actually, this is a false alarm — `gte 3` means MORE matches is fine; over-counting does not cause a FAIL. No fix required. |

---

<!-- fc-phase-5-completed -->

## Research Report

### Rule Ownership Attribution

| Rule | Owner | Package | Built-in | Current Severity (AnalysisLevel=latest-recommended) |
|------|-------|---------|:--------:|------------------------------------------------------|
| CA1502 | Microsoft | Microsoft.CodeAnalysis.NetAnalyzers | Yes | none (disabled by default) |
| CA1506 | Microsoft | Microsoft.CodeAnalysis.NetAnalyzers | Yes | none (disabled by default) |
| IDE0060 | Roslyn compiler | Built-in IDE analyzer | Yes | none during `dotnet build` (requires EnforceCodeStyleInBuild=true) |

Enabling CA1502 and CA1506 requires an explicit `.editorconfig` override (e.g., `dotnet_diagnostic.CA1502.severity = warning`) because `AnalysisLevel=latest-recommended` sets them to `severity=none`. They do not require the Roslynator.Analyzers package. IDE0060 requires both `.editorconfig` severity promotion and `EnforceCodeStyleInBuild=true` to be enforced during `dotnet build`; without the latter property, IDE-prefix rules are silently ignored by the compiler regardless of `.editorconfig` settings.

- Evidence: `Directory.Build.props:4` sets `AnalysisLevel=latest-recommended`; Microsoft .NET SDK documentation confirms CA1502 and CA1506 are Microsoft.CodeAnalysis.NetAnalyzers rules with `severity=none` at `latest-recommended`; Roslyn compiler documentation confirms IDE0060 is a built-in IDE analyzer not enforced during build unless `EnforceCodeStyleInBuild=true`.

### RCS-Prefix Incremental Value

| Factor | Assessment |
|--------|------------|
| Roslynator rule count | 500+ RCS-prefix rules (code simplification, readability, naming, refactoring) |
| Build-break risk | HIGH — adding Roslynator under TreatWarningsAsErrors=true causes potentially hundreds of new build errors on first run |
| Existing NoWarn debt | 25 suppressed CA rules already exist in Directory.Build.props; Roslynator would compound this debt |
| Incremental value over built-in analyzers | Marginal for this project — the three originally-requested rules (CA1502, CA1506, IDE0060) are all built-in; Roslynator's unique value is limited to stylistic RCS rules not covered by Microsoft.CodeAnalysis.NetAnalyzers |
| Mitigation available | `roslynator_analyzers.enabled_by_default=false` in `.editorconfig` allows explicit opt-in per rule, but requires per-rule review of 500+ candidates before enabling any |

The cost/benefit ratio is unfavorable: adopting Roslynator.Analyzers would require an upfront audit of 500+ rules to identify those worth enabling, plus remediation of any violations found, all under `TreatWarningsAsErrors=true` which converts every new warning into a build break. Given that the three originally-requested rules are built-in and require only `.editorconfig` changes, Roslynator adds no value for the stated goal. The existing 25-rule NoWarn debt is already an acknowledged liability; adding a third-party package with 500+ rules risks compounding that debt significantly.

Recommendation: Do not adopt Roslynator.Analyzers. The incremental value is marginal and the build-break risk is high given the current configuration.

- Evidence: NoWarn count = 25 rules in `Directory.Build.props:23`; Roslynator.Analyzers NuGet package provides 500+ RCS-prefix rules; `TreatWarningsAsErrors=true` in `Directory.Build.props:3` converts all new warnings to errors.

### EnforceCodeStyleInBuild Gap Analysis

`EnforceCodeStyleInBuild` is absent from all three repos' `Directory.Build.props` files:

| Repo | Directory.Build.props | EnforceCodeStyleInBuild present? |
|------|----------------------|:---------------------------------:|
| devkit | `C:\Era\devkit\Directory.Build.props` | No |
| core | `C:\Era\core\Directory.Build.props` | No |
| engine | `C:\Era\engine\Directory.Build.props` | No |

Effect of absence: IDE-prefix rules (including IDE0060) are silently ignored during `dotnet build`. They remain visible in editors and IDE analysis, but do not produce build warnings or errors. This means the IDE0060 rule (Remove unused parameter) is currently not enforced at the CI/build level in any repo.

Risk of enabling: Setting `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` would activate all IDE-prefix rules configured in `.editorconfig`. Under `TreatWarningsAsErrors=true`, any IDE rule at `warning` severity immediately becomes a build error. The full scope of violations is unknown without a baseline scan; enabling this property without first auditing the codebase could cause widespread build breaks.

Recommendation: Enable `EnforceCodeStyleInBuild` in a separate dedicated feature with a staged approach — first scan at `suggestion` severity, remediate violations, then promote to `warning`. Do not enable as part of this feature.

- Evidence: Grep for `EnforceCodeStyleInBuild` returned zero matches in `devkit/Directory.Build.props`, `core/Directory.Build.props`, and `engine/Directory.Build.props`; `TreatWarningsAsErrors=true` in `Directory.Build.props:3` confirms enabling the property carries immediate build-break risk.

Verdict: REJECT — Roslynator.Analyzers package adoption is not justified; CA1502, CA1506, and IDE0060 are built-in rules requiring only .editorconfig and EnforceCodeStyleInBuild configuration

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 5 | Read `Directory.Build.props` and .NET SDK documentation; write `### Rule Ownership Attribution` section inside `## Research Report` with per-rule evaluation (CA1502, CA1506, IDE0060) and at least one `- Evidence:` line | | [x] |
| 2 | 2, 5 | Read Roslynator rule inventory and assess RCS-prefix rules against existing 25-rule NoWarn debt; write `### RCS-Prefix Incremental Value` section with cost/benefit evaluation and at least one `- Evidence:` line | | [x] |
| 3 | 3, 5 | Grep all `Directory.Build.props` files for `EnforceCodeStyleInBuild`; write `### EnforceCodeStyleInBuild Gap Analysis` section documenting the gap and risk of enabling under `TreatWarningsAsErrors=true` with at least one `- Evidence:` line | | [x] |
| 4 | 4 | Write `Verdict: ADOPT`, `Verdict: REJECT`, or `Verdict: CONDITIONAL` as a standalone unindented line at the end of `## Research Report` with rationale, based on T1-T3 findings | | [x] |
| 5 | 6 | Based on verdict outcome (T4), populate Mandatory Handoffs table with follow-up feature entries for: (a) .editorconfig rule enablement if CA/IDE rules recommended, (b) EnforceCodeStyleInBuild enablement if recommended, (c) Roslynator RCS adoption if verdict is ADOPT/CONDITIONAL | | [x] |

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
| 1 | implementer | sonnet | `Directory.Build.props`, .NET SDK documentation, Roslynator rule list | `### Rule Ownership Attribution` section in `feature-831.md` |
| 2 | implementer | sonnet | Roslynator RCS rule inventory, `Directory.Build.props` NoWarn count | `### RCS-Prefix Incremental Value` section in `feature-831.md` |
| 3 | implementer | sonnet | Grep result: `EnforceCodeStyleInBuild` across all repos' `Directory.Build.props` | `### EnforceCodeStyleInBuild Gap Analysis` section in `feature-831.md` |
| 4 | implementer | sonnet | T1-T3 findings | `Verdict:` line in `feature-831.md` |
| 5 | implementer | sonnet | T4 verdict outcome | Mandatory Handoffs entries in `feature-831.md` |

### Pre-conditions

- `pm/features/feature-831.md` is the implementation target — all deliverables are written as new sections within this file
- No code files are modified; this is an authoring-only feature
- Append a new top-level `## Research Report` section to the file (before `## Tasks`) containing the three `###` sub-sections and the `Verdict:` line

### Execution Order

1. **Task 1 — Rule Ownership Attribution**
   - Read `C:\Era\devkit\Directory.Build.props` (confirm `AnalysisLevel=latest-recommended`)
   - Confirm CA1502 and CA1506 are Microsoft.CodeAnalysis.NetAnalyzers rules (built-in at `AnalysisLevel=latest-recommended`, severity=none)
   - Confirm IDE0060 is a Roslyn compiler diagnostic (built-in, not Roslynator)
   - Write `### Rule Ownership Attribution` under `## Research Report` with per-rule evaluation table and `- Evidence:` line

2. **Task 2 — RCS-Prefix Incremental Value**
   - Count existing NoWarn rules in `Directory.Build.props` (expected ~25)
   - Assess Roslynator RCS-prefix rules on cost/benefit axis: rule count, build-break risk under `TreatWarningsAsErrors=true`, incremental value over built-in analyzers
   - Write `### RCS-Prefix Incremental Value` with evaluation and `- Evidence:` line

3. **Task 3 — EnforceCodeStyleInBuild Gap Analysis**
   - Grep for `EnforceCodeStyleInBuild` in: `C:\Era\devkit\Directory.Build.props`, `C:\Era\core\Directory.Build.props`, `C:\Era\engine\Directory.Build.props`
   - Confirm absence across all repos
   - Assess risk of enabling under `TreatWarningsAsErrors=true` (IDE-prefix rules silently ignored during `dotnet build` without this property)
   - Write `### EnforceCodeStyleInBuild Gap Analysis` with gap confirmation and `- Evidence:` line

4. **Task 4 — Verdict**
   - Based on T1-T3 findings, write `Verdict: ADOPT`, `Verdict: REJECT`, or `Verdict: CONDITIONAL` as a plain unindented line (no bold markers, no list prefix, no leading spaces)
   - Add rationale on the same line or following line
   - Verify AC#4 regex `^Verdict:.*(ADOPT|REJECT|CONDITIONAL)` matches

5. **Task 5 — Follow-up Tracking**
   - Read the Verdict line written in Task 4
   - Based on the verdict:
     - If ADOPT or CONDITIONAL: Add Mandatory Handoff entry for Roslynator RCS rule adoption feature
     - If any CA/IDE rule recommended for enablement: Add Mandatory Handoff entry for .editorconfig configuration feature
     - If EnforceCodeStyleInBuild recommended: Add Mandatory Handoff entry for EnforceCodeStyleInBuild enablement feature
   - Each Mandatory Handoff entry: Issue = recommendation summary, Reason = verdict rationale, Destination = "Feature" (new), Destination ID = "TBD (create during /run)", Creation Task = "T5"
   - At least one entry must exist to satisfy AC#6

### Report Section Format

Report section names match AC#1-3 patterns exactly; Verdict line matches AC#4. See AC Definition Table for patterns.

### Success Criteria

- `Grep(feature-831.md, "^### Rule Ownership Attribution")` → matches (AC#1)
- `Grep(feature-831.md, "^### RCS-Prefix Incremental Value")` → matches (AC#2)
- `Grep(feature-831.md, "^### EnforceCodeStyleInBuild Gap Analysis")` → matches (AC#3)
- `Grep(feature-831.md, "^Verdict:.*(ADOPT|REJECT|CONDITIONAL)")` → matches (AC#4)
- `Grep(feature-831.md, "^- Evidence:")` → gte 3 matches (AC#5)
- `Grep(feature-831.md, "^\| .+ \| .+ \| Feature \|")` → gte 1 matches (AC#6)

### Error Handling

- If `EnforceCodeStyleInBuild` is found in any repo's `Directory.Build.props`: document the finding accurately; do not assert absence if not confirmed
- If Roslynator RCS rule count cannot be verified from available documentation: note the limitation in the Evidence line and proceed with best available data
- If verdict is ambiguous after investigation: STOP → Ask user for guidance before writing Verdict line

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Enable CA1502 and CA1506 via .editorconfig override | CA1502 (cyclomatic complexity) and CA1506 (class coupling) are built-in rules disabled at AnalysisLevel=latest-recommended; explicit .editorconfig severity promotion required | Feature | F836 | T5 | [x] | — |
| Enable EnforceCodeStyleInBuild for IDE-prefix rule enforcement | EnforceCodeStyleInBuild is absent from all repos; IDE0060 and other IDE-prefix rules are silently ignored during dotnet build; staged enablement needed | Feature | F837 | T5 | [x] | — |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T14:00:00Z | PHASE_START | orchestrator | Phase 1: Initialize | Status [REVIEWED] → [WIP] |
| 2026-03-06T14:01:00Z | PHASE_START | orchestrator | Phase 2: Investigation (research) | Artifacts confirmed: Directory.Build.props, EnforceCodeStyleInBuild absent in all 3 repos |
| 2026-03-06 14:10 | START | implementer | Tasks 1-5 | Research Report authoring |
| 2026-03-06 14:15 | END | implementer | Tasks 1-5 | SUCCESS — Research Report written; F836/F837 created; Handoffs recorded |
| 2026-03-06T14:16:00Z | PHASE_START | orchestrator | Phase 4: Implementation (research) | All 5 Tasks completed, all 6 ACs verified GREEN |
| 2026-03-06T14:20:00Z | PHASE_START | orchestrator | Phase 7: Verification | AC lint OK, ac-tester 6/6 PASS, all ACs [x] |
| 2026-03-06T14:22:00Z | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: Transferred column [ ] for F836/F837 (fixed to [x]) |
| 2026-03-06T14:23:00Z | PHASE_START | orchestrator | Phase 8: Post-Review | 8.1 fixed; 8.2 skip (no extensibility); 8.3 N/A (research) |
| 2026-03-06T14:25:00Z | DEVIATION | Bash | ac-static-verifier.py --feature 831 --ac-type file | exit code 1, 0/6 passed — parser cannot handle Grep(file, pattern) method format; actual ACs verified 6/6 PASS via ac-tester |
| 2026-03-06T14:26:00Z | DEVIATION | Bash | verify-logs.py --scope feature:831 | exit code 1, ERR:1/6 — stale static verifier results |
| 2026-03-06T14:28:00Z | PHASE_START | orchestrator | Phase 9: Report & Approval | 3 DEVIATIONs (1 fixed, 2 PRE-EXISTING tool); user approved |
| 2026-03-06T14:30:00Z | PHASE_START | orchestrator | Phase 10: Finalize | Status [WIP] → [DONE]; index updated |
| 2026-03-06T14:30:00Z | CodeRabbit | Skip (research) | - |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-5-completed -->
<!-- run-phase-6-completed -->
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->
<!-- run-phase-9-completed -->
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: pm/features/feature-831.md (between Review Notes and Links) | Missing ## Improvement Log section
- [fix] Phase3-Maintainability iter2: pm/features/feature-831.md ## Mandatory Handoffs | Missing follow-up tracking for research recommendations (added Task 5, AC#6, Implementation Contract Phase 5)
- [fix] Phase3-Maintainability iter2: pm/features/feature-831.md ## Tasks / ## Implementation Contract | No task to create follow-up features based on verdict (added Task 5 with AC#6)
- [fix] Phase2-Review iter3: pm/features/feature-831.md AC#6 | AC#6 regex pattern matched 7 pre-existing table rows (vacuous test); narrowed to `^\| .+ \| .+ \| Feature \|` targeting Mandatory Handoffs 7-column structure
- [fix] Phase2-Uncertain iter3: pm/features/feature-831.md Goal Coverage Table | Goal items 5-6 not in Goal text; expanded Goal to include (4) evidence traceability and (5) follow-up tracking
- [problem-fix] Step9.5: ac-test:ac-static-verifier cannot parse Grep(file, pattern) method format — ACs verified 6/6 PASS via ac-tester; tool limitation is PRE-EXISTING

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 831 (2026-03-06)
- [revised] Goal Coverage逆検証ステップ追加 (Step 11.4): Goal Coverage表の各行がGoal本文にtrace可能か検証 → `.claude/agents/ac-designer.md` (Opus指摘: ac-designerが正しいターゲット、validation-onlyステップとして追加)
- [rejected] Improvement Logセクション検出 (V1n) — on-demandセクションのためFC品質チェック対象外。275+全featureがfalse positiveになる
- [accept] RESEARCH.md Issue 18 (follow-up tracking) — F831教訓として既に適用済み
- [accept] RESEARCH.md Issue 17/18 (vacuous pattern prevention) — F831教訓として既に適用済み

---

<!-- fc-phase-6-completed -->
## Links
- [Related: F814](feature-814.md) - Phase 22 Planning (original assignment)
- [Related: F819](feature-819.md) - Clothing System (declared out-of-scope)
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (routing origin)
- [Related: F813](feature-813.md) - Post-Phase Review 21 (NoWarn debt fix)
