# Waseel Delivery App Backend Startup Script
Write-Host "Starting Waseel Delivery App Backend..." -ForegroundColor Green
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

# Navigate to the API host directory
Set-Location "src\DeliveryApp.HttpApi.Host"

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the project
Write-Host "Building the project..." -ForegroundColor Yellow
dotnet build --configuration Release

# Run the project
Write-Host "Starting the application..." -ForegroundColor Green
Write-Host ""
Write-Host "Backend will be available at:" -ForegroundColor Cyan
Write-Host "- HTTPS: https://localhost:5001" -ForegroundColor White
Write-Host "- HTTP:  http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

dotnet run --configuration Release
