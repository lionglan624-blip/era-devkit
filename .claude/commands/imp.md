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

Output: JSON array of review verdicts per proposal:
[{"proposal": "...", "verdict": "accept|revise|reject", "reason": "...", "revision": "..." (if revise)}]
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

### 次のアクション
1. [高] ...
2. [中] ...
3. [中] ...
```
