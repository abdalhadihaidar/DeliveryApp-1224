# Fix OpenIddict Client Secret with PBKDF2 Hashing
Write-Host "Fixing OpenIddict client secret with PBKDF2 hashing..." -ForegroundColor Green

# Database connection
$server = "sql6030.site4now.net"
$database = "db_abd52c_sa"
$username = "db_abd52c_sa_admin"
$password = "RUN404error"

# The plain text client secret
$plainTextSecret = "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP"

# Create PBKDF2 hash (OpenIddict standard)
Add-Type -AssemblyName System.Security

# Generate a random salt (16 bytes)
$salt = New-Object byte[] 16
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($salt)

# Create PBKDF2 hash
$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($plainTextSecret, $salt, 10000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$hashBytes = $pbkdf2.GetBytes(32)

# Combine salt + hash and encode as base64
$combinedBytes = $salt + $hashBytes
$hashedSecret = [System.Convert]::ToBase64String($combinedBytes)

Write-Host "Plain text secret: $plainTextSecret" -ForegroundColor Yellow
Write-Host "PBKDF2 hashed secret: $hashedSecret" -ForegroundColor Yellow

# Update the client secret with the PBKDF2 hashed version
$updateQuery = @"
UPDATE OpenIddictApplications 
SET ClientSecret = '$hashedSecret'
WHERE ClientId = 'DeliveryApp_App'
"@

try {
    Write-Host "Updating client secret with PBKDF2 hash..." -ForegroundColor Yellow
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
            Write-Host "✅ Client secret updated correctly with PBKDF2 hash!" -ForegroundColor Green
        } else {
            Write-Host "❌ Client secret update failed!" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "Verification error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart your backend application" -ForegroundColor White
Write-Host "2. Test authentication with the same plain text client secret" -ForegroundColor White
Write-Host "3. OpenIddict should now properly validate the client secret" -ForegroundColor White
