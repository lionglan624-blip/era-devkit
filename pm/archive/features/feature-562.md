# Feature 562: Architecture Analysis: C#/YAML Boundary Definition

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

## Type: research

## Summary

**Feature to create Features**: Architecture Analysis producing design document as input for F563 implementation and recommendations for Phase 17 feature revisions.

## Background

### Philosophy (Mid-term Vision)

Migration Architecture - Establish clear separation between C# engine (interpreter/framework) and YAML content (community-editable), maintaining ERA-style community participation while gaining modern engine benefits. SSOT for data placement strategy that distinguishes genuinely moddable content (kojo/character combinations) from engine-dependent features requiring C# code changes, preventing "phantom moddability" where YAML additions don't work without corresponding engine modifications.

**Existing C# Implementation Disposition Criteria**: For C# implementations created under prior assumptions (before boundary definition), determine disposition based on the new boundary:
- **Engine Infrastructure** (keep C#): Interpreters, effect handlers, condition evaluators, registry systems
- **Content Definitions** (evaluate YAML migration): Data definitions that community could extend (COM definitions, character templates)
- **Hybrid** (interface C#, data YAML): Systems where logic stays in C# but data moves to YAML

**ERA-style Community Participation Definition**: The ability to add/modify game content via text file (YAML) editing alone, WITHOUT requiring:
- C# compilation
- Pull request approval
- Unity build
This workflow ("edit text file → game works") was the hallmark of ERB-era community modding and must be preserved for genuinely moddable content types.

### Problem (Current Issue)

Current architecture.md assumes 'data separation' means external files for testability/type-safety (developer focus), but doesn't address community moddability. Phase 17 plans CSV→YAML migration without clear principle of what should be C# vs YAML. F537 FL review revealed 'phantom moddability' issue - YAML data additions don't work without C# code changes for many content types. The architecture treats all data as equally moddable, but practical moddability varies dramatically (80% of COMs can be YAML-only, but new talents require C# handlers).

**Additionally**: Existing C# implementations (COM classes, TalentIndex constants, etc.) were created under the assumption that C# is the target format. If YAML is now preferred for community moddability, these existing implementations may need to be reconsidered or migrated back to YAML-based definitions.

### Goal (What to Achieve)

1) Analyze architecture.md and existing implementations deeply to understand current data placement assumptions
2) Define clear boundary: C# Engine (logic, effect handlers, condition evaluators) vs YAML Content (COM definitions, character data, kojo)
3) Identify Phase 17+ features requiring revision based on new C#/YAML boundary
4) Document community moddability scope explicitly with concrete examples of what works vs what requires C# changes
5) **Audit existing C# implementations** (COM classes, loaders, type definitions) and determine disposition (keep C#, migrate to YAML, or hybrid)
6) **Define migration path** for existing C# code that should become YAML-based content
7) **Produce actionable recommendations** for F563 (Architecture Implementation) to execute
8) **Recommend F537 disposition**: F537 is currently [BLOCKED] pending this analysis. Output concrete recommendation for F537: CANCEL (obsolete due to new boundary), REVISE (with specific scope changes), or PROCEED (original scope valid)

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Architecture analysis complete | file | Glob | exists | "Game/agents/designs/architecture-analysis-562.md" | [x] |
| 2 | C#/YAML boundary principles | file | Grep | matches | "C# Engine.*YAML Content.*boundary" | [x] |
| 3 | Moddability scope documented | file | Grep | matches | "Community Moddability.*scope.*examples" | [x] |
| 4 | Phase 17 feature revision list | file | Grep | matches | "revision.*(F5[23][0-9]|F540)" | [x] |
| 5 | Phantom moddability addressed | file | Grep | matches | "phantom.*moddability.*prevention" | [x] |
| 6 | COM YAML-ization analysis | file | Grep | matches | "COMs.*YAML.*(percentage|majority)" | [x] |
| 7 | Talent/Ability strategy | file | Grep | matches | "Talent.*Ability.*C#.*enum" | [x] |
| 8 | Community participation preserved | file | Grep | matches | "ERA-style.*community.*participation" | [x] |
| 9 | F537 implications documented | file | Grep | matches | "F537.*Transform Rules.*recommendation" | [x] |
| 10 | SSOT consistency verified | manual | /audit | succeeds | "No issues found" | [x] |
| 11 | All links valid | manual | reference-checker | succeeds | "Agent execution succeeds" | [x] |
| 12 | Existing C# implementation audit | file | Grep | matches | "Existing.*C#.*Implementation.*Audit" | [x] |
| 13 | C# to YAML migration path | file | Grep | matches | "Migration.*Path.*C#.*YAML" | [x] |
| 14 | COM C# class disposition | file | Grep | matches | "COM.*class.*disposition.*YAML" | [x] |
| 15 | F563 recommendations section | file | Grep | matches | "Recommendations.*F563.*Implementation" | [x] |

