---
description: "Improvement analysis for feature lifecycle sessions"
argument-hint: "<feature-id | imp | (empty)>"
---

# /imp Command - Improvement Analysis

Analyze past FC/FL/RUN sessions for a feature and propose improvements.

## Usage

| Invocation | Mode | Description |
|-----------|------|-------------|
| `/imp {ID}` | Per-feature | Analyze FC/FL/RUN sessions for a specific feature |
| `/imp` | Cross-review | Cross-feature横断レビュー: 全Improvement Logを集約分析 |
| `/imp imp` | Self-improvement | impコマンド自体の改良提案 |

## Language
**Output MUST be in Japanese.** Internal reasoning in English is OK.

## Mode Router

If `$ARGUMENTS` is empty → go to **Cross-Review Mode**.
If `$ARGUMENTS` is `imp` → go to **Self-Improvement Mode**.
Otherwise → go to **Per-Feature Mode** (Step 1).

---

## Cross-Review Mode

### CR-1: Run Cross-Analysis Script

```bash
python src/tools/python/imp-analyzer.py --cross
```

### CR-2: Synthesize Cross-Feature Report

Using the script output (aggregated metrics from all Improvement Logs), produce a report covering:

#### 1. ホットスポット分析 (Hotspot Analysis)

From the target file frequency table:
- Identify files modified across 3+ features → these are systemic hotspots
- For each hotspot: read the current file and summarize what kind of changes keep recurring
- Propose root-cause fixes that would eliminate the need for repeated /imp patches

#### 2. 却下パターン改善 (Rejection Pattern Improvement)

From the rejection analysis:
- Identify the most common rejection categories
- For "Already implemented" rejections: the /imp analysis phase failed to detect existing functionality → propose Step 2.5 improvements
- For "Insufficient evidence" rejections: single-feature observations were proposed as systemic → propose evidence threshold rules

#### 3. 改善効果追跡 (Improvement Effect Tracking)

For applied improvements in earlier features:
- Did the same FL fix category recur in later features?
- If yes → the improvement was insufficient; propose a stronger fix
- If no → the improvement worked; note as validated

#### 4. ワークフロー傾向 (Workflow Trends)

From the timeline:
- Is the applied rate increasing over time? (learning curve)
- Is the rejection rate decreasing? (proposal quality improving)
- Are new categories emerging?

### CR-3: Action Items

End with **3-5 concrete next actions** prioritized by cross-feature impact.

### CR-4: Review of Cross-Review Proposals (Sonnet-First Escalation)

**MANDATORY: All proposals must be reviewed before finalization.**

**Escalation Pattern**: Sonnet reviews first. If sonnet finds 0 issues → 1x opus verification before proceeding. If sonnet finds issues → fix without opus (sonnet sufficient for identifying concrete problems).

```
sonnet_result = Agent(subagent_type: "general-purpose", model: "sonnet", prompt: """
Review the /imp cross-review improvement proposals against the actual workflow implementation.

For each proposed action item:
1. Read the target file (skill/agent/script) at the referenced path
2. Verify the "root cause" claim is accurate — does the hotspot pattern actually exist as described?
3. Check if the proposed fix is already handled by another mechanism
4. Assess whether the change would cause side effects on per-feature /imp mode or other workflows
5. For rejection-pattern improvements: verify the rejection counts match actual Improvement Log data
6. For effect-tracking claims: spot-check 1-2 features to confirm recurrence/non-recurrence
7. Flag any proposal that:
   - Is based on insufficient cross-feature evidence (fewer than 3 features)
   - Would increase /imp execution cost without proportional benefit
   - Duplicates functionality already present in per-feature mode (Step 5)
   - Mischaracterizes what the target file currently does

Output: JSON array of review verdicts per proposal:
[{"proposal": "...", "verdict": "accept|revise|reject", "reason": "...", "evidence_checked": "..." (for reject), "revision": "..." (if revise)}]
""")

# ESCALATION: If sonnet found 0 reject/revise → opus verification
IF all sonnet_result verdicts are "accept":
    opus_result = Agent(subagent_type: "deep-explorer", model: "opus", prompt: """
    Sonnet reviewed all proposals as "accept". Verify this is correct — check for subtle issues sonnet may have missed.
    Proposals: {proposals}. Sonnet verdicts: {sonnet_result}.
    Output: JSON array with same format. Only flag genuine issues.
    """)
    result = opus_result
ELSE:
    result = sonnet_result
```
```

### CR-4.5: Verify Review Results

**MANDATORY: Orchestrator must verify review verdicts before presenting to user.**

For each verdict from CR-4:

1. **accept verdicts**: Briefly confirm the target file exists and the proposal is actionable (no deep re-read needed)
2. **revise verdicts**: Re-read the target file at the referenced path. Verify:
   - The revision rationale is factually correct (not based on assumed file contents)
   - The revised proposal doesn't conflict with existing functionality
   - If the revision changes the target file or scope, confirm the new target is valid
3. **reject verdicts**: Spot-check the rejection evidence:
   - If "already implemented" → Grep/Read the cited location to confirm the implementation exists
   - If "insufficient evidence" → Verify the feature count claim is accurate
   - If evidence is unverifiable or incorrect → override to `revise` with investigation notes

Present verified results as a table with columns: `#`, `提案`, `レビュー判定`, `検証結果`, `最終判定`.

If any verdict was overridden, explain why.

### CR-5: User Confirmation

```
AskUserQuestion: "検証済みの横断改善提案を適用しますか？"
Options: "Yes — apply all accepted/revised proposals", "No — report only"
```

If Yes: apply changes. If No: report only.

---

## Self-Improvement Mode

### SI-1: Run Self-Analysis Script

```bash
python src/tools/python/imp-analyzer.py imp
```

### SI-2: Read Current /imp Implementation

```
Read(.claude/commands/imp.md)
Read(src/tools/python/imp-analyzer.py) — focus on build_parser(), main(), generate functions
```

### SI-3: Analyze /imp Effectiveness

Using the script's Section 6 (self-analysis metrics) plus the cross-feature data:

#### 1. 提案精度分析 (Proposal Precision)

- applied率 vs rejected率の推移
- 却下理由の分類 → どのStep（Step 2.5 skill読み込み, Step 5 レビュー）で防げたか
- "Already implemented" 却下を減らすための具体的チェック追加

#### 2. /imp スクリプト改善 (Script Improvements)

- imp-analyzer.py の出力で不足している情報
- パース精度の問題（entry_re でキャプチャ漏れがないか）
- 新しい分析セクションの提案

#### 3. /imp コマンド改善 (Command Improvements)

- Step間のフロー最適化
- レビュー（Step 5）の精度向上策
- 反復パターンの自動検出ルール

#### 4. ワークフロー統合 (Workflow Integration)

- /imp 結果を次回FC/FL/RUNに自動反映する仕組み
- Improvement Log → FC予防ルールへの自動フィードバックループ

### SI-4: Action Items

End with **3-5 concrete changes to /imp itself**, each with target file and expected impact.

### SI-5: Review of Self-Improvement Proposals (Sonnet-First Escalation)

**MANDATORY: All proposals must be reviewed before finalization.**

**Escalation Pattern**: Sonnet reviews first. If sonnet finds 0 issues → 1x opus verification before proceeding.

```
sonnet_result = Agent(subagent_type: "general-purpose", model: "sonnet", prompt: """
Review the /imp imp self-improvement proposals against the actual implementation.

Context: These proposals target the /imp command itself (.claude/commands/imp.md) and/or
its analysis script (src/tools/python/imp-analyzer.py).

For each proposed change:
1. Read the target file at the referenced path
2. Verify the "Current behavior" claim is accurate
3. Check if the "Gap" is real or already handled
4. Assess whether the change would break existing /imp {ID} per-feature mode
5. For proposals targeting imp-analyzer.py: verify the Python change is syntactically correct and doesn't break --cross/--self/per-feature modes
6. For proposals targeting imp.md: verify the Step change doesn't conflict with Per-Feature Mode or Cross-Review Mode
7. Flag any proposal that:
   - Is based on insufficient data (e.g. only 1-2 /imp runs observed)
   - Would increase /imp execution cost without proportional benefit
   - Duplicates checks already performed by review in Per-Feature Mode (Step 5)
   - Would cause regression in existing modes

Cross-check against the Improvement Log data:
- Verify rejection rate claims against actual counts in the script output
- Verify hotspot claims against the target file frequency table
- Check if proposed "new patterns" are actually already captured

Output: JSON array of review verdicts per proposal:
[{"proposal": "...", "verdict": "accept|revise|reject", "reason": "...", "evidence_checked": "..." (for reject), "revision": "..." (if revise)}]
""")

# ESCALATION: If sonnet found 0 reject/revise → opus verification
IF all sonnet_result verdicts are "accept":
    opus_result = Agent(subagent_type: "deep-explorer", model: "opus", prompt: """
    Sonnet reviewed all self-improvement proposals as "accept". Verify this is correct.
    Proposals: {proposals}. Sonnet verdicts: {sonnet_result}.
    Output: JSON array with same format. Only flag genuine issues.
    """)
    result = opus_result
ELSE:
    result = sonnet_result
```
```

### SI-5.5: Verify Review Results

**MANDATORY: Orchestrator must verify review verdicts before presenting to user.**

For each verdict from SI-5:

1. **accept verdicts**: Briefly confirm the target file exists and the proposal is actionable
2. **revise verdicts**: Re-read the target file at the referenced path. Verify:
   - The revision rationale is factually correct
   - The revised proposal doesn't break existing /imp per-feature or cross-review modes
   - If the revision changes the target file or scope, confirm the new target is valid
3. **reject verdicts**: Spot-check the rejection evidence:
   - If "already implemented" → Grep/Read the cited location to confirm
   - If "insufficient evidence" → Verify the data claim is accurate
   - If evidence is unverifiable or incorrect → override to `revise` with investigation notes

Present verified results as a table with columns: `#`, `提案`, `レビュー判定`, `検証結果`, `最終判定`.

If any verdict was overridden, explain why.

### SI-6: User Confirmation

```
AskUserQuestion: "検証済みの/imp自己改善提案を適用しますか？"
Options: "Yes — apply all accepted/revised proposals", "No — report only"
```

If Yes: apply changes to imp.md and/or imp-analyzer.py. If No: report only.

---

## Per-Feature Mode

### Step 1: Run Analysis Script

```bash
python src/tools/python/imp-analyzer.py {ID}
```

If the script fails or finds no sessions, report the error and stop.

### Step 2: Read Feature Context

```
Read(pm/features/feature-{ID}.md)
```

Focus on: `## Review Notes` (FL fix history), `## Execution Log` (RUN deviations), `## AC Definition Table` (scope).

### Step 2.5: Read Relevant Workflow Skills

**CRITICAL: Proposals MUST be grounded in actual skill/agent implementations, not assumptions.**

Based on the FL fix categories from Step 1, identify and read the workflow files responsible for catching those issues. At minimum:

- FC phases that should have prevented the fixes: read the relevant skill files (e.g., `.claude/skills/run-workflow/PHASE-*.md`, `.claude/skills/fl-workflow/PHASE-*.md`)
- Agent prompts referenced in fix categories (ac-designer, quality-fixer, ac-task-aligner, etc.)
- Any checker/validator scripts mentioned in the workflow

For each improvement proposal, explicitly state:
- **Current behavior**: What the skill/agent actually does now (with file path + line reference)
- **Gap**: What it doesn't do that caused the FL fix
- **Proposed change**: Concrete diff or rule addition

Do NOT propose improvements based on assumed behavior. If a skill already handles something, say so.

### Step 3: Synthesize Report

Using the script output (raw metrics) plus the **actual content of workflow skills read in Step 2.5**, produce a report covering **4 areas**:

#### 1. FC予防分析 (FC Prevention)

For each FL fix category, explain:
- **Why** FC didn't catch it (root cause)
- **How** to prevent it (concrete FC rule or check to add)
- Whether the fix is a systemic pattern or one-off

Present as a table + narrative summary.

#### 2. ツール・スクリプト最適化 (Tool/Script Optimization)

From tool usage patterns:
- Identify highly-repeated tool calls that could be automated
- Propose improvements to existing Python tools (session-search.py, ac_ops.py, feature-status.py, etc.)
- Suggest new utility scripts if a pattern appears frequently
- Flag unnecessary agent dispatches or model upgrades

#### 3. ワークフロー改善 (Workflow Improvements)

From FL iteration counts and phase distribution:
- Identify which FL phases generate most fixes
- Propose workflow changes (new FC checks, FL phase reordering, skip rules)
- Analyze RUN deviations for process gaps
- Compare this feature's metrics to expectations

#### 4. 手間削減 (Tedium Reduction)

From tedium indicators:
- List the most time-consuming repeated operations
- Propose concrete automations (scripts, hooks, caching)
- Estimate impact (how many tool calls would be saved)

#### 5. エラー・Hook分析 (Error & Hook Analysis)

From the error/hook/denial data in section 5 of the script output:
- Identify recurring hook errors (pre-commit failures, commit-msg hook issues)
- Analyze tool errors by tool name — which tools fail most?
- Review tool denials — are permission settings too restrictive or are wrong tools being called?
- For hook errors: propose pre-validation steps or hook configuration fixes
- For tool errors: propose workflow changes to avoid repeated failures
- For tool denials: propose permission mode adjustments or alternative approaches

#### 6. CLIツール活用監査 (Tool Usage Audit)

From tool usage patterns:
- CLIツール（feature-status.py, ac_ops.py, session-search.py）が使われたか検出
- 手動代替パターン（Status grep, AC手動Edit）との比較
- 未使用ツールがあればワークフロースキルへの使用ルール追加を提案
- ツールが使われなかった理由の分析（認知度不足 vs 使いにくさ）

### Step 4: Action Items

End with a prioritized list of **3-5 concrete next actions**, each with:
- What to do
- Where to implement (file path or skill name)
- Expected impact (high/medium/low)

### Step 5: Review of Proposed Changes

**MANDATORY: All proposals must be reviewed before finalization.**

Dispatch an opus-model agent to review the Step 3-4 output against the actual workflow files:

```
Agent(subagent_type: "deep-explorer", model: "opus", prompt: """
Review the /imp {ID} improvement proposals against the actual workflow implementation.

For each proposed change:
1. Read the target file (skill/agent/script) at the referenced path+line
2. Grep the target file for keywords from the proposal description to check for existing similar rules
3. Verify the "Current behavior" claim is accurate
4. Check if the "Gap" is real or already handled by another mechanism
5. Assess whether the "Proposed change" would cause side effects on other features/types
6. Flag any proposal that:
   - Mischaracterizes what the current code does
   - Would break existing workflow for other feature types
   - Duplicates functionality that already exists elsewhere
   - Has insufficient evidence (single-feature observation presented as systemic)
7. For REJECT verdicts, verify the rejection evidence:
   - If rejection claims "already implemented" → Read the target file and confirm actual implementation exists (not just pseudocode/documentation)
   - If rejection claims "low frequency" or includes numeric data → Cross-check against available logs/metrics (e.g. .claude/hooks/post-hook.log)
   - If rejection claims "single-feature observation" → Check if the same pattern appeared in previous /imp runs (grep Improvement Log sections)
   - A reject verdict with unverified claims must be changed to "revise" with investigation action items

Output: JSON array of review verdicts per proposal:
[{"proposal": "...", "verdict": "accept|revise|reject", "reason": "...", "evidence_checked": "..." (for reject), "revision": "..." (if revise)}]
""")
```

### Step 5.5: Verify Review Results

**MANDATORY: Orchestrator must verify review verdicts before presenting to user.**

For each verdict from Step 5:

1. **accept verdicts**: Briefly confirm the target file exists and the proposal is actionable
2. **revise verdicts**: Re-read the target file at the referenced path. Verify:
   - The revision rationale is factually correct (not based on assumed file contents)
   - The revised proposal doesn't conflict with existing functionality
   - If the revision changes the target file or scope, confirm the new target is valid
3. **reject verdicts**: Spot-check the rejection evidence:
   - If "already implemented" → Grep/Read the cited location to confirm the implementation exists
   - If "insufficient evidence" → Verify the feature count claim is accurate
   - If evidence is unverifiable or incorrect → override to `revise` with investigation notes

Present verified results as a table with columns: `#`, `提案`, `レビュー判定`, `検証結果`, `最終判定`.

If any verdict was overridden, explain why.

### Step 6: User Confirmation and Apply

Present the reviewed action items to the user with a single confirmation prompt:

```
AskUserQuestion: "レビュー済みの改善提案を対象ファイルに反映しますか？"
Options: "Yes — apply all accepted/revised proposals", "No — report only"
```

- **Yes**: Apply all `accept` and `revise` proposals by editing the target files (skill/agent/script). Mark applied changes in the report as `[applied]`.
- **No**: Output the report without modifications. User can apply manually later.

### Step 7: Record Improvement Log

**MANDATORY after Step 6.** Record modification results in `pm/features/feature-{ID}.md`.

#### Location

Insert `## Improvement Log` section between `## Review Notes` and `## Links`. If the section already exists (from a previous /imp run), append a new subsection.

#### Format

```markdown
## Improvement Log

### /imp {ID} ({date})
- [applied] {description} → `{target file path}`
- [applied] {description} → `{target file path}`
- [rejected] {description} — {reason}
- [revised] {description} → `{target file path}` ({what was revised})
```

#### Rules

- **Only record modifications**: applied/revised/rejected verdicts from Step 5-6. No analysis narrative.
- **One line per proposal**: verdict tag + what + where (for applied/revised) or why not (for rejected).
- **Multiple runs**: Each `/imp` execution gets its own `### /imp {ID} ({date})` subsection.
- **"No — report only"**: Still record the section, but mark all items as `[proposed]` instead of `[applied]`.

## Output Format

```markdown
## /imp {ID} 分析結果

### セッション概要
(table from script output)

### 1. FC予防分析
(table + narrative)

### 2. ツール・スクリプト最適化
(table + proposals)

### 3. ワークフロー改善
(metrics table + proposals)

### 4. 手間削減
(tedium table + automation proposals)

### 5. エラー・Hook分析
(error/hook/denial tables + proposals)

### 次のアクション
1. [高] ...
2. [中] ...
3. [中] ...
```
