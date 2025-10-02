-- Simple check of AppUsers table data and structure

-- Check if AppUsers table exists and has data
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AppUsers')
BEGIN
    PRINT 'AppUsers table exists.';
    
    -- Check row count
    DECLARE @AppUsersCount INT;
    SELECT @AppUsersCount = COUNT(*) FROM AppUsers;
    PRINT 'AppUsers table has ' + CAST(@AppUsersCount AS VARCHAR(10)) + ' records.';
    
    -- If there are records, show the structure
    IF @AppUsersCount > 0
    BEGIN
        PRINT 'AppUsers table structure:';
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'AppUsers'
        ORDER BY ORDINAL_POSITION;
        
        -- Try to show sample data (just the first few columns)
        PRINT 'Sample AppUsers data:';
        SELECT TOP 3 * FROM AppUsers;
    END
    ELSE
    BEGIN
        PRINT 'AppUsers table is empty.';
    END
END
ELSE
BEGIN
    PRINT 'AppUsers table does not exist.';
END

-- Check AbpUsers table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AbpUsers')
BEGIN
    PRINT 'AbpUsers table exists.';
    
    DECLARE @AbpUsersCount INT;
    SELECT @AbpUsersCount = COUNT(*) FROM AbpUsers;
    PRINT 'AbpUsers table has ' + CAST(@AbpUsersCount AS VARCHAR(10)) + ' records.';
END
ELSE
BEGIN
    PRINT 'AbpUsers table does not exist.';
END
