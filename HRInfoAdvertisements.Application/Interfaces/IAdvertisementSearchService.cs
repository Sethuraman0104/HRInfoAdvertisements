using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.DTOs.Common;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementSearchService
{
    Task<PagedResponse<AdvertisementSearchResponse>>
        SearchAsync(
            AdvertisementSearchRequest request);
}