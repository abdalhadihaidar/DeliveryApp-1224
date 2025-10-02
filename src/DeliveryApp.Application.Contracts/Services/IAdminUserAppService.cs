using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;

using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IAdminUserAppService : IApplicationService
    {
        Task<PagedResultDto<UserDto>> GetPendingUsersAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input);
        Task<UserDto> ApproveAsync(ApproveUserDto input);
        Task<UserDto> RejectAsync(RejectUserDto input);
    }
}
