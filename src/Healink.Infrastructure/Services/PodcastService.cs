using AutoMapper;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.Models;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Services;

/// <summary>
/// Service implementation for podcast management
/// </summary>
public class PodcastService : IPodcastService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PodcastService> _logger;

    public PodcastService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PodcastService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get podcast by ID
    /// </summary>
    public async Task<Result<PodcastDto>> GetPodcastAsync(Guid id, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcast = await _unitOfWork.PodcastRepository.GetByIdAsync(id, cancellationToken);
            if (podcast == null)
            {
                return Result<PodcastDto>.Failure("Podcast not found", ErrorCode.NotFound, 404);
            }

            var podcastDto = _mapper.Map<PodcastDto>(podcast);
            
            // Add user-specific data if user is authenticated
            if (currentUserId.HasValue)
            {
                await EnrichWithUserDataAsync(podcastDto, currentUserId.Value, cancellationToken);
            }

            return Result<PodcastDto>.Success(podcastDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast {PodcastId}", id);
            return Result<PodcastDto>.Failure("An error occurred while retrieving the podcast", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get all podcasts with pagination
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetAllPodcastsAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetPagedAsync(page, pageSize, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all podcasts");
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving podcasts", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get podcasts by mood
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetPodcastsByMoodAsync(MoodType mood, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetByMoodAsync(mood, take, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts by mood {Mood}", mood);
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving podcasts by mood", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get random podcasts by mood
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetRandomPodcastsByMoodAsync(MoodType mood, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetRandomByMoodAsync(mood, take, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random podcasts by mood {Mood}", mood);
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving random podcasts", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get mood-based recommendations for user
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetRecommendationsAsync(PodcastRecommendationRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userProfile = await _unitOfWork.UserProfileRepository.GetWithMoodPreferencesAsync(currentUserId, cancellationToken);
            if (userProfile == null)
            {
                return Result<IEnumerable<PodcastDto>>.Failure("User profile not found", ErrorCode.NotFound, 404);
            }

            // If no current mood specified, use user's stored mood or preferences
            var moods = request.PreferredMoods?.Any() == true 
                ? request.PreferredMoods 
                : request.CurrentMood.HasValue 
                    ? new List<MoodType> { request.CurrentMood.Value }
                    : userProfile.CurrentMood.HasValue 
                        ? new List<MoodType> { userProfile.CurrentMood.Value }
                        : new List<MoodType> { MoodType.Calm }; // Default fallback

            var podcasts = await _unitOfWork.PodcastRepository.GetRecommendationsAsync(
                currentUserId, 
                moods, 
                request.PreferredContentTypes,
                request.MaxDurationMinutes,
                request.MinDurationMinutes,
                request.ExcludeRecentlyPlayed,
                request.ExcludeRecentDays,
                request.Count,
                cancellationToken);

            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            foreach (var dto in podcastDtos)
            {
                await EnrichWithUserDataAsync(dto, currentUserId, cancellationToken);
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations for user {UserId}", currentUserId);
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while getting recommendations", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get featured podcasts
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetFeaturedPodcastsAsync(int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetFeaturedAsync(take, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured podcasts");
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving featured podcasts", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get trending podcasts
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetTrendingPodcastsAsync(int days = 7, int take = 10, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetTrendingAsync(days, take, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending podcasts");
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving trending podcasts", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Search podcasts
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> SearchPodcastsAsync(string searchTerm, int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.SearchAsync(searchTerm, page, pageSize, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching podcasts with term {SearchTerm}", searchTerm);
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while searching podcasts", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get podcasts by category
    /// </summary>
    public async Task<Result<IEnumerable<PodcastDto>>> GetPodcastsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, Guid? currentUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var podcasts = await _unitOfWork.PodcastRepository.GetByCategoryAsync(categoryId, page, pageSize, cancellationToken);
            var podcastDtos = _mapper.Map<IEnumerable<PodcastDto>>(podcasts);

            if (currentUserId.HasValue)
            {
                foreach (var dto in podcastDtos)
                {
                    await EnrichWithUserDataAsync(dto, currentUserId.Value, cancellationToken);
                }
            }

            return Result<IEnumerable<PodcastDto>>.Success(podcastDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts by category {CategoryId}", categoryId);
            return Result<IEnumerable<PodcastDto>>.Failure("An error occurred while retrieving podcasts by category", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Create new podcast
    /// </summary>
    public async Task<Result<PodcastDto>> CreatePodcastAsync(CreatePodcastRequest request, string audioUrl, string? coverImageUrl, int durationSeconds, long fileSizeBytes, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var podcast = new Podcast
            {
                Title = request.Title,
                Description = request.Description,
                AudioUrl = audioUrl,
                CoverImageUrl = coverImageUrl,
                DurationSeconds = durationSeconds,
                FileSizeBytes = fileSizeBytes,
                CategoryId = request.CategoryId,
                PrimaryMood = request.PrimaryMood,
                AuthorName = request.AuthorName,
                NarratorName = request.NarratorName,
                Language = request.Language,
                Transcript = request.Transcript,
                Tags = request.Tags,
                IsFeatured = request.IsFeatured,
                Status = PodcastStatus.Draft,
                CreatedBy = createdBy
            };

            await _unitOfWork.PodcastRepository.AddAsync(podcast, cancellationToken);

            // Add mood tags
            foreach (var mood in request.AdditionalMoods)
            {
                var moodTag = new PodcastMoodTag
                {
                    PodcastId = podcast.Id,
                    MoodType = mood,
                    Weight = 5,
                    IsPrimary = false,
                    TaggedBy = "system"
                };
                await ((HealinkDbContext)_unitOfWork.BusinessContext).Set<PodcastMoodTag>().AddAsync(moodTag, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var podcastDto = _mapper.Map<PodcastDto>(podcast);
            return Result<PodcastDto>.Success(podcastDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating podcast {Title}", request.Title);
            return Result<PodcastDto>.Failure("An error occurred while creating the podcast", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Record podcast play
    /// </summary>
    public async Task<Result<Guid>> RecordPlayAsync(PodcastPlayRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(currentUserId, cancellationToken);
            if (userProfile == null)
            {
                return Result<Guid>.Failure("User profile not found", ErrorCode.NotFound, 404);
            }

            var podcast = await _unitOfWork.PodcastRepository.GetByIdAsync(request.PodcastId, cancellationToken);
            if (podcast == null)
            {
                return Result<Guid>.Failure("Podcast not found", ErrorCode.NotFound, 404);
            }

            var playHistory = new PodcastPlayHistory
            {
                UserProfileId = currentUserId,
                PodcastId = request.PodcastId,
                PlayedAt = DateTime.UtcNow,
                MoodAtPlay = request.MoodAtPlay,
                DiscoverySource = request.DiscoverySource,
                DeviceType = request.DeviceType,
                IsFromQrCode = request.IsFromQrCode,
                QrBraceletId = request.QrBraceletId,
                ListenedSeconds = request.StartPosition
            };

            await _unitOfWork.PodcastPlayHistoryRepository.AddAsync(playHistory, cancellationToken);

            // Update podcast play count
            await _unitOfWork.PodcastRepository.UpdatePlayCountAsync(request.PodcastId, cancellationToken);

            // Update category total plays
            await _unitOfWork.PodcastCategoryRepository.UpdateTotalPlaysAsync(podcast.CategoryId, 1, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<Guid>.Success(playHistory.Id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error recording podcast play for podcast {PodcastId}", request.PodcastId);
            return Result<Guid>.Failure("An error occurred while recording podcast play", ErrorCode.InternalServerError);
        }
    }

    // Additional methods would be implemented here following the same pattern...
    // UpdatePodcastAsync, DeletePodcastAsync, UpdatePlayProgressAsync, etc.

    /// <summary>
    /// Helper method to enrich podcast DTO with user-specific data
    /// </summary>
    private async Task EnrichWithUserDataAsync(PodcastDto podcastDto, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var hasPlayed = await _unitOfWork.PodcastPlayHistoryRepository.HasUserPlayedPodcastAsync(userId, podcastDto.Id, cancellationToken);
            podcastDto.HasBeenPlayedByCurrentUser = hasPlayed;

            if (hasPlayed)
            {
                var lastPosition = await _unitOfWork.PodcastPlayHistoryRepository.GetLastPlayPositionAsync(userId, podcastDto.Id, cancellationToken);
                podcastDto.LastPlayedPosition = lastPosition;
            }

            // Additional user-specific enrichment could be added here
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enriching podcast {PodcastId} with user data for user {UserId}", podcastDto.Id, userId);
            // Don't fail the main operation for enrichment errors
        }
    }

    public Task<Result<PodcastDto>> UpdatePodcastAsync(Guid id, UpdatePodcastRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeletePodcastAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdatePlayProgressAsync(PodcastPlayUpdateRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<PodcastDto>>> GetUserPlayHistoryAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<PodcastDto>>> GetRecentlyPlayedAsync(Guid userId, int days = 7, int take = 10, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<PodcastDto>>> GetUserFavoritesAsync(Guid userId, int take = 10, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
