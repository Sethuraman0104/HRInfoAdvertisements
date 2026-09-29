using HRInfoAdvertisements.Application.DTOs.SystemSettings;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _context;

    public SystemSettingService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET SYSTEM SETTINGS
    // ============================================================

    public async Task<SystemSettingListResponse>
        GetSystemSettingsAsync(
            SystemSettingListRequest request)
    {
        var pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        var pageSize =
            request.PageSize switch
            {
                <= 0 => 50,
                > 100 => 100,
                _ => request.PageSize
            };

        var query =
            _context.SystemSettings
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search =
                request.Search.Trim();

            query = query.Where(x =>
                x.SettingKey.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == request.IsActive.Value);
        }

        var totalRecords =
            await query.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        if (totalPages > 0 &&
            pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var items =
            await query
                .OrderBy(x => x.SettingKey)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x =>
                    new SystemSettingListItemResponse
                    {
                        SystemSettingID =
                            x.SystemSettingID,

                        SettingKey =
                            x.SettingKey,

                        SettingValue =
                            x.SettingValue,

                        Description =
                            x.Description,

                        IsEncrypted =
                            x.IsEncrypted,

                        IsActive =
                            x.IsActive,

                        CreatedDate =
                            x.CreatedDate,

                        ModifiedDate =
                            x.ModifiedDate
                    })
                .ToListAsync();

        return new SystemSettingListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // ============================================================
    // GET SYSTEM SETTING BY ID
    // ============================================================

    public async Task<SystemSettingDetailResponse?>
        GetSystemSettingByIdAsync(
            int systemSettingId)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .Where(x =>
                x.SystemSettingID ==
                systemSettingId)
            .Select(x =>
                new SystemSettingDetailResponse
                {
                    SystemSettingID =
                        x.SystemSettingID,

                    SettingKey =
                        x.SettingKey,

                    SettingValue =
                        x.SettingValue,

                    Description =
                        x.Description,

                    IsEncrypted =
                        x.IsEncrypted,

                    IsActive =
                        x.IsActive,

                    CreatedDate =
                        x.CreatedDate,

                    ModifiedDate =
                        x.ModifiedDate
                })
            .FirstOrDefaultAsync();
    }

    // ============================================================
    // CREATE SYSTEM SETTING
    // ============================================================

    public async Task<int?>
        CreateSystemSettingAsync(
            CreateSystemSettingRequest request,
            long createdBy)
    {
        var settingKey =
            request.SettingKey.Trim();

        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return null;
        }

        var exists =
            await _context.SystemSettings
                .AnyAsync(x =>
                    x.SettingKey.ToLower() ==
                    settingKey.ToLower());

        if (exists)
        {
            return null;
        }

        var setting =
            new SystemSetting
            {
                SettingKey =
                    settingKey,

                SettingValue =
                    string.IsNullOrWhiteSpace(
                        request.SettingValue)
                        ? null
                        : request.SettingValue.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Description)
                        ? null
                        : request.Description.Trim(),

                IsEncrypted =
                    request.IsEncrypted,

                IsActive =
                    request.IsActive,

                CreatedDate =
                    DateTime.UtcNow
            };

        _context.SystemSettings.Add(setting);

        await _context.SaveChangesAsync();

        return setting.SystemSettingID;
    }

    // ============================================================
    // UPDATE SYSTEM SETTING
    // ============================================================

    public async Task<bool>
        UpdateSystemSettingAsync(
            int systemSettingId,
            UpdateSystemSettingRequest request,
            long modifiedBy)
    {
        var setting =
            await _context.SystemSettings
                .FirstOrDefaultAsync(x =>
                    x.SystemSettingID ==
                    systemSettingId);

        if (setting == null)
        {
            return false;
        }

        var settingKey =
            request.SettingKey.Trim();

        if (string.IsNullOrWhiteSpace(settingKey))
        {
            return false;
        }

        var duplicateKey =
            await _context.SystemSettings
                .AnyAsync(x =>
                    x.SystemSettingID !=
                        systemSettingId &&
                    x.SettingKey.ToLower() ==
                        settingKey.ToLower());

        if (duplicateKey)
        {
            return false;
        }

        setting.SettingKey =
            settingKey;

        setting.SettingValue =
            string.IsNullOrWhiteSpace(
                request.SettingValue)
                ? null
                : request.SettingValue.Trim();

        setting.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        setting.IsEncrypted =
            request.IsEncrypted;

        setting.IsActive =
            request.IsActive;

        setting.ModifiedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // UPDATE STATUS
    // ============================================================

    public async Task<bool>
        UpdateSystemSettingStatusAsync(
            int systemSettingId,
            bool isActive,
            long modifiedBy)
    {
        var setting =
            await _context.SystemSettings
                .FirstOrDefaultAsync(x =>
                    x.SystemSettingID ==
                    systemSettingId);

        if (setting == null)
        {
            return false;
        }

        setting.IsActive =
            isActive;

        setting.ModifiedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}