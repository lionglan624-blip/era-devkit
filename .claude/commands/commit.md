---
description: Commit changes with logical grouping
---

**Language**: Thinking in English, respond to user in Japanese.

---

Commit uncommitted changes across all 5 repositories.

**Principle**: Separate changes by logical unit and commit individually.
- **No confirmation needed**: Automatically separate without asking user
- **When in doubt, separate**: If unsure whether to combine, make separate commits

## 1. Snapshot Current Changes (CRITICAL)

**IMPORTANT**: Take a snapshot of changed files FIRST. Only these files are commit targets.
Other sessions may edit files during commit process - ignore those new changes.

```bash
# Devkit (current repo)
DEVKIT_FILES=$(git status --porcelain)
echo "$DEVKIT_FILES"

# Game repo
GAME_FILES=$(cd "${GAME_PATH:-/c/Era/game}" && git status --porcelain)
echo "$GAME_FILES"

# Core repo
CORE_FILES=$(cd "${CORE_PATH:-/c/Era/core}" && git status --porcelain)
echo "$CORE_FILES"

# Engine repo
ENGINE_FILES=$(cd "${ENGINE_PATH:-/c/Era/engine}" && git status --porcelain)
echo "$ENGINE_FILES"

# Dashboard repo
DASHBOARD_FILES=$(cd "${DASHBOARD_PATH:-/c/Era/dashboard}" && git status --porcelain)
echo "$DASHBOARD_FILES"
```

**Rule**: Throughout this command, ONLY process files that appeared in this initial snapshot.
If `git status` shows new files later, ignore them - they belong to another session.

## 2. Categorize Snapshot Files by Logical Unit

Group files into **separate commits** by logical unit:

| Category | Repo | Files | Commit Type |
|----------|------|-------|-------------|
| Feature docs | devkit | pm/features/feature-*.md | `docs:` |
| Index/tracking | devkit | index-features.md | `docs:` |
| Config/commands | devkit | .claude/**, CLAUDE.md | `docs:` or `chore:` |
| ERB scripts | game | ERB/**/*.ERB | `feat:` or `fix:` |
| YAML data | game | data/**/*.yaml | `feat:` or `fix:` |
| Engine code | engine | Assets/Scripts/** | `feat:` or `fix:` |
| Core library | core | src/Era.Core/** | `feat:` or `fix:` |
| Core tests | core | src/Era.Core.Tests/** | `test:` |
| Tool code | devkit | src/tools/** | `feat:` or `fix:` |
| Tests | devkit | test/** | `test:` |
| Build config | any | *.csproj, .gitignore | `chore:` |
| Dashboard | dashboard | ** | `feat:` or `fix:` |

**Important**: Only group related changes in one commit. Changes to different features or purposes should be separate commits.

## 3. Handle Untracked Files

**Recommended**: Add build artifacts and temporary outputs to `.gitignore` without asking.

| Pattern | Action | Reason |
|---------|--------|--------|
| `_ul` | .gitignore | Unity lock file |
| `bin/`, `obj/` | .gitignore | Build output |
| `*.user` | .gitignore | User settings |
| `*_results.txt` | .gitignore | Test output |
| `*-result.json` | .gitignore | Test results |

**Only ask** for files that are clearly intentional additions (new source files, documentation, configs).

## 4. Commit Strategy (Per Logical Unit)

**CRITICAL**: Use explicit file paths from the snapshot. Do NOT use wildcards like `*.md` or `**/*.ERB` - they may capture files added by other sessions after snapshot.

### Devkit (current repo)
```bash
git add pm/features/feature-050.md
git commit -m "docs: Add Feature 050 specification

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

### Other repos (use subshell to avoid cwd change)
```bash
# Game repo
(cd "${GAME_PATH:-/c/Era/game}" && git add <files> && git commit -m "<type>: <description>

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>")

# Core repo
(cd "${CORE_PATH:-/c/Era/core}" && git add <files> && git commit -m "<type>: <description>

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>")

# Engine repo
(cd "${ENGINE_PATH:-/c/Era/engine}" && git add <files> && git commit -m "<type>: <description>

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>")

# Dashboard repo
(cd "${DASHBOARD_PATH:-/c/Era/dashboard}" && git add <files> && git commit -m "<type>: <description>

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>")
```

## 5. Commit Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `refactor`: Code refactoring
- `test`: Adding/updating tests
- `chore`: Maintenance tasks

## 6. Completion Criteria (CRITICAL)

**Done when**: All files from the initial snapshot (Step 1) have been committed or ignored.

**Do NOT**:
- Run `git status` again after commits
- Process any files not in the original snapshot
- Loop back to check for "remaining" changes

**Verification**: Compare committed files against the snapshot list, NOT against current `git status`.

## 7. Report

```
=== Commit Summary ===

Devkit:
1. {hash}: {message} ({files})

Game:
(no changes)

Core:
1. {hash}: {message} ({files})

Engine:
(no changes)

Dashboard:
(no changes)

Snapshot files: {count} -> Committed: {count}, Ignored: {count}
```
