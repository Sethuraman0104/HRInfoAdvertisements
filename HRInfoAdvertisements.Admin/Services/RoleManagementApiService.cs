using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.AdminManagement;

namespace HRInfoAdvertisements.Admin.Services;

public class RoleManagementApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdminAuthenticationStateProvider _authenticationStateProvider;

    public RoleManagementApiService(
        IHttpClientFactory httpClientFactory,
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    private HttpClient CreateClient()
    {
        var client =
            _httpClientFactory.CreateClient("HRInfoAdvertisementsAPI");

        var token =
            _authenticationStateProvider.AccessToken;

        client.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue(
                    "Bearer",
                    token);

        return client;
    }

    // ============================================================
    // GET ALL ROLES
    // ============================================================

    public async Task<List<RoleListResponse>> GetRolesAsync()
    {
        var client = CreateClient();

        var result =
            await client.GetFromJsonAsync<List<RoleListResponse>>(
                "api/v1/roles");

        return result ?? new List<RoleListResponse>();
    }

    // ============================================================
    // GET ROLE BY ID
    // ============================================================

    public async Task<RoleDetailResponse?> GetRoleByIdAsync(
        int roleId)
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<RoleDetailResponse>(
            $"api/v1/roles/{roleId}");
    }

    // ============================================================
    // CREATE ROLE
    // ============================================================

    public async Task<RoleApiResult> CreateRoleAsync(
        CreateRoleRequest request)
    {
        var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "api/v1/roles",
                request);

        return await ReadResultAsync(
            response,
            "Role created successfully.");
    }

    // ============================================================
    // UPDATE ROLE
    // ============================================================

    public async Task<RoleApiResult> UpdateRoleAsync(
        int roleId,
        UpdateRoleRequest request)
    {
        var client = CreateClient();

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/roles/{roleId}",
                request);

        return await ReadResultAsync(
            response,
            "Role updated successfully.");
    }

    // ============================================================
    // UPDATE ROLE STATUS
    // ============================================================

    public async Task<RoleApiResult> UpdateRoleStatusAsync(
        int roleId,
        bool isActive)
    {
        var client = CreateClient();

        var request =
            new UpdateRoleStatusRequest
            {
                IsActive = isActive
            };

        var response =
            await client.PutAsJsonAsync(
                $"api/v1/roles/{roleId}/status",
                request);

        return await ReadResultAsync(
            response,
            isActive
                ? "Role activated successfully."
                : "Role deactivated successfully.");
    }

    // ============================================================
    // READ API RESULT
    // ============================================================

    private static async Task<RoleApiResult> ReadResultAsync(
        HttpResponseMessage response,
        string defaultSuccessMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result =
                    await response.Content.ReadFromJsonAsync<RoleApiResponse>();

                return new RoleApiResult
                {
                    Success = true,
                    Message =
                        string.IsNullOrWhiteSpace(result?.Message)
                            ? defaultSuccessMessage
                            : result.Message
                };
            }
            catch
            {
                return new RoleApiResult
                {
                    Success = true,
                    Message = defaultSuccessMessage
                };
            }
        }

        var errorMessage =
            await ExtractErrorMessageAsync(response);

        return new RoleApiResult
        {
            Success = false,
            Message = errorMessage
        };
    }

    // ============================================================
    // EXTRACT API ERROR
    // ============================================================

    private static async Task<string> ExtractErrorMessageAsync(
        HttpResponseMessage response)
    {
        try
        {
            var apiError =
                await response.Content
                    .ReadFromJsonAsync<RoleApiResponse>();

            if (!string.IsNullOrWhiteSpace(apiError?.Message))
                return apiError.Message;
        }
        catch
        {
            // Ignore JSON parsing errors and use fallback below.
        }

        try
        {
            var problem =
                await response.Content
                    .ReadFromJsonAsync<ValidationProblemResponse>();

            if (problem?.Errors != null)
            {
                var messages =
                    problem.Errors
                        .SelectMany(x => x.Value ?? Array.Empty<string>())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }
        }
        catch
        {
            // Ignore and use generic message.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized =>
                "You are not authorized to perform this action.",

            System.Net.HttpStatusCode.Forbidden =>
                "You do not have permission to perform this action.",

            System.Net.HttpStatusCode.NotFound =>
                "The requested role was not found.",

            System.Net.HttpStatusCode.Conflict =>
                "The role already exists.",

            System.Net.HttpStatusCode.BadRequest =>
                "The request could not be processed.",

            _ =>
                $"Unable to complete the request. HTTP {(int)response.StatusCode}."
        };
    }
}


// ================================================================
// ROLE API RESULT
// ================================================================

public class RoleApiResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}


// ================================================================
// API RESPONSE
// ================================================================

internal class RoleApiResponse
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public int? RoleID { get; set; }

    public int? RoleId { get; set; }

    public int? PermissionID { get; set; }

    public int? PermissionId { get; set; }
}


// ================================================================
// VALIDATION PROBLEM RESPONSE
// ================================================================

internal class ValidationProblemResponse
{
    public Dictionary<string, string[]>? Errors { get; set; }
}