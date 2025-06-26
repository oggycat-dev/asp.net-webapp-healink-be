using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for login command
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to authenticate user: {Username}", request.Username);

            // Authenticate user
            var authResult = await _identityService.AuthenticateAsync(
                request.Username, 
                request.Password, 
                request.GrantType);

            if (!authResult.IsSuccess)
            {
                _logger.LogWarning("Authentication failed for user: {Username}. Reason: {Error}", 
                    request.Username, authResult.Message);
                return Result<AuthenticationResponse>.Failure(authResult.Message ?? "Authentication failed", ErrorCode.InvalidCredentials);
            }

            var user = authResult.Data!;

            // Generate JWT token
            var (token, roles, expiresInMinutes) = _jwtService.GenerateJwtTokenWithExpiration(user);
            
            // Generate refresh token
            var refreshToken = await _identityService.GenerateRefreshTokenAsync(user);

            var response = new AuthenticationResponse
            {
                UserId = user.Id,
                EntityId = user.EntityId,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresInMinutes = expiresInMinutes,
                IsActive = user.IsActive
            };

            _logger.LogInformation("User {Username} authenticated successfully with roles: {Roles}", 
                request.Username, string.Join(", ", roles));

            return Result<AuthenticationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while authenticating user: {Username}", request.Username);
            return Result<AuthenticationResponse>.Failure("An error occurred during authentication", ErrorCode.InternalServerError);
        }
    }
} 