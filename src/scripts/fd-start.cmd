@echo off
REM Feature Dashboard + Proxy - Start via pm2
where pm2 >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: pm2 not found. Install with: npm install -g pm2
    exit /b 1
)

pushd "%~dp0"
echo Starting proxy + backend + frontend + lsp-daemon via pm2...
call pm2 start ..\tools\node\feature-dashboard\ecosystem.config.cjs
call pm2 start ..\..\ecosystem.config.js || call pm2 restart lsp-daemon
popd
echo.
echo   proxy            http://127.0.0.1:8888
echo   backend + WS     http://localhost:3001
echo   frontend         http://localhost:5173
echo   lsp-daemon       http://127.0.0.1:19999
echo.
call pm2 list
