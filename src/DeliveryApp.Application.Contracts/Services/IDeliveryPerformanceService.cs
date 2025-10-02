using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IDeliveryPerformanceService
    {
        Task<List<DeliveryPersonPerformanceDto>> GetPerformanceAsync(DateTime fromDate, DateTime toDate);
    }
}
