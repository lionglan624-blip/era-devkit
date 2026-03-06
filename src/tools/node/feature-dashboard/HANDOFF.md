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
5. **Update coverage & mutation scores**: Update figures in [OPS.md](OPS.md) Test Coverage section
6. **Update docs**: Update affected sections in HANDOFF.md, [INTERNALS.md](INTERNALS.md), or [OPS.md](OPS.md)
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
cd src/tools/node/feature-dashboard && npm start    # backend:3001 + frontend:5173

# Test
npm test                                    # both backend & frontend
npm run test:mutation                       # backend mutation testing

# Restart (from dashboard UI)
dr button                                   # pm2 restart all
```

### Key Timeouts

| Setting | Value | Purpose |
|---------|------:|---------|
| Rate limit cache | dynamic | CCS profile-level usage cache (weekly/session/sonnet). See [INTERNALS.md](INTERNALS.md) Rate Limit Cache Details |
| Rate limit polling | 5min | Periodic background capture interval |
| Rate limit capture | 20s | Overall timeout for node-pty capture (typical: 7-10s via `/usage` command) |
| Stall check interval | 30s | Check for stall (worst-case detection ≤90s) |
| Stall detection | 60s | Mark execution as stalled |
| Execution TTL | 1h | Keep completed executions in memory |
| Stuck cleanup | 2h | Force-terminate unresponsive executions |
| Pending handoff timeout (y/n) | 10s | Fallback terminal handoff if result event never arrives for y/n prompts |
| AskUserQuestion | kill+resume | Process killed on tool_use detection; browser answer resumes via `--resume` |
| Account limit (429) | auto-retry (queue) | 429 detection + auto-recovery. Multiple concurrent 429s queued and drained sequentially (profile switch / timed retry). See [INTERNALS.md](INTERNALS.md) Account Limit (429) Details |
| Auto-switch (≥80%) | proactive | Proactive profile switch at ≥80% usage. See [INTERNALS.md](INTERNALS.md) Auto-Switch Details |
| Context retry | 3x (5s delay) | Retry on **conversation context** exhaustion (error_max_turns, max_tokens, prompt too long, success+is_error **only when `!accountLimitHit`**, exit code 3 with null subtype). Counter: `contextRetryCount` (independent from FL). Blocked by `accountLimitHit`. On exhaustion: email subject `context-limit 3/3` |
| FL auto-retry | 3x (5s delay) | Retry FL on non-context failure (non-zero exit) or re-run request (text pattern). Counter: `retryCount` (independent from context). Blocked by `accountLimitHit` and `isContextExhausted`. On exhaustion: `fl-retry-exhausted` WS event + email subject `fl-retry 3/3` |
| FL incomplete retry | 3x (5s delay) | Retry FL when exit 0 + `subtype=success` but feature status didn't advance (still `[PROPOSED]`, not `[REVIEWED]`). Detects context/max_turns exhaustion mid-work where CLI reports success but FL didn't finish. Uses `fileWatcher.statusCache` for status check. Shares `retryCount` with FL auto-retry. Skips when status is `[BLOCKED]` (legitimate). WS event: `chain-retry` with `retryType: 'incomplete'`. On exhaustion: falls through to email notification |
| Stale waiter → Auto-DR | 5min + 10min | Chain waiters older than `CHAIN_WAITER_TIMEOUT_MS` (5min) are cleaned by `_cleanupOldExecutions()` (runs every 10min). On cleanup, `onExecutionComplete()` is called to trigger deferred Auto-DR re-check |
| Tmp cleanup interval | 6h | Purge old dashboard debug/daily logs (debug-*.log: 3 days, daily logs: 7 days) |
| Insights capture | ~2min | `/insights` via node-pty ConPTY. Completion: dual detection (report.html mtime change + PTY `"report is ready"` pattern). Emails HTML report via `emailService.sendHtml()`. Scheduler: cron-style `setTimeout` (Monday 07:00 JST). API: `POST /api/insights/capture`, `GET /api/insights/status` |

Full config: `backend/src/config.js`

### API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/execution/{fc,fl,run,imp}` | POST | Execute command |
| `/api/execution/terminal` | POST | Open terminal |
| `/api/execution/shell` | POST | Run cs, dr, upd |
| `/api/execution/slash` | POST | Run /commit, /sync-deps |
| `/api/execution/debug` | POST | Run arbitrary prompt (requires `.debug-enabled` file gate, 30-min TTL). Body: `{ "prompt": "..." }` |
| `/api/execution/:id/resume/{browser,terminal}` | POST | Resume session |
| `/api/execution/:id/answer` | POST | Answer input prompt in browser (y/n or AskUserQuestion option) |
| `/api/execution/:id` | GET | Execution status |
| `/api/execution/:id/logs` | GET | Execution logs (with offset) |
| `/api/execution/:id` | DELETE | Stop execution |
| `/api/execution/history` | GET | Persistent execution history (JSONL, 7-day, survives DR/reload) |
| `/api/execution/history` | DELETE | Clear execution history (test support) |
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
| `account-limit` | S→C (all) | Anthropic account rate limit hit (429 detected) |
| `chain-blocked` | S→C (all) | Chain blocked by pending deps *(FE handler exists, not yet emitted from BE)* |
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
[DRAFT] → fc → [PROPOSED] → fl → [REVIEWED] → run → [DONE] → imp → [DONE]
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

