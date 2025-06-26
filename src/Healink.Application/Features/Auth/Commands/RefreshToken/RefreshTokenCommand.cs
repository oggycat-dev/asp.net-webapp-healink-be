using MediatR;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token using refresh token
/// </summary>
public record RefreshTokenCommand : IRequest<Result<AuthenticationResponse>>
{
    /// <summary>
    /// The refresh token
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    
    /// <summary>
    /// User ID to refresh token for
    /// </summary>
    public string UserId { get; init; } = string.Empty;
} 