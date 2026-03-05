# ERB Type Quality Guide

Issues specific to `Type: erb` features (ERB game scripts).

---

## Granularity

- **1 feature** per functional unit
- Volume limit: ~500 lines (ERB tends to be more verbose than C#)
- AC count: 5-12
- Positive AND Negative tests where applicable

---

## Common Issues

### Issue 1: Missing RETURN Rule Compliance

**Symptom**: ERB function lacks RETURN or RETURNF at required points.

**Fix**: All ERB functions must follow RETURN rules per erb-syntax skill. Read `Skill(erb-syntax)` before writing ERB.

---

### Issue 2: Variable Scope Confusion

**Symptom**: Using LOCAL vs ARG vs global variables incorrectly.

**Fix**: Document variable scope in Implementation Contract. Use LOCAL for function-internal, ARG for parameters, CFLAG/TALENT for character state.

---

### Issue 3: AC Method Uses Headless Mode Without Scenario

**Symptom**: AC expects output verification but no test scenario file specified.

**Example (Bad)**:
```markdown
| 1 | Menu displays | output | ? | contains | "選択してください" | [ ] |
```

**Example (Good)**:
```markdown
| 1 | Menu displays | output | scenario 1 | contains | "選択してください" | [ ] |
```

**Fix**: All output/variable ACs must reference a specific test scenario number. Create scenario in `test/`.

---

### Issue 4: PRINT Statement Encoding

**Symptom**: Japanese text in PRINT statements may have encoding issues in test verification.

**Fix**: Use exact Japanese strings in AC Expected values. Verify with headless mode output.

---

### Issue 5: Game State Dependency Not Documented

**Symptom**: ERB feature requires specific game state (character exists, flag set) but not documented.

**Fix**: Document required game state preconditions in AC Details or test scenario setup.

---

### Issue 6: Mandatory Handoff Destination Becomes [DONE]

**Symptom**: Mandatory Handoff points to a sibling/predecessor feature that was [PROPOSED] at /fc time but became [DONE] before /run.

**Example (Bad)**:
```markdown
| BodyDetailInit migration | ... | F778 (Body Initialization) | F778 | Task 3 |
<!-- F778 is now [DONE] — cannot accept new work -->
```

**Example (Good)**:
```markdown
| BodyDetailInit migration | ... | Feature | F{new_id} | Task {N} |
<!-- New [DRAFT] created for the migration work -->
```

**Fix**: During FL Phase 1 drift detection, if handoff destination feature is now [DONE], create a new [DRAFT] feature for the deferred work.

---

### Issue 7: ERB-to-C# Delegation Assumed Without Mechanism

**Symptom**: AC/Task assumes ERB function can directly call C# method (e.g., `CALL IBodySettings.Tidy`) but no ERB-to-C# delegation mechanism exists in the engine. ERB `CALL` can only invoke ERB `@functions`.

**Example (Bad)**:
```markdown
| 28 | @体詳細整頓 delegates to C# BodySettings.Tidy | code | Grep(体設定.ERB) | matches | `CALL.*Tidy` | [ ] |
```

**Example (Good)**:
```markdown
<!-- ERB delegation deferred to Mandatory Handoffs — no engine mechanism for ERB function override exists -->
```

**Fix**: Before creating ERB→C# delegation ACs/Tasks, verify the engine supports the delegation mechanism. If no mechanism exists, extract business logic to C# for testing only and defer ERB wiring to Mandatory Handoffs.

---

### Issue 8: CFLAG Index Space Confusion (CSV vs YAML)

**Symptom**: CharacterFlagIndex constants use CFLAG.yaml indices (name-resolution space) instead of CFLAG.csv indices (raw array space). IVariableStore.GetCharacterFlag uses `_cflags[index.Value]` — the raw array index from CFLAG.csv.

**Example (Bad)**:
```markdown
| 18 | CharacterFlagIndex HairLength | code | Grep | matches | `HairLength.*new\(1175\)` | [ ] |
<!-- 1175 is from CFLAG.yaml — wrong index space for IVariableStore -->
```

**Example (Good)**:
```markdown
| 18 | CharacterFlagIndex HairLength | code | Grep | matches | `HairLength.*new\(500\)` | [ ] |
<!-- 500 is from CFLAG.csv — matches IVariableStore raw array index -->
```

**Fix**: Always use CFLAG.csv indices for CharacterFlagIndex constants. CFLAG.yaml indices are for ERB runtime name resolution only.

---

### Issue 9: Inverted ERB Logic Requires Dedicated Behavioral ACs

**Symptom**: ERB-to-C# migration has functions with inverted semantics (e.g., a flag check with opposite polarity, or a utility returning available instead of unavailable). Generic count/existence ACs pass trivially with stub defaults but don't verify the inverted behavior.

**Example (Bad)**:
```markdown
| 47 | COM_ABLE461 test exists | code | Grep(Tests.cs) | matches | `IsAvailable461_.*Assert` | [ ] |
<!-- Single AC cannot verify both TFLAG:100 polarities -->
```

**Example (Good)**:
```markdown
| 47a | COM_ABLE461 returns true when TFLAG:100=0 | code | Grep(Tests.cs) | matches | `IsAvailable461_ReturnsTrue_WhenTFlagComableIsZero` | [ ] |
| 47b | COM_ABLE461 returns false when TFLAG:100=1 | code | Grep(Tests.cs) | matches | `IsAvailable461_ReturnsFalse_WhenTFlagComableIsSet` | [ ] |
<!-- Both polarities explicitly verified -->
```

**Fix**: For any ERB function with inverted logic (opposite polarity from sibling functions), add dedicated behavioral ACs verifying both branches. Generic assertion-existence ACs are insufficient.

---

### Issue 10: Injectable Interface Without Call-Site Count AC

**Symptom**: Feature verifies interface injection (constructor parameter AC) but not that the interface methods are actually called at all ERB call sites. An implementation can inject an interface and never call it.

**Example (Bad)**:
```markdown
| 30 | IShrinkageSystem injected | code | Grep(Handler.cs) | matches | `IShrinkageSystem` | [ ] |
<!-- Only verifies injection, not usage -->
```

**Example (Good)**:
```markdown
| 30 | IShrinkageSystem injected | code | Grep(Handler.cs) | matches | `IShrinkageSystem` | [ ] |
| 86 | UpdateShrinkage called | code | Grep(Handler.cs) | gte | `\.UpdateShrinkage\(` = 8 [I] | [ ] |
<!-- Both injection AND call-site count verified -->
```

**Fix**: For every injectable interface, add both (a) injection AC (interface name in constructor) and (b) call-site count AC (method call gte=N [I] where N = ERB call count). Applies to CSTR (SetCharacterString), TouchSet, UpdateShrinkage, and all external function abstractions.

---

### Issue 11: OR-Pattern First-Interface Omission

**Symptom**: When closing an OR-pattern gap (e.g., `A|B|C|D` with `matches`) by adding individual ACs for each alternative, the first interface in the OR is consistently forgotten. N-1 of N interfaces get individual ACs.

**Example (Bad)**:
```markdown
| 63 | SourceEntrySystem references all facades | ... | matches | `ITrainingCheckService\|IKojoMessageService\|INtrUtilityService\|IWcSexHaraService` | [ ] |
| 106 | ... IKojoMessageService | ... | matches | `IKojoMessageService` | [ ] |
| 107 | ... INtrUtilityService | ... | matches | `INtrUtilityService` | [ ] |
| 108 | ... IWcSexHaraService | ... | matches | `IWcSexHaraService` | [ ] |
<!-- ITrainingCheckService (1st in OR) omitted! -->
```

**Example (Good)**:
```markdown
| 112 | ... ITrainingCheckService | ... | matches | `ITrainingCheckService` | [ ] |
<!-- All N interfaces have individual ACs -->
```

**Fix**: When adding individual ACs for an OR-pattern, verify count matches: N alternatives in OR → N individual ACs. Check the first alternative explicitly.

---

### Issue 12: Bulk Interface Routing Uses `matches` Instead of `gte`

**Symptom**: AC verifies "30+ function calls route through interface" but uses `matches` (existence check). A single constructor injection satisfies `matches` while 29 call sites could be missing.

**Example (Bad)**:
```markdown
| 43 | ISourceCalculator referenced | ... | matches | `ISourceCalculator` | [ ] |
```

**Example (Good)**:
```markdown
| 43 | ISourceCalculator method calls | ... | gte | 30 | [ ] |
<!-- Count-based verification matching the claimed call count -->
```

**Fix**: When AC claims N+ function calls route through an interface, use `gte N` with field-access pattern (`_field\.`) instead of `matches` with type name.

---

### Issue 13: Multiline Regex Alternation Grouping

**Symptom**: Multiline co-location AC matcher uses `A|B[\s\S]{0,N}C|D` without parentheses. Due to regex alternation having lowest precedence, `B` and `D` (and bare `A`) each independently match anywhere in the file, making the AC trivially satisfiable with zero co-location enforcement.

**Example (Bad)**:
```markdown
| 91 | Ufufu co-located with TCVAR | ... | multiline matches | `Ufufu\|うふふ[\s\S]{0,1200}EjaculationLocationFlag\|GetExp` | [ ] |
<!-- GetExp matches independently — no co-location enforced -->
```

**Example (Good)**:
```markdown
| 91 | Ufufu co-located with TCVAR | ... | multiline matches | `(Ufufu\|うふふ)[\s\S]{0,1200}(EjaculationLocationFlag\|GetExp)\|(EjaculationLocationFlag\|GetExp)[\s\S]{0,1200}(Ufufu\|うふふ)` | [ ] |
<!-- Parentheses enforce co-location; bidirectional for test structure flexibility -->
```

**Fix**: Always parenthesize alternation groups in multiline matchers. Use bidirectional pattern `(group1)[\s\S]{0,N}(group2)|(group2)[\s\S]{0,N}(group1)` for co-location enforcement.

---

### Issue 14: Bare Enum Name Cross-Contamination in AC Matchers

**Symptom**: AC matcher uses bare enum member name (e.g., `Kiss`) that matches unrelated identifiers (e.g., `KissExp`, `VirginityType.Kiss`), inflating counts or producing false-positive matches.

**Example (Bad)**:
```markdown
| 24 | Dispatch tests reference 6 branches | ... | gte | `SeductiveGesture\|Kiss\|Masturbation` = 6 | [ ] |
<!-- Kiss matches KissExp, VirginityType.Kiss; Masturbation matches MasturbationExp -->
```

**Example (Good)**:
```markdown
| 24 | Dispatch tests reference 6 branches | ... | gte | `CounterActionId\.SeductiveGesture\|CounterActionId\.Kiss\|CounterActionId\.Masturbation` = 6 | [ ] |
<!-- Fully-qualified names prevent cross-contamination -->
```

**Fix**: Use fully-qualified enum references (`EnumType.Member`) in AC matchers when bare member names are substrings of other identifiers in the same file.

---

### Issue 15: SSOT Claim Without Negative Enforcement AC

**Symptom**: Philosophy claims a class is the "SSOT" (Single Source of Truth) for a responsibility, but no AC prevents competing implementations. Another class could also implement the same interface, violating the SSOT claim.

**Example (Bad)**:
```markdown
| 116 | SourceEntrySystem implements ISourceSystem | code | Grep(SourceEntrySystem.cs) | matches | `ISourceSystem` | [ ] |
<!-- Only verifies one implementation exists, not that it's the ONLY one -->
```

**Example (Good)**:
```markdown
| 116 | SourceEntrySystem implements ISourceSystem | code | Grep(SourceEntrySystem.cs) | matches | `ISourceSystem` | [ ] |
| 137 | ISourceSystem SSOT enforcement | code | Grep(Counter/Source/) | lte | `(: |, )ISourceSystem` = 1 | [ ] |
<!-- Positive + negative: exactly one implementer -->
```

**Fix**: When Philosophy claims SSOT, add a `lte 1` count AC on the interface implementation pattern across the relevant directory. This enforces the absolute claim.

---

### Issue 16: Handler Group Matcher Uses Representative ID Instead of Range

**Symptom**: AC matcher for a handler group (e.g., MESSAGE70-75) uses a single exact ID (`Message70`) instead of a character class range (`Message7[0-5]`). Tests for other handlers in the group (Message71-75) produce no matches, allowing the threshold to be satisfied without coverage of non-representative handlers.

**Example (Bad)**:
```markdown
| 23 | Simple handler coverage | code | Grep(Tests.cs) | gte | `Message70\|Message80` = 15 | [ ] |
<!-- Message71-75 and Message81-83 don't match, threshold satisfied from other groups -->
```

**Example (Good)**:
```markdown
| 23 | Simple handler coverage | code | Grep(Tests.cs) | gte | `Message7[0-5]\|Message8[0-3]` = 15 | [ ] |
<!-- Character class ranges cover all handlers in each sub-group -->
```

**Fix**: When an AC matcher counts test references across a handler group, use character class ranges (`[0-5]`, `[0-3]`) instead of single representative IDs. Verify the range matches exactly the handler IDs in the group.

---

### Issue 17: Impl/Test AC Pair Scope Mismatch

**Symptom**: Implementation-side AC covers multiple operations (e.g., `SetBase|SetDownbase`) but the corresponding test-side AC only covers a subset (e.g., `SetDownbase` only). Tests may exist for the uncovered operations but don't contribute to the threshold.

**Example (Bad)**:
```markdown
| 67 | SetBase/SetDownbase impl | code | Grep(Impl.cs) | gte | `SetBase\|SetDownbase` = 1 | [ ] |
| 18 | SetDownbase tests | code | Grep(Tests.cs) | gte | `SetDownbase` = 2 | [ ] |
<!-- SetBase tests exist but don't count toward AC#18 -->
```

**Example (Good)**:
```markdown
| 67 | SetBase/SetDownbase impl | code | Grep(Impl.cs) | gte | `SetBase\|SetDownbase` = 1 | [ ] |
| 18 | SetBase/SetDownbase tests | code | Grep(Tests.cs) | gte | `SetBase\|SetDownbase` = 2 | [ ] |
<!-- Both ACs cover the same operation scope -->
```

**Fix**: When creating impl/test AC pairs, ensure the test-side pattern covers the same scope as the impl-side pattern. Verify by comparing the OR-alternation members in both patterns.

---

### Issue 18: AC Verifies Pre-Existing Infrastructure, Not Feature Work

**Symptom**: AC greps for an interface or registration that already exists before the feature starts (e.g., `IInputHandler` from a previous feature). The AC always passes regardless of whether the current feature uses it correctly.

**Example (Bad)**:
```markdown
| 6 | IInputHandler interface exists | code | Grep(Era.Core/Input/) | matches | `interface IInputHandler` | [ ] |
<!-- IInputHandler already exists — AC passes without any F822 work -->
```

**Example (Good)**:
```markdown
| 6 | IInputHandler injected into BirthProcess | code | Grep(Era.Core/State/BirthProcess.cs) | matches | `IInputHandler` | [ ] |
<!-- Verifies the current feature's actual usage of the pre-existing interface -->
```

**Fix**: When a feature depends on a pre-existing interface/registration, verify *injection into the new code* rather than *existence of the pre-existing artifact*. The AC should fail if the feature doesn't use the dependency, not just if the dependency doesn't exist.

---

## Checklist

- [ ] RETURN rules followed per erb-syntax skill
- [ ] Variable scope (LOCAL/ARG/global) documented
- [ ] All output ACs reference test scenario number
- [ ] Japanese text encoding verified in test output
- [ ] Game state preconditions documented
- [ ] ERB file placement follows existing directory structure
- [ ] PRINT/PRINTL usage matches expected output format
- [ ] AC matchers use specific identifiers (not broad patterns)
- [ ] Test scenarios in test/ with proper setup
- [ ] Mandatory Handoff destinations are not [DONE] features
- [ ] ERB-to-C# delegation ACs verified against actual engine mechanism
- [ ] CharacterFlagIndex uses CFLAG.csv indices (raw array), not CFLAG.yaml indices
- [ ] Inverted logic functions have dedicated behavioral ACs for both polarities
- [ ] Injectable interfaces have both injection AC AND call-site count AC
- [ ] OR-pattern individual ACs cover ALL N alternatives (check first interface explicitly)
- [ ] Bulk routing ACs use `gte N` count, not `matches` existence
- [ ] Multiline regex alternation groups are parenthesized for co-location enforcement
- [ ] Bare enum names in matchers use fully-qualified `EnumType.Member` form to prevent cross-contamination
- [ ] SSOT claims in Philosophy have negative enforcement ACs (`lte 1` on interface implementations)
- [ ] Handler group matchers use character class ranges (`[0-5]`) not single representative IDs
- [ ] Impl/test AC pairs cover the same operation scope (OR-alternation members match)
- [ ] ACs verify feature's own work, not pre-existing infrastructure (inject into new code, not existence of old interface)
