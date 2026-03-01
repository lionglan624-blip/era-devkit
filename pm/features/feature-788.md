# Feature 788: IConsoleOutput Phase 20 Extensions

## Status: [DONE]
<!-- fl-reviewed: 2026-02-14T00:00:00Z -->

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

<!-- Sub-Feature Requirements (architecture.md): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

<!-- fc-phase-1-completed -->
## Background
<!-- Written by: consensus-synthesizer (Phase 1) -->

### Philosophy (Mid-term Vision)

Pipeline Continuity -- IConsoleOutput is the single source of truth for ERA print/display command abstractions required by Phase 20 Shop Core migrations within Era.Core. Each ERA display command used by Phase 20 migrated C# code must have a corresponding IConsoleOutput method. This ensures Phase 20 sub-features (F775-F777) can delegate display operations through a clean interface contract rather than embedding private stubs with NotImplementedException, maintaining the incremental migration architecture established in Phase 9.

### Problem (Current Issue)

Era.Core's IConsoleOutput interface (Era.Core/Interfaces/IConsoleOutput.cs) was designed during Phase 9 (Command Infrastructure) with only 8 methods covering PRINT/PRINTL/PRINTW/PRINTFORM/PRINTDATA/PRINTBUTTON/PRINTBUTTONC. Phase 20 ERB files (SHOP.ERB, SHOP2.ERB) use 4 additional ERA display commands not mapped to the interface:

- **DRAWLINE** (18 uses across Phase 20 ERBs) -- draws horizontal separator line. Engine: PrintBar() + NewLine() (HeadlessConsole.cs:121-130, Process.ScriptProc.cs:212-216)
- **CLEARLINE** (3 uses) -- deletes N lines from display. Engine: deleteLine(count) + RefreshStrings (HeadlessConsole.cs:176-181, Instraction.Child.cs:523-536)
- **PRINTLC** (15 uses) -- column-aligned left text. Engine: PrintC(str, alignRight=false) = PadRight(20) (HeadlessConsole.cs:143-150, Instraction.Child.cs:66-70,149-150). Note: the "C" suffix means "column alignment", NOT "center alignment". PRINTLC uses PrintC(str, false) where false = alignRight=false, producing left-column PadRight(20) output. Does NOT append newline.
- **BAR** (7 uses) -- visual bar display [*****...]. Engine: CreateBar(var, max, length) builds string with Config.BarChar1='*' and Config.BarChar2='.' (ExpressionMediator.cs:123-146, ConfigData.cs:132-133)

These 4 commands were identified by auditing all NotImplementedException stubs referencing "IConsoleOutput extension" in F774-migrated files (ShopSystem.cs, ShopDisplay.cs). F774's comprehensive SHOP.ERB + SHOP2.ERB migration ensures the stub set is exhaustive: any unmapped ERA display command would have produced an additional stub.

F774 Shop Core migration created 5 private NotImplementedException stubs for these operations:
- ShopSystem.cs:387-391 -- DrawLine(), ClearLine(int)
- ShopDisplay.cs:435-440 -- DrawLine(), Bar(int, int, int), PrintLineC(string)

The engine-layer IConsoleOutput (engine/Headless/IConsoleOutput.cs) already has equivalent capabilities (PrintBar, DeleteLine, PrintC), but the Era.Core interface was not extended to match.

**Scope reduction**: PRINTFORMLC (5 ERB occurrences) was excluded because all PRINTFORMLC calls were already translated to C# string interpolation + PrintLineC during F774 migration. Zero occurrences of "PrintFormLC" exist in any Era.Core file (verified by Grep). Including it would violate YAGNI.

### Goal (What to Achieve)

1. Add 4 method signatures to Era.Core IConsoleOutput interface: DrawLine(), ClearLine(int), PrintColumnLeft(string), Bar(long, long, long)
2. Replace 5 private NotImplementedException stubs in ShopSystem.cs and ShopDisplay.cs with _console.Method() calls
3. Update 2 test mock classes (MockConsoleOutput, EquivalenceConsoleOutput) to implement the new interface methods
4. All methods follow existing Result<Unit> return type convention with XML documentation

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do ShopSystem.cs and ShopDisplay.cs have NotImplementedException stubs? | These migrated C# files need ERA display commands (DRAWLINE, CLEARLINE, PRINTLC, BAR) that IConsoleOutput does not provide | Era.Core/Shop/ShopSystem.cs:387-391, Era.Core/Shop/ShopDisplay.cs:435-440 |
| 2 | Why does IConsoleOutput lack these methods? | The interface was designed during Phase 9 with only the 8 basic print commands needed at that time | Era.Core/Interfaces/IConsoleOutput.cs:9-39 |
| 3 | Why did Phase 9 only include 8 methods? | Phase 9 scoped to the training system's most common print commands; shop/settings UI commands were not anticipated | Era.Core/DependencyInjection/ServiceCollectionExtensions.cs:365-372 |
| 4 | Why were shop display commands not anticipated? | Phase 20 planning (F647) happened long after the interface was finalized, and the architecture follows incremental interface extension | pm/features/feature-647.md |
| 5 | Why (Root)? | The architecture intentionally extends interfaces incrementally -- new ERA commands get added only when migrated features actually need them, creating a gap when Phase 20 consumers arrive before the interface is extended | engine/Assets/Scripts/Emuera/Headless/IConsoleOutput.cs:59-106 (engine already has these methods) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 5 NotImplementedException stubs in ShopSystem.cs and ShopDisplay.cs | IConsoleOutput interface missing 4 ERA display command abstractions |
| Where | Era.Core/Shop/ShopSystem.cs:387-391, Era.Core/Shop/ShopDisplay.cs:435-440 | Era.Core/Interfaces/IConsoleOutput.cs (8 methods, needs 12) |
| Fix | Keep private stubs (band-aid) | Add DrawLine, ClearLine, PrintColumnLeft, Bar to IConsoleOutput interface |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Predecessor -- Phase 20 Planning parent |
| F774 | [DONE] | Predecessor -- Shop Core, Mandatory Handoff origin for IConsoleOutput gap |
| F775 | [DRAFT] | Successor -- Collection (SHOP_COLLECTION.ERB) uses DRAWLINE (6 instances) |
| F776 | [DRAFT] | Successor -- Items (SHOP_ITEM.ERB) uses DRAWLINE (3 instances) |
| F777 | [DRAFT] | Successor -- Customization (SHOP_CUSTOM.ERB) uses DRAWLINE (4 instances) + BAR (4 instances) |
| F778 | [DRAFT] | Related -- Body Settings (no dependency, uses none of the 4 proposed methods) |
| F779 | [DRAFT] | Related -- Body Settings (no dependency, uses none of the 4 proposed methods) |
| F780 | [DRAFT] | Related -- Body Settings (no dependency, uses none of the 4 proposed methods) |
| F781 | [DRAFT] | Related -- Body Settings (no dependency, uses none of the 4 proposed methods) |
| F789 | [PROPOSED] | Related -- IVariableStore Phase 20 Extensions (parallel, no dependency) |
| F790 | [PROPOSED] | Related -- Engine Data Access Layer (parallel, no dependency) |
| F791 | [PROPOSED] | Related -- Engine State Transitions (parallel, no dependency) |
| F782 | [DRAFT] | Successor -- Post-Phase Review (depends on F788 completion) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface modification scope | FEASIBLE | Adding 4 methods to a 39-line interface; minimal, non-breaking for in-project consumers |
| Implementation reference | FEASIBLE | Engine-layer HeadlessConsole already implements equivalent methods (PrintBar, DeleteLine, PrintC) |
| Test infrastructure | FEASIBLE | Only 2 mock classes implement IConsoleOutput (MockConsoleOutput in PrintCommandTests.cs:282, EquivalenceConsoleOutput in PrintOutputEquivalenceTests.cs:248); both in-project |
| Return type convention | FEASIBLE | All existing methods use Result<Unit>; new methods follow same pattern |
| BAR config dependency | FEASIBLE | Hardcode defaults BarChar1='*', BarChar2='.' matching engine ConfigData.cs:132-133; no ERA game overrides these |
| Breaking change risk | FEASIBLE | IConsoleOutput is host-provided (not DI-registered); only the 2 test mocks break, and Shop test files only reference the interface in comments |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces/IConsoleOutput.cs | HIGH | 4 new method signatures added to the interface contract |
| Era.Core/Shop/ShopSystem.cs | MEDIUM | Replace 2 private stubs (DrawLine, ClearLine) with _console calls |
| Era.Core/Shop/ShopDisplay.cs | MEDIUM | Replace 3 private stubs (DrawLine, Bar, PrintLineC) with _console calls |
| Era.Core.Tests/Commands/Print/PrintCommandTests.cs | MEDIUM | MockConsoleOutput must implement 4 new methods |
| Era.Core.Tests/Commands/Print/PrintOutputEquivalenceTests.cs | MEDIUM | EquivalenceConsoleOutput must implement 4 new methods |
| F775/F776/F777 (downstream) | LOW | These features should declare F788 as Predecessor (currently missing) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| All methods must return Result<Unit> | Existing IConsoleOutput convention (IConsoleOutput.cs:12-38) | New methods MUST follow same return type pattern |
| TreatWarningsAsErrors=true | Directory.Build.props | All new code must compile without warnings; XML doc comments required |
| IConsoleOutput is host-provided, not DI-registered | ServiceCollectionExtensions.cs (no IConsoleOutput registration) | F788 defines interface contract only; no Era.Core implementation needed |
| Two separate IConsoleOutput interfaces exist | Era.Core vs engine namespaces | Must not confuse; F788 modifies Era.Core only |
| BAR rendering uses Config.BarChar1='*', Config.BarChar2='.' | ConfigData.cs:132-133, ExpressionMediator.cs:142-143 | Era.Core Bar method must accept or hardcode these defaults |
| BAR max must be > 0, length must be > 0 and < 100 | ExpressionMediator.cs:125-130 | Validation guards needed in Bar method contract documentation |
| PRINTLC uses column alignment, not centering | Instraction.Child.cs:66-70,149-150; HeadlessConsole.cs:143-150 | Method name must reflect column-left-alignment semantics (PrintColumnLeft, not PrintLineC) |
| ClearLine in headless is a no-op (adjusts lineCount_) | HeadlessConsole.cs:176-181 | Interface contract does not mandate visual clearing |
| PrintPlayerBars stub NOT resolved by F788 alone | ShopSystem.cs:401-402 | Needs BAR + F789 (IVariableStore) + F790 (data access); out of scope |
| Equivalence testing N/A for interface-only feature | C6 (host-provided interface), architecture.md sub-feature requirement #3 | F788 defines method signatures only; no Era.Core behavior to compare against legacy. Equivalence testing applies when host implementations are created (successor features with actual rendering logic) |
| IConsoleOutput completeness is scoped to F788's 4 methods | Philosophy Derivation Row 1 (pre-condition assumption) | If F775-F777 discover additional ERA display commands needing IConsoleOutput methods, a new extension feature must be created. F788's 4-method set is derived from F774 stubs, not a permanent invariant |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| PRINTLC semantic mismatch if named "PrintLineC" (implies centering) | HIGH | MEDIUM | Name method PrintColumnLeft to accurately reflect PadRight(20) left-column-alignment behavior |
| BAR rendering config dependency | LOW | LOW | Hardcode defaults matching engine (BarChar1='*', BarChar2='.'); document in XML comment |
| Breaking change on IConsoleOutput (2 test mocks) | HIGH | LOW | Both mocks are in-project; update in same feature |
| F775/F776/F777 missing F788 as Predecessor in index-features.md | HIGH | MEDIUM | Document as Mandatory Handoff; update during /fl or /run |
| ShopSystem stubs beyond F788 scope (PrintPlayerBars, PrintDateMoneyStatus, etc.) | MEDIUM | LOW | Explicitly out of scope; requires F789 + F790 |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| IConsoleOutput method count | Grep "Result<Unit>" IConsoleOutput.cs | 8 methods | Target: 12 (add 4) |
| NotImplementedException stubs in ShopSystem.cs | Grep "NotImplementedException.*IConsoleOutput" ShopSystem.cs | 2 stubs (DrawLine, ClearLine) | Target: 0 |
| NotImplementedException stubs in ShopDisplay.cs | Grep "NotImplementedException.*IConsoleOutput" ShopDisplay.cs | 3 stubs (DrawLine, Bar, PrintLineC) | Target: 0 |
| Test mock implementations | Grep "class.*IConsoleOutput" Era.Core.Tests | 2 classes | Both must implement new methods |

**Baseline File**: `.tmp/baseline-788.txt`

---

## AC Design Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All new methods must return Result<Unit> | IConsoleOutput existing pattern (IConsoleOutput.cs:12-38) | AC must verify return type consistency for all 4 new methods |
| C2 | Method names must reflect ERA command semantics accurately | Instraction.Child.cs:66-70,149-150 | PRINTLC = column-left-aligned (PadRight), NOT centered; use PrintColumnLeft |
| C3 | BAR output must produce [*****...] format | ExpressionMediator.cs:123-146, ConfigData.cs:132-133 | AC must verify Bar method signature and XML doc specifies boundary constraints (max > 0, length > 0, length < 100). Boundary behavior testing is host-implementation responsibility per C6 (interface-only scope) |
| C4 | DrawLine must produce separator line + newline | DrawLineCommand.cs:19-20, HeadlessConsole.cs:121-130 | AC must verify composite operation (PrintBar + NewLine equivalent) |
| C5 | ClearLine(count) must handle edge cases gracefully | HeadlessConsole.cs:176-181, Instraction.Child.cs:523-536 | AC must verify non-negative count handling and count=0 |
| C6 | IConsoleOutput is host-provided, not DI-registered | ServiceCollectionExtensions.cs | ACs verify interface contract and test mock compilation only, not DI registration |
| C7 | All existing test mocks must compile and pass | PrintCommandTests.cs:282-344, PrintOutputEquivalenceTests.cs:248-317 | AC must verify both mock classes implement new methods and existing tests still pass |
| C8 | XML documentation required for all new methods | Directory.Build.props TreatWarningsAsErrors=true | Each new method needs summary doc comment |
| C9 | PrintFormLC is NOT in scope | Zero occurrences in Era.Core (YAGNI) | AC must NOT require PrintFormLC; only 4 methods |
| C10 | PrintPlayerBars stub is NOT resolved by F788 | ShopSystem.cs:401-402 | AC should NOT verify PrintPlayerBars resolution; needs F789 + F790 |

### Constraint Details

**C1: Result<Unit> Return Type Convention**
- **Source**: All 8 existing IConsoleOutput methods return Result<Unit>
- **Verification**: Grep "Result<Unit>" in IConsoleOutput.cs; count should increase from 8 to 12
- **AC Impact**: Every new method signature must include Result<Unit> return type

**C2: PRINTLC Column Alignment Semantics**
- **Source**: Engine Instraction.Child.cs:66-70 sets isLC=true for "LC" suffix; line 149-150 calls PrintC(str, false) where false = alignRight=false; HeadlessConsole.cs:143-150 maps alignRight=false to PadRight(20)
- **Verification**: Method name in interface must NOT suggest "centering"
- **AC Impact**: Method must be named PrintColumnLeft (not PrintLineC) to avoid semantic confusion

**C3: BAR Format and Validation**
- **Source**: ExpressionMediator.cs:123-146 builds [*****...] with Config.BarChar1='*' and Config.BarChar2='.'; validates max > 0, length > 0, length < 100
- **Verification**: Bar method contract must document character defaults and validation boundaries
- **AC Impact**: AC must verify Bar method signature includes long parameters (AC#4) and XML doc specifies boundary constraints (max > 0, length > 0, length < 100). Boundary behavior testing is host-implementation responsibility per C6 (interface-only scope)

**C4: DrawLine Composite Operation**
- **Source**: DrawLineCommand.cs:19-20 calls PrintBar() then NewLine(); HeadlessConsole.PrintBar() produces 60 chars of '-' pattern
- **Verification**: DrawLine produces a separator line (default '-' character repeated) followed by newline
- **AC Impact**: AC must verify the composite nature (line + newline)

**C5: ClearLine Edge Cases**
- **Source**: HeadlessConsole.cs:176-181 uses Math.Max(0, lineCount_ - count) ensuring non-negative result
- **Verification**: ClearLine(0) is a no-op; negative values handled gracefully
- **AC Impact**: AC must test edge cases including count=0

**C6: Host-Provided Interface**
- **Source**: IConsoleOutput is NOT registered in ServiceCollectionExtensions.cs; runtime hosts provide their own implementation
- **Verification**: No DI registration AC needed
- **AC Impact**: ACs focus on interface contract definition and test mock compilation

**C7: Test Mock Compilation**
- **Source**: MockConsoleOutput (PrintCommandTests.cs:282-344) and EquivalenceConsoleOutput (PrintOutputEquivalenceTests.cs:248-317) are the only 2 classes implementing IConsoleOutput in Era.Core.Tests. Shop test files reference IConsoleOutput only in comments.
- **Verification**: dotnet test Era.Core.Tests must pass after interface extension
- **AC Impact**: Both mock classes must add 4 new method implementations

---

## Dependencies
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (parent) |
| Predecessor | F774 | [DONE] | Shop Core (Mandatory Handoff origin) |
| Successor | F775 | [DONE] | Collection -- will consume DrawLine |
| Successor | F776 | [DONE] | Items -- will consume DrawLine |
| Successor | F777 | [DONE] | Customization -- will consume DrawLine + Bar |
| Successor | F782 | [DRAFT] | Post-Phase Review |
| Related | F789 | [DONE] | IVariableStore Phase 20 Extensions (parallel, no dependency) |
| Related | F790 | [DONE] | Engine Data Access Layer (parallel, no dependency) |
| Related | F791 | [DONE] | Engine State Transitions (parallel, no dependency) |
| Related | F778 | [DONE] | Body Settings (no dependency, uses none of the 4 proposed methods) |
| Related | F779 | [DONE] | Body Settings (no dependency, uses none of the 4 proposed methods) |
| Related | F780 | [PROPOSED] | Body Settings (no dependency, uses none of the 4 proposed methods) |
| Related | F781 | [DONE] | Body Settings (no dependency, uses none of the 4 proposed methods) |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "IConsoleOutput is the single source of truth for ERA print/display command abstractions required by Phase 20 Shop Core migrations within Era.Core" | Every Phase 20 ERA display command used by migrated C# code must have a corresponding IConsoleOutput method. Completeness is a pre-condition assumption verified during /fc Phase 1 (Problem section's 4-command analysis of SHOP.ERB/SHOP2.ERB; F774 scope = SHOP.ERB + SHOP2.ERB defines the complete set of Phase 20 Shop Core migration sources), not an AC-enforced invariant; AC#5 count=12 confirms the 8+4 addition was applied correctly | AC#1, AC#2, AC#3, AC#4, AC#5 |
| "Each ERA display command used by migrated C# code must have a corresponding IConsoleOutput method" | The 4 missing methods (DrawLine, ClearLine, PrintColumnLeft, Bar) must be added to the interface | AC#1, AC#2, AC#3, AC#4 |
| "Phase 20 sub-features can delegate display operations through a clean interface contract rather than embedding private stubs" | Private NotImplementedException stubs must be replaced with _console.Method() calls | AC#6, AC#7, AC#8, AC#15, AC#16, AC#17, AC#18, AC#19 |
| "Maintaining the incremental migration architecture established in Phase 9" | Existing methods and tests must continue to work; new methods follow same Result<Unit> convention; new interface methods document ERA command semantics in XML comments (faithfulness to engine behavior verified at host-implementation level per C6) | AC#5, AC#9, AC#10, AC#11, AC#25, AC#26, AC#27, AC#28, AC#29 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DrawLine method exists in IConsoleOutput (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | Result<Unit> DrawLine() | [x] |
| 2 | ClearLine method exists in IConsoleOutput (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | Result<Unit> ClearLine(int count) | [x] |
| 3 | PrintColumnLeft method exists in IConsoleOutput (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | Result<Unit> PrintColumnLeft(string text) | [x] |
| 4 | Bar method exists in IConsoleOutput (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | Result<Unit> Bar(long value, long max, long length) | [x] |
| 5 | IConsoleOutput has exactly 12 Result<Unit> methods (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | count_equals | Result<Unit> (12) | [x] |
| 6 | ShopSystem.cs DrawLine stub removed (Neg) | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_contains | DRAWLINE requires IConsoleOutput extension | [x] |
| 7 | ShopSystem.cs ClearLine stub removed (Neg) | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_contains | CLEARLINE requires IConsoleOutput extension | [x] |
| 8 | ShopDisplay.cs console extension stubs removed (Neg) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | not_matches | (DRAWLINE\|BAR\|PRINTLC) requires IConsoleOutput extension | [x] |
| 9 | MockConsoleOutput implements new methods (Pos) | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 10 | Era.Core build succeeds (Pos) | build | dotnet build Era.Core | succeeds | - | [x] |
| 11 | XML documentation on all new methods (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | count_equals | /// <summary> (13) | [x] |
| 12 | PrintFormLC not added (Neg) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | not_contains | PrintFormLC | [x] |
| 13 | PrintPlayerBars stub unchanged (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | contains | PrintPlayerBars | [x] |
| 14 | Zero technical debt in modified files (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs,Era.Core/Shop/ShopSystem.cs,Era.Core/Shop/ShopDisplay.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 15 | ShopSystem.cs delegates DrawLine to _console (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | contains | _console.DrawLine() | [x] |
| 16 | ShopSystem.cs delegates ClearLine to _console (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | contains | _console.ClearLine(count) | [x] |
| 17 | ShopDisplay.cs delegates DrawLine to _console (Pos) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | contains | _console.DrawLine() | [x] |
| 18 | ShopDisplay.cs delegates Bar to _console (Pos) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | contains | _console.Bar(value, max, width) | [x] |
| 19 | ShopDisplay.cs delegates PrintColumnLeft to _console (Pos) | code | Grep(Era.Core/Shop/ShopDisplay.cs) | contains | _console.PrintColumnLeft(text) | [x] |
| 20 | Behavioral delegation tests pass (Pos) | test | dotnet test Era.Core.Tests --filter "ConsoleOutput" | succeeds | - | [x] |
| 21 | engine-dev SKILL.md documents DrawLine in IConsoleOutput (Pos) | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | Result<Unit> DrawLine() | [x] |
| 22 | engine-dev SKILL.md documents ClearLine in IConsoleOutput (Pos) | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | Result<Unit> ClearLine(int count) | [x] |
| 23 | engine-dev SKILL.md documents PrintColumnLeft in IConsoleOutput (Pos) | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | Result<Unit> PrintColumnLeft(string text) | [x] |
| 24 | engine-dev SKILL.md documents Bar in IConsoleOutput (Pos) | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | Result<Unit> Bar(long value, long max, long length) | [x] |
| 25 | DrawLine XML doc specifies separator line with newline (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | separator line with newline | [x] |
| 26 | PrintColumnLeft XML doc specifies NOT centering (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | NOT centering | [x] |
| 27 | Bar XML doc specifies max > 0 constraint (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | must be > 0 | [x] |
| 28 | ClearLine XML doc specifies best-effort semantics (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | best-effort | [x] |
| 29 | Bar XML doc specifies length < 100 upper bound (Pos) | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | contains | < 100 | [x] |
| 30 | F775 lists F788 as Predecessor (Pos) | code | Grep(pm/features/feature-775.md) | contains | Predecessor \| F788 | [x] |
| 31 | F776 lists F788 as Predecessor (Pos) | code | Grep(pm/features/feature-776.md) | contains | Predecessor \| F788 | [x] |
| 32 | F777 lists F788 as Predecessor (Pos) | code | Grep(pm/features/feature-777.md) | contains | Predecessor \| F788 | [x] |

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add 4 method signatures to IConsoleOutput interface | AC#1, AC#2, AC#3, AC#4, AC#12 |
| 2 | Replace 5 private NotImplementedException stubs with _console calls | AC#6, AC#7, AC#8, AC#15, AC#16, AC#17, AC#18, AC#19 |
| 3 | Update 2 test mock classes to implement new interface methods | AC#9 |
| 4 | All methods follow Result<Unit> return type convention with XML documentation | AC#5, AC#11, AC#14, AC#25, AC#26, AC#27, AC#28, AC#29 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Extend the Era.Core IConsoleOutput interface (Era.Core/Interfaces/IConsoleOutput.cs) by adding 4 method signatures that mirror existing engine capabilities.

| ERA Command | Engine Method | IConsoleOutput Method | Mapping |
|-------------|---------------|----------------------|---------|
| DRAWLINE | PrintBar() + NewLine() | DrawLine() | Composite operation |
| CLEARLINE N | DeleteLine(count) | ClearLine(int count) | Direct mapping |
| PRINTLC | PrintC(str, alignRight=false) | PrintColumnLeft(string text) | Left-column PadRight(20) alignment |
| BAR a, b, c | CreateBar(var, max, length) | Bar(long value, long max, long length) | Visual bar [*****...] |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Method naming for PRINTLC | PrintLineC (matches ERB), PrintColumnLeft (semantic) | PrintColumnLeft | Avoids semantic confusion -- "C" means column, not center |
| BAR parameter type | int (matches ERB usage), long (matches engine) | long (Int64) | Matches engine's CreateBar signature and ERA's Int64 convention |
| ClearLine contract | Mandate visual clearing, Best-effort abstraction | Best-effort abstraction | Headless cannot clear; interface allows implementation flexibility |
| BAR config dependency | Require IConsoleConfig injection, Hardcode defaults in XML doc | Hardcode defaults in XML doc | ERA games never override BarChar1/BarChar2; YAGNI |
| ShopDisplay Bar parameter name | Rename width→length, Keep width | Keep width | Preserves ERB migration naming from F774; implicit int→long widening is safe |

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,4,5,11,25,26,27,28,29 | Extend IConsoleOutput interface with 4 new method signatures | | [x] |
| 2 | 6,7,15,16 | Replace ShopSystem.cs stubs with _console calls | | [x] |
| 3 | 8,17,18,19 | Replace ShopDisplay.cs stubs with _console calls | | [x] |
| 4 | 9 | Update MockConsoleOutput in PrintCommandTests.cs | | [x] |
| 5 | 9 | Update EquivalenceConsoleOutput in PrintOutputEquivalenceTests.cs | | [x] |
| 6 | 9,10,14 | Verify build and tests pass | | [x] |
| 7 | 12,13 | Verify scope exclusions | | [x] |
| 8 | 20 | Create behavioral delegation tests for stub replacements | [I] | [x] |
| 9 | 21,22,23,24 | Update engine-dev SKILL.md IConsoleOutput section with 4 new methods | | [x] |
| 10 | 30,31,32 | Update F775/F776/F777 Dependencies tables to add F788 as Predecessor | | [x] |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| F775/F776/F777 missing F788 as Predecessor | Phase 20 sub-features depend on IConsoleOutput extensions but index-features.md does not reflect dependency | Feature | F775, F776, F777 | - |
| PrintPlayerBars stub resolution tracking | ShopSystem.cs:401-402 PrintPlayerBars requires BAR (F788) + BASE/MAXBASE (existing IVariableStore) + MASTER (F790); F790 is the natural home for engine-state-dependent display operations | Feature | F790 | - |
| ShopDisplay.PrintLineC rename to PrintColumnLeft | Private method `PrintLineC(string text)` delegates to `_console.PrintColumnLeft(text)` but retains misleading name suggesting centering; internal callers (ShowCharaData2 etc.) perpetuate semantic confusion. F775 chosen as first successor per user decision | Feature | F775 | - |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-15 | IMPL | implementer | Tasks 1-5 (interface + stubs + mocks) | SUCCESS |
| 2026-02-15 | IMPL | implementer | Task 8 [I] delegation tests | SUCCESS (5 tests) |
| 2026-02-15 | IMPL | opus | Tasks 6,7,9,10 (verify + SKILL + deps) | SUCCESS |
| 2026-02-15 | AC-VERIFY | opus | All 32 ACs verified | PASS |
| 2026-02-15 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit 1 (tool limitation: count_equals unsupported, regex escaping) → Action A: F792 作成済み |

---

## Review Notes
- [resolved-applied] Phase3-Maintainability restart-iter4: [DEP-004] Mandatory Handoff destination tracking — added HTML comments to F775/F776/F777/F790 per user decision
- [fix] Phase2-Review restart-iter3: [CON-001] Mandatory Handoffs Row 3 | Added F775 destination rationale to Reason column
- [resolved-applied] Phase2-Uncertain restart-iter2: [AC-005] Philosophy SSOT scope — added Technical Constraints row documenting temporal boundary per user decision
- [fix] PostLoop-UserFix post-loop: [AC-005] Technical Constraints | Added IConsoleOutput completeness scope constraint noting 4-method set is not a permanent invariant
- [fix] Phase2-Review restart-iter2: [AC-005] AC#18 Detail | Added width→length naming divergence rationale note referencing Key Decisions
- [fix] Phase2-Review restart-iter1: [AC-005] Task 10 AC coverage | Added AC#30-32 for F775/F776/F777 Predecessor verification
- [resolved-applied] Phase3-Maintainability iter2(new): [CON-001] ShopDisplay.PrintLineC rename Mandatory Handoff destination ambiguity — resolved to F775 per user decision
- [resolved-applied] Phase3-Maintainability iter2(new): [SCP-001] F775/F776/F777 Predecessor enforcement — added Task 10 per user decision
- [fix] Phase3-Maintainability iter1: Task 10 | Added explicit status verification instruction for F788 row updates in F775/F776/F777
- [resolved-applied] Phase3-Maintainability iter1: [CON-001] Mandatory Handoff Row 3 PrintLineC rename tracking — added explicit visible markdown text to F775 Background per user decision
- [fix] PostLoop-UserFix post-loop: [CON-001] F775 Background | Added visible PrintLineC→PrintColumnLeft rename obligation text
- [resolved-skipped] Phase3-Maintainability iter1: [AC-005] Philosophy SSOT claim vs ongoing integrity — already addressed by Technical Constraints row and Philosophy Derivation boundary note; user chose to skip
- [resolved-skipped] Phase3-Maintainability iter1: [AC-006] AC#20 filter 'ConsoleOutput' may not match implementer naming — user chose to skip (same as iter4 decision)
- [fix] Phase3-Maintainability iter1: AC#18 Detail | Added AC#10 compilation correctness cross-reference for int→long widening
- [fix] PostLoop-UserFix post-loop: [SCP-001] Tasks/Implementation Contract | Added Task 10 for F775/F776/F777 Dependency update

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning
- [Predecessor: F774](feature-774.md) - Shop Core (Mandatory Handoff origin)
- [Successor: F775](feature-775.md) - Collection (uses DrawLine)
- [Successor: F776](feature-776.md) - Items (uses DrawLine)
- [Successor: F777](feature-777.md) - Customization (uses DrawLine + Bar)
- [Successor: F782](feature-782.md) - Post-Phase Review
- [Related: F778](feature-778.md) - Body Settings
- [Related: F779](feature-779.md) - Body Settings
- [Related: F780](feature-780.md) - Body Settings
- [Related: F781](feature-781.md) - Body Settings
- [Related: F789](feature-789.md) - IVariableStore Phase 20 Extensions
- [Related: F790](feature-790.md) - Engine Data Access Layer
- [Related: F791](feature-791.md) - Engine State Transitions
