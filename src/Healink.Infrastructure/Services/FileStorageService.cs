using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using Healink.Application.Common.Interfaces.Services;

namespace Healink.Infrastructure.Services;

/// <summary>
/// AWS S3 file storage service implementation
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _bucketName;
    private readonly string _baseUrl;

    public FileStorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS_S3_BUCKET_NAME"] ?? "healink-files";
        _baseUrl = _configuration["STORAGE_BASE_URL"] ?? $"https://{_bucketName}.s3.amazonaws.com";
    }

    /// <summary>
    /// Upload podcast audio file to S3
    /// </summary>
    public async Task<(string url, long fileSize, int durationSeconds)> UploadPodcastAudioAsync(IFormFile file, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ValidateAudioFile(file))
            {
                throw new ArgumentException("Invalid audio file format");
            }

            var key = $"podcasts/audio/{fileName}";
            var duration = await GetAudioDurationAsync(file, cancellationToken);

            using var stream = file.OpenReadStream();
            
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata =
                {
                    ["original-name"] = file.FileName,
                    ["upload-date"] = DateTime.UtcNow.ToString("O"),
                    ["duration-seconds"] = duration.ToString()
                }
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Failed to upload file to S3");
            }

            var url = $"{_baseUrl}/{key}";
            _logger.LogInformation("Uploaded podcast audio {FileName} to S3: {Url}", fileName, url);

            return (url, file.Length, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading podcast audio {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Upload image file to S3
    /// </summary>
    public async Task<string> UploadImageAsync(IFormFile file, string fileName, string folder = "images", CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ValidateImageFile(file))
            {
                throw new ArgumentException("Invalid image file format");
            }

            var key = $"{folder}/{fileName}";

            using var stream = file.OpenReadStream();
            
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata =
                {
                    ["original-name"] = file.FileName,
                    ["upload-date"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Failed to upload image to S3");
            }

            var url = $"{_baseUrl}/{key}";
            _logger.LogInformation("Uploaded image {FileName} to S3: {Url}", fileName, url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Upload any file to S3
    /// </summary>
    public async Task<string> UploadFileAsync(IFormFile file, string fileName, string folder = "files", CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{folder}/{fileName}";

            using var stream = file.OpenReadStream();
            
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata =
                {
                    ["original-name"] = file.FileName,
                    ["upload-date"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Failed to upload file to S3");
            }

            var url = $"{_baseUrl}/{key}";
            _logger.LogInformation("Uploaded file {FileName} to S3: {Url}", fileName, url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Delete file from S3
    /// </summary>
    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request, cancellationToken);
            _logger.LogInformation("Deleted file from S3: {Key}", key);

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
            return false;
        }
    }

    /// <summary>
    /// Get file metadata
    /// </summary>
    public async Task<(bool exists, long fileSize, DateTime lastModified)> GetFileMetadataAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return (false, 0, DateTime.MinValue);
            }

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            
            return (true, response.ContentLength, response.LastModified);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (false, 0, DateTime.MinValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata {FileUrl}", fileUrl);
            return (false, 0, DateTime.MinValue);
        }
    }

    /// <summary>
    /// Generate presigned URL for direct upload
    /// </summary>
    public async Task<string> GeneratePresignedUploadUrlAsync(string fileName, string contentType, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";
            
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ContentType = contentType
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            _logger.LogInformation("Generated presigned upload URL for {FileName}", fileName);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned upload URL for {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Generate presigned URL for download
    /// </summary>
    public async Task<string> GeneratePresignedDownloadUrlAsync(string fileUrl, int expirationMinutes = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Invalid file URL");
            }

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned download URL for {FileUrl}", fileUrl);
            throw;
        }
    }

    /// <summary>
    /// Get audio file duration (simplified implementation)
    /// </summary>
    public async Task<int> GetAudioDurationAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation
        // In production, you'd use a library like NAudio, FFMpegCore, or MediaInfo to get actual duration
        await Task.CompletedTask;
        
        // For now, estimate based on file size (very rough approximation)
        // 1MB ≈ 60 seconds for average quality MP3
        var estimatedDuration = (int)(file.Length / 1024 / 1024 * 60);
        
        return Math.Max(estimatedDuration, 30); // Minimum 30 seconds
    }

    /// <summary>
    /// Validate audio file format
    /// </summary>
    public bool ValidateAudioFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var allowedTypes = new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/mp4", "audio/aac" };
        var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac" };

        var isValidType = allowedTypes.Contains(file.ContentType.ToLower());
        var extension = Path.GetExtension(file.FileName).ToLower();
        var isValidExtension = allowedExtensions.Contains(extension);

        return isValidType && isValidExtension;
    }

    /// <summary>
    /// Validate image file format
    /// </summary>
    public bool ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var isValidType = allowedTypes.Contains(file.ContentType.ToLower());
        var extension = Path.GetExtension(file.FileName).ToLower();
        var isValidExtension = allowedExtensions.Contains(extension);

        return isValidType && isValidExtension;
    }

    /// <summary>
    /// Extract S3 key from full URL
    /// </summary>
    private string ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            return uri.AbsolutePath.TrimStart('/');
        }
        catch
        {
            return string.Empty;
        }
    }
}
