# Infra Type Quality Guide

Issues specific to `Type: infra` features (Workflow/Review).

---

## Granularity

- **Change set** as one feature
- No strict volume limit
- AC count: 8-15

---

## Characteristics

Infra features modify:
- Workflow documentation (.md)
- Agent definitions (.claude/agents/)
- Skill definitions (.claude/skills/)
- Configuration files
- Git hooks

---

## Common Issues

### Issue 1: Documentation Consistency Not Verified

**Symptom**: Changed docs without cross-reference verification.

**Example (Good)**:
```markdown
| 5 | SSOT consistency | file | /audit | succeeds | - | [ ] |
```

Or manual verification:
```markdown
| 5 | CLAUDE.md updated | file | Grep | contains | "feature-quality" | [ ] |
| 6 | Skills table updated | file | Grep | contains | "feature-quality.*FL review" | [ ] |
```

---

### Issue 2: Link Validation Missing

**Symptom**: New docs created without link verification.

**Example (Good)**:
```markdown
| 7 | All links valid | file | reference-checker | succeeds | - | [ ] |
```

With AC Details:
```markdown
**AC#7**: All markdown links in new/modified files resolve correctly
- Internal links point to existing files
- Anchor links point to existing headers
```

---

### Issue 3: Agent/Skill Format Not Verified

**Symptom**: New agent/skill without format validation.

**Agent format requirements**:
- Located in `.claude/agents/`
- Has clear Purpose section
- Has Procedure section with steps

**Skill format requirements**:
- YAML frontmatter with `name` and `description`
- `name`: lowercase, hyphens, max 64 chars
- `description`: max 1024 chars, includes when to use

**Example (Good)**:
```markdown
| 8 | SKILL.md format valid | file | Grep | contains | "^---\nname:" | [ ] |
| 9 | description present | file | Grep | contains | "description:" | [ ] |
```

---

### Issue 4: Hook Testing Not Included

**Symptom**: Git hook changes without verification.

**Example (Good)**:
```markdown
| 10 | pre-commit hook works (Pos) | exit_code | git commit | succeeds | - | [ ] |
| 11 | pre-commit blocks bad commit (Neg) | exit_code | git commit | fails | - | [ ] |
```

---

### Issue 5: Rollback Plan Missing

**Symptom**: Workflow change without rollback strategy.

**Example (Good)**:
```markdown
## Implementation Contract

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix
```

---

### Issue 6: Impact Analysis Missing

**Symptom**: Change without documenting what it affects.

**Example (Good)**:
```markdown
## Background

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| CLAUDE.md | Add skill reference | All agents see new skill |
| .claude/skills/ | New skill files | /fc auto-loads quality checks |
| feature-template.md | (unchanged) | N/A |
```

---

### Issue 7: Slash Command in Bash AC

**Symptom**: AC uses Bash method with slash command as Expected value.

**Example (Bad)**:
```markdown
| 6 | Documentation consistency verified | file | Bash | succeeds | "/audit" | [ ] |
```
`/audit` is a slash command, not a Bash command. This AC would fail with "command not found".

**Example (Good)**:
```markdown
| 6 | Documentation consistency verified | manual | /audit | succeeds | "No issues found" | [ ] |
```
Use `manual` type with `/audit` as Method, or use the pattern from Issue 1 (`file | /audit | succeeds`).

**Fix**: For verification via slash commands, use `Type: manual | Method: /command` or reference-checker agent.

---

### Issue 8: Deferred Tasks Not Tracked (Post-Phase Review)

**Symptom**: Post-Phase Review feature doesn't verify that deferred tasks from predecessor features are properly handed off to the next Phase.

**Example**: F473 had `// prerequisite checks deferred to future feature` but F470 (Post-Phase Review) didn't verify this was added to Phase 14 Tasks.

**Required AC for Post-Phase Review**:
```markdown
| N | Deferred tasks tracked | file | Grep | contains | "{deferred task}" in architecture.md Phase N+1 | [ ] |
```

**Verification Steps**:
1. Check all predecessor features' Deferred Tasks sections
2. Check predecessor features' code comments for `deferred`, `TODO`, `future feature`
3. Verify each deferred task is in `architecture.md` next Phase Tasks
4. Verify each deferred task is in next Phase Planning feature

