# Feature 344: ERB/C# Codebase Analysis for Migration Design

## Status: [DONE]

## Type: research

## Created: 2026-01-04

---

## Summary

Analyze actual ERB and uEmuera C# source code to produce detailed migration specifications. Grounds F343's conceptual architecture in real codebase patterns, documenting ERB variable catalog, branching patterns, and reusable components.

---

## Background

### Philosophy (Mid-term Vision)

**Evidence-Based Design**: Migration design must be grounded in actual code analysis, not theoretical patterns. Understanding real variable usage, branching patterns, and state management enables accurate migration planning and reduces implementation surprises.

### Problem (Current Issue)

F343 produced conceptual C#/Unity architecture ([designs/full-csharp-architecture.md](designs/full-csharp-architecture.md)) but:

| Gap | Impact |
|-----|--------|
| ERB variable patterns unknown | YAML schema may miss edge cases |
| Kojo branching complexity unanalyzed | Conversion rules may be incomplete |
| uEmuera C# architecture undocumented | Reuse opportunities missed |
| Actual state management unclear | Save/load migration risk |

Current kojo development has frequent issues, suggesting patterns need documentation.

### Goal (What to Achieve)

1. Document actual ERB patterns (variables, branching, PRINT commands)
2. Analyze uEmuera C# architecture for reusable components
3. Produce detailed ERB → YAML mapping rules
4. Define concrete migration phase tasks

---

## Design Document

**Output**: [designs/codebase-analysis-report.md](designs/codebase-analysis-report.md)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Analysis report created | file | Glob | exists | Game/agents/designs/codebase-analysis-report.md | [x] |
| 2 | ERB variable catalog | file | Grep | contains | "## ERB Variable Catalog" | [x] |
| 3 | Kojo branching patterns | file | Grep | contains | "## Kojo Branching Patterns" | [x] |
| 4 | uEmuera architecture summary | file | Grep | contains | "## uEmuera C# Architecture" | [x] |
| 5 | ERB → YAML mapping rules | file | Grep | contains | "## ERB to YAML Mapping" | [x] |
| 6 | Reusable components list | file | Grep | contains | "## Reusable Components" | [x] |
| 7 | Migration task breakdown | file | Grep | contains | "## Migration Tasks" | [x] |
| 8 | Document finalized | file | Grep | not_contains | "**Status**: DRAFT" | [x] |

**Target file for AC#2-8**: `designs/codebase-analysis-report.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create analysis report skeleton | [x] |
| 2 | 2 | Analyze ERB variable usage (CFLAG, FLAG, TFLAG, LOCAL, ARG) | [x] |
| 3 | 3 | Document kojo branching patterns (TALENT, ABL, EXP, NTR) | [x] |
| 4 | 4 | Analyze uEmuera C# engine architecture | [x] |
| 5 | 5 | Define ERB → YAML mapping rules based on actual patterns | [x] |
| 6 | 6 | Identify reusable uEmuera components | [x] |
| 7 | 7 | Break down migration into concrete tasks | [x] |
| 8 | 8 | Finalize document (change Status from DRAFT to FINAL) | [x] |

---

## Analysis Scope

### ERB Analysis Targets

| Category | Files | Focus |
|----------|-------|-------|
| Kojo | `Game/ERB/口上/*.ERB` | Branching, DATALIST, PRINT patterns |
| Variables | `Game/CSV/*.CSV` | CFLAG, FLAG definitions |
| Core Logic | `Game/ERB/*.ERB` | State management, COM execution |

### C# Analysis Targets

| Category | Path | Focus |
|----------|------|-------|
| Engine Core | `engine/Assets/Scripts/` | Architecture, reusable classes |
| ERB Interpreter | `engine/Assets/Scripts/Emuera/` | Variable handling, execution flow |
| Headless Mode | `engine/Assets/Scripts/Emuera/Headless/` | Test infrastructure patterns |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F343 | Conceptual architecture to ground |
| Successor | TBD | Implementation features based on analysis results |

---

## Links

- [feature-343.md](feature-343.md) - Conceptual architecture design
- [designs/full-csharp-architecture.md](designs/full-csharp-architecture.md) - F343 output
- [designs/codebase-analysis-report.md](designs/codebase-analysis-report.md) - This feature output

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | create | - | Initial proposal | PROPOSED |
| 2026-01-05 10:30 | START | implementer | Tasks 1-8 execution | - |
| 2026-01-05 12:45 | END | implementer | Tasks 1-8 complete | SUCCESS |
