# Feature 256: Philosophy Gate for FL

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
FL (/fl) 縺ｯ Feature 縺ｮ蜩∬ｳｪ繧剃ｿ晁ｨｼ縺吶ｋ譛邨ゅご繝ｼ繝医→縺励※讖溯・縺吶∋縺阪・C/Task 縺ｮ蠖｢蠑冗噪謨ｴ蜷域ｧ縺縺代〒縺ｪ縺上・*Philosophy 縺ｮ諢丞袖逧・＃謌・* 繧よ､懆ｨｼ縺吶ｋ蠢・ｦ√′縺ゅｋ縲・

### Problem (Current Issue)
F254 縺ｧ逋ｺ逕溘＠縺溷撫鬘・
- Philosophy: 縲訓hase 8 Summary 繧・**螳悟・莉｣譖ｿ** 縺吶∋縺阪・
- 螳溯｣・ `--progress` 繧ｪ繝励す繝ｧ繝ｳ・亥庄隕門喧繝・・繝ｫ・峨・縺ｿ
- 邨先棡: Philosophy 縺ｮ荳驛ｨ縺励°驕疲・縺励※縺・↑縺・・縺ｫ縲悟ｮ御ｺ・阪→蛻､螳・

**譬ｹ譛ｬ蜴溷屏**:
- feature-reviewer 縺ｯ AC/Task 繧貞渕貅悶↓繝ｬ繝薙Η繝ｼ
- 迴ｾ蝨ｨ縺ｮ AC/Task 縺九ｉ騾・ｮ励☆繧九→縲後％繧後〒蜊∝・縲阪→骭ｯ隕・
- Philosophy 縺ｨ縺ｮ荵夜屬繧呈､懷・縺吶ｋ莉慕ｵ・∩縺後↑縺・

### Goal (What to Achieve)
FL 繝ｫ繝ｼ繝怜ｮ御ｺ・ｾ後↓ **Philosophy Gate** 繧定ｿｽ蜉:
1. Philosophy 縺九ｉ縲悟ｰ主・縺吶∋縺阪ち繧ｹ繧ｯ縲阪ｒ豁｣邂暦ｼ育樟蝨ｨ縺ｮ AC/Task 縺ｫ蠑輔″縺壹ｉ繧後↑縺・ｼ・
2. 蟆主・繧ｿ繧ｹ繧ｯ縺ｨ迴ｾ蝨ｨ繧ｿ繧ｹ繧ｯ繧堤ｪ√″蜷医ｏ縺帙※ Gap 繧呈､懷・
3. Opus 縺・Gap 縺ｫ蟇ｾ縺・ADOPT/DEFER/REJECT 繧貞愛譁ｭ
4. ADOPT 縺ｮ蝣ｴ蜷医・繝ｫ繝ｼ繝玲怙蛻昴↓謌ｻ繧雁・讀懆ｨｼ

### Session Context
- **Origin**: F254 螳御ｺ・凾縺ｮ繝ｬ繝薙Η繝ｼ縺ｧ Philosophy 譛ｪ驕疲・繧堤匱隕・
- **Lesson**: AC/Task 縺九ｉ縺ｮ騾・ｮ励〒縺ｯ縺ｪ縺上￣hilosophy 縺九ｉ縺ｮ豁｣邂励′蠢・ｦ・

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | philosophy-deriver.md 菴懈・ | file | Glob | exists | .claude/agents/philosophy-deriver.md | [x] |
| 2 | deriver 縺・opus 繝｢繝・Ν謖・ｮ・| file | Grep | contains | "model: opus" | [x] |
| 3 | task-comparator.md 菴懈・ | file | Glob | exists | .claude/agents/task-comparator.md | [x] |
| 4 | comparator 縺・haiku 繝｢繝・Ν謖・ｮ・| file | Grep | contains | "model: haiku" | [x] |
| 5 | fl.md 縺ｫ繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 霑ｽ蜉 | file | Grep | contains | "Philosophy Gate" | [x] |
| 6 | 繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 縺・deriver 蜻ｼ縺ｳ蜃ｺ縺・| file | Grep | contains | "philosophy-deriver" | [x] |
| 7 | 繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 縺・comparator 蜻ｼ縺ｳ蜃ｺ縺・| file | Grep | contains | "task-comparator" | [x] |
| 8 | ADOPT 譎・GOTO WHILE 蜀埼幕 | file | Grep | contains | "GOTO WHILE" | [x] |
| 9 | CLAUDE.md 縺ｫ philosophy-deriver 霑ｽ蜉 | file | Grep | contains | "philosophy-deriver" | [x] |
| 10 | CLAUDE.md 縺ｫ task-comparator 霑ｽ蜉 | file | Grep | contains | "task-comparator" | [x] |

### AC Details

**AC1-2**: philosophy-deriver.md
- Model: opus・磯ｫ伜ｺｦ縺ｪ謚ｽ雎｡謗ｨ隲悶′蠢・ｦ・ｼ・
- 雋ｬ蜍・ Philosophy 竊・蟆主・縺吶∋縺阪ち繧ｹ繧ｯ繧貞・謖・
- 迴ｾ蝨ｨ縺ｮ AC/Task 繧定ｦ九↑縺・ｼ医ヰ繧､繧｢繧ｹ蝗樣∩・・

**AC3-4**: task-comparator.md
- Model: haiku・亥腰邏斐↑遯√″蜷医ｏ縺幢ｼ・
- 雋ｬ蜍・ 蟆主・繧ｿ繧ｹ繧ｯ vs 迴ｾ蝨ｨ繧ｿ繧ｹ繧ｯ 竊・Gap 讀懷・

**AC5-8**: fl.md 繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 (Philosophy Gate)
- 繝ｫ繝ｼ繝怜ｮ御ｺ・ｾ鯉ｼ・ero issues 蠕鯉ｼ峨↓螳溯｡・
- Features 縺ｮ縺ｿ・・ommand/agent/skill/doc 縺ｯ繧ｹ繧ｭ繝・・・・
- ADOPT 竊・GOTO WHILE 縺ｧ繝ｫ繝ｼ繝怜・髢・
- DEFER 竊・谿玖ｪｲ鬘後→縺励※險倬鹸縲：L 螳御ｺ・
- REJECT 竊・Gap 縺ｯ蟇ｾ雎｡螟悶：L 螳御ｺ・

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | philosophy-deriver.md 菴懈・ | [x] |
| 2 | 3,4 | task-comparator.md 菴懈・ | [x] |
| 3 | 5,6,7,8 | fl.md 縺ｫ繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 霑ｽ蜉 | [x] |
| 4 | 9,10 | CLAUDE.md Subagent Strategy 縺ｫ譁ｰ繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝郁ｿｽ蜉 | [x] |

<!-- AC:Task Consolidation Rationale:
- Task 1 (AC1+2): Agent file creation includes frontmatter content (single file operation)
- Task 2 (AC3+4): Same rationale as Task 1
- Task 3 (AC5-8): fl.md 繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 is a single logical section with interdependent components
-->

---

## Technical Notes

### philosophy-deriver.md 險ｭ險・

```markdown
---
name: philosophy-deriver
description: Derive required tasks from Philosophy. Model: opus.
model: opus
tools: Read
---

# Philosophy Deriver Agent

## Task
Read Philosophy section and derive what tasks SHOULD exist to achieve it.

## Input
- Feature ID

## Process
1. Read feature-{ID}.md Philosophy section ONLY
2. **CRITICAL**: Do NOT read Goal/AC/Task sections (bias avoidance)
3. Ask: "To fully achieve this Philosophy, what must be done?"
4. List derived tasks with rationale

## Output Format
{
  "philosophy_text": "...",
  "absolute_claims": ["螳悟・", "縺吶∋縺ｦ", etc.],
  "derived_tasks": [
    {"task": "蜿ｯ隕門喧繝・・繝ｫ霑ｽ蜉", "rationale": "騾ｲ謐励ｒ隕九∴繧九ｈ縺・↓縺吶ｋ"},
    {"task": "繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ邨ｱ蜷・, "rationale": "譌｢蟄倥ヤ繝ｼ繝ｫ縺悟盾辣ｧ縺励※蛻昴ａ縺ｦ莉｣譖ｿ"},
    {"task": "譌ｧ繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ蟒・ｭ｢遒ｺ隱・, "rationale": "螳悟・莉｣譖ｿ縺ｮ讀懆ｨｼ"}
  ]
}
```

### task-comparator.md 險ｭ險・

```markdown
---
name: task-comparator
description: Compare derived tasks with current tasks. Model: haiku.
model: haiku
tools: Read
---

# Task Comparator Agent

## Task
Compare philosophy-derived tasks with current feature tasks.

## Input
- Feature ID
- Derived tasks (from philosophy-deriver)

## Process
1. Read feature-{ID}.md Tasks section
2. Map each derived task to current task (semantic matching)
3. Identify gaps (derived but not in current)
4. Identify extras (in current but not derived - OK, just note)

## Output Format
{
  "mappings": [
    {"derived": "蜿ｯ隕門喧繝・・繝ｫ霑ｽ蜉", "current_task": "Task 1-5", "status": "covered"},
    {"derived": "繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ邨ｱ蜷・, "current_task": null, "status": "gap"},
    {"derived": "譌ｧ繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ蟒・ｭ｢遒ｺ隱・, "current_task": null, "status": "gap"}
  ],
  "gaps": [
    {"task": "繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ邨ｱ蜷・, "rationale": "..."},
    {"task": "譌ｧ繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ蟒・ｭ｢遒ｺ隱・, "rationale": "..."}
  ],
  "recommendation": "PARTIAL - 2 gaps detected"
}
```

### fl.md 繧ｻ繧ｯ繧ｷ繝ｧ繝ｳ 6 險ｭ險・

```markdown
## 6. Philosophy Gate (Post-loop, Features Only)

**Integration Point**: Insert between Section 5 (Report) and post-loop processing. Runs after WHILE loop exits with zero issues, BEFORE Report generation.

After loop completes with zero issues, run Philosophy Gate:

### Skip Conditions
- target_type != "feature" 竊・Skip
- Feature has no Philosophy section 竊・Skip with WARNING

### 6.1 Derive Tasks from Philosophy

derived = Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: `Read .claude/agents/philosophy-deriver.md and execute for Feature {target_id}`
)

### 6.2 Compare with Current Tasks

comparison = Task(
  subagent_type: "general-purpose",
  model: "haiku",
  prompt: `Read .claude/agents/task-comparator.md and execute for Feature {target_id}. Derived tasks: {derived.derived_tasks}`
)

### 6.3 Opus Decision

IF comparison.gaps is empty:
    竊・FL Complete (Philosophy fully covered)

IF comparison.gaps is not empty:
    Display gaps to Opus (parent):

    FOR each gap in comparison.gaps:
        decision = Opus evaluates:

        | Decision | Criteria | Action |
        |----------|----------|--------|
        | ADOPT | Essential for Philosophy, feasible in this Feature | Add as new AC/Task 竊・GOTO WHILE |
        | DEFER | Essential but too large for this Feature | Add to "谿玖ｪｲ鬘・ section 竊・Continue |
        | REJECT | Not actually required (deriver overreached) | Skip 竊・Continue |

    IF any ADOPT decisions:
        Apply AC/Task additions
        GOTO WHILE  # Restart loop for re-verification
    ELSE:
        IF any DEFER decisions:
            Append to feature-{ID}.md "谿玖ｪｲ鬘・ section
        FL Complete

### 6.4 Loop Limit

Philosophy Gate can trigger at most 1 re-loop.
If Phase 6 runs a second time and still has gaps 竊・Force DEFER all remaining gaps.
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Feature completion | finalizer | Mark [DONE], all ACs passed | READY_TO_COMMIT |

---

## Links

- [feature-254.md](feature-254.md) - Philosophy 譛ｪ驕疲・縺ｮ逋ｺ遶ｯ
- [feature-255.md](feature-255.md) - F254 谿玖ｪｲ鬘・
- [fl.md](../../../archive/claude_legacy_20251230/commands/fl.md) - 菫ｮ豁｣蟇ｾ雎｡
