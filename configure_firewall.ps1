# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "This script requires administrator privileges. Please run PowerShell as Administrator and try again."
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "Configuring Windows Firewall for DeliveryApp.Web..." -ForegroundColor Green
Write-Host ""

# Add inbound rule for HTTPS port 44356
try {
    New-NetFirewallRule -DisplayName "DeliveryApp.Web HTTPS" -Direction Inbound -Protocol TCP -LocalPort 44356 -Action Allow
    Write-Host "✓ Inbound rule added successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to add inbound rule: $($_.Exception.Message)" -ForegroundColor Red
}

# Add outbound rule for HTTPS port 44356
try {
    New-NetFirewallRule -DisplayName "DeliveryApp.Web HTTPS Outbound" -Direction Outbound -Protocol TCP -LocalPort 44356 -Action Allow
    Write-Host "✓ Outbound rule added successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to add outbound rule: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Firewall configuration completed!" -ForegroundColor Green
Write-Host "You can now access DeliveryApp.Web from other devices on your network at:" -ForegroundColor Yellow
Write-Host "https://192.168.1.107:44356" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit" 