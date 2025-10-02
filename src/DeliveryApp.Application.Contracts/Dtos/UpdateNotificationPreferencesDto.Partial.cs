using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public partial class UpdateNotificationPreferencesDto
    {
        /// <summary>
        /// Returns a list of field names that were set (non-null) on this DTO instance.
        /// The implementation is simplified and returns an empty list when reflection is unavailable.
        /// </summary>
        public List<string> GetUpdatedFields()
        {
            var fields = new List<string>();
            if (EmailNotifications.HasValue) fields.Add(nameof(EmailNotifications));
            if (PushNotifications.HasValue) fields.Add(nameof(PushNotifications));
            if (SmsNotifications.HasValue) fields.Add(nameof(SmsNotifications));
            if (OrderUpdates.HasValue) fields.Add(nameof(OrderUpdates));
            if (PromotionalOffers.HasValue) fields.Add(nameof(PromotionalOffers));
            if (RestaurantUpdates.HasValue) fields.Add(nameof(RestaurantUpdates));
            if (DeliveryUpdates.HasValue) fields.Add(nameof(DeliveryUpdates));
            if (PaymentNotifications.HasValue) fields.Add(nameof(PaymentNotifications));
            if (SecurityAlerts.HasValue) fields.Add(nameof(SecurityAlerts));
            if (NewsAndUpdates.HasValue) fields.Add(nameof(NewsAndUpdates));
            if (SurveyRequests.HasValue) fields.Add(nameof(SurveyRequests));
            if (WeeklyDigest.HasValue) fields.Add(nameof(WeeklyDigest));
            if (QuietHoursEnabled.HasValue) fields.Add(nameof(QuietHoursEnabled));
            if (QuietHoursStart.HasValue) fields.Add(nameof(QuietHoursStart));
            if (QuietHoursEnd.HasValue) fields.Add(nameof(QuietHoursEnd));
            if (PreferredLanguage != null) fields.Add(nameof(PreferredLanguage));
            if (TimeZone != null) fields.Add(nameof(TimeZone));
            if (PreferredTiming != null) fields.Add(nameof(PreferredTiming));
            if (MutedTopics != null) fields.Add(nameof(MutedTopics));
            if (CustomPreferences != null) fields.Add(nameof(CustomPreferences));
            return fields;
        }
    }
} 
