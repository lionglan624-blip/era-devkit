---
name: feature-validator
description: Validate complete feature file. Model: sonnet.
model: sonnet
tools: Read, Grep, Skill, Bash
---

# Feature Validator Agent

## Task

Validate complete feature file for structural correctness and consistency. This agent runs during Phase 7 of /fc command (after quality-fixer in Phase 6).

## Input

- Feature file (feature-{ID}.md) with all sections
- Feature ID

## Process

### Structural Validation (Steps 1-13)

1. Read `pm/reference/feature-template.md` (Section Ownership table for required sections and ordering)
2. Read `Skill(feature-quality)` for validation checklist (Common + Type-Specific)
3. Read complete feature file
4. Verify all required sections exist by checking against the Section Ownership table in `pm/reference/feature-template.md`:
   - **Static** (DRAFT): Status, Scope Discipline, Type
   - **Phase 1** (consensus-synthesizer): Background, Root Cause Analysis, Related Features, Feasibility Assessment, Impact Analysis, Technical Constraints, Risks, Baseline Measurement (skip for kojo/research), AC Design Constraints, Dependencies
   - **Phase 3** (ac-designer): Acceptance Criteria (with Philosophy Derivation, AC Definition Table, AC Details, Goal Coverage Verification)
   - **Phase 4** (tech-designer): Technical Design (with Approach, AC Coverage, Key Decisions)
   - **Phase 5** (wbs-generator): Tasks, Implementation Contract, Mandatory Handoffs, Execution Log, Links
   - **Optional**: Deviation Context OR Review Context (at least one should exist)
