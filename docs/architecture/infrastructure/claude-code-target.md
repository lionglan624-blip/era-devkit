# Claude Code Target Architecture Design

**Feature**: 149
**Version**: 2.0 (REVISED)
**Date**: 2025-12-20
**Status**: Design Specification - **CORRECTED**

---

## ⚠️ REVISION NOTICE (2025-12-20)

**このドキュメントは修正版です。**

初期設計（v1.0）には以下の誤りがありました：
1. Skills に `paths` フィールドがあると想定 → **存在しない（Rulesのみ）**
2. ~~Subagent に `skills:` フィールドで統合 → 存在しない機能~~ → **再検証で存在確認（効果未検証）**
3. Skills が subagent に自動適用される → **`skills:` 明示指定が必要（効果は公式に不明確）**

**公式ドキュメントに基づく正しい理解**:
- Skills: `name`, `description`, `allowed-tools` のみ（paths なし）
- Rules: `paths` フィールドでpath-based loading
- 両方ともメインコンテキスト専用（subagent には適用されない）

**このプロジェクトへの影響**:
- 作業の90%はsubagent経由のため、Skills/Rulesの効果は**限定的**
- reference/*.md は subagent 用に**維持が必要**

---

## 1. Target Directory Structure

```
.claude/
├── settings.json              # Core permissions and config
├── settings.local.json        # Local overrides (gitignored)
│
├── commands/                  # User-triggered workflows
│   ├── imple.md              # /imple implementation workflow
│   ├── next.md               # /next feature proposal
│   ├── queue.md              # /queue execution planning
│   └── roadmap.md            # /roadmap feature planning
│
├── agents/                    # Subagent definitions
│   ├── initializer.md        # Feature state initialization
│   ├── explorer.md           # Investigation (built-in wrapper)
│   ├── ac-task-aligner.md    # AC:Task alignment verification
│   ├── feasibility-checker.md # Technical feasibility analysis
│   ├── ac-validator.md       # AC TDD validation + fix
│   ├── kojo-writer.md        # Dialogue creation (opus)
│   ├── implementer.md        # ERB/Engine implementation (sonnet)
│   ├── unit-tester.md        # Single test execution
│   ├── regression-tester.md  # Full test suite execution
│   ├── ac-tester.md          # AC verification (compact)
│   ├── debugger.md           # Error fix (built-in wrapper)
│   ├── finalizer.md          # Status update + commit prep
│   └── doc-reviewer.md       # Documentation quality review
│
├── hooks/                     # Deterministic automation
│   └── post-erb-write.ps1    # Auto-format ERB files after write
│
├── skills/                    # NEW: Progressive Disclosure
│   ├── erb-lang/             # ERB language skill
│   │   ├── SKILL.md          # Overview, key concepts (<500 lines)
│   │   ├── syntax.md         # Syntax reference (<500 lines)
│   │   └── functions.md      # Built-in functions (<500 lines)
│   │
│   ├── kojo-writing/         # Dialogue writing skill
│   │   ├── SKILL.md          # Overview, character voice principles
│   │   ├── patterns.md       # Common dialogue patterns
│   │   └── canon-lines.md    # Character-specific canonical examples
│   │
│   ├── testing/              # Testing skill
│   │   ├── SKILL.md          # Test types, philosophy
│   │   ├── commands.md       # dotnet test, strict-warnings usage
│   │   └── scenarios.md      # Test scenario authoring
│   │
│   ├── engine-dev/           # Engine development skill
│   │   ├── SKILL.md          # Architecture overview
│   │   ├── interfaces.md     # Key interfaces, extension points
│   │   └── commands.md       # Command system architecture
│   │
│   └── ntr-system/           # NTR game mechanics skill
│       ├── SKILL.md          # NTR system overview
│       ├── variables.md      # Variable definitions
│       └── phases.md         # Phase system mechanics
│
└── rules/                     # NEW: Path-based auto-loading
    ├── 00-always.md          # Core project conventions (always loaded)
    ├── erb-editing.md        # Loaded for ERB files (paths: ["**/*.erb", "**/*.erh"])
    ├── csharp-editing.md     # Loaded for C# files (paths: ["**/*.cs"])
    ├── testing.md            # Loaded for test work (paths: ["**/tests/**", "**/*.Tests/**"])
    └── kojo-editing.md       # Loaded for kojo files (paths: ["**/口上/**/*.erb"])
