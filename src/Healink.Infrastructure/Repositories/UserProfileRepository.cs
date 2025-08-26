using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for user profile management
/// </summary>
public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(HealinkDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user profile by AppUser ID
    /// </summary>
    public async Task<UserProfile?> GetByAppUserIdAsync(string appUserId, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(u => u.AppUserId == appUserId && u.Status == EntityStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get user with mood preferences
    /// </summary>
    public async Task<UserProfile?> GetWithMoodPreferencesAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(u => u.Id == userProfileId && u.Status == EntityStatus.Active,
                                   u => u.MoodPreferences)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get user with play history
    /// </summary>
    public async Task<UserProfile?> GetWithPlayHistoryAsync(Guid userProfileId, int? lastNDays = null, CancellationToken cancellationToken = default)
    {
        var query = FindByCondition(u => u.Id == userProfileId && u.Status == EntityStatus.Active,
                                  u => u.PlayHistory);

        if (lastNDays.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-lastNDays.Value);
            query = query.Include(u => u.PlayHistory.Where(ph => ph.PlayedAt >= cutoffDate));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Update user's current mood
    /// </summary>
    public async Task UpdateCurrentMoodAsync(Guid userProfileId, MoodType mood, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userProfileId, cancellationToken);
        if (user != null)
        {
            user.CurrentMood = mood;
            user.MoodUpdatedAt = DateTime.UtcNow;
            Update(user);
        }
    }

    /// <summary>
    /// Update user's total listening time
    /// </summary>
    public async Task UpdateListeningTimeAsync(Guid userProfileId, int additionalMinutes, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userProfileId, cancellationToken);
        if (user != null)
        {
            user.TotalListeningMinutes += additionalMinutes;
            Update(user);
        }
    }

    /// <summary>
    /// Increment diary entries count
    /// </summary>
    public async Task IncrementDiaryEntriesCountAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userProfileId, cancellationToken);
        if (user != null)
        {
            user.DiaryEntriesCount++;
            Update(user);
        }
    }

    /// <summary>
    /// Get users by mood type (for analytics)
    /// </summary>
    public async Task<IEnumerable<UserProfile>> GetByCurrentMoodAsync(MoodType mood, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(u => u.CurrentMood == mood && u.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user listening statistics
    /// </summary>
    public async Task<(int totalMinutes, int totalPodcasts, MoodType? favoriteMood)> GetListeningStatsAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var user = await FindByCondition(u => u.Id == userProfileId && u.Status == EntityStatus.Active,
                                        u => u.PlayHistory)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return (0, 0, null);

        var totalMinutes = user.TotalListeningMinutes;
        var totalPodcasts = user.PlayHistory.Count();
        
        // Calculate favorite mood based on most played content
        var favoriteMood = user.PlayHistory
            .Where(ph => ph.MoodAtPlay.HasValue)
            .GroupBy(ph => ph.MoodAtPlay)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        return (totalMinutes, totalPodcasts, favoriteMood);
    }
}
