using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace HRInfoAdvertisements.Admin.Services;

public class PublicAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    private ClaimsPrincipal _currentUser =
        Anonymous;

    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _expiresAt;

    public string? AccessToken =>
        _accessToken;

    public string? RefreshToken =>
        _refreshToken;

    public DateTime ExpiresAt =>
        _expiresAt;

    public bool IsAuthenticated =>
        _currentUser.Identity?.IsAuthenticated == true;

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
        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "PUBLIC SIGN-IN CALLED");

        Console.WriteLine(
            $"PUBLIC SIGN-IN USER ID: {userId}");

        Console.WriteLine(
            $"PUBLIC SIGN-IN USER NAME: {userName}");

        Console.WriteLine(
            $"PUBLIC SIGN-IN ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(accessToken)}");

        Console.WriteLine(
            $"PUBLIC SIGN-IN ACCESS TOKEN LENGTH: " +
            $"{accessToken?.Length ?? 0}");

        Console.WriteLine(
            $"PUBLIC SIGN-IN REFRESH TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(refreshToken)}");

        Console.WriteLine(
            $"PUBLIC SIGN-IN EXPIRES AT: {expiresAt}");

        Console.WriteLine(
            "================================================");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Login succeeded, but the API did not return a valid access token.");
        }

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

        foreach (var role in roles ?? Enumerable.Empty<string>())
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        foreach (var permission in permissions ?? Enumerable.Empty<string>())
        {
            claims.Add(
                new Claim(
                    "permission",
                    permission));
        }

        var identity =
            new ClaimsIdentity(
                claims,
                authenticationType: "PublicJwt");

        _currentUser =
            new ClaimsPrincipal(identity);

        _accessToken =
            accessToken;

        _refreshToken =
            refreshToken;

        _expiresAt =
            expiresAt;

        Console.WriteLine(
            $"PUBLIC PROVIDER TOKEN AFTER SIGN-IN: " +
            $"{!string.IsNullOrWhiteSpace(_accessToken)}");

        Console.WriteLine(
            $"PUBLIC PROVIDER TOKEN LENGTH AFTER SIGN-IN: " +
            $"{_accessToken?.Length ?? 0}");

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        Console.WriteLine(
            "PUBLIC SIGN-OUT CALLED");

        _currentUser =
            Anonymous;

        _accessToken = null;

        _refreshToken = null;

        _expiresAt =
            default;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }
}