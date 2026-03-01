# Feature 620: /fc Resume Capability

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **TRACK** - Choose one concrete destination:
>    - Option A: Create new Feature → Add Task to create F{ID}
>    - Option B: Add to existing Feature → Add Task to update F{ID}#T{N}
>    - Option C: Add to architecture.md Phase → Verify Phase exists
> 3. **HANDOFF** - Record in this feature's Handoff section
> 4. **CONTINUE** - Resume this feature's scope
>
> **TBD is FORBIDDEN**. Every discovered issue must have actionable handoff.

## Type: infra

## Background

### Philosophy (思想・上位目標)
Feature creation should support session continuity. When /fc is interrupted mid-execution, users should be able to resume from the last completed phase rather than restarting from scratch.

### Problem (現状の問題)
1. **No resume capability**: Initial /fc implementation (F619) executes phases 1-5 sequentially without persistence
2. **Session interruption risk**: Long-running /fc commands may timeout or be interrupted
3. **Lost work**: Partially completed feature files cannot be resumed

### Goal (このFeatureで達成すること)
1. Add phase detection logic to /fc command
2. Enable resume from last completed phase
3. Persist phase completion state in feature file sections

### Session Context
- **Created by**: F619 (Feature Creation Workflow with [DRAFT] Status and /fc Command)
- **Predecessor**: F619

## Root Cause Analysis

### 5 Whys

1. Why: Users cannot resume /fc command after interruption
2. Why: The /fc command (fc.md) does not check existing file content before executing phases
3. Why: F619 was scoped as initial implementation with resume explicitly deferred to F620
4. Why: Breaking changes into incremental features reduces risk and scope per feature
5. Why: Anthropic recommended patterns favor iterative improvement over monolithic features

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| /fc restarts from Phase 1 even when sections exist | fc.md lacks phase detection logic before Task dispatch |
| Lost work on interruption | No state persistence mechanism (feature file sections are state, but not read) |
| Users must manually identify resume point | Missing section-presence detection function |

### Conclusion

Root cause is **missing phase detection logic** in fc.md. The feature file already serves as state storage (each phase writes distinct sections), but fc.md does not read this state before dispatching agents. The solution is to add detection logic that checks for section presence before each phase dispatch.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F619 | [DONE] | Predecessor | Created /fc command without resume. Handoff explicitly created F620. |
| F610 | [DONE] | Design foundation | Feature Creator 5-Phase Orchestrator - defined the 5-phase workflow |
| F612 | [DONE] | Design patterns | Extensible Orchestrator Design - contains ResumeLogicPattern spec |

### Pattern Analysis

The resume capability was intentionally deferred from F619 to F620 following the incremental feature pattern. This is not a recurring problem but a planned follow-up feature. The design foundation already exists in F612's ResumeLogicPattern.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | ResumeLogicPattern already designed in F612; feature-creator/SKILL.md has implementation reference |
| Scope is realistic | YES | Single file change (fc.md) with section detection logic |
| No blocking constraints | YES | F619 is [DONE], no external dependencies |

**Verdict**: FEASIBLE

