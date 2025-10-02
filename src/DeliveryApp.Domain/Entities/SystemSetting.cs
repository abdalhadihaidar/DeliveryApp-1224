using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// System settings entity for storing application configuration
    /// </summary>
    public class SystemSetting : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// General application settings (JSON)
        /// </summary>
        [StringLength(4000)]
        public string GeneralSettings { get; set; } = string.Empty;

        /// <summary>
        /// Notification settings (JSON)
        /// </summary>
        [StringLength(4000)]
        public string NotificationSettings { get; set; } = string.Empty;

        /// <summary>
        /// Delivery settings (JSON)
        /// </summary>
        [StringLength(4000)]
        public string DeliverySettings { get; set; } = string.Empty;

        /// <summary>
        /// Security settings (JSON)
        /// </summary>
        [StringLength(4000)]
        public string SecuritySettings { get; set; } = string.Empty;

        /// <summary>
        /// Maintenance settings (JSON)
        /// </summary>
        [StringLength(4000)]
        public string MaintenanceSettings { get; set; } = string.Empty;

        /// <summary>
        /// Settings version for migration purposes
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Whether settings are active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
