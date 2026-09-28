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
// The public website uses:
//      PublicAuthenticationStateProvider
//
// This includes:
//      Home
//      My Ads
//      Create Advertisement
//      Account
//      Public authentication modal
//
// ADMIN AUTHENTICATION
//
// The Admin area uses:
//      AdminAuthenticationStateProvider
//
// This includes:
//      Admin login
//      Admin dashboard
//      Admin advertisements
//      Admin moderation
//
// IMPORTANT:
//
// These providers must remain separate because they maintain
// different JWT tokens and different authentication states.
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
// DEFAULT / CASCADING AUTHENTICATION PROVIDER
// ------------------------------------------------------------
//
// The public authentication provider is the default provider
// for the Blazor application.
//
// Therefore:
//
//     AuthenticationStateProvider
//              ↓
//     PublicAuthenticationStateProvider
//
// Admin components that require administrator authentication
// explicitly inject:
//
//     AdminAuthenticationStateProvider
//
// This prevents the Admin authentication state from affecting
// the public Home page and other public components.
// ------------------------------------------------------------

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
//
// Used by the public advertisement functionality:
//
//     Home
//     Browse Advertisements
//     My Ads
//     Create Advertisement
//     Advertisement Details
//     Account
//
// AdvertisementApiService explicitly retrieves the public JWT
// from PublicAuthenticationStateProvider when an authenticated
// public request is required.
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

builder.Services.AddScoped<RoleManagementApiService>();

builder.Services.AddScoped<UserManagementApiService>();

builder.Services.AddScoped<CategoryManagementApiService>();

builder.Services.AddScoped<
    AdvertisementTypeManagementApiService>();
// ============================================================
// ADMIN MODERATION SERVICE
// ============================================================
//
// This service explicitly uses:
//
//     AdminAuthenticationStateProvider
//
// for administrator JWT authentication.
//
// It must NOT use PublicAuthenticationStateProvider.
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