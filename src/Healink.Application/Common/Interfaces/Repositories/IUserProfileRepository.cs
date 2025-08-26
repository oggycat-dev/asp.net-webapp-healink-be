using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for user profile management
/// </summary>
public interface IUserProfileRepository : IGenericRepository<UserProfile>
{
    /// <summary>
    /// Get user profile by AppUser ID
    /// </summary>
    Task<UserProfile?> GetByAppUserIdAsync(string appUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user with mood preferences
    /// </summary>
    Task<UserProfile?> GetWithMoodPreferencesAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user with play history
    /// </summary>
    Task<UserProfile?> GetWithPlayHistoryAsync(Guid userProfileId, int? lastNDays = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update user's current mood
    /// </summary>
    Task UpdateCurrentMoodAsync(Guid userProfileId, MoodType mood, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update user's total listening time
    /// </summary>
    Task UpdateListeningTimeAsync(Guid userProfileId, int additionalMinutes, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Increment diary entries count
    /// </summary>
    Task IncrementDiaryEntriesCountAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get users by mood type (for analytics)
    /// </summary>
    Task<IEnumerable<UserProfile>> GetByCurrentMoodAsync(MoodType mood, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user listening statistics
    /// </summary>
    Task<(int totalMinutes, int totalPodcasts, MoodType? favoriteMood)> GetListeningStatsAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
