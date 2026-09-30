namespace HRInfoAdvertisements.Infrastructure.Email;

public sealed class EmailOptions
{
    public string Provider { get; set; } = "Brevo";   // Brevo | Gmail | Ses
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";

    public SmtpSettings Smtp { get; set; } = new();
    public BrevoSettings Brevo { get; set; } = new();
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class BrevoSettings
{
    public string ApiKey { get; set; } = "";
}