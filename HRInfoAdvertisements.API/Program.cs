using System.Text;

using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Settings;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Data.Seed;
using HRInfoAdvertisements.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Controllers
// ------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HRInfoAdvertisements.API",
        Version = "v1"
    });

    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT access token. Example: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});


// ------------------------------------------------------------
// Database
// ------------------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


// ------------------------------------------------------------
// CORS
// ------------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ------------------------------------------------------------
// JWT Settings
// ------------------------------------------------------------

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));


// ------------------------------------------------------------
// Application Services
// ------------------------------------------------------------

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<
    IPasswordService,
    PasswordService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IAdvertisementService,
    AdvertisementService>();

builder.Services.AddScoped<
    IAdvertisementMediaService,
    AdvertisementMediaService>();

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();

// ------------------------------------------------------------
// JWT Authentication
// ------------------------------------------------------------

var jwtSettings =
    builder.Configuration
        .GetSection("JwtSettings")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JwtSettings configuration is missing.");


if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured.");
}


if (jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters.");
}


if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is not configured.");
}


if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT Audience is not configured.");
}


var signingKey =
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            jwtSettings.SecretKey));


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidateAudience = true,

                ValidAudience =
                    jwtSettings.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    signingKey,

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });


// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

builder.Services.AddAuthorization();


// ------------------------------------------------------------
// Build Application
// ------------------------------------------------------------

var app = builder.Build();


// ------------------------------------------------------------
// Database Migration / Seeding
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<ApplicationDbContext>();

        await DatabaseSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger =
            services.GetRequiredService<
                ILogger<Program>>();

        logger.LogError(
            ex,
            "An error occurred while seeding the database.");
    }
}


// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ------------------------------------------------------------
// HTTP Pipeline
// ------------------------------------------------------------

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("DevelopmentPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();