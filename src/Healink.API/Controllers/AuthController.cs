using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Features.Auth.Commands.Login;
using Healink.Application.Features.Auth.Commands.RefreshToken;
using Healink.Application.Features.Auth.Commands.Logout;
using Healink.API.Middlewares;

namespace Healink.API.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password,
                GrantType = request.GrantType,
                RememberMe = request.RememberMe
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for user: {Username}. Reason: {Error}", 
                    request.Username, result.Message);
                
                return result.StatusCode switch
                {
                    401 => Unauthorized(new { message = result.Message }),
                    403 => Forbid(),
                    _ => BadRequest(new { message = result.Message })
                };
            }

            _logger.LogInformation("Login successful for user: {Username}", request.Username);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new { message = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Refresh token endpoint
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication response</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt for user: {UserId}", request.UserId);

            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                UserId = request.UserId
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Token refresh failed for user: {UserId}. Reason: {Error}", 
                    request.UserId, result.Message);
                
                return result.StatusCode switch
                {
                    401 => Unauthorized(new { message = result.Message }),
                    404 => NotFound(new { message = result.Message }),
                    _ => BadRequest(new { message = result.Message })
                };
            }

            _logger.LogInformation("Token refresh successful for user: {UserId}", request.UserId);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh for user: {UserId}", request.UserId);
            return StatusCode(500, new { message = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Logout endpoint
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var command = new LogoutCommand();
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Logout failed. Reason: {Error}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            _logger.LogInformation("Logout successful");
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            var entityId = User.FindFirst("entity_id")?.Value;

            var userInfo = new
            {
                UserId = userId,
                Email = email,
                Roles = roles,
                EntityId = entityId
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(500, new { message = "An internal server error occurred" });
        }
    }
} 