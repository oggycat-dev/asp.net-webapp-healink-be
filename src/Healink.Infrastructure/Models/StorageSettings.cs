namespace Healink.Infrastructure.Models;

/// <summary>
/// Storage settings configuration
/// </summary>
public class StorageSettings
{
    public string ProviderType { get; set; } = "LocalStorage";
    public string BaseUrl { get; set; } = string.Empty;
    public LocalStorageSettings LocalStorage { get; set; } = new();
    public AmazonS3Settings AmazonS3 { get; set; } = new();
    public AzureBlobSettings AzureBlob { get; set; } = new();
}

/// <summary>
/// Local storage specific settings
/// </summary>
public class LocalStorageSettings
{
    public string RootPath { get; set; } = "wwwroot/uploads";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".epub" };
}

/// <summary>
/// Amazon S3 specific settings
/// </summary>
public class AmazonS3Settings
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public bool UseHttps { get; set; } = true;
    public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".epub" };
}

/// <summary>
/// Azure Blob storage specific settings
/// </summary>
public class AzureBlobSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".epub" };
} 