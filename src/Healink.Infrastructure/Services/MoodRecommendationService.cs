using Microsoft.Extensions.Logging;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Services;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Services;

/// <summary>
/// Service implementation for mood-based podcast recommendations
/// </summary>
public class MoodRecommendationService : IMoodRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MoodRecommendationService> _logger;

    // Mood compatibility matrix for transitions
    private readonly Dictionary<MoodType, Dictionary<MoodType, double>> _moodTransitionMatrix;

    public MoodRecommendationService(
        IUnitOfWork unitOfWork,
        ILogger<MoodRecommendationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _moodTransitionMatrix = InitializeMoodTransitionMatrix();
    }

    /// <summary>
    /// Get podcast recommendations based on user's current mood
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetMoodBasedRecommendationsAsync(
        Guid userProfileId, 
        MoodType currentMood, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mood-based recommendations for user {UserId} with mood {Mood}", userProfileId, currentMood);

            // Get user's preferences for this mood
            var userProfile = await _unitOfWork.UserProfileRepository.GetWithMoodPreferencesAsync(userProfileId, cancellationToken);
            
            var moodPreference = userProfile?.MoodPreferences?.FirstOrDefault(mp => mp.MoodType == currentMood && mp.IsEnabled);
            
            // Get direct mood matches
            var directMatches = await _unitOfWork.PodcastRepository.GetByMoodAsync(currentMood, count, cancellationToken);
            
            // Get complementary moods for this current mood
            var complementaryMoods = GetComplementaryMoods(currentMood);
            var complementaryMatches = await _unitOfWork.PodcastRepository.GetByMoodsAsync(complementaryMoods, count / 2, cancellationToken);
            
            // Combine and score
            var allPodcasts = directMatches.Concat(complementaryMatches).Distinct();
            
            var scoredPodcasts = new List<(Podcast podcast, double score)>();
            
            foreach (var podcast in allPodcasts)
            {
                var score = await CalculateMoodCompatibilityAsync(userProfileId, podcast.Id, currentMood, cancellationToken);
                scoredPodcasts.Add((podcast, score));
            }
            
            // Return top scored podcasts
            return scoredPodcasts
                .OrderByDescending(x => x.score)
                .Take(count)
                .Select(x => x.podcast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mood-based recommendations for user {UserId}", userProfileId);
            return Enumerable.Empty<Podcast>();
        }
    }

    /// <summary>
    /// Get personalized recommendations based on user's listening history
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetPersonalizedRecommendationsAsync(
        Guid userProfileId, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userProfile = await _unitOfWork.UserProfileRepository.GetWithPlayHistoryAsync(userProfileId, 30, cancellationToken);
            if (userProfile?.PlayHistory?.Any() != true)
            {
                // New user, return trending podcasts
                return await _unitOfWork.PodcastRepository.GetTrendingAsync(7, count, cancellationToken);
            }

            // Analyze listening patterns
            var moodPatterns = await _unitOfWork.PodcastPlayHistoryRepository.GetMoodListeningPatternsAsync(userProfileId, 30, cancellationToken);
            var timePatterns = await _unitOfWork.PodcastPlayHistoryRepository.GetTimeListeningPatternsAsync(userProfileId, 30, cancellationToken);

            // Get current time of day preference
            var currentHour = DateTime.Now.Hour;
            var preferredMoods = GetMoodsForTimeOfDay(currentHour, moodPatterns);

            // Get recommendations based on preferred moods
            var recommendations = await _unitOfWork.PodcastRepository.GetRecommendationsAsync(
                userProfileId,
                preferredMoods,
                new List<ContentType>(),
                null, null, true, 7, count,
                cancellationToken);

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations for user {UserId}", userProfileId);
            return Enumerable.Empty<Podcast>();
        }
    }

    /// <summary>
    /// Analyze user's mood from diary content (basic implementation)
    /// </summary>
    public async Task<MoodType?> AnalyzeMoodFromTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            var lowerText = text.ToLower();
            
            // Simple keyword-based mood detection (in production, use ML/NLP)
            var moodKeywords = new Dictionary<MoodType, string[]>
            {
                [MoodType.Happy] = new[] { "happy", "joy", "excited", "great", "wonderful", "amazing", "fantastic", "good" },
                [MoodType.Sad] = new[] { "sad", "down", "depressed", "upset", "cry", "tears", "blue", "lonely" },
                [MoodType.Anxious] = new[] { "anxious", "worried", "nervous", "stress", "panic", "fear", "concern" },
                [MoodType.Calm] = new[] { "calm", "peaceful", "relaxed", "tranquil", "serene", "quiet" },
                [MoodType.Energetic] = new[] { "energetic", "active", "pump", "motivated", "driven", "productive" },
                [MoodType.Grateful] = new[] { "grateful", "thankful", "blessed", "appreciate", "lucky" },
                [MoodType.Frustrated] = new[] { "frustrated", "angry", "mad", "annoyed", "irritated", "upset" },
                [MoodType.Hopeful] = new[] { "hopeful", "optimistic", "positive", "looking forward", "excited about" }
            };

            var moodScores = new Dictionary<MoodType, int>();

            foreach (var mood in moodKeywords)
            {
                var score = mood.Value.Count(keyword => lowerText.Contains(keyword));
                if (score > 0)
                {
                    moodScores[mood.Key] = score;
                }
            }

            if (moodScores.Any())
            {
                return moodScores.OrderByDescending(x => x.Value).First().Key;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing mood from text");
            return null;
        }
    }

    /// <summary>
    /// Get mood transition recommendations (from current mood to desired mood)
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetMoodTransitionRecommendationsAsync(
        MoodType fromMood, 
        MoodType toMood, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transitionMoods = GetTransitionMoods(fromMood, toMood);
            var podcasts = await _unitOfWork.PodcastRepository.GetByMoodsAsync(transitionMoods, count, cancellationToken);
            
            return podcasts.OrderBy(p => GetMoodTransitionScore(fromMood, toMood, p.PrimaryMood));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mood transition recommendations from {FromMood} to {ToMood}", fromMood, toMood);
            return Enumerable.Empty<Podcast>();
        }
    }

    /// <summary>
    /// Update user's mood preferences based on listening behavior
    /// </summary>
    public async Task UpdateUserMoodPreferencesAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var moodPatterns = await _unitOfWork.PodcastPlayHistoryRepository.GetMoodListeningPatternsAsync(userProfileId, 90, cancellationToken);
            
            foreach (var pattern in moodPatterns)
            {
                var preference = new UserMoodPreference
                {
                    UserProfileId = userProfileId,
                    MoodType = pattern.Key,
                    PreferenceWeight = Math.Min(10, pattern.Value / 5), // Scale based on listening frequency
                    IsEnabled = true,
                    UsageCount = pattern.Value,
                    LastUsedAt = DateTime.UtcNow
                };

                // This would typically check if preference exists and update accordingly
                await ((HealinkDbContext)_unitOfWork.BusinessContext).Set<UserMoodPreference>().AddAsync(preference, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mood preferences for user {UserId}", userProfileId);
        }
    }

    /// <summary>
    /// Get mood-based content categories
    /// </summary>
    public async Task<Dictionary<MoodType, List<ContentType>>> GetMoodContentMappingAsync(CancellationToken cancellationToken = default)
    {
        return new Dictionary<MoodType, List<ContentType>>
        {
            [MoodType.Happy] = new List<ContentType> { ContentType.Motivation, ContentType.PersonalGrowth },
            [MoodType.Sad] = new List<ContentType> { ContentType.SelfCare, ContentType.Meditation },
            [MoodType.Anxious] = new List<ContentType> { ContentType.Anxiety, ContentType.StressManagement, ContentType.Meditation },
            [MoodType.Calm] = new List<ContentType> { ContentType.Meditation, ContentType.Sleep, ContentType.Spiritual },
            [MoodType.Energetic] = new List<ContentType> { ContentType.Motivation, ContentType.PersonalGrowth },
            [MoodType.Peaceful] = new List<ContentType> { ContentType.Meditation, ContentType.Spiritual, ContentType.Sleep },
            [MoodType.Motivated] = new List<ContentType> { ContentType.Motivation, ContentType.PersonalGrowth },
            [MoodType.Reflective] = new List<ContentType> { ContentType.Spiritual, ContentType.PersonalGrowth },
            [MoodType.Stressed] = new List<ContentType> { ContentType.StressManagement, ContentType.Meditation, ContentType.SelfCare },
            [MoodType.Grateful] = new List<ContentType> { ContentType.Spiritual, ContentType.SelfCare },
            [MoodType.Hopeful] = new List<ContentType> { ContentType.Motivation, ContentType.PersonalGrowth },
            [MoodType.Frustrated] = new List<ContentType> { ContentType.StressManagement, ContentType.SelfCare }
        };
    }

    /// <summary>
    /// Calculate mood compatibility score between user and podcast
    /// </summary>
    public async Task<double> CalculateMoodCompatibilityAsync(
        Guid userProfileId, 
        Guid podcastId, 
        MoodType? currentMood = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var podcast = await _unitOfWork.PodcastRepository.GetByIdAsync(podcastId, cancellationToken);
            if (podcast == null) return 0;

            var userProfile = await _unitOfWork.UserProfileRepository.GetWithMoodPreferencesAsync(userProfileId, cancellationToken);
            var mood = currentMood ?? userProfile?.CurrentMood ?? MoodType.Calm;

            double score = 0;

            // Direct mood match
            if (podcast.PrimaryMood == mood)
                score += 1.0;

            // Mood tag matches
            var moodTags = podcast.MoodTags?.Where(mt => mt.MoodType == mood);
            if (moodTags?.Any() == true)
            {
                score += 0.5 + (moodTags.Sum(mt => mt.Weight) / 10.0);
            }

            // User preferences
            if (userProfile?.MoodPreferences?.Any() == true)
            {
                var preference = userProfile.MoodPreferences.FirstOrDefault(mp => mp.MoodType == mood);
                if (preference != null)
                {
                    score += preference.PreferenceWeight / 10.0;
                }
            }

            // Quality factors
            score += (double)podcast.AverageRating / 5.0; // 0-1 based on rating
            score += Math.Log10(podcast.PlayCount + 1) / 10.0; // Popularity factor

            return Math.Min(score, 10.0); // Cap at 10
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating mood compatibility for user {UserId} and podcast {PodcastId}", userProfileId, podcastId);
            return 0;
        }
    }

    /// <summary>
    /// Get time-based mood recommendations (considering time of day)
    /// </summary>
    public async Task<IEnumerable<Podcast>> GetTimeBasedRecommendationsAsync(
        Guid userProfileId, 
        TimeOnly currentTime, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var hour = currentTime.Hour;
            var recommendedMoods = GetMoodsForTimeOfDay(hour);

            return await _unitOfWork.PodcastRepository.GetByMoodsAsync(recommendedMoods, count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time-based recommendations for user {UserId}", userProfileId);
            return Enumerable.Empty<Podcast>();
        }
    }

    #region Private Helper Methods

    private Dictionary<MoodType, Dictionary<MoodType, double>> InitializeMoodTransitionMatrix()
    {
        // Simplified mood transition compatibility matrix
        return new Dictionary<MoodType, Dictionary<MoodType, double>>
        {
            [MoodType.Sad] = new Dictionary<MoodType, double>
            {
                [MoodType.Calm] = 0.9,
                [MoodType.Peaceful] = 0.8,
                [MoodType.Hopeful] = 0.7,
                [MoodType.Grateful] = 0.6
            },
            [MoodType.Anxious] = new Dictionary<MoodType, double>
            {
                [MoodType.Calm] = 1.0,
                [MoodType.Peaceful] = 0.9,
                [MoodType.Reflective] = 0.6
            },
            [MoodType.Frustrated] = new Dictionary<MoodType, double>
            {
                [MoodType.Calm] = 0.8,
                [MoodType.Peaceful] = 0.7,
                [MoodType.Reflective] = 0.6
            }
        };
    }

    private List<MoodType> GetComplementaryMoods(MoodType currentMood)
    {
        return currentMood switch
        {
            MoodType.Sad => new List<MoodType> { MoodType.Hopeful, MoodType.Grateful, MoodType.Calm },
            MoodType.Anxious => new List<MoodType> { MoodType.Calm, MoodType.Peaceful, MoodType.Reflective },
            MoodType.Frustrated => new List<MoodType> { MoodType.Calm, MoodType.Peaceful },
            MoodType.Happy => new List<MoodType> { MoodType.Energetic, MoodType.Motivated, MoodType.Grateful },
            _ => new List<MoodType> { MoodType.Calm, MoodType.Peaceful }
        };
    }

    private List<MoodType> GetTransitionMoods(MoodType fromMood, MoodType toMood)
    {
        if (_moodTransitionMatrix.ContainsKey(fromMood))
        {
            var transitions = _moodTransitionMatrix[fromMood];
            return transitions.Keys.Append(toMood).ToList();
        }

        return new List<MoodType> { fromMood, toMood };
    }

    private double GetMoodTransitionScore(MoodType fromMood, MoodType toMood, MoodType podcastMood)
    {
        if (podcastMood == toMood) return 1.0;
        if (podcastMood == fromMood) return 0.5;

        if (_moodTransitionMatrix.ContainsKey(fromMood) && 
            _moodTransitionMatrix[fromMood].ContainsKey(podcastMood))
        {
            return _moodTransitionMatrix[fromMood][podcastMood];
        }

        return 0.1;
    }

    private List<MoodType> GetMoodsForTimeOfDay(int hour, Dictionary<MoodType, int>? userPatterns = null)
    {
        // Morning (6-11): Energetic, Motivated
        if (hour >= 6 && hour < 12)
            return new List<MoodType> { MoodType.Energetic, MoodType.Motivated, MoodType.Happy };

        // Afternoon (12-17): Focused, Calm
        if (hour >= 12 && hour < 18)
            return new List<MoodType> { MoodType.Calm, MoodType.Reflective, MoodType.Motivated };

        // Evening (18-22): Relaxing, Peaceful
        if (hour >= 18 && hour < 23)
            return new List<MoodType> { MoodType.Peaceful, MoodType.Calm, MoodType.Grateful };

        // Night (23-5): Sleep, Meditation
        return new List<MoodType> { MoodType.Peaceful, MoodType.Calm };
    }

    #endregion
}
