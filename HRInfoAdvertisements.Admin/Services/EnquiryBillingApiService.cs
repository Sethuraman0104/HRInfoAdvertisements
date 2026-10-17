using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Admin.Services;

public sealed class EnquiryBillingApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PublicAuthenticationStateProvider _authenticationStateProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EnquiryBillingApiService(
        IHttpClientFactory httpClientFactory,
        PublicAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    private HttpClient CreateClient(bool requireAuthentication)
    {
        var client = _httpClientFactory.CreateClient(
            "HRInfoAdvertisementsPublicAPI");

        if (requireAuthentication)
        {
            var accessToken = _authenticationStateProvider.AccessToken;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException(
                    "Please sign in to access your package requests.");
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client;
    }

    public async Task<List<EnquiryPackageResponse>> GetAvailablePackagesAsync(
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(requireAuthentication: false);

        using var response = await client.GetAsync(
            "api/v1/billing/packages",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                BuildErrorMessage(
                    body,
                    "Unable to load the available packages.",
                    response.StatusCode));
        }

        return JsonSerializer.Deserialize<List<EnquiryPackageResponse>>(
                   body,
                   JsonOptions)
               ?? new List<EnquiryPackageResponse>();
    }

    public async Task<PackagePurchaseRequestResponse>
        CreatePurchaseRequestAsync(
            int enquiryPackageId,
            CancellationToken cancellationToken = default)
    {
        if (enquiryPackageId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(enquiryPackageId),
                "Select a valid package.");
        }

        using var client = CreateClient(requireAuthentication: true);

        using var response = await client.PostAsJsonAsync(
            "api/v1/billing/purchase-requests",
            new CreatePackagePurchaseRequest
            {
                EnquiryPackageID = enquiryPackageId
            },
            JsonOptions,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                BuildErrorMessage(
                    body,
                    "Unable to submit the package request.",
                    response.StatusCode),
                null,
                response.StatusCode);
        }

        var envelope = JsonSerializer.Deserialize<ApiEnvelope<PackagePurchaseRequestResponse>>(
            body,
            JsonOptions);

        if (envelope?.Data is not null)
        {
            return envelope.Data;
        }

        // Also accept a direct DTO response if the API response format changes.
        var directResponse =
            JsonSerializer.Deserialize<PackagePurchaseRequestResponse>(
                body,
                JsonOptions);

        if (directResponse is not null &&
            directResponse.PackagePurchaseRequestID > 0)
        {
            return directResponse;
        }

        throw new InvalidOperationException(
            "The request was submitted, but the API response could not be read. " +
            "Check My Package Requests before submitting again.");
    }

    public async Task<MyPackageBalanceResponse> GetMyPackageBalanceAsync(
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(requireAuthentication: true);

        using var response = await client.GetAsync(
            "api/v1/billing/purchase-requests/my-balance",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                BuildErrorMessage(
                    body,
                    "Unable to load your package balance.",
                    response.StatusCode),
                null,
                response.StatusCode);
        }

        return JsonSerializer.Deserialize<MyPackageBalanceResponse>(
                   body,
                   JsonOptions)
               ?? throw new InvalidOperationException(
                   "The package balance response could not be read.");
    }
    public async Task<List<PackagePurchaseRequestResponse>>
        GetMyPurchaseRequestsAsync(
            CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(requireAuthentication: true);

        using var response = await client.GetAsync(
            "api/v1/billing/purchase-requests/mine",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                BuildErrorMessage(
                    body,
                    "Unable to load your package requests.",
                    response.StatusCode),
                null,
                response.StatusCode);
        }

        return JsonSerializer.Deserialize<List<PackagePurchaseRequestResponse>>(
                   body,
                   JsonOptions)
               ?? new List<PackagePurchaseRequestResponse>();
    }

    private static string BuildErrorMessage(
        string body,
        string fallback,
        System.Net.HttpStatusCode statusCode)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);

                if (TryGetPropertyIgnoreCase(
                        document.RootElement,
                        "Message",
                        out var message) &&
                    message.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(message.GetString()))
                {
                    return message.GetString()!;
                }

                if (TryGetPropertyIgnoreCase(
                        document.RootElement,
                        "title",
                        out var title) &&
                    title.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(title.GetString()))
                {
                    return title.GetString()!;
                }
            }
            catch (JsonException)
            {
                // The response was not JSON; use the standard message below.
            }
        }

        if (statusCode == System.Net.HttpStatusCode.Conflict)
        {
            return "A package request could not be created because it conflicts " +
                   "with your current request. Check My Package Requests.";
        }

        if (statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return "Your session may have expired. Please sign in again.";
        }

        return $"{fallback} (HTTP {(int)statusCode}).";
    }

    private static bool TryGetPropertyIgnoreCase(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                        property.Name,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
    }
}
