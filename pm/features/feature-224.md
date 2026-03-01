# Feature 224: Agent STOP Conditions

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

Establish a self-correcting CI/CD pipeline where LLM agents strictly adhere to documented workflows and STOP when they cannot proceed correctly. **"Follow the flow, or STOP if you cannot"** must be enforced across all subagents through explicit STOP conditions that prevent independent judgment-based continuation.

### Problem (Current Issue)

F223 audit revealed that multiple agents lack explicit STOP conditions for known failure scenarios, leading to cascading failures:

| Issue | Agent | Missing STOP Condition | Risk |
|:-----:|-------|------------------------|------|
| A1 | kojo-writer | Existing stub function found | Duplicate functions ↁEOutput invalidation |
| A2 | kojo-writer | Target file does not exist | Unknown write target ↁECrash |
| A3 | kojo-writer | Different character code found | Editing wrong character's dialogue |
| A4 | implementer | Code contradicts existing implementation | Inconsistent implementation |
| A5 | implementer | Documentation vs reality mismatch | Implementing based on wrong assumptions |
| A7 | ac-tester | 3 consecutive failures | Infinite loop |
| A8 | initializer | Feature already [DONE] | Double execution |
| A9 | implementer | Task complexity exceeds Sonnet capabilities | Incomplete implementation |

These gaps cause agents to proceed with invalid assumptions or incomplete information, resulting in bugs like F187's duplicate functions (K2/K4/K9) and F190's COM duplication across 6 characters.

### Goal (What to Achieve)

Define explicit STOP conditions in agent .md files for all identified failure scenarios (A1-A5, A7-A9), ensuring agents halt execution and report blockers rather than proceeding with invalid states.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writer: Existing stub STOP | code | Grep | contains | "BLOCKED:EXISTING_STUB" | [x] |
| 2 | kojo-writer: File not exists STOP | code | Grep | contains | "BLOCKED:FILE_NOT_FOUND" | [x] |
| 3 | kojo-writer: Wrong char code STOP | code | Grep | contains | "BLOCKED:WRONG_CHARACTER" | [x] |
| 4 | implementer: Code conflict STOP | code | Grep | contains | "BLOCKED:CODE_CONFLICT" | [x] |
| 5 | implementer: Doc mismatch STOP | code | Grep | contains | "BLOCKED:DOC_MISMATCH" | [x] |
| 6 | ac-tester: 3-failure STOP | code | Grep | contains | "3rd failure" | [x] |
| 7 | initializer: DONE state STOP | code | Grep | contains | "BLOCKED:ALREADY_DONE" | [x] |
| 8 | implementer: Complexity STOP | code | Grep | contains | "BLOCKED:COMPLEXITY_EXCEEDED" | [x] |

### AC Details

**Test pattern**: Verify each agent .md file contains documented STOP condition with:
1. Condition description (when to STOP)
2. Detection method (how to recognize the condition)
3. Output format (how to report BLOCKED status)

**Target files by AC**:
| AC# | Target File |
|:---:|-------------|
| 1-3 | `.claude/agents/kojo-writer.md` |
| 4-5, 8 | `.claude/agents/implementer.md` |
| 6 | `.claude/agents/ac-tester.md` |
| 7 | `.claude/agents/initializer.md` |

**Verification command**:
```bash
grep -n "BLOCKED:" .claude/agents/kojo-writer.md .claude/agents/implementer.md .claude/agents/ac-tester.md .claude/agents/initializer.md
```

**Expected**: All 8 STOP conditions documented across 4 agent files.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add "STOP: Existing Stub Detection" section to kojo-writer.md | [x] |
| 2 | 2 | Add "STOP: File Not Found" section to kojo-writer.md | [x] |
| 3 | 3 | Add "STOP: Wrong Character Code" section to kojo-writer.md | [x] |
| 4 | 4 | Add "STOP: Code Conflict Detection" section to implementer.md | [x] |
| 5 | 5 | Add "STOP: Documentation Mismatch" section to implementer.md | [x] |
| 6 | 6 | Verify ac-tester.md contains 3-failure STOP (already documented at L89-90) | [x] |
| 7 | 7 | Add "STOP: Already DONE" section to initializer.md | [x] |
| 8 | 8 | Add "STOP: Complexity Exceeded & Escalation" section to implementer.md | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialization | initializer | Status [PROPOSED]→[WIP] | READY |
| 2025-12-27 | Implementation | implementer | Tasks 1-8 | SUCCESS |
| 2025-12-27 | AC Verification | opus | Grep validation | OK:8/8 |

---

## Dependencies

None (blocks F227, F228)

---

## Links

- [F223](feature-223.md) - Parent audit feature
- [F187](feature-187.md) - K2/K4/K9 duplicate functions (motivating example for A1)
- [F190](feature-190.md) - COM_60 duplication (motivating example for A1)
- [F219](feature-219.md) - TDD protection (related: A6 resolved)

---

## Notes

### Detailed STOP Condition Specifications

#### A1: Existing Stub Detection (kojo-writer)

**Trigger**: When searching for write location, finds existing `@KOJO_MESSAGE_COM_K{N}_{X}` or `@KOJO_MESSAGE_COM_K{N}_{X}_1` function stub.

