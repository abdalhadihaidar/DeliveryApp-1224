@echo off
echo Fixing OpenIddict Client Secret...
echo.

REM Run the PowerShell script
powershell.exe -ExecutionPolicy Bypass -File "fix_openiddict_client_secret.ps1"

echo.
echo Press any key to continue...
pause >nul