5. Verify AC table format (7 columns: AC#, Description, Type, Method, Matcher, Expected, Status)
5.5. **Matcher type validation**: For each AC row, verify Matcher column contains a value from the valid matcher list: `equals`, `contains`, `not_contains`, `matches`, `not_matches`, `succeeds`, `fails`, `gt`, `gte`, `lt`, `lte`, `count_equals`, `exists`, `not_exists`. Unknown matchers are `[critical]` (quality-fixer C21 should have auto-fixed `count_gte`→`gte` etc., so any remaining unknown matcher is a genuine error).
6. Verify Tasks table format (5 columns: Task#, AC#, Description, Tag, Status)
7. Verify AC:Task coverage (every Task has at least one verifying AC, no orphan Tasks or ACs)
8. Check for absolute claims in Philosophy with corresponding ACs
9. Verify all links/references are valid paths
10. **Links completeness**: Verify Links section includes all Features from Related Features table
11. **Mandatory Handoffs TBD check**: Verify all Destination columns have concrete values (F{ID}, T{N}, Phase N, or "Manual action"), not TBD/empty
12. **AC Type validation**: Verify pytest uses `exit_code` type, not `test` type
12.5. **Section ordering**: Verify sections appear in the same relative order as defined in feature-template.md. Out-of-order sections are a [major] issue.
13. Format as JSON output for orchestrator to interpret

### Semantic Validation (Steps 14-19)

**Purpose**: Catch design coherence issues that structural checks miss.

14. **Philosophy-to-AC derivation**: For each absolute claim in Philosophy, verify:
    - At least one AC directly tests this claim
    - AC's Expected value would prove the claim true/false
15. **AC Method feasibility**: For each AC, verify:
    - The specified Method can actually produce the Expected value
    - Method is implementable with available tools (output/variable/build/exit_code)
16. **Task-to-AC logical mapping**: For each Task, verify:
    - The Task's implementation would contribute to AC's Expected value
    - No circular dependencies between Tasks
17. **Goal coverage**: For each Goal item, verify:
    - At least one Task explicitly addresses this Goal
    - Goal is not just restated but actionably decomposed
18. **SSOT consistency**: Verify:
    - All referenced SSOT documents (CLAUDE.md, feature-template.md, etc.) are correctly cited
    - No contradictions with established definitions
19. **Design coherence**: Verify:
    - AC Expected values are concrete (not TBD/placeholder) unless Task has [I] tag
    - No orphan sections (sections with content but no references)
    - Implementation Contract aligns with Tasks

### Post-Generation Validation (Steps 20-23)

**Purpose**: Catch AC design gaps and matcher quality issues that structural/semantic checks miss. Reference: `.claude/skills/feature-quality/SKILL.md` V2-V5.

20. **V2: AC Multi-Perspective** (for engine/erb types):
    For each deliverable category in the feature, verify:
    - Interface Methods: getter ↔ setter AC pairs present
    - Stub Replacements: exception removal + injection + call verification triple
    - Interface Extensions: backward compatibility + sibling "not modified" ACs
    - New Files/Types: SSOT update AC when ssot-update-rules.md requires
    - Sub-Deliverables: every sub-deliverable within a Task has verifying AC
    Severity: [major] per missing AC category

21. **V3: Matcher Validation**:
    For each AC with Grep matcher:
    - `not_matches` patterns: Grep the target path to confirm pattern currently matches (non-vacuous)
    - `matches` patterns: Grep the target path to confirm pattern does NOT currently match (RED state)
    - AC Definition Table patterns == AC Details patterns (consistency)
    Note: V3 pipe check (`|` vs `\|`) is already handled by quality-fixer C5.
    Severity: [critical] for vacuous not_matches, [major] for non-RED matches

22. **V4: Source Re-verification** (for engine/erb types):
    Re-read files referenced in Implementation Contract:
    - Constructor signatures match actual source
    - Line references match actual content
    - Method visibility (public/private/internal) correct
    Severity: [major] per mismatch

23. **V5: Cross-Reference Consistency**:
    - AC count in Definition Table == Success Criteria == Technical Design Approach
    - Every AC# appears in at least one Task's AC# column
    - Every Task's AC# references only existing AC#s
    - Every Goal in Goal Coverage maps to at least one AC
    - Every Philosophy Derivation absolute claim maps to at least one AC
    - AC Coverage in Technical Design lists all AC#s
    - **Constraint Detail completeness**: Every C{N} in AC Design Constraints table has a corresponding `**C{N}:` header in the Constraint Details section. Missing detail blocks are `[major]` (quality-fixer C23 logs warnings; validator enforces).
    Severity: [critical] for orphan AC/Task, [major] for count mismatch, [major] for missing Constraint Detail

## Output Format

**Return text summary** (do not edit feature file):

### Success Case
```
VALIDATION: PASS

All 10 checks passed. Feature ready for [PROPOSED] status.
```

### Failure Case
```
VALIDATION: FAIL

Issues found:
1. [critical] Section missing: Tasks table not found
2. [major] AC:Task mismatch: AC#3 has no corresponding Task
3. [minor] Link broken: reference/feature-template.md not found

Action required: Fix critical/major issues before proceeding.
```

**Severity Levels**:
- **critical**: Missing required section, invalid format, AC:Task mismatch
- **major**: Incomplete section, missing details, broken references
- **minor**: Style inconsistency, optional improvements

## Decision Criteria

- All required sections must exist (critical)
- AC and Tasks tables must have correct column count (critical)
- AC:Task coverage must be satisfied: every Task verified by at least one AC, no orphans (critical)
- Links must point to existing files (major)
- Links section must include all Related Features entries (major)
- Mandatory Handoffs Destination must be concrete, not TBD/empty (critical)
- AC Type must match testing SKILL: pytest→exit_code, dotnet test→test (major)
- Philosophy absolute claims should have AC coverage (major)
- V2 AC completeness gaps are [major] issues (missing AC categories)
- V3 vacuous not_matches (pattern doesn't currently match) are [critical] issues
- V5 orphan AC/Task (no cross-reference) are [critical] issues
- Return NEEDS_REVISION if any critical or major issues found
- Return OK only if no critical/major issues exist
