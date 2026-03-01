# Repository Recovery Plan

Authoritative recovery procedure for F733/F734/F735/F736. Created before /fc to prevent subagent drift.

**This document is the ground truth. Post-completion verification MUST check adherence.**

---

## Incident Summary

- **Cause**: ac-tester subagent executed `git checkout -- .` during F729 run
- **Impact**: ALL uncommitted tracked file changes across the entire repository reverted to HEAD (~80 files)
- **Dashboard**: 16+ files (backend, frontend, infra configs) → F734/F735
- **Workflow config**: 31 .claude/ files (agents, commands, skills, hooks) → F736
- **KojoComparer**: 5 files → F736
- **variable_sizes.yaml**: 3 values → Already fixed manually (EXP:200, EXPNAME:200, TCVAR:600)
- **Design docs**: 26 files → F736 (low priority)
- **Not affected**: Untracked files (test files, coverage dirs, test/setup.js)
- **Partial recovery already done**: package.json (3), vite.config.js (test config), settings.json (deny rules)
- **Prevention**: settings.json deny rules added for `git checkout -- .`, `git restore *`, `git reset --hard*`, `git clean *`

## Recovery Data Sources

| Source | Path | Content |
|--------|------|---------|
| Session JSONL | `~/.ccs/shared/projects/C--Era-erakoumakanNTR/*.jsonl` | toolUseResult with filePath, oldString, newString, originalFile |
| Dashboard debug logs | `.tmp/dashboard/debug-*.log` | Stream-json output (less useful) |
| HANDOFF.md | `src/tools/node/feature-dashboard/HANDOFF.md` | Architectural spec, feature list, API spec |
| Test files (untracked) | `backend/src/**/*.test.js`, `frontend/src/**/*.test.{js,jsx}` | Expected behavior specification |
| node_modules | `src/tools/node/feature-dashboard/node_modules/` | Installed deps (vitest, jsdom, etc.) |

## Lost Files Inventory

### Backend (5 files)

| File | Key lost changes |
|------|-----------------|
| `claudeService.js` | named exports (validateFeatureId, validateCommand, INPUT_WAIT_PATTERNS), dispose(), _checkActivity(), _getSessionFilePath(), _buildClaudeEnv terminal mode, stall re-broadcast prevention, _pushLog margin, _cleanupOldExecutions interval clear, ACTIVITY_CHECK_INTERVAL_MS, session file mtime polling |
| `fileWatcher.js` | _parseStatus(), extractStatus(async), initializeCache() cache population, checkStatusChange() updates, stop() cleanup (pendingStatusChanges, statusCache clear) |
| `execution.js` | resume/browser prompt sanitization (10000 char limit, control char removal) |
| `server.js` | Unknown scope - check session data |
| `logger.js` | Unknown scope - check session data |

### Frontend (8 files)

| File | Key lost changes |
|------|-----------------|
| `App.jsx` | useReducer integration (dispatch instead of multi-setState), single-pass useMemo derivation, WS message handlers dispatching to reducer, health status indicators, command buttons, notification dedup |
| `ExecutionPanel.jsx` | Tab management with reducer, status icons, input detection, resume buttons |
| `LogViewer.jsx` | Virtual scroll (LINE_HEIGHT=20px, OVERSCAN=20), position:relative wrapper, auto-scroll smart detection |
| `TreeView.jsx` | Elapsed time display, [DONE] session visibility, orphan ⟳ prefix, running command display |
| `useExecution.js` | useReducer-based state (executions, executionStates, featurePhases, inputRequests), BATCH_LOGS, WS_STATE, WS_INPUT_REQUIRED, WS_STALLED, WS_HANDOFF, SET_FEATURE_PHASE, ADD_EXECUTION, UPDATE_EXECUTION, CLOSE_TAB, CLOSE_FINISHED_TABS actions, fetchExecutions with Map replacement |
| `useWebSocket.js` | beforeunload reconnect prevention |
| `main.css` | All UI/UX styling (state colors, mobile layout, virtual scroll, tree layout) |
| `vite.config.js` | test config already restored |

### Deleted file

