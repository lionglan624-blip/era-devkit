# I/O Format Guide

Reference for unified subagent input/output contracts following the "Minimal Context" principle.

---

## Principle

**Minimal Context** (from CLAUDE.md Design Principles):
- Subagents return minimal status on SUCCESS
- Detailed output only on ERROR
- Caller reads feature.md for context, not from subagent output
- Reduces token usage and prevents redundant data transfer

**Progressive Disclosure** (from CLAUDE.md Design Principles):
- Agents read information from SSOT sources (feature.md, Skills) on-demand
- Callers provide minimal pointers (ID, AC#, Task#), not full context
- Information flows through files, not message passing

---

## Agent I/O Contracts

### initializer

**Input**:
- Feature ID (number or empty for auto-select)

**Output**:
- `READY:{ID}:{Type}` - Success, ready for implementation
- `NO_FEATURE` - No pending features found
- `BLOCKED:ALREADY_DONE:{ID}` - Feature already complete
- `ERROR` - Validation issue

**Caller Reads**: `feature-{ID}.md` for Background, Tasks, ACs after receiving READY.

---

### implementer

**Input**:
- Feature ID
- Task number
- Type (erb/engine/infra)

**Output**:
- `SUCCESS` - Task complete
- `BUILD_FAIL` - Error (file, line, message), Suggestion
- `BLOCKED:CODE_CONFLICT:Task{N}` - Existing code contradicts requirements
- `BLOCKED:DOC_MISMATCH:Task{N}` - Documentation vs implementation mismatch
- `BLOCKED:COMPLEXITY_EXCEEDED:Task{N}` - Requires Opus-level agent
- `ERROR` - Error Type, Cause, Scope

**Caller Reads**: `feature-{ID}.md` Execution Log for file changes, build status after SUCCESS.

---

### kojo-writer

**Input**:
- Feature ID
- Character number (K1-K10)
- eraTW cache path

**Output**:
- `OK:K{N}` - Dialogue written successfully
- `BLOCKED:EXISTING_STUB:K{N}` - Function already exists
- `BLOCKED:FILE_NOT_FOUND:K{N}` - Target ERB file missing
- `BLOCKED:WRONG_CHARACTER:K{N}` - File contains different character code
- `ERROR:K{N}:{reason}` - Implementation error

**Status File**: `pm/status/{ID}_K{N}.txt` (created on completion, content optional)

**Caller Polls**: Glob count only, does NOT Read status file content.

---

### ac-tester

**Input**:
- Feature ID
- AC# to test

**Output** (single AC):
- `OK:AC{N}` - Test passed
- `ERR:{count}|{ACs}\nAC{N}:{matcher}:{expected}:{actual}` - Test failed
- `CRASH:AC{N}:{error}` - Test crashed
- `BLOCKED:AC{N}:{reason}` - Cannot execute test

**Output** (batch):
- `OK:{passed}/{total}` - All tests passed
- `ERR:{failed}|{total}` - Some tests failed

**Caller Reads**: `feature-{ID}.md` AC table for Type, Matcher, Expected (agent reads this internally too).

---

### debugger

**Input Contract** (minimum required):
- Error type (BUILD_FAIL, TEST_FAIL, CRASH)
- File path where error occurred
- Line number (if applicable)
- Error message (compiler/runtime output)

**Output**:
- `FIXED` - Minimal fix applied, RETRY_TEST
- `UNFIXABLE` - Next attempt suggestion
- `QUICK_WIN` - Out of scope, recorded
- `BLOCKED` - Needs design change

**Agent Reads**: `feature-{ID}.md` internally for error context.

---

### regression-tester

**Input**:
- Feature ID
- Scope (full or feature-scoped)

**Output**:
- `OK:{passed}/{total}` - All regression tests passed
- `ERR:{failed}|{total}\n{details}` - Regression failures detected
- `BLOCKED:{reason}` - Cannot run tests

---

### finalizer

**Input**:
- Feature ID

**Output**:
- `SUCCESS` - Feature finalized, ready for commit
- `ERROR:{reason}` - Cannot finalize

**Caller Reads**: `feature-{ID}.md` Status, Execution Log after SUCCESS.

---

## Good Patterns

### Minimal Success Response

**Good**:
```
SUCCESS
```

**Bad** (redundant data transfer):
```
SUCCESS
Files: Game/ERB/kojo/美鈴.erb (modified)
Build: PASS
Changes: Added KOJO_MESSAGE_COM_K1_62_1 function
Docs: Updated
Next: Phase 6
```

Caller already knows Feature ID and can read feature.md for details.

---

### Error-Only Details

**Good** (ERROR with context):
```
BUILD_FAIL
File: Game/ERB/kojo/美鈴.erb
Line: 42
Error: PRINTW requires argument
Suggestion: Check PRINTFORMW vs PRINTW usage
```

**Good** (SUCCESS without redundancy):
```
SUCCESS
```

---

### Pointer-Based Input

**Good** (caller provides pointer):
```
Dispatch ac-tester: "AC5, Feature 228"
```
Agent reads feature-228.md internally for AC5 definition.

**Bad** (caller duplicates data):
```
Dispatch ac-tester: "AC5, Type: output, Matcher: contains, Expected: 'READY:228:infra', Feature 228"
```
Duplicates information already in feature.md.

---

### File-Based Status (kojo-writer)

**Good** (status via file existence):
```
Glob("pm/status/{ID}_K*.txt")  # count check only
```

**Bad** (reading status content):
```
Read("pm/status/{ID}_K1.txt")  # returns "OK:K1"
Read("pm/status/{ID}_K2.txt")  # returns "OK:K2"
...
```
File existence is sufficient for completion detection. Content is optional.

---

## Anti-Patterns

### Verbose Success Output

**Problem**: Returning detailed information on SUCCESS that caller can read from feature.md

**Fix**: Return status only. Caller reads feature.md for details.

---

### Context Duplication in Input

**Problem**: Caller passes full AC definition when agent can read feature.md

**Fix**: Pass AC# only. Agent reads feature.md internally.

---

### TaskOutput for Long-Running Agents

**Problem**: Using TaskOutput to check kojo-writer completion (100K+ token transcript)

**Fix**: Use Glob to count status files. See do.md Phase 4 polling logic.

---

## SSOT Reference

**Principle Source**: CLAUDE.md Design Principles
- Minimal Context: OK = brief, ERR = detailed
- Progressive Disclosure: Read on-demand, not passed as parameters

**Agent Definitions**: `.claude/agents/{agent}.md`
- Input section: What caller provides
- Output section: What agent returns
- Input Contract section (if applicable): Minimum required fields

**Workflow**: `.claude/commands/do.md`
- Dispatch patterns
- Polling logic (kojo)
- Status file usage

---

## Migration Checklist

When updating agent I/O:

1. Define Input Contract (minimum required fields)
2. Update Output format (status-only on SUCCESS)
3. Document what caller reads from feature.md
4. Update dispatch prompts in do.md
5. Verify SSOT sources (Skills, feature.md)
6. Test with actual workflow

---

## Notes

- This guide is SSOT for I/O format conventions
- Agent .md files inherit from this guide
- Exceptions must be documented with rationale
- See `.claude/reference/agent-registry.md` for dispatch patterns
