# SpecialOffer Implementation Guide

## Overview

The SpecialOffer system provides comprehensive CRUD operations and advanced scheduling capabilities for restaurant offers and promotions. This implementation includes recurring offers, time-based scheduling, priority management, and usage tracking.

## Features

### Core CRUD Operations
- ✅ Create, Read, Update, Delete operations
- ✅ Authorization and ownership validation
- ✅ Comprehensive validation rules
- ✅ AutoMapper integration

### Advanced Scheduling
- ✅ Recurring offers (Daily, Weekly, Monthly, Yearly)
- ✅ Time-based scheduling (specific hours of the day)
- ✅ Day-of-week restrictions
- ✅ Priority-based offer ordering
- ✅ Conflict detection and validation

### Status Management
- ✅ Draft, Active, Paused, Inactive, Expired, Scheduled states
- ✅ Automatic status transitions
- ✅ Manual status control (Activate, Deactivate, Pause, Resume)

### Usage Tracking
- ✅ Usage count tracking
- ✅ Maximum usage limits
- ✅ Last usage timestamp
- ✅ Popular offers analytics

### Search and Filtering
- ✅ Text-based search
- ✅ Status filtering
- ✅ Date range filtering
- ✅ Category filtering
- ✅ Priority filtering

## Architecture

### Domain Layer
- **SpecialOffer Entity**: Core domain model with business logic
- **OfferStatus Enum**: Status management
- **ISpecialOfferRepository**: Repository interface

### Application Layer
- **ISpecialOfferAppService**: Service interface
- **SpecialOfferAppService**: Main service implementation
- **OfferSchedulingService**: Specialized scheduling service
- **RecurringOffersBackgroundService**: Background processing

### Infrastructure Layer
- **SpecialOfferRepository**: EF Core implementation
- **AutoMapper Profiles**: Object mapping configuration

## Usage Examples

### Creating a Simple Offer

```csharp
var offerDto = new CreateSpecialOfferDto
{
    RestaurantId = restaurantId,
    Title = "20% Off All Items",
    Description = "Get 20% discount on all menu items",
    StartDate = DateTime.Now.AddDays(1),
    EndDate = DateTime.Now.AddDays(7),
    DiscountPercentage = 20,
    Priority = 1,
    IsActive = true
};

var result = await specialOfferService.CreateAsync(offerDto);
```

### Creating a Recurring Offer

```csharp
var recurrencePattern = new RecurrencePatternDto
{
    Type = "Weekly",
    Interval = 1,
    DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    MaxOccurrences = 12
};

var offerDto = new CreateSpecialOfferDto
{
    RestaurantId = restaurantId,
    Title = "Weekend Special",
    Description = "Special offers every weekend",
    StartDate = DateTime.Now.AddDays(1),
    EndDate = DateTime.Now.AddMonths(3),
    IsRecurring = true,
    StartTime = new TimeSpan(18, 0, 0), // 6 PM
    EndTime = new TimeSpan(22, 0, 0),   // 10 PM
    ApplicableDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday },
    Priority = 2
};

var result = await specialOfferService.CreateAsync(offerDto);
```

### Managing Offer Status

```csharp
// Activate an offer
await specialOfferService.ActivateOfferAsync(offerId);

// Pause an offer
await specialOfferService.PauseOfferAsync(offerId);

// Resume a paused offer
await specialOfferService.ResumeOfferAsync(offerId);

// Deactivate an offer
await specialOfferService.DeactivateOfferAsync(offerId);
```

### Querying Offers

```csharp
// Get active offers for a restaurant
var activeOffers = await specialOfferService.GetActiveRestaurantOffersAsync(restaurantId);

// Get offers for a specific date
var dateOffers = await specialOfferService.GetOffersForDateAsync(restaurantId, targetDate);

// Search offers by text
var searchResults = await specialOfferService.SearchOffersAsync(restaurantId, "discount");

// Get offers by priority
var highPriorityOffers = await specialOfferService.GetOffersByPriorityAsync(restaurantId, 3);
```

## Configuration

### AutoMapper Profile

The AutoMapper profile is automatically configured in `DeliveryAppApplicationAutoMapperProfile.cs`:

```csharp
// Special Offer mapping - Enhanced with new properties
CreateMap<SpecialOffer, SpecialOfferDto>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.IsValidNow, opt => opt.Ignore()) // Computed property
    .ForMember(dest => dest.IsExpired, opt => opt.Ignore()) // Computed property
    .ForMember(dest => dest.IsUpcoming, opt => opt.Ignore()) // Computed property
    .ForMember(dest => dest.RemainingTime, opt => opt.Ignore()); // Computed property
```

### Background Service

The recurring offers background service runs every 15 minutes to process recurring offers:

```csharp
public class RecurringOffersBackgroundService : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromMinutes(15);
    // ... implementation
}
```

## Business Rules

### Validation Rules
1. **Date Validation**: Start date must be before end date and cannot be in the past
2. **Ownership Validation**: Only restaurant owners or admins can modify offers
3. **Conflict Detection**: Maximum 5 active offers at the same time
4. **Time Conflicts**: No overlapping time-based offers
5. **Recurrence Validation**: Valid recurrence patterns required for recurring offers

### Status Transitions
- **Draft** → **Active**: Manual activation
- **Active** → **Paused**: Manual pause
- **Paused** → **Active**: Manual resume (if within date range)
- **Active** → **Inactive**: Manual deactivation
- **Active** → **Expired**: Automatic when end date is reached

### Priority System
- Higher priority numbers = higher priority
- Offers are ordered by priority first, then by start date
- Priority 1 is the default

## Testing

Comprehensive unit tests are provided in `SpecialOfferAppServiceTests.cs` covering:

- ✅ CRUD operations
- ✅ Validation rules
- ✅ Authorization
- ✅ Status management
- ✅ Scheduling functionality
- ✅ Search and filtering
- ✅ Error handling

## API Endpoints

The service provides the following endpoints:

### Basic CRUD
- `GET /api/app/special-offer` - Get list of offers
- `GET /api/app/special-offer/{id}` - Get specific offer
- `POST /api/app/special-offer` - Create new offer
- `PUT /api/app/special-offer/{id}` - Update offer
- `DELETE /api/app/special-offer/{id}` - Delete offer

### Advanced Operations
- `POST /api/app/special-offer/{id}/activate` - Activate offer
- `POST /api/app/special-offer/{id}/deactivate` - Deactivate offer
- `POST /api/app/special-offer/{id}/pause` - Pause offer
- `POST /api/app/special-offer/{id}/resume` - Resume offer
- `POST /api/app/special-offer/{id}/schedule` - Schedule offer

### Query Operations
- `GET /api/app/special-offer/by-status/{restaurantId}/{status}` - Get offers by status
- `GET /api/app/special-offer/recurring/{restaurantId}` - Get recurring offers
- `GET /api/app/special-offer/upcoming/{restaurantId}` - Get upcoming offers
- `GET /api/app/special-offer/expired/{restaurantId}` - Get expired offers
- `GET /api/app/special-offer/search/{restaurantId}?term={searchTerm}` - Search offers
- `GET /api/app/special-offer/by-category/{restaurantId}/{category}` - Get offers by category

## Performance Considerations

1. **Indexing**: Ensure proper database indexes on frequently queried fields
2. **Pagination**: Use pagination for large result sets
3. **Caching**: Consider caching for frequently accessed offers
4. **Background Processing**: Recurring offers are processed in the background

## Security

1. **Authorization**: Role-based access control (Admin, Manager, Owner)
2. **Ownership Validation**: Restaurant owners can only modify their own offers
3. **Input Validation**: Comprehensive validation of all inputs
4. **Audit Trail**: Full audit trail through ABP framework

## Monitoring and Logging

1. **Structured Logging**: Comprehensive logging with structured data
2. **Error Tracking**: Detailed error logging with context
3. **Performance Metrics**: Background service performance monitoring
4. **Usage Analytics**: Offer usage tracking and analytics

## Future Enhancements

1. **Notification System**: Automatic notifications for offer changes
2. **Analytics Dashboard**: Advanced reporting and analytics
3. **A/B Testing**: Offer performance testing capabilities
4. **Integration**: Third-party marketing platform integration
5. **Mobile Push Notifications**: Real-time offer notifications

## Troubleshooting

### Common Issues

1. **Offer Not Appearing**: Check status, date range, and active state
2. **Recurring Offers Not Working**: Verify recurrence pattern and background service
3. **Authorization Errors**: Ensure proper user roles and ownership
4. **Validation Errors**: Check date ranges and business rules

### Debug Information

Enable debug logging for detailed information:

```json
{
  "Logging": {
    "LogLevel": {
      "DeliveryApp.Application.Services.SpecialOfferAppService": "Debug"
    }
  }
}
```

## Support

For issues or questions regarding the SpecialOffer implementation:

1. Check the unit tests for usage examples
2. Review the business rules and validation logic
3. Enable debug logging for detailed information
4. Consult the ABP framework documentation for advanced features
