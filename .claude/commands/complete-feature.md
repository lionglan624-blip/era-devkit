---
description: Verify and complete a feature
argument-hint: "<feature-id>"
---

**Language**: Thinking in English, respond to user in Japanese.

---

Complete a feature by verifying all requirements and updating status.

**Usage**:
- `/complete-feature <ID>` - Verify and complete a single feature
- `/complete-feature` - Batch verify and complete all WIP features

**Target Feature ID**: $1

## 1. Parse Feature ID

Feature ID = `$1` (zero-pad to 3 digits if needed, e.g., "76" → "076").

If `$1` is empty → **Batch mode**:
1. Read `pm/index-features.md`
2. Find all features with `[WIP]` status in Active Features table
3. For each feature found:
   - Read `pm/features/feature-{ID}.md`
   - Check Tasks table: **any incomplete → skip this feature**
   - If all Tasks complete → candidate for AC verification
4. For candidates with all Tasks complete:
   - Run AC verification (Step 4) for **unchecked ACs only** (`[ ]` status)
   - Skip already verified ACs (`[x]` status)
   - If ALL unchecked ACs pass → mark as completable
5. If no completable features exist, report summary and STOP
6. Process each completable feature through Steps 5-8 (skip Steps 2-4, already done)
7. Finalize all and commit together

## 2. Read Feature Document

Read: `pm/features/feature-{ID}.md`

Extract:
- Status (must be [WIP])
- Type (kojo, erb, engine)
- AC table (all rows)
- Tasks table
- Modified files (from Execution Log or Tasks)

## 3. Verify Task Completion

Check feature-{ID}.md Tasks table:
- Count tasks with `[x]` or `[○]` (complete)
- Count tasks with `[ ]` or `[~]` (incomplete)
- If any incomplete tasks exist, report them and STOP

## 4. Run AC Verification (ac-tester)

For EACH AC in the table, dispatch ac-tester:

```
Task(
  subagent_type: "general-purpose",
  model: "haiku",
  prompt: "Read .claude/agents/ac-tester.md and verify Feature {ID} AC {N}"
)
```

**Run in parallel** where possible (ACs with no dependencies).

Collect results:
- PASS: AC verified
- FAIL: AC not met → Report and STOP
- BLOCKED: Cannot test → Report and STOP

**All ACs must PASS to continue.**

## 5. Run Documentation Review (doc-reviewer)

**If feature modified any `.md` files** (agent docs, reference docs, etc.):

```
Task(
  subagent_type: "general-purpose",
  model: "sonnet",
  prompt: "Read .claude/agents/doc-reviewer.md and review documentation for Feature {ID}"
)
```

Results:
- PASS (A/B/C rating): Continue
- NEEDS_REVISION (D/F rating): Report issues and STOP

**Skip this step** if no documentation files were modified.

## 6. Run Regression Tests

Execute: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test src/engine.Tests/uEmuera.Tests.csproj'`
- If tests fail, report and STOP

## 7. Cleanup Test Logs

Delete accumulated test log files:
```bash
rm -f uEmuera/logs/*.trx
```

## 8. Update Status

If all checks pass:

1. Update feature-{ID}.md:
   - Change `## Status: [WIP]` to `## Status: [DONE]`
   - When CANCELLED: Add `**Cancellation Reason**: {reason}` below Status

2. Update index-features.md:
   - Remove feature from Active Features table
   - Add to Recent Activity table (✅=DONE, ❌=CANCELLED)
   - **6-item rule**: If Recent Activity has 6+ items, move oldest to index-features-history.md
     - DONE → "Completed Features (Full List)" section
     - CANCELLED → "Cancelled Features" section

## 9. Report

### Single Feature Mode

```
=== Feature {ID} Completion Verification ===

Tasks: {X}/{X} complete
AC Verification: {Y}/{Y} PASS
Documentation Review: {PASS|SKIPPED} ({rating if applicable})
Regression Tests: PASSED
Test logs: Cleaned up

Status updated to [DONE]
Feature added to Recent Activity

Commit these changes? (y/n)
```

### Batch Mode Report

```
=== WIP Features Check ===

## Phase 1: Task Completion Check

| ID | Status | Tasks | Result |
|:---|:------:|:-----:|:------:|
| 155 | [WIP] | 5/5 ✓ | → AC verification |
| 169 | [WIP] | 2/4 | ❌ Skip |
| 174 | [WIP] | 0/3 | ❌ Skip |

Tasks complete: 1 feature(s) → AC verification
Tasks incomplete: 2 feature(s) → skipped

---

## Phase 2: AC Verification (for Task-complete features)

Running AC tests for Feature 155...

| AC# | Description | Result |
|:---:|-------------|:------:|
| 1 | Clit cap attachment check | ✓ (already verified) |
| 2 | Sensitivity increase check | ✅ PASS (tested) |
| 3 | TALENT branch check | ✅ PASS (tested) |

Feature 155: ALL ACs PASSED → completable

---

## Phase 3: Finalize & Commit

Completable: 1 feature(s)
  - 155: COM_42 Clit Cap Dialogue

Skipped (Tasks incomplete):
  - 169: Tasks 2/4
  - 174: Tasks 0/3

Processing Feature 155...
  ✓ Documentation review: SKIPPED (no .md changes)
  ✓ Regression tests: PASSED
  ✓ Status updated to [DONE]
  ✓ index-features.md updated

---

=== Batch Complete Summary ===

Completed: 1 feature(s)
  - 155: COM_42 Clit Cap Dialogue [DONE]

Skipped: 2 feature(s)
  - 169: Tasks incomplete (2/4)
  - 174: Tasks incomplete (0/3)

Committing changes...
```

## Error Cases

### Batch Mode - No Completable Features (All Tasks Incomplete)

```
=== WIP Features Check ===

## Phase 1: Task Completion Check

| ID | Status | Tasks | Result |
|:---|:------:|:-----:|:------:|
| 155 | [WIP] | 3/5 | ❌ Skip |
| 169 | [WIP] | 2/4 | ❌ Skip |

Tasks complete: 0 feature(s)
Tasks incomplete: 2 feature(s)

---

Skipped Features (Tasks incomplete):
  - 155: Tasks 3/5
    - [ ] Task 4: Add dialogue variations
    - [ ] Task 5: Run AC tests
  - 169: Tasks 2/4
    - [ ] Task 3: Fix hook execution
    - [ ] Task 4: Add tests

No completable features found.
Please complete tasks before running again.
```

### Batch Mode - AC Verification Failed

```
=== WIP Features Check ===

## Phase 1: Task Completion Check

| ID | Status | Tasks | Result |
|:---|:------:|:-----:|:------:|
| 155 | [WIP] | 5/5 ✓ | → AC検証へ |

Tasks complete: 1 feature(s) → AC verification

---

## Phase 2: AC Verification

Running AC tests for Feature 155...

| AC# | Description | Result |
|:---:|-------------|:------:|
| 1 | Clit cap attachment check | ✅ PASS |
| 2 | Sensitivity increase check | ❌ FAIL |
| 3 | TALENT branch check | ✅ PASS |

Feature 155: AC FAILED
  - AC2: Sensitivity increase check
    Expected: contains "感度が上がった"
    Actual: (output did not contain expected string)

---

No completable features found.
Please fix implementation to pass AC verification.
```

### Batch Mode - No Active Features

```
=== WIP Features Status ===

Active WIP features: 0

No active features in index-features.md.
```

### Task Incomplete

```
=== Feature {ID} Incomplete ===

Tasks: {X}/{Total} complete
  - Incomplete: Task {N}: {description}

Please complete tasks before running again.
```

### AC Verification Failed

```
=== Feature {ID} AC Verification Failed ===

AC Verification: {X}/{Total} PASS

Failed ACs:
  - AC{N}: {description}
    Matcher: {matcher}({expected})
    Actual: {what was found}
    Gap: {specific mismatch}

Please fix implementation before running again.
```

### Documentation Review Failed

```
=== Feature {ID} Documentation Quality Insufficient ===

Documentation Review: NEEDS_REVISION ({rating})

Issues:
  - [{severity}] {file}: {issue}

Required Fixes:
  1. {action}

Please fix documentation before running again.
```

### Regression Test Failed

```
=== Feature {ID} Regression Test Failed ===

Regression Tests: FAILED

{test output}

Please fix tests before running again.
```
