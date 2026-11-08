@echo off
setlocal
if not exist "%~dp0launcher\HanCSharp.Start.exe" (
  echo Please download and fully extract the Windows release ZIP.
  pause
  exit /b 1
)
start "" "%~dp0launcher\HanCSharp.Start.exe"
