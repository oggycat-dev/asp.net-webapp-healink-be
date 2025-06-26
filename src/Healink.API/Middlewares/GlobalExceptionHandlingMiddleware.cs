using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.Exceptions;
using Healink.Application.Common.Models;
using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Healink.API.Middlewares;

/// <summary>
/// Middleware for handling exceptions globally
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    
    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception occurred. RequestPath: {RequestPath}, Method: {Method}, User: {User}, " +
                "Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                context.Request.Path, 
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous",
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message, errors) = ClassifyException(exception);
        
        context.Response.StatusCode = (int)statusCode;
        var result = Result.Failure(message, errorCode, errors);
        context.Response.ContentType = "application/json";
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
    
    private (HttpStatusCode statusCode, ErrorCode errorCode, string message, List<string>? errors) ClassifyException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationFailed,
                "Validation failed",
                validationEx.Errors.SelectMany(e => e.Value).ToList()
            ),
            
            ArgumentException => (
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidInput,
                "Invalid request parameters",
                null
            ),
            
            UnauthorizedAccessException unauthorizedEx when IsFileAccess(unauthorizedEx) => (
                HttpStatusCode.Forbidden,
                ErrorCode.StorageError,
                "File access denied",
                null
            ),
            
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ErrorCode.Unauthorized,
                "Authentication required",
                null
            ),
            
            ForbiddenAccessException => (
                HttpStatusCode.Forbidden,
                ErrorCode.Forbidden,
                "Access denied",
                null
            ),
            
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                ErrorCode.NotFound,
                "Resource not found",
                null
            ),
            
            SqlException sqlEx => HandleDatabaseException(sqlEx),
            DbUpdateException dbEx => HandleDatabaseException(dbEx),
            InvalidOperationException invalidEx when IsDatabaseRelated(invalidEx) => HandleDatabaseException(invalidEx),
            
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                ErrorCode.ExternalServiceError,
                "Request timeout - please try again later",
                null
            ),
            
            FileNotFoundException => (
                HttpStatusCode.NotFound,
                ErrorCode.FileNotFound,
                "File not found",
                null
            ),
            
            DirectoryNotFoundException => (
                HttpStatusCode.NotFound,
                ErrorCode.FileNotFound,
                "Directory not found", 
                null
            ),
            
            HttpRequestException => (
                HttpStatusCode.BadGateway,
                ErrorCode.ExternalServiceError,
                "External service unavailable",
                null
            ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCode.InternalError,
                "An internal server error occurred",
                null
            )
        };
    }
    
    private (HttpStatusCode, ErrorCode, string, List<string>?) HandleDatabaseException(Exception exception)
    {
        return exception switch
        {
            SqlException sqlEx => sqlEx.Number switch
            {
                2 or 53 or 121 or 232 or 258 or 1231 or 1232 => (
                    HttpStatusCode.ServiceUnavailable,
                    ErrorCode.DatabaseError,
                    "Database service temporarily unavailable",
                    null
                ),
                
                18456 or 18486 or 18487 or 18488 => (
                    HttpStatusCode.ServiceUnavailable,
                    ErrorCode.DatabaseError,
                    "Database authentication failed",
                    null
                ),
                
                547 => (
                    HttpStatusCode.Conflict,
                    ErrorCode.ResourceConflict,
                    "Operation violates data constraints",
                    null
                ),
                
                2627 or 2601 => (
                    HttpStatusCode.Conflict,
                    ErrorCode.DuplicateEntry,
                    "Duplicate entry found",
                    null
                ),
                
                -2 => (
                    HttpStatusCode.RequestTimeout,
                    ErrorCode.DatabaseError,
                    "Database request timeout",
                    null
                ),
                
                _ => (
                    HttpStatusCode.InternalServerError,
                    ErrorCode.DatabaseError,
                    "Database operation failed",
                    null
                )
            },
            
            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCode.DatabaseError,
                "Database operation failed",
                null
            )
        };
    }
    
    private bool IsDatabaseRelated(Exception exception)
    {
        return exception.Message.Contains("database", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
               exception.Source?.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase) == true ||
               exception.Source?.Contains("SqlClient", StringComparison.OrdinalIgnoreCase) == true;
    }
    
    private bool IsFileAccess(Exception exception)
    {
        return exception.Message.Contains("file", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("path", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("directory", StringComparison.OrdinalIgnoreCase);
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
} 