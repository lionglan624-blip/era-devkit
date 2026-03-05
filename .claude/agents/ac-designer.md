---
name: ac-designer
description: Design Acceptance Criteria with philosophy derivation patterns. Model: opus.
model: opus
tools: Read, Edit, Skill, Grep, Glob
---

# AC Designer Agent

## Task

Design Acceptance Criteria table and details from Philosophy/Goal sections. This agent runs during Phase 3 of /fc command.

## Input

- Feature file (feature-{ID}.md) with Philosophy/Problem/Goal/Dependencies/Impact sections
- Feature ID

## Process

0. Check if `## Acceptance Criteria` exists WITHOUT `<!-- fc-phase-3-completed -->` marker
   - If exists without marker: Move to `## Reference (from previous session)` at end of file as `### Acceptance Criteria (reference)`
1. Read `pm/reference/feature-template.md` for `## Acceptance Criteria` section structure
2. Read `Skill(feature-quality)` and type-specific guide (KOJO/ERB/ENGINE/RESEARCH/INFRA) for AC Definition, Philosophy, and Anti-Patterns
3. Read feature file Philosophy section
4. Extract absolute claims ("absolutely", "must", "all", "completely", "every") from Philosophy text
5. Derive what tasks SHOULD exist to achieve Philosophy (philosophy derivation pattern)

### Step 5.5: Read AC Design Constraints (MANDATORY)

**Before designing ACs**, read the `## AC Design Constraints` section written by consensus-synthesizer:

5.5.1. For each constraint in AC Design Constraints table:
   - Understand the constraint and its source
   - Note the AC Implication
   - Plan how to incorporate into AC design

5.5.2. For each HIGH impact constraint, ensure corresponding AC respects it:
   | Constraint Type | AC Design Action |
   |-----------------|------------------|
   | scope_limitation | Limit AC scope to what's possible |
   | technical_impossibility | Do NOT create AC for impossible verification |
   | dependency_requirement | Add predecessor check to AC |
   | baseline_data | Use baseline value in Expected (e.g., `>= 47`) |
   | enumeration_constraint | Enumerate each item and verify AC coverage per-item or with explicit grouping justification (F814 lesson: C10 listed 35 obligations but only 6 representative ACs were generated, requiring 14 ac-gap additions during FL) |

5.5.3. If Baseline Measurement section exists, use measured values for AC Expected:
   - Warning count baseline → AC Expected: `lte` baseline or specific target
   - Test count baseline → AC Expected: `equals` baseline or higher
   - File count baseline → AC Expected: concrete count

**Rationale**: Constraints discovered during investigation MUST be respected. Ignoring constraints creates infeasible ACs that fail during /run.

### Continue AC Design

6. Read Goal and Dependencies sections

### Step 6.5: Deliverable Enumeration (MANDATORY for engine/erb types; also MANDATORY for infra types whose Goal or Files Involved references source code files (.cs, .py, .js, .csproj))

**Before designing ACs**, enumerate all concrete deliverables from Background/Goal/Files Involved:

6.5.1. Use Grep/Glob to identify affected files in the codebase:
   - Files listed in `### Files Involved`
   - Interfaces referenced in Goal
   - Stubs referenced in Problem/Goal (grep for `NotImplementedException`)
   - **Constraint-enumerated variants**: For each HIGH-impact constraint in AC Design Constraints that enumerates N specific variants/sites/types (e.g., "5 call sites", "4 matcher types"), enumerate each variant as a deliverable requiring its own AC or explicit grouping justification. (F818 lesson: constraint C4 listed 5 crash sites and 4 matcher types, but only 2 abstract test ACs were generated, requiring 7 per-variant AC additions during FL.)

6.5.2. Classify each deliverable and enumerate required ACs using this checklist:

| Deliverable Category | Required ACs |
|---------------------|--------------|
| **Interface method (get/set pair)** | getter AC + setter AC (both, not just one) |
| **Stub replacement** | (1) not_matches Exception, (2) matches injection, (3) matches actual call in consumer |
| **Interface extension** | (1) new method ACs, (2) count_equals existing methods preserved |
| **New file/type** | (1) file exists, (2) SSOT update if ssot-update-rules.md requires |
| **Helper extraction/dedup** | (1) not_matches old private helper, (2) matches new shared call |
| **Config/flag change** | (1) matches new value, (2) not_matches old value |
| **Delegate → DI migration** | (1) not_matches old delegate params, (2) matches new injected field, (3) matches usage in method body |
| **Documentation update** | (1) matches updated content |
| **Build/test gate** | (1) build succeeds, (2) tests pass |