```

**Not in .claude/**: Project-wide docs remain in root
- `CLAUDE.md` - Always-loaded project overview (stays in root)
- `pm/index-features.md` - Feature tracking
- `pm/features/feature-*.md` - Feature specs
- `docs/architecture/` - Design documents

---

## 2. Skills Design (REVISED)

### ⚠️ 重要な修正（2025-12-20 再検証）

**初期設計の誤り**:
- ❌ `paths` フィールドで自動ロード → Skills には存在しない（Rulesの機能）
- ❌ 500行制限 → 公式ドキュメントに記載なし

**公式ドキュメントに基づく正しい理解**:

```yaml
# SKILL.md の正しいフォーマット（公式3フィールドのみ）
---
name: skill-name
description: 説明文（Claudeがこれを読んで自動検出）
allowed-tools: Read, Grep, Glob  # オプション：ツール制限
---
# paths: は存在しない（Rulesのみ）
```

**Subagent Frontmatter（公式6フィールド）**:
```yaml
---
name: agent-name                    # Required
description: When to use this       # Required
tools: Read, Grep, Glob            # Optional: omit to inherit all
model: sonnet | opus | haiku       # Optional: defaults to sonnet
permissionMode: default            # Optional
skills: skill1, skill2             # Optional: 存在するが効果は未検証
---
```

**Skills の特性**:
- **description ベース検出**: Claude が自動判断
- **paths なし**: path-based loading は Rules の機能
- **subagent への適用**: `skills:` フィールドは存在するが、効果は公式ドキュメントで不明確

### 2.1 Design Principles (Corrected)

**このプロジェクトでの Skills 活用**:
- Skills は Opus（オーケストレーター）が直接作業する場合にのみ有効
- subagent 経由の作業（90%以上）には効果なし
- **結論**: 導入効果は限定的、オプション扱い

### 2.2 Skill Catalog

#### **erb-lang** skill

**When loaded**: Agent works with ERB/ERH files, or subagent has `skills: ["erb-lang"]`

**Files**:
```
skills/erb-lang/
├── SKILL.md          # Overview, key concepts, "see syntax.md for details"
├── syntax.md         # Control flow, variable syntax, special forms
└── functions.md      # Built-in functions reference
```

**SKILL.md content** (~300 lines):
- ERB language overview
- Key concepts: PRINT*, DRAWLINE, variables (@, #, $), control flow basics
- Common patterns: IF/ENDIF, SELECTCASE, FOR loops
- References: "For full syntax reference, see syntax.md"
- References: "For built-in functions, see functions.md"

**Token budget**: 300 (SKILL.md) + 400 (syntax.md when needed) = 700 lines max

**Migration from**: `pm/reference/erb-reference.md` (currently 800 lines)

---

#### **kojo-writing** skill

**When loaded**: `kojo-writer` subagent, or working with `口上/` files

**Files**:
```
skills/kojo-writing/
├── SKILL.md          # Principles, character voice, branching
├── patterns.md       # Common dialogue patterns (confession, jealousy, etc)
└── canon-lines.md    # Character-specific canonical examples (Meiling, Sakuya, etc)
```

**SKILL.md content** (~350 lines):
- Character voice principles (keigo, personality consistency)
- Kojo file structure (conditional blocks, affection triggers)
- Branching patterns (IF @ABL:FLAG:X / ELSEIF / ENDIF)
- Quality criteria (canon adherence, emotional depth)
- References: "See patterns.md for common scenarios"
- References: "See canon-lines.md for character-specific examples"

**Token budget**: 350 (SKILL.md) + 300 (patterns.md when needed) = 650 lines max

**Migration from**: `pm/reference/kojo-reference.md` (currently 600 lines)

---

#### **testing** skill

**When loaded**: Any test-related task, or subagent has `skills: ["testing"]`

**Files**:
```
skills/testing/
├── SKILL.md          # Test types, philosophy, overview
├── commands.md       # dotnet test, strict-warnings, coverage
└── scenarios.md      # Test scenario authoring, assertion patterns
```

**SKILL.md content** (~250 lines):
- Test types: unit, integration, AC verification
- Test philosophy: binary pass/fail, no confidence levels
- Headless testing: `cd Game && dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . < tests/scenario.txt`
- Strict warnings: `--strict-warnings` flag for loading errors
- References: "See commands.md for full command syntax"
- References: "See scenarios.md for scenario authoring guide"

**Token budget**: 250 (SKILL.md) + 200 (commands.md) = 450 lines max

**Migration from**: Scattered across `erb-reference.md`, `engine-reference.md`, and `CLAUDE.md`

---

#### **engine-dev** skill

**When loaded**: C# engine work, or subagent has `skills: ["engine-dev"]`

**Files**:
```
skills/engine-dev/
├── SKILL.md          # Architecture overview, design philosophy
├── interfaces.md     # Key interfaces, extension points
└── commands.md       # Command system architecture, VM internals
```

**SKILL.md content** (~400 lines):
- Engine architecture: VM, command registry, state management
- Key namespaces: uEmuera.Commands, uEmuera.GameData, uEmuera.VM
- Extension philosophy: prefer new commands over VM changes
- Build verification: `dotnet build uEmuera/uEmuera.Headless.csproj`
- References: "See interfaces.md for key interfaces"
- References: "See commands.md for command system details"

**Token budget**: 400 (SKILL.md) + 500 (interfaces.md when needed) = 900 lines max

**Migration from**: `pm/reference/engine-reference.md` (currently 1200 lines)

---

#### **ntr-system** skill

**When loaded**: NTR game logic work, or kojo-writer working on NTR dialogue

**Files**:
```
skills/ntr-system/
├── SKILL.md          # NTR system overview, design principles
├── variables.md      # Variable definitions, flags, TALENTs
└── phases.md         # Phase system (思慕→両想い→恋人→寝取られ)
```

**SKILL.md content** (~350 lines):
- NTR system overview (affection, jealousy, corruption)
- Core mechanics: phase progression, variable interactions
- Design philosophy: gradual corruption, emotional complexity
- References: "See variables.md for flag/variable definitions"
- References: "See phases.md for phase transition rules"

**Token budget**: 350 (SKILL.md) + 250 (variables.md) + 300 (phases.md) = 900 lines max

**Migration from**: `pm/reference/ntr-reference.md` (currently 700 lines)

---

### 2.3 Skill Loading Mechanism

**Skills には `paths:` フィールドは存在しない**（Rulesのみ）

**メインコンテキストでの検出**:
- Claudeが `description` を読んで自動判断
- タスク内容に関連すると判断した場合にロード

**Subagent での利用（未検証）**:
```yaml
# agents/implementer.md
---
name: implementer
skills: erb-lang, testing  # 公式フィールドだが効果は未検証
---
```

> ⚠️ `skills:` フィールドは公式ドキュメントに存在するが、
> subagent コンテキストへの Skills ロード効果は明示されていない。
> 現行の Read 参照パターンが動作確認済みのため、移行は慎重に。

**On-demand reference** (in SKILL.md):
```markdown
# SKILL.md
For full syntax reference, see syntax.md
```
Agent reads `syntax.md` only when needed via Read tool.

---

## 3. Rules Design (Path-based Auto-loading)

### 3.1 Design Principles

**Auto-loading**:
- Rules are loaded **automatically** based on file path patterns
- Rules are **concise** reminders (<200 lines), not comprehensive docs
- Rules **supplement** skills, not replace them

**Layering**:
1. `00-always.md` - Always loaded (core conventions)
2. Path-specific rules - Loaded when working with matching files

### 3.2 Rule Catalog

#### **00-always.md** (Always loaded)

**Content** (~200 lines):
```markdown
---
name: always
description: Core project conventions (always loaded)
---

