using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;

// Alias Firebase types to avoid clashes with our DTO classes
using FirebaseMessage = FirebaseAdmin.Messaging.Message;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;
using FirebaseAndroidConfig = FirebaseAdmin.Messaging.AndroidConfig;
using FirebaseAndroidNotification = FirebaseAdmin.Messaging.AndroidNotification;
using FirebaseApnsConfig = FirebaseAdmin.Messaging.ApnsConfig;
using FirebaseAps = FirebaseAdmin.Messaging.Aps;
using FirebaseApsAlert = FirebaseAdmin.Messaging.ApsAlert;
using FirebaseWebpushConfig = FirebaseAdmin.Messaging.WebpushConfig;
using FirebaseWebpushNotification = FirebaseAdmin.Messaging.WebpushNotification;
using FirebasePriority = FirebaseAdmin.Messaging.Priority;
using AppNotificationPriority = DeliveryApp.Application.Contracts.Dtos.NotificationPriority;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Firebase Cloud Messaging service implementation
    /// </summary>
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationAnalyticsService _analyticsService;
        private readonly FirebaseMessaging _messaging;

        public FirebaseNotificationService(
            ILogger<FirebaseNotificationService> logger,
            IConfiguration configuration,
            INotificationAnalyticsService analyticsService)
        {
            _logger = logger;
            _configuration = configuration;
            _analyticsService = analyticsService;
            
            InitializeFirebase();
            _messaging = FirebaseMessaging.DefaultInstance;
        }

        /// <summary>
        /// Send push notification to a single device
        /// </summary>
        public async Task<NotificationResult> SendNotificationAsync(string deviceToken, NotificationMessage message)
        {
            try
            {
                var firebaseMessage = BuildFirebaseMessage(deviceToken, message);
                var notificationId = Guid.NewGuid().ToString();
                
                var response = await _messaging.SendAsync(firebaseMessage);
                
                var result = new NotificationResult
                {
                    Success = !string.IsNullOrEmpty(response),
                    NotificationId = notificationId,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = 1,
                    FailedCount = 0
                };

                // Track notification sent
                await _analyticsService.TrackNotificationSentAsync(new NotificationTrackingData
                {
                    NotificationId = notificationId,
                    DeviceToken = deviceToken,
                    Platform = GetPlatformFromToken(deviceToken),
                    SentAt = DateTime.UtcNow,
                    Metadata = message.CustomData
                });

                _logger.LogInformation("Notification sent successfully to device {DeviceToken}", MaskToken(deviceToken));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to device {DeviceToken}", MaskToken(deviceToken));
                
                return new NotificationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = 0,
                    FailedCount = 1,
                    FailedTokens = new List<string> { deviceToken }
                };
            }
        }

        /// <summary>
        /// Send push notification to multiple devices
        /// </summary>
        public async Task<List<NotificationResult>> SendNotificationToMultipleAsync(List<string> deviceTokens, NotificationMessage message)
        {
            var results = new List<NotificationResult>();
            
            try
            {
                // Firebase supports batch sending up to 500 tokens
                const int batchSize = 500;
                var batches = deviceTokens.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    var batchResult = await SendBatchNotificationAsync(batch.ToList(), message);
                    results.Add(batchResult);
                }

                _logger.LogInformation("Batch notifications sent to {TokenCount} devices", deviceTokens.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send batch notifications");
                
                results.Add(new NotificationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = 0,
                    FailedCount = deviceTokens.Count,
                    FailedTokens = deviceTokens
                });

                return results;
            }
        }

        /// <summary>
        /// Send notification to a topic (group of users)
        /// </summary>
        public async Task<NotificationResult> SendNotificationToTopicAsync(string topic, NotificationMessage message)
        {
            try
            {
                var firebaseMessage = BuildTopicMessage(topic, message);
                var notificationId = Guid.NewGuid().ToString();
                
                var response = await _messaging.SendAsync(firebaseMessage);
                
                var result = new NotificationResult
                {
                    Success = !string.IsNullOrEmpty(response),
                    NotificationId = notificationId,
                    SentAt = DateTime.UtcNow
                };

                // Track topic notification
                await _analyticsService.TrackNotificationSentAsync(new NotificationTrackingData
                {
                    NotificationId = notificationId,
                    SentAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["topic"] = topic,
                        ["customData"] = message.CustomData
                    }
                });

                _logger.LogInformation("Topic notification sent successfully to topic {Topic}", topic);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to topic {Topic}", topic);
                
                return new NotificationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = 0,
                    FailedCount = 1
                };
            }
        }

        /// <summary>
        /// Subscribe user to a notification topic
        /// </summary>
        public async Task<bool> SubscribeToTopicAsync(string deviceToken, string topic)
        {
            try
            {
                await _messaging.SubscribeToTopicAsync(new List<string> { deviceToken }, topic);
                
                _logger.LogInformation("Device {DeviceToken} subscribed to topic {Topic}", 
                    MaskToken(deviceToken), topic);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe device {DeviceToken} to topic {Topic}", 
                    MaskToken(deviceToken), topic);
                
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe user from a notification topic
        /// </summary>
        public async Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic)
        {
            try
            {
                await _messaging.UnsubscribeFromTopicAsync(new List<string> { deviceToken }, topic);
                
                _logger.LogInformation("Device {DeviceToken} unsubscribed from topic {Topic}", 
                    MaskToken(deviceToken), topic);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe device {DeviceToken} from topic {Topic}", 
                    MaskToken(deviceToken), topic);
                
                return false;
            }
        }

        /// <summary>
        /// Schedule a notification for future delivery
        /// </summary>
        public async Task<string> ScheduleNotificationAsync(ScheduledNotification notification)
        {
            try
            {
                // For demo purposes, storing in memory
                // In production, use a job scheduler like Hangfire or Quartz.NET
                var notificationId = Guid.NewGuid().ToString();
                
                // Schedule the notification using proper async/await pattern
                var delay = notification.ScheduledTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    // Use proper async background task instead of ContinueWith
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(delay);
                            
                            if (!string.IsNullOrEmpty(notification.Topic))
                            {
                                await SendNotificationToTopicAsync(notification.Topic, notification.Message);
                            }
                            else if (notification.DeviceTokens?.Any() == true)
                            {
                                await SendNotificationToMultipleAsync(notification.DeviceTokens, notification.Message);
                            }
                            
                            _logger.LogInformation("Scheduled notification sent successfully: {NotificationId}", notificationId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending scheduled notification: {NotificationId}", notificationId);
                        }
                    });
                }
                else
                {
                    // Send immediately if scheduled time has passed
                    if (!string.IsNullOrEmpty(notification.Topic))
                    {
                        await SendNotificationToTopicAsync(notification.Topic, notification.Message);
                    }
                    else if (notification.DeviceTokens?.Any() == true)
                    {
                        await SendNotificationToMultipleAsync(notification.DeviceTokens, notification.Message);
                    }
                }

                _logger.LogInformation("Notification scheduled for {ScheduledTime}", notification.ScheduledTime);
                return notificationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule notification");
                throw;
            }
        }

        /// <summary>
        /// Cancel a scheduled notification
        /// </summary>
        public async Task<bool> CancelScheduledNotificationAsync(string notificationId)
        {
            try
            {
                // Implementation depends on the job scheduler used
                // For demo purposes, returning true
                await Task.CompletedTask;
                
                _logger.LogInformation("Scheduled notification {NotificationId} cancelled", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel scheduled notification {NotificationId}", notificationId);
                return false;
            }
        }

        /// <summary>
        /// Get notification delivery statistics
        /// </summary>
        public async Task<NotificationStatistics> GetNotificationStatisticsAsync(string notificationId)
        {
            try
            {
                // In a real implementation, this would query the analytics database
                // For demo purposes, returning sample data
                await Task.CompletedTask;
                
                return new NotificationStatistics
                {
                    NotificationId = notificationId,
                    TotalSent = 1000,
                    TotalDelivered = 950,
                    TotalOpened = 380,
                    TotalClicked = 95,
                    TotalFailed = 50,
                    DeliveryRate = 95.0,
                    OpenRate = 40.0,
                    ClickRate = 25.0,
                    SentAt = DateTime.UtcNow.AddHours(-1),
                    PlatformBreakdown = new Dictionary<string, int>
                    {
                        ["iOS"] = 600,
                        ["Android"] = 350,
                        ["Web"] = 50
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification statistics for {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Validate device token
        /// </summary>
        public async Task<bool> ValidateDeviceTokenAsync(string deviceToken)
        {
            try
            {
                var testMessage = new FirebaseMessage
                {
                    Token = deviceToken,
                    Notification = new FirebaseNotification
                    {
                        Title = "Test",
                        Body = "Token validation"
                    },
                    Android = new FirebaseAndroidConfig { Priority = FirebasePriority.High },
                    Apns = new FirebaseApnsConfig
                    {
                        Aps = new FirebaseAps { ContentAvailable = true }
                    }
                };

                // Dry-run validation
                await _messaging.SendAsync(testMessage, dryRun: true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Invalid device token: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Send notification with A/B testing
        /// </summary>
        public async Task<NotificationResult> SendABTestNotificationAsync(ABTestNotification notification)
        {
            try
            {
                var totalTokens = notification.DeviceTokens.Count;
                var splitIndex = (int)(totalTokens * (notification.SplitPercentage / 100.0));
                
                var variantATokens = notification.DeviceTokens.Take(splitIndex).ToList();
                var variantBTokens = notification.DeviceTokens.Skip(splitIndex).ToList();

                // Send variant A
                var variantAResults = await SendNotificationToMultipleAsync(variantATokens, notification.VariantA);
                
                // Send variant B
                var variantBResults = await SendNotificationToMultipleAsync(variantBTokens, notification.VariantB);

                // Combine results
                var combinedResult = new NotificationResult
                {
                    Success = variantAResults.All(r => r.Success) && variantBResults.All(r => r.Success),
                    NotificationId = notification.TestId,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = variantAResults.Sum(r => r.DeliveredCount) + variantBResults.Sum(r => r.DeliveredCount),
                    FailedCount = variantAResults.Sum(r => r.FailedCount) + variantBResults.Sum(r => r.FailedCount)
                };

                // Track A/B test metrics
                notification.Metrics = new ABTestMetrics
                {
                    VariantAId = "A",
                    VariantBId = "B",
                    VariantASent = variantATokens.Count,
                    VariantBSent = variantBTokens.Count
                };

                _logger.LogInformation("A/B test notification sent: Variant A to {VariantACount}, Variant B to {VariantBCount}", 
                    variantATokens.Count, variantBTokens.Count);

                return combinedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send A/B test notification");
                throw;
            }
        }

        #region Private Methods

        private void InitializeFirebase()
        {
            try
            {
                var serviceAccountPath = _configuration["Firebase:ServiceAccountPath"];
                var projectId = _configuration["Firebase:ProjectId"];

                if (string.IsNullOrEmpty(serviceAccountPath) || string.IsNullOrEmpty(projectId))
                {
                    _logger.LogWarning("Firebase not initialized: ServiceAccountPath or ProjectId is missing in configuration. Push notifications will be disabled.");
                    return;
                }

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(serviceAccountPath),
                        ProjectId = projectId
                    });
                }

                _logger.LogInformation("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase. Push notifications will be disabled.");
            }
        }

        private FirebaseMessage BuildFirebaseMessage(string deviceToken, NotificationMessage message)
        {
            var firebaseMessage = new FirebaseMessage
            {
                Token = deviceToken,
                Notification = new FirebaseNotification
                {
                    Title = message.Title,
                    Body = message.Body,
                    ImageUrl = message.ImageUrl
                },
                Data = message.Data ?? new Dictionary<string, string>(),
                Android = BuildAndroidConfig(message),
                Apns = BuildApnsConfig(message),
                Webpush = BuildWebpushConfig(message)
            };

            return firebaseMessage;
        }

        private FirebaseMessage BuildTopicMessage(string topic, NotificationMessage message)
        {
            var firebaseMessage = new FirebaseMessage
            {
                Topic = topic,
                Notification = new FirebaseNotification
                {
                    Title = message.Title,
                    Body = message.Body,
                    ImageUrl = message.ImageUrl
                },
                Data = message.Data ?? new Dictionary<string, string>(),
                Android = BuildAndroidConfig(message),
                Apns = BuildApnsConfig(message),
                Webpush = BuildWebpushConfig(message)
            };

            return firebaseMessage;
        }

        private FirebaseAndroidConfig BuildAndroidConfig(NotificationMessage message)
        {
            var priority = message.Priority switch
            {
                AppNotificationPriority.Low => FirebasePriority.Normal,
                AppNotificationPriority.Normal => FirebasePriority.Normal,
                AppNotificationPriority.High => FirebasePriority.High,
                AppNotificationPriority.Critical => FirebasePriority.High,
                _ => FirebasePriority.Normal
            };

            var androidNotification = new FirebaseAndroidNotification
            {
                Title = message.Title,
                Body = message.Body,
                Icon = "ic_notification",
                Color = "#FF6B35",
                Sound = message.Sound ?? "default",
                ClickAction = message.ClickAction,
                ChannelId = "default"
            };

            return new FirebaseAndroidConfig
            {
                Priority = priority,
                Notification = androidNotification
            };
        }

        private FirebaseApnsConfig BuildApnsConfig(NotificationMessage message)
        {
            var aps = new FirebaseAps
            {
                Sound = message.Sound ?? "default"
            };

            if (message.Badge)
            {
                aps.Badge = 1;
            }

            if (!string.IsNullOrEmpty(message.Category))
            {
                aps.Category = message.Category;
            }

            aps.Alert = new FirebaseApsAlert { Title = message.Title, Body = message.Body };

            return new FirebaseApnsConfig { Aps = aps };
        }

        private FirebaseWebpushConfig BuildWebpushConfig(NotificationMessage message)
        {
            var webNotif = new FirebaseWebpushNotification
            {
                Title = message.Title,
                Body = message.Body,
                Icon = message.ImageUrl
            };

            return new FirebaseWebpushConfig { Notification = webNotif };
        }

        private async Task<NotificationResult> SendBatchNotificationAsync(List<string> deviceTokens, NotificationMessage message)
        {
            try
            {
                var messages = deviceTokens.Select(token => BuildFirebaseMessage(token, message)).ToList();
                var response = await _messaging.SendAllAsync(messages);

                var result = new NotificationResult
                {
                    Success = response.FailureCount == 0,
                    NotificationId = Guid.NewGuid().ToString(),
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = response.SuccessCount,
                    FailedCount = response.FailureCount
                };

                // Collect failed tokens
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        result.FailedTokens.Add(deviceTokens[i]);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send batch notification");
                throw;
            }
        }

        public async Task<NotificationResult> GetNotificationStatusAsync(string notificationId)
        {
            try
            {
                _logger.LogInformation("Getting notification status for: {NotificationId}", notificationId);
                
                // In a real implementation, you would query your database or Firebase
                // to get the actual status of the notification
                // For now, we'll return a mock result
                return new NotificationResult
                {
                    Success = true,
                    NotificationId = notificationId,
                    SentAt = DateTime.UtcNow.AddMinutes(-5), // Mock sent time
                    DeliveredCount = 1,
                    FailedCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification status for: {NotificationId}", notificationId);
                return new NotificationResult
                {
                    Success = false,
                    NotificationId = notificationId,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    DeliveredCount = 0,
                    FailedCount = 1
                };
            }
        }

        private string GetPlatformFromToken(string deviceToken)
        {
            // This is a simplified implementation
            // In reality, you'd need to track platform information separately
            if (deviceToken.Length > 100)
                return "iOS";
            else if (deviceToken.StartsWith("f"))
                return "Android";
            else
                return "Web";
        }

        private string MaskToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 8)
                return "****";
            
            return token.Substring(0, 4) + "****" + token.Substring(token.Length - 4);
        }

        #endregion
    }
}

