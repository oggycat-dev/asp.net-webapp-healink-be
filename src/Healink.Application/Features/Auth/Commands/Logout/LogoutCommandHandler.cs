using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler for logout command
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        ILogger<LogoutCommandHandler> logger)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = request.UserId ?? _currentUserService.UserId;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Logout attempted without valid user ID");
                return Result.Failure("Invalid user session", ErrorCode.Unauthorized);
            }

            _logger.LogInformation("Attempting to logout user: {UserId}", userId);

            var result = await _identityService.LogoutAsync(userId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Logout failed for user: {UserId}. Reason: {Error}", userId, result.Message);
                return result;
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while logging out user: {UserId}", request.UserId);
            return Result.Failure("An error occurred during logout", ErrorCode.InternalServerError);
        }
    }
} 