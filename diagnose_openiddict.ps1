# Diagnose OpenIddict Configuration
# This script checks the current OpenIddict client configuration

Write-Host "🔍 Diagnosing OpenIddict Configuration..." -ForegroundColor Yellow

# Database connection details from env.waselsy
$connectionString = "Data Source=sql6030.site4now.net;Initial Catalog=db_abd52c_sa;User Id=db_abd52c_sa_admin;Password=RUN404error"

# SQL command to check OpenIddict applications
$sqlCommand = @"
SELECT 
    ClientId,
    ApplicationType,
    ClientType,
    ConsentType,
    DisplayName,
    ClientUri,
    RedirectUris,
    PostLogoutRedirectUris,
    Permissions,
    Requirements,
    Settings,
    ClientSecret
FROM OpenIddictApplications
ORDER BY ClientId;

SELECT 
    Id,
    UserName,
    Email,
    EmailConfirmed,
    PhoneNumberConfirmed,
    LockoutEnabled,
    AccessFailedCount
FROM AbpUsers 
WHERE Email = 'admin@waselsy.com';

SELECT 
    u.Email,
    r.Name as RoleName
FROM AbpUsers u
JOIN AbpUserRoles ur ON u.Id = ur.UserId
JOIN AbpRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@waselsy.com';
"@

try {
    Write-Host "📊 Executing diagnostic queries..." -ForegroundColor Blue
    
    $sqlcmdArgs = @(
        "-S", "sql6030.site4now.net"
        "-d", "db_abd52c_sa"
        "-U", "db_abd52c_sa_admin"
        "-P", "RUN404error"
        "-Q", $sqlCommand
    )
    
    $result = & sqlcmd @sqlcmdArgs 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Diagnostic completed successfully!" -ForegroundColor Green
        Write-Host "`n📋 Results:" -ForegroundColor Cyan
        Write-Host $result -ForegroundColor Gray
        
        # Check if DeliveryApp_App exists
        if ($result -match "DeliveryApp_App") {
            Write-Host "`n✅ DeliveryApp_App client found in database" -ForegroundColor Green
        } else {
            Write-Host "`n❌ DeliveryApp_App client NOT found in database" -ForegroundColor Red
        }
        
        # Check if admin user exists
        if ($result -match "admin@waselsy.com") {
            Write-Host "✅ Admin user found in database" -ForegroundColor Green
        } else {
            Write-Host "❌ Admin user NOT found in database" -ForegroundColor Red
        }
        
    } else {
        Write-Host "❌ Diagnostic failed!" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Error executing diagnostic: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 You may need to run the SQL script manually in SQL Server Management Studio" -ForegroundColor Yellow
}

Write-Host "`n🔧 Expected Configuration:" -ForegroundColor Cyan
Write-Host "ClientId: DeliveryApp_App" -ForegroundColor White
Write-Host "ClientSecret: YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP" -ForegroundColor White
Write-Host "ApplicationType: confidential" -ForegroundColor White
Write-Host "ClientType: confidential" -ForegroundColor White
Write-Host "Admin Email: admin@waselsy.com" -ForegroundColor White
Write-Host "Admin Password: Admin123!" -ForegroundColor White
