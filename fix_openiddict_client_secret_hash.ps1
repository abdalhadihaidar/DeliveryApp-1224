# Fix OpenIddict Client Secret with Proper Hashing
Write-Host "Fixing OpenIddict client secret with proper hashing..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# The plain text client secret
$plainTextSecret = "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP"

# Create a simple hash using .NET's built-in hashing
Add-Type -AssemblyName System.Security
$bytes = [System.Text.Encoding]::UTF8.GetBytes($plainTextSecret)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash($bytes)
$hashedSecret = [System.Convert]::ToBase64String($hashBytes)

Write-Host "Plain text secret: $plainTextSecret" -ForegroundColor Yellow
Write-Host "Hashed secret: $hashedSecret" -ForegroundColor Yellow

# Update the client secret with the hashed version
$updateQuery = @"
UPDATE OpenIddictApplications 
SET ClientSecret = '$hashedSecret'
WHERE ClientId = 'DeliveryApp_App'
"@

try {
    Write-Host "Updating client secret with hash..." -ForegroundColor Yellow
    $result = sqlcmd -S $server -d $database -U $username -P $password -Q $updateQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Client secret updated successfully!" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
    } else {
        Write-Host "Failed to update client secret!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Verify the update
Write-Host "`nVerifying client secret update..." -ForegroundColor Cyan
$verifyQuery = "SELECT ClientId, ClientSecret FROM OpenIddictApplications WHERE ClientId = 'DeliveryApp_App'"

try {
    $verifyResult = sqlcmd -S $server -d $database -U $username -P $password -Q $verifyQuery
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Verification result:" -ForegroundColor Green
        Write-Host $verifyResult -ForegroundColor White
        
        if ($verifyResult -match $hashedSecret) {
            Write-Host "✅ Client secret updated correctly with hash!" -ForegroundColor Green
        } else {
            Write-Host "❌ Client secret update failed!" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Verification error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart your backend application" -ForegroundColor White
Write-Host "2. Test authentication with the same client secret" -ForegroundColor White
Write-Host "3. If still failing, try using the plain text secret in client requests" -ForegroundColor White
