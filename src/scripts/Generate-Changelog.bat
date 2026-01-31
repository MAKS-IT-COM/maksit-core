@echo off

REM Change directory to the location of the script
cd /d %~dp0

REM Run AI changelog generator (dry-run mode with debug output)
powershell -ExecutionPolicy Bypass -File "%~dp0Generate-Changelog.ps1"

pause
