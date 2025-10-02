# Fix Web.config Duplicate Collection Entry Errors
# This script resolves "Cannot add duplicate collection entry" errors for MIME types and file extensions

param(
    [string]$WebConfigPath = "C:\inetpub\wwwroot\WASEL\web.config"
)

Write-Host "=== Fixing Web.config Duplicate MIME Type Error ===" -ForegroundColor Green

# Check if web.config exists
if (-not (Test-Path $WebConfigPath)) {
    Write-Host "Error: web.config not found at: $WebConfigPath" -ForegroundColor Red
    Write-Host "Please update the WebConfigPath parameter to point to your web.config file" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found web.config at: $WebConfigPath" -ForegroundColor Green

# Backup the original web.config
$backupPath = "$WebConfigPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item $WebConfigPath $backupPath
Write-Host "Created backup: $backupPath" -ForegroundColor Yellow

# Read the web.config content
$webConfigContent = Get-Content $WebConfigPath -Raw

Write-Host "Fixing duplicate collection entries..." -ForegroundColor Yellow

# Fix the staticContent section to remove duplicates before adding new ones
$fixedContent = $webConfigContent -replace '(?s)<staticContent>\s*<mimeMap fileExtension="\.json"', '<staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json"'

$fixedContent = $fixedContent -replace '(?s)<mimeMap fileExtension="\.woff"', '      <remove fileExtension=".woff" />
      <mimeMap fileExtension=".woff"'

$fixedContent = $fixedContent -replace '(?s)<mimeMap fileExtension="\.woff2"', '      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".woff2"'

# Fix the requestFiltering fileExtensions section
$fixedContent = $fixedContent -replace '(?s)<fileExtensions>\s*<add fileExtension="\.config"', '<fileExtensions>
          <remove fileExtension=".config" />
          <add fileExtension=".config"'

$fixedContent = $fixedContent -replace '(?s)<add fileExtension="\.log"', '          <remove fileExtension=".log" />
          <add fileExtension=".log"'

# Write the fixed content back to the file
Set-Content -Path $WebConfigPath -Value $fixedContent -Encoding UTF8

Write-Host "✓ Web.config has been fixed" -ForegroundColor Green

# Verify the fix by checking for duplicate entries
Write-Host "Verifying the fix..." -ForegroundColor Yellow

$content = Get-Content $WebConfigPath -Raw
$jsonCount = ([regex]::Matches($content, 'fileExtension="\.json"')).Count
$woffCount = ([regex]::Matches($content, 'fileExtension="\.woff"')).Count
$woff2Count = ([regex]::Matches($content, 'fileExtension="\.woff2"')).Count

if ($jsonCount -eq 2 -and $woffCount -eq 2 -and $woff2Count -eq 2) {
    Write-Host "✓ Fix verified - Each MIME type now has exactly 2 entries (remove + mimeMap)" -ForegroundColor Green
} else {
    Write-Host "⚠ Warning - Unexpected MIME type count. Manual verification recommended." -ForegroundColor Yellow
    Write-Host "  JSON entries: $jsonCount" -ForegroundColor Cyan
    Write-Host "  WOFF entries: $woffCount" -ForegroundColor Cyan
    Write-Host "  WOFF2 entries: $woff2Count" -ForegroundColor Cyan
}

# Test IIS configuration
Write-Host "Testing IIS configuration..." -ForegroundColor Yellow
try {
    # Try to get the website to test configuration
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    $sites = Get-Website | Where-Object { $_.PhysicalPath -like "*WASEL*" }
    
    if ($sites) {
        Write-Host "Found website(s) with WASEL in path:" -ForegroundColor Green
        foreach ($site in $sites) {
            Write-Host "  - $($site.Name): $($site.PhysicalPath)" -ForegroundColor Cyan
        }
        
        Write-Host "Restarting application pool to apply changes..." -ForegroundColor Yellow
        foreach ($site in $sites) {
            if ($site.ApplicationPool) {
                Restart-WebAppPool -Name $site.ApplicationPool
                Write-Host "  ✓ Restarted application pool: $($site.ApplicationPool)" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "No websites found with WASEL in the path" -ForegroundColor Yellow
        Write-Host "You may need to restart your application pool manually" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Could not automatically restart application pool: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Please restart your application pool manually in IIS Manager" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=== Fix Complete ===" -ForegroundColor Green
Write-Host "The duplicate MIME type error should now be resolved." -ForegroundColor White
Write-Host "If you still encounter issues:" -ForegroundColor Yellow
Write-Host "1. Check Windows Event Logs for more detailed error information" -ForegroundColor White
Write-Host "2. Verify that the web.config file is valid XML" -ForegroundColor White
Write-Host "3. Ensure your application pool is running" -ForegroundColor White
Write-Host "4. Test your website again" -ForegroundColor White
