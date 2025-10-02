-- Create admin@waselsy.com user
-- This script creates the missing admin user for production

-- First, let's check if the user already exists
SELECT Id, UserName, Email FROM AbpUsers WHERE Email = 'admin@waselsy.com';

-- Create the admin user if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM AbpUsers WHERE Email = 'admin@waselsy.com')
BEGIN
    DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @AdminRoleId UNIQUEIDENTIFIER;
    
    -- Get the admin role ID
    SELECT @AdminRoleId = Id FROM AbpRoles WHERE Name = 'admin';
    
    -- Insert the admin user
    INSERT INTO AbpUsers (
        Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed, 
        LockoutEnabled, AccessFailedCount, CreationTime, ConcurrencyStamp,
        ExtraProperties, IsDeleted, DeleterId, DeletionTime
    ) VALUES (
        @UserId, 
        'admin@waselsy.com', 
        'admin@waselsy.com', 
        1, -- EmailConfirmed
        1, -- PhoneNumberConfirmed
        1, -- LockoutEnabled
        0, -- AccessFailedCount
        GETUTCDATE(), -- CreationTime
        NEWID(), -- ConcurrencyStamp
        '{}', -- ExtraProperties
        0, -- IsDeleted
        NULL, -- DeleterId
        NULL -- DeletionTime
    );
    
    -- Add user to admin role if admin role exists
    IF @AdminRoleId IS NOT NULL
    BEGIN
        INSERT INTO AbpUserRoles (UserId, RoleId) VALUES (@UserId, @AdminRoleId);
    END
    
    PRINT 'Admin user created successfully';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- Verify the user was created
SELECT Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed FROM AbpUsers WHERE Email = 'admin@waselsy.com';

-- Check user roles
SELECT 
    u.Email,
    r.Name as RoleName
FROM AbpUsers u
LEFT JOIN AbpUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AbpRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@waselsy.com';

