using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Healink.Application.Common.Models;

namespace Healink.API.Middlewares;

/// <summary>
/// Global model validation filter
/// </summary>
public class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var result = Result.Failure("Validation failed", ErrorCode.ValidationFailed, errors);
            context.Result = new BadRequestObjectResult(result);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No implementation needed
    }
}

/// <summary>
/// Extension methods for validation configuration
/// </summary>
public static class ValidationConfiguration
{
    /// <summary>
    /// Add validation configuration to services
    /// </summary>
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        // Configure model validation
        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Disable automatic model validation responses
            options.SuppressModelStateInvalidFilter = true;
        });

        // Add global validation filter
        services.AddScoped<ValidationActionFilter>();

        return services;
    }
} 