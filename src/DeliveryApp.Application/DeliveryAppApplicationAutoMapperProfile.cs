using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application
{
    public class DeliveryAppApplicationAutoMapperProfile : Profile
    {
        public DeliveryAppApplicationAutoMapperProfile()
        {
            // User mapping
            CreateMap<AppUser, UserDto>();
            CreateMap<AppUser, RestaurantOwnerDto>();
            CreateMap<AppUser, RestaurantOwnerDetailsDto>()
                .ForMember(dest => dest.RestaurantCount, opt => opt.Ignore())
                .ForMember(dest => dest.Restaurants, opt => opt.Ignore());
            CreateMap<AppUser, CustomerDto>();
            CreateMap<AppUser, DeliveryPersonDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<PaymentMethod, PaymentMethodDto>();
            
            // Restaurant mapping
            CreateMap<Restaurant, RestaurantDto>()
                .ForMember(dest => dest.Menu, opt => opt.MapFrom(src => src.Menu))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Name : string.Empty))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))
                .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetRestaurantStatus(src)))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => GetRejectionReason(src)));
            CreateMap<Restaurant, RestaurantSummaryDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetRestaurantStatus(src)));
            
            // Restaurant Category mapping
            CreateMap<RestaurantCategory, RestaurantCategoryDto>()
                .ForMember(dest => dest.RestaurantCount, opt => opt.Ignore()); // This will be set manually in the service
            CreateMap<RestaurantCategory, RestaurantCategoryListDto>()
                .ForMember(dest => dest.RestaurantCount, opt => opt.Ignore()); // This will be set manually in the service
            CreateMap<MenuItem, MenuItemDto>()
                .ForMember(dest => dest.MealCategoryName, opt => opt.MapFrom(src => src.MealCategory != null ? src.MealCategory.Name : null));
            
            // Meal Category mapping
            CreateMap<MealCategory, MealCategoryDto>();
            
            // Ad Request mapping
            CreateMap<AdRequest, AdRequestDto>()
                .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : string.Empty))
                .ForMember(dest => dest.ReviewedByName, opt => opt.MapFrom(src => src.ReviewedBy != null ? src.ReviewedBy.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            
            // Order mapping
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
            
            // Review mapping
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl));
            
            // Special Offer mapping - Enhanced with new properties
            CreateMap<SpecialOffer, SpecialOfferDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.IsValidNow, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.IsExpired, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.IsUpcoming, opt => opt.Ignore()) // Computed property
                .ForMember(dest => dest.RemainingTime, opt => opt.Ignore()); // Computed property
            
            CreateMap<CreateSpecialOfferDto, SpecialOffer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifierId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeleterId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionTime, opt => opt.Ignore())
                .ForMember(dest => dest.NextOccurrence, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentOccurrences, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUses, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsed, opt => opt.Ignore());
            
            CreateMap<UpdateSpecialOfferDto, SpecialOffer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifierId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeleterId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionTime, opt => opt.Ignore())
                .ForMember(dest => dest.NextOccurrence, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentOccurrences, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUses, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsed, opt => opt.Ignore());
            
            // Advertisement mapping
            CreateMap<Advertisement, AdvertisementDto>();
            CreateMap<CreateAdvertisementDto, Advertisement>();
            CreateMap<UpdateAdvertisementDto, Advertisement>();
            CreateMap<Advertisement, AdvertisementSummaryDto>();

            // Delivery Assignment Mappings
            CreateMap<AppUser, AvailableDeliveryPersonDto>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.CurrentLocation != null ? src.CurrentLocation.Latitude : 0.0))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.CurrentLocation != null ? src.CurrentLocation.Longitude : 0.0))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.DeliveryStatus != null ? src.DeliveryStatus.IsAvailable : false))
                .ForMember(dest => dest.LastLocationUpdate, opt => opt.MapFrom(src => src.CurrentLocation != null ? src.CurrentLocation.LastModificationTime ?? DateTime.UtcNow : DateTime.UtcNow))
                .ForMember(dest => dest.DistanceKm, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.CurrentActiveOrders, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.Rating, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.CompletedDeliveries, opt => opt.Ignore()); // Will be calculated in service

            // Restaurant Report Mappings
            CreateMap<MenuItem, PopularMenuItemDto>()
                .ForMember(dest => dest.MenuItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TotalOrders, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.TotalQuantitySold, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore()) // Will be calculated in service
                .ForMember(dest => dest.PopularityScore, opt => opt.Ignore()); // Will be calculated in service

            // Chat Mappings
            CreateMap<DeliveryApp.Domain.Entities.ChatSession, ChatSessionDto>();
            CreateMap<DeliveryApp.Domain.Entities.ChatMessage, ChatMessageDto>();
        }

        private static string GetRestaurantStatus(Restaurant restaurant)
        {
            if (restaurant.Tags.Contains("في انتظار الموافقة"))
                return "pending";
            if (restaurant.Tags.Contains("موافق عليه"))
                return "approved";
            if (restaurant.Tags.Contains("مرفوض"))
                return "rejected";
            return "approved"; // Default status
        }

        private static string GetRejectionReason(Restaurant restaurant)
        {
            var rejectionTag = restaurant.Tags.FirstOrDefault(t => t.StartsWith("سبب الرفض:"));
            return rejectionTag?.Replace("سبب الرفض: ", "") ?? string.Empty;
        }
    }
}
