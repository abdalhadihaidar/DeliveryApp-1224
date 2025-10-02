-- Corrected AppUser Data Migration Script
-- This script first checks the table structure and then migrates accordingly

-- Step 1: Check what columns exist in AppUsers table
PRINT 'Checking AppUsers table structure...';
SELECT 'AppUsers Columns:' as Info, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AppUsers'
ORDER BY ORDINAL_POSITION;

-- Step 2: Check what columns exist in AbpUsers table
PRINT 'Checking AbpUsers table structure...';
SELECT 'AbpUsers Columns:' as Info, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AbpUsers'
ORDER BY ORDINAL_POSITION;

-- Step 3: Check current data counts
PRINT 'Checking current data counts...';
SELECT 'AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Step 4: Check if there are any AppUsers that don't exist in AbpUsers
PRINT 'Checking for missing users...';
SELECT 'AppUsers not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Step 5: If AppUsers table has the expected structure, run the migration
-- This will only work if AppUsers has the same columns as AbpUsers
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppUsers' AND COLUMN_NAME = 'UserName')
BEGIN
    PRINT 'AppUsers table has expected structure. Running migration...';
    
    -- Insert missing AppUsers into AbpUsers
    INSERT INTO AbpUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
        PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
        TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, Name, Surname, 
        IsActive, IsDeleted, CreationTime, LastModificationTime, TenantId,
        ProfileImageUrl, ReviewStatus, ReviewReason
    )
    SELECT 
        au.Id,
        au.UserName,
        UPPER(au.UserName) as NormalizedUserName,
        au.Email,
        UPPER(au.Email) as NormalizedEmail,
        ISNULL(au.EmailConfirmed, 0) as EmailConfirmed,
        ISNULL(au.PasswordHash, '') as PasswordHash,
        ISNULL(au.SecurityStamp, NEWID()) as SecurityStamp,
        ISNULL(au.ConcurrencyStamp, NEWID()) as ConcurrencyStamp,
        ISNULL(au.PhoneNumber, '') as PhoneNumber,
        ISNULL(au.PhoneNumberConfirmed, 0) as PhoneNumberConfirmed,
        ISNULL(au.TwoFactorEnabled, 0) as TwoFactorEnabled,
        au.LockoutEnd,
        ISNULL(au.LockoutEnabled, 1) as LockoutEnabled,
        ISNULL(au.AccessFailedCount, 0) as AccessFailedCount,
        au.Name,
        au.Surname,
        ISNULL(au.IsActive, 1) as IsActive,
        ISNULL(au.IsDeleted, 0) as IsDeleted,
        ISNULL(au.CreationTime, GETUTCDATE()) as CreationTime,
        ISNULL(au.LastModificationTime, GETUTCDATE()) as LastModificationTime,
        au.TenantId,
        au.ProfileImageUrl,
        au.ReviewStatus,
        au.ReviewReason
    FROM AppUsers au
    LEFT JOIN AbpUsers abu ON au.Id = abu.Id
    WHERE abu.Id IS NULL;
    
    PRINT 'Migration completed. Inserted missing users.';
    
    -- Update existing AbpUsers with AppUser-specific data
    UPDATE abu
    SET 
        ProfileImageUrl = ISNULL(au.ProfileImageUrl, abu.ProfileImageUrl),
        ReviewStatus = ISNULL(au.ReviewStatus, abu.ReviewStatus),
        ReviewReason = ISNULL(au.ReviewReason, abu.ReviewReason)
    FROM AbpUsers abu
    INNER JOIN AppUsers au ON abu.Id = au.Id;
    
    PRINT 'Updated existing users with AppUser-specific data.';
END
ELSE
BEGIN
    PRINT 'AppUsers table does not have the expected structure.';
    PRINT 'Please check the table structure and adjust the migration script accordingly.';
END

-- Step 6: Verify the migration
PRINT 'Verifying migration...';
SELECT 'After migration - AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'After migration - AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Check if all AppUsers now exist in AbpUsers
SELECT 'AppUsers still not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

PRINT 'Migration verification completed.';
