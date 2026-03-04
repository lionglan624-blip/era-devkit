# CLAUDE.md

Guidance for Claude Code when working with this repository.

**CRITICAL: MUST follow documented workflow exactly. NEVER make unrequested changes. Ask when uncertain.**

---

## Project Overview

**devkit** - Development tools and project management repository for era紅魔館protoNTR.

Part of a 5-repo split: devkit contains CLI tools, PM files, docs, AC definitions, and test infrastructure. The game runtime data, C# library, engine, and dashboard each live in separate repos.

## Quick Start

```bash
# Build all devkit tools
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build devkit.sln'

# Run all tool tests (requires GAME_PATH for integration tests)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && GAME_PATH=/mnt/c/Era/game /home/siihe/.dotnet/dotnet test devkit.sln --blame-hang-timeout 10s'

# Run headless game (cross-repo)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/game && /home/siihe/.dotnet/dotnet run --project /mnt/c/Era/engine/uEmuera.Headless.csproj -- .'
```

## Git Hooks Setup

```bash
git config core.hooksPath .githooks
```

## 5-Repo Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `GAME_PATH` | game repo path | `C:\Era\game` |
| `CORE_PATH` | core repo path | `C:\Era\core` |
| `ENGINE_PATH` | engine repo path | `C:\Era\engine` |
| `DEVKIT_ROOT` | devkit repo path (this repo) | `C:\Era\devkit` |
| `DASHBOARD_PATH` | dashboard repo path | `C:\Era\dashboard` |

## Project Structure

```
src/
  .serena/            # Serena LSP project config
  scripts/            # Dev convenience scripts (fd-*.cmd)
  tools/
    dotnet/           # C# tools (ErbParser, KojoComparer, etc. + .Tests each)
    python/           # Python tools (lsp.py, feature-status.py, ac_ops.py, etc.)
    node/             # Node.js (session-extractor, lsp-daemon-launcher)
    go/               # Go (com-validator)
    kojo-mapper/      # Kojo mapper (standalone Python)
    schemas/          # JSON Schemas for validation
test/
  _archived/          # Archived regression scenarios
  ac/                 # Acceptance criteria
  configs/            # flow-test-detection.json etc.
  fixtures/           # yaml/, json/
  scripts/            # Test runner scripts
docs/
  architecture/       # systems/, migration/, infrastructure/, analysis/
  game/               # Game docs (architecture/, data-formats/, modding/)
  reference/          # Domain references (ERB, kojo, NTR, engine)
  tools/              # Tool docs (lsp-daemon.md)
  strategy/           # Test strategy, naming, design
pm/
  features/           # feature-{ID}.md (275+ files)
  reference/          # Workflow references (feature-template, ac-matcher, designs)
  audit/              # Audit reports
  templates/          # Content creation templates (ERB kojo originals)
  provenance/         # Original source attribution
  cache/              # (gitignored) eraTW COM extraction cache
  status/             # (gitignored) Kojo progress tracking
  .fl-pending/        # (gitignored) FL review intermediate state
  index-features.md
  content-roadmap.md
_out/                 # Generated output (gitignored)
  logs/               # ALL logs (CI, debug)
  test-results/       # All test output
  stryker/            # Mutation testing output
  tmp/                # Temporary files
.claude/              # Claude Code (agents, commands, skills, reference)
.githooks/            # Git hooks
```

`Directory.Build.props`: TreatWarningsAsErrors=true (F708). Opt-out: `_out/`, `src/tools/dotnet/_archived/`

## Subagent Strategy

**Opus handles decisions only. Delegate ALL implementation to subagents.**

**Registry**: `.claude/reference/agent-registry.md` (full agent table, model info, dispatch rules)

**Single Responsibility**: Creation agents create only. Test agents test only. Debug agent fixes only.

| Check | Action |
|-------|--------|
| Making a DECISION? | OK |
| READING to understand? | OK |
| WRITING/EDITING code? | **DISPATCH subagent** |
| Running TESTS? | **DISPATCH tester** |

## Slash Commands

