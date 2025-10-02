# Fix OpenIddict Client Configuration
# This script runs the SQL fix to correct the OpenIddict client configuration

Write-Host "🔧 Fixing OpenIddict Client Configuration..." -ForegroundColor Green

# Read connection string from environment
$connectionString = $env:CONNECTION_STRING
if (-not $connectionString) {
    Write-Host "❌ CONNECTION_STRING environment variable not found!" -ForegroundColor Red
    Write-Host "Please set the CONNECTION_STRING environment variable with your database connection string." -ForegroundColor Yellow
    exit 1
}

Write-Host "📋 Connection String: $($connectionString.Substring(0, [Math]::Min(50, $connectionString.Length)))..." -ForegroundColor Cyan

# SQL commands to fix the OpenIddict client
$sqlCommands = @"
-- Fix OpenIddict Client Configuration
UPDATE AbpOpenIddictApplications 
SET ApplicationType = 'confidential'
WHERE ClientId = 'DeliveryApp_App';

UPDATE AbpOpenIddictApplications 
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
    Settings = '{"AccessTokenLifetime":"86400","RefreshTokenLifetime":"2592000","AuthorizationCodeLifetime":"300","DeviceCodeLifetime":"300","UserCodeLifetime":"300","SlidingRefreshTokenLifetime":"true","RequirePkce":"true","RequireClientSecret":"true"}'
WHERE ClientId = 'DeliveryApp_App';

-- Verify the update
SELECT 
    ClientId,
    ApplicationType,
    ClientType,
    ConsentType,
    DisplayName,
    ClientUri,
    RedirectUris,
    Permissions,
    Settings
FROM AbpOpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';
"@

try {
    # Use sqlcmd to execute the SQL commands
    Write-Host "🚀 Executing SQL commands..." -ForegroundColor Yellow
    
    $sqlCommands | sqlcmd -S "sql6030.site4now.net" -d "db_abd52c_sa" -U "db_abd52c_sa_admin" -P "RUN404error" -I
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ OpenIddict client configuration fixed successfully!" -ForegroundColor Green
        Write-Host "🎯 The client should now work properly for authentication." -ForegroundColor Cyan
    } else {
        Write-Host "❌ Failed to execute SQL commands. Exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error executing SQL commands: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Make sure sqlcmd is installed and accessible in your PATH." -ForegroundColor Yellow
}

Write-Host "`n📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Test the authentication endpoint: POST /connect/token" -ForegroundColor White
Write-Host "2. Verify the client credentials work with your frontend" -ForegroundColor White
Write-Host "3. Check the application logs for any remaining issues" -ForegroundColor White

