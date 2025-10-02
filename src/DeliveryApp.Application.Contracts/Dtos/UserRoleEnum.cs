using System.Text.Json.Serialization;

namespace DeliveryApp.Application.Contracts.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Customer = 0,
        Delivery = 1,
        RestaurantOwner = 2
    }
} 