| Command | Description |
|---------|-------------|
| `/run [ID]` | Execute feature with Progressive Disclosure workflow |
| `/commit` | Commit changes with logical grouping |
| `/complete-feature` | Verify and complete a feature |
| `/fc` | Feature Completion - generate AC/Tasks for [DRAFT] feature |
| `/kojo-init` | Initialize up to 5 kojo feature.md files for batch workflow |
| `/fl` | Feature review-fix loop until zero issues |
| `/audit` | Audit documentation consistency |
| `/sync-deps` | Sync dependency statuses in feature files |
| `/test-audit` | Audit test coverage and mutation scores |
| `/imp [ID]` | Improvement analysis for feature lifecycle sessions |

## Feature Implementation Workflow

**MANDATORY**: Before creating ANY feature, read `Skill(feature-quality)` and the type-specific guide.

```
[DRAFT] -> /fc -> [PROPOSED] -> /fl -> [REVIEWED] -> /run -> [WIP] -> [DONE]
```

| Step | Command | Status Change | Description |
|:----:|---------|---------------|-------------|
| 1 | `/fc {ID}` | `[DRAFT]` -> `[PROPOSED]` | Complete feature (generate AC/Tasks) |
| 2 | `/fl {ID}` | -> `[REVIEWED]` or `[BLOCKED]` | Review-fix loop (gate) + Dependency Gate |
| 3 | `/run {ID}` | -> `[WIP]` -> `[DONE]` or `[BLOCKED]` | Execute implementation |

**Gate Rules**:
- `/run` accepts `[REVIEWED]`, `[WIP]`, or `[BLOCKED]` (resume). Rejects `[PROPOSED]`.
- `/fl` sets `[BLOCKED]` if predecessor is not `[DONE]`.
- `/run` sets `[BLOCKED]` if AC has `[B]` and user chooses to wait.

## Design Principles

**Thoroughness over speed. There is no time pressure.**

| Principle | Description |
|-----------|-------------|
| **SSOT** | Skills are the truth for details. Skills > CLAUDE.md > commands > agents |
| **TDD** | RED->GREEN, test-first (run-workflow Phase 3) |
| **STOP on Ambiguity** | STOP instead of guessing when unclear |
| **Separation of Concerns** | Opus=decisions, Subagent=implementation |
| **Fail Fast** | STOP after 3 failures |
| **Immutable Tests** | AC/Regression tests are read-only (pre-commit hook) |
| **AC Coverage** | ACs must comprehensively verify all Tasks. N ACs : 1 Task allowed |
| **Binary Judgment** | PASS/FAIL only, no ambiguous judgments |
| **Progressive Disclosure** | Load info progressively via skill YAML frontmatter |
| **Track What You Skip** | Defer is OK, forget is not. All issues must be tracked |
| **Zero Debt Upfront** | Pay costs now to eliminate future debt. Design for the future |
| **Root Cause Resolution** | Fix root cause, not symptoms |
| **Challenge Exclusions** | Exclusions require justification and concrete tracking |

## Skills

Domain knowledge loaded by Skill tool. **Skills are the SSOT for commands.**

Skills are **directories** with `SKILL.md` entry point:
- `Skill(fl-workflow)` -> `.claude/skills/fl-workflow/SKILL.md`
- Manual: `Read(.claude/skills/{name}/SKILL.md)` (NOT `{name}.md`)

**Warning**: Do not use `requires:` in SKILL.md entry points for phase-based skills.

| Skill | When to use |
|-------|-------------|
| `csharp-14` | C# 14 features, .NET 10, DDD patterns |
| `erb-syntax` | ERB scripting, RETURN rules, PRINT commands |
| `engine-dev` | C# engine, GlobalStatic, headless mode |
| `kojo-writing` | Character dialogue, TALENT branching |
| `testing` | Test scenarios, AC verification |
| `feature-quality` | **MANDATORY before feature creation** |
| `run-workflow` | /run phases (Progressive Disclosure) |
| `fl-workflow` | FL review-fix loop |

Other skills (context:fork): `initializer`, `finalizer`, `reference-checker`, `eratw-reader`, `philosophy-deriver`, `task-comparator`

## Feature Types

```markdown
Type: kojo    # -> uses kojo-writer (opus)
Type: erb     # -> uses implementer (sonnet)
Type: engine  # -> uses implementer (sonnet)
```

## AC Definition Format

**Strict matcher-based verification** -- see [feature-template.md](pm/reference/feature-template.md) for full format and matcher list.

**Judgment**: Binary yes/no only. No "Confidence" levels.

## External Dependencies

| Dependency | Path |
|-----------|------|
| eraTW Reference | `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920` (env: `ERATW_PATH`) |
| User Screenshots | `C:\Users\siihe\OneDrive\画像\Screenshots` |

## Key Documents

| Document | Purpose |
|----------|---------|
| `pm/index-features.md` | Active features, roadmap |
| `pm/features/feature-{ID}.md` | Feature spec + progress log |
| `pm/reference/feature-template.md` | Feature spec template |
| `pm/content-roadmap.md` | Content plan, COM series order |
| `docs/reference/ntr-system-map.md` | NTR system reference |
| `.claude/reference/ssot-update-rules.md` | SSOT update rules |
| `NOTICE.md` | License |
| `src/tools/node/feature-dashboard/HANDOFF.md` | Feature Dashboard spec (devkit内、PM2実行パス). **「HANDOFF.md読んで」と言われたら無条件で全文Read（セクション確認不要）** |
| `src/tools/python/session-search.py` | Session JSONL search — see "Session JSONL Investigation" section for task→flag table |
| `src/tools/python/feature-status.py` | Status/dependency sync (`--help`) |
| `src/tools/python/ac_ops.py` | AC operations (`python src/tools/python/ac_ops.py --help`) |

## Escalation Policy

**CRITICAL: When issues occur, DO NOT change procedures independently. Ask user for guidance.**

| Situation | Action |
|-----------|--------|
| Test framework bug / Concurrent execution issue | **STOP** -> Report to user |
| Doc vs actual behavior mismatch | **STOP** -> Report to user |
| 3 consecutive failures | **STOP** -> Report to user |

**NEVER**: Skip steps, ignore failures, execute undocumented workarounds.

**Deferred Task Protocol**: `.claude/reference/deferred-task-protocol.md` (destination selection, TBD prohibition)

## Test Coverage Policy

**SSOT**: `docs/strategy/test-strategy.md`

- New C# code MUST have unit tests (TDD RED->GREEN)
- Migration features MUST include equivalence tests
- Coverage regression requires justification
- Run (via WSL): `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && GAME_PATH=/mnt/c/Era/game /home/siihe/.dotnet/dotnet test devkit.sln /p:CollectCoverage=true /p:CoverageOutputFormat=cobertura'`
- **Hang prevention**: `Skill(testing)` "Hang Detection" section. Always add `--blame-hang-timeout` to `dotnet test`

## Email Notification

See `.claude/reference/email-notification.md` (user-requested task completion emails)

## Commit Convention

`feat:` / `fix:` / `docs:` / `refactor:` / `test:` -- **Before commit**: `dotnet build` + `dotnet test` (via WSL pre-commit hook)

## File Placement

| Target | Location | Lifecycle |
|--------|----------|-----------|
| Temporary files | `_out/tmp/` | 7-day rotation (pre-commit) |
| Test results | `_out/test-results/` (use `--results-directory`) | 7-day rotation (pre-commit) |
| Debug logs | `_out/logs/debug/` | manual |
| CI logs | `_out/logs/ci/` | 30-day rotation (pre-commit) |
| AC logs | `_out/logs/prod/ac/` | per-feature (finalizer on [DONE]) |
| Kojo progress | `pm/status/` (gitignored) | finalizer on [DONE] + 30-day rotation |
| FL intermediates | `pm/.fl-pending/` (gitignored) | finalizer on [DONE] + 14-day rotation |
| eraTW cache | `pm/cache/` (gitignored) | 90-day rotation (pre-commit) |

## Reference Distinction

| Location | Content | Examples |
|----------|---------|----------|
| `docs/reference/` | **Domain knowledge** — game systems, ERB syntax, engine behavior | ntr-system-map, erb-reference, kojo-reference |
| `pm/reference/` | **Workflow artifacts** — templates, mappings, design docs for features | feature-template, ac-matcher-mapping, design-template |

Rule: "Would this exist without the PM system?" → Yes: `docs/reference/`. No: `pm/reference/`.

## LSP Daemon (C# Semantic Operations)

Zero-token alternative to Serena MCP. Persistent HTTP daemon wrapping Serena's Python API.

**C# code exploration: use lsp.py first.** Higher precision, lower tokens than Read/Grep.

| Task | Tool | Example |
|------|------|---------|
| File structure | `lsp.py symbols` | `python src/tools/python/lsp.py symbols src/tools/dotnet/ErbParser/Foo.cs --depth 1` |
| Symbol search | `lsp.py find` | `python src/tools/python/lsp.py find ClassName --path src/tools/dotnet/` |
| Method body | `lsp.py find --body` | `python src/tools/python/lsp.py find Class/Method --path File.cs --body` |
| Find references | `lsp.py refs` | `python src/tools/python/lsp.py refs Method --path File.cs` |
| Rename | `lsp.py rename` | `python src/tools/python/lsp.py rename Old New --path File.cs` |
| Non-C# files | Read / Grep | ERB, CSV, YAML, comments |

- **PM2**: `pm2 start ecosystem.config.js` / `pm2 stop lsp-daemon` / `dr` (restart all)
- **Commands**: `status`, `symbols`, `find`, `refs`, `rename`, `replace`, `insert-before`, `insert-after`, `restart`
- **Details**: `docs/tools/lsp-daemon.md` (error handling, PM2 management)

## Session JSONL Investigation

**Session JSONL investigation: use session-search.py first.** Never write manual python one-liners for JSONL parsing.

Run from project root: `cd /c/Era/devkit && python src/tools/python/session-search.py`

| Task | Flags | Example |
|------|-------|---------|
| List recent sessions | `--list` | `python src/tools/python/session-search.py --list --after 2026-03-03` |
| Find tool usage | `--tool Name` | `python src/tools/python/session-search.py --tool AskUserQuestion --session 37ef` |
| Extract raw JSON | `--line N --raw` | `python src/tools/python/session-search.py --session 37ef --line 158 --raw` |
| Session timeline | `--timeline` | `python src/tools/python/session-search.py --session 37ef --timeline` |
| Text search | `"pattern"` | `python src/tools/python/session-search.py "handoff" --after 2026-03-01` |
| Text-only search | `--type text` | `python src/tools/python/session-search.py --session 37ef "issue" --type text` |
| Session overview | `--summary` | `python src/tools/python/session-search.py --session 37ef --summary` |
| Context around match | `-C N` | `python src/tools/python/session-search.py "error" --session 37ef -C 3` |

## Shell Environment

Claude Code Bash = **Git Bash** (cwd: project root). User terminal = **cmd/PowerShell** (cwd: `C:\Users\siihe`).

**Command format**: Full path + `&&` chaining on **one line** for user commands.

**CRLF**: Both root and engine/ `.gitattributes` set `* text=auto eol=lf`. Matches Write tool LF output.

### CLI Shortcuts (`~/.local/bin/`)

| Command | Description |
|---------|-------------|
| `dk` | Dashboard kill (pm2 stop + orphan cleanup) |
| `dr` | Dashboard restart (pm2 restart all) |
| `ds` | Dashboard start (pm2 start) |
| `cl` | Claude direct (proxy bypass, fallback用) |

## WSL (dotnet execution)

Smart App Control: all dotnet commands via WSL2 Ubuntu 24.04.

| Item | Value |
|------|-------|
| Distro | Ubuntu 24.04 LTS |
| .NET SDK | `/home/siihe/.dotnet/dotnet` |
| Repo mount | `/mnt/c/Era/devkit` |
| pre-commit | `.githooks/pre-commit` `wsl_dotnet()` function |

**Agent pattern** (Git Bash):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && GAME_PATH=/mnt/c/Era/game /home/siihe/.dotnet/dotnet test devkit.sln --blame-hang-timeout 10s'
```

**MSYS_NO_PATHCONV=1**: Disable Git Bash MSYS path conversion (required).

## Terminology

- **Phase N** (user context): Refers to `docs/architecture/migration/full-csharp-architecture.md` migration Phases, not FL workflow Phases.

## Language

In-game text: Japanese (ERB dialogue, game content)

## Architecture

### Layer Structure

```
[EXTERNAL REPOS]
game (C:\Era\game)        ERB scripts, YAML dialogue, CSV, config
core (C:\Era\core)        Era.Core C# runtime library (NuGet: Era.Core 1.0.0)
engine (C:\Era\engine)    uEmuera game engine (Unity 6000.3.1f1)

[THIS REPO: devkit]
src/tools/                10+ C# CLI tools with paired test projects
  +-- All tools reference Era.Core via NuGet PackageReference
pm/                       Feature management (275+ features)
docs/                     Architecture docs, game docs, tool docs
test/                     AC definitions, test fixtures
```

### COM YAML System (core architecture)

The dialogue system loads character speech (COM) from YAML files through a cached pipeline:

- **YamlComLoader / CustomComLoader** -> **ComDefinitionCache** (ConcurrentDictionary with file-modification-time invalidation) -> **ComDefinition** -> **Era.Core rendering**
- **ComHotReload**: FileSystemWatcher with 100ms debounce for live YAML editing
- **IComLoader** interface abstracts loading; uses ILogger DI (not Console.WriteLine) for thread-safe logging in tests
- **Era.Core** is consumed as a NuGet package (`Era.Core 1.0.0`) -- source lives in the `core` repo

## Test Commands

```bash
# Build all tools
dotnet build devkit.sln

# Run all tool tests (requires GAME_PATH for integration tests)
GAME_PATH=/mnt/c/Era/game dotnet test devkit.sln --blame-hang-timeout 10s --results-directory _out/test-results

# Run a specific tool's tests
dotnet test src/tools/dotnet/KojoComparer.Tests/ --blame-hang-timeout 10s --results-directory _out/test-results

# Run tests by class or method name
dotnet test src/tools/dotnet/ErbParser.Tests/ --blame-hang-timeout 10s --results-directory _out/test-results --filter "FullyQualifiedName~ErbParserTests"

# Run tests by category trait (Unit, Integration, Schema)
dotnet test src/tools/dotnet/KojoComparer.Tests/ --blame-hang-timeout 10s --results-directory _out/test-results --filter "Category=Unit"

# Code coverage
GAME_PATH=/mnt/c/Era/game dotnet test devkit.sln --blame-hang-timeout 10s --results-directory _out/test-results --collect:"XPlat Code Coverage"
```

Note: All `dotnet` commands must be run via WSL (see WSL section above). `--results-directory _out/test-results` prevents TestResults/ from accumulating in source dirs.

## Code Conventions

### C# (.NET 10 / C# 14)
- File-scoped namespaces, System usings first
- No unnecessary `this.` qualification
- `var` only when type is apparent; explicit types otherwise
- Test naming: `{Method}_{Scenario}_{ExpectedResult}`
- Test traits: `[Trait("Category", "Unit|Integration|Schema")]`
- Arrange-Act-Assert pattern in tests

### ERB Scripts
- COM files follow `COMF{N}.ERB` naming (N = character/situation ID)
- Kojo functions: `@KOJO_MESSAGE_COM_K{N}_{comId}`
- NTR system modularized in `Game/ERB/NTR/` (not monolithic) -- lives in game repo

### Formatting
- 4 spaces indent (2 for YAML/JSON), LF line endings, UTF-8
- Allman braces for C# (new line before open brace)
- Full rules in `.editorconfig`

## Key Tools

| Tool | Purpose |
|------|---------|
| `ErbParser` | ERB syntax analysis |
| `ErbToYaml` | CSV->YAML migration |
| `KojoComparer` | Compare ERB vs YAML dialogue output (`--origin` for baseline) |
| `KojoQualityValidator` | Character dialogue quality checks |
| `YamlSchemaGen` | Generate JSON Schema for dialogue YAML |
| `YamlValidator` | Validate YAML against schema (CI-ready exit codes) |
| `SaveAnalyzer` | Game save file analysis |

## Related Repositories

| Repo | Location | Purpose |
|------|----------|---------|
| game | `C:\Era\game` | Game runtime data (ERB, YAML, CSV) |
| core | `C:\Era\core` | Era.Core C# runtime library |
| engine | `C:\Era\engine` | uEmuera game engine (Unity) |
| dashboard | `C:\Era\dashboard` | Feature dashboard (Node.js) |
