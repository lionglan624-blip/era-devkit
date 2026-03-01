# Claude Code Architecture - Current State

> **Created:** 2025-12-20
> **Purpose:** Complete inventory and analysis of Claude Code configuration for Feature 149 refactoring

---

## Executive Summary

| Category | Files | Lines | Est. Tokens |
|----------|------:|------:|------------:|
| Settings | 2 | 67 | 268 |
| Commands | 8 | 1,722 | 6,888 |
| Subagents | 13 | 967 | 3,868 |
| Hooks | 1 | 102 | 408 |
| CLAUDE.md | 1 | 159 | 636 |
| **Total .claude/** | **25** | **3,017** | **12,068** |
| reference/ | 10 | 933 | 3,732 |
| **Grand Total** | **35** | **3,950** | **15,800** |

**Key Finding:** Commands directory contains 57% of total .claude/ token usage, indicating potential for refactoring.

---

## 1. Complete File Inventory

### 1.1 Settings (2 files, 67 lines, 268 tokens)

| File | Lines | Tokens | Purpose |
|------|------:|-------:|---------|
| `.claude/settings.json` | 42 | 168 | Production config (permissions, hooks) |
| `.claude/settings.local.json` | 25 | 100 | Local overrides (slash commands) |

**Key Dependencies:**
- `settings.json` → `hooks/post-erb-write.ps1` (line 24)
- `settings.local.json` → All slash commands via `SlashCommand(/*)`

---

### 1.2 Commands (8 files, 1,722 lines, 6,888 tokens)

| File | Lines | Tokens | Purpose | Dependencies |
|------|------:|-------:|---------|--------------|
| `imple.md` | 276 | 1,104 | Main implementation workflow | All subagents, all references |
| `next.md` | 289 | 1,156 | Feature proposal workflow | feasibility-checker, ac-task-aligner, ac-validator |
| `roadmap.md` | 352 | 1,408 | Mid-long term planning | index-features, content-roadmap, designs/ |
| `kojo-init.md` | 209 | 836 | Batch kojo feature creation | index-features, kojo-reference |
| `doc-audit.md` | 189 | 756 | Documentation consistency checks | All .md files |
| `queue.md` | 181 | 724 | Execution order planning | index-features, feature-*.md |
| `complete-feature.md` | 183 | 732 | Feature completion verification | ac-tester, doc-reviewer, regression-tester |
| `commit.md` | 121 | 484 | Logical commit grouping | Git status |

**Critical Observations:**
- **roadmap.md** (352 lines) is largest, handles 6 different workflows
- **next.md** (289 lines) orchestrates 3 subagents sequentially
- **imple.md** (276 lines) is central to all implementation work

---

### 1.3 Subagents (13 files, 967 lines, 3,868 tokens)

| File | Lines | Tokens | Model | Purpose | Dependencies |
|------|------:|-------:|:-----:|---------|--------------|
| `kojo-writer.md` | 76 | 304 | opus | Dialogue creation | kojo-reference, kojo-canon-lines, cache/eratw-* |
| `implementer.md` | 56 | 224 | sonnet | ERB/Engine code | erb-reference OR engine-reference |
| `ac-validator.md` | 49 | 196 | sonnet | AC TDD validation + fix | feature-template, ERB investigation |
| `doc-reviewer.md` | 49 | 196 | sonnet | Doc quality review | Modified docs, peer docs |
| `debugger.md` | 48 | 192 | sonnet→opus | Error diagnosis + fix | erb/engine/testing-reference |
| `ac-tester.md` | 85 | 340 | haiku | AC verification | feature-*.md AC tables, test scenarios |
| `smoke-tester.md` | 70 | 280 | haiku | Smoke test execution | feature-*.md |
| `regression-tester.md` | 61 | 244 | haiku | Full regression suite | dotnet build/test, test scenarios |
| `finalizer.md` | 52 | 208 | haiku | Status update + commit prep | feature-*.md, index-features |
| `initializer.md` | 42 | 168 | haiku | Feature state initialization | feature-*.md, index-features |
| `eratw-reader.md` | 48 | 192 | haiku | eraTW reference extraction | External eraTW path |
| `ac-task-aligner.md` | 46 | 184 | haiku | AC:Task 1:1 alignment | feature-template |
| `feasibility-checker.md` | 40 | 160 | sonnet | Task feasibility validation | kojo/erb/engine-reference |

**Model Distribution:**
- **opus**: 1 agent (kojo-writer only)
- **sonnet**: 4 agents (implementer, ac-validator, doc-reviewer, debugger)
- **haiku**: 8 agents (all test/validation agents)

**Size Analysis:**
- Average: 74 lines/agent
- Largest: ac-tester (85 lines) - most complex test logic
- Smallest: feasibility-checker (40 lines) - focused investigation

---

### 1.4 Hooks (1 file, 102 lines, 408 tokens)

| File | Lines | Tokens | Purpose | Execution Time |
|------|------:|-------:|---------|----------------|
| `hooks/post-erb-write.ps1` | 102 | 408 | ERB post-processing (BOM/Build/Strict/Smoke) | ~8s per ERB file |

**Checks Performed:**
1. BOM check + auto-add (~0s)
2. Build verification (~1s)
3. Strict warnings (~4s)
4. Smoke test for kojo (~2s)

**Trigger:** Any Write/Edit tool use on .ERB or .ERH files (via settings.json line 20)

---

### 1.5 CLAUDE.md (1 file, 159 lines, 636 tokens)

| Section | Lines | Purpose |
|---------|------:|---------|
| Project Overview | 33 | Quick start, structure |
| Subagent Strategy | 62 | Dispatch pattern, model escalation, discipline |
| Slash Commands | 4 | Command list |
| Key Documents | 3 | Document map |
| Feature Types | 7 | Type routing |
| AC Definition Format | 10 | Matcher-based verification |
| Language | 4 | Language rules |
| Commit Convention | 6 | Commit format |

**Key Dependencies:**
- References all 13 subagents (lines 48-60)
- Links to `index-features.md`, `feature-{ID}.md`, `reference/*.md`

---

## 2. pm/reference/ Files (10 files, 933 lines, 3,732 tokens)

| File | Lines | Tokens | Purpose | Referenced By |
|------|------:|-------:|---------|---------------|
| `testing-reference.md` | 286 | 1,144 | Test types, commands, formats | debugger, ac-tester, smoke-tester, regression-tester |
| `kojo-reference.md` | 135 | 540 | Kojo structure, naming, speech patterns | kojo-writer, ac-validator, feasibility-checker |
| `engine-reference.md` | 121 | 484 | C# interfaces, extension points | implementer, feasibility-checker, debugger |
| `erb-reference.md` | 100 | 400 | ERB syntax, commands, flow control | implementer, feasibility-checker, debugger |
| `feature-template.md` | 79 | 316 | Feature spec template, AC format | next, ac-task-aligner, ac-validator |
| `kojo-canon-lines.md` | 105 | 420 | Character speech patterns | kojo-writer |
| `kojo-phases.md` | 88 | 352 | Phase 8 expansion plans | roadmap, kojo-init |
| `ntr-system-map.md` | 70 | 280 | NTR implementation reference | implementer (NTR features) |
| `hooks-reference.md` | 104 | 416 | Hooks syntax and examples | Settings configuration |
| `sessions-reference.md` | 67 | 268 | Sessions usage, resume patterns | Task tool usage |

**Usage Frequency (by agent references):**
1. **testing-reference.md** - 4 agents (all test agents)
2. **kojo-reference.md** - 3 agents (kojo-writer, ac-validator, feasibility-checker)
3. **engine-reference.md** - 3 agents (implementer, feasibility-checker, debugger)
4. **erb-reference.md** - 3 agents (implementer, feasibility-checker, debugger)
5. **feature-template.md** - 3 agents (next, ac-task-aligner, ac-validator)

---

## 3. Dependency Graph

### 3.1 Top-Level Dependencies

```
CLAUDE.md (loaded on every session)
    ├─→ settings.json (permissions + hooks)
    ├─→ settings.local.json (slash commands)
    └─→ All subagents (dispatch pattern reference)

settings.json
    └─→ hooks/post-erb-write.ps1 (PostToolUse event)
```

### 3.2 Command Dependencies

```
/next (next.md)
    ├─→ index-features.md (check existing [PROPOSED])
    ├─→ feasibility-checker.md ──→ kojo/erb/engine-reference.md
    ├─→ ac-task-aligner.md ──→ feature-template.md
    └─→ ac-validator.md ──→ feature-template.md, ERB files

/imple (imple.md)
    ├─→ initializer.md ──→ feature-*.md, index-features.md
    ├─→ Explore (built-in) ──→ Codebase
    ├─→ eratw-reader.md ──→ External eraTW path
    ├─→ kojo-writer.md ──→ kojo-reference.md, kojo-canon-lines.md, cache/eratw-*
    ├─→ implementer.md ──→ erb-reference.md OR engine-reference.md
    ├─→ smoke-tester.md ──→ testing-reference.md
    ├─→ regression-tester.md ──→ testing-reference.md
    ├─→ debugger.md ──→ erb/engine/testing-reference.md
    ├─→ ac-tester.md ──→ testing-reference.md, feature-*.md
    └─→ finalizer.md ──→ feature-*.md, index-features.md

/complete-feature (complete-feature.md)
    ├─→ ac-tester.md (parallel × N ACs)
    ├─→ doc-reviewer.md (if docs modified)
    └─→ regression-tester.md

/roadmap (roadmap.md)
    ├─→ index-features.md
    ├─→ content-roadmap.md
    └─→ designs/README.md, designs/*.md

/kojo-init (kojo-init.md)
    ├─→ index-features.md
    └─→ kojo-reference.md (COM name lookup)

/queue (queue.md)
    ├─→ index-features.md
    └─→ feature-*.md (dependency analysis)

/commit (commit.md)
    └─→ Git status

/doc-audit (doc-audit.md)
    └─→ All .md files (consistency checks)
```

### 3.3 Reference File Dependencies

```
testing-reference.md
    ├─← ac-tester.md
    ├─← smoke-tester.md
    ├─← regression-tester.md
    └─← debugger.md

kojo-reference.md
    ├─← kojo-writer.md
    ├─← ac-validator.md
    └─← feasibility-checker.md

engine-reference.md
    ├─← implementer.md
    ├─← feasibility-checker.md
    └─← debugger.md

erb-reference.md
    ├─← implementer.md
    ├─← feasibility-checker.md
    └─← debugger.md

feature-template.md
    ├─← next.md
    ├─← ac-task-aligner.md
    └─← ac-validator.md
```

### 3.4 Circular References

**None detected.** All dependencies are acyclic:
- Commands → Subagents (one-way)
- Subagents → References (one-way)
- References → No outbound dependencies (terminal nodes)

---

## 4. Token Budget Analysis

### 4.1 Typical Session Load

**Minimum Context (every session):**
```
CLAUDE.md                    636 tokens
settings.json                168 tokens
settings.local.json          100 tokens
────────────────────────────────────────
Baseline:                    904 tokens
```

**Feature Creation (/next):**
```
Baseline                     904 tokens
next.md                    1,156 tokens
feasibility-checker.md       160 tokens
ac-task-aligner.md           184 tokens
ac-validator.md              196 tokens
feature-template.md          316 tokens
kojo/erb/engine-ref (avg)    476 tokens
────────────────────────────────────────
Typical /next:             3,392 tokens
```

**Feature Implementation (/imple for kojo):**
```
Baseline                     904 tokens
imple.md                   1,104 tokens
kojo-writer.md               304 tokens
kojo-reference.md            540 tokens
kojo-canon-lines.md          420 tokens
testing-reference.md       1,144 tokens
feature-*.md (avg)           400 tokens
────────────────────────────────────────
Typical /imple kojo:       4,816 tokens
```

**Feature Implementation (/imple for engine):**
```
Baseline                     904 tokens
imple.md                   1,104 tokens
implementer.md               224 tokens
engine-reference.md          484 tokens
testing-reference.md       1,144 tokens
feature-*.md (avg)           400 tokens
────────────────────────────────────────
Typical /imple engine:     4,260 tokens
```

### 4.2 Worst-Case Scenario

**All commands + all subagents + all references:**
```
Settings                     268 tokens
Commands (8 files)         6,888 tokens
Subagents (13 files)       3,868 tokens
CLAUDE.md                    636 tokens
References (10 files)      3,732 tokens
────────────────────────────────────────
Worst-case total:         15,392 tokens
```

**Status:** Well within Claude's context window (~200K tokens). No immediate bloat risk.

---

## 5. Redundancy Analysis

### 5.1 AC Format Definitions

**Duplicated in:**
- CLAUDE.md (lines 126-137) - 12 lines
- testing-reference.md (lines 20-30) - 11 lines
- feature-template.md (lines 57-63) - 7 lines

**Recommendation:** Keep in all 3 for different audiences:
- CLAUDE.md: Quick reference for Opus
- testing-reference.md: Test agent reference
- feature-template.md: Template usage

### 5.2 Test Commands

**Duplicated in:**
- testing-reference.md (comprehensive)
- smoke-tester.md (subset)
- ac-tester.md (subset)
- regression-tester.md (subset)

**Recommendation:** Test agents should reference testing-reference.md instead of duplicating.

### 5.3 Dispatch Pattern

**Duplicated in:**
- CLAUDE.md (lines 63-65)
- imple.md (lines 25-44)
- next.md (partial)

**Recommendation:** Keep in CLAUDE.md only, commands reference it.

---

## 6. Complexity Hotspots

### 6.1 Multi-Workflow Commands

| Command | Workflows | Lines | Complexity |
|---------|:---------:|------:|:----------:|
| roadmap.md | 6 | 352 | HIGH |
| next.md | 3 | 289 | MEDIUM |
| imple.md | 8 phases | 276 | HIGH |

**Recommendation:** Consider splitting roadmap.md into 6 separate commands or using subagents.

### 6.2 Dense Subagents

| Agent | Lines | Model | Complexity |
|-------|------:|:-----:|:----------:|
| ac-tester.md | 85 | haiku | MEDIUM |
| kojo-writer.md | 76 | opus | MEDIUM |

**Recommendation:** Both are domain-specific and appropriately sized. No action needed.

---

## 7. Maintenance Burden

### 7.1 High-Touch Files

**Updated frequently (>5 features):**
- `index-features.md` - Every feature lifecycle change
- `feature-*.md` - Per-feature updates
- `content-roadmap.md` - Roadmap changes

**Rarely updated (<1 per month):**
- All reference/*.md files
- All subagent definitions
- CLAUDE.md

### 7.2 Synchronization Requirements

**Manual sync needed:**
- CLAUDE.md subagent list ↔ .claude/agents/ directory
- settings.local.json SlashCommand list ↔ .claude/commands/ directory
- roadmap.md workflow options ↔ designs/README.md statuses

**Recommendation:** Add /doc-audit checks for these (already partially implemented in doc-audit.md).

---

## 8. Performance Characteristics

### 8.1 Hooks Performance

**post-erb-write.ps1:** ~8s per ERB file
- BOM check: instant
- Build: ~1s
- Strict warnings: ~4s
- Smoke test: ~2s

**Impact:** Acceptable for single-file edits. Consider disabling for batch operations.

### 8.2 Subagent Dispatch Overhead

**Sequential chain (next.md):**
```
feasibility-checker → ac-task-aligner → ac-validator
(~30s total for 5 tasks)
```

**Parallel execution (imple.md kojo):**
```
5× kojo-writer in parallel
(~2min total vs ~10min sequential)
```

**Recommendation:** Current parallel strategy is effective. No changes needed.

---

## 9. Coverage Gaps

### 9.1 Missing Subagents

**Potential candidates:**
- **unit-tester.md** - Referenced in CLAUDE.md but doesn't exist
- **explorer.md** - Uses built-in Explore, could benefit from custom wrapper

### 9.2 Missing Commands

**Potential candidates:**
- `/test [ID]` - Run regression tests for specific feature
- `/validate [ID]` - Run all validation (feasibility + AC alignment + AC TDD) without creating feature

### 9.3 Missing References

**Potential candidates:**
- **csv-reference.md** - CSV file format and rules
- **parallel-execution-reference.md** - Parallel dispatch patterns

---

## 10. Security & Safety

### 10.1 Protected Files

**settings.json permissions:**
- Allow: Bash, Read, Write, Edit, Glob, Grep, Task, WebSearch, WebFetch
- Deny: (none)
- Ask: (none)

**Hook-based protection:**
- CSV files: Should be protected via PreToolUse hook (not currently implemented)
- .git directory: Should be protected (not currently implemented)

**Recommendation:** Add PreToolUse hook to block .CSV, .SAV, .git edits.

### 10.2 External Dependencies

**External file paths:**
- `eratw-reader.md` line 25: Hardcoded Windows path to eraTW installation
- Risk: Breaks on different machines

**Recommendation:** Move to config file or environment variable.

---

## 11. Refactoring Priorities (for Feature 149)

### 11.1 High Priority

1. **Consolidate roadmap.md workflows** (352 lines → 6 separate commands or subagents)
2. **Extract dispatch pattern** from commands to shared reference
3. **Add missing protection hooks** for CSV/SAV/git files

### 11.2 Medium Priority

4. **Reduce testing-reference.md duplication** in test subagents
5. **Create unit-tester.md** to replace references to non-existent agent
6. **Externalize eraTW path** from eratw-reader.md

### 11.3 Low Priority

7. **Add /validate command** for pre-approval validation
8. **Create csv-reference.md** for CSV editing guidelines
9. **Add doc-audit checks** for CLAUDE.md ↔ directory sync

---

## 12. Architecture Strengths

### 12.1 Well-Structured

✅ **Clear separation of concerns:**
- Commands orchestrate workflows
- Subagents execute single tasks
- References provide domain knowledge

✅ **Acyclic dependencies:**
- No circular references detected
- Clean dependency graph

✅ **Model selection:**
- Appropriate model choices (opus for creative, sonnet for implementation, haiku for validation)

### 12.2 Scalability

✅ **Token budget:**
- Current load: 3,392 - 4,816 tokens per typical session
- Worst-case: 15,392 tokens (< 10% of context window)
- Room for growth

✅ **Parallel execution:**
- Effective use of run_in_background
- Status file polling for kojo-writer batch

### 12.3 Maintainability

✅ **Documentation:**
- All files have clear purpose statements
- Reference files are terminal nodes (no outbound dependencies)

✅ **Consistency:**
- Standard frontmatter in all subagent files
- Consistent table formats across commands

---

## 13. Architecture Weaknesses

### 13.1 Complexity Concentration

❌ **Large multi-workflow files:**
- roadmap.md (6 workflows, 352 lines)
- imple.md (8 phases, 276 lines)

❌ **Command duplication:**
- Dispatch pattern repeated in multiple commands
- AC format duplicated in 3 files (justified but verbose)

### 13.2 Fragility

❌ **Hardcoded paths:**
- eraTW path in eratw-reader.md
- Hook script path in settings.json

❌ **Manual synchronization:**
- CLAUDE.md subagent list vs actual files
- settings.local.json commands vs actual files

### 13.3 Missing Safeguards

❌ **No protection for:**
- CSV files (should be read-only)
- SAV files (should be read-only)
- .git directory (should be protected)

---

## Links

- [Feature 149 Specification](../feature-149.md)
- [index-features.md](../index-features.md)
- [CLAUDE.md](../../../CLAUDE.md)
