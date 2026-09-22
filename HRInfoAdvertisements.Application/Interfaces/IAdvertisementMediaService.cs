using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementMediaService
{
    Task<AdvertisementImageResponse> UploadImageAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        bool isPrimary);

    Task<List<AdvertisementImageResponse>> GetImagesAsync(
        long advertisementId,
        long? userId = null);

    Task<bool> SetPrimaryImageAsync(
        long userId,
        long advertisementId,
        long imageId);

    Task<bool> UpdateImageOrderAsync(
        long userId,
        long advertisementId,
        long imageId,
        int displayOrder);

    Task<bool> DeleteImageAsync(
        long userId,
        long advertisementId,
        long imageId);


    Task<AdvertisementVideoResponse> UploadVideoAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType);

    Task<List<AdvertisementVideoResponse>> GetVideosAsync(
        long advertisementId,
        long? userId = null);

    Task<bool> DeleteVideoAsync(
        long userId,
        long advertisementId,
        long videoId);


    Task<AdvertisementDocumentResponse> UploadDocumentAsync(
        long userId,
        long advertisementId,
        Stream fileStream,
        string originalFileName,
        string contentType);

    Task<List<AdvertisementDocumentResponse>> GetDocumentsAsync(
        long advertisementId,
        long? userId = null);

    Task<bool> DeleteDocumentAsync(
        long userId,
        long advertisementId,
        long documentId);
}