The solution is straightforward: add section detection before each Task dispatch in fc.md. The design patterns already exist (F612), and a reference implementation exists in feature-creator/SKILL.md (determine_resume_point function).

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F619 | [DONE] | Initial /fc command implementation |
| Related | F610 | [DONE] | Feature Creator 5-Phase Orchestrator Redesign |
| Related | F612 | [DONE] | Extensible Orchestrator Design (ResumeLogicPattern) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| None | - | - | Pure markdown/command change, no external libraries |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| All future [DRAFT] features | MEDIUM | /fc command used to complete DRAFT features |
| Run-workflow PHASE-4/8 | LOW | References /fc but doesn't depend on resume capability |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| .claude/commands/fc.md | Update | Add phase detection logic before Task dispatch sequence |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Section markers must be stable | fc.md, agent outputs | MEDIUM - Detection relies on section headers (## Root Cause, ## Acceptance Criteria, etc.) |
| Phase order is fixed | F619 design | LOW - tech-investigator → ac-designer → tech-designer → wbs-generator → feature-validator |
| Sections are phase markers | Design decision | LOW - Each phase writes specific sections that serve as completion markers |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Partial section detection (section exists but incomplete) | Low | Medium | Check for multiple section markers per phase (e.g., both "## Root Cause Analysis" and "## Feasibility Assessment" for tech-investigator) |
| Agent writes invalid section format | Low | Low | Feature-validator (Phase 5) validates structure; resume can skip to validation |
| Detection logic too permissive | Medium | Low | Use conservative detection - only skip if ALL phase sections present |

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "should support session continuity" | /fc command must enable resuming interrupted sessions | AC#1, AC#2, AC#3, AC#4 |
| "able to resume from the last completed phase" | Detection logic must identify phase completion state | AC#5, AC#6, AC#7, AC#8 |
| "rather than restarting from scratch" | Completed phases must be skipped on resume | AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 1 detection: Root Cause section | file | Grep(.claude/commands/fc.md) | matches | "Grep.*Root Cause Analysis.*feature" | [x] |
| 2 | Phase 1 detection: Feasibility section | file | Grep(.claude/commands/fc.md) | matches | "Grep.*Feasibility Assessment.*feature" | [x] |
| 3 | Phase 2 detection: Acceptance Criteria section | file | Grep(.claude/commands/fc.md) | matches | "Grep.*Acceptance Criteria.*feature" | [x] |
| 4 | Phase 3 detection: Technical Design section | file | Grep(.claude/commands/fc.md) | matches | "Grep.*Technical Design.*feature" | [x] |
| 5 | Phase 4 detection: Tasks section | file | Grep(.claude/commands/fc.md) | contains | "## Tasks" | [x] |
| 6 | Resume entry point exists | file | Grep(.claude/commands/fc.md) | contains | "determine_resume_point" | [x] |
| 7 | Skip logic for completed phases | file | Grep(.claude/commands/fc.md) | matches | "If resume_from.*<=" | [x] |
| 8 | Conservative detection documented | file | Grep(.claude/commands/fc.md) | matches | "ALL.*section.*present" | [x] |
| 9 | Status change only on completion | file | Grep(.claude/commands/fc.md) | matches | "resume_from.*feature-validator" | [x] |
| 10 | Links validated | file | reference-checker | succeeds | - | [x] |

**Note**: 10 ACs within infra range (8-15).

### AC Details

