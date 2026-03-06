---
name: feature-quality
description: REQUIRED before writing any feature-{ID}.md file. Skipping causes FL review failures and rework. Contains type-specific checklists (kojo/engine/infra) that catch common mistakes. Call this skill FIRST when creating features manually or via /fc.
---

# Feature Quality Reference

> **Purpose**: Prevent common FL review issues by checking quality at feature creation time.
> **Last Updated**: 2026-02-01

---

## Common Checklist (All Types)

### Dependencies & Links
- [ ] All interface dependencies listed in Dependencies table
- [ ] Related features (same phase) cross-linked in Links section
- [ ] Predecessor/Successor relationships documented

### Scope Discipline
- [ ] Out-of-scope issues have tracking plan (new feature or Mandatory Handoffs)
- [ ] Mandatory Handoffs destinations comply with `deferred-task-protocol.md` Option B Guards (apply at creation time, not just Phase 9)
- [ ] Summary scope matches Tasks/ACs (no scope creep)
- [ ] Type field matches actual implementation
- [ ] File counts verified by Glob/ls before writing spec (not assumed)
- [ ] AC count ≤ 50 (hard limit — MUST split feature)
- [ ] AC count ≤ 30 (soft limit — SHOULD consider splitting)

### Baseline Measurement (F714 Lesson)
- [ ] **Output-dependent features** (warnings, errors, counts): Run the actual command during FC and parse real output before writing ACs
- [ ] AC Expected values derived from empirical data, not estimates (「~392」のような推定は禁止)
- [ ] All output categories discovered and listed — each category explicitly marked In-Scope or Out-of-Scope
- [ ] Baseline data saved to `_out/tmp/` for reproducibility

**Anti-pattern**: コード読解だけで「こういう出力が出るはず」と推論し、実行せずにAC定義する → カテゴリ見落とし・件数誤差の原因

### AC Definition Basics
- [ ] Each AC is verifiable within the Feature's scope alone (external/pre-existing conditions are out of scope)
- [ ] Each AC has specific, measurable Expected value
- [ ] Technical debt ACs use `TODO|FIXME|HACK` pattern
- [ ] Grep patterns use `.*` for flexibility
- [ ] File existence → Glob, content check → Grep
- [ ] AC table includes Method column (Glob/Grep/dotnet test)
- [ ] AC matchers use specific identifiers (not broad patterns)
- [ ] Threshold-matcher ACs all have AC Details with Derivation
- [ ] Every getter AC has corresponding setter AC when interface defines both (V2)
- [ ] Stub replacement ACs include positive call verification (V2)
- [ ] Grep patterns use `|` not `\|` for ripgrep alternation (V3)
- [ ] Grep patterns match within single line, no cross-line assumptions (V3)
- [ ] Behavioral contract ACs exist for boundary/default values (V2)

### Structure Compliance
- [ ] All Phase 1 investigation sections present (Root Cause Analysis, Related Features, Feasibility Assessment, Impact Analysis, Technical Constraints, Risks)
- [ ] Baseline Measurement present (DELETE for kojo/research per template)
- [ ] AC Design Constraints present with constraint details
- [ ] Technical Design section present (Approach, AC Coverage, Key Decisions sub-sections)
- [ ] Philosophy Derivation table present in Acceptance Criteria
- [ ] Goal Coverage Verification table present in Acceptance Criteria
- [ ] Links section populated (not just placeholder)
- [ ] Section ordering matches `pm/reference/feature-template.md`
- [ ] Source: [`feature-template.md`](../../../pm/reference/feature-template.md)
- [ ] Task Tags subsection present after Tasks table (V1)
- [ ] Template comments present in Mandatory Handoffs, Tasks, Dependencies (V1)
- [ ] AC count consistent across Definition Table, Success Criteria, Technical Design (V5)
- [ ] Every AC# mapped to at least one Task; every Task mapped to at least one AC (V5)

### Philosophy & Tasks
- [ ] Philosophy includes SSOT claim and scope (not just principle name)
- [ ] Tasks that are same edit operation merged into atomic task

---

## Type-Specific Guides

Load the appropriate guide based on feature Type:

| Type | Guide | When to Use |
|------|-------|-------------|
| kojo | [KOJO.md](KOJO.md) | Dialogue implementation (future YAML migration planned) |
| erb | [ERB.md](ERB.md) | ERB-to-C# migration (output is C#) |
| engine | [ENGINE.md](ENGINE.md) | C# engine, Era.Core |
| research | [RESEARCH.md](RESEARCH.md) | Feature to create Features |
| infra | [INFRA.md](INFRA.md) | Workflow/Review |

---

## FC Post-Generation Validation (Mandatory)

