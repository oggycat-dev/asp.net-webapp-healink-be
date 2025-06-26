using System.Net;

namespace Healink.Application.Common.Models;

/// <summary>
/// Generic result wrapper for API responses
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// List of errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Create successful result
    /// </summary>
    public static Result Success(string? message = null)
    {
        return new Result
        {
            IsSuccess = true,
            Message = message,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Create failure result
    /// </summary>
    public static Result Failure(string message, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }

    /// <summary>
    /// Create failure result with multiple errors
    /// </summary>
    public static Result Failure(List<string> errors, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = errors.FirstOrDefault(),
            Errors = errors,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Create failure result with error code
    /// </summary>
    public static Result Failure(string message, ErrorCode errorCode, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }

    /// <summary>
    /// Create failure result with error code and multiple errors
    /// </summary>
    public static Result Failure(string message, ErrorCode errorCode, List<string>? errors, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string> { message }
        };
    }

    /// <summary>
    /// Get HTTP status code for response
    /// </summary>
    public int GetHttpStatusCode()
    {
        return StatusCode;
    }
}

/// <summary>
/// Generic result wrapper with data
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Result data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Create successful result with data
    /// </summary>
    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Create failure result
    /// </summary>
    public static new Result<T> Failure(string message, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }

    /// <summary>
    /// Create failure result with multiple errors
    /// </summary>
    public static new Result<T> Failure(List<string> errors, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = errors.FirstOrDefault(),
            Errors = errors,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Create failure result with error code
    /// </summary>
    public static Result<T> Failure(string message, ErrorCode errorCode, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }

    /// <summary>
    /// Create failure result with error code and multiple errors
    /// </summary>
    public static Result<T> Failure(string message, ErrorCode errorCode, List<string>? errors, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string> { message }
        };
    }
} 