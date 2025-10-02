using System;
using System.Threading.Tasks;
using DeliveryApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace DeliveryApp.Web.Controllers
{
    [Route("api/role-test")]
    public class RoleTestController : DeliveryAppController
    {
        private readonly IAuthorizationService _authorizationService;

        public RoleTestController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet("restaurants")]
        [Authorize(Roles = "admin,manager,restaurant_owner,customer")]
        public async Task<IActionResult> TestRestaurantViewPermission()
        {
            return Ok(new { message = "You have permission to view restaurants", timestamp = DateTime.UtcNow });
        }

        [HttpPost("restaurants")]
        [Authorize(Roles = "admin,manager,restaurant_owner")]
        public async Task<IActionResult> TestRestaurantCreatePermission()
        {
            return Ok(new { message = "You have permission to create restaurants", timestamp = DateTime.UtcNow });
        }

        [HttpGet("orders")]
        [Authorize(Roles = "admin,manager,restaurant_owner,delivery,customer")]
        public async Task<IActionResult> TestOrderViewPermission()
        {
            return Ok(new { message = "You have permission to view orders", timestamp = DateTime.UtcNow });
        }

        [HttpPost("orders")]
        [Authorize(Roles = "admin,manager,customer")]
        public async Task<IActionResult> TestOrderCreatePermission()
        {
            return Ok(new { message = "You have permission to create orders", timestamp = DateTime.UtcNow });
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestAdminOnly()
        {
            return Ok(new { message = "You are an admin", timestamp = DateTime.UtcNow });
        }

        [HttpGet("restaurant-owner-only")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<IActionResult> TestRestaurantOwnerOnly()
        {
            return Ok(new { message = "You are a restaurant owner", timestamp = DateTime.UtcNow });
        }

        [HttpGet("customer-only")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> TestCustomerOnly()
        {
            return Ok(new { message = "You are a customer", timestamp = DateTime.UtcNow });
        }

        [HttpGet("delivery-only")]
        [Authorize(Roles = "delivery")]
        public async Task<IActionResult> TestDeliveryOnly()
        {
            return Ok(new { message = "You are a delivery person", timestamp = DateTime.UtcNow });
        }

        [HttpGet("manager-only")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> TestManagerOnly()
        {
            return Ok(new { message = "You are a manager", timestamp = DateTime.UtcNow });
        }

        [HttpGet("assignable-roles")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> TestAssignableRoles()
        {
            return Ok(new { 
                message = "You have permission to view assignable roles", 
                timestamp = DateTime.UtcNow,
                roles = new[] { "admin", "manager", "restaurant_owner", "delivery", "customer" }
            });
        }
    }
}

