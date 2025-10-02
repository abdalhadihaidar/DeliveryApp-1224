-- Migration to add discriminator column for AppUser/IdentityUser inheritance
-- This fixes the "No discriminators matched the discriminator value" error

PRINT 'Adding discriminator column to AbpUsers table...';

-- Step 1: Add discriminator column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AbpUsers' AND COLUMN_NAME = 'Discriminator')
BEGIN
    ALTER TABLE AbpUsers ADD Discriminator NVARCHAR(50) NOT NULL DEFAULT 'IdentityUser';
    PRINT 'Added Discriminator column to AbpUsers table.';
END
ELSE
BEGIN
    PRINT 'Discriminator column already exists in AbpUsers table.';
END

-- Step 2: Update existing records to have proper discriminator values
-- All existing users should be AppUser since they have AppUser-specific properties
PRINT 'Updating discriminator values for existing users...';

-- Update users that have corresponding AppUser records to be 'AppUser'
UPDATE abu
SET Discriminator = 'AppUser'
FROM AbpUsers abu
INNER JOIN AppUsers au ON abu.Id = au.Id;

DECLARE @UpdatedToAppUser INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedToAppUser AS VARCHAR(10)) + ' users to have AppUser discriminator.';

-- Update remaining users to be 'IdentityUser' (if any)
UPDATE abu
SET Discriminator = 'IdentityUser'
FROM AbpUsers abu
LEFT JOIN AppUsers au ON abu.Id = au.Id
WHERE au.Id IS NULL;

DECLARE @UpdatedToIdentityUser INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedToIdentityUser AS VARCHAR(10)) + ' users to have IdentityUser discriminator.';

-- Step 3: Verify the discriminator values
PRINT 'Verifying discriminator values...';
SELECT 'Discriminator distribution:' as Info, Discriminator, COUNT(*) as Count
FROM AbpUsers
GROUP BY Discriminator;

-- Step 4: Show sample data
PRINT 'Sample of updated data:';
SELECT TOP 3 
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

PRINT 'Discriminator migration completed successfully!';
