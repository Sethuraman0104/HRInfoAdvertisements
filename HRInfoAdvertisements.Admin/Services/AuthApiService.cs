using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using HRInfoAdvertisements.Application.DTOs.Authentication;

namespace HRInfoAdvertisements.Admin.Services;

public class AuthApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public AuthApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    // ============================================================
    // LOGIN
    // ============================================================

    public async Task<AuthResponse?> LoginAsync(
        string email,
        string password)
    {
        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/login",
                request);

        Console.WriteLine(
            $"LOGIN STATUS: {(int)response.StatusCode} {response.StatusCode}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                $"LOGIN RESPONSE: {responseBody}");
        }

        // ========================================================
        // IMPORTANT
        //
        // The API may return a structured AuthResponse even when
        // the authentication flow requires email verification.
        // Always try to deserialize the response before returning
        // null for a non-success HTTP status.
        // ========================================================

        AuthResponse? result = null;

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                result =
                    JsonSerializer.Deserialize<AuthResponse>(
                        responseBody,
                        JsonOptions);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"LOGIN RESPONSE DESERIALIZATION ERROR: {ex.Message}");
            }
        }

        if (result is null)
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"LOGIN ERROR STATUS: {(int)response.StatusCode} {response.StatusCode}");
            }

            return null;
        }

        Console.WriteLine(
            $"LOGIN SUCCESS: {result.Success}");

        Console.WriteLine(
            $"LOGIN MESSAGE: {result.Message}");

        Console.WriteLine(
            $"LOGIN USER ID: {result.UserID}");

        Console.WriteLine(
            $"LOGIN USER NAME: {result.UserName}");

        Console.WriteLine(
            $"LOGIN EMAIL: {result.Email}");

        Console.WriteLine(
            $"LOGIN REQUIRES EMAIL VERIFICATION: {result.RequiresEmailVerification}");

        // ========================================================
        // Do NOT print access-token or refresh-token values.
        // We only report whether they exist.
        // ========================================================

        Console.WriteLine(
            $"LOGIN ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(result.AccessToken)}");

        Console.WriteLine(
            $"LOGIN REFRESH TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(result.RefreshToken)}");

        Console.WriteLine(
            $"LOGIN EXPIRES AT: {result.ExpiresAt}");

        return result;
    }


    // ============================================================
    // REGISTER
    // ============================================================

    public async Task<AuthResponse?> RegisterAsync(
        RegisterRequest request)
    {
        if (request is null)
        {
            return null;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/register",
                request);

        Console.WriteLine(
            $"REGISTER STATUS: {(int)response.StatusCode} {response.StatusCode}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                $"REGISTER RESPONSE: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AuthResponse>(
                responseBody,
                JsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"REGISTER RESPONSE DESERIALIZATION ERROR: {ex.Message}");

            return null;
        }
    }


    // ============================================================
    // VERIFY EMAIL OTP
    // ============================================================

    public async Task<AuthResponse?> VerifyEmailOtpAsync(
        string email,
        string otp)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(otp))
        {
            return null;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var request = new VerifyEmailOtpRequest
        {
            Email = email.Trim(),
            OTP = otp.Trim()
        };

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/verify-email",
                request);

        Console.WriteLine(
            $"VERIFY EMAIL STATUS: " +
            $"{(int)response.StatusCode} {response.StatusCode}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                $"VERIFY EMAIL RESPONSE: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AuthResponse>(
                responseBody,
                JsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"VERIFY EMAIL RESPONSE DESERIALIZATION ERROR: {ex.Message}");

            return null;
        }
    }


    // ============================================================
    // RESEND EMAIL VERIFICATION OTP
    // ============================================================

    public async Task<bool> ResendEmailVerificationAsync(
        string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var request =
            new ResendEmailVerificationRequest
            {
                Email = email.Trim()
            };

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/resend-email-verification",
                request);

        Console.WriteLine(
            $"RESEND EMAIL VERIFICATION STATUS: " +
            $"{(int)response.StatusCode} {response.StatusCode}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                $"RESEND EMAIL VERIFICATION RESPONSE: {responseBody}");
        }

        return response.IsSuccessStatusCode;
    }


    // ============================================================
    // REFRESH TOKEN
    // ============================================================

    public async Task<AuthResponse?> RefreshTokenAsync(
        string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/refresh",
                request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>(
                JsonOptions);
    }


    // ============================================================
    // CURRENT USER
    // ============================================================

    public async Task<AuthResponse?> GetCurrentUserAsync(
        string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/v1/Auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var response =
            await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<AuthResponse>(
                JsonOptions);
    }


    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    public async Task<bool> ChangePasswordAsync(
        string accessToken,
        ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        if (request is null)
        {
            return false;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/v1/Auth/change-password");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        httpRequest.Content =
            JsonContent.Create(request);

        var response =
            await client.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"CHANGE PASSWORD STATUS: " +
            $"{(int)response.StatusCode} {response.StatusCode}");

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            Console.WriteLine(
                $"CHANGE PASSWORD RESPONSE: {responseBody}");
        }

        return response.IsSuccessStatusCode;
    }


    // ============================================================
    // LOGOUT
    // ============================================================

    public async Task<bool> LogoutAsync(
        string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var client =
            _httpClientFactory.CreateClient(
                "HRInfoAdvertisementsAPI");

        var request =
            new
            {
                RefreshToken = refreshToken
            };

        var response =
            await client.PostAsJsonAsync(
                "api/v1/Auth/logout",
                request);

        return response.IsSuccessStatusCode;
    }
}