using HRInfoAdvertisements.Admin.Components;
using HRInfoAdvertisements.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// AUTHENTICATION / AUTHORIZATION
// ============================================================

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


// ============================================================
// AUTHENTICATION STATE PROVIDERS
// ============================================================

builder.Services.AddScoped<
    AdminAuthenticationStateProvider>();

builder.Services.AddScoped<
    PublicAuthenticationStateProvider>();


// ============================================================
// JWT HANDLERS
// ============================================================

builder.Services.AddTransient<
    AdminJwtHandler>();

builder.Services.AddTransient<
    PublicJwtHandler>();


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
// ADMIN API CLIENT
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<
        AdminJwtHandler>();


// ============================================================
// PUBLIC AUTHENTICATED API CLIENT
// ============================================================

builder.Services.AddHttpClient(
    "HRInfoAdvertisementsPublicAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(60);
    })
    .AddHttpMessageHandler<
        PublicJwtHandler>();


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
// BUILD
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

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();