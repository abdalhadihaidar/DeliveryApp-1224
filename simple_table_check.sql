-- Simple check of table structures

-- Check AppUsers columns
SELECT 'AppUsers Columns:' as Info, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AppUsers'
ORDER BY ORDINAL_POSITION;

-- Check AbpUsers columns
SELECT 'AbpUsers Columns:' as Info, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AbpUsers'
ORDER BY ORDINAL_POSITION;

-- Check if AppUsers has any data
SELECT 'AppUsers Count:' as Info, COUNT(*) as Count FROM AppUsers;

-- Show sample AppUsers data (just the ID for now)
SELECT TOP 3 'AppUsers Sample:' as Info, Id FROM AppUsers;
