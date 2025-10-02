-- Simple script to check the current database state
-- Run this first to see what data exists

-- Check if tables exist
SELECT 'AppUsers table exists:' as Check, 
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AppUsers') 
            THEN 'YES' ELSE 'NO' END as Result

UNION ALL

SELECT 'AbpUsers table exists:' as Check,
       CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AbpUsers') 
            THEN 'YES' ELSE 'NO' END as Result;

-- Check record counts
SELECT 'AppUsers count:' as TableName, COUNT(*) as RecordCount FROM AppUsers
UNION ALL
SELECT 'AbpUsers count:' as TableName, COUNT(*) as RecordCount FROM AbpUsers;

-- Check for missing users
SELECT 'AppUsers not in AbpUsers:' as Description, COUNT(*) as Count
FROM AppUsers au
LEFT JOIN AbpUsers abu ON au.Id = abu.Id
WHERE abu.Id IS NULL;

-- Show sample data from both tables
SELECT 'Sample AppUsers:' as TableName, TOP 3 Id, UserName, Email, Name, ProfileImageUrl, ReviewStatus FROM AppUsers
UNION ALL
SELECT 'Sample AbpUsers:' as TableName, TOP 3 Id, UserName, Email, Name, ProfileImageUrl, ReviewStatus FROM AbpUsers;
