# Feature 500: Test Strategy Design: E2E and /do Integration

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

## Type: engine

## Created: 2026-01-14

---

## Summary

Complete test strategy design with E2E test methodology, /do command integration, log output, pre-commit hooks, and AC verification flow.

**Design Scope**:
- /do command test execution (Phase 3 TDD, Phase 6 Verification)
- Log output patterns (TRX, JSON)
- Pre-commit hook test execution
- AC verification flow (AC Type別検証方法)

Note: E2E test methodology (seed固定, 不変条件, Golden Master) covered by F499 section 1 (E2E Test Guidelines).

**Output**: test-strategy.md sections 3 (/do Command Integration), 4 (Log Output), 5 (Pre-commit Hook), 6 (AC Verification Flow).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 15: Architecture Review** - Validate Phase 1-14 implementations against Phase 4 design principles (SRP, DIP, Strongly Typed IDs, Result type, DI registration) before large-scale parallel implementation phases (Phase 19-21). Ensures architectural consistency and identifies technical debt requiring resolution.

### Problem (Current Issue)

Test execution and verification workflows not formalized:
- /do command test execution not documented (Phase 3 TDD, Phase 6 AC verification)
- Log output format and location inconsistent
- Pre-commit hook test requirements unclear
- AC verification flow (verify-logs.py integration) not defined
- E2E test methodology needs specification

### Goal (What to Achieve)

