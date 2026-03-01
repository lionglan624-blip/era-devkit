# Feature 347: TALENT Branching Extractor

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Extract TALENT branching conditions from ERB AST into structured condition tree objects for downstream YAML conversion (F349). CFLAG/function extraction handled by F353.

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. The branching extractor isolates condition analysis from parsing (F346) and conversion (F349), enabling independent verification of condition tree correctness.

### Problem (Current Issue)

F344 identified kojo branching as complex (TALENT, ABL, EXP checks across IF/ELSEIF/ELSE). Current F345 analysis shows:
- TALENT branching embedded in DATALIST blocks
- Conditions reference game state variables
- Mapping to YAML requires structured representation

Without extraction, conditions would be opaque strings in YAML, preventing runtime evaluation.

### Goal (What to Achieve)

Build an extractor that:
1. Parses IF/ELSEIF/ELSE conditions from AST
2. Identifies TALENT references
3. Outputs structured condition objects for YAML serialization

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | IfNode enhancement | test | dotnet test | succeeds | IfNodeTests.ElseIfElseBranchesExist passes | [x] |
| 1 | Extract simple condition | test | dotnet test | succeeds | TalentBranchingExtractorTests.ExtractSimpleCondition passes | [x] |
| 2 | Handle complex branching | test | dotnet test | succeeds | TalentBranchingExtractorTests.HandleComplexBranching passes (4-branch) | [x] |
| 3 | Output structured object | test | dotnet test | succeeds | TalentBranchingExtractorTests.SerializeConditionTree passes | [x] |
| 4 | Invalid TALENT reference (Neg) | test | dotnet test | succeeds | TalentBranchingExtractorTests.InvalidTalentReference passes | [x] |
| 5 | Malformed TALENT condition (Neg) | test | dotnet test | succeeds | TalentBranchingExtractorTests.MalformedTalentCondition passes | [x] |

### AC Details

**AC#0 Test**: Parse IF/ELSEIF/ELSEIF/ELSE/ENDIF block from test fixture `tools/ErbParser.Tests/TestData/if_elseif_else.erb`, verify IfNode has ElseIfBranches (List<ElseIfBranch>) and ElseBranch properties populated

**AC#1 Test**: Parse `IF TALENT:MASTER:恋人 != 0` or `IF TALENT:恋人` condition string, verify condition tree contains TalentRef(target=MASTER or implicit, name=恋人). **Scope**: Handles TALENT references only. CFLAG/ABL/EXP conditions are out of scope (F353).

**AC#2 Test**: Parse IF/ELSEIF/ELSEIF/ELSE block (恋人/恋慕/思慕/なし pattern), verify tree contains 4 condition nodes

**AC#3 Test**: Extract conditions, verify output serializes to valid JSON structure (using System.Text.Json)

**AC#4 Test**: Pass null/empty TALENT reference, verify graceful handling (no exception, returns empty/null)

**AC#5 Test**: Pass malformed TALENT condition string (e.g., empty TALENT reference `TALENT:`, invalid syntax `TALENT::name`), verify graceful error handling

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | Enhance IfNode: Add ElseIfBranch/ElseBranch classes, update IfNode properties, update ParseIfBlock(). Create test fixture `if_elseif_else.erb`. (Backward-compatible extension) | [x] |
| 1 | 1 | Implement TalentConditionParser: Input=IfNode.Condition (string), Output=TalentCondition object | [x] |
| 2 | 2 | Build ConditionBranch tree: Input=IfNode with ElseIf/Else, Output=List<ConditionBranch> | [x] |
| 3 | 3 | Add JSON serialization for ConditionBranch/TalentCondition objects | [x] |
| 4 | 4 | Add null/empty TALENT reference handling with graceful fallback | [x] |
| 5 | 5 | Add malformed TALENT condition error handling | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Feature breakdown and design source |
| Predecessor | F346 | Requires AST input. Note: Task#0 extends F346's IfNode class with ElseIfBranch/ElseBranch properties |
| Successor | F349 | DATALIST Converter uses condition trees |
| Successor | F353 | CFLAG/Function Extractor extends condition extraction |

---

## Links

- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-346.md](feature-346.md) - ERB Parser (AST source)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (consumer)
- [feature-353.md](feature-353.md) - CFLAG/Function Extractor (scope extension)
- [feature-344.md](feature-344.md) - Codebase Analysis (branching patterns reference)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal from F345 breakdown | PROPOSED |
| 2026-01-05 10:00 | START | implementer | Task 0 | - |
| 2026-01-05 10:01 | END | implementer | Task 0 | SUCCESS |
| 2026-01-05 10:03 | START | implementer | Task 1 | - |
| 2026-01-05 10:03 | END | implementer | Task 1 | SUCCESS |
