# Feature 611: COM YAML Linter with Japanese Support

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

Provide accessible validation tooling for community kojo writers. Lower the barrier to contribution by offering rich Japanese error messages, examples, and suggestions - no development environment required. Support both VS Code users (F599) and text editor users with a standalone validator. Enable community validation without technical setup dependencies through single-binary distribution with bundled schemas. Note: This feature focuses on COM schema validation only; kojo dialogue schema validation is out of scope.

### Problem (Current Issue)

yajsv provides schema validation but outputs English-only error messages without context. Community kojo writers using simple text editors (Notepad, Sakura Editor) cannot easily understand validation errors or learn correct YAML syntax. This creates a barrier for non-developer contributors. Existing tools/YamlValidator requires .NET runtime and English technical knowledge. Community members cannot validate their YAML files without setting up a development environment.

### Goal (What to Achieve)

Create a Go-based standalone YAML validator (com-validator or kojo-lint) with: Japanese error messages, line number references, usage examples (凡例), typo suggestions, and bundled schema. Single binary with no dependencies, invokable via batch file double-click. Provide accessible validation for community contributors using any text editor, with clear guidance and corrective suggestions.

### Impact Analysis

| Component | Change Type | Description |
|-----------|-------------|-------------|
| tools/com-validator/ | New | Go-based standalone validator with Japanese support |
| tools/com-validator/README.md | New | Documentation for the new validator |
| tools/com-validator/validate.bat | New | Windows batch file for double-click execution |
| tools/com-validator/schemas/com.schema.json | New | Embedded copy of COM schema for standalone binary |
| .githooks/schema-sync-check | New | Pre-commit hook for schema synchronization verification |
| .githooks/pre-commit | Existing | Add schema-sync-check.sh call |
| Game/agents/reference/community-tools.md | New | Community tools documentation |
| CLAUDE.md | Existing | Add com-validator to Project Structure |

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Go project structure created | file | Glob | exists | tools/com-validator/go.mod | [x] |
| 2 | Main validator binary builds | exit_code | cd tools/com-validator && go build | succeeds | - | [x] |
| 3 | Japanese error messages | file | Grep(tools/com-validator/) | contains | "エラー" | [x] |
| 4 | Usage examples included | file | Grep(tools/com-validator/) | contains | "凡例" | [x] |
| 5 | Typo suggestion feature | file | Grep(tools/com-validator/) | contains | "もしかして" | [x] |
| 6 | Bundled schema support | file | Grep(tools/com-validator/) | contains | "//go:embed" | [x] |
| 7 | Windows batch file exists | file | Glob | exists | tools/com-validator/validate.bat | [x] |
| 8 | Batch file double-click friendly | file | Grep(tools/com-validator/validate.bat) | contains | "pause" | [x] |
| 9 | README documentation created | file | Glob | exists | tools/com-validator/README.md | [x] |
| 10 | Japanese README section | file | Grep(tools/com-validator/README.md) | contains | "## 使い方" | [x] |
| 11 | Community tools documentation | file | Glob | exists | Game/agents/reference/community-tools.md | [x] |
| 12 | Schema validation positive test | exit_code | Bash | succeeds | cd tools/com-validator && go build -o com-validator.exe && ./com-validator.exe ../../Game/data/coms/training/touch/caress.yaml | [x] |
| 13 | Schema validation negative test | exit_code | Bash | fails | cd tools/com-validator && mkdir -p .tmp && echo "invalid: [yaml" > .tmp/test-invalid.yaml && go build -o com-validator.exe && ./com-validator.exe .tmp/test-invalid.yaml && rm .tmp/test-invalid.yaml | [x] |
| 14 | Japanese error output test | output | Bash | contains | "type.*mismatch\|型.*エラー" | [x] |
| 15 | CLAUDE.md updated | file | Grep(CLAUDE.md) | contains | "tools/com-validator" | [x] |
| 16 | Schema files synchronized | exit_code | Bash | succeeds | diff Game/schemas/com.schema.json tools/com-validator/schemas/com.schema.json | [x] |
| 17 | Pre-commit hook functionality | exit_code | Bash | succeeds | cd .githooks && ./schema-sync-check | [x] |
| 18 | Pre-commit integration verification | file | Grep(.githooks/pre-commit) | contains | "schema-sync-check" | [x] |
| 19 | Test file cleanup verification | exit_code | Bash | succeeds | cd tools/com-validator && find .tmp/ -name "*.yaml" | wc -l | grep "^0$" | [x] |
| 20 | Japanese error with line number | output | Bash | contains | "行.*列\|line.*column" | [x] |
| 21 | README tool selection criteria | file | Grep(tools/com-validator/README.md) | contains | "YamlValidator" | [x] |

