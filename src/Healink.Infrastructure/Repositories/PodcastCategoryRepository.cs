using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for podcast category management
/// </summary>
public class PodcastCategoryRepository : GenericRepository<PodcastCategory>, IPodcastCategoryRepository
{
    public PodcastCategoryRepository(HealinkDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all categories ordered by display order
    /// </summary>
    public async Task<IEnumerable<PodcastCategory>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await FindByCondition(c => c.Status == EntityStatus.Active)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get featured categories
    /// </summary>
    public async Task<IEnumerable<PodcastCategory>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await FindByCondition(c => c.IsFeatured && c.Status == EntityStatus.Active)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    public async Task<PodcastCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(c => c.Slug == slug && c.Status == EntityStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get categories by content type
    /// </summary>
    public async Task<IEnumerable<PodcastCategory>> GetByContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(c => c.ContentType == contentType && c.Status == EntityStatus.Active)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get categories by mood type
    /// </summary>
    public async Task<IEnumerable<PodcastCategory>> GetByMoodAsync(MoodType mood, CancellationToken cancellationToken = default)
    {
        return await FindByCondition(c => c.PrimaryMood == mood && c.Status == EntityStatus.Active)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update category podcast count
    /// </summary>
    public async Task UpdatePodcastCountAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(categoryId, cancellationToken);
        if (category != null)
        {
            // Count published podcasts in this category
            var podcastCount = await FindByCondition(c => c.Id == categoryId)
                .SelectMany(c => c.Podcasts)
                .CountAsync(p => p.Status == PodcastStatus.Published, cancellationToken);
            
            category.PodcastCount = podcastCount;
            Update(category);
        }
    }

    /// <summary>
    /// Update category total plays
    /// </summary>
    public async Task UpdateTotalPlaysAsync(Guid categoryId, int additionalPlays, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(categoryId, cancellationToken);
        if (category != null)
        {
            category.TotalPlays += additionalPlays;
            Update(category);
        }
    }
}
