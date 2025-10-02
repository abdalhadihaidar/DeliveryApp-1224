using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    [RemoteService]
    public interface ICustomerAppService : IApplicationService
    {
        // Profile management
        Task<CustomerDto> GetProfileAsync();
        Task<CustomerDto> UpdateProfileAsync(UpdateUserDto input);
        
        // Address management
        Task<List<AddressDto>> GetAddressesAsync();
        Task<AddressDto> AddAddressAsync(AddressDto input);
        Task<AddressDto> UpdateAddressAsync(Guid id, AddressDto input);
        Task<bool> DeleteAddressAsync(Guid id);
        Task<AddressDto> SetDefaultAddressAsync(Guid id);
        
        // Payment method management
        Task<List<PaymentMethodDto>> GetPaymentMethodsAsync();
        Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodDto input);
        Task<bool> DeletePaymentMethodAsync(Guid id);
        Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(Guid id);
        
        // Favorite restaurants
        Task<List<RestaurantDto>> GetFavoriteRestaurantsAsync();
        Task<bool> AddFavoriteRestaurantAsync(Guid restaurantId);
        Task<bool> RemoveFavoriteRestaurantAsync(Guid restaurantId);
        
        // Order history
        Task<List<OrderDto>> GetOrderHistoryAsync(OrderStatus? status = null);
        Task<OrderDto> GetOrderDetailsAsync(Guid orderId);
        Task<OrderDto> PlaceOrderAsync(CreateOrderDto input);
        Task<bool> CancelOrderAsync(Guid orderId);
    }
}
