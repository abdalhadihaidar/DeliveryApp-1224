using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    [Authorize(Roles = "customer")]
    public class CustomerAppService : ApplicationService, ICustomerAppService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Address, Guid> _addressRepository;
        private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;
        private readonly IRepository<FavoriteRestaurant, Guid> _favoriteRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;
        private readonly IOrderStatusNotificationService _orderStatusNotifier;

        public CustomerAppService(
            IUserRepository userRepository,
            IRepository<Address, Guid> addressRepository,
            IRepository<PaymentMethod, Guid> paymentMethodRepository,
            IRepository<FavoriteRestaurant, Guid> favoriteRepository,
            IOrderRepository orderRepository,
            IRestaurantRepository restaurantRepository,
            IdentityUserManager userManager,
            ICurrentUser currentUser,
            IOrderStatusNotificationService orderStatusNotifier)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _favoriteRepository = favoriteRepository;
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _userManager = userManager;
            _currentUser = currentUser;
            _orderStatusNotifier = orderStatusNotifier;
        }

        private Guid GetCurrentUserId()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            
            var userId = _currentUser.GetId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in authentication token");
            }
            
            return userId;
        }

        public async Task<CustomerDto> GetProfileAsync()
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            
            var customerDto = ObjectMapper.Map<AppUser, CustomerDto>(user);
            
            // Map favorite restaurant IDs
            customerDto.FavoriteRestaurantIds = user.FavoriteRestaurants
                .Select(fr => fr.RestaurantId)
                .ToList();
                
            return customerDto;
        }

        public async Task<CustomerDto> UpdateProfileAsync(UpdateUserDto input)
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            
            user.Name = input.Name;
            user.SetPhoneNumber(input.PhoneNumber, false);
            
            if (!string.IsNullOrEmpty(input.ProfileImageUrl))
            {
                // Check if it's a base64 data URL (which we don't want to store directly)
                if (input.ProfileImageUrl.StartsWith("data:image/"))
                {
                    throw new Exception("Base64 image data is not supported. Please upload the image file first and provide the URL.");
                }
                
                // Only store if it's a valid URL (starts with http/https or is a relative path)
                if (input.ProfileImageUrl.StartsWith("http://") || 
                    input.ProfileImageUrl.StartsWith("https://") || 
                    input.ProfileImageUrl.StartsWith("/"))
                {
                    user.ProfileImageUrl = input.ProfileImageUrl;
                }
                else
                {
                    throw new Exception("Invalid image URL format. Please provide a valid URL.");
                }
            }
            else
            {
                user.ProfileImageUrl = string.Empty;
            }
            
            await _userRepository.UpdateAsync(user);
            
            return await GetProfileAsync();
        }

        public async Task<List<AddressDto>> GetAddressesAsync()
        {
            var userId = GetCurrentUserId();
            var addresses = await _addressRepository.GetListAsync(a => a.UserId == userId);
            
            return ObjectMapper.Map<List<Address>, List<AddressDto>>(addresses);
        }

        public async Task<AddressDto> AddAddressAsync(AddressDto input)
        {
            var userId = GetCurrentUserId();
            // Create new address with generated ID
            var addressId = GuidGenerator.Create();
            var address = new Address(addressId)
            {
                Title = input.Title,
                Street = input.Street,
                City = input.City,
                State = input.State,
                ZipCode = input.ZipCode,
                FullAddress = input.FullAddress,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                IsDefault = input.IsDefault,
                UserId = userId
            };
            
            // If this is the first address or marked as default, set as default
            var existingAddresses = await _addressRepository.CountAsync(a => a.UserId == userId);
            if (existingAddresses == 0 || input.IsDefault)
            {
                address.IsDefault = true;
                
                // If setting this as default, unset others
                if (existingAddresses > 0)
                {
                    var defaultAddresses = await _addressRepository.GetListAsync(
                        a => a.UserId == userId && a.IsDefault);
                    
                    foreach (var defaultAddress in defaultAddresses)
                    {
                        defaultAddress.IsDefault = false;
                        await _addressRepository.UpdateAsync(defaultAddress);
                    }
                }
            }
            
            await _addressRepository.InsertAsync(address);
            
            return ObjectMapper.Map<Address, AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid id, AddressDto input)
        {
            var userId = GetCurrentUserId();
            var address = await _addressRepository.GetAsync(a => a.Id == id && a.UserId == userId);
            
            ObjectMapper.Map(input, address);
            
            // If setting this as default, unset others
            if (input.IsDefault && !address.IsDefault)
            {
                var defaultAddresses = await _addressRepository.GetListAsync(
                    a => a.UserId == userId && a.IsDefault && a.Id != id);
                
                foreach (var defaultAddress in defaultAddresses)
                {
                    defaultAddress.IsDefault = false;
                    await _addressRepository.UpdateAsync(defaultAddress);
                }
                
                address.IsDefault = true;
            }
            
            await _addressRepository.UpdateAsync(address);
            
            return ObjectMapper.Map<Address, AddressDto>(address);
        }

        public async Task<bool> DeleteAddressAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var address = await _addressRepository.GetAsync(a => a.Id == id && a.UserId == userId);
            
            await _addressRepository.DeleteAsync(address);
            
            // If the deleted address was the default, set another as default
            if (address.IsDefault)
            {
                var remainingAddress = await _addressRepository.FirstOrDefaultAsync(a => a.UserId == userId);
                if (remainingAddress != null)
                {
                    remainingAddress.IsDefault = true;
                    await _addressRepository.UpdateAsync(remainingAddress);
                }
            }
            
            return true;
        }

        public async Task<AddressDto> SetDefaultAddressAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            
            // Unset current default
            var defaultAddresses = await _addressRepository.GetListAsync(
                a => a.UserId == userId && a.IsDefault);
            
            foreach (var defaultAddress in defaultAddresses)
            {
                defaultAddress.IsDefault = false;
                await _addressRepository.UpdateAsync(defaultAddress);
            }
            
            // Set new default
            var address = await _addressRepository.GetAsync(a => a.Id == id && a.UserId == userId);
            address.IsDefault = true;
            await _addressRepository.UpdateAsync(address);
            
            return ObjectMapper.Map<Address, AddressDto>(address);
        }

        public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync()
        {
            var userId = GetCurrentUserId();
            var paymentMethods = await _paymentMethodRepository.GetListAsync(p => p.UserId == userId);
            
            return ObjectMapper.Map<List<PaymentMethod>, List<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodDto input)
        {
            var userId = GetCurrentUserId();
            var paymentMethodId = GuidGenerator.Create();
            var paymentMethod = new PaymentMethod(paymentMethodId);
            ObjectMapper.Map(input, paymentMethod);
            paymentMethod.UserId = userId;
            
            // If this is the first payment method or marked as default, set as default
            var existingPaymentMethods = await _paymentMethodRepository.CountAsync(p => p.UserId == userId);
            if (existingPaymentMethods == 0 || input.IsDefault)
            {
                paymentMethod.IsDefault = true;
                
                // If setting this as default, unset others
                if (existingPaymentMethods > 0)
                {
                    var defaultPaymentMethods = await _paymentMethodRepository.GetListAsync(
                        p => p.UserId == userId && p.IsDefault);
                    
                    foreach (var defaultPaymentMethod in defaultPaymentMethods)
                    {
                        defaultPaymentMethod.IsDefault = false;
                        await _paymentMethodRepository.UpdateAsync(defaultPaymentMethod);
                    }
                }
            }
            
            await _paymentMethodRepository.InsertAsync(paymentMethod);
            
            return ObjectMapper.Map<PaymentMethod, PaymentMethodDto>(paymentMethod);
        }

        public async Task<bool> DeletePaymentMethodAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var paymentMethod = await _paymentMethodRepository.GetAsync(p => p.Id == id && p.UserId == userId);
            
            await _paymentMethodRepository.DeleteAsync(paymentMethod);
            
            // If the deleted payment method was the default, set another as default
            if (paymentMethod.IsDefault)
            {
                var remainingPaymentMethod = await _paymentMethodRepository.FirstOrDefaultAsync(p => p.UserId == userId);
                if (remainingPaymentMethod != null)
                {
                    remainingPaymentMethod.IsDefault = true;
                    await _paymentMethodRepository.UpdateAsync(remainingPaymentMethod);
                }
            }
            
            return true;
        }

        public async Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            
            // Unset current default
            var defaultPaymentMethods = await _paymentMethodRepository.GetListAsync(
                p => p.UserId == userId && p.IsDefault);
            
            foreach (var defaultPaymentMethod in defaultPaymentMethods)
            {
                defaultPaymentMethod.IsDefault = false;
                await _paymentMethodRepository.UpdateAsync(defaultPaymentMethod);
            }
            
            // Set new default
            var paymentMethod = await _paymentMethodRepository.GetAsync(p => p.Id == id && p.UserId == userId);
            paymentMethod.IsDefault = true;
            await _paymentMethodRepository.UpdateAsync(paymentMethod);
            
            return ObjectMapper.Map<PaymentMethod, PaymentMethodDto>(paymentMethod);
        }

        public async Task<List<RestaurantDto>> GetFavoriteRestaurantsAsync()
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteRepository.GetListAsync(f => f.UserId == userId);
            
            var restaurantIds = favorites.Select(f => f.RestaurantId).ToList();
            var restaurants = await _restaurantRepository.GetRestaurantsByIdsAsync(restaurantIds);
            
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        public async Task<bool> AddFavoriteRestaurantAsync(Guid restaurantId)
        {
            var userId = GetCurrentUserId();
            
            // Check if already a favorite
            var existingFavorite = await _favoriteRepository.FirstOrDefaultAsync(
                f => f.UserId == userId && f.RestaurantId == restaurantId);
            
            if (existingFavorite != null)
            {
                return true; // Already a favorite
            }
            
            // Add to favorites
            var favoriteId = GuidGenerator.Create();
            var favorite = new FavoriteRestaurant(favoriteId)
            {
                UserId = userId,
                RestaurantId = restaurantId
            };
            
            await _favoriteRepository.InsertAsync(favorite);
            
            return true;
        }

        public async Task<bool> RemoveFavoriteRestaurantAsync(Guid restaurantId)
        {
            var userId = GetCurrentUserId();
            
            var favorite = await _favoriteRepository.FirstOrDefaultAsync(
                f => f.UserId == userId && f.RestaurantId == restaurantId);
            
            if (favorite != null)
            {
                await _favoriteRepository.DeleteAsync(favorite);
            }
            
            return true;
        }

        public async Task<List<OrderDto>> GetOrderHistoryAsync(OrderStatus? status = null)
        {
            var userId = GetCurrentUserId();
            var orders = await _orderRepository.GetUserOrdersAsync(userId, status);
            
            return ObjectMapper.Map<List<Order>, List<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetOrderDetailsAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order belongs to the current user
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this order.");
            }
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<OrderDto> PlaceOrderAsync(CreateOrderDto input)
        {
            var userId = GetCurrentUserId();
            
            // Validate restaurant exists
            var restaurant = await _restaurantRepository.GetAsync(input.RestaurantId);
            
            // Create new order
            var orderId = GuidGenerator.Create();
            var order = new Order(orderId)
            {
                RestaurantId = input.RestaurantId,
                UserId = userId,
                OrderDate = DateTime.Now,
                Subtotal = input.Subtotal,
                DeliveryFee = input.DeliveryFee,
                Tax = input.Tax,
                TotalAmount = input.TotalAmount,
                Status = OrderStatus.Pending, // Pending
                DeliveryAddressId = input.DeliveryAddressId,
                PaymentMethodId = input.PaymentMethodId
            };
            
            // Add order items
            foreach (var itemDto in input.Items)
            {
                var orderItemId = GuidGenerator.Create();
                var orderItem = new OrderItem(orderItemId)
                {
                    OrderId = order.Id,
                    MenuItemId = itemDto.MenuItemId,
                    Name = itemDto.Name,
                    Quantity = (int)itemDto.Quantity,
                    Price = itemDto.Price,
                    SpecialInstructions = itemDto.SpecialInstructions
                };
                
                // Add selected options
                if (itemDto.SelectedOptions != null)
                {
                    foreach (var option in itemDto.SelectedOptions)
                    {
                        orderItem.SelectedOptions.Add(option);
                    }
                }
                
                order.Items.Add(orderItem);
            }
            
            await _orderRepository.InsertAsync(order);
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetAsync(orderId);
            
            // Verify the order belongs to the current user
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to cancel this order.");
            }
            
            // Check if order can be cancelled based on current status
            if (order.Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("Cannot cancel an order that has already been delivered.");
            }
            
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("This order has already been cancelled.");
            }
            
            if (order.Status == OrderStatus.Delivering)
            {
                throw new InvalidOperationException("Cannot cancel an order that is currently being delivered. Please contact customer support.");
            }
            
            if (order.Status == OrderStatus.ReadyForDelivery || order.Status == OrderStatus.WaitingCourier)
            {
                throw new InvalidOperationException("Cannot cancel an order that is ready for delivery or waiting for courier assignment. Please contact the restaurant directly.");
            }
            
            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            
            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.Cancelled, userId);
            
            return true;
        }
    }
}
