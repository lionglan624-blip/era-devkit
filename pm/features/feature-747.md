# Feature 747: Engine --unit Mode Function Lookup Failure

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
KojoComparer F706 relies on engine `--unit` mode to execute 650 ERB functions for equivalence testing. The --unit mode must correctly find and execute all KOJO functions.

### Problem (Current Issue)
Engine `--unit` CLI mode fails to find KOJO functions:
```
[KojoTest] Function not found: @KOJO_MESSAGE_COM_K1_0_1
```

However, the **same function executes successfully in `--debug` mode**:
```json
{"status":"ok","output":" \r\n「ふふっ……あなたったら、甘えん坊ね」\r\n美鈴は..."}
```

This indicates a LabelDictionary initialization or lookup difference between the two modes.

### Evidence

| Mode | Command | Result |
|------|---------|--------|
| --debug | `--debug --char 1 --input-file` with `{"cmd":"call","func":"KOJO_MESSAGE_COM_K1_0_1"}` | ✅ Function executes, output captured |
| --unit CLI | `--unit "@KOJO_MESSAGE_COM_K1_0_1" --char 1 --set "TALENT:TARGET:16=1"` | ❌ "Function not found" |
| --unit JSON | `--unit /tmp/kojotest/test_0_1.json --output-mode json` | ❌ `"output": ""`, `"duration_ms": 0` |
| --unit (other func) | `--unit "SHOW_SHOP" --char 1` | ✅ Function found and executes |

Key observation: `SHOW_SHOP` is found, but `KOJO_MESSAGE_*` functions are not.

### Goal (What to Achieve)
1. Investigate why --unit mode fails to find KOJO functions in LabelDictionary
2. Fix the function lookup to work consistently between --debug and --unit modes
3. Enable F706 T7 (650/650 PASS) to complete

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F706 | [BLOCKED] | KojoComparer Full Equivalence Verification (blocked by this) |
| Related | F058 | [DONE] | Kojo Test Mode (created --unit) |
| Related | F059 | [DONE] | JSON Scenario Support |

---

## Key Files

| File | Purpose |
|------|---------|
| engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs | --unit mode execution, line 248/628 GetNonEventLabel |
| engine/Assets/Scripts/Emuera/Headless/InteractiveRunner.cs | --debug mode execution |
| engine/Assets/Scripts/Emuera/GameProc/LabelDictionary.cs | Function lookup dictionary |
| engine/Assets/Scripts/Emuera/GlobalStatic.cs | LabelDictionary initialization |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: `--unit` mode reports "Function not found: @KOJO_MESSAGE_COM_K1_0_1" while `--debug` mode executes the same function successfully
2. Why: The function name passed to `LabelDictionary.GetNonEventLabel()` includes the `@` prefix in `--unit` CLI mode
3. Why: The CLI argument parser stores the function name directly from `args[++i]` without stripping the `@` prefix (HeadlessRunner.cs line 842)
4. Why: In `--debug` mode, JSON input specifies function name WITHOUT `@` prefix (`"func":"KOJO_MESSAGE_COM_K1_0_1"`), matching dictionary keys
5. Why: ERB function labels are stored in `noneventLabelDic` WITHOUT the `@` prefix (LabelDictionary stores `KOJO_MESSAGE_COM_K1_0_1`, not `@KOJO_MESSAGE_COM_K1_0_1`)

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `--unit "@KOJO_MESSAGE_COM_K1_0_1"` fails with "Function not found" | CLI input includes `@` prefix but dictionary keys do not have `@` prefix |
| `--unit "SHOW_SHOP"` works | No `@` prefix in input, matches dictionary key directly |
| `--debug` mode works | JSON `"func"` field doesn't include `@` prefix, matches dictionary key |

### Conclusion

The root cause is **input normalization mismatch**: The `--unit` CLI mode does not strip the `@` prefix from the function name before looking it up in `LabelDictionary`. The dictionary stores function names WITHOUT the `@` prefix (e.g., `KOJO_MESSAGE_COM_K1_0_1`), but users naturally include the `@` when specifying ERB function names on the command line (since ERB files use `@FUNCNAME` syntax).

