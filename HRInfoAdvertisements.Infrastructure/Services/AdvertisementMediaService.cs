using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementMediaService : IAdvertisementMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly string[] AllowedVideoTypes =
    {
        "video/mp4",
        "video/webm",
        "video/quicktime"
    };

    private static readonly string[] AllowedDocumentTypes =
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public AdvertisementMediaService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    // ============================================================
    // IMAGES
    // ============================================================

    public async Task<AdvertisementImageResponse> UploadImageAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        bool isPrimary)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        ValidateContentType(
            contentType,
            AllowedImageTypes,
            "image");

        ValidateFileSize(
            fileStream,
            10 * 1024 * 1024,
            "Image");

        var existingCount =
            await _context.AdvertisementImages
                .CountAsync(x =>
                    x.AdvertisementID == advertisementId);

        if (isPrimary || existingCount == 0)
        {
            await ClearPrimaryImageAsync(
                advertisementId);
        }

        var storedFile =
            await _fileStorageService.SaveFileAsync(
                fileStream,
                originalFileName,
                "images",
                contentType);

        var image = new AdvertisementImage
        {
            AdvertisementID = advertisementId,
            FileName = storedFile.FileName,
            FileURL = storedFile.FileUrl,
            StorageKey = storedFile.FileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize,
            IsPrimary = isPrimary || existingCount == 0,
            DisplayOrder = existingCount + 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.AdvertisementImages.Add(image);

        await _context.SaveChangesAsync();

        return MapImage(image);
    }

    public async Task<List<AdvertisementImageResponse>> GetImagesAsync(
        long advertisementId,
        long? userId = null)
    {
        await EnsureAdvertisementVisibleAsync(
            advertisementId,
            userId);

        var images =
            await _context.AdvertisementImages
                .AsNoTracking()
                .Where(x =>
                    x.AdvertisementID == advertisementId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

        return images
            .Select(MapImage)
            .ToList();
    }

    public async Task<bool> SetPrimaryImageAsync(
        long userId,
        long advertisementId,
        long imageId)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        var image =
            await _context.AdvertisementImages
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementImageID == imageId &&
                    x.AdvertisementID == advertisementId);

        if (image == null)
            return false;

        await ClearPrimaryImageAsync(
            advertisementId);

        image.IsPrimary = true;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateImageOrderAsync(
        long userId,
        long advertisementId,
        long imageId,
        int displayOrder)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        if (displayOrder < 1)
        {
            throw new InvalidOperationException(
                "Display order must be greater than zero.");
        }

        var image =
            await _context.AdvertisementImages
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementImageID == imageId &&
                    x.AdvertisementID == advertisementId);

        if (image == null)
            return false;

        image.DisplayOrder = displayOrder;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteImageAsync(
        long userId,
        long advertisementId,
        long imageId)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        var image =
            await _context.AdvertisementImages
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementImageID == imageId &&
                    x.AdvertisementID == advertisementId);

        if (image == null)
            return false;

        await _fileStorageService.DeleteFileAsync(
            image.FileURL);

        _context.AdvertisementImages.Remove(image);

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // VIDEOS
    // ============================================================

    public async Task<AdvertisementVideoResponse> UploadVideoAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        ValidateContentType(
            contentType,
            AllowedVideoTypes,
            "video");

        ValidateFileSize(
            fileStream,
            100 * 1024 * 1024,
            "Video");

        var existingCount =
            await _context.AdvertisementVideos
                .CountAsync(x =>
                    x.AdvertisementID == advertisementId);

        var storedFile =
            await _fileStorageService.SaveFileAsync(
                fileStream,
                originalFileName,
                "videos",
                contentType);

        var video = new AdvertisementVideo
        {
            AdvertisementID = advertisementId,
            FileName = storedFile.FileName,
            FileURL = storedFile.FileUrl,
            StorageKey = storedFile.FileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize,
            DisplayOrder = existingCount + 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.AdvertisementVideos.Add(video);

        await _context.SaveChangesAsync();

        return MapVideo(video);
    }

    public async Task<List<AdvertisementVideoResponse>> GetVideosAsync(
        long advertisementId,
        long? userId = null)
    {
        await EnsureAdvertisementVisibleAsync(
            advertisementId,
            userId);

        var videos =
            await _context.AdvertisementVideos
                .AsNoTracking()
                .Where(x =>
                    x.AdvertisementID == advertisementId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

        return videos
            .Select(MapVideo)
            .ToList();
    }

    public async Task<bool> DeleteVideoAsync(
        long userId,
        long advertisementId,
        long videoId)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        var video =
            await _context.AdvertisementVideos
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementVideoID == videoId &&
                    x.AdvertisementID == advertisementId);

        if (video == null)
            return false;

        await _fileStorageService.DeleteFileAsync(
            video.FileURL);

        _context.AdvertisementVideos.Remove(video);

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // DOCUMENTS
    // ============================================================

    public async Task<AdvertisementDocumentResponse> UploadDocumentAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        ValidateContentType(
            contentType,
            AllowedDocumentTypes,
            "document");

        ValidateFileSize(
            fileStream,
            20 * 1024 * 1024,
            "Document");

        var storedFile =
            await _fileStorageService.SaveFileAsync(
                fileStream,
                originalFileName,
                "documents",
                contentType);

        var document = new AdvertisementDocument
        {
            AdvertisementID = advertisementId,
            DocumentName = storedFile.OriginalFileName,
            FileURL = storedFile.FileUrl,
            StorageKey = storedFile.FileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize,
            CreatedDate = DateTime.UtcNow
        };

        _context.AdvertisementDocuments.Add(document);

        await _context.SaveChangesAsync();

        return MapDocument(document);
    }

    public async Task<List<AdvertisementDocumentResponse>> GetDocumentsAsync(
        long advertisementId,
        long? userId = null)
    {
        await EnsureAdvertisementVisibleAsync(
            advertisementId,
            userId);

        var documents =
            await _context.AdvertisementDocuments
                .AsNoTracking()
                .Where(x =>
                    x.AdvertisementID == advertisementId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

        return documents
            .Select(MapDocument)
            .ToList();
    }

    public async Task<bool> DeleteDocumentAsync(
        long userId,
        long advertisementId,
        long documentId)
    {
        await GetEditableAdvertisementAsync(
            userId,
            advertisementId);

        var document =
            await _context.AdvertisementDocuments
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementDocumentID == documentId &&
                    x.AdvertisementID == advertisementId);

        if (document == null)
            return false;

        await _fileStorageService.DeleteFileAsync(
            document.FileURL);

        _context.AdvertisementDocuments.Remove(document);

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // SECURITY / VALIDATION
    // ============================================================

    private async Task<Advertisement>
        GetEditableAdvertisementAsync(
            long userId,
            long advertisementId)
    {
        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId);

        if (advertisement == null)
        {
            throw new InvalidOperationException(
                "Advertisement not found or you are not the owner.");
        }

        var status =
            advertisement.Status.StatusCode;

        if (status != "DRAFT" &&
            status != "REJECTED")
        {
            throw new InvalidOperationException(
                "Media can only be modified while the advertisement is Draft or Rejected.");
        }

        return advertisement;
    }

    private async Task EnsureAdvertisementVisibleAsync(
        long advertisementId,
        long? userId)
    {
        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId);

        if (advertisement == null)
        {
            throw new InvalidOperationException(
                "Advertisement not found.");
        }

        if (userId.HasValue &&
            advertisement.UserID == userId.Value)
        {
            return;
        }

        if (advertisement.Status.StatusCode != "PUBLISHED")
        {
            throw new InvalidOperationException(
                "Advertisement is not available.");
        }
    }

    private async Task ClearPrimaryImageAsync(
        long advertisementId)
    {
        var primaryImages =
            await _context.AdvertisementImages
                .Where(x =>
                    x.AdvertisementID == advertisementId &&
                    x.IsPrimary)
                .ToListAsync();

        foreach (var image in primaryImages)
        {
            image.IsPrimary = false;
        }
    }

    private static void ValidateContentType(
        string contentType,
        string[] allowedTypes,
        string mediaType)
    {
        if (string.IsNullOrWhiteSpace(contentType) ||
            !allowedTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Invalid {mediaType} file type.");
        }
    }

    private static void ValidateFileSize(
        Stream stream,
        long maximumSize,
        string mediaType)
    {
        if (stream.Length > maximumSize)
        {
            throw new InvalidOperationException(
                $"{mediaType} exceeds the maximum allowed file size.");
        }
    }

    // ============================================================
    // MAPPING
    // ============================================================

    private static AdvertisementImageResponse MapImage(
        AdvertisementImage image)
    {
        return new AdvertisementImageResponse
        {
            AdvertisementImageID =
                image.AdvertisementImageID,

            AdvertisementID =
                image.AdvertisementID,

            FileName =
                image.FileName,

            FileURL =
                image.FileURL,

            ContentType =
                image.ContentType,

            FileSize =
                image.FileSize,

            IsPrimary =
                image.IsPrimary,

            DisplayOrder =
                image.DisplayOrder,

            CreatedDate =
                image.CreatedDate
        };
    }

    private static AdvertisementVideoResponse MapVideo(
        AdvertisementVideo video)
    {
        return new AdvertisementVideoResponse
        {
            AdvertisementVideoID =
                video.AdvertisementVideoID,

            AdvertisementID =
                video.AdvertisementID,

            FileName =
                video.FileName,

            FileURL =
                video.FileURL,

            ContentType =
                video.ContentType,

            FileSize =
                video.FileSize,

            DisplayOrder =
                video.DisplayOrder,

            CreatedDate =
                video.CreatedDate
        };
    }

    private static AdvertisementDocumentResponse MapDocument(
        AdvertisementDocument document)
    {
        return new AdvertisementDocumentResponse
        {
            AdvertisementDocumentID =
                document.AdvertisementDocumentID,

            AdvertisementID =
                document.AdvertisementID,

            DocumentName =
                document.DocumentName,

            FileURL =
                document.FileURL,

            ContentType =
                document.ContentType,

            FileSize =
                document.FileSize,

            CreatedDate =
                document.CreatedDate
        };
    }
}