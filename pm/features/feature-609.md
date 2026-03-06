# Feature 609: Create reference document mapping patterns to alternative approaches

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **RECORD** - Add to Handoff section with concrete destination
> 3. **CONTINUE** - Return to in-scope work
>
> **Definition**: This feature creates AC pattern mapping documentation only. Implementation of alternative verification methods, tool creation, or workflow changes are out of scope.

## Type: infra

## Background

### Philosophy
Documentation Completeness - Testing SKILL AC patterns require alternative verification approaches when subprocess execution fails. This reference document serves as SSOT providing representative mappings for each documented pattern type (output/variable/build/exit_code/file/code/test) to corresponding manual/alternative verification methods, serving as guidance for developers.

### Problem
Handoff from Feature 601: Multiple pattern analysis beyond slash commands is required to map AC verification patterns to alternative approaches when subprocess execution is not feasible.

### Goal
Create Game/agents/reference/ac-pattern-alternatives.md containing structured mapping table with columns: Pattern Type, Matcher, Standard Method, Alternative Method, Example. Document must provide representative examples for each of the 7 AC Types documented in Testing SKILL (output/variable/build/exit_code/file/code/test), demonstrating alternative verification approaches with actionable implementation guidance.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Reference document exists | file | Glob | exists | Game/agents/reference/ac-pattern-alternatives.md | [x] |
| 2 | All Testing SKILL output patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "output.*contains" | [x] |
| 3 | All Testing SKILL variable patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "variable.*equals" | [x] |
| 4 | All Testing SKILL build patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "build.*succeeds" | [x] |
| 5 | All Testing SKILL file patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "file.*exists" | [x] |
| 6 | Alternative approaches documented | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "Alternative.*Method" | [x] |
| 7 | Pattern examples provided | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | contains | "Example:" | [x] |
| 8 | SSOT links validated | file | reference-checker | succeeds | - | [x] |
| 9 | Document structure validated | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "Pattern Type.*Standard Method.*Alternative" | [x] |
| 10 | Cross-reference accuracy | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | contains | "Testing SKILL" | [x] |
| 11 | All Testing SKILL exit_code patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "exit_code.*equals" | [x] |
| 12 | All Testing SKILL code patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "code.*contains" | [x] |
| 13 | All Testing SKILL test patterns mapped | file | Grep(Game/agents/reference/ac-pattern-alternatives.md) | matches | "test.*succeeds" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Write Game/agents/reference/ac-pattern-alternatives.md with structured table format | [x] |
| 2 | 2 | Create output pattern section documenting --unit/--flow methods and manual verification alternatives | [x] |
| 3 | 3 | Create variable pattern section documenting variable inspection methods and manual verification alternatives | [x] |
| 4 | 4 | Create build pattern section documenting build success verification and manual check alternatives | [x] |
| 5 | 5 | Create file pattern section documenting file existence verification and manual check alternatives | [x] |
| 6 | 6 | Write Alternative Method section: (1) When to use, (2) Exact verification steps or commands, (3) Limitations. Use bullet format per Testing SKILL style | [x] |
| 7 | 7 | Add concrete usage examples for each pattern type with before/after verification scenarios | [x] |
| 8 | 8 | Run reference-checker on completed document to validate all SSOT references | [x] |
| 9 | 9 | Structure document with sections: Overview, Pattern Type Table (per AC Type), Examples, Limitations. Follow Testing SKILL header style | [x] |
| 10 | 10 | Include proper references to Testing SKILL source with accurate line numbers and quotes | [x] |
| 11 | 11 | Create exit_code pattern section documenting exit code verification and manual check alternatives | [x] |
| 12 | 12 | Create code pattern section documenting code content verification and manual inspection alternatives | [x] |
| 13 | 13 | Create test pattern section documenting C# unit test verification and manual execution alternatives | [x] |

## AC Details

