@echo off
title ThesisBuilder

echo.
echo   ===================================
echo    ThesisBuilder Paper Tool
echo    Starting...
echo   ===================================
echo.

set "LAUNCHER_DIR=%~dp0"
if "%LAUNCHER_DIR:~-1%"=="\" set "LAUNCHER_DIR=%LAUNCHER_DIR:~0,-1%"

wsl bash -c "cd $(wslpath '%LAUNCHER_DIR%') && bash start.sh"

echo.
echo   Stopped.
pause
