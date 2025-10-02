using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using NotificationType = DeliveryApp.Application.Contracts.Dtos.NotificationType;
using NotificationPriority = DeliveryApp.Application.Contracts.Dtos.NotificationPriority;
using CampaignStatus = DeliveryApp.Domain.Entities.CampaignStatus;
using CustomerSegment = DeliveryApp.Application.Contracts.Dtos.CustomerSegment;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Service for restaurant owners to send targeted notifications to their customers
    /// </summary>
    public class RestaurantOwnerNotificationService : ApplicationService, IRestaurantOwnerNotificationService
    {
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IRepository<Order, Guid> _orderRepository;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IRepository<FavoriteRestaurant, Guid> _favoriteRepository;
        private readonly IRepository<NotificationPreference, string> _notificationPreferenceRepository;
        private readonly IRepository<NotificationCampaign, Guid> _campaignRepository;
        private readonly IRepository<Domain.Entities.NotificationTemplate, Guid> _templateRepository;
        private readonly IFirebaseNotificationService _firebaseNotificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RestaurantOwnerNotificationService> _logger;

        public RestaurantOwnerNotificationService(
            IRepository<Restaurant, Guid> restaurantRepository,
            IRepository<Order, Guid> orderRepository,
            IRepository<AppUser, Guid> userRepository,
            IRepository<FavoriteRestaurant, Guid> favoriteRepository,
            IRepository<NotificationPreference, string> notificationPreferenceRepository,
            IRepository<NotificationCampaign, Guid> campaignRepository,
            IRepository<Domain.Entities.NotificationTemplate, Guid> templateRepository,
            IFirebaseNotificationService firebaseNotificationService,
            IEmailService emailService,
            ILogger<RestaurantOwnerNotificationService> logger)
        {
            _restaurantRepository = restaurantRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _favoriteRepository = favoriteRepository;
            _notificationPreferenceRepository = notificationPreferenceRepository;
            _campaignRepository = campaignRepository;
            _templateRepository = templateRepository;
            _firebaseNotificationService = firebaseNotificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<NotificationCampaignDto> SendNotificationToCustomersAsync(SendRestaurantNotificationDto dto, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to send notifications for this restaurant");
                }

                // Get targeted customers
                var customers = await GetTargetedCustomersAsync(dto.RestaurantId, dto.TargetingCriteria);

                // Create campaign
                var campaignId = GuidGenerator.Create();
                var campaign = new NotificationCampaign
                {
                    RestaurantId = dto.RestaurantId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = (Domain.Entities.NotificationType)dto.Type,
                    Priority = (Domain.Entities.NotificationPriority)dto.Priority,
                    Status = CampaignStatus.Sending,
                    TotalRecipients = customers.Count,
                    SentCount = 0,
                    DeliveredCount = 0,
                    FailedCount = 0,
                    ScheduledTime = dto.ScheduledTime,
                    CustomData = dto.CustomData,
                    TargetingCriteria = SerializeTargetingCriteria(dto.TargetingCriteria)
                };

                await _campaignRepository.InsertAsync(campaign);

                // Send notifications
                var sentCount = 0;
                var deliveredCount = 0;
                var failedCount = 0;

                foreach (var customer in customers)
                {
                    try
                    {
                        var success = await SendNotificationToCustomerAsync(customer, dto);
                        if (success)
                        {
                            deliveredCount++;
                        }
                        else
                        {
                            failedCount++;
                        }
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to customer {CustomerId}", customer.CustomerId);
                        failedCount++;
                    }
                }

                // Update campaign status
                campaign.SentCount = sentCount;
                campaign.DeliveredCount = deliveredCount;
                campaign.FailedCount = failedCount;
                campaign.Status = CampaignStatus.Sent;
                campaign.SentAt = DateTime.UtcNow;

                await _campaignRepository.UpdateAsync(campaign);

                _logger.LogInformation("Notification campaign {CampaignId} completed. Sent: {SentCount}, Delivered: {DeliveredCount}, Failed: {FailedCount}", 
                    campaignId, sentCount, deliveredCount, failedCount);

                return ObjectMapper.Map<NotificationCampaign, NotificationCampaignDto>(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to customers for restaurant {RestaurantId}", dto.RestaurantId);
                throw;
            }
        }

        public async Task<PagedResultDto<RestaurantCustomerDto>> GetRestaurantCustomersAsync(GetRestaurantCustomersDto dto, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view customers for this restaurant");
                }

                // Get customers with orders from this restaurant
                var customers = await GetRestaurantCustomersInternalAsync(dto.RestaurantId, dto);

                // Apply pagination
                var totalCount = customers.Count;
                var pagedCustomers = customers
                    .Skip(dto.SkipCount)
                    .Take(dto.MaxResultCount)
                    .ToList();

                return new PagedResultDto<RestaurantCustomerDto>(totalCount, pagedCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting restaurant customers for restaurant {RestaurantId}", dto.RestaurantId);
                throw;
            }
        }

        public async Task<List<Contracts.Services.CustomerSegmentDto>> GetCustomerSegmentsAsync(Guid restaurantId, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view customer segments for this restaurant");
                }

                var segments = new List<Contracts.Services.CustomerSegmentDto>();

                // Get all customers for this restaurant
                var customers = await GetRestaurantCustomersInternalAsync(restaurantId, new GetRestaurantCustomersDto { RestaurantId = restaurantId });

                // Calculate segments
                var allCustomers = customers.ToList();
                var vipCustomers = allCustomers.Where(c => c.CustomerSegment == "VIP").ToList();
                var activeCustomers = allCustomers.Where(c => c.CustomerSegment == "Active").ToList();
                var regularCustomers = allCustomers.Where(c => c.CustomerSegment == "Regular").ToList();
                var occasionalCustomers = allCustomers.Where(c => c.CustomerSegment == "Occasional").ToList();
                var inactiveCustomers = allCustomers.Where(c => c.CustomerSegment == "Inactive").ToList();

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.All,
                    Name = "All Customers",
                    Description = "All customers who have ordered from this restaurant",
                    CustomerCount = allCustomers.Count,
                    TotalSpent = allCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = allCustomers.Any() ? allCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = allCustomers.Any() ? allCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.VIP,
                    Name = "VIP Customers",
                    Description = "High-value, frequent customers",
                    CustomerCount = vipCustomers.Count,
                    TotalSpent = vipCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = vipCustomers.Any() ? vipCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = vipCustomers.Any() ? vipCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.Active,
                    Name = "Active Customers",
                    Description = "Regular, active customers",
                    CustomerCount = activeCustomers.Count,
                    TotalSpent = activeCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = activeCustomers.Any() ? activeCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = activeCustomers.Any() ? activeCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.Regular,
                    Name = "Regular Customers",
                    Description = "Occasional but regular customers",
                    CustomerCount = regularCustomers.Count,
                    TotalSpent = regularCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = regularCustomers.Any() ? regularCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = regularCustomers.Any() ? regularCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.Occasional,
                    Name = "Occasional Customers",
                    Description = "Infrequent customers",
                    CustomerCount = occasionalCustomers.Count,
                    TotalSpent = occasionalCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = occasionalCustomers.Any() ? occasionalCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = occasionalCustomers.Any() ? occasionalCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                segments.Add(new Contracts.Services.CustomerSegmentDto
                {
                    Segment = CustomerSegment.Inactive,
                    Name = "Inactive Customers",
                    Description = "Customers who haven't ordered recently",
                    CustomerCount = inactiveCustomers.Count,
                    TotalSpent = inactiveCustomers.Sum(c => c.TotalSpent),
                    AverageOrderValue = inactiveCustomers.Any() ? inactiveCustomers.Average(c => c.AverageOrderValue) : 0,
                    LastActivity = inactiveCustomers.Any() ? inactiveCustomers.Max(c => c.LastOrderDate) : DateTime.MinValue
                });

                return segments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer segments for restaurant {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<PagedResultDto<NotificationCampaignDto>> GetNotificationCampaignsAsync(Guid restaurantId, PagedAndSortedResultRequestDto input, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view campaigns for this restaurant");
                }

                var campaigns = await _campaignRepository.GetListAsync(c => c.RestaurantId == restaurantId);
                var totalCount = campaigns.Count;

                var pagedCampaigns = campaigns
                    .OrderByDescending(c => c.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList();

                return new PagedResultDto<NotificationCampaignDto>(totalCount, 
                    ObjectMapper.Map<List<NotificationCampaign>, List<NotificationCampaignDto>>(pagedCampaigns));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification campaigns for restaurant {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<NotificationCampaignDto> GetNotificationCampaignAsync(Guid campaignId, string restaurantOwnerId)
        {
            try
            {
                var campaign = await _campaignRepository.GetAsync(campaignId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(campaign.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view this campaign");
                }

                return ObjectMapper.Map<NotificationCampaign, NotificationCampaignDto>(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification campaign {CampaignId}", campaignId);
                throw;
            }
        }

        public async Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(Guid campaignId, string restaurantOwnerId)
        {
            try
            {
                var campaign = await _campaignRepository.GetAsync(campaignId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(campaign.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view analytics for this campaign");
                }

                // Calculate analytics (simplified for now)
                var analytics = new NotificationAnalyticsDto
                {
                    CampaignId = campaignId,
                    CampaignTitle = campaign.Title,
                    TotalSent = campaign.SentCount,
                    TotalDelivered = campaign.DeliveredCount,
                    TotalFailed = campaign.FailedCount,
                    DeliveryRate = campaign.SentCount > 0 ? (double)campaign.DeliveredCount / campaign.SentCount * 100 : 0,
                    TotalOpened = 0, // Would need to track opens
                    OpenRate = 0,
                    TotalClicked = 0, // Would need to track clicks
                    ClickRate = 0,
                    SentAt = campaign.SentAt ?? campaign.CreationTime
                };

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification analytics for campaign {CampaignId}", campaignId);
                throw;
            }
        }

        public async Task<bool> CancelNotificationCampaignAsync(Guid campaignId, string restaurantOwnerId)
        {
            try
            {
                var campaign = await _campaignRepository.GetAsync(campaignId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(campaign.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to cancel this campaign");
                }

                if (campaign.Status == CampaignStatus.Sent || campaign.Status == CampaignStatus.Failed)
                {
                    return false; // Cannot cancel already sent campaigns
                }

                campaign.Status = CampaignStatus.Cancelled;
                await _campaignRepository.UpdateAsync(campaign);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling notification campaign {CampaignId}", campaignId);
                throw;
            }
        }

        public async Task<NotificationTemplateDto> CreateNotificationTemplateAsync(CreateNotificationTemplateDto dto, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to create templates for this restaurant");
                }

                var templateId = GuidGenerator.Create();
                var template = new Domain.Entities.NotificationTemplate
                {
                    RestaurantId = dto.RestaurantId,
                    Name = dto.Name,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = (Domain.Entities.NotificationType)dto.Type,
                    Priority = (Domain.Entities.NotificationPriority)dto.Priority,
                    DefaultTargetingCriteria = SerializeTargetingCriteria(dto.DefaultTargetingCriteria),
                    CustomData = dto.CustomData,
                    IsActive = true
                };

                await _templateRepository.InsertAsync(template);

                return ObjectMapper.Map<Domain.Entities.NotificationTemplate, NotificationTemplateDto>(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template for restaurant {RestaurantId}", dto.RestaurantId);
                throw;
            }
        }

        public async Task<NotificationTemplateDto> UpdateNotificationTemplateAsync(Guid templateId, UpdateNotificationTemplateDto dto, string restaurantOwnerId)
        {
            try
            {
                var template = await _templateRepository.GetAsync(templateId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(template.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to update this template");
                }

                if (dto.Name != null) template.Name = dto.Name;
                if (dto.Title != null) template.Title = dto.Title;
                if (dto.Message != null) template.Message = dto.Message;
                if (dto.Type.HasValue) template.Type = (Domain.Entities.NotificationType)dto.Type.Value;
                if (dto.Priority.HasValue) template.Priority = (Domain.Entities.NotificationPriority)dto.Priority.Value;
                if (dto.DefaultTargetingCriteria != null) template.DefaultTargetingCriteria = SerializeTargetingCriteria(dto.DefaultTargetingCriteria);
                if (dto.CustomData != null) template.CustomData = dto.CustomData;
                if (dto.IsActive.HasValue) template.IsActive = dto.IsActive.Value;

                await _templateRepository.UpdateAsync(template);

                return ObjectMapper.Map<Domain.Entities.NotificationTemplate, NotificationTemplateDto>(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<List<NotificationTemplateDto>> GetNotificationTemplatesAsync(Guid restaurantId, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view templates for this restaurant");
                }

                var templates = await _templateRepository.GetListAsync(t => t.RestaurantId == restaurantId && t.IsActive);
                return ObjectMapper.Map<List<Domain.Entities.NotificationTemplate>, List<NotificationTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates for restaurant {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationTemplateAsync(Guid templateId, string restaurantOwnerId)
        {
            try
            {
                var template = await _templateRepository.GetAsync(templateId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(template.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to delete this template");
                }

                await _templateRepository.DeleteAsync(template);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<NotificationCampaignDto> SendNotificationFromTemplateAsync(Guid templateId, CustomerTargetingCriteria? customTargeting, string restaurantOwnerId)
        {
            try
            {
                var template = await _templateRepository.GetAsync(templateId);
                
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(template.RestaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to send notifications from this template");
                }

                var targetingCriteria = customTargeting ?? DeserializeTargetingCriteria(template.DefaultTargetingCriteria);

                var sendDto = new SendRestaurantNotificationDto
                {
                    RestaurantId = template.RestaurantId,
                    Title = template.Title,
                    Message = template.Message,
                    Type = (Contracts.Dtos.NotificationType)template.Type,
                    Priority = (Contracts.Dtos.NotificationPriority)template.Priority,
                    TargetingCriteria = targetingCriteria,
                    CustomData = template.CustomData
                };

                return await SendNotificationToCustomersAsync(sendDto, restaurantOwnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification from template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<RestaurantNotificationStatsDto> GetNotificationStatisticsAsync(Guid restaurantId, DateTime? startDate, DateTime? endDate, string restaurantOwnerId)
        {
            try
            {
                // Verify restaurant ownership
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                if (restaurant.OwnerId.ToString() != restaurantOwnerId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view statistics for this restaurant");
                }

                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var campaigns = await _campaignRepository.GetListAsync(c => 
                    c.RestaurantId == restaurantId && 
                    c.CreationTime >= start && 
                    c.CreationTime <= end);

                var customers = await GetRestaurantCustomersInternalAsync(restaurantId, new GetRestaurantCustomersDto { RestaurantId = restaurantId });

                var stats = new RestaurantNotificationStatsDto
                {
                    RestaurantId = restaurantId,
                    RestaurantName = restaurant.Name,
                    TotalCampaigns = campaigns.Count(),
                    TotalNotificationsSent = campaigns.Sum(c => c.SentCount),
                    TotalNotificationsDelivered = campaigns.Sum(c => c.DeliveredCount),
                    AverageDeliveryRate = campaigns.Any() ? campaigns.Average(c => c.SentCount > 0 ? (double)c.DeliveredCount / c.SentCount * 100 : 0) : 0,
                    TotalNotificationsOpened = 0, // Would need to track opens
                    AverageOpenRate = 0,
                    TotalNotificationsClicked = 0, // Would need to track clicks
                    AverageClickRate = 0,
                    ActiveCustomers = customers.Count,
                    CustomersWithDeviceTokens = customers.Count(c => c.HasDeviceToken),
                    CustomersWithEmailEnabled = customers.Count(c => c.EmailNotificationsEnabled),
                    CustomersWithPushEnabled = customers.Count(c => c.PushNotificationsEnabled),
                    RecentCampaigns = ObjectMapper.Map<List<NotificationCampaign>, List<NotificationCampaignDto>>(campaigns.Take(5).ToList()),
                    CustomerSegments = await GetCustomerSegmentsAsync(restaurantId, restaurantOwnerId)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification statistics for restaurant {RestaurantId}", restaurantId);
                throw;
            }
        }

        #region Private Methods

        private async Task<List<RestaurantCustomerDto>> GetTargetedCustomersAsync(Guid restaurantId, CustomerTargetingCriteria? criteria)
        {
            var customers = await GetRestaurantCustomersInternalAsync(restaurantId, new GetRestaurantCustomersDto { RestaurantId = restaurantId });

            if (criteria == null)
            {
                return customers.ToList();
            }

            var filteredCustomers = customers.AsQueryable();

            // Apply specific customer IDs filter
            if (criteria.SpecificCustomerIds?.Any() == true)
            {
                filteredCustomers = filteredCustomers.Where(c => criteria.SpecificCustomerIds.Contains(c.CustomerId));
            }

            // Apply segment filter
            if (criteria.Segment.HasValue && criteria.Segment.Value != CustomerSegment.All)
            {
                var segmentName = criteria.Segment.Value.ToString();
                filteredCustomers = filteredCustomers.Where(c => c.CustomerSegment == segmentName);
            }

            // Apply date filters
            if (criteria.LastOrderAfter.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.LastOrderDate >= criteria.LastOrderAfter.Value);
            }

            if (criteria.LastOrderBefore.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.LastOrderDate <= criteria.LastOrderBefore.Value);
            }

            // Apply order value filters
            if (criteria.MinOrderValue.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.TotalSpent >= criteria.MinOrderValue.Value);
            }

            if (criteria.MaxOrderValue.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.TotalSpent <= criteria.MaxOrderValue.Value);
            }

            // Apply order count filters
            if (criteria.MinOrderCount.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.TotalOrders >= criteria.MinOrderCount.Value);
            }

            if (criteria.MaxOrderCount.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.TotalOrders <= criteria.MaxOrderCount.Value);
            }

            // Apply other filters
            if (criteria.HasFavoritedRestaurant.HasValue)
            {
                filteredCustomers = filteredCustomers.Where(c => c.IsFavorited == criteria.HasFavoritedRestaurant.Value);
            }

            if (!string.IsNullOrEmpty(criteria.City))
            {
                filteredCustomers = filteredCustomers.Where(c => c.City.Contains(criteria.City, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(criteria.PreferredLanguage))
            {
                filteredCustomers = filteredCustomers.Where(c => c.PreferredLanguage == criteria.PreferredLanguage);
            }

            return filteredCustomers.ToList();
        }

        private async Task<List<RestaurantCustomerDto>> GetRestaurantCustomersInternalAsync(Guid restaurantId, GetRestaurantCustomersDto dto)
        {
            // Get all orders for this restaurant
            var orders = await _orderRepository.GetListAsync(o => o.RestaurantId == restaurantId);
            
            // Get unique customer IDs
            var customerIds = orders.Select(o => o.UserId).Distinct().ToList();
            
            // Get customer details
            var customers = await _userRepository.GetListAsync(u => customerIds.Contains(u.Id));
            
            // Get customer preferences
            var preferences = await _notificationPreferenceRepository.GetListAsync(p => customerIds.Contains(Guid.Parse(p.UserId)));
            var preferencesDict = preferences.ToDictionary(p => p.UserId, p => p);
            
            // Get favorites
            var favorites = await _favoriteRepository.GetListAsync(f => f.RestaurantId == restaurantId && customerIds.Contains(f.UserId));
            var favoriteCustomerIds = favorites.Select(f => f.UserId).ToHashSet();
            
            var result = new List<RestaurantCustomerDto>();
            
            foreach (var customer in customers)
            {
                var customerOrders = orders.Where(o => o.UserId == customer.Id).ToList();
                var totalSpent = customerOrders.Sum(o => o.TotalAmount);
                var averageOrderValue = customerOrders.Any() ? totalSpent / customerOrders.Count : 0;
                var lastOrderDate = customerOrders.Any() ? customerOrders.Max(o => o.OrderDate) : DateTime.MinValue;
                
                // Determine customer segment
                var customerSegment = DetermineCustomerSegment(customerOrders.Count, totalSpent, lastOrderDate);
                
                // Get preferences
                var customerPreferences = preferencesDict.GetValueOrDefault(customer.Id.ToString());
                
                result.Add(new RestaurantCustomerDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.Name,
                    Email = customer.Email ?? string.Empty,
                    PhoneNumber = customer.PhoneNumber ?? string.Empty,
                    ProfileImageUrl = customer.ProfileImageUrl ?? string.Empty,
                    TotalOrders = customerOrders.Count,
                    TotalSpent = totalSpent,
                    LastOrderDate = lastOrderDate,
                    AverageOrderValue = averageOrderValue,
                    CustomerSegment = customerSegment,
                    IsFavorited = favoriteCustomerIds.Contains(customer.Id),
                    PreferredCategories = new List<string>(), // Would need to implement category analysis
                    City = string.Empty, // Would need to implement city extraction from addresses
                    PreferredLanguage = customerPreferences?.PreferredLanguage ?? "en",
                    DeviceToken = customer.DeviceToken ?? string.Empty,
                    HasDeviceToken = !string.IsNullOrEmpty(customer.DeviceToken),
                    EmailNotificationsEnabled = customerPreferences?.EmailNotifications ?? true,
                    PushNotificationsEnabled = customerPreferences?.PushNotifications ?? true
                });
            }
            
            return result;
        }

        private string DetermineCustomerSegment(int orderCount, decimal totalSpent, DateTime lastOrderDate)
        {
            var daysSinceLastOrder = (DateTime.UtcNow - lastOrderDate).TotalDays;
            
            if (orderCount >= 10 && totalSpent >= 1000 && daysSinceLastOrder <= 30)
                return "VIP";
            else if (orderCount >= 5 && daysSinceLastOrder <= 60)
                return "Active";
            else if (orderCount >= 2 && daysSinceLastOrder <= 90)
                return "Regular";
            else if (orderCount >= 1 && daysSinceLastOrder <= 180)
                return "Occasional";
            else
                return "Inactive";
        }

        private async Task<bool> SendNotificationToCustomerAsync(RestaurantCustomerDto customer, SendRestaurantNotificationDto dto)
        {
            try
            {
                var success = false;
                
                // Send push notification if enabled and device token available
                if (customer.PushNotificationsEnabled && customer.HasDeviceToken)
                {
                    var notificationMessage = new NotificationMessage
                    {
                        Title = dto.Title,
                        Body = dto.Message,
                        Priority = dto.Priority,
                        CustomData = dto.CustomData?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new Dictionary<string, object>()
                    };
                    
                    var result = await _firebaseNotificationService.SendNotificationAsync(customer.DeviceToken ?? string.Empty, notificationMessage);
                    success = result.Success;
                }
                
                // Send email if enabled
                if (customer.EmailNotificationsEnabled && !string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, dto.Title, dto.Message);
                    success = true; // Assume email sending is successful
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to customer {CustomerId}", customer.CustomerId);
                return false;
            }
        }

        private string SerializeTargetingCriteria(CustomerTargetingCriteria? criteria)
        {
            if (criteria == null) return string.Empty;
            return System.Text.Json.JsonSerializer.Serialize(criteria);
        }

        private CustomerTargetingCriteria? DeserializeTargetingCriteria(string? serializedCriteria)
        {
            if (string.IsNullOrEmpty(serializedCriteria)) return null;
            return System.Text.Json.JsonSerializer.Deserialize<CustomerTargetingCriteria>(serializedCriteria);
        }

        #endregion
    }
}
