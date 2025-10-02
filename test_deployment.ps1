# Test DeliveryApp.Web Deployment
# Run this script to verify your deployment is working correctly

param(
    [string]$BaseUrl = "http://backend.waselsy.com",
    [string]$HttpsUrl = "https://backend.waselsy.com"
)

Write-Host "=== Testing DeliveryApp.Web Deployment ===" -ForegroundColor Green

$testResults = @()

# Test 1: Basic HTTP connectivity
Write-Host "1. Testing HTTP connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $BaseUrl -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ HTTP endpoint is responding (Status: $($response.StatusCode))" -ForegroundColor Green
        $testResults += "HTTP: PASS"
    } else {
        Write-Host "   ✗ HTTP endpoint returned status: $($response.StatusCode)" -ForegroundColor Red
        $testResults += "HTTP: FAIL - Status $($response.StatusCode)"
    }
} catch {
    Write-Host "   ✗ HTTP endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "HTTP: FAIL - $($_.Exception.Message)"
}

# Test 2: HTTPS connectivity (if available)
Write-Host "2. Testing HTTPS connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $HttpsUrl -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ HTTPS endpoint is responding (Status: $($response.StatusCode))" -ForegroundColor Green
        $testResults += "HTTPS: PASS"
    } else {
        Write-Host "   ✗ HTTPS endpoint returned status: $($response.StatusCode)" -ForegroundColor Red
        $testResults += "HTTPS: FAIL - Status $($response.StatusCode)"
    }
} catch {
    Write-Host "   ⚠ HTTPS endpoint not available or failed: $($_.Exception.Message)" -ForegroundColor Yellow
    $testResults += "HTTPS: SKIP - $($_.Exception.Message)"
}

# Test 3: API endpoints
Write-Host "3. Testing API endpoints..." -ForegroundColor Yellow

$apiEndpoints = @(
    "/swagger/index.html",
    "/api/app/health",
    "/connect/token"
)

foreach ($endpoint in $apiEndpoints) {
    try {
        $url = "$BaseUrl$endpoint"
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 15
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✓ $endpoint is responding" -ForegroundColor Green
            $testResults += "$endpoint: PASS"
        } else {
            Write-Host "   ✗ $endpoint returned status: $($response.StatusCode)" -ForegroundColor Red
            $testResults += "$endpoint: FAIL - Status $($response.StatusCode)"
        }
    } catch {
        Write-Host "   ✗ $endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += "$endpoint: FAIL - $($_.Exception.Message)"
    }
}

# Test 4: Static content
Write-Host "4. Testing static content..." -ForegroundColor Yellow
try {
    $url = "$BaseUrl/logo.png"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Static content is accessible" -ForegroundColor Green
        $testResults += "Static Content: PASS"
    } else {
        Write-Host "   ✗ Static content returned status: $($response.StatusCode)" -ForegroundColor Red
        $testResults += "Static Content: FAIL - Status $($response.StatusCode)"
    }
} catch {
    Write-Host "   ✗ Static content failed: $($_.Exception.Message)" -ForegroundColor Red
    $testResults += "Static Content: FAIL - $($_.Exception.Message)"
}

# Test 5: Database connectivity (if health endpoint exists)
Write-Host "5. Testing database connectivity..." -ForegroundColor Yellow
try {
    $url = "$BaseUrl/api/app/health"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -eq 200) {
        $content = $response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($content -and $content.status -eq "healthy") {
            Write-Host "   ✓ Database connectivity is healthy" -ForegroundColor Green
            $testResults += "Database: PASS"
        } else {
            Write-Host "   ⚠ Database health check inconclusive" -ForegroundColor Yellow
            $testResults += "Database: UNKNOWN"
        }
    } else {
        Write-Host "   ⚠ Health endpoint not available" -ForegroundColor Yellow
        $testResults += "Database: SKIP"
    }
} catch {
    Write-Host "   ⚠ Database connectivity test skipped" -ForegroundColor Yellow
    $testResults += "Database: SKIP"
}

# Test 6: CORS headers
Write-Host "6. Testing CORS configuration..." -ForegroundColor Yellow
try {
    $url = "$BaseUrl/api/app/health"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10
    $corsHeader = $response.Headers["Access-Control-Allow-Origin"]
    if ($corsHeader) {
        Write-Host "   ✓ CORS headers are present: $corsHeader" -ForegroundColor Green
        $testResults += "CORS: PASS"
    } else {
        Write-Host "   ⚠ CORS headers not found" -ForegroundColor Yellow
        $testResults += "CORS: WARNING"
    }
} catch {
    Write-Host "   ⚠ CORS test skipped" -ForegroundColor Yellow
    $testResults += "CORS: SKIP"
}

# Summary
Write-Host ""
Write-Host "=== Test Results Summary ===" -ForegroundColor Green
foreach ($result in $testResults) {
    if ($result -match "PASS") {
        Write-Host "✓ $result" -ForegroundColor Green
    } elseif ($result -match "FAIL") {
        Write-Host "✗ $result" -ForegroundColor Red
    } elseif ($result -match "WARNING") {
        Write-Host "⚠ $result" -ForegroundColor Yellow
    } else {
        Write-Host "- $result" -ForegroundColor Cyan
    }
}

$passCount = ($testResults | Where-Object { $_ -match "PASS" }).Count
$failCount = ($testResults | Where-Object { $_ -match "FAIL" }).Count
$totalTests = $testResults.Count

Write-Host ""
Write-Host "Overall Result: $passCount/$totalTests tests passed" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Yellow" })

if ($failCount -eq 0) {
    Write-Host "🎉 Deployment is working correctly!" -ForegroundColor Green
} else {
    Write-Host "⚠ Some tests failed. Please check the configuration." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. If tests passed, your deployment is ready for production" -ForegroundColor White
Write-Host "2. If tests failed, check the IIS logs and application logs" -ForegroundColor White
Write-Host "3. Verify database connection strings in appsettings.Production.json" -ForegroundColor White
Write-Host "4. Check Windows Event Logs for any errors" -ForegroundColor White

