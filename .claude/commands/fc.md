---
description: "Feature Completion - generate AC/Tasks for [DRAFT] feature"
argument-hint: "<feature-id>"
---

# /fc Command - Feature Completion

Complete a [DRAFT] feature by generating AC/Tasks via Task dispatch.

## Usage
/fc {ID}

## Precondition
- feature-{ID}.md exists with [DRAFT] status
- Philosophy section exists (Problem/Goal may be empty — Phase 1 will populate)

## Resume Detection

When re-running /fc on an existing feature, detect which phases were completed **by /fc** (not manually written) and skip them.

**Marker Format**: Each agent writes a completion marker when finishing its phase:
```html
<!-- fc-phase-{N}-completed -->
```

**Marker Placement**: Immediately before the section header the agent writes.

| Phase | Marker | Section |
|:-----:|--------|---------|
| 1 | `<!-- fc-phase-1-completed -->` | ## Background |
| 2 | `<!-- fc-phase-2-completed -->` | ## Root Cause Analysis |
| 3 | `<!-- fc-phase-3-completed -->` | ## Acceptance Criteria |
| 4 | `<!-- fc-phase-4-completed -->` | ## Technical Design |
| 5 | `<!-- fc-phase-5-completed -->` | ## Tasks |
| 6 | `<!-- fc-phase-6-completed -->` | (before ## Links) |

**determine_resume_point(feature_file) → Integer (1-7)**

Returns the phase number to resume from. Checks markers in reverse order for conservative detection.

**Logic**:
```
If Grep("<!-- fc-phase-6-completed -->", feature_file) succeeds → return 7 (skip to validation)
If Grep("<!-- fc-phase-5-completed -->", feature_file) succeeds → return 6 (skip to quality auto-fix)
If Grep("<!-- fc-phase-4-completed -->", feature_file) succeeds → return 5 (skip to WBS)
If Grep("<!-- fc-phase-3-completed -->", feature_file) succeeds → return 4 (skip to tech design)
If Grep("<!-- fc-phase-2-completed -->", feature_file) succeeds → return 3 (skip to AC design)
If Grep("<!-- fc-phase-1-completed -->", feature_file) succeeds → return 2 (skip to consensus review)
Otherwise → return 1 (start from consensus investigation)
```

**Key Distinction**:
- Marker present → /fc wrote this section → skip
- Section present but no marker → manual/other session wrote → treat as **reference**, re-execute phase

**Reference Handling**: When a section exists without a marker, the agent MUST:
1. Move existing section to `## Reference (from previous session)` at end of file
2. Rename moved section header to `### {Original Section Name} (reference)`
3. Generate new content (may incorporate existing as reference)
4. Write new section with marker in original location

Example:
```markdown
<!-- Before -->
## Root Cause Analysis
[existing content without marker]

<!-- After -->
<!-- fc-phase-2-completed -->
## Root Cause Analysis
[newly generated content]

...

## Reference (from previous session)

### Root Cause Analysis (reference)
[existing content preserved here]
```

**Skip Behavior**: On resume, skip all Task() dispatches where phase number < resume_from value. feature-validator (Phase 7) always executes regardless of resume_from to validate complete feature.

## Available Tools

| Tool | Usage |
|------|-------|
| `python src/tools/python/feature-status.py set {ID} PROPOSED` | Step 9: status transition |
| `python src/tools/python/ac_ops.py ac-check {ID}` | AC整合性チェック（gaps, orphan refs, count mismatches） |
| `python src/tools/python/ac_ops.py ac-renumber {ID}` | AC番号の隙間を詰める（例: 1,2,5 → 1,2,3） |
| `python src/tools/python/ac_ops.py ac-fix {ID} --ac N --expected VAL` | AC Expected/Description を個別修正 |
| `python src/tools/python/ac_ops.py ac-insert {ID} --after N` | AC挿入（後続ACを+1シフト） |
| `python src/tools/python/ac_ops.py ac-delete {ID} --ac N` | AC削除+自動renumber |

Details: `python src/tools/python/ac_ops.py --help`, `python src/tools/python/feature-status.py --help`

## Procedure

1. Read feature-{ID}.md, verify [DRAFT] status
1b. Call determine_resume_point(feature_file) → resume_from value
2. If resume_from <= 1: **Phase 1 — Consensus Investigation (Round 1)**
   a. Orchestrator: collect baseline context via Bash (git log, relevant file listings)
   b. Construct investigation prompt from **Investigation Prompt Template** (see below), embedding:
      - Feature file context section (Deviation Context or Review Context — whichever exists) and existing Background/Philosophy
      - Pre-collected baseline data
   c. Launch 3 deep-explorer Tasks in parallel with identical prompt:
      ```
      Task(subagent_type: "deep-explorer", prompt: investigation_prompt)  // ×3 in single message
      ```
      **CRITICAL: Do NOT use `run_in_background: true`. All 3 Tasks MUST be synchronous calls in a single message. Background tasks cause premature session exit in `-p` mode (the parent process terminates before background agents complete, losing all results).**
   d. Collect 3 investigation results (each returns structured report as text)
   e. Task(consensus-synthesizer, model: opus, prompt: "Read .claude/agents/consensus-synthesizer.md. Mode: synthesis. Feature: {ID}. Path: pm/features/feature-{ID}.md.\n\n## Investigation Result 1\n{result1}\n\n## Investigation Result 2\n{result2}\n\n## Investigation Result 3\n{result3}")
      - Writes `_out/tmp/consensus-synthesis-{ID}.md` (full consensus analysis with agreement matrix)
      - Edits feature-{ID}.md: Background + Root Cause + Feasibility + Dependencies + all investigation sections
      - Adds `<!-- fc-phase-1-completed -->` marker before `## Background`
      - Does NOT add `<!-- fc-phase-2-completed -->` (Round 2 vote required)
   f. If ERR/INSUFFICIENT → STOP, report to user
3. If resume_from <= 2: **Phase 2 — Consensus Review (Round 2)**
   a. Read synthesized Background + Root Cause + all investigation sections from feature-{ID}.md
   b. Construct review prompt from **Review Prompt Template** (see below), embedding synthesized content
   c. Launch 3 reviewer Tasks in parallel with identical prompt:
      ```
      Task(subagent_type: "deep-explorer", model: "sonnet", prompt: review_prompt)  // ×3 in single message
      ```
      **Model**: sonnet, NOT opus. Round 2 is verification (read synthesis → check code → vote GO/NO-GO), not open-ended investigation. Evidence: Round 2 checks are 5 structured items (factual accuracy, root cause validity, coverage gaps, conclusion alignment, feasibility) — all verifiable by code-reading, not creative reasoning. Saves 3 opus calls per /fc.
      **CRITICAL: Do NOT use `run_in_background: true`. All 3 Tasks MUST be synchronous calls in a single message.**
   d. Parse each result for `GO` or `NO-GO` verdict (search for `### Verdict` section)
   e. Tally votes and apply gate:
      - **3/3 GO (sonnet)** → Escalate: 1x opus verification
        ```
        Task(subagent_type: "deep-explorer", prompt: review_prompt)  // opus (frontmatter default)
        ```
        - opus GO → Add `<!-- fc-phase-2-completed -->` marker, proceed to Phase 3
        - opus NO-GO → Task(consensus-synthesizer, model: "sonnet", mode: "revision", feedback: opus_nogo) → Add marker, proceed to Phase 3
      - **2/3 GO** → Task(consensus-synthesizer, model: "sonnet", mode: revision) with NO-GO reviewer's feedback → micro-revision → Add marker, proceed to Phase 3
      - **1/3 or 0/3 GO** → Re-dispatch: Task(consensus-synthesizer, mode: revision) with ALL NO-GO feedback → re-run Round 2 (steps 3a-3d)
        - If retry achieves 2/3+ GO → micro-revision if needed → Add marker, proceed to Phase 3
        - If retry still ≤1/3 GO → STOP, present all feedback to user via AskUserQuestion
   f. If any reviewer flags NOT_FEASIBLE → additional STOP condition, report to user
4. If resume_from <= 3: Task(ac-designer) → Acceptance Criteria (table + details)
5. If resume_from <= 4: Task(tech-designer) → Technical Design (to satisfy ACs)
5b. **Upstream Issue Gate** (after tech-designer):
    - Read `### Upstream Issues` section from Technical Design
    - IF non-empty: Orchestrator dispatches micro-revisions:
      - AC gaps → re-dispatch ac-designer with specific fix instructions
      - Constraint gaps → orchestrator edits AC Design Constraints directly
      - Interface API gaps → orchestrator adds to Mandatory Handoffs + AC Design Constraints
    - IF empty: proceed
6. If resume_from <= 5: Task(wbs-generator) → Tasks + Implementation Contract
7. If resume_from <= 6: Task(quality-fixer, model: "sonnet") → Quality Auto-Fix (feature-quality checklist)
7b. **AC Structural Lint Gate** (always execute after quality-fixer)
    ```bash
    python src/tools/python/ac_ops.py ac-check {ID}
    ```
    - Exit 0 → Proceed to step 8
    - Exit 1 → **STOP, report raw output to user verbatim.** Do NOT interpret or dismiss errors. Do NOT promote to [PROPOSED].
8. Task(feature-validator) → Validation (always execute)
8b. **Validation Gate**
    - Parse feature-validator output for `VALIDATION: PASS` or `VALIDATION: FAIL`
    - IF `VALIDATION: PASS` OR only `[minor]` issues:
        → Proceed to step 9
    - IF `VALIDATION: FAIL` with `[critical]` or `[major]` issues:
        → Identify the responsible agent for each issue using this routing table:
          - V2 (AC gap) → re-dispatch ac-designer
          - V3 (matcher quality) → re-dispatch ac-designer
          - V4 (source mismatch) → re-dispatch tech-designer
          - V5 (cross-ref: AC#↔Task mapping) → re-dispatch wbs-generator
          - V5 (cross-ref: Goal↔AC mapping) → re-dispatch ac-designer
          - Other (AC format, Task coverage, etc.) → identify responsible agent from context
        → Re-dispatch the responsible agent to fix the specific issues
        → Re-run feature-validator (1 retry only)
        → IF still FAIL: STOP, report issues to user, do NOT promote to [PROPOSED]
        → IF now PASS: Proceed to step 9
9. Update status and index:
   ```bash
   python src/tools/python/feature-status.py set {ID} PROPOSED
   ```
   Note: If feature is new and not yet in index-features.md, manually add the row first, then run the tool.
10. Report completion

## Agent Responsibilities

| Agent | Model | Count | Edits feature file | Output Sections | Marker |
|-------|:-----:|:-----:|:------------------:|-----------------|--------|
| deep-explorer (Round 1) | opus | ×3 parallel | No | Investigation report (structured text) | - |
| consensus-synthesizer | opus | ×1 | Yes | Background, Root Cause, Feasibility, Dependencies, Impact, Constraints, Risks | `<!-- fc-phase-1-completed -->` |
| deep-explorer (Round 2 screen) | sonnet | ×3 parallel | No | GO/NO-GO verdict + rationale | - |
| deep-explorer (Round 2 verify) | opus | ×0~1 (only if 3/3 GO) | No | GO/NO-GO verification | - |
| consensus-synthesizer (revision) | sonnet | ×0~1 | Yes | Micro-revision of flagged sections | - |
| Orchestrator | - | - | Yes (marker only) | - | `<!-- fc-phase-2-completed -->` |
| ac-designer | opus | ×1 | Yes | Acceptance Criteria (Philosophy Derivation, AC Table, AC Details) | `<!-- fc-phase-3-completed -->` |
| tech-designer | sonnet | ×1 | Yes | Technical Design (Approach, AC Coverage, Key Decisions, Upstream Issues) | `<!-- fc-phase-4-completed -->` |
| wbs-generator | sonnet | ×1 | Yes | Tasks, Implementation Contract, Mandatory Handoffs, Execution Log, Links | `<!-- fc-phase-5-completed -->` |
| quality-fixer | sonnet | ×1 | Yes | (edits existing sections for quality compliance) | `<!-- fc-phase-6-completed -->` |
| feature-validator | sonnet | ×1 | No | Text summary (PASS/FAIL) | - |

**CRITICAL**: Each agent MUST write its marker immediately before its first section header.

**Section Structure SSOT**: All agents MUST read `pm/reference/feature-template.md` and follow the section structure defined there. Agent `.md` files contain semantic rules only; structural definitions live in the template.

**Cost Profile**: Phase 1-2 uses 5-6 opus + 3 sonnet calls on happy path (Round 2: 3x sonnet screen + 1x opus verify on 3/3 GO), up to 9 opus on retry. When Round 2 has NO-GO votes, opus verification is skipped (saving 1 opus call). Phase 5b (Upstream Issue Gate) adds 0-1 ac-designer re-dispatch when tech-designer flags upstream issues. Phase 6 (quality-fixer) uses sonnet for semantic depth (haiku insufficient for V2/V3 validation). Phase 7 (feature-validator) uses sonnet for semantic depth. Total wall clock: ~5 sequential steps (Round 1 parallel → synthesis → Round 2 parallel → ac/tech/wbs → quality-fixer → validator), +2 on retry. Investment here reduces FL iterations significantly.

## Section Flow (TDD-compliant, Consensus)

```
                    ┌─ explorer-1 ─┐              ┌─ explorer-1 ─┐                                    ┌──────────────┐
Feature Context   → ├─ explorer-2 ─┤→ synthesizer →├─ explorer-2 ─┤→ gate → ac-designer → tech-designer →│Upstream Gate │→ wbs-generator → quality-fixer → validator
(Deviation/Review)  └─ explorer-3 ─┘    合成+編集   └─ explorer-3 ─┘  投票     完成定義        設計       └──────────────┘   作業分解       品質自動修正      検証
                      Round 1                         Round 2                                    ↓ AC micro-revision
                    (独立調査×3)     (合意形成)     (審議投票×3)    (2/3GO)  (何を達成)    (ACを満たす方法)  (上流修正)    (どう作業)     (パターン修正)
```

**TDD Principle**: AC (完成定義) を先に定義し、それを満たす Design を後で作成

**Consensus Principle**: Round 1 (発散: 独立調査) → 合成 (収束) → Round 2 (検証投票) → Gate (2/3合意)

## Consensus Prompt Templates

### Investigation Prompt Template (Round 1)

Used by 3 deep-explorers in parallel. Orchestrator fills `{placeholders}`.

```markdown
You are independently investigating Feature F{ID} as part of a 3-person consensus review.
Your investigation is INDEPENDENT — do not assume what others will find.

## Feature Context

{Deviation Context OR Review Context section from feature file — include whichever exists}

{Existing Background/Philosophy if any}

## Baseline Data (pre-collected by orchestrator)

{git log output, relevant file listings, etc.}

## Your Task

Investigate the codebase thoroughly and produce a structured report.

### Required Sections

1. **Root Cause Hypotheses** (minimum 2, ranked by confidence)
   - Each with Why Chain (5 Whys) and file:line evidence
2. **Evidence Log** (file:line references for each finding)
3. **Affected Files** (files requiring changes, with change type)
4. **Related Features** (search index-features.md — pm/index-features.md)
5. **Feasibility Assessment** (FEASIBLE / NEEDS_REVISION / NOT_FEASIBLE with evidence)
6. **Dependencies** (predecessor/related features, external deps)
7. **Technical Constraints** (blocking limitations with source)
8. **Risks** (likelihood × impact)
9. **AC Design Constraints** (constraints that must be respected when defining ACs)
10. **Interface Dependency Scan** (MANDATORY for erb/engine types): For each existing Era.Core interface that the migrated code will depend on, list the interface file path and verify which methods exist vs. which methods are needed but missing. This prevents compilation-blocking gaps from reaching downstream phases.
11. **Sibling Feature Call Chain Analysis** (MANDATORY for erb/engine types): Read index-features.md to identify sibling features in the same Phase. For each sibling's ERB files, search for CALL/JUMP/GOTO/CALLFORM/JUMPFORM references FROM this feature's files TO sibling files and vice versa. When a hard call dependency exists (Feature A's code CALLs a function defined in Feature B's scope), the dependency Type MUST be Predecessor (not Related). This prevents undeclared blocking dependencies between sibling features.

## Output Format (STRICT — max 600 lines for erb/engine types, 400 lines for others)

# Investigation Report: F{ID}

## Root Cause Hypotheses

### Hypothesis 1 (PRIMARY)
- **Confidence**: HIGH/MEDIUM/LOW
- **Statement**: {1-2 sentence root cause}
- **Why Chain**:
  1. Symptom: {observed} — Evidence: {file:line}
  2. Why: {cause} — Evidence: {file:line}
  3. Why: {cause} — Evidence: {file:line}
  4. Why: {cause} — Evidence: {file:line}
  5. Why: {root cause} — Evidence: {file:line}

### Hypothesis 2 (ALTERNATIVE)
(same format)

## Evidence Log
| # | Type | Location | Finding |
|:-:|------|----------|---------|

## Affected Files
| File | Change Type | Description |
|------|-------------|-------------|

## Related Features
| Feature | Status | Relationship |
|---------|--------|--------------|

## Feasibility Assessment
| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
**Verdict**: FEASIBLE / NEEDS_REVISION / NOT_FEASIBLE

## Dependencies
| Type | Feature/Component | Status | Description |
|------|-------------------|--------|-------------|

## Technical Constraints
| Constraint | Source | Impact |
|------------|--------|--------|

## Risks
| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|

## AC Design Constraints
| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|

## Interface Dependency Scan
<!-- MANDATORY for erb/engine types. Omit for kojo/research/infra. -->

| Interface | File | Method Needed | Exists? | Gap Description |
|-----------|------|---------------|:-------:|-----------------|

## Sibling Feature Call Chain Analysis
<!-- MANDATORY for erb/engine types. Omit for kojo/research/infra. -->
<!-- Check CALL/JUMP/GOTO/CALLFORM/JUMPFORM between this feature's files and sibling features' files -->
<!-- When hard call dependency found: recommend Predecessor type, not Related -->

| Direction | This Feature File:Line | Sibling Feature | Sibling File:Function | Call Type | Dependency Recommendation |
|-----------|------------------------|-----------------|----------------------|-----------|--------------------------|
| OUT (this→sibling) | {file:line} | F{ID} | {file:function} | CALL/JUMP | Predecessor on F{ID} |
| IN (sibling→this) | {file:function} | F{ID} | {file:line} | CALL | Successor of F{ID} |

## Key Observations
{Anything unexpected or noteworthy that other investigators should know about}
```

### Review Prompt Template (Round 2)

Used by 3 deep-explorers in parallel. Orchestrator fills `{placeholders}`.

```markdown
You are reviewing the synthesized investigation for Feature F{ID} as part of a 3-person consensus vote.
Your review is INDEPENDENT — form your own judgment by verifying claims against actual code.

## Synthesized Investigation (to review)

{All investigation sections from feature file: Background, Root Cause Analysis,
 Feasibility, Dependencies, Impact, Constraints, Risks, AC Design Constraints}

## Your Task

Review the synthesis for quality and correctness. For each check, **verify by reading actual code** — do not trust the synthesis blindly.

### Checks

1. **事実の正確性**: Do code references (file:line) match actual code content?
2. **根本原因の妥当性**: Is the 5 Whys chain logically sound? Any leaps in reasoning?
3. **見落とし**: Are there important code paths or files not covered?
4. **結論の整合性**: Do Problem/Goal align with Root Cause conclusion?
5. **実現可能性**: Is the Feasibility assessment realistic given code evidence?

## Output Format (STRICT)

# Review: F{ID}

## Checks

| # | Check | Result | Evidence |
|:-:|-------|:------:|----------|
| 1 | 事実の正確性 | PASS/FAIL | {specific file:line verification} |
| 2 | 根本原因の妥当性 | PASS/FAIL | {logical assessment} |
| 3 | 見落とし | PASS/FAIL | {missed items, or "None found"} |
| 4 | 結論の整合性 | PASS/FAIL | {alignment assessment} |
| 5 | 実現可能性 | PASS/FAIL | {feasibility verification} |

### Verdict

GO / NO-GO

### Rationale
{If NO-GO: specific issues that MUST be fixed before proceeding, with file:line evidence}
{If GO: brief confirmation of synthesis quality}
```

## Vote Tallying Logic

Orchestrator performs after collecting 3 Round 2 results:

```
votes = [parse_verdict(result) for result in [r1, r2, r3]]
go_count = votes.count("GO")
nogo_feedback = [result for result, vote in zip(results, votes) if vote == "NO-GO"]

if go_count == 3:
    # Unanimous — proceed
    write_phase_2_marker(feature_file)

elif go_count == 2:
    # Majority — micro-revision with minority feedback
    Task(consensus-synthesizer, model: "sonnet", mode: "revision", feedback: nogo_feedback[0])
    write_phase_2_marker(feature_file)

elif go_count <= 1:
    # No consensus — re-dispatch with all feedback (1 retry)
    Task(consensus-synthesizer, model: "sonnet", mode: "revision", feedback: nogo_feedback)  # ALL NO-GO feedback
    # Re-run Round 2 (also sonnet — see Round 2 model note above)
    retry_votes = run_round_2(feature_file)
    retry_go = retry_votes.count("GO")

    if retry_go >= 2:
        # Recovery succeeded
        if retry_go == 2:
            Task(consensus-synthesizer, model: "sonnet", mode: "revision", feedback: retry_nogo[0])  # micro-fix
        write_phase_2_marker(feature_file)
    else:
        # Still no consensus after retry — STOP
        AskUserQuestion(
            question: "再ディスパッチ後も合意に至りませんでした。どうしますか？",
            header: "合議結果",
            options: [
                { label: "再調査", description: "Round 1からやり直し (Phase 1-2 リセット)" },
                { label: "手動修正", description: "指摘を踏まえてユーザーが修正後、/fc を再実行" },
                { label: "続行", description: "現状のまま Phase 3 へ進む (合議をスキップ)" }
            ]
        )
```

## User Output Templates (日本語)

### Phase進捗報告

```
## /fc {ID} 進行中

| Phase | 状態 |
|:-----:|:----:|
| 合議調査 (Round 1) | ✅ 完了 |
| 合議審議 (Round 2) | ✅ 完了 (3/3 GO) |
| AC設計 | 🔄 実行中 |
| 技術設計 | ⏳ 待機 |
| WBS生成 | ⏳ 待機 |
| 品質自動修正 | ⏳ 待機 |
| 検証 | ⏳ 待機 |
```

### Consensus報告 (Round 2 結果)

```
### 合議結果

| Reviewer | Verdict | 主な所見 |
|:--------:|:-------:|----------|
| Explorer 1 | GO | {1行サマリー} |
| Explorer 2 | GO | {1行サマリー} |
| Explorer 3 | NO-GO | {1行サマリー} |

**結果**: 2/3 GO → 少数意見を反映して微修正後、続行
```

### STOP報告 (NEEDS_REVISION / NOT_FEASIBLE / 合議不成立)

First, output the blocking issue report as text:

```
## /fc {ID} 停止 - {理由}

**Feature**: F{ID} - {タイトル}

### ブロッキング問題

{問題の説明}

| 項目 | 詳細 |
|------|------|
| ... | ... |
```

Then, use AskUserQuestion tool to present options (enables Auto Handoff in dashboard):

```
AskUserQuestion(
  question: "どのように進めますか？",
  header: "解決方法",
  options: [
    { label: "オプションA", description: "{推奨オプションの説明}" },
    { label: "オプションB", description: "{代替オプションの説明}" },
    ...additional options as needed (2-4 total)
  ]
)
```

**CRITICAL**: Do NOT print options as text table. Use AskUserQuestion so the dashboard can detect user input is needed and auto-handoff to terminal.

### 完了報告

```
## /fc {ID} 完了

**Feature**: F{ID} - {タイトル}

### 生成セクション

| セクション | 状態 |
|------------|:----:|
| Background (Consensus Investigation) | ✅ |
| Root Cause Analysis (Consensus Reviewed) | ✅ |
| Acceptance Criteria | ✅ |
| Technical Design | ✅ |
| Tasks | ✅ |
| Quality Auto-Fix | ✅ |

### 合議結果

| Round | 結果 |
|:-----:|:----:|
| Round 1 (調査) | 3名完了、合成済み |
| Round 2 (審議) | {X}/3 GO |

### 検証結果

{feature-validatorの結果サマリー}

### ステータス更新

- feature-{ID}.md: `[DRAFT]` → `[PROPOSED]`
- index-features.md: エントリ追加/更新済み

次のステップ: `/fl {ID}` でレビュー
```
