using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for mood-based podcast recommendations
/// </summary>
public interface IMoodRecommendationService
{
    /// <summary>
    /// Get podcast recommendations based on user's current mood
    /// </summary>
    Task<IEnumerable<Podcast>> GetMoodBasedRecommendationsAsync(
        Guid userProfileId, 
        MoodType currentMood, 
        int count = 10, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get personalized recommendations based on user's listening history
    /// </summary>
    Task<IEnumerable<Podcast>> GetPersonalizedRecommendationsAsync(
        Guid userProfileId, 
        int count = 10, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze user's mood from diary content
    /// </summary>
    Task<MoodType?> AnalyzeMoodFromTextAsync(string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get mood transition recommendations (from current mood to desired mood)
    /// </summary>
    Task<IEnumerable<Podcast>> GetMoodTransitionRecommendationsAsync(
        MoodType fromMood, 
        MoodType toMood, 
        int count = 10, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update user's mood preferences based on listening behavior
    /// </summary>
    Task UpdateUserMoodPreferencesAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get mood-based content categories
    /// </summary>
    Task<Dictionary<MoodType, List<ContentType>>> GetMoodContentMappingAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate mood compatibility score between user and podcast
    /// </summary>
    Task<double> CalculateMoodCompatibilityAsync(
        Guid userProfileId, 
        Guid podcastId, 
        MoodType? currentMood = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get time-based mood recommendations (considering time of day)
    /// </summary>
    Task<IEnumerable<Podcast>> GetTimeBasedRecommendationsAsync(
        Guid userProfileId, 
        TimeOnly currentTime, 
        int count = 10, 
        CancellationToken cancellationToken = default);
}
