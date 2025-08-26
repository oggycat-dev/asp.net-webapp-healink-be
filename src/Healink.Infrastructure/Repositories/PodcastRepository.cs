using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for podcast management
/// </summary>
public class PodcastRepository : GenericRepository<Podcast>, IPodcastRepository
{
    public PodcastRepository(HealinkDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get podcasts by mood type
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetByMoodAsync(MoodType mood, int take = 10, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                         (p.PrimaryMood == mood || p.MoodTags.Any(mt => mt.MoodType == mood)),
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PlayCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get random podcasts by mood type
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetRandomByMoodAsync(MoodType mood, int take = 10, CancellationToken cancellationToken = default)
    {
        var podcasts = await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                                 (p.PrimaryMood == mood || p.MoodTags.Any(mt => mt.MoodType == mood)),
                                            p => p.Category, p => p.MoodTags)
            .ToListAsync(cancellationToken);

        // Randomize using GUID for simplicity (in production, consider better randomization)
        return podcasts.OrderBy(x => Guid.NewGuid()).Take(take);
    }

    /// <summary>
    /// Get podcasts by category
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(p => p.CategoryId == categoryId && p.Status == PodcastStatus.Published,
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get featured podcasts
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetFeaturedAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(p => p.IsFeatured && p.Status == PodcastStatus.Published,
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PublishedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get trending podcasts (most played recently)
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetTrendingAsync(int days = 7, int take = 10, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                         p.PlayHistory.Any(ph => ph.PlayedAt >= cutoffDate),
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PlayHistory.Count(ph => ph.PlayedAt >= cutoffDate))
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search podcasts by title, description, or tags
    /// </summary>
    public async Task<IEnumerable<Podcast>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                         (p.Title.ToLower().Contains(lowerSearchTerm) ||
                                          (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)) ||
                                          (p.Tags != null && p.Tags.ToLower().Contains(lowerSearchTerm)) ||
                                          (p.AuthorName != null && p.AuthorName.ToLower().Contains(lowerSearchTerm))),
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PlayCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get podcasts for recommendation based on user preferences
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetRecommendationsAsync(
        Guid userProfileId, 
        IEnumerable<MoodType> moods, 
        IEnumerable<ContentType> contentTypes,
        int? maxDurationMinutes = null,
        int? minDurationMinutes = null,
        bool excludeRecentlyPlayed = true,
        int excludeRecentDays = 7,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = FindByCondition(p => p.Status == PodcastStatus.Published,
                                  p => p.Category, p => p.MoodTags, p => p.PlayHistory);

        // Filter by moods
        if (moods.Any())
        {
            query = query.Where(p => moods.Contains(p.PrimaryMood) || 
                                   p.MoodTags.Any(mt => moods.Contains(mt.MoodType)));
        }

        // Filter by content types
        if (contentTypes.Any())
        {
            query = query.Where(p => contentTypes.Contains(p.Category.ContentType));
        }

        // Filter by duration
        if (maxDurationMinutes.HasValue)
        {
            query = query.Where(p => p.DurationSeconds <= maxDurationMinutes.Value * 60);
        }
        
        if (minDurationMinutes.HasValue)
        {
            query = query.Where(p => p.DurationSeconds >= minDurationMinutes.Value * 60);
        }

        // Exclude recently played
        if (excludeRecentlyPlayed)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-excludeRecentDays);
            query = query.Where(p => !p.PlayHistory.Any(ph => ph.UserProfileId == userProfileId && 
                                                             ph.PlayedAt >= cutoffDate));
        }

        return await query
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.PlayCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get podcasts by multiple mood types
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetByMoodsAsync(IEnumerable<MoodType> moods, int take = 10, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                         (moods.Contains(p.PrimaryMood) || 
                                          p.MoodTags.Any(mt => moods.Contains(mt.MoodType))),
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.PlayCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get podcasts with specific duration range
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetByDurationRangeAsync(int minSeconds, int maxSeconds, int take = 10, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(p => p.Status == PodcastStatus.Published &&
                                         p.DurationSeconds >= minSeconds &&
                                         p.DurationSeconds <= maxSeconds,
                                   p => p.Category, p => p.MoodTags)
            .OrderByDescending(p => p.AverageRating)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update podcast play count
    /// </summary>
    public async Task UpdatePlayCountAsync(Guid podcastId, CancellationToken cancellationToken = default)
    {
        var podcast = await GetByIdAsync(podcastId, cancellationToken);
        if (podcast != null)
        {
            podcast.PlayCount++;
            Update(podcast);
        }
    }

    /// <summary>
    /// Update podcast rating
    /// </summary>
    public async Task UpdateRatingAsync(Guid podcastId, decimal newRating, int ratingCount, CancellationToken cancellationToken = default)
    {
        var podcast = await GetByIdAsync(podcastId, cancellationToken);
        if (podcast != null)
        {
            podcast.AverageRating = newRating;
            podcast.RatingCount = ratingCount;
            Update(podcast);
        }
    }

    /// <summary>
    /// Get podcast statistics
    /// </summary>
    public async Task<(int totalPodcasts, int totalPlays, decimal averageRating)> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var podcasts = await FindByCondition(p => p.Status == PodcastStatus.Published)
            .ToListAsync(cancellationToken);

        var totalPodcasts = podcasts.Count;
        var totalPlays = podcasts.Sum(p => p.PlayCount);
        var averageRating = podcasts.Where(p => p.RatingCount > 0).Average(p => p.AverageRating);

        return (totalPodcasts, totalPlays, averageRating);
    }
}