> See [INTERNALS.md](INTERNALS.md) "Browser-First Input Handling" for detailed y/n and AskUserQuestion flows, session preservation, and revert instructions.

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
│   │   ├── claudeService.js     # claude.exe spawn, Auto Handoff, completion logic, execution history (JSONL)
│   │   ├── streamParser.js      # stream-json parsing, state detection (extracted from claudeService)
│   │   ├── chainExecutor.js     # Chain execution (fc→fl→run)
│   │   ├── ccsUtils.js          # CCS profile reading (config.yaml, auth list)
│   │   ├── ratelimitService.js  # Rate limit capture (fork worker for production, in-process DI for tests)
│   │   ├── vtScreenBuffer.js   # VT terminal emulator (screen buffer from escape sequences)
│   │   ├── featureService.js    # feature-{ID}.md reading, pendingDeps
│   │   ├── fileWatcher.js       # chokidar watch, status change detection
│   │   ├── statusMailService.js # IMAP IDLE status mail (auto-reply to empty emails)
│   │   ├── emailService.js      # Email notification (handoff/completion)
│   │   ├── cleanupService.js     # Tmp file cleanup (debug logs, daily logs, artifacts, history JSONL pruning)
│   │   ├── insightsService.js   # /insights PTY capture + email report (weekly scheduler)
│   │   ├── usageService.js      # CCS usage tracking (not exposed via API)
│   │   ├── inputPatterns.js     # Input wait patterns
│   │   ├── phaseUtils.js        # Phase detection utilities
│   │   ├── validation.js        # Input validation
│   │   └── workers/
│   │       └── ptyCapture.js   # Forked worker: node-pty ConPTY capture (crash-isolated from main process)
│   ├── parsers/
│   │   ├── featureParser.js     # Feature markdown parsing
│   │   └── indexParser.js       # index-features.md parsing
│   ├── websocket/
│   │   └── logStreamer.js       # WebSocket broadcast (sub/all)
│   └── utils/
│       ├── exitCodes.js         # Windows NTSTATUS exit code decoder
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
│   │   ├── ExecutionPanel.jsx   # Log display, Resume/Stop buttons, tabs, History toggle
│   │   ├── HistoryView.jsx     # Persistent execution history list (Copy ID, Resume)
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
cd src/tools/node/feature-dashboard && npm test          # both via workspaces
cd src/tools/node/feature-dashboard/backend && npm test  # backend only
cd src/tools/node/feature-dashboard/frontend && npm test # frontend only

# Check coverage
npx vitest run --coverage

# Mutation testing (after adding new tests)
npm run test:mutation
```

**Test levels** (prefer higher levels when feasible):

| Level | What | Pattern | Example |
|-------|------|---------|---------|
| **Route/Behavior** | HTTP request→response through Express | `execution.test.js` — `request(app, method, url, body)` | `GET /history` returns entries, `DELETE /history` then `GET` returns empty |
| **Service unit** | Single method with mocked deps | `cleanupService.test.js` — `svc._pruneHistoryJsonl(path, days)` | History pruning removes old entries |
| **Component** | React component rendering | `ExecutionPanel.test.jsx` — `render(<Component {...props} />)` | History button renders in tab bar |

**Prefer behavior tests over unit tests**: When adding a new API endpoint, always write route-level tests in `execution.test.js` (or the relevant route test file) that exercise the full request→service→response path. Unit tests on the service alone are insufficient — they miss routing bugs, param validation, error handling at the HTTP layer, and UUID validator interference.

**Test support APIs**: Endpoints for test setup/teardown (e.g., `DELETE /api/execution/history` for clearing state). These enable multi-step behavior tests that verify state transitions across API calls.

**Mutation testing interpretation**:
- **Killed**: Mutant detected by tests → good test
- **Survived**: Mutant escaped tests → test gap
- Target: 60%+ mutation score (covered scope)

### Security

All user input is whitelist-validated before passing to spawn:

| Function | Validation |
|----------|------------|
| `validateFeatureId()` | Numeric only (`/^\d+$/`) |
| `validateCommand()` | `fc`, `fl`, `run`, `imp` only |
| `runShellCommand()` | `cs`, `dr`, `upd` only |
| `executeSlashCommand()` | `commit`, `sync-deps` only |
| `answerInBrowser()` | `sanitizeInput(answer, 1000)` — control chars stripped, 1000 char limit |

---

## See Also

- [INTERNALS.md](INTERNALS.md) — Design decisions, detailed flows (rate limit, input handling, retry logic)
- [OPS.md](OPS.md) — Platform requirements, pm2, CCS profile setup, debugging, coverage stats
