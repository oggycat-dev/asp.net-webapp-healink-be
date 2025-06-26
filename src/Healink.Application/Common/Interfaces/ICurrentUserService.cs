namespace Healink.Application.Common.Interfaces;

/// <summary>
/// Current user service interface
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Current user ID
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Current user email
    /// </summary>
    string? UserEmail { get; }
    
    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
} 