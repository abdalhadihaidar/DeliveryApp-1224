@echo off
echo ========================================
echo    Waseel Delivery App - Project Runner
echo ========================================
echo.

REM Set environment variables
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000

REM Load environment variables from env.production file
if exist "env.production" (
    echo Loading environment variables from env.production...
    for /f "usebackq tokens=1,2 delims==" %%a in ("env.production") do (
        if not "%%a"=="" if not "%%a:~0,1%"=="#" (
            set "%%a=%%b"
        )
    )
)

echo.
echo Available Projects:
echo 1. HttpApi.Host (API Backend) - Recommended
echo 2. Web (Web Application)
echo 3. Blazor.WebApp (Blazor Server)
echo 4. Blazor.WebApp.Tiered (Tiered Blazor)
echo 5. DbMigrator (Database Migration)
echo.

set /p choice="Select project to run (1-5): "

if "%choice%"=="1" goto :run_host
if "%choice%"=="2" goto :run_web
if "%choice%"=="3" goto :run_blazor
if "%choice%"=="4" goto :run_blazor_tiered
if "%choice%"=="5" goto :run_migrator
goto :invalid_choice

:run_host
echo.
echo Starting HttpApi.Host (API Backend)...
cd src\DeliveryApp.HttpApi.Host
goto :run_project

:run_web
echo.
echo Starting Web Application...
cd src\DeliveryApp.Web
goto :run_project

:run_blazor
echo.
echo Starting Blazor WebApp...
cd src\DeliveryApp.Blazor.WebApp
goto :run_project

:run_blazor_tiered
echo.
echo Starting Blazor WebApp Tiered...
cd src\DeliveryApp.Blazor.WebApp.Tiered
goto :run_project

:run_migrator
echo.
echo Running Database Migrator...
cd src\DeliveryApp.DbMigrator
dotnet restore
dotnet build --configuration Release
dotnet run --configuration Release
goto :end

:run_project
echo Restoring packages...
dotnet restore

echo Building project...
dotnet build --configuration Release

echo.
echo Starting application...
echo Backend will be available at:
echo - HTTPS: https://localhost:5001
echo - HTTP:  http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run --configuration Release
goto :end

:invalid_choice
echo Invalid choice. Please run the script again and select 1-5.
goto :end

:end
pause
