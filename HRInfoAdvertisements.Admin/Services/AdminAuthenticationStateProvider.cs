using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    private ClaimsPrincipal _currentUser = Anonymous;

    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _expiresAt;

    public string? AccessToken => _accessToken;

    public string? RefreshToken => _refreshToken;

    public DateTime ExpiresAt => _expiresAt;

    public override Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        return Task.FromResult(
            new AuthenticationState(_currentUser));
    }

    public void SignIn(
        long userId,
        string userName,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        string accessToken,
        string refreshToken,
        DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Name,
                userName),

            new(
                ClaimTypes.Email,
                email)
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(
                new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "AdminJwt");

        _currentUser = new ClaimsPrincipal(identity);

        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _expiresAt = expiresAt;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _currentUser = Anonymous;

        _accessToken = null;
        _refreshToken = null;
        _expiresAt = default;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }
}