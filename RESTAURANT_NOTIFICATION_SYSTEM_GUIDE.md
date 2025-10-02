# Restaurant Notification System Guide

## Overview

The Restaurant Notification System allows restaurant owners to send targeted notifications to their previous customers. This system provides advanced targeting capabilities, campaign management, template creation, and comprehensive analytics.

## Features

### 🎯 **Targeted Notifications**
- Send notifications to specific customer segments
- Advanced targeting criteria based on order history, spending patterns, and engagement
- Geographic targeting by city and distance from restaurant
- Time-based targeting with preferred days and hours

### 📊 **Customer Segmentation**
- **New Customers**: First-time customers
- **Returning Customers**: Regular customers
- **High Value Customers**: Customers with high order values
- **Frequent Customers**: Customers who order frequently
- **Inactive Customers**: Customers who haven't ordered recently
- **Loyal Customers**: Long-term customers
- **One-time Customers**: Customers who only ordered once
- **Recent Customers**: Customers who ordered recently
- **All Customers**: All previous customers

### 📱 **Notification Types**
- **Promotional Offers**: Special deals and discounts
- **Restaurant Updates**: New menu items, hours, etc.
- **General Messages**: Custom messages
- **Order Updates**: Order status changes
- **Payment Confirmations**: Payment receipts
- **Delivery Updates**: Delivery status changes
- **Security Alerts**: Account security notifications
- **News Updates**: Restaurant news and announcements
- **Survey Requests**: Customer feedback requests
- **System Maintenance**: Service notifications

### ⚡ **Priority Levels**
- **Low**: Non-urgent notifications
- **Normal**: Standard notifications
- **High**: Important notifications
- **Critical**: Urgent notifications

### 📅 **Scheduling Options**
- Send immediately
- Schedule for specific date and time
- Recurring notifications (future enhancement)

### 📈 **Analytics & Reporting**
- Campaign performance metrics
- Delivery rates, open rates, click rates
- Customer engagement analytics
- Revenue attribution
- A/B testing capabilities

## Architecture

### Backend Components

#### 1. **RestaurantNotificationService**
```csharp
public interface IRestaurantNotificationService : IApplicationService
{
    Task<RestaurantNotificationCampaignResult> SendTargetedNotificationAsync(RestaurantTargetedNotificationRequest request);
    Task<List<RestaurantNotificationCampaignSummary>> GetRestaurantCampaignsAsync(Guid restaurantId);
    Task<RestaurantNotificationAnalytics> GetRestaurantAnalyticsAsync(Guid restaurantId);
    // ... other methods
}
```

#### 2. **DTOs**
- `RestaurantTargetedNotificationRequest`: Request to send targeted notification
- `CustomerTargetingCriteria`: Targeting criteria for customer selection
- `RestaurantNotificationCampaignResult`: Result of notification campaign
- `RestaurantNotificationCampaignSummary`: Campaign summary for listing
- `RestaurantNotificationAnalytics`: Analytics data
- `RestaurantNotificationTemplate`: Reusable notification templates

#### 3. **API Controller**
```csharp
[Route("api/restaurant-notifications")]
public class RestaurantNotificationController : DeliveryAppController
{
    [HttpPost("send-targeted")]
    public Task<RestaurantNotificationCampaignResult> SendTargetedNotificationAsync(RestaurantTargetedNotificationRequest request);
    
    [HttpGet("campaigns/{restaurantId}")]
    public Task<List<RestaurantNotificationCampaignSummary>> GetRestaurantCampaignsAsync(Guid restaurantId);
    
    // ... other endpoints
}
```

### Frontend Components

#### 1. **RestaurantNotificationsComponent**
- Main dashboard component for restaurant owners
- Tabbed interface with Send Notification, Campaign History, and Templates
- Advanced targeting form with real-time customer count preview
- Template management and creation
- Analytics dashboard

#### 2. **Key Features**
- **Form Validation**: Comprehensive validation for all inputs
- **Real-time Preview**: Live customer count based on targeting criteria
- **Template System**: Create and manage reusable notification templates
- **Campaign Management**: View, cancel, and analyze campaigns
- **Analytics Dashboard**: Comprehensive performance metrics

## Usage Examples

### 1. **Send Promotional Offer to High-Value Customers**

```typescript
const request: RestaurantTargetedNotificationRequest = {
  restaurantId: "restaurant-guid",
  title: "Special 20% Off for Our VIP Customers!",
  message: "Enjoy 20% off your next order. Use code VIP20. Valid until end of month.",
  imageUrl: "https://example.com/promotion.jpg",
  actionUrl: "https://app.example.com/offers/vip20",
  priority: NotificationPriority.High,
  type: NotificationType.PromotionalOffer,
  targetingCriteria: {
    customerSegments: [CustomerSegment.HighValueCustomers],
    minTotalSpent: 100.0,
    lastOrderAfter: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) // Last 30 days
  },
  sendImmediately: true
};

const result = await restaurantNotificationService.sendTargetedNotification(request);
```

### 2. **Schedule New Menu Announcement**

```typescript
const request: RestaurantTargetedNotificationRequest = {
  restaurantId: "restaurant-guid",
  title: "New Menu Items Available!",
  message: "Check out our new summer menu with fresh ingredients and exciting flavors.",
  imageUrl: "https://example.com/new-menu.jpg",
  actionUrl: "https://app.example.com/menu",
  priority: NotificationPriority.Normal,
  type: NotificationType.RestaurantUpdate,
  targetingCriteria: {
    customerSegments: [CustomerSegment.ReturningCustomers, CustomerSegment.LoyalCustomers],
    lastOrderAfter: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000) // Last 90 days
  },
  sendImmediately: false,
  scheduledTime: new Date(Date.now() + 2 * 60 * 60 * 1000) // 2 hours from now
};

const result = await restaurantNotificationService.sendTargetedNotification(request);
```

### 3. **Target Customers by Geographic Location**

```typescript
const request: RestaurantTargetedNotificationRequest = {
  restaurantId: "restaurant-guid",
  title: "Free Delivery in Your Area!",
  message: "We now offer free delivery within 5km of our restaurant. Order now!",
  priority: NotificationPriority.Normal,
  type: NotificationType.PromotionalOffer,
  targetingCriteria: {
    customerSegments: [CustomerSegment.AllCustomers],
    cities: ["damascus", "aleppo"],
    maxDistanceFromRestaurant: 5.0
  },
  sendImmediately: true
};
```

### 4. **Create Reusable Template**

```typescript
const templateRequest: CreateRestaurantNotificationTemplateDto = {
  name: "Weekly Special Offer",
  description: "Template for weekly promotional offers",
  titleTemplate: "This Week's Special: {offerName}",
  messageTemplate: "Hi {customerName}! Don't miss our special offer: {offerDescription}. Use code {promoCode} for {discount}% off!",
  imageUrlTemplate: "https://example.com/offers/{offerId}.jpg",
  type: NotificationType.PromotionalOffer,
  targetSegment: CustomerSegment.ReturningCustomers,
  defaultData: {
    "restaurantName": "My Restaurant",
    "discount": "20"
  }
};

const template = await restaurantNotificationService.createTemplate(templateRequest);
```

## Targeting Criteria

### **Order History Criteria**
- `minOrderCount`: Minimum number of orders
- `maxOrderCount`: Maximum number of orders
- `minTotalSpent`: Minimum total amount spent
- `maxTotalSpent`: Maximum total amount spent
- `lastOrderAfter`: Last order after this date
- `lastOrderBefore`: Last order before this date

### **Customer Segment Criteria**
- `customerSegments`: Array of customer segments to target
- `minAverageOrderValue`: Minimum average order value
- `maxAverageOrderValue`: Maximum average order value

### **Geographic Criteria**
- `cities`: Array of cities to target
- `maxDistanceFromRestaurant`: Maximum distance from restaurant (km)

### **Engagement Criteria**
- `hasOpenedPreviousNotifications`: Whether customer opened previous notifications
- `hasClickedPreviousNotifications`: Whether customer clicked previous notifications
- `lastActiveAfter`: Last active after this date

### **Time-based Criteria**
- `preferredDays`: Array of preferred days of week
- `preferredTimeStart`: Preferred start time
- `preferredTimeEnd`: Preferred end time

### **Exclusion Criteria**
- `excludeCustomerIds`: Array of customer IDs to exclude
- `excludeRecentlyNotified`: Exclude customers notified recently
- `excludeNotifiedWithinHours`: Exclude customers notified within this many hours

## API Endpoints

### **Send Targeted Notification**
```
POST /api/restaurant-notifications/send-targeted
```

### **Get Campaign History**
```
GET /api/restaurant-notifications/campaigns/{restaurantId}?skipCount=0&maxResultCount=10
```

### **Get Campaign Details**
```
GET /api/restaurant-notifications/campaigns/details/{campaignId}
```

### **Get Analytics**
```
GET /api/restaurant-notifications/analytics/{restaurantId}?startDate=2024-01-01&endDate=2024-01-31
```

### **Preview Customer Count**
```
POST /api/restaurant-notifications/preview-customers/{restaurantId}
```

### **Get Customer Segments**
```
GET /api/restaurant-notifications/customer-segments/{restaurantId}
```

### **Template Management**
```
POST /api/restaurant-notifications/templates
GET /api/restaurant-notifications/templates/{restaurantId}
PUT /api/restaurant-notifications/templates/{templateId}
DELETE /api/restaurant-notifications/templates/{templateId}
```

### **Customer Preferences**
```
GET /api/restaurant-notifications/customer-preferences/{customerId}/{restaurantId}
PUT /api/restaurant-notifications/customer-preferences/{customerId}
```

### **Campaign Management**
```
POST /api/restaurant-notifications/campaigns/{campaignId}/cancel
POST /api/restaurant-notifications/campaigns/{campaignId}/resend-failed
GET /api/restaurant-notifications/campaigns/{campaignId}/metrics
```

### **Test Notifications**
```
POST /api/restaurant-notifications/test/{restaurantId}
```

### **Settings Management**
```
GET /api/restaurant-notifications/settings/{restaurantId}
PUT /api/restaurant-notifications/settings/{restaurantId}
```

## Dashboard Features

### **Send Notification Tab**
- **Basic Details**: Title, message, image, action URL
- **Type & Priority**: Notification type and priority level
- **Scheduling**: Immediate or scheduled sending
- **Customer Targeting**: Advanced targeting criteria
- **Real-time Preview**: Live customer count based on criteria

### **Campaign History Tab**
- **Campaign List**: All sent campaigns with metrics
- **Performance Metrics**: Delivery, open, and click rates
- **Campaign Actions**: View details, cancel scheduled campaigns
- **Status Tracking**: Real-time campaign status

### **Templates Tab**
- **Template Management**: Create, edit, delete templates
- **Template Usage**: Track template usage and performance
- **Variable Support**: Use dynamic variables in templates
- **Quick Send**: Send notifications using templates

### **Analytics Dashboard**
- **Overall Metrics**: Total campaigns, sent, delivered, opened, clicked
- **Performance Rates**: Delivery rate, open rate, click rate
- **Customer Engagement**: New customers, returning customers
- **Campaign Breakdown**: Performance by notification type
- **Time-based Analysis**: Daily, weekly, monthly metrics

## Best Practices

### **Targeting**
1. **Start Broad**: Begin with broader targeting and narrow down based on performance
2. **Segment Wisely**: Use customer segments to personalize messages
3. **Respect Preferences**: Honor customer notification preferences
4. **Avoid Spam**: Don't send too many notifications to the same customers

### **Content**
1. **Clear Subject**: Use clear, engaging titles
2. **Personalize**: Use customer names and relevant information
3. **Call to Action**: Include clear action buttons or links
4. **Visual Appeal**: Use high-quality images when possible

### **Timing**
1. **Optimal Times**: Send during peak engagement hours
2. **Respect Quiet Hours**: Avoid sending during customer quiet hours
3. **Frequency Control**: Limit notification frequency per customer
4. **Time Zones**: Consider customer time zones for scheduling

### **Testing**
1. **A/B Testing**: Test different messages and targeting criteria
2. **Small Batches**: Start with small test groups
3. **Monitor Performance**: Track metrics and adjust accordingly
4. **Learn from Data**: Use analytics to improve future campaigns

## Security & Privacy

### **Data Protection**
- Customer data is encrypted and protected
- Notification preferences are respected
- Opt-out mechanisms are provided
- Data retention policies are followed

### **Access Control**
- Restaurant owners can only send to their own customers
- Admin approval may be required for certain campaigns
- Rate limiting prevents spam
- Audit logs track all activities

### **Compliance**
- GDPR compliance for EU customers
- CCPA compliance for California customers
- Local privacy laws compliance
- Clear privacy policies and terms

## Future Enhancements

### **Planned Features**
1. **A/B Testing**: Built-in A/B testing capabilities
2. **Automated Campaigns**: Trigger-based automated notifications
3. **Rich Media**: Support for videos and interactive content
4. **Advanced Analytics**: Machine learning insights and predictions
5. **Integration**: Integration with marketing tools and CRM systems
6. **Multi-language**: Support for multiple languages
7. **Push Notifications**: Mobile push notification support
8. **Email Integration**: Email notification support
9. **SMS Integration**: SMS notification support
10. **Webhook Support**: Real-time webhook notifications

### **Technical Improvements**
1. **Performance Optimization**: Faster query processing
2. **Caching**: Improved caching strategies
3. **Scalability**: Better handling of large customer bases
4. **Real-time Updates**: Live campaign status updates
5. **Mobile App**: Dedicated mobile app for restaurant owners

## Troubleshooting

### **Common Issues**

#### **No Customers Found**
- Check targeting criteria
- Verify customer data exists
- Ensure restaurant has previous customers

#### **Low Delivery Rates**
- Check device token validity
- Verify notification service configuration
- Review customer notification preferences

#### **Low Open Rates**
- Improve message content
- Optimize send timing
- Review targeting criteria

#### **High Bounce Rates**
- Check image URLs
- Verify action URLs
- Review message formatting

### **Support**
- Check logs for detailed error messages
- Contact technical support for complex issues
- Review documentation for configuration help
- Use test notifications to verify setup

## Conclusion

The Restaurant Notification System provides a comprehensive solution for restaurant owners to engage with their customers through targeted notifications. With advanced targeting capabilities, template management, and detailed analytics, restaurants can create effective marketing campaigns that drive customer engagement and increase revenue.

The system is designed to be user-friendly while providing powerful features for advanced users. Regular updates and improvements ensure the system stays current with industry best practices and customer expectations.