# Core Project Conventions

## Language
- Documentation: English
- User responses: Japanese
- In-game text: Japanese
- Commit messages: English

## Commit Convention
- feat: New features
- fix: Bug fixes
- docs: Documentation
- refactor: Code refactoring
- test: Test additions

## Build Verification
Before commit: `dotnet build` + `dotnet test`

## File Encoding
- All ERB/ERH files: UTF-8 with BOM
- All C# files: UTF-8

## Line Endings
- Windows: CRLF (default)
```

**Token budget**: 200 lines

**Migration from**: `CLAUDE.md` (partial, core conventions only)

---

#### **erb-editing.md** (Auto-loaded for ERB files)

**Trigger**: paths: `["**/*.erb", "**/*.erh"]`

**Content** (~150 lines):
```markdown
---
name: erb-editing
description: ERB syntax reminders (auto-loaded for ERB files)
paths:
  - "**/*.erb"
  - "**/*.erh"
---

# ERB Editing Rules

## Syntax Quick Reference
- PRINTFORML for dialogue with newline
- @VAR for temporary variables
- #VAR for static variables
- ABL:FLAG:X for character flags
- Always use ENDIF to close IF blocks

## Common Mistakes
- Forgetting ENDIF (causes parse errors)
- Using PRINTFORM instead of PRINTFORML (missing newline)
- Mixing @VAR and #VAR scopes

