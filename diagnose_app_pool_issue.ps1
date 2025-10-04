# Diagnose Application Pool Issue
# This script helps diagnose and fix HTTP 403.18 errors

param(
    [string]$SiteName = "WASEL",
    [string]$AppPoolName = "WASEL",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\WASEL"
)

Write-Host "=== Diagnosing Application Pool Issue ===" -ForegroundColor Green

# Import required modules
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "1. Checking IIS Installation..." -ForegroundColor Yellow
if (Get-Module -ListAvailable -Name WebAdministration) {
    Write-Host "   ✓ IIS WebAdministration module is available" -ForegroundColor Green
} else {
    Write-Host "   ✗ IIS WebAdministration module not found. Please install IIS with Management Tools." -ForegroundColor Red
    exit 1
}

Write-Host "2. Checking Application Pools..." -ForegroundColor Yellow
$allAppPools = Get-IISAppPool
Write-Host "   Available Application Pools:" -ForegroundColor Cyan
foreach ($pool in $allAppPools) {
    $status = if ($pool.State -eq "Started") { "✓" } else { "✗" }
    Write-Host "     $status $($pool.Name) - $($pool.State)" -ForegroundColor $(if ($pool.State -eq "Started") { "Green" } else { "Red" })
}

# Check if the specific application pool exists
$appPool = Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
if ($appPool) {
    Write-Host "   ✓ Application pool '$AppPoolName' exists" -ForegroundColor Green
    Write-Host "     - .NET CLR Version: $($appPool.ManagedRuntimeVersion)" -ForegroundColor Cyan
    Write-Host "     - Process Model Identity: $($appPool.ProcessModel.IdentityType)" -ForegroundColor Cyan
    Write-Host "     - State: $($appPool.State)" -ForegroundColor Cyan
    
    if ($appPool.State -ne "Started") {
        Write-Host "     Starting application pool..." -ForegroundColor Yellow
        Start-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 3
        Write-Host "     ✓ Application pool started" -ForegroundColor Green
    }
} else {
    Write-Host "   ✗ Application pool '$AppPoolName' not found" -ForegroundColor Red
    Write-Host "   Creating application pool..." -ForegroundColor Yellow
    
    New-WebAppPool -Name $AppPoolName
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false
    Start-WebAppPool -Name $AppPoolName
    
    Write-Host "   ✓ Application pool '$AppPoolName' created and started" -ForegroundColor Green
}

Write-Host "3. Checking Websites..." -ForegroundColor Yellow
$allWebsites = Get-Website
Write-Host "   Available Websites:" -ForegroundColor Cyan
foreach ($site in $allWebsites) {
    $status = if ($site.State -eq "Started") { "✓" } else { "✗" }
    Write-Host "     $status $($site.Name) - $($site.State) - Pool: $($site.ApplicationPool)" -ForegroundColor $(if ($site.State -eq "Started") { "Green" } else { "Red" })
}

# Check if the specific website exists
$website = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($website) {
    Write-Host "   ✓ Website '$SiteName' exists" -ForegroundColor Green
    Write-Host "     - Physical Path: $($website.PhysicalPath)" -ForegroundColor Cyan
    Write-Host "     - Application Pool: $($website.ApplicationPool)" -ForegroundColor Cyan
    Write-Host "     - State: $($website.State)" -ForegroundColor Cyan
    Write-Host "     - Bindings: $($website.Bindings)" -ForegroundColor Cyan
    
    # Check if the application pool matches
    if ($website.ApplicationPool -ne $AppPoolName) {
        Write-Host "     ⚠ Website is using different application pool: $($website.ApplicationPool)" -ForegroundColor Yellow
        Write-Host "     Updating website to use correct application pool..." -ForegroundColor Yellow
        Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $AppPoolName
        Write-Host "     ✓ Website application pool updated" -ForegroundColor Green
    }
    
    # Ensure website is running
    if ($website.State -ne "Started") {
        Write-Host "     Starting website..." -ForegroundColor Yellow
        Start-Website -Name $SiteName
        Write-Host "     ✓ Website started" -ForegroundColor Green
    }
} else {
    Write-Host "   ✗ Website '$SiteName' not found" -ForegroundColor Red
    Write-Host "   Creating website..." -ForegroundColor Yellow
    
    # Ensure physical path exists
    if (-not (Test-Path $PhysicalPath)) {
        New-Item -ItemType Directory -Path $PhysicalPath -Force
        Write-Host "     ✓ Created physical path: $PhysicalPath" -ForegroundColor Green
    }
    
    New-Website -Name $SiteName -Port 80 -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName
    Write-Host "   ✓ Website '$SiteName' created" -ForegroundColor Green
}

