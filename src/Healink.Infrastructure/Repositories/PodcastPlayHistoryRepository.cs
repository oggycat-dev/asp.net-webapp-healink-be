using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for podcast play history management
/// </summary>
public class PodcastPlayHistoryRepository : GenericRepository<PodcastPlayHistory>, IPodcastPlayHistoryRepository
{
    public PodcastPlayHistoryRepository(HealinkDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user's play history
    /// </summary>
    public async Task<IEnumerable<PodcastPlayHistory>> GetByUserAsync(Guid userProfileId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(ph => ph.UserProfileId == userProfileId,
                                   ph => ph.Podcast, ph => ph.Podcast.Category)
            .OrderByDescending(ph => ph.PlayedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user's recently played podcasts
    /// </summary>
    public async Task<IEnumerable<PodcastPlayHistory>> GetRecentlyPlayedAsync(Guid userProfileId, int days = 7, int take = 10, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await FindByCondition(ph => ph.UserProfileId == userProfileId && ph.PlayedAt >= cutoffDate,
                                   ph => ph.Podcast, ph => ph.Podcast.Category)
            .OrderByDescending(ph => ph.PlayedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user's favorite podcasts (highly rated and completed)
    /// </summary>
    public async Task<IEnumerable<PodcastPlayHistory>> GetFavoritesAsync(Guid userProfileId, int take = 10, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(ph => ph.UserProfileId == userProfileId &&
                                          ph.IsLiked == true &&
                                          ph.CompletionPercentage >= 80,
                                   ph => ph.Podcast, ph => ph.Podcast.Category)
            .OrderByDescending(ph => ph.Rating)
            .ThenByDescending(ph => ph.CompletionPercentage)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get podcast's play statistics
    /// </summary>
    public async Task<(int totalPlays, int uniqueUsers, decimal averageCompletion, decimal averageRating)> GetPodcastStatsAsync(Guid podcastId, CancellationToken cancellationToken = default)
    {
        var playHistory = await FindByCondition(ph => ph.PodcastId == podcastId)
            .ToListAsync(cancellationToken);

        var totalPlays = playHistory.Count;
        var uniqueUsers = playHistory.Select(ph => ph.UserProfileId).Distinct().Count();
        var averageCompletion = playHistory.Any() ? (decimal)playHistory.Average(ph => ph.CompletionPercentage) : 0;
        var ratingsOnly = playHistory.Where(ph => ph.Rating.HasValue).ToList();
        var averageRating = ratingsOnly.Any() ? (decimal)ratingsOnly.Average(ph => ph.Rating!.Value) : 0;

        return (totalPlays, uniqueUsers, averageCompletion, averageRating);
    }

    /// <summary>
    /// Get user's listening patterns by mood
    /// </summary>
    public async Task<Dictionary<MoodType, int>> GetMoodListeningPatternsAsync(Guid userProfileId, int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var moodCounts = await FindByCondition(ph => ph.UserProfileId == userProfileId &&
                                                    ph.PlayedAt >= cutoffDate &&
                                                    ph.MoodAtPlay.HasValue)
            .GroupBy(ph => ph.MoodAtPlay!.Value)
            .Select(g => new { Mood = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return moodCounts.ToDictionary(mc => mc.Mood, mc => mc.Count);
    }

    /// <summary>
    /// Get user's listening patterns by time of day
    /// </summary>
    public async Task<Dictionary<int, int>> GetTimeListeningPatternsAsync(Guid userProfileId, int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var timeCounts = await FindByCondition(ph => ph.UserProfileId == userProfileId && ph.PlayedAt >= cutoffDate)
            .GroupBy(ph => ph.PlayedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return timeCounts.ToDictionary(tc => tc.Hour, tc => tc.Count);
    }

    /// <summary>
    /// Check if user has played a podcast
    /// </summary>
    public async Task<bool> HasUserPlayedPodcastAsync(Guid userProfileId, Guid podcastId, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(ph => ph.UserProfileId == userProfileId && ph.PodcastId == podcastId)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Get user's last play position for a podcast
    /// </summary>
    public async Task<int?> GetLastPlayPositionAsync(Guid userProfileId, Guid podcastId, CancellationToken cancellationToken = default)
    {
        var lastPlay = await FindByCondition(ph => ph.UserProfileId == userProfileId && ph.PodcastId == podcastId)
            .OrderByDescending(ph => ph.PlayedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return lastPlay?.ListenedSeconds;
    }

    /// <summary>
    /// Get trending podcasts based on recent plays
    /// </summary>
    public async Task<IEnumerable<Guid>> GetTrendingPodcastIdsAsync(int days = 7, int take = 10, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await FindByCondition(ph => ph.PlayedAt >= cutoffDate)
            .GroupBy(ph => ph.PodcastId)
            .OrderByDescending(g => g.Count())
            .Take(take)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get QR code scan statistics
    /// </summary>
    public async Task<(int totalScans, int uniqueUsers, Dictionary<Guid, int> braceletScans)> GetQrScanStatsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var qrScans = await FindByCondition(ph => ph.IsFromQrCode && ph.PlayedAt >= cutoffDate)
            .ToListAsync(cancellationToken);

        var totalScans = qrScans.Count;
        var uniqueUsers = qrScans.Select(ph => ph.UserProfileId).Distinct().Count();
        
        var braceletScans = qrScans
            .Where(ph => ph.QrBraceletId.HasValue)
            .GroupBy(ph => ph.QrBraceletId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return (totalScans, uniqueUsers, braceletScans);
    }
}
