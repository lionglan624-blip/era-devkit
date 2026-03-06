# Feature 701: ErbToYaml.Tests Pre-existing Test Failures Fix

## Status: [DONE]

## Type: infra

## Created: 2026-01-31

---

## Background

### Problem (Current Issue)

ErbToYaml.Tests has 5 pre-existing test failures discovered during F696 (xUnit v3 Re-migration). These failures exist with both xUnit v2 and v3, indicating they are not caused by the xUnit version but by schema/converter issues.

### Failing Tests

1. `ErbToYaml.Tests.ConverterTests.EmbedConditions`
2. `ErbToYaml.Tests.ConverterTests.MissingCondition`
3. `ErbToYaml.Tests.ConverterTests.ConvertSimpleDatalist`
4. `ErbToYaml.Tests.FileConverterTests.Test_ConditionalPreservation_IfWrappedPrintData`
5. `ErbToYaml.Tests.FileConverterTests.Test_ConditionalPreservation_SimplePrintData`

### Goal (What to Achieve)

Fix the 5 failing tests in ErbToYaml.Tests to restore full test suite health.

---

## Links

- [feature-696.md](feature-696.md) - Origin: Discovered during xUnit v3 re-migration
- [feature-675.md](feature-675.md) - Root cause: YAML format unification (branches → entries)
- [feature-349.md](feature-349.md) - Original DATALIST→YAML Converter tests
- [feature-634.md](feature-634.md) - FileConverterTests origin
- [feature-652.md](feature-652.md) - Similar migration: Updated test YAML files for entries: format

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} {iter}: {description} -->

- [resolved-invalid] Phase1-Uncertain iter1: Method column inconsistency - whether --filter arguments belong in Method vs AC Details (Validation: AC Definition Table Method column format is stylistic - specific --filter commands documented in AC Details section per feature-template.md)

---

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 5 tests in ErbToYaml.Tests fail with assertion errors
2. Why: Tests expect `branches:` key in YAML output but converter produces `entries:` key
3. Why: F675 (YAML Format Unification) changed DatalistConverter, FileConverter, and PrintDataConverter to output `entries:` format instead of `branches:` format
4. Why: F675 did not update the existing unit tests in ErbToYaml.Tests to match the new format
5. Why: F675's scope was focused on converter output and production YAML re-conversion; test assertion updates were overlooked

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `Assert.Contains("branches:", yaml)` fails | F675 changed output format from `branches:` to `entries:` but tests were not updated |
| `Assert.Contains("TALENT:", yaml)` fails | Condition format changed from nested `{ TALENT: { N: { ne: 0 } } }` to `{ type: "Talent", talentType: "N", threshold: 1 }` via BranchesToEntriesConverter |
| `Assert.Contains("branches", yamlObject.Keys)` fails | Top-level key is now `entries` not `branches` |

### Conclusion

**The tests are outdated, not the converter.** F675 (completed 2026-01-29) successfully unified the YAML format from legacy `branches:` to canonical `entries:` format. However, the ErbToYaml.Tests were not updated to reflect this schema change:

1. **ConverterTests** (F349 origin):
   - `ConvertSimpleDatalist`: Expects `branches:` → Should expect `entries:`
   - `EmbedConditions`: Expects `TALENT:` nested format → Should expect `{ type: "Talent", talentType: ... }` format
   - `MissingCondition`: Expects `branches` key in parsed YAML → Should expect `entries`

2. **FileConverterTests** (F634 origin):
   - `Test_ConditionalPreservation_IfWrappedPrintData`: Expects `branches:` → Should expect `entries:`
   - `Test_ConditionalPreservation_SimplePrintData`: Expects `branches:` → Should expect `entries:`

**The fix is test updates, not converter changes.** The converter is working correctly per F675's design.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F675 | [DONE] | Cause of divergence | Changed output format, did not update tests |
| F349 | [DONE] | Test origin (ConverterTests) | Original tests for DATALIST→YAML converter |
| F634 | [DONE] | Test origin (FileConverterTests) | Original tests for FileConverter |
| F696 | [DONE] | Discovery point | Tests were found failing during xUnit v3 re-migration |
| F652 | [DONE] | Similar migration | Updated test YAML files for entries: format, but not unit test assertions |

### Pattern Analysis

This is a **test maintenance gap** pattern:
1. Production code updated (F675 converters)
2. Production data updated (F675 YAML files)
3. Unit tests NOT updated (gap)

F675's scope explicitly covered:
- AC#1-5: Converter output format
- AC#6-10: Production YAML files
- AC#8: YamlDialogueLoader unit tests (Era.Core.Tests)

