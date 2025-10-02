using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    [Authorize(Roles = "admin")]
    public class AdminUserAppService : ApplicationService, IAdminUserAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly IEmailNotifier _emailNotifier;

        public AdminUserAppService(
            IdentityUserManager userManager,
            IRepository<AppUser, Guid> userRepository,
            ICurrentUser currentUser,
            IRealtimeNotifier realtimeNotifier,
            IEmailNotifier emailNotifier)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _realtimeNotifier = realtimeNotifier;
            _emailNotifier = emailNotifier;
        }

        public async Task<PagedResultDto<UserDto>> GetPendingUsersAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input)
        {
            var queryable = await _userRepository.GetQueryableAsync();
            var query = queryable.Where(u => u.ReviewStatus == Domain.Enums.ReviewStatus.Pending);
            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtos = ObjectMapper.Map<System.Collections.Generic.List<AppUser>, System.Collections.Generic.List<UserDto>>(users);
            return new PagedResultDto<UserDto>(total, dtos);
        }

        public async Task<UserDto> ApproveAsync(ApproveUserDto input)
        {
            var user = await _userRepository.GetAsync(input.UserId);

            // Use the existing ReviewStatus system
            user.ReviewStatus = Domain.Enums.ReviewStatus.Accepted;
            user.ReviewReason = null; // Clear any previous rejection reason
            
            // Set email/phone confirmed flags as requested
            user.IsEmailConfirmed = input.ConfirmEmail;
            user.IsPhoneConfirmed = input.ConfirmPhone;
            user.ApprovedTime = Clock.Now;
            user.ApprovedById = _currentUser.GetId();

            await _userRepository.UpdateAsync(user, autoSave: true);

            // Send real-time notification
            await _realtimeNotifier.NotifyUserApprovedAsync(user.Id);

            // Send email notification if email is confirmed
            if (input.ConfirmEmail && !string.IsNullOrEmpty(user.Email))
            {
                await _emailNotifier.SendAccountApprovedAsync(user.Email, user.Name ?? "User");
            }

            return ObjectMapper.Map<AppUser, UserDto>(user);
        }

        public async Task<UserDto> RejectAsync(RejectUserDto input)
        {
            var user = await _userRepository.GetAsync(input.UserId);

            // Use the existing ReviewStatus system
            user.ReviewStatus = Domain.Enums.ReviewStatus.Rejected;
            user.ReviewReason = input.Reason ?? "Application rejected by admin";
            
            // Clear approval flags
            user.IsEmailConfirmed = false;
            user.IsPhoneConfirmed = false;
            user.ApprovedTime = null;
            user.ApprovedById = null;

            await _userRepository.UpdateAsync(user, autoSave: true);

            // Optionally send rejection notification email
            if (!string.IsNullOrEmpty(user.Email))
            {
                // Could implement SendAccountRejectedAsync method
                // await _emailNotifier.SendAccountRejectedAsync(user.Email, user.Name ?? "User", user.ReviewReason);
            }

            return ObjectMapper.Map<AppUser, UserDto>(user);
        }
    }
}