| File | Note |
|------|------|
| `usageService.js` | Should be deleted (was removed in Feature #50) |

## F733: Session JSONL Extractor - Procedure

### Purpose
Extract the final known-good state of each lost file from session JSONL data.

### Tool Design

```
tools/session-extractor/
├── index.js          # CLI entry point
├── jsonl-parser.js   # Streaming JSONL line parser
├── edit-replayer.js  # Apply Edit(oldString→newString) to file content
└── package.json
```

### Algorithm

1. **Identify target sessions**: `grep -l "feature-dashboard" ~/.ccs/shared/projects/C--Era-erakoumakanNTR/*.jsonl`
2. **For each target file**:
   a. Scan all matching sessions chronologically (by session file mtime)
   b. Collect Write operations → record `originalFile` as snapshot with timestamp
   c. Collect Edit operations → record `oldString`, `newString` with timestamp
3. **Reconstruct final state**:
   a. Start from the latest Write snapshot (if any)
   b. Apply all Edit operations after that snapshot, in chronological order
   c. If no Write exists, start from current HEAD content and apply all Edits
4. **Output**: Write reconstructed files to `.tmp/recovery/{relative-path}`
5. **Summary**: JSON report listing each file, source sessions, operation count, confidence

### Critical constraints
- **Streaming parser**: JSONL files can be hundreds of MB. Parse line-by-line, never load entire file
- **Exact string matching**: Edit `oldString` must match exactly in the current file state. If mismatch, log warning and skip (indicates gap in chain)
- **Timestamp ordering**: Session file mtime determines session order. Within a session, line order = chronological order
- **Path normalization**: Session data uses Windows paths (`C:\\Era\\...`). Normalize for comparison

### Output validation
- Diff each reconstructed file against HEAD to confirm non-trivial changes recovered
- Diff against current working tree (some files already partially modified in this session)

## F734: Backend Recovery - Procedure

### Step 1: Apply F733 output
```bash
cp .tmp/recovery/src/tools/node/feature-dashboard/backend/src/services/claudeService.js \
   src/tools/node/feature-dashboard/backend/src/services/claudeService.js
# Repeat for each backend file
```

### Step 2: Manual review
- Compare F733 output with HANDOFF.md spec (API, WebSocket events, constants)
- Check that today's changes (getTotalPhases, totalPhases in broadcastState/getExecution/listExecutions) are preserved
- Verify usageService.js is NOT restored (was intentionally deleted)

### Step 3: Test verification
```bash
cd src/tools/node/feature-dashboard/backend && npx vitest run
```
- Target: 132/132 pass (currently 93 pass, 39 fail)
- If tests fail after applying F733 output, use test expectations as spec to fix

### Step 4: Verify named exports
Tests import: `{ validateFeatureId, validateCommand, INPUT_WAIT_PATTERNS, ClaudeService }`
- These MUST be exported from claudeService.js

### Key backend changes to verify against HANDOFF

| HANDOFF Section | Source file | What to check |
|-----------------|------------|---------------|
| セッションファイルmtime監視 | claudeService.js | _checkActivity(), ACTIVITY_CHECK_INTERVAL_MS, _getSessionFilePath() |
| Auto Handoff | claudeService.js | _handoffToTerminal(), _checkInputWaitPatterns() |
| メモリ管理 | claudeService.js | MAX_LOG_ENTRIES with margin, _cleanupOldExecutions() |
| バリデーション | claudeService.js, execution.js | validateFeatureId(), resume prompt sanitize |
| Graceful shutdown | server.js | killAllRunning(), 5s timeout |
| FileWatcher | fileWatcher.js | debounce, statusCache, _parseStatus() |

## F735: Frontend Recovery - Procedure

### Step 1: Apply F733 output
Same as backend - copy reconstructed files.

### Step 2: Manual review
- Compare with HANDOFF.md UI spec (TreeView, state coloring, LogViewer, ExecutionPanel)
- Check that today's changes preserved:
  - TreeView: `P{N}/{total}` display, ID ascending sort
  - useExecution: totalPhases in featurePhases

### Step 3: Test verification
```bash
cd src/tools/node/feature-dashboard/frontend && npx vitest run
```
- Target: 224/224 pass (currently 168 pass, 56 fail)

### Step 4: UI/UX visual verification (manual)
Tests do NOT cover CSS/visual changes. Must visually verify:
- [ ] State-based tile coloring (blue=executable, red=failed, orange=handoff, purple=input-waiting, black=blocked, green=step-done)
- [ ] Mobile layout (buttons left-aligned, min-height 44px)
- [ ] Virtual scroll in LogViewer (smooth scrolling with 5000+ log entries)
- [ ] Tree layout (2-row format, detail panel on right, responsive at 480px)
- [ ] Header buttons (cs, dr, /commit, /sync-deps with state colors)
- [ ] Notification toast (left-top, 2s dedup, 10s auto-dismiss)
- [ ] Reconnection banner (red, exponential backoff)
- [ ] Tab management (× close, Close Done/Fail button)

### Key frontend changes to verify against HANDOFF

| HANDOFF Section | Source file | What to check |
|-----------------|------------|---------------|
| フロントエンド状態管理 | useExecution.js | useReducer with atomic dispatch |
| RAFログバッチング | useExecution.js | requestAnimationFrame batching in addLog |
| Single-pass useMemo | App.jsx | Single iteration over executions for all derived data |
| TreeNode最適化 | TreeView.jsx | Running node local interval, memo |
| 仮想スクロール | LogViewer.jsx | LINE_HEIGHT=20px, OVERSCAN=20, viewport calculation |
| 状態別カラーリング | TreeView.jsx, main.css | Priority order: input > failed > handoff > step-done > executable > blocked |

## Post-Recovery Verification Checklist

After all 3 features complete, verify against this plan:

### Structural checks
- [ ] F733 tool exists at `tools/session-extractor/` and runs successfully
- [ ] No unintended files restored (especially usageService.js)
- [ ] package.json files have test scripts and devDependencies
- [ ] vite.config.js has test config (environment: jsdom, setupFiles)
- [ ] settings.json deny/ask rules intact

### Test checks
- [ ] Backend: 132/132 pass (`cd src/tools/node/feature-dashboard/backend && npx vitest run`)
- [ ] Frontend: 224/224 pass (`cd src/tools/node/feature-dashboard/frontend && npx vitest run`)

### HANDOFF alignment
- [ ] All constants in HANDOFF "設定定数" table match source values
- [ ] All WebSocket events in HANDOFF match actual implementation
- [ ] All API endpoints in HANDOFF match actual routes
- [ ] Phase display shows `P{N}/{total}` format (today's addition)
- [ ] TreeView sorts by ID ascending (today's addition)

### Behavioral checks (manual)
- [ ] Dashboard starts: `npm start` → backend(3001) + frontend(5173)
- [ ] Feature list loads in TreeView
- [ ] Tile tap triggers fc/fl/run chain
- [ ] State coloring works (at least executable=blue visible)
- [ ] Log viewer displays logs for running execution

### What NOT to change
- Do NOT modify test files (they are the spec)
- Do NOT restructure the architecture (restore, don't redesign)
- Do NOT add new features during recovery
- Do NOT change HANDOFF.md beyond what was already updated today (Phase display)
- Do NOT remove the session-extractor tool after use (may be useful again)

## F736: Non-Dashboard Recovery - Procedure

### Scope
Use F733 extraction tool with broader file path patterns to recover non-dashboard changes.

### Priority Order

#### 1. HIGH: .claude/ workflow files (31 files)
These directly affect fc/fl/run quality. Recover first.

```
.claude/agents/ac-tester.md, ac-validator.md, com-auditor.md, doc-reviewer.md,
  eratw-reader.md, feasibility-checker.md, feature-reviewer.md, feature-validator.md,
  planning-validator.md, reference-checker.md, tech-investigator.md, wbs-generator.md
.claude/commands/audit.md, complete-feature.md, fc.md, kojo-init.md, next.md, plan.md, sync-deps.md
.claude/skills/fl-workflow/SKILL.md, PHASE-1..7, POST-LOOP.md
.claude/skills/run-workflow/SKILL.md, PHASE-1..8, PHASE-10.md
.claude/skills/feature-quality/SKILL.md, ENGINE.md, INFRA.md
.claude/skills/finalizer/SKILL.md
.claude/skills/testing/SKILL.md
.claude/hooks/statusline.ps1
```

**Validation**: Run `/fl` on a test feature and verify workflow executes correctly.

#### 2. MEDIUM: KojoComparer (5 files)
```
tools/KojoComparer/BatchExecutor.cs
tools/KojoComparer/BatchProcessor.cs
tools/KojoComparer/FileDiscovery.cs
tools/KojoComparer/Program.cs
tools/KojoComparer.Tests/BatchProcessorTests.cs
```

**Validation**: `dotnet build tools/KojoComparer/` + `dotnet test tools/KojoComparer.Tests/`

#### 3. LOW: Design docs and misc (28 files)
```
docs/architecture/*.md (26 files)
src/Era.Core.Tests/TrainingIntegrationTests.cs
pm/features/feature-703.md, feature-709.md
```

**Validation**: Visual review only. These are reference documents.

### Recovery approach for .claude/ files
Unlike dashboard files which have tests as spec, .claude/ files are markdown with no automated validation.
Recovery must rely entirely on F733 extraction. Manual review of each file diff is recommended before applying.

### Already fixed (do NOT include in F736)
- `variable_sizes.yaml` → Fixed manually (EXP:200, EXPNAME:200, TCVAR:600)
- `CLAUDE.md` → Committed today, no loss
- `.claude/settings.json` → Modified in current session, deny rules added
- `pm/index-features.md` → Committed today, no loss
- `pm/features/feature-729.md`, `feature-730.md` → Committed today, no loss
