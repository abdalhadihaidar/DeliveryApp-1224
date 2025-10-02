# IIS VPS Deployment Script for DeliveryApp.Web
# Run this script as Administrator on your VPS

param(
    [string]$AppPath = "C:\inetpub\wwwroot\DeliveryApp",
    [string]$AppPoolName = "DeliveryAppPool",
    [string]$SiteName = "DeliveryApp",
    [string]$Domain = "backend.waselsy.com"
)

Write-Host "=== DeliveryApp.Web IIS VPS Deployment ===" -ForegroundColor Green

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires administrator privileges. Please run as administrator." -ForegroundColor Red
    exit 1
}

# Import required modules
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "1. Checking Prerequisites..." -ForegroundColor Yellow

# Check .NET 9.0 Runtime
$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion -match "9\.") {
    Write-Host "   ✓ .NET 9.0 Runtime found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "   ✗ .NET 9.0 Runtime not found. Please install .NET 9.0 Runtime." -ForegroundColor Red
    Write-Host "   Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
    exit 1
}

# Check ASP.NET Core Module
$aspNetCoreModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
if ($aspNetCoreModule) {
    Write-Host "   ✓ ASP.NET Core Module V2 is installed" -ForegroundColor Green
} else {
    Write-Host "   ✗ ASP.NET Core Module V2 not found" -ForegroundColor Red
    Write-Host "   Please install ASP.NET Core Hosting Bundle for .NET 9.0" -ForegroundColor Yellow
    exit 1
}

Write-Host "2. Creating Application Pool..." -ForegroundColor Yellow

# Remove existing application pool if it exists
if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
    Write-Host "   Removing existing application pool..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $AppPoolName
}

# Create new application pool
New-WebAppPool -Name $AppPoolName
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false
Start-WebAppPool -Name $AppPoolName

Write-Host "   ✓ Application pool '$AppPoolName' created and started" -ForegroundColor Green

Write-Host "3. Creating Website..." -ForegroundColor Yellow

# Remove existing website if it exists
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Write-Host "   Removing existing website..." -ForegroundColor Yellow
    Remove-Website -Name $SiteName
}

# Create application directory
if (-not (Test-Path $AppPath)) {
    New-Item -ItemType Directory -Path $AppPath -Force
    Write-Host "   ✓ Application directory created: $AppPath" -ForegroundColor Green
}

# Create website
New-Website -Name $SiteName -Port 80 -PhysicalPath $AppPath -ApplicationPool $AppPoolName

Write-Host "   ✓ Website '$SiteName' created" -ForegroundColor Green

Write-Host "4. Setting up File Permissions..." -ForegroundColor Yellow

# Set permissions for application pool identity
$appPoolIdentity = "IIS AppPool\$AppPoolName"

# Grant full control to application pool identity
icacls $AppPath /grant "${appPoolIdentity}:(OI)(CI)F" /T 2>$null

# Create and set permissions for Logs folder
$logsPath = Join-Path $AppPath "Logs"
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force
}
icacls $logsPath /grant "${appPoolIdentity}:(OI)(CI)F" /T 2>$null

Write-Host "   ✓ File permissions configured" -ForegroundColor Green

Write-Host "5. Configuring HTTPS (Optional)..." -ForegroundColor Yellow

# Check if SSL certificate exists
$cert = Get-ChildItem -Path "Cert:\LocalMachine\My" | Where-Object { $_.Subject -like "*$Domain*" } | Select-Object -First 1

if ($cert) {
    Write-Host "   Found SSL certificate: $($cert.Subject)" -ForegroundColor Green
    
    # Add HTTPS binding
    New-WebBinding -Name $SiteName -Protocol "https" -Port 443 -SslFlags 1
    $binding = Get-WebBinding -Name $SiteName -Protocol "https"
    $binding.AddSslCertificate($cert.GetCertHashString(), "my")
    
    Write-Host "   ✓ HTTPS binding configured" -ForegroundColor Green
} else {
    Write-Host "   No SSL certificate found for $Domain" -ForegroundColor Yellow
    Write-Host "   You can add HTTPS binding later using IIS Manager or PowerShell" -ForegroundColor Cyan
}

Write-Host "6. Final Configuration..." -ForegroundColor Yellow

# Ensure website is started
Start-Website -Name $SiteName

# Test website
try {
    $response = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✓ Website is responding (HTTP 200)" -ForegroundColor Green
} catch {
    Write-Host "   ⚠ Website may not be fully deployed yet" -ForegroundColor Yellow
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Upload your published application files to: $AppPath" -ForegroundColor White
Write-Host "2. Ensure appsettings.Production.json is configured correctly" -ForegroundColor White
Write-Host "3. Test the application at: http://$Domain" -ForegroundColor White
Write-Host "4. Configure SSL certificate for HTTPS if needed" -ForegroundColor White
Write-Host ""
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Cyan
Write-Host "Website: $SiteName" -ForegroundColor Cyan
Write-Host "Physical Path: $AppPath" -ForegroundColor Cyan
Write-Host "Domain: $Domain" -ForegroundColor Cyan

