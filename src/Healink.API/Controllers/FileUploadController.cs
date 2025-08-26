using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.Interfaces.Services;
using Healink.Application.Common.DTOs.Podcast;
using Healink.Application.Common.Models;
using Healink.API.Extensions;
using Healink.Domain.Enums;

namespace Healink.API.Controllers;

/// <summary>
/// File upload controller for podcasts and images
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IPodcastService _podcastService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileStorageService fileStorageService,
        IPodcastService podcastService,
        ILogger<FileUploadController> logger)
    {
        _fileStorageService = fileStorageService;
        _podcastService = podcastService;
        _logger = logger;
    }

    /// <summary>
    /// Upload podcast with audio file and optional cover image
    /// </summary>
    [HttpPost("podcast")]
    [RequestSizeLimit(500_000_000)] // 500MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPodcast([FromForm] PodcastUploadRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            // Validate audio file
            if (!_fileStorageService.ValidateAudioFile(request.AudioFile))
            {
                return this.BadRequest(ErrorCode.BadRequest, "Invalid audio file format. Supported formats: MP3, WAV, M4A, AAC");
            }

            // Validate cover image if provided
            if (request.CoverImage != null && !_fileStorageService.ValidateImageFile(request.CoverImage))
            {
                return this.BadRequest(ErrorCode.BadRequest, "Invalid image file format. Supported formats: JPG, PNG, GIF, WEBP");
            }

            _logger.LogInformation("Starting podcast upload for user {UserId}: {Title}", currentUserId, request.Title);

            // Generate unique file names
            var audioFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.AudioFile.FileName)}";
            var coverImageFileName = request.CoverImage != null ? $"{Guid.NewGuid()}{Path.GetExtension(request.CoverImage.FileName)}" : null;

            // Upload audio file
            var (audioUrl, fileSize, duration) = await _fileStorageService.UploadPodcastAudioAsync(request.AudioFile, audioFileName);

            // Upload cover image if provided
            string? coverImageUrl = null;
            if (request.CoverImage != null && coverImageFileName != null)
            {
                coverImageUrl = await _fileStorageService.UploadImageAsync(request.CoverImage, coverImageFileName, "podcasts/covers");
            }

            // Create podcast request from the upload request
            var createPodcastRequest = new CreatePodcastRequest
            {
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                PrimaryMood = request.PrimaryMood,
                AuthorName = request.AuthorName,
                NarratorName = request.NarratorName,
                Language = request.Language,
                Transcript = request.Transcript,
                Tags = request.Tags,
                AdditionalMoods = request.AdditionalMoods,
                IsFeatured = request.IsFeatured
            };

            // Create podcast
            var result = await _podcastService.CreatePodcastAsync(
                createPodcastRequest, 
                audioUrl, 
                coverImageUrl, 
                duration, 
                fileSize, 
                currentUserId.Value);

            if (!result.IsSuccess)
            {
                // Cleanup uploaded files if podcast creation failed
                await _fileStorageService.DeleteFileAsync(audioUrl);
                if (coverImageUrl != null)
                {
                    await _fileStorageService.DeleteFileAsync(coverImageUrl);
                }

                return this.FromResult(result);
            }

            _logger.LogInformation("Successfully uploaded podcast {PodcastId} for user {UserId}", result.Data!.Id, currentUserId);

            return this.FromResult(result, "Podcast uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading podcast {Title}", request.Title);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Upload cover image for existing podcast
    /// </summary>
    [HttpPost("podcast/{podcastId:guid}/cover")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPodcastCover(Guid podcastId, [FromForm] PodcastCoverUploadRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return this.Unauthorized("User ID is required");

            // Validate image file
            if (!_fileStorageService.ValidateImageFile(request.CoverImage))
            {
                return this.BadRequest(ErrorCode.BadRequest, "Invalid image file format. Supported formats: JPG, PNG, GIF, WEBP");
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.CoverImage.FileName)}";

            // Upload image
            var imageUrl = await _fileStorageService.UploadImageAsync(request.CoverImage, fileName, "podcasts/covers");

            // Update podcast with new cover image
            var updateRequest = new UpdatePodcastRequest { };
            // Note: You would need to add CoverImageUrl to UpdatePodcastRequest
            // var result = await _podcastService.UpdatePodcastAsync(podcastId, updateRequest, currentUserId.Value);

            _logger.LogInformation("Successfully uploaded cover image for podcast {PodcastId}", podcastId);

            return this.Success(new { ImageUrl = imageUrl }, "Cover image uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading cover image for podcast {PodcastId}", podcastId);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Upload general image file
    /// </summary>
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
    {
        try
        {
            // Validate image file
            if (!_fileStorageService.ValidateImageFile(request.ImageFile))
            {
                return this.BadRequest(ErrorCode.BadRequest, "Invalid image file format. Supported formats: JPG, PNG, GIF, WEBP");
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";

            // Upload image
            var imageUrl = await _fileStorageService.UploadImageAsync(request.ImageFile, fileName, request.Folder ?? "images");

            _logger.LogInformation("Successfully uploaded image to {ImageUrl}", imageUrl);

            return this.Success(new { ImageUrl = imageUrl }, "Image uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", request.ImageFile.FileName);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get presigned upload URL for direct S3 upload
    /// </summary>
    [HttpPost("presigned-url")]
    public async Task<IActionResult> GetPresignedUploadUrl([FromBody] PresignedUrlRequest request)
    {
        try
        {
            var url = await _fileStorageService.GeneratePresignedUploadUrlAsync(
                request.FileName, 
                request.ContentType, 
                request.ExpirationMinutes);

            return this.Success(new { UploadUrl = url }, "Presigned URL generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for {FileName}", request.FileName);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get presigned download URL for file access
    /// </summary>
    [HttpPost("presigned-download")]
    public async Task<IActionResult> GetPresignedDownloadUrl([FromBody] PresignedDownloadRequest request)
    {
        try
        {
            var url = await _fileStorageService.GeneratePresignedDownloadUrlAsync(
                request.FileUrl, 
                request.ExpirationMinutes);

            return this.Success(new { DownloadUrl = url }, "Presigned download URL generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned download URL for {FileUrl}", request.FileUrl);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Delete file from storage
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
    {
        try
        {
            var success = await _fileStorageService.DeleteFileAsync(fileUrl);
            
            if (success)
            {
                _logger.LogInformation("Successfully deleted file {FileUrl}", fileUrl);
                return this.Success(true, "File deleted successfully");
            }
            else
            {
                return this.BadRequest(ErrorCode.BadRequest, "Failed to delete file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
            return this.InternalServerError();
        }
    }

    /// <summary>
    /// Get file metadata
    /// </summary>
    [HttpGet("metadata")]
    public async Task<IActionResult> GetFileMetadata([FromQuery] string fileUrl)
    {
        try
        {
            var (exists, fileSize, lastModified) = await _fileStorageService.GetFileMetadataAsync(fileUrl);
            
            if (!exists)
            {
                return this.NotFound("File not found");
            }

            var metadata = new
            {
                Exists = exists,
                FileSize = fileSize,
                LastModified = lastModified,
                FormattedFileSize = FormatFileSize(fileSize)
            };

            return this.Success(metadata, "File metadata retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata for {FileUrl}", fileUrl);
            return this.InternalServerError();
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("entity_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Format file size for human readability
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}

/// <summary>
/// Request model for presigned upload URL
/// </summary>
public class PresignedUrlRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Request model for presigned download URL
/// </summary>
public class PresignedDownloadRequest
{
    public string FileUrl { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Request model for podcast upload with files
/// </summary>
public class PodcastUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public MoodType PrimaryMood { get; set; }
    public string? AuthorName { get; set; }
    public string? NarratorName { get; set; }
    public string Language { get; set; } = "vi-VN";
    public string? Transcript { get; set; }
    public string? Tags { get; set; }
    public List<MoodType> AdditionalMoods { get; set; } = new();
    public bool IsFeatured { get; set; } = false;
    
    // File uploads
    public IFormFile AudioFile { get; set; } = null!;
    public IFormFile? CoverImage { get; set; }
}

/// <summary>
/// Request model for podcast cover image upload
/// </summary>
public class PodcastCoverUploadRequest
{
    public IFormFile CoverImage { get; set; } = null!;
}

/// <summary>
/// Request model for general image upload
/// </summary>
public class ImageUploadRequest
{
    public IFormFile ImageFile { get; set; } = null!;
    public string? Folder { get; set; }
}
