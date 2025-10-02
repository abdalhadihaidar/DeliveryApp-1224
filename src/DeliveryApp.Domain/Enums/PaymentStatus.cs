using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Domain.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Pending", Description = "معلق")]
        Pending = 0,
        [Display(Name = "Paid", Description = "مدفوع")]
        Paid = 1,
        [Display(Name = "Failed", Description = "فشل")]
        Failed = 2,
        [Display(Name = "Refunded", Description = "مسترد")]
        Refunded = 3,
        [Display(Name = "Cancelled", Description = "ملغي")]
        Cancelled = 4
    }
}
