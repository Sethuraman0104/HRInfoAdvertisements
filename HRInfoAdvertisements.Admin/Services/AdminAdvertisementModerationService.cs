using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.DTOs.Admin;
using HRInfoAdvertisements.Application.Interfaces;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminAdvertisementModerationService
    : IAdvertisementModerationService
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

    public AdminAdvertisementModerationService(
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
    //
    // IMPORTANT:
    // AdminJwtHandler is NOT used here.
    //
    // The JWT is retrieved directly from the same
    // AdminAuthenticationStateProvider instance used by the
    // current Blazor Server circuit and attached explicitly
    // to the HttpClient Authorization header.
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
        "ADMIN MODERATION HTTP CLIENT");

    Console.WriteLine(
        $"API BASE ADDRESS: {client.BaseAddress}");

    Console.WriteLine(
        $"ACCESS TOKEN AVAILABLE: " +
        $"{!string.IsNullOrWhiteSpace(accessToken)}");

    Console.WriteLine(
        $"ACCESS TOKEN LENGTH: " +
        $"{accessToken?.Length ?? 0}");

    Console.WriteLine(
        $"TOKEN EXPIRES AT: " +
        $"{_authenticationStateProvider.ExpiresAt}");

    Console.WriteLine(
        $"CURRENT USER AUTHENTICATED: " +
        $"{GetAuthenticationStatus()}");

    if (string.IsNullOrWhiteSpace(accessToken))
    {
        Console.WriteLine(
            "AUTHORIZATION HEADER: NO TOKEN AVAILABLE.");

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
        "AUTHORIZATION HEADER: Bearer token attached.");

    Console.WriteLine(
        "================================================");

    return client;
}

