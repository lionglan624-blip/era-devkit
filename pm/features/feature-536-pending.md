# Feature 536 Pending User Decisions

## Iteration: 1
## Phase: Phase2-Validate, Phase3-Maintainability

---

## Issue 1: String Table File Path Location (Phase2-Validate)

**Severity**: major
**Location**: AC#1-4 file paths

**Issue**:
String table YAML files are currently placed in `Era.Core/Data/Strings/` (e.g., `Era.Core/Data/Strings/Str.yaml`), but architecture.md Phase 17 deliverables table (lines 3833-3841) shows all CSV→YAML migrations targeting `Game/` paths:
- `VariableSize.csv` → `Game/config/variable_sizes.yaml`
- `GameBase.csv` → `Game/config/game_base.yaml`
- `FLAG.CSV` → `Game/config/flags.yaml`
- etc.

F528 (Critical Config Files Migration) established `Game/config/` as the target location for critical config files. String tables (Str, CSTR, TSTR, TCVAR) are categorized under "Strings" in architecture.md line 3823, alongside other CSV files that target `Game/` directories.

**Question**:
Should string table YAML files be placed in:
- **Option A**: `Era.Core/Data/Strings/` (current design) - if string tables are engine-layer data models requiring Era.Core integration
- **Option B**: `Game/config/strings/` - to align with F528 pattern and architecture.md deliverables table
- **Option C**: Different location with specific rationale

**Context**:
- F528 placed VariableSize/GameBase in `Game/config/` as config files
- Architecture.md line 3823 lists Strings category: `Str.csv`, `CSTR.csv`, `TSTR.csv`, `TCVAR.csv`
- Architecture.md deliverables table doesn't show Era.Core paths for any CSV migrations
- String tables contain string constants referenced by game logic, not necessarily engine internals

**Impact if unchanged**:
- If Era.Core/Data/Strings/ is incorrect, equivalence tests will fail to find files
- Inconsistent migration pattern across Phase 17 features
- Potential rework of file paths, tests, and Implementation Contract

**Recommendation**:
Clarify semantic classification of string tables (engine data vs game data) to determine correct placement. If string tables are game data (referenced by game scripts), follow F528 pattern and use `Game/config/strings/`.

---

## Issue 2: Missing F528 Pattern Elements (Phase3-Maintainability)

**Severity**: critical
**Location**: Implementation Contract - Pattern Completeness

**Issue**:
F528 (the dependency/pattern precedent) created a complete data loading architecture:
1. **Interfaces**: IVariableSizeLoader, IGameBaseLoader
2. **Implementations**: YamlVariableSizeLoader, YamlGameBaseLoader
3. **Data Models**: VariableSizeConfig, GameBaseConfig (strongly typed)
4. **DI Registration**: AddSingleton<IVariableSizeLoader, YamlVariableSizeLoader>
5. **ACs for each component**: AC#3-8 verified interfaces, models, and DI registration

F536 currently only specifies:
- YAML file creation (AC#1-4)
- Equivalence tests (AC#5-8)
- Build/debt verification (AC#9-10)

**Missing from F536**:
- No string table loader interfaces mentioned
- No data model definitions (StrConfig, CstrConfig, TstrConfig, TcvarConfig)
- No DI registration documented
- No ACs to verify these components

**Question**:
Should F536 follow the complete F528 pattern or use a simplified approach?

**Option A: Complete Pattern** (Zero Debt Upfront)
- Define IStringTableLoader interfaces (or individual IStrLoader, ICstrLoader, etc.)
- Implement YamlStringTableLoader classes
- Create strongly typed models (StrConfig, CstrConfig, TstrConfig, TcvarConfig)
- Document DI registration
- Add ACs to verify interfaces/loaders/models exist
- Add Tasks for interface/loader/model creation
- Benefits: Consistent with F528, enables DI-based engine integration, supports unit testing
- Cost: Increases scope from 4 tasks to ~7-8 tasks, adds ~150 lines of code

**Option B: YAML-Only Migration**
- Create YAML files only (current design)
- No loaders/interfaces (direct YAML loading in game code)
- Benefits: Smaller scope, faster implementation
- Concerns: Violates F528 pattern consistency, creates technical debt (no abstraction), harder to unit test, doesn't align with Phase 4 design requirements (IDataLoader interface pattern)

**Option C: Defer Loader Implementation**
- Create YAML files in F536
- Create loaders/interfaces in separate follow-up feature
- Benefits: Splits work into smaller chunks
- Concerns: Creates intermediate state where YAMLs exist but no typed access, violates "complete migration" goal

**Context**:
- Architecture.md Phase 17 explicitly requires: "Phase 4 Design Requirements - IDataLoader interface, DI registration, Strongly Typed data models" (lines 3744-3764)
- F528 Review Notes (line 459) show FL validation caught path mismatches - similar scrutiny expected for F536
- Zero Debt Upfront principle: Option A is technically correct, Options B/C create debt

**Recommendation**:
Option A (Complete Pattern) aligns with Zero Debt Upfront principle and architecture.md Phase 17 requirements. Expand F536 scope to include loaders/interfaces/models, following F528 precedent exactly.

**Impact if unchanged**:
- Violates architecture.md Phase 17 requirements
- Creates inconsistent migration pattern (F528 has loaders, F536 doesn't)
- Blocks future DI-based engine integration
- Technical debt accumulation (direct YAML access instead of abstraction)

---

## Issue 3: Schema Design Documentation Missing (Phase3-Maintainability)

**Severity**: major
**Location**: Implementation Contract - Schema Design section missing

**Issue**:
Implementation Contract states "manual schema creation - YamlSchemaGen only supports dialogue schemas" but provides no schema design guidance. F528 documented detailed YAML structure examples with type mapping rules (lines 352-387). String tables have different structures that need upfront design.

**Question**:
What are the expected YAML structures for each string table?

**Required Information**:
1. **Str.csv structure analysis**:
   - Current CSV format: ?
   - Target YAML format: ?
   - Key naming convention: ?

2. **CSTR.csv structure analysis**:
   - Current CSV format: ?
   - Target YAML format: ?
   - Key naming convention: ?

3. **TSTR.csv structure analysis**:
   - Current CSV format: ?
   - Target YAML format: ?
   - Key naming convention: ?

4. **TCVAR.csv structure analysis**:
   - Current CSV format: ?
   - Target YAML format: ?
   - Key naming convention: ?

**Context**:
- F528 provided complete YAML examples (VariableSize.yaml with inline flow style [dim1, dim2], GameBase.yaml with English key mappings)
- String tables may contain: simple key-value pairs, indexed arrays, localized strings, format templates
- Schema validation requires JSON Schema definitions - need structure first

**Recommendation**:
Read actual CSV files (Game/CSV/Str.csv, CSTR.csv, TSTR.csv, TCVAR.csv) and derive YAML structure. Add "Schema Design" section to Implementation Contract with concrete examples.

**Impact if unchanged**:
- Implementer must guess YAML format (error-prone)
- No schema validation possible without defined structure
- Equivalence tests cannot be written without knowing target format
- Risk of format mismatch between files

---

**Total pending issues**: 3
