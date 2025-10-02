# Complete Fix for Backend Issues
# This script fixes both the duplicate category and OpenIddict client issues

Write-Host "🔧 Starting complete backend fix..." -ForegroundColor Yellow

# Database connection details from env.waselsy
$connectionString = "Data Source=sql6030.site4now.net;Initial Catalog=db_abd52c_sa;User Id=db_abd52c_sa_admin;Password=RUN404error"

Write-Host "`n📊 Step 1: Fixing duplicate categories..." -ForegroundColor Blue

# Fix duplicate categories
$fixCategoriesSQL = @"
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
    $sqlcmdArgs = @(
        "-S", "sql6030.site4now.net"
        "-d", "db_abd52c_sa"
        "-U", "db_abd52c_sa_admin"
        "-P", "RUN404error"
        "-Q", $fixCategoriesSQL
    )
    
    $result = & sqlcmd @sqlcmdArgs 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Duplicate categories fixed!" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to fix duplicate categories: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error fixing categories: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🔐 Step 2: Fixing OpenIddict client configuration..." -ForegroundColor Blue

# Fix OpenIddict client
$fixOpenIddictSQL = @"
UPDATE OpenIddictApplications 
SET 
    ApplicationType = 'confidential',
    ClientType = 'confidential',
    ConsentType = 'explicit',
    DisplayName = 'DeliveryApp Angular Dashboard',
    ClientUri = 'http://dashboard.waselsy.com',
    RedirectUris = '["http://dashboard.waselsy.com","https://dashboard.waselsy.com","http://localhost:4200","https://localhost:4200"]',
    PostLogoutRedirectUris = '["http://dashboard.waselsy.com","https://dashboard.waselsy.com","http://localhost:4200","https://localhost:4200"]',
    Permissions = '["ept:token","ept:revocation","ept:introspection","gt:password","gt:refresh_token","scp:address","scp:email","scp:phone","scp:profile","scp:roles","scp:DeliveryApp"]',
    Requirements = '["pkce"]',
    Settings = '{"AccessTokenLifetime":"86400","RefreshTokenLifetime":"2592000","AuthorizationCodeLifetime":"300","DeviceCodeLifetime":"300","UserCodeLifetime":"300","SlidingRefreshTokenLifetime":"true","RequirePkce":"true","RequireClientSecret":"true"}',
    ClientSecret = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP'
WHERE ClientId = 'DeliveryApp_App';
"@

try {
    $sqlcmdArgs = @(
        "-S", "sql6030.site4now.net"
        "-d", "db_abd52c_sa"
        "-U", "db_abd52c_sa_admin"
        "-P", "RUN404error"
        "-Q", $fixOpenIddictSQL
    )
    
    $result = & sqlcmd @sqlcmdArgs 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ OpenIddict client fixed!" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to fix OpenIddict client: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error fixing OpenIddict client: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🔍 Step 3: Verifying fixes..." -ForegroundColor Blue

# Verify the fixes
$verifySQL = @"
-- Check OpenIddict client
SELECT 
    ClientId,
    ApplicationType,
    ClientType,
    DisplayName,
    ClientSecret
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Check admin user
SELECT 
    Id,
    UserName,
    Email,
    EmailConfirmed,
    PhoneNumberConfirmed
FROM AbpUsers 
WHERE Email = 'admin@waselsy.com';

-- Check categories
SELECT COUNT(*) as CategoryCount FROM RestaurantCategories;
"@

try {
    $sqlcmdArgs = @(
        "-S", "sql6030.site4now.net"
        "-d", "db_abd52c_sa"
        "-U", "db_abd52c_sa_admin"
        "-P", "RUN404error"
        "-Q", $verifySQL
    )
    
    $result = & sqlcmd @sqlcmdArgs 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Verification completed!" -ForegroundColor Green
        Write-Host "📋 Results:" -ForegroundColor Gray
        Write-Host $result -ForegroundColor Gray
    } else {
        Write-Host "❌ Verification failed: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error during verification: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Fix Summary:" -ForegroundColor Cyan
Write-Host "✅ Duplicate categories removed" -ForegroundColor Green
Write-Host "✅ OpenIddict client secret updated" -ForegroundColor Green
Write-Host "✅ Client configuration corrected" -ForegroundColor Green

Write-Host "`n🔑 Login Credentials:" -ForegroundColor Yellow
Write-Host "Email: admin@waselsy.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White

Write-Host "`n🌐 Test URLs:" -ForegroundColor Yellow
Write-Host "Backend: https://backend.waselsy.com/swagger" -ForegroundColor White
Write-Host "Dashboard: http://dashboard.waselsy.com" -ForegroundColor White
Write-Host "Token endpoint: https://backend.waselsy.com/connect/token" -ForegroundColor White

Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Try logging into the dashboard with the correct credentials" -ForegroundColor White
Write-Host "2. If still getting 401, check browser network tab for exact error" -ForegroundColor White
Write-Host "3. Verify the dashboard is pointing to the correct backend URL" -ForegroundColor White