private bool GetAuthenticationStatus()
{
    try
    {
        var authenticationState =
            _authenticationStateProvider
                .GetAuthenticationStateAsync()
                .GetAwaiter()
                .GetResult();

        return authenticationState.User.Identity?.IsAuthenticated
            == true;
    }
    catch
    {
        return false;
    }
}

    // ============================================================
    // GET ADMIN ADVERTISEMENTS
    // ============================================================

    public async Task<List<AdminAdvertisementListResponse>>
        GetAdvertisementsAsync(
            string? statusCode = null,
            string? search = null,
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

        var query =
            new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

        if (!string.IsNullOrWhiteSpace(statusCode))
        {
            query.Add(
                $"statusCode={Uri.EscapeDataString(
                    statusCode.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(
                    search.Trim())}");
        }

        var url =
            "api/v1/admin/advertisements?" +
            string.Join("&", query);

        var client =
            CreateClient();

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN MODERATION - GET ADVERTISEMENTS");

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
            Console.WriteLine(
                $"ADMIN MODERATION GET FAILED: " +
                $"{(int)response.StatusCode} " +
                $"{response.StatusCode}");

            throw new HttpRequestException(
                $"Unable to retrieve advertisements. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new List<AdminAdvertisementListResponse>();
        }

        var result =
            JsonSerializer.Deserialize<
                List<AdminAdvertisementListResponse>>(
                responseBody,
                JsonOptions);

        return result
            ?? new List<AdminAdvertisementListResponse>();
    }

    // ============================================================
    // GET ADVERTISEMENT DETAIL
    // ============================================================

    public async Task<AdminAdvertisementDetailResponse?>
        GetAdvertisementAsync(
            long advertisementId)
    {
        if (advertisementId <= 0)
        {
            return null;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN MODERATION - GET DETAIL");

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
                $"Unable to retrieve advertisement detail. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        return JsonSerializer.Deserialize<
            AdminAdvertisementDetailResponse>(
            responseBody,
            JsonOptions);
    }

    // ============================================================
    // APPROVE ADVERTISEMENT
    // ============================================================

    public async Task<bool>
        ApproveAdvertisementAsync(
            long adminUserId,
            long advertisementId,
            ApproveAdvertisementRequest request)
    {
        if (advertisementId <= 0)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(request);

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}/approve";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"ADMIN MODERATION - APPROVE: " +
            $"{advertisementId}");

        Console.WriteLine(
            $"POST: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PostAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"APPROVE STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"APPROVE RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        return response.IsSuccessStatusCode;
    }

    // ============================================================
    // REJECT ADVERTISEMENT
    // ============================================================

    public async Task<bool>
        RejectAdvertisementAsync(
            long adminUserId,
            long advertisementId,
            RejectAdvertisementRequest request)
    {
        if (advertisementId <= 0)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(request);

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}/reject";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"ADMIN MODERATION - REJECT: " +
            $"{advertisementId}");

        Console.WriteLine(
            $"POST: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PostAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"REJECT STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"REJECT RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        return response.IsSuccessStatusCode;
    }

    // ============================================================
    // SUSPEND ADVERTISEMENT
    // ============================================================

    public async Task<bool>
        SuspendAdvertisementAsync(
            long adminUserId,
            long advertisementId,
            SuspendAdvertisementRequest request)
    {
        if (advertisementId <= 0)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(request);

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}/suspend";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"ADMIN MODERATION - SUSPEND: " +
            $"{advertisementId}");

        Console.WriteLine(
            $"POST: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.PostAsJsonAsync(
                url,
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"SUSPEND STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"SUSPEND RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        return response.IsSuccessStatusCode;
    }

    // ============================================================
    // REACTIVATE ADVERTISEMENT
    // ============================================================

    public async Task<bool>
        ReactivateAdvertisementAsync(
            long adminUserId,
            long advertisementId,
            string? comments)
    {
        if (advertisementId <= 0)
        {
            return false;
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}/reactivate";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"ADMIN MODERATION - REACTIVATE: " +
            $"{advertisementId}");

        Console.WriteLine(
            $"POST: {url}");

        Console.WriteLine(
            "================================================");

        /*
         * The API action expects:
         *
         * [FromBody] string? comments
         *
         * Therefore we send a JSON string rather than
         * an anonymous/object wrapper.
         */

        var response =
            await client.PostAsJsonAsync(
                url,
                comments);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"REACTIVATE STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        Console.WriteLine(
            $"REACTIVATE RESPONSE LENGTH: " +
            $"{responseBody.Length}");

        return response.IsSuccessStatusCode;
    }

    // ============================================================
    // GET APPROVAL HISTORY
    // ============================================================

    public async Task<List<ApprovalHistoryResponse>>
        GetApprovalHistoryAsync(
            long advertisementId)
    {
        if (advertisementId <= 0)
        {
            return new List<ApprovalHistoryResponse>();
        }

        var client =
            CreateClient();

        var url =
            $"api/v1/admin/advertisements/" +
            $"{advertisementId}/approval-history";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"ADMIN MODERATION - GET APPROVAL HISTORY: " +
            $"{advertisementId}");

        Console.WriteLine(
            $"GET: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"APPROVAL HISTORY STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve approval history. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new List<ApprovalHistoryResponse>();
        }

        return JsonSerializer.Deserialize<
            List<ApprovalHistoryResponse>>(
            responseBody,
            JsonOptions)
            ?? new List<ApprovalHistoryResponse>();
    }

    // ============================================================
    // GET REJECTION REASONS
    // ============================================================

    public async Task<List<RejectionReasonResponse>>
        GetRejectionReasonsAsync()
    {
        var client =
            CreateClient();

        const string url =
            "api/v1/admin/advertisements/rejection-reasons";

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN MODERATION - GET REJECTION REASONS");

        Console.WriteLine(
            $"GET: {url}");

        Console.WriteLine(
            "================================================");

        var response =
            await client.GetAsync(url);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"REJECTION REASONS STATUS: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve rejection reasons. " +
                $"HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new List<RejectionReasonResponse>();
        }

        return JsonSerializer.Deserialize<
            List<RejectionReasonResponse>>(
            responseBody,
            JsonOptions)
            ?? new List<RejectionReasonResponse>();
    }
}