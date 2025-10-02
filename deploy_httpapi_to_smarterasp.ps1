# SmarterASP Deployment Script for DeliveryApp HttpApi.Host
# This script builds and deploys the HttpApi.Host application to SmarterASP hosting

param(
    [string]$Configuration = "Release",
    [string]$Environment = "Production"
)

Write-Host "=== SmarterASP HttpApi.Host Deployment Script ===" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Set environment variable for the build
$env:ASPNETCORE_ENVIRONMENT = $Environment

try {
    # Navigate to the HttpApi.Host project directory
    Set-Location "src\DeliveryApp.HttpApi.Host"
    
    # Clean previous builds
    Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
    dotnet clean --configuration $Configuration
    
    # Restore packages
    Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
    dotnet restore
    
    # Build the project
    Write-Host "Building project..." -ForegroundColor Cyan
    dotnet build --configuration $Configuration --no-restore
    
    # Publish the project
    Write-Host "Publishing project..." -ForegroundColor Cyan
    dotnet publish --configuration $Configuration --no-build --output "bin\Publish"
    
    # Copy additional files needed for deployment
    Write-Host "Copying deployment files..." -ForegroundColor Cyan
    
    # Copy web.config to publish directory if it exists
    if (Test-Path "web.config") {
        Copy-Item "web.config" "bin\Publish\web.config" -Force
    }
    
    # Copy appsettings files
    if (Test-Path "appsettings.json") {
        Copy-Item "appsettings.json" "bin\Publish\appsettings.json" -Force
    }
    if (Test-Path "appsettings.$Environment.json") {
        Copy-Item "appsettings.$Environment.json" "bin\Publish\appsettings.$Environment.json" -Force
    }
    
    # Create Logs directory in publish folder
    New-Item -ItemType Directory -Path "bin\Publish\Logs" -Force | Out-Null
    
    # Ensure wwwroot directory exists and copy it
    if (Test-Path "wwwroot") {
        Write-Host "Copying wwwroot directory..." -ForegroundColor Cyan
        Copy-Item "wwwroot" "bin\Publish\wwwroot" -Recurse -Force
    }
    
    Write-Host "=== Deployment Package Ready ===" -ForegroundColor Green
    Write-Host "Publish directory: src\DeliveryApp.HttpApi.Host\bin\Publish" -ForegroundColor Yellow
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Upload the contents of 'bin\Publish' folder to your SmarterASP hosting" -ForegroundColor White
    Write-Host "2. Make sure the remote database credentials are correct in appsettings.$Environment.json" -ForegroundColor White
    Write-Host "3. Check the Logs folder on the server for any startup issues" -ForegroundColor White
    Write-Host "4. Ensure the wwwroot folder is uploaded with all frontend files" -ForegroundColor White
    
} catch {
    Write-Host "Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Reset environment variable
    Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    Set-Location "..\.."
}

Write-Host "=== Deployment Script Completed ===" -ForegroundColor Green

