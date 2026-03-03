# Feature Dashboard

A browser-based dashboard for managing Claude Code feature workflows with automatic chain execution.

> **For Claude (orchestrator)**: This file is both documentation and a change checklist.
> When modifying dashboard code, follow the **Orchestrator Workflow** below.
>
> **Source of truth**: `C:\Era\devkit\src\tools\node\feature-dashboard\` (inside devkit repo).
> `C:\Era\dashboard\` is a **separate git repo** that tracks the same code independently — do NOT edit files there.
> PM2 runs from the devkit path. All file creation and edits MUST target `C:\Era\devkit\src\tools\node\feature-dashboard\`.

### Orchestrator Workflow

1. **Identify affected files**: Use the Adding Features table to determine which files to modify
2. **Implement**
3. **Verify**: Start with `npm start` and confirm behavior in browser
4. **Test**: Run `npm test` (backend + frontend)
5. **Update coverage & mutation scores**: Update figures in the Reference section
6. **Update HANDOFF.md**: Update affected sections (API Endpoints, WebSocket Events, File Structure, Design Decisions, etc.)
7. **Commit**

> **WARNING: Before `pm2 restart` or `dr`**: Check for running Claude executions via
> `GET /api/health` → `claude.runningCount`. Restarting kills all child PTY processes
> (active `/run`, `/fl`, `/fc` sessions). If executions are running, either wait for
> completion or warn the user before restarting.
>
> **NEVER use `pm2 delete all` or restart the proxy.** The proxy carries the active Claude Code
> session — killing it severs the conversation with no recovery. Use `pm2 restart dashboard-backend` only.

### Report Format

Report to user on completion (in Japanese):

- **Scope**: FE / BE / Both
- **DR (Dashboard Restart)**: Required / Not required
- **New API/WS events**: List if any (recommended)

---

## Quick Reference

### Commands

```bash
# Start
cd tools/feature-dashboard && npm start    # backend:3001 + frontend:5173

# Test
npm test                                    # both backend & frontend
npm run test:mutation                       # backend mutation testing

# Restart (from dashboard UI)
dr button                                   # pm2 restart all
```

### Key Timeouts

| Setting | Value | Purpose |
|---------|------:|---------|
| Rate limit cache | dynamic | CCS profile-level usage cache (weekly/session/sonnet). See [details below](#rate-limit-cache-details) |
| Rate limit polling | 5min | Periodic background capture interval |
| Rate limit capture | 20s | Overall timeout for node-pty capture (typical: 7-10s via `/usage` command) |
| Stall check interval | 30s | Check for stall (worst-case detection ≤90s) |
| Stall detection | 60s | Mark execution as stalled |
| Execution TTL | 1h | Keep completed executions in memory |
| Stuck cleanup | 2h | Force-terminate unresponsive executions |
| Pending handoff timeout (y/n) | 10s | Fallback terminal handoff if result event never arrives for y/n prompts |
| AskUserQuestion | kill+resume | Process killed on tool_use detection; browser answer resumes via `--resume` |
| Account limit (429) | auto-retry | 429 detection + auto-recovery (profile switch / timed retry). See [details below](#account-limit-429-details) |
| Auto-switch (≥80%) | proactive | Proactive profile switch at ≥80% usage. See [details below](#auto-switch-details) |
| Context retry | 3x (5s delay) | Retry on **conversation context** exhaustion (error_max_turns, max_tokens, prompt too long, success+is_error **only when `!accountLimitHit`**, exit code 3 with null subtype). Counter: `contextRetryCount` (independent from FL). Blocked by `accountLimitHit`. On exhaustion: email subject `context-limit 3/3` |
| FL auto-retry | 3x (5s delay) | Retry FL on non-context failure (non-zero exit) or re-run request (text pattern). Counter: `retryCount` (independent from context). Blocked by `accountLimitHit` and `isContextExhausted`. On exhaustion: `fl-retry-exhausted` WS event + email subject `fl-retry 3/3` |
| FL incomplete retry | 3x (5s delay) | Retry FL when exit 0 + `subtype=success` but feature status didn't advance (still `[PROPOSED]`, not `[REVIEWED]`). Detects context/max_turns exhaustion mid-work where CLI reports success but FL didn't finish. Uses `fileWatcher.statusCache` for status check. Shares `retryCount` with FL auto-retry. Skips when status is `[BLOCKED]` (legitimate). WS event: `chain-retry` with `retryType: 'incomplete'`. On exhaustion: falls through to email notification |
| Stale waiter → Auto-DR | 5min + 10min | Chain waiters older than `CHAIN_WAITER_TIMEOUT_MS` (5min) are cleaned by `_cleanupOldExecutions()` (runs every 10min). On cleanup, `onExecutionComplete()` is called to trigger deferred Auto-DR re-check |
| Tmp cleanup interval | 6h | Purge old dashboard debug/daily logs (debug-*.log: 3 days, daily logs: 7 days) |
| Insights capture | ~2min | `/insights` via node-pty ConPTY. Completion: dual detection (report.html mtime change + PTY `"report is ready"` pattern). Emails HTML report via `emailService.sendHtml()`. Scheduler: cron-style `setTimeout` (Monday 07:00 JST). API: `POST /api/insights/capture`, `GET /api/insights/status` |

Full config: `backend/src/config.js`

#### Rate Limit Cache Details

Tracks CCS **profile-level** usage (weekly/session/sonnet — API quota limits, NOT conversation context).

- **expiresAt**: `min(weekly reset, session reset, sonnet reset, 1 week)`
- **refreshAt**: Adaptive 5min–2h based on activity and max percent
  - Idle (no running/queued executions): 2h
  - Active ≥90%: 5min
  - Active 80–89%: 10min
  - Active <80%: 30min
- **Session burn rate prediction**: When session elapsed ≥30min, percent ≥5%, and <100%, projects usage to end of 5h window
  - Projected >150%: 5min refresh
  - Projected >100%: 10min refresh (weekly unchanged)
- **Triggers**: On-start `capture()`, on-completion `capture({ forceRefresh: true })`, on-FE-refresh (`/api/health?refresh=1`)
- **Persistence**: `/usage` data preserves all types; capture failure preserves cache. Persisted to `_out/tmp/dashboard/ratelimit-cache.json`

#### Account Limit (429) Details

**Detection** (3 layers):
1. stderr regex: `rate_limit_error` / `hit your limit` / `exceed your account's rate limit`
2. Non-JSON stdout regex (same patterns)
3. Two-pass debug log tail scan — last 16KB of `--debug-file` for `rate_limit_error`/`429` (16KB needed: CLI writes hooks/telemetry after 429, pushing error >4KB from EOF)

**Behavior**: Sets `accountLimitHit` → blocks context retry and FL retry

**Recovery** (all executions, chain and non-chain):
1. Try auto-switch to safe profile → immediate retry after 5s
2. If no safe profile → timed retry at earliest `resetsAt` + 1min buffer. Sets `_rateLimitPaused` to block queue

