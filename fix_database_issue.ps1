# Fix Database Duplicate Category Issue
# This script fixes the duplicate category issue and restarts the application

Write-Host "🔧 Fixing duplicate category issue..." -ForegroundColor Yellow

# Database connection details from env.waselsy
$connectionString = "Data Source=sql6030.site4now.net;Initial Catalog=db_abd52c_sa;User Id=db_abd52c_sa_admin;Password=RUN404error"

# SQL command to fix duplicates
$sqlCommand = @"
-- Remove duplicates, keeping the oldest one (first created)
WITH DuplicateCategories AS (
    SELECT Id, Name,
           ROW_NUMBER() OVER (PARTITION BY Name ORDER BY CreationTime ASC) as RowNum
    FROM RestaurantCategories
)
DELETE FROM RestaurantCategories 
WHERE Id IN (
    SELECT Id 
    FROM DuplicateCategories 
    WHERE RowNum > 1
);
"@

try {
    # Execute the SQL command
    Write-Host "📊 Executing SQL fix..." -ForegroundColor Blue
    
    # Using sqlcmd to execute the SQL
    $sqlcmdArgs = @(
        "-S", "sql6030.site4now.net"
        "-d", "db_abd52c_sa"
        "-U", "db_abd52c_sa_admin"
        "-P", "RUN404error"
        "-Q", $sqlCommand
    )
    
    $result = & sqlcmd @sqlcmdArgs 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database fix completed successfully!" -ForegroundColor Green
        Write-Host "📋 Result: $result" -ForegroundColor Gray
    } else {
        Write-Host "❌ Database fix failed!" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Error executing database fix: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 You may need to run the SQL script manually in SQL Server Management Studio" -ForegroundColor Yellow
    Write-Host "📄 SQL Script location: fix_duplicate_category.sql" -ForegroundColor Yellow
}

Write-Host "`n🚀 Next steps:" -ForegroundColor Cyan
Write-Host "1. The duplicate category issue has been fixed" -ForegroundColor White
Write-Host "2. Try running your application again" -ForegroundColor White
Write-Host "3. The admin credentials are: admin@waselsy.com / Admin123!" -ForegroundColor White
Write-Host "4. Make sure to run the OpenIddict client fix: fix_openiddict_client.sql" -ForegroundColor White