**Example (Good)**:
```markdown
**AC#13**: Deferred tasks from Phase 13 tracked
- Test: Grep "SCOMF prerequisite" in architecture.md Phase 14 Tasks
- Verifies: F473's deferred IsScenarioAvailable checks are in Phase 14
```

**Rationale**: Post-Phase Review's purpose is **leak prevention**. Without this AC, deferred tasks can be forgotten.

---

### Issue 9: Phase/Section Naming Confusion

**Symptom**: Workflow changes add numbered phases/steps without clarifying naming scheme.

**Example (Bad)**:
```markdown
Add as Phase 8 between Phase 7 and Section 8. Sections 8,9,10 become 9,10,11.
```
Confuses "Phase 8 (in-loop)" with "Section 8 (post-loop header)".

**Example (Good)**:
```markdown
Add Phase 8 pseudocode within WHILE loop (after Phase 7, before BREAK).
Note: Documentation sections (## 8. Report) are separate from Phase numbers.
```

**Fix**: Explicitly distinguish between in-loop Phase numbers and document Section headers.

---

### Issue 10: Related Feature in Links but not Dependencies

**Symptom**: A feature is mentioned in Summary and appears in Links section, but not in Dependencies table. Repeated FL warnings about adding it to Dependencies.

**Example (Bad)**:
```markdown
## Summary
Phase 16 features (F509-F514, per F503 scope; F517 is a follow-up fix)

## Dependencies
| Predecessor | F514 | ... |
| Successor | F516 | ... |
<!-- F517 not listed even though mentioned in Summary -->

## Links
- [feature-517.md](feature-517.md) - Phase 16 follow-up fix
```

**Resolution**: If a feature is **referenced but not a dependency** (no blocking relationship), listing it in Links is sufficient. Dependencies table is for Predecessor/Successor/blocking Related features only.

**Example (Good)**:
```markdown
## Links
- [feature-517.md](feature-517.md) - Phase 16 follow-up fix (not in review scope)

## Review Notes
- [resolved] F517 in Links is sufficient. Adding to Dependencies would be duplicative.
```

**Fix**: Document resolution in Review Notes rather than adding duplicate entries. Links section is for traceability, Dependencies table is for blocking relationships.

---

### Issue 11: Redundant Grep Alternation Patterns

**Symptom**: Grep pattern uses alternation where one branch already covers another.

**Example (Bad)**:
```markdown
| 5 | File loading time recorded | file | Grep | contains | "loading.*time|load.*time" | [ ] |
```
`load.*time` already matches `loading.*time`, so the `loading.*time|` part is redundant.

**Example (Good)**:
```markdown
| 5 | File loading time recorded | file | Grep | contains | "load.*time" | [ ] |
```

**Fix**: Simplify to the more general pattern that covers all intended matches.

---

### Issue 12: count_equals Pattern Too Broad

**Symptom**: AC uses count_equals with a pattern that may match unintended occurrences (e.g., prose mentions, comments).

**Example (Bad)**:
```markdown
| 7 | Exactly 1 Skill call | file | Grep | count_equals | "Skill(initializer" | 1 | [ ] |
```
Pattern `Skill(initializer` could match prose like "Skill(initializer is the recommended pattern".

**Example (Good)**:
```markdown
| 7 | Exactly 1 Skill dispatch | file | Grep | count_equals | "Skill(initializer," | 1 | [ ] |
```
Adding comma after function name ensures only actual dispatch calls (with arguments) are counted.

**Fix**: For count_equals verification, include distinguishing suffix (comma, paren, colon) to exclude prose mentions.

---

### Issue 13: Regex Braces in Expected Not Escaped

**Symptom**: AC pattern contains `{ID}` or similar placeholder which ripgrep interprets as regex quantifier.

**Example (Bad)**:
```markdown
| 5 | Syntax valid | file | Grep | matches | "Skill\\(init,.*args.*{ID}" | [ ] |
```
Ripgrep error: `repetition quantifier expects a valid decimal`

**Example (Good)**:
```markdown
| 5 | Syntax valid | file | Grep | matches | "Skill\\(init,.*args.*\\{ID\\}" | [ ] |
```
Braces escaped with `\\{` and `\\}` for literal matching.

**Fix**: When Expected contains literal braces, escape them with `\\{` and `\\}` for ripgrep compatibility.

---

### Issue 14: Pipe Character in Grep Pattern

