using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Domain.Enums
{
    public enum ReviewStatus
    {
        [Display(Name = "Pending", Description = "معلق")]
        Pending = 0,
        
        [Display(Name = "Accepted", Description = "مقبول")]
        Accepted = 1,
        
        [Display(Name = "Rejected", Description = "مرفوض")]
        Rejected = 2
    }
}
