using HRInfoAdvertisements.Application.DTOs.Dashboard;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class DashboardStatisticsService : IDashboardStatisticsService
{
    private readonly ApplicationDbContext _context;

    public DashboardStatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatisticsResponse> GetStatisticsAsync(
        DashboardStatisticsRequest request)
    {
        var fromDate = request.FromDate?.Date
            ?? DateTime.UtcNow.Date.AddDays(-29);

        var toDateExclusive = request.ToDate?.Date.AddDays(1)
            ?? DateTime.UtcNow.Date.AddDays(1);

        if (fromDate >= toDateExclusive)
        {
            throw new ArgumentException(
                "FromDate must be earlier than ToDate.");
        }

        var groupBy = string.IsNullOrWhiteSpace(request.GroupBy)
            ? "day"
            : request.GroupBy.Trim().ToLowerInvariant();

        if (groupBy is not ("day" or "week" or "month"))
        {
            throw new ArgumentException(
                "GroupBy must be day, week, or month.");
        }

        var users = await _context.Users
            .AsNoTracking()
            .Where(x =>
                x.CreatedDate >= fromDate &&
                x.CreatedDate < toDateExclusive)
            .Select(x => x.CreatedDate)
            .ToListAsync();

        var advertisements = await _context.Advertisements
            .AsNoTracking()
            .Where(x =>
                (x.CreatedDate >= fromDate &&
                 x.CreatedDate < toDateExclusive)
                ||
                (x.PublishedDate.HasValue &&
                 x.PublishedDate.Value >= fromDate &&
                 x.PublishedDate.Value < toDateExclusive))
            .Select(x => new
            {
                x.CreatedDate,
                x.PublishedDate
            })
            .ToListAsync();

        var views = await _context.AdvertisementViews
            .AsNoTracking()
            .Where(x =>
                x.ViewedDate >= fromDate &&
                x.ViewedDate < toDateExclusive)
            .Select(x => x.ViewedDate)
            .ToListAsync();

        var favorites = await _context.Favorites
            .AsNoTracking()
            .Where(x =>
                x.CreatedDate >= fromDate &&
                x.CreatedDate < toDateExclusive)
            .Select(x => x.CreatedDate)
            .ToListAsync();

        var enquiries = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .Where(x =>
                x.CreatedDate >= fromDate &&
                x.CreatedDate < toDateExclusive)
            .Select(x => x.CreatedDate)
            .ToListAsync();

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(x =>
                x.CreatedDate >= fromDate &&
                x.CreatedDate < toDateExclusive)
            .Select(x => x.CreatedDate)
            .ToListAsync();

        var reports = await _context.Reports
            .AsNoTracking()
            .Where(x =>
                x.CreatedDate >= fromDate &&
                x.CreatedDate < toDateExclusive)
            .Select(x => x.CreatedDate)
            .ToListAsync();

        var userCounts = GroupDates(users, groupBy);

        var advertisementCounts = GroupDates(
            advertisements.Select(x => x.CreatedDate),
            groupBy);

        var publishedAdvertisementCounts = GroupDates(
            advertisements
                .Where(x => x.PublishedDate.HasValue)
                .Select(x => x.PublishedDate!.Value),
            groupBy);

        var viewCounts = GroupDates(views, groupBy);
        var favoriteCounts = GroupDates(favorites, groupBy);
        var enquiryCounts = GroupDates(enquiries, groupBy);
        var messageCounts = GroupDates(messages, groupBy);
        var reportCounts = GroupDates(reports, groupBy);

        var periods = BuildPeriods(
            fromDate,
            toDateExclusive,
            groupBy);

        var items = periods
            .Select(period =>
            {
                var key = GetPeriodKey(period, groupBy);

                return new DashboardStatisticsItemResponse
                {
                    Period = FormatPeriod(period, groupBy),

                    NewUsers = GetCount(userCounts, key),

                    NewAdvertisements =
                        GetCount(advertisementCounts, key),

                    PublishedAdvertisements =
                        GetCount(
                            publishedAdvertisementCounts,
                            key),

                    AdvertisementViews =
                        GetCount(viewCounts, key),

                    Favorites =
                        GetCount(favoriteCounts, key),

                    Enquiries =
                        GetCount(enquiryCounts, key),

                    Messages =
                        GetCount(messageCounts, key),

                    Reports =
                        GetCount(reportCounts, key)
                };
            })
            .ToList();

        return new DashboardStatisticsResponse
        {
            FromDate = fromDate,
            ToDate = toDateExclusive.AddDays(-1),
            GroupBy = groupBy,
            Items = items
        };
    }

    private static Dictionary<DateTime, int> GroupDates(
        IEnumerable<DateTime> dates,
        string groupBy)
    {
        return dates
            .GroupBy(x => GetPeriodKey(x, groupBy))
            .ToDictionary(
                x => x.Key,
                x => x.Count());
    }

    private static int GetCount(
        Dictionary<DateTime, int> counts,
        DateTime key)
    {
        return counts.TryGetValue(key, out var count)
            ? count
            : 0;
    }

    private static List<DateTime> BuildPeriods(
        DateTime fromDate,
        DateTime toDateExclusive,
        string groupBy)
    {
        var periods = new List<DateTime>();

        var current = GetPeriodStart(fromDate, groupBy);

        while (current < toDateExclusive)
        {
            periods.Add(current);
            current = AddPeriod(current, groupBy);
        }

        return periods;
    }

    private static DateTime GetPeriodKey(
        DateTime date,
        string groupBy)
    {
        return GetPeriodStart(date, groupBy);
    }

    private static DateTime GetPeriodStart(
        DateTime date,
        string groupBy)
    {
        return groupBy switch
        {
            "day" => date.Date,

            "week" => StartOfWeek(date.Date),

            "month" => new DateTime(
                date.Year,
                date.Month,
                1),

            _ => date.Date
        };
    }

    private static DateTime AddPeriod(
        DateTime date,
        string groupBy)
    {
        return groupBy switch
        {
            "day" => date.AddDays(1),
            "week" => date.AddDays(7),
            "month" => date.AddMonths(1),
            _ => date.AddDays(1)
        };
    }

    private static string FormatPeriod(
        DateTime period,
        string groupBy)
    {
        return groupBy switch
        {
            "day" => period.ToString("yyyy-MM-dd"),
            "week" => period.ToString("yyyy-MM-dd"),
            "month" => period.ToString("yyyy-MM"),
            _ => period.ToString("yyyy-MM-dd")
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var difference =
            (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

        return date.AddDays(-difference).Date;
    }
}