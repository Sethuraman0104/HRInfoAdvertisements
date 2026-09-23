using HRInfoAdvertisements.Application.Profile.DTOs;

namespace HRInfoAdvertisements.Application.Profile;

public interface IProfileService
{
    Task<UserProfileResponse> GetProfileAsync(long userId);

    Task<UserProfileResponse> UpdateProfileAsync(
        long userId,
        UpdateUserProfileRequest request);

    Task<List<UserAddressResponse>> GetAddressesAsync(long userId);

    Task<UserAddressResponse?> GetAddressAsync(
        long userId,
        long addressId);

    Task<UserAddressResponse> AddAddressAsync(
        long userId,
        CreateUserAddressRequest request);

    Task<UserAddressResponse?> UpdateAddressAsync(
        long userId,
        long addressId,
        UpdateUserAddressRequest request);

    Task<bool> DeleteAddressAsync(
        long userId,
        long addressId);

    Task<bool> SetPrimaryAddressAsync(
        long userId,
        long addressId);
}
