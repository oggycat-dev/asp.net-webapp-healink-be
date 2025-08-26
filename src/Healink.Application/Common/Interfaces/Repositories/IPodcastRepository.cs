using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for podcast management
/// </summary>
public interface IPodcastRepository : IGenericRepository<Podcast>
{
    /// <summary>
    /// Get podcasts by mood type
    /// </summary>
    Task<IEnumerable<Podcast>> GetByMoodAsync(MoodType mood, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get random podcasts by mood type
    /// </summary>
    Task<IEnumerable<Podcast>> GetRandomByMoodAsync(MoodType mood, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts by category
    /// </summary>
    Task<IEnumerable<Podcast>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get featured podcasts
    /// </summary>
    Task<IEnumerable<Podcast>> GetFeaturedAsync(int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get trending podcasts (most played recently)
    /// </summary>
    Task<IEnumerable<Podcast>> GetTrendingAsync(int days = 7, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search podcasts by title, description, or tags
    /// </summary>
    Task<IEnumerable<Podcast>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts for recommendation based on user preferences
    /// </summary>
    Task<IEnumerable<Podcast>> GetRecommendationsAsync(
        Guid userProfileId, 
        IEnumerable<MoodType> moods, 
        IEnumerable<ContentType> contentTypes,
        int? maxDurationMinutes = null,
        int? minDurationMinutes = null,
        bool excludeRecentlyPlayed = true,
        int excludeRecentDays = 7,
        int take = 10,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts by multiple mood types
    /// </summary>
    Task<IEnumerable<Podcast>> GetByMoodsAsync(IEnumerable<MoodType> moods, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcasts with specific duration range
    /// </summary>
    Task<IEnumerable<Podcast>> GetByDurationRangeAsync(int minSeconds, int maxSeconds, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update podcast play count
    /// </summary>
    Task UpdatePlayCountAsync(Guid podcastId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update podcast rating
    /// </summary>
    Task UpdateRatingAsync(Guid podcastId, decimal newRating, int ratingCount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcast statistics
    /// </summary>
    Task<(int totalPodcasts, int totalPlays, decimal averageRating)> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
