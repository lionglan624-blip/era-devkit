# Phase 9: Report & Approval

## Goal

Report results to user and obtain approval.

---

## Step 9.1: Execution History Verification (MANDATORY FIRST)

**CRITICAL: Execute before creating report. Do NOT skip this step.**

### 9.1.1: Mechanical Check (F575 Lesson)

**CRITICAL: Do NOT rely on memory. Mechanically scan conversation history.**

Count the following in this conversation:

| Check | Method | Exclude |
|-------|--------|---------|
| Bash exit ≠ 0 | Count non-zero exits | Phase 3 TDD RED |
| Re-executions | Detect retry patterns | - |
| Agent failures | Count ERR:* returns | - |
| timeout | Count exit code 124 | - |
| Manual intervention | Count taskkill, kill, manual | - |

**Required search patterns** (scan conversation history):
- `Exit code:` or `exit code` to check exit codes
- `exit 1`, `exit 124` and other non-zero exits
- `taskkill`, `kill`, `KillShell` usage
- `retry`, `re-run` patterns
- `timeout` occurrences

**Prohibited**: Memory-based reporting like "probably 0 occurrences"

### 9.1.2: Compile Results

```markdown
| Check | Count | Details |
|-------|:-----:|---------|
| Bash exit ≠ 0 | {N} | {list commands} |
| Re-executions | {N} | {list phases/steps} |
| Agent failures | {N} | {list agents} |
```

### 9.1.3: Verify Against Execution Log

**Mechanical count** (MUST execute):
```bash
# Hook記録のdeviation-log
cat _out/tmp/deviation-log.txt 2>/dev/null || echo "No deviation log"
wc -l < _out/tmp/deviation-log.txt 2>/dev/null || echo "0"

# feature-{ID}.mdのDEVIATION数
grep -c "DEVIATION" pm/features/feature-{ID}.md
```

**Compare results**:

| Source | Count |
|--------|:-----:|
| deviation-log.txt (hook) | {A} |
| feature-{ID}.md | {B} |
| Step 9.1.2 (manual count) | {C} |

| Comparison | Action |
|------------|--------|
| A ≤ B and C ≤ B | Proceed to Step 9.2 |
| A > B or C > B | **Add missing DEVIATION to Execution Log** |

**Do NOT proceed to Step 9.2 until all recorded.**

---

## Step 9.2: Final AC Re-Verification

**CRITICAL: Phase 8 fixes may modify implementation code (NEEDS_REVISION → debugger). Phase 7 JSON logs become stale after such changes. Always re-verify to ensure final results reflect current code state.**

### 9.2.1: Re-run Static AC Verification

Re-run `ac-static-verifier.py` for each AC type present in the feature's AC Definition Table. This overwrites Phase 7 JSON logs with current results.

```bash
# Check which AC types exist in the feature, then run only those:
python src/tools/python/ac-static-verifier.py --feature {ID} --ac-type code   # if code ACs exist
python src/tools/python/ac-static-verifier.py --feature {ID} --ac-type file   # if file ACs exist
python src/tools/python/ac-static-verifier.py --feature {ID} --ac-type build  # if build ACs exist
```

| Result | Action |
|--------|--------|
| All exit 0 | Proceed to 9.2.2 |
| Any exit ≠ 0 | **DEVIATION** → Record → Step 9.5 (Problem Resolution) |

**Behavioral AC re-test gate** (`output`/`exit_code` types):

These AC types have no static verifier support. Check mechanically:

| Condition | Action |
|-----------|--------|
| Phase 8 had zero NEEDS_REVISION | Skip — Phase 7 results are valid |
| Phase 8 NEEDS_REVISION changed only docs/skills (.md files) | Skip — behavioral outputs unaffected |
| Phase 8 NEEDS_REVISION changed implementation code (.py, .cs, .erb, etc.) AND feature has output/exit_code ACs | **Re-dispatch ac-tester** for those ACs before 9.2.2 |

### 9.2.2: Aggregate Results

```bash
python src/tools/python/verify-logs.py --scope feature:{ID}
```

Expected output format:
```
Feature-{ID}:    OK:{N}/{N}
Result:          OK:{total}/{total}
```

**Note**: The AC label is `Feature-{ID}` (not `AC`) when using `--scope feature:{ID}`.

---

## Step 9.3: Route Based on Status

### Problem Resolution Tracking (File-Based)
- `problem_resolution_attempts` = count of `[problem-fix]` entries in feature.md Review Notes
- `resolved_issues` = set of issue strings parsed from `[problem-fix]` entries
- Read from file at each Step 9.3 entry. Survives context compression and session crashes.
- Format: `- [problem-fix] Step9.5: {issue_type}:{issue_description}`

| Condition | Route |
|-----------|-------|
| All PASS **AND** DEVIATION == 0 | Step 9.4 |
| Any FAIL **OR** DEVIATION > 0 | Step 9.5 (Problem Resolution) |

---

## Step 9.4: Handoff Destination Verification

IF "Mandatory Handoffs" section exists AND has rows:

```
FOR each row:
  IF destination is empty OR contains "TBD":
    Execute decision logic (no user confirmation)
```

**Decision Logic** (orchestrator decides and executes):

```
1. Should it be done now? (urgency)
   ├─ Yes → go to 2 (resolve within this Feature)
   └─ No  → go to 3

2. Does it belong to an existing Feature?
   ├─ Yes → B (add to existing Feature)
   └─ No  → A (create new Feature DRAFT)

3. Which Phase is suitable? (scope specification)
   └─ Is that Phase DONE? (check **Phase Status** in designs/phases/*.md)
      ├─ DONE → REJECT: completed phases cannot receive handoffs. Select a different Phase.
      └─ Not DONE → continue
         └─ Are there features for that Phase?
            ├─ Yes → go to 2 (existing Feature in that Phase or new [DRAFT])
            └─ No  → C (write to Phase tasks in architecture.md)
```

**Actions**:
- A: Create DRAFT now → Use Deviation Context template (see below), register in index-features.md
- B: Add to existing Feature's Tasks
- C: Add to architecture.md Phase Tasks (features未作成のPhaseのみ有効)

**Action A — Deviation Context DRAFT Template**:
```markdown
# Feature {ID}: {Title}

## Status: [DRAFT]

## Type: {type}

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F{parent_ID} |
| Discovery Phase | Phase {N} |
| Timestamp | {YYYY-MM-DD} |

### Observable Symptom
{What happened. Facts only, no interpretation.}

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `{failed command}` |
| Exit Code | {N} |
| Error Output | `{first 200 chars of stderr}` |
| Expected | {expected behavior} |
| Actual | {actual behavior} |

### Files Involved
| File | Relevance |
|------|-----------|
| {path} | {why relevant} |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| {description} | {FAIL/PARTIAL} | {reason} |
| None | - | - |

### Parent Session Observations
{Observations from parent session. Max 200 words.}

## Background

### Philosophy (Mid-term Vision)
{Inherit from parent Feature's Philosophy}

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
```

**No user confirmation needed. Execute immediately after decision.**

**Handoff Destination Status Gate (MANDATORY)**:
```
FOR each destination Feature (Action A or B):
  status = Read feature-{dest_id}.md Status
  IF status != [DRAFT]:
    REJECT: "F{dest_id} is {status}, not [DRAFT]. Handoff destinations must be [DRAFT] to ensure the handoff is incorporated into the feature's design phase."
    → Create new [DRAFT] feature instead (Action A)
```
Rationale: Features in [PROPOSED]/[WIP]/[DONE] have already passed their design phase and cannot reliably incorporate new requirements. F806→F807 lesson: F807 was [WIP] when handoffs were created, resulting in handoffs being acknowledged but not executed.

Verify all destinations are valid before proceeding to 9.4.1.

### 9.4.1: Transfer Execution (F805 Lesson)

**CRITICAL: Clean Report (9.6) に Result を表示するため、転記は 9.6 の前に完了させる。**

Destination 確定後、全 Mandatory Handoff 行の転記を実行する:

```
FOR each Mandatory Handoff row:
  SWITCH Action:
    A (DRAFT作成):
      test -f pm/features/feature-{dest_id}.md
      → exists: Transferred [x], Result = 作成済み
      → NOT found: STOP (9.4 Decision Logic が DRAFT を作成していないのは異常)

    B (既存Feature):
      grep "{keyword}" pm/features/feature-{dest_id}.md
      → found: Transferred [x], Result = 確認済み
      → NOT found: Write obligation to destination file
        → Transferred [x], Result = 追記済み

    C (Phase):
      grep "{keyword}" docs/architecture/phases/*.md
      → found: Transferred [x], Result = 確認済み
      → NOT found: Write to architecture phase file
        → Transferred [x], Result = 記載済み
```

**Result values**:
- `作成済み`: Action A — DRAFT file created
- `追記済み`: Action B — Written to existing Feature
- `記載済み`: Action C — Written to architecture.md Phase
- `確認済み`: Content already exists in destination (grep verified)

**Exit criteria**: ALL rows have `Transferred = [x]` AND `Result` filled

| Result | Action |
|--------|--------|
| All rows transferred | Proceed to Step 9.6 |
| Any row failed | **STOP** → Fix before proceeding |

---

## Step 9.5: Problem Resolution (if issues)

**Do NOT ask for Finalize. Resolve first.**

### Retry Guard
problem_resolution_attempts = count [problem-fix] entries in feature.md Review Notes
IF problem_resolution_attempts >= 3:
    STOP → Report to user: "3 problem resolution attempts exhausted without convergence. Manual intervention required."
    Record remaining issues as DEVIATIONs → proceed to Step 9.8

### Issue Deduplication
resolved_issues = parse [problem-fix] entries from feature.md Review Notes
IF current issue type+description already in resolved_issues:
    STOP → Report: "Same issue class recurring after fix attempt. Root cause not resolved."
    Record as DEVIATION → proceed to Step 9.8

**Root Cause Resolution (MANDATORY)**:
- Identify WHY the problem occurred, not just WHAT went wrong
- Propose fixes that address the source, not symptoms
- Reject workarounds (special-case handling, retry logic, "known limitation" docs)
- If root cause is in workflow/docs, fix those first

Report format (user-facing, Japanese):
```
=== Feature {ID} Problem Resolution ===

**Discovered Issues**:
| # | Type | Problem | Cause |
|:-:|------|---------|-------|
| 1 | {type} | {description} | {cause} |

**Resolution Actions**:
| # | Action | Target |
|:-:|--------|--------|
| {A/B/C} | {description} | {target} |

(Decided and executed by orchestrator)
```

Execute determined action → Update tracking:
```
# Persist to feature.md Review Notes (immediate file write)
Append to Review Notes: - [problem-fix] Step9.5: {issue_type}:{issue_description}
```

**Re-entry routing** (based on origin of the failure):

| Origin | Re-run from |
|--------|-------------|
| Step 9.2.1 (AC re-verification failure) | → Re-run Step 9.2.1 (re-verify fix, then 9.2.2 → 9.3) |
| Step 9.3 (FAIL or DEVIATION from earlier phases) | → Re-run Step 9.3 |

---

## Step 9.6: Clean Report

**条件**: 全PASS、全DEVIATION記録済み

### レポートテンプレート

```
<!-- 出力ルール: このテンプレートをそのままコピーし、{} プレースホルダのみ置換する。
     日本語で出力。セクション追加・削除・言語変更は禁止。 -->

=== Feature {ID} 完了 ===
Type: {type} | Tasks: {done}/{total}
ACs: {pass} PASS / {fail} FAIL / {blocked} BLOCKED / {total} total

**DEVIATION Gate**:
| Error | Retry | Rerun |
|:-----:|:-----:|:-----:|
| {N} | {N} | {N} |

* Error: Execution Logの DEVIATION 行数
* Retry: 同一ステップの再試行回数
* Rerun: debugger/fix後のAC再検証回数
* 正常: 0/0/0 = DEVIATIONなし

**DEVIATION Root Cause Analysis**:
| DEVIATION | Root Cause | Action | Destination | Result |
|-----------|------------|:------:|-------------|--------|
| {説明} | {発生理由} | {A/B/C/D} | {dest} | {result} |

* Action値: A=新Feature作成, B=既存Featureに追記, C=architecture.mdに記載, D=このFeature内で修正済み
* Destination: Action=D の場合は `-`（ハンドオフ不要）、A/B/C の場合は F{ID} or Phase N
* Result値: 作成済み(A), 追記済み(B), 記載済み(C), 修正済み(D)
* DEVIATIONなしの場合、テーブルの代わりに "None" と記載

**Mandatory Handoffs** (feature-{ID}.md から転記):
| Issue | Action | Destination | Result |
|-------|:------:|-------------|--------|
| {issue} | B | F{ID} | {result} |

* feature-{ID}.md の Mandatory Handoffs テーブルが空の場合: "None"
* Result値: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)

**Philosophy達成**:
- Philosophy: {Backgroundから引用}
- 達成: {達成した内容}
```

### Completion Gate（BLOCKED ACルール）

| ACステータス | [DONE]可能？ |
|-------------|:------------:|
| 全て `[x]` | Yes |
| `[-]` あり | **No** - 修正必須 |
| `[B]` あり | **No** - ユーザーの明示的waive必須 |

**[B]が存在する場合**: レポートで選択肢を提示:
- A: BLOCKED → [WIP]のまま、ブロッカー解消を待つ
- B: ユーザーがwaive → [DONE]にして[B] ACをMandatory Handoffsとして記録
- C: このFeature内でブロッカーを修正

**[B] ACが存在する場合、ユーザー決定なしに[DONE]を推奨してはならない。**

### Status Transition on [BLOCKED]

When user chooses option A (wait for blocker):

1. Record decision in Execution Log
2. Proceed to Phase 10 — **finalizer handles atomic status update** (feature-{ID}.md + index-features.md)

**Note**: Status changes are performed by finalizer. PHASE-9 does not change status.

**Next** → Step 9.7

---

## Step 9.7: AC Coverage Check

Verify threshold-matcher AC Details match implementation:

| Status | Action |
|--------|--------|
| All threshold AC Details (Derivation) requirements satisfied | → Continue |
| Intentionally unimplemented requirements | → Record in Handoff Destinations section |

**Unimplemented Record Format**:
```markdown
| Unimplemented | AC#{N} {requirement} - {reason} | → F{xxx} or Phase N |
```

**CRITICAL**: Completing without recording unimplemented items is prohibited.

**Next**:
- If DEVIATION > 0 OR Remaining Issues exist → Step 9.8
- Else → "Finalize and commit? (y/n)"

---

## Step 9.8: Remaining Issues Review (if issues OR DEVIATION > 0)

**Trigger**: Remaining issues exist OR DEVIATION > 0

**CRITICAL: A/B/C/D decisions are made and executed by orchestrator. Do not ask user to choose.**

**Decision Logic**:
```
0. Already fixed in this session?
   ├─ Yes → D (このFeature内で修正済み、Destination = "-")
   └─ No  → go to 1

1. Should it be done now? (urgency)
   ├─ Yes → go to 2 (resolve within this Feature)
   └─ No  → go to 3

2. Does it belong to an existing Feature?
   ├─ Yes → B (add to existing Feature)
   └─ No  → A (create new Feature DRAFT)

3. Which Phase is suitable? (scope specification)
   └─ Is that Phase DONE? (check **Phase Status** in designs/phases/*.md)
      ├─ DONE → REJECT: completed phases cannot receive handoffs. Select a different Phase.
      └─ Not DONE → continue
         └─ Are there features for that Phase?
            ├─ Yes → go to 2 (existing Feature in that Phase or new [DRAFT])
            └─ No  → C (write to Phase tasks in architecture.md)
```

### 9.8.1: DEVIATION Root Cause Analysis

**For each DEVIATION in Execution Log**:

```
| DEVIATION | Root Cause | Action | Destination | Result |
|-----------|------------|:------:|-------------|--------|
| {description} | {why it occurred} | A/B/C/D | F{ID} or Phase N or - | 作成済み/追記済み/記載済み/修正済み |
```

**Actions** (orchestrator decides):
- A: Create Feature now (workflow fix)
- B: Add to existing Feature's Tasks
- C: Add to architecture.md Phase Tasks
- D: このFeature内で修正済み（ハンドオフ不要、Destination = `-`）

**Result values**:
- `作成済み`: Action A - New Feature DRAFT created
- `追記済み`: Action B - Added to existing Feature
- `記載済み`: Action C - Added to architecture.md Phase
- `修正済み`: Action D - Fixed within this Feature

**CRITICAL**: "Orchestrator error" is not an action. Select what workflow to fix.

**Root Cause Resolution Criteria**:
| Valid Root Cause | Invalid (Workaround) |
|------------------|----------------------|
| Fix workflow that caused error | "Add retry logic" |
| Update docs that were unclear | "Document as limitation" |
| Fix design that allowed bad state | "Add validation here" |
| Generalize solution | "Special-case this scenario" |

**Forbidden Actions** (these are NOT valid Action values):
| Pattern | Why Invalid |
|---------|-------------|
| "PRE-EXISTING" | Does not fix the workflow |
| "Tool limitation" | Tool should be fixed |
| "Already documented" | Documentation is not a fix |
| "Manual verification OK" | Does not prevent recurrence |
| "-" or "N/A" | Action must be A/B/C/D |

**PRE-EXISTING Clarification (F696 Lesson)**:
- "PRE-EXISTING" is **invalid as Action** but **tracking is mandatory**
- Even for PRE-EXISTING issues, **decide A/B/C/D and create/specify Destination**
- ❌ `| PRE-EXISTING issue | PRE-EXISTING | - |` ← No Destination is prohibited
- ✅ `| PRE-EXISTING issue | Schema change in converter | A | F701 |` ← Create new Feature for tracking

**実例**:
```
| DEVIATION | Root Cause | Action | Destination | Result |
|-----------|------------|:------:|-------------|--------|
| AC静的検証 70/81 | 正規表現パターン不一致 + 実装漏れ | D | - | 修正済み |
| PRINT_STATE表示処理スタブ | F777スコープ外の表示委譲 | B | F782 | 追記済み |
| ビルド失敗: 未定義型 | Phase 21で追加予定の型が未作成 | A | F801 | 作成済み |
| CSV名前解決の不整合 | Phase 20設計文書に記載漏れ | C | Phase 20 | 記載済み |
```

### 9.8.2: Other Remaining Issues

**Remaining Issue Sources**:
1. feature-{ID}.md Out-of-Scope Note
2. Unaddressed items discovered during implementation
3. Next steps needed for Philosophy achievement

**For each remaining issue**:

```
| Issue | Background | Action |
|-------|------------|:------:|
| {description} | {why it occurred} | A/B/C |
```

**Actions** (orchestrator decides - see Decision Logic):
- A: Create Feature now
- B: Add to existing Feature's Tasks
- C: Add to architecture.md Phase Tasks

**TBD Check**:
```bash
grep -i "TBD" pm/features/feature-{ID}.md
```
→ If match exists, **STOP**: Resolve before proceeding.

### 9.8.3: Handoff Materialization Gate (MANDATORY)

**CRITICAL: Deciding A/B/C is not enough. The destination must exist before Phase 10.**

For each Action decided in 9.8.1 / 9.8.2, **AND** each Mandatory Handoff row from feature-{ID}.md (9.4.1 漏れ検証):

| Action | Exit Criteria | Verification |
|:------:|---------------|--------------|
| A | DRAFT file exists AND registered in index-features.md | `test -f pm/features/feature-{N}.md && grep "F{N}" pm/index-features.md` |
| B | Target Feature's Tasks table contains the new item | `grep "{description}" pm/features/feature-{target}.md` |
| C | architecture.md Phase section contains the item | `grep "{description}" docs/architecture/migration/full-csharp-architecture.md` |

**STOP if any Action lacks materialized destination.** Create/edit now before proceeding.

### Root Cause Fix Validation (MANDATORY)

**For each Destination, ask: "Will this Destination result in code being written to fix the problem?"**

| Answer | Action |
|--------|--------|
| Yes (Feature with Tasks that change code) | Pass |
| No (points to documentation, Known Limitations, or existing text) | **REJECT** → Action A: Create Feature DRAFT to implement the fix |

**Rejected patterns** (these are NOT valid Destinations):
- Documentation reference (e.g., "testing SKILL Known Limitations")
- Existing text that describes but does not fix the issue
- "Already tracked in {doc}" without actionable fix Tasks

Also update feature-{ID}.md:
1. `Remaining Issues` table: every row has a concrete `Destination` (no empty cells)
2. `Mandatory Handoffs` table: every row has `Destination ID` filled

---

## Approval Gate

Wait for user response:
- "y/yes/OK" → proceed to Phase 10
- Other → clarify or address concerns

---

## Next

On user approval:
```
Read(.claude/skills/run-workflow/PHASE-10.md)
```
