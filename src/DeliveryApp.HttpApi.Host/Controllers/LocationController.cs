using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Volo.Abp.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [Route("api/app/location")]
    [ApiController]
    [AllowAnonymous]
    public class LocationController : AbpController
    {
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public LocationController(HttpClient httpClient, IStringLocalizer<DeliveryAppResource> localizer)
        {
            _httpClient = httpClient;
            _localizer = localizer;
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> GeocodeAddress([FromQuery] string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                {
                    return BadRequest("Address is required");
                }

                // Use OpenStreetMap Nominatim API for geocoding
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1";

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonSerializer.Deserialize<List<JsonElement>>(response);

                if (results != null && results.Count > 0)
                {
                    var firstResult = results[0];
                    var lat = firstResult.GetProperty("lat").GetDouble();
                    var lon = firstResult.GetProperty("lon").GetDouble();

                    return Ok(new
                    {
                        latitude = lat,
                        longitude = lon,
                        address = address
                    });
                }

                return NotFound("Address not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = _localizer["Payment:GeocodingFailed"], message = ex.Message });
            }
        }

        [HttpGet("restaurant/{restaurantId}/location")]
        public async Task<IActionResult> GetRestaurantLocation(Guid restaurantId)
        {
            try
            {
                // This would typically fetch from your database
                // For now, return a mock location
                var mockLocations = new Dictionary<string, object>
                {
                    { "restaurant1", new { latitude = 24.7136, longitude = 46.6753 } }, // Riyadh
                    { "restaurant2", new { latitude = 21.4858, longitude = 39.1925 } }, // Jeddah
                    { "restaurant3", new { latitude = 26.4207, longitude = 50.0888 } }, // Dammam
                };

                var restaurantKey = restaurantId.ToString();
                if (mockLocations.ContainsKey(restaurantKey))
                {
                    return Ok(mockLocations[restaurantKey]);
                }

                // Default location (Riyadh)
                return Ok(new
                {
                    latitude = 24.7136,
                    longitude = 46.6753
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get restaurant location", message = ex.Message });
            }
        }

        [HttpGet("distance")]
        public IActionResult CalculateDistance(
            [FromQuery] double lat1, 
            [FromQuery] double lon1, 
            [FromQuery] double lat2, 
            [FromQuery] double lon2)
        {
            try
            {
                var distance = CalculateDistanceInMeters(lat1, lon1, lat2, lon2);
                return Ok(new { distance = distance });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to calculate distance", message = ex.Message });
            }
        }

        private double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371000; // Earth's radius in meters

            var lat1Rad = lat1 * Math.PI / 180;
            var lat2Rad = lat2 * Math.PI / 180;
            var deltaLat = (lat2 - lat1) * Math.PI / 180;
            var deltaLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }
    }
} 