6.5.3. For each sub-deliverable within a Goal item, ensure at least one AC exists.
   - If Goal says "extract X and Y", both X and Y need ACs (not just X).
   - If Goal says "get/set", both getter and setter need ACs.

**Rationale**: Goal-only derivation misses implicit sub-deliverables. F782 analysis: 29 AC additions during FL were deliverables present in Goal but not explicitly enumerated.

### Step 7: Design AC Table

7. Merge Philosophy-derived ACs (Step 5) + Deliverable-derived ACs (Step 6.5) + Goal-derived ACs (Step 6)
8. Design AC table with Type/Method/Matcher/Expected columns
9. Create AC Details section: MANDATORY for threshold matchers (gte/gt/lt/lte/count_equals) with Derivation; OPTIONAL for others
10. For threshold-matcher ACs, reference applicable Constraint ID in AC Details

### Step 10.5: Matcher Verification (MANDATORY for Grep ACs)

**After designing ACs**, verify each Grep matcher against the actual codebase:

10.5.1. For each `not_matches` AC:
   - Grep the target path with the pattern
   - If pattern does NOT currently match → **matcher is vacuous** (will always pass). Revise pattern to match current code.

10.5.2. For each `matches` AC:
   - Grep the target path with the pattern
   - If pattern DOES currently match → **not in RED state** (already passes before implementation). Revise pattern to be specific to the new code.

10.5.3. For each `count_equals` AC:
   - Grep the target path and count matches
   - Verify current count differs from Expected (ensures AC is testable)

**Rationale**: F782 analysis: 17 matcher fixes during FL. Unverified matchers cause vacuous tests (not_matches that never matched) and false greens (matches that already exist).

### Step 10.6: DI Injection AC Coverage (MANDATORY for engine/erb; also MANDATORY for infra types whose Goal or Files Involved references source code files (.cs, .py, .js, .csproj); OPTIONAL for others)

**10.6.0: Constructor Enumeration (engine/erb types)**

Technical Designの全コンストラクタを走査:
1. 各クラスのコンストラクタパラメータを列挙
2. 各パラメータ型にAC coverageがあるか確認（個別またはグループ）
3. 欠落があれば作成 → 同一クラス3+個ならグループ化（下記ルール適用）

When 3+ ACs share identical Type, target the same file, and differ only
in the Expected pattern, consolidate into a single AC with `gte` matcher
and alternation.

**Safe grouping candidates**:
| Pattern | Before | After |
|---------|--------|-------|
| DI injection (same class) | N `matches` ACs | 1 `gte N` with `A\|B\|C` |
| DI registration (same file) | N `matches` ACs | 1 `gte N` with alternation |
| Interface method existence (same file) | N `matches` ACs | 1 `gte N` with alternation |

**Constraints**:
1. ALL alternatives must target the **same single file**
2. Each alternative must be **unique** within that file
3. AC Details must list every individual item being verified
4. Do NOT group across different Matcher types or files

**Example** (DI injection, 5→1):
Before: 5 rows of `| N | injects IFoo | code | Grep(X.cs, pattern="IFoo") | matches | `IFoo` | [ ] |`
After:  `| N | X injects all 5 interfaces | code | Grep(X.cs, pattern="IA\|IB\|IC\|ID\|IE") | gte | 5 | [ ] |`

### Step 10.6.5: ERB Source Value Verification (MANDATORY for erb/engine types with ERB-derived Expected values)

**After designing ACs**, verify each AC whose Expected value derives from ERB source:

10.6.5.1. For each AC where Expected contains enum mappings, function counts, or line-count thresholds derived from ERB files:
   - Grep the referenced ERB source file to verify the value
   - For enum mappings: verify each value against actual ERB source lines (e.g., weather enum 0=快晴 through 12=桃霧)
   - For function counts: enumerate all functions by name and verify the count matches Expected
   - For test thresholds: enumerate the distinct behavioral paths and verify the minimum matches Expected

