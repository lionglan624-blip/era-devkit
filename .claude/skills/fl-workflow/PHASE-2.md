# Phase 2: Review-Validate-Apply

**CRITICAL: This phase contains 3 steps that MUST execute without interruption.**
**DO NOT output to user. DO NOT stop between steps. Complete all 3 steps in one turn.**

## Entry Check (MANDATORY)

```
- Previous Phase: 1 or any phase with applied_fixes > 0
- Condition met: (Phase 1 OK) or (previous phase applied_fixes > 0)
- Expected: Phase 2
- Actual: Phase 2
- Match confirmed → proceed
```

### Iteration Tracking

```
iteration += 1

# Persist state to disk (crash recovery — see SKILL.md "FL State File")
Write("_out/tmp/fl-state-{ID}.json", JSON.stringify({
    "iteration": iteration,
    "current_phase": 2,
    "timestamp": now_iso()
}))

IF iteration >= MAX_ITERATIONS:
    → Enter Forward-Only Mode (see SKILL.md)
```

## FORBIDDEN Suggestions (Zero Debt Upfront - Anti-YAGNI)

**CRITICAL**: This project does NOT follow YAGNI. The following suggestions are **FORBIDDEN**:

| Forbidden Pattern | Why |
|-------------------|-----|
| "Match existing pattern" | Existing may be debt; new design may be correct |
| "Keep it simple/minimal" | Causes future cost increase |
| "YAGNI" / "Not needed now" | Violates Zero Debt Upfront principle |
| "Over-engineering" | Proper design investment is not over-engineering |

**Correct Approach**:
- Research future extension patterns before design
- Reference similar systems / prior implementations
- Choose technically correct implementation from the start

---

## Step 2.1: Update Task

```
TaskUpdate(subject: "Phase 2: Review-Validate-Apply", status: "in_progress")
```

---

# STEP A: REVIEW

## Goal

Discovery phase - identify all issues in the target.

**CRITICAL: DO NOT APPLY ANY FIXES IN STEP A**
- Step A ONLY collects issues from reviewer
- Fixes are validated in Step B and applied in Step C
- **CONTINUE IMMEDIATELY to Step 2.5 - NO OUTPUT TO USER**

## Step 2.2: Trivial Fix Fast-Path (OPTIONAL)

**Purpose**: Skip full subagent dispatch for mechanical/trivial fixes.

**Trivial Fix Patterns** (auto-apply without validation):

| Pattern | Detection | Auto-Fix | Category |
|---------|-----------|----------|----------|
| Status typo | `[PRPOOSED]`, `[REVIWED]` | Correct spelling | FMT-004 |
| Missing bracket | `Status: PROPOSED` | Add `[]` | FMT-004 |
| Trailing whitespace | Line ends with spaces | Trim | FMT-004 |
| Empty line excess | 3+ consecutive empty lines | Reduce to 2 | FMT-004 |

```
trivial_fixes = 0

# Check for trivial patterns
FOR pattern in TRIVIAL_PATTERNS:
    matches = Grep(pattern.regex, target_file)
    IF matches.count > 0:
        FOR match in matches:
            apply_trivial_fix(match, pattern.fix)
            persist_fix({location: match.location, issue: pattern.name}, iteration, "Phase2-Trivial")
            trivial_fixes++

IF trivial_fixes > 0:
    # Log trivial fixes but CONTINUE to full review
    # (trivial fixes don't substitute for full review)
    Log: "Applied {trivial_fixes} trivial fixes"
```

**IMPORTANT**: Trivial fixes are applied BEFORE full review, not INSTEAD OF. Always proceed to Step 2.3.

**Criteria for Trivial Fix Pattern**:
1. 100% deterministic (no judgment needed)
2. Cannot affect semantic meaning
3. Pattern is specific (low false positive risk)

## Step 2.3: Dispatch Reviewers (Escalation Model)

**Purpose**: Review feature spec for issues. Uses sonnet-first escalation to minimize opus calls.

**Escalation Pattern**: sonnet finds issues → fix without opus. sonnet finds 0 issues → 1x opus verification before proceeding.

**Evidence**: /fl 777 (3 iterations) — opus SEMANTIC ran 3 times. With escalation: iter1-2 had issues (sonnet sufficient), only iter3 was clean → opus 1 call instead of 3.

```
IF target_type == "feature":
    # Pre-compute dependency status (eliminates subagent Status grep — F819 lesson: 28+ redundant greps)
    deps_result = Bash("python src/tools/python/feature-status.py deps {target_id}")

    # Pre-compute predecessor context and materialize to file (F844 lesson: pseudocode-only approach
    # led to 119+ repeated reads of predecessor files because subagents ignored inline instructions).
    # The orchestrator reads each DONE predecessor, extracts Key Decisions (Decision+Selected only)
    # and relevant Mandatory Handoffs, then writes to a temp file for subagent consumption.
    # Cap: 200 tokens per predecessor.
    predecessor_context = ""
    FOR dep in deps_result.predecessors WHERE dep.status == "DONE":
        kd = extract_key_decisions(dep.feature_file)  # Decision + Selected columns only (orchestrator reads and extracts)
        mh = extract_relevant_handoffs(dep.feature_file, target_id)  # orchestrator reads and extracts
        predecessor_context += f"\n### {dep.id}: {kd}\nHandoffs: {mh}\n"  # 200 token cap per dep
    Write("_out/tmp/predecessor-context-{target_id}.md", predecessor_context)
    # Downstream phases (3, 7) and RUN Phase 8 use Read() on this file instead of inline context.
    # File cleaned by existing _out/tmp/ 7-day rotation (CLAUDE.md File Placement).

    # Perspective 1: Structural/Format Review (FIRST ITERATION ONLY)
    IF iteration == 1:
        structural_result = Task(
          subagent_type: "feature-reviewer",
          model: "sonnet",  # Format check is mechanical — sonnet sufficient
          prompt: `Feature {target_id}.

PERSPECTIVE: STRUCTURAL
FOCUS: Format compliance, section completeness, table column count, markdown syntax, template adherence.
IGNORE: Semantic validity, design quality, philosophy coverage.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
        )
    ELSE:
        # Loop-back: STRUCTURAL already verified in iter1, skip
        structural_result = {"status": "OK", "issues": []}

    # Perspective 2: Semantic/Logic Review — SONNET FIRST (Escalation Tier 1)
    semantic_result = Task(
      subagent_type: "feature-reviewer",
      model: "sonnet",  # Tier 1: sonnet screens for issues
      prompt: `Feature {target_id}.

PERSPECTIVE: SEMANTIC
FOCUS: Philosophy-to-AC derivation, AC coverage completeness, Task-to-AC alignment, SSOT consistency, design coherence.
IGNORE: Format, spelling, template structure.

DEPENDENCY STATUS (pre-computed — do NOT grep individual feature files for status):
{deps_result}

PREDECESSOR CONTEXT (pre-computed — do NOT Read predecessor feature files for Key Decisions or Handoffs):
{predecessor_context}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
    )

    # Merge results (union, deduplicate by location)
    IF structural_result.issues:
        result.issues = merge_and_dedupe(structural_result.issues, semantic_result.issues)
    ELSE:
        result.issues = semantic_result.issues

    # ESCALATION: If sonnet found 0 issues → opus verification (Tier 2)
    IF result.issues.count == 0:
        opus_result = Task(
          subagent_type: "feature-reviewer",
          # model: opus (default for feature-reviewer frontmatter)
          prompt: `Feature {target_id}.

PERSPECTIVE: SEMANTIC
FOCUS: Philosophy-to-AC derivation, AC coverage completeness, Task-to-AC alignment, SSOT consistency, design coherence.
IGNORE: Format, spelling, template structure.
CONTEXT: Sonnet review found 0 issues. Verify this is genuinely clean — check for subtle gaps sonnet may have missed.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
        )
        result.issues = opus_result.issues if opus_result.issues else []

    result.status = "NEEDS_REVISION" if result.issues.count > 0 else "OK"

ELSE:
    # Non-feature targets: single reviewer (unchanged)
    reviewer = "doc-reviewer"
    review_prompt = `Review {target_type} document.

Target: {target_path}
Type: {target_type}

Read the target file fully.

Read baseline SSOT for {target_type} from CLAUDE.md.

Check for:
- Consistency with CLAUDE.md definitions
- YAML frontmatter correctness (name, description, model, tools)
- Internal consistency (no contradictions)
- Cross-references validity (referenced files exist)
- Format/structure compliance

Output JSON:
{
  "status": "OK | NEEDS_REVISION",
  "issues": [{"severity": "critical|major|minor", "location": "...", "issue": "...", "fix": "..."}]
}`

    result = Task(
      subagent_type: reviewer,
      prompt: review_prompt
    )
```

### Issue Merge Protocol

```
def merge_and_dedupe(structural_issues, semantic_issues):
    all_issues = []
    seen_keys = set()

    # Process both lists, dedupe by (location, issue description)
    for issue in structural_issues + semantic_issues:
        key = (issue.location, issue.issue)
        if key not in seen_keys:
            all_issues.append(issue)
            seen_keys.add(key)
        else:
            # Same location AND same issue: keep higher severity
            existing = find_by_key(all_issues, key)
            if severity_rank(issue) > severity_rank(existing):
                replace(all_issues, existing, issue)

    return all_issues  # No cap — report all unique issues
```

## Step 2.4: Check Issue Count

```
IF result.issues.count == 0:
    # No issues found - skip validation/apply, go directly to Phase 3
    TaskUpdate(subject: "Phase 2: Review-Validate-Apply", status: "completed")
    GOTO Declare Next Phase (with issues.count == 0)
ELSE:
    # Issues found - CONTINUE IMMEDIATELY to Step 2.5
```

**CONTINUE IMMEDIATELY - NO OUTPUT TO USER**

---

# STEP B: VALIDATE

## Goal

Validate each reviewer issue against SSOT. Classify issues as valid/invalid/uncertain.

**CRITICAL**:
- Orchestrator MUST NOT self-judge issue validity. Always dispatch Validation subagent.
- **DO NOT APPLY ANY FIXES IN STEP B** - fixes are applied in Step 2.6
- **CONTINUE IMMEDIATELY to Step 2.6 - NO OUTPUT TO USER**

## Step 2.5: Dispatch Validator

**Mandatory - DO NOT skip**

**CRITICAL**: Validator MUST be a DIFFERENT agent from Step 2.3 reviewers to avoid confirmation bias.
- Step 2.3 uses: feature-reviewer (opus for SEMANTIC, sonnet for STRUCTURAL)
- Step 2.5 uses: feature-reviewer (sonnet) ROLE: VALIDATOR - SSOT comparison is mechanical

**Evidence for sonnet**: 4 FL sessions analyzed — validator correctly classified valid/invalid/uncertain in all cases. SSOT comparison does not require opus-level reasoning.

```
IF target_type == "feature":
    validate_ssot = `- index-features.md (Status Legend, structure rules)
- feature-template.md (Feature format, AC rules)`
    validator_agent = "feature-reviewer"  # ROLE: VALIDATOR (no PERSPECTIVE — validates against SSOT)
ELSE:
    validate_ssot = `- CLAUDE.md ({target_type} section)`
    validator_agent = "doc-reviewer"  # Same as Step 2.3 for non-features (acceptable)

validation = Task(
  subagent_type: validator_agent,
  model: "sonnet",  # Validation is SSOT comparison — sonnet sufficient
  prompt: `Validate reviewer output.

Target: {target_path}
Type: {target_type}
Reviewer JSON:
{result.json}

Read target file fully.

Before validating, read baseline SSOT:
{validate_ssot}

For each issue/fix, read relevant sources based on issue type:
- Skills (.claude/skills/)
- Subagents (.claude/agents/)
- Commands (.claude/commands/)
- Related features (pm/features/feature-*.md)
- Test files (test/)
- Source code (ERB, C#)

Verify each fix is correct against SSOT.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.
{
  "valid": [0, 2],
  "invalid": [{"index": 1, "reason": "..."}],
  "uncertain": [{"index": 3, "reason": "..."}]
}`
)
```

**CONTINUE IMMEDIATELY - NO OUTPUT TO USER**

---

# STEP C: APPLY FIXES

## Goal

Apply validated fixes from Step 2.5. Only fixes classified as "valid" are applied.

**CRITICAL**: This is the ONLY step where fixes are applied. Steps 2.2-2.5 are read-only.

## Step 2.6: Process Validation Results

**Apply fixes only for issues validated as "valid" or "uncertain" in Step 2.5.**

```
FOR issue in result.issues:
    index = issue.index
    IF index in validation.invalid:
        # Reviewer was wrong, skip this fix (no persist needed)
        CONTINUE
    IF index in validation.uncertain:
        # Uncertain issues go through is_pending() like valid issues
        # "Uncertain" = validator unsure of validity, but decidability/severity logic still applies
        IF is_orchestrator_decidable(issue):
            apply_fix(issue)
            persist_fix(issue, iteration, "Phase2-Uncertain")
            applied_fixes++
        ELIF is_pending(issue):  # CRITICAL or loop
            persist_pending(issue, iteration, "Phase2-Uncertain")  # Immediate file write
        # ELSE: non-critical, non-loop, non-decidable + uncertain = skip
        CONTINUE
    IF is_pending(issue):  # CRITICAL or loop
        persist_pending(issue, iteration, "Phase2-Pending")  # Immediate file write
    ELSE:
        apply_fix(issue)
        persist_fix(issue, iteration, "Phase2-Review")
        applied_fixes++
```

## Step 2.6.1: Post-Fix Structural Cleanup

**Purpose**: Eliminate structural side-effects from Step 2.6 fixes before LLM re-review.

```
IF applied_fixes > 0:
    Bash: python src/tools/python/ac_ops.py ac-check {target_id} --fix
    Bash: python src/tools/python/ac_ops.py ac-renumber {target_id}
    # Structural side-effects (stale count, numbering gap, invalid matcher) cleaned immediately
    # ac-renumber closes numbering gaps from AC insertion/deletion/reordering
    # These fixes are NOT counted in applied_fixes (not subject to LLM re-review)
    # Remaining issues are informational (Phase 4 re-detects them)
```

**Rationale**: LLM fixes in Step 2.6 often introduce structural side-effects (e.g., deleting an AC creates a numbering gap, adding ACs makes "All N ACs" stale). Without cleanup, the re-review in the next iteration detects these as "new issues", inflating iteration count. ac-renumber is idempotent (no-op when no gaps exist).

---

## Step 2.6.5: Pending-Gate (Early POST-LOOP Escape)

**Purpose**: When all reviewer issues are user-pending or invalid and no fixes were applied, remaining phases (3-8) will only re-detect the same pending issues. Skip directly to POST-LOOP to ask the user.

**Evidence**: F779 session — 2 pending issues (AC-005/AC-006) were re-reported 10+ times across Phase 2/3/5, consuming ~300K tokens with 0 net fixes applied. Phase 3-8 cannot resolve user-decision issues.

```
IF applied_fixes == 0 AND result.issues.count > 0:
    # Count how issues were classified in Step 2.6
    pending_count = count(issues routed to persist_pending)
    invalid_count = count(issues in validation.invalid)

    IF pending_count > 0 AND (pending_count + invalid_count) == result.issues.count:
        # At least one [pending] issue exists, rest are invalid
        # No progress possible without user input → skip to POST-LOOP
        #
        # NOTE: "all invalid" (pending_count == 0) does NOT trigger Pending-Gate.
        # If all reviewer issues were invalid, the feature may have real issues
        # detectable by Phases 3-8 (Maintainability, AC Validation, etc.).

        TaskUpdate(subject: "Phase 2: Review-Validate-Apply", status: "completed")
        TaskUpdate(subject: "Iteration {I}/10: POST-LOOP (Pending-Gate)", status: "pending")
        Read(.claude/skills/fl-workflow/POST-LOOP.md)
        # EXIT — do not proceed to Step 2.7 or Declare Next Phase
```

**Interaction with Forward-Only Mode**: Pending-Gate applies in Forward-Only Mode too. If all issues are pending at MAX_ITERATIONS, going forward through Phases 3-8 will not resolve them.

**Interaction with Phase 2 re-run routing**: Pending-Gate only fires when `applied_fixes == 0`. If any fix was applied, normal routing (loop-back to Phase 2 or proceed to Phase 3) applies.

---

## Step 2.7: Complete Phase 2

```
TaskUpdate(subject: "Phase 2: Review-Validate-Apply", status: "completed")
```

## Step 2.8: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| issues.count == 0 (feature) | 3 |
| issues.count == 0 (non-feature) | POST-LOOP |
| applied_fixes > 0 | 2 (re-run) |
| applied_fixes == 0 | 3 |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **3** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 3 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 2
- Condition: {condition}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
