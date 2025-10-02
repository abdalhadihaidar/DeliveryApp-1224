# Test OpenIddict Authentication
Write-Host "Testing OpenIddict Authentication..." -ForegroundColor Green

# Test parameters
$baseUrl = "https://localhost:5001"
$clientId = "DeliveryApp_App"
$clientSecret = "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP"

# Test with different users
$testUsers = @(
    @{ username = "admin@example.com"; password = "1q2w3E*" },
    @{ username = "admin@waselsy.com"; password = "1q2w3E*" },
    @{ username = "manager@example.com"; password = "1q2w3E*" }
)

foreach ($user in $testUsers) {
    Write-Host "`nTesting authentication for: $($user.username)" -ForegroundColor Cyan
    
    # Prepare the request body
    $body = @{
        grant_type = "password"
        username = $user.username
        password = $user.password
        client_id = $clientId
        client_secret = $clientSecret
    }
    
    # Convert to form data
    $formData = ($body.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&"
    
    try {
        # Make the authentication request
        $response = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method Post -Body $formData -ContentType "application/x-www-form-urlencoded" -SkipCertificateCheck
        
        if ($response.access_token) {
            Write-Host "✅ Authentication successful for $($user.username)" -ForegroundColor Green
            Write-Host "Access Token: $($response.access_token.Substring(0, 50))..." -ForegroundColor White
            Write-Host "Token Type: $($response.token_type)" -ForegroundColor White
            Write-Host "Expires In: $($response.expires_in) seconds" -ForegroundColor White
            
            if ($response.refresh_token) {
                Write-Host "Refresh Token: $($response.refresh_token.Substring(0, 50))..." -ForegroundColor White
            }
        } else {
            Write-Host "❌ Authentication failed for $($user.username)" -ForegroundColor Red
            Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ Authentication error for $($user.username): $($_.Exception.Message)" -ForegroundColor Red
        
        if ($_.Exception.Response) {
            $errorResponse = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorResponse)
            $errorBody = $reader.ReadToEnd()
            Write-Host "Error Response: $errorBody" -ForegroundColor Red
        }
    }
}

Write-Host "`n🔧 Authentication Test Complete!" -ForegroundColor Yellow
Write-Host "If authentication is still failing, check:" -ForegroundColor White
Write-Host "1. Backend application is running" -ForegroundColor White
Write-Host "2. Database connection is working" -ForegroundColor White
Write-Host "3. User passwords are correct" -ForegroundColor White
Write-Host "4. Client secret matches exactly" -ForegroundColor White
