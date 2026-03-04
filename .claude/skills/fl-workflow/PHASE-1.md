# Phase 1: Reference Check (Features Only)

## Entry Check (MANDATORY)

```
- Previous Phase: (start of FL loop)
- Condition met: FL command invoked
- Expected: Phase 1
- Actual: Phase 1
- ✓ Match confirmed → proceed
```

## Goal

Validate all references before detailed review. Prevents F329-style errors where related Features (e.g., F323) are overlooked.

---

## Step 1.1: Update Task

```
TaskUpdate(subject: "Phase 1: Reference Check", status: "in_progress")
```

## Step 1.1.5: Status Gate

```pseudocode
IF target_type == "feature":
    feature_status = Read status from feature-{target_id}.md
    IF feature_status == "[DRAFT]":
        STOP → Report to user: "Feature {target_id} is [DRAFT]. Run `/fc {target_id}` first to generate ACs/Tasks before FL review."
```

## Step 1.2: Dispatch reference-checker

**For features only. Skip for non-feature targets.**

```
IF target_type == "feature":
    ref_check = Task(
      subagent_type: "general-purpose",
      model: "sonnet",
      prompt: `Read .claude/skills/reference-checker/SKILL.md and execute for Feature {target_id}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
    )

    IF ref_check.status == "NEEDS_REVISION":
        FOR issue in ref_check.issues:
            IF issue.severity == "critical":
                persist_pending(issue, 1, "Phase1-RefCheck")  # Immediate file write
            ELSE:
                apply_fix(issue)
                persist_fix(issue, 1, "Phase1-RefCheck")

        IF any fixes applied:
            # Re-run reference check after fixes
            GOTO Phase 1
ELSE:
    TaskUpdate(subject: "Phase 1: Reference Check", status: "completed", details: "Phase 1: Reference Check (skipped - non-feature)")
```

## Step 1.3: Complete Phase 1

```
TaskUpdate(subject: "Phase 1: Reference Check", status: "completed")
```

## Step 1.4: Dependency Status Sync

**For features only. Skip for non-feature targets.**

依存先ステータスを実際の値に同期してからBLOCKED判定を行う。

```
IF target_type == "feature":
    drift_candidates = []

    # Single command replaces manual Read/Edit loop
    result = Bash("python src/tools/python/feature-status.py deps {ID} --sync")

    # Parse drift candidates from output
    FOR each line matching "DRIFT: F(\d+) \((\w+)\)":
        drift_candidates.append({feature: match.group(1), relationship: match.group(2)})

    # Note: get_feature_dependencies handles all dep types (Predecessor, Blocker,
    # Successor, Related) so separate Related Features table processing is unnecessary.

    Proceed to Step 1.4.5 (Codebase Drift Detection)
```

## Step 1.4.5: Codebase Drift Detection

**For features only. Triggered when sibling/related features completed since design.**

兄弟/関連featureが設計後に完了した場合、Technical Design の前提がコードベースと矛盾している可能性を検出する。

**Root Cause**: `/fc` はコードベースのスナップショットで設計を書く。兄弟featureが先に `/run` 完了するとコードベースが変わり、設計前提が陳腐化する（F764事例: F765完了後にSelectCaseNode/ARGハンドラの前提が全て無効化）。

```
IF drift_candidates is empty:
    → Skip to Step 1.5 (no drift possible)

# Check if drift was already analyzed for these candidates
already_checked = Read feature-{ID}.md Review Notes for "Phase1-DriftChecked: F{X}" entries
unchecked = drift_candidates.filter(c => c.feature NOT in already_checked)

IF unchecked is empty:
    → Skip to Step 1.5 (all drift candidates already checked)

drift_check = Task(
  subagent_type: "drift-checker",
  prompt: `Execute drift check.

Target feature: {target_id}
Drift candidates: {unchecked}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)

IF drift_check.status == "DRIFT_DETECTED":
    FOR assumption in drift_check.stale_assumptions:
        persist_pending(assumption, iteration, "Phase1-CodebaseDrift")

# Record checked candidates (prevents re-checking on next /fl run)
FOR each candidate in unchecked:
    Append to Review Notes: "- [info] Phase1-DriftChecked: F{candidate.feature} ({candidate.relationship})"

→ Proceed to Step 1.5 (Dependency Gate)
```

**Note**: Drift issues are persisted as `[pending]` and resolved in POST-LOOP Step 2. Phase 2 reviewers will also catch contradictions independently, but drift detection provides early visibility and categorized root-cause tracking.

## Step 1.5: Dependency Gate

```
IF target_type == "feature":
    1. Set blocked_by = []
    2. FOR each row in Dependencies WHERE Type = "Predecessor":
       IF row.Status ≠ [DONE]: blocked_by.append({row.Feature, row.Status})
    3. IF blocked_by is NOT empty:
       - IF feature Status == [BLOCKED]:
           Report: "Feature {ID} remains [BLOCKED]: Predecessor {blocked_by[0].id} is {blocked_by[0].status}"
           **STOP** (do not proceed to Phase 2)
       - ELSE:
           Edit feature-{ID}.md Status → [BLOCKED]
           Edit index-features.md Status → [BLOCKED]
           Report: "Feature {ID} [BLOCKED]: Predecessor {blocked_by[0].id} is {blocked_by[0].status}"
           **STOP** (do not proceed to Phase 2)
    4. IF feature Status == [BLOCKED] AND blocked_by is empty:
       - Edit feature-{ID}.md Status → [PROPOSED]
       - Edit index-features.md Status → [PROPOSED]
       - Report: "Feature {ID} unblocked: All predecessors now [DONE]. Continuing FL review."
       - CONTINUE to Phase 2 (FL completes → [REVIEWED])
    5. CONTINUE to Phase 2
```

## Step 1.6: Declare Next Phase (MANDATORY)

**ルーティングテーブル参照**:

| Condition | Next Phase |
|-----------|:----------:|
| ref_check.status == OK | 2 |
| ref_check.status == NEEDS_REVISION AND non-critical fixes applied | 1 (re-run) |
| ref_check.status == NEEDS_REVISION (critical only OR no fixes) | 2 (critical persisted to POST-LOOP) |

**実行**:
```
## Declare Next Phase
- Current Phase: 1
- Condition: ref_check.status = {status}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**判断しない。テーブルに従う。**