**AC#1: Phase 1 detection - Root Cause section**
- Test: Grep(fc.md) for "Root Cause Analysis"
- Rationale: tech-investigator outputs "## Root Cause Analysis" section. Detection of this section indicates Phase 1 started.
- Edge case: Partial phase completion is addressed by requiring multiple section markers (AC#1 + AC#2).

**AC#2: Phase 1 detection - Feasibility section**
- Test: Grep(fc.md) for "Feasibility Assessment"
- Rationale: tech-investigator outputs "## Feasibility Assessment" as final section. Detection of BOTH Root Cause (AC#1) AND Feasibility (AC#2) indicates Phase 1 complete.
- Conservative detection: Only skip Phase 1 when ALL its sections present.

**AC#3: Phase 2 detection - Acceptance Criteria section**
- Test: Grep(fc.md) for "Grep.*Acceptance Criteria.*feature"
- Rationale: Verify fc.md contains detection logic that greps for Acceptance Criteria section in feature file, not just prose mentions.
- Distinguishes detection implementation from existing prose in current fc.md.

**AC#4: Phase 3 detection - Technical Design section**
- Test: Grep(fc.md) for "Grep.*Technical Design.*feature"
- Rationale: Verify fc.md contains detection logic that greps for Technical Design section in feature file, not just prose mentions.
- Distinguishes detection implementation from existing prose in current fc.md.

**AC#5: Phase 4 detection - Tasks section**
- Test: Grep(fc.md) for "## Tasks"
- Rationale: wbs-generator outputs "## Tasks" table and Implementation Contract.
- Note: Uses "## Tasks" (with ##) to distinguish from prose mentions of "Tasks".

**AC#6: Resume entry point exists**
- Test: Grep(fc.md) for "determine_resume_point"
- Rationale: fc.md must contain logic to determine which phase to start from.
- Following ResumeLogicPattern from F612/extensible-orchestrator.md.

**AC#7: Skip logic for completed phases**
- Test: Grep(fc.md) for "If resume_from.*<="
- Rationale: fc.md must contain conditional dispatch logic that uses resume_from comparison for phase skip control.
- Verifies skip logic pattern without requiring specific formatting (resilient to line breaks).

**AC#8: Conservative detection documented**
- Test: Grep(fc.md) for "ALL.*section.*present"
- Rationale: F620 Risks table identifies "Partial section detection" as Medium risk.
- Verifies conservative detection logic requires ALL sections for multi-section phases per Technical Design.

**AC#9: Status change only on completion**
- Test: Grep(fc.md) for "resume_from.*feature-validator"
- Rationale: Verifies resume logic exists and feature-validator executes after conditional phase dispatches.
- Ensures TDD red-green cycle by requiring resume implementation before test passes.

**AC#10: Links validated**
- Test: reference-checker validation
- Rationale: Infra features require link validation per INFRA.md Issue 2.
- Verifies: All references in modified fc.md are valid.

## Technical Design

### Approach

Implement resume capability by adding phase detection logic to fc.md before each Task() dispatch. The solution follows the ResumeLogicPattern from F612's extensible-orchestrator.md with section presence detection.

**Core mechanism**:
1. Add `determine_resume_point()` function that checks for section presence to detect completed phases
2. Call this function at procedure start to identify first incomplete phase
3. Use conditional logic to skip completed phase dispatches
4. Preserve existing status update location (after all phases complete)

**Conservative detection strategy**: A phase is considered complete only when ALL its output sections are present. For tech-investigator (Phase 1), this means BOTH "## Root Cause Analysis" AND "## Feasibility Assessment" must exist.

**Implementation target**: Single file modification (fc.md). No agent file changes required - agents already write stable section markers.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add section detection: `Grep("## Root Cause Analysis", feature_file)` in determine_resume_point() |
| 2 | Add section detection: `Grep("## Feasibility Assessment", feature_file)` in determine_resume_point() |
| 3 | Add section detection: `Grep("## Acceptance Criteria", feature_file)` in determine_resume_point() |
| 4 | Add section detection: `Grep("## Technical Design", feature_file)` in determine_resume_point() |
| 5 | Add section detection: `Grep("## Tasks", feature_file)` in determine_resume_point() |
| 6 | Add determine_resume_point() function before Procedure section in fc.md |
| 7 | Wrap each Task() dispatch in conditional: `if resume_from <= N: Task(...)` to skip completed phases |
| 8 | Document conservative detection in determine_resume_point() comment: "Only skip phase when ALL its sections present" |
| 9 | Preserve existing Step 7 location (status update after all Task() dispatches complete) |
| 10 | Run reference-checker skill after fc.md modification to validate all links |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Detection mechanism | A) Completion markers (`<!-- PHASE-N-COMPLETE -->`), B) Section presence, C) File timestamps | B | Section presence is simpler and doesn't require agents to write extra markers. Agents already write stable section headers. |
| Resume granularity | A) Resume from exact failed phase, B) Always restart from phase 1 | A | Maximizes work preservation. User can resume interrupted sessions without redoing completed phases. |
| Conservative detection | A) Single section check, B) Multiple section check per phase | B | Reduces risk of false positive detection when phase partially completes. Tech-investigator writes 2+ sections, check both. |
| Skip implementation | A) GOTO phase N, B) Conditional dispatch, C) Loop with phase pointer | B | Markdown command files use sequential procedure format. Conditional `if resume_from <= N` is clearest. |
| Status update timing | A) Update status after each phase, B) Update only after all phases | B | Preserves existing behavior (AC#9). Status [DRAFT] -> [PROPOSED] only when feature fully complete. |

### Interfaces / Data Structures

**determine_resume_point() function signature** (added to fc.md):

```markdown
## Resume Detection

**Function**: determine_resume_point(feature_file)

**Input**: feature_file path (e.g., "Game/agents/feature-620.md")

**Output**: Integer (1-5) indicating first incomplete phase, or 6 if all phases complete

**Logic**:
1. Check for "## Tasks" section → If present, return 5 (resume from Phase 5 - validation)
2. Check for "## Technical Design" section → If present, return 4 (resume from Phase 4)
3. Check for "## Acceptance Criteria" section → If present, return 3 (resume from Phase 3)
4. Check for "## Root Cause Analysis" AND "## Feasibility Assessment" sections → If BOTH present, return 2 (resume from Phase 2)
5. Otherwise, return 1 (resume from Phase 1)

**Conservative detection**: Phase N is complete only when ALL its output sections exist.

**Note**: Phase 5 (feature-validator) always executes regardless of resume point since it validates the complete feature and does not write persistent sections.
```

**Modified Procedure structure** (fc.md):

```markdown
## Procedure
1. Read feature-{ID}.md, verify [DRAFT] status
1b. Determine resume point: resume_from = determine_resume_point(feature_file)
2. If resume_from <= 2: Task(tech-investigator) → Root Cause, Related Features, Feasibility, Dependencies, Impact, Constraints, Risks
   - If NOT_FEASIBLE or NEEDS_REVISION → STOP, report to user
3. If resume_from <= 3: Task(ac-designer) → Acceptance Criteria (table + details)
4. If resume_from <= 4: Task(tech-designer) → Technical Design (to satisfy ACs)
5. If resume_from <= 5: Task(wbs-generator) → Tasks + Implementation Contract
6. Task(feature-validator) → Validation (always run, validates complete feature)
7. Update status [DRAFT] → [PROPOSED]
8. Report completion
```

**Section detection examples**:

| Phase | Sections to Check | Detection Pattern |
|:-----:|-------------------|-------------------|
| 1 (tech-investigator) | Root Cause Analysis, Feasibility Assessment | `Grep("## Root Cause Analysis") AND Grep("## Feasibility Assessment")` |
| 2 (ac-designer) | Acceptance Criteria | `Grep("## Acceptance Criteria")` |
| 3 (tech-designer) | Technical Design | `Grep("## Technical Design")` |
| 4 (wbs-generator) | Tasks | `Grep("## Tasks")` |
| 5 (feature-validator) | (Validation phase - no persistent section, always run) | Always execute |

**Note**: feature-validator (Phase 5/Step 6) always runs to validate complete feature, even on resume. It does not write persistent sections, so cannot be skipped.

## Links
- [index-features.md](index-features.md)
- [F619: Feature Creation Workflow](feature-619.md)
- [F610: Feature Creator 5-Phase Orchestrator Redesign](feature-610.md)
- [F612: Extensible Orchestrator Design](feature-612.md)
- [extensible-orchestrator.md](designs/extensible-orchestrator.md) - ResumeLogicPattern reference
- [feature-creator SKILL](../../.claude/skills/feature-creator/SKILL.md)

---

## Review Notes

- [resolved-applied] Phase1-Uncertain iter2: Pattern 'determine.*phase' may be ambiguous - 'determine' and 'phase' could appear separately in unrelated contexts. Suggested fix: 'determine_resume_point' or 'determine.*resume.*phase'
- [resolved-applied] Phase1-Uncertain iter7: AC#9 pattern 'status.*DRAFT.*PROPOSED' already matches existing fc.md before implementation, violating TDD red-green principle. Fixed with multiline pattern 'feature-validator.*status.*DRAFT.*PROPOSED' to verify correct sequencing.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Add Phase 1 detection logic (Root Cause + Feasibility sections) | [x] |
| 2 | 3 | Add Phase 2 detection logic (Acceptance Criteria section) | [x] |
| 3 | 4 | Add Phase 3 detection logic (Technical Design section) | [x] |
| 4 | 5 | Add Phase 4 detection logic (Tasks section) | [x] |
| 5 | 6,7,8 | Implement determine_resume_point() function with conservative detection and skip logic | [x] |
| 6 | 9 | Verify status change timing (only after all phases complete) | [x] |
| 7 | 10 | Run reference-checker validation on modified fc.md | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-6 | fc.md with resume logic |
| 2 | ac-tester | haiku | Task 7 | AC verification results |

**Constraints** (from Technical Design):
1. Section markers must be stable - detection relies on section headers (## Root Cause Analysis, ## Acceptance Criteria, etc.)
2. Phase order is fixed - tech-investigator → ac-designer → tech-designer → wbs-generator → feature-validator
3. Conservative detection - only skip phase when ALL its sections present

**Pre-conditions**:
- F619 is [DONE] (.claude/commands/fc.md exists with basic 5-phase implementation)
- Feature file structure is stable (agents write consistent section headers)

**Execution Steps**:

**Step 1: Read current fc.md**
- Bash: Read .claude/commands/fc.md to understand current structure
- Identify Procedure section and Task() dispatch locations

**Step 2: Implement Tasks 1-4 (Phase detection logic)**
- Add section detection for each phase (AC#1-5):
  - Phase 1: Check for "## Root Cause Analysis" AND "## Feasibility Assessment"
  - Phase 2: Check for "## Acceptance Criteria"
  - Phase 3: Check for "## Technical Design"
  - Phase 4: Check for "## Tasks"
- Implementation location: New "Resume Detection" section before Procedure

**Step 3: Implement Task 5 (determine_resume_point function)**
- Add determine_resume_point() function specification to fc.md
- Function signature: `determine_resume_point(feature_file) → Integer (1-6)`
- Logic: Check sections in reverse order (Phase 4 → Phase 1)
- Conservative detection: ALL sections for multi-section phases
- Document skip logic: "skip completed phases on resume"

**Step 4: Modify Procedure section (Task 6)**
- Add Step 1b: Call determine_resume_point() to get resume_from value
- Wrap existing Task() dispatches in conditionals:
  - Step 2: `If resume_from <= 2: Task(tech-investigator)`
  - Step 3: `If resume_from <= 3: Task(ac-designer)`
  - Step 4: `If resume_from <= 4: Task(tech-designer)`
  - Step 5: `If resume_from <= 5: Task(wbs-generator)`
  - Step 6: Always execute `Task(feature-validator)` (no skip)
- Preserve Step 7 location: Status update after all phases

**Step 5: Verify status change timing (Task 6)**
- Confirm Step 7 (status update) remains after all Task() dispatches
- No status update mid-resume
- Status [DRAFT] → [PROPOSED] only when all phases complete

**Step 6: Run reference-checker (Task 7)**
- Skill(reference-checker) to validate all links in modified fc.md
- Verify: F619, F610, F612, extensible-orchestrator.md references are valid

**Rollback Plan**:

If implementation causes issues with /fc command:

1. **Detect Issues**:
   - /fc fails to parse existing DRAFT features
   - Resume logic incorrectly skips incomplete phases
   - Status update timing breaks existing workflow

2. **Rollback Steps**:
   - Git checkout previous version: `git checkout HEAD~1 .claude/commands/fc.md`
   - Restore original fc.md from F619 commit: `git show bf8c4ab:.claude/commands/fc.md > .claude/commands/fc.md`
   - Test /fc with existing DRAFT feature to verify restoration

3. **Verify Rollback**:
   - Existing DRAFT features process normally with /fc
   - No resume logic interference
   - Status update [DRAFT] → [PROPOSED] works as before F620

## Execution Log

| Timestamp | Event | Source | Action | Detail |
|-----------|-------|--------|--------|--------|
| 2026-01-25 | START | /run | Phase 1 | Initialize F620 |
| 2026-01-25 | OK | implementer | Tasks 1-6 | Implementation complete |
| 2026-01-25 | DEVIATION | ac-static-verifier | AC verification | 7/10 failed - AC#1-5,9 patterns not found, AC#10 unknown matcher |
| 2026-01-25 | OK | debugger | Fix AC patterns | Updated fc.md with Grep patterns |
| 2026-01-25 | OK | ac-static-verifier | Re-verify | 9/10 passed (AC#1-9) |
| 2026-01-25 | OK | reference-checker | AC#10 manual | PASS - no broken links |

## Mandatory Handoffs

(No deferred issues)

**Success Criteria**:
- AC#1-5: fc.md contains section detection patterns for all phases
- AC#6: determine_resume_point() function documented in fc.md
- AC#7: Skip logic present in conditional Task() dispatches
- AC#8: Conservative detection documented ("ALL sections")
- AC#9: Status update remains at Step 7 (after all phases)
- AC#10: reference-checker validation passes