| AC# | Verification Context | Additional Notes |
|:---:|---------------------|------------------|
| 1 | File creation at specified path | Validates document exists before content verification |
| 2-5,11-13 | Pattern mapping completeness | Verifies all 7 Testing SKILL AC Types have documented alternatives |
| 6 | Alternative method documentation | Ensures document provides actionable alternatives, not just pattern lists |
| 7 | Concrete usage examples | Validates practical implementation guidance beyond theoretical mapping |
| 8 | Reference link integrity | SSOT links in document must resolve to existing files (Testing SKILL, etc.) |
| 9 | Document structure consistency | Verifies structured table format enabling easy lookup by pattern type |
| 10 | Source attribution accuracy | Validates references to Testing SKILL are accurate and traceable |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F601 | [DONE] | ac-static-verifier Slash Command Alternative Verification - creates need for pattern mapping |
| Related | F608 | [PROPOSED] | Testing SKILL pattern audit - optional enhancement, F609 reads Testing SKILL directly |

## Review Notes
- [resolved-invalid] Phase1-Uncertain iter1: Implementation Contract section requirement - feature-template.md lines 131-135 explicitly mark Implementation Contract as optional ("Delete this section if not needed"). F609 creates additive documentation, no rollback/impact needed.
- [resolved-invalid] Phase1-Uncertain iter2: Rollback Plan requirement - INFRA.md Issue 5 targets workflow changes with impact. F609 creates reference documentation only.
- [resolved-invalid] Phase1-Uncertain iter2: Impact Analysis requirement - INFRA.md Issue 6 targets changes affecting existing components. F609 adds new standalone documentation.
- [resolved-applied] Phase1-Uncertain iter3: Pending item resolution format - [resolved-invalid] format validated via F601 precedent (lines 240, 242, 246).
- [resolved-applied] Phase1-Uncertain iter4: AC Expected patterns revised - Grep patterns appropriate for document content verification per Testing SKILL Method Column Usage.
- [resolved-applied] Phase1-Uncertain iter4: AC#9 section header made flexible - Updated to match structured table content rather than specific headers.
- [resolved-applied] Phase1-Uncertain iter4: Review Notes resolution - All items now resolved with rationale.
- [resolved-invalid] Phase1-Uncertain iter5: AC#7 'Example:' generic but acceptable for purpose-specific new document with minimal false positive risk.
- [resolved-invalid] Phase1-Uncertain iter5: Mandatory Handoffs relationship to pending items - only 1 pending item remains, not requiring handoffs
- [resolved-applied] Phase1-Uncertain iter7: AC#9 pipe character location - fixed format: moved pattern to Expected column with matches Matcher, used .* instead of pipe per INFRA.md Issue 14
- [resolved-applied] Phase2-Maintainability iter10: Philosophy/Goal modified to "representative mappings as guidance" instead of "enabling reliable validation" - aligns with structural verification ACs.
- [resolved-applied] Phase2-Maintainability iter10: AC#7 'Example:' pattern retained - "Example:" format adopted as document specification for consistency.

## Mandatory Handoffs

| Issue | Destination | Description |
|-------|-------------|-------------|

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-24 | START | implementer | Tasks 1-13 | SUCCESS |
| 2026-01-24 | DEVIATION | ac-static-verifier | --feature 609 --ac-type file | exit code 1 (11/13 passed: AC#1 matcher parse issue, AC#8 manual) |
| 2026-01-24 | VERIFY | Glob | AC#1 file exists | PASS (manual) |
| 2026-01-24 | VERIFY | reference-checker | AC#8 SSOT links | PASS (0 issues) |
| 2026-01-24 | END | Phase 6 | All ACs verified | 13/13 PASS |
| 2026-01-24 | DEVIATION | feature-reviewer | Mode: post | NEEDS_REVISION: broken link INFRA.md |
| 2026-01-24 | FIX | Edit | ac-pattern-alternatives.md:261 | Fixed INFRA.md path |
| 2026-01-24 | VERIFY | feature-reviewer | Mode: post (retry) | OK |
| 2026-01-24 | VERIFY | feature-reviewer | Mode: doc-check | OK |
| 2026-01-24 | VERIFY | SSOT update check | N/A | No SSOT updates needed (doc only) |
| 2026-01-24 | FIX | Edit | AC#1 format | Method=exists→Glob, Matcher=-→exists |
| 2026-01-24 | HANDOFF | F618 | ac-static-verifier MANUAL counting | Created via feature-creator |

## Links
[index-features.md](index-features.md)
[feature-601.md](feature-601.md) - Predecessor: Creates need for pattern mapping
[feature-608.md](feature-608.md) - Related: Provides pattern inventory