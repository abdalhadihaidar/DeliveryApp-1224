# Create Admin User - Final Version
Write-Host "Creating admin@waselsy.com user..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Create admin user
$createQuery = "INSERT INTO AbpUsers (Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed, LockoutEnabled, AccessFailedCount, CreationTime, ConcurrencyStamp, ExtraProperties, IsDeleted) VALUES (NEWID(), 'admin@waselsy.com', 'admin@waselsy.com', 1, 1, 1, 0, GETUTCDATE(), NEWID(), '{}', 0)"

try {
    Write-Host "Creating admin user..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $createQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Admin user created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create admin user!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Add user to admin role
$roleQuery = "INSERT INTO AbpUserRoles (UserId, RoleId) SELECT u.Id, r.Id FROM AbpUsers u, AbpRoles r WHERE u.Email = 'admin@waselsy.com' AND r.Name = 'admin'"

try {
    Write-Host "Adding user to admin role..." -ForegroundColor Yellow
    $roleResult = sqlcmd -S $server -d $database -U $username -P $password -Q $roleQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "User added to admin role successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to add user to admin role!" -ForegroundColor Red
        Write-Host $roleResult -ForegroundColor Red
    }
} catch {
    Write-Host "Role assignment error: $($_.Exception.Message)" -ForegroundColor Red
}

# Verify the user was created
Write-Host "Verifying user creation..." -ForegroundColor Cyan
$verifyQuery = "SELECT Id, UserName, Email, EmailConfirmed FROM AbpUsers WHERE Email = 'admin@waselsy.com'"

try {
    $verifyResult = sqlcmd -S $server -d $database -U $username -P $password -Q $verifyQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Verification result:" -ForegroundColor Green
        Write-Host $verifyResult -ForegroundColor White
        
        if ($verifyResult -match "admin@waselsy.com") {
            Write-Host "Admin user created successfully!" -ForegroundColor Green
        } else {
            Write-Host "Admin user creation failed!" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Verification error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Login Credentials:" -ForegroundColor Yellow
Write-Host "Email: admin@waselsy.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White

