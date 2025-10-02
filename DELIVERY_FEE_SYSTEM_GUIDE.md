# 🚚 Advanced Delivery Fee System - Implementation Guide

## Overview

The delivery fee system has been completely redesigned to support dynamic, intelligent pricing based on multiple factors. This replaces the previous fixed $5.00 delivery fee with a sophisticated calculation engine.

## ✨ New Features Implemented

### 1. **Dynamic Fee Calculation** ✅
- **Before**: Fixed $5.00 delivery fee
- **After**: Intelligent calculation based on multiple factors

### 2. **Free Delivery Threshold** ✅
- **Logic**: Orders above threshold get free delivery
- **Default**: $50.00 (configurable)
- **Implementation**: Automatic detection and application

### 3. **Rush Delivery** ✅
- **Feature**: Express delivery option
- **Fee**: Additional $10.00 (configurable)
- **Time**: 30 minutes delivery time
- **Implementation**: Optional flag in order creation

### 4. **Distance-Based Pricing** ✅
- **Logic**: First 5km included in base fee, additional km charged
- **Rates**: 
  - In-town: $0.50 per additional km
  - Out-of-town: $1.00 per additional km
- **Implementation**: Haversine formula for accurate distance calculation

### 5. **Restaurant-Specific Fees** ✅
- **Feature**: Each restaurant can set custom delivery fees
- **Fallback**: System default if restaurant fee not set
- **Implementation**: Restaurant.DeliveryFee property

### 6. **City-Based Delivery Types** ✅
- **In-Town**: Within distance threshold (fixed fee, no distance charges)
- **Out-of-Town**: Beyond distance threshold (auto-calculated per km)
- **Implementation**: Distance-based detection with configurable threshold

## 🏗️ System Architecture

### Core Components

#### 1. **DeliveryFeeCalculationService**
```csharp
public class DeliveryFeeCalculationService : IDeliveryFeeCalculationService
{
    // Main calculation method
    Task<DeliveryFeeCalculationResultDto> CalculateDeliveryFeeAsync(DeliveryFeeCalculationRequestDto request);
    
    // Get available options
    Task<DeliveryFeeOptionsDto> GetDeliveryFeeOptionsAsync(Guid restaurantId, Guid customerAddressId);
}
```

#### 2. **Delivery Fee DTOs**
- `DeliveryFeeCalculationRequestDto` - Input parameters
- `DeliveryFeeCalculationResultDto` - Calculation results
- `DeliveryFeeBreakdownDto` - Detailed fee breakdown
- `DeliveryFeeOptionsDto` - Available delivery options

#### 3. **Enums**
- `DeliveryCityType` - InTown/OutOfTown
- `DeliveryType` - Standard/Rush

## 💰 Fee Calculation Logic

### Formula
```
Final Fee = Base Fee + Distance Fee + Rush Fee

Where:
- Base Fee = In-Town Base Fee OR Out-of-Town Base Fee (based on distance threshold)
- Distance Fee = 0 (in-town) OR (Distance - Threshold) × Rate per km (out-of-town)
- Rush Fee = $10.00 if rush delivery
```

### Free Delivery Logic
```
IF Order Amount >= Free Delivery Threshold
    THEN Delivery Fee = $0.00
    ELSE Apply normal calculation
```

### Distance Calculation
```
IF Distance <= In-Town Threshold (5km)
    THEN Distance Fee = $0.00 (Fixed in-town fee)
    ELSE Distance Fee = (Distance - In-Town Threshold) × Out-of-Town Rate per km
```

## 🔧 Configuration

### System Settings (DeliverySettingsDto)
```csharp
public class DeliverySettingsDto
{
    public double DeliveryFee { get; set; } = 5.0;           // Legacy base delivery fee
    public double FreeDeliveryThreshold { get; set; } = 50.0; // Free delivery threshold
    public double RushDeliveryFee { get; set; } = 10.0;      // Rush delivery fee
    public int RushDeliveryTime { get; set; } = 30;          // Rush delivery time (minutes)
    public double MinimumOrderAmount { get; set; } = 20.0;   // Minimum order amount
    public double MaxDeliveryDistance { get; set; } = 25.0;  // Maximum delivery distance
    
    // New city-based delivery settings
    public double InTownDistanceThreshold { get; set; } = 5.0; // km - within this distance is in-town
    public double OutOfTownRatePerKm { get; set; } = 2.0;     // Rate per km for out-of-town
    public double InTownBaseFee { get; set; } = 5.0;          // Fixed fee for in-town
    public double OutOfTownBaseFee { get; set; } = 8.0;       // Base fee for out-of-town
}
```

### Restaurant Settings
```csharp
public class RestaurantDto
{
    public decimal DeliveryFee { get; set; } // Restaurant-specific fee
    // ... other properties
}
```

## 📊 Usage Examples

### 1. Standard In-Town Delivery
```
Order Amount: $30.00
Distance: 3km (within 5km threshold)
City Type: In-Town
Rush Delivery: No

Calculation:
- Base Fee: $5.00 (In-Town Base Fee)
- Distance Fee: $0.00 (no distance charges for in-town)
- Rush Fee: $0.00
- Final Fee: $5.00
```

### 2. Rush Out-of-Town Delivery
```
Order Amount: $40.00
Distance: 8km (beyond 5km threshold)
City Type: Out-of-Town
Rush Delivery: Yes

Calculation:
- Base Fee: $8.00 (Out-of-Town Base Fee)
- Distance Fee: $6.00 (3km × $2.00 per km)
- Rush Fee: $10.00
- Final Fee: $24.00
```

### 3. Free Delivery
```
Order Amount: $60.00
Distance: 4km
City Type: In-Town
Rush Delivery: No

Calculation:
- Order exceeds $50.00 threshold
- Final Fee: $0.00 (FREE DELIVERY)
```

## 🚀 API Endpoints

### Calculate Delivery Fee
```http
POST /api/delivery-fee/calculate
Content-Type: application/json

{
    "orderId": "guid",
    "restaurantId": "guid",
    "customerAddressId": "guid",
    "orderAmount": 30.00,
    "isRushDelivery": false,
    "preferredDeliveryTime": "2024-01-01T18:00:00Z"
}
```

### Get Delivery Options
```http
GET /api/delivery-fee/options?restaurantId=guid&customerAddressId=guid
```

## 🔄 Integration Points

### Order Creation Flow
1. **Customer** creates order with items
2. **System** calculates subtotal
3. **DeliveryFeeCalculationService** calculates delivery fee
4. **System** applies tax and calculates total
5. **Order** saved with calculated fees

### Order DTO Updates
```csharp
public class CreateOrderDto
{
    // ... existing properties
    public bool? IsRushDelivery { get; set; } = false;
    public DateTime? PreferredDeliveryTime { get; set; }
}
```

## 🛡️ Error Handling

### Validation Errors
- **RESTAURANT_LOCATION_MISSING**: Restaurant address not found
- **CUSTOMER_ADDRESS_MISSING**: Customer address not found
- **MINIMUM_ORDER_NOT_MET**: Order below minimum amount
- **DELIVERY_DISTANCE_EXCEEDED**: Distance exceeds maximum allowed

### Business Logic
- **Free Delivery**: Applied automatically when threshold met
- **Distance Limits**: Orders beyond max distance rejected
- **Minimum Orders**: Orders below minimum amount rejected

## 📈 Benefits

### For Customers
- **Transparent Pricing**: Clear breakdown of delivery costs
- **Free Delivery**: Automatic application when threshold met
- **Rush Options**: Express delivery for urgent orders
- **Fair Pricing**: Distance-based pricing ensures fairness

### For Restaurants
- **Custom Fees**: Set restaurant-specific delivery fees
- **Flexible Pricing**: Different rates for different areas
- **Revenue Control**: Manage delivery pricing strategy

### For Platform
- **Dynamic Pricing**: Adapt to market conditions
- **Revenue Optimization**: Maximize delivery revenue
- **Customer Satisfaction**: Fair and transparent pricing

## 🔧 Future Enhancements

### Potential Additions
1. **Peak Hours Pricing**: Higher fees during busy times
2. **Weather-Based Pricing**: Adjust fees for weather conditions
3. **Demand-Based Pricing**: Surge pricing during high demand
4. **Loyalty Discounts**: Reduced fees for frequent customers
5. **Promotional Codes**: Special delivery fee discounts

### Technical Improvements
1. **Caching**: Cache distance calculations
2. **Real-time Updates**: Live fee updates based on conditions
3. **Analytics**: Track delivery fee performance
4. **A/B Testing**: Test different pricing strategies

## 🎯 Implementation Status

- ✅ **Dynamic Calculation**: Implemented
- ✅ **Free Delivery**: Implemented
- ✅ **Rush Delivery**: Implemented
- ✅ **Distance-Based**: Implemented
- ✅ **Restaurant-Specific**: Implemented
- ✅ **City-Based Types**: Implemented
- ✅ **API Endpoints**: Implemented
- ✅ **Error Handling**: Implemented
- ✅ **Integration**: Implemented

The delivery fee system is now fully functional and ready for production use!
