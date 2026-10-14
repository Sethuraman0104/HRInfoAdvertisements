
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Admin.Services;

public sealed class EnquiryPackageAdminApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly AdminAuthenticationStateProvider _auth;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EnquiryPackageAdminApiService(
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
            throw new UnauthorizedAccessException(
                "Administrator session is missing. Please sign in again.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public async Task<List<EnquiryPackageResponse>> GetAllAsync()
    {
        using var response = await CreateClient().GetAsync(
            "api/v1/admin/enquiry-packages");

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Unable to load packages. HTTP {(int)response.StatusCode}: {body}");

        var envelope = JsonSerializer.Deserialize<ApiEnvelope<List<EnquiryPackageResponse>>>(
            body, JsonOptions);

        return envelope?.Data ?? new();
    }

    public async Task<EnquiryPackageResponse> UpdateAsync(
        int packageId,
        UpdateEnquiryPackageRequest request)
    {
        using var response = await CreateClient().PutAsJsonAsync(
            $"api/v1/admin/enquiry-packages/{packageId}", request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Unable to save package. HTTP {(int)response.StatusCode}: {body}");

        var envelope = JsonSerializer.Deserialize<ApiEnvelope<EnquiryPackageResponse>>(
            body, JsonOptions);

        return envelope?.Data
            ?? throw new InvalidOperationException(
                "The API saved the request but returned no package data.");
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
