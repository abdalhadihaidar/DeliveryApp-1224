# Check All Users
Write-Host "Checking All Users..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Check all users
$query = "SELECT Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed FROM AbpUsers ORDER BY Email"

try {
    Write-Host "Checking all users..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $query
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Query successful!" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
        
        if ($result -match "admin@example.com") {
            Write-Host "Found admin@example.com user!" -ForegroundColor Yellow
        }
        if ($result -match "admin@waselsy.com") {
            Write-Host "Found admin@waselsy.com user!" -ForegroundColor Green
        }
    } else {
        Write-Host "Query failed!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

