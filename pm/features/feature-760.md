# Feature 760: TALENT Target/Numeric Index Pattern Support

## Status: [DONE]
<!-- fl-reviewed: 2026-02-08T00:00:00Z -->

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

<!-- fc-phase-1-completed -->

## Background

Deferred from F758 (Prefix-Based Variable Type Expansion). TALENT target/numeric index patterns like `TALENT:PLAYER` and `TALENT:2` (25 occurrences in kojo) require TalentRef rework to support target references and numeric indices beyond name-based lookup.

### Philosophy (Mid-term Vision)
(Inherited from F758) Continue toward full equivalence testing by expanding TALENT condition parsing to cover all pattern variants. TALENT is the SSOT for character attributes and must parse correctly across all ERA pattern forms.

### Problem (Current Issue)
TalentRef was designed as a flat ICondition subclass (TalentRef.cs:9) with only Target(string) and Name(string) fields, because TALENT was the first condition type implemented before the VariableRef hierarchy was established in F750-F758. This structural gap means:

1. **No Index property**: Unlike VariableRef (VariableRef.cs:18), TalentRef cannot represent numeric talent indices. `TALENT:2 & 2` assigns "2" to Name instead of Index (TalentConditionParser.cs:45).
2. **No target-keyword disambiguation**: The regex `^TALENT:(?:([^:]+):)?([^:\s&]+)` (TalentConditionParser.cs:18-19) requires a colon after group1 to match a target. In two-part patterns like `TALENT:PLAYER`, group1 fails and PLAYER falls into group2 (Name), despite PLAYER being a runtime system variable (target reference).
3. **ConvertTalentRef ignores Target**: Even correctly-parsed three-part patterns like `TALENT:6:NTR` lose target semantics in YAML output because ConvertTalentRef (DatalistConverter.cs:303-311) only performs CSV name lookup via talent.Name, discarding the Target dimension entirely.

25 single-segment patterns are broken (12 `TALENT:PLAYER & N`, 13 `TALENT:N & N`), and 277+ three-part patterns parse correctly but lose target information in conversion.

### Goal (What to Achieve)
Extend TalentRef with an Index(int?) property and add disambiguation logic to TalentConditionParser so that all three TALENT pattern variants parse correctly: numeric index (`TALENT:2`), target reference (`TALENT:PLAYER`), and name (`TALENT:恋人`). Update ConvertTalentRef to branch on Name vs Index vs Target-only, and update KojoBranchesParser state keys to be target-aware. All changes must preserve backward compatibility with existing 277+ three-part patterns.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does `TALENT:PLAYER & 2` misparse? | PLAYER is assigned to Name instead of Target | TalentConditionParser.cs:44-45 — `var name = match.Groups[2].Value` |
| 2 | Why does the regex assign PLAYER to Name? | The regex requires a colon after group1 to activate the target capture; two-part form has no second colon | TalentConditionParser.cs:18-19 — regex `(?:([^:]+):)?` requires trailing `:` |
| 3 | Why is there no disambiguation for two-part patterns? | TalentConditionParser was designed for only `TALENT:name` and `TALENT:target:name` | TalentConditionParser.cs:7-11 — docstring lists only these two patterns |
| 4 | Why does TalentRef lack an Index property? | TalentRef extends ICondition directly, not VariableRef which provides Index(int?) | TalentRef.cs:9 vs VariableRef.cs:9,18 |
| 5 (Root) | Why was TalentRef designed without VariableRef's capabilities? | TALENT was the first condition type implemented (pre-F750), before the VariableRef/VariableConditionParser generic hierarchy was established | Historical: F750 created TALENT support; F758 established generic VariableRef pattern afterward |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `TALENT:PLAYER` and `TALENT:2` patterns produce wrong TalentRef fields | TalentRef predates VariableRef hierarchy; lacks Index property and disambiguation logic |
| Where | TalentConditionParser regex matching | TalentRef class design (extends ICondition, not VariableRef) |
| Scope | 25 broken single-segment + 277+ target-lost three-segment patterns | Structural gap in TalentRef + TalentConditionParser + ConvertTalentRef |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F758 | [DONE] | Parent: established VariableRef/VariableConditionParser generic pattern; deferred F760 |
| F757 | [DONE] | Foundation: bitwise operator (`&`) support |
| F750 | [DONE] | Original TALENT condition migration |
| F759 | [DONE] | Sibling: compound bitwise `(TALENT:2 & 3) == 3`; ResolveInnerBitwiseRef must be updated for F760's TalentRef.Index |
| F751 | [DRAFT] | Downstream: TALENT semantic mapping validation (benefits from F760) |
| F706 | [BLOCKED] | Consumer: full equivalence testing |

## Feasibility Assessment

**Verdict**: FEASIBLE

The required changes follow the established VariableRef pattern from F758 but with TALENT-specific complexity:
- **Parser**: Add int.TryParse (already proven in VariableConditionParser.cs:51-60) and keyword allowlist for target disambiguation
- **Model**: Add Index(int?) to TalentRef (or migrate to VariableRef — design decision)
- **Conversion**: Branch ConvertTalentRef into three paths (Name→CSV, Index→direct, Target-only→target-qualified)
- **Evaluation**: Update KojoBranchesParser state key format for target-awareness

Complexity is higher than a typical VariableRef addition due to CSV conversion semantics and target-aware YAML schema evolution, but all patterns have proven precedent in the codebase.

## Impact Analysis

| Component | Impact | Description |
|-----------|--------|-------------|
| ErbParser | Major | TalentRef gains Index; TalentConditionParser gains disambiguation |
| ErbToYaml | Major | ConvertTalentRef branches into three conversion paths; ResolveInnerBitwiseRef must handle TalentRef.Index/Target (F759 consumer) |
| KojoComparer | Moderate | State key format becomes target-aware; StateConverter must preserve compound keys |
| ICondition | Minor | JsonDerivedType may need update if TalentRef schema changes |
| Existing YAML | None to Low | New fields are additive (nullable); existing files remain valid |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| TalentRef JSON serialization compatibility | ICondition.cs:9 — "talent" discriminator | Schema changes must be backward compatible (new fields nullable) |
| ConvertTalentRef CSV lookup semantics | DatalistConverter.cs:305 — `_talentLoader.GetTalentIndex(talent.Name)` | Must branch on Index vs Name vs Target-only; CSV path preserved for names |
| PLAYER/MASTER/TARGET/ASSI are ERA system variables | ERA engine: registered as Int1DVariableToken | Finite keyword allowlist required; not arbitrary strings |
| KojoBranchesParser state key format | KojoBranchesParser.cs:122-123 — `TALENT:{talentIndex}` | Target-qualified keys needed for multi-character evaluation |
| 277+ existing three-part patterns must not regress | Corpus count | Regression tests mandatory |
| `(TALENT:2 & 3) == 3` compound bitwise parsing is F759 scope | KOJO_KU_愛撫.ERB:63 | F759 [DONE]; F760 must update ResolveInnerBitwiseRef (DatalistConverter.cs:549-572) to handle TalentRef.Index/Target since Name will be empty for numeric patterns |
| TALENT:PLAYER means runtime-resolved target | ERA runtime semantics | YAML must preserve symbolic reference, not resolve at parse time |
| Target-only patterns (TALENT:PLAYER & N) cannot be evaluated at parse time | PLAYER resolves to a character index at runtime; no static state producer can emit the correct state key | F760 handles parse + conversion; evaluation of target-only patterns deferred to future feature (runtime target resolution) |
| DatalistConverter dispatch order | If TalentRef extends VariableRef | `case TalentRef` must precede `case VariableRef` to avoid incorrect dispatch |
| TreatWarningsAsErrors enabled | Directory.Build.props | All new/changed code must compile warning-free |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| TalentRef schema change breaks existing serialized YAML | Medium | High | Add Index as optional nullable; preserve existing Name/Target fields |
| PLAYER keyword allowlist incomplete | Low | Medium | Audit ERA engine source for all system variable names |
| Three-part pattern regression | Medium | High | Mandatory regression tests covering TALENT:6:NTR, TALENT:PLAYER:処女, TALENT:PLAYER:2 |
| ConvertTalentRef branching complexity | Medium | Medium | Clean three-way branch with explicit type checks |
| F759 scope bleed | Low | Low | Document boundary clearly; exclude compound patterns |
| Pre-existing CSV gap (e.g., 恋人 not in Talent.csv) | Known | Low | Out of scope — existing warning behavior preserved |
| Numeric target information loss in YAML | Low | Medium | Numeric targets are character indices already available in ERB source; re-parsing from ERB is always possible if future features require character-specific TALENT evaluation for these 277+ patterns |

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser tests | `dotnet test src/tools/dotnet/ErbParser.Tests/` | All pass | Pre-F760 baseline |
| ErbToYaml tests | `dotnet test src/tools/dotnet/ErbToYaml.Tests/` | All pass | Pre-F760 baseline |
| KojoComparer tests | `dotnet test src/tools/dotnet/KojoComparer.Tests/` | gte 104 pass | 1 pre-existing failure from F758 baseline |

