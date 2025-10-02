-- Final AppUser Data Migration Script
-- Based on actual table structure: AppUsers only has Id, ProfileImageUrl, ReviewReason, ReviewStatus

PRINT 'Starting AppUser data migration...';
PRINT 'AppUsers table structure: Id, ProfileImageUrl, ReviewReason, ReviewStatus';

-- Step 1: Check current state
PRINT 'Checking current data counts...';
SELECT 'AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Step 2: Check which AppUsers don't exist in AbpUsers
PRINT 'Checking for AppUsers not in AbpUsers...';
SELECT 'AppUsers not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Step 3: Update existing AbpUsers with AppUser-specific data
PRINT 'Updating existing AbpUsers with AppUser data...';
UPDATE abu
SET 
    ProfileImageUrl = ISNULL(au.ProfileImageUrl, abu.ProfileImageUrl),
    ReviewStatus = ISNULL(au.ReviewStatus, abu.ReviewStatus),
    ReviewReason = ISNULL(au.ReviewReason, abu.ReviewReason),
    Discriminator = 'AppUser'  -- Set discriminator to AppUser for users with AppUser data
FROM AbpUsers abu
INNER JOIN AppUsers au ON abu.Id = au.Id;

DECLARE @UpdatedRows INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedRows AS VARCHAR(10)) + ' existing users in AbpUsers with AppUser data.';

-- Step 4: Check for any AppUsers that don't exist in AbpUsers
-- These would be orphaned AppUser records without corresponding AbpUsers
PRINT 'Checking for orphaned AppUser records...';
SELECT 'Orphaned AppUsers (not in AbpUsers):' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Step 5: Show sample of updated data
PRINT 'Sample of updated AbpUsers data:';
SELECT TOP 3 
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

-- Step 6: Final verification
PRINT 'Final verification...';
SELECT 'Final AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'Final AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Check if all AppUsers now have corresponding AbpUsers
SELECT 'AppUsers with corresponding AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
INNER JOIN AbpUsers abu ON au.Id = abu.Id;

SELECT 'Orphaned AppUsers remaining:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

PRINT 'Migration completed successfully!';
PRINT 'All existing AbpUsers have been updated with AppUser-specific data.';
