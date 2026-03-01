---
description: Audit documentation consistency
---

**Language**: Thinking in English, respond to user in Japanese.

---

Audit documentation consistency across the project. Perform these checks:

---

# Part A: Reference Chain Audit

Starting from CLAUDE.md, audit all documents reachable by following references.

## A1. Reference Graph Construction

Starting from `CLAUDE.md`, build a directed graph of document references:

```
CLAUDE.md
├── .claude/reference/agent-registry.md (Agent Table)
├── .claude/agents/*.md (agent definitions)
├── .claude/commands/*.md (Slash Commands table)
├── .claude/skills/*/SKILL.md (Skills table)
├── pm/index-features.md (Key Documents table)
├── pm/reference/*.md (Key Documents table)
└── src/tools/*/README.md (Project Structure)
```

### A1.1 Reachability Check
- Start from CLAUDE.md
- For each referenced file, recursively check its references
- Build complete reachability set
- Report unreachable files in `.claude/` and `pm/` directories

### A1.2 Orphan Detection
- Files in `.claude/agents/`, `.claude/commands/`, `.claude/skills/` not referenced from CLAUDE.md
- Files in `pm/reference/` not referenced from any document
- Report as `[ORPHAN]`

### A1.3 Circular Reference Detection
- Detect cycles in reference graph (A → B → C → A)
- Report cycles (not necessarily errors, but worth noting)

### A1.4 Broken Reference Detection
- For each reference found, verify target file exists
- Report as `[BROKEN]` with source file and line number

---

# Part B: Category Exhaustive Audit

Audit each category independently and exhaustively.

## B1. Feature Status Consistency

Compare `index-features.md` with each `feature-*.md`:
- For each feature in Completed Features section with `[DONE]`
- Verify the corresponding feature-*.md has `Status: [DONE]`
- Report any mismatches

## B2. Task/AC Completion Consistency

For each feature marked `[DONE]`:
- Check feature-*.md Tasks table all marked `[○]`
- Check feature-*.md Acceptance Criteria all marked `[x]`
- Report incomplete items

## B3. Interface Documentation

Check interface files in the engine repo (`$ENGINE_PATH` / `C:\Era\engine`):
- Path: `Assets/Scripts/Emuera/Sub/I*.cs`
- List all interface files found
- Verify each is documented in `docs/reference/engine-reference.md`
- Report undocumented interfaces

## B4. Links Validation

For each `.md` file in `pm/`:
- Parse `## Links` section
- Verify each linked file exists
- Report broken links

**Exception**: `archive/` files may have stale links to files that were later renamed/split/deleted. Report as `[INFO]` not `[WARN]` (historical documents).

## B5. Skills Structure Consistency

Verify `.claude/skills/` structure is valid:

### 5.1 SKILL.md Existence
- Each skill directory must have a `SKILL.md` file
- Check: all directories in `.claude/skills/`

### 5.2 YAML Frontmatter
- Each SKILL.md must have valid YAML frontmatter with `description:` field
- Frontmatter must be parseable

### 5.3 Skill Registration
- All skill directories in `.claude/skills/` MUST be listed in CLAUDE.md Skills table
- Report unregistered skills as `[ORPHAN]`

### 5.4 Phase File Integrity
- For multi-phase skills (run-workflow, fl-workflow, feature-quality), parse SKILL.md for `Read()` or `PHASE-*.md` references
- Verify each referenced phase file actually exists on disk
- Report missing phase files as `[BROKEN]`
- Check for orphan phase files not referenced from SKILL.md or any other phase file
- Expected structures:
  - `run-workflow/`: PHASE-1.md through PHASE-10.md
  - `fl-workflow/`: PHASE-1.md through PHASE-8.md, POST-LOOP.md
  - Other skills: single SKILL.md (no phases expected)

## B6. Testing Documentation Consistency

Check `.claude/skills/testing/*.md` files:

### 6.1 Test Type Coverage
- SKILL.md documents test workflow overview
- ENGINE.md, KOJO.md cover respective test types
- All test types mentioned in CLAUDE.md are documented

### 6.2 Cross-Reference Accuracy
- Commands referenced in testing docs exist in `.claude/commands/`
- Agent references match `.claude/agents/` definitions

### 6.3 Test Scenario Inventory
- Check `test/**/*.json` exist and are valid JSON
- Directories: `ac/`
- Check for orphaned scenarios (reference deleted functions)
- Ignore `_archived/` and `output/` directories

---

## B7. Tool Documentation

### 7.1 Tool Directory Inventory
- Scan all subdirectories under `src/tools/` (excluding `_archived/`, `tests/`, `*Tests/`)
- For each tool directory, check if `README.md` exists
- Cross-check against CLAUDE.md Project Structure section: every tool listed there must have a directory, and every directory should be listed
- Report missing README as `[WARN]`, unregistered directory as `[ORPHAN]`

### 7.2 Active Tool README Validation
- For each tool with README.md, verify it contains:
  - Purpose/description section
  - Usage or CLI options section
- Report incomplete READMEs as `[INFO]`

### 7.3 Archived Tools
- Check `src/tools/dotnet/_archived/` subdirectories
- Verify each has README.md for historical documentation
- Report as `[INFO]` only (historical, not actionable)

---

## B8. Subagent Definitions

Check `.claude/agents/*.md` files:

### 8.1 Structure Validation
- Each agent file must have YAML frontmatter with `name`, `description`, `model`, `tools`
- Verify model is valid: `haiku`, `sonnet`, or `opus`
- Verify tools list matches agent purpose

### 8.2 CLAUDE.md Registration
- All agents in `.claude/agents/` must be listed in `.claude/reference/agent-registry.md` Agent Table
- Report unregistered agents as `[ORPHAN]`

### 8.2a Skill/Agent Boundary Check
- Some entries in agent-registry.md Agent Table are implemented as Skills (context:fork), not agent .md files
- For each entry in the Agent Table, check:
  - If `.claude/agents/{name}.md` exists → OK (agent)
  - Else if `.claude/skills/{name}/SKILL.md` exists → OK (skill-based agent)
  - Else → `[BROKEN]` (registered but no implementation)
- For agent name mismatches (e.g., table says "philosophy-deriver" but file is "philosophy-definer"), report as `[MISMATCH]` with suggested correction

### 8.3 Cross-Reference Validation
- Commands referencing agents → verify agent file exists
- Agent files referencing Skills → verify skill exists
- Agent files referencing other agents → verify target exists

### 8.4 Stale Reference Check
- Check for references to completed/archived features
- Check for references to deleted files

---

## B8a. Command Definitions

Check `.claude/commands/*.md` files:

### 8a.1 Structure Validation
- Each command file must have YAML frontmatter with `description`
- Command body should contain clear instructions

### 8a.2 CLAUDE.md Registration
- All commands in `.claude/commands/` must be listed in CLAUDE.md Slash Commands table
- Report unregistered commands as `[ORPHAN]`

### 8a.3 Cross-Reference Validation
- Commands referencing agents → verify agent file exists
- Commands referencing skills → verify skill exists
- Commands referencing other commands → verify target exists

### 8a.4 Consistency Check
- Command description in CLAUDE.md matches file's frontmatter description
- Report mismatches

---

## B8b. Git Hooks

Check `.githooks/` files:

### 8b.1 Hook Existence
- Verify expected hooks exist: `pre-commit`, `schema-sync-check`
- Report missing expected hooks

### 8b.2 Hook Executability
- Verify hooks have execute permission
- Report non-executable hooks

### 8b.3 Hook Documentation
- Hooks should be documented in CLAUDE.md or a reference file
- Report undocumented hooks

### 8b.4 Hook Validity
- Parse hook scripts for syntax errors
- Verify referenced commands/tools exist

---

## B9. CSV/Kojo Consistency

### 9.1 Talent.csv vs kojo-reference.md
- TALENT numbers in kojo-reference.md match Talent data definitions in the game repo (`$GAME_PATH` / `C:\Era\game`)
- Lover/Admiration/Affection branch numbers match

---

## B10. Designs Status Check

Check `docs/architecture/` files:

### 10.1 Stale APPROVED Designs
- Find designs with `Status: APPROVED` that have no corresponding Features in `index-features.md`
- Report as `[STALE]` if APPROVED for 30+ days without Feature creation

### 10.2 DRAFT Cleanup
- Count DRAFT designs older than 60 days
- Suggest archive or deletion

### 10.3 Architecture README Sync
- Verify `docs/architecture/README.md` lists all design subdirectories and key files
- Report orphan design files not referenced from README

---

## B11. KOJO_KX.ERB Template Consistency

Check `pm/templates/KOJO_KX.ERB` against actual kojo files:

### 11.1 Guide Section Presence
- Verify template has "ABL/TALENT分岐ガイド" section
- Check all 10 character files follow template structure

### 11.2 Function Naming Convention
- Spot-check 5 kojo files for `KOJO_MESSAGE_COM_K{N}_{COM}` pattern
- Report non-compliant function names

---

## B12. Test Coverage Delta

Track test file changes:

### 12.1 Test File Count
- Count files in `test/ac/`
- Compare with previous audit (if available in `_out/tmp/audit-last.json`)
- Report delta: `+N new tests` or `-N removed tests`

### 12.2 Feature Coverage
- List Features with AC tests in `test/ac/kojo/feature-*/`
- Identify recent DONE features without AC test directories

---

## B13. COM Implementation Status

Cross-check `Skill(kojo-writing)` COM → File Placement and `content-roadmap.md` COM Series Order with actual implementation:

### 13.1 Kojo Function Existence
- For each COM in kojo-writing SKILL (COM → File Placement section):
  - Check if `KOJO_MESSAGE_COM_K*_{COM}` functions exist in ERB files in the game repo (`$GAME_PATH` / `C:\Era\game`)
  - Report missing implementations by COM range

### 13.2 index-features.md Sync
- Compare "Phase 8 Summary" done counts with actual feature completion
- Report mismatches

### 13.3 Dead COM Detection
- Identify COMs with no kojo functions AND no planned Feature in index-features.md
- Report as candidates for Feature creation or documentation update

---

## B14. Reference File Consolidation Check

Identify opportunities to reduce reference file count:

### 14.1 Small File Detection
- List reference/*.md files under 100 lines
- Suggest merge candidates (e.g., erb-reference.md + hooks-reference.md)

### 14.2 Overlap Detection
- Check for similar content between:
  - `kojo-reference.md` and `kojo-writing/SKILL.md`
  - `engine-reference.md` and `engine-dev/SKILL.md`
- Report significant overlap (>50% similar sections)

### 14.3 Unused Reference Detection
- Find reference/*.md files not linked from any feature or agent
- Report as consolidation candidates

---

## B15. Document Bloat Risk

Monitor document sizes to prevent context window inefficiency.

### 15.1 Core Document Size Check

| Document | Warning Threshold | Error Threshold |
|----------|------------------:|----------------:|
| CLAUDE.md | 200 lines | 280 lines |
| index-features.md | 200 lines | 300 lines |
| reference/*.md (each) | 800 lines | 1200 lines |

- Count lines for each document
- Report if exceeding warning threshold
- Suggest split/refactor if exceeding error threshold

### 15.2 Context Load Estimation

Estimate typical session context load:
```
Session context = CLAUDE.md + agents.md + (triggered rules) + (read references)
```

- Calculate worst-case context load (all rules + all references)
- Report if total exceeds 3000 lines

---

## B16. Key Documents Existence

Verify all files listed in CLAUDE.md Key Documents table actually exist:

### 16.1 Key Documents Table
- Parse the `| Document | Purpose |` table in CLAUDE.md
- For each document path, verify the file exists on disk
- Report missing files as `[BROKEN]`

### 16.2 External Path Validation
- Check `ERATW_PATH` external dependency path if accessible
- Report as `[INFO]` if inaccessible (external, not an error)

---

## B17. Settings & Configuration Consistency

Check `.claude/settings.json` and `.claude/settings.local.json`:

### 17.1 Hook Reference Validation
- Parse hooks defined in settings.json
- Verify each hook's command/script path exists
- Report broken hook references as `[BROKEN]`

### 17.2 Permission Coherence
- Check that allowed/denied tool patterns are consistent
- Report contradictory rules (same pattern in both allow and deny) as `[WARN]`

### 17.3 Settings Documentation
- Verify CLAUDE.md mentions `settings.json` configuration where relevant (e.g., Git Hooks Setup)
- Report undocumented critical settings as `[INFO]`

---

## B18. Temporary File Hygiene

Monitor `_out/tmp/` directory for accumulation:

### 18.1 File Count & Size
- Count files in `_out/tmp/`
- Calculate total size
- Report as `[INFO]` with counts
- Report `[WARN]` if >50 files or >50MB total

### 18.2 Staleness Check
- List files older than 30 days
- Suggest cleanup candidates as `[INFO]`

---

## B19. Language Consistency (English Audit)

Technical documents should be written in English. Documents requiring Japanese nuance are exempt.

### 19.1 Scope Classification

**Must be English** (audit targets):
- `.claude/agents/*.md` — Agent definitions
- `.claude/commands/*.md` — Command definitions
- `.claude/skills/*/SKILL.md` and phase files — Skill definitions
- `docs/architecture/*.md` — Design documents
- `docs/reference/engine-reference.md` — Engine technical reference
- `pm/reference/feature-template.md` — Feature template
- `src/tools/*/README.md` — Tool documentation
- `CLAUDE.md` — Project instructions
- `NOTICE.md` — License information

**Japanese exempt** (skip):
- Game repo ERB files (`$GAME_PATH`) — Game scripts (in-game text)
- Game repo CSV files (`$GAME_PATH`) — Game data definitions
- `pm/content-roadmap.md` — Content planning (character/plot nuance)
- `docs/reference/kojo-reference.md` — Character dialogue reference
- `docs/reference/ntr-system-map.md` — NTR system (game-domain nuance)
- `pm/features/feature-*.md` — Feature files (mixed: Japanese game content descriptions acceptable)
- `pm/index-features.md` — Feature index (Japanese feature names acceptable)
- `.claude/skills/kojo-writing/` — Kojo skill (Japanese examples expected)
- `C:\Era\dashboard\HANDOFF.md` — Dashboard handoff (user-facing, mixed OK)

### 19.2 Detection Method

For each audit-target file:
1. Count lines containing Japanese characters (hiragana/katakana/CJK: `[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF]`)
2. Calculate Japanese line ratio: `japanese_lines / total_lines`
3. Classify:
   - Ratio ≤ 5%: `[OK]` (incidental Japanese, e.g., file paths, variable names)
   - 5% < Ratio ≤ 20%: `[INFO]` — Moderate Japanese, review recommended
   - Ratio > 20%: `[WARN]` — Significant Japanese content, should be English

### 19.3 Reporting

For each `[INFO]` or `[WARN]` file, report:
- File path
- Japanese line count / total line count
- Sample Japanese lines (up to 3) for context
- Severity: `[INFO]` or `[WARN]`

**Note**: Inline Japanese in code comments, variable names, or quoted game-domain terms (e.g., `口上`, `TALENT`) are acceptable and should not be flagged individually. The ratio-based approach handles this naturally.

---

## Output Format

```
╔══════════════════════════════════════════════════════════════╗
║              Documentation Audit Report                       ║
╚══════════════════════════════════════════════════════════════╝

═══ Part A: Reference Chain Audit ═══

[OK] Reachability: 45 documents reachable from CLAUDE.md
[ORPHAN] .claude/agents/deprecated-agent.md - not referenced
[BROKEN] CLAUDE.md:42 → reference/deleted-file.md
[CYCLE] feature-001.md ↔ feature-002.md (mutual reference)

═══ Part B: Category Exhaustive Audit ═══

--- B1-B4: Core Consistency ---
[OK] Feature Status: All 67 features consistent
[OK] Task/AC Completion: All tasks [○], all ACs [x]
[OK] Interface Docs: All 23 interfaces documented
[OK] Links: All 100 links valid

--- B5-B6: Skills & Testing ---
[OK] Skills Structure: All 16 SKILL.md files valid
[OK] Phase Files: run-workflow 10/10, fl-workflow 9/9
[OK] Testing Docs: All test types documented

--- B7: Tools ---
[OK] Tool Inventory: 8 tool directories, all have README.md
[WARN] src/tools/dotnet/ErbToYaml/ - no README.md
[ORPHAN] src/tools/node/session-extractor/ - not listed in CLAUDE.md Project Structure

--- B8-B8b: Agents, Commands, Hooks ---
[OK] Subagents: 26 agents, all registered in CLAUDE.md
[MISMATCH] philosophy-deriver (CLAUDE.md) → philosophy-definer.md (disk)
[OK] Skill/Agent Boundary: 8 skill-based agents confirmed
[OK] Commands: 9 commands, all registered in CLAUDE.md
[WARN] Command description mismatch: /audit
[OK] Hooks: pre-commit exists and executable

--- B9: CSV/Kojo ---
[OK] CSV/Kojo: Talent numbers consistent

═══ Document Bloat Check ═══

| Document | Lines | Threshold | Status |
|----------|------:|----------:|--------|
| CLAUDE.md | 195 | 250 | OK |
| index-features.md | 144 | 200 | OK |
| reference/engine-reference.md | 552 | 800 | OK |

[OK] Bloat Risk: All documents within thresholds
[OK] Context Load: Worst-case 2,100 lines (< 3,000)

═══ B10-B14: New Checks ═══

--- B10: Designs Status ---
[OK] Designs: 3 APPROVED, 5 DRAFT
[STALE] brothel-mob-system.md: APPROVED 45 days, no Feature

--- B11: KOJO_KX.ERB Template ---
[OK] Template guide section present
[OK] Function naming: 5/5 checked compliant

--- B12: Test Coverage ---
[INFO] Tests: 88 files (+10 since last audit)
[WARN] Feature 214 has no AC test directory

--- B13: COM-map Status ---
[OK] ★★★ COMs: 15/18 have kojo functions
[WARN] COM_81 (Fellatio) ★★★ missing kojo

--- B14: Reference Consolidation ---
[INFO] Small files (<100 lines): erb-reference.md (99), ntr-system-map.md (69)
[INFO] Merge candidate: sessions-reference.md (66) → hooks-reference.md

--- B16: Key Documents ---
[OK] Key Documents: All 12 paths exist
[BROKEN] _out/tmp/phase20-34-review-report.md - file not found

--- B17: Settings & Configuration ---
[OK] settings.json: 3 hooks, all references valid
[OK] Permissions: No contradictions

--- B18: Temporary Files ---
[INFO] _out/tmp/: 12 files, 2.3MB total
[INFO] 4 files older than 30 days (cleanup candidates)

--- B19: Language Consistency ---
[OK] .claude/agents/: 26 files, all English (avg JP ratio 2%)
[OK] .claude/commands/: 9 files, all English
[OK] .claude/skills/: 14 SKILL.md files checked
[WARN] .claude/skills/testing/ENGINE.md: 45% Japanese (28/62 lines)
[INFO] docs/architecture/migration/full-csharp-architecture.md: 12% Japanese (15/125 lines)

═══════════════════════════════════════════════════════════════
Summary: 1 orphan, 1 mismatch, 1 broken, 3 warnings, 5 suggestions
═══════════════════════════════════════════════════════════════
```

Run all checks and report findings.
