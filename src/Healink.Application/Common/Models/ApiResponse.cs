using System.Text.Json.Serialization;

namespace Healink.Application.Common.Models;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Error code - 0 for success, > 0 for various error types
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Success status - true for success, false for failure
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data payload (null for failures)
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Validation errors (for 400 Bad Request responses)
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful response with data
    /// </summary>
    public static ApiResponse<T> SuccessWithData(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Code = 0,
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a failure response with error code
    /// </summary>
    public static ApiResponse<T> Failure(int errorCode, string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Code = errorCode,
            Success = false,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a validation failure response
    /// </summary>
    public static ApiResponse<T> ValidationFailure(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        return new ApiResponse<T>
        {
            Code = (int)ErrorCode.ValidationFailed,
            Success = false,
            Message = message,
            Data = default,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a failure response from ErrorCode enum
    /// </summary>
    public static ApiResponse<T> Failure(ErrorCode errorCode, string message, T? data = default)
    {
        return Failure((int)errorCode, message, data);
    }
}

/// <summary>
/// Non-generic ApiResponse for responses without data
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// Create a successful response without data
    /// </summary>
    public static ApiResponse<object> Success(string message = "Operation completed successfully")
    {
        return new ApiResponse<object>
        {
            Code = 0,
            Success = true,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a failure response
    /// </summary>
    public static ApiResponse<object> Failure(int errorCode, string message)
    {
        return new ApiResponse<object>
        {
            Code = errorCode,
            Success = false,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a failure response from ErrorCode enum
    /// </summary>
    public static ApiResponse<object> Failure(ErrorCode errorCode, string message)
    {
        return Failure((int)errorCode, message);
    }

    /// <summary>
    /// Create a validation failure response
    /// </summary>
    public static ApiResponse<object> ValidationFailure(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        return new ApiResponse<object>
        {
            Code = (int)ErrorCode.ValidationFailed,
            Success = false,
            Message = message,
            Data = null,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }
}
