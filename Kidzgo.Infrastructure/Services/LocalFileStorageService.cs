using System.Security.Cryptography;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Services;

/// <summary>
/// Local file storage service - lưu file vào thư mục trên VPS
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly string _publicPathPrefix;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly long _maxFileSize;
    private readonly Dictionary<string, string[]> _allowedExtensions;
    private readonly Dictionary<string, string[]> _allowedMimeTypes;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        
        var fileStorageConfig = configuration.GetSection("FileStorage:Local");
        _basePath = fileStorageConfig["BasePath"] ?? "/var/www/kidzgo/storage";
        _baseUrl = fileStorageConfig["BaseUrl"] ?? "/storage";
        _publicPathPrefix = ResolvePublicPathPrefix(_baseUrl);
        
        // Max file sizes in bytes
        _maxFileSize = long.Parse(configuration["FileStorage:MaxFileSize:Default"] ?? "104857600"); // 100MB default
        
        _allowedExtensions = new Dictionary<string, string[]>
        {
            ["image"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" },
            ["video"] = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv", ".hevc" },
            ["audio"] = new[] { ".mp3", ".wav" },
            ["document"] = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt" },
            ["excel"] = new[] { ".xlsx", ".xls", ".csv" }
        };
        
        _allowedMimeTypes = new Dictionary<string, string[]>
        {
            ["image"] = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/svg+xml" },
            ["video"] = new[] { "video/mp4", "video/quicktime", "video/webm", "video/x-msvideo", "video/x-matroska", "video/hevc", "video/h265" },
            ["audio"] = new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/wave" },
            ["document"] = new[] { 
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "text/plain"
            },
            ["excel"] = new[] { 
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/csv"
            }
        };

        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created storage base directory: {Path}", _basePath);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "auto",
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        // Determine resource type from extension if "auto"
        if (resourceType == "auto")
        {
            resourceType = GetResourceTypeFromExtension(extension);
        }

        // Validate extension
        if (!_allowedExtensions.TryGetValue(resourceType, out var allowedExts) ||
            !allowedExts.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File type '{extension}' is not allowed for resource type '{resourceType}'. " +
                $"Allowed: {string.Join(", ", allowedExts ?? Array.Empty<string>())}");
        }

        // Create folder path
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        // Generate unique filename with hash to avoid collisions
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var randomBytes = new byte[4];
        RandomNumberGenerator.Fill(randomBytes);
        var randomHex = Convert.ToHexString(randomBytes).ToLowerInvariant();
        var uniqueFileName = $"{timestamp}-{randomHex}{extension}";
        
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Copy file stream to disk
        using var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        // Build public URL
        var publicUrl = $"{_publicPathPrefix}/{folder}/{uniqueFileName}";

        _logger.LogInformation(
            "File uploaded successfully: {FileName} -> {Url} ({Size} bytes)",
            fileName, publicUrl, outputStream.Length);

        return publicUrl;
    }

    public Task<bool> DeleteFileAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var localPath = GetLocalPathFromUrl(publicUrl);
            
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                _logger.LogWarning("File not found for deletion: {Url}", publicUrl);
                return Task.FromResult(false);
            }

            File.Delete(localPath);
            _logger.LogInformation("File deleted successfully: {Url}", publicUrl);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Url}", publicUrl);
            return Task.FromResult(false);
        }
    }

    public string GetTransformedUrl(string publicUrl, int? width = null, int? height = null, string? format = null)
    {
        // Local storage doesn't support transformations like Cloudinary
        // For images, you could implement using System.Drawing or ImageSharp
        // For now, return the original URL
        return publicUrl;
    }

    public string GetDownloadUrl(string publicUrl)
    {
        // Add Content-Disposition header for download
        // In production, you might want to use a different endpoint that sets headers
        return publicUrl;
    }

    public (bool IsValid, string? Error, string? ResourceType) ValidateFile(
        string fileName,
        long fileSize,
        string? mimeType = null)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var resourceType = GetResourceTypeFromExtension(extension);

        // Check file size
        if (fileSize > _maxFileSize)
        {
            return (false, $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB", null);
        }

        // Check extension
        if (!_allowedExtensions.TryGetValue(resourceType, out var allowedExts) ||
            !allowedExts.Contains(extension))
        {
            return (false, $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", allowedExts ?? Array.Empty<string>())}", null);
        }

        // Check MIME type if provided
        if (!string.IsNullOrEmpty(mimeType) && _allowedMimeTypes.TryGetValue(resourceType, out var allowedMimes))
        {
            if (!allowedMimes.Contains(mimeType.ToLowerInvariant()))
            {
                return (false, $"MIME type '{mimeType}' is not allowed for this file type", null);
            }
        }

        return (true, null, resourceType);
    }

    private string GetResourceTypeFromExtension(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" => "image",
            ".mp4" or ".mov" or ".avi" or ".webm" or ".mkv" or ".hevc" => "video",
            ".mp3" or ".wav" => "audio",
            ".pdf" or ".doc" or ".docx" or ".ppt" or ".pptx" or ".txt" => "document",
            ".xlsx" or ".xls" or ".csv" => "excel",
            _ => "document" // Default to document
        };
    }

    private string? GetLocalPathFromUrl(string publicUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(publicUrl))
                return null;

            // Remove base URL to get relative path
            var relativePath = publicUrl;
            
            if (publicUrl.StartsWith(_baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = publicUrl.Substring(_baseUrl.Length).TrimStart('/');
            }
            else if (publicUrl.StartsWith("http://") || publicUrl.StartsWith("https://"))
            {
                // Try to extract path from full URL
                var uri = new Uri(publicUrl);
                relativePath = uri.AbsolutePath.TrimStart('/');
                // Remove /storage prefix if present
                if (relativePath.StartsWith("storage/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath.Substring(8);
                }
            }

            return Path.Combine(_basePath, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing URL to local path: {Url}", publicUrl);
            return null;
        }
    }

    private static string ResolvePublicPathPrefix(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return "/storage";
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var absoluteUri))
        {
            var absolutePath = absoluteUri.AbsolutePath.TrimEnd('/');
            return string.IsNullOrWhiteSpace(absolutePath) ? "/storage" : absolutePath;
        }

        var normalized = baseUrl.TrimEnd('/');
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized;
    }
}