> **Purpose**: Systematic validation after all FC phases complete. Prevents ~68% of FL review fixes based on F789-F791 analysis.
> - **V1**: Run by quality-fixer (Phase 6) — mechanical auto-fix
> - **V2-V5**: Run by feature-validator (Phase 7) — detection and reporting

### V1: Template Lint

Mechanically verify template compliance without reading content semantics:

- [ ] Task Tags subsection present after Tasks table (with `[I]` tag explanation)
- [ ] No non-template fields (Created, Summary) unless Sub-Feature Requirements exists
- [ ] Template comments present in Mandatory Handoffs section (CRITICAL, Option A/B/C, Validation, DRAFT Creation)
- [ ] Template comments present in Tasks section (AC Coverage Rule)
- [ ] Template comments present in Dependencies section (Dependency Types SSOT)
- [ ] Section separator `---` present before Execution Log and Review Notes
- [ ] Upstream Issues uses table format `| Issue | Upstream Section | Suggested Fix |`
- [ ] Execution Log uses table format (may be empty but table header required)
- [ ] No spurious `---` inside sections (only between top-level sections)

### V2: AC Multi-Perspective Checklist

For each deliverable category, verify AC completeness:

**Interface Methods**:
- [ ] Every getter has a corresponding setter AC (when interface defines both)
- [ ] Boundary/default value test AC exists (out-of-bounds, uninitialized)
- [ ] Max valid index test AC exists (verifies array dimensions correct)

**Stub Replacements**:
- [ ] Exception removal AC exists (not_matches NotImplementedException)
- [ ] Interface injection AC exists (matches field/constructor)
- [ ] Actual call verification AC exists (matches method call in consuming file — prevents hardcoded return)

**Interface Extensions**:
- [ ] Backward compatibility AC exists (count_equals for pre-existing methods)
- [ ] "Not modified" AC for sibling interfaces when ISP applied

**New Files/Types**:
- [ ] SSOT update AC exists when ssot-update-rules.md requires it (engine-dev SKILL.md, etc.)

**Sub-Deliverables**:
- [ ] Comment update ACs exist when Tasks include documentation updates
- [ ] Every sub-deliverable within a Task has a verifying AC

**ERB Responsibility Boundary (erb/engine migration — F808 Lesson)**:
- [ ] Each source ERB file's functions analyzed for dependency clustering (not just file-level migration)
- [ ] If ERB file header lists multiple concerns ("X and Y") → class split evaluated
- [ ] If proposed constructor has >7 dependencies → split analysis mandatory
- [ ] If function subsets use <50% overlapping dependencies → separate class candidates documented in Key Decisions
- [ ] ERB "leftover" files (functions not fitting elsewhere) explicitly identified and split candidates flagged

**Constructor Dependencies (erb/engine migration)**:
- [ ] Every constructor parameter of each new class has an injection verification AC (Grep for field/parameter type in target file)
- [ ] Investigation's Interface Dependency Scan entries are 1:1 mapped to constructor injection ACs
- [ ] Predecessor-provided sealed/concrete classes: delegation pattern decided and documented in Key Decisions (F805 lesson: DES-001)
- [ ] Sealed class testing strategy documented (real instances with mocked deps, not mock sealed — F805 lesson: DES-003)

**DI Composition Root (erb/engine migration)**:
- [ ] Every new class has AddSingleton/AddTransient registration AC targeting composition root file
- [ ] Nullable interface parameters: DI resolution strategy documented (MS DI does NOT resolve unregistered nullable as null — F805 lesson)
- [ ] Transitive dependency registration scope clarified (own-class only vs full chain)

**ERB Branch Coverage (erb migration)**:
- [ ] Every SELECTCASE block in source ERB has representative behavioral test AC(s)
- [ ] Dispatch method branch count verification AC exists (gte matcher with floor count from ERB analysis)
- [ ] Each AC Design Constraint (C1, C2, ...) is covered by at least one behavioral test AC (not just structural grep)

**Upstream Interface Signatures (erb/engine migration)**:
- [ ] Interface method signatures sufficient for all call sites (parameter types/counts match — F805 lesson: IKojoMessageService)
- [ ] DIM default values for new interface methods use fail-loud pattern (throw NotImplementedException) unless game-logic default is justified

### V3: Matcher Validation

For each AC with Grep matcher, verify pattern quality:

