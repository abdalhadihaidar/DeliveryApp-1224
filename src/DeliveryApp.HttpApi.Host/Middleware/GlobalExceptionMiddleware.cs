using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace DeliveryApp.HttpApi.Host.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// Provides centralized error handling and logging
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.TraceIdentifier;
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var clientIp = GetClientIpAddress(context);

            // Log the exception with structured logging
            _logger.LogError(exception, 
                "Unhandled exception occurred. " +
                "CorrelationId: {CorrelationId}, " +
                "Path: {RequestPath}, " +
                "Method: {RequestMethod}, " +
                "ClientIP: {ClientIP}, " +
                "UserAgent: {UserAgent}",
                correlationId, requestPath, requestMethod, clientIp, userAgent);

            // Determine response based on exception type
            var (statusCode, errorResponse) = GetErrorResponse(exception, correlationId);

            // Set response headers
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            // Add security headers
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Write error response
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private (HttpStatusCode statusCode, object errorResponse) GetErrorResponse(Exception exception, string correlationId)
        {
            return exception switch
            {
                // Business logic exceptions
                UserFriendlyException userEx => (HttpStatusCode.BadRequest, new
                {
                    error = "Bad Request",
                    message = userEx.Message,
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "UserFriendlyException"
                }),

                // Validation exceptions
                ArgumentException argEx => (HttpStatusCode.BadRequest, new
                {
                    error = "Bad Request",
                    message = argEx.Message,
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "ValidationException"
                }),

                // Unauthorized access
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, new
                {
                    error = "Unauthorized",
                    message = "You are not authorized to perform this action",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "UnauthorizedException"
                }),

                // Not found
                KeyNotFoundException => (HttpStatusCode.NotFound, new
                {
                    error = "Not Found",
                    message = "The requested resource was not found",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "NotFoundException"
                }),

                // Database exceptions
                InvalidOperationException when exception.Message.Contains("database") => (HttpStatusCode.ServiceUnavailable, new
                {
                    error = "Service Unavailable",
                    message = "Database service is temporarily unavailable",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "DatabaseException"
                }),

                // Timeout exceptions
                TimeoutException => (HttpStatusCode.RequestTimeout, new
                {
                    error = "Request Timeout",
                    message = "The request timed out",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "TimeoutException"
                }),

                // Generic server error
                _ => (HttpStatusCode.InternalServerError, new
                {
                    error = "Internal Server Error",
                    message = "An unexpected error occurred",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    type = "InternalServerException"
                })
            };
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for real IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
