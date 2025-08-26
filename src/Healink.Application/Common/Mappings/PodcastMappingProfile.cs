using AutoMapper;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using System.Text.Json;

namespace Healink.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for podcast-related mappings
/// </summary>
public class PodcastMappingProfile : Profile
{
    public PodcastMappingProfile()
    {
        CreateMap<Podcast, PodcastDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => src.FormattedDuration))
            .ForMember(dest => dest.MoodTags, opt => opt.MapFrom(src => src.MoodTags.Select(mt => mt.MoodType)))
            .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentUserRating, opt => opt.Ignore())
            .ForMember(dest => dest.HasBeenPlayedByCurrentUser, opt => opt.Ignore())
            .ForMember(dest => dest.LastPlayedPosition, opt => opt.Ignore());

        CreateMap<PodcastCategory, PodcastCategoryDto>()
            .ForMember(dest => dest.SecondaryMoods, opt => opt.MapFrom(src => ParseSecondaryMoods(src.SecondaryMoods)));

        CreateMap<CreatePodcastRequest, Podcast>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.DurationSeconds, opt => opt.Ignore())
            .ForMember(dest => dest.FileSizeBytes, opt => opt.Ignore())
            .ForMember(dest => dest.AudioFormat, opt => opt.Ignore())
            .ForMember(dest => dest.PlayCount, opt => opt.Ignore())
            .ForMember(dest => dest.LikeCount, opt => opt.Ignore())
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.RatingCount, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.MoodTags, opt => opt.Ignore())
            .ForMember(dest => dest.PlayHistory, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        CreateMap<UserProfile, UserProfileDto>();

        CreateMap<PodcastPlayHistory, PodcastPlayHistoryDto>()
            .ForMember(dest => dest.PodcastTitle, opt => opt.MapFrom(src => src.Podcast.Title))
            .ForMember(dest => dest.PodcastCoverImageUrl, opt => opt.MapFrom(src => src.Podcast.CoverImageUrl))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Podcast.Category.Name));
    }

    private static List<MoodType> ParseSecondaryMoods(string? secondaryMoodsJson)
    {
        if (string.IsNullOrEmpty(secondaryMoodsJson))
            return new List<MoodType>();

        try
        {
            var moodValues = JsonSerializer.Deserialize<int[]>(secondaryMoodsJson);
            return moodValues?.Cast<MoodType>().ToList() ?? new List<MoodType>();
        }
        catch
        {
            return new List<MoodType>();
        }
    }
}

/// <summary>
/// DTO for user profile data
/// </summary>
public class UserProfileDto
{
    public Guid Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? TimeZone { get; set; }
    public string Language { get; set; } = string.Empty;
    public MoodType? CurrentMood { get; set; }
    public DateTime? MoodUpdatedAt { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool EnableMoodRecommendations { get; set; }
    public bool EnableDailyReminders { get; set; }
    public string? DailyReminderTime { get; set; }
    public int TotalListeningMinutes { get; set; }
    public int DiaryEntriesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for podcast play history
/// </summary>
public class PodcastPlayHistoryDto
{
    public Guid Id { get; set; }
    public Guid PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public string? PodcastCoverImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
    public int ListenedSeconds { get; set; }
    public int CompletionPercentage { get; set; }
    public MoodType? MoodAtPlay { get; set; }
    public MoodType? MoodAfterPlay { get; set; }
    public int? Rating { get; set; }
    public bool? IsLiked { get; set; }
    public bool IsCompleted { get; set; }
    public string? DiscoverySource { get; set; }
    public string? DeviceType { get; set; }
    public bool IsFromQrCode { get; set; }
    public Guid? QrBraceletId { get; set; }
}