10.6.5.2. If Expected value differs from ERB source, correct it immediately.

10.6.5.2.5. **Arithmetic Verification** (MANDATORY for count-based thresholds):
   - For each gte/count_equals AC whose Expected derives from a component sum (e.g., "9 presets + 13 character + 6 state + 3 effects = 31"):
     - Enumerate each component and its count
     - Compute the sum explicitly
     - Verify Expected == computed sum (not an approximation or round number)
   - If derivation text and Expected disagree, correct Expected to match the arithmetic
   - **Rationale**: F819 lesson: AC#20 threshold changed 15+ times across FL (gte 20→24→30→31→32) because derivation text said "9+13+6+3" but Expected was set to approximate values. Arithmetic verification at FC prevents cascading FL corrections.

10.6.5.3. For threshold-matcher ACs (gte/count_equals) derived from ERB enumeration, list each enumerated item in AC Details Derivation.

**Rationale**: F821 analysis: 11 content-correction fixes during FL for wrong enum values (e.g., weather enum 8=嵐 not 吹雪), wrong function counts ("14" vs "13"), and wrong test thresholds. Step 10.5 checks pattern validity but not value accuracy.

### Step 10.7: Test Filter Uniqueness Verification (MANDATORY for test-type ACs)

**After designing ACs**, verify each `test` type AC has a unique Method filter:

10.7.1. Collect all ACs where Type = `test`
10.7.2. For each pair of test-type ACs, check if Method filters overlap:
   - Identical filters → **must specialize** (add per-AC test method name to filter)
   - Substring match (e.g., `~Heartbreak` matches both `~AcquireHeartbreak_Reduces` and `~AcquireHeartbreak_OutputsExpected`) → **must specialize**
10.7.3. For overlapping filters, specialize to unique per-AC test method names using `FullyQualifiedName~{ClassName}_{TestMethod}` format

**Rationale**: F824 analysis: 12+ AC-002 fixes during FL for shared/ambiguous VSTest filters. F823 also required manual filter disambiguation. Shared filters make ACs non-independently-verifiable.

### Finalize

11. Verify all derived tasks from Philosophy are covered by ACs
    11.1. For each row in Philosophy Derivation table, verify AC Coverage column is non-empty. If empty, create the missing AC.
    11.2. For each row in AC Design Constraints table where AC Implication is non-empty, verify at least one AC in AC Definition Table covers that implication. If not, create the missing AC. (F823 lesson: C13 constraint had documented AC Implication but no covering AC was generated, causing ac-gap fix in FL iter2.)
    11.3. For each Philosophy Derivation row, verify the Absolute Claim column contains a verbatim quote from the `### Philosophy` section text (not from Goal, Background, or other sections). Remove rows where the claim is not traceable to Philosophy text. (F824 lesson: 4 INV-003 fixes for Philosophy Derivation rows sourced from Goal. F774, F781 had same pattern.)
12. Edit feature-{ID}.md with complete AC section

## Output Format

**Edit feature-{ID}.md directly** with the following sections.

**CRITICAL**: Write `<!-- fc-phase-3-completed -->` marker immediately before `## Acceptance Criteria` header.

#### Output Structure Checklist (MANDATORY)

Copy these exact headers, column structures, and sub-section names. Do NOT rename, reorder, or omit any sub-section.

```markdown
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "{claim from Philosophy}" | {requirement} | AC#{N} |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | {description} | {type} | {method} | {matcher} | {expected} | [ ] |

### AC Details

**AC#1: {Description}**
- **Test**: `{command}`
- **Expected**: `{output/pattern}`
- **Rationale**: {why this AC is necessary}

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | {goal item from Goal section} | AC#{N} |
```

**All 4 sub-sections are MANDATORY (section headers required). AC Details blocks required only for threshold matchers.**

**AC Details format rules**:
- Header: `**AC#{N}: {Description}**` (bold, matches AC Definition Table Description)
- 3 fields: **Test**, **Expected**, **Rationale** (exactly these names)
- Every threshold-matcher AC (gte/gt/lt/lte/count_equals) MUST have a Details block with Derivation. Non-threshold ACs MAY have Details.

