# Create Admin User
Write-Host "Creating admin@waselsy.com user..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# SQL script to create admin user
$sqlScript = @"
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

SELECT 'Admin user created successfully' as Result;
"@

try {
    Write-Host "Creating admin user..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $sqlScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Query successful!" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
    } else {
        Write-Host "Query failed!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Verify the user was created
Write-Host "`nVerifying user creation..." -ForegroundColor Cyan
$verifyQuery = "SELECT Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed FROM AbpUsers WHERE Email = 'admin@waselsy.com'"

try {
    $verifyResult = sqlcmd -S $server -d $database -U $username -P $password -Q $verifyQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Verification successful!" -ForegroundColor Green
        Write-Host $verifyResult -ForegroundColor White
        
        if ($verifyResult -match "admin@waselsy.com") {
            Write-Host "✅ Admin user created successfully!" -ForegroundColor Green
        } else {
            Write-Host "❌ Admin user creation failed!" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Verification error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🔑 Login Credentials:" -ForegroundColor Yellow
Write-Host "Email: admin@waselsy.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White

