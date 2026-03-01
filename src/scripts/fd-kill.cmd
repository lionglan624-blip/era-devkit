@echo off
REM Feature Dashboard + Proxy - Kill all processes
REM Ports: proxy(8888), backend+WS(3001), frontend(5173)
setlocal enabledelayedexpansion

REM Try pm2 first (preferred)
where pm2 >nul 2>&1
if %errorlevel%==0 (
    echo [pm2] Stopping all processes...
    call pm2 stop proxy dashboard-backend dashboard-frontend lsp-daemon 2>nul
    call pm2 delete proxy dashboard-backend dashboard-frontend lsp-daemon 2>nul
    echo [pm2] Stopped.
)

REM Also kill by port (catch orphans not managed by pm2)
set "killed=0"
for %%p in (8888 3001 5173 19999) do (
    for /f "tokens=5" %%a in ('netstat -aon 2^>nul ^| findstr ":%%p "') do (
        if not "%%a"=="0" (
            taskkill /f /pid %%a >nul 2>&1 && (
                echo Killed PID %%a ^(port %%p^)
                set "killed=1"
            )
        )
    )
)

if "!killed!"=="0" (
    echo No orphan processes found on ports.
)
echo Dashboard + Proxy stopped.
endlocal
exit /b 0
