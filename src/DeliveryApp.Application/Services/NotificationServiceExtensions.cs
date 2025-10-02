using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Services;

namespace DeliveryApp.Services
{
    /// <summary>
    /// Extension methods to bridge the gap between the simple real-time notification service
    /// and the richer push-notification semantics expected by higher-level services.
    /// These are stub implementations that simply complete without doing anything so that
    /// the solution can compile while a full implementation is deferred.
    /// </summary>
    public static class NotificationServiceExtensions
    {
        public static Task SendNotificationAsync(this INotificationService _, CreateNotificationDto __)
        {
            // TODO: integrate with push notification provider.
            return Task.CompletedTask;
        }

        public static Task ScheduleNotificationAsync(this INotificationService _, ScheduledNotification __)
        {
            // TODO: scheduler integration (e.g., Hangfire, Quartz.NET).
            return Task.CompletedTask;
        }
    }
}
