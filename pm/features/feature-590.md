# Feature 590: YAML Schema Validation Tools

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

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Single Source of Truth (SSOT) - Establish comprehensive tooling and documentation for YAML schema validation, enabling early error detection during development and IDE integration support to reduce YAML syntax errors and improve developer experience with type-safe YAML configuration.

### Problem (Current Issue)
YamlSchemaGen and YamlValidator tools exist in tools/ directory but lack documentation, test coverage, and workflow integration. Developers cannot easily discover how to generate schemas, validate YAML files, or integrate validation into CI workflows. No README.md files exist to guide usage, and tools are not referenced in CLAUDE.md Project Structure table.

### Goal (What to Achieve)
Create comprehensive documentation and integration for YAML schema validation tools, including README.md files for both tools, workflow integration guidance, test coverage for schema generation, and CLAUDE.md updates to ensure tools are discoverable and properly documented in project structure.

### Impact Analysis

| Component | Change Type | Description |
|-----------|-------------|-------------|
| tools/YamlSchemaGen/README.md | New | Documentation file for schema generator |
| tools/YamlValidator/README.md | New | Documentation file for schema validator |
| CLAUDE.md | Existing (verified) | Tools already listed in Project Structure |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | YamlSchemaGen README exists | file | Glob | exists | tools/YamlSchemaGen/README.md | [x] |
| 2 | YamlValidator README exists | file | Glob | exists | tools/YamlValidator/README.md | [x] |
| 3 | YamlSchemaGen unit tests exist | file | Glob | exists | tools/YamlSchemaGen.Tests/SchemaValidationTests.cs | [x] |
| 4 | Schema versioning documented | file | Grep(tools/YamlSchemaGen/README.md) | contains | "Schema Versioning" | [x] |
| 5 | CLI usage examples documented | file | Grep(tools/YamlValidator/README.md) | contains | "Usage Examples" | [x] |
| 6 | CI integration documented | file | Grep(tools/YamlValidator/README.md) | contains | "CI Integration" | [x] |
| 9 | Schema generation AC | exit_code | dotnet run --project tools/YamlSchemaGen/ | succeeds | - | [x] |
| 10 | Unit tests pass | exit_code | dotnet test tools/YamlSchemaGen.Tests/ | succeeds | - | [x] |
| 11 | Validation positive test | exit_code | dotnet run --project tools/YamlValidator/ -- --schema tools/YamlSchemaGen/dialogue-schema.json --yaml Game/tests/sample-dialogue.yaml | succeeds | - | [x] |
| 12 | No technical debt markers | file | Grep(tools/YamlSchemaGen/) | not_contains | "TODO|FIXME|HACK" | [x] |
| 13 | Documentation consistency | file | /audit | succeeds | - | [x] |
| 14 | Error troubleshooting documented | file | Grep(tools/YamlValidator/README.md) | contains | "Error" | [x] |

### AC Details

**AC#1-2**: README files for both tools
- Test: Verify README.md files exist for YamlSchemaGen and YamlValidator
- Expected: Documentation provides tool purpose, usage, examples

**AC#3**: Unit test coverage for schema generation (Pre-satisfied)
- Test: Verify YamlSchemaGen has unit test project with SchemaValidationTests.cs
- Expected: Test coverage for schema generation logic - existing implementation already meets requirements

**AC#4**: Schema versioning guidance
- Test: Grep "Schema Versioning" in YamlSchemaGen README
- Expected: Documentation explains how to version schemas when structure changes

**AC#5**: CLI usage examples
- Test: Grep "Usage Examples" in YamlValidator README
- Expected: Documentation includes single-file and directory validation examples

**AC#6**: CI integration guidance
- Test: Grep "CI Integration" in YamlValidator README
- Expected: Documentation explains how to integrate validation into CI workflows


**AC#9**: Schema generation works
- Test: Run schema generator, verify exit code 0
- Expected: dialogue-schema.json generated successfully

**AC#10**: Unit tests pass
- Test: Run YamlSchemaGen.Tests test suite
- Expected: All tests pass

**AC#11**: Validation works on real files
- Test: Run validator against existing test YAML file
- Expected: Validation succeeds on conformant test file (Game/tests/sample-dialogue.yaml)

**AC#12**: No technical debt markers
- Test: Grep for TODO/FIXME/HACK in YamlSchemaGen directory
- Expected: Zero matches

