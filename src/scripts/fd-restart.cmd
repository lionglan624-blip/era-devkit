@echo off
REM Feature Dashboard + Proxy + LSP Daemon - Restart (kill + start)
echo === Stopping ===
cmd /c "%~dp0fd-kill.cmd"
ping -n 3 127.0.0.1 >nul
echo.
echo === Starting ===
cmd /c "%~dp0fd-start.cmd"
