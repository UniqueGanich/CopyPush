@echo off
setlocal
rem ============================================================
rem  CopyPush - удаление
rem  Запустите этот файл, если хотите убрать утилиту из системы.
rem ============================================================

echo.
echo  Удаление CopyPush...
echo.

taskkill /IM CopyPush.exe /F >nul 2>&1

reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v CopyPush /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run" /v CopyPush /f >nul 2>&1
reg delete "HKCU\Software\CopyPush" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CopyPush" /f >nul 2>&1

rem удаляем папку с логом
if exist "%LOCALAPPDATA%\CopyPush" rmdir /s /q "%LOCALAPPDATA%\CopyPush"

rem удаляем эту папку (вместе с самим скриптом)
cd /d "%TEMP%"
rmdir /s /q "%~dp0"

echo  Готово. CopyPush удален.
timeout /t 3 >nul
endlocal
