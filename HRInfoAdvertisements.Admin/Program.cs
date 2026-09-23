using HRInfoAdvertisements.Admin.Components;
using HRInfoAdvertisements.Admin.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

// ------------------------------------------------------------
// Razor Components
// ------------------------------------------------------------

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// ------------------------------------------------------------
// Admin Authentication State
// ------------------------------------------------------------

builder.Services.AddScoped<
    AdminAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    provider =>
        provider.GetRequiredService<
            AdminAuthenticationStateProvider>());

// ------------------------------------------------------------
// JWT Handler
// ------------------------------------------------------------

builder.Services.AddTransient<AdminJwtHandler>();

// ------------------------------------------------------------
// API HttpClient
// ------------------------------------------------------------

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsAPI",
    client =>
    {
        var baseUrl =
            builder.Configuration[
                "ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "ApiSettings:BaseUrl is not configured.");
        }

        client.BaseAddress =
            new Uri(baseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<AdminJwtHandler>();

// ------------------------------------------------------------
// Application Services
// ------------------------------------------------------------

builder.Services.AddScoped<DashboardApiService>();

builder.Services.AddScoped<AuthApiService>();

builder.Services.AddScoped<AdvertisementApiService>();

// ------------------------------------------------------------
// Build
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// HTTP Pipeline
// ------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

// ------------------------------------------------------------
// Antiforgery
// ------------------------------------------------------------

app.UseAntiforgery();

// ------------------------------------------------------------
// Static Assets
// ------------------------------------------------------------

app.MapStaticAssets();

// ------------------------------------------------------------
// Blazor
// ------------------------------------------------------------

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();