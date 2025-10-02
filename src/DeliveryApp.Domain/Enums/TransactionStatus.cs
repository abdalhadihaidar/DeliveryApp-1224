using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Domain.Enums
{
    public enum TransactionStatus
    {
        [Display(Name = "Pending", Description = "معلق")]
        Pending = 0,
        
        [Display(Name = "Processing", Description = "قيد المعالجة")]
        Processing = 1,
        
        [Display(Name = "Succeeded", Description = "نجح")]
        Succeeded = 2,
        
        [Display(Name = "Failed", Description = "فشل")]
        Failed = 3,
        
        [Display(Name = "Cancelled", Description = "ملغي")]
        Cancelled = 4
    }
}