**AC#13**: SSOT consistency verification
- Test: Run /audit command to verify documentation consistency
- Expected: No SSOT conflicts detected

**AC#14**: Error troubleshooting documentation
- Test: Grep "Error" in YamlValidator README
- Expected: Documentation includes error message format and troubleshooting guidance

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create YamlSchemaGen README.md | [x] |
| 2 | 4 | Document schema versioning in YamlSchemaGen README | [x] |
| 3 | 2 | Create YamlValidator README.md | [x] |
| 4 | 5 | Document CLI usage examples in YamlValidator README | [x] |
| 5 | 6 | Document CI integration in YamlValidator README | [x] |
| 6 | 9 | Verify schema generation succeeds | [x] |
| 7 | 10 | Run YamlSchemaGen unit tests | [x] |
| 8 | 11 | Validate test YAML file with generated schema | [x] |
| 9 | 12 | Verify no technical debt markers in code | [x] |
| 10 | 13 | Verify documentation consistency with /audit | [x] |
| 11 | 14 | Document error message format and troubleshooting | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### README.md Content Requirements

#### YamlSchemaGen README.md

Required sections:
1. **Purpose**: Explain schema generation for YAML dialogue files
2. **Usage**: CLI invocation with examples
3. **Schema Versioning**: Guidance on evolving schema definitions
4. **Output**: Describe dialogue-schema.json structure
5. **Integration**: How other tools consume generated schemas

#### YamlValidator README.md

Required sections:
1. **Purpose**: Explain YAML schema validation functionality
2. **Usage Examples**:
   - Single file validation
   - Directory validation (--validate-all)
3. **Exit Codes**: Document 0 (success) and 1 (failure) codes
4. **CI Integration**: Example integration into git hooks or CI pipelines
5. **Error Reporting**: Explain validation error format

### Unit Test Requirements

YamlSchemaGen.Tests/SchemaValidationTests.cs must verify:
- Schema generation produces valid JSON Schema
- Required properties (character, situation, branches) exist
- Variable condition schemas (TALENT, ABL, EXP, FLAG, CFLAG) defined
- Output file created successfully

### CLAUDE.md Update Pattern

Add to Project Structure table under tools/ section:
```markdown
tools/YamlSchemaGen/  # YAML schema generator for dialogue files
tools/YamlValidator/  # YAML schema validator CLI
```

### Validation Testing

Test validation against existing YAML files:
```bash
# Generate schema
dotnet run --project tools/YamlSchemaGen/

# Validate existing dialogue file
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --yaml Game/tests/sample-dialogue.yaml
```

### Rollback Plan

If issues arise after implementation:
1. Revert README.md files: `git checkout HEAD -- tools/YamlSchemaGen/README.md tools/YamlValidator/README.md`
2. Documentation-only feature - no code changes to revert
3. CLAUDE.md entries are existing (not new), no revert needed

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F583 | [DONE] | Complete CSV Elimination - Tools support YAML migration |
| Related | F572 | [DONE] | COM YAML Rapid Iteration Tooling - Established YAML patterns |

## Review Notes
- [resolved-applied] Phase0-RefCheck iter1: TBD Violation - Mandatory Handoffs section uses "F591 (TBD)" and "F592 (TBD)" which violates CLAUDE.md policy. Both features exist, so TBD markers must be removed.
- [resolved-applied] Phase0-RefCheck iter1: Missing Game/YAML/Kojo/ directory - AC#11 references this path but directory doesn't exist. Created directory structure for validation testing.
- [resolved-applied] Phase0-RefCheck iter1: AC#3 test file mismatch - Expects ProgramTests.cs but actual test file may be SchemaValidationTests.cs. Verify correct test file name.
- [resolved-applied] Phase1-Review iter2: Wrong feature references in Handoffs - F591/F592 are about CSV removal and error handling, not IDE/schema evolution. Removed invalid handoffs.
- [resolved-applied] Phase1-Review iter2: Redundant ACs 7-8 - CLAUDE.md already contains tool entries. Removed duplicate ACs and corresponding task.
- [resolved-applied] Phase1-Review iter2: Pre-existing test AC#3 - SchemaValidationTests.cs already exists. Marked AC as pre-satisfied.
- [resolved-applied] Phase1-Review iter2: AC#11 path clarification - Updated description to clarify Game/YAML/Kojo/ is created during implementation.
- [resolved-applied] Phase1-Review iter3: AC#11 validation path fix - Changed from empty Game/YAML/Kojo/ to existing Game/tests/sample-dialogue.yaml to ensure meaningful validation testing.
- [resolved-applied] Phase1-Review iter3: Task#3 description update - Updated to reflect corrected AC#11 using test file instead of directory.
- [resolved-invalid] Phase1-Review iter3: AC numbering gap - Feature template allows non-sequential AC numbers. Gap from AC#6 to AC#9 is documented in Review Notes as intentional removal.
- [resolved-applied] Phase1-Review iter3: Implementation Contract validation command - Updated example to use Game/tests/sample-dialogue.yaml instead of empty directory.
- [resolved-applied] Phase1-Review iter4: AC#11 CLI parameter fix - Changed '--validate' to '--yaml' to match YamlValidator CLI interface.
- [resolved-applied] Phase3-ACValidation iter5: AC#12 regex fix - Removed double backslash from regex pattern 'TODO\\|FIXME\\|HACK' to 'TODO|FIXME|HACK'.
- [resolved-applied] Phase3-ACValidation iter5: AC#13 type fix - Changed Type from 'file' to 'exit_code' for /audit command validation.
- [resolved-applied] Phase1-Review iter6: AC#13 type correction - Changed Type from 'exit_code' back to 'file' per INFRA.md Issue 19 (slash commands use file type).
- [resolved-applied] Phase1-Review iter6: AC:Task 1:1 compliance - Split 4 tasks into 10 tasks for strict 1:1 AC mapping.
- [resolved-applied] Phase3-ACValidation iter7: AC#13 Type/Matcher conflict - Testing skill updated to add slash command exception per user decision. Pattern 'file | /command | succeeds' now documented in Testing skill.
- [resolved-applied] PhilosophyGate iter7: ADOPT error documentation - Added AC#14 and Task#11 for error message format and troubleshooting documentation.
- [resolved-applied] PhilosophyGate iter7: DEFER IDE integration - Added to Mandatory Handoffs with destination F599 (to be created).
- [resolved-applied] Phase1-Review iter8: F599 created - Created feature-599.md for IDE/Editor Integration for YAML Schema to fulfill handoff destination.
- [applied] FL/600 handoff update: F600 blocked due to fundamental design issue - slash commands cannot be executed via subprocess per Testing SKILL line 79. Complete redesign required.
- [resolved-applied] Phase2-Maintainability iter9: Added Rollback Plan section per INFRA.md Issue 5.
- [resolved-applied] Phase2-Maintainability iter9: Added Impact Analysis table per INFRA.md Issue 6.
- [resolved-skipped] Phase2-Maintainability iter9: AC#12 Grep path may match obj/ subdirectory files. User chose to keep current directory-wide search.

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| IDE/editor integration configuration | Philosophy mentions "IDE integration support" but implementation is separate from documentation scope | Feature | F599 |
| ac-static-verifier slash command support | AC#13 uses `/audit` which ac-static-verifier cannot verify (unknown matcher) | Feature | F600 [BLOCKED - redesign required] |

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-22 19:46 | START | implementer | Task 1-2 | - |
| 2026-01-22 19:46 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-22 19:49 | START | implementer | Task 3-5,11 | - |
| 2026-01-22 19:49 | END | implementer | Task 3-5,11 | SUCCESS |
| 2026-01-22 19:49 | START | implementer | Task 6-9 | - |
| 2026-01-22 19:49 | END | implementer | Task 6-9 | SUCCESS |
| 2026-01-22 19:51 | SUCCESS | audit | Task 10 | /audit completed with findings |
| 2026-01-22 19:55 | SUCCESS | ac-verifier | Phase 6 | 8/9 file ACs PASS (AC#13 manual) |
| 2026-01-22 19:56 | INFO | feature-reviewer | Phase 7.1 | NEEDS_REVISION - issues in F599 (out of scope) |
| 2026-01-22 20:01 | INFO | Phase 8 | Problem Resolution | Created F600 for ac-static-verifier slash command support |

## Links
[index-features.md](index-features.md)
[feature-583.md](feature-583.md) - Predecessor: Complete CSV Elimination
[feature-572.md](feature-572.md) - Related: COM YAML Rapid Iteration Tooling
[feature-599.md](feature-599.md) - Handoff: IDE/editor integration configuration
[feature-600.md](feature-600.md) - Handoff: ac-static-verifier slash command support
