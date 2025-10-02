@echo off
echo Starting Waseel Delivery App Backend...
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

REM Navigate to the API host directory
cd src\DeliveryApp.HttpApi.Host

REM Restore packages
echo Restoring NuGet packages...
dotnet restore

REM Build the project
echo Building the project...
dotnet build --configuration Release

REM Run the project
echo Starting the application...
echo.
echo Backend will be available at:
echo - HTTPS: https://localhost:5001
echo - HTTP:  http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run --configuration Release

pause
