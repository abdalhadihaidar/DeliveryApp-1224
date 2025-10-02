# Simple Authentication Test
Write-Host "Testing OpenIddict Authentication..." -ForegroundColor Green

$baseUrl = "https://localhost:5001"
$clientId = "DeliveryApp_App"
$clientSecret = "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP"
$username = "admin@example.com"
$password = "Admin123!"

$body = @{
    grant_type = "password"
    username = $username
    password = $password
    client_id = $clientId
    client_secret = $clientSecret
}

$formData = ($body.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&"

try {
    Write-Host "Making authentication request..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method Post -Body $formData -ContentType "application/x-www-form-urlencoded"
    
    if ($response.access_token) {
        Write-Host "SUCCESS: Authentication worked!" -ForegroundColor Green
        Write-Host "Access Token: $($response.access_token.Substring(0, 50))..." -ForegroundColor White
    } else {
        Write-Host "FAILED: No access token received" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
