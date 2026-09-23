using System.Net.Http.Headers;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminJwtHandler : DelegatingHandler
{
    private readonly AdminAuthenticationStateProvider _authenticationStateProvider;

    public AdminJwtHandler(
        AdminAuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken =
            _authenticationStateProvider.AccessToken;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}