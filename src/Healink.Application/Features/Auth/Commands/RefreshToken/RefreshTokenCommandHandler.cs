using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for refresh token command
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token for user: {UserId}", request.UserId);

            // Re-authenticate using refresh token
            var authResult = await _identityService.ReAuthenticateAsync(request.UserId);

            if (!authResult.IsSuccess)
            {
                _logger.LogWarning("Token refresh failed for user: {UserId}. Reason: {Error}", 
                    request.UserId, authResult.Message);
                return Result<AuthenticationResponse>.Failure(authResult.Message ?? "Token refresh failed", ErrorCode.Unauthorized);
            }

            var user = authResult.Data!;

            // Validate refresh token matches
            if (user.RefreshToken != request.RefreshToken)
            {
                _logger.LogWarning("Invalid refresh token provided for user: {UserId}", request.UserId);
                return Result<AuthenticationResponse>.Failure("Invalid refresh token", ErrorCode.Unauthorized);
            }

            // Generate new JWT token
            var (token, roles, expiresInMinutes) = _jwtService.GenerateJwtTokenWithExpiration(user);
            
            // Generate new refresh token
            var newRefreshToken = await _identityService.GenerateRefreshTokenAsync(user);

            var response = new AuthenticationResponse
            {
                UserId = user.Id,
                EntityId = user.EntityId,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles,
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresInMinutes = expiresInMinutes,
                IsActive = user.IsActive
            };

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", request.UserId);

            return Result<AuthenticationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while refreshing token for user: {UserId}", request.UserId);
            return Result<AuthenticationResponse>.Failure("An error occurred during token refresh", ErrorCode.InternalServerError);
        }
    }
} 