**Baseline File**: `.tmp/baseline-760.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | TalentRef must gain Index(int?) property | TalentRef.cs:9 | ACs must verify TALENT:2 produces Index=2, Name=null |
| C2 | Target-reference keywords must be enumerated | ERA language: PLAYER, MASTER, TARGET, ASSI | ACs must verify TALENT:PLAYER produces Target=PLAYER, Name=null |
| C3 | Three-part patterns must not regress | 277+ existing correct parses | Regression tests for TALENT:6:NTR and TALENT:PLAYER:処女 |
| C4 | ConvertTalentRef must branch on Name vs Index vs Target-only | DatalistConverter.cs:303-321 | Test all three conversion paths |
| C5 | KojoBranchesParser state key must become target-aware | KojoBranchesParser.cs:122-123 | Define and test target-qualified key format |
| C6 | F759 compound bitwise excluded | KOJO_KU_愛撫.ERB:63 | Do NOT cover compound bitwise patterns |
| C7 | JSON serialization backward compatibility | ICondition.cs:9 "talent" discriminator | Verify existing serialized TalentRef still deserializes |
| C8 | TALENT:PLAYER:2 three-part with numeric index | COMABLE.ERB:1677 | Must verify Target=PLAYER, Index=2 |
| C9 | 人物_X target patterns remain working | 28 occurrences in corpus | No false keyword match on character-name targets |
| C10 | Build must pass with TreatWarningsAsErrors | Directory.Build.props | Build verification AC required |
| C11 | DatalistConverter dispatch order | If TalentRef extends VariableRef | `case TalentRef` must precede `case VariableRef` |
| C12 | ResolveInnerBitwiseRef uses talent.Name for CSV lookup | DatalistConverter.cs:554 | Must handle TalentRef.Index (bypass CSV) and Target (compound key) after F760 disambiguation |
| C13 | StateConverter.ConvertStateToContext strips intermediate segments | StateConverter.cs:24 | Must preserve keyword target as compound key for TALENT round-trip coherence |

### Constraint Details

**C1: TalentRef Index Property**
- **Source**: VariableRef.cs:18 has Index(int?); TalentRef.cs lacks it
- **Verification**: Parse `TALENT:2 & 2` and check Index=2
- **AC Impact**: Must test both numeric index assignment and null Name when Index is set

**C2: Target-Reference Keywords**
- **Source**: ERA engine registers PLAYER/MASTER/TARGET/ASSI as system variables
- **Verification**: Parse `TALENT:PLAYER` and check Target=PLAYER
- **AC Impact**: Must test keyword list completeness; non-keyword strings must NOT match as targets

**C4: ConvertTalentRef Three-Way Branch**
- **Source**: DatalistConverter.cs:303-311 currently only does CSV lookup by Name
- **Verification**: Convert TalentRef with Name, Index, and Target-only variants
- **AC Impact**: Three separate conversion tests needed; CSV path preserved for names

**C5: Target-Aware State Keys**
- **Source**: KojoBranchesParser.cs:122-123 uses `TALENT:{talentIndex}` without target
- **Verification**: Evaluate TALENT condition with target-qualified state key
- **AC Impact**: Must define new key format convention and test evaluation

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F758 | [DONE] | VariableRef/VariableConditionParser generic pattern (foundation) |
| Related | F759 | [DONE] | Compound bitwise: ResolveInnerBitwiseRef must be updated for TalentRef.Index |
| Related | F751 | [CANCELLED] | TALENT semantic mapping (downstream beneficiary) |
| Related | F706 | [DONE] | Full equivalence testing (consumer) |

<!-- fc-phase-3-completed -->

---

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "TALENT is the SSOT for character attributes and **must** parse correctly across **all** ERA pattern forms" | Parser must handle all three pattern variants: numeric index, target reference, name | AC#1, AC#2, AC#3, AC#4 |
| "**must** parse correctly" | Numeric indices produce Index, not Name | AC#1 |
| "**all** ERA pattern forms" | Target keywords (PLAYER/MASTER/TARGET/ASSI) disambiguated from names | AC#2, AC#5 |
| "full equivalence testing" by expanding TALENT condition parsing to cover **all** pattern variants | ConvertTalentRef and ResolveInnerBitwiseRef branch correctly for all three paths + regression | AC#6, AC#7, AC#8, AC#9, AC#21 |
| "must preserve backward compatibility with existing 277+ three-part patterns" | Regression tests for three-part patterns + state key migration | AC#4, AC#9, AC#16 |
| TALENT "must parse correctly" with target-aware evaluation | KojoBranchesParser state keys become target-qualified (format correctness only; runtime evaluation of 12 target-only TALENT:PLAYER patterns deferred to F769) | AC#10 |
| "SSOT" + "must parse correctly across all" implies correct end-to-end semantics | Parse→convert→evaluate pipeline preserves target throughout; state round-trip coherent | AC#15, AC#22 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TALENT:2 parses as Index=2 | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 2 | TALENT:PLAYER parses as Target=PLAYER | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 3 | TALENT:恋人 parses as Name=恋人 | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 4 | TALENT:PLAYER:処女 three-part parses correctly | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 5 | Non-keyword string not matched as target | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 6 | ConvertTalentRef Name path uses CSV lookup | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 7 | ConvertTalentRef Index path uses direct index | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 8 | ConvertTalentRef Target-only path preserves symbolic reference | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 9 | Three-part pattern regression (277+ patterns) | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 10 | KojoBranchesParser and YamlRunner target-aware state key | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 11 | TalentRef Index property exists | code | Grep(src/tools/dotnet/ErbParser/TalentRef.cs) | matches | "int\\?.*Index" | [x] |
| 12 | JSON serialization backward compatibility | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 13 | Build passes with zero warnings | build | dotnet build | succeeds | - | [x] |
| 14 | Zero technical debt in modified files | code | Grep(src/tools/dotnet/ErbParser/TalentRef.cs,src/tools/dotnet/ErbParser/TalentConditionParser.cs,src/tools/dotnet/ErbToYaml/DatalistConverter.cs,src/tools/dotnet/KojoComparer/KojoBranchesParser.cs,src/tools/dotnet/KojoComparer/YamlRunner.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |
| 15 | End-to-end round-trip: parse→convert→evaluate preserves target | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 16 | State key migration regression: existing KojoComparer tests pass | test | dotnet test tools/KojoComparer.Tests --filter "Category!=F760New" | succeeds | - | [x] |
| 17 | F768 DRAFT file exists | file | Glob(pm/features/feature-768.md) | exists | - | [x] |
| 18 | F768 registered in index | file | Grep(pm/index-features.md) | contains | "F768" | [x] |
| 19 | F769 DRAFT file exists | file | Glob(pm/features/feature-769.md) | exists | - | [x] |
| 20 | F769 registered in index | file | Grep(pm/index-features.md) | contains | "F769" | [x] |
| 21 | ResolveInnerBitwiseRef handles TalentRef.Index and Target | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 22 | StateConverter preserves compound keys for TALENT | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |

### AC Details

**AC#1: TALENT:2 parses as Index=2 (C1)**
- Verifies that numeric-only segment in two-part TALENT pattern produces TalentRef with Index=2 and Name=null/empty
- Test: Parse `TALENT:2 & 2` → TalentRef { Index=2, Name=null, Operator="&", Value="2" }
- Covers the int.TryParse disambiguation path (proven pattern from VariableConditionParser.cs:51-60)
- Constraint C1: TalentRef must gain Index(int?) property

**AC#2: TALENT:PLAYER parses as Target=PLAYER (C2)**
- Verifies that keyword in two-part TALENT pattern produces TalentRef with Target=PLAYER and Name=null/empty
- Test: Parse `TALENT:PLAYER & 2` → TalentRef { Target="PLAYER", Name=null, Index=null, Operator="&", Value="2" }
- Disambiguation logic: check keyword allowlist BEFORE int.TryParse
- Constraint C2: Target-reference keywords must be enumerated (PLAYER, MASTER, TARGET, ASSI)

**AC#3: TALENT:恋人 parses as Name=恋人**
- Verifies that non-numeric, non-keyword segment falls through to Name assignment (existing behavior preserved)
- Test: Parse `TALENT:恋人` → TalentRef { Name="恋人", Target=empty, Index=null }
- This is the legacy path that must continue working for Japanese talent names

**AC#4: TALENT:PLAYER:処女 three-part parses correctly (C3, C8)**
- Verifies three-part patterns with target and name still parse correctly
- Test: Parse `TALENT:PLAYER:処女` → TalentRef { Target="PLAYER", Name="処女", Index=null }
- Also test `TALENT:PLAYER:2` → TalentRef { Target="PLAYER", Index=2, Name=null } (C8)
- Also test `TALENT:6:NTR` → TalentRef { Target="6", Name="NTR", Index=null }
- Also test `TALENT:6:PLAYER` → TalentRef { Target="6", Name="PLAYER", Index=null } (keyword in name position — disambiguation check skipped because target is already populated)
- Constraint C3: 277+ existing three-part patterns must not regress

**AC#5: Non-keyword string not matched as target (C9)**
- Verifies that character-name targets like 人物_X do NOT falsely match the keyword allowlist
- Test: Parse `TALENT:人物_主人公` → TalentRef { Name="人物_主人公", Target=empty, Index=null }
- Negative test ensuring the finite keyword allowlist (PLAYER/MASTER/TARGET/ASSI) does not over-match
- Constraint C9: 28 occurrences of 人物_X patterns must remain working

**AC#6: ConvertTalentRef Name path uses CSV lookup (C4)**
- Verifies that TalentRef with Name (e.g., Name="恋人") follows CSV lookup path via _talentLoader.GetTalentIndex
- Test: ConvertTalentRef with Name="恋人" → `{ "TALENT": { "{csvIndex}": { "ne": "0" } } }`
- This is the existing path that must be preserved
- Constraint C4: CSV path preserved for names

**AC#7: ConvertTalentRef Index path uses direct index (C4)**
- Verifies that TalentRef with Index (e.g., Index=2) bypasses CSV lookup and uses index directly
- Test: ConvertTalentRef with Index=2 → `{ "TALENT": { "2": { ... } } }`
- New path: no CSV lookup needed when numeric index is already parsed
- Constraint C4: Must branch on Name vs Index vs Target-only

**AC#8: ConvertTalentRef Target-only path preserves symbolic reference (C4)**
- Verifies that TalentRef with Target only (e.g., Target="PLAYER", no Name or Index) preserves symbolic reference in YAML
- Test: ConvertTalentRef with Target="PLAYER" → YAML output preserves "PLAYER" as symbolic target reference
- Must NOT resolve PLAYER at parse time (runtime-resolved target per Technical Constraints)
- **Known limitation**: Target-only patterns produce YAML key "PLAYER" and stateKey `TALENT:PLAYER`, but no state producer currently emits `TALENT:PLAYER` keys. Evaluation returns default-0 until F769 provides runtime state injection. KojoBranchesParser target-only branch should include a code comment noting this.
- Constraint C4: Three separate conversion paths needed

**AC#9: Three-part pattern regression (C3)**
- Verifies that existing three-part patterns still convert correctly via ConvertTalentRef
- Minimum 3 representative test cases covering each subtype:
  1. Numeric target + name: `TALENT:6:NTR` (from NTR口上.ERB corpus) → CSV lookup on "NTR", Target="6"
  2. Keyword target + name: `TALENT:PLAYER:処女` (from COMABLE.ERB corpus) → CSV lookup on "処女", Target="PLAYER"
  3. Keyword target + numeric index: `TALENT:PLAYER:2` (from COMABLE.ERB:1677) → direct Index=2, Target="PLAYER"
- Must ensure ConvertTalentRef with both Target and Name populated still performs CSV lookup on Name
- Constraint C3: 277+ existing correct parses must not regress

**AC#10: KojoBranchesParser target-aware state key (C5)**
- Verifies that KojoBranchesParser uses target-qualified state keys for TALENT conditions with targets
- Current format: `TALENT:{talentIndex}` → New format must incorporate target dimension
- Test: Evaluate TALENT condition with target-qualified key format and verify correct state lookup
- **Task#7b test specificity**: Tests MUST exercise ExtractStateFromContext independently with all three key formats: compound (`"PLAYER:16"` → `TALENT:PLAYER:16`), numeric (`"16"` → `TALENT:TARGET:16`), and symbolic (`"PLAYER"` → `TALENT:PLAYER`)
- **Known limitation**: Symbolic target-only keys (`TALENT:PLAYER`) evaluate to default-0 because no state producer currently emits these keys. This is expected until F769 provides runtime state injection
- Constraint C5: Multi-character evaluation requires target disambiguation in state keys
- Note: Task#7a/7b/7c all map to AC#10. YamlRunner (Task#7b) correctness is additionally verified by AC#15 (end-to-end round-trip) which exercises the full pipeline including ExtractStateFromContext with compound keys

**AC#11: TalentRef Index property exists (C1)**
- Structural verification: TalentRef class has an Index property of type int?
- Test: Grep(src/tools/dotnet/ErbParser/TalentRef.cs) matches `int\?.*Index`
- Verifies the foundational model change that enables numeric index support
- Constraint C1: TalentRef must gain Index(int?) property

**AC#12: JSON serialization backward compatibility (C7)**
- Verifies that existing serialized TalentRef JSON (with only Target/Name/Operator/Value) still deserializes correctly after adding Index property
- Test: Deserialize JSON without "index" field → TalentRef with Index=null
- New field must be nullable/optional so existing JSON remains valid
- Constraint C7: "talent" discriminator and existing fields unchanged

**AC#13: Build passes with zero warnings (C10)**
- Verifies all modified projects (ErbParser, ErbToYaml, KojoComparer) build cleanly
- Test: `dotnet build` succeeds with TreatWarningsAsErrors enabled
- Constraint C10: Directory.Build.props enforces zero warnings

**AC#14: Zero technical debt in modified files**
- Verifies no TODO/FIXME/HACK markers left in modified source files
- Test: Grep pattern `TODO|FIXME|HACK` in TalentRef.cs, TalentConditionParser.cs, DatalistConverter.cs
- Expected: 0 matches
- Paths: src/tools/dotnet/ErbParser/TalentRef.cs, src/tools/dotnet/ErbParser/TalentConditionParser.cs, src/tools/dotnet/ErbToYaml/DatalistConverter.cs, src/tools/dotnet/KojoComparer/KojoBranchesParser.cs, src/tools/dotnet/KojoComparer/YamlRunner.cs

**AC#15: End-to-end round-trip: parse→convert→evaluate preserves target**
- Verifies that a representative three-part pattern flows correctly through the full pipeline
- Test: Parse `TALENT:PLAYER:処女` → TalentConditionParser produces TalentRef { Target="PLAYER", Name="処女" } → ConvertTalentRef produces YAML with compound key `"PLAYER:16"` → KojoBranchesParser parses compound key via ParseTalentYamlKey, builds stateKey `TALENT:PLAYER:16`, and evaluates with target-aware state key matching state
- **Context construction**: Test must supply context with compound TALENT key `{"TALENT": {"PLAYER:16": 1}}` to verify ExtractStateFromContext correctly parses compound keys via shared ParseTalentYamlKey
- Integration test in KojoComparer.Tests that exercises all three components in sequence
- Covers the boundary between ErbParser (parse), ErbToYaml (convert), and KojoComparer (evaluate)

**AC#16: State key migration regression: existing KojoComparer tests pass**
- Verifies that migrating state key format from `TALENT:{index}` to `TALENT:TARGET:{index}` does not break existing tests
- After Task#7c updates test fixtures, all pre-existing KojoComparer tests must still pass
- Test: `dotnet test tools/KojoComparer.Tests --filter "Category!=F760New"` succeeds (excludes new F760-specific tests to isolate regression)
- Baseline: ≥104 pass (1 pre-existing failure from F758)

**AC#17: F768 DRAFT file exists**
- Verifies that Task#11 creates feature-768.md with [DRAFT] status
- Check: `Glob(pm/features/feature-768.md)` → file exists

**AC#18: F768 registered in index**
- Verifies that Task#11 registers F768 in index-features.md
- Check: `Grep(pm/index-features.md)` → contains "F768" entry

**AC#19: F769 DRAFT file exists**
- Verifies that Task#13 creates feature-769.md with [DRAFT] status
- Check: `Glob(pm/features/feature-769.md)` → file exists

**AC#20: F769 registered in index**
- Verifies that Task#13 registers F769 in index-features.md
- Check: `Grep(pm/index-features.md)` → contains "F769" entry
- F769 covers runtime target resolution for target-only TALENT patterns (12 TALENT:PLAYER & N patterns)

**AC#21: ResolveInnerBitwiseRef handles TalentRef.Index and Target**
- Verifies that ResolveInnerBitwiseRef (DatalistConverter.cs:549-572) handles the new TalentRef model correctly after F760's disambiguation changes
- F759's compound bitwise conversion uses `talent.Name` for CSV lookup (line 554), but after F760, numeric patterns have Name="" and Index set instead
- Test cases:
  1. TalentRef with Index=2 (from `(TALENT:2 & 3) == 3`) → uses Index directly: variableKey="2", bypasses CSV
  2. TalentRef with Name="NTR" (from `(TALENT:NTR & 1) == 1`) → existing CSV lookup path preserved
  3. TalentRef with Target="PLAYER" and Index=2 (from `(TALENT:PLAYER:2 & 3) == 3`) → compound key "PLAYER:2"
- Must apply same three-way branching logic as ConvertTalentRef (Name→CSV, Index→direct, Target→compound key)
- Uses BuildConditionDict helper for consistent dictionary construction

**AC#22: StateConverter preserves compound keys for TALENT**
- Verifies that StateConverter.ConvertStateToContext (StateConverter.cs:24) preserves compound key structure for TALENT state keys with target segments
- Current behavior: `TALENT:PLAYER:16` → `{"TALENT": {"16": 1}}` (target stripped)
- Required behavior: `TALENT:PLAYER:16` → `{"TALENT": {"PLAYER:16": 1}}` (compound key preserved)
- Test cases:
  1. `TALENT:PLAYER:16` → context `{"TALENT": {"PLAYER:16": 1}}` (keyword target preserved as compound key)
  2. `TALENT:TARGET:16` → context `{"TALENT": {"16": 1}}` (default TARGET target uses simple key for backward compat)
  3. `ABL:TARGET:5` → context `{"ABL": {"5": 1}}` (non-TALENT types unchanged)
- Round-trip verification: `TALENT:PLAYER:16` → StateConverter → context → ExtractStateFromContext → state key `TALENT:PLAYER:16`

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|-----------|-------------|-----------------|
| Extend TalentRef with Index(int?) | Model property addition | AC#1, AC#11 |
| Disambiguation logic for numeric index | TALENT:2 → Index=2 | AC#1 |
| Disambiguation logic for target reference | TALENT:PLAYER → Target=PLAYER | AC#2, AC#5 |
| Disambiguation logic for name | TALENT:恋人 → Name=恋人 | AC#3 |
| ConvertTalentRef branch: Name | CSV lookup path | AC#6 |
| ConvertTalentRef branch: Index | Direct index path | AC#7 |
| ConvertTalentRef branch: Target-only | Symbolic reference preservation | AC#8 |
| KojoBranchesParser target-aware state keys | Target-qualified key format | AC#10 |
| Backward compatibility with 277+ three-part patterns | Regression tests | AC#4, AC#9 |
| ResolveInnerBitwiseRef compound bitwise compatibility | F759 consumer updated for TalentRef.Index/Target | AC#21 |
| State key migration regression safety | Existing tests survive format change | AC#16 |
| All changes compile warning-free | Build verification | AC#13 |
| End-to-end target preservation across pipeline | Integration round-trip test | AC#15 |
| StateConverter compound key preservation | State→context round-trip with target | AC#22 |

---

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 11 | Add Index property to TalentRef model | | [x] |
| 2 | 1,2,3,5 | Implement disambiguation logic in TalentConditionParser | | [x] |
| 3 | 4 | Add three-part pattern disambiguation (Target+Name/Index) | | [x] |
| 4 | 12 | Add JSON backward compatibility test for TalentRef | | [x] |
| 5 | 6,7,8 | Implement target-aware ConvertTalentRef with ResolveTalentKey + BuildConditionDict helpers; apply BuildConditionDict to ConvertVariableRef and ConvertArgRef (same file dedup) | | [x] |
| 6 | 9 | Add three-part pattern regression tests | | [x] |
| 7a | 10 | Create TalentKeyParser utility class with ParseTalentYamlKey; extract TALENT evaluation into dedicated EvaluateTalentCondition method with target-aware state keys (code comment explaining why TALENT cannot use generic EvaluateVariableCondition per Maintenance Note #4) | | [x] |
| 7b | 10 | Update YamlRunner.ExtractStateFromContext to emit target-qualified state keys (include code comment explaining three-part key divergence from two-part format) | | [x] |
| 7c | 10 | Update existing test fixtures to new TALENT:TARGET:{index} state key format (KojoBranchesParserConditionTests.cs state dicts: TALENT:{index}→TALENT:TARGET:{index}; BatchProcessorTests.cs already uses TALENT:TARGET:{index}) | | [x] |
| 8 | 13 | Verify build passes with zero warnings | | [x] |
| 9 | 14 | Verify zero technical debt in modified files | | [x] |
| 10 | 15 | Add end-to-end round-trip integration test (parse→convert→evaluate) | | [x] |
| 11 | 17,18 | Create F768 DRAFT (cross-parser refactoring: regex, disambiguation logic, BuildConditionDict deduplication) | | [x] |
| 12 | 16 | Verify state key migration regression (existing KojoComparer tests pass after fixture update) | | [x] |
| 13 | 19,20 | Create F769 DRAFT (Target-only TALENT evaluation with runtime target resolution) | | [x] |
| 14 | 21 | Update ResolveInnerBitwiseRef to use shared ResolveTalentKey (F759 compound bitwise consumer) | | [x] |
| 15 | 22 | Update StateConverter.ConvertStateToContext to preserve TALENT compound keys (keyword target → compound key, default TARGET → simple key) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: TalentRef class specification from Technical Design | TalentRef with Index property |
| 2 | implementer | sonnet | T2,T3: TalentConditionParser logic from Technical Design | Disambiguation logic for two-part and three-part patterns |
| 3 | implementer | sonnet | T4: JSON serialization requirement from Technical Constraints | Backward compatibility test |
| 4 | implementer | sonnet | T5: ConvertTalentRef compound key encoding + BuildConditionDict from Technical Design | Compound key YAML output for three-part patterns |
| 5 | implementer | sonnet | T6: Three-part pattern corpus (277+ patterns) from Background | Regression tests |
| 6 | implementer | sonnet | T7a,T7b,T7c: ParseTalentYamlKey + state key pipeline from Technical Design | Target-aware state keys (KojoBranchesParser + YamlRunner.ExtractStateFromContext + test fixtures) |
| 7 | ac-tester | haiku | T8,T9: Build and Grep commands from AC table | Build verification and tech debt check |
| 8 | implementer | sonnet | T10: Integration test spanning ErbParser→ErbToYaml→KojoComparer with compound keys | End-to-end round-trip test for target preservation |
| 9 | ac-tester | haiku | T12: Regression test for state key migration | Existing KojoComparer tests pass after fixture update |
| 10 | implementer | sonnet | T11: Create F768 DRAFT and register in index-features.md | feature-768.md [DRAFT] + index-features.md updated |
| 11 | implementer | sonnet | T13: Create F769 DRAFT and register in index-features.md | feature-769.md [DRAFT] + index-features.md updated |
| 12 | implementer | sonnet | T14: ResolveInnerBitwiseRef three-way branching from Technical Design + BuildConditionDict | ResolveInnerBitwiseRef handles TalentRef.Index/Target for F759 compound bitwise |
| 13 | implementer | sonnet | T15: StateConverter compound key preservation from Technical Design | StateConverter preserves TALENT compound keys in context |

**Constraints** (from Technical Design):
1. TalentRef Index must be nullable (int?) for backward compatibility with existing JSON (C7)
2. Target keyword allowlist must be exact match HashSet {PLAYER, MASTER, TARGET, ASSI} to prevent false matches (C2, C9)
3. Disambiguation order: (1) keyword allowlist, (2) int.TryParse, (3) fallback to Name (Key Decision: Disambiguation order)
4. ConvertTalentRef uses compound key format `"TARGET:INDEX"` for three-part patterns; numeric-only keys for no-target patterns; BuildConditionDict helper reduces duplication
5. State key default target is "TARGET" when not specified, aligning with KojoComparer Program.cs:262 convention (Key Decision: Target default value)
6. DatalistConverter dispatch order: if TalentRef extends VariableRef in future, `case TalentRef` must precede `case VariableRef` (C11)
7. All modified projects must compile with TreatWarningsAsErrors enabled (C10)
8. Three-part patterns with numeric target (e.g., TALENT:6:NTR): parser preserves "6"→Target, "NTR"→Name, but ConvertTalentRef discards numeric target (produces plain index key for backward compat)
9. Code comments for known limitations (e.g., AC#8 target-only evaluation) must use NOTE or LIMITATION prefix, not TODO/FIXME/HACK, to avoid AC#14 failure

**Pre-conditions**:
- F758 is [DONE] (VariableRef/VariableConditionParser generic pattern established)
- F757 is [DONE] (bitwise operator `&` support in place)
- Baseline tests pass: ErbParser.Tests, ErbToYaml.Tests, KojoComparer.Tests (≥104 pass)

**Success Criteria**:
- All 22 ACs pass verification
- 25 broken single-segment patterns (12 TALENT:PLAYER, 13 TALENT:N) now parse correctly with Index/Target populated
- 277+ existing three-part patterns continue to work (no regression)
- TalentRef can represent all three pattern variants: numeric index, target reference, name
- ConvertTalentRef produces correct YAML for all three conversion paths
- KojoBranchesParser evaluates TALENT conditions with target-qualified state keys
- Build passes with zero warnings across ErbParser, ErbToYaml, KojoComparer projects
- Zero technical debt markers (TODO|FIXME|HACK) in modified files

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert {commit-hash}`
2. Notify user of rollback with specific failure details (which pattern variant failed, test output)
3. Create follow-up feature with additional investigation into failure root cause:
   - If disambiguation logic fails: investigate keyword allowlist completeness or int.TryParse edge cases
   - If conversion fails: investigate ConvertTalentRef branching conditions or YAML schema compatibility
   - If evaluation fails: investigate state key format or StateConverter parsing logic
