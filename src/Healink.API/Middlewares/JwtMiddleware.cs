using Microsoft.AspNetCore.Http;
using Healink.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Text.Json;
using Healink.Application.Common.Models;

namespace Healink.API.Middlewares;

/// <summary>
/// Middleware for validating JWT tokens in requests
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    
    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    /// <summary>
    /// Process the request to validate JWT token
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        bool requiresAuthorization = false;
        
        if (endpoint != null)
        {
            requiresAuthorization = endpoint.Metadata.GetMetadata<AuthorizeAttribute>() != null && 
                                  endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() == null;
        }
        
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (token != null)
        {
            var userAttached = AttachUserToContext(context, token);
            
            if (!userAttached && requiresAuthorization)
            {
                await SendUnauthorizedResponse(context, "Invalid or expired token", ErrorCode.InvalidToken);
                return;
            }
        }
        else if (requiresAuthorization)
        {
            await SendUnauthorizedResponse(context, "Authorization token is missing", ErrorCode.Unauthorized);
            return;
        }
            
        await _next(context);
    }
    
    /// <summary>
    /// Attach user information to the HttpContext if token is valid
    /// </summary>
    private bool AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();
            
            var userId = claims.FirstOrDefault(x => 
                x.Type == ClaimTypes.NameIdentifier || 
                x.Type == "sub" || 
                x.Type == "nameid")?.Value;
                
            if (!string.IsNullOrEmpty(userId))
            {
                var identity = new ClaimsIdentity(claims, "jwt");
                context.User = new ClaimsPrincipal(identity);
                return true;
            }
            
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Send a 401 Unauthorized response
    /// </summary>
    private async Task SendUnauthorizedResponse(HttpContext context, string message, ErrorCode errorCode)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        
        var result = Result.Failure(message, errorCode);
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
} 