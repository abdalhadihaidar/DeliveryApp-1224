@echo off
echo Discriminator Fix Tool
echo =====================
echo.

REM Navigate to the discriminator fix project directory
cd src\DeliveryApp.DiscriminatorFix

REM Build and run the discriminator fix tool
echo Building discriminator fix tool...
dotnet build

if %errorlevel% neq 0 (
    echo Error: Failed to build the discriminator fix tool
    pause
    exit /b 1
)

echo.
echo Running discriminator fix tool...
dotnet run

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
