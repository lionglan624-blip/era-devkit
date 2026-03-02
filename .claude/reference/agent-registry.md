# Agent Registry

> **Purpose**: Central registry of all subagents and their dispatch configuration.
>
> **Referenced by**: CLAUDE.md (pointer), ssot-update-rules.md, audit.md, feature-reviewer.md

---

## Model Context Windows

| Model | ID | Context |
|-------|-----|:-------:|
| Opus 4.6 | claude-opus-4-6 | 200K (1M beta) |
| Sonnet 4.5 | claude-sonnet-4-5-20250929 | 200K (1M beta) |
| Haiku 4.5 | claude-haiku-4-5-20251001 | 200K |

**1M context**: Use `[1m]` suffix (e.g., `/model opus[1m]`). Requires pay-as-you-go plan.

---

## Agent Table

| Agent | Type | Model | Purpose |
|-------|------|:-----:|---------|
| initializer | skill (fork) | sonnet | Feature state init |
| ~~tech-investigator~~ | ~~general-purpose~~ | ~~opus~~ | ~~Deep investigation~~ (archived → use deep-explorer) |
| tech-designer | general-purpose | sonnet | Technical Design (to satisfy ACs) |
| ~~goal-setter~~ | ~~skill (fork)~~ | ~~haiku~~ | ~~Version goal concretization~~ (archived) |
| ~~spec-writer~~ | ~~general-purpose~~ | ~~sonnet~~ | ~~Design specification writer~~ (archived) |
| ~~dependency-analyzer~~ | ~~skill (fork)~~ | ~~sonnet~~ | ~~Dependency analysis and risk identification~~ (archived) |
| feature-reviewer | feature-reviewer | opus | Holistic feature review |
| reference-checker | skill (fork) | sonnet | Feature reference/link validation |
| feasibility-checker | feasibility-checker | sonnet | Task feasibility |
| ac-task-aligner | ac-task-aligner | sonnet | AC:Task 1:1 alignment |
| ac-validator | ac-validator | opus | AC TDD validation |
| planning-validator | general-purpose | opus | Research feature validation (pre: design, post: coverage) |
| eratw-reader | skill (fork) | sonnet | eraTW reference extraction |
| kojo-writer | kojo-writer | opus | Dialogue creation (ERB only) |
| implementer | implementer | sonnet | ERB/Engine code |
| smart-implementer | smart-implementer | opus | Complex ERB/Engine code (multi-file, architectural) |
| ac-tester | ac-tester | sonnet | AC verification (read-only) |
| doc-reviewer | doc-reviewer | sonnet | Documentation review |
| debugger | debugger | sonnet→opus | Error fix (escalates) |
| finalizer | skill (fork) | sonnet | Status update |
| philosophy-deriver | skill (fork) | opus | Derive tasks from Philosophy |
| task-comparator | skill (fork) | sonnet | Compare derived/current tasks |
| ac-designer | general-purpose | opus | Design ACs with philosophy derivation patterns |
| wbs-generator | general-purpose | sonnet | Generate Tasks with AC:Task alignment |
| quality-fixer | quality-fixer | sonnet | Apply feature-quality checklist fixes before validation |
| feature-validator | general-purpose | sonnet | Validate complete feature file |
| consensus-synthesizer | general-purpose | opus/sonnet | Synthesize investigations (opus) / Micro-revision (sonnet) |
| drift-checker | drift-checker | sonnet | Codebase drift detection |
| ~~com-auditor~~ | ~~com-auditor~~ | ~~opus~~ | ~~COM-Kojo semantic audit~~ (archived) |
| deep-explorer | deep-explorer | opus | Expert codebase exploration (/fc consensus) |
| ~~smart-explorer~~ | ~~smart-explorer~~ | ~~sonnet~~ | ~~Advanced codebase explorer~~ (archived) |

**Built-in types** (system-provided): `Explore`, `Plan`, `Bash`, `claude-code-guide`, `statusline-setup`

---

## Dispatch Rules

**Format**: `Task(subagent_type: "{type}", prompt: "...")`

- Agents with `.claude/agents/{name}.md`: use `subagent_type: "{name}"` (enforces frontmatter `tools` restriction)
- Agents without matching system type: use `subagent_type: "general-purpose"` with `"Read .claude/agents/{agent}.md"` in prompt
- Skill (fork) agents: Use `Skill({name})` for isolated execution OR `Task()` for inline

**CRITICAL**: Custom subagent types auto-load frontmatter (tools, model) and markdown body as system prompt. `general-purpose` ignores frontmatter — tools restrictions are NOT enforced.

**CRITICAL**: Do NOT use `run_in_background: true` for ANY Task dispatch. Background tasks cause premature session exit in `-p` mode.

**CRITICAL**: Do NOT use `model:` in Skill frontmatter. See [Issue #14882](https://github.com/anthropics/claude-code/issues/14882), [Issue #17283](https://github.com/anthropics/claude-code/issues/17283). For Skill (fork) agents that need model enforcement, use `Task(subagent_type: "general-purpose", model: "{model}", prompt: "Read .claude/skills/{name}/SKILL.md and execute...")` instead of `Skill({name})`.

**TaskOutput Caution**: Full transcript can consume 100K+ tokens. For long-running agents (kojo-writer), prefer file polling (Glob) over TaskOutput.
