# Test script to verify antiforgery token fix
# This script tests POST requests to the backend to ensure they work without antiforgery errors

Write-Host "Testing Antiforgery Token Fix..." -ForegroundColor Green

$baseUrl = "https://backend.waselsy.com"
$testEndpoints = @(
    @{ Path = "/"; Method = "POST"; ExpectedStatus = 404; Description = "Root POST request" },
    @{ Path = "/Index"; Method = "POST"; ExpectedStatus = 404; Description = "Index POST request" },
    @{ Path = "/swagger"; Method = "GET"; ExpectedStatus = 200; Description = "Swagger documentation" },
    @{ Path = "/api/app/health"; Method = "GET"; ExpectedStatus = 200; Description = "Health check endpoint" }
)

foreach ($endpoint in $testEndpoints) {
    Write-Host "`nTesting: $($endpoint.Description)" -ForegroundColor Yellow
    Write-Host "URL: $baseUrl$($endpoint.Path)" -ForegroundColor Cyan
    Write-Host "Method: $($endpoint.Method)" -ForegroundColor Cyan
    
    try {
        if ($endpoint.Method -eq "GET") {
            $response = Invoke-WebRequest -Uri "$baseUrl$($endpoint.Path)" -Method GET -UseBasicParsing -TimeoutSec 10
        } else {
            $response = Invoke-WebRequest -Uri "$baseUrl$($endpoint.Path)" -Method POST -UseBasicParsing -TimeoutSec 10 -ContentType "application/json" -Body "{}"
        }
        
        if ($response.StatusCode -eq $endpoint.ExpectedStatus) {
            Write-Host "✅ SUCCESS: Status $($response.StatusCode) (Expected: $($endpoint.ExpectedStatus))" -ForegroundColor Green
        } else {
            Write-Host "⚠️  WARNING: Status $($response.StatusCode) (Expected: $($endpoint.ExpectedStatus))" -ForegroundColor Yellow
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $endpoint.ExpectedStatus) {
            Write-Host "✅ SUCCESS: Status $statusCode (Expected: $($endpoint.ExpectedStatus))" -ForegroundColor Green
        } else {
            Write-Host "❌ FAILED: Status $statusCode (Expected: $($endpoint.ExpectedStatus))" -ForegroundColor Red
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`n`nAntiforgery Token Fix Test Complete!" -ForegroundColor Green
Write-Host "If you see SUCCESS for POST requests to / and /Index with status 404, the fix is working." -ForegroundColor Cyan
Write-Host "The 404 status is expected because these endpoints should redirect to /swagger for GET requests" -ForegroundColor Cyan
Write-Host "and return 404 for POST requests (which is the correct behavior for an API-only host)." -ForegroundColor Cyan