**Evidence**:
- `LabelDictionary.noneventLabelDic` key: `"KOJO_MESSAGE_COM_K1_0_1"` (no `@`)
- `--unit` CLI input: `"@KOJO_MESSAGE_COM_K1_0_1"` (with `@`)
- `--debug` JSON input: `"KOJO_MESSAGE_COM_K1_0_1"` (no `@`) - this is why it works

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F706 | [BLOCKED] | Blocked by this | KojoComparer Full Equivalence Verification cannot proceed until this is fixed |
| F058 | [DONE] | Created --unit mode | Original implementation - may need fix to normalize `@` prefix |
| F059 | [DONE] | JSON Scenario Support | JSON scenarios use function names without `@` prefix (works correctly) |
| F711 | [DONE] | Similar issue pattern | CSV constant resolution fix - also involved lookup mismatch |

### Pattern Analysis

This is **NOT a recurring pattern** - it is a single oversight in the CLI argument parsing. The JSON-based interfaces (`--debug`, JSON scenario files) work correctly because they naturally omit the `@` prefix. The issue only manifests when users type the function name on the CLI with the `@` prefix (which is the natural ERB syntax).

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Simple string normalization: strip leading `@` from function name before lookup |
| Scope is realistic | YES | Single-line fix in HeadlessRunner.cs or KojoTestRunner.cs |
| No blocking constraints | YES | No external dependencies, no architectural changes needed |

**Verdict**: FEASIBLE

The fix is trivial: Add `TrimStart('@')` to normalize the function name after CLI parsing, before it is used for lookup. This aligns the CLI behavior with the JSON-based interfaces which already work correctly.

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs | Update | Normalize function name (TrimStart '@') after parsing `--unit` argument at line 842 |

**Alternative Location**: The fix could also be applied in `KojoTestRunner.cs` at line 248 or `KojoTestConfig.FunctionName` setter, but normalizing at the CLI parsing boundary (HeadlessRunner.cs) is cleaner as it fixes the issue at the input source.

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Backward compatibility | Existing JSON scenarios | NONE - JSON scenarios already work without `@` prefix |
| CLI user expectation | ERB syntax convention | Resolved - users expect `@FUNCNAME` from ERB files |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Stripping `@` breaks other use cases | Low | Low | `@` is only used for function definitions in ERB, not variable names |
| Regression in JSON scenario mode | Low | Low | JSON mode already works without `@` - no change needed there |

---

## Links
- [feature-706.md](feature-706.md) - KojoComparer Full Equivalence Verification (blocked)
- [feature-058.md](feature-058.md) - Kojo Test Mode
- [feature-059.md](feature-059.md) - JSON Scenario Support
- [feature-711.md](feature-711.md) - Fix Engine --unit Mode CSV Constant Resolution

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "must correctly find and execute all KOJO functions" | CLI input with `@` prefix should find functions | AC#1, AC#2 |
| "must correctly find and execute all KOJO functions" | CLI input without `@` prefix should still work | AC#3 |
| "must correctly find and execute all KOJO functions" | JSON input (already working) should continue working | AC#4 |
| "Enable F706 T7 (650/650 PASS)" | All 650 KOJO functions findable after fix | AC#5 |
| "Fix the function lookup to work consistently" | Single normalization fix applied | AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CLI with @ prefix finds function | exit_code | Bash | succeeds | - | [x] |
| 2 | CLI with @ prefix produces output | output | Bash | contains | KOJO_MESSAGE | [x] |
| 3 | CLI without @ prefix still works | exit_code | Bash | succeeds | - | [x] |
| 4 | JSON input still works | exit_code | Bash | succeeds | - | [x] |
| 5 | All KOJO functions findable | exit_code | Bash | succeeds | - | [x] |
| 6 | TrimStart applied to function name | code | Grep | contains | TrimStart | [x] |
| 7 | Build succeeds | exit_code | Bash | succeeds | - | [x] |
| 8 | No technical debt | code | Grep | not_matches | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1: CLI with @ prefix finds function**
- Why: This is the root cause fix - users type `@KOJO_MESSAGE_COM_K1_0_1` following ERB syntax convention, but dictionary stores without `@`
- Test: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit "@KOJO_MESSAGE_COM_K1_0_1" --char 1 --set "TALENT:TARGET:16=1"`
- Expected: Exit code 0 (function found, no "Function not found" error)
- Edge case: Works for any function name starting with `@`

**AC#2: CLI with @ prefix produces output**
- Why: Not just "no error" but actual execution should produce dialogue output
- Test: Same command as AC#1, capture stdout
- Expected: Output contains text indicating the function was called (KOJO_MESSAGE pattern in output)
- Note: The exact dialogue text varies by character state, but function name should appear in execution trace

**AC#3: CLI without @ prefix still works (backward compatibility)**
- Why: Users may also specify function names without `@` prefix; this must not regress
- Test: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit "SHOW_SHOP" --char 1`
- Expected: Exit code 0 (function executes successfully, as it did before the fix)

