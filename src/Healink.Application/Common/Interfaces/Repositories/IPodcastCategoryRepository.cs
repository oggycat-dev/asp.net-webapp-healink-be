using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for podcast category management
/// </summary>
public interface IPodcastCategoryRepository : IGenericRepository<PodcastCategory>
{
    /// <summary>
    /// Get all categories ordered by display order
    /// </summary>
    Task<IEnumerable<PodcastCategory>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get featured categories
    /// </summary>
    Task<IEnumerable<PodcastCategory>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get category by slug
    /// </summary>
    Task<PodcastCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get categories by content type
    /// </summary>
    Task<IEnumerable<PodcastCategory>> GetByContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get categories by mood type
    /// </summary>
    Task<IEnumerable<PodcastCategory>> GetByMoodAsync(MoodType mood, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update category podcast count
    /// </summary>
    Task UpdatePodcastCountAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update category total plays
    /// </summary>
    Task UpdateTotalPlaysAsync(Guid categoryId, int additionalPlays, CancellationToken cancellationToken = default);
}
