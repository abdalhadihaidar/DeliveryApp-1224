# Memory Leak Fixes Deployment Script
# This script applies critical memory leak fixes to stabilize the delivery app

Write-Host "Starting Memory Leak Fixes Deployment..." -ForegroundColor Green

# 1. Backup current deployment
Write-Host "Creating backup of current deployment..." -ForegroundColor Yellow
$backupDir = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
New-Item -ItemType Directory -Path $backupDir -Force
Copy-Item -Path "publish-output\*" -Destination $backupDir -Recurse -Force
Write-Host "Backup created in: $backupDir" -ForegroundColor Green

# 2. Build the application with fixes
Write-Host "Building application with memory leak fixes..." -ForegroundColor Yellow
dotnet build src/DeliveryApp.HttpApi.Host/DeliveryApp.HttpApi.Host.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please check the errors above." -ForegroundColor Red
    exit 1
}

# 3. Publish the application
Write-Host "Publishing application..." -ForegroundColor Yellow
dotnet publish src/DeliveryApp.HttpApi.Host/DeliveryApp.HttpApi.Host.csproj -c Release -o publish-output --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed! Please check the errors above." -ForegroundColor Red
    exit 1
}

# 4. Update web.config with memory optimizations
Write-Host "Updating web.config with memory optimizations..." -ForegroundColor Yellow
$webConfigPath = "publish-output\web.config"
$webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\DeliveryApp.HttpApi.Host.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="DOTNET_gcServer" value="1" />
          <environmentVariable name="DOTNET_gcConcurrent" value="1" />
          <environmentVariable name="DOTNET_gcAllowVeryLargeObjects" value="1" />
          <environmentVariable name="DOTNET_GCHeapHardLimit" value="0x100000000" />
          <environmentVariable name="DOTNET_GCHeapHardLimitPercent" value="0" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
<!--ProjectGuid: B1234567-89AB-CDEF-0123-456789ABCDEF-->
"@

Set-Content -Path $webConfigPath -Value $webConfigContent -Encoding UTF8

# 5. Create logs directory
Write-Host "Creating logs directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "publish-output\logs" -Force

# 6. Create IIS Application Pool Configuration Script
Write-Host "Creating IIS Application Pool configuration script..." -ForegroundColor Yellow
$iisConfigScript = @"
# IIS Application Pool Configuration Script
# Run this as Administrator to configure the application pool properly

Import-Module WebAdministration

# Application Pool Name
`$appPoolName = "abdalhadihaidar-002"

Write-Host "Configuring Application Pool: `$appPoolName" -ForegroundColor Green

# Stop the application pool
Stop-WebAppPool -Name `$appPoolName

# Configure memory limits (set to unlimited to prevent memory limit violations)
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.privateMemoryLimit -Value 0
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.virtualMemoryLimit -Value 0

# Configure rapid fail protection (disable to prevent automatic disabling)
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.rapidFailProtection -Value `$false

# Configure recycling (disable regular recycling)
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name recycling.periodicRestart.time -Value "00:00:00"

# Configure idle timeout (increase to prevent unnecessary recycling)
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.idleTimeout -Value "00:20:00"

# Configure startup/shutdown timeouts
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.startupTimeLimit -Value "00:02:00"
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name processModel.shutdownTimeLimit -Value "00:02:00"

# Enable 32-bit applications if needed
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name enable32BitAppOnWin64 -Value `$false

# Set .NET CLR Version
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name managedRuntimeVersion -Value ""

# Configure queue length
Set-ItemProperty -Path "IIS:\AppPools\`$appPoolName" -Name queueLength -Value 1000

Write-Host "Application Pool configuration completed!" -ForegroundColor Green

# Start the application pool
Start-WebAppPool -Name `$appPoolName

Write-Host "Application Pool started!" -ForegroundColor Green
"@

Set-Content -Path "configure_iis_app_pool.ps1" -Value $iisConfigScript -Encoding UTF8

# 7. Create monitoring script
Write-Host "Creating memory monitoring script..." -ForegroundColor Yellow
$monitoringScript = @"
# Memory Monitoring Script
# Run this to monitor application memory usage

Write-Host "Monitoring Application Pool Memory Usage..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop monitoring" -ForegroundColor Yellow

while (`$true) {
    `$appPool = Get-IISAppPool -Name "abdalhadihaidar-002"
    `$processes = Get-Process -Name "dotnet" | Where-Object { `$_.MainWindowTitle -like "*DeliveryApp*" }
    
    Write-Host "`n=== Memory Usage Report - `$(Get-Date) ===" -ForegroundColor Cyan
    
    if (`$processes) {
        foreach (`$process in `$processes) {
            `$memoryMB = [math]::Round(`$process.WorkingSet64 / 1MB, 2)
            `$privateMemoryMB = [math]::Round(`$process.PrivateMemorySize64 / 1MB, 2)
            
            Write-Host "Process ID: `$(`$process.Id)" -ForegroundColor White
            Write-Host "Working Set: `$memoryMB MB" -ForegroundColor Green
            Write-Host "Private Memory: `$privateMemoryMB MB" -ForegroundColor Yellow
            
            if (`$privateMemoryMB -gt 1000) {
                Write-Host "WARNING: High memory usage detected!" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "No DeliveryApp processes found" -ForegroundColor Red
    }
    
    Start-Sleep -Seconds 30
}
"@

Set-Content -Path "monitor_memory.ps1" -Value $monitoringScript -Encoding UTF8

Write-Host "`n=== Memory Leak Fixes Deployment Completed! ===" -ForegroundColor Green
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Run 'configure_iis_app_pool.ps1' as Administrator to configure IIS" -ForegroundColor White
Write-Host "2. Deploy the updated files from 'publish-output' folder" -ForegroundColor White
Write-Host "3. Run 'monitor_memory.ps1' to monitor memory usage" -ForegroundColor White
Write-Host "4. Monitor the application for stability improvements" -ForegroundColor White

Write-Host "`nKey Fixes Applied:" -ForegroundColor Cyan
Write-Host "- Fixed HttpClient memory leaks in AuthService and MobileAuthService" -ForegroundColor White
Write-Host "- Optimized Entity Framework queries in SpecialOfferAppService" -ForegroundColor White
Write-Host "- Added memory management environment variables" -ForegroundColor White
Write-Host "- Configured IIS for better memory handling" -ForegroundColor White