## Testing
After editing: Run headless test
```

**Token budget**: 150 lines

**Migration from**: New content (distilled from erb-reference.md)

---

#### **csharp-editing.md** (Auto-loaded for C# files)

**Trigger**: paths: `["**/*.cs"]`

**Content** (~180 lines):
```markdown
---
name: csharp-editing
description: C# conventions for uEmuera engine
paths:
  - "**/*.cs"
---

# C# Editing Rules

## Code Style
- Follow existing code patterns in uEmuera
- Prefer readonly fields over properties where applicable
- Use meaningful variable names (no single-letter except loop indices)

## Extension Points
- New commands: Inherit from BaseCommand
- New functions: Register in FunctionRegistry
- Avoid modifying VM core unless necessary

## Build Verification
```bash
dotnet build uEmuera/uEmuera.Headless.csproj
```

## Testing
After engine changes: Run full test suite
```

**Token budget**: 180 lines

**Migration from**: New content (distilled from engine-reference.md)

---

#### **testing.md** (Auto-loaded for test work)

**Trigger**: paths: `["**/tests/**", "**/*.Tests/**"]`

**Content** (~120 lines):
```markdown
---
name: testing
description: Testing rules and commands
paths:
  - "**/tests/**"
  - "**/*.Tests/**"
---

# Testing Rules

## Test Types
- Unit tests: Single function/command
- Integration tests: Multi-file scenarios
- AC tests: Feature acceptance verification

## Judgment
- Binary pass/fail only
- No confidence levels
- No partial credit

## Headless Testing
```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . < tests/scenario.txt
```

## Strict Warnings
Use `--strict-warnings` flag to catch loading errors.
```

**Token budget**: 120 lines

**Migration from**: Scattered across multiple references

---

#### **kojo-editing.md** (Auto-loaded for kojo files)

**Trigger**: paths: `["**/口上/**/*.erb"]`

**Content** (~140 lines):
```markdown
---
name: kojo-editing
description: Kojo (dialogue) editing reminders
paths:
  - "**/口上/**/*.erb"
---

# Kojo Editing Rules

## Character Voice
- Maintain consistent keigo (敬語) level per character
- Meiling: Polite but casual (です/ます)
- Sakuya: Formal keigo (でございます)
- Patchouli: Intellectual, reserved

## Conditional Blocks
- Use IF @ABL:FLAG:X for affection-gated dialogue
- Always provide ELSE fallback for robustness

