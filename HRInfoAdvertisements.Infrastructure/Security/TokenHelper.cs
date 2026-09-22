using System.Security.Cryptography;
using System.Text;

namespace HRInfoAdvertisements.Infrastructure.Security;

public static class TokenHelper
{
    public static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(
        string token)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}