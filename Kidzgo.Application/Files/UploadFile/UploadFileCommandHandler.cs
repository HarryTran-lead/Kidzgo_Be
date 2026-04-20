using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.UploadFile;

public sealed class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
    IDbContext context,
    IUserContext userContext,
    ILogger<UploadFileCommandHandler> logger
) : ICommandHandler<UploadFileCommand, UploadFileResponse>
{
    private static readonly Dictionary<string, string[]> AllowedExtensions = new()
    {
        ["image"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" },
        ["video"] = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv", ".hevc" },
        ["audio"] = new[] { ".mp3", ".wav" },
        ["document"] = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt" },
        ["excel"] = new[] { ".xlsx", ".xls", ".csv" }
    };

    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        ["image"] = 10 * 1024 * 1024,      // 10MB
        ["video"] = 100 * 1024 * 1024,     // 100MB
        ["audio"] = 50 * 1024 * 1024,      // 50MB
        ["document"] = 50 * 1024 * 1024,   // 50MB
        ["excel"] = 20 * 1024 * 1024       // 20MB
    };

    public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(command.FileName).ToLowerInvariant();
        
        // Auto-detect resource type from extension
        var resourceType = command.ResourceType == "auto" 
            ? DetectResourceType(fileExtension) 
            : command.ResourceType;

        // Get max size for this type
        var maxSize = MaxFileSizes.GetValueOrDefault(resourceType, MaxFileSizes["document"]);
        
        // Validate file size
        if (command.FileSize > maxSize)
        {
            return Result.Failure<UploadFileResponse>(
                FileErrors.FileSizeExceedsLimit(resourceType, maxSize / 1024 / 1024));
        }

        // Validate file extension
        var allowedExts = AllowedExtensions.GetValueOrDefault(resourceType, AllowedExtensions["document"]);
        if (!allowedExts.Contains(fileExtension))
        {
            return Result.Failure<UploadFileResponse>(
                FileErrors.InvalidFileType(resourceType, allowedExts));
        }

        if (command.UpdateUserAvatar || command.UpdateProfileAvatar)
        {
            var avatarContextResult = await ValidateAvatarUpdateContextAsync(command, cancellationToken);
            if (avatarContextResult.IsFailure)
            {
                return Result.Failure<UploadFileResponse>(avatarContextResult.Error);
            }
        }

        try
        {
            var url = await fileStorageService.UploadFileAsync(
                command.FileStream,
                command.FileName,
                command.Folder,
                resourceType,
                cancellationToken);
            
            if (command.UpdateUserAvatar || command.UpdateProfileAvatar)
            {
                await UpdateAvatarAsync(command, url, cancellationToken);
            }

            logger.LogInformation("File uploaded successfully: {FileName} -> {Url}", command.FileName, url);

            return Result.Success(new UploadFileResponse
            {
                Url = url,
                FileName = command.FileName,
                Size = command.FileSize,
                Folder = command.Folder,
                ResourceType = resourceType
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file: {FileName}", command.FileName);
            return Result.Failure<UploadFileResponse>(FileErrors.UploadFailed(ex.Message));
        }
    }

    private async Task<Result> ValidateAvatarUpdateContextAsync(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == userContext.UserId && u.IsActive && !u.IsDeleted,
                cancellationToken);

        if (user is null)
        {
            return Result.Failure(FileErrors.Unauthorized());
        }

        if (command.UpdateProfileAvatar)
        {
            var profile = await ResolveAvatarProfileAsync(user.Id, command.TargetProfileId, cancellationToken);
            if (profile is null && user.Role == UserRole.Parent)
            {
                return Result.Failure(FileErrors.ParentProfileSelectionRequired());
            }
        }

        return Result.Success();
    }

    private async Task UpdateAvatarAsync(UploadFileCommand command, string url, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var hasChanges = false;
        var now = VietnamTime.UtcNow();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        var profile = command.UpdateProfileAvatar
            ? await ResolveAvatarProfileAsync(userId, command.TargetProfileId, cancellationToken)
            : null;

        if (command.UpdateUserAvatar &&
            user is not null &&
            ShouldUpdateUserAvatar(user, profile))
        {
            user.AvatarUrl = url;
            user.AvatarMimeType = command.ContentType;
            user.AvatarFileSize = command.FileSize;
            user.UpdatedAt = now;
            hasChanges = true;
        }

        if (command.UpdateProfileAvatar && profile is not null)
        {
            profile.AvatarUrl = url;
            profile.AvatarMimeType = command.ContentType;
            profile.AvatarFileSize = command.FileSize;
            profile.UpdatedAt = now;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Profile?> ResolveAvatarProfileAsync(
        Guid userId,
        Guid? explicitTargetProfileId,
        CancellationToken cancellationToken)
    {
        var targetProfileId = explicitTargetProfileId ?? userContext.StudentId;

        if (targetProfileId.HasValue)
        {
            var ownedProfile = await context.Profiles
                .FirstOrDefaultAsync(
                    p => p.Id == targetProfileId.Value &&
                         p.UserId == userId &&
                         !p.IsDeleted,
                    cancellationToken);

            if (ownedProfile is not null)
            {
                return ownedProfile;
            }

            return await context.ParentStudentLinks
                .Where(link => link.StudentProfileId == targetProfileId.Value &&
                               !link.ParentProfile.IsDeleted &&
                               link.ParentProfile.UserId == userId)
                .Select(link => link.StudentProfile)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await context.Profiles
            .Where(p => p.UserId == userId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.ProfileType == ProfileType.Parent ? 0 : 1)
            .ThenBy(p => p.ProfileType == ProfileType.Student ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static bool ShouldUpdateUserAvatar(User user, Profile? targetProfile)
    {
        if (user.Role != UserRole.Parent)
        {
            return true;
        }

        if (targetProfile is null)
        {
            return true;
        }

        return targetProfile.ProfileType == ProfileType.Parent;
    }

    private static string DetectResourceType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" => "image",
            ".mp4" or ".mov" or ".avi" or ".webm" or ".mkv" or ".hevc" => "video",
            ".mp3" or ".wav" => "audio",
            ".pdf" or ".doc" or ".docx" or ".ppt" or ".pptx" or ".txt" => "document",
            ".xlsx" or ".xls" or ".csv" => "excel",
            _ => "document"
        };
    }
}

