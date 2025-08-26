using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for podcast category management
/// </summary>
public interface IPodcastCategoryService
{
    /// <summary>
    /// Get all categories
    /// </summary>
    Task<Result<IEnumerable<PodcastCategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get featured categories
    /// </summary>
    Task<Result<IEnumerable<PodcastCategoryDto>>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<Result<PodcastCategoryDto>> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get category by slug
    /// </summary>
    Task<Result<PodcastCategoryDto>> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get categories by content type
    /// </summary>
    Task<Result<IEnumerable<PodcastCategoryDto>>> GetCategoriesByContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get categories by mood
    /// </summary>
    Task<Result<IEnumerable<PodcastCategoryDto>>> GetCategoriesByMoodAsync(MoodType mood, CancellationToken cancellationToken = default);
}
