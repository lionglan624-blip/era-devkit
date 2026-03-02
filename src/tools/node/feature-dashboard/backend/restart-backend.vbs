Set WshShell = CreateObject("WScript.Shell")
WshShell.Run "cmd /c pm2 stop dashboard-backend >nul 2>&1 && ping -n 4 127.0.0.1 >nul && pm2 restart dashboard-backend >nul 2>&1", 0, False
