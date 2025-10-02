-- Complete OpenIddict Client Configuration Fix
-- This script completes the client configuration after the ApplicationType fix

-- Step 1: Verify current state
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
    Settings
FROM AbpOpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Step 2: Complete the client configuration
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

-- Step 3: Verify the complete configuration
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
    Settings
FROM AbpOpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

