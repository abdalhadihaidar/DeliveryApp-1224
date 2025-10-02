using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Linq;
using System;

namespace DeliveryApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowDashboard")]
    public class TestController : ControllerBase
    {
        [HttpGet("cors-test")]
        public IActionResult CorsTest()
        {
            return Ok(new { 
                message = "CORS test successful", 
                timestamp = DateTime.UtcNow,
                origin = Request.Headers["Origin"].ToString(),
                method = Request.Method,
                path = Request.Path,
                query = Request.QueryString.ToString()
            });
        }
        
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }
        
        [HttpOptions("cors-test")]
        public IActionResult CorsTestOptions()
        {
            return Ok();
        }
        
        [HttpOptions("health")]
        public IActionResult HealthOptions()
        {
            return Ok();
        }
    }
}
