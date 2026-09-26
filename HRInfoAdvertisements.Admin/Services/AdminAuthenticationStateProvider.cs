using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace HRInfoAdvertisements.Admin.Services;

public class AdminAuthenticationStateProvider
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


    public override Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        return Task.FromResult(
            new AuthenticationState(
                _currentUser));
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
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException(
                "Access token cannot be empty.",
                nameof(accessToken));
        }


        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),

                new(
                    ClaimTypes.Name,
                    userName ?? string.Empty),

                new(
                    ClaimTypes.Email,
                    email ?? string.Empty)
            };


        foreach (var role in roles ?? Enumerable.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }


        foreach (
            var permission
            in permissions ?? Enumerable.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(permission))
            {
                claims.Add(
                    new Claim(
                        "permission",
                        permission));
            }
        }


        var identity =
            new ClaimsIdentity(
                claims,
                authenticationType: "AdminJwt");


        _currentUser =
            new ClaimsPrincipal(identity);


        _accessToken =
            accessToken;


        _refreshToken =
            refreshToken;


        _expiresAt =
            expiresAt;


        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "ADMIN AUTHENTICATION STATE PROVIDER");

        Console.WriteLine(
            "SIGN IN");

        Console.WriteLine(
            $"USER ID: {userId}");

        Console.WriteLine(
            $"USER NAME: {userName}");

        Console.WriteLine(
            $"AUTHENTICATED: " +
            $"{_currentUser.Identity?.IsAuthenticated}");

        Console.WriteLine(
            $"ACCESS TOKEN AVAILABLE: " +
            $"{!string.IsNullOrWhiteSpace(_accessToken)}");

        Console.WriteLine(
            $"ACCESS TOKEN LENGTH: " +
            $"{_accessToken?.Length ?? 0}");

        Console.WriteLine(
            $"EXPIRES AT: {_expiresAt}");

        Console.WriteLine(
            "================================================");


        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(
                    _currentUser)));
    }


    public void SignOut()
    {
        _currentUser =
            Anonymous;

        _accessToken =
            null;

        _refreshToken =
            null;

        _expiresAt =
            default;


        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(
                    _currentUser)));
    }
}