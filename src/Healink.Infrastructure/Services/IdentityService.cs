using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using Healink.Domain.Entities.Identity;
using Healink.Infrastructure.Models;

namespace Healink.Infrastructure.Services;

/// <summary>
/// Implementation of identity service for authentication and user management
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly Application.Common.Models.JwtSettings _jwtSettings;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public IdentityService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService,
        IOptions<Application.Common.Models.JwtSettings> jwtSettings,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Authenticate a user and return AppUser if successful
    /// </summary>
    public async Task<Result<AppUser>> AuthenticateAsync(string username, string password, string? grantType = null)
    {
        // Find user by username
        var user = await _userManager.FindByNameAsync(username);
        
        if (user == null)
        {
            return Result<AppUser>.Failure("Username or password is incorrect", ErrorCode.InvalidCredentials);
        }
        
        // Check if user is active
        if (!user.IsActive)
        {
            return Result<AppUser>.Failure("User account is disabled", ErrorCode.AccountDeactivated);
        }
        
        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        
        if (!result.Succeeded)
        {
            return Result<AppUser>.Failure("Username or password is incorrect", ErrorCode.InvalidCredentials);
        }
        
        return Result<AppUser>.Success(user);
    }
    
    /// <summary>
    /// Re-authenticate a user using refresh token
    /// </summary>
    public async Task<Result<AppUser>> ReAuthenticateAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return Result<AppUser>.Failure("User not found", ErrorCode.UserNotFound);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<AppUser>.Failure("User account is disabled", ErrorCode.AccountDeactivated);
        }

        // Validate refresh token
        if (string.IsNullOrEmpty(user.RefreshToken))
        {
            return Result<AppUser>.Failure("Refresh token not found", ErrorCode.Unauthorized);
        }

        if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
        {
            // Clear expired refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
            
            return Result<AppUser>.Failure("Refresh token expired", ErrorCode.Unauthorized);
        }

        return Result<AppUser>.Success(user);
    }
    
    /// <summary>
    /// Create a refresh token for a user
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync(AppUser user)
    {
        // Generate random token
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        
        // Store token in user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);
        
        await _userManager.UpdateAsync(user);
        
        return refreshToken;
    }
    
    /// <summary>
    /// Register a new user account
    /// </summary>
    public async Task<Result<AppUser>> RegisterAsync(AppUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<AppUser>.Failure("Create account failed", ErrorCode.ValidationFailed, errors);
        }
        return Result<AppUser>.Success(user);
    }

    /// <summary>
    /// Register a new staff account
    /// </summary>
    public async Task<Result<StaffRegistrationResponse>> RegisterStaffAsync(StaffRegistrationRequest request)
    {
        // Create identity user
        var entityId = Guid.NewGuid();
        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EntityId = entityId,
            IsActive = true
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<StaffRegistrationResponse>.Failure("Create account failed", ErrorCode.ValidationFailed, errors);
        }
        
        // Assign role based on staff type
        var role = request.Role ?? "Staff";
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            return Result<StaffRegistrationResponse>.Failure("Assign role failed", ErrorCode.ValidationFailed, errors);
        }
            
        // Return response
        return Result<StaffRegistrationResponse>.Success(new StaffRegistrationResponse
        {
            UserId = user.Id,
            EntityId = entityId,
            Username = user.UserName!,
            Email = user.Email!,
            CreatedAt = DateTime.UtcNow,
            IsSuccess = true,
            Message = "Staff account created successfully"
        });
    }
    
    /// <summary>
    /// Update entity ID for a user (link to business entity)
    /// </summary>
    public async Task<Result> UpdateEntityIdAsync(string userId, Guid entityId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found", ErrorCode.UserNotFound);
        }
        
        user.EntityId = entityId;
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure("Update failed", ErrorCode.ValidationFailed, errors);
        }
        
        return Result.Success();
    }
    
    /// <summary>
    /// Change user password (with old password verification)
    /// </summary>
    public async Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found", ErrorCode.UserNotFound);
        }
        
        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure("Change password failed", ErrorCode.ValidationFailed, errors);
        }
        
        return Result.Success("Password changed successfully");
    }
    
    /// <summary>
    /// Change current user password
    /// </summary>
    public async Task<Result> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure("User not authenticated", ErrorCode.Unauthorized);
        }
        
        return await ChangePasswordAsync(userId, oldPassword, newPassword);
    }
    
    /// <summary>
    /// Logout user (clear refresh token)
    /// </summary>
    public async Task<Result> LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found", ErrorCode.UserNotFound);
        }
        
        // Clear refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure("Logout failed", ErrorCode.InternalServerError);
        }
        
        return Result.Success("Logged out successfully");
    }
    
    /// <summary>
    /// Logout current user
    /// </summary>
    public async Task<Result> LogoutAsync()
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure("User not authenticated", ErrorCode.Unauthorized);
        }
        
        return await LogoutAsync(userId);
    }
    
    /// <summary>
    /// Check if phone number is unique
    /// </summary>
    public async Task<Result<bool>> IsPhoneUniqueAsync(string phone)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        return Result<bool>.Success(user == null);
    }
    
    /// <summary>
    /// Check if email is unique
    /// </summary>
    public async Task<Result<bool>> IsEmailUniqueAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return Result<bool>.Success(user == null);
    }
    
    /// <summary>
    /// Check if username is unique
    /// </summary>
    public async Task<Result<bool>> IsUsernameUniqueAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return Result<bool>.Success(user == null);
    }
    
    /// <summary>
    /// Get user roles
    /// </summary>
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new List<string>();
        }
        
        return await _userManager.GetRolesAsync(user);
    }
    
    /// <summary>
    /// Find user by username
    /// </summary>
    public async Task<Result<AppUser?>> FindByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return Result<AppUser?>.Success(user);
    }
    
    /// <summary>
    /// Find user by ID
    /// </summary>
    public async Task<Result<AppUser?>> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return Result<AppUser?>.Success(user);
    }
    
    /// <summary>
    /// Find user by email
    /// </summary>
    public async Task<Result<AppUser?>> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return Result<AppUser?>.Success(user);
    }
    
    /// <summary>
    /// Add user to role
    /// </summary>
    public async Task<IdentityResult> AddToRoleAsync(AppUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }
    
    /// <summary>
    /// Update user
    /// </summary>
    public async Task<Result> UpdateUserAsync(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure("Update failed", ErrorCode.ValidationFailed, errors);
        }
        
        return Result.Success("User updated successfully");
    }
    
    /// <summary>
    /// Check if user exists
    /// </summary>
    public async Task<bool> IsUserIdExist(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null;
    }
    
    /// <summary>
    /// Delete user
    /// </summary>
    public async Task<bool> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
    
    /// <summary>
    /// Reset user password
    /// </summary>
    public async Task<Result> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found", ErrorCode.UserNotFound);
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure("Reset password failed", ErrorCode.ValidationFailed, errors);
        }
        
        return Result.Success("Password reset successfully");
    }
} 