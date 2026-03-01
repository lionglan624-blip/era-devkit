# Feature 353: CFLAG/Function Condition Extractor

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Extract CFLAG references and function calls from ERB condition expressions into structured condition objects. Extends F347 TALENT extractor to handle complete condition expressions for YAML conversion.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Condition Extraction**: Full migration from ERB to YAML requires extracting all condition types, not just TALENT. This feature completes the condition extraction pipeline started by F347.

### Problem (Current Issue)

F347 extracts only TALENT references from IF/ELSEIF/ELSE blocks. Real kojo conditions also include:
- CFLAG references (e.g., `CFLAG:MASTER:100`)
- Function calls (e.g., `HAS_VAGINA()`, `FIRSTTIME()`)
- Comparison operators and logical operators

Without CFLAG/function extraction, YAML conversion would be incomplete.

### Goal (What to Achieve)

Build an extractor that:
1. Parses CFLAG references from condition strings
2. Identifies function calls in conditions
3. Integrates with F347 TalentRef/ConditionBranch for complete condition trees

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | Polymorphic condition type | test | dotnet test | succeeds | IConditionInterfaceTests pass | [x] |
| 1 | Extract CFLAG reference | test | dotnet test | succeeds | CflagExtractorTests.ExtractCflagReference passes | [x] |
| 2 | Extract function call | test | dotnet test | succeeds | FunctionExtractorTests.ExtractFunctionCall passes | [x] |
| 3 | Combined condition extraction | test | dotnet test | succeeds | ConditionExtractorTests.CombinedExtraction passes | [x] |
| 4 | Invalid CFLAG reference | test | dotnet test | succeeds | CflagExtractorTests.InvalidCflagReference passes | [x] |
| 5 | Invalid function syntax | test | dotnet test | succeeds | FunctionExtractorTests.InvalidFunctionSyntax passes | [x] |
| 6 | Logical operator parsing | test | dotnet test | succeeds | LogicalOperatorParserTests.ParseAndOr passes | [x] |

### AC Details

**AC#0 Test**: `dotnet test tools/ErbParser.Tests/ --filter IConditionInterfaceTests`
Test cases: (1) TalentRef instances can be assigned to ICondition variable, (2) ConditionBranch.Condition property accepts TalentRef via ICondition, (3) Existing TalentRef serialization unchanged

**AC#1 Test**: `dotnet test tools/ErbParser.Tests/ --filter CflagExtractorTests`
Parse CFLAG references in multiple forms:
- `CFLAG:MASTER:現在位置` → CflagRef(target=MASTER, name=現在位置)
- `CFLAG:100` → CflagRef(index=100)
- `CFLAG:睡眠` → CflagRef(name=睡眠)

**AC#2 Test**: `dotnet test tools/ErbParser.Tests/ --filter FunctionExtractorTests`
Parse `IF HAS_VAGINA(TARGET)` block, verify FunctionCall(name=HAS_VAGINA, args=[TARGET])

**AC#3 Test**: `dotnet test tools/ErbParser.Tests/ --filter ConditionExtractorTests`
Input=IfNode.Condition string containing logical operators (e.g., `TALENT:恋人 && CFLAG:MASTER:100`), Output=LogicalOp tree with TalentRef and CflagRef children
**Prerequisite**: AC#6 (LogicalOperatorParser) must pass first - AC#3 tests integration of all condition types

**AC#4 Test**: `dotnet test tools/ErbParser.Tests/ --filter CflagExtractorTests.InvalidCflagReference`
Parse invalid/empty CFLAG string, verify graceful null return

**AC#5 Test**: `dotnet test tools/ErbParser.Tests/ --filter FunctionExtractorTests.InvalidFunctionSyntax`
Parse malformed function call syntax, verify graceful null return

**AC#6 Test**: `dotnet test tools/ErbParser.Tests/ --filter LogicalOperatorParserTests`
Parse `condition1 && condition2` and `condition1 || condition2`, verify LogicalOp(left, operator, right)

---

## Output Class Design

**ICondition**: Marker interface for all condition types (TalentRef, CflagRef, FunctionCall, LogicalOp implement this). For polymorphic JSON serialization, ICondition should use [JsonDerivedType] attributes for each implementing type.

**TalentRef** (existing in ErbParser, will add ICondition implementation - backward compatible): `{ Target, Name, Operator?, Value? }`
**CflagRef**: `{ Target?, Name?, Index?, Operator?, Value? }` - parallels TalentRef structure (implements ICondition). Name and Index are mutually exclusive: parser sets Index if numeric literal detected, Name otherwise. Target is optional prefix (e.g., MASTER in CFLAG:MASTER:100)
**FunctionCall**: `{ Name, Args[] }` - function name and argument list (implements ICondition)
**LogicalOp**: `{ Left: ICondition, Operator: string, Right: ICondition }` - binary logical operation node (implements ICondition)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | Refactor ConditionBranch: Define ICondition interface with [JsonDerivedType] for polymorphic serialization, migrate TalentRef to implement ICondition, change ConditionBranch.Condition type from TalentRef? to ICondition?. Note: JSON output may change with type discriminator - verify existing tests pass. | [x] |
| 1 | 1 | Implement CflagConditionParser: Input=condition string, Output=CflagRef (implements ICondition) | [x] |
| 2 | 2 | Implement FunctionCallParser: Input=condition string, Output=FunctionCall (implements ICondition) | [x] |
| 3 | 4 | Add null/empty CFLAG reference handling with graceful fallback | [x] |
| 4 | 5 | Add malformed function syntax handling with graceful fallback | [x] |
| 5 | 6 | Implement LogicalOperatorParser for && and || operators (LogicalOp implements ICondition) | [x] |
| 6 | 3 | Update ConditionExtractor.Extract() method to detect TALENT/CFLAG/function patterns and return appropriate ICondition subtype. (Prerequisite: Task#5 LogicalOperatorParser) | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Phase 1 breakdown (added to scope 2026-01-05) |
| Predecessor | F347 | TALENT extractor provides base condition extraction |
| Enhancement | F349 | DATALIST Converter can integrate extended conditions (F349 is DONE with F347 TALENT-only support; F353 provides optional enhancement) |
| Successor | F351 | Pilot Conversion (via F349) |

---

## Links

- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-347.md](feature-347.md) - TALENT Branching Extractor (predecessor)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (consumer)
- [feature-351.md](feature-351.md) - Pilot Conversion (via F349)
- [feature-344.md](feature-344.md) - Codebase Analysis (condition patterns reference)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | FL | Created from F347 scope split (user decision) | PROPOSED |
| 2026-01-05 15:11 | START | implementer | Task 0 | - |
| 2026-01-05 15:11 | END | implementer | Task 0 | SUCCESS |
| 2026-01-05 15:14 | START | implementer | Task 1 | - |
| 2026-01-05 15:14 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 15:16 | START | implementer | Task 2 | - |
| 2026-01-05 15:16 | END | implementer | Task 2 | SUCCESS |
| 2026-01-05 15:19 | START | implementer | Task 5 | - |
| 2026-01-05 15:19 | END | implementer | Task 5 | SUCCESS |
| 2026-01-05 15:21 | START | implementer | Task 6 | - |
| 2026-01-05 15:21 | END | implementer | Task 6 | SUCCESS |
| 2026-01-05 15:23 | NOTE | opus | Task 3, 4 | Implemented inline with Task 1, 2 (defensive coding) |
