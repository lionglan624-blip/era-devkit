# Feature 232: File Placement Cleanup

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`logs`, `debug`, `agents/logs`, `Game/logs`, `uEmuera`, `exe`, `shortcut`, `lnk`, `placement`, `cleanup`

---

## Summary

Unify file placement rules discovered during F229 verification: debug log location inconsistency, legacy log directory cleanup, and GUI exe/shortcut documentation.

---

## Background

### Philosophy (Mid-term Vision)

File placement must be consistent, documented, and predictable. All temporary/debug outputs should go to designated locations. Build artifacts should be clearly separated from source.

### Problem (Current Issue)

F229 verification revealed additional file placement issues not covered by its scope:

| Issue | Problem | Impact |
|:-----:|---------|--------|
| P5 | Debug log location unclear: CLAUDE.md `.tmp/` is generic for ad-hoc output, but do.md and testing SKILL use `logs/debug/` (relative) for reviewable debug logs | Need explicit `Game/logs/debug/` in CLAUDE.md |
| P6 | `Game/agents/logs/` contains legacy logs with malformed filenames (`${timestamp}`) | Workspace pollution, obsolete data |
| P7 | GUI exe placement undocumented: currently root, should be `uEmuera_build/` with shortcut | Unclear build artifact handling |

### Goal (What to Achieve)

1. Unify debug log location documentation (CLAUDE.md and do.md agree)
2. Remove legacy `Game/agents/logs/` directory
3. Document GUI exe placement and shortcut convention in CLAUDE.md
4. Automate shortcut (.lnk) creation for GUI exe

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CLAUDE.md debug log location updated | code | Grep | contains | "Game/logs/debug" | [x] |
| 2 | do.md uses logs/debug path | code | Grep | contains | "logs/debug" | [x] |
| 3 | Game/agents/logs/ cleaned | file | Glob | count_equals | 1 | [x] |
| 4 | CLAUDE.md documents GUI exe placement | code | Grep | contains | "uEmuera_build" | [x] |
| 5 | CLAUDE.md documents shortcut convention | code | Grep | contains | ".lnk" | [x] |
| 6 | Build succeeds | build | dotnet | succeeds | engine/uEmuera.Headless.csproj | [x] |
| 7 | Shortcut creation automated | file | Bash | exists | uEmuera.lnk | [x] |

### AC Details

**AC1 Test**: Verify CLAUDE.md references Game/logs/debug for debug output
```
Grep(pattern="Game/logs/debug", path="CLAUDE.md")
```

**AC2 Test**: Verify do.md uses logs/debug path (relative path is correct since executed from Game/)
```
Grep(pattern="logs/debug", path=".claude/commands/do.md")
```

**AC3 Test**: Verify legacy logs directory cleaned (only .gitkeep remains)
```
Glob(pattern="Game/agents/logs/*").count == 1 (the remaining file should be .gitkeep)
```

**AC4 Test**: Verify GUI exe placement documentation
```
Grep(pattern="uEmuera_build", path="CLAUDE.md")
```

**AC5 Test**: Verify shortcut convention documentation
```
Grep(pattern=".lnk", path="CLAUDE.md")
```

**AC6 Test**: Build succeeds
```
dotnet build engine/uEmuera.Headless.csproj
```

**AC7 Test**: Verify shortcut exists (in repository root)
```
Glob(pattern="uEmuera.lnk") → expect match
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update CLAUDE.md: Clarify `.tmp/` is for throwaway files, add `Game/logs/debug/` for debug logs that may need review | [x] |
| 2 | 2 | Verify do.md uses logs/debug path (relative path is correct) | [x] |
| 3 | 3 | Remove all files and subdirectories from `Game/agents/logs/` except .gitkeep | [x] |
| 4 | 4 | Add GUI exe placement section to CLAUDE.md (`uEmuera_build/` directory) | [x] |
| 5 | 5 | Document shortcut convention in CLAUDE.md (root shortcut points to uEmuera_build/uEmuera.exe) | [x] |
| 6 | 6 | Verify `dotnet build engine/uEmuera.Headless.csproj` succeeds | [x] |
| 7 | 7 | Create shortcut automation (hook or script) for uEmuera.lnk | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Technical Details

### Debug Log Location Decision

**Current state**:
- CLAUDE.md Temporary Files section: "Ad-hoc output → `.tmp/`"
- do.md Phase 5/6 sections: References `logs/debug/` (relative path)
- Actual structure: `Game/logs/debug/failed/` exists and is used

**Decision**: Use `Game/logs/debug/` as the SSOT for debug logs.
- `.tmp/` → temporary script output, throwaway files
- `Game/logs/debug/` → debug logs that may need review

### Legacy Logs Cleanup

**Target**: Remove all files and subdirectories from `Game/agents/logs/` except `.gitkeep`
- Contains ~25 directories + 11 malformed files with `${timestamp}` pattern
- All are gitignored but pollute local workspace

**Note**: Production logs are in `Game/logs/prod/` - DO NOT touch.

### GUI Placement Convention

**Current**: Both `uEmuera.exe` (in root) and `uEmuera_build/uEmuera.exe` exist. The root copy is from manual copying after build.
**Proposed**:
- Build output: `uEmuera_build/uEmuera.exe` (already exists)
- Root: Shortcut (`.lnk`) pointing to `uEmuera_build/uEmuera.exe`
- `.gitignore`: Already has `/*.exe` and `uEmuera_build/` rules

### Shortcut Creation (AC7)

**Implementation**: PowerShell WshShell COM object

```powershell
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$PWD\uEmuera.lnk")
$Shortcut.TargetPath = "$PWD\uEmuera_build\uEmuera.exe"
$Shortcut.WorkingDirectory = "$PWD\Game"
$Shortcut.Save()
```

**Options**: Hook (post-build) or standalone script. Shortcut is gitignored (`/*.lnk`).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 15:03 | START | implementer | Task 1 | - |
| 2025-12-27 15:03 | END | implementer | Task 1 | SUCCESS |
| 2025-12-27 15:03 | START | implementer | Task 2 | - |
| 2025-12-27 15:03 | END | implementer | Task 2 | SUCCESS |
| 2025-12-27 15:03 | START | implementer | Task 3 | - |
| 2025-12-27 15:03 | END | implementer | Task 3 | SUCCESS |
| 2025-12-27 15:03 | START | implementer | Task 6 | - |
| 2025-12-27 15:03 | END | implementer | Task 6 | SUCCESS |
| 2025-12-27 15:04 | START | implementer | Task 7 | - |
| 2025-12-27 15:04 | END | implementer | Task 7 | SUCCESS |

---

## Dependencies

- **F229** (Infrastructure Verification): Parent issue that identified these gaps

---

## Links

- [F229](feature-229.md) - Infrastructure Verification (parent)
- [F223](feature-223.md) - /do Workflow Comprehensive Audit (Category 8: File Placement)
