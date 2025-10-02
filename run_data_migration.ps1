# PowerShell script to run the AppUser data migration
# This script connects to the database and runs the migration SQL

param(
    [string]$ConnectionString = "Server=localhost;Database=DeliveryApp;Trusted_Connection=true;TrustServerCertificate=true;",
    [string]$SqlFile = "migrate_appuser_data.sql"
)

Write-Host "Starting AppUser data migration..." -ForegroundColor Green

# Check if SQL file exists
if (-not (Test-Path $SqlFile)) {
    Write-Host "Error: SQL file '$SqlFile' not found!" -ForegroundColor Red
    exit 1
}

# Read the SQL file
$sqlContent = Get-Content $SqlFile -Raw

try {
    # Execute the SQL script
    Write-Host "Executing migration script..." -ForegroundColor Yellow
    
    # Using sqlcmd to execute the script
    $result = sqlcmd -S "localhost" -d "DeliveryApp" -E -i $SqlFile -o "migration_output.txt"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration completed successfully!" -ForegroundColor Green
        Write-Host "Check 'migration_output.txt' for detailed results." -ForegroundColor Cyan
        
        # Display the output
        if (Test-Path "migration_output.txt") {
            Write-Host "`nMigration Results:" -ForegroundColor Yellow
            Get-Content "migration_output.txt" | ForEach-Object { Write-Host $_ }
        }
    } else {
        Write-Host "Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        if (Test-Path "migration_output.txt") {
            Write-Host "Error details:" -ForegroundColor Red
            Get-Content "migration_output.txt" | ForEach-Object { Write-Host $_ }
        }
    }
} catch {
    Write-Host "Error executing migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nMigration process completed." -ForegroundColor Green
