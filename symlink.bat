@echo off
openfiles >nul 2>&1
if %errorlevel% NEQ 0 (
    powershell start-process -verb runas '%0'
    exit /b
)

set /p path_input=Rain World mods folder: 
mklink /d "%path_input%\ArenaPlus" "%~dp0mod"
pause