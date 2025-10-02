using System;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// Restaurant owner request for advertisement placement that requires admin approval
    /// </summary>
    public class AdRequest : FullAuditedAggregateRoot<Guid>
    {
        public AdRequest() { }

        public AdRequest(Guid id) : base(id) { }

        /// <summary>
        /// Restaurant submitting the ad request
        /// </summary>
        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string LinkUrl { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; } = 1;

        [StringLength(50)]
        public string? TargetAudience { get; set; }

        [StringLength(50)]
        public string? Location { get; set; }

        /// <summary>
        /// Ad request status for admin approval workflow
        /// </summary>
        public AdRequestStatus Status { get; set; } = AdRequestStatus.Pending;

        /// <summary>
        /// Admin review reason for approval/rejection
        /// </summary>
        [StringLength(1000)]
        public string? ReviewReason { get; set; }

        /// <summary>
        /// Admin who reviewed the request
        /// </summary>
        public Guid? ReviewedById { get; set; }
        public virtual AppUser? ReviewedBy { get; set; }

        /// <summary>
        /// Date when admin reviewed the request
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Final advertisement created from this request (if approved)
        /// </summary>
        public Guid? AdvertisementId { get; set; }
        public virtual Advertisement? Advertisement { get; set; }

        /// <summary>
        /// Requested budget for the ad campaign
        /// </summary>
        [Range(0, 100000)]
        public decimal? Budget { get; set; }

        public void Approve(Guid adminId, string? reason = null)
        {
            Status = AdRequestStatus.Approved;
            ReviewedById = adminId;
            ReviewedAt = DateTime.UtcNow;
            ReviewReason = reason;
        }

        public void Reject(Guid adminId, string reason)
        {
            Status = AdRequestStatus.Rejected;
            ReviewedById = adminId;
            ReviewedAt = DateTime.UtcNow;
            ReviewReason = reason;
        }

        public void MarkAsProcessed(Guid advertisementId)
        {
            AdvertisementId = advertisementId;
            Status = AdRequestStatus.Processed;
        }
    }
}
