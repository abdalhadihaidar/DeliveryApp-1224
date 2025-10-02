# Fix OpenIddict Client Secret - Simple Version
Write-Host "Fixing OpenIddict client secret..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Update the client secret
$updateQuery = "UPDATE OpenIddictApplications SET ClientSecret = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP' WHERE ClientId = 'DeliveryApp_App'"

try {
    Write-Host "Updating client secret..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $updateQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Client secret updated successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to update client secret!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Verify the update
Write-Host "Verifying client secret update..." -ForegroundColor Cyan
$verifyQuery = "SELECT ClientId, ClientSecret FROM OpenIddictApplications WHERE ClientId = 'DeliveryApp_App'"

try {
    $verifyResult = sqlcmd -S $server -d $database -U $username -P $password -Q $verifyQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Verification result:" -ForegroundColor Green
        Write-Host $verifyResult -ForegroundColor White
    }
} catch {
    Write-Host "Verification error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart your backend application" -ForegroundColor White
Write-Host "2. Try logging in with admin@waselsy.com / Admin123!" -ForegroundColor White

