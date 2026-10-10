using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.UserManagement;
using HRInfoAdvertisements.Application.Profile.DTOs;
using HRInfoAdvertisements.Application.NotificationPreferences.DTOs;

namespace HRInfoAdvertisements.Admin.Services;

public class UserManagementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public UserManagementApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory =
            httpClientFactory;

        _authenticationStateProvider =
            authenticationStateProvider;
    }

    // ============================================================
    // HTTP CLIENT
    // ============================================================

    private HttpClient CreateClient()
    {
        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var accessToken =
            _authenticationStateProvider.AccessToken;

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT HTTP CLIENT");

        Console.WriteLine(
            $"API BASE ADDRESS: {client.BaseAddress}");

        Console.WriteLine(
            $"ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(accessToken)}");

        Console.WriteLine(
            $"ACCESS TOKEN LENGTH: " +
            $"{accessToken?.Length ?? 0}");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Console.WriteLine(
                "USER MANAGEMENT: NO ADMIN TOKEN AVAILABLE.");

            Console.WriteLine(
                "================================================");

            throw new UnauthorizedAccessException(
                "Administrator access token is not available. " +
                "Please sign in again.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        Console.WriteLine(
            "USER MANAGEMENT: ADMIN JWT ATTACHED.");

        Console.WriteLine(
            "================================================");

        return client;
    }

    // ============================================================
    // GET USERS
    // ============================================================

    public async Task<UserListResponse>
        GetUsersAsync(
            string? search = null,
            string? accountStatus = null,
            int? roleId = null,
            int pageNumber = 1,
            int pageSize = 20)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var query = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(accountStatus))
        {
            query.Add(
                $"accountStatus={Uri.EscapeDataString(
                    accountStatus.Trim())}");
        }

        if (roleId.HasValue && roleId.Value > 0)
        {
            query.Add(
                $"roleId={roleId.Value}");
        }

        var url =
            "api/v1/users?" +
            string.Join("&", query);

        var client =
            CreateClient();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - GET USERS");

        Console.WriteLine(
            $"GET: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE LENGTH: {responseBody.Length}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve users. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new UserListResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        return JsonSerializer.Deserialize<UserListResponse>(
                   responseBody,
                   JsonOptions)
               ?? new UserListResponse
               {
                   PageNumber = pageNumber,
                   PageSize = pageSize
               };
    }

// ============================================================
// UPDATE USER PROFILE
// ============================================================