## Quality Checklist
- [ ] Canon personality adherence
- [ ] Emotional depth (not just exposition)
- [ ] Natural Japanese phrasing
- [ ] Conditional branching for multiple affection levels
```

**Token budget**: 140 lines

**Migration from**: New content (distilled from kojo-reference.md)

---

### 3.3 Token Efficiency Comparison

| Component | Current (all loaded) | Target (demand-load) | Savings |
|-----------|:--------------------:|:--------------------:|:-------:|
| **reference/*.md** | 4,300 lines | 0 (migrated to skills) | +4,300 |
| **Skills (average)** | 0 | ~500 (when needed) | -500 |
| **Rules (always)** | 0 | 200 (00-always.md) | -200 |
| **Rules (path-triggered)** | 0 | ~150 (avg per file, when triggered) | -150 |
| **Net savings** | - | - | **~3,450 lines** |

**Scenario Analysis**:
- **ERB editing task**: 200 (always) + 150 (erb-editing) + 300 (erb-lang/SKILL.md) = 650 lines
  - vs Current: 200 (CLAUDE.md) + 800 (erb-reference.md) = 1,000 lines
  - **Savings**: 350 lines (35%)

- **Kojo writing task**: 200 (always) + 140 (kojo-editing) + 350 (kojo-writing/SKILL.md) = 690 lines
  - vs Current: 200 (CLAUDE.md) + 600 (kojo-reference.md) + 700 (ntr-reference.md) = 1,500 lines
  - **Savings**: 810 lines (54%)

- **Engine task**: 200 (always) + 180 (csharp-editing) + 400 (engine-dev/SKILL.md) = 780 lines
  - vs Current: 200 (CLAUDE.md) + 1,200 (engine-reference.md) = 1,400 lines
  - **Savings**: 620 lines (44%)

---

## 4. Role Assignment Table

| Function | hooks | skills | subagents | commands | rules | CLAUDE.md |
|----------|:-----:|:------:|:---------:|:--------:|:-----:|:---------:|
| Auto-run on file change | ● | | | | | |
| Domain knowledge (comprehensive) | | ● | | | | |
| Separate context work | | | ● | | | |
| User workflow trigger | | | | ● | | |
| Path-conditional load | | ● | | | ● | |
| Quick syntax reminders | | | | | ● | |
| Always-loaded overview | | | | | | ● |
| Progressive disclosure | | ● | | | | |
| Decision-making delegation | | | ● | | | |

### 4.1 Detailed Responsibilities

#### **hooks** (Deterministic Automation)
- **Purpose**: Run automatically after specific file operations
- **Examples**:
  - `post-erb-write.ps1` - Auto-format ERB files after Write tool
  - `post-commit.ps1` - Update feature index after commit (future)
- **Criteria**:
  - Deterministic (no AI decision-making)
  - Fast (<5s execution)
  - Idempotent (safe to run multiple times)

#### **skills** (Progressive Disclosure)
- **Purpose**: Comprehensive domain knowledge, loaded on-demand
- **Examples**:
  - `erb-lang` - ERB syntax, functions, patterns
  - `kojo-writing` - Dialogue writing principles, character voice
  - `engine-dev` - Engine architecture, extension points
- **Criteria**:
  - >500 lines of content (if smaller, use rules)
  - Loaded on-demand (not always)
  - Supports 1-level deep references (SKILL.md → supporting files)

#### **subagents** (Separate Context Work)
- **Purpose**: Delegate work to separate context with fresh memory
- **Examples**:
  - `implementer` - ERB/Engine code implementation
  - `kojo-writer` - Dialogue creation (opus model)
  - `ac-tester` - AC verification
- **Criteria**:
  - Requires separate context (memory isolation)
  - May require different model (e.g., opus for kojo)
  - Results persist in feature-{ID}.md

#### **commands** (User Workflow Trigger)
- **Purpose**: User-triggered workflows (slash commands)
- **Examples**:
  - `/imple` - Execute feature implementation
  - `/next` - Propose next work item
  - `/queue` - Report execution order
- **Criteria**:
  - Explicitly triggered by user
  - Orchestrates multiple subagents
  - Has clear entry/exit points

#### **rules** (Path-conditional Reminders)
- **Purpose**: Concise syntax reminders, auto-loaded by file path
- **Examples**:
  - `erb-editing.md` - Quick ERB syntax reminders
  - `csharp-editing.md` - C# style conventions
  - `00-always.md` - Core project conventions
- **Criteria**:
  - <200 lines per file
  - Quick reference, not comprehensive
  - Auto-loaded by path pattern

#### **CLAUDE.md** (Always-loaded Overview)
- **Purpose**: High-level project overview, always in context
- **Examples**:
  - Project structure overview
  - Quick start commands
  - Subagent dispatch table
  - Language conventions
- **Criteria**:
  - <1000 lines (currently ~400)
  - Always loaded (part of every context)
  - High-level overview, delegates to skills/rules for details

---

## 5. Decision Flowchart

```
┌─────────────────────────────────────────────────────────────┐
│ Need to add functionality to Claude Code?                   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
            ┌────────────────────┐
            │ Need automation?   │
            └────┬───────────┬───┘
                 │ Yes       │ No
                 ▼           ▼
        ┌────────────┐   ┌──────────────────┐
        │Deterministic?│  │Domain knowledge? │
        └───┬────┬────┘   └────┬──────────┬──┘
            │Yes │No           │Yes       │No
            ▼    ▼             ▼          ▼
         ┌────┐ ┌────────┐ ┌──────────┐ ┌────────────────┐
         │hook│ │subagent│ │Large     │ │User-triggered  │
         └────┘ └────────┘ │(>500     │ │workflow?       │
                            │lines)?   │ └────┬───────┬───┘
                            └──┬───┬───┘      │Yes    │No
                               │Yes│No        ▼       ▼
                               ▼   ▼      ┌───────┐ ┌──────────┐
                            ┌─────┐ ┌────┐│command│ │Path-     │
                            │skill│ │rule││       │ │specific? │
                            └─────┘ └────┘└───────┘ └──┬───┬───┘
                                                        │Yes│No
                                                        ▼   ▼
                                                    ┌────┐ ┌──────────┐
                                                    │rule│ │CLAUDE.md │
                                                    └────┘ └──────────┘
