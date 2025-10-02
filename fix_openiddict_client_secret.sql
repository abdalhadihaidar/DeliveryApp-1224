-- Fix OpenIddict Client Secret
-- This script updates the DeliveryApp_App client secret in the database

-- Update the OpenIddict application with the correct client secret
UPDATE [OpenIddictApplications] 
SET [ClientSecret] = 'YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP'
WHERE [ClientId] = 'DeliveryApp_App';

-- Verify the update
SELECT [ClientId], [ClientSecret], [DisplayName] 
FROM [OpenIddictApplications] 
WHERE [ClientId] = 'DeliveryApp_App';

