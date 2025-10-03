using System;
using System.Threading.Tasks;
using DeliveryApp.Controllers;
using DeliveryApp.Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryApp.HttpApi.Host.Controllers.Admin
{
    [Route("api/admin/data-seeder")]
    [Authorize(Roles = "admin")]
    public class DataSeederController : DeliveryAppController
    {
        private readonly IServiceProvider _serviceProvider;

        public DataSeederController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Seed advertisement data
        /// </summary>
        [HttpPost("seed-advertisements")]
        public async Task<ActionResult> SeedAdvertisements()
        {
            try
            {
                var seeder = _serviceProvider.GetRequiredService<AdvertisementDataSeeder>();
                await seeder.SeedAsync(new Volo.Abp.Data.DataSeedContext());
                
                return Ok(new { message = "Advertisements seeded successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to seed advertisements: {ex.Message}" });
            }
        }
    }
}