```

### 5.1 Decision Examples

**Example 1**: "I need ERB syntax reference"
- Domain knowledge? **Yes**
- Large (>500 lines)? **Yes** (800 lines)
- **Decision**: Create `skills/erb-lang/`

**Example 2**: "I need to remind agents to use UTF-8 encoding"
- Domain knowledge? **Yes**
- Large (>500 lines)? **No** (10 lines)
- Path-specific? **No** (applies to all files)
- **Decision**: Add to `CLAUDE.md` or `rules/00-always.md`

**Example 3**: "I need to auto-format ERB files after editing"
- Need automation? **Yes**
- Deterministic? **Yes** (run formatter script)
- **Decision**: Create `hooks/post-erb-write.ps1`

**Example 4**: "I need to implement ERB code"
- Need automation? **Yes**
- Deterministic? **No** (requires AI decision-making)
- **Decision**: Use `subagents/implementer.md`

**Example 5**: "I need quick ERB syntax reminders when editing ERB files"
- Domain knowledge? **Yes**
- Large (>500 lines)? **No** (150 lines)
- Path-specific? **Yes** (*.erb, *.erh)
- **Decision**: Create `rules/erb-editing.md` with paths: ["**/*.erb", "**/*.erh"]

**Example 6**: "I need a /imple workflow"
- User-triggered workflow? **Yes**
- **Decision**: Create `commands/imple.md`

---

## 6. Subagent Integration with Skills (2025-12-20 再検証)

### 公式ドキュメントに基づく正しい理解

**Subagent Frontmatter の公式フィールド（6つ）**:

| フィールド | 必須 | 説明 |
|-----------|:----:|------|
| `name` | ✓ | エージェント名 |
| `description` | ✓ | 使用タイミングの説明 |
| `tools` | | 省略で全ツール継承 |
| `model` | | デフォルト: sonnet |
| `permissionMode` | | パーミッションモード |
| `skills` | | **存在する**が効果は未検証 |

### 6.1 Skills/Rules の適用範囲

```
┌─────────────────────────────────────────────────────────────┐
│  メインコンテキスト（Opus）                                   │
│  ├── CLAUDE.md ✓                                            │
│  ├── Skills ✓ (description ベースで自動検出)                 │
│  └── Rules ✓ (paths ベースで自動ロード)                      │
├─────────────────────────────────────────────────────────────┤
│  Subagent コンテキスト（Task tool 経由）                      │
│  ├── CLAUDE.md → 不明（公式ドキュメントに記載なし）           │
│  ├── Skills → skills: フィールドで指定可能（効果未検証）      │
│  ├── Rules → 不明（公式ドキュメントに記載なし）               │
│  └── prompts + Read 参照 → 動作確認済み                      │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 推奨パターン

**現在のRead参照パターンは動作確認済み**:
```markdown
# .claude/agents/implementer.md
Read pm/reference/erb-reference.md to understand ERB syntax.
Read pm/reference/engine-reference.md for engine API.
```

**`skills:` フィールドの活用は実験後に判断**:
```yaml
# 実験用設定
---
name: implementer
skills: erb-lang, testing  # 効果を検証してから本番適用
---
```

### 6.3 トークン効率への影響

**Skills/Rules 導入によるトークン削減効果**:
- メインコンテキスト（Opus直接作業）: 効果あり
- subagent 経由: `skills:` フィールドの効果次第（要検証）

**結論**:
- 現行 Read 参照パターンは維持（動作確認済み）
- `skills:` フィールドは Feature 150 で実験後に判断

---

## 7. Migration Impact Analysis

### 7.1 File Count Changes

| Directory | Current | Target | Change |
|-----------|:-------:|:------:|:------:|
| `.claude/commands/` | 4 files | 4 files | 0 |
| `.claude/agents/` | 13 files | 13 files | 0 |
| `.claude/hooks/` | 1 file | 1 file | 0 |
| `.claude/skills/` | 0 | 5 dirs (15 files) | +15 |
| `.claude/rules/` | 0 | 5 files | +5 |
| `pm/reference/` | 5 files | 0 files (migrated) | -5 |
| **Total** | 23 files | 38 files | **+15 files** |

### 7.2 Token Budget Changes

**Current** (worst case - all references loaded):
- `CLAUDE.md`: 400 lines
- `erb-reference.md`: 800 lines
- `kojo-reference.md`: 600 lines
- `ntr-reference.md`: 700 lines
- `engine-reference.md`: 1,200 lines
- `feature-template.md`: 600 lines
- **Total**: 4,300 lines

**Target** (typical ERB task):
- `CLAUDE.md`: 400 lines
- `rules/00-always.md`: 200 lines
- `rules/erb-editing.md`: 150 lines
- `skills/erb-lang/SKILL.md`: 300 lines
- `skills/testing/SKILL.md`: 250 lines
- **Total**: 1,300 lines

**Savings**: 3,000 lines (70% reduction for typical task)

### 7.3 Backward Compatibility

**Preserved**:
- `/imple`, `/next`, `/queue`, `/roadmap` commands (no changes)
- Subagent dispatch pattern (Task tool usage)
- Feature-{ID}.md format
- AC definition table format
- Execution log format