But F675 did NOT include ACs for ErbToYaml.Tests assertion updates. The gap was created because:
- F675 focused on "fix at source" strategy for converters
- Batch re-conversion verified production files
- Existing ErbToYaml.Tests were assumed to still pass (they didn't)
- No regression test execution for ErbToYaml.Tests was performed

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Tests need assertion updates only; no logic changes |
| Scope is realistic | YES | 5 tests with straightforward assertion changes |
| No blocking constraints | YES | Tests are independent; no circular dependencies |

**Verdict**: FEASIBLE

**Rationale**:
1. Tests are correct in their PURPOSE (verify converter functionality)
2. Tests have incorrect EXPECTATIONS (old format assertions)
3. Fix is mechanical: update expected values from `branches:` to `entries:`, update condition format assertions
4. No converter changes needed (F675 already implemented correct behavior)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F675 | [DONE] | Cause of test failures; defines new format |
| Related | F349 | [DONE] | Original ConverterTests implementation |
| Related | F634 | [DONE] | Original FileConverterTests implementation |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| dialogue-schema.json | Schema | Low | Schema already updated in F675 for entries: format |
| BranchesToEntriesConverter | Runtime | Low | Already tested via F675 |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| CI/CD pipeline | HIGH | Test failures block other test suites |
| ErbToYaml tool verification | MEDIUM | Cannot verify converter behavior with failing tests |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbToYaml.Tests/ConverterTests.cs | Update | Update 3 test assertions: ConvertSimpleDatalist, EmbedConditions, MissingCondition |
| tools/ErbToYaml.Tests/FileConverterTests.cs | Update | Update 2 test assertions: Test_ConditionalPreservation_IfWrappedPrintData, Test_ConditionalPreservation_SimplePrintData |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Must match entries: format from F675 | dialogue-schema.json, BranchesToEntriesConverter | HIGH - Assertions must reflect new schema structure |
| Condition format is now flat record | DialogueCondition.cs | MEDIUM - EmbedConditions test must check for type/talentType/threshold fields |
| No branches: key exists in output | DatalistConverter uses BranchesToEntriesConverter | HIGH - All branches: assertions must change to entries: |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| New assertions may not match actual output | Low | Medium | Run tests after each change to verify |
| Condition format assertion may be incomplete | Low | Low | Review BranchesToEntriesConverter.TransformCondition() for exact format |
| Schema validation test may need adjustment | Low | Low | SchemaValidation test already passes (uses schema, not string matching) |

### Detailed Test Failure Analysis

#### Test 1: ConvertSimpleDatalist (Line 64)
```
Assert.Contains("branches:", yaml);  // FAILS
```
**Fix**: Change to `Assert.Contains("entries:", yaml);`

#### Test 2: EmbedConditions (Line 97)
```
Assert.Contains("TALENT:", yaml);  // FAILS - nested format removed
```
**Fix**: Change to assertions matching new condition format:
```csharp
Assert.Contains("type: Talent", yaml);  // or talentType, threshold
```

#### Test 3: MissingCondition (Line 208)
```
Assert.Contains("branches", yamlObject.Keys);  // FAILS
```
**Fix**: Change to `Assert.Contains("entries", yamlObject.Keys);`

#### Test 4: Test_ConditionalPreservation_IfWrappedPrintData (Line 216)
```
Assert.Contains("branches:", yamlContent);  // FAILS
```
**Fix**: Change to `Assert.Contains("entries:", yamlContent);`

Also Line 217-218:
```
Assert.Contains("condition:", yamlContent);  // May still pass (entries have conditions)
Assert.Contains("TALENT:", yamlContent);  // FAILS - format changed
```
**Fix**: Update TALENT assertion to match new condition format

#### Test 5: Test_ConditionalPreservation_SimplePrintData (Line 262)
```
Assert.Contains("branches:", yamlContent);  // FAILS
```
**Fix**: Change to `Assert.Contains("entries:", yamlContent);`

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Fix the 5 failing tests" | All 5 tests must pass after fix | AC#1-5 |
| "restore full test suite health" | ErbToYaml.Tests suite must have 0 failures | AC#6 |
| "tests are outdated, not the converter" | Assertions updated, converter unchanged | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ConvertSimpleDatalist passes | test | dotnet test | succeeds | - | [x] |
| 2 | EmbedConditions passes | test | dotnet test | succeeds | - | [x] |
| 3 | MissingCondition passes | test | dotnet test | succeeds | - | [x] |
| 4 | Test_ConditionalPreservation_IfWrappedPrintData passes | test | dotnet test | succeeds | - | [x] |
| 5 | Test_ConditionalPreservation_SimplePrintData passes | test | dotnet test | succeeds | - | [x] |
| 6 | Full ErbToYaml.Tests suite passes | test | dotnet test | succeeds | tools/ErbToYaml.Tests | [x] |

**Note**: 6 ACs for infra feature (typical range: 8-15). Count justified by focused scope (test assertion updates only). AC#7 removed as redundant - test-only constraint enforced by Implementation Contract.

### AC Details

**AC#1: ConvertSimpleDatalist passes**
- Verifies: ConverterTests.ConvertSimpleDatalist test passes after assertion update
- Change: `Assert.Contains("branches:", yaml)` → `Assert.Contains("entries:", yaml)`
- Command: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~ConvertSimpleDatalist"`

**AC#2: EmbedConditions passes**
- Verifies: ConverterTests.EmbedConditions test passes after assertion update
- Change: `Assert.Contains("TALENT:", yaml)` → assertions matching new condition format (`type: Talent`, `talentType`, `threshold`)
- Command: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~EmbedConditions"`

**AC#3: MissingCondition passes**
- Verifies: ConverterTests.MissingCondition test passes after assertion update
- Change: `Assert.Contains("branches", yamlObject.Keys)` → `Assert.Contains("entries", yamlObject.Keys)`
- Command: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~MissingCondition"`

**AC#4: Test_ConditionalPreservation_IfWrappedPrintData passes**
- Verifies: FileConverterTests.Test_ConditionalPreservation_IfWrappedPrintData test passes after assertion update
- Changes:
  - `Assert.Contains("branches:", yamlContent)` → `Assert.Contains("entries:", yamlContent)`
  - `Assert.Contains("TALENT:", yamlContent)` → assertions matching new condition format
- Command: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~Test_ConditionalPreservation_IfWrappedPrintData"`

**AC#5: Test_ConditionalPreservation_SimplePrintData passes**
- Verifies: FileConverterTests.Test_ConditionalPreservation_SimplePrintData test passes after assertion update
- Change: `Assert.Contains("branches:", yamlContent)` → `Assert.Contains("entries:", yamlContent)`
- Command: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~Test_ConditionalPreservation_SimplePrintData"`

**AC#6: Full ErbToYaml.Tests suite passes**
- Verifies: Entire ErbToYaml.Tests project passes with 0 failures
- This is the aggregate verification that all 5 individual test fixes work together
- Command: `dotnet test tools/ErbToYaml.Tests/`
- Expected: All tests pass (exit code 0)

(AC#7 removed - test-only constraint enforced by Implementation Contract)

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Test-only assertion updates** - No converter logic changes. Update test assertions to match the new `entries:` format that F675 implemented.

**Rationale**: F675 successfully unified YAML format from legacy `branches:` to canonical `entries:` format in production converters. The converters are working correctly; only the test expectations are outdated.

**Strategy**:
1. **String assertions**: Change `branches:` to `entries:` in all `Assert.Contains()` calls
2. **Key assertions**: Change `"branches"` to `"entries"` in dictionary key checks
3. **Condition format assertions**: Update from nested TALENT format (`TALENT:`) to flat record format (`type: Talent`, `talentType`, `threshold`)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Update ConverterTests.cs line 64: Change `Assert.Contains("branches:", yaml)` to `Assert.Contains("entries:", yaml)` |
| 2 | Update ConverterTests.cs line 97: Replace `Assert.Contains("TALENT:", yaml)` with assertions matching new condition format: `Assert.Contains("type: Talent", yaml)` and `Assert.Contains("talentType", yaml)` |
| 3 | Update ConverterTests.cs line 208: Change `Assert.Contains("branches", yamlObject.Keys)` to `Assert.Contains("entries", yamlObject.Keys)` |
| 4 | Update FileConverterTests.cs lines 216, 218: Change `Assert.Contains("branches:", yamlContent)` to `Assert.Contains("entries:", yamlContent)` and `Assert.Contains("TALENT:", yamlContent)` to match new condition format |
| 5 | Update FileConverterTests.cs line 262: Change `Assert.Contains("branches:", yamlContent)` to `Assert.Contains("entries:", yamlContent)` |
| 6 | Run `dotnet test tools/ErbToYaml.Tests/` after all updates to verify entire suite passes |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Fix location | A) Update tests to match new format<br>B) Revert converters to old format<br>C) Support both formats | A | F675 already migrated production YAML files and schema to `entries:` format. Tests must align with production reality. |
| Condition assertion strategy | A) Check for exact YAML structure<br>B) Check for key indicators (`type:`, `talentType:`) | B | More resilient to formatting changes. Verifies semantic content without coupling to YAML serialization details. |
| TALENT assertion replacement | A) Single assertion for `type: Talent`<br>B) Multiple assertions for all condition fields<br>C) Parse YAML and check object structure | B | Balances verification thoroughness with test maintainability. Confirms transformation occurred without over-specifying format. |

