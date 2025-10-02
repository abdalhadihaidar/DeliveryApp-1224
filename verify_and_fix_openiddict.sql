-- Verify and Fix OpenIddict Client Configuration
-- This script checks the current state and applies the complete fix

-- Step 1: Check current state
SELECT 
    'BEFORE FIX' as Status,
    ClientId,
    ApplicationType,
    ClientType,
    ClientSecret,
    RedirectUris,
    Requirements,
    Settings
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Step 2: Apply complete fix
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
    Settings = '{"AccessTokenLifetime":"86400","RefreshTokenLifetime":"2592000","AuthorizationCodeLifetime":"300","DeviceCodeLifetime":"300","UserCodeLifetime":"300","SlidingRefreshTokenLifetime":"true","RequirePkce":"true","RequireClientSecret":"true"}'
WHERE ClientId = 'DeliveryApp_App';

-- Step 3: Verify the fix
SELECT 
    'AFTER FIX' as Status,
    ClientId,
    ApplicationType,
    ClientType,
    ClientSecret,
    RedirectUris,
    Requirements,
    Settings
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Step 4: Check if client secret matches environment variable
SELECT 
    'SECRET COMPARISON' as Status,
    ClientSecret,
    CASE 
        WHEN ClientSecret = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP' 
        THEN 'MATCHES ENVIRONMENT' 
        ELSE 'DOES NOT MATCH ENVIRONMENT' 
    END as SecretStatus
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';
