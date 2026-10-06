// HRInfoAdvertisements.Admin/Services/SiteSettingsService.cs
using System.Net.Http.Json;
using HRInfoAdvertisements.Application.DTOs.SystemSettings;
using Microsoft.Extensions.Caching.Memory;

namespace HRInfoAdvertisements.Admin.Services;

public sealed class SiteSettingsService
{
    private const string CacheKey = "public-site-settings";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public SiteSettingsService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<PublicSiteSettingsResponse> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out PublicSiteSettingsResponse? cached)
            && cached is not null)
        {
            return cached;
        }

        try
        {
            var settings = await _http.GetFromJsonAsync<PublicSiteSettingsResponse>(
                "api/v1/system-settings/public");

            if (settings is not null)
            {
                _cache.Set(CacheKey, settings, TimeSpan.FromMinutes(5));
                return settings;
            }
        }
        catch
        {
            // Fall back to defaults; not cached, so the next call retries.
        }

        return new PublicSiteSettingsResponse();
    }

    public void Invalidate() => _cache.Remove(CacheKey);
}