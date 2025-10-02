# PowerShell script to fix discriminator issue
# This script runs the SQL fix for the discriminator problem

Write-Host "Starting discriminator fix..." -ForegroundColor Green

# Get the current directory
$currentDir = Get-Location
$sqlFile = Join-Path $currentDir "fix_discriminator_issue.sql"

# Check if SQL file exists
if (-not (Test-Path $sqlFile)) {
    Write-Host "Error: fix_discriminator_issue.sql not found in current directory" -ForegroundColor Red
    exit 1
}

# Database connection parameters (adjust as needed)
$serverName = "localhost"
$databaseName = "DeliveryAppDb"
$connectionString = "Server=$serverName;Database=$databaseName;Integrated Security=true;TrustServerCertificate=true;"

Write-Host "Running discriminator fix SQL script..." -ForegroundColor Yellow
Write-Host "Database: $databaseName" -ForegroundColor Cyan
Write-Host "Server: $serverName" -ForegroundColor Cyan

try {
    # Read the SQL file content
    $sqlContent = Get-Content $sqlFile -Raw
    
    # Execute the SQL script
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlContent -Verbose
    
    Write-Host "Discriminator fix completed successfully!" -ForegroundColor Green
    Write-Host "You can now run the application without discriminator errors." -ForegroundColor Green
}
catch {
    Write-Host "Error executing SQL script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check your database connection and try again." -ForegroundColor Red
    exit 1
}

Write-Host "Script completed." -ForegroundColor Green
