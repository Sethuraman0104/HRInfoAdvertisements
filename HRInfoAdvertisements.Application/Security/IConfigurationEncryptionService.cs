namespace HRInfoAdvertisements.Application.Security;

public interface IConfigurationEncryptionService
{
    string Protect(string value);

    string Unprotect(string protectedValue);
}