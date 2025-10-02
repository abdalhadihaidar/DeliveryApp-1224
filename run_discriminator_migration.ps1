# PowerShell script to run discriminator migration
Write-Host "Running discriminator migration..." -ForegroundColor Green

# SQL Server connection parameters
$server = "localhost"
$database = "DeliveryApp"
$connectionString = "Server=$server;Database=$database;Integrated Security=true;TrustServerCertificate=true;"

try {
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected to database successfully." -ForegroundColor Green
    
    # Read and execute the SQL script
    $sqlScript = Get-Content "add_discriminator_migration.sql" -Raw
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlScript, $connection)
    $command.CommandTimeout = 30
    
    Write-Host "Executing discriminator migration script..." -ForegroundColor Yellow
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Discriminator migration completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host "Script execution finished." -ForegroundColor Blue