1. **Document /do test execution** for Phase 3 (TDD) and Phase 6 (Verification)
2. **Specify log output format** (TRX for C#, JSON for AC)
3. **Define pre-commit hook** test execution requirements
4. **Document AC verification flow** (AC Type別検証方法)
5. **Complete test-strategy.md** with sections 3-6 per architecture.md Phase 15

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | /do Command Integration section | file | Grep | contains | "## 3. /do Command Integration" | [x] |
| 2 | Log Output section | file | Grep | contains | "## 4. Log Output" | [x] |
| 3 | Pre-commit Hook section | file | Grep | contains | "## 5. Pre-commit Hook" | [x] |
| 4 | AC Verification Flow section | file | Grep | contains | "## 6. AC Verification Flow" | [x] |
| 5 | Phase 3 TDD workflow | file | Grep | contains | "Phase 3: TDD" | [x] |
| 6 | Phase 6 AC verification | file | Grep | contains | "Phase 6: Verification" | [x] |
| 7 | TRX naming convention | file | Grep | contains | "feature-{ID}-red.trx" | [x] |
| 8 | verify-logs.py CLI usage | file | Grep | contains | "verify-logs.py --scope feature:{ID}" | [x] |
| 9 | AC Type matcher (equals) | file | Grep | contains | "equals" | [x] |
| 10 | AC Type matcher (not_contains) | file | Grep | contains | "not_contains" | [x] |
| 11 | 負債ゼロ (TODO) | file | Grep | not_contains | "TODO" | [x] |
| 12 | 負債ゼロ (FIXME) | file | Grep | not_contains | "FIXME" | [x] |
| 13 | 負債ゼロ (HACK) | file | Grep | not_contains | "HACK" | [x] |
| 14 | testing SKILL references test-strategy.md | file | Grep | contains | "test-strategy.md" | [x] |

### AC Details

**Target file**: `Game/agents/designs/test-strategy.md` for AC#1-13. AC#14 targets `.claude/skills/testing/SKILL.md`.

**AC#1-4**: Required sections exist
- Test: Grep patterns for sections 3-6
- Expected: All four sections present

**AC#5**: Phase 3 TDD workflow documented
- Test: Grep pattern="Phase 3: TDD" in section 3
- Expected: Describes C# test creation and RED confirmation

**AC#6**: Phase 6 AC verification documented
- Test: Grep pattern="Phase 6: Verification" in section 3
- Expected: Describes AC verification execution and log output

**AC#7**: TRX naming convention specified (F500-unique)
- Test: Grep pattern="feature-{ID}-red.trx" in section 4
- Expected: Documents TRX file naming convention for RED confirmation

**AC#8**: verify-logs.py CLI usage documented (F500-unique)
- Test: Grep pattern="verify-logs.py --scope feature:{ID}" in section 4 or 6
- Expected: Describes verify-logs.py CLI usage with colon-separated scope syntax (execute from repo root)

**AC#9-10**: AC Type matchers documented
- Test: Grep for representative matchers (equals, not_contains) in section 6
- Expected: Matcher definitions present

**AC#11-13**: Zero technical debt in test strategy documentation
- Test: Grep for TODO, FIXME, HACK separately
- Expected: 0 matches each

**AC#14**: Testing SKILL references test-strategy.md (SSOT update verification)
- Test: Grep pattern="test-strategy.md" in `.claude/skills/testing/SKILL.md`
- Expected: Reference to test-strategy.md exists in testing SKILL

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Create test-strategy.md sections 3-6 | [x] |
| 2 | 5,6,7,8 | Document /do command integration and log output | [x] |
| 3 | 9,10 | Document AC verification flow with matchers (負債解消) | [x] |
| 4 | 11,12,13 | Verify zero technical debt in test strategy documentation (負債解消) | [x] |
| 5 | 14 | Update testing SKILL with test-strategy.md reference | [x] |

<!-- AC:Task 1:1 Rule: 14 ACs = 5 Tasks following F384 precedent for design documentation features:
- Task#1 (AC#1-4): structural verification (section headers exist)
- Task#2 (AC#5-8): semantic verification (specific content within sections)
- Task#3 (AC#9-10): matcher definition verification
- Task#4 (AC#11-13): negative constraint verification (no TODO/FIXME/HACK)
- Task#5 (AC#14): SSOT update verification (testing SKILL references test-strategy.md)
Grouping by verification category is appropriate for documentation features. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### test-strategy.md Required Sections

Per architecture.md Phase 15 requirements, sections 3-6:

```markdown
## 3. /do Command Integration

### Phase 3: TDD (Test-Driven Development)

**/do command responsibilities**:
1. Create C# test methods based on ACs
2. Execute tests to confirm RED (tests fail initially)
3. Implement feature code
4. Re-execute tests to confirm GREEN (tests pass)

**Workflow**:
```bash
# Phase 3: Create tests (implementer agent)
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=feature-{ID}-red.trx"
# Expected: Tests FAIL (RED confirmation)

# Phase 3: Implement feature code
# ... implementation ...

# Phase 3: Re-execute tests
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=feature-{ID}-green.trx"
# Expected: Tests PASS (GREEN confirmation)
```

### Phase 6: Verification (AC検証)

**/do command responsibilities**:
1. Execute all AC verification tests
2. Output logs to `Game/logs/prod/ac/engine/feature-{ID}/`
3. Run verify-logs.py for result aggregation
4. Update feature-{ID}.md AC Status column

**Workflow**:
```bash
# Phase 6: Execute AC tests (ac-tester agent)
dotnet test --filter "FullyQualifiedName~Feature{ID}" --logger "trx;LogFileName=ac-{AC#}.trx"

# Phase 6: Aggregate results
python tools/verify-logs.py --scope feature:{ID}

# Phase 6: Update feature file
# Edit feature-{ID}.md: [ ] → [x] or [-] based on results
```

### Scope Specification

**Feature scope**:
```bash
--filter "FullyQualifiedName~Feature{ID}"
```

**AC scope**:
```bash
--filter "FullyQualifiedName~Feature{ID}AC{N}"
```

## 4. Log Output

### TRX Output (C# Tests)

**Path**: `Game/logs/prod/ac/engine/feature-{ID}/`

**Naming Convention**:
- RED confirmation: `feature-{ID}-red.trx`
- GREEN confirmation: `feature-{ID}-green.trx`
- AC verification: `ac-{AC#}.trx`

**Format**: MSTest TRX (XML)

### JSON Output (AC Verification)

**Path**: `Game/logs/prod/ac/feature-{ID}.json`

**Format**:
```json
{
  "feature_id": "F{ID}",
  "total_acs": N,
  "passed": N,
  "failed": N,
  "acs": [
    {
      "ac_number": 1,
      "description": "...",
      "status": "PASS|FAIL",
      "matcher": "contains",
      "expected": "...",
      "actual": "..."
    }
  ]
}
```

### verify-logs.py Integration

**Usage**:
```bash
# Scope: Single feature
python tools/verify-logs.py --scope feature:{ID}

# Scope: All tests
python tools/verify-logs.py --scope all

# Output: Console report with PASS/FAIL counts
```

**Result Aggregation** (execute from repository root):
- Reads JSON result files matching `*-result.json` pattern in ac/ subdirectories
- Always checks regression logs (regression is default scope)
- Outputs console summary (PASS/FAIL counts per category)

## 5. Pre-commit Hook

**Note**: This section documents the DESIGNED pre-commit hook behavior for C# development. Current hook (.githooks/pre-commit) runs verify_com_map.py and conditionally verify-logs.py. The C# test execution described here is for FUTURE phases when C# migration is complete.

### Execution Target (Future State)

**Command**: `dotnet test Era.Core.Tests`

**Scope**: All Era.Core tests (unit + integration)

### Execution Conditions

**Always execute**:
- Pre-commit hook runs full test suite
- All tests must PASS before commit allowed

**Feature-specific scope** (during feature implementation):
- Add feature scope filter: `--filter "FullyQualifiedName~Feature{ID}"`
- Faster feedback during development

### Linter Integration

**C# Static Analysis**: Roslyn Analyzer
- Configured in Era.Core.csproj
- Runs automatically during `dotnet build`
- Warnings treated as errors (WarningsAsErrors=true)

**ErbLinter**: Deprecated (current state)
- ErbLinter tool deprecated - no ERB static analysis currently
- ERB code files remain in Game/ERB/ for execution but new development is C#
- C# code is linted by Roslyn Analyzer (separate concern from ERB)

### Hook Configuration

```bash
# .githooks/pre-commit
dotnet build Era.Core
dotnet test Era.Core.Tests
# Exit code: 0 (PASS) → Allow commit, Non-zero (FAIL) → Block commit
```

## 6. AC Verification Flow

### AC Type別検証方法

| AC Type | Method | Matcher | Verification Tool |
|---------|--------|---------|-------------------|
| **test** | dotnet test | succeeds/fails | dotnet test + TRX |
| **code** | Grep | contains/not_contains/matches | Grep tool |
| **file** | Glob/Grep | exists/contains | Glob/Grep tools |
| **build** | dotnet build | succeeds | dotnet build |
| **output** | headless mode | contains/matches | Bash + Grep |
| **variable** | headless mode | equals | Bash + Grep |

### Matcher Definitions

| Matcher | Behavior | Example |
|---------|----------|---------|
| **equals** | Exact match | Expected: "100", Actual: "100" → PASS |
| **contains** | Substring match | Expected: "Error", Actual: "RuntimeError occurred" → PASS |
| **not_contains** | Absence check | Expected: "TODO", Actual: "// Clean code" → PASS |
| **matches** | Regex match | Expected: "\\d{3}", Actual: "123" → PASS |
| **succeeds** | Exit code 0 | Command exits 0 → PASS |
| **fails** | Exit code non-zero | Command exits 1 → PASS |
| **exists** | File exists | Glob finds file → PASS |
| **not_exists** | File absent | Glob finds no file → PASS |
| **count_equals** | Count match | Expected: 5, Actual: 5 instances → PASS |
| **gt/gte/lt/lte** | Numeric comparison | Expected: ">= 10", Actual: 15 → PASS |

### Log Format

**PASS Example**:
```
[PASS] AC#1: CharacterId interface exists
  Type: file
  Method: Glob
  Matcher: exists
  Expected: Era.Core/Types/CharacterId.cs
  Actual: File found
```

**FAIL Example**:
```
[FAIL] AC#2: Zero technical debt
  Type: file
  Method: Grep
  Matcher: not_contains
  Expected: TODO|FIXME|HACK
  Actual: Found 3 matches in Era.Core/Functions/Foo.cs:42, 58, 91
```

### Result Judgment

**Binary**: PASS/FAIL only (no "Confidence" levels)

**Criteria**:
- Matcher satisfied → PASS
- Matcher not satisfied → FAIL
- Tool error (file not found, command failed) → FAIL with ERROR annotation

## 負債の意図的受け入れ:
(Document any test execution debt accepted with justification)
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Parent | F486 | Phase 15 Planning (parent feature) |
| Predecessor | F499 | Test Strategy Design: IRandomProvider must complete first |
| Successor | F501 | Architecture Refactoring (implements IRandomProvider migration per F499 design) |
| Related | architecture.md | Test strategy sections must align with Phase 15 requirements |

---

## Links

- [feature-486.md](feature-486.md) - Phase 15 Planning (parent feature)
- [feature-498.md](feature-498.md) - Testability Assessment (State Management Testability reference)
- [feature-499.md](feature-499.md) - Test Strategy Design: IRandomProvider (predecessor)
- [feature-501.md](feature-501.md) - Architecture Refactoring (successor, implements IRandomProvider migration per F499 design)
- [testability-assessment-15.md](designs/testability-assessment-15.md) - F498 output: State Management Testability, Hard-to-Test Patterns (reference)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 15 test strategy requirements
- [testing SKILL](../../.claude/skills/testing/SKILL.md) - AC Type and Matcher reference

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-15 FL iter1**: [resolved] Phase2-Validate - AC#5/6 patterns: Simplified to "Phase 3: TDD" and "Phase 6: Verification". Implementation Contract uses English consistently.
- **2026-01-15 FL iter1**: [applied] Phase2-Validate - Task#5 AC#=-: User chose to add AC#14 for testing SKILL update verification.
- **2026-01-15 FL iter2**: AC#7, AC#8 updated to F500-unique content (TRX naming convention, verify-logs.py CLI usage).
- **2026-01-15 FL iter3**: verify-logs.py CLI syntax corrected to colon format (feature:{ID}), removed non-existent --type parameter.
- **2026-01-15 FL iter4**: verify-logs.py Result Aggregation corrected - it reads TRX/JSON files and outputs console summary only (does not generate JSON or update feature files).
- **2026-01-15 FL iter5 maintainability**: Added F486 as Parent dependency; clarified ErbLinter deprecation state; added repo root execution context; corrected verify-logs.py always checks regression.
- **2026-01-15 FL iter6 maintainability**: Pre-commit Hook section clarified as FUTURE state; F501 dependency updated from "may" to "implements"; E2E scope moved to note (covered by F499); ErbLinter section clarified ERB/C# separation.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | spec-writer | Created from F486 Phase 15 Planning | PROPOSED |
| 2026-01-15 11:49 | START | implementer | Task 1-5 | - |
| 2026-01-15 11:49 | END | implementer | Task 1-5 | SUCCESS |
