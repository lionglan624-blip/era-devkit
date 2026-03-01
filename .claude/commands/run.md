---
description: "Execute feature with Progressive Disclosure workflow"
argument-hint: "<feature-id>"
---

# /run Command

**Progressive Disclosure version of Feature implementation command (pilot)**

Execute feature implementation with phase-by-phase rule loading.

## Usage

```
/run {ID}
/run dev|kojo|erb|engine|infra|research
```

## Core Principle

**Load the relevant Phase's Skill at the start of each Phase. Do not read everything upfront.**

## Execution Flow

1. Read `Skill(run-workflow)` for overview and Phase 1 entry
2. Execute Phase 1
3. At Phase 1 end, Skill tells you to read Phase 2
4. Repeat until completion

## Start

```
Skill(run-workflow)
```
