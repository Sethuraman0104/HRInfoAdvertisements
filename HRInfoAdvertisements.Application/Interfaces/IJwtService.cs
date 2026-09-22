using HRInfoAdvertisements.Domain.Entities;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);

    DateTime GetAccessTokenExpiration();
}