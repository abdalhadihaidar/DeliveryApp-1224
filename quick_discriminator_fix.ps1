# Quick Discriminator Fix Script
# This script runs the SQL fix immediately to resolve the discriminator issue

Write-Host "Quick Discriminator Fix" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green
Write-Host ""

# Database connection parameters (adjust as needed)
$serverName = "localhost"
$databaseName = "DeliveryAppDb"
$connectionString = "Server=$serverName;Database=$databaseName;Integrated Security=true;TrustServerCertificate=true;"

Write-Host "Database: $databaseName" -ForegroundColor Cyan
Write-Host "Server: $serverName" -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "Step 1: Checking current discriminator values..." -ForegroundColor Yellow
    
    # Check current discriminator distribution
    $checkQuery = @"
SELECT 'Current discriminator distribution:' as Info, 
       ISNULL(Discriminator, 'NULL') as DiscriminatorValue, 
       COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator
"@
    
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $checkQuery
    
    Write-Host ""
    Write-Host "Step 2: Fixing discriminator values..." -ForegroundColor Yellow
    
    # Fix discriminator values
    $fixQuery = @"
-- Update users with empty discriminator to 'IdentityUser' by default
UPDATE AbpUsers 
SET Discriminator = 'IdentityUser'
WHERE Discriminator IS NULL OR Discriminator = '';

-- Update users with AppUser-specific properties to 'AppUser'
UPDATE AbpUsers 
SET Discriminator = 'AppUser'
WHERE (ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '') 
   OR (ReviewStatus IS NOT NULL AND ReviewStatus != '') 
   OR (ReviewReason IS NOT NULL AND ReviewReason != '');
"@
    
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $fixQuery
    
    Write-Host ""
    Write-Host "Step 3: Verifying the fix..." -ForegroundColor Yellow
    
    # Verify the fix
    $verifyQuery = @"
SELECT 'Final discriminator distribution:' as Info, 
       Discriminator, 
       COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator;

SELECT 'Empty discriminators remaining:' as Info, 
       COUNT(*) as Count
FROM AbpUsers
WHERE Discriminator IS NULL OR Discriminator = '';
"@
    
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $verifyQuery
    
    Write-Host ""
    Write-Host "Discriminator fix completed successfully!" -ForegroundColor Green
    Write-Host "You can now run the application without discriminator errors." -ForegroundColor Green
}
catch {
    Write-Host "Error executing SQL script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check your database connection and try again." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Script completed." -ForegroundColor Green
