-- Check the actual structure of both tables
-- This will help us understand what columns exist

-- Check AppUsers table structure
SELECT 'AppUsers Table Structure:' as TableName
UNION ALL
SELECT 'Column: ' + COLUMN_NAME + ' | Type: ' + DATA_TYPE + ' | Nullable: ' + IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AppUsers'
ORDER BY ORDINAL_POSITION;

-- Check AbpUsers table structure  
SELECT 'AbpUsers Table Structure:' as TableName
UNION ALL
SELECT 'Column: ' + COLUMN_NAME + ' | Type: ' + DATA_TYPE + ' | Nullable: ' + IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AbpUsers'
ORDER BY ORDINAL_POSITION;

-- Check if AppUsers table has any data
SELECT 'AppUsers Data Sample:' as Description
UNION ALL
SELECT TOP 5 'ID: ' + CAST(Id as VARCHAR(50)) + ' | Other columns: ' + 
    CASE 
        WHEN COLUMN_NAME = 'Id' THEN ''
        ELSE COLUMN_NAME + '=' + ISNULL(CAST(COLUMN_VALUE as VARCHAR(100)), 'NULL') + ' | '
    END
FROM AppUsers
CROSS APPLY (
    SELECT COLUMN_NAME, 
           CASE COLUMN_NAME
               WHEN 'Id' THEN CAST(Id as VARCHAR(100))
               -- Add other columns as needed based on the actual structure
           END as COLUMN_VALUE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AppUsers'
) cols;