public async Task<
    HRInfoAdvertisements.Application.DTOs.UserManagement.UserProfileResponse>
    UpdateUserProfileAsync(
        long userId,
        UpdateUserProfileRequest request)
{
    if (userId <= 0)
    {
        throw new ArgumentException(
            "Invalid user ID.",
            nameof(userId));
    }

    var client = CreateClient();

    var url =
        $"api/v1/users/{userId}/profile";

    Console.WriteLine(
        "================================================");

    Console.WriteLine(
        "USER MANAGEMENT - UPDATE USER PROFILE");

    Console.WriteLine(
        $"PUT: {url}");

    Console.WriteLine(
        $"FIRST NAME: {request.FirstName}");

    Console.WriteLine(
        $"LAST NAME: {request.LastName}");

    Console.WriteLine(
        $"NATIONALITY: {request.Nationality}");

    Console.WriteLine(
        $"PREFERRED LANGUAGE: {request.PreferredLanguage}");

    Console.WriteLine(
        $"IS BUSINESS ACCOUNT: {request.IsBusinessAccount}");

    Console.WriteLine(
        $"COMPANY NAME: {request.CompanyName}");

    Console.WriteLine(
        $"PROFILE PHOTO URL: {request.ProfilePhotoURL}");

    Console.WriteLine(
        "================================================");

    var response =
        await client.PutAsJsonAsync(
            url,
            request);

    var responseBody =
        await response.Content.ReadAsStringAsync();

    Console.WriteLine(
        $"STATUS: {(int)response.StatusCode} " +
        $"{response.StatusCode}");

    Console.WriteLine(
        $"RESPONSE: {responseBody}");

    Console.WriteLine(
        "================================================");

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Unable to update user profile. " +
            $"HTTP {(int)response.StatusCode} " +
            $"({response.StatusCode}). " +
            $"Response: {responseBody}");
    }

    var result =
        JsonSerializer.Deserialize<
            HRInfoAdvertisements.Application.DTOs.UserManagement.UserProfileResponse>(
                responseBody,
                JsonOptions);

    if (result == null)
    {
        throw new InvalidOperationException(
            "The API returned an empty user profile response.");
    }

    return result;
}

    // ============================================================
    // GET USER BY ID
    // ============================================================

    public async Task<UserDetailResponse?>
        GetUserByIdAsync(
            long userId)
    {
        if (userId <= 0)
        {
            return null;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/users/{userId}";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - GET USER");

        Console.WriteLine(
            $"GET: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve user. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserDetailResponse>(
            responseBody,
            JsonOptions);
    }

    
    // ============================================================
    // GET USER NOTIFICATION PREFERENCES
    // ============================================================

    public async Task<NotificationPreferenceResponse>
        GetNotificationPreferencesAsync(long userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "Invalid user ID.",
                nameof(userId));
        }

        var client = CreateClient();

        var url =
            $"api/v1/users/{userId}/notification-preferences";

        var response = await client.GetAsync(url);
        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                "Unable to retrieve notification preferences. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). Response: {responseBody}");
        }

        var result =
            JsonSerializer.Deserialize<NotificationPreferenceResponse>(
                responseBody,
                JsonOptions);

        return result
            ?? throw new InvalidOperationException(
                "The API returned an empty notification preference response.");
    }


    // ============================================================
    // UPDATE USER NOTIFICATION PREFERENCES
    // ============================================================

    public async Task<NotificationPreferenceResponse>
        UpdateNotificationPreferencesAsync(
            long userId,
            UpdateNotificationPreferenceRequest request)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "Invalid user ID.",
                nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(request);

        var client = CreateClient();

        var url =
            $"api/v1/users/{userId}/notification-preferences";

        var response =
            await client.PutAsJsonAsync(url, request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                "Unable to update notification preferences. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). Response: {responseBody}");
        }

        // The API returns { success, message, preferences }.
        // Extract the preferences object from that response.
        using var document =
            JsonDocument.Parse(responseBody);

        if (!document.RootElement.TryGetProperty(
                "preferences",
                out var preferencesElement))
        {
            throw new InvalidOperationException(
                "The API response does not contain notification preferences.");
        }

        return preferencesElement.Deserialize<NotificationPreferenceResponse>(
                   JsonOptions)
               ?? throw new InvalidOperationException(
                   "The API returned an empty notification preference response.");
    }

    // ============================================================
    // UPDATE ACCOUNT STATUS
    // ============================================================

    public async Task<bool>
        UpdateUserStatusAsync(
            long userId,
            string accountStatus)
    {
        if (userId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(accountStatus))
        {
            throw new ArgumentException(
                "Account status is required.",
                nameof(accountStatus));
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/users/{userId}/status";

        var request = new
        {
            AccountStatus = accountStatus
        };

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - UPDATE STATUS");

        Console.WriteLine(
            $"PUT: {url}");

        Console.WriteLine(
            $"ACCOUNT STATUS: {accountStatus}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PutAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE: {responseBody}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update user account status. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }

    // ============================================================
    // LOCK / UNLOCK USER
    // ============================================================

    public async Task<bool>
        UpdateUserLockAsync(
            long userId,
            bool isLocked,
            DateTime? lockoutEndDate = null)
    {
        if (userId <= 0)
        {
            return false;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/users/{userId}/lock";

        var request = new
        {
            IsLocked = isLocked,
            LockoutEndDate = lockoutEndDate
        };

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - UPDATE LOCK");

        Console.WriteLine(
            $"PUT: {url}");

        Console.WriteLine(
            $"IS LOCKED: {isLocked}");

        Console.WriteLine(
            $"LOCKOUT END DATE: {lockoutEndDate}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PutAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE: {responseBody}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to update user lock status. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }

    // ============================================================
    // ASSIGN ROLE
    // ============================================================

    public async Task<bool>
        AssignRoleAsync(
            long userId,
            int roleId)
    {
        if (userId <= 0 || roleId <= 0)
        {
            return false;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/users/{userId}/roles";

        var request = new
        {
            RoleID = roleId
        };

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - ASSIGN ROLE");

        Console.WriteLine(
            $"POST: {url}");

        Console.WriteLine(
            $"ROLE ID: {roleId}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PostAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE: {responseBody}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to assign user role. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }

    // ============================================================
    // REMOVE ROLE
    // ============================================================

    public async Task<bool>
        RemoveRoleAsync(
            long userId,
            int roleId)
    {
        if (userId <= 0 || roleId <= 0)
        {
            return false;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/users/{userId}/roles/{roleId}";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "USER MANAGEMENT - REMOVE ROLE");

        Console.WriteLine(
            $"DELETE: {url}");

        Console.WriteLine(
            $"ROLE ID: {roleId}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.DeleteAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"STATUS: {(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"RESPONSE: {responseBody}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to remove user role. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        return true;
    }
}