**Symptom**: AC pattern contains literal pipe `|` which ripgrep interprets as regex alternation.

**Example (Bad)**:
```markdown
| 1 | Type table updated | file | Grep | contains | "research | /fc" | [ ] |
```
The `|` between `research` and `/fc` is interpreted as OR, matching either "research " OR " /fc" independently.

**Example (Good)**:
```markdown
| 1 | Type table updated | file | Grep | contains | "research.*/fc" | [ ] |
```
Use `.*` instead of literal pipe to match any characters between the components.

**Fix**: For patterns with markdown table cells, use `.*` regex wildcard instead of literal pipe character. **Note**: Issue 14 workaround (`.*` substitution) is no longer necessary for complex method `pattern=` parameters after F817 fixes the pipe-escaping bug in ac-static-verifier.py. The `unescape()` function now correctly handles `\\(`, `\\)`, `\\.`, `\\w`, `\\?` and other markdown-escaped metacharacters. The `.*` workaround remains relevant only for patterns in the Expected column that contain literal pipe characters as data (not regex alternation).

---

### Issue 15: Status Update Missing index-features.md

**Symptom**: Feature spec describes status updates to feature-{ID}.md but forgets index-features.md must also be updated.

**Example (Bad)**:
```markdown
| 2 | Sets BLOCKED | file | Grep(fl.md) | contains | "\\[BLOCKED\\].*Predecessor" | [ ] |
<!-- Missing: index-features.md update verification -->
```

**Example (Good)**:
```markdown
| 2 | Sets BLOCKED | file | Grep(fl.md) | contains | "\\[BLOCKED\\].*Predecessor" | [ ] |
| 11 | Updates index-features.md | file | Grep(fl.md) | contains | "index-features.md.*\\[BLOCKED\\]" | [ ] |
```

**Fix**: When a feature modifies status in feature-{ID}.md, add AC to verify index-features.md is also updated.

---

### Issue 16: Implementation Contract Step Numbering Ambiguity

**Symptom**: Implementation Contract specifies insertion between existing steps without explicit renumbering instructions.

**Example (Bad)**:
```markdown
Insert as Step 0.3.5 between Step 0.3 and existing Step 0.4:
```
Could confuse implementer about whether existing Step 0.4 needs renumbering.

**Example (Good)**:
```markdown
Insert as Step 0.4 (renumber existing Step 0.4 to Step 0.5):
```

**Fix**: Explicitly state renumbering requirement when inserting between existing numbered steps.

---

### Issue 27: Problem Premise Based on Code Misunderstanding

**Symptom**: Feature created to fix a bug that doesn't actually exist due to misunderstanding existing code logic.

**Example (Bad)**:
```markdown
### Problem
F609 discovery: ac-static-verifier.py counts MANUAL as failed. Line 586: `failed = total - passed - manual` means any MANUAL AC causes exit code 1.
```
When actual code shows `failed = total - passed - manual` correctly **excludes** MANUAL from failed count.

**Example (Good)**:
```markdown
### Problem
**INVESTIGATION COMPLETE**: F609 discovery was based on misunderstanding. Current ac-static-verifier.py correctly handles MANUAL status:
- Line 586: `failed = total - passed - manual` EXCLUDES MANUAL from failed count
- CONCLUSION: No bug exists. This feature is unnecessary as described.
```

**Fix**: Always verify code behavior before creating feature. Read relevant source carefully to understand actual logic.

---

## Checklist

