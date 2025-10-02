-- Fix for discriminator issue: "No discriminators matched the discriminator value ''"
-- This script updates existing users to have proper discriminator values

PRINT 'Starting discriminator fix...';

-- Step 1: Check current discriminator values
PRINT 'Checking current discriminator values...';
SELECT 'Current discriminator distribution:' as Info, 
       ISNULL(Discriminator, 'NULL') as DiscriminatorValue, 
       COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator;

-- Step 2: Update all existing users to have 'IdentityUser' discriminator by default
PRINT 'Updating all users to have IdentityUser discriminator...';
UPDATE AbpUsers 
SET Discriminator = 'IdentityUser'
WHERE Discriminator IS NULL OR Discriminator = '';

DECLARE @UpdatedToIdentityUser INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedToIdentityUser AS VARCHAR(10)) + ' users to have IdentityUser discriminator.';

-- Step 3: Check if there are any AppUser-specific properties and update accordingly
-- If a user has ProfileImageUrl, ReviewStatus, or ReviewReason, they should be AppUser
PRINT 'Checking for users with AppUser-specific properties...';
UPDATE AbpUsers 
SET Discriminator = 'AppUser'
WHERE (ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '') 
   OR (ReviewStatus IS NOT NULL AND ReviewStatus != '') 
   OR (ReviewReason IS NOT NULL AND ReviewReason != '');

DECLARE @UpdatedToAppUser INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedToAppUser AS VARCHAR(10)) + ' users to have AppUser discriminator.';

-- Step 4: Final verification
PRINT 'Final discriminator distribution:';
SELECT 'Final discriminator distribution:' as Info, 
       Discriminator, 
       COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator;

-- Step 5: Show sample data
PRINT 'Sample of updated data:';
SELECT TOP 5 
    Id, 
    UserName, 
    Email, 
    Name,
    Discriminator,
    ProfileImageUrl, 
    ReviewStatus, 
    ReviewReason
FROM AbpUsers
ORDER BY CreationTime DESC;

PRINT 'Discriminator fix completed successfully!';
PRINT 'All users now have proper discriminator values.';
