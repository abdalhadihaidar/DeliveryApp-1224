@echo off
echo === SmarterASP Deployment Script ===
echo.

REM Set environment variable for deployment
set ASPNETCORE_ENVIRONMENT=Deployment

echo Building and publishing for SmarterASP deployment...
echo.

REM Navigate to the Web project directory
cd src\DeliveryApp.Web

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean --configuration Release

REM Restore packages
echo Restoring NuGet packages...
dotnet restore

REM Build the project
echo Building project...
dotnet build --configuration Release --no-restore

REM Publish the project
echo Publishing project...
dotnet publish --configuration Release --no-build --output "bin\Publish"

REM Copy additional files needed for deployment
echo Copying deployment files...

REM Copy web.config to publish directory
copy "web.config" "bin\Publish\web.config" /Y

REM Copy appsettings files
copy "appsettings.json" "bin\Publish\appsettings.json" /Y
copy "appsettings.Deployment.json" "bin\Publish\appsettings.Deployment.json" /Y

REM Create Logs directory in publish folder
if not exist "bin\Publish\Logs" mkdir "bin\Publish\Logs"

echo.
echo === Deployment Package Ready ===
echo Publish directory: bin\Publish
echo.
echo Next steps:
echo 1. Upload the contents of 'bin\Publish' folder to your SmarterASP hosting
echo 2. Make sure the remote database credentials are correct
echo 3. Check the Logs folder on the server for any startup issues
echo.

REM Reset environment variable
set ASPNETCORE_ENVIRONMENT=

REM Return to original directory
cd ..\..

echo === Deployment Script Completed ===
pause