- [ ] Documentation consistency verified (SSOT)
- [ ] Index-features.md update verified when status changes (Issue 15)
- [ ] Slash commands not used as Bash commands (Issue 7)
- [ ] All links validated
- [ ] Agent/Skill format requirements met
- [ ] Git hooks tested (Pos and Neg)
- [ ] Rollback plan documented
- [ ] Impact analysis included
- [ ] 8-15 AC count respected
- [ ] Grep patterns without redundant alternations (Issue 11)
- [ ] count_equals patterns specific enough (Issue 12)
- [ ] Regex braces escaped for ripgrep (Issue 13)
- [ ] Pipe characters in Grep patterns use `.*` instead (Issue 14; F817 fixed complex method path — `.*` workaround no longer needed for `pattern=` params)
- [ ] Phase/Section numbering clearly distinguished (Issue 9)
- [ ] Implementation Contract step numbering explicit (Issue 16)
- [ ] **Post-Phase Review only**: Deferred tasks tracked in next Phase (Issue 8)
- [ ] Milestone thresholds match Implementation Contract calculations (Issue 17)
- [ ] AC count Note updated after adding new ACs (Issue 18)
- [ ] AC Type/Method consistency for slash commands (Issue 19)
- [ ] Grep Method includes path specification (Issue 20)
- [ ] Line number references avoided - use section names instead (Issue 21)
- [ ] AC Expected values specific enough (Issue 23)
- [ ] Slash commands not assumed executable via subprocess (Issue 24)
- [ ] AC Method column uses method names, not full commands (Issue 25)
- [ ] Count consistency between descriptive text and enumerated lists (Issue 26)
- [ ] Problem premise verified by reading actual code (Issue 27)
- [ ] Philosophy Derivation table AC mappings correct (Issue 28)
- [ ] AC:Task 1:1 mapping maintained throughout feature (Issue 30)
- [ ] Handoff destinations verified to exist before referencing (Issue 31)
- [ ] CLI tool exit code behavior verified from source (Issue 32)
- [ ] Grep patterns use ripgrep syntax (unescaped |) (Issue 33)
- [ ] Technical Design avoids acknowledged limitations that defer complexity (Issue 35)
- [ ] Technical Design verified against existing codebase infrastructure (Issue 36)
- [ ] Grep patterns avoid unintended dot wildcards (Issue 37)
- [ ] exit_code Type only used with Expected values 0-255 (Issue 38)
- [ ] Replacement file existence verified when removal ACs are present (Issue 39)
- [ ] Key Decisions table uses template columns: Decision, Options Considered, Selected, Rationale (V1j, F818 lesson)
- [ ] Non-template subsections removed (e.g., Success Criteria — redundant with AC table) (V1k, F818 lesson)
- [ ] count_gte/count_equals used when verifying N occurrences, not contains (F818 lesson)

---

### Issue 17: Milestone Threshold Calculation Inconsistency

**Symptom**: AC milestone thresholds don't match the actual file count calculations in Implementation Contract.

**Example (Bad)**:
```markdown
| 5b | Phase B effects milestone | ... | >=85 | [ ] |
<!-- But Implementation Contract shows Phase A (57) + Phase B (45) = 102 cumulative -->
```

**Example (Good)**:
```markdown
**Milestone Breakdown**:
- Phase A: 57 files (14+17+26)
- Phase B: 45 files (17+11+17) - Cumulative 102 files
| 5b | Phase B effects milestone | ... | >=102 | [ ] |
```

**Fix**: Ensure AC milestone thresholds match Implementation Contract calculations. Add explicit file count breakdown in Milestone section.

---

### Issue 18: AC Count Update After Additions

**Symptom**: Feature Note about AC count not updated when new ACs are added during FL review.

**Example (Bad)**:
```markdown
**Note**: 22 ACs exceed typical range...
<!-- But AC table has 23 ACs after adding AC#20 -->
```

**Example (Good)**:
```markdown
**Note**: 23 ACs exceed typical infra feature range (8-15) but are justified by complex infrastructure scope.
```

**Fix**: Update AC count Note when adding new ACs during review iterations.

---

### Issue 19: AC Type/Method Mismatch with Slash Commands

**Symptom**: AC uses `exit_code` Type with slash command (e.g. `/audit`) as Method, but slash commands don't return shell exit codes.

**Example (Bad)**:
```markdown
| 4 | Documentation consistency verified | exit_code | /audit | succeeds | - | [ ] |
```
`exit_code` Type expects shell commands that return exit codes (0=success, non-zero=failure). Slash commands like `/audit` are Claude Code internal commands.

**Example (Good)**:
```markdown
| 4 | Documentation consistency verified | file | /audit | succeeds | - | [ ] |
```
Use `file` Type when verifying with slash commands that produce output verification.

**Fix**: Use `Type: file` for slash command verification, or `Type: manual` for human verification of slash command output.

---

### Issue 20: Grep Method Missing Path Specification

**Symptom**: AC Method shows "Grep" without path specification, making test execution unclear.

**Example (Bad)**:
```markdown
| 2 | No remaining references | file | Grep | not_contains | "DEAD_CODE" | [ ] |
```

**Example (Good)**:
```markdown
| 2 | No remaining references | file | Grep(pm/reference/) | not_contains | "DEAD_CODE" | [ ] |
```