### Assertion Changes Detail

#### ConverterTests.cs

**Test 1: ConvertSimpleDatalist (Line 64)**
```csharp
// OLD
Assert.Contains("branches:", yaml);

// NEW
Assert.Contains("entries:", yaml);
```

**Test 2: EmbedConditions (Line 97)**
```csharp
// OLD
Assert.Contains("TALENT:", yaml);

// NEW - Check for transformed condition format
Assert.Contains("type: Talent", yaml);  // Confirms condition type field exists
Assert.Contains("talentType", yaml);     // Confirms talentType field exists (value varies by test data)
```
**Justification**: BranchesToEntriesConverter.TransformCondition() converts `{ TALENT: { N: { ne: 0 } } }` to `{ type: "Talent", talentType: "N", threshold: 1 }`. We verify the presence of both `type: Talent` and `talentType` to confirm transformation occurred.

**Test 3: MissingCondition (Line 208)**
```csharp
// OLD
Assert.Contains("branches", yamlObject.Keys);

// NEW
Assert.Contains("entries", yamlObject.Keys);
```

#### FileConverterTests.cs

**Test 4: Test_ConditionalPreservation_IfWrappedPrintData (Lines 216, 218)**
```csharp
// OLD (Line 216)
Assert.Contains("branches:", yamlContent);

// NEW (Line 216)
Assert.Contains("entries:", yamlContent);

// OLD (Line 218)
Assert.Contains("TALENT:", yamlContent);

// NEW (Line 218)
Assert.Contains("type: Talent", yamlContent);  // Confirms condition transformed
```

**Test 5: Test_ConditionalPreservation_SimplePrintData (Line 262)**
```csharp
// OLD
Assert.Contains("branches:", yamlContent);

// NEW
Assert.Contains("entries:", yamlContent);
```

### Verification Plan

**Phase 1: Individual test verification** (AC#1-5)
- Run each test individually with `dotnet test --filter` to confirm fix
- Verify assertion failures are resolved
- Ensure no new failures introduced

**Phase 2: Full suite verification** (AC#6)
- Run `dotnet test tools/ErbToYaml.Tests/` to verify 0 failures
- Confirms all tests pass together (no interaction issues)

(Converter integrity verification removed - test-only constraint enforced by Implementation Contract)

### Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Condition format assertion may not match actual output | Review BranchesToEntriesConverter.TransformCondition() to confirm exact key names (`type`, `talentType`, `threshold`) |
| YAML serialization may vary (indentation, quotes) | Use substring matching (`Contains`) rather than exact string matching to tolerate formatting variations |
| New assertions may be too strict | Keep assertions minimal - verify semantic content (key presence) not formatting details |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Update ConverterTests.cs assertions (ConvertSimpleDatalist, EmbedConditions, MissingCondition) | [x] |
| 2 | 4,5 | Update FileConverterTests.cs assertions (Test_ConditionalPreservation_IfWrappedPrintData, Test_ConditionalPreservation_SimplePrintData) | [x] |
| 3 | 6 | Run full ErbToYaml.Tests suite verification | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Technical Design assertion changes | Updated test files |
| 2 | ac-tester | haiku | T3 | AC verification commands | Test results |

**Constraints** (from Technical Design):
1. Test-only changes - No converter logic modifications
2. Assertions must match entries: format from F675
3. Condition format assertions must use flat record structure (type/talentType/threshold)

**Pre-conditions**:
- F675 completed (YAML format unified to entries:)
- BranchesToEntriesConverter implemented and tested
- dialogue-schema.json updated for entries: format

**Success Criteria**:
- All 5 individual tests pass (AC#1-5)
- Full ErbToYaml.Tests suite passes with 0 failures (AC#6)
- Test-only changes (enforced by Implementation Contract constraints)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | Created | DRAFT from F696 残課題 |
| 2026-01-31 | Phase 2 | tech-investigator root cause analysis |
| 2026-01-31 | Phase 3 | ac-designer AC definition |
| 2026-01-31 | Phase 4 | tech-designer technical design |
| 2026-01-31 | Phase 5 | wbs-generator Tasks and Implementation Contract |