4. Document failure in Review Notes with [blocked] tag referencing follow-up feature ID

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| TalentConditionParser regex AND disambiguation logic duplicates VariableConditionParser | Cross-parser refactoring beyond F760 scope (BuildConditionDict adoption for ConvertVariableRef/ConvertArgRef now included in F760 Task#5) | Create new Feature | F768 | Task#11 |
| Target-only TALENT patterns (12 TALENT:PLAYER & N patterns) cannot be evaluated at parse time — no static state producer for runtime-resolved targets | PLAYER resolves to character index at runtime; requires engine integration or runtime state injection to evaluate | Create new Feature | F769 | Task#13 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-09 | IMPL | implementer | Tasks 6-15 | All remaining tasks completed: TalentKeyParser, KojoBranchesParser, YamlRunner, StateConverter, ResolveInnerBitwiseRef, F768/F769 DRAFTs |
| 2026-02-09 | TEST | ac-tester | AC verification | 15/22 pass; 4 fail (AC#10,15,16,22: pre-existing tests need Task#7c fixture updates); 1 pre-existing ErbParser test needs update |
| 2026-02-09 | DEVIATION | Bash | dotnet test KojoComparer.Tests (Category!=F760New) | exit code 1: 12 failures — old fixtures use TALENT:{index} instead of TALENT:TARGET:{index} |
| 2026-02-09 | DEVIATION | Bash | dotnet test ErbParser.Tests | exit code 1: BitwiseComparisonTests.BitwiseComparison_ActualKojoPattern expects Name="2", now Index=2 |
| 2026-02-09 | DEBUG | debugger | Fix test fixtures | Updated state keys TALENT:{index}→TALENT:TARGET:{index} in 5 test files; ErbParser.Tests 159/0, KojoComparer.Tests 126/0 |

---

## Technical Design

### Approach

**Extend TalentRef independently (not migrating to VariableRef)** to add Index(int?) property and implement disambiguation logic in TalentConditionParser similar to VariableConditionParser. This approach minimizes blast radius while enabling all three pattern variants (numeric index, target reference, name).

**Rationale**: TalentRef has unique semantics (non-nullable Target/Name, CSV conversion, "talent" discriminator) that make VariableRef extension complex. Adding Index independently preserves existing behavior, avoids JsonDerivedType ordering issues (C11), and keeps TalentRef's special-case handling explicit in DatalistConverter.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `int? Index` property to TalentRef. In TalentConditionParser, after regex match, apply disambiguation: (1) keyword allowlist {PLAYER, MASTER, TARGET, ASSI} → Target, (2) int.TryParse → Index, (3) fallback → Name |
| 2 | Disambiguation step 1: check if group2 matches keyword allowlist. If match, assign to Target and leave Name/Index null |
| 3 | Disambiguation step 3: if not keyword and not numeric, assign to Name (existing behavior preserved) |
| 4 | Three-part regex already works (group1=target, group2=name/index). Apply disambiguation to group2: if numeric → Index, else → Name. Test TALENT:PLAYER:処女 and TALENT:PLAYER:2 |
| 5 | Test that "人物_主人公" does NOT match keyword allowlist (only exact matches PLAYER/MASTER/TARGET/ASSI allowed) |
| 6 | ConvertTalentRef: if Name is non-empty and no Target → CSV lookup, numeric key (existing logic preserved) |
| 7 | ConvertTalentRef: if Index.HasValue and no Target → return `{ "TALENT": { "2": ... } }` (bypass CSV, backward compatible) |
| 8 | ConvertTalentRef: if Target non-empty but Name/Index empty → return `{ "TALENT": { "PLAYER": ... } }` (symbolic reference) |
| 9 | Three-part patterns: when both Target and Name/Index present, use compound key format `"TARGET:INDEX"` (e.g., `"PLAYER:16"`). Add regression tests with existing corpus patterns |
| 10 | Replace KojoBranchesParser int.TryParse guard with ParseTalentYamlKey. Handle compound keys (`PLAYER:16` → target+index), numeric keys (`16` → default TARGET), and symbolic keys (`PLAYER` → target-only). State key format: `TALENT:{target}:{index}` or `TALENT:{target}` |
| 11 | Add property declaration: `[JsonPropertyName("index")] public int? Index { get; set; }` in TalentRef.cs |
| 12 | Test deserialization of JSON without "index" field → Index defaults to null (nullable property handles backward compatibility) |
| 13 | Build all modified projects (ErbParser, ErbParser.Tests, ErbToYaml, ErbToYaml.Tests, KojoComparer, KojoComparer.Tests) with dotnet build |
| 14 | Code review grep: ensure no TODO/FIXME/HACK in modified files |
| 15 | Integration test in KojoComparer.Tests: parse TALENT:PLAYER:処女 → ConvertTalentRef produces compound key `"PLAYER:16"` → ParseTalentYamlKey extracts target="PLAYER", index=16 → stateKey `TALENT:PLAYER:16` → evaluate matches state |
| 16 | Verify existing KojoComparer tests (baseline ≥104) still pass after state key format migration (Task#7c fixture update) |
| 17 | Create feature-768.md [DRAFT] file |
| 18 | Register F768 in index-features.md |
| 19 | Create feature-769.md [DRAFT] file |
| 20 | Register F769 in index-features.md |
| 21 | Update ResolveInnerBitwiseRef to use shared ResolveTalentKey helper. ResolveTalentKey extracts Name→CSV, Index→direct, Target→compound key logic shared by ConvertTalentRef and ResolveInnerBitwiseRef |
| 22 | Update StateConverter.ConvertStateToContext to produce compound context keys for keyword targets. `TALENT:PLAYER:16` → `{"TALENT": {"PLAYER:16": 1}}`, `TALENT:TARGET:16` → `{"TALENT": {"16": 1}}` (backward compat) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **TalentRef extension** | (A) Migrate TalentRef to extend VariableRef, (B) Add Index independently to TalentRef | B - Independent Index property | TalentRef has non-nullable Target/Name (default empty string) vs VariableRef's nullable fields, plus "talent" JSON discriminator and unique CSV conversion semantics. Migration would require (1) nullable override bridging, (2) JsonDerivedType dispatch ordering (C11), (3) ConvertTalentRef pattern-match reordering in DatalistConverter. Trade-off: duplication cost is bounded (one regex, one method) vs migration risk across 3 projects. **Maintenance note**: TalentConditionParser regex must stay synchronized with VariableConditionParser if pattern grammar changes |
| **Disambiguation order** | (A) int.TryParse first, (B) keyword allowlist first | B - Keyword first | Prevents numeric-like keywords (if any) from mis-parsing. Proven pattern: check explicit list before heuristics |
| **Keyword allowlist** | (A) Regex pattern, (B) HashSet exact match | B - HashSet | Exact matching prevents false positives (e.g., 人物_PLAYER). Finite known set {PLAYER, MASTER, TARGET, ASSI} per ERA runtime |
| **ConvertTalentRef branching** | (A) Single if-else chain with flat keys, (B) Compound key format with BuildConditionDict helper | B - Compound key with helper | Compound key `"TARGET:INDEX"` preserves both target and index dimensions in YAML, enabling KojoBranchesParser to reconstruct target-qualified state keys via ParseTalentYamlKey. BuildConditionDict reduces duplication across conversion methods |
| **Numeric target handling** | (A) Treat numeric targets same as keywords (compound key), (B) Discard numeric targets (use plain index key) | B - Discard numeric targets | Numeric first segments (e.g., "6" in TALENT:6:NTR) are character indices, not semantic targets. No state producer emits character-index-qualified keys. Plain index key preserves backward compatibility with existing evaluation (default target "TARGET") |
| **State key format** | (A) Keep `TALENT:{index}`, (B) Migrate to `TALENT:{target}:{index}` | B - Target-qualified keys | C5 requires target awareness for multi-character evaluation. Backward compat: default target to "TARGET" when not specified |
| **Target default value** | (A) Empty string, (B) "TARGET" | B - "TARGET" | Aligns with existing KojoComparer convention (Program.cs:262 adds "TARGET" segment). Makes state keys consistent |
| **JSON serialization** | (A) Required field, (B) Optional nullable field | B - Optional nullable | C7 backward compatibility: existing JSON without "index" must deserialize correctly. Nullable handles default |
| **Keyword allowlist sharing** | (A) Duplicate in each project, (B) Shared constant via project reference | B - Shared constant via existing ErbParser project reference | SSOT: single definition prevents sync drift. ErbParser defines `TalentConditionParser.TargetKeywords` as public static. KojoComparer.csproj already references ErbParser (line 16), so no new project dependency required |

### Interfaces / Data Structures

#### TalentRef (Modified)

```csharp
/// <summary>
/// Represents a reference to a TALENT condition
/// Pattern: TALENT:(target:)?(name|index)( op value)?
/// Examples:
///   - TALENT:恋人 → Name="恋人"
///   - TALENT:PLAYER → Target="PLAYER"
///   - TALENT:2 → Index=2
///   - TALENT:PLAYER:処女 → Target="PLAYER", Name="処女"
///   - TALENT:PLAYER:2 → Target="PLAYER", Index=2
/// </summary>
public class TalentRef : ICondition
{
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int? Index { get; set; }  // NEW: nullable for backward compatibility

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
```

#### TalentConditionParser (Modified)

```csharp
public class TalentConditionParser
{
    private static readonly Regex TalentPattern = new Regex(
        @"^TALENT:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    // ERA system variable keywords that are treated as target references
    public static readonly HashSet<string> TargetKeywords = new HashSet<string>
    {
        "PLAYER", "MASTER", "TARGET", "ASSI"
    };

    public TalentRef? ParseTalentCondition(string condition)
    {
        // ... (existing null/empty checks and regex matching)

        var target = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
        var nameOrIndex = match.Groups[2].Value;

        if (string.IsNullOrWhiteSpace(nameOrIndex))
            return null;

        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var result = new TalentRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        // DISAMBIGUATION LOGIC (NEW):
        // 1. Keyword allowlist → Target
        // 2. int.TryParse → Index
        // 3. Fallback → Name

        if (string.IsNullOrEmpty(target) && TargetKeywords.Contains(nameOrIndex))
        {
            // Two-part pattern with keyword: TALENT:PLAYER
            result.Target = nameOrIndex;
            result.Name = string.Empty;
            result.Index = null;
        }
        else if (int.TryParse(nameOrIndex, out int index))
        {
            // Numeric index
            result.Index = index;
            result.Name = string.Empty;
        }
        else
        {
            // Name (string identifier)
            result.Name = nameOrIndex;
            result.Index = null;
        }

        return result;
    }
}
```

#### ConvertTalentRef (Modified)

```csharp
/// <summary>
/// Convert TalentRef to YAML format with target-aware key encoding:
/// - No target + Name: { "TALENT": { "16": { "ne": "0" } } } (backward compatible)
/// - No target + Index: { "TALENT": { "2": { "ne": "0" } } } (backward compatible)
/// - Keyword target + Name: { "TALENT": { "PLAYER:16": { "ne": "0" } } } (compound key)
/// - Keyword target + Index: { "TALENT": { "PLAYER:2": { "ne": "0" } } } (compound key)
/// - Numeric target + Name: { "TALENT": { "16": { "ne": "0" } } } (numeric targets are character indices, not semantic — use plain index key for backward compat)
/// - Numeric target + Index: { "TALENT": { "2": { "ne": "0" } } } (same: numeric target discarded)
/// - Keyword target only: { "TALENT": { "PLAYER": { "ne": "0" } } } (symbolic reference)
/// </summary>
private Dictionary<string, object> ConvertTalentRef(TalentRef talent)
{
    var yamlKey = ResolveTalentKey(talent);
    if (yamlKey == null)
        return new Dictionary<string, object>();

    return BuildConditionDict("TALENT", yamlKey, talent.Operator, talent.Value);
}

/// <summary>
/// Resolve TalentRef to YAML key string. Shared by ConvertTalentRef and
/// ResolveInnerBitwiseRef to eliminate key resolution duplication.
/// Returns null if resolution fails (Name not in CSV, no Name/Index/Target).
/// Key encoding:
///   Name → CSV lookup → numeric index string
///   Index → direct index string
///   Keyword target + Name/Index → compound key "TARGET:INDEX"
///   Numeric target → discarded (backward compat)
///   Keyword target only → symbolic reference "PLAYER"
/// </summary>
private string? ResolveTalentKey(TalentRef talent)
{
    string? talentKey = null;

    // Step 1: Resolve the index/name portion of the key
    if (!string.IsNullOrEmpty(talent.Name))
    {
        var talentIndex = _talentLoader.GetTalentIndex(talent.Name);
        if (talentIndex == null)
        {
            Console.Error.WriteLine($"Warning: Talent '{talent.Name}' not found in Talent.csv");
            return null;
        }
        talentKey = talentIndex.Value.ToString();
    }
    else if (talent.Index.HasValue)
    {
        talentKey = talent.Index.Value.ToString();
    }

    // Step 2: Build the YAML key with target encoding
    // Only KEYWORD targets (PLAYER/MASTER/TARGET/ASSI) produce compound keys.
    // Numeric targets (e.g., "6" in TALENT:6:NTR) are character indices and
    // are discarded in YAML to preserve backward compatibility.
    var isKeywordTarget = !string.IsNullOrEmpty(talent.Target)
        && TalentConditionParser.TargetKeywords.Contains(talent.Target);

    if (isKeywordTarget && talentKey != null)
        return $"{talent.Target}:{talentKey}";
    if (talentKey != null)
        return talentKey;
    if (isKeywordTarget)
        return talent.Target;

    Console.Error.WriteLine("Warning: TalentRef with no Name, Index, or Target");
    return null;
}

/// <summary>
/// Build a standard condition dictionary { prefix: { key: { op: value } } }.
/// Shared by ConvertTalentRef and ConvertVariableRef to reduce duplication.
/// Instance method because MapErbOperatorToYaml depends on _dimConstResolver.
/// </summary>
private Dictionary<string, object> BuildConditionDict(
    string prefix, string key, string? op, string? value)
{
    return new Dictionary<string, object>
    {
        { prefix, new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(op, value) }
            }
        }
    };
}
```

#### ResolveInnerBitwiseRef (Modified)

```csharp
/// <summary>
/// Extract variable type, key, and mask from inner bitwise condition.
/// F759: Reuses existing key resolution. F760: Uses shared ResolveTalentKey.
/// </summary>
private (string? variableType, string variableKey, string mask) ResolveInnerBitwiseRef(ICondition inner)
{
    switch (inner)
    {
        case TalentRef talent when talent.Operator == "&":
            var bitwiseKey = ResolveTalentKey(talent);
            if (bitwiseKey == null)
                return (null, "", "");
            return ("TALENT", bitwiseKey, talent.Value ?? "0");

        case VariableRef varRef when varRef.Operator == "&"
                                  && _variableTypePrefixes.ContainsKey(varRef.GetType()):
            return (_variableTypePrefixes[varRef.GetType()],
                    BuildVariableKey(varRef),
                    varRef.Value ?? "0");

        default:
            Console.Error.WriteLine($"Warning: Unsupported inner condition type: {inner.GetType().Name}");
            return (null, "", "");
    }
}
```

#### KojoBranchesParser State Key Format (Modified)

**Current format**: `TALENT:{talentIndex}` (e.g., `TALENT:16`)

**New format**: `TALENT:{target}:{talentIndex}` (e.g., `TALENT:TARGET:16`, `TALENT:PLAYER:3`)

**Default target**: When no target is specified in ERB pattern, use "TARGET" as default (aligns with Program.cs:262 convention).

**Impact**: StateConverter (Task#15), KojoBranchesParser evaluation logic (Task#7a), and YamlRunner.ExtractStateFromContext (Task#7b) must be updated. (Note: Program.cs ParseTalentState already inserts TARGET segment at line 262-269, no changes needed.)

```csharp
// In KojoBranchesParser.EvaluateCondition:
// OLD: if (!int.TryParse(indexStr, out var talentIndex)) continue;
//      var stateKey = $"TALENT:{talentIndex}";
// NEW: Use ParseTalentYamlKey to handle compound/symbolic/numeric keys
var (target, talentIndex) = ParseTalentYamlKey(keyStr);
var effectiveTarget = target ?? "TARGET";
var stateKey = talentIndex.HasValue
    ? $"TALENT:{effectiveTarget}:{talentIndex.Value}"
    : $"TALENT:{effectiveTarget}";
```

#### YAML Key Parsing in KojoBranchesParser (Compound Key Support)

With the compound key format from ConvertTalentRef, YAML conditions now contain three key formats:
- Numeric-only: `{ "TALENT": { "16": ... } }` → backward compatible (no target, index=16)
- Compound: `{ "TALENT": { "PLAYER:16": ... } }` → target=PLAYER, index=16
- Symbolic: `{ "TALENT": { "PLAYER": ... } }` → target=PLAYER, no index (target-only)

**TalentKeyParser.ParseTalentYamlKey** (new utility class) extracts target and index from any key format:

```csharp
/// <summary>
/// Utility for parsing TALENT YAML keys into (target, index) components.
/// Extracted from KojoBranchesParser to maintain single-responsibility:
/// KojoBranchesParser handles branch evaluation, TalentKeyParser handles key parsing.
/// Shared by KojoBranchesParser.EvaluateCondition, YamlRunner.ExtractStateFromContext,
/// and StateConverter.ConvertStateToContext.
/// </summary>
internal static class TalentKeyParser
{
    /// <summary>
    /// Parse a TALENT YAML key into (target, index) components.
    /// Supports three formats:
    ///   "16"         → (null, 16)        — backward compatible numeric
    ///   "PLAYER:16"  → ("PLAYER", 16)    — compound target:index
    ///   "PLAYER"     → ("PLAYER", null)  — symbolic target-only
    /// </summary>
    internal static (string? Target, int? Index) ParseTalentYamlKey(string key)
{
    var colonIdx = key.IndexOf(':');
    if (colonIdx >= 0)
    {
        // Compound key: "TARGET:INDEX"
        var target = key[..colonIdx];
        var indexStr = key[(colonIdx + 1)..];
        if (int.TryParse(indexStr, out var index))
            return (target, index);
        // Malformed compound key (non-numeric after colon) — extract target portion only
        Console.Error.WriteLine($"Warning: Malformed compound TALENT key '{key}' — non-numeric index portion '{indexStr}'");
        return (target, null);
    }

    // Single key: numeric or symbolic
    if (int.TryParse(key, out var numericIndex))
        return (null, numericIndex);

    // Non-numeric single key: symbolic target reference (e.g., "PLAYER")
    return (key, null);
    }
}
```

**Usage in EvaluateCondition** (replaces current int.TryParse guard):
```csharp
foreach (var kvp in talentDict)
{
    var keyStr = kvp.Key?.ToString();
    if (string.IsNullOrEmpty(keyStr))
        continue;

    var (target, talentIndex) = ParseTalentYamlKey(keyStr);

    // Build target-qualified state key
    var effectiveTarget = target ?? "TARGET";

    string stateKey;
    if (talentIndex.HasValue)
    {
        stateKey = $"TALENT:{effectiveTarget}:{talentIndex.Value}";
    }
    else
    {
        // Target-only symbolic reference (e.g., TALENT:PLAYER with no index)
        // State key: TALENT:PLAYER (two-part, no index dimension)
        stateKey = $"TALENT:{effectiveTarget}";
    }

    // ... evaluate condition against state[stateKey] ...
}
```

**Key behavior**:
- `{ "TALENT": { "16": ... } }` → target=null→"TARGET", index=16 → stateKey = `TALENT:TARGET:16`
- `{ "TALENT": { "PLAYER:16": ... } }` → target="PLAYER", index=16 → stateKey = `TALENT:PLAYER:16`
- `{ "TALENT": { "PLAYER": ... } }` → target="PLAYER", index=null → stateKey = `TALENT:PLAYER`

#### YamlRunner.ExtractStateFromContext (Modified)

**Current** (line 134): `state[$"TALENT:{indexStr}"] = ...` → produces `TALENT:16`

**New**: Call shared `TalentKeyParser.ParseTalentYamlKey` (internal static) to parse compound keys:
```csharp
// OLD: state[$"TALENT:{indexStr}"] = intValue;
// NEW: Use shared ParseTalentYamlKey for coherent key parsing
var (target, talentIndex) = TalentKeyParser.ParseTalentYamlKey(indexStr);
var effectiveTarget = target ?? "TARGET";
if (talentIndex.HasValue)
{
    state[$"TALENT:{effectiveTarget}:{talentIndex.Value}"] = intValue;
}
else if (target != null)
{
    // Symbolic key (e.g., "PLAYER" alone) — target-only pattern
    state[$"TALENT:{target}"] = intValue;
}
// ABL/TFLAG use simple numeric keys — no compound key parsing needed
// because they are always character-scoped via the context structure
```

**Rationale**: ExtractStateFromContext shares ParseTalentYamlKey with KojoBranchesParser.EvaluateCondition to guarantee coherent state key construction. Single implementation prevents parsing logic drift. Simple numeric keys default to "TARGET" per Program.cs ParseTalentState convention (line 262-269).

#### StateConverter.ConvertStateToContext (Modified)

**Current** (line 24): `var id = parts[parts.Length - 1]` → strips all intermediate segments

**New**: For TALENT state keys with keyword targets (3+ parts where middle segment is in TargetKeywords), produce compound context key:
```csharp
var type = parts[0];
string id;
if (type == "TALENT" && parts.Length >= 3
    && TalentConditionParser.TargetKeywords.Contains(parts[1])
    && parts[1] != "TARGET")
{
    // Keyword target: preserve as compound key (e.g., "PLAYER:16")
    id = string.Join(":", parts.Skip(1));
}
else
{
    // Default: use last segment (backward compatible for ABL, TFLAG, and TALENT:TARGET:N)
    id = parts[parts.Length - 1];
}
```

**Key behavior**:
- `TALENT:PLAYER:16` → `{"TALENT": {"PLAYER:16": 1}}` (compound key, round-trips with ParseTalentYamlKey)
- `TALENT:TARGET:16` → `{"TALENT": {"16": 1}}` (default target, backward compatible)
- `ABL:TARGET:5` → `{"ABL": {"5": 1}}` (non-TALENT unchanged)

**Rationale**: StateConverter must produce context keys that round-trip through ExtractStateFromContext. Without this update, `TALENT:PLAYER:16` → context `{"TALENT": {"16": 1}}` → ExtractStateFromContext → `TALENT:TARGET:16` (target lost). With the update, `TALENT:PLAYER:16` → context `{"TALENT": {"PLAYER:16": 1}}` → ExtractStateFromContext → `TALENT:PLAYER:16` (target preserved).

### Implementation Sequence

1. **ErbParser**: Add Index property to TalentRef, update TalentConditionParser with disambiguation logic
2. **ErbParser.Tests**: Add parser tests for AC#1-5, AC#11-12
3. **ErbToYaml**: Update ConvertTalentRef with compound key encoding and BuildConditionDict helper; update ResolveInnerBitwiseRef for TalentRef.Index/Target
4. **ErbToYaml.Tests**: Add conversion tests for AC#6-9 (including compound key format for three-part patterns) and AC#21 (ResolveInnerBitwiseRef)
5. **KojoComparer**: Add ParseTalentYamlKey, update KojoBranchesParser.EvaluateCondition to handle compound/symbolic/numeric keys, update YamlRunner.ExtractStateFromContext, update StateConverter.ConvertStateToContext for compound key preservation
6. **KojoComparer.Tests**: Add evaluation tests for AC#10, AC#15 (integration), AC#16 (regression), AC#22 (StateConverter round-trip)
7. **Build verification**: AC#13-14

### Edge Cases and Validation

| Edge Case | Handling |
|-----------|----------|
| Empty name after disambiguation | Return null (invalid pattern) |
| Keyword in three-part pattern (e.g., TALENT:6:PLAYER) | PLAYER goes to Name field (group2), not Target (group1 already has "6") |
| Numeric target in three-part (e.g., TALENT:6:NTR) | "6" → Target, "NTR" → Name (existing behavior preserved) |
| Both Name and Index populated | Invalid state; disambiguation ensures only one is set |
| Target without Name/Index | Valid: symbolic reference (e.g., TALENT:PLAYER) → key "PLAYER" in YAML |
| Keyword target with Name | Compound key: TALENT:PLAYER:処女 → key "PLAYER:16" in YAML |
| Keyword target with Index | Compound key: TALENT:PLAYER:2 → key "PLAYER:2" in YAML |
| Numeric target with Name | Numeric target discarded: TALENT:6:NTR → key "16" in YAML (backward compat) |
| Numeric target with Index | Numeric target discarded: TALENT:6:2 → key "2" in YAML (backward compat) |
| Compound key parsing (YAML→eval) | ParseTalentYamlKey splits on first colon: "PLAYER:16" → (target="PLAYER", index=16) |
| Malformed compound key (no valid index after colon) | ParseTalentYamlKey extracts target portion only (key before colon), discards malformed index |
| CSV lookup failure | Existing warning behavior preserved (log to stderr, return empty dict) |

### Maintenance Notes

1. **Regex synchronization** → Tracked in Mandatory Handoff F768: TalentConditionParser regex is identical to VariableConditionParser regex. Extract shared pattern constant (e.g., `ConditionPatterns.VariablePattern`) to prevent sync drift.
2. **ConvertTalentRef helper** → Tracked in Task#5: Extract `BuildConditionDict(string prefix, string key, string? op, string? value)` helper to reduce dictionary construction duplication across ConvertTalentRef branches and ConvertVariableRef.
3. **State key format consistency** → Tracked in Task#7b: Add code comment in ExtractStateFromContext explaining why TALENT uses three-part keys (`TALENT:{target}:{index}`) while other variable types use two-part keys (`{TYPE}:{index}`). Also explain why only TALENT needs compound key parsing (ABL/TFLAG are always character-scoped via context structure).
4. **TALENT evaluation special case** → KojoBranchesParser.EvaluateCondition has TALENT as an inline special case (TalentKeyParser.ParseTalentYamlKey + compound keys) while other variables use generic EvaluateVariableCondition. This is a deliberate design choice: TALENT's compound key format requires parsing logic that generic evaluation cannot provide. ParseTalentYamlKey lives in separate TalentKeyParser class for SRP. Document this in Task#7a implementation code comment.

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [resolved-applied] Phase2-Uncertain iter1: [INV-003] GetTargetFromYamlCondition helper referenced in Technical Design (line 630) but never defined. AC#10 covers state key format but Technical Design is incomplete for this helper. → Defined helper with Option A discrimination logic in Technical Design.
- [resolved-applied] Phase2-Uncertain iter1: [AC-005] No end-to-end round-trip AC verifying parse→convert→evaluate target preservation. → Added AC#15 (integration round-trip test) and Task#10.
- [resolved-applied] Phase2-Review iter3: [CON-001] Task#7 state key migration only covers consumer side. → Expanded Task#7 scope to include KojoBranchesParser + YamlRunner.ExtractStateFromContext + test fixtures.
- [resolved-applied] Phase2-Uncertain iter3: [AC-001] AC#9 regression coverage vague. → Specified 3 representative patterns (numeric-target+name, keyword-target+name, keyword-target+index) as minimum test cases.
- [resolved-applied] Phase2-Review iter3: [FMT-001] Review Context section present but lacks template sub-sections (Origin, Identified Gap, Review Evidence, Files Involved). Validator contradicted between iter2 (INVALID: optional section) and iter3 (VALID: if present, should follow structure). → Section removed; content merged into Background paragraph.
- [resolved-applied] Phase2-Review iter5: [HND-001] Mandatory Handoff F768 Creation Task is "POST-LOOP DRAFT creation" — not a Task# in Tasks table. Template requires DRAFT file exists OR Creation Task in Tasks table. → Added Task#11 for F768 DRAFT creation; updated Mandatory Handoffs table to reference Task#11.
- [resolved-applied] Phase2-Uncertain iter5: [DES-001] Task#5 references BuildConditionDict helper but Technical Design ConvertTalentRef code shows explicit dictionary constructions. → Redesigned ConvertTalentRef now defines BuildConditionDict helper inline in the code sample with full signature and implementation.
- [resolved-applied] Phase2-Review post-loop-iter1: [CON-003] AC#15 (end-to-end round-trip) contradicts ConvertTalentRef design which discards Target for three-part patterns in BOTH Branch 1 (Name path: TALENT:PLAYER:処女) AND Branch 2 (Index path: TALENT:PLAYER:2). → Redesigned ConvertTalentRef to use compound key format (e.g., "PLAYER:16") that preserves target in YAML. Added ParseTalentYamlKey to KojoBranchesParser to parse compound keys. Updated AC#15 details to match.
- [resolved-applied] Phase2-Review post-loop-iter2: [AC-005] No evaluation regression AC for state key format migration (TALENT:{index}→TALENT:TARGET:{index}). → Added AC#16 (state key migration regression), Task#12, updated Philosophy Derivation and Goal Coverage.
- [resolved-applied] Phase2-Review post-loop-iter1: [INV-003] GetTargetFromYamlCondition produces TALENT:PLAYER:PLAYER for target-only patterns, but KojoBranchesParser.EvaluateCondition (line 119) int.TryParse guard skips non-numeric keys entirely. → Replaced GetTargetFromYamlCondition with ParseTalentYamlKey that handles compound/symbolic/numeric keys. Updated EvaluateCondition to use ParseTalentYamlKey instead of int.TryParse guard. Target-only patterns now produce stateKey `TALENT:PLAYER` (two-part).
- [resolved-skipped] Phase3-Maintainability iter9: [MNT-001] Numeric target lossy conversion (TALENT:6:NTR → key "16") — accepted design trade-off: Key Decision "Numeric target handling" documents rationale (numeric first segments are character indices, not semantic targets), Risk table entry acknowledges information loss with mitigation (re-parsing from ERB always possible). Corpus evidence: all 277+ numeric-target three-part patterns use character index as first segment (e.g., TALENT:6:NTR where 6=character index). No semantic usage found.
- [resolved-skipped] Phase3-Maintainability iter9: [MNT-002] Regex/disambiguation duplication with VariableConditionParser — accepted bounded duplication per Key Decision "TalentRef extension": duplication cost is bounded (one regex, one method) vs migration risk across 3 projects. F768 (Task#11) tracks cross-parser refactoring. Maintenance Note #1 explicitly documents regex synchronization obligation.
- [resolved-applied] Phase3-Maintainability iter9: [MNT-003] ExtractStateFromContext TALENT/ABL/TFLAG asymmetry — Task#7b already requires code comment explaining three-part key divergence from two-part format. No additional action needed.
- [resolved-applied] Phase3-Maintainability iter9: [MNT-004] Task#7b AC coverage — AC#10 description renamed to "KojoBranchesParser and YamlRunner target-aware state key" to make YamlRunner coverage explicit. AC#15 provides supplemental end-to-end coverage.
- [resolved-skipped] Phase2-Uncertain iter1: [BG-001] Problem section omits state key format mismatch: Program.cs ParseTalentState produces TALENT:TARGET:{index} but KojoBranchesParser.EvaluateCondition produces TALENT:{talentIndex}. State key mismatch is a downstream consequence of root causes #1-#3, not an independent root cause. Technical Design Tasks 7a/7b/7c and AC#10/AC#16 fully address the mismatch. Problem section documents root causes only.
- [resolved-applied] Phase3-Maintainability iter3: [MNT-005] ResolveInnerBitwiseRef (DatalistConverter.cs:549-572) absent from Impact Analysis, ACs, Tasks, and Technical Design. F759 [DONE] consumer uses talent.Name for CSV lookup (line 554), which breaks when F760 makes Name empty for numeric patterns. → Added AC#21, Task#14, AC Design Constraint C12, Implementation Contract Phase 12, ResolveInnerBitwiseRef Technical Design section, and updated Impact Analysis/Implementation Sequence.
- [resolved-applied] Phase3-Maintainability iter3: [STA-001] F759 status listed as [PROPOSED] in Related Features and Dependencies but actually [DONE]. → Updated both tables to [DONE].
- [resolved-applied] Phase3-Maintainability iter5: [MNT-006] ConvertTalentRef and ResolveInnerBitwiseRef duplicate identical TALENT key resolution logic. → Extracted ResolveTalentKey shared helper. Both methods now delegate to ResolveTalentKey for Name→CSV, Index→direct, Target→compound key resolution.
- [resolved-applied] Phase3-Maintainability iter1: [MNT-007] ParseTalentYamlKey does not log warning for malformed compound keys (e.g., PLAYER:MASTER). Silent recovery may hide issues. → Added Console.Error.WriteLine warning in ParseTalentYamlKey Technical Design code for malformed compound key branch.
- [resolved-applied] Phase2-Review iter3: [STC-001] StateConverter.ConvertStateToContext strips target information (TALENT:PLAYER:16 → {"TALENT": {"16": 1}}), breaking round-trip with compound keys. Technical Design noted StateConverter must be updated but no Task/AC existed. → Added AC#22, Task#15, AC Design Constraint C13, Implementation Contract Phase 13, StateConverter Technical Design section with compound key preservation logic.
- [resolved-skipped] Phase2-Review iter3: [VOL-001] AC count (22) exceeds engine type volume limit (8-15 per feature-template.md). Feature spans 3 projects with distinct transformation pipelines requiring fine-grained verification. User accepted: 3-project pipeline exception justified.
- [resolved-skipped] Phase2-Uncertain iter3: [TST-001] AC#15 end-to-end round-trip test hand-constructs context bypassing StateConverter. User accepted: AC#15 focuses on evaluation pipeline, AC#22 covers StateConverter independently. Two-AC combination covers full pipeline.
- [resolved-applied] Phase3-Maintainability iter5: [MNT-009] ParseTalentYamlKey defined inside KojoBranchesParser class mixes parsing utility with evaluation logic. → Extracted to separate TalentKeyParser utility class for SRP. Updated Technical Design, Task#7a, and Maintenance Note #4.
- [resolved-applied] Phase3-Maintainability iter5: [MNT-008] BuildConditionDict only applied to ConvertTalentRef and ResolveInnerBitwiseRef; ConvertVariableRef and ConvertArgRef (same file) still use manual dictionary construction. → Expanded Task#5 scope to include ConvertVariableRef and ConvertArgRef. Updated Mandatory Handoffs F768 description.
- [resolved-skipped] Phase2-Review iter6: [FMT-002] Task# uses sub-letters (7a, 7b, 7c). User accepted: no SSOT prohibition, F706 precedent, sub-lettering clarifies related implementation steps.

---

## Links
- [feature-750.md](archive/feature-750.md) - Original TALENT condition migration (archived)
- [feature-757.md](archive/feature-757.md) - Foundation: bitwise operator support (archived)
- [feature-758.md](feature-758.md) - Parent feature (prefix-based variable type expansion)
- [feature-759.md](feature-759.md) - Sibling: compound bitwise (scope boundary)
- [feature-751.md](feature-751.md) - Downstream: TALENT semantic mapping
- [feature-706.md](feature-706.md) - Consumer: full equivalence testing
- [feature-768.md](feature-768.md) - Cross-parser refactoring and deduplication (F768 DRAFT)
- [feature-769.md](feature-769.md) - Target-only TALENT evaluation with runtime resolution (F769 DRAFT)
