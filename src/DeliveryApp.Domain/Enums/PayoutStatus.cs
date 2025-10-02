using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Domain.Enums
{
    public enum PayoutStatus
    {
        [Display(Name = "Pending", Description = "معلق")]
        Pending = 0,
        
        [Display(Name = "Processing", Description = "قيد المعالجة")]
        Processing = 1,
        
        [Display(Name = "Completed", Description = "مكتمل")]
        Completed = 2,
        
        [Display(Name = "Failed", Description = "فشل")]
        Failed = 3
    }
}
