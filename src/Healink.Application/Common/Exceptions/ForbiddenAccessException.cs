namespace Healink.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a resource they are not authorized to access
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class
    /// </summary>
    public ForbiddenAccessException()
        : base("Access denied. You do not have permission to access this resource.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
} 