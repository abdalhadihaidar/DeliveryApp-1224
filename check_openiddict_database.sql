-- Check OpenIddict Database State
-- This script shows the current state of the OpenIddictApplications table

-- Check all applications
SELECT 
    'ALL APPLICATIONS' as Status,
    ClientId,
    ApplicationType,
    ClientType,
    ConsentType,
    DisplayName,
    ClientSecret,
    RedirectUris,
    PostLogoutRedirectUris,
    Permissions,
    Requirements,
    Settings
FROM OpenIddictApplications;

-- Check specific DeliveryApp_App
SELECT 
    'DELIVERYAPP_APP SPECIFIC' as Status,
    ClientId,
    ApplicationType,
    ClientType,
    ConsentType,
    DisplayName,
    ClientSecret,
    RedirectUris,
    PostLogoutRedirectUris,
    Permissions,
    Requirements,
    Settings
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

-- Check if client secret matches expected value
SELECT 
    'SECRET VERIFICATION' as Status,
    ClientId,
    ClientSecret,
    CASE 
        WHEN ClientSecret = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP' 
        THEN 'MATCHES EXPECTED SECRET' 
        ELSE 'DOES NOT MATCH EXPECTED SECRET' 
    END as SecretStatus,
    LEN(ClientSecret) as SecretLength
FROM OpenIddictApplications 
WHERE ClientId = 'DeliveryApp_App';

