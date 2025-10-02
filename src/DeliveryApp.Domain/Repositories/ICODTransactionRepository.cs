using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface ICODTransactionRepository : IRepository<CODTransaction, Guid>
    {
        Task<List<CODTransaction>> GetTransactionsByDeliveryPersonAsync(Guid deliveryPersonId, int skipCount, int maxResultCount, string sorting);
        
        Task<int> GetTransactionsCountByDeliveryPersonAsync(Guid deliveryPersonId);
        
        Task<List<CODTransaction>> GetTransactionsByOrderAsync(Guid orderId);
        
        Task<List<CODTransaction>> GetPendingTransactionsAsync();
        
        Task<CODTransaction?> GetActiveTransactionForOrderAsync(Guid orderId);
        
        Task<List<CODTransaction>> GetTransactionsByStatusAsync(CODTransactionStatus status, int skipCount, int maxResultCount, string sorting);
        
        Task<int> GetTransactionsCountByStatusAsync(CODTransactionStatus status);
    }
}
