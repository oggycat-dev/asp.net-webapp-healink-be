namespace Healink.Infrastructure.Models;

/// <summary>
/// JWT settings configuration for Infrastructure layer
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key for token signing
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes (kept for backward compatibility)
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 60;

    /// <summary>
    /// Token expiration time in minutes (standard naming)
    /// </summary>
    public int ExpiresInMinutes 
    { 
        get => ExpiryInMinutes; 
        set => ExpiryInMinutes = value; 
    }

    /// <summary>
    /// Refresh token expiration time in days
    /// </summary>
    public int RefreshTokenExpiresInDays { get; set; } = 7;

    /// <summary>
    /// Token algorithm
    /// </summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>
    /// Validate issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Validate audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Validate lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Validate issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew in minutes
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
} 