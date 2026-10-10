using HRInfoAdvertisements.Application.DTOs.Settings;
using HRInfoAdvertisements.Domain.Entities;

using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Services;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HRInfoAdvertisements.Tests;

public class FavoriteAdvertisementNotificationTests
{
    [Fact]
    public async Task DoesNotSendEmail_WhenAutomatedEmailsAreDisabled()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService =
            new Mock<IApplicationSettingsService>();

        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = false,
                FavoriteAdvertisementEmailEnabled = true
            });

        var emailService = new Mock<IEmailService>();

        var logger =
            Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        await service.SendFavoriteAdvertisementUpdatedAsync(123);

        emailService.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenFavoriteUpdatesAreDisabled()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true,
                SiteName = "AdMind"
            });

        var emailService = new Mock<IEmailService>();
        var logger = Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        var status = new HRInfoAdvertisements.Domain.Entities.AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var user = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 10,
            UserName = "Test User",
            Email = "test@example.com",
            NotificationPreference = new HRInfoAdvertisements.Domain.Entities.NotificationPreference
            {
                NotificationPreferenceID = 1,
                UserID = 10,
                EmailNotificationsEnabled = true,
                FavoriteAdvertisementUpdatesEnabled = false
            }
        };

        var advertisement = new HRInfoAdvertisements.Domain.Entities.Advertisement
        {
            AdvertisementID = 100,
            UserID = 20,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = 1,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            User = new HRInfoAdvertisements.Domain.Entities.User
            {
                UserID = 20,
                UserName = "Advertiser",
                Email = "advertiser@example.com"
            },
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Advertisements.Add(advertisement);
        context.Users.Add(user);

        context.AdvertisementFavorites.Add(
            new HRInfoAdvertisements.Domain.Entities.AdvertisementFavorite
            {
                AdvertisementFavoriteID = 1,
                UserID = 10,
                AdvertisementID = 100,
                CreatedDate = DateTime.UtcNow,
                User = user,
                Advertisement = advertisement
            });

        await context.SaveChangesAsync();

        await service.SendFavoriteAdvertisementUpdatedAsync(100);

        emailService.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendsEmail_WhenFavoriteUpdatesAreEnabled()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();

        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true,
                SiteName = "AdMind"
            });

        settingsService
            .Setup(x => x.GetStringAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) =>
                key == "SITE_URL" ? "https://example.com" : "");

        var emailService = new Mock<IEmailService>();
        var logger = Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        var status = new HRInfoAdvertisements.Domain.Entities.AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var advertiser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var favoriteUser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 10,
            UserName = "Favorite User",
            Email = "favorite@example.com",
            NotificationPreference =
                new HRInfoAdvertisements.Domain.Entities.NotificationPreference
                {
                    NotificationPreferenceID = 1,
                    UserID = 10,
                    EmailNotificationsEnabled = true,
                    FavoriteAdvertisementUpdatesEnabled = true
                }
        };

        var advertisement = new HRInfoAdvertisements.Domain.Entities.Advertisement
        {
            AdvertisementID = 100,
            UserID = 20,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = 1,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            User = advertiser,
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Users.AddRange(advertiser, favoriteUser);
        context.Advertisements.Add(advertisement);

        context.AdvertisementFavorites.Add(
            new HRInfoAdvertisements.Domain.Entities.AdvertisementFavorite
            {
                AdvertisementFavoriteID = 1,
                UserID = 10,
                AdvertisementID = 100,
                CreatedDate = DateTime.UtcNow,
                User = favoriteUser,
                Advertisement = advertisement
            });

        await context.SaveChangesAsync();

        await service.SendFavoriteAdvertisementUpdatedAsync(100);

        emailService.Verify(
            x => x.SendAsync(
                "favorite@example.com",
                It.Is<string>(subject =>
                    subject.Contains("favorited advertisement has been updated")),
                It.Is<string>(body =>
                    body.Contains("Test advertisement") &&
                    body.Contains("https://example.com/advertisements/100"))),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenAdvertisementIsNotPublished()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true
            });

        var emailService = new Mock<IEmailService>();
        var logger = Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        var status = new HRInfoAdvertisements.Domain.Entities.AdvertisementStatus
        {
            StatusID = 2,
            StatusCode = "DRAFT",
            StatusName = "Draft"
        };

        var advertiser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var advertisement = new HRInfoAdvertisements.Domain.Entities.Advertisement
        {
            AdvertisementID = 200,
            UserID = 20,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = 2,
            AdvertisementNumber = "TEST-200",
            Title = "Draft advertisement",
            Description = "Test description",
            User = advertiser,
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Users.Add(advertiser);
        context.Advertisements.Add(advertisement);
        await context.SaveChangesAsync();

        await service.SendFavoriteAdvertisementUpdatedAsync(200);

        emailService.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenRecipientDisablesAllEmailNotifications()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true
            });

        settingsService
            .Setup(x => x.GetStringAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) =>
                key == "SITE_URL" ? "https://example.com" : "");

        var emailService = new Mock<IEmailService>();
        var logger = Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        var status = new HRInfoAdvertisements.Domain.Entities.AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var advertiser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var favoriteUser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 10,
            UserName = "Favorite User",
            Email = "favorite@example.com",
            NotificationPreference =
                new HRInfoAdvertisements.Domain.Entities.NotificationPreference
                {
                    NotificationPreferenceID = 1,
                    UserID = 10,
                    EmailNotificationsEnabled = false,
                    FavoriteAdvertisementUpdatesEnabled = true
                }
        };

        var advertisement = new HRInfoAdvertisements.Domain.Entities.Advertisement
        {
            AdvertisementID = 100,
            UserID = 20,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = 1,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            User = advertiser,
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Users.AddRange(advertiser, favoriteUser);
        context.Advertisements.Add(advertisement);

        context.AdvertisementFavorites.Add(
            new HRInfoAdvertisements.Domain.Entities.AdvertisementFavorite
            {
                AdvertisementFavoriteID = 1,
                UserID = 10,
                AdvertisementID = 100,
                CreatedDate = DateTime.UtcNow,
                User = favoriteUser,
                Advertisement = advertisement
            });

        await context.SaveChangesAsync();

        await service.SendFavoriteAdvertisementUpdatedAsync(100);

        emailService.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenRecipientEmailIsBlank()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true
            });

        settingsService
            .Setup(x => x.GetStringAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) =>
                key == "SITE_URL" ? "https://example.com" : "");

        var emailService = new Mock<IEmailService>();
        var logger = Mock.Of<ILogger<MarketplaceEmailNotificationService>>();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger);

        var status = new HRInfoAdvertisements.Domain.Entities.AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var advertiser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var favoriteUser = new HRInfoAdvertisements.Domain.Entities.User
        {
            UserID = 10,
            UserName = "Favorite User",
            Email = "   ",
            NotificationPreference =
                new HRInfoAdvertisements.Domain.Entities.NotificationPreference
                {
                    NotificationPreferenceID = 1,
                    UserID = 10,
                    EmailNotificationsEnabled = true,
                    FavoriteAdvertisementUpdatesEnabled = true
                }
        };

        var advertisement = new HRInfoAdvertisements.Domain.Entities.Advertisement
        {
            AdvertisementID = 100,
            UserID = 20,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = 1,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            User = advertiser,
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Users.AddRange(advertiser, favoriteUser);
        context.Advertisements.Add(advertisement);

        context.AdvertisementFavorites.Add(
            new HRInfoAdvertisements.Domain.Entities.AdvertisementFavorite
            {
                AdvertisementFavoriteID = 1,
                UserID = 10,
                AdvertisementID = 100,
                CreatedDate = DateTime.UtcNow,
                User = favoriteUser,
                Advertisement = advertisement
            });

        await context.SaveChangesAsync();

        await service.SendFavoriteAdvertisementUpdatedAsync(100);

        emailService.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ContinuesSendingEmails_WhenOneRecipientEmailFails()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settings = new ApplicationSettings
        {
            EmailEnabled = true,
            AutomatedEmailEnabled = true,
            FavoriteAdvertisementEmailEnabled = true
        };

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService
            .Setup(x => x.GetApplicationSettingsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        settingsService
            .Setup(x => x.GetStringAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) =>
                key == "SITE_URL" ? "https://example.com" : string.Empty);

        var emailService = new Mock<IEmailService>();

        emailService
            .Setup(x => x.SendAsync(
                "first@example.com",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(
                "Simulated email failure"));

        emailService
            .Setup(x => x.SendAsync(
                "second@example.com",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var logger =
            new Mock<ILogger<MarketplaceEmailNotificationService>>();

        var advertiser = new User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var firstUser = new User
        {
            UserID = 10,
            UserName = "First Recipient",
            Email = "first@example.com",
            NotificationPreference = new NotificationPreference
            {
                NotificationPreferenceID = 1,
                UserID = 10,
                EmailNotificationsEnabled = true,
                FavoriteAdvertisementUpdatesEnabled = true
            }
        };

        var secondUser = new User
        {
            UserID = 11,
            UserName = "Second Recipient",
            Email = "second@example.com",
            NotificationPreference = new NotificationPreference
            {
                NotificationPreferenceID = 2,
                UserID = 11,
                EmailNotificationsEnabled = true,
                FavoriteAdvertisementUpdatesEnabled = true
            }
        };

        var status = new AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var advertisement = new Advertisement
        {
            AdvertisementID = 100,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            UserID = advertiser.UserID,
            User = advertiser,
            StatusID = status.StatusID,
            Status = status
        };

        context.Users.AddRange(advertiser, firstUser, secondUser);
        context.AdvertisementStatuses.Add(status);
        context.Advertisements.Add(advertisement);

        context.AdvertisementFavorites.AddRange(
            new AdvertisementFavorite
            {
                AdvertisementFavoriteID = 1,
                UserID = firstUser.UserID,
                User = firstUser,
                AdvertisementID = advertisement.AdvertisementID,
                Advertisement = advertisement
            },
            new AdvertisementFavorite
            {
                AdvertisementFavoriteID = 2,
                UserID = secondUser.UserID,
                User = secondUser,
                AdvertisementID = advertisement.AdvertisementID,
                Advertisement = advertisement
            });

        await context.SaveChangesAsync();

        var service = new MarketplaceEmailNotificationService(
            context,
            settingsService.Object,
            emailService.Object,
            logger.Object);

        await service.SendFavoriteAdvertisementUpdatedAsync(100);

        emailService.Verify(
            x => x.SendAsync(
                "first@example.com",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);

        emailService.Verify(
            x => x.SendAsync(
                "second@example.com",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotThrow_WhenPublicationEmailFails()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var settingsService = new Mock<IApplicationSettingsService>();
        settingsService.Setup(x => x.GetApplicationSettingsAsync(
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationSettings
            {
                EmailEnabled = true,
                AutomatedEmailEnabled = true,
                AdvertisementEmailEnabled = true,
                FavoriteAdvertisementEmailEnabled = true,
                SiteName = "AdMind"
            });

        settingsService.Setup(x => x.GetStringAsync(
            "SITE_URL", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://example.com");
        settingsService.Setup(x => x.GetStringAsync(
            "EMAIL_LOGO_URL", It.IsAny<CancellationToken>()))
            .ReturnsAsync("");
        settingsService.Setup(x => x.GetStringAsync(
            "SUPPORT_EMAIL", It.IsAny<CancellationToken>()))
            .ReturnsAsync("");

        var emailService = new Mock<IEmailService>();
        emailService.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(
                "Simulated publication email failure"));

        var logger =
            new Mock<ILogger<MarketplaceEmailNotificationService>>();

        var status = new AdvertisementStatus
        {
            StatusID = 1,
            StatusCode = "PUBLISHED",
            StatusName = "Published"
        };

        var advertiser = new User
        {
            UserID = 20,
            UserName = "Advertiser",
            Email = "advertiser@example.com"
        };

        var advertisement = new Advertisement
        {
            AdvertisementID = 100,
            UserID = advertiser.UserID,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = status.StatusID,
            AdvertisementNumber = "TEST-100",
            Title = "Test advertisement",
            Description = "Test description",
            User = advertiser,
            Status = status
        };

        context.AdvertisementStatuses.Add(status);
        context.Users.Add(advertiser);
        context.Advertisements.Add(advertisement);
        await context.SaveChangesAsync();

        var service = new MarketplaceEmailNotificationService(
            context, settingsService.Object, emailService.Object, logger.Object);

        var exception = await Record.ExceptionAsync(
            () => service.SendAdvertisementPublishedAsync(100));

        Assert.Null(exception);

        emailService.Verify(x => x.SendAsync(
            "advertiser@example.com", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        logger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) =>
                state.ToString()!.Contains("Failed to send publication email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
