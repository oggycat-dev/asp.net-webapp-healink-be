using AutoMapper;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;

namespace Healink.Infrastructure.Services;

/// <summary>
/// Service implementation for podcast category management
/// </summary>
public class PodcastCategoryService : IPodcastCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PodcastCategoryService> _logger;

    public PodcastCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PodcastCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<Result<IEnumerable<PodcastCategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.PodcastCategoryRepository.GetAllOrderedAsync(cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<PodcastCategoryDto>>(categories);
            
            return Result<IEnumerable<PodcastCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all podcast categories");
            return Result<IEnumerable<PodcastCategoryDto>>.Failure("An error occurred while retrieving categories", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get featured categories
    /// </summary>
    public async Task<Result<IEnumerable<PodcastCategoryDto>>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.PodcastCategoryRepository.GetFeaturedAsync(cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<PodcastCategoryDto>>(categories);
            
            return Result<IEnumerable<PodcastCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured podcast categories");
            return Result<IEnumerable<PodcastCategoryDto>>.Failure("An error occurred while retrieving featured categories", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    public async Task<Result<PodcastCategoryDto>> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.PodcastCategoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return Result<PodcastCategoryDto>.Failure("Category not found", ErrorCode.NotFound, 404);
            }

            var categoryDto = _mapper.Map<PodcastCategoryDto>(category);
            return Result<PodcastCategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast category {CategoryId}", id);
            return Result<PodcastCategoryDto>.Failure("An error occurred while retrieving the category", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    public async Task<Result<PodcastCategoryDto>> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.PodcastCategoryRepository.GetBySlugAsync(slug, cancellationToken);
            if (category == null)
            {
                return Result<PodcastCategoryDto>.Failure("Category not found", ErrorCode.NotFound, 404);
            }

            var categoryDto = _mapper.Map<PodcastCategoryDto>(category);
            return Result<PodcastCategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast category by slug {Slug}", slug);
            return Result<PodcastCategoryDto>.Failure("An error occurred while retrieving the category", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get categories by content type
    /// </summary>
    public async Task<Result<IEnumerable<PodcastCategoryDto>>> GetCategoriesByContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.PodcastCategoryRepository.GetByContentTypeAsync(contentType, cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<PodcastCategoryDto>>(categories);
            
            return Result<IEnumerable<PodcastCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories by content type {ContentType}", contentType);
            return Result<IEnumerable<PodcastCategoryDto>>.Failure("An error occurred while retrieving categories by content type", ErrorCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get categories by mood
    /// </summary>
    public async Task<Result<IEnumerable<PodcastCategoryDto>>> GetCategoriesByMoodAsync(MoodType mood, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.PodcastCategoryRepository.GetByMoodAsync(mood, cancellationToken);
            var categoryDtos = _mapper.Map<IEnumerable<PodcastCategoryDto>>(categories);
            
            return Result<IEnumerable<PodcastCategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories by mood {Mood}", mood);
            return Result<IEnumerable<PodcastCategoryDto>>.Failure("An error occurred while retrieving categories by mood", ErrorCode.InternalServerError);
        }
    }
}
