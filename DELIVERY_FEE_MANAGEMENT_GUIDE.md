# 🚚 Delivery Fee Management System - Complete Guide

## Overview

The delivery fee system has been fully implemented across both the **Dashboard** and **Mobile App** with comprehensive management capabilities. This guide covers how to manage, configure, and use the system effectively.

## 🎯 System Components

### **Backend Services**
- ✅ `DeliveryFeeCalculationService` - Core calculation engine
- ✅ `DeliveryFeeController` - API endpoints
- ✅ `SystemSettingsDto` - Configuration management
- ✅ Integration with `OrderAppService`

### **Dashboard Management**
- ✅ Enhanced Settings Page with delivery fee controls
- ✅ Delivery Fee Management Component
- ✅ Real-time fee calculation testing
- ✅ Visual fee breakdown and examples

### **Mobile App Integration**
- ✅ `DeliveryFeeService` - API communication
- ✅ `DeliveryFeeWidget` - User interface
- ✅ `DeliveryFeePreviewWidget` - Fee preview
- ✅ Local calculation capabilities

## 🖥️ Dashboard Management

### **1. Settings Page Enhancement**

The existing settings page has been enhanced with new delivery fee controls:

#### **New Settings Fields:**
```typescript
// City-Based Delivery Settings
inTownDistanceThreshold: 5.0,     // km - within this distance is in-town
inTownBaseFee: 5.0,               // Fixed fee for in-town delivery
outOfTownBaseFee: 8.0,            // Base fee for out-of-town delivery
outOfTownRatePerKm: 2.0,          // Rate per km for out-of-town
rushDeliveryFee: 10.0,            // Rush delivery fee
minimumOrderAmount: 20.0,         // Minimum order amount
```

#### **Access Path:**
1. Navigate to **Dashboard** → **Settings**
2. Expand **Delivery Settings** section
3. Configure the new city-based delivery settings
4. Click **Save All** to apply changes

### **2. Delivery Fee Management Component**

A new dedicated component for testing and managing delivery fees:

#### **Features:**
- **Test Calculation**: Test fee calculation with different parameters
- **Real-time Results**: See immediate calculation results
- **Detailed Breakdown**: View step-by-step fee calculation
- **Examples Table**: Pre-configured scenarios for reference
- **Visual Feedback**: Color-coded results and status indicators

#### **Access Path:**
1. Navigate to **Dashboard** → **Delivery Fee Management**
2. Use the test form to calculate fees
3. View detailed breakdowns and examples

### **3. API Endpoints**

#### **Calculate Delivery Fee**
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

#### **Get Delivery Options**
```http
GET /api/delivery-fee/options?restaurantId=guid&customerAddressId=guid
```

## 📱 Mobile App Integration

### **1. Delivery Fee Widget**

The mobile app includes a comprehensive delivery fee widget:

#### **Features:**
- **Multiple Options**: Standard and Rush delivery options
- **Real-time Calculation**: Live fee calculation based on location
- **Free Delivery Detection**: Automatic free delivery eligibility
- **Visual Selection**: Clear option selection with visual feedback
- **Detailed Information**: Distance, thresholds, and fee breakdown

#### **Usage:**
```dart
DeliveryFeeWidget(
  restaurantId: 'restaurant-id',
  customerAddressId: 'address-id', 
  orderAmount: 30.0,
  onDeliveryOptionSelected: (option) {
    // Handle selection
  },
  selectedOption: selectedOption,
)
```

### **2. Delivery Fee Service**

Service for API communication and local calculations:

#### **Key Methods:**
```dart
// Calculate delivery fee
calculateDeliveryFee({
  required String orderId,
  required String restaurantId,
  required String customerAddressId,
  required double orderAmount,
  bool isRushDelivery = false,
})

// Get delivery options
getDeliveryFeeOptions({
  required String restaurantId,
  required String customerAddressId,
})

// Get delivery preview
getDeliveryFeePreview({
  required String restaurantId,
  required String customerAddressId,
  required double orderAmount,
})
```

### **3. Local Calculation**

The app can calculate fees locally for preview:

```dart
// Local calculation for preview
calculateDeliveryFeeLocally({
  required double orderAmount,
  required double distance,
  required DeliverySettings settings,
  bool isRushDelivery = false,
})
```

## ⚙️ Configuration Management

### **1. System Settings**

All delivery fee settings are managed through the system settings:

#### **Configuration Structure:**
```typescript
delivery: {
  // Basic Settings
  defaultDeliveryRadius: 10.0,
  maxDeliveryTime: 60,
  deliveryFee: 5.0,
  freeDeliveryThreshold: 50.0,
  maxDeliveryDistance: 25.0,
  
  // City-Based Settings
  inTownDistanceThreshold: 5.0,    // km threshold for in-town
  inTownBaseFee: 5.0,              // Fixed in-town fee
  outOfTownBaseFee: 8.0,           // Out-of-town base fee
  outOfTownRatePerKm: 2.0,         // Rate per additional km
  
  // Rush Delivery
  rushDeliveryFee: 10.0,
  rushDeliveryTime: 30,
  
  // Order Requirements
  minimumOrderAmount: 20.0,
  allowScheduledDelivery: true,
  maxScheduledDays: 7,
  requireDeliveryConfirmation: true
}
```

### **2. Restaurant-Specific Settings**

Individual restaurants can override system defaults:

```typescript
restaurant: {
  deliveryFee: 6.0,  // Override system default
  // Other restaurant settings...
}
```

## 💰 Fee Calculation Logic

### **1. Calculation Formula**

```
Final Fee = Base Fee + Distance Fee + Rush Fee

Where:
- Base Fee = In-Town Base Fee OR Out-of-Town Base Fee
- Distance Fee = 0 (in-town) OR (Distance - Threshold) × Rate per km
- Rush Fee = Rush Delivery Fee (if applicable)
```

### **2. Free Delivery Logic**

```
IF Order Amount >= Free Delivery Threshold
    THEN Delivery Fee = $0.00
    ELSE Apply normal calculation
```

### **3. City Type Determination**

```
IF Distance <= In-Town Threshold
    THEN City Type = In-Town
    ELSE City Type = Out-of-Town
```

## 📊 Management Features

### **1. Real-Time Testing**

The dashboard provides real-time testing capabilities:

- **Test Different Scenarios**: Various order amounts, distances, and options
- **Immediate Results**: See calculation results instantly
- **Detailed Breakdown**: Step-by-step fee calculation
- **Visual Examples**: Pre-configured scenarios for reference

### **2. Configuration Validation**

- **Input Validation**: All settings have proper validation rules
- **Range Checks**: Min/max values for all numeric settings
- **Required Fields**: Mandatory fields are properly marked
- **Error Handling**: Comprehensive error messages and handling

### **3. Visual Management**

- **Color-Coded Results**: Different colors for different fee types
- **Status Indicators**: Clear visual indicators for free delivery, rush delivery
- **Progress Indicators**: Loading states and progress feedback
- **Responsive Design**: Works on all screen sizes

## 🔧 Usage Examples

### **1. Dashboard Management**

#### **Configure Delivery Settings:**
1. Go to **Settings** → **Delivery Settings**
2. Set **In-Town Distance Threshold** to 5km
3. Set **In-Town Base Fee** to $5.00
4. Set **Out-of-Town Base Fee** to $8.00
5. Set **Out-of-Town Rate per km** to $2.00
6. Click **Save All**

#### **Test Fee Calculation:**
1. Go to **Delivery Fee Management**
2. Enter test parameters:
   - Order Amount: $30.00
   - Distance: 3km
   - Restaurant ID: [select]
   - Address ID: [select]
3. Click **Calculate**
4. View detailed breakdown

### **2. Mobile App Integration**

#### **Add Delivery Fee Widget:**
```dart
// In your order screen
DeliveryFeeWidget(
  restaurantId: selectedRestaurant.id,
  customerAddressId: selectedAddress.id,
  orderAmount: cartTotal,
  onDeliveryOptionSelected: (option) {
    setState(() {
      selectedDeliveryOption = option;
    });
  },
  selectedOption: selectedDeliveryOption,
)
```

#### **Get Delivery Preview:**
```dart
// Get delivery fee preview
final preview = await deliveryFeeService.getDeliveryFeePreview(
  restaurantId: restaurantId,
  customerAddressId: addressId,
  orderAmount: orderAmount,
);

// Display preview
showDialog(
  context: context,
  builder: (context) => DeliveryFeePreviewWidget(
    orderAmount: orderAmount,
    distance: preview.distanceKm,
    settings: settings,
    isRushDelivery: false,
  ),
);
```

## 🎨 UI/UX Features

### **1. Dashboard Features**

- **Expandable Panels**: Organized settings in collapsible sections
- **Form Validation**: Real-time validation with error messages
- **Save States**: Visual feedback for save operations
- **Reset Functionality**: Easy reset to default values
- **Export/Import**: Settings backup and restore

### **2. Mobile App Features**

- **Card-Based Design**: Clean, modern card layout
- **Selection States**: Clear visual feedback for selections
- **Loading States**: Smooth loading animations
- **Error Handling**: User-friendly error messages
- **Responsive Layout**: Adapts to different screen sizes

## 🔍 Monitoring and Analytics

### **1. Fee Calculation Tracking**

- **Calculation Logs**: Track all fee calculations
- **Performance Metrics**: Monitor calculation performance
- **Error Tracking**: Log and track calculation errors
- **Usage Statistics**: Track feature usage

### **2. Business Intelligence**

- **Fee Analysis**: Analyze delivery fee patterns
- **Revenue Tracking**: Track delivery fee revenue
- **Customer Behavior**: Understand delivery preferences
- **Optimization Insights**: Identify optimization opportunities

## 🚀 Future Enhancements

### **1. Advanced Features**

- **Dynamic Pricing**: Time-based and demand-based pricing
- **Promotional Codes**: Special delivery fee discounts
- **Loyalty Programs**: Reduced fees for frequent customers
- **A/B Testing**: Test different pricing strategies

### **2. Analytics Dashboard**

- **Fee Performance**: Visual fee performance metrics
- **Revenue Analysis**: Delivery fee revenue analysis
- **Customer Insights**: Delivery preference insights
- **Optimization Recommendations**: AI-powered suggestions

## 📋 Best Practices

### **1. Configuration Management**

- **Regular Reviews**: Review settings monthly
- **A/B Testing**: Test different configurations
- **Performance Monitoring**: Monitor system performance
- **Backup Settings**: Regular settings backup

### **2. User Experience**

- **Clear Communication**: Explain fee structure clearly
- **Transparent Pricing**: Show detailed fee breakdown
- **Error Prevention**: Validate inputs before calculation
- **Responsive Design**: Ensure mobile-friendly interface

### **3. System Maintenance**

- **Regular Updates**: Keep system updated
- **Performance Optimization**: Monitor and optimize performance
- **Error Handling**: Implement comprehensive error handling
- **Documentation**: Maintain up-to-date documentation

## 🎯 Conclusion

The delivery fee management system provides comprehensive control over delivery pricing with:

- ✅ **Complete Dashboard Management**
- ✅ **Mobile App Integration**
- ✅ **Real-Time Testing**
- ✅ **Visual Management Tools**
- ✅ **Flexible Configuration**
- ✅ **User-Friendly Interface**

The system is production-ready and provides all the tools needed to effectively manage delivery fees across both web and mobile platforms.
