@echo off
REM Stop the LSP daemon by sending a request then killing the process
echo Stopping LSP daemon...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :19999 ^| findstr LISTENING') do (
    echo Killing PID %%a
    taskkill /PID %%a /F
)