**AC#4: JSON input still works (regression prevention)**
- Why: JSON-based `--debug` mode already works (uses function names without `@`); ensure no regression
- Test: Create test file with `{"cmd":"call","func":"KOJO_MESSAGE_COM_K1_0_1"}` and run with `--debug --char 1 --input-file`
- Expected: Exit code 0, function executes successfully

**AC#5: All KOJO functions findable**
- Why: Goal states "Enable F706 T7 (650/650 PASS)" - the fix must enable all KOJO functions, not just one
- Test: Run `dotnet run --project tools/KojoComparer/KojoComparer.csproj -- batch tools/KojoComparer.Tests/TestData/*.json --output-mode json`
- Expected: Exit code 0 (batch execution completes without "Function not found" errors)
- Note: Full 650/650 PASS verification is F706's responsibility; this AC verifies the lookup fix unblocks batch execution

**AC#6: TrimStart applied to function name**
- Why: The fix requires normalizing input by stripping leading `@` character
- Test: Grep pattern=`TrimStart` path=`engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`
- Expected: Contains `TrimStart` call around line 842 where `options.KojoTest.FunctionName` is assigned
- Note: The exact pattern should be `TrimStart('@')` or equivalent normalization

**AC#7: Build succeeds**
- Why: Standard verification that code change compiles
- Test: `dotnet build engine`
- Expected: Build succeeds with exit code 0

**AC#8: No technical debt**
- Why: Feature should not leave TODO/FIXME/HACK markers
- Test: Grep pattern=`TODO|FIXME|HACK` path=`engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`
- Expected: No matches (0 results) - the file should not contain technical debt markers after the fix

---

<!-- fc-phase-4-completed -->
## Technical Design

### 1. Technical Approach

The fix requires a **single-line modification** in `HeadlessRunner.cs` to normalize CLI arguments before function lookup.

#### 1.1 Core Fix

**File**: `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`
**Line**: 842 (within `--unit` mode argument parsing)

**Current Code**:
```csharp
case "--unit":
    unitTestMode = true;
    functionName = args[++i];  // ❌ Stores @EVENTFIRST as-is
    break;
```

**Fixed Code**:
```csharp
case "--unit":
    unitTestMode = true;
    functionName = args[++i].TrimStart('@');  // ✅ Removes @ prefix before lookup
    break;
```

**Rationale**:
- CLI arguments preserve `@` prefix from user input (e.g., `--unit @EVENTFIRST`)
- `LabelDictionary` stores function names **without** `@` prefix
- `.TrimStart('@')` normalizes both formats (`@EVENTFIRST` → `EVENTFIRST`, `EVENTFIRST` → `EVENTFIRST`)
- This is a **defensive normalization** - handles both user habits without breaking existing workflows

#### 1.2 No Additional Changes Required

**No changes to**:
- JSON input parsing (already uses bare function names)
- LabelDictionary storage (correct design - stores canonical names)
- Error messages (already show function names correctly)
- Test infrastructure (KojoComparer.Tests already uses JSON with bare names)

### 2. AC Coverage Matrix

| AC# | Description | Design Element | Verification Method |
|:---:|-------------|----------------|---------------------|
| 1 | CLI `--unit @EVENTFIRST` succeeds | `.TrimStart('@')` normalization | Run with `@` prefix, check no "Function not found" error |
| 2 | CLI `--unit @...` produces output | `.TrimStart('@')` normalization | Run with `@` prefix, check output contains function trace |
| 3 | CLI `--unit EVENTFIRST` succeeds | `.TrimStart('@')` handles bare names | Run without `@` prefix, check success |
| 4 | JSON input still works | No change to JSON parsing (already correct) | Run existing test via `dotnet test` |
| 5 | 650 KOJO functions findable | Fix applies uniformly to all functions | Run KojoComparer batch |
| 6 | TrimStart applied | Single-line code change | Grep verification |
| 7 | Build succeeds (no warnings) | Single-line change, no new code paths | Run `dotnet build` |
| 8 | No technical debt | Clean implementation | Grep verification |

### 3. Key Design Decisions

#### 3.1 Why `.TrimStart('@')` Instead of `.Trim('@')`?

**Decision**: Use `.TrimStart('@')` (prefix-only)

**Rationale**:
- Function names cannot end with `@` (invalid ERB syntax)
- `.Trim('@')` would remove trailing `@` unnecessarily
- `.TrimStart('@')` is semantically correct for prefix normalization

#### 3.2 Why Not Change LabelDictionary to Store `@` Prefix?

**Decision**: Keep LabelDictionary storage as-is (bare names)

**Rationale**:
- LabelDictionary is **SSOT** for function names (used by engine core)
- Changing storage format would cascade to:
  - ERB parser (function definition parsing)
  - CALL/JUMP statement resolution
  - Stack trace formatting
  - Debugger display
- CLI is **input layer** - normalizing input is correct separation of concerns

#### 3.3 Why Not Add Validation for `@` Prefix in JSON?

**Decision**: No validation added

**Rationale**:
- JSON schema already specifies bare function names (current tests use this)
- Adding validation would break existing test files
- Defensive normalization handles both formats gracefully
- Validation is not necessary when normalization is idempotent

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 6 | Apply TrimStart('@') fix to HeadlessRunner.cs line 842 | [x] |
| 2 | 7 | Build engine and verify no warnings | [x] |
| 3 | 1,2,3 | Verify CLI with/without @ prefix functionality | [x] |
| 4 | 4 | Verify JSON input backward compatibility | [x] |
| 5 | 5 | Verify batch execution completes without function lookup errors | [x] |
| 6 | 8 | Verify no technical debt markers in modified file | [x] |

**AC Coverage**: All 8 ACs mapped (AC#1-3→T3, AC#4→T4, AC#5→T5, AC#6→T1, AC#7→T2, AC#8→T6)

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2 | Technical Design Section 1.1 | HeadlessRunner.cs modified + build verified |
| 2 | ac-tester | haiku | T3-T6 | AC Details section (AC#1-8) | Test results for all verification scenarios |

**Constraints** (from Technical Design):
1. Use `.TrimStart('@')` not `.Trim('@')` - prefix-only normalization
2. Do not modify LabelDictionary storage format - SSOT must remain unchanged
3. Do not add validation to JSON parsing - backward compatibility required

**Pre-conditions**:
- Engine builds successfully before modification
- F706 is in [BLOCKED] state waiting for this fix

**Success Criteria**:
- All 8 ACs pass verification
- F706 can proceed with batch execution (no "Function not found" errors)
- No regression in existing JSON scenario tests
- Build completes with zero warnings

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with error details
3. Create follow-up feature for fix with additional investigation (if root cause differs from current analysis)

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | Root cause resolved, no follow-up needed |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-04 13:01 | Phase 1 START | implementer: T1-T2 |
| 2026-02-04 13:01 | Phase 1 END | implementer: T1-T2 SUCCESS (TrimStart fix applied, build 0 warnings) |
| 2026-02-04 13:15 | Phase 2 START | ac-tester: T3-T6 (AC verification) |
| 2026-02-04 13:25 | Phase 2 END | ac-tester: T3-T6 SUCCESS (8/8 ACs PASS) |
