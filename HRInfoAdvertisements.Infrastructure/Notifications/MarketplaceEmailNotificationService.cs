using System.Text.Encodings.Web;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Services;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRInfoAdvertisements.Infrastructure.Notifications;

public sealed class MarketplaceEmailNotificationService
    : IMarketplaceEmailNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationSettingsService _settingsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<MarketplaceEmailNotificationService> _logger;

    public MarketplaceEmailNotificationService(
        ApplicationDbContext context,
        IApplicationSettingsService settingsService,
        IEmailService emailService,
        ILogger<MarketplaceEmailNotificationService> logger)
    {
        _context = context;
        _settingsService = settingsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendAdvertisementPublishedAsync(
        long advertisementId)
    {
        try
        {
            var settings =
                await _settingsService.GetApplicationSettingsAsync();

            if (!settings.EmailEnabled ||
                !settings.AutomatedEmailEnabled ||
                !settings.AdvertisementEmailEnabled)
            {
                return;
            }

            var advertisement =
                await _context.Advertisements
                    .AsNoTracking()
                    .Include(x => x.User)
                    .ThenInclude(x => x.NotificationPreference)
                    .FirstOrDefaultAsync(x =>
                        x.AdvertisementID == advertisementId &&
                        x.Status.StatusCode == "PUBLISHED");

            if (advertisement == null)
            {
                return;
            }

            var user = advertisement.User;
            var preference = user.NotificationPreference;

            // Missing preference records default to enabled.
            if (preference != null &&
                (!preference.EmailNotificationsEnabled ||
                 !preference.AdvertisementUpdatesEnabled))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

var siteUrl = (
    await _settingsService.GetStringAsync("SITE_URL")
    ?? "").Trim().TrimEnd('/');

var logoUrl = (
    await _settingsService.GetStringAsync("EMAIL_LOGO_URL")
    ?? "").Trim();

var supportEmail = (
    await _settingsService.GetStringAsync("SUPPORT_EMAIL")
    ?? "").Trim();

if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var siteUri) ||
    (siteUri.Scheme != Uri.UriSchemeHttp &&
     siteUri.Scheme != Uri.UriSchemeHttps))
{
    _logger.LogWarning(
        "Publication email skipped because SITE_URL is invalid.");
    return;
}

if (!string.IsNullOrWhiteSpace(logoUrl) &&
    (!Uri.TryCreate(logoUrl, UriKind.Absolute, out var logoUri) ||
     logoUri.Scheme != Uri.UriSchemeHttps))
{
    logoUrl = "";
}

var siteName = HtmlEncoder.Default.Encode(settings.SiteName);
var title = HtmlEncoder.Default.Encode(advertisement.Title ?? "");
var number = HtmlEncoder.Default.Encode(
    advertisement.AdvertisementNumber ?? "");

var advertisementUrl =
    $"{siteUrl}/advertisements/{advertisement.AdvertisementID}";

var safeAdvertisementUrl =
    HtmlEncoder.Default.Encode(advertisementUrl);

var safeLogoUrl = HtmlEncoder.Default.Encode(logoUrl);
var safeSupportEmail = HtmlEncoder.Default.Encode(supportEmail);

var logoHtml = string.IsNullOrWhiteSpace(logoUrl)
    ? $"""<div style="font-size:26px;font-weight:bold;color:#991717;">{siteName}</div>"""
    : $"""
        <img src="{safeLogoUrl}" alt="{siteName}" width="180"
             style="display:block;width:180px;max-width:100%;
                    height:auto;border:0;">
        """;

var supportHtml = string.IsNullOrWhiteSpace(supportEmail)
    ? ""
    : $"""
        <p style="margin:8px 0 0;font-size:13px;color:#666666;">
            Need help?
            <a href="mailto:{safeSupportEmail}"
               style="color:#991717;text-decoration:none;">
                {safeSupportEmail}
            </a>
        </p>
        """;

var subject = $"{settings.SiteName}: Your advertisement is published";

var body = $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Your advertisement is published</title>
</head>
<body style="margin:0;padding:0;background-color:#F7F6F1;
             font-family:Arial,Helvetica,sans-serif;color:#333333;">
<table role="presentation" width="100%" cellpadding="0" cellspacing="0"
       border="0" style="background-color:#F7F6F1;">
<tr><td align="center" style="padding:30px 12px;">
<table role="presentation" width="600" cellpadding="0" cellspacing="0"
       border="0"
       style="width:100%;max-width:600px;background:#FFFFFF;
              border-radius:12px;overflow:hidden;">

<tr><td align="center"
        style="padding:28px 24px;border-bottom:4px solid #991717;">
    {logoHtml}
</td></tr>

<tr><td style="padding:30px 28px 16px;">
    <div style="text-align:center;background:#F0F3E9;
                border-radius:10px;padding:24px 16px;">
        <div style="font-size:34px;color:#223906;">&#10003;</div>
        <h1 style="margin:10px 0;color:#223906;font-size:24px;
                   line-height:1.35;">
            Your advertisement is published!
        </h1>
        <p style="margin:0;color:#555555;font-size:15px;line-height:1.7;">
            Your advertisement has been approved and is now available to view.
        </p>
    </div>
</td></tr>

<tr><td style="padding:12px 28px 8px;">
    <h2 style="margin:0 0 16px;font-size:18px;color:#223906;">
        Advertisement details
    </h2>
    <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
           border="0" style="border:1px solid #E7E3DC;border-radius:8px;">
        <tr><td style="padding:18px 16px 8px;">
            <p style="margin:0 0 6px;font-size:11px;letter-spacing:1px;
                      color:#777777;">ADVERTISEMENT TITLE</p>
            <p style="margin:0;font-size:20px;font-weight:bold;
                      color:#223906;line-height:1.5;">{title}</p>
        </td></tr>
        <tr><td style="padding:8px 16px 18px;">
            <p style="margin:0;font-size:13px;color:#777777;">
                Reference number
            </p>
            <p style="margin:5px 0 0;font-size:15px;font-weight:bold;
                      color:#333333;">{number}</p>
        </td></tr>
    </table>
</td></tr>

<tr><td align="center" style="padding:24px 28px 12px;">
    <table role="presentation" cellpadding="0" cellspacing="0" border="0">
        <tr><td align="center" bgcolor="#991717" style="border-radius:6px;">
            <a href="{safeAdvertisementUrl}" target="_blank"
               style="display:inline-block;padding:15px 30px;color:#FFFFFF;
                      text-decoration:none;font-size:15px;font-weight:bold;">
                View Advertisement &#8599;
            </a>
        </td></tr>
    </table>
</td></tr>

<tr><td style="padding:12px 28px 30px;text-align:center;">
    <p style="margin:0;font-size:14px;line-height:1.8;color:#555555;">
        Thank you for choosing {siteName}.
    </p>
    {supportHtml}
</td></tr>

<tr><td align="center"
        style="padding:18px;background:#F7F6F1;
               border-top:1px solid #E7E3DC;">
    <p style="margin:0;font-size:12px;color:#666666;">
        &copy; {DateTime.UtcNow.Year} {siteName}
    </p>
    <p style="margin:6px 0 0;font-size:11px;color:#888888;">
        This is an automated notification.
    </p>
</td></tr>

</table>
</td></tr>
</table>
</body>
</html>
""";

            await _emailService.SendAsync(
                user.Email,
                subject,
                body);
        }
        catch (Exception ex)
        {
            // Publication has already been saved. Do not fail approval
            // merely because email delivery failed.
            _logger.LogError(
                ex,
                "Failed to send publication email for advertisement {AdvertisementID}.",
                advertisementId);
        }
    }

    
    public async Task SendFavoriteAdvertisementUpdatedAsync(
        long advertisementId)
    {
        try
        {
            var settings =
                await _settingsService.GetApplicationSettingsAsync();

            if (!settings.EmailEnabled ||
                !settings.AutomatedEmailEnabled ||
                !settings.FavoriteAdvertisementEmailEnabled)
            {
                return;
            }

            var advertisement =
                await _context.Advertisements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.AdvertisementID == advertisementId &&
                        x.Status.StatusCode == "PUBLISHED");

            if (advertisement == null)
                return;

            var recipients =
                await _context.AdvertisementFavorites
                    .AsNoTracking()
                    .Where(x => x.AdvertisementID == advertisementId)
                    .Select(x => new
                    {
                        x.User.Email,
                        Preference = x.User.NotificationPreference
                    })
                    .ToListAsync();

            var siteUrl = (
                await _settingsService.GetStringAsync("SITE_URL")
                ?? "").Trim().TrimEnd('/');

            var logoUrl = (
                await _settingsService.GetStringAsync("EMAIL_LOGO_URL")
                ?? "").Trim();

            var supportEmail = (
                await _settingsService.GetStringAsync("SUPPORT_EMAIL")
                ?? "").Trim();

            if (!Uri.TryCreate(
                    siteUrl, UriKind.Absolute, out var siteUri) ||
                (siteUri.Scheme != Uri.UriSchemeHttp &&
                 siteUri.Scheme != Uri.UriSchemeHttps))
            {
                _logger.LogWarning(
                    "Favorite-update email skipped because SITE_URL is invalid.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(logoUrl) &&
                (!Uri.TryCreate(
                     logoUrl, UriKind.Absolute, out var logoUri) ||
                 logoUri.Scheme != Uri.UriSchemeHttps))
            {
                logoUrl = "";
            }

            var siteName =
                HtmlEncoder.Default.Encode(settings.SiteName ?? "");

            var title =
                HtmlEncoder.Default.Encode(advertisement.Title ?? "");

            var number =
                HtmlEncoder.Default.Encode(
                    advertisement.AdvertisementNumber ?? "");

            var advertisementUrl =
                $"{siteUrl}/advertisements/{advertisement.AdvertisementID}";

            var safeAdvertisementUrl =
                HtmlEncoder.Default.Encode(advertisementUrl);

            var safeLogoUrl =
                HtmlEncoder.Default.Encode(logoUrl);

            var safeSupportEmail =
                HtmlEncoder.Default.Encode(supportEmail);

            var logoHtml = string.IsNullOrWhiteSpace(logoUrl)
                ? $"""
                    <div style="font-size:26px;font-weight:bold;color:#991717;">
                        {siteName}
                    </div>
                    """
                : $"""
                    <img src="{safeLogoUrl}" alt="{siteName}" width="180"
                         style="display:block;width:180px;max-width:100%;
                                height:auto;border:0;">
                    """;

            var supportHtml = string.IsNullOrWhiteSpace(supportEmail)
                ? ""
                : $"""
                    <p style="margin:8px 0 0;font-size:13px;color:#666666;">
                        Need help?
                        <a href="mailto:{safeSupportEmail}"
                           style="color:#991717;text-decoration:none;">
                            {safeSupportEmail}
                        </a>
                    </p>
                    """;

            var subject =
                $"{settings.SiteName}: A favorited advertisement has been updated";

            var body = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Favorite advertisement updated</title>
                </head>
                <body style="margin:0;padding:0;background-color:#F7F6F1;
                             font-family:Arial,Helvetica,sans-serif;color:#333333;">
                <table role="presentation" width="100%" cellpadding="0"
                       cellspacing="0" border="0" style="background:#F7F6F1;">
                <tr><td align="center" style="padding:30px 12px;">
                <table role="presentation" width="600" cellpadding="0"
                       cellspacing="0" border="0"
                       style="width:100%;max-width:600px;background:#FFFFFF;
                              border-radius:12px;overflow:hidden;">

                <tr><td align="center"
                        style="padding:28px 24px;border-bottom:4px solid #991717;">
                    {logoHtml}
                </td></tr>

                <tr><td style="padding:30px 28px 16px;">
                    <div style="text-align:center;background:#F0F3E9;
                                border-radius:10px;padding:24px 16px;">
                        <h1 style="margin:10px 0;color:#223906;font-size:24px;
                                   line-height:1.35;">
                            A saved advertisement has been updated
                        </h1>
                        <p style="margin:0;color:#555555;font-size:15px;line-height:1.7;">
                            An advertisement in your favorites has been updated.
                            View it to see the latest details.
                        </p>
                    </div>
                </td></tr>

                <tr><td style="padding:12px 28px 8px;">
                    <h2 style="margin:0 0 16px;font-size:18px;color:#223906;">
                        Advertisement details
                    </h2>
                    <table role="presentation" width="100%" cellpadding="0"
                           cellspacing="0" border="0"
                           style="border:1px solid #E7E3DC;border-radius:8px;">
                        <tr><td style="padding:18px 16px 8px;">
                            <p style="margin:0 0 6px;font-size:11px;
                                      letter-spacing:1px;color:#777777;">
                                ADVERTISEMENT TITLE
                            </p>
                            <p style="margin:0;font-size:20px;font-weight:bold;
                                      color:#223906;line-height:1.5;">
                                {title}
                            </p>
                        </td></tr>
                        <tr><td style="padding:8px 16px 18px;">
                            <p style="margin:0;font-size:13px;color:#777777;">
                                Reference number
                            </p>
                            <p style="margin:5px 0 0;font-size:15px;
                                      font-weight:bold;color:#333333;">
                                {number}
                            </p>
                        </td></tr>
                    </table>
                </td></tr>

                <tr><td align="center" style="padding:24px 28px 12px;">
                    <table role="presentation" cellpadding="0"
                           cellspacing="0" border="0">
                        <tr><td align="center" bgcolor="#991717"
                                style="border-radius:6px;">
                            <a href="{safeAdvertisementUrl}" target="_blank"
                               style="display:inline-block;padding:15px 30px;
                                      color:#FFFFFF;text-decoration:none;
                                      font-size:15px;font-weight:bold;">
                                View Updated Advertisement &#8599;
                            </a>
                        </td></tr>
                    </table>
                </td></tr>

                <tr><td style="padding:12px 28px 30px;text-align:center;">
                    <p style="margin:0;font-size:14px;line-height:1.8;
                              color:#555555;">
                        Thank you for choosing {siteName}.
                    </p>
                    {supportHtml}
                </td></tr>

                <tr><td align="center"
                        style="padding:18px;background:#F7F6F1;
                               border-top:1px solid #E7E3DC;">
                    <p style="margin:0;font-size:12px;color:#666666;">
                        &copy; {DateTime.UtcNow.Year} {siteName}
                    </p>
                    <p style="margin:6px 0 0;font-size:11px;color:#888888;">
                        This is an automated notification.
                    </p>
                </td></tr>

                </table>
                </td></tr>
                </table>
                </body>
                </html>
                """;

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrWhiteSpace(recipient.Email))
                    continue;

                var preference = recipient.Preference;

                // Missing preference records default to enabled.
                if (preference != null &&
                    (!preference.EmailNotificationsEnabled ||
                     !preference.FavoriteAdvertisementUpdatesEnabled))
                {
                    continue;
                }

                try
                {
                    await _emailService.SendAsync(
                        recipient.Email,
                        subject,
                        body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send favorite update email for advertisement {AdvertisementID}.",
                        advertisementId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process favorite update emails for advertisement {AdvertisementID}.",
                advertisementId);
        }
    }
}