Write-Host "4. Checking Physical Path..." -ForegroundColor Yellow
if (Test-Path $PhysicalPath) {
    Write-Host "   ✓ Physical path exists: $PhysicalPath" -ForegroundColor Green
    
    # Check if DeliveryApp.Web.dll exists
    $dllPath = Join-Path $PhysicalPath "DeliveryApp.Web.dll"
    if (Test-Path $dllPath) {
        Write-Host "   ✓ DeliveryApp.Web.dll found" -ForegroundColor Green
    } else {
        Write-Host "   ✗ DeliveryApp.Web.dll not found" -ForegroundColor Red
        Write-Host "   Please ensure you have deployed the application files to: $PhysicalPath" -ForegroundColor Yellow
    }
    
    # Check if web.config exists
    $webConfigPath = Join-Path $PhysicalPath "web.config"
    if (Test-Path $webConfigPath) {
        Write-Host "   ✓ web.config found" -ForegroundColor Green
    } else {
        Write-Host "   ✗ web.config not found" -ForegroundColor Red
        Write-Host "   Please ensure web.config is deployed to: $PhysicalPath" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ Physical path does not exist: $PhysicalPath" -ForegroundColor Red
    Write-Host "   Creating physical path..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $PhysicalPath -Force
    Write-Host "   ✓ Physical path created" -ForegroundColor Green
}

Write-Host "5. Setting File Permissions..." -ForegroundColor Yellow
$appPoolIdentity = "IIS AppPool\$AppPoolName"

try {
    # Grant full control to application pool identity
    icacls $PhysicalPath /grant "${appPoolIdentity}:(OI)(CI)F" /T 2>$null
    Write-Host "   ✓ File permissions set for application pool identity" -ForegroundColor Green
    
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

Write-Host "6. Checking .NET Runtime..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion) {
    Write-Host "   ✓ .NET Runtime version: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "   ✗ .NET Runtime not found" -ForegroundColor Red
    Write-Host "   Please install .NET 9.0 Runtime" -ForegroundColor Yellow
}

Write-Host "7. Checking ASP.NET Core Module..." -ForegroundColor Yellow
$aspNetCoreModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
if ($aspNetCoreModule) {
    Write-Host "   ✓ ASP.NET Core Module V2 is installed" -ForegroundColor Green
} else {
    Write-Host "   ✗ ASP.NET Core Module V2 not found" -ForegroundColor Red
    Write-Host "   Please install ASP.NET Core Hosting Bundle" -ForegroundColor Yellow
}

Write-Host "8. Testing Website..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Website is responding (HTTP 200)" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Website returned status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Website test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Diagnosis Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. If application pool or website was created, restart IIS" -ForegroundColor White
Write-Host "2. Ensure all application files are deployed to: $PhysicalPath" -ForegroundColor White
Write-Host "3. Test the website again at: http://localhost" -ForegroundColor White
Write-Host "4. Check Windows Event Logs for any additional errors" -ForegroundColor White
Write-Host ""
Write-Host "If issues persist:" -ForegroundColor Yellow
Write-Host "- Run: iisreset" -ForegroundColor White
Write-Host "- Check Windows Event Logs" -ForegroundColor White
Write-Host "- Verify all application files are deployed correctly" -ForegroundColor White











