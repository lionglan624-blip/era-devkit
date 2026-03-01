# Feature 743: Session Extractor Fundamental Redesign

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**Complete recovery = Functional code that passes all tests, not just operation replay.**

Session extractor must produce code that WORKS, not just code that represents "final state operations applied".

---

### Problem (Current Issue)

F742's session extractor produces **broken code** despite "successful" recovery:

| Symptom | Root Cause |
|---------|------------|
| `CHAIN_WAITER_TIMEOUT_MS is not defined` | Definition operation skipped, usage operation applied |
| `validateCommand is not defined` | Same pattern - export exists, definition missing |
| 27 test failures after merge | Incomplete code due to chain gaps |

**The fundamental flaw**: Skip rate measures operation count, not code correctness.

```
Skip rate 17% sounds good
But if the 17% includes critical definitions
The resulting code is 100% broken
```

### Why Chain Gaps Break Code

```
Session history:
1. const FOO = 123;        <- chain gap, SKIPPED
2. function bar() {...}    <- chain gap, SKIPPED
3. export { FOO, bar };    <- APPLIED
4. bar();                  <- APPLIED
5. console.log(FOO);       <- APPLIED

Result: Code references FOO and bar() but they don't exist
```

---

### Historical Context

| Feature | Approach | Result |
|---------|----------|--------|
| F733 | Basic JSONL extraction | 85% skip rate |
| F738 | HEAD + timestamp filter | 8.26% skip |
| F740 | Session-start state | 21.74% (regression) |
| F742 | F740 + deduplication | 9.32% skip |
| F742 merge | Full file replacement | **Broken code** |

**Key Learning**: Skip rate is meaningless if critical operations are skipped.

---

## Goal (What to Achieve)

Design and implement a recovery approach that produces **functional code**:

1. Recovery output must pass existing tests
2. Recovery output must build without errors
3. Chain gaps must be detected and reported BEFORE merge
4. User must be able to review and approve each file

---

## Technical Design (To Be Designed)

### Approach Options

| Option | Description | Pros | Cons |
|:------:|-------------|------|------|
| A | **Validation-first**: Run tests/build on recovery before merge | Catches broken code | Doesn't fix it |
| B | **Selective merge**: Only merge files with 0% skip rate | Safe | Loses partial recovery |
| C | **Diff-based merge**: Show diff, let user cherry-pick | User control | Manual effort |
| D | **Hybrid HEAD+Recovery**: Start from HEAD, apply only verified changes | Preserves working code | Complex |
| E | **AST-aware recovery**: Parse code, detect undefined references | Smart | Complex to implement |

### Recommended: Option D (Hybrid)

```
1. Start from HEAD (known working state)
2. For each recovered file:
   a. If skip rate = 0%: Safe to replace
   b. If skip rate > 0%:
      - Generate diff between HEAD and recovery
      - Parse for undefined references
      - Show user which hunks are safe vs risky
      - User selects which to apply
3. Run tests after each file merge
4. Rollback on failure
```

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Recovery validation before merge | code | exists | validation script | [ ] |
| 2 | Skip rate 0% files identified | output | contains | list of safe files | [ ] |
| 3 | Undefined reference detection | code | exists | reference checker | [ ] |
| 4 | Dashboard tests pass after merge | test | succeeds | npm test | [ ] |
| 5 | No ReferenceError in merged code | output | not_contains | "is not defined" | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | - | Design: Finalize approach (A-E or hybrid) | [ ] |
| 2 | 1 | Implement recovery validation script | [ ] |
| 3 | 2 | Generate safe-file list (skip rate = 0%) | [ ] |
| 4 | 3 | Implement undefined reference detector | [ ] |
| 5 | 4,5 | Test hybrid merge on Dashboard files | [ ] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F742 | [DRAFT] | Session extractor improvements (baseline) |
| Related | F735 | [DONE] | Dashboard recovery (test-driven, partial) |

---

## Links

- [F742](feature-742.md) - Session Extractor Complete Recovery (current approach, flawed)
- [F738](feature-738.md) - Session Extractor v1 (timestamp filter approach)
- [F735](feature-735.md) - Dashboard Frontend Recovery (test-driven)

---

## Review Notes

### F742 Merge Failure Analysis (2026-02-03)

**What happened:**
1. Deep-explorer said 9.32% skip rate was acceptable
2. Full merge of ~790 files executed
3. Dashboard tests failed with ReferenceErrors
4. Constants and functions were used but undefined
5. Reverted to HEAD

**Root cause:**
- Skip rate measures operation count, not semantic completeness
- Chain gaps can skip definitions while keeping usages
- No validation before merge

**Lesson:**
> **Never merge recovery output without validation.**
> Skip rate tells you nothing about code functionality.

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-03 | DRAFT | Created after F742 merge failure |

