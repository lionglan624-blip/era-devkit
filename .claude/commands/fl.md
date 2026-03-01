---
description: Feature review-fix loop until zero issues
argument-hint: "<target>"
---

# /fl Command

**Progressive Disclosure feature review command**

Review-fix loop until **zero issues**.

## Usage

| Target | Example | Description |
|--------|---------|-------------|
| Feature | `/fl 208` | Review feature spec |
| Command | `/fl commit` or `/fl cmd:commit` | Review slash command |
| Agent | `/fl feature-reviewer` or `/fl agent:feature-reviewer` | Review subagent |
| Skill | `/fl erb-syntax` or `/fl skill:erb-syntax` | Review skill |
| Doc | `/fl doc:content-roadmap` | Review pm/ document |

## Core Principle

**Load the corresponding Phase skill at each Phase start. Do not read the entire content upfront.**

## Language

**User-facing explanations MUST be in Japanese.** Internal thinking and documentation remain in English.

## Execution Flow

1. Read `Skill(fl-workflow)` for overview and Phase 1 entry
2. Execute Phase 1 (Reference Check)
3. At Phase 1 end, Skill tells you to read Phase 2
4. Main loop: Phase 2-8
5. POST-LOOP for pending_user, Report, Status Update, Philosophy Gate
6. Repeat until completion

## Start

```
Skill(fl-workflow)
```

**CRITICAL**: Do NOT read the full fl.md content. This command uses progressive skill loading instead.
