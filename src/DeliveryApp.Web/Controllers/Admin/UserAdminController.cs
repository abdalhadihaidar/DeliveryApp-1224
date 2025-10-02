using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers.Admin
{
    [RemoteService]
    [Route("api/app/admin/users")]
    [Authorize(Roles = "admin")]
    public class UserAdminController : AbpController
    {
        private readonly AdminUserAppService _userService;

        public UserAdminController(AdminUserAppService userService)
        {
            _userService = userService;
        }

        [HttpGet("pending")]
        public Task<Application.Contracts.Dtos.PagedResultDto<UserDto>> GetPendingAsync([FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 20)
        {
            return _userService.GetPendingUsersAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto { SkipCount = skipCount, MaxResultCount = maxResultCount });
        }

        [HttpPost("approve")]
        public Task<UserDto> ApproveAsync([FromBody] ApproveUserDto input)
        {
            return _userService.ApproveAsync(input);
        }

        [HttpPost("reject")]
        public Task<UserDto> RejectAsync([FromBody] RejectUserDto input)
        {
            return _userService.RejectAsync(input);
        }
    }
}

