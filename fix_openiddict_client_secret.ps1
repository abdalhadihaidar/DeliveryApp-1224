# Fix OpenIddict Client Secret
# This script updates the DeliveryApp_App client secret in the database

Write-Host "Fixing OpenIddict Client Secret..." -ForegroundColor Green

# Use the connection string from web.config
$connectionString = "Data Source=sql6030.site4now.net;Initial Catalog=db_abd52c_sa;User Id=db_abd52c_sa_admin;Password=RUN404error"

Write-Host "Using connection string: $($connectionString.Substring(0, 50))..." -ForegroundColor Yellow

# SQL command to update the client secret
$sqlCommand = @"
UPDATE [OpenIddictApplications] 
SET [ClientSecret] = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP'
WHERE [ClientId] = 'DeliveryApp_App';

SELECT [ClientId], [ClientSecret], [DisplayName] 
FROM [OpenIddictApplications] 
WHERE [ClientId] = 'DeliveryApp_App';
"@

try {
    # Execute the SQL command
    Write-Host "Executing SQL command..." -ForegroundColor Yellow
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlCommand
    
    Write-Host "✅ OpenIddict client secret updated successfully!" -ForegroundColor Green
    Write-Host "The DeliveryApp_App now uses the correct client secret." -ForegroundColor Green
    Write-Host "You can now test the authentication." -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error executing SQL command: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the database is accessible and the connection string is correct." -ForegroundColor Red
    exit 1
}
