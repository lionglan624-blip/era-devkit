# DR Restart EADDRINUSE Investigation (2026-03-03)

Dashboard backend crash loop on restart — full investigation and failed approaches.

## Symptom

After DR button or Auto-DR, backend enters crash loop:
- `EADDRINUSE: address already in use :::3001` repeating
- pm2 restart counter climbing (↺188 over 8 minutes observed)
- Browser shows constant reconnection

## Root Cause

`pm2 restart` sends SIGTERM to the old process and starts a new one **simultaneously**.
On Windows, TCP port release lags process death by several seconds.
New process tries `server.listen(3001)` while old process still holds the port → EADDRINUSE.

### Why dr.bat works but DR button didn't

| Method | Mechanism | Why it works |
|--------|-----------|-------------|
| `dr.bat` | External cmd.exe: `pm2 stop` → `ping -n 3` (2s wait) → `pm2 start` | Sequential stop→wait→start from OUTSIDE the process |
| DR button (old) | `pm2 restart all` from WITHIN the backend process | pm2 kills old + starts new simultaneously → EADDRINUSE |
| Auto-DR (old) | `pm2 restart dashboard-backend` from WITHIN | Same as above |

Key insight: **the restart command must come from OUTSIDE the dying process's process group.**

### Why it "worked before"

Unclear. Likely combination of:
- Auto-DR / DR button used infrequently (manual `dr.bat` was the primary method)
- 3-layer defense (startup `cleanupPort` + EADDRINUSE retry) masked the issue — loop eventually resolved by luck
- The `detached: false` pm2 patch (for KB5077181 console flash) may have worsened process group behavior

## Approaches Tried (in order)

### 1. exp_backoff_restart_delay + remove startup cleanupPort

**Files**: `ecosystem.config.cjs`, `server.js`

**Idea**: Remove startup `cleanupPort()` (which killed sibling pm2 instances), add exponential backoff to pm2 restart delay.

**Result**: ❌ Reduced loop from 188 to ~10 cycles, but didn't eliminate it. `pm2 restart` still starts new process before old dies. `exp_backoff_restart_delay` only applies to crash restarts, NOT to `pm2 restart` command.

### 2. EADDRINUSE backoff with delayed cleanup

**Files**: `server.js`

**Idea**: On EADDRINUSE, wait 2s/4s without killing. Only `cleanupPort()` on final retry. `process.exit(1)` after 3 failed retries.

**Result**: ❌ `process.exit(1)` triggers pm2 autorestart → new cycle. Each cycle takes ~12s (3 retries × 2s/4s/6s). Multiple cycles still needed.

### 3. Never exit on EADDRINUSE (adaptive polling)

**Files**: `server.js`

**Idea**: Remove `process.exit(1)` from EADDRINUSE handler entirely. Poll with adaptive delay (500ms → 2s), `cleanupPort()` after 10s as last resort.

**Result**: ✅ Partial — process no longer crashes on EADDRINUSE. But still takes 10+ seconds to recover because pm2 starts new instance immediately, creating a race.

### 4. stop → wait → start via cmd.exe (inline command chain)

**Files**: `server.js`, `claudeService.js`

**Idea**: Replace `pm2 restart` with `spawn('cmd', ['/c', 'pm2 stop ... && ping -n 3 ... && pm2 start ...'], { detached: true })`.

**Result**: ❌ `pm2 stop` kills the parent Node.js process. Despite `detached: true`, the child cmd.exe also dies. Reason: pm2's ForkMode `detached: false` patch means all children share the daemon's process group. When pm2 kills the backend worker, cmd.exe (same group) gets killed too.

### 5. stop → wait → start via external .cmd script

**Files**: `restart-backend.cmd`, `server.js`, `claudeService.js`

**Idea**: Same as #4 but via an external .cmd file. `spawn(script, [], { detached: true })`.

**Result**: ❌ Same process group issue. Also: `.cmd` files cannot be spawned with `shell: false` on Windows (EINVAL error). With `shell: true` + `detached: true`, still killed by pm2 stop.

### 6. schtasks (Windows Task Scheduler)

**Files**: `server.js`, `claudeService.js`

**Idea**: Create and run a one-shot scheduled task that executes `restart-backend.cmd`. Runs in a completely separate context.

**Result**: ❌ Requires administrator privileges. pm2 runs under user account.

### 7. WScript.Shell.Run via VBScript ✅

**Files**: `restart-backend.vbs`, `server.js`, `claudeService.js`

**Idea**: `spawn('wscript', ['restart-backend.vbs'])` → VBScript uses `WScript.Shell.Run` with `vbHide(0), async(False)` to launch cmd.exe in a **new, independent process group**.

**Result**: ✅ Works. `pm2 stop` kills the backend, but wscript.exe + cmd.exe survive in their own process group. The cmd script waits 3s for port release, then `pm2 restart` succeeds on a free port.

## Final Architecture

```
[DR Button / Auto-DR]
  → spawn('wscript', ['restart-backend.vbs'])
    → WScript.Shell.Run "cmd /c restart-backend.cmd", 0, False
      → pm2 stop dashboard-backend
      → ping -n 4 (3s wait)
      → pm2 restart dashboard-backend
        → New process starts, port 3001 is free
        → server.listen(3001) succeeds immediately
```

Fallback defense in `server.js` (for cold starts or edge cases):
- EADDRINUSE adaptive polling (500ms → 2s, never exits)
- `cleanupPort()` after 10s as absolute last resort

## Files Changed

| File | Change |
|------|--------|
| `backend/restart-backend.vbs` | NEW — VBScript wrapper for process-group-independent restart |
| `backend/restart-backend.cmd` | NEW — stop → wait → restart sequence |
| `backend/server.js` | Auto-DR uses VBScript; EADDRINUSE polls without exiting |
| `backend/src/services/claudeService.js` | DR button uses VBScript; `__dirname` polyfill for ESM |
| `backend/src/config.js` | Comment update |
| `ecosystem.config.cjs` | `exp_backoff_restart_delay: 200` (replaces `restart_delay: 2000`) |

## Troubleshooting Checklist (for future DR issues)

1. **Backend stuck in `waiting`?** → `pm2 restart dashboard-backend` manually
2. **Port 3001 stuck?** → `netstat -ano | findstr ":3001"` then `taskkill /F /PID <pid>`
3. **Proxy crash loop (↺ climbing)?** → `pm2 stop proxy` (existing proxy from Claude session holds port 8888)
4. **`restart-backend.vbs` not running?** → Check `pm2 logs dashboard-backend --err` for spawn errors
5. **Full reset**: `pm2 delete all` → kill orphan ports → `pm2 start ecosystem.config.cjs` + `pm2 start ecosystem.config.js`
6. **`dr.bat` always works** as fallback — it runs from external cmd.exe, not inside pm2
