using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using Healink.Domain.Entities.Identity;

namespace Healink.Infrastructure.Services;

/// <summary>
/// Implementation of JWT service for generating and validating tokens
/// </summary>
public class JwtService : IJwtService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    
    public JwtService(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }
    
    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    public (string token, List<string> roles) GenerateJwtToken(AppUser user, string? requestOrigin = null)
    {
        // Get user claims and roles
        var userRoles = _userManager.GetRolesAsync(user).Result.ToList();
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
        };
        
        // Add role claims
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        // Add entity ID if available (for staff profile)
        if (user.EntityId.HasValue)
        {
            claims.Add(new Claim("entity_id", user.EntityId.Value.ToString()));
        }
        
        // Create signing credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // Create token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            signingCredentials: creds
        );
        
        return (new JwtSecurityTokenHandler().WriteToken(token), userRoles ?? new List<string>());
    }

    /// <summary>
    /// Generate JWT token with expiration info for a user
    /// </summary>
    public (string token, List<string> roles, int expiresInMinutes) GenerateJwtTokenWithExpiration(AppUser user, string? requestOrigin = null)
    {
        var (token, roles) = GenerateJwtToken(user, requestOrigin);
        return (token, roles, _jwtSettings.ExpiresInMinutes);
    }
    
    /// <summary>
    /// Get token expiration time in minutes
    /// </summary>
    public int GetTokenExpirationMinutes()
    {
        return _jwtSettings.ExpiresInMinutes;
    }
    
    /// <summary>
    /// Generate refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Get user id from token
    /// </summary>
    public string? GetUserIdFromToken(string token)
    {
        if (!ValidateToken(token))
            return null;
            
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    }
    
    /// <summary>
    /// Get entity id from token (for staff profile)
    /// </summary>
    public string? GetEntityIdFromToken(string token)
    {
        if (!ValidateToken(token))
            return null;
            
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        return jwtToken.Claims.FirstOrDefault(x => x.Type == "entity_id")?.Value;
    }
    
    /// <summary>
    /// Get principal claims from token
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false // Don't validate lifetime here
            }, out SecurityToken validatedToken);
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
} 