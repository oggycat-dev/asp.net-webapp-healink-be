using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;
using Healink.API.Extensions;

namespace Healink.API.Controllers;

/// <summary>
/// Podcast management controller for wellness content
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PodcastController : ControllerBase
{
    private readonly IPodcastService _podcastService;
    private readonly ILogger<PodcastController> _logger;

    public PodcastController(
        IPodcastService podcastService,
        ILogger<PodcastController> logger)
    {
        _podcastService = podcastService;
        _logger = logger;
    }

    /// <summary>
    /// Get all podcasts with pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPodcasts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetAllPodcastsAsync(page, pageSize, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get podcast by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPodcast(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetPodcastAsync(id, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast {PodcastId}", id);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get podcasts by mood type
    /// </summary>
    [HttpGet("by-mood/{mood}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPodcastsByMood(MoodType mood, [FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetPodcastsByMoodAsync(mood, take, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts by mood {Mood}", mood);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get random podcasts by mood type
    /// </summary>
    [HttpGet("random/{mood}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRandomPodcastsByMood(MoodType mood, [FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetRandomPodcastsByMoodAsync(mood, take, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random podcasts by mood {Mood}", mood);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get mood-based recommendations for current user
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromBody] PodcastRecommendationRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.GetRecommendationsAsync(request, currentUserId.Value);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast recommendations");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get featured podcasts
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedPodcasts([FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetFeaturedPodcastsAsync(take, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured podcasts");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get trending podcasts
    /// </summary>
    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrendingPodcasts([FromQuery] int days = 7, [FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetTrendingPodcastsAsync(days, take, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending podcasts");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Search podcasts
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPodcasts([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return this.BadRequest(ErrorCode.BadRequest, "Search term is required");

            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.SearchPodcastsAsync(searchTerm, page, pageSize, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching podcasts with term {SearchTerm}", searchTerm);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get podcasts by category
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPodcastsByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _podcastService.GetPodcastsByCategoryAsync(categoryId, page, pageSize, currentUserId);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts by category {CategoryId}", categoryId);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Record podcast play
    /// </summary>
    [HttpPost("play")]
    public async Task<IActionResult> RecordPlay([FromBody] PodcastPlayRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.RecordPlayAsync(request, currentUserId.Value);
            return this.FromResult(result, "Play recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording podcast play for podcast {PodcastId}", request.PodcastId);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Update podcast play progress
    /// </summary>
    [HttpPut("play/progress")]
    public async Task<IActionResult> UpdatePlayProgress([FromBody] PodcastPlayUpdateRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.UpdatePlayProgressAsync(request, currentUserId.Value);
            return this.FromResult(result, "Play progress updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating play progress for play history {PlayHistoryId}", request.PlayHistoryId);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get user's play history
    /// </summary>
    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyPlayHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.GetUserPlayHistoryAsync(currentUserId.Value, page, pageSize);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user play history");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get user's recently played podcasts
    /// </summary>
    [HttpGet("my-recent")]
    public async Task<IActionResult> GetMyRecentlyPlayed([FromQuery] int days = 7, [FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.GetRecentlyPlayedAsync(currentUserId.Value, days, take);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently played podcasts");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get user's favorite podcasts
    /// </summary>
    [HttpGet("my-favorites")]
    public async Task<IActionResult> GetMyFavorites([FromQuery] int take = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            var result = await _podcastService.GetUserFavoritesAsync(currentUserId.Value, take);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user favorites");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Helper method to get current user ID
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("entity_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Helper method to get current user ID (required)
    /// </summary>
    private Guid? GetCurrentUserIdRequired()
    {
        return GetCurrentUserId();
    }
}
