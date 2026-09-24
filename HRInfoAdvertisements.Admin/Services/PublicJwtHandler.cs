using System.Net.Http.Headers;

namespace HRInfoAdvertisements.Admin.Services;

public class PublicJwtHandler : DelegatingHandler
{
    private readonly PublicAuthenticationStateProvider
        _authenticationStateProvider;

    public PublicJwtHandler(
        PublicAuthenticationStateProvider authenticationStateProvider)
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
            $"PUBLIC JWT AVAILABLE: {!string.IsNullOrWhiteSpace(accessToken)}");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            Console.WriteLine(
                "PUBLIC JWT ATTACHED TO REQUEST.");
        }
        else
        {
            Console.WriteLine(
                "PUBLIC JWT IS MISSING.");
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}