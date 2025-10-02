using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Middleware
{
    public class SecurityMiddleware : IMiddleware, ITransientDependency
    {
        private readonly ILogger<SecurityMiddleware> _logger;

        public SecurityMiddleware(ILogger<SecurityMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Add security headers
            AddSecurityHeaders(context);

            // Log request for security monitoring
            await LogRequest(context);

            // Validate request size
            if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit
            {
                context.Response.StatusCode = 413; // Payload Too Large
                await context.Response.WriteAsync("Request too large");
                return;
            }

            // Rate limiting check (basic implementation)
            if (await IsRateLimited(context))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            // Validate request content
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                if (!await ValidateRequestContent(context))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    await context.Response.WriteAsync("Invalid request content");
                    return;
                }
            }

            await next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;
            
            // Prevent clickjacking
            response.Headers.Add("X-Frame-Options", "DENY");
            
            // Prevent MIME type sniffing
            response.Headers.Add("X-Content-Type-Options", "nosniff");
            
            // Enable XSS protection
            response.Headers.Add("X-XSS-Protection", "1; mode=block");
            
            // Strict transport security
            response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // Content security policy
            response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");
            
            // Referrer policy
            response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Remove server header
            response.Headers.Remove("Server");
        }

        private async Task LogRequest(HttpContext context)
        {
            var request = context.Request;
            var logData = new
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                IPAddress = GetClientIPAddress(context),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Request: {LogData}", JsonSerializer.Serialize(logData));
        }

        private string GetClientIPAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIP = context.Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrEmpty(realIP))
            {
                return realIP;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private async Task<bool> IsRateLimited(HttpContext context)
        {
            // Basic rate limiting implementation
            // In production, use Redis or a proper rate limiting service
            var clientIP = GetClientIPAddress(context);
            
            // For now, just log potential rate limiting
            _logger.LogDebug("Rate limit check for IP: {ClientIP}", clientIP);
            
            // TODO: Implement actual rate limiting logic
            return false;
        }

        private async Task<bool> ValidateRequestContent(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                // Check for suspicious content
                if (ContainsSuspiciousContent(body))
                {
                    _logger.LogWarning("Suspicious content detected in request from {IP}: {Content}", 
                        GetClientIPAddress(context), body);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating request content");
                return false;
            }
        }

        private bool ContainsSuspiciousContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return false;

            var suspiciousPatterns = new[]
            {
                "<script",
                "javascript:",
                "vbscript:",
                "onload=",
                "onerror=",
                "onclick=",
                "eval(",
                "document.cookie",
                "document.write",
                "window.location",
                "union select",
                "drop table",
                "insert into",
                "delete from",
                "update set",
                "../",
                "..\\",
                "cmd.exe",
                "powershell"
            };

            var lowerContent = content.ToLower();
            foreach (var pattern in suspiciousPatterns)
            {
                if (lowerContent.Contains(pattern.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

