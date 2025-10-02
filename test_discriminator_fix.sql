-- Test script to verify discriminator fix
-- Run this after applying the discriminator fix to verify it worked

PRINT 'Testing discriminator fix...';

-- Test 1: Check discriminator distribution
PRINT 'Test 1: Discriminator distribution';
SELECT 'Discriminator distribution:' as Test, 
       ISNULL(Discriminator, 'NULL') as DiscriminatorValue, 
       COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator;

-- Test 2: Check for empty discriminators
PRINT 'Test 2: Empty discriminators check';
SELECT 'Empty discriminators:' as Test, 
       COUNT(*) as Count
FROM AbpUsers
WHERE Discriminator IS NULL OR Discriminator = '';

-- Test 3: Sample data verification
PRINT 'Test 3: Sample data verification';
SELECT TOP 3 
    'Sample data:' as Test,
    Id, 
    UserName, 
    Email, 
    Name,
    Discriminator,
    CASE 
        WHEN ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '' THEN 'Has ProfileImageUrl'
        WHEN ReviewStatus IS NOT NULL AND ReviewStatus != '' THEN 'Has ReviewStatus'
        WHEN ReviewReason IS NOT NULL AND ReviewReason != '' THEN 'Has ReviewReason'
        ELSE 'No AppUser properties'
    END as AppUserProperties
FROM AbpUsers
ORDER BY CreationTime DESC;

-- Test 4: Verify AppUser discriminators
PRINT 'Test 4: AppUser discriminator verification';
SELECT 'AppUser discriminators:' as Test,
       COUNT(*) as Count
FROM AbpUsers
WHERE Discriminator = 'AppUser';

-- Test 5: Verify IdentityUser discriminators
PRINT 'Test 5: IdentityUser discriminator verification';
SELECT 'IdentityUser discriminators:' as Test,
       COUNT(*) as Count
FROM AbpUsers
WHERE Discriminator = 'IdentityUser';

-- Test 6: Check for users with AppUser properties but wrong discriminator
PRINT 'Test 6: Users with AppUser properties but wrong discriminator';
SELECT 'Users with AppUser properties but IdentityUser discriminator:' as Test,
       COUNT(*) as Count
FROM AbpUsers
WHERE Discriminator = 'IdentityUser'
  AND ((ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '') 
       OR (ReviewStatus IS NOT NULL AND ReviewStatus != '') 
       OR (ReviewReason IS NOT NULL AND ReviewReason != ''));

-- Summary
PRINT 'Test Summary:';
SELECT 
    'Total users:' as Metric,
    COUNT(*) as Value
FROM AbpUsers
UNION ALL
SELECT 
    'Users with proper discriminators:' as Metric,
    COUNT(*) as Value
FROM AbpUsers
WHERE Discriminator IN ('IdentityUser', 'AppUser')
UNION ALL
SELECT 
    'Users with empty discriminators:' as Metric,
    COUNT(*) as Value
FROM AbpUsers
WHERE Discriminator IS NULL OR Discriminator = '';

PRINT 'Discriminator fix test completed!';
