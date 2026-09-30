using System.Net.Http.Json;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace HRInfoAdvertisements.Infrastructure.Email;

public sealed class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public BrevoEmailService(HttpClient http, IOptions<EmailOptions> options)
    {
        _http = http;
        _o = options.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _o.Brevo.ApiKey);
        request.Content = JsonContent.Create(new
        {
            sender = new { name = _o.FromName, email = _o.FromAddress },
            to = new[] { new { email = to } },
            subject,
            htmlContent = htmlBody
        });

        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Brevo error {(int)response.StatusCode}: {detail}");
        }
    }
}