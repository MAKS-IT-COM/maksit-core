@echo off
setlocal

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Force-AmendTaggedCommit.ps1"

pause