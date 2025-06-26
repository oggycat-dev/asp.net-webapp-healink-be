using MediatR;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login command to authenticate user
/// </summary>
public record LoginCommand : IRequest<Result<AuthenticationResponse>>
{
    /// <summary>
    /// Username or email
    /// </summary>
    public string Username { get; init; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// Optional grant type for OAuth flows
    /// </summary>
    public string? GrantType { get; init; }
    
    /// <summary>
    /// Remember me option for extended session
    /// </summary>
    public bool RememberMe { get; init; } = false;
} 