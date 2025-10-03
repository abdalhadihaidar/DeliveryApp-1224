# Fix HTTP 403.18 Error - Application Pool Issue
# This script resolves the "cannot be processed in the application pool" error

param(
    [string]$SiteName = "WASEL",
    [string]$AppPoolName = "WASEL",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\WASEL"
)

Write-Host "=== Fixing HTTP 403.18 Error ===" -ForegroundColor Green

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires administrator privileges. Please run as administrator." -ForegroundColor Red
    exit 1
}

# Import required modules
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "1. Stopping existing website and application pool..." -ForegroundColor Yellow

# Stop website if it exists
$website = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($website) {
    Stop-Website -Name $SiteName
    Write-Host "   ✓ Website stopped" -ForegroundColor Green
}

# Stop application pool if it exists
$appPool = Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
if ($appPool) {
    Stop-WebAppPool -Name $AppPoolName
    Write-Host "   ✓ Application pool stopped" -ForegroundColor Green
}

Write-Host "2. Removing existing configuration..." -ForegroundColor Yellow

# Remove website if it exists
if ($website) {
    Remove-Website -Name $SiteName
    Write-Host "   ✓ Website removed" -ForegroundColor Green
}

# Remove application pool if it exists
if ($appPool) {
    Remove-WebAppPool -Name $AppPoolName
    Write-Host "   ✓ Application pool removed" -ForegroundColor Green
}

Write-Host "3. Creating fresh application pool..." -ForegroundColor Yellow

# Create new application pool with correct settings
New-WebAppPool -Name $AppPoolName
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.loadUserProfile" -Value $false

Write-Host "   ✓ Application pool created with correct settings" -ForegroundColor Green

Write-Host "4. Creating fresh website..." -ForegroundColor Yellow

# Ensure physical path exists
if (-not (Test-Path $PhysicalPath)) {
    New-Item -ItemType Directory -Path $PhysicalPath -Force
    Write-Host "   ✓ Created physical path: $PhysicalPath" -ForegroundColor Green
}

# Create new website
New-Website -Name $SiteName -Port 80 -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName

Write-Host "   ✓ Website created with correct settings" -ForegroundColor Green

Write-Host "5. Setting file permissions..." -ForegroundColor Yellow

$appPoolIdentity = "IIS AppPool\$AppPoolName"

try {
    # Grant full control to application pool identity
    icacls $PhysicalPath /grant "${appPoolIdentity}:(OI)(CI)F" /T 2>$null
    Write-Host "   ✓ File permissions set" -ForegroundColor Green
    
    # Create and set permissions for Logs folder
    $logsPath = Join-Path $PhysicalPath "Logs"
    if (-not (Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force
    }
    icacls $logsPath /grant "${appPoolIdentity}:(OI)(CI)F" /T 2>$null
    Write-Host "   ✓ Logs folder permissions set" -ForegroundColor Green
} catch {
    Write-Host "   ⚠ Could not set file permissions: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "6. Starting services..." -ForegroundColor Yellow

# Start application pool
Start-WebAppPool -Name $AppPoolName
Write-Host "   ✓ Application pool started" -ForegroundColor Green

# Start website
Start-Website -Name $SiteName
Write-Host "   ✓ Website started" -ForegroundColor Green

Write-Host "7. Verifying configuration..." -ForegroundColor Yellow

# Wait a moment for services to start
Start-Sleep -Seconds 5

# Check application pool status
$appPoolStatus = (Get-IISAppPool -Name $AppPoolName).State
Write-Host "   Application Pool Status: $appPoolStatus" -ForegroundColor $(if ($appPoolStatus -eq "Started") { "Green" } else { "Red" })

# Check website status
$websiteStatus = (Get-Website -Name $SiteName).State
Write-Host "   Website Status: $websiteStatus" -ForegroundColor $(if ($websiteStatus -eq "Started") { "Green" } else { "Red" })

Write-Host "8. Testing website..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Website is responding (HTTP 200)" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Website returned status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Website test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   This might be expected if the application files are not yet deployed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Fix Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Cyan
Write-Host "  - Application Pool: $AppPoolName" -ForegroundColor White
Write-Host "  - Website: $SiteName" -ForegroundColor White
Write-Host "  - Physical Path: $PhysicalPath" -ForegroundColor White
Write-Host "  - Port: 80" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Deploy your application files to: $PhysicalPath" -ForegroundColor White
Write-Host "2. Ensure web.config is properly configured" -ForegroundColor White
Write-Host "3. Test your application at: http://localhost" -ForegroundColor White
Write-Host "4. Test Swagger at: http://localhost/swagger" -ForegroundColor White
Write-Host ""
Write-Host "If you still get 403.18 errors:" -ForegroundColor Red
Write-Host "- Run: iisreset" -ForegroundColor White
Write-Host "- Check Windows Event Logs" -ForegroundColor White
Write-Host "- Verify application files are deployed correctly" -ForegroundColor White









