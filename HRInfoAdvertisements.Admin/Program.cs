using HRInfoAdvertisements.Admin.Components;
using HRInfoAdvertisements.Admin.Services;
using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();


// ============================================================
// AUTHENTICATION STATE PROVIDERS
// ============================================================

builder.Services.AddScoped<
    AdminAuthenticationStateProvider>();

builder.Services.AddScoped<
    AuthenticationStateProvider>(
        serviceProvider =>
            serviceProvider.GetRequiredService<
                AdminAuthenticationStateProvider>());

builder.Services.AddScoped<
    PublicAuthenticationStateProvider>();


// ============================================================
// RAZOR COMPONENTS
// ============================================================

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


// ============================================================
// API BASE URL
// ============================================================

var apiBaseUrl =
    builder.Configuration[
        "ApiSettings:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "ApiSettings:BaseUrl is not configured.");
}


// ============================================================
// ADMIN / APPLICATION API CLIENT
// ============================================================
//
// IMPORTANT:
//
// We intentionally do NOT use AdminJwtHandler here.
//
// The Admin application is a Blazor Server application and
// AdminAuthenticationStateProvider stores the JWT inside the
// current Blazor circuit.
//
// AdminAdvertisementModerationService explicitly reads the
// token from that provider and attaches it to each request.
//
// This prevents IHttpClientFactory handler scopes from using
// a different AdminAuthenticationStateProvider instance.
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);
    });


// ============================================================
// PUBLIC API CLIENT
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsPublicAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(60);
    });


// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    DashboardApiService>();

builder.Services.AddScoped<
    AuthApiService>();

builder.Services.AddScoped<
    AdvertisementApiService>();


// ============================================================
// ADMIN MODERATION SERVICE
// ============================================================

builder.Services.AddScoped<
    IAdvertisementModerationService,
    AdminAdvertisementModerationService>();


// ============================================================
// BUILD
// ============================================================

var app =
    builder.Build();


// ============================================================
// HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();