**Changed**:
- `pm/reference/*.md` files deleted (migrated to skills)
- Subagent prompts updated to reference skills instead of reference/*.md
- CLAUDE.md slimmed down (delegates to skills/rules)

**Migration safety**:
- Phase 1: Create skills/ and rules/ (additive, no deletions)
- Phase 2: Update subagent definitions (skills: field)
- Phase 3: Test with parallel reference/*.md (validation)
- Phase 4: Delete reference/*.md after validation

---

## 8. Implementation Roadmap Preview

**Feature 149** (current):
- Design only (this document)

**Feature 150**: Skills Implementation
- Task 1: Create `skills/erb-lang/` (SKILL.md, syntax.md, functions.md)
- Task 2: Create `skills/kojo-writing/` (SKILL.md, patterns.md, canon-lines.md)
- Task 3: Create `skills/testing/` (SKILL.md, commands.md, scenarios.md)
- Task 4: Create `skills/engine-dev/` (SKILL.md, interfaces.md, commands.md)
- Task 5: Create `skills/ntr-system/` (SKILL.md, variables.md, phases.md)

**Feature 151**: Rules Implementation
- Task 1: Create `rules/00-always.md`
- Task 2: Create `rules/erb-editing.md`
- Task 3: Create `rules/csharp-editing.md`
- Task 4: Create `rules/testing.md`
- Task 5: Create `rules/kojo-editing.md`

**Feature 152**: Subagent Integration
- Task 1: Update all subagent definitions with `skills:` field
- Task 2: Update subagent prompts to reference skills (not reference/*.md)
- Task 3: Update CLAUDE.md (slim down, delegate to skills/rules)

**Feature 153**: Validation & Migration
- Task 1: Parallel testing (skills + old reference/*.md)
- Task 2: AC validation (all existing features still work)
- Task 3: Delete `pm/reference/*.md`
- Task 4: Update all documentation references

---

## 9. Validation Criteria

**Feature 149 complete** when:
- [x] Target architecture design document exists (this file)
- [x] Skills catalog defined (5 skills)
- [x] Rules catalog defined (5 rules)
- [x] Role assignment table complete
- [x] Decision flowchart complete
- [x] Token efficiency analysis complete
- [x] Migration roadmap preview complete

**Future features succeed** when:
- All `/imple` workflows still work (no regression)
- Token usage reduced by >50% for typical tasks
- All existing features (145-148) still pass AC tests
- No functionality lost in migration

---

## Appendix A: Anthropic BP Compliance Checklist

| Best Practice | Current | Target | Status |
|--------------|---------|--------|:------:|
| Skills for domain knowledge | ✗ (using reference/*.md) | ✓ (skills/) | PLANNED |
| Progressive disclosure (<500 lines) | ✗ (3000+ line files) | ✓ (<500 per file) | PLANNED |
| Rules for path-conditional loading | ✗ (not used) | ✓ (rules/) | PLANNED |
| Hooks for deterministic automation | ✓ (post-erb-write.ps1) | ✓ (preserve) | DONE |
| Subagents for separate context | ✓ (13 subagents) | ✓ (preserve) | DONE |
| Commands for user workflows | ✓ (/imple, /next, etc) | ✓ (preserve) | DONE |
| 1-level deep skill references | N/A | ✓ (SKILL.md → files) | PLANNED |

---

## Appendix B: Token Budget Breakdown

### Skills Token Budget (Max Load)

| Skill | SKILL.md | Supporting Files (max) | Total |
|-------|:--------:|:----------------------:|:-----:|
| erb-lang | 300 | 400 (syntax.md) | 700 |
| kojo-writing | 350 | 300 (patterns.md) | 650 |
| testing | 250 | 200 (commands.md) | 450 |
| engine-dev | 400 | 500 (interfaces.md) | 900 |
| ntr-system | 350 | 300 (phases.md) | 650 |

### Rules Token Budget (Always + Path-Triggered)

| Rule | Always Loaded | Path-Triggered | Lines |
|------|:-------------:|:--------------:|:-----:|
| 00-always.md | ✓ | | 200 |
| erb-editing.md | | ✓ (*.erb) | 150 |
| csharp-editing.md | | ✓ (*.cs) | 180 |
| testing.md | | ✓ (tests/*) | 120 |
| kojo-editing.md | | ✓ (口上/*) | 140 |

### Scenario Token Loads

**Scenario 1: ERB Implementation Task**
- CLAUDE.md: 400
- rules/00-always.md: 200
- rules/erb-editing.md: 150 (path-triggered)
- skills/erb-lang/SKILL.md: 300 (via skills: field)
- skills/testing/SKILL.md: 250 (via skills: field)
- **Total**: 1,300 lines

**Scenario 2: Kojo Writing Task**
- CLAUDE.md: 400
- rules/00-always.md: 200
- rules/kojo-editing.md: 140 (path-triggered)
- skills/kojo-writing/SKILL.md: 350 (via skills: field)
- skills/ntr-system/SKILL.md: 350 (via skills: field)
- **Total**: 1,440 lines

**Scenario 3: Engine Development Task**
- CLAUDE.md: 400
- rules/00-always.md: 200
- rules/csharp-editing.md: 180 (path-triggered)
- skills/engine-dev/SKILL.md: 400 (via skills: field)
- skills/testing/SKILL.md: 250 (via skills: field)
- **Total**: 1,430 lines

**Average token load**: ~1,400 lines (vs current 4,300 lines = **67% reduction**)

---

**Document End**
