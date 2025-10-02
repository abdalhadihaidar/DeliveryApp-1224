# Test Database Connection Script
# This script tests the database connection to SmarterASP

param(
    [string]$Server = "sql6030.site4now.net",
    [string]$Database = "db_abd52c_sa",
    [string]$UserId = "db_abd52c_sa_admin",
    [string]$Password = "RUN404error"
)

Write-Host "Testing database connection to SmarterASP..." -ForegroundColor Green
Write-Host "Server: $Server" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host "User: $UserId" -ForegroundColor Yellow

# Test connection using .NET SqlConnection
try {
    Add-Type -AssemblyName "System.Data"
    
    $connectionString = "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;TrustServerCertificate=True;Connection Timeout=30;"
    
    Write-Host "`nConnection String:" -ForegroundColor Cyan
    Write-Host $connectionString -ForegroundColor Gray
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    
    Write-Host "`nAttempting to connect..." -ForegroundColor Yellow
    $connection.Open()
    
    if ($connection.State -eq "Open") {
        Write-Host "✅ SUCCESS: Database connection established!" -ForegroundColor Green
        
        # Test a simple query
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT @@VERSION"
        $result = $command.ExecuteScalar()
        
        Write-Host "`nDatabase Version:" -ForegroundColor Cyan
        Write-Host $result -ForegroundColor Gray
        
        # Test database existence
        $command.CommandText = "SELECT DB_NAME()"
        $dbName = $command.ExecuteScalar()
        Write-Host "`nConnected to database: $dbName" -ForegroundColor Green
        
    } else {
        Write-Host "❌ FAILED: Connection state is $($connection.State)" -ForegroundColor Red
    }
    
    $connection.Close()
    
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nDetailed Error Information:" -ForegroundColor Yellow
    Write-Host "Exception Type: $($_.Exception.GetType().FullName)" -ForegroundColor Gray
    Write-Host "Error Number: $($_.Exception.Number)" -ForegroundColor Gray
    Write-Host "State: $($_.Exception.State)" -ForegroundColor Gray
    Write-Host "Class: $($_.Exception.Class)" -ForegroundColor Gray
    
    if ($_.Exception.InnerException) {
        Write-Host "Inner Exception: $($_.Exception.InnerException.Message)" -ForegroundColor Gray
    }
}

Write-Host "`nConnection test completed." -ForegroundColor Green