**Detection method**:
```
Before writing, Grep pattern: @KOJO_MESSAGE_COM_K{N}_{X}(_1)?
If match found ↁESTOP
```

**Output**:
```
BLOCKED:EXISTING_STUB:K{N}
Function @KOJO_MESSAGE_COM_K{N}_{X}_1 already exists in {file_path}:{line}
```

**Context**: F187 K2/K4/K9 duplicates occurred because kojo-writer did not check for existing implementations before writing.

---

#### A2: File Not Found (kojo-writer)

**Trigger**: Target ERB file specified by kojo-writing SKILL COM→File mapping does not exist.

**Detection method**:
```
After consulting SKILL for file path:
If Glob returns no results ↁESTOP
```

**Output**:
```
BLOCKED:FILE_NOT_FOUND:K{N}
Expected file for character {char_name}: {expected_path}
Cannot proceed without valid write target.
```

**Context**: Prevents crashes when COM→File mapping is incomplete or incorrect.

---

#### A3: Wrong Character Code Detection (kojo-writer)

**Trigger**: When opening target file, finds different character code (K1-K10) than assigned.

**Detection method**:
```
After opening file, Grep for @KOJO_MESSAGE_COM_K{OTHER}
If OTHER ≠ N ↁESTOP
```

**Output**:
```
BLOCKED:WRONG_CHARACTER:K{N}
File {path} contains K{OTHER} code, but task is for K{N}
```

**Context**: Prevents cross-character dialogue contamination.

---

#### A4: Code Conflict Detection (implementer)

**Trigger**: Discovers existing code that contradicts task requirements.

**Detection method**:
```
During Read phase:
If existing implementation conflicts with task specification ↁESTOP
```

**Output**:
```
BLOCKED:CODE_CONFLICT:Task{N}
Found existing implementation at {file}:{line} that contradicts task requirements.
Existing: {summary}
Required: {summary}
```

**Context**: Prevents introducing inconsistencies when modifying existing systems.

---

#### A5: Documentation Mismatch (implementer)

**Trigger**: SKILL, CLAUDE.md, or feature spec conflicts with actual codebase behavior.

**Detection method**:
```
During investigation:
If documented behavior ≠ actual behavior ↁESTOP
```

**Output**:
```
BLOCKED:DOC_MISMATCH:Task{N}
Documentation states: {doc_claim}
Actual implementation: {actual_behavior}
Location: {file}:{line}
```

**Escalation**: Report to user for SSOT clarification before proceeding.

**Context**: F223 S1-S14 revealed widespread SSOT violations. Agents must not guess which source is correct.

---

#### A7: 3 Consecutive Failures (ac-tester)

**Status**: ✁EAlready documented in ac-tester.md L89-90

**Verify**: Current text states "On 3rd failure: Report BLOCKED with reason and STOP (do not loop indefinitely)"

**Task 6 is verification only** - confirm existing documentation is sufficient.

---

#### A8: Already DONE Detection (initializer)

**Trigger**: Feature Status is already `[DONE]` when attempting initialization.

**Detection method**:
```
After reading feature-{ID}.md Status field:
If Status = [DONE] ↁESTOP
```

**Output**:
```
BLOCKED:ALREADY_DONE:{ID}
Feature {ID} is already [DONE]. Cannot re-execute.
```

**Context**: Prevents double execution when /do is accidentally run on completed features.

---

#### A9: Complexity Exceeded & Escalation (implementer)

**Trigger**: Task requires capabilities beyond Sonnet's scope (e.g., architectural redesign, multi-file refactoring with complex dependencies).

**Detection method**:
```
During Task planning:
If task requires:
  - Major architectural changes
  - Coordinated changes across >5 files
  - Design decisions beyond clear specifications
ↁESTOP & escalate to Opus
```

**Output**:
```
BLOCKED:COMPLEXITY_EXCEEDED:Task{N}
Task requires capabilities beyond agent scope:
{reason}

Escalation recommended: Dispatch to Opus-level agent or split into subtasks.
```

**Context**: Prevents Sonnet from producing incomplete or inconsistent implementations when task complexity exceeds single-agent capabilities.

---

### Implementation Guidance

For each STOP condition, agent .md files should include:

1. **Section header**: `## STOP: {Condition Name}`
2. **Trigger description**: When this condition occurs
3. **Detection method**: How to recognize it (Grep pattern, file check, etc.)
4. **Output format**: Exact BLOCKED status format to return
5. **Example scenario**: Reference to past issue (F187, F190, etc.)

**Format template**:
```markdown
## STOP: {Condition Name}

**Trigger**: {When to STOP}

**Detection**:
- {Step-by-step detection method}

**Output**:
\```
BLOCKED:{CODE}:{Context}
{Detailed explanation}
\```

**Example**: {Reference to past issue}
```

### Verification Method

After implementation, run:
```bash
grep -n "STOP:" .claude/agents/kojo-writer.md .claude/agents/implementer.md .claude/agents/ac-tester.md .claude/agents/initializer.md
```

Expected: 8+ matches (one per STOP condition, potentially with section headers and references).
