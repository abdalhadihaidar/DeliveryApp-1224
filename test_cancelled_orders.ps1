# Test script to create cancelled orders for testing the cancelled orders endpoint
# This script helps test the cancelled orders functionality by creating test cancelled orders

param(
    [string]$BaseUrl = "https://localhost:5001",
    [string]$AuthToken = "",
    [int]$NumberOfOrders = 1
)

Write-Host "Testing Cancelled Orders Endpoint" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Check if auth token is provided
if ([string]::IsNullOrEmpty($AuthToken)) {
    Write-Host "Please provide an auth token using -AuthToken parameter" -ForegroundColor Red
    Write-Host "Example: .\test_cancelled_orders.ps1 -AuthToken 'your_jwt_token_here'" -ForegroundColor Yellow
    exit 1
}

# Headers for API requests
$headers = @{
    "Authorization" = "Bearer $AuthToken"
    "Content-Type" = "application/json"
}

try {
    # First, get all orders to see what's available
    Write-Host "`n1. Getting all orders to find candidates for cancellation..." -ForegroundColor Cyan
    $ordersResponse = Invoke-RestMethod -Uri "$BaseUrl/api/admin/orders?skipCount=0&maxResultCount=50" -Method GET -Headers $headers
    
    Write-Host "Found $($ordersResponse.totalCount) total orders" -ForegroundColor Yellow
    
    # Filter orders that can be cancelled (not delivered or already cancelled)
    $cancellableOrders = $ordersResponse.items | Where-Object { $_.status -ne 7 -and $_.status -ne 8 }
    
    Write-Host "Found $($cancellableOrders.Count) orders that can be cancelled" -ForegroundColor Yellow
    
    if ($cancellableOrders.Count -eq 0) {
        Write-Host "No orders available for cancellation. All orders are either delivered or already cancelled." -ForegroundColor Red
        Write-Host "Status breakdown:" -ForegroundColor Yellow
        $ordersResponse.items | Group-Object status | ForEach-Object {
            $statusName = switch ($_.Name) {
                "0" { "Pending" }
                "1" { "Confirmed" }
                "2" { "Preparing" }
                "3" { "ReadyForDelivery" }
                "4" { "WaitingCourier" }
                "5" { "OutForDelivery" }
                "6" { "Delivering" }
                "7" { "Delivered" }
                "8" { "Cancelled" }
                default { "Unknown" }
            }
            Write-Host "  Status $($_.Name) ($statusName): $($_.Count) orders" -ForegroundColor White
        }
        exit 0
    }
    
    # Cancel the first few orders
    $ordersToCancel = $cancellableOrders | Select-Object -First $NumberOfOrders
    
    Write-Host "`n2. Cancelling $($ordersToCancel.Count) orders..." -ForegroundColor Cyan
    
    foreach ($order in $ordersToCancel) {
        Write-Host "Cancelling order $($order.id)..." -ForegroundColor White
        
        try {
            $cancelResponse = Invoke-RestMethod -Uri "$BaseUrl/api/dashboard/test-cancelled-order/$($order.id)" -Method POST -Headers $headers
            
            if ($cancelResponse) {
                Write-Host "  ✓ Successfully cancelled order $($order.id)" -ForegroundColor Green
            } else {
                Write-Host "  ✗ Failed to cancel order $($order.id)" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "  ✗ Error cancelling order $($order.id): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Now test the cancelled orders endpoint
    Write-Host "`n3. Testing cancelled orders endpoint..." -ForegroundColor Cyan
    
    try {
        $cancelledOrdersResponse = Invoke-RestMethod -Uri "$BaseUrl/api/dashboard/cancelled-orders?page=1&pageSize=10" -Method GET -Headers $headers
        
        Write-Host "Cancelled orders endpoint response:" -ForegroundColor Yellow
        Write-Host "  Total cancelled orders: $($cancelledOrdersResponse.totalCount)" -ForegroundColor White
        Write-Host "  Items returned: $($cancelledOrdersResponse.items.Count)" -ForegroundColor White
        
        if ($cancelledOrdersResponse.items.Count -gt 0) {
            Write-Host "`nCancelled orders details:" -ForegroundColor Green
            foreach ($order in $cancelledOrdersResponse.items) {
                Write-Host "  - Order ID: $($order.id)" -ForegroundColor White
                Write-Host "    Customer: $($order.customerName)" -ForegroundColor White
                Write-Host "    Store: $($order.storeName)" -ForegroundColor White
                Write-Host "    Amount: $($order.amount)" -ForegroundColor White
                Write-Host "    Order Time: $($order.orderTime)" -ForegroundColor White
                Write-Host "    Cancellation Reason: $($order.cancellationReason)" -ForegroundColor White
                Write-Host ""
            }
        } else {
            Write-Host "No cancelled orders found in the response." -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Error testing cancelled orders endpoint: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host "`nTest completed!" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the backend is running and the auth token is valid." -ForegroundColor Yellow
}

