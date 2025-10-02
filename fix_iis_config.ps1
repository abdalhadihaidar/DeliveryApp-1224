# Fix IIS Configuration for DeliveryApp Backend
# This script helps resolve HTTP 403.18 errors

Write-Host "=== Fixing IIS Configuration for DeliveryApp Backend ===" -ForegroundColor Green

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires administrator privileges. Please run as administrator." -ForegroundColor Red
    exit 1
}

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "1. Checking IIS Installation..." -ForegroundColor Yellow
if (Get-Module -ListAvailable -Name WebAdministration) {
    Write-Host "   ✓ IIS WebAdministration module is available" -ForegroundColor Green
} else {
    Write-Host "   ✗ IIS WebAdministration module not found. Please install IIS with Management Tools." -ForegroundColor Red
    exit 1
}

Write-Host "2. Checking Application Pool..." -ForegroundColor Yellow
$appPoolName = "abdalhadihaidar-002"
$appPool = Get-IISAppPool -Name $appPoolName -ErrorAction SilentlyContinue

if ($appPool) {
    Write-Host "   ✓ Application pool '$appPoolName' exists" -ForegroundColor Green
    
    # Check application pool settings
    Write-Host "   - .NET CLR Version: $($appPool.ManagedRuntimeVersion)" -ForegroundColor Cyan
    Write-Host "   - Process Model Identity: $($appPool.ProcessModel.IdentityType)" -ForegroundColor Cyan
    Write-Host "   - State: $($appPool.State)" -ForegroundColor Cyan
    
    # Ensure application pool is running
    if ($appPool.State -ne "Started") {
        Write-Host "   Starting application pool..." -ForegroundColor Yellow
        Start-WebAppPool -Name $appPoolName
        Start-Sleep -Seconds 3
        Write-Host "   ✓ Application pool started" -ForegroundColor Green
    }
} else {
    Write-Host "   ✗ Application pool '$appPoolName' not found" -ForegroundColor Red
    Write-Host "   Creating application pool..." -ForegroundColor Yellow
    
    New-WebAppPool -Name $appPoolName
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    Start-WebAppPool -Name $appPoolName
    
    Write-Host "   ✓ Application pool created and started" -ForegroundColor Green
}

Write-Host "3. Checking Website Configuration..." -ForegroundColor Yellow
$siteName = "abdalhadihaidar-002"
$site = Get-Website -Name $siteName -ErrorAction SilentlyContinue

if ($site) {
    Write-Host "   ✓ Website '$siteName' exists" -ForegroundColor Green
    Write-Host "   - Physical Path: $($site.PhysicalPath)" -ForegroundColor Cyan
    Write-Host "   - Application Pool: $($site.ApplicationPool)" -ForegroundColor Cyan
    Write-Host "   - State: $($site.State)" -ForegroundColor Cyan
    
    # Ensure website is running
    if ($site.State -ne "Started") {
        Write-Host "   Starting website..." -ForegroundColor Yellow
        Start-Website -Name $siteName
        Write-Host "   ✓ Website started" -ForegroundColor Green
    }
} else {
    Write-Host "   ✗ Website '$siteName' not found" -ForegroundColor Red
}

Write-Host "4. Checking ASP.NET Core Module..." -ForegroundColor Yellow
$aspNetCoreModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue

if ($aspNetCoreModule) {
    Write-Host "   ✓ ASP.NET Core Module V2 is installed" -ForegroundColor Green
} else {
    Write-Host "   ✗ ASP.NET Core Module V2 not found" -ForegroundColor Red
    Write-Host "   Please install ASP.NET Core Hosting Bundle" -ForegroundColor Yellow
}

Write-Host "5. Checking .NET Runtime..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion) {
    Write-Host "   ✓ .NET Runtime version: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "   ✗ .NET Runtime not found" -ForegroundColor Red
}

Write-Host "6. Application Pool Recommendations..." -ForegroundColor Yellow
Write-Host "   - Ensure .NET CLR Version is set to 'No Managed Code'" -ForegroundColor Cyan
Write-Host "   - Use 'ApplicationPoolIdentity' for Process Model Identity" -ForegroundColor Cyan
Write-Host "   - Set Idle Timeout to 0 (no timeout) for production" -ForegroundColor Cyan
Write-Host "   - Enable 32-bit applications: False" -ForegroundColor Cyan

Write-Host "7. Troubleshooting Steps..." -ForegroundColor Yellow
Write-Host "   - Check Windows Event Logs for detailed error information" -ForegroundColor Cyan
Write-Host "   - Verify the application is deployed to the correct physical path" -ForegroundColor Cyan
Write-Host "   - Ensure all required .NET dependencies are installed" -ForegroundColor Cyan
Write-Host "   - Check file permissions on the application directory" -ForegroundColor Cyan

Write-Host "=== IIS Configuration Check Complete ===" -ForegroundColor Green
Write-Host "If issues persist, check the Windows Event Logs for more details." -ForegroundColor Yellow



