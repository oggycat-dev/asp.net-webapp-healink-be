using Microsoft.AspNetCore.Http;

namespace Healink.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for file storage operations (AWS S3)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload podcast audio file to S3
    /// </summary>
    Task<(string url, long fileSize, int durationSeconds)> UploadPodcastAudioAsync(IFormFile file, string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload image file to S3
    /// </summary>
    Task<string> UploadImageAsync(IFormFile file, string fileName, string folder = "images", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload any file to S3
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string fileName, string folder = "files", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete file from S3
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get file metadata
    /// </summary>
    Task<(bool exists, long fileSize, DateTime lastModified)> GetFileMetadataAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate presigned URL for direct upload
    /// </summary>
    Task<string> GeneratePresignedUploadUrlAsync(string fileName, string contentType, int expirationMinutes = 60, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate presigned URL for download
    /// </summary>
    Task<string> GeneratePresignedDownloadUrlAsync(string fileUrl, int expirationMinutes = 60, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get audio file duration
    /// </summary>
    Task<int> GetAudioDurationAsync(IFormFile file, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate audio file format
    /// </summary>
    bool ValidateAudioFile(IFormFile file);
    
    /// <summary>
    /// Validate image file format
    /// </summary>
    bool ValidateImageFile(IFormFile file);
}
