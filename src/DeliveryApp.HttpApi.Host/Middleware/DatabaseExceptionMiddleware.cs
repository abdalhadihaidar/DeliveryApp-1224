using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DeliveryApp.HttpApi.Host.Middleware;

public class DatabaseExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseExceptionMiddleware> _logger;

    public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
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
        catch (Exception ex) when (IsDatabaseException(ex))
        {
            _logger.LogWarning(ex, "Database exception occurred: {Message}", ex.Message);
            
            // Handle specific database errors gracefully
            if (IsMissingTableError(ex))
            {
                _logger.LogInformation("Missing table error detected - this is expected during initial setup");
                // Don't throw the exception, let the application continue
                return;
            }

            // For other database errors, return a proper error response
            await HandleDatabaseExceptionAsync(context, ex);
        }
    }

    private static bool IsDatabaseException(Exception ex)
    {
        return ex is SqlException || 
               ex is DbUpdateException || 
               ex is InvalidOperationException ||
               (ex.InnerException is SqlException);
    }

    private static bool IsMissingTableError(Exception ex)
    {
        var sqlException = ex as SqlException ?? ex.InnerException as SqlException;
        return sqlException?.Number == 208; // Invalid object name
    }

    private static async Task HandleDatabaseExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Database error occurred",
            message = "A database error occurred while processing your request",
            details = ex.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