- [ ] Pattern uses `|` (pipe) for alternation, NOT `\|` (backslash-pipe) — ripgrep treats `\|` as literal
- [ ] Pattern matches within a single line — no cross-line assumptions (method name + exception on different lines)
- [ ] `not_matches` patterns: confirm pattern DOES currently match in codebase (ensures non-vacuous test)
- [ ] `matches` patterns: confirm pattern does NOT currently match (ensures RED state for TDD)
- [ ] For ACs with Details: AC Definition Table patterns consistent with AC Details patterns
- [ ] Matcher name is in allowed list: `matches`, `not_matches`, `gte`, `lte`, `count_equals`, `file_exists`, `dotnet_test` (F805 lesson: `count_gte` is invalid)
- [ ] Regex has no trailing `\\|` or unclosed groups (F805 lesson: AC#53 had stray `\\|`)
- [ ] Matcher/Expected/Method columns are not swapped (F805 lesson: AC#56 had Matcher↔Expected swap)
- [ ] Threshold-matcher ACs (gte/gt/lt/lte/count_equals) include derivation in AC Details

### V4: Source Re-verification

After Implementation Contract is written, re-read referenced files:

- [ ] Constructor signatures in Implementation Contract match actual source code (re-read file, don't trust investigation cache)
- [ ] Line references match actual file content (Grep to verify)
- [ ] Loop bounds verified against ERB source (ERA FOR uses exclusive upper bound)
- [ ] Method visibility (public/private/internal) matches actual source
- [ ] Using directives complete (no missing namespace imports)

### V5: Cross-Reference Consistency

Verify all cross-reference tables agree:

- [ ] AC count in AC Definition Table == count mentioned in Success Criteria
- [ ] AC count in AC Definition Table == count mentioned in Technical Design Approach
- [ ] Every AC# in Definition Table appears in at least one Task's AC# column
- [ ] Every Task's AC# column references only existing AC#s
- [ ] Every Goal in Goal Coverage maps to at least one AC
- [ ] Every Philosophy Derivation absolute claim maps to at least one AC
- [ ] AC Coverage in Technical Design lists all AC#s (none missing)

### V6: Cascading Consistency (F826 Lesson)

Verify that threshold values and descriptive text are consistent across ALL sections where they appear:

- [ ] AC threshold values (gte/lte/count_equals Expected) consistent between AC Definition Table, AC Details Derivation, Technical Design, and Implementation Contract
- [ ] AC description text consistent between AC Definition Table Description column and AC Details header
- [ ] Mandatory Handoff row counts consistent between table row count and any descriptive text referencing the count
- [ ] When fixing a threshold or description in one location, grep for the old value across the entire feature file to find all occurrences

**Rationale**: F826 consumed 4/10 FL iterations on cascading fixes — changing a threshold in AC Definition Table without updating the same value in Technical Design and Implementation Contract. V5 catches structural cross-references (counts, mappings) but not value consistency.

---

## Anti-Patterns (All Types)

| Anti-Pattern | Example | Fix |
|--------------|---------|-----|
| Vague equivalence | "matches legacy behavior" | Specify exact input/output pairs |
| TODO-only pattern | `Grep "TODO"` | `Grep "TODO\|FIXME\|HACK"` |
| Rigid grep | `"AddSingleton<IFoo, Foo>"` | `"AddSingleton.*IFoo.*Foo"` |
| Missing dependencies | (not listed) | Add to Dependencies table |
| Scope creep | Summary > Tasks | Add explicit scope boundary |
| Broad matcher | `"private static readonly HashSet"` | Use specific identifier `"HalfwidthChars"` |
| Missing Method column | AC table has no Method | Add Glob/Grep/dotnet test column |
| AC Details orphan | AC Details lists method, no AC | Add AC for each promised method |
| Redundant tasks | Task#2 refactor, Task#3 remove | Merge: same edit operation |
| Narrow philosophy | "DRY - Eliminate duplicates" | Add SSOT claim + scope + benefit |
| Unverified file count | Summary "~10 files", AC count_gte 8 | Glob/ls actual files BEFORE writing spec (F457) |
| Scope-exceeding AC | AC tests `--strict-warnings` (all 12K warnings) but Feature only fixes TALENT constants (F711) | AC must be verifiable within Feature scope alone |
| Desk-only investigation | FC predicts "~392 warnings" from code reading without running command (F714) | Run baseline command, parse output, derive ACs from real data |

---

## When to Use This Skill

- `/fc` command execution (feature completion)
- Manual feature-{ID}.md writing
- FL review preparation
- AC definition refinement

---

## Maintenance

### Adding New Issues

When FL review reveals new patterns, add to the appropriate type file:

1. **Use existing format**:
   ```markdown
   ### Issue N: {Title}

   **Symptom**: {What is the problem}

   **Example (Bad)**:
   ```markdown
   ...
   ```

   **Example (Good)**:
   ```markdown
   ...
   ```

   **Fix**: {One-line solution}
   ```

2. **Include concrete examples** from actual features (reference F421, etc.)
3. **Keep self-contained** - Should be immediately understandable when read
4. **Update Checklist** at file end if new check item needed

---

## Links

- [feature-template.md](../../../pm/reference/feature-template.md) - Feature template
- [testing SKILL](../testing/SKILL.md) - AC type and matcher reference
