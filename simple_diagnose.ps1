# Simple OpenIddict Diagnostic
Write-Host "Checking OpenIddict Configuration..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# Simple SQL query
$query = "SELECT ClientId, ClientSecret, ApplicationType, ClientType FROM OpenIddictApplications WHERE ClientId = 'DeliveryApp_App'"

try {
    Write-Host "Executing query..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $query
    
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

Write-Host "Expected ClientSecret: YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP" -ForegroundColor Cyan

