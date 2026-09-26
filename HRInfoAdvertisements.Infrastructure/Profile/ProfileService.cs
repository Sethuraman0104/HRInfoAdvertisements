using HRInfoAdvertisements.Application.Profile;
using HRInfoAdvertisements.Application.Profile.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Profile;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ------------------------------------------------------------
    // Profile
    // ------------------------------------------------------------

    public async Task<UserProfileResponse> GetProfileAsync(long userId)
    {
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserID == userId);

        if (profile == null)
        {
            throw new KeyNotFoundException(
                "User profile was not found.");
        }

        return MapProfile(profile);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(
        long userId,
        UpdateUserProfileRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException(
                "Profile information is required.");
        }

        var firstName = request.FirstName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (firstName.Length > 100)
        {
            throw new ArgumentException(
                "First name cannot exceed 100 characters.");
        }

        var lastName = request.LastName?.Trim();

        if (lastName?.Length > 100)
        {
            throw new ArgumentException(
                "Last name cannot exceed 100 characters.");
        }

        var nationality = request.Nationality?.Trim();

        if (nationality?.Length > 100)
        {
            throw new ArgumentException(
                "Nationality cannot exceed 100 characters.");
        }

        var preferredLanguage = request.PreferredLanguage?.Trim();

        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            preferredLanguage = "en";
        }

        if (preferredLanguage.Length > 10)
        {
            throw new ArgumentException(
                "Preferred language cannot exceed 10 characters.");
        }

        var profilePhotoUrl = request.ProfilePhotoURL?.Trim();

        if (profilePhotoUrl?.Length > 1000)
        {
            throw new ArgumentException(
                "Profile photo URL cannot exceed 1000 characters.");
        }

        var companyName = request.CompanyName?.Trim();

        if (companyName?.Length > 250)
        {
            throw new ArgumentException(
                "Company name cannot exceed 250 characters.");
        }

        if (!request.IsBusinessAccount)
        {
            companyName = null;
        }
        else if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException(
                "Company name is required for a business account.");
        }

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserID == userId);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "User was not found.");
        }

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserID == userId);

        if (profile == null)
        {
            profile = new UserProfile
            {
                UserID = userId,
                FirstName = firstName,
                LastName = lastName,
                ProfilePhotoURL = profilePhotoUrl,
                Nationality = nationality,
                PreferredLanguage = preferredLanguage,
                IsBusinessAccount = request.IsBusinessAccount,
                CompanyName = companyName,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserProfiles.Add(profile);
        }
        else
        {
            profile.FirstName = firstName;
            profile.LastName = lastName;
            profile.ProfilePhotoURL = profilePhotoUrl;
            profile.Nationality = nationality;
            profile.PreferredLanguage = preferredLanguage;
            profile.IsBusinessAccount = request.IsBusinessAccount;
            profile.CompanyName = companyName;
            profile.ModifiedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapProfile(profile);
    }

    // ------------------------------------------------------------
    // Addresses
    // ------------------------------------------------------------

    public async Task<List<UserAddressResponse>> GetAddressesAsync(
        long userId)
    {
        return await _context.UserAddresses
            .AsNoTracking()
            .Where(x => x.UserID == userId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.CreatedDate)
            .Select(x => new UserAddressResponse
            {
                UserAddressID = x.UserAddressID,
                UserID = x.UserID,

                CountryID = x.CountryID,
                CountryName = x.Country != null
                    ? x.Country.CountryName
                    : null,

                StateID = x.StateID,
                StateName = x.State != null
                    ? x.State.StateName
                    : null,

                CityID = x.CityID,
                CityName = x.City != null
                    ? x.City.CityName
                    : null,

                AreaID = x.AreaID,
                AreaName = x.Area != null
                    ? x.Area.AreaName
                    : null,

                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2,
                BuildingNo = x.BuildingNo,
                RoadNo = x.RoadNo,
                BlockNo = x.BlockNo,
                PostalCode = x.PostalCode,

                IsPrimary = x.IsPrimary,

                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate
            })
            .ToListAsync();
    }

    public async Task<UserAddressResponse?> GetAddressAsync(
        long userId,
        long addressId)
    {
        return await _context.UserAddresses
            .AsNoTracking()
            .Where(x =>
                x.UserID == userId &&
                x.UserAddressID == addressId)
            .Select(x => new UserAddressResponse
            {
                UserAddressID = x.UserAddressID,
                UserID = x.UserID,

                CountryID = x.CountryID,
                CountryName = x.Country != null
                    ? x.Country.CountryName
                    : null,

                StateID = x.StateID,
                StateName = x.State != null
                    ? x.State.StateName
                    : null,

                CityID = x.CityID,
                CityName = x.City != null
                    ? x.City.CityName
                    : null,

                AreaID = x.AreaID,
                AreaName = x.Area != null
                    ? x.Area.AreaName
                    : null,

                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2,
                BuildingNo = x.BuildingNo,
                RoadNo = x.RoadNo,
                BlockNo = x.BlockNo,
                PostalCode = x.PostalCode,

                IsPrimary = x.IsPrimary,

                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserAddressResponse> AddAddressAsync(
        long userId,
        CreateUserAddressRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException(
                "Address information is required.");
        }

        await ValidateLocationAsync(
            request.CountryID,
            request.StateID,
            request.CityID,
            request.AreaID);

        ValidateAddressFields(
            request.AddressLine1,
            request.AddressLine2,
            request.BuildingNo,
            request.RoadNo,
            request.BlockNo,
            request.PostalCode);

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserID == userId);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "User was not found.");
        }

        var hasExistingAddress = await _context.UserAddresses
            .AnyAsync(x => x.UserID == userId);

        var makePrimary =
            request.IsPrimary ||
            !hasExistingAddress;

        if (makePrimary)
        {
            await ClearPrimaryAddressAsync(userId);
        }

        var address = new UserAddress
        {
            UserID = userId,

            CountryID = request.CountryID,
            StateID = request.StateID,
            CityID = request.CityID,
            AreaID = request.AreaID,

            AddressLine1 = request.AddressLine1?.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            BuildingNo = request.BuildingNo?.Trim(),
            RoadNo = request.RoadNo?.Trim(),
            BlockNo = request.BlockNo?.Trim(),
            PostalCode = request.PostalCode?.Trim(),

            IsPrimary = makePrimary,

            CreatedDate = DateTime.UtcNow
        };

        _context.UserAddresses.Add(address);

        await _context.SaveChangesAsync();

        return await GetAddressAsync(
                userId,
                address.UserAddressID)
            ?? throw new InvalidOperationException(
                "The address could not be retrieved after creation.");
    }

    public async Task<UserAddressResponse?> UpdateAddressAsync(
        long userId,
        long addressId,
        UpdateUserAddressRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException(
                "Address information is required.");
        }

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                x.UserAddressID == addressId);

        if (address == null)
        {
            return null;
        }

        await ValidateLocationAsync(
            request.CountryID,
            request.StateID,
            request.CityID,
            request.AreaID);

        ValidateAddressFields(
            request.AddressLine1,
            request.AddressLine2,
            request.BuildingNo,
            request.RoadNo,
            request.BlockNo,
            request.PostalCode);

        if (request.IsPrimary)
        {
            await ClearPrimaryAddressAsync(
                userId,
                addressId);
        }
        else if (address.IsPrimary)
        {
            throw new ArgumentException(
                "The primary address cannot be unset. " +
                "Set another address as primary instead.");
        }

        address.CountryID = request.CountryID;
        address.StateID = request.StateID;
        address.CityID = request.CityID;
        address.AreaID = request.AreaID;

        address.AddressLine1 = request.AddressLine1?.Trim();
        address.AddressLine2 = request.AddressLine2?.Trim();
        address.BuildingNo = request.BuildingNo?.Trim();
        address.RoadNo = request.RoadNo?.Trim();
        address.BlockNo = request.BlockNo?.Trim();
        address.PostalCode = request.PostalCode?.Trim();

        address.IsPrimary = request.IsPrimary;
        address.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetAddressAsync(
            userId,
            addressId);
    }

    public async Task<bool> DeleteAddressAsync(
        long userId,
        long addressId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                x.UserAddressID == addressId);

        if (address == null)
        {
            return false;
        }

        if (address.IsPrimary)
        {
            var replacementAddress =
                await _context.UserAddresses
                    .Where(x =>
                        x.UserID == userId &&
                        x.UserAddressID != addressId)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

            if (replacementAddress != null)
            {
                replacementAddress.IsPrimary = true;
                replacementAddress.ModifiedDate =
                    DateTime.UtcNow;
            }
        }

        _context.UserAddresses.Remove(address);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetPrimaryAddressAsync(
        long userId,
        long addressId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                x.UserAddressID == addressId);

        if (address == null)
        {
            return false;
        }

        if (address.IsPrimary)
        {
            return true;
        }

        await ClearPrimaryAddressAsync(
            userId,
            addressId);

        address.IsPrimary = true;
        address.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Location validation
    // ------------------------------------------------------------

    private async Task ValidateLocationAsync(
        int? countryId,
        int? stateId,
        int? cityId,
        int? areaId)
    {
        if (stateId.HasValue &&
            !countryId.HasValue)
        {
            throw new ArgumentException(
                "Country is required when State is specified.");
        }

        if (cityId.HasValue &&
            !stateId.HasValue)
        {
            throw new ArgumentException(
                "State is required when City is specified.");
        }

        if (areaId.HasValue &&
            !cityId.HasValue)
        {
            throw new ArgumentException(
                "City is required when Area is specified.");
        }

        // --------------------------------------------------------
        // Country
        // --------------------------------------------------------

        if (countryId.HasValue)
        {
            var countryExists =
                await _context.Countries
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.CountryID == countryId.Value &&
                        x.IsActive);

            if (!countryExists)
            {
                throw new ArgumentException(
                    "The selected country was not found or is inactive.");
            }
        }

        // --------------------------------------------------------
        // State
        // --------------------------------------------------------

        if (stateId.HasValue)
        {
            var state =
                await _context.States
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.StateID == stateId.Value &&
                        x.IsActive);

            if (state == null)
            {
                throw new ArgumentException(
                    "The selected state was not found or is inactive.");
            }

            if (countryId.HasValue &&
                state.CountryID != countryId.Value)
            {
                throw new ArgumentException(
                    "The selected state does not belong to the selected country.");
            }
        }

        // --------------------------------------------------------
        // City
        //
        // Cities do NOT have CountryID.
        // The relationship is:
        //
        // City.StateID -> State.StateID
        // State.CountryID -> Country.CountryID
        // --------------------------------------------------------

        if (cityId.HasValue)
        {
            var city =
                await _context.Cities
                    .AsNoTracking()
                    .Where(x =>
                        x.CityID == cityId.Value &&
                        x.IsActive)
                    .Select(x => new
                    {
                        x.CityID,
                        x.StateID,
                        StateCountryID =
                            x.State != null
                                ? x.State.CountryID
                                : (int?)null
                    })
                    .FirstOrDefaultAsync();

            if (city == null)
            {
                throw new ArgumentException(
                    "The selected city was not found or is inactive.");
            }

            if (!city.StateID.HasValue)
            {
                throw new ArgumentException(
                    "The selected city is not associated with a state.");
            }

            if (stateId.HasValue &&
                city.StateID.Value != stateId.Value)
            {
                throw new ArgumentException(
                    "The selected city does not belong to the selected state.");
            }

            if (countryId.HasValue &&
                city.StateCountryID != countryId.Value)
            {
                throw new ArgumentException(
                    "The selected city does not belong to the selected country.");
            }
        }

        // --------------------------------------------------------
        // Area
        // --------------------------------------------------------

        if (areaId.HasValue)
{
    var area =
        await _context.Areas
            .AsNoTracking()
            .Where(x =>
                x.AreaID == areaId.Value &&
                x.IsActive)
            .Select(x => new
            {
                x.AreaID,
                x.CityID,
                CityStateID =
                    x.City != null
                        ? x.City.StateID
                        : (int?)null,
                CityStateCountryID =
                    x.City != null &&
                    x.City.State != null
                        ? x.City.State.CountryID
                        : (int?)null
            })
            .FirstOrDefaultAsync();

    if (area == null)
    {
        throw new ArgumentException(
            "The selected area was not found or is inactive.");
    }

    if (cityId.HasValue &&
        area.CityID != cityId.Value)
    {
        throw new ArgumentException(
            "The selected area does not belong to the selected city.");
    }

    if (!area.CityStateID.HasValue)
    {
        throw new ArgumentException(
            "The selected area is not associated with a state.");
    }

    if (stateId.HasValue &&
        area.CityStateID.Value != stateId.Value)
    {
        throw new ArgumentException(
            "The selected area does not belong to the selected state.");
    }

    if (countryId.HasValue &&
        area.CityStateCountryID != countryId.Value)
    {
        throw new ArgumentException(
            "The selected area does not belong to the selected country.");
    }
}
    }

    // ------------------------------------------------------------
    // Address validation
    // ------------------------------------------------------------

    private static void ValidateAddressFields(
        string? addressLine1,
        string? addressLine2,
        string? buildingNo,
        string? roadNo,
        string? blockNo,
        string? postalCode)
    {
        if (addressLine1?.Trim().Length > 300)
        {
            throw new ArgumentException(
                "Address Line 1 cannot exceed 300 characters.");
        }

        if (addressLine2?.Trim().Length > 300)
        {
            throw new ArgumentException(
                "Address Line 2 cannot exceed 300 characters.");
        }

        if (buildingNo?.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Building number cannot exceed 50 characters.");
        }

        if (roadNo?.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Road number cannot exceed 50 characters.");
        }

        if (blockNo?.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Block number cannot exceed 50 characters.");
        }

        if (postalCode?.Trim().Length > 30)
        {
            throw new ArgumentException(
                "Postal code cannot exceed 30 characters.");
        }
    }

    // ------------------------------------------------------------
    // Primary address helper
    // ------------------------------------------------------------

    private async Task ClearPrimaryAddressAsync(
        long userId,
        long? exceptAddressId = null)
    {
        var primaryAddresses =
            await _context.UserAddresses
                .Where(x =>
                    x.UserID == userId &&
                    x.IsPrimary &&
                    (!exceptAddressId.HasValue ||
                     x.UserAddressID != exceptAddressId.Value))
                .ToListAsync();

        foreach (var address in primaryAddresses)
        {
            address.IsPrimary = false;
            address.ModifiedDate = DateTime.UtcNow;
        }
    }

    // ------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------

    private static UserProfileResponse MapProfile(
        UserProfile profile)
    {
        return new UserProfileResponse
        {
            UserProfileID = profile.UserProfileID,
            UserID = profile.UserID,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            ProfilePhotoURL = profile.ProfilePhotoURL,
            Nationality = profile.Nationality,
            PreferredLanguage = profile.PreferredLanguage,
            IsBusinessAccount = profile.IsBusinessAccount,
            CompanyName = profile.CompanyName,
            CreatedDate = profile.CreatedDate,
            ModifiedDate = profile.ModifiedDate
        };
    }
}