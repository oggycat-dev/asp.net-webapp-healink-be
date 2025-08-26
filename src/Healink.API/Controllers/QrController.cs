using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Domain.Enums;
using Healink.API.Extensions;

namespace Healink.API.Controllers;

/// <summary>
/// QR code controller for bracelet landing pages
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QrController : ControllerBase
{
    private readonly IPodcastService _podcastService;
    private readonly IMoodRecommendationService _moodService;
    private readonly ILogger<QrController> _logger;

    public QrController(
        IPodcastService podcastService,
        IMoodRecommendationService moodService,
        ILogger<QrController> logger)
    {
        _podcastService = podcastService;
        _moodService = moodService;
        _logger = logger;
    }

    /// <summary>
    /// QR code landing page - returns mood-based random podcast
    /// </summary>
    [HttpGet("{qrCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> QrLandingPage(string qrCode, [FromQuery] MoodType? mood = null)
    {
        try
        {
            _logger.LogInformation("QR code scanned: {QrCode} with mood: {Mood}", qrCode, mood);

            // Default mood if not specified
            var selectedMood = mood ?? MoodType.Calm;

            // Get random podcast for the mood
            var result = await _podcastService.GetRandomPodcastsByMoodAsync(selectedMood, 1);
            
            if (!result.IsSuccess || !result.Data!.Any())
            {
                // Fallback to any random podcast
                var fallbackResult = await _podcastService.GetFeaturedPodcastsAsync(1);
                if (!fallbackResult.IsSuccess || !fallbackResult.Data!.Any())
                {
                    return this.NotFound("No podcasts available");
                }
                result = fallbackResult;
            }

            var podcast = result.Data!.First();

            // Create QR landing page response
            var qrResponse = new
            {
                QrCode = qrCode,
                ScannedAt = DateTime.UtcNow,
                SelectedMood = selectedMood,
                Podcast = podcast,
                Message = $"Hello! Here's a {selectedMood.ToString().ToLower()} podcast just for you 🎧",
                WelcomeText = "Welcome to Healink - Your wellness companion",
                Instructions = "Tap play to start your healing journey"
            };

            return this.Success(qrResponse, "QR landing page loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QR code {QrCode}", qrCode);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get mood-specific landing page
    /// </summary>
    [HttpGet("{qrCode}/mood/{mood}")]
    [AllowAnonymous]
    public async Task<IActionResult> QrMoodLandingPage(string qrCode, MoodType mood, [FromQuery] int count = 3)
    {
        try
        {
            _logger.LogInformation("QR mood landing page accessed: {QrCode} for mood: {Mood}", qrCode, mood);

            // Get multiple podcasts for the mood
            var result = await _podcastService.GetPodcastsByMoodAsync(mood, count);
            
            if (!result.IsSuccess)
            {
                return this.FromResult(result);
            }

            var qrResponse = new
            {
                QrCode = qrCode,
                ScannedAt = DateTime.UtcNow,
                SelectedMood = mood,
                Podcasts = result.Data,
                Message = GetMoodMessage(mood),
                WelcomeText = "Welcome to Healink - Your wellness companion",
                MoodDescription = GetMoodDescription(mood)
            };

            return this.Success(qrResponse, "QR mood landing page loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QR mood landing page {QrCode} for mood {Mood}", qrCode, mood);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Record QR scan event
    /// </summary>
    [HttpPost("{qrCode}/scan")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordQrScan(string qrCode, [FromBody] QrScanRequest request)
    {
        try
        {
            var scanEvent = new
            {
                QrCode = qrCode,
                ScannedAt = DateTime.UtcNow,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = GetClientIpAddress(),
                Mood = request.Mood,
                DeviceType = request.DeviceType
            };

            _logger.LogInformation("QR scan recorded: {@ScanEvent}", scanEvent);

            return this.Success(scanEvent, "QR scan recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording QR scan for {QrCode}", qrCode);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Play podcast from QR scan
    /// </summary>
    [HttpPost("{qrCode}/play/{podcastId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> PlayFromQr(string qrCode, Guid podcastId, [FromBody] QrPlayRequest request)
    {
        try
        {
            var playRequest = new PodcastPlayRequest
            {
                PodcastId = podcastId,
                MoodAtPlay = request.Mood,
                DiscoverySource = "qr_code",
                DeviceType = request.DeviceType,
                IsFromQrCode = true
            };

            // If user is authenticated, record play history
            var userIdClaim = User.FindFirst("entity_id")?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var result = await _podcastService.RecordPlayAsync(playRequest, userId);
                return this.FromResult(result, "Podcast play recorded from QR code");
            }

            // For anonymous users, just return success
            var anonymousResponse = new
            {
                QrCode = qrCode,
                PodcastId = podcastId,
                PlayedAt = DateTime.UtcNow,
                Source = "qr_code"
            };

            return this.Success(anonymousResponse, "Podcast play initiated from QR code");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing podcast {PodcastId} from QR {QrCode}", podcastId, qrCode);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get available moods for QR selection
    /// </summary>
    [HttpGet("moods")]
    [AllowAnonymous]
    public IActionResult GetAvailableMoods()
    {
        try
        {
            var moods = Enum.GetValues<MoodType>()
                .Select(m => new
                {
                    Value = (int)m,
                    Name = m.ToString(),
                    DisplayName = GetMoodDisplayName(m),
                    Description = GetMoodDescription(m),
                    Color = GetMoodColor(m),
                    Icon = GetMoodIcon(m)
                })
                .ToList();

            return this.Success(moods, "Available moods retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available moods");
            return this.InternalServerError();
        }
    }

    #region Private Helper Methods

    private string GetMoodMessage(MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "Spread the joy! Here are some uplifting podcasts for you 😊",
            MoodType.Sad => "It's okay to feel sad. These gentle podcasts are here to comfort you 🤗",
            MoodType.Anxious => "Take a deep breath. These calming podcasts will help ease your anxiety 🌸",
            MoodType.Calm => "Perfect! Here are some peaceful podcasts to maintain your zen 🧘‍♀️",
            MoodType.Energetic => "Channel that energy! Here are some motivating podcasts 🔥",
            MoodType.Peaceful => "Embrace the peace. These serene podcasts will enhance your tranquility ☮️",
            MoodType.Motivated => "You're on fire! Here are some inspiring podcasts to fuel your drive 💪",
            MoodType.Reflective => "Time for introspection. These thoughtful podcasts will guide your reflection 💭",
            MoodType.Stressed => "Let's ease that stress. These relaxing podcasts will help you unwind 🌊",
            MoodType.Grateful => "Beautiful! Here are some heartwarming podcasts to amplify your gratitude 🙏",
            MoodType.Hopeful => "Keep that hope alive! These encouraging podcasts will brighten your day ✨",
            MoodType.Frustrated => "We understand. These soothing podcasts will help you find your center 🌱",
            _ => "Here's something special just for you 🎧"
        };
    }

    private string GetMoodDescription(MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "Feeling joyful and positive",
            MoodType.Sad => "Feeling down or melancholic",
            MoodType.Anxious => "Feeling worried or uneasy",
            MoodType.Calm => "Feeling peaceful and relaxed",
            MoodType.Energetic => "Feeling active and vibrant",
            MoodType.Peaceful => "Feeling serene and tranquil",
            MoodType.Motivated => "Feeling driven and inspired",
            MoodType.Reflective => "Feeling thoughtful and introspective",
            MoodType.Stressed => "Feeling overwhelmed or tense",
            MoodType.Grateful => "Feeling thankful and appreciative",
            MoodType.Hopeful => "Feeling optimistic about the future",
            MoodType.Frustrated => "Feeling annoyed or upset",
            _ => "Current emotional state"
        };
    }

    private string GetMoodDisplayName(MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "Happy",
            MoodType.Sad => "Sad",
            MoodType.Anxious => "Anxious",
            MoodType.Calm => "Calm",
            MoodType.Energetic => "Energetic",
            MoodType.Peaceful => "Peaceful",
            MoodType.Motivated => "Motivated",
            MoodType.Reflective => "Reflective",
            MoodType.Stressed => "Stressed",
            MoodType.Grateful => "Grateful",
            MoodType.Hopeful => "Hopeful",
            MoodType.Frustrated => "Frustrated",
            _ => mood.ToString()
        };
    }

    private string GetMoodColor(MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "#FFD700",        // Gold
            MoodType.Sad => "#6495ED",          // Cornflower Blue
            MoodType.Anxious => "#DDA0DD",      // Plum
            MoodType.Calm => "#98FB98",         // Pale Green
            MoodType.Energetic => "#FF6347",    // Tomato
            MoodType.Peaceful => "#87CEEB",     // Sky Blue
            MoodType.Motivated => "#FF4500",    // Orange Red
            MoodType.Reflective => "#9370DB",   // Medium Purple
            MoodType.Stressed => "#F0E68C",     // Khaki
            MoodType.Grateful => "#FFB6C1",     // Light Pink
            MoodType.Hopeful => "#90EE90",      // Light Green
            MoodType.Frustrated => "#FFA07A",   // Light Salmon
            _ => "#808080"                      // Gray
        };
    }

    private string GetMoodIcon(MoodType mood)
    {
        return mood switch
        {
            MoodType.Happy => "😊",
            MoodType.Sad => "😢",
            MoodType.Anxious => "😰",
            MoodType.Calm => "😌",
            MoodType.Energetic => "⚡",
            MoodType.Peaceful => "☮️",
            MoodType.Motivated => "💪",
            MoodType.Reflective => "🤔",
            MoodType.Stressed => "😓",
            MoodType.Grateful => "🙏",
            MoodType.Hopeful => "✨",
            MoodType.Frustrated => "😤",
            _ => "🎧"
        };
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    #endregion
}

/// <summary>
/// Request model for QR scan recording
/// </summary>
public class QrScanRequest
{
    public MoodType? Mood { get; set; }
    public string? DeviceType { get; set; }
}

/// <summary>
/// Request model for QR podcast play
/// </summary>
public class QrPlayRequest
{
    public MoodType? Mood { get; set; }
    public string? DeviceType { get; set; }
}