**Session resume**: When failed execution has `sessionId`, retry uses `claude -p "continue" --resume <sessionId>` (preserves context); falls back to fresh `executeCommand()` when no sessionId

**Events**:
- WS: `account-limit` → `rate-limit-waiting` → `rate-limit-retry` or `rate-limit-exhausted`. `rate-limit-retry` includes `resumed: true/false`
- Email: `account-limit` (with retry schedule) → `rate-limit-recovered` or `rate-limit-exhausted`

#### Auto-Switch Details

When active profile reaches ≥80% on any rate limit type (weekly/session/sonnet):

- `_checkAutoSwitch` fires `onAutoSwitch(safeProfile)` → runs `ccs auth default "<safeProfile>"`
- **Trigger**: Every `capture()` completion
- **Purpose**: Prevent 429 before it happens
- **Config**: `AUTO_SWITCH_THRESHOLD` in `config.js`
- **WS event**: `auto-switch`

### API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/execution/{fc,fl,run}` | POST | Execute command |
| `/api/execution/terminal` | POST | Open terminal |
| `/api/execution/shell` | POST | Run cs, dr, upd |
| `/api/execution/slash` | POST | Run /commit, /sync-deps |
| `/api/execution/debug` | POST | Run arbitrary prompt (requires `.debug-enabled` file gate, 30-min TTL). Body: `{ "prompt": "..." }` |
| `/api/execution/:id/resume/{browser,terminal}` | POST | Resume session |
| `/api/execution/:id/answer` | POST | Answer input prompt in browser (y/n or AskUserQuestion option) |
| `/api/execution/:id` | GET | Execution status |
| `/api/execution/:id/logs` | GET | Execution logs (with offset) |
| `/api/execution/:id` | DELETE | Stop execution |
| `/api/execution` | GET | List all executions |
| `/api/ratelimit/:profile` | POST | Manual rate limit cache injection |
| `/api/execution/queue` | GET | Queue status |
| `/api/execution/queue/clear` | POST | Clear queued items |
| `/api/features` | GET | List features |
| `/api/features/:id` | GET | Feature detail |
| `/api/health` | GET | Health check |
| `/api/insights/capture` | POST | Trigger `/insights` capture (fire-and-forget). Body: `{ sendEmail: bool }` (default true). Returns 409 if already running |
| `/api/insights/status` | GET | Check capture status: `{ running, lastResult }` |

### WebSocket Events

| Event | Direction | Content |
|-------|-----------|---------|
| `log` | S→C (sub) | Log entry |
| `state` | S→C (sub) | Execution state (phase, contextPercent, isStalled, tokenUsage) |
| `status` | S→C (sub) | Completion/failure notification |
| `handoff` | S→C (sub) | Terminal handoff notification |
| `input-required` | S→C (sub) | AskUserQuestion detected |
| `input-wait` | S→C (sub) | y/n pattern detected |
| `stalled` | S→C (sub) | No output for >60s |
| `chain-progress` | S→C (all) | Chain next step started |
| `chain-retry` | S→C (all) | Auto-retry triggered (retryType: 'context' or 'fl') |
| `fl-retry-exhausted` | S→C (all) | FL retry counter maxed out, manual re-run needed |
| `rate-limit-waiting` | S→C (all) | Rate limit retry scheduled (retryAt, delayMs) |
| `rate-limit-retry` | S→C (all) | Rate limit retry triggered (oldExecutionId, newExecutionId) |
| `rate-limit-exhausted` | S→C (all) | Rate limit retry failed, manual re-run needed |
| `auto-switch` | S→C (all) | CCS profile auto-switched (safeProfile) |
| `chain-blocked` | S→C (all) | Chain blocked by pending deps |
| `features-updated` | S→C (all) | Feature file changed |
| `status-changed` | S→C (all) | Feature status changed (e.g., [DRAFT]→[PROPOSED]) |
| `queue-updated` | S→C (all) | Queue state changed |
| `execution-started` | S→C (all) | New execution started (API-spawned slash/debug). FE auto-subscribes |
| `upd-complete` | S→C (all) | CCS update completed |
| `shell-complete` | S→C (all) | Shell command (cs/dr/upd) completed |
| `auto-dr` | S→C (all) | Backend auto-restarting (file change detected) |
| `auto-dr-pending` | S→C (all) | Backend restart deferred (executions/chain active) |
| `subscribe` | C→S | Subscribe to execution logs |
| `unsubscribe` | C→S | Unsubscribe from execution logs |

Direction: S→C (sub) = server to subscribed clients, S→C (all) = server to all clients, C→S = client to server.

---

## Architecture

```
Frontend (React+Vite :5173)  →  Backend (Express :3001)  →  claude.exe (spawn)
     ↕ WebSocket /ws                  ↕ spawn + pipe            stdio: ['ignore', 'pipe', 'pipe']
  Tile UI + Log viewer           stream-json parse            --output-format stream-json
  + Toast notifications          + Chain execution            --verbose
  + Input answer buttons         + answerInBrowser()

                                      ↓ (Browser Answer)
                                 claude -p "answer" --resume <sessionId>
                                 (or: wt.exe --resume via Terminal fallback)
```

### Chain Execution Flow

```
[DRAFT] → fc → [PROPOSED] → fl → [REVIEWED] → run → [DONE]
```

**Stop conditions**: Error, Handoff, [BLOCKED], User kill

### Input Handling (Browser-First)

Since `stdin: 'ignore'` prevents user input, the dashboard detects input prompts and shows interactive UI in the browser ExecutionPanel. Terminal handoff is a user-initiated fallback.

**Detection triggers**:
- **y/n patterns**: `(y/n)`, `[Y/n]`, `Continue?`, `Proceed?` → Yes/No buttons in ExecutionPanel
- **AskUserQuestion** tool_use → Clickable option buttons in ExecutionPanel
- Normal completion with output ending in `?` → Terminal handoff (only when not already waiting for input)

**Browser answer flow**:

```
1. Pattern/AskUserQuestion detected → broadcast input-wait / input-required WS event
2. FE shows answer buttons (Yes/No or option list) + "Terminal" fallback button
3a. User clicks answer → POST /api/execution/:id/answer { answer }
    → answerInBrowser() kills process, resumes with -p "answer" --resume <sessionId>
3b. User clicks "Terminal" → POST /api/execution/:id/resume/terminal
    → Opens wt.exe --resume (old behavior)
```

**y/n flow detail**: CLI emits `result` event (session saved) → streamParser cancels pendingHandoff timeout → process exits naturally via `_handleCompletion` → execution status = `completed` but `waitingForInput` remains `true` → FE shows Yes/No buttons on the completed execution → user clicks → `answerInBrowser` resumes session.

**AskUserQuestion flow detail**: StreamParser detects `AskUserQuestion` tool_use in assistant message → sets `execution._killedForAskUser = true` → kills process immediately (before CLI auto-responds with empty answer) → `_handleCompletion` guard preserves `inputRequired` and keeps status `running` → FE shows option buttons → user clicks → `answerInBrowser` resumes with `-p "selected option" --resume sessionId` → CLI receives answer as new user message in context. Guard also ignores buffered tool_result that arrives after kill. "Terminal" button is the manual fallback.

**Session preservation**: y/n text is saved to session JSONL (`result` event confirms this). AskUserQuestion tool_use IS saved (process is killed after tool_use but before tool_result, so the session has the pending tool call) — the answer is sent as a `-p` prompt on `--resume`, and Claude continues with the user's actual choice. Terminal "Resume" button still writes `resume-context.txt` for AskUserQuestion context display.

**Revert to terminal-first**: To restore the old immediate-handoff behavior, add `this.handoffToTerminal(execution, 'AskUserQuestion requires user input')` in the AskUserQuestion handler in `streamParser.js` (after `broadcastInputRequired`).

### Data Flow

```
User action → App.jsx handler → useExecution API call
                                        ↓
                              execution.js route
                                        ↓
                              claudeService.js spawn
                                        ↓
                              stream-json stdout
                                        ↓
                              streamParser.js (parse + state detection)
                                        ↓
                              logStreamer.broadcast (WS)
                                        ↓
                              App.jsx wsHandlers → useExecution dispatch → UI update
```

---

## Development Guide

### Adding Features

| Goal | Files to modify |
|------|-----------------|
| New command | `execution.js` (route) → `claudeService.js` (logic) |
| New shell command | `execution.js` allowedShell → `claudeService.js` runShellCommand |
| Stream event handling | `streamParser.js` handleStreamEvent → `claudeService.js` callbacks |
| UI component | `frontend/src/components/` → `App.jsx` import |
| WebSocket event | `claudeService.js` broadcast → `App.jsx` wsHandlers |
| Feature parsing | `featureParser.js` → `featureService.js` |
| Input detection pattern | `inputPatterns.js` INPUT_WAIT_PATTERNS |
| Styling | `frontend/src/styles/main.css` |

### File Structure

```
backend/
├── server.js                    # Express setup, service wiring
├── send-email.mjs               # CLI email sender (used by Claude Code)
├── email.config.example.json    # Email config template
├── src/
│   ├── config.js                # Constants (timeouts, proxy, CCS)
│   ├── routes/
│   │   ├── execution.js         # /api/execution/* endpoints
│   │   └── features.js          # /api/features/*
│   ├── services/
│   │   ├── claudeService.js     # claude.exe spawn, Auto Handoff, completion logic
│   │   ├── streamParser.js      # stream-json parsing, state detection (extracted from claudeService)
│   │   ├── chainExecutor.js     # Chain execution (fc→fl→run)
│   │   ├── ccsUtils.js          # CCS profile reading (config.yaml, auth list)
│   │   ├── ratelimitService.js  # Rate limit capture (node-pty + VtScreenBuffer)
│   │   ├── featureService.js    # feature-{ID}.md reading, pendingDeps
│   │   ├── fileWatcher.js       # chokidar watch, status change detection
│   │   ├── statusMailService.js # IMAP IDLE status mail (auto-reply to empty emails)
│   │   ├── emailService.js      # Email notification (handoff/completion)
│   │   ├── cleanupService.js     # Tmp file cleanup (debug logs, daily logs, artifacts)
│   │   ├── insightsService.js   # /insights PTY capture + email report (weekly scheduler)
│   │   ├── usageService.js      # CCS usage tracking (not exposed via API)
│   │   ├── inputPatterns.js     # Input wait patterns
│   │   ├── phaseUtils.js        # Phase detection utilities
│   │   └── validation.js        # Input validation
│   ├── parsers/
│   │   ├── featureParser.js     # Feature markdown parsing
│   │   └── indexParser.js       # index-features.md parsing
│   ├── websocket/
│   │   └── logStreamer.js       # WebSocket broadcast (sub/all)
│   └── utils/
│       ├── logger.js            # Logging (daily rotation, JST)
│       └── timeUtils.js         # Shared time utilities (nowJST)

frontend/
├── src/
│   ├── App.jsx                  # State management, wsHandlers, header UI
│   ├── hooks/
│   │   ├── useExecution.js      # Execution state reducer, API calls
│   │   ├── useFeatures.js       # Feature fetching
│   │   └── useWebSocket.js      # WS connection, reconnection
│   ├── components/
│   │   ├── TreeView.jsx         # Dependency tree, tile tap, phase collapse
│   │   ├── ExecutionPanel.jsx   # Log display, Resume/Stop buttons, tabs
│   │   ├── FeatureDetail.jsx    # Feature detail overlay
│   │   ├── LogViewer.jsx        # Log line display
│   │   ├── StatusBadge.jsx      # Status badge
│   │   ├── FeatureTile.jsx      # Feature tile component
│   │   ├── ProgressBar.jsx      # Progress bar component
│   │   ├── PhaseSection.jsx     # Phase section header
│   │   └── QueueIndicator.jsx   # Queue indicator
│   └── styles/
│       └── main.css             # All styles
```

### Testing Requirements

After code changes:

```bash
# Run tests
cd tools/feature-dashboard/backend && npm test
cd tools/feature-dashboard/frontend && npm test

# Check coverage
npx vitest run --coverage

# Mutation testing (after adding new tests)
npm run test:mutation
```

**Mutation testing interpretation**:
- **Killed**: Mutant detected by tests → good test
- **Survived**: Mutant escaped tests → test gap
- Target: 60%+ mutation score (covered scope)

### Security

All user input is whitelist-validated before passing to spawn:

| Function | Validation |
|----------|------------|
| `validateFeatureId()` | Numeric only (`/^\d+$/`) |
| `validateCommand()` | `fc`, `fl`, `run` only |
| `runShellCommand()` | `cs`, `dr`, `upd` only |
| `executeSlashCommand()` | `commit`, `sync-deps` only |
| `answerInBrowser()` | `sanitizeInput(answer, 1000)` — control chars stripped, 1000 char limit |

---

## Operations

### Platform Requirements

**Windows + Windows Terminal required**

| Requirement | Reason |
|-------------|--------|
| Windows 10/11 | `taskkill` command, path separators |
| Windows Terminal (`wt.exe`) | Auto Handoff, Resume features |
| Node.js 18+ | ES Modules, fs/promises |

Linux/macOS: Limited support. Process termination (`SIGTERM`) works, but Terminal integration (`wt.exe`) does not.

### Infrastructure

**pm2**:
```bash
pm2 start ecosystem.config.cjs    # proxy(8888) + backend(3001) + frontend(5173)
pm2 save                          # Persist
```

> **Caution**: `pm2 restart` kills all child processes including active Claude sessions.
> Always verify no executions are running: `curl --noproxy localhost http://localhost:3001/api/health | jq '.claude.runningCount'`
>
> **CRITICAL: NEVER `pm2 delete` or restart the proxy process.** The proxy (port 8888) handles the
> active Claude Code session's API connection. Killing it severs the current conversation immediately
> with no recovery. Use `pm2 restart dashboard-backend` (NOT `pm2 restart all` or `pm2 delete all`)
> when only the backend needs restarting. If a full restart is truly needed, warn the user first.

**CCS Profile**: Set via `CLAUDE_CONFIG_DIR` env var. Points to `~/.ccs/instances/{profile}/`.

**New profile setup**: Two types of shared resources — static config (manual symlinks) and dynamic context (CCS managed).

**(1) Static config symlinks** — `cmd.exe` `mklink /D` (NOT Git Bash `ln -s`, which silently copies on Windows):

```bat
:: Run in cmd.exe (not Git Bash) — replace {profile} with profile name
for %d in (agents commands plugins skills) do mklink /D C:\Users\siihe\.ccs\instances\{profile}\%d C:\Users\siihe\.ccs\shared\%d
mklink C:\Users\siihe\.ccs\instances\{profile}\settings.json C:\Users\siihe\.ccs\shared\settings.json
```

Required symlinks (5): `agents`, `commands`, `plugins`, `skills`, `settings.json` → `~/.ccs/shared/`.

**(2) CCS shared context** — session/continuity sharing across profiles:

```bash
# New profile: create with shared context
ccs auth create {profile} --share-context --deeper-continuity

# Existing profile: add to config.yaml accounts section
#   context_mode: shared
#   context_group: default
#   continuity_mode: deeper
```

CCS sync runs on `claude` launch (NOT on `ccs auth default`). On first launch after enabling shared:
- `projects/` (real dir) → merged into `~/.ccs/shared/context-groups/{group}/projects/` → replaced with symlink
- `session-env`, `file-history`, `shell-snapshots`, `todos` → symlinked to `~/.ccs/shared/context-groups/{group}/continuity/`

**Shared context architecture** (CCS v7.51.0, investigated 2026-02-28):

| Resource | Shared by CCS | Location when shared |
|----------|:---:|---|
| `projects/` (session JSONLs) | Yes | `shared/context-groups/{group}/projects/` |
| `session-env/` | Yes (deeper) | `shared/context-groups/{group}/continuity/session-env/` |
| `file-history/` | Yes (deeper) | `shared/context-groups/{group}/continuity/file-history/` |
| `shell-snapshots/` | Yes (deeper) | `shared/context-groups/{group}/continuity/shell-snapshots/` |
| `todos/` | Yes (deeper) | `shared/context-groups/{group}/continuity/todos/` |
| `history.jsonl` | **No** | Per-profile (prompt input history, NOT session index) |
| `memory/` | **No** | Per-project inside `projects/{slug}/memory/` (shared with projects) |
| `.claude.json` | **No** | Per-profile (numStartups, feature flags) |
| `.credentials.json` | **No** | Per-profile (auth tokens) |
| `sessions-index.json` | **No** | Per-project inside `projects/{slug}/` (shared with projects) |

**Security check** (`shared-manager.js` L701-706): CCS validates project merge sources via `isSafeProjectsMergeSource()`. Only paths within `shared/context-groups/` or `instances/{self}/projects/` are allowed. Manual symlinks to `shared/projects/` trigger: `[!] Skipping unsafe project merge source outside CCS roots`.

**Migration gotchas** (isolated → shared):
- If `projects/` is renamed/absent before first shared launch, CCS creates empty shared dir (no merge). Must manually copy old sessions into `shared/context-groups/{group}/projects/{slug}/`.
- `sessions-index.json` contains `fullPath` with old profile-absolute paths. Delete it to force Claude Code to rebuild from JSONL scan.
- Claude Code may recreate `projects/` as a real dir during session if symlink target doesn't exist yet, blocking revert (`rmdir` fails on non-empty real dir).

**`/resume` behavior**: Lists sessions from `projects/{slug}/` JSONL scan (filtered by current working directory). Shared projects means sessions created in any profile are visible from all profiles in the same context group — confirmed working for apple ↔ google ↔ proton (2026-02-28). **Important**: `{slug}` is derived from the CWD (`slugify(cwd)`), so sessions created from a different CWD (e.g., `C:\Users\siihe` → `C--Users-siihe`) appear only in "show all projects" view, not in the default `/resume` list. This is Claude Code behavior, not a CCS limitation.

**Cross-profile session resume**: Rate limit retry (`_startRateLimitRetry`) switches profile via `ccs auth default`, then spawns `claude --resume <sessionId>`. Since all profiles' `projects/` JUNCTIONs resolve to the same shared directory, the sessionId is accessible regardless of which profile resumes it. Verified via code path: `_switchProfile()` → `_buildClaudeEnv()` reads new default → `CLAUDE_CONFIG_DIR` points to new profile → JUNCTION → same shared JSONL.

**Do NOT manually symlink `projects/`** — use CCS `--share-context` instead. Manual symlinks bypass the security check and break session loading.

**Diagnostic tool**: `~/.local/bin/ccs-symlink-test.bat` — dead man's switch for testing projects/ symlink changes. Auto-reverts after timeout. See script for usage.

**Proxy**: `127.0.0.1:8888` - Auto-injected via `_buildClaudeEnv()` as `HTTPS_PROXY`/`HTTP_PROXY`.

### Debugging

```bash
DASHBOARD_DEBUG=1 npm start    # Verbose logging (spawn args, non-JSON stream lines, Task tool depth)
```

**Debug endpoint activation** (separate from verbose logging):
```bash
# Claude: Write(_out/tmp/dashboard/.debug-enabled) with any content
# Manual: echo 1 > _out/tmp/dashboard/.debug-enabled
# TTL: 30 minutes from file mtime. Expired files auto-deleted on next request.
```

**Log and session files**:

All dashboard logs are under **project root** `{projectRoot}/_out/tmp/dashboard/` (NOT `tools/feature-dashboard/_out/tmp/`). Path is resolved by `logger.js` going up 5 levels from `backend/src/utils/`.

| Type | Location (relative to project root) | Description |
|------|--------------------------------------|-------------|
| `server-{date}.log` | `_out/tmp/dashboard/logs/` | Express server startup, routes |
| `websocket-{date}.log` | `_out/tmp/dashboard/logs/` | WebSocket connections, broadcasts |
| `claude-{date}.log` | `_out/tmp/dashboard/logs/` | ClaudeService: spawn, chain, retry, completion |
| `watcher-{date}.log` | `_out/tmp/dashboard/logs/` | FileWatcher: status changes, file updates |
| `debug-{execId}.log` | `_out/tmp/dashboard/` | Per-execution Claude CLI `--debug-file` output |
| `sessions.json` | `_out/tmp/dashboard/` | Persistent session ID map (survives execution TTL eviction, 7-day retention) |
| Session JSONL | `~/.ccs/instances/{profile}/projects/{project}/` | Claude conversation history (token usage, messages) |

Backend logs rotate automatically at midnight (UTC). Session JSONL files are managed by CCS.

### Restart Matrix

| Change | Backend restart | Frontend restart |
|--------|:---------------:|:----------------:|
| backend/*.js | Auto-DR | Not required |
| frontend/*.jsx | Not required | Auto (HMR) |
| HANDOFF.md only | Not required | Not required |

**Auto-DR (Auto Dashboard Restart)**: Backend source files (`src/**/*.js`, `server.js`, excludes `*.test.js`) are watched by chokidar. On change, if no Claude executions are running/queued and no chain waiters are pending, `pm2 restart dashboard-backend` is triggered automatically (2s debounce). If executions or chain waiters are active, restart is deferred until all complete. WS event `auto-dr` is broadcast before restart; `auto-dr-pending` is broadcast when restart is deferred (FE shows DR button pulse). Config: `AUTO_DR_DEBOUNCE_MS` in `config.js`.

**Manual restart methods**:
- Dashboard UI `dr` button (pm2 restart all)
- `pm2 restart dashboard-backend` / `pm2 restart dashboard-frontend`

**CAUTION**: Manual `dr` does NOT check for running executions. Before manual restart, confirm no Claude child processes are running (`tasklist | findstr claude`). Backend restart kills the Express server but orphans spawned `claude.exe` processes — they lose their parent and completion/handoff/email callbacks never fire. Auto-DR handles this automatically.

---

## Known Limitations

| Issue | Notes |
|-------|-------|
| Stale data on WS disconnect | `executionsRef` is frontend state; may be stale during disconnect. On reconnect, `fetchExecutions()` rehydrates executions + executionStates + featurePhases from backend API. Resume button still uses frontend ref (acceptable for personal tool). |
| fileWatcher depth:0 | Intentional (watch agents/ directory only) |
| Diamond dependencies | In TreeView, A→C, B→C shows C only under first-visited parent. Intentional: shows execution order (depth-first), not full DAG. Actual deps visible via pendingDeps. |
| CCS YAML parsing | Simple regex parsing of config.yaml. Assumes `default:` is top-level key. No anchor/multiline support. Sufficient for current use. |
| engine/ git monitoring | `/api/health` monitors both main repo and `engine/` for git dirty indicator. Removal tracked in `full-csharp-architecture.md` Phase 30 Task 7. |
| CLI 429 not in streams | Claude CLI writes `rate_limit_error` to `--debug-file` only, not to stdout/stderr stream-json. CLI emits `result { subtype: 'success', is_error: true }` for 429, same pattern as context exhaustion. Dashboard uses **two-pass debug log scan**: (1) immediate `readFileSync` at process `close` event, (2) deferred 500ms re-scan if immediate scan fails (Windows file lock / flush timing). Deferred detection cancels any in-flight context retry timer and routes to rate limit retry instead. If CLI changes to emit 429 in streams, the stderr/non-JSON detection will catch it first and the debug scan becomes redundant (harmless). |
| Context % with subagents | `cache_read_input_tokens` from Agent tool can exceed `contextWindow`, producing >100%. Capped at 100% in UI; raw values preserved in `tokenUsage`. Subagent assistant events are filtered by `taskDepth > 0` (incremented on `block.name === 'Agent'` tool_use, decremented on matching tool_result). |

### Windows-Specific Constraints

- `shell: false` required (shell:true breaks piping)
- `windowsHide: true` hides console window
- `stdio: ['pipe', 'pipe', 'pipe']` with immediate `proc.stdin.end()` — stdin must be closed to prevent CLI hang in `-p` mode
- `--verbose` must precede `--output-format stream-json`
- `taskkill /F /T /PID` kills entire process tree

### pm2 ForkMode `detached: true` Console Flash (KB5077181)

**Symptom**: `pm2 start/restart` briefly flashes a `C:\Windows\system32` console window.

**Root cause**: Windows 11 KB5077181 (2026-02-11, build 26200.7840) changed how `CREATE_NEW_PROCESS_GROUP` interacts with console-less processes. pm2's God daemon (itself detached, no console) spawns child processes with `detached: true`, which now triggers visible console allocation despite `windowsHide: true` (`CREATE_NO_WINDOW`).

**Fix**: Patch `node_modules/pm2/lib/God/ForkMode.js` line 98: `detached: true` → `detached: false`.

**Tradeoff**: Children share the daemon's process group. If the daemon crashes, children may terminate too. pm2 manages lifecycle individually, so practical impact is minimal.

**Re-apply after**: `npm update -g pm2` (overwrites the patch). Run:
```bash
node tools/feature-dashboard/patch-pm2.js && pm2 kill && pm2 start tools/feature-dashboard/ecosystem.config.cjs
```

### pm2 Restart Port Retention (`detached: false`)

**Symptom**: After Auto-DR or manual `pm2 restart`, old process retains port 3001. New process fails with `EADDRINUSE`. DR button does not light up (browser connects to stale process).

**Root cause**: `pm2 restart` sends SIGTERM to the old process and starts a new one **simultaneously**. On Windows, port release lags process death. The new process hits EADDRINUSE because the old one hasn't released port 3001 yet. See `docs/architecture/infrastructure/dr-restart-investigation.md` for the full investigation log.

**Fix (2026-03-03, VBScript delegation)**:

Auto-DR and DR button delegate restart to `restart-backend.vbs` → `restart-backend.cmd`:
1. `pm2 stop dashboard-backend` — kill old process cleanly
2. `ping -n 4` — wait ~3s for Windows to release port
3. `pm2 restart dashboard-backend` — start new process on free port

VBScript (`WScript.Shell.Run`) is used because it spawns cmd.exe in a **new process group**, independent of pm2's process tree. Direct `spawn({detached: true})` does NOT work due to the pm2 ForkMode `detached: false` patch — all pm2 children share the daemon's process group, so `pm2 stop` kills them all.

**Defense layers in `server.js`**:
1. **Graceful shutdown**: `server.closeAllConnections()` + 1500ms forced exit timeout in SIGINT/SIGTERM handlers
2. **EADDRINUSE polling**: On port conflict, poll with adaptive delay (500ms → 2s) without crashing. Never `process.exit(1)` on EADDRINUSE.
3. **Last-resort cleanup**: `cleanupPort(PORT)` after 10s of polling, kills whatever holds the port

`ecosystem.config.cjs`: `kill_timeout: 3000` + `exp_backoff_restart_delay: 200`.

### `-p --resume` File Writing (Resolved)

Previously assumed broken, but **works correctly in 2.1.0+**. Tested (2026-02-04):

```bash
claude -p "hello" --verbose --output-format stream-json  # → session_id取得
claude --resume {session_id} -p "write file" ...         # → Write tool成功
```

Root cause: v2.1.0 fixed "files and skills not being properly discovered when resuming sessions with `-c` or `--resume`". Enables orchestrator patterns (Claude spawning Claude via `-p --resume`).

---

## Reference

### Test Coverage (2026-02-14)

| Target | Stmts | Branch | Funcs | Lines | Tests | Goal |
|--------|------:|-------:|------:|------:|------:|:----:|
| Backend | 84.3% | 77.5% | 82.8% | 85.5% | 654 | 80%+ ✓ |
| Frontend | 73.7% | 68.8% | 72.5% | 76.6% | 231 | 75%+ ✓ |

**Backend module coverage**:

| Module | Lines | Tests | Notes |
|--------|------:|------:|-------|
| validation.js | 100% | - | Input validation |
| inputPatterns.js | 100% | - | Input wait patterns |
| phaseUtils.js | 100% | - | Phase detection |
| config.js | 100% | - | Configuration constants |
| timeUtils.js | 100% | 26 | Shared time utilities |
| chainExecutor.js | 100% | 35 | Chain execution |
| ccsUtils.js | 100% | 29 | CCS profile (execSync) |
| featureService.js | 100% | 30 | Feature aggregation |
| usageService.js | 100% | 45 | Usage tracking |
| streamParser.js | 99.4% | - | Stream-json parsing (tested via claudeService) |
| emailService.js | 98.1% | 13 | Email notification |
| statusMailService.js | 94.9% | 57 | IMAP IDLE status mail |
| parsers/ | 93.8% | 64 | Markdown parsing |
| logger.js | 80.5% | 37 | Logging |
| fileWatcher.js | 75.0% | 32 | File watching |
| claudeService.js | 74.3% | 180 | Main logic (delegates to streamParser) |

**Mutation score (Backend)**:

| Metric | Value | Notes |
|--------|------:|-------|
| Overall | 57.7% | killed / total |
| Covered scope | 70.0% | killed / covered |
| Killed | 2,452 | Detected mutants |
| Survived | 1,059 | Escaped mutants |

### Design Decisions

**Context splitting (TreeView.jsx)**: Split into `TreeCallbacksContext` (stable callbacks) and `TreeDataContext` (dynamic data). Callbacks rarely change, so TreeNode memo can skip re-renders when only data changes.

**Dynamic project path**: ExecutionPanel "Copy ID" button fetches `projectRoot` from `/api/health` to generate paths dynamically. Eliminates hardcoded path issues.

**Process termination safety**: `_killProcess()` checks for null pid to prevent taskkill execution with undefined pid.

**stdin handling (`proc.stdin.end()`)**: Claude CLI in `-p` mode hangs if stdin is an open pipe (waits for EOF before processing the prompt). Solution: `stdio: ['pipe', 'pipe', 'pipe']` with immediate `proc.stdin.end()` after spawn. This closes stdin, letting the CLI process the `-p` prompt immediately. AskUserQuestion uses kill+resume (not stdin), so closing stdin has no side effects.

**AskUserQuestion kill-on-detect**: StreamParser kills the process immediately on detecting `AskUserQuestion` tool_use to prevent the CLI from auto-responding with an empty answer. Guard `_killedForAskUser` ensures: (1) buffered tool_result doesn't clear `inputRequired`, (2) `_handleCompletion` preserves `running` status for browser UI. The session is saved with the pending tool call, so `--resume` continues correctly with the user's actual answer.

**Debug endpoint**: `POST /api/execution/debug` accepts arbitrary prompts. Gated by file-based activation: `_out/tmp/dashboard/.debug-enabled` must exist and be less than 30 minutes old. Claude activates by writing this file (`Write(_out/tmp/dashboard/.debug-enabled)` with any content); expired files are auto-deleted on next request. `DASHBOARD_DEBUG=1` env var now controls only verbose logging, not endpoint access. Essential for testing AskUserQuestion flow, stream parsing, and other behaviors without running real feature commands. Sends `execution-started` WebSocket event so the FE auto-subscribes and shows the execution tab.

**Security hardening**: Backend binds to `127.0.0.1` only (not `0.0.0.0`). CORS restricted to `localhost:5173` and `localhost:3001`. Debug endpoint defaults to closed; requires explicit file-based activation with 30-min TTL.

**Module extraction (claudeService.js)**: Extracted utilities and constants into separate modules:
- `streamParser.js`: Stream-json parsing, event handling, state detection (StreamParser class)
- `chainExecutor.js`: Chain execution logic (ChainExecutor class)
- `validation.js`: Input validation
- `inputPatterns.js`: Input wait patterns
- `phaseUtils.js`: Phase detection
- `ccsUtils.js`: CCS profile reading
- `config.js`: Configuration constants
- `timeUtils.js`: Shared time utilities (nowJST, format: `YYYY/MM/DD HH:mm:ss JST`)

Benefits: Single responsibility, clearer test targets, improved maintainability (1,600 → ~1,300 lines).

**Tile height consistency (TreeView.jsx)**: Placeholder design to unify tile heights:

| Element | Root tile (depth=0) | Child tile (depth>0) |
|---------|:-------------------:|:--------------------:|
| Left (RUN label) | 48px placeholder | None (left-aligned) |
| row2 (phase/context/elapsed) | Placeholder shown | None (left-aligned) |

Root tiles reserve space to prevent layout shift when execution starts. Child tiles prioritize compact display. Placeholders use `visibility: hidden` (hidden but space preserved).

**Email notification (emailService.js)**: Sends Gmail SMTP notifications on terminal handoff and execution completion. Config stored in `email.config.json` (gitignored) with `email.config.example.json` as template. DI: constructor accepts `configLoader` and `transportFactory` for testing. Fire-and-forget: errors logged but never block execution. Triggers: `_handoffToTerminal()` and `_handleCompletion()` in claudeService.js. Setup: copy `email.config.example.json` to `email.config.json`, set `enabled: true`, `user` (Gmail address), `pass` (Gmail app password).

**Status mail service (statusMailService.js)**: IMAP IDLE listener that monitors Gmail inbox for status requests. Trigger: self-sent empty email (no subject, no body). Response: auto-reply with dashboard status report (executions, rate limits, feature summary). IMAP config via `email.config.json` key `statusMail: { enabled, imapHost, imapPort, allowedSenders, reconnectDelayMs }`. Sender whitelist: self + configured `allowedSenders` (case-insensitive). Loop prevention: ignores `Re:` subjects. Reconnects on disconnect with configurable delay. DI: constructor accepts `imapClientFactory` and `transportFactory` for testing.

**Stream parser (streamParser.js)**: Extracted from claudeService.js for separation of concerns. Handles all stream-json output parsing: JSON line parsing, event type dispatch, phase/iteration detection, input wait pattern matching, AskUserQuestion detection, token usage calculation, and context percentage. Uses constructor DI for all callbacks (pushLog, broadcast, broadcastState, handoffToTerminal, handleCompletion). claudeService.js retains wrapper methods for backward compatibility.

#### Rate Limit Capture (`ratelimitService.js`)

Captures Claude Code's exact usage percentages via `/usage` slash command through `node-pty` (ConPTY) + `VtScreenBuffer` (minimal VT terminal emulator).

**Profile enumeration**: `ccs auth list` via `getCcsProfiles()` in `ccsUtils.js` — only `[OK]` profiles.

**Capture flow per profile**:
1. Spawn `claude` via `pty.spawn('cmd.exe', ['/c', 'claude'], { cols: 120, rows: 30, useConptyDll: true })` in headless ConPTY
2. Feed VT output into `VtScreenBuffer` for TUI detection
3. Detect TUI loaded (status bar `Context:N%`)
4. Wait 1.5s for TUI stabilization
5. Type `/usage`, wait 800ms (autocomplete menu settle), create clean `VtScreenBuffer`, send Enter
6. Detect `/usage` output completion (`Esc to cancel` or `Sonnet only` + `%used` pattern), wait 500ms for full render
7. Resolve with captured text (clean buffer preferred, fallback to main buffer)

**Parsing** (`_parseUsageOutput()`): Positional — finds section headers (`Current session`, `Current week (all models)`, `Sonnet only`), maps subsequent `(\d+)%[^\d%]{0,15}used` and `Resets (.+?)(\(|$)` to nearest preceding section.

**Infrastructure**:
- `useConptyDll: true` — bundled `conpty.dll` + `OpenConsole.exe` bypasses Windows Terminal's default terminal interception
- Kill: `pty.kill()` with `taskkill /F /T` fallback
- DI: constructor accepts `ptySpawn` for testing

**Cache**: Per-profile `Map<profileName, {data, timestamp, expiresAt}>`, dynamic TTL (see [Rate Limit Cache Details](#rate-limit-cache-details)). On `/usage` success: all three types stored. On failure: existing cache preserved if not expired. Persisted to `_out/tmp/dashboard/ratelimit-cache.json` (loaded on startup, saved on every update). Manual injection via `POST /api/ratelimit/:profile`.

**Triggers**: Startup (background) + 5min periodic polling + after each command completion (fire-and-forget).

**FE display**: Header shows `{profile} W:XX% S:XX%` with reset times (`↻W:` / `↻S:`) when percent > 75% (yellow ≥70%, red pulse ≥90%). Sonnet data captured but not displayed (used for auto-switch and rate limit retry).

**Helpers**: `getEarliestResetTime()` — earliest reset across all profiles (for timed retry). `getSafeProfile(excludeProfile)` — profile below 90% (for switch retry). Capture time: ~7-10s per profile.

**Auto-switch (`ratelimitService.js` + `server.js`)**: Proactive profile switching to prevent 429.
- `_checkAutoSwitch()` runs after every `capture()` completion
- When active profile reaches ≥`AUTO_SWITCH_THRESHOLD` (80%, in `config.js`) on any rate limit type and another profile is below threshold → fires `onAutoSwitch(safeProfile)`
- server.js callback validates `safeProfile` against `getCcsProfiles()` (command injection prevention) → `ccs auth default "<safeProfile>"`
- Next spawned claude.exe picks up new profile via `getCcsProfile()` → `CLAUDE_CONFIG_DIR`
- WS event: `auto-switch` with `safeProfile` field

#### Rate Limit Retry (`claudeService.js`)

When an execution hits 429, `_scheduleRateLimitRetry()` attempts recovery:

**Strategy 1 (immediate — profile switch)**:
- `rateLimitService.getSafeProfile(currentProfile)` finds alternative profile below 90%
- If found: `_switchProfile(safeProfile)` (`ccs auth default` with validation) → retry after 5s

**Strategy 2 (timed — wait for reset)**:
- `rateLimitService.getEarliestResetTime()` → schedule timer at `resetTime + RATE_LIMIT_RETRY_BUFFER_MS` (1min)
- Sets `_rateLimitPaused = true` → blocks `_dequeueNext()` to prevent cascading 429
- On timer: `_executeRateLimitRetry()` re-captures rate limits, checks safe (< `RATE_LIMIT_SAFE_THRESHOLD` 95%), retries or gives up

**Session resume**: `_startRateLimitRetry()` checks for `sessionId`:
- With sessionId: `claude -p "continue" --resume <sessionId>` (preserves context — avoids re-reading files)
- Without sessionId: falls back to fresh `executeCommand()`
- Resume creates new execution with original command name (not `resume:` prefix) for chain compatibility

**Cleanup**: `killExecution()` and `killAllRunning()` clear retry timer and pause flag. `getQueueStatus()` exposes `rateLimitWaiting`.

**Events**: WS `rate-limit-retry` includes `resumed: true/false`. Email: `account-limit` → `rate-limit-recovered` or `rate-limit-exhausted`.

**Auto-DR (`server.js`)**: Watches backend source files (`src/**/*.js`, `server.js`) via chokidar with 2s debounce.
- On change: checks `claudeService.getQueueStatus()` → if idle, `pm2 restart dashboard-backend`
- If executions active: sets `pendingRestart` flag
- `onExecutionComplete` callback (after `_handleCompletion` **and** stale chain waiter cleanup) triggers deferred restart when queue becomes idle
- Broadcasts `auto-dr` WS event 500ms before restart
- Excludes: `*.test.js`, `node_modules/`
- Graceful shutdown: `server.closeAllConnections()` → `server.close()` → 1500ms forced exit fallback
- Startup: `cleanupPort(PORT)` + EADDRINUSE retry safety net

**FL incomplete termination detection (`claudeService.js`)**: Detects context/max_turns exhaustion mid-work where CLI reports success but FL didn't finish.
- **Condition**: Exit 0 + `resultSubtype === 'success'` but status still `[PROPOSED]` (not `[REVIEWED]`, via `fileWatcher.statusCache`)
- **Action**: Auto-retries FL as new execution (up to `MAX_FL_RETRIES`, shared counter with FL auto-retry)
- **Skipped**: `[BLOCKED]` (legitimate) or `fileWatcher` null (fallback: register waiter normally)
- **WS**: `chain-retry` with `retryType: 'incomplete'`. On exhaustion: falls through to email notification

#### Browser-First Input Handling (`streamParser.js` + `claudeService.js`)

Both y/n patterns and AskUserQuestion are handled browser-first. When detected, `pendingHandoff` is set with a timeout, and WS events notify FE.

**y/n flow**:
- `result` event arrives → `pendingHandoff` **cancelled** (not triggered) → process exits naturally
- `_handleCompletion` completes with `waitingForInput: true` → FE shows Yes/No buttons on completed execution
- Timeout: `PENDING_HANDOFF_TIMEOUT_MS` (10s)

**AskUserQuestion flow**:
- StreamParser detects tool_use → kills process (`_killedForAskUser` guard) → FE shows option buttons while `running`
- User answer → kill + `--resume` with answer as `-p` prompt
- No auto-timeout; "Terminal" button is the manual fallback

**Common**:
- "Terminal" fallback button always available
- `answerInBrowser(executionId, answer)` kills stuck process, resumes with `-p "answer" --resume <sessionId>`, transfers chain/phase state
- `_handleCompletion` skips `endsWithQuestion` handoff when `waitingForInput` or `inputRequired` is set

**Revert**: See "Input Handling (Browser-First)" section above for 2-line revert instructions.

**Tmp cleanup (`cleanupService.js`)**: Purges old files from `_out/tmp/dashboard/` to prevent unbounded disk growth.
- **Targets**: `debug-*.log` (3d), `*-YYYY-MM-DD.log` (7d), `term-*.debug.log` (7d), `exec-*.jsonl/sh` (7d)
- **Schedule**: Initial purge on startup + every 6h (`TMP_CLEANUP_INTERVAL_MS`)
- **Protected** (never deleted): `ratelimit-cache.json`, `sessions.json`, `latest` symlink, `logs/` dir
- Regex pattern matching + `fs.stat` mtime. Errors logged, never crash. Logs summary (count + MB freed)

**Browser refresh resilience**: State is split into backend-authoritative and frontend-only.
- **Backend stores**: Execution objects (process, logs, phase, context%, tokenUsage — in-memory Map, 1h TTL), shell command results (`shellStates` Map, exposed via `/api/health`), rate limit cache (persisted to disk)
- **Frontend stores**: UI interaction state (selected tab, panel visibility, notifications)
- **On F5**:
  1. `fetchExecutions()` rehydrates `executions` + `executionStates` + `featurePhases` from `GET /api/execution` + logs API
  2. `checkHealth(true)` restores `shellStates` button colors + triggers `capture({ forceRefresh: true })` via `?refresh=1`
  3. Health poll at 8s/30s picks up fresh rate limit data
- WS `shell-complete` and `state` events continue real-time updates after rehydration

#### Three Limit Concepts

The word "session" appears in two unrelated contexts:

| Concept | What it means | Retryable | Detection |
|---------|--------------|-----------|-----------|
| **Conversation context** exhaustion | Single `-p` session's token window is full | Yes (3x, fresh session) | `error_max_turns`, `max_tokens`, `promptTooLong`, `exitCode≠0 && subtype=success && !accountLimitHit`, `exitCode===3 && !subtype` |
| **CCS session** rate limit | Profile's per-session API quota (hours) | Yes (all executions) | `accountLimitHit` flag (429 `rate_limit_error`) |
| **CCS weekly** rate limit | Profile's weekly API quota | Yes (all executions) | Same `accountLimitHit` flag |

**429 detection** (3 layers):
1. stderr regex: `/hit your limit|rate_limit_error|exceed your (organization|account)('s|s)? rate limit/i`
2. Non-JSON stdout regex (same patterns)
3. Two-pass debug log tail scan — last 16KB of `--debug-file` for `rate_limit_error` or `429.*rate.?limit`
   - 16KB needed: CLI writes hooks/telemetry/session-save after 429, pushing error >4KB from EOF
   - Pass 1: immediate scan at process `close` event
   - Pass 2: deferred 500ms re-scan if Pass 1 missed (Windows file lock/flush timing)

**Deferred detection behavior**:
- Cancels in-flight `_contextRetryTimer` (RETRY_DELAY_MS=5s > 500ms, timer hasn't fired yet) → routes to `_scheduleRateLimitRetry()`
- Errors logged to `claudeLog.debug` (previously silent `catch{}`)

**Retry guards**:
- `needsContextRetry` and `flWantsRetry` block all retries when `accountLimitHit` is true
- `exitCode≠0 && subtype=success` context heuristic guarded by `!accountLimitHit` — debug log scan promoting 429 prevents false context retry

**Rate limit retry** (all executions — chain gate removed):
- `_scheduleRateLimitRetry()`: (1) profile switch (immediate), or (2) timed retry at `resetsAt` + 1min
- Sets `_rateLimitPaused` to block queue dequeue
- Re-captures before retry; gives up if still ≥95%

**Email subjects**: `account-limit` (429), `rate-limit-recovered`, `rate-limit-exhausted`, `context-limit N/3`, `fl-retry N/3`. Result derived from chain history's last entry (single source from claudeService).

**Session ID persistence (`claudeService.js`)**: Session IDs persisted to `_out/tmp/dashboard/sessions.json` on completion and handoff.
- Loaded on startup, pruned (entries >7 days removed on each save)
- `resumeInBrowser()`/`resumeInTerminal()` falls back to `_lookupSessionId()` when execution evicted from in-memory Map (1h TTL)
- Enables resume hours/days after execution without increasing `EXECUTION_TTL_MS`
- Protected from cleanup (excluded by pattern — only `debug-*.log` and `*-YYYY-MM-DD.log` targeted)

**Context percent cap (streamParser.js)**: `contextPercent` is capped at 100%. Subagent-heavy executions (`/run` with many Task tool invocations) can report cumulative `cache_read_input_tokens` that exceed the single-session context window, producing misleading values like 1015% or 4383%. The cap prevents UI confusion while preserving the raw token counts in `tokenUsage` for debugging.

---

## Future Work

### Repository Separation

The dashboard has grown into a standalone web application (650+ backend tests, 230+ frontend tests, 12+ service modules) and should eventually be extracted into its own repository. The main coupling points are:

- **`projectRoot` hardcoding**: `featureParser`, `fileWatcher`, and `logger.js` resolve paths relative to the parent project. Replace with a `PROJECT_ROOT` environment variable in `config.js`.
- **`_out/tmp/dashboard/` log output**: `logger.js` resolves the project root by traversing 5 directory levels. Should use `PROJECT_ROOT` instead.
- **`patch-pm2.js` location**: Currently at project root `tools/feature-dashboard/`. Move into the dashboard repo.

**Preparation steps** (can be done incrementally before separation):
1. Add `PROJECT_ROOT` env var to `config.js`, default to current path resolution for backward compatibility
2. Replace all hardcoded path traversals with the config value
3. Verify all tests pass with an explicit `PROJECT_ROOT` setting

**Trigger**: Extract when a second project needs the dashboard, or when the dashboard's commit volume justifies independent history.

### Terminal Handoff Reduction

Since `-p --resume` works in v2.1.0+, many terminal handoff scenarios can be handled in-browser via `resumeInBrowser()`. This eliminates the need for `wt.exe` in most cases.

**Completed**:
- [x] Rate limit (429): `_startRateLimitRetry()` uses `--resume` when sessionId available, preserving conversation context
- [x] y/n prompts: Yes/No buttons in ExecutionPanel → `answerInBrowser(id, 'y'|'n')` → `-p --resume`
- [x] AskUserQuestion: Clickable option buttons in ExecutionPanel → `answerInBrowser(id, selectedOption)` → `-p --resume`
- [x] Terminal handoff is opt-in only ("Terminal" fallback button in input panels)

**Known limitation**: AskUserQuestion incomplete tool_use turn is NOT saved to session JSONL. The browser answer is sent as a `-p` prompt on `--resume`, relying on Claude to infer context from conversation history. If this proves unreliable, revert AskUserQuestion to immediate terminal handoff (see revert instructions in "Input Handling" section).

**Remaining**:
- [ ] Verify AskUserQuestion browser-answer reliability across multi-option and multiSelect scenarios
