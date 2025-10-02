# Test script to verify authentication token generation
# This script tests the authentication endpoints to ensure they return real tokens instead of "N/A"

param(
    [string]$BaseUrl = "https://localhost:44356",
    [string]$TestEmail = "test@example.com",
    [string]$TestPassword = "TestPassword123!"
)

Write-Host "🔐 Testing Authentication Token Generation" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Test Email: $TestEmail" -ForegroundColor Yellow
Write-Host ""

# Test 1: Direct OpenIddict Token Endpoint
Write-Host "Test 1: Testing OpenIddict /connect/token endpoint" -ForegroundColor Cyan
try {
    $tokenEndpoint = "$BaseUrl/connect/token"
    $body = @{
        grant_type = "password"
        username = $TestEmail
        password = $TestPassword
        client_id = "DeliveryApp_App"
        client_secret = "DeliveryApp_App"
        scope = "DeliveryApp offline_access"
    }
    
    $response = Invoke-RestMethod -Uri $tokenEndpoint -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"
    
    if ($response.access_token -and $response.access_token -ne "N/A") {
        Write-Host "✅ OpenIddict token generation: SUCCESS" -ForegroundColor Green
        Write-Host "   Access Token: $($response.access_token.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Token Type: $($response.token_type)" -ForegroundColor Gray
        Write-Host "   Expires In: $($response.expires_in) seconds" -ForegroundColor Gray
    } else {
        Write-Host "❌ OpenIddict token generation: FAILED" -ForegroundColor Red
        Write-Host "   Response: $($response | ConvertTo-Json)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ OpenIddict token generation: ERROR" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: AuthService Login Endpoint
Write-Host "Test 2: Testing AuthService login endpoint" -ForegroundColor Cyan
try {
    $loginEndpoint = "$BaseUrl/api/auth/login/email"
    $loginBody = @{
        email = $TestEmail
        password = $TestPassword
        rememberMe = $false
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri $loginEndpoint -Method Post -Body $loginBody -ContentType "application/json"
    
    if ($response.success -and $response.accessToken -and $response.accessToken -ne "N/A") {
        Write-Host "✅ AuthService login: SUCCESS" -ForegroundColor Green
        Write-Host "   Access Token: $($response.accessToken.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Refresh Token: $($response.refreshToken.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Expires At: $($response.expiresAt)" -ForegroundColor Gray
    } else {
        Write-Host "❌ AuthService login: FAILED" -ForegroundColor Red
        Write-Host "   Response: $($response | ConvertTo-Json)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ AuthService login: ERROR" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: MobileAuthService Login Endpoint
Write-Host "Test 3: Testing MobileAuthService login endpoint" -ForegroundColor Cyan
try {
    $mobileLoginEndpoint = "$BaseUrl/api/app/mobile-auth/login-with-email"
    $mobileLoginBody = @{
        email = $TestEmail
        password = $TestPassword
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri $mobileLoginEndpoint -Method Post -Body $mobileLoginBody -ContentType "application/json"
    
    if ($response.success -and $response.accessToken -and $response.accessToken -ne "N/A") {
        Write-Host "✅ MobileAuthService login: SUCCESS" -ForegroundColor Green
        Write-Host "   Access Token: $($response.accessToken.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Refresh Token: $($response.refreshToken.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Expires At: $($response.expiresAt)" -ForegroundColor Gray
    } else {
        Write-Host "❌ MobileAuthService login: FAILED" -ForegroundColor Red
        Write-Host "   Response: $($response | ConvertTo-Json)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ MobileAuthService login: ERROR" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: UserController Login Endpoint (Dashboard)
Write-Host "Test 4: Testing UserController login endpoint (Dashboard)" -ForegroundColor Cyan
try {
    $userLoginEndpoint = "$BaseUrl/api/app/user/login"
    $userLoginBody = @{
        emailOrPhone = $TestEmail
        password = $TestPassword
        rememberMe = $false
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri $userLoginEndpoint -Method Post -Body $userLoginBody -ContentType "application/json"
    
    if ($response.token -and $response.token -ne "N/A") {
        Write-Host "✅ UserController login: SUCCESS" -ForegroundColor Green
        Write-Host "   Token: $($response.token.Substring(0, 20))..." -ForegroundColor Gray
        Write-Host "   Expires In: $($response.expiresIn) seconds" -ForegroundColor Gray
        Write-Host "   User ID: $($response.user.id)" -ForegroundColor Gray
    } else {
        Write-Host "❌ UserController login: FAILED" -ForegroundColor Red
        Write-Host "   Response: $($response | ConvertTo-Json)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ UserController login: ERROR" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 Authentication Token Generation Test Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "If all tests show SUCCESS, your token generation fix is working correctly!" -ForegroundColor Green
Write-Host "If any tests show FAILED or ERROR, there may be configuration issues." -ForegroundColor Yellow