**Fix**: Always specify path in Method column as `Grep(path)` for file/code Type ACs to clarify test scope.

---

### Issue 21: Line Number References in Documentation

**Symptom**: Implementation Contract or AC references specific line numbers (e.g., "SKILL.md line ~189") which become invalid when files are edited.

**Example (Bad)**:
```markdown
**persist_pending Definition**: See SKILL.md line ~189.
```

**Example (Good)**:
```markdown
**persist_pending Definition**: See SKILL.md section "persist_pending Definition".
```

**Fix**: Use section/header references instead of line numbers. Section names are stable across edits while line numbers shift.

---

### Issue 22: AC Details vs AC Table Mismatch

**Symptom**: AC table (Type/Method/Matcher/Expected) and AC Details describe different verification methods.

**Example (Bad)**:
```markdown
| 5 | Documentation verified | file | Grep(.claude/commands/audit.md) | contains | "Audit" | [ ] |

**AC#5**: Documentation consistency verification via audit command
- Method: /audit command succeeds with no SSOT violations
```
AC table uses Grep file verification, but AC Details describes running `/audit` command.

**Example (Good)**:
```markdown
| 5 | audit.md contains expected description | file | Grep(.claude/commands/audit.md) | contains | "Audit documentation consistency" | [ ] |

**AC#5**: audit.md contains expected description
- Method: Grep(.claude/commands/audit.md) for "Audit documentation consistency"
- Expected: Verifies audit command documentation exists
```

**Fix**: Ensure AC Details matches AC table. If AC table uses Grep, AC Details should describe Grep verification. If AC Details describes command execution, AC table should use `manual` type.

---

### Issue 23: AC Expected Values Too Generic

**Symptom**: AC Expected values use generic terms that could match unrelated code.

**Example (Bad)**:
```markdown
| 1 | Parser handles escapes | file | Grep | contains | "replace" | [ ] |
| 7 | Documentation updated | file | Grep | contains | "escape" | [ ] |
```
"replace" or "escape" are common words that could appear in unrelated contexts.

**Example (Good)**:
```markdown
| 1 | Parser handles escapes | file | Grep | contains | "unescape" | [ ] |
| 7 | Documentation updated | file | Grep | contains | "backslash escape" | [ ] |
```

---

### Issue 24: Slash Command Subprocess Execution Assumption

**Symptom**: Feature assumes slash commands can be executed via subprocess.Popen() or similar shell execution.

**Example (Bad)**:
```markdown
def _execute_slash_command(self, command: str) -> Dict[str, Any]:
    """Execute slash command and return result."""
    result = subprocess.run([command], capture_output=True)
```
Implementation Contract assumes slash commands like `/audit` can be executed as shell commands.

**Example (Good)**:
```markdown
def _handle_slash_command_ac(self, command: str) -> Dict[str, Any]:
    """Handle slash command AC by marking as manual verification."""
    return {"status": "MANUAL", "message": f"Manual verification required for {command}"}
```

**Fix**: Testing SKILL line 79 explicitly states "Slash commands (e.g., /audit) are not shell commands and don't return exit codes." Alternative approaches include: (1) Mark slash command ACs as manual verification type, (2) Check for command output files/logs, or (3) Document verification limitation.

**Fix**: Use specific patterns that clearly relate to the feature's functionality rather than generic terms.

---

### Issue 25: AC Table Method Column Contains Full Commands

**Symptom**: AC table Method column contains entire command strings instead of method references.

**Example (Bad)**:
```markdown
| 12 | Validation test | exit_code | cd src/tools/go/com-validator && go build -o com-validator.exe && ./com-validator.exe file.yaml | succeeds | - | [ ] |
```

**Example (Good)**:
```markdown
| 12 | Validation test | exit_code | Bash | succeeds | cd src/tools/go/com-validator && go build -o com-validator.exe && ./com-validator.exe file.yaml | [ ] |
```

**Fix**: Use "Bash" in Method column and move full command to Expected column for proper AC table format.

---

### Issue 26: Count Inconsistency Between Text and Lists

**Symptom**: Descriptive text claims a different count than the actual enumerated list contains.

