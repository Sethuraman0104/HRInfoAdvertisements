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
//
// PUBLIC AUTHENTICATION
//
// Used by:
//      Home
//      My Ads
//      Create Advertisement
//      Account
//      Public authentication modal
//
// ADMIN AUTHENTICATION
//
// Used by:
//      Admin Login
//      Admin Dashboard
//      Admin Advertisements
//      Admin Moderation
//      Admin Profile
//
// IMPORTANT
//
// These providers intentionally remain separate.
//
// PublicAuthenticationStateProvider
//      -> Public JWT
//
// AdminAuthenticationStateProvider
//      -> Admin JWT
//
// Do NOT replace this architecture with a single provider.
// ============================================================


// ------------------------------------------------------------
// PUBLIC AUTHENTICATION PROVIDER
// ------------------------------------------------------------

builder.Services.AddScoped<
    PublicAuthenticationStateProvider>();


// ------------------------------------------------------------
// ADMIN AUTHENTICATION PROVIDER
// ------------------------------------------------------------

builder.Services.AddScoped<
    AdminAuthenticationStateProvider>();


// ------------------------------------------------------------
// DEFAULT AUTHENTICATION STATE PROVIDER
// ------------------------------------------------------------
//
// The default AuthenticationStateProvider is the PUBLIC provider.
//
// Therefore components/services that inject:
//
//     AuthenticationStateProvider
//
// receive:
//
//     PublicAuthenticationStateProvider
//
// Admin services that require the administrator token must
// explicitly inject:
//
//     AdminAuthenticationStateProvider
//
// This is intentional.
// ============================================================

builder.Services.AddScoped<
    AuthenticationStateProvider>(
        serviceProvider =>
            serviceProvider.GetRequiredService<
                PublicAuthenticationStateProvider>());


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
    builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "ApiSettings:BaseUrl is not configured.");
}

if (!Uri.TryCreate(
        apiBaseUrl,
        UriKind.Absolute,
        out var apiBaseUri))
{
    throw new InvalidOperationException(
        $"ApiSettings:BaseUrl is invalid: {apiBaseUrl}");
}


// ============================================================
// APPLICATION / ADMIN API CLIENT
// ============================================================
//
// This HttpClient is used for protected API calls.
//
// IMPORTANT:
//
// We intentionally DO NOT configure a JWT DelegatingHandler
// here.
//
// AdminAuthenticationStateProvider stores the administrator
// access token for the current Blazor circuit.
//
// Admin services such as:
//
//     AdminAdvertisementModerationService
//     AdminProfileApiService
//     RoleManagementApiService
//     UserManagementApiService
//
// explicitly retrieve the Admin access token and attach:
//
//     Authorization: Bearer <Admin JWT>
//
// to the request.
//
// This avoids a different scoped provider instance being used
// by an IHttpClientFactory handler.
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsAPI",
    client =>
    {
        client.BaseAddress = apiBaseUri;

        client.Timeout =
            TimeSpan.FromSeconds(30);
    });


// ============================================================
// PUBLIC API CLIENT
// ============================================================
//
// Used by public functionality:
//
//     Home
//     Browse Advertisements
//     Advertisement Details
//     My Ads
//     Create Advertisement
//     Account
//
// Public services explicitly obtain the Public JWT from:
//
//     PublicAuthenticationStateProvider
//
// when authentication is required.
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsPublicAPI",
    client =>
    {
        client.BaseAddress = apiBaseUri;

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

builder.Services.AddScoped<
    RoleManagementApiService>();

builder.Services.AddScoped<
    UserManagementApiService>();

builder.Services.AddScoped<
    CategoryManagementApiService>();

builder.Services.AddScoped<
    SystemSettingsApiService>();

builder.Services.AddScoped<
    FavoritesApiService>();

builder.Services.AddScoped<
    PublicFavoritesApiService>();

builder.Services.AddScoped<
    AdminProfileApiService>();

builder.Services.AddScoped<
    AdvertisementTypeManagementApiService>();

// ============================================================
// ADMIN MODERATION SERVICE
// ============================================================
//
// This service MUST use:
//
//     AdminAuthenticationStateProvider
//
// for administrator JWT authentication.
//
// It must NOT use:
//
//     PublicAuthenticationStateProvider
//
// The service itself is responsible for attaching the Admin
// Bearer token to protected API requests.
// ============================================================

builder.Services.AddScoped<
    IAdvertisementModerationService,
    AdminAdvertisementModerationService>();


// ============================================================
// MEMORY CACHE
// ============================================================
//
// Used by SiteSettingsService.
// ============================================================

builder.Services.AddMemoryCache();


// ============================================================
// PUBLIC SITE SETTINGS SERVICE
// ============================================================
//
// SiteSettingsService calls the public/anonymous settings
// endpoint and therefore does not require the Admin JWT.
// ============================================================

builder.Services.AddHttpClient<
    SiteSettingsService>(
        client =>
        {
            client.BaseAddress = apiBaseUri;

            client.Timeout =
                TimeSpan.FromSeconds(15);
        });


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


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


// ============================================================
// ANTIFORGERY
// ============================================================

app.UseAntiforgery();


// ============================================================
// STATIC ASSETS
// ============================================================

app.MapStaticAssets();


// ============================================================
// RAZOR COMPONENTS
// ============================================================

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// ============================================================
// RUN
// ============================================================

app.Run();