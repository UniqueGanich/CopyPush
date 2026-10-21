@echo off
setlocal
rem ============================================================
rem  CopyPush - uninstaller
rem  Run this file to remove CopyPush from the system.
rem ============================================================

echo.
echo  Removing CopyPush...
echo.

taskkill /IM CopyPush.exe /F >nul 2>&1

reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v CopyPush /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" /v CopyPush /f >nul 2>&1
reg delete "HKCU\Software\CopyPush" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CopyPush" /f >nul 2>&1

rem remove the log folder
if exist "%LOCALAPPDATA%\CopyPush" rmdir /s /q "%LOCALAPPDATA%\CopyPush"

rem remove this folder together with the script itself
cd /d "%TEMP%"
rmdir /s /q "%~dp0"

echo  Done. CopyPush has been removed.
timeout /t 3 >nul
endlocal