# Post-Loop Processing

## Goal

Handle pending user issues, update skill patterns, run Philosophy Gate, update status, sync index, and generate report.

**Step Order**: Pending User → **Dependency Propagation** → Skill Update → **Philosophy Gate** → Status Update → **Index Sync** → Report

---

## Loop Control

```
post_loop_restarts = 0  # Initialize at POST-LOOP entry
```

---

## Step 1: Update Task

```
TaskUpdate(subject: "Post-loop: pending_user confirmation", status: "in_progress")
```

## Step 2: Pending User Confirmation

Check if **unresolved** `[pending]` remains in Review Notes.

**CRITICAL**: Filter out already-resolved entries. Only `[pending]` tags require user confirmation.
- `[resolved-applied]` = Fixed during loop (no user action needed)
- `[resolved-invalid]` = Invalidated by validation (no user action needed)

```
# Filter: Extract only unresolved [pending] entries
# EXCLUDE: [resolved-applied], [resolved-invalid]
unresolved_pending = Review Notes entries WHERE tag == "[pending]"
                     # Entries with [resolved-*] tags are already handled

IF pending_user is not empty OR unresolved_pending is not empty:
    user_decisions = AskUserQuestion(unresolved_pending)
    deferred_drafts = []  # Collect DRAFT creation requests for Step 6.3
    has_applied_fixes = false  # Track if any fixes were applied

    FOR decision in user_decisions:
        IF decision.action == "apply":
            # GUARD: Step 2 does NOT create new Feature files (DRAFT).
            # File creation uses Step 6.3 DEFER template exclusively.
            IF decision requires creating new Feature DRAFT:
                deferred_drafts.append(decision)
                resolve_pending(decision.issue, "post-loop", "resolved-applied")
                # Note: actual file creation happens in Step 6.3
            ELSE:
                apply_fix(decision.issue)
                resolve_pending(decision.issue, "post-loop", "resolved-applied")
                persist_fix(decision.issue, "post-loop", "PostLoop-UserFix")
                has_applied_fixes = true
        ELIF decision.action == "skip":
            resolve_pending(decision.issue, "post-loop", "resolved-skipped")
        ELIF decision.action == "cancel_fl":
            # User explicitly cancelled FL
            GOTO Report
    pending_user.clear()

    # Execute Step 3 (Dependency Propagation) BEFORE re-loop
    # User decisions may affect other features — propagation context is lost after GOTO
    → Execute Step 3 (Dependency Propagation Gate) with user_decisions

    # After user decisions with fixes, restart loop for re-verification
    IF has_applied_fixes:
        post_loop_restarts += 1
        IF post_loop_restarts >= 2:
            STOP → Report to user: "FL has re-looped 2 times from POST-LOOP. Manual intervention required. Feature remains [PROPOSED]."
        iteration = 0
        GOTO Phase 2  # Restart from Phase 2 (Review-Validate-Apply) per SKILL.md routing table
```

### AskUserQuestion Execution (Mandatory Steps)

**CRITICAL**: One issue at a time. Do NOT batch.

**Rationale**:
- User may give nuanced response that affects subsequent issues
- Batching overwhelms user and reduces decision quality
- Issues may have hidden dependencies

**Anti-pattern WARNING**:
- "Efficiency" is NOT a valid reason to batch
- Even with 10+ issues, process one-by-one
- If tempted to batch → This is a sign to slow down

**Execution Flow** (3 separate steps - do NOT skip any):

```
Step 1: Output text explanation (Japanese)
        ↓
Step 2: Call AskUserQuestion tool
        ↓
Step 3: Wait for response, then next issue
```

---

**Step 1: Output Text** (respond in Japanese)

Output the following format as text BEFORE calling AskUserQuestion:

```
【Issue N/M: {課題タイトル}】

【背景】
{なぜこの判断が必要か、技術的コンテキスト}

【選択肢】
A) {推奨選択肢}（推奨） - {トレードオフ}
B) {代替選択肢} - {トレードオフ}
C) スキップ - {結果}

【理由】{Aを推奨する理由、長期保守性に基づく}
```

---

**Step 2: Call AskUserQuestion**

AFTER outputting Step 1 text, call AskUserQuestion with options in A/B/C order.

**CRITICAL**: Step 1のテキスト出力時点で**推奨をA)に配置**する。これによりテキストとoptions配列が自然に一致。

Example:
```json
{
  "options": [
    {"label": "A) 概算表記に変更（推奨）", "description": "「約40+ CSV files」などに変更"},
    {"label": "B) 44 CSV filesに修正", "description": "正確な数値に更新"},
    {"label": "C) スキップ", "description": "現状維持"}
  ]
}
```

---

**Recommendation Criteria**: Based on **long-term maintainability and zero technical debt**. Prioritize consistency, testability, and future extensibility over simplicity.

### Recommendation Self-Check (MANDATORY)

**BEFORE writing Step 1 text**, verify:

```
□ Is my recommendation "DEFER" or "Skip"?
  → If YES: Is it because this is TECHNICALLY IMPOSSIBLE in this feature?
  → If NO (just "scope" or "cost" reason): Change to ADOPT

□ Does my reasoning contain FORBIDDEN words?
  - "keep scope"
  - "already covered"
  - "cost"
  - "keep simple"
  - "not needed now"
  - "the main purpose of this Feature is..." (scope excuse)
  → If YES: Rewrite reasoning from FUTURE perspective

□ Is my fix a WORKAROUND instead of ROOT CAUSE resolution?
  - Adding special-case handling
  - Documenting as "known limitation"
  - Adding retry/fallback for flaky behavior
  - Fixing symptom but not source
  → If YES: Find and recommend root cause fix instead
```

**Default recommendation**: ADOPT (add to this feature now)

**Only valid DEFER reasons**:
- Requires predecessor feature not yet [DONE]
- Requires external dependency (API, library) not available
- Technically conflicts with this feature's changes

**Root Cause Resolution**: Always recommend fixing the source of the problem, not adding workarounds. Workarounds create hidden technical debt.

## Step 3: Dependency Propagation Gate

**Trigger**: AskUserQuestion completed with decisions that affect other features.

**Purpose**: Propagate user decisions to affected features (AC/Task/Dependencies/Review Notes updates).

### 3.1 Analyze User Decisions

```
affected_features = []

FOR decision in user_decisions:
    IF decision involves handoff OR coordination with other feature:
        affected = {
            feature_id: extract_feature_id(decision),
            changes_needed: []
        }

        # Determine what needs updating in target feature
        IF decision changes target's responsibility:
            affected.changes_needed.append("AC updates")
            affected.changes_needed.append("Task updates")
        IF decision changes dependency relationship:
            affected.changes_needed.append("Dependencies table")
        IF decision requires context transfer:
            affected.changes_needed.append("Review Notes")
            affected.changes_needed.append("Links section")

        affected_features.append(affected)
```

### 3.2 Apply Propagation

```
IF affected_features is not empty:
    FOR feature in affected_features:
        Read("pm/features/feature-{feature.feature_id}.md")

        FOR change in feature.changes_needed:
            IF change == "AC updates":
                # Update AC table to reflect new responsibility
                Edit AC Definition Table as needed
            IF change == "Task updates":
                # Update Tasks to match AC changes
                Edit Tasks table as needed
            IF change == "Dependencies table":
                # Update Dependencies (status, description)
                Edit Dependencies table
            IF change == "Review Notes":
                # Add handoff context
                Append to Review Notes: "### {source_feature} Handoff ({date})\n{context}"
            IF change == "Links section":
                # Update link description if needed
                Edit Links section

        propagation_log.append(feature)

    Report: "Dependency propagation: {affected_features.count} features updated"
ELSE:
    Report: "Dependency propagation: No affected features"
```

### 3.3 Propagation Verification

```
# Verify all affected features are consistent
FOR feature in affected_features:
    # Check Dependencies table matches reality
    verify_dependencies_consistency(feature.feature_id)

    # Check Links section includes this feature
    verify_links_bidirectional(target_id, feature.feature_id)
```

## Step 4: Complete Post-Loop Task

```
TaskUpdate(subject: "Post-loop: pending_user confirmation", status: "completed")
```

## Step 5: Skill Update (Log-Only)

**Purpose**: Record fix patterns for future `/run` or `/imp` sessions to act on. Actual skill edits happen in `/run` (Phase 8.2) or `/imp`, not here.

```
IF target_type == "feature":
    feature_type = read_feature_type(target_path)  # engine, erb, kojo, etc.
    fix_entries = Grep("[fix]", feature.md Review Notes)  # All [fix] tagged lines

    IF fix_entries is not empty:
        Log: "Step 5: {len(fix_entries)} fix entries found for {feature_type} type. Patterns deferred to /run or /imp."
    ELSE:
        Log: "Step 5: No fix entries. Skipping."
```

**Rationale**: FL fixes are recorded in Review Notes. Skill file updates require reading + comparison which is better suited to `/run` Phase 8.2 (extensibility review) or `/imp` (cross-feature pattern analysis) where broader context is available.

## Step 6: Philosophy Gate (Features Only)

**Integration Point**: Runs BEFORE Status Update. Status is only updated after Philosophy Gate passes.

### Skip Conditions
- target_type != "feature" → Skip to Step 9 (Report)
- Feature has no Philosophy section → Skip with WARNING

### 6.1 Derive Tasks from Philosophy

```
derived = Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: `Read .claude/skills/philosophy-deriver/SKILL.md and execute for Feature {target_id}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)
```

### 6.2 Compare with Current Tasks

```
comparison = Task(
  subagent_type: "general-purpose",
  model: "sonnet",
  prompt: `Read .claude/skills/task-comparator/SKILL.md and execute for Feature {target_id}. Derived tasks: {derived.derived_tasks}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)
```

### 6.3 Process Decisions (REJECT → DEFER → ADOPT)

**Processing order**: REJECT first, then DEFER (materialize before ADOPT), then ADOPT last (may trigger GOTO WHILE).

**Step 2 deferred_drafts**: If Step 2 collected any `deferred_drafts` (user decisions requiring new Feature DRAFTs), process them here using the same DEFER destination A template below. This ensures ALL DRAFT creation uses a single template.

```
# First: process deferred_drafts from Step 2 (if any)
FOR draft_request in deferred_drafts:
    # Use destination A template below (same as Philosophy Gate DEFER)
    → Execute "IF destination == A" block for each draft_request

IF comparison.gaps is empty AND deferred_drafts is empty:
    → Continue to Step 7 (Status Update)

IF comparison.gaps is not empty:
    # Classify all gaps first
    FOR each gap in comparison.gaps:
        decision = Opus evaluates:
        | Decision | Criteria | Action |
        |----------|----------|--------|
        | ADOPT | Essential for Philosophy, feasible in this Feature | Add as new AC/Task |
        | DEFER | Technically impossible in this Feature | Create destination (A/B/C) |
        | REJECT | Not actually required (deriver overreached) | Skip |

    ## REJECT decisions (no action needed)
    FOR gap WHERE decision == REJECT:
        Skip

    ## DEFER decisions (process before ADOPT — prevent GOTO WHILE loss)
    FOR gap WHERE decision == DEFER:
        destination = determine_destination(gap)  # A/B/C Decision Logic (.claude/reference/deferred-task-protocol.md)

        IF destination == A:
            # Option A: Create DRAFT (Review Context template)
            new_id = next_feature_number()
            Write("pm/features/feature-{new_id}.md"):
                # Feature {new_id}: {gap title}
                # Status: [DRAFT]
                # Type: {inferred from gap}
                # Review Context (FL POST-LOOP Step 6.3)
                # + Philosophy inherited from parent
                # + Empty Problem/Goal for /fc
                # + Review Notes, Mandatory Handoffs sections
            Edit index-features.md: add row for F{new_id} [DRAFT]
            Edit index-features.md: increment "Next Feature number" to new_id + 1
            Edit feature-{target_id}.md Mandatory Handoffs: add row (Destination ID: F{new_id})

        ELIF destination == B:
            # Option B: Add to existing Feature's Tasks
            Edit feature-{target_feature}.md Tasks table: add new task row

        ELIF destination == C:
            # Option C: Add to architecture.md Phase
            Edit architecture.md: add item to Phase {N}

    ## Materialization Gate (after all DEFER processing)
    FOR each DEFER decision:
        IF destination == A:
            verify: file "pm/features/feature-{new_id}.md" exists
                    AND index row for F{new_id} exists
                    AND "Next Feature number" > new_id
                    AND handoff row in parent feature exists
        IF destination == B:
            verify: target Feature Tasks table contains new item
        IF destination == C:
            verify: architecture.md Phase section contains item
        IF verification fails:
            STOP → Report to user: "Materialization failed for DEFER destination"

    ## Root Cause Fix Validation (after Materialization Gate)
    FOR each DEFER destination:
        Ask: "Will this Destination result in code/implementation to fix the gap?"
        IF No (points to documentation, known limitations, existing text):
            REJECT destination → Change to Action A: Create Feature DRAFT
            Re-run DEFER processing for this gap

    ## ADOPT decisions (last — may trigger GOTO WHILE)
    IF any ADOPT decisions:
        Apply AC/Task additions to feature-{target_id}.md
        GOTO Phase 2  # Restart from Phase 2 (Review-Validate-Apply) for re-verification
    ELSE:
        Continue to Step 7 (Status Update)
```

### 6.4 Loop Limit

Philosophy Gate can trigger at most 1 re-loop.
If Philosophy Gate runs a second time and still has gaps → Force DEFER all remaining gaps → route through Step 6.3 DEFER processing (A/B/C Decision Logic + Materialization Gate + Root Cause Fix Validation).

## Step 7: Status Update (Features Only)

**Trigger**: FL completes with zero issues AND Philosophy Gate passed.

**Re-review**: FL can be executed on any status (including [REVIEWED], [WIP]). No confirmation needed.

```
## Max Iterations Status Determination
#
# forward_fixes_total: total fixes applied during Forward-Only Mode (itr == MAX)
# - Tracked in SKILL.md Loop Control section
# - Phase 2 re-verification is skipped in Forward-Only Mode
# - Only forward_fixes_total == 0 guarantees Phase 2 has verified the final state
#
# Determination:
#   forward_fixes_total == 0 → "Max Iterations (No New Issues)" → REVIEWED
#   forward_fixes_total > 0  → "Max Iterations (New Issues Found)" → re-run /fl

IF target_type == "feature" AND (status == "Zero Issues (Complete)" OR (status == "Max Iterations" AND forward_fixes_total == 0)):
    current_status = read feature status (via: python src/tools/python/feature-status.py query {ID})

    IF current_status == "[PROPOSED]":
        # Automated status update + fl-reviewed marker + index sync:
        python src/tools/python/feature-status.py set {ID} REVIEWED --fl-reviewed
        new_status = "[REVIEWED]"
    ELSE:
        # Already [REVIEWED] or higher - no status change needed
        new_status = current_status

ELIF target_type == "feature" AND status == "Max Iterations" AND forward_fixes_total > 0:
    # Fixes applied during Forward-Only but Phase 2 never re-verified
    # Do NOT update status - user must re-run /fl
    new_status = current_status

ELIF target_type == "feature" AND pending_gate_fired:
    # Pending-Gate exit: Phase 2 found only [pending] issues, Phases 3-8 skipped
    # If user applied fixes → loop restarted (handled above, never reaches here)
    # If user skipped all → feature has known unresolved issues, do NOT promote
    new_status = current_status
```

**Gate Rule**: `/run` command only accepts `[REVIEWED]` or `[WIP]` status. This ensures FL review is mandatory before implementation.

| FL Result | Condition | Current Status | Status Update | Next Action |
|-----------|-----------|----------------|---------------|-------------|
| Zero Issues | - | [PROPOSED] | → [REVIEWED] | `/run {ID}` |
| Zero Issues | - | [REVIEWED]+ | No change | `/run {ID}` |
| Max Iterations | forward_fixes_total == 0 | [PROPOSED] | → [REVIEWED] | `/run {ID}` |
| Max Iterations | forward_fixes_total == 0 | [REVIEWED]+ | No change | `/run {ID}` |
| Max Iterations | forward_fixes_total > 0 | Any | No change | `/fl {ID}` を再実行 |
| Pending-Gate | User skipped all pending | Any | No change | `/fl {ID}` を再実行 |
| User Cancelled | - | Any | No change | Resume `/fl` later |

---

## Step 8: Index Sync Gate (Features Only)

**Note**: `feature-status.py set` in Step 7 already handles index sync, fl-reviewed marker, and dependency cascade atomically. This step is now a no-op for status changes handled by the tool.

**Remaining manual work**: Only needed if Step 7 did NOT change status (e.g., already [REVIEWED]).

### 8.1 Update Task

```
TaskUpdate(subject: "Post-loop: Index Sync", status: "completed")
```

### 8.2 Unblock Dependent Features

Handled automatically by `feature-status.py set` when status is DONE (via finalizer, not FL).
FL only promotes to [REVIEWED], which does not trigger unblocking.
All cascade logic is now handled by `feature-status.py set {ID} DONE` (called by finalizer, not FL).

---

## Step 9: Report

**Generate report after Index Sync completes.**

### Zero Issues (Complete)
```
=== FL完了: {target_type} {target_name} ===
イテレーション: {N}/{MAX} | 自動修正: {count}件
AC検証: 完了 ✓ (featureのみ、その他はN/A)
実現可能性: 完了 ✓ (featureのみ、その他はN/A)
Philosophyゲート: 完了 ✓ (featureのみ、その他はN/A)
依存性伝播: {propagation_log.count}件更新 (または「影響なし」)
Skill更新: {N}パターン追加 (または「新規パターンなし」)
Index同期: {unblocked_list.count}件ブロック解除 (または「変更なし」)
Critical: なし (またはpending_userアイテムをリスト)

ステータス: {previous_status} → {new_status}

適用した修正:
  - {location}: {old} → {new}
  - ...

ブロック解除されたFeature:
  - F{id}: {name}
  - ...
```

### Max Iterations - No New Issues (forward_fixes_total == 0)
```
=== FL完了(Forward): {target_type} {target_name} ===
イテレーション: {MAX}/{MAX} | 自動修正: {count}件
Forward-Only: 修正0件（Phase 2検証済み） → REVIEWED
AC検証: 完了 ✓ (featureのみ、その他はN/A)
実現可能性: 完了 ✓ (featureのみ、その他はN/A)
Philosophyゲート: 完了 ✓ (featureのみ、その他はN/A)
依存性伝播: {propagation_log.count}件更新 (または「影響なし」)
Skill更新: {N}パターン追加 (または「新規パターンなし」)
Index同期: {unblocked_list.count}件ブロック解除 (または「変更なし」)
Critical: なし (またはpending_userアイテムをリスト)

ステータス: {previous_status} → {new_status}

適用した修正:
  - {location}: {old} → {new}
  - ...
```

### Max Iterations - Fixes Without Re-verification (forward_fixes_total > 0)
```
=== FL停止: {target_type} {target_name} ===
イテレーション: {MAX}/{MAX} | 自動修正: {count}件 | Forward修正: {forward_fixes_total}件（Phase 2未検証）
残存課題: {remaining_issues.count}件
  - {issue}
Skill更新: {N}パターン追加 (または「スキップ - ループ未完了」)
Index同期: スキップ (ループ未完了)
Critical: なし (またはpending_userアイテムをリスト)

適用した修正:
  - {location}: {old} → {new}
  - ...

→ `/fl {ID}` を再実行してください
```

### User Cancelled
```
=== FLキャンセル: {target_type} {target_name} ===
イテレーション: {N}/{MAX} | 自動修正: {count}件
理由: ユーザーによるキャンセル
キャンセル時点の未解決課題: {pending_user.count}件
  - {issue}
Skill更新: スキップ (キャンセル)
Index同期: スキップ (キャンセル)

適用した修正:
  - {location}: {old} → {new}
  - ...
```

### Pending-Gate (User Decision Required)
```
=== FL一時停止（Pending-Gate）: {target_type} {target_name} ===
イテレーション: {N}/{MAX} | 自動修正: {count}件
Phase 2でユーザー判断待ちの課題のみ検出 → Phase 3-8スキップ
ユーザー判断: {applied_count}件適用 / {skipped_count}件スキップ

ステータス: {previous_status} → {new_status}

適用した修正:
  - {location}: {old} → {new}
  - ...

スキップした課題:
  - {issue}
  - ...

→ {next_action}
```

---

## Complete

FL workflow complete.
