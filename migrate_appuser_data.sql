-- Data Migration Script: Move AppUser data to AbpUsers table
-- This script migrates existing AppUser data to the AbpUsers table
-- since AppUser now inherits from IdentityUser and uses the same table

-- First, let's check what data exists in both tables
SELECT 'AppUsers table count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'AbpUsers table count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Check if there are any AppUsers that don't exist in AbpUsers
SELECT 'AppUsers not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- If there are AppUsers that don't exist in AbpUsers, we need to insert them
-- This handles the case where users were created directly in AppUsers table
INSERT INTO AbpUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumber, 
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnd, 
    LockoutEnabled, 
    AccessFailedCount, 
    Name, 
    Surname, 
    IsActive, 
    IsDeleted, 
    CreationTime, 
    LastModificationTime, 
    TenantId,
    ProfileImageUrl,
    ReviewStatus,
    ReviewReason
)
SELECT 
    au.Id,
    au.UserName,
    UPPER(au.UserName) as NormalizedUserName,
    au.Email,
    UPPER(au.Email) as NormalizedEmail,
    0 as EmailConfirmed, -- Default to false
    '' as PasswordHash, -- Will need to be set properly
    NEWID() as SecurityStamp,
    NEWID() as ConcurrencyStamp,
    '' as PhoneNumber,
    0 as PhoneNumberConfirmed,
    0 as TwoFactorEnabled,
    NULL as LockoutEnd,
    1 as LockoutEnabled,
    0 as AccessFailedCount,
    au.Name,
    au.Surname,
    1 as IsActive,
    0 as IsDeleted,
    GETUTCDATE() as CreationTime,
    GETUTCDATE() as LastModificationTime,
    au.TenantId,
    au.ProfileImageUrl,
    au.ReviewStatus,
    au.ReviewReason
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Update existing AbpUsers with AppUser-specific data
UPDATE abu
SET 
    ProfileImageUrl = au.ProfileImageUrl,
    ReviewStatus = au.ReviewStatus,
    ReviewReason = au.ReviewReason
FROM AbpUsers abu
INNER JOIN AppUsers au ON abu.Id = au.Id
WHERE (abu.ProfileImageUrl IS NULL OR abu.ProfileImageUrl = '') 
   OR (abu.ReviewStatus IS NULL OR abu.ReviewStatus = '')
   OR (abu.ReviewReason IS NULL);

-- Verify the migration
SELECT 'After migration - AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'After migration - AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Check if all AppUsers now exist in AbpUsers
SELECT 'AppUsers still not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Show sample of migrated data
SELECT TOP 5 
    abu.Id,
    abu.UserName,
    abu.Email,
    abu.Name,
    abu.ProfileImageUrl,
    abu.ReviewStatus,
    abu.ReviewReason
FROM AbpUsers abu
INNER JOIN AppUsers au ON abu.Id = au.Id
ORDER BY abu.CreationTime DESC;
