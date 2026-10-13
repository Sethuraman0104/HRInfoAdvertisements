using HRInfoAdvertisements.Application.Enquiries.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Enquiries;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Tests;

public class AdvertisementEnquiryValidationTests
{
    private static (ApplicationDbContext Context, AdvertisementEnquiryService Service)
        CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        return (context, new AdvertisementEnquiryService(context));
    }

    private static CreateAdvertisementEnquiryRequest ValidRequest(long advertisementId = 1)
        => new()
        {
            AdvertisementID = advertisementId,
            Subject = "Test enquiry",
            Message = "Test message"
        };

    private static User CreateUser(long id, string name)
        => new()
        {
            UserID = id,
            UserName = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            PasswordHash = "test-hash",
            CreatedDate = DateTime.UtcNow
        };

    private static AdvertisementStatus CreateStatus(int id, string code)
        => new()
        {
            StatusID = id,
            StatusCode = code,
            StatusName = code,
            IsActive = true
        };

    private static Advertisement CreateAdvertisement(
        long id,
        long ownerId,
        int statusId)
        => new()
        {
            AdvertisementID = id,
            UserID = ownerId,
            CategoryID = 1,
            AdvertisementTypeID = 1,
            StatusID = statusId,
            AdvertisementNumber = $"TEST-{id}",
            Title = "Test advertisement",
            Description = "Test description",
            CreatedDate = DateTime.UtcNow
        };

    [Fact]
    public async Task CreateAsync_RejectsInvalidAdvertisementId()
    {
        var (_, service) = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(1, ValidRequest(0)));

        Assert.Contains("AdvertisementID", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsBlankSubject()
    {
        var (_, service) = CreateService();
        var request = ValidRequest();
        request.Subject = " ";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(1, request));

        Assert.Contains("Subject", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsBlankMessage()
    {
        var (_, service) = CreateService();
        var request = ValidRequest();
        request.Message = " ";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(1, request));

        Assert.Contains("Message", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsMissingAdvertisement()
    {
        var (context, service) = CreateService();

        try
        {
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateAsync(1, ValidRequest(999)));

            Assert.Equal("Advertisement not found.", exception.Message);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateAsync_RejectsUnpublishedAdvertisement()
    {
        var (context, service) = CreateService();

        try
        {
            context.Users.Add(CreateUser(2, "owner"));
            context.AdvertisementStatuses.Add(CreateStatus(1, "DRAFT"));
            context.Advertisements.Add(CreateAdvertisement(10, 2, 1));
            await context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(1, ValidRequest(10)));

            Assert.Contains("published advertisements", exception.Message);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }

    [Fact]
    public async Task CreateAsync_RejectsOwnAdvertisement()
    {
        var (context, service) = CreateService();

        try
        {
            context.Users.Add(CreateUser(2, "owner"));
            context.AdvertisementStatuses.Add(CreateStatus(1, "PUBLISHED"));
            context.Advertisements.Add(CreateAdvertisement(10, 2, 1));
            await context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(2, ValidRequest(10)));

            Assert.Contains("own advertisement", exception.Message);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }
}
