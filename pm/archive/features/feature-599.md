# Feature 599: IDE/Editor Integration for YAML Schema

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
Enable IDE integration support for YAML schema validation, providing type-safe YAML configuration and early error detection during development through VS Code and other editor integrations. This supports the SSOT philosophy by ensuring YAML files are validated against canonical schemas at development time, preventing syntax errors from reaching runtime.

### Problem (Current Issue)
Philosophy of F590 mentions "IDE integration support" but F590 scope is limited to documentation. No IDE-specific configuration exists for YAML schema validation (e.g., .vscode/settings.json with YAML schema associations). Existing .vscode/settings.json only configures C# and ERB file associations but lacks YAML schema mappings. Developers editing YAML files in Game/data/ and character definitions get no intellisense or validation feedback.

### Goal (What to Achieve)
Implement VS Code and IDE integration for YAML dialogue schema, including .vscode/settings.json configuration, extension recommendations, and integration documentation for other editors. Ensure YAML files are automatically validated against Game/schemas/com.schema.json and other schemas during editing.

## Impact Analysis

| Component | Type | Changes | Risk | Rollback |
|-----------|------|---------|------|----------|
| .vscode/settings.json | Existing | Add yaml.schemas configuration | Low | Remove yaml.schemas section |
| .vscode/extensions.json | New | Create with YAML extension recommendations | Low | Delete file |
| Game/agents/reference/ide-integration.md | New | IDE setup documentation | Low | Delete file |
| CLAUDE.md | Existing | Add .vscode/ to Project Structure code block | Low | Remove .vscode/ entry |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VS Code YAML schema associations | file | Grep(.vscode/settings.json) | contains | "yaml.schemas" | [x] |
| 2 | Game/data pattern mapping exists | file | Grep(.vscode/settings.json) | contains | "Game/data/**/*.yaml" | [x] |
| 3 | VS Code extension recommendations | file | Glob | exists | .vscode/extensions.json | [x] |
| 4 | YAML Language Support recommended | file | Grep(.vscode/extensions.json) | contains | "redhat.vscode-yaml" | [x] |
| 5 | Schema validation documentation | file | Glob | exists | Game/agents/reference/ide-integration.md | [x] |
| 6 | VS Code setup instructions | file | Grep(Game/agents/reference/ide-integration.md) | contains | "VS Code Setup" | [x] |
| 7 | IntelliJ IDEA setup instructions | file | Grep(Game/agents/reference/ide-integration.md) | contains | "IntelliJ IDEA" | [x] |
| 8 | Schema path references valid | file | Grep(.vscode/settings.json) | contains | "Game/schemas/com.schema.json" | [x] |
| 9 | YAML validation test passes | manual | Open test YAML file in VS Code | succeeds | Schema validation active | [x] |
| 10 | Error reporting test | manual | Add invalid YAML syntax to test file | succeeds | Red underline shown | [x] |
| 11 | Documentation consistency | file | /audit | succeeds | - | [x] |
| 12 | No technical debt markers | file | Grep(.vscode/) | not_contains | "TODO|FIXME|HACK" | [x] |
| 13 | CLAUDE.md Project Structure updated | file | Grep(CLAUDE.md) | contains | ".vscode/" | [x] |

### AC Details

**AC#1-2**: VS Code YAML schema association configuration
- Method: Grep(.vscode/settings.json) for yaml.schemas and target file pattern mapping
- Expected: Automatic schema validation for YAML files in Game/data/ directory

**AC#3-4**: VS Code extension recommendations
- Method: Check .vscode/extensions.json exists and contains YAML extension
- Expected: Workspace recommends redhat.vscode-yaml for YAML support

**AC#5-7**: IDE integration documentation
- Method: Verify Game/agents/reference/ide-integration.md exists with setup instructions for multiple editors
- Expected: Clear setup instructions for VS Code and IntelliJ IDEA

**AC#8**: Schema path validation
- Method: Grep(.vscode/settings.json) for valid schema file path
- Expected: Schema paths point to existing Game/schemas/com.schema.json

**AC#9-10**: Manual validation tests
- Method: Manual testing in VS Code with YAML files (requires human verification during implementation)
- AC#9 Pass criteria: Open existing caress.yaml, hover over property shows schema tooltip/IntelliSense with type information
- AC#10 Pass criteria: Add invalid syntax (e.g., "invalid: true") to test YAML file, red underline appears within 2 seconds

**AC#11**: SSOT documentation consistency
- Method: /audit command succeeds with no violations
- Expected: All documentation references are consistent

**AC#13**: Project structure documentation update
- Method: Grep(CLAUDE.md) for .vscode configuration mention
- Expected: CLAUDE.md Project Structure table includes .vscode/ directory

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Configure VS Code yaml.schemas with COM schema mapping to Game/data/**/*.yaml pattern | [x] |
| 2 | 3-4 | Create .vscode/extensions.json with YAML Language Support recommendation | [x] |
| 3 | 5 | Create IDE integration documentation file | [x] |
| 4 | 6 | Document VS Code setup instructions | [x] |
| 5 | 7 | Document IntelliJ IDEA setup instructions | [x] |
| 6 | 8 | Verify schema path references | [x] |
| 7 | 9 | Test YAML validation in IDE | [x] |
| 8 | 10 | Test error reporting functionality | [x] |
| 9 | 11 | Verify documentation consistency | [x] |
| 10 | 12 | Check for technical debt markers | [x] |
| 11 | 13 | Update CLAUDE.md Project Structure | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Step | Description |
|-------|------|-------------|
| 1 | 1.1 | Read existing .vscode/settings.json to understand current configuration |
| 1 | 1.2 | Add yaml.schemas configuration mapping Game/schemas/com.schema.json to Game/data/**/*.yaml pattern |
| 1 | 1.3 | Verify schema file path exists and is correct |
| 2 | 2.1 | Create .vscode/extensions.json with workspace extension recommendations |
| 2 | 2.2 | Add redhat.vscode-yaml extension for YAML language support |
| 2 | 2.3 | Add any other relevant YAML/JSON schema extensions |
| 4 | 4.1 | Create Game/agents/reference/ide-integration.md with clear setup instructions |
| 4 | 4.2 | Document VS Code setup process with relevant configuration examples |
| 4 | 4.3 | Document IntelliJ IDEA setup process for YAML schema integration |
| 4 | 4.4 | Include troubleshooting section for common issues |
| 8 | 8.1 | Identify existing YAML file in Game/data/ for validation testing (e.g., Game/data/coms/training/touch/caress.yaml) |
| 8 | 8.2 | Test YAML validation in VS Code: Open caress.yaml, hover over property, verify schema tooltip/IntelliSense appears showing property type |
| 8 | 8.3 | Test error reporting: Add invalid syntax 'invalid: true' to test YAML file, verify red underline appears within 2 seconds |
| 8 | 8.4 | Verify intellisense/autocompletion works for COM schema properties |
| 10 | 10.1 | Update CLAUDE.md Project Structure table with .vscode/ entry |
| 10 | 10.2 | Run /audit to verify documentation consistency |
| 10 | 10.3 | Fix any SSOT violations found |

## Rollback Plan

If implementation issues arise, rollback in reverse order:

1. **CLAUDE.md changes**: Remove .vscode/ entry from Project Structure table
2. **Game/agents/reference/ide-integration.md**: Delete the file entirely (new file, no existing state to restore)
3. **.vscode/extensions.json**: Delete the file entirely (new file, no existing state to restore)
4. **.vscode/settings.json**: Remove yaml.schemas section only, preserve existing configurations

**Verification**: After rollback, run `dotnet build` and `/audit` to ensure no broken references remain.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F590 | [DONE] | YAML Schema Validation Tools - provides base schema files |

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- [resolved-applied] Phase1 iter1: AC#13 pattern format unclear - Changed to use literal pattern '.vscode/ # IDE configuration'
- [resolved-applied] Phase1 iter3: AC#2 escaped quotes pattern - simplified to 'com.schema.json.*Game/data' for Grep compatibility
- [resolved-deferred] Phase1 iter6: Consider adding reference-checker AC for link validation in new documentation file - /audit (AC#11) covers external links but not anchor links. Deferred to post-implementation validation.
- [resolved-applied] Phase1 iter7: Resolve pending review notes - adding resolution status tracking for review transparency
- [resolved-accepted] Phase1 iter8: Minor description inconsistency between Links and Handoffs for F602 - acceptable as contextual annotation for clarity
- [resolved-deferred] Phase1 iter1: F602 path inconsistency (docs/ide-integration.md vs Game/agents/reference/ide-integration.md) - out of scope for F599, noted for F602 review
- [resolved-applied] Phase1 iter1: Pattern 'com.schema.json.*Game/data' may not match actual JSON structure depending on formatting - verified single-line JSON format is valid approach
- [resolved-accepted] Phase1 iter3: AC#13 pattern '.vscode/' may need specific comment suffix to match Project Structure entry format - pattern will match new entry to be added
- [resolved-accepted] Phase2 iter5: Philosophy coverage gap - manual tests AC#9-10 cannot verify runtime error prevention, but sufficient for IDE integration validation purpose
- [resolved-deferred] Phase2 iter5: Task#8 overlap with Task#1-2 - validation is implicit in configuration tasks, explicit verification acceptable for thoroughness
- [resolved-accepted] Phase2 iter5: AC#2 pattern formatting dependency - single-line JSON format choice reduces maintenance risk, validated approach
- [resolved-accepted] Phase2 iter7: Manual test limitation for AC#9-10 acknowledged - IDE integration verification requires visual confirmation, automated schema syntax validation separate concern
- [resolved-noted] Phase6 iter10: F602 must update path from docs/ide-integration.md to Game/agents/reference/ide-integration.md before F602 implementation

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Other editor integrations (Vim, Emacs, Atom) | Limited scope to major IDEs for this feature | Feature | F602 (Additional IDE integrations for Claude Code) |
| F602 path inconsistency (docs/ide-integration.md vs Game/agents/reference/ide-integration.md) | F602 references wrong path but F599 creates correct path structure | Feature | F602 (Path correction required) |

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-23 07:28 | START | implementer | Tasks 1-6, 11 | - |
| 2026-01-23 07:28 | END | implementer | Tasks 1-6, 11 | SUCCESS |
| 2026-01-23 | DEVIATION | Bash | ac-static-verifier | exit 1: AC#11 matcher 'succeeds' unsupported for /audit (expected per INFRA.md Issue 19) |
| 2026-01-23 | END | manual | AC#9, AC#10 | PASS - IDE validation confirmed by user |

## Links
- [index-features.md](index-features.md)
- [feature-590.md](feature-590.md) - YAML Schema Validation Tools (Related)
- [feature-602.md](feature-602.md) - Additional IDE Integrations (Follow-up)