### AC Details

**AC#1**: Architecture analysis document creation
- Test: Glob pattern="Game/agents/designs/architecture-analysis-562.md"
- Expected: Analysis document exists with deep review of current architecture.md assumptions

**AC#2**: C#/YAML boundary principles defined
- Test: Grep pattern="C# Engine.*YAML Content.*boundary" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Clear principles distinguishing C# engine code from YAML content (in analysis doc)

**AC#3**: Community moddability scope documentation
- Test: Grep pattern="Community Moddability.*scope.*examples" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Explicit documentation of what community can mod vs what requires C# changes

**AC#4**: Phase 17 feature revision identification
- Test: Grep pattern="revision.*(F5[23][0-9]|F540)" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: List of Phase 17 features (F520-F540 range) requiring revision based on new boundary

**AC#5**: Phantom moddability issue addressed
- Test: Grep pattern="phantom.*moddability.*prevention" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Strategy to prevent YAML additions that don't work without C# changes

**AC#6**: COM YAML-ization feasibility analysis
- Test: Grep pattern="COMs.*YAML.*(percentage|majority)" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Analysis quantifying what percentage of COMs can be implemented with YAML-only

**AC#7**: Talent/Ability implementation strategy
- Test: Grep pattern="Talent.*Ability.*C#.*enum" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Decision on whether Talent/Ability definitions should be C# enums vs YAML

**AC#8**: Community participation preservation
- Test: Grep pattern="ERA-style.*community.*participation" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Explicit commitment to maintaining community participation opportunities

**AC#9**: F537 recommendation documented
- Test: Grep pattern="F537.*Transform Rules.*recommendation" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Specific recommendation for F537 (cancel, revise, or proceed) based on new principles

**AC#10**: SSOT consistency verification
- Test: Manual execution of /audit slash command
- Expected: /audit reports no issues found in cross-references

**AC#11**: Link validation
- Test: Manual dispatch of reference-checker agent on all modified files
- Expected: reference-checker agent reports all links resolve correctly

**AC#12**: Existing C# implementation audit
- Test: Grep pattern="Existing.*C#.*Implementation.*Audit" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Complete inventory of existing C# implementations (COM classes, loaders, type definitions) with disposition recommendation

**AC#13**: C# to YAML migration path
- Test: Grep pattern="Migration.*Path.*C#.*YAML" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Clear migration strategy for C# code that should become YAML-based content

**AC#14**: COM C# class disposition
- Test: Grep pattern="COM.*class.*disposition.*YAML" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Specific recommendation for existing COM C# classes (CombatTraining, etc.) - keep, migrate to YAML, or hybrid

**AC#15**: F563 recommendations section
- Test: Grep pattern="Recommendations.*F563.*Implementation" path="Game/agents/designs/architecture-analysis-562.md"
- Expected: Actionable recommendations section for F563 to implement

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create architecture analysis document | [x] |
| 2 | 6 | Analyze COM YAML-ization feasibility | [x] |
| 3 | 9 | Document F537 implications and recommendations | [x] |
| 4 | 12 | Audit existing C# implementations inventory | [x] |
| 5 | 14 | Determine COM C# class disposition (keep/migrate/hybrid) | [x] |
| 6 | 2 | Define C# Engine vs YAML Content boundary principles | [x] |
| 7 | 5 | Address phantom moddability prevention | [x] |
| 8 | 7 | Define Talent/Ability implementation strategy | [x] |
| 9 | 3 | Document community moddability scope with examples | [x] |
| 10 | 8 | Preserve ERA-style community participation | [x] |
| 11 | 4 | Review Phase 17 features (F529-F540) and identify revision requirements | [x] |
| 12 | 13 | Define migration path for existing C# code that should become YAML | [x] |
| 13 | 15 | Write actionable recommendations section for F563 implementation | [x] |
| 14 | 10 | Verify SSOT consistency with /audit command | [x] |
| 15 | 11 | Validate all links with reference-checker agent | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Requirements

1. **Deep Architecture Review**: Read Game/agents/designs/full-csharp-architecture.md entirely, paying special attention to Phase 17 section and data migration assumptions
2. **F537 FL Discussion Integration**: Incorporate all findings from F537 FL Review sections, especially the "phantom moddability" insight and COM YAML-ization analysis
3. **Community Impact Assessment**: Analyze how C# vs YAML decisions affect community participation barriers
4. **Practical Moddability Testing**: Verify the 80% COM YAML-only claim with concrete examples
5. **Existing C# Implementation Audit**: Inventory C# code created under "C# is target" assumption:
   - Era.Core/Commands/Com/ - COM implementations (100+ classes in Training, Equipment, Utility, etc.)
   - Era.Core/Types/ - Type definitions (ComId, TalentIndex, AbilityIndex, FlagIndex, etc.)
   - Era.Core/Data/ - Data loaders (IGameBaseLoader, YamlGameBaseLoader, etc.)

   **Audit Level**: Category analysis (not individual COM analysis)
   - Analyze COM by subdirectory category (Training, Equipment, Bondage, Oral, Penetration, Undressing, Utility)
   - Select 1-2 representative examples per category
   - Determine disposition at category level (keep C#/migrate to YAML/hybrid)
   - Individual COM disposition is delegated to F563

### Document Structure Requirements

**architecture-analysis-562.md**:
- Current State Analysis (architecture.md assumptions)
- F537 Discussion Summary (key insights)
- **Existing C# Implementation Audit** (inventory with disposition)
- C#/YAML Boundary Definition (with examples)
- Community Moddability Assessment (what works vs what doesn't)
- Phase 17 Feature Impact Analysis (revision requirements)
- **COM C# Class Disposition** (CombatTraining etc. - keep/migrate/hybrid)
- **Migration Path** (how to convert C# content to YAML)
- **Recommendations for F563 Implementation** (actionable items for architecture.md update)

### F563 Recommendations Section Requirements

The recommendations section MUST provide actionable items for F563 with the following **mandatory quality criteria**:

1. **architecture.md Changes** (具体的テキスト提案)
   - Exact text to add/modify in Data Placement Strategy section
   - Exact text to add/modify in Community Moddability Scope section
   - Line numbers or section headers where changes apply

2. **Phase 17 Feature Revisions** (F529-F540変更リスト)
   - Feature ID + specific AC/scope changes needed
   - Rationale for each change based on new C#/YAML boundary

3. **Migration Tasks** (工数見積付き)
   - Each task with effort estimate: S(Small: <1 day), M(Medium: 1-3 days), L(Large: >3 days)
   - Priority order for execution

4. **Phantom Moddability Prevention** (実装ステップ)
   - Concrete implementation steps, not abstract principles
   - Verification method for each prevention measure

### Rollback Plan

If issues arise after deployment:
1. Revert architecture.md changes with `git revert`
2. Restore original Phase 17 feature specifications if any were modified
3. Create follow-up feature for incremental fixes
4. Notify user of rollback and issues encountered

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| architecture-analysis-562.md | New analysis document | F563 uses this as input for implementation |
| F563 | Successor feature | Implements recommendations from this analysis |
| Phase 17 feature specs | Revision list identified | F563 executes revisions to F529-F540 |
| F537 status | Recommendation documented | F563 may change status based on analysis |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Spawned-From | F537 | [BLOCKED] | F562 created from F537 FL Review discussion findings |
| Successor | F563 | [PROPOSED] | Architecture Implementation (executes F562 recommendations) |

## Links

- [index-features.md](index-features.md)
- [feature-537.md](feature-537.md) - F537 FL Review Discussion provides foundation
- [feature-563.md](feature-563.md) - Successor: Architecture Implementation (executes F562 recommendations)
- [designs/full-csharp-architecture.md](designs/full-csharp-architecture.md) - Analysis target (updated by F563)
- [feature-516.md](feature-516.md) - Phase 17 Planning (may require updates via F563)

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Architecture implementation | F562 produces analysis only | Feature F563 | F563 |
| Phase 17 feature revisions | F563 implements based on F562 recommendations | Feature F563 tasks | F563 |

## Review Notes

### AC Count Justification

15 ACs required for comprehensive architecture analysis:
- AC#1: Document creation (deliverable verification)
- AC#2-9: Eight distinct architecture analysis dimensions (boundary principles, moddability scope, feature revision list, phantom moddability, COM analysis, Talent/Ability strategy, community participation, F537 implications)
- AC#10-11: Manual verification tasks (SSOT consistency, link validation)
- AC#12-15: Four existing C# implementation analysis dimensions (audit, migration path, COM class disposition, F563 recommendations)

Each AC validates a specific deliverable section. Consolidation would reduce validation granularity for this critical architecture foundation. Similar to F424/F437 precedent where complex analysis requires detailed acceptance criteria.

<!-- FL Review findings will be documented here -->

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-19 | create | feature-creator | Created Feature 562 Architecture Revision | PROPOSED |
| 2026-01-19 | scope-expand | orchestrator | Added AC#13-15 for existing C# implementation audit and migration path | UPDATED |
| 2026-01-19 | scope-split | orchestrator | Split F562 into analysis-only (F562) + implementation (F563) per user request | UPDATED |
| 2026-01-19 | fl-review | fl-workflow | FL Review: Phase 0-9 + Post-loop. 15+ fixes applied. 8 pending issues resolved with user. Philosophy Gate PASS. | REVIEWED |
| 2026-01-19 | implement | implementer | Completed Tasks 1-13: Created 49-section architecture analysis document (15,000 words). Category-level COM audit (168 files), Phase 17 feature revision specs, C#/YAML boundary definition, phantom moddability prevention strategy, F563 actionable recommendations. | SUCCESS |