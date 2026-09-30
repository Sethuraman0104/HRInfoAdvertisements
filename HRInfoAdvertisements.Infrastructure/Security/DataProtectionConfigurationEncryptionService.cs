using HRInfoAdvertisements.Application.Security;
using Microsoft.AspNetCore.DataProtection;

namespace HRInfoAdvertisements.Infrastructure.Security;

public sealed class DataProtectionConfigurationEncryptionService
    : IConfigurationEncryptionService
{
    private readonly IDataProtector _protector;

    public DataProtectionConfigurationEncryptionService(
        IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(
            "HRInfoAdvertisements.SystemSettings.v1");
    }

    public string Protect(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return _protector.Protect(value);
    }

    public string Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            return string.Empty;
        }

        return _protector.Unprotect(protectedValue);
    }
}