**Note**: 21 ACs exceed typical infra range (8-15) but are justified by Go tool complexity (build, embedding, localization, testing, documentation, synchronization, cleanup)

### AC Details

**AC#1**: Go project structure created
- Method: Glob for tools/com-validator/go.mod
- Expected: Go module initialization file exists

**AC#2**: Main validator binary builds
- Method: go build in tools/com-validator directory
- Expected: Compilation succeeds without errors

**AC#3**: Japanese error messages
- Method: Grep for Japanese error message patterns in source code
- Expected: Error messages contain Japanese with line/column references

**AC#4**: Usage examples included
- Method: Grep for Japanese usage examples in source code
- Expected: Built-in help contains 凡例 (examples) section

**AC#5**: Typo suggestion feature
- Method: Grep for "もしかして" (did you mean) functionality
- Expected: Common typos suggest corrections (e.g., "charcter" -> "character")

**AC#6**: Bundled schema support
- Method: Grep for Go embed directive for schema files
- Expected: Schema files embedded in binary for standalone operation

**AC#7**: Windows batch file exists
- Method: Glob for validate.bat file
- Expected: Batch file for Windows double-click execution

**AC#8**: Batch file double-click friendly
- Method: Grep for "pause" command in batch file
- Expected: Batch file includes pause to show results before closing

**AC#9**: README documentation created
- Method: Glob for README.md file
- Expected: Documentation file exists

**AC#10**: Japanese README section
- Method: Grep for Japanese usage section header
- Expected: README contains Japanese section for community users

**AC#11**: Community tools documentation
- Method: Glob for community-tools.md reference document
- Expected: Documentation file for community tools exists

**AC#12**: Schema validation positive test
- Method: Run validator binary on valid YAML file
- Expected: Validation command succeeds (exit code 0)

**AC#13**: Schema validation negative test
- Method: Run validator binary on invalid YAML file
- Expected: Validation command fails (non-zero exit code)

**AC#14**: Japanese error output test
- Method: Run validator binary on type mismatch YAML and check output
- Expected: Error message contains Japanese type mismatch description

**AC#15**: CLAUDE.md updated
- Method: Grep for com-validator reference in CLAUDE.md
- Expected: Tool listed in Project Structure with description

**AC#16**: Schema files synchronized
- Method: Compare Game/schemas/com.schema.json with tools/com-validator/schemas/com.schema.json using diff command
- Expected: Files have identical content (diff succeeds with exit code 0)

**AC#17**: Pre-commit hook functionality
- Method: Execute schema synchronization check hook script
- Expected: Hook script runs successfully and verifies schema sync (exit code 0)

**AC#18**: Pre-commit integration verification
- Method: Verify pre-commit hook calls schema-sync-check script
- Expected: Pre-commit hook contains reference to schema-sync-check script

**AC#19**: Test file cleanup verification
- Method: Verify .tmp/ directory is empty after test execution
- Expected: No test artifacts remain (exit code 0 from file count check)

**AC#20**: Japanese error with line number
- Method: Test YAML syntax error and verify line number in Japanese error output
- Expected: Error message contains line and column references in Japanese or English

**AC#21**: README tool selection criteria
- Method: Verify README documents when to use YamlValidator vs com-validator
- Expected: README contains reference to YamlValidator for comparison/selection

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create Go project with main.go and go.mod | [x] |
| 2 | 3,4,5 | Implement Japanese error messages and suggestions | [x] |
| 3 | 6 | Implement schema embedding and validation logic | [x] |
| 4 | 7,8 | Create Windows batch file for community use | [x] |
| 5 | 9,10,11,21 | Write documentation (README and community-tools.md) | [x] |
| 6 | 12,13,14,19,20 | Implement and test validation functionality | [x] |
| 7 | 15 | Update CLAUDE.md with tool reference | [x] |
| 8 | 16 | Copy schema file to tools/com-validator/schemas/ directory | [x] |
| 9 | 17,18 | Implement and test pre-commit hook for schema sync | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Development Steps

1. **Go Project Setup**
   - **Prerequisite**: Install Go (https://go.dev/dl/) and verify 'go version' works
   - Initialize Go module in tools/com-validator/
   - Create main.go with CLI argument parsing
   - Add dependencies: gopkg.in/yaml.v3, github.com/xeipuuv/gojsonschema

2. **Schema Integration**
   - Copy Game/schemas/com.schema.json to tools/com-validator/schemas/ and use go:embed directive for schemas/*.json
   - Add AC to verify schema hash equality between original and copy to prevent divergence
   - Implement pre-commit hook that automatically verifies schema synchronization before commit
   - Implement schema loading and validation logic

3. **Japanese Localization**
   - Create Japanese error message templates
   - Map JSON schema validation errors to Japanese descriptions
   - Implement line/column number reporting in Japanese format
   - Add usage examples (凡例) in help text

4. **Typo Detection**
   - Create common typo mapping for YAML field names
   - Implement "もしかして" (did you mean) suggestions
   - Add fuzzy matching for property name suggestions

5. **Community Accessibility**
   - Create validate.bat for Windows double-click execution
   - Add pause command to show results before closing
   - Implement file drag-and-drop support

6. **Testing and Documentation**
   - Test with valid/invalid YAML files (clean up test files after execution or use .tmp/ directory)
   - Create README.md with English and Japanese sections including tool selection criteria (YamlValidator for CI/development with .NET, com-validator for standalone community use)
   - Document community usage in Game/agents/reference/community-tools.md
   - Update CLAUDE.md Project Structure

### Rollback Plan

If issues arise after deployment:
1. Remove tools/com-validator/ directory
2. Revert CLAUDE.md changes
3. Remove community-tools.md if created
4. Document rollback reason in feature execution log

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F590 | [DONE] | YAML Schema Validation Tools (provides com.schema.json) |
| Related | F599 | [DONE] | IDE/Editor Integration for YAML Schema (provides VS Code config) |

---

## Review Notes
- [resolved-accepted] Phase1-Uncertain iter1: AC:Task 1:1 is documented as ideal rule, but many existing features (F092, F140, F148, F158, F256, etc.) have Tasks covering multiple related ACs. Severity should be minor, not major
- [resolved-accepted] Phase1-Uncertain iter1: F590 is already [DONE]. Per SSOT, Predecessor vs Related only matters for blocking check when predecessor is not DONE. Since F590 is DONE, the type has no practical effect. However, semantically schema is a required input, so Predecessor could be argued as more accurate
- [resolved-applied] Phase1-Uncertain iter4: AC#15 Expected pattern 'tools/com-validator.*Community YAML validator' expects both path and description on same line in CLAUDE.md. Current CLAUDE.md Project Structure uses tree format where path and comment may be on same line or separate. Pattern may not match.
- [resolved-applied] Phase1-Uncertain iter7: AC#14 pattern '文字列.*数値' assumes specific error message format. Japanese error output for type mismatch may use different wording. Pattern is unverified against actual implementation.
- [resolved-applied] Phase1-Uncertain iter8: Review Notes contains [pending] item for AC#14 pattern. Question whether unresolved pending items should block leaving PROPOSED status - not explicitly prohibited by SSOT but indicates incomplete review.
- [resolved-applied] Phase1-Uncertain iter9: Git hook naming convention - existing pre-commit has no extension while proposed schema-sync-check.sh uses .sh extension. Both work in Git Bash but naming inconsistency could be confusing. User decision: Use extension-less naming for consistency with existing pre-commit.
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| No current handoffs | Initial feature creation | N/A | N/A |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-23 | create | feature-creator | Feature creation | PROPOSED |
| 2026-01-24 | init | initializer | Status change [REVIEWED]→[WIP] | OK |
| 2026-01-24 | DEVIATION | implementer | Task 1 - go build | BLOCKED:MISSING_PREREQUISITE - Go compiler not installed |
| 2026-01-24 | resolved | user | Go installation | Go 1.25.6 installed, build OK |
| 2026-01-24 13:44 | END | implementer | Task 2 | SUCCESS - Japanese localization implemented |
| 2026-01-24 07:42 | END | implementer | Task 3 | SUCCESS - Schema embedding and validation logic implemented |
| 2026-01-24 07:45 | END | implementer | Task 4 | SUCCESS - Windows batch file created |
| 2026-01-24 07:47 | END | implementer | Task 9 | SUCCESS - Pre-commit hook implemented and tested |
| 2026-01-24 07:49 | END | implementer | Task 5 | SUCCESS - Documentation created (README.md, community-tools.md) |
| 2026-01-24 07:52 | END | implementer | Task 6 | SUCCESS - Validation functionality tested (AC#12-14,19-20 verified) |

## Links

- [index-features.md](index-features.md)
- [feature-590.md](feature-590.md) - YAML Schema Validation Tools
- [feature-599.md](feature-599.md) - IDE/Editor Integration for YAML Schema