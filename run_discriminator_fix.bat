@echo off
echo Starting discriminator fix...
echo.

REM Check if PowerShell is available
powershell -Command "Get-Host" >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: PowerShell is not available
    pause
    exit /b 1
)

REM Run the PowerShell script
echo Running discriminator fix script...
powershell -ExecutionPolicy Bypass -File "run_discriminator_fix.ps1"

if %errorlevel% equ 0 (
    echo.
    echo Discriminator fix completed successfully!
    echo You can now run the application.
) else (
    echo.
    echo Discriminator fix failed. Please check the error messages above.
)

echo.
pause
