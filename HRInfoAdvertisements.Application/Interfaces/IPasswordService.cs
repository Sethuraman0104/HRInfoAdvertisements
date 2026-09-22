using HRInfoAdvertisements.Domain.Entities;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(
        User user,
        string hashedPassword,
        string providedPassword);
}