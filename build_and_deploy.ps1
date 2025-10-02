# Build and Deploy DeliveryApp.Web to IIS VPS
# This script builds the project and prepares it for deployment

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\src\DeliveryApp.Web\bin\Publish",
    [string]$AppPath = "C:\inetpub\wwwroot\DeliveryApp"
)

Write-Host "=== Building and Deploying DeliveryApp.Web ===" -ForegroundColor Green

# Check if we're in the right directory
if (-not (Test-Path ".\src\DeliveryApp.Web\DeliveryApp.Web.csproj")) {
    Write-Host "Error: Please run this script from the backend_v1_3 directory" -ForegroundColor Red
    exit 1
}

Write-Host "1. Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
    Write-Host "   ✓ Previous build cleaned" -ForegroundColor Green
}

Write-Host "2. Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ".\src\DeliveryApp.Web\DeliveryApp.Web.csproj"
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Packages restored" -ForegroundColor Green

Write-Host "3. Building project..." -ForegroundColor Yellow
dotnet build ".\src\DeliveryApp.Web\DeliveryApp.Web.csproj" --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Project built successfully" -ForegroundColor Green

Write-Host "4. Publishing project..." -ForegroundColor Yellow
dotnet publish ".\src\DeliveryApp.Web\DeliveryApp.Web.csproj" --configuration $Configuration --output $OutputPath --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ✗ Publish failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Project published to: $OutputPath" -ForegroundColor Green

Write-Host "5. Copying configuration files..." -ForegroundColor Yellow

# Copy web.config
Copy-Item ".\src\DeliveryApp.Web\web.config" -Destination $OutputPath -Force
Write-Host "   ✓ web.config copied" -ForegroundColor Green

# Copy production appsettings
Copy-Item ".\src\DeliveryApp.Web\appsettings.Production.json" -Destination $OutputPath -Force
Write-Host "   ✓ appsettings.Production.json copied" -ForegroundColor Green

Write-Host "6. Creating deployment package..." -ForegroundColor Yellow

# Create deployment package
$deploymentPackage = ".\DeliveryApp-Web-Deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
Compress-Archive -Path "$OutputPath\*" -DestinationPath $deploymentPackage -Force
Write-Host "   ✓ Deployment package created: $deploymentPackage" -ForegroundColor Green

Write-Host "7. Deployment Instructions..." -ForegroundColor Yellow
Write-Host ""
Write-Host "To deploy to your VPS:" -ForegroundColor Cyan
Write-Host "1. Upload the contents of: $OutputPath" -ForegroundColor White
Write-Host "2. Or upload the deployment package: $deploymentPackage" -ForegroundColor White
Write-Host "3. Extract to your IIS application directory: $AppPath" -ForegroundColor White
Write-Host "4. Run the IIS configuration script on your VPS" -ForegroundColor White
Write-Host ""
Write-Host "Files ready for deployment:" -ForegroundColor Green
Write-Host "   - Published application: $OutputPath" -ForegroundColor White
Write-Host "   - Deployment package: $deploymentPackage" -ForegroundColor White
Write-Host ""
Write-Host "=== Build and Package Complete ===" -ForegroundColor Green

