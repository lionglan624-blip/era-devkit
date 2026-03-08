---
description: "Review loop until plan converges (zero issues)"
---

# /converge Command

Convergent review loop for plans, proposals, or fix sets. Dispatches Opus reviewer + validator in a loop until the reviewer issues a **Go** verdict.

## When to Use

Call `/converge` when the orchestrator has produced a plan, execution proposal, or set of changes that needs quality validation before applying.

## Language

**User-facing output MUST be in Japanese.** Internal reasoning in English is OK.

## Input

The plan/proposal currently in session context. No arguments needed — the command operates on whatever the orchestrator has produced so far.

## Loop

```
LOOP_COUNT = 0

REPEAT:
  LOOP_COUNT += 1

  # ── Step 1: Review ──
  reviewer_result = Agent(model: "opus", prompt: """
  You are a critical reviewer. Review the following plan/proposal for correctness,
  completeness, consistency, and feasibility.

  ## Plan to Review
  {current_plan}

  ## Unresolved REVISE Items (if any)
  {revise_items — issues marked REVISE in prior loops, requiring re-investigation}

  ## Instructions
  1. Review the plan AS A WHOLE with fresh eyes.
     Look for structural, logical, and completeness issues.
  2. Additionally, check the REVISE items above — these are known open issues
     that need re-evaluation in the context of the current plan.
  3. Output a list of issues found. For each issue:
     - description: what is wrong
     - location: where in the plan
     - severity: HIGH / MEDIUM / LOW
     - suggestion: how to fix
  4. Output a final verdict: **Go** (no issues) or **NoGo** (issues found).
     - Go: The plan is sound. Zero issues remain.
     - NoGo: Issues exist that must be addressed.

  ## Output Format
  ### Issues
  1. [HIGH] ...
  2. [MEDIUM] ...
  3. [LOW] ...
  (or "None")

  ### Verdict: Go / NoGo
  """)

  IF reviewer_result.verdict == "Go":
    BREAK — plan has converged

  # ── Step 2: Validate ──
  validator_result = Agent(model: "opus", prompt: """
  You are an independent validator. The reviewer found the following issues
  with a plan. Investigate ALL issues and determine which are valid.

  ## Current Plan
  {current_plan}

  ## Issues from Reviewer
  {reviewer_result.issues}

  ## Instructions
  For EACH issue:
  1. Read any referenced files, code, or context to verify the claim
  2. Determine: is this issue VALID (real problem) or INVALID (false positive)?
  3. If VALID, propose a concrete fix (specific text changes to the plan)
  4. If INVALID, explain why with evidence

  ## Output Format
  For each issue:
  - issue: (original description)
  - verdict: VALID / INVALID
  - evidence: (what you checked)
  - fix: (concrete change, if VALID)
  """)

  # ── Step 3: Apply ──
  FOR each issue in validator_result:
    IF issue.verdict == "VALID":
      Apply the fix to current_plan

  # ── Step 4: Report Loop Status ──
  Print to user:
    Loop {LOOP_COUNT}: {N} issues found, {M} valid, {K} invalid
    Applied {M} fixes. Re-reviewing...

  GOTO REPEAT
```

## Post-Loop

When the reviewer issues **Go**:

1. Print final summary to user:
   ```
   /converge 完了 (ループ {LOOP_COUNT}回)
   最終verdict: Go — 指摘なし
   ```

2. If fixes were applied during the loop, show a diff summary of all changes made to the plan.

3. Return control to the orchestrator with the converged plan.

## Rules

- **No max iterations.** The loop runs until Go.
- **Reviewer and Validator are DIFFERENT subagents** each loop (fresh context, no bias carryover).
- **Flat review** — the reviewer receives only the current plan (no issue history). Fresh eyes each loop.
- **REVISE items carry forward** — issues the validator marked as needing refinement are explicitly passed to the next reviewer for re-investigation alongside the flat plan review.
- **Severity filtering: NONE.** All issues (HIGH, MEDIUM, and LOW) are addressed.
