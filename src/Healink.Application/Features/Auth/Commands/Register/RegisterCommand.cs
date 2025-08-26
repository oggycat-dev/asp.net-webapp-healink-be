using MediatR;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterCommand : IRequest<Result<UserRegistrationResponse>>
{
    /// <summary>
    /// Username for the new account
    /// </summary>
    public string Username { get; init; } = string.Empty;
    
    /// <summary>
    /// Email address for the new account
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Password for the new account
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// Password confirmation
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; init; }
}
