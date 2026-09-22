using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Settings;
using HRInfoAdvertisements.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(
        IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var expires =
            DateTime.UtcNow.AddMinutes(
                _settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.UserID.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.UserID.ToString()),

            new(
                ClaimTypes.Name,
                user.UserName),

            new(
                ClaimTypes.Email,
                user.Email)
        };

        foreach (var role in roles.Distinct())
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        foreach (var permission in permissions.Distinct())
        {
            claims.Add(
                new Claim(
                    "permission",
                    permission));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _settings.SecretKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            _settings.AccessTokenMinutes);
    }
}