**Example (Bad)**:
```markdown
Nine additional matchers documented in Testing SKILL require audit verification: not_contains, matches, succeeds, fails, not_exists, gt/gte/lt/lte, count_equals.
```
The text says "Nine" but the list actually contains 10 when gt/gte/lt/lte are counted individually.

**Example (Good)**:
```markdown
Ten additional matchers documented in Testing SKILL require audit verification: not_contains, matches, succeeds, fails, not_exists, gt, gte, lt, lte, count_equals.
```

**Fix**: Count the actual items in lists and ensure descriptive text matches. Consider expanding grouped items (like gt/gte/lt/lte) when counting.

---

### Issue 28: Philosophy Derivation Table AC Mapping Inconsistency

**Symptom**: Philosophy Derivation table maps abstract claims to wrong ACs that don't actually verify those claims.

**Example (Bad)**:
```markdown
| "maintains SSOT integrity" | Documentation consistency verified | AC#12 |
<!-- But AC#12 is "YamlValidator tool exists" - tool existence, not SSOT verification -->
```

**Example (Good)**:
```markdown
| "maintains SSOT integrity" | Documentation consistency verified | AC#16 |
<!-- AC#16: SSOT consistency verified | file | Grep(...) | contains | "SSOT Consistency Verified" -->
```

**Fix**: Ensure Philosophy Derivation table AC references match actual AC purposes. Tool existence checks don't verify SSOT consistency; require dedicated verification ACs.

---

### Issue 29: Regex Pattern Field Order Dependency

**Symptom**: AC uses `matches` matcher with regex pattern that assumes specific JSON/dict field order in source code.

**Example (Bad)**:
```markdown
| 9 | JSON schema preserved | code | Grep(file.py) | matches | "summary.*total.*passed.*manual.*failed" | [ ] |
```
If source code reorders fields (e.g., `total, failed, passed, manual`), AC fails even though schema is preserved.

**Example (Good)**:
```markdown
| 9 | JSON schema preserved | code | Grep(file.py) | contains | "\"summary\":" | [ ] |
```
Or split into multiple field existence checks:
```markdown
| 9a | summary field exists | code | Grep(file.py) | contains | "\"total\":" | [ ] |
| 9b | passed field exists | code | Grep(file.py) | contains | "\"passed\":" | [ ] |
```

**Fix**: Use `contains` for field existence checks instead of `matches` with order-dependent regex. For JSON structure verification, check individual fields independently.

---

### Issue 30: AC:Task 1:1 Mapping Violations During FL Iterations

**Symptom**: Multiple ACs mapped to single Task during feature specification, violating the project principle "1 AC = 1 Task = 1 Dispatch".

**Example (Bad)**:
```markdown
| 4 | 4,5 | Consolidate verify_code_ac and _verify_file_content... | [ ] |
| 7 | 9,10,11,12 | Verify preserved contracts... | [ ] |
```
One Task covers multiple ACs, violating 1:1 mapping requirement.

**Example (Good)**:
```markdown
| 4 | 4 | Run literal pattern regression test | [ ] |
| 5 | 5 | Run regex pattern regression test | [ ] |
| 9 | 9 | Verify CLI interface preserved | [ ] |
| 10 | 10 | Verify JSON output schema preserved | [ ] |
```

**Fix**: Split multi-AC tasks during specification phase to maintain strict 1:1 AC:Task mapping throughout the feature lifecycle.

---

### Issue 31: TBD Prohibition Violation in Handoffs

**Symptom**: Mandatory Handoffs reference non-existent destinations like "F555 Phase 19 Mandatory Handoffs" where the target feature lacks the referenced section.

**Example (Bad)**:
```markdown
| Non-KOJO file conversion | F555 Phase 19 Mandatory Handoffs | Tracked as systemic issue... |
```
Where F555 contains no Mandatory Handoffs section.

**Example (Good)**:
```markdown
| Non-KOJO file conversion | F672 | Tracked as systemic issue... |
```
Where F672 is a concrete new feature or existing feature with the referenced section.

**Fix**: Verify handoff destinations exist before referencing them. Create concrete features or sections rather than referencing non-existent destinations.

---

### Issue 32: CLI Tool Exit Code Assumptions

**Symptom**: ACs assume CLI tools return exit code 0 for partial success when tools are designed to return exit code 1 on any failures.

**Example (Bad)**:
```markdown
| 1 | Batch conversion succeeds | exit_code | ErbToYaml --batch | succeeds | exit code 0 | [ ] |
```
When the tool returns exit code 1 for partial failures (some files failed).

**Example (Good)**:
```markdown
| 1 | Batch conversion completes | exit_code | ErbToYaml --batch | fails | exit code 1 | [ ] |
```
Or use output verification instead:
```markdown
| 1 | Batch conversion completes | output | ErbToYaml --batch | contains | "Total: 10, Success: 6, Failed: 4" | [ ] |
```

**Fix**: Read tool source code to verify actual exit code behavior, especially for batch operations with expected partial failures.

---

### Issue 33: Grep Pattern Syntax Mismatch

**Symptom**: Using grep-style escaped alternation (\\|) instead of ripgrep-style unescaped alternation (|).

**Example (Bad)**:
```markdown
| 7 | No technical debt | file | Grep | count_equals | "TODO\\|FIXME\\|HACK" | 0 | [ ] |
```
Ripgrep treats \\| as literal pipe, not alternation.

**Example (Good)**:
```markdown
| 7 | No technical debt | file | Grep | count_equals | "TODO|FIXME|HACK" | 0 | [ ] |
```

**Fix**: Use unescaped pipes (|) for alternation in Grep patterns, as the Grep tool uses ripgrep syntax.

---

### Issue 34: MSBuild Inheritance Inconsistent Opt-out Coverage

**Symptom**: Feature handles _out/tmp/ Directory.Build.props inheritance with opt-out but overlooks src/tools/dotnet/_archived/ which also inherits from repository root.

**Example (Bad)**:
```markdown
### Out-of-scope .csproj files (excluded)
| _out/tmp/*.csproj (8 files) | Temporary/debug projects, not part of the build |
| src/tools/dotnet/_archived/ErbLinter*.csproj (2 files) | Archived, not compiled |

### Task 2: Create _out/tmp/Directory.Build.props with TreatWarningsAsErrors=false opt-out
```
Only _out/tmp/ gets opt-out treatment despite both directories being affected by MSBuild inheritance.

**Example (Good)**:
```markdown
### Out-of-scope .csproj files (excluded)
| _out/tmp/*.csproj (8 files) | Temporary/debug projects (gets opt-out Directory.Build.props) |
| src/tools/dotnet/_archived/ErbLinter*.csproj (2 files) | Archived (gets opt-out Directory.Build.props) |

### Task 2: Create opt-out Directory.Build.props files (_out/tmp/ and src/tools/dotnet/_archived/)
```

**Fix**: When analyzing MSBuild inheritance, systematically identify ALL subdirectories under repository root, not just _out/tmp/. Both excluded directories need consistent opt-out treatment.

---

### Issue 35: Technical Design Single-Character Escape Logic Limitations

**Symptom**: Technical Design chooses simple single-character lookback approach with acknowledged limitations that violate Zero Debt Upfront principle.

**Example (Bad)**:
```markdown
**Decision 3: Escaped quote detection**
- **Chosen**: Check `line[i-1] != '\\'` before toggling quote state
- **Limitation**: This only handles single-level escape (e.g., `\"` correctly, but `\\"` would fail)
- **Mitigation**: If this edge case appears, extend the state machine later
```
Defers complexity to avoid upfront cost, creating potential future technical debt.

**Example (Good)**:
```markdown
**Decision 3: Escaped quote detection**
- **Chosen**: Count consecutive backslashes before quote character and check parity
- **Approach**: Even count (including 0) = unescaped quote; odd count = escaped quote
- **Handles**: All escape levels (`\"`, `\\"`, `\\\"`, etc.) correctly without limitations
```

**Fix**: Design for complete correctness from the start rather than accepting known limitations. "Pay large costs now to eliminate future technical debt" principle requires proper escape handling upfront, not deferred workarounds.

---

### Issue 36: Technical Design Built on Incomplete Codebase Investigation

**Symptom**: Technical Design proposes creating new infrastructure (classes, parsers, frameworks) when equivalent or superior infrastructure already exists in the codebase. Root Cause Analysis investigated a narrow code path and missed broader infrastructure.

**Example (Bad)**:
```markdown
### Root Cause Analysis
TalentConditionParser only matches TALENT: pattern. No compound parser exists.

### Technical Design
Create new CompoundConditionParser class with tokenization for &&/||/! operators.
```
When the same project already contains `ConditionExtractor`, `LogicalOperatorParser` (with `&&`/`||` precedence AND parenthesis support), `CflagConditionParser`, and `FunctionCallParser`.

**Example (Good)**:
```markdown
### Root Cause Analysis
DatalistConverter.ParseCondition() bypasses existing ConditionExtractor/LogicalOperatorParser infrastructure and delegates directly to TalentConditionParser.

### Technical Design
Wire DatalistConverter.ParseCondition() to use ConditionExtractor. Add TcvarConditionParser (new). Implement ICondition-to-YAML conversion layer.
```

**Fix**: During /fc Phase 2 (deep-explorer), search the ENTIRE project for related infrastructure before concluding something doesn't exist. Use `Glob("**/*Parser*.cs")`, `Glob("**/*Condition*.cs")`, `Grep("&&")` to discover existing parsers and infrastructure. A single missed file can invalidate the entire Technical Design.

---

### Issue 37: Grep Pattern Unintended Dot Wildcard

**Symptom**: AC grep pattern uses `.` (dot) as literal character but grep interprets it as regex wildcard matching any character.

**Example (Bad)**:
```markdown
| 2 | talentType:16 present | code | Grep(Game/YAML/Kojo/) | count_equals | "talentType: .16." | 608 | [ ] |
```
`.16.` matches `talentType: "16"` but also `talentType: 160` or `talentType: x16y`. If the actual file uses unquoted `talentType: 16`, the pattern may not even match (`.` before `16` consumes the digit `1`).

**Example (Good)**:
```markdown
| 2 | talentType:16 present | code | Grep(Game/YAML/Kojo/) | count_equals | "talentType: 16" | 608 | [ ] |
```

**Fix**: Use exact string matching without dots. If quotes are expected, use explicit `"` or `\\"` in the pattern. Never use `.` as a substitute for quote characters.

---

### Issue 38: exit_code Type with Expected > 255

**Symptom**: AC uses `Type: exit_code` but Expected value exceeds the 0-255 exit code range (POSIX/Windows limitation).

**Example (Bad)**:
```markdown
| 1 | Target file count | exit_code | Bash | equals | 608 | [ ] |
```
Exit codes are limited to 0-255. `wc -l` outputs to stdout, not as exit code. With `exit_code` type, the ac-tester checks the command's exit code (0 on success), not the stdout count.

**Example (Good)**:
```markdown
| 1 | Target file count | output | Bash | equals | 608 | [ ] |
```

**Fix**: Use `Type: output` when Expected is a stdout value (count, string). Use `Type: exit_code` only when checking command success/failure (Expected: 0, succeeds, fails).

---

### Issue 39: Removal AC Without Replacement Existence AC

**Symptom**: ACs verify old code is removed (count=0) but no AC verifies the replacement file/class exists at the designed path. Build success alone doesn't enforce architectural placement.

**Example (Bad)**:
```markdown
| 2 | Private helpers removed from State/ | code | Grep(src/Era.Core/State/) | count_equals | "private.*GetCFlag" = 0 | [ ] |
| 20 | SSOT documents new class | file | Grep(SKILL.md) | matches | "NewExtensions" | [ ] |
<!-- No AC verifying NewExtensions.cs exists at designed path -->
```
A developer could satisfy removal ACs by inlining code or placing helpers in an existing file, bypassing the designed architecture.

**Example (Good)**:
```markdown
| 2 | Private helpers removed from State/ | code | Grep(src/Era.Core/State/) | count_equals | "private.*GetCFlag" = 0 | [ ] |
| 20 | Extension class exists at designed path | code | Grep(src/Era.Core/Interfaces/NewExtensions.cs) | matches | "public static class NewExtensions" | [ ] |
| 21 | SSOT documents new class | file | Grep(SKILL.md) | matches | "NewExtensions" | [ ] |
```

**Fix**: When a Task creates a new file to replace removed code, add an AC verifying the replacement file exists at the designed path with the expected class declaration. Removal ACs (count=0) + build success do not guarantee architectural compliance.

**Extension (F782)**: Beyond file existence, verify **call-site adoption** — that the replacement is actually used by consumers. Example: AC#21 `Grep("_variables\\.GetCFlag|_variables\\.SetCFlag", path="src/Era.Core/State/") >= 3` verifies State files call the shared extension methods, preventing inline reimplementation that passes removal ACs.
