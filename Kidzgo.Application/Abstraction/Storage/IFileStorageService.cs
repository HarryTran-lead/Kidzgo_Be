namespace Kidzgo.Application.Abstraction.Storage;

/// Service for uploading files to storage
public interface IFileStorageService
{
    /// Upload a file and return the public URL
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Folder path in storage (e.g., "tickets", "media", "blog")</param>
    /// <param name="resourceType">Resource type such as "image", "video", "audio", "document", or "excel"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "auto",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get URL with force download flag (fl_attachment) for raw files like PDF
    /// </summary>
    string GetDownloadUrl(string publicUrl);

    /// Delete a file from storage by its public URL
    /// <param name="publicUrl">Public URL of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteFileAsync(string publicUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate file before upload (extension, size, MIME type)
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="fileSize">File size in bytes</param>
    /// <param name="mimeType">MIME type (optional, auto-detected from extension if not provided)</param>
    /// <returns>Tuple of (IsValid, ErrorMessage, ResourceType)</returns>
    (bool IsValid, string? Error, string? ResourceType) ValidateFile(
        string fileName,
        long fileSize,
        string? mimeType = null);

    /// Get optimized/transformed URL for an image (e.g., resize, format conversion)
    /// <param name="publicUrl">Original public URL</param>
    /// <param name="width">Desired width (optional)</param>
    /// <param name="height">Desired height (optional)</param>
    /// <param name="format">Desired format (optional, e.g., "webp", "jpg")</param>
    /// <returns>Transformed URL</returns>
    string GetTransformedUrl(string publicUrl, int? width = null, int? height = null, string? format = null);
}

