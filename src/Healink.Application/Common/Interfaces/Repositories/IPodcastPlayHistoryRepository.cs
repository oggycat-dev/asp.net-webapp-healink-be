using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for podcast play history management
/// </summary>
public interface IPodcastPlayHistoryRepository : IGenericRepository<PodcastPlayHistory>
{
    /// <summary>
    /// Get user's play history
    /// </summary>
    Task<IEnumerable<PodcastPlayHistory>> GetByUserAsync(Guid userProfileId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's recently played podcasts
    /// </summary>
    Task<IEnumerable<PodcastPlayHistory>> GetRecentlyPlayedAsync(Guid userProfileId, int days = 7, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's favorite podcasts (highly rated and completed)
    /// </summary>
    Task<IEnumerable<PodcastPlayHistory>> GetFavoritesAsync(Guid userProfileId, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get podcast's play statistics
    /// </summary>
    Task<(int totalPlays, int uniqueUsers, decimal averageCompletion, decimal averageRating)> GetPodcastStatsAsync(Guid podcastId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's listening patterns by mood
    /// </summary>
    Task<Dictionary<MoodType, int>> GetMoodListeningPatternsAsync(Guid userProfileId, int days = 30, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's listening patterns by time of day
    /// </summary>
    Task<Dictionary<int, int>> GetTimeListeningPatternsAsync(Guid userProfileId, int days = 30, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if user has played a podcast
    /// </summary>
    Task<bool> HasUserPlayedPodcastAsync(Guid userProfileId, Guid podcastId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's last play position for a podcast
    /// </summary>
    Task<int?> GetLastPlayPositionAsync(Guid userProfileId, Guid podcastId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get trending podcasts based on recent plays
    /// </summary>
    Task<IEnumerable<Guid>> GetTrendingPodcastIdsAsync(int days = 7, int take = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get QR code scan statistics
    /// </summary>
    Task<(int totalScans, int uniqueUsers, Dictionary<Guid, int> braceletScans)> GetQrScanStatsAsync(int days = 30, CancellationToken cancellationToken = default);
}