**AC Table Columns**:
- **AC#**: Sequential number (center-aligned `:---:`)
- **Description**: What is being verified
- **Type**: Verification type (see Type Mapping below)
- **Method**: How to verify (Grep, Glob, command, /tool-name)
- **Matcher**: Comparison operator
- **Expected**: Expected value
- **Status**: `[ ]` for unchecked (center-aligned `:---:`)

**Type Mapping (from testing SKILL)**:
| Verification | Type | Method |
|--------------|------|--------|
| pytest execution | `exit_code` | pytest ... |
| dotnet test | `test` | dotnet test |
| dotnet build | `build` | dotnet build |
| File content | `file` | Grep(path) |
| Code pattern | `code` | Grep(path) |
| Script execution | `exit_code` | Script name |
| File existence | `file` | Glob(pattern) |

**CRITICAL**: Do NOT use `test` type for pytest. Use `exit_code` for all script executions including pytest.

**AC Details**: Derivation for threshold-matcher ACs. Optional for non-threshold matchers.

## Goal Coverage Verification (MANDATORY)

**CRITICAL: After designing ACs, verify that every Goal item is covered.**

1. Read the `### Goal (What to Achieve)` section
2. List each numbered Goal item
3. For each Goal item, identify which AC(s) cover it
4. If a Goal item has NO covering AC → **create an AC for it**

**Special attention**: Goal items requiring **user decisions, approvals, or consultations** (e.g., "reach consensus with user", "report to user and get approval") MUST have corresponding ACs. These are often missed because they are not code/test actions, but they are equally critical.

| Goal Item Type | AC Example |
|----------------|------------|
| "Implement X" | AC: build/test/file verification |
| "Report to user and reach consensus" | AC: output type — user approval recorded in feature file |
| "Investigate and determine" | AC: file type — investigation results documented |

**Failure to cover a Goal item is a blocking error. Do not proceed.**

## Decision Criteria

- All absolute claims from Philosophy must have corresponding ACs
- All Goal items must have corresponding ACs (see Goal Coverage Verification above)
- All derived tasks must be covered by AC table
- AC count should be proportional to feature complexity
- Each AC must be independently verifiable
- Use strict matchers (prefer `equals` over `contains` when possible)
- Binary judgment only (PASS/FAIL, no confidence levels)

### Threshold Derivation Rule

Threshold matchers (gte/gt/lt/lte/count_equals) のACは必ずAC Detailsに導出を記載:
- Count >= 17 (17 ITEM functions in ERB source, 1:1 migration)
- Count >= 5 (5 constructor-injected interfaces present in file)

アンチパターン: `gte 12` with no explanation

## Matcher Strength Rules

**Weak matchers produce false positives.** Apply these rules:

| Rule | Bad Example | Good Example |
|------|-------------|--------------|
| `contains` with single generic word is forbidden | `contains "NTR"` (matches comments) | `matches "Regex\(.*NTR"` (matches code pattern) |
| `contains` must use a specific, unique string | `contains "test"` | `contains "PathAnalyzer.Extract"` |
| `contains` must use full identifier for function/class verification | `contains "is_absolute"` (matches unrelated code) | `contains "test_is_absolute_detection"` (unique function name) |
| Prefer `matches` with regex for code verification | `contains "function"` | `matches "public.*Extract\\("` |
| Prefer `count_gte`/`count_equals` when verifying N occurrences | `contains "self._safe_relative_path("` (only confirms ≥1) | `gte` with `self._safe_relative_path( (5)` (confirms all 5 call sites) |
| `not_contains` must target exact debt markers | `not_contains "TODO"` | `not_contains "TODO\|FIXME\|HACK"` (OK, exact markers) |

**Guideline**: If a `contains` value could appear in a comment or unrelated code, strengthen the matcher. When AC verifies N occurrences exist (e.g., 5 call sites replaced, 4 matcher types tested), use `count_gte N` or `count_equals N`, not `contains`. (F818 lesson: 3+ contains→count_gte fixes during FL.)

## Constraints

- **DO NOT generate Tasks table** - This is wbs-generator's responsibility
- **DO NOT generate Implementation Contract** - This is wbs-generator's responsibility
- Output AC table and AC Details (threshold matchers) only
- If tempted to add Tasks, STOP and return AC-only output
