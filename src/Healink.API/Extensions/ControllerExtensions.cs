using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Healink.Application.Common.Models;

namespace Healink.API.Extensions;

/// <summary>
/// Extension methods for controllers to return standardized API responses
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Return a successful response with data
    /// </summary>
    public static IActionResult Success<T>(this ControllerBase controller, T data, string message = "Operation completed successfully")
    {
        var response = ApiResponse<T>.SuccessWithData(data, message);
        return controller.Ok(response);
    }

    /// <summary>
    /// Return a successful response without data
    /// </summary>
    public static IActionResult Success(this ControllerBase controller, string message = "Operation completed successfully")
    {
        var response = ApiResponse.Success(message);
        return controller.Ok(response);
    }

    /// <summary>
    /// Return a created response with data
    /// </summary>
    public static IActionResult Created<T>(this ControllerBase controller, T data, string message = "Resource created successfully")
    {
        var response = ApiResponse<T>.SuccessWithData(data, message);
        return controller.StatusCode(201, response);
    }

    /// <summary>
    /// Return a bad request response
    /// </summary>
    public static IActionResult BadRequest(this ControllerBase controller, ErrorCode errorCode, string message)
    {
        var response = ApiResponse.Failure(errorCode, message);
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Return a bad request response with custom error code
    /// </summary>
    public static IActionResult BadRequest(this ControllerBase controller, int errorCode, string message)
    {
        var response = ApiResponse.Failure(errorCode, message);
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Return a validation failure response
    /// </summary>
    public static IActionResult ValidationError(this ControllerBase controller, Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        var response = ApiResponse.ValidationFailure(errors, message);
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Return a validation failure response from FluentValidation exception
    /// </summary>
    public static IActionResult ValidationError(this ControllerBase controller, ValidationException ex, string message = "Validation failed")
    {
        var errors = ex.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        
        var response = ApiResponse.ValidationFailure(errors, message);
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Return a not found response
    /// </summary>
    public static IActionResult NotFound(this ControllerBase controller, string message = "Resource not found")
    {
        var response = ApiResponse.Failure(ErrorCode.NotFound, message);
        return controller.StatusCode(404, response);
    }

    /// <summary>
    /// Return an unauthorized response
    /// </summary>
    public static IActionResult Unauthorized(this ControllerBase controller, string message = "Unauthorized access")
    {
        var response = ApiResponse.Failure(ErrorCode.Unauthorized, message);
        return controller.StatusCode(401, response);
    }

    /// <summary>
    /// Return a forbidden response
    /// </summary>
    public static IActionResult Forbidden(this ControllerBase controller, string message = "Access forbidden")
    {
        var response = ApiResponse.Failure(ErrorCode.Forbidden, message);
        return controller.StatusCode(403, response);
    }

    /// <summary>
    /// Return a conflict response
    /// </summary>
    public static IActionResult Conflict(this ControllerBase controller, string message = "Resource conflict")
    {
        var response = ApiResponse.Failure(ErrorCode.DuplicateEntry, message);
        return controller.StatusCode(409, response);
    }

    /// <summary>
    /// Return an internal server error response
    /// </summary>
    public static IActionResult InternalServerError(this ControllerBase controller, string message = "An internal server error occurred")
    {
        var response = ApiResponse.Failure(ErrorCode.InternalServerError, message);
        return controller.StatusCode(500, response);
    }

    /// <summary>
    /// Handle Result<T> responses from application layer
    /// </summary>
    public static IActionResult FromResult<T>(this ControllerBase controller, Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return controller.Success(result.Data!, successMessage ?? "Operation completed successfully");
        }

        // Map status codes to appropriate responses
        return result.StatusCode switch
        {
            400 => controller.BadRequest(ErrorCode.BadRequest, result.Message ?? "Bad request"),
            401 => controller.Unauthorized(result.Message ?? "Unauthorized access"),
            403 => controller.Forbidden(result.Message ?? "Access forbidden"),
            404 => controller.NotFound(result.Message ?? "Resource not found"),
            409 => controller.Conflict(result.Message ?? "Resource conflict"),
            _ => controller.InternalServerError(result.Message ?? "An internal server error occurred")
        };
    }

    /// <summary>
    /// Handle Result responses from application layer (without data)
    /// </summary>
    public static IActionResult FromResult(this ControllerBase controller, Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return controller.Success(successMessage ?? result.Message ?? "Operation completed successfully");
        }

        // Map status codes to appropriate responses
        return result.StatusCode switch
        {
            400 => controller.BadRequest(ErrorCode.BadRequest, result.Message ?? "Bad request"),
            401 => controller.Unauthorized(result.Message ?? "Unauthorized access"),
            403 => controller.Forbidden(result.Message ?? "Access forbidden"),
            404 => controller.NotFound(result.Message ?? "Resource not found"),
            409 => controller.Conflict(result.Message ?? "Resource conflict"),
            _ => controller.InternalServerError(result.Message ?? "An internal server error occurred")
        };
    }
}
