using System;
using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public partial class UserEngagementMetrics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalSessions { get; set; }
        public int TotalScreenViews { get; set; }
        public int TotalOrders { get; set; }

        public double TotalTimeSpent { get; set; }
        public double AverageSessionDuration { get; set; }
        public DateTime? LastActiveDate { get; set; }

        public double RetentionRate { get; set; }
        public double ConversionRate { get; set; }

        public Dictionary<string, int> ActivityBreakdown { get; set; } = new();
        public Dictionary<DateTime, int> DailyActivity { get; set; } = new();
        public Dictionary<string, int> DeviceBreakdown { get; set; } = new();
        public Dictionary<string, int> ChannelBreakdown { get; set; } = new();
    }
} 
