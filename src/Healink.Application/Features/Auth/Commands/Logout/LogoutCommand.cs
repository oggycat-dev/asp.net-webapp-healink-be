using MediatR;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout user and invalidate tokens
/// </summary>
public record LogoutCommand : IRequest<Result>
{
    /// <summary>
    /// User ID to logout (optional - if not provided, uses current user)
    /// </summary>
    public string? UserId { get; init; }
} 