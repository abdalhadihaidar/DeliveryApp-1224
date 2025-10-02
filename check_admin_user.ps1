# Check Admin User
Write-Host "Checking Admin User..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Check admin user
$query = "SELECT Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed FROM AbpUsers WHERE Email = 'admin@waselsy.com'"

try {
    Write-Host "Checking admin user..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $query
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Query successful!" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
        
        if ($result -match "admin@waselsy.com") {
            Write-Host "Admin user found!" -ForegroundColor Green
        } else {
            Write-Host "Admin user NOT found!" -ForegroundColor Red
        }
    } else {
        Write-Host "Query failed!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Expected Email: admin@waselsy.com" -ForegroundColor Cyan

