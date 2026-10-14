using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Admin.Services;

public sealed class PackagePurchaseRequestAdminApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly AdminAuthenticationStateProvider _auth;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PackagePurchaseRequestAdminApiService(
        IHttpClientFactory factory,
        AdminAuthenticationStateProvider auth)
    {
        _factory = factory;
        _auth = auth;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("HRInfoAdvertisementsAPI");
        var token = _auth.AccessToken;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Administrator session is missing. Please sign in again.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public async Task<List<AdminPackagePurchaseRequestResponse>>
        GetActionableAsync()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(
            "api/v1/admin/package-purchase-requests/actionable");

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to load purchase requests. HTTP {(int)response.StatusCode}: {body}");
        }

        var envelope =
            JsonSerializer.Deserialize<ApiEnvelope<List<AdminPackagePurchaseRequestResponse>>>(
                body, JsonOptions);

        return envelope?.Data ?? new();
    }

    public Task ApproveAsync(long requestId) =>
        PostActionAsync(
            $"api/v1/admin/package-purchase-requests/{requestId}/approve");

    public Task RejectAsync(long requestId) =>
        PostActionAsync(
            $"api/v1/admin/package-purchase-requests/{requestId}/reject");

    private async Task PostActionAsync(string path)
    {
        using var client = CreateClient();
        using var response = await client.PostAsync(path, content: null);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"The purchase-request action failed. HTTP {(int)response.StatusCode}: {body}");
        }
    }

    public async Task ConfirmPaymentAsync(
        long requestId,
        ConfirmPackagePaymentRequest request)
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            $"api/v1/admin/package-purchase-requests/{requestId}/confirm-payment",
            request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Payment confirmation failed. HTTP {(int)response.StatusCode}: {body}");
        }
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}