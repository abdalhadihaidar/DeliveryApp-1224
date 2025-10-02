using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [RemoteService]
    [Route("api/customer")]
    [Authorize(Roles = "customer")]
    public class CustomerController : AbpController
    {
        private readonly ICustomerAppService _customerAppService;

        public CustomerController(ICustomerAppService customerAppService)
        {
            _customerAppService = customerAppService;
        }

        [HttpGet("profile")]
        public async Task<CustomerDto> GetProfile()
        {
            return await _customerAppService.GetProfileAsync();
        }

        [HttpPut("profile")]
        public async Task<CustomerDto> UpdateProfile([FromBody] UpdateUserDto input)
        {
            return await _customerAppService.UpdateProfileAsync(input);
        }

        [HttpGet("addresses")]
        public async Task<List<AddressDto>> GetAddresses()
        {
            return await _customerAppService.GetAddressesAsync();
        }

        [HttpPost("addresses")]
        public async Task<AddressDto> AddAddress([FromBody] AddressDto input)
        {
            return await _customerAppService.AddAddressAsync(input);
        }

        [HttpPut("addresses/{id}")]
        public async Task<AddressDto> UpdateAddress(Guid id, [FromBody] AddressDto input)
        {
            return await _customerAppService.UpdateAddressAsync(id, input);
        }

        [HttpDelete("addresses/{id}")]
        public async Task<bool> DeleteAddress(Guid id)
        {
            return await _customerAppService.DeleteAddressAsync(id);
        }

        [HttpPut("addresses/{id}/default")]
        public async Task<AddressDto> SetDefaultAddress(Guid id)
        {
            return await _customerAppService.SetDefaultAddressAsync(id);
        }

        [HttpGet("payment-methods")]
        public async Task<List<PaymentMethodDto>> GetPaymentMethods()
        {
            return await _customerAppService.GetPaymentMethodsAsync();
        }

        [HttpPost("payment-methods")]
        public async Task<PaymentMethodDto> AddPaymentMethod([FromBody] PaymentMethodDto input)
        {
            return await _customerAppService.AddPaymentMethodAsync(input);
        }

        [HttpDelete("payment-methods/{id}")]
        public async Task<bool> DeletePaymentMethod(Guid id)
        {
            return await _customerAppService.DeletePaymentMethodAsync(id);
        }

        [HttpPut("payment-methods/{id}/default")]
        public async Task<PaymentMethodDto> SetDefaultPaymentMethod(Guid id)
        {
            return await _customerAppService.SetDefaultPaymentMethodAsync(id);
        }

        [HttpGet("favorite-restaurants")]
        public async Task<List<RestaurantDto>> GetFavoriteRestaurants()
        {
            return await _customerAppService.GetFavoriteRestaurantsAsync();
        }

        [HttpPost("favorite-restaurants/{restaurantId}")]
        public async Task<bool> AddFavoriteRestaurant(Guid restaurantId)
        {
            return await _customerAppService.AddFavoriteRestaurantAsync(restaurantId);
        }

        [HttpDelete("favorite-restaurants/{restaurantId}")]
        public async Task<bool> RemoveFavoriteRestaurant(Guid restaurantId)
        {
            return await _customerAppService.RemoveFavoriteRestaurantAsync(restaurantId);
        }

        [HttpGet("orders")]
        public async Task<List<OrderDto>> GetOrderHistory([FromQuery] string status = null)
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }
            return await _customerAppService.GetOrderHistoryAsync(orderStatus);
        }

        [HttpGet("orders/{orderId}")]
        public async Task<OrderDto> GetOrderDetails(Guid orderId)
        {
            return await _customerAppService.GetOrderDetailsAsync(orderId);
        }

        [HttpPost("orders")]
        public async Task<OrderDto> PlaceOrder([FromBody] CreateOrderDto input)
        {
            return await _customerAppService.PlaceOrderAsync(input);
        }

        [HttpPost("orders/{orderId}/cancel")]
        public async Task<bool> CancelOrder(Guid orderId)
        {
            return await _customerAppService.CancelOrderAsync(orderId);
        }
    }
}
