# Check Detailed OpenIddict Client Configuration
Write-Host "Checking detailed OpenIddict client configuration..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Detailed query for DeliveryApp_App
$query = @"
SELECT 
    ClientId,
    ClientSecret,
    ApplicationType,
    ClientType,
    ConsentType,
    DisplayName,
    Permissions,
    Requirements,
    Settings,
    RedirectUris,
    PostLogoutRedirectUris
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App'
"@

try {
    Write-Host "Executing detailed query..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $query
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Query successful!" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
        
        # Check if client exists
        if ($result -match "DeliveryApp_App") {
            Write-Host "`n✅ DeliveryApp_App client found!" -ForegroundColor Green
            
            # Check for required permissions
            if ($result -match "ept:token") {
                Write-Host "✅ Token endpoint permission found" -ForegroundColor Green
            } else {
                Write-Host "❌ Token endpoint permission missing" -ForegroundColor Red
            }
            
            if ($result -match "gt:password") {
                Write-Host "✅ Password grant type found" -ForegroundColor Green
            } else {
                Write-Host "❌ Password grant type missing" -ForegroundColor Red
            }
            
            if ($result -match "confidential") {
                Write-Host "✅ Client type is confidential" -ForegroundColor Green
            } else {
                Write-Host "❌ Client type is not confidential" -ForegroundColor Red
            }
        } else {
            Write-Host "`n❌ DeliveryApp_App client NOT found!" -ForegroundColor Red
        }
    } else {
        Write-Host "Query failed!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nExpected Configuration:" -ForegroundColor Cyan
Write-Host "ClientId: DeliveryApp_App" -ForegroundColor White
Write-Host "ClientSecret: YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP" -ForegroundColor White
Write-Host "ApplicationType: confidential" -ForegroundColor White
Write-Host "ClientType: confidential" -ForegroundColor White
Write-Host "Permissions: ept:token, gt:password, scp:email, scp:profile, scp:roles" -ForegroundColor White

