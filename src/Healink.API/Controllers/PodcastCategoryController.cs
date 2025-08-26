using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;
using Healink.API.Extensions;

namespace Healink.API.Controllers;

/// <summary>
/// Podcast category management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PodcastCategoryController : ControllerBase
{
    private readonly IPodcastCategoryService _categoryService;
    private readonly ILogger<PodcastCategoryController> _logger;

    public PodcastCategoryController(
        IPodcastCategoryService categoryService,
        ILogger<PodcastCategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all podcast categories
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast categories");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get featured categories
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedCategories()
    {
        try
        {
            var result = await _categoryService.GetFeaturedCategoriesAsync();
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured categories");
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            var result = await _categoryService.GetCategoryAsync(id);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", id);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(slug))
                return this.BadRequest(ErrorCode.BadRequest, "Category slug is required");

            var result = await _categoryService.GetCategoryBySlugAsync(slug);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get categories by content type
    /// </summary>
    [HttpGet("content-type/{contentType}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoriesByContentType(ContentType contentType)
    {
        try
        {
            var result = await _categoryService.GetCategoriesByContentTypeAsync(contentType);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories by content type {ContentType}", contentType);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get categories by mood
    /// </summary>
    [HttpGet("mood/{mood}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoriesByMood(MoodType mood)
    {
        try
        {
            var result = await _categoryService.GetCategoriesByMoodAsync(mood);
            return this.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories by mood {Mood}", mood);
            return this.InternalServerError();
        }
    }
}
