# Check OpenIddict Applications in Database
Write-Host "Checking OpenIddict Applications..." -ForegroundColor Green

# Use the connection string from web.config
$connectionString = "Data Source=sql6030.site4now.net;Initial Catalog=db_abd52c_sa;User Id=db_abd52c_sa_admin;Password=RUN404error"

Write-Host "Using connection string: $($connectionString.Substring(0, 50))..." -ForegroundColor Yellow

# SQL command to check all applications
$sqlCommand = @"
SELECT 
    [Id],
    [ClientId], 
    [ClientSecret], 
    [DisplayName],
    [ClientType],
    [Permissions],
    [RedirectUris],
    [PostLogoutRedirectUris]
FROM [OpenIddictApplications]
ORDER BY [ClientId];
"@

try {
    # Execute the SQL command
    Write-Host "Executing SQL command..." -ForegroundColor Yellow
    $results = Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlCommand
    
    Write-Host "`n=== OpenIddict Applications ===" -ForegroundColor Cyan
    $results | Format-Table -AutoSize
    
    # Check specifically for DeliveryApp_App
    $deliveryApp = $results | Where-Object { $_.ClientId -eq "DeliveryApp_App" }
    if ($deliveryApp) {
        Write-Host "`n=== DeliveryApp_App Details ===" -ForegroundColor Cyan
        Write-Host "ClientId: $($deliveryApp.ClientId)" -ForegroundColor White
        Write-Host "ClientSecret: $($deliveryApp.ClientSecret)" -ForegroundColor White
        Write-Host "DisplayName: $($deliveryApp.DisplayName)" -ForegroundColor White
        Write-Host "ClientType: $($deliveryApp.ClientType)" -ForegroundColor White
        Write-Host "Permissions: $($deliveryApp.Permissions)" -ForegroundColor White
    } else {
        Write-Host "`n❌ DeliveryApp_App not found in database!" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Error executing SQL command: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

