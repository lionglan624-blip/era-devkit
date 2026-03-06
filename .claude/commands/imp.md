---
description: "Improvement analysis for feature lifecycle sessions"
argument-hint: "<feature-id>"
---

# /imp Command - Improvement Analysis

Analyze past FC/FL/RUN sessions for a feature and propose improvements.

## Usage
/imp {ID}

## Language
**Output MUST be in Japanese.** Internal reasoning in English is OK.

## Procedure

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

### Step 5: Opus Review of Proposed Changes

**MANDATORY: All proposals must be reviewed before finalization.**

Dispatch an opus-model agent to review the Step 3-4 output against the actual workflow files:

```
Agent(subagent_type: "deep-explorer", model: opus, prompt: """
Review the /imp {ID} improvement proposals against the actual workflow implementation.

For each proposed change:
1. Read the target file (skill/agent/script) at the referenced path+line
2. Verify the "Current behavior" claim is accurate
3. Check if the "Gap" is real or already handled by another mechanism
4. Assess whether the "Proposed change" would cause side effects on other features/types
5. Flag any proposal that:
   - Mischaracterizes what the current code does
   - Would break existing workflow for other feature types
   - Duplicates functionality that already exists elsewhere
   - Has insufficient evidence (single-feature observation presented as systemic)
6. For REJECT verdicts, verify the rejection evidence:
   - If rejection claims "already implemented" → Read the target file and confirm actual implementation exists (not just pseudocode/documentation)
   - If rejection claims "low frequency" or includes numeric data → Cross-check against available logs/metrics (e.g. .claude/hooks/post-hook.log)
   - If rejection claims "single-feature observation" → Check if the same pattern appeared in previous /imp runs (grep Improvement Log sections)
   - A reject verdict with unverified claims must be changed to "revise" with investigation action items

Output: JSON array of review verdicts per proposal:
[{"proposal": "...", "verdict": "accept|revise|reject", "reason": "...", "evidence_checked": "..." (for reject), "revision": "..." (if revise)}]
""")
```

Incorporate review results into the final report:
- **accept**: Include as-is
- **revise**: Update proposal per reviewer feedback, mark as `[revised]`
- **reject**: Remove from action items, add to "Rejected Proposals" appendix with reason

### Step 6: User Confirmation and Apply

Present the reviewed action items to the user with a single confirmation prompt:

```
AskUserQuestion: "Opusレビュー済みの改善提案を対象ファイルに反映しますか？"
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
