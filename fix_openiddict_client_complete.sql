-- Complete OpenIddict Client Fix
-- This script fixes the client secret mismatch and ensures proper configuration

-- First, let's see what's currently in the database
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
WHERE ClientId = 'DeliveryApp_App';

-- Update the client with the correct secret from env.waselsy
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
    Settings,
    ClientSecret
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Also check if the admin user exists
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

-- Check user roles
SELECT 
    u.Email,
    r.Name as RoleName
FROM AbpUsers u
JOIN AbpUserRoles ur ON u.Id = ur.UserId
JOIN AbpRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@waselsy.com';
