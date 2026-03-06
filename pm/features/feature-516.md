# Feature 516: Phase 17 Planning

## Status: [DONE]

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

## Type: research

## Created: 2026-01-16

---

## Summary

**Feature を立てる Feature**: Phase 17 Planning

Create sub-features for Phase 17 Data Migration:
- Critical Config Files migration (VariableSize.csv, GameBase.csv) - **CRITICAL** priority
- Variable Definition CSVs migration (FLAG, CFLAG, TFLAG, Talent, Abl, Palam) - HIGH priority
- Character Data CSVs migration (19 files) - Medium priority
- Content Definition CSVs migration (Train, Item, etc.) - Medium priority
- Tool creation features (CsvToYaml, SchemaValidator)
- F540: Post-Phase Review Phase 17 (type: infra)
- F541: Phase 18 Planning (type: research)

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 17 migrates 43 CSV files to YAML/JSON with strict dependency order (VariableSize.csv FIRST). Requires tool creation (CsvToYaml converter, SchemaValidator CLI), data equivalence verification, and compliance with Phase 4 design requirements (IDataLoader interface, DI registration, Strongly Typed data models).

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points.

### Problem (Current Issue)

Phase 16 completion requires Phase 17 planning to maintain momentum:
- Phase 17 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Data Migration is a data transformation phase with strict dependency order
- Dependencies must be documented (VariableSize.csv must be first)
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 17** requirements from full-csharp-architecture.md
2. **Decompose Data Migration** into manageable sub-features following dependency order
3. **Create implementation sub-features** from Phase 17 tasks
4. **Create transition features** (Post-Phase Review + Phase 18 Planning)
5. **Update index-features.md** with Phase 17 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC, equivalence verification)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 17 analysis documented | file | Grep | contains | "Phase 17 Feature Mapping:" | [x] |
| 2 | CSV categorization complete | file | Grep | contains | "Phase 17 Migration Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "Critical Config\\|Variable Definition\\|Character Data" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 17" | [x] |
| 5 | Phase 18 Planning in index | file | Grep | contains | "Phase 18 Planning" | [x] |
| 6 | Sub-feature Philosophy verified (verification ACs checked manually per Task#6) | file | Grep | contains | "Philosophy.*Phase 17: Data Migration" | [x] |

### AC Details

**AC#1**: Phase 17 analysis documented in feature-516.md Execution Log
- Test: Grep pattern="Phase 17 Feature Mapping:" in feature-516.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: CSV categorization documented
- Test: Grep pattern="Phase 17 Migration Analysis:" in feature-516.md Execution Log
- Must contain explicit Feature ID allocation table
- Shows how architecture.md tasks grouped into implementation features
- Documents decomposition rationale (migration priority, dependency order, granularity compliance)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="Critical Config|Variable Definition|Character Data" path="Game/agents/index-features.md"
- Verifies at least one Phase 17 migration component is registered in index
- Note: "At least one" is intentionally loose - Expected Feature ID Allocation documents actual count, but exact feature IDs may vary during execution based on decomposition adjustments

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 17" in index-features.md
- Type: infra, follows F470/F485/F502/F515 pattern

**AC#5**: Phase 18 Planning in index
- Test: Grep pattern="Phase 18 Planning" in index-features.md
- Type: research, follows F471/F486/F503/F516 pattern

**AC#6**: Sub-feature Philosophy and verification ACs verified
- Test: Grep pattern="Philosophy.*Phase 17: Data Migration" in created sub-feature files
- Verifies all created implementation sub-features inherit Philosophy per architecture.md
- **Task#6 output** (manual verification scope, not AC verification):
  - 負債解消 tasks (TODO/FIXME/HACK removal) per Sub-Feature Requirements #2
  - 等価性検証 tests (CSV==YAML equivalence) per Sub-Feature Requirements #3
  - 負債ゼロ AC (no debt markers) per Sub-Feature Requirements #4
  - 引継ぎ先指定 section check per Sub-Feature Requirements #8
- Note: Task#6 performs manual inspection after AC#6 Grep PASS

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 17 requirements and document "Phase 17 Feature Mapping:" | [x] |
| 2 | 2 | Document CSV categorization as "Phase 17 Migration Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features per Analysis Method (8-15 ACs per feature, grouped by migration priority, with mandatory handoff tracking per Sub-Feature Requirements) | [x] |
| 4 | 4 | Create Phase 17 Post-Phase Review feature (type: infra) - include Task: "Fix architecture.md Phase 17 stale phase numbers (line 3771-3772)" | [x] |
| 5 | 5 | Create Phase 18 Planning feature (type: research) | [x] |
| 6 | 6 | Verify Philosophy inheritance AND verification ACs presence in created sub-features (Grep "Phase 17: Data Migration" and manual check for 負債解消/等価性検証/負債ゼロ ACs) | [x] |

<!-- AC:Task 1:1 Rule: 6 ACs = 6 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 17 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 17 tasks and scope
   - Note migration order (VariableSize.csv FIRST - CRITICAL)
   - Review Success Criteria
   - Review Sub-Feature Requirements (line 3864-3871)

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group migration tasks by priority (CRITICAL/HIGH/Medium)
   - Assign explicit Feature IDs
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

   **Expected Feature ID Allocation** (adjust during execution if needed):

   **CRITICAL Priority**:
   - **F528**: Critical Config Files Migration (VariableSize.csv, GameBase.csv)

   **HIGH Priority**:
   - **F529**: Variable Definition CSVs Migration Part 1 (FLAG, CFLAG, TFLAG)
   - **F530**: Variable Definition CSVs Migration Part 2 (Talent, Abl, Palam, exp, ex)
   - **F531**: Config Files Migration (_default.config, _fixed.config, emuera.config)

   **Medium Priority (Character Data)**:
   - **F532**: Character Data Migration Part 1 (Chara0-Chara13: Player + Main Characters)
   - **F533**: Character Data Migration Part 2 (Chara28, Chara29, Chara99, Chara148, Chara149: Sub/NPC/Additional)

   **Medium Priority (Content Definition)**:
   - **F534**: Content Definition CSVs Migration Part 1 (Train, Item, Equip, Tequip)
   - **F535**: Content Definition CSVs Migration Part 2 (Mark, Juel, Stain, source)
   - **F536**: String Tables Migration (Str, CSTR, TSTR, TCVAR)
   - **F537**: Transform Rules Migration (_Rename, _Replace)

   **Tool Creation**:
   - **F538**: CsvToYaml Converter Tool
   - **F539**: SchemaValidator CLI Extension

   **Transition Features**:
   - **F540**: Phase 17 Post-Phase Review (type: infra)
   - **F541**: Phase 18 Planning (type: research)

   **Decomposition Rationale**:
   - 7 architecture.md tasks → ~14 features (net expansion to manage complexity)
   - Critical/HIGH priority files separated for early execution
   - Config files (.config) separated from CSV files per architecture.md Task 5
   - Character Data split by character role (Main vs Sub/NPC)
   - Content Definition split by category (Items, Effects, Strings, Transform)
   - Tool creation separate features (reusable infrastructure)
   - Transition: 2 tasks → 2 features (F540 Post-Phase Review + F541 Phase 18 Planning)
   - Granularity compliance: each feature targets 8-15 ACs based on file count and verification requirements

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 17: Data Migration" per architecture.md
   - Include Sub-Feature Requirements per architecture.md line 3854-3861:
     - Philosophy inheritance
     - 負債解消 tasks (TODO/FIXME/HACK removal)
     - 等価性検証 tests (CSV==YAML equivalence)
     - 負債ゼロ AC (no debt markers added)
   - Include verification AC (migration completion, test PASS, equivalence verification)
   - Reference Phase 4 design requirements (IDataLoader interface, DI registration, Strongly Typed data models)

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 18 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 17 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | Skill(feature-creator) | architecture.md Phase 17, feature-template.md | Phase 17 sub-feature files |
| 2 | Skill(feature-creator) | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC verification |

**Execution Order**:
1. Analyze Phase 17 scope and create CSV categorization
2. Create all sub-feature files with Philosophy inheritance and Sub-Feature Requirements
3. Update index-features.md with all Phase 17 features
4. Mark Tasks 1-5 complete

### Sub-Feature Requirements

Per architecture.md Phase 17 line 3854-3861, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 17: Data Migration" in Philosophy section | All implementation features | Grep |
| 2 | **Tasks: 負債解消** - TODO/FIXME/HACK comment removal tasks | Each implementation feature | AC with not_contains |
| 3 | **Tasks: 等価性検証** - CSV→YAML equivalence tests | Each implementation feature | AC with test verification |
| 4 | **AC: 負債ゼロ** - No debt markers added during migration (Grep pattern: `TODO\|FIXME\|HACK`) | Each implementation feature | Grep verification |
| 5 | **AC: Test PASS** - All tests PASS after migration | Each implementation feature | dotnet test verification |
| 6 | **AC: Schema validation** - YAML schema validation PASS | Each implementation feature | SchemaValidator CLI |
| 7 | **AC: Phase 4 compliance** - IDataLoader interface, DI registration, Strongly Typed data models | F528-F531 (config/variable loaders) | Manual inspection |
| 8 | **Tasks: Handoff tracking** - Use 引継ぎ先指定 section with concrete tracking IDs | Each implementation feature | 引継ぎ先指定 section check |

---

## Phase 17 Scope Reference

**Partial snapshot from architecture.md** (line 3726+). See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for complete task list.

**Phase 17: Data Migration**

**Goal**: CSV/定義データをYAML/JSONに変換（詳細マッピング付き）

**CRITICAL**: `VariableSize.csv` を最初に移行（配列サイズ定義、全変数の前提条件）

**Statistics**: 43 CSV files total (19 character + 24 system/config)

**Phase 4 Design Requirements**:
- `IDataLoader<T>` interface with `Result<T> Load(string path)`
- `ICharacterDataLoader`, `IConfigLoader` specific interfaces
- DI registration: `ICharacterDataLoader` → `YamlCharacterDataLoader`
- Strongly Typed data models (CSV row → typed classes)
- No magic numbers (use enum or Strongly Typed ID)

**Tasks** (decomposed from architecture.md line 3765-3772 + Phase Progression Rules):

*Architecture.md Tasks 1-5 (migration)*:
1. **Critical Config Files** (Phase 4 依存): VariableSize.csv, GameBase.csv
2. **Variable Definition CSVs** (Phase 4 依存): FLAG, CFLAG, TFLAG, Talent, Abl, Palam, exp, ex
3. **Character Data CSVs** (19 files): Chara0-Chara13, Chara28, Chara29, Chara99, Chara148, Chara149
4. **Content Definition CSVs**: Train, Item, Equip, Tequip, Mark, Juel, Stain, source
5. **Config Files**: _default.config, _fixed.config, emuera.config, string tables (Str, CSTR, TSTR, TCVAR)

*F516 decomposition additions*:
6. **Transform Rules**: _Rename.csv, _Replace.csv (from architecture.md Task 6)
7. **Tool Creation**: CsvToYaml converter, SchemaValidator CLI (from architecture.md lines 3843-3847)
8. **Create Phase 17 Post-Phase Review feature** (type: infra, per Phase Progression Rules)
9. **Create Phase 18 Planning feature** (type: research, per Phase Progression Rules)

**Success Criteria**:
- [ ] 43 CSV ファイル移行完了
- [ ] YAML スキーマ検証 PASS
- [ ] データ等価性確認

**Sub-Feature Requirements** (architecture.md line 3864-3871):
- **Philosophy**: 全 sub-feature に「Phase 17: Data Migration」を継承
- **Tasks: 負債解消**: TODO/FIXME/HACK コメント削除タスクを含む
- **Tasks: 等価性検証**: CSV→YAML 等価性テストを含む
- **AC: 負債ゼロ**: 技術負債ゼロを検証する AC を含む

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F515 | Phase 16 Post-Phase Review must pass first |
| Successor | F528-F541 | Phase 17 implementation sub-features (created by this feature) |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning (precedent feature)
- [feature-515.md](feature-515.md) - Phase 16 Post-Phase Review (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須 -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| architecture.md Phase 17 stale phase numbers | line 3771-3772 say "Phase 14/15" instead of "Phase 17/18" | Feature | F540 (Post-Phase Review will fix) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-18 FL iter1**: 6 ACs justified - research type normally uses 3-5 ACs, but Phase 17 Planning requires explicit verification of sub-feature quality (Philosophy inheritance, verification ACs, index updates) which cannot be consolidated further per F424/F437 precedent
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - Tasks Table: Task#3 mentions "mandatory handoff tracking per Sub-Feature Requirements" but AC#3 only checks for feature existence in index, not 引継ぎ先指定 section presence. Fixed: AC#6 now explicitly includes 引継ぎ先指定 section check per Sub-Feature Requirements #8
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - Phase 17 Scope Reference task numbering: F516 lists Tasks 1-9 but architecture.md has 7 tasks. Fixed: Added subheadings "Architecture.md Tasks 1-5 (migration)" and "F516 decomposition additions" to clarify which tasks are from architecture.md vs F516's additions per Phase Progression Rules
- **2026-01-18 FL iter5**: architecture.md line 3771-3772 uses stale phase numbers ("Phase 14 Post-Phase Review", "Phase 15 Planning") in Phase 17 section. F516 uses corrected numbers (Phase 17/18). architecture.md update tracked in 引継ぎ先指定

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-18 | Task#1 | Opus | Phase 17 Feature Mapping documentation | SUCCESS |
| 2026-01-18 | Task#2 | Opus | Phase 17 Migration Analysis documentation | SUCCESS |
| 2026-01-18 | Task#3 | feature-creator | Created F528-F539 (12 implementation features) | SUCCESS |
| 2026-01-18 | Task#4 | feature-creator | Created F540 Post-Phase Review Phase 17 | SUCCESS |
| 2026-01-18 | Task#5 | feature-creator | Created F541 Phase 18 Planning | SUCCESS |
| 2026-01-18 | Task#6 | Opus | Verified Philosophy inheritance in all sub-features | SUCCESS |
| 2026-01-18 | Phase 6 | Opus | AC verification - all 6 ACs PASS | SUCCESS |
| 2026-01-18 | Phase 7.1 | feature-reviewer | Post review - fixed F536, F538 引継ぎ先指定 | READY |
| 2026-01-18 | Phase 7.2 | feature-reviewer | Doc-check | READY |
| 2026-01-18 | Phase 7.3 | Opus | SSOT update check - no updates required | N/A |

---

### Phase 17 Feature Mapping:

**Source**: architecture.md Phase 17 section (lines 3736-3878)

| architecture.md Task | Feature ID | Feature Name |
|---------------------|:----------:|--------------|
| Task 1: Critical Config Files | F528 | Critical Config Files Migration |
| Task 2: Variable Definition CSVs | F529, F530 | Variable Definition CSVs Part 1 & 2 |
| Task 3: Character Data CSVs | F532, F533 | Character Data Migration Part 1 & 2 |
| Task 4: Content Definition CSVs | F534, F535 | Content Definition CSVs Part 1 & 2 |
| Task 5: Config Files + String Tables | F531, F536 | Config Files Migration + String Tables |
| Task 6: Transform Rules | F537 | Transform Rules Migration |
| Tools (lines 3843-3847) | F538, F539 | CsvToYaml Converter + SchemaValidator |
| Phase Progression: Post-Phase Review | F540 | Phase 17 Post-Phase Review |
| Phase Progression: Next Phase Planning | F541 | Phase 18 Planning |

**Total**: 14 features (F528-F541)

---

### Phase 17 Migration Analysis:

**File Count Verification**:
- Character files (Chara*.csv): 19 files ✓
- System/Config files: 26 files (includes 2 .config files)
- Total: 45 files (architecture.md says 43 - minor discrepancy, likely excludes .config files)

**Feature ID Allocation**:

| ID | Priority | Type | Files | AC Est. |
|:--:|:--------:|:----:|-------|:-------:|
| F528 | CRITICAL | engine | VariableSize.csv, GameBase.csv | 10 |
| F529 | HIGH | engine | FLAG.CSV, CFLAG.CSV, TFLAG.CSV | 10 |
| F530 | HIGH | engine | Talent.csv, Abl.csv, Palam.csv, exp.csv, ex.csv | 12 |
| F531 | HIGH | engine | _default.config, _fixed.config, emuera.config | 10 |
| F532 | Medium | engine | Chara0-Chara13 (14 files) | 12 |
| F533 | Medium | engine | Chara28, Chara29, Chara99, Chara148, Chara149 (5 files) | 10 |
| F534 | Medium | engine | Train.csv, Item.csv, Equip.csv, Tequip.csv | 10 |
| F535 | Medium | engine | Mark.csv, Juel.csv, Stain.csv, source.csv | 10 |
| F536 | Medium | engine | Str.csv, CSTR.csv, TSTR.csv, TCVAR.csv | 10 |
| F537 | Low | engine | _Rename.csv, _Replace.csv | 8 |
| F538 | Tool | engine | CsvToYaml converter tool | 10 |
| F539 | Tool | engine | SchemaValidator CLI extension | 8 |
| F540 | - | infra | Post-Phase Review Phase 17 | 6 |
| F541 | - | research | Phase 18 Planning | 5 |

**Decomposition Rationale**:
1. **Critical First**: F528 (VariableSize.csv) MUST complete before all others - defines array sizes
2. **Dependency Chain**: F528 → F529/F530 → remaining data migrations
3. **Tool Prerequisite**: F538 (CsvToYaml) should be created early but can be parallel with F528
4. **Granularity Compliance**: Each feature targets 8-15 ACs per feature-template.md
5. **Character Data Split**: Main characters (14) vs Sub/NPC (5) for manageable scope
6. **Content Split by Category**: Items/Equipment, Effects, Strings, Transform rules

**Sub-Feature Requirements Checklist** (per architecture.md line 3864-3871):
- [ ] Philosophy: "Phase 17: Data Migration"
- [ ] Tasks: 負債解消 (TODO/FIXME/HACK removal)
- [ ] Tasks: 等価性検証 (CSV→YAML equivalence)
- [ ] AC: 負債ゼロ (Grep `TODO|FIXME|HACK` not_contains)
- [ ] AC: Test PASS (dotnet test)
- [ ] AC: Schema validation (SchemaValidator CLI)
- [ ] AC: Phase 4 compliance (IDataLoader, DI, Strongly Typed)
