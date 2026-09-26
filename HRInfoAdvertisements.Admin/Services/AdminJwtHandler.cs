using System.Net.Http.Headers;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminJwtHandler : DelegatingHandler
{
    private readonly AdminAuthenticationStateProvider
        _authenticationStateProvider;

    public AdminJwtHandler(
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider =
            authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken =
            _authenticationStateProvider.AccessToken;

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN JWT HANDLER");

        Console.WriteLine(
            $"REQUEST: {request.Method} {request.RequestUri}");

        Console.WriteLine(
            $"ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(accessToken)}");

        Console.WriteLine(
            $"ACCESS TOKEN LENGTH: " +
            $"{accessToken?.Length ?? 0}");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            Console.WriteLine(
                "AUTHORIZATION HEADER: Bearer token attached.");
        }
        else
        {
            Console.WriteLine(
                "AUTHORIZATION HEADER: NO TOKEN AVAILABLE.");
        }

        Console.WriteLine(
            "================================================");

        var response =
            await base.SendAsync(
                request,
                cancellationToken);

        Console.WriteLine(
            $"ADMIN JWT HANDLER RESPONSE: " +
            $"{(int)response.StatusCode} " +
            $"{response.StatusCode}");

        return response;
    }
}