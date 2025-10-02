# Waseel Delivery App - Project Runner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Waseel Delivery App - Project Runner" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "https://localhost:5001;http://localhost:5000"

# Load environment variables from env.production file
if (Test-Path "env.production") {
    Write-Host "Loading environment variables from env.production..." -ForegroundColor Yellow
    Get-Content "env.production" | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            if ($name -and $value) {
                [Environment]::SetEnvironmentVariable($name, $value, "Process")
                Write-Host "Set $name" -ForegroundColor Gray
            }
        }
    }
}

Write-Host ""
Write-Host "Available Projects:" -ForegroundColor Green
Write-Host "1. HttpApi.Host (API Backend) - Recommended" -ForegroundColor White
Write-Host "2. Web (Web Application)" -ForegroundColor White
Write-Host "3. Blazor.WebApp (Blazor Server)" -ForegroundColor White
Write-Host "4. Blazor.WebApp.Tiered (Tiered Blazor)" -ForegroundColor White
Write-Host "5. DbMigrator (Database Migration)" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Select project to run (1-5)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "Starting HttpApi.Host (API Backend)..." -ForegroundColor Green
        Set-Location "src\DeliveryApp.HttpApi.Host"
    }
    "2" {
        Write-Host ""
        Write-Host "Starting Web Application..." -ForegroundColor Green
        Set-Location "src\DeliveryApp.Web"
    }
    "3" {
        Write-Host ""
        Write-Host "Starting Blazor WebApp..." -ForegroundColor Green
        Set-Location "src\DeliveryApp.Blazor.WebApp"
    }
    "4" {
        Write-Host ""
        Write-Host "Starting Blazor WebApp Tiered..." -ForegroundColor Green
        Set-Location "src\DeliveryApp.Blazor.WebApp.Tiered"
    }
    "5" {
        Write-Host ""
        Write-Host "Running Database Migrator..." -ForegroundColor Green
        Set-Location "src\DeliveryApp.DbMigrator"
        Write-Host "Restoring packages..." -ForegroundColor Yellow
        dotnet restore
        Write-Host "Building project..." -ForegroundColor Yellow
        dotnet build --configuration Release
        Write-Host "Running migrator..." -ForegroundColor Yellow
        dotnet run --configuration Release
        return
    }
    default {
        Write-Host "Invalid choice. Please run the script again and select 1-5." -ForegroundColor Red
        return
    }
}

Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release

Write-Host ""
Write-Host "Starting application..." -ForegroundColor Green
Write-Host "Backend will be available at:" -ForegroundColor Cyan
Write-Host "- HTTPS: https://localhost:5001" -ForegroundColor White
Write-Host "- HTTP:  http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

dotnet run --configuration Release
