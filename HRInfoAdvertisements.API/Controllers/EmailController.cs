using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/email-test")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _email;
    public EmailController(IEmailService email) => _email = email;

    public sealed record TestEmailRequest(string To, string Subject, string Message);

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] TestEmailRequest req)
    {
        try
        {
            var html = $"<p>{System.Net.WebUtility.HtmlEncode(req.Message)}</p>";
            await _email.SendAsync(req.To, req.Subject, html);
            return Ok(new { sent = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { sent = false, error = ex.Message });
        }
    }
}