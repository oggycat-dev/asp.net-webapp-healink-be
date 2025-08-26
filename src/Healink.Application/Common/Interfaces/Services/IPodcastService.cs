using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for podcast management
/// </summary>
public interface IPodcastService
{
    /// <summary>
    /// Get podcast by ID
    /// </summary>
    Task<Result<PodcastDto>> GetPodcastAsync(Guid id, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all podcasts with pagination
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetAllPodcastsAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts by mood
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetPodcastsByMoodAsync(MoodType mood, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get random podcasts by mood
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetRandomPodcastsByMoodAsync(MoodType mood, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get mood-based recommendations for user
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetRecommendationsAsync(PodcastRecommendationRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get featured podcasts
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetFeaturedPodcastsAsync(int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get trending podcasts
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetTrendingPodcastsAsync(int days = 7, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search podcasts
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> SearchPodcastsAsync(string searchTerm, int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts by category
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetPodcastsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new podcast
    /// </summary>
    Task<Result<PodcastDto>> CreatePodcastAsync(CreatePodcastRequest request, string audioUrl, string? coverImageUrl, int durationSeconds, long fileSizeBytes, Guid createdBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update podcast
    /// </summary>
    Task<Result<PodcastDto>> UpdatePodcastAsync(Guid id, UpdatePodcastRequest request, Guid updatedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete podcast
    /// </summary>
    Task<Result> DeletePodcastAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Record podcast play
    /// </summary>
    Task<Result<Guid>> RecordPlayAsync(PodcastPlayRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update podcast play progress
    /// </summary>
    Task<Result> UpdatePlayProgressAsync(PodcastPlayUpdateRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's play history
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetUserPlayHistoryAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's recently played podcasts
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetRecentlyPlayedAsync(Guid userId, int days = 7, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's favorite podcasts
    /// </summary>
    Task<Result<IEnumerable<PodcastDto>>> GetUserFavoritesAsync(Guid userId, int take = 10, CancellationToken cancellationToken = default);
}
