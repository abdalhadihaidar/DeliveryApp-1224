using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;

namespace DeliveryApp.Application.Tests.Services
{
    public class AdminUserAppServiceTests : DeliveryAppApplicationTestBase<DeliveryAppApplicationTestModule>
    {
        private readonly IAdminUserAppService _adminUserAppService;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IRepository<AppUser, Guid> _userRepository;

        public AdminUserAppServiceTests()
        {
            _adminUserAppService = GetRequiredService<IAdminUserAppService>();
            _userManager = GetRequiredService<IdentityUserManager>();
            _roleManager = GetRequiredService<IdentityRoleManager>();
            _userRepository = GetRequiredService<IRepository<AppUser, Guid>>();
        }

        [Fact]
        public async Task Should_Get_Pending_Users()
        {
            // Arrange
            var testUser = new AppUser(Guid.NewGuid(), "test@example.com", "test@example.com")
            {
                Name = "Test User",
                IsAdminApproved = false
            };
            await _userRepository.InsertAsync(testUser);

            // Act
            var result = await _adminUserAppService.GetPendingUsersAsync(
                new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 10
                });

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Should_Approve_User()
        {
            // Arrange
            var testUser = new AppUser(Guid.NewGuid(), "approve@example.com", "approve@example.com")
            {
                Name = "Test User for Approval",
                IsAdminApproved = false,
                IsEmailConfirmed = false,
                IsPhoneConfirmed = false
            };
            await _userRepository.InsertAsync(testUser);

            // Act
            var result = await _adminUserAppService.ApproveAsync(new ApproveUserDto
            {
                UserId = testUser.Id,
                ConfirmEmail = true,
                ConfirmPhone = true
            });

            // Assert
            result.ShouldNotBeNull();
            
            var updatedUser = await _userRepository.GetAsync(testUser.Id);
            updatedUser.IsAdminApproved.ShouldBeTrue();
            updatedUser.IsEmailConfirmed.ShouldBeTrue();
            updatedUser.IsPhoneConfirmed.ShouldBeTrue();
            updatedUser.ApprovedTime.ShouldNotBeNull();
        }
    }
}
