using System;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class ApproveUserDto
    {
        public Guid UserId { get; set; }
        public bool ConfirmEmail { get; set; }
        public bool ConfirmPhone { get; set; }
    }

    public class RejectUserDto
    {
        public Guid UserId { get; set; }
        public string? Reason { get; set; }
    }
}
