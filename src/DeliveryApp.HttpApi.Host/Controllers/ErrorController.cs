using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DeliveryApp.HttpApi.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Error()
    {
        var correlationId = HttpContext.TraceIdentifier;
        var requestPath = HttpContext.Request.Path;
        var requestMethod = HttpContext.Request.Method;
        var clientIp = GetClientIpAddress();

        _logger.LogError(
            "Unhandled error occurred. " +
            "CorrelationId: {CorrelationId}, " +
            "Path: {RequestPath}, " +
            "Method: {RequestMethod}, " +
            "ClientIP: {ClientIP}",
            correlationId, requestPath, requestMethod, clientIp);

        return StatusCode(500, new { 
            error = "Internal Server Error",
            message = "An internal error occurred",
            correlationId = correlationId,
            timestamp = DateTime.UtcNow,
            path = requestPath,
            method = requestMethod
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.3.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new { 
            application = "DeliveryApp API",
            version = "1.3.0",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            machine = Environment.MachineName,
            os = Environment.OSVersion.ToString()
        });
    }

    private string GetClientIpAddress()
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
