using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Domain.Enums
{
    public enum RestaurantApprovalStatus
    {
        [Display(Name = "Pending", Description = "في انتظار الموافقة")]
        Pending = 0,
        
        [Display(Name = "Approved", Description = "موافق عليه")]
        Approved = 1,
        
        [Display(Name = "Rejected", Description = "مرفوض")]
        Rejected = 2,
        
        [Display(Name = "Unknown", Description = "حالة غير معروفة")]
        Unknown = 3
    }

    public enum AdRequestStatus
    {
        [Display(Name = "Pending", Description = "في انتظار الموافقة")]
        Pending = 0,
        
        [Display(Name = "Approved", Description = "موافق عليه")]
        Approved = 1,
        
        [Display(Name = "Rejected", Description = "مرفوض")]
        Rejected = 2,
        
        [Display(Name = "Processed", Description = "تم المعالجة")]
        Processed = 3
    }
}
