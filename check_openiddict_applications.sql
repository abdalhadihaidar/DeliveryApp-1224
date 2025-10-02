-- Check all OpenIddict applications in the database
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

