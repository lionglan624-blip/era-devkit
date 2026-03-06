# Feature 345: Phase 1 Migration Feature Breakdown

## Status: [DONE]

## Type: research

## Created: 2026-01-05

---

## Summary

Define testable minimum unit features for Phase 1 of the C#/Unity migration. Converts F344's Phase 1 plan into concrete, independently testable features.

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. This aligns with TDD principles and the project's AC:Task 1:1 policy, enabling clear failure diagnosis and parallel development.

### Problem (Current Issue)

F344 defined Phase 1 with 4 tasks:

| Task | Content | Complexity |
|------|---------|:----------:|
| 1.1 | ERB→YAML Converter Tool | High |
| 1.2 | YAML Schema Generator | Medium |
| 1.3 | YAML Dialogue Renderer | Medium |
| 1.4 | Pilot Conversion (美鈴 COM_0) | Low |

**Issues**:
- Task 1.1 (High complexity) is too large for single feature
- Dependencies unclear between tasks
- Testing strategy not defined per component

### Goal (What to Achieve)

1. Split Phase 1 into testable minimum unit features
2. Define clear dependencies between features
3. Specify testable ACs for each feature
4. Enable parallel development where possible

---

## Analysis

### Task 1.1 Breakdown (ERB→YAML Converter)

F344 defined 6 subtasks:
1. Implement ERB lexer/parser (reuse Emuera components)
2. Build DATALIST→YAML converter
3. Implement TALENT branching extraction
4. Add CALLNAME→placeholder conversion
5. Generate YAML with YamlDotNet
6. Add edge case logging

**Testable Units**:

| Unit | Input | Output | Testable? |
|------|-------|--------|:---------:|
| ERB Parser | ERB source | AST | ✓ |
| DATALIST Converter | DATALIST block | YAML array | ✓ |
| TALENT Extractor | IF/ELSEIF/ELSE | Condition objects | ✓ |
| Placeholder Converter | CALLNAME etc. | {placeholder} | ✓ |

### Task 1.2-1.4 Analysis

| Task | Testable as Single Unit? | Notes |
|------|:------------------------:|-------|
| 1.2 Schema Generator | ✓ | VariableCode → Schema is atomic |
| 1.3 YAML Renderer | ✓ | YAML → Text is atomic |
| 1.4 Pilot Conversion | ✓ | Integration test |

### Dependency Graph

```
F346 (ERB Parser) ──┬──→ F347 (TALENT Extractor) ─┬─→ F353 (CFLAG/Func) ─┐
                    │                              │                      │
                    └──────────────────────────────┴──────────────────────┼─→ F349 (Converter) ─┐
                                                                          │                     │
F348 (Schema Generator) ──────────────────────────────────────────────────┘                     ├─→ F351 (Pilot)
                                                                                                │
F350 (YAML Renderer) ───────────────────────────────────────────────────────────────────────────┘
```

Note: F347 depends on F346's AST output. F353 extends F347 for CFLAG/Function conditions. F349 requires F346, F347, F348, and F353.

---

## Proposed Feature Breakdown

### Option A: 6 Features (Minimum Testable Units)

**Proposed IDs**: 346-351 (next available per index-features.md)

| ID | Name | Input | Output | Test |
|:--:|------|-------|--------|------|
| 346 | ERB Parser | ERB source | AST | Parse → validate structure |
| 347 | TALENT Branching Extractor | AST | Condition tree | Extract → verify conditions |
| 348 | YAML Schema Generator | VariableCode.cs | JSON Schema | Generate → validate sample |
| 349 | DATALIST→YAML Converter | AST (F346) + Conditions (F347) | YAML | Convert → schema validate |
| 350 | YAML Dialogue Renderer | YAML + context | Text | Render → compare expected |
| 351 | Pilot Conversion (美鈴 COM_0) | Full pipeline | ERB==YAML | All 16 variants match |

### Option B: 4 Features (Grouped by Deliverable)

| ID | Name | Content | Test |
|:--:|------|---------|------|
| 346 | ERB→YAML Converter | Parser + Extractor + Converter | Convert single file |
| 347 | YAML Schema Generator | Schema from VariableCode | Validate sample YAML |
| 348 | YAML Dialogue Renderer | Load + evaluate + render | Render test cases |
| 349 | Pilot Conversion | Integration test | ERB==YAML for 美鈴 |

### Option C: 3 Features (Original Proposal)

| ID | Name | Content |
|:--:|------|---------|
| 346 | ERB→YAML Conversion Base | Converter + Schema |
| 347 | YAML Dialogue Renderer | Renderer |
| 348 | Pilot Conversion | Integration |

---

## Recommendation

**Selected Option: Option A (6 Features)** - Testable minimum units

**Rationale**:
1. Each feature has clear, binary pass/fail test
2. Failure diagnosis is immediate (know exactly which component failed)
3. Parallel development possible (346 and 348 can run concurrently; 347 after 346)
4. Aligns with project's TDD and AC:Task 1:1 principles
5. Smaller features = faster feedback loops

**Trade-off**:
- More feature management overhead (6 vs 3)
- Mitigated by clear dependency graph

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Analysis documented | file | Grep | contains | "Testable Units" in feature-345.md | [x] |
| 2 | Split decision documented | file | Grep | contains | "Selected Option" in feature-345.md | [x] |
| 3 | Feature files created | file | Bash | count_equals | 6 | [x] |
| 4 | Dependencies documented | file | Grep | contains | "Predecessor" | [x] |
| 5 | index-features.md updated | file | Grep | contains | "feature-346.md" | [x] |

### AC Details

**AC#3 Test**: `ls Game/agents/feature-346.md Game/agents/feature-347.md Game/agents/feature-348.md Game/agents/feature-349.md Game/agents/feature-350.md Game/agents/feature-351.md 2>/dev/null | wc -l` → Expected: `6`

**AC#4 Target**: feature-346.md through feature-351.md (verify each contains "Predecessor" in Dependencies)

**AC#5 Target**: Game/agents/index-features.md (verify Active Features contains F346-F351 entries)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze actual ERB/C# code for testable units | [x] |
| 2 | 2 | Document Selected Option in feature-345.md | [x] |
| 3 | 3 | Create feature files for each unit (depends on Task#2) | [x] |
| 4 | 4 | Add dependency Links to each feature | [x] |
| 5 | 5 | Update index-features.md with new features | [x] |

---

## Reference

### User Requirements

- 1 Feature = Testable minimum unit
- TDD alignment
- Clear failure diagnosis

### Proposed Output Features (Option A, Extended)

- [feature-346.md](feature-346.md) - ERB Parser
- [feature-347.md](feature-347.md) - TALENT Branching Extractor
- [feature-348.md](feature-348.md) - YAML Schema Generator
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter
- [feature-350.md](feature-350.md) - YAML Dialogue Renderer
- [feature-351.md](feature-351.md) - Pilot Conversion
- [feature-353.md](feature-353.md) - CFLAG/Function Condition Extractor (added 2026-01-05)

See Analysis section for detailed breakdown and dependency graph.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F344 | Phase 1 definition source |
| Successor | F346-F351, F353 | Features created from this breakdown (Option A, extended) |
| Successor | F352 | Phase 2 Planning (triggered on F351 completion) |

---

## Links

- [feature-344.md](feature-344.md) - Codebase Analysis (Phase 1 source)
- [designs/codebase-analysis-report.md](designs/codebase-analysis-report.md) - F344 output
- [index-features.md](index-features.md) - Target for AC#5 update
- Output features: F346-F351, F353 (created), F352 (Phase 2 trigger)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal with discussion context | PROPOSED |
| 2026-01-05 | START | implementer | Tasks 3-5: Create feature files F346-F351, add dependencies, update index | - |
| 2026-01-05 | END | implementer | Tasks 3-5 complete | SUCCESS |
