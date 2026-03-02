@echo off
REM Restart dashboard-backend via pm2 (stop → wait → start).
REM Called by Auto-DR and DR button from within the backend process.
REM Must be an external script because the backend cannot restart itself
REM (pm2 stop kills the calling process before pm2 start can execute).
pm2 stop dashboard-backend >nul 2>&1
ping -n 4 127.0.0.1 >nul
pm2 restart dashboard-backend >nul 2>&1
