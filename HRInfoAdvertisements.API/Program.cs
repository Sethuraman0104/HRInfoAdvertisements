using System.Security.Claims;
using System.Text;

using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Application.Settings;

using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Data.Seed;
using HRInfoAdvertisements.Infrastructure.Services;

using HRInfoAdvertisements.Application.Enquiries;
using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Infrastructure.Enquiries;
using HRInfoAdvertisements.Infrastructure.Billing;

using HRInfoAdvertisements.Application.Messaging;
using HRInfoAdvertisements.Infrastructure.Messaging;

using HRInfoAdvertisements.Application.Notifications;
using HRInfoAdvertisements.Infrastructure.Notifications;

using HRInfoAdvertisements.Application.Profile;
using HRInfoAdvertisements.Infrastructure.Profile;

using HRInfoAdvertisements.Application.Services;

using HRInfoAdvertisements.Infrastructure.Email;

using HRInfoAdvertisements.Application.Security;
using HRInfoAdvertisements.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HRInfoAdvertisements.Application.NotificationPreferences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection();


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "HRInfoAdvertisements.API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                Microsoft.OpenApi.Models
                    .SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In =
                Microsoft.OpenApi.Models
                    .ParameterLocation.Header,

            Description =
                "Enter your JWT access token. Example: Bearer {token}"
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
                            Type =
                                Microsoft.OpenApi.Models
                                    .ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DevelopmentPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ============================================================
// JWT SETTINGS
// ============================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));


// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IJwtService,
    JwtService>();

builder.Services.AddScoped<
    IPasswordService,
    PasswordService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    IDashboardStatisticsService,
    DashboardStatisticsService>();

builder.Services.AddScoped<
    IUserManagementService,
    UserManagementService>();

builder.Services.AddScoped<
    IReportManagementService,
    ReportManagementService>();

builder.Services.AddScoped<
    IAdvertisementService,
    AdvertisementService>();

builder.Services.AddScoped<
    IAdvertisementLookupService,
    AdvertisementLookupService>();

builder.Services.AddScoped<
    IAdvertisementMediaService,
    AdvertisementMediaService>();

builder.Services.AddScoped<
    IAdvertisementModerationService,
    AdvertisementModerationService>();

builder.Services.AddScoped<
    IAdvertisementSearchService,
    AdvertisementSearchService>();

builder.Services.AddScoped<
    IAdvertisementFavoriteService,
    AdvertisementFavoriteService>();

builder.Services.AddScoped<
    IEnquiryBillingService,
    EnquiryBillingService>();

builder.Services.AddScoped<
    IAdvertisementEnquiryService,
    AdvertisementEnquiryService>();

builder.Services.AddScoped<
    IMessageService,
    MessageService>();

builder.Services.AddScoped<
    INotificationService,
    NotificationService>();

builder.Services.AddScoped<
    IMarketplaceEmailNotificationService,
    MarketplaceEmailNotificationService>();
    
builder.Services.AddScoped<
    IProfileService,
    ProfileService>();

builder.Services.AddScoped<
    INotificationPreferenceService,
    NotificationPreferenceService>();

builder.Services.AddScoped<
    IRoleManagementService,
    RoleManagementService>();

builder.Services.AddScoped<
    ICategoryManagementService,
    CategoryManagementService>();

builder.Services.AddScoped<
    IAdvertisementTypeManagementService,
    AdvertisementTypeManagementService>();

builder.Services.AddScoped<
    IPermissionManagementService,
    PermissionManagementService>();

builder.Services.AddScoped<
    IRolePermissionManagementService,
    RolePermissionManagementService>();

builder.Services.AddScoped<
    IReportReviewService,
    ReportReviewService>();

builder.Services.AddScoped<
    IReportSubmissionService,
    ReportSubmissionService>();

builder.Services.AddScoped<
    IAuditLogService,
    AuditLogService>();

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();

builder.Services.AddScoped<
    IApplicationSettingsService,
    ApplicationSettingsService>();

builder.Services.AddScoped<
    ISystemSettingService,
    SystemSettingService>();

builder.Services.AddScoped<
    IConfigurationEncryptionService,
    DataProtectionConfigurationEncryptionService>();

builder.Services.AddScoped<
    IAdminProfileService,
    AdminProfileService>();

builder.Services.AddScoped<
    IAdvertisementRemovalRequestService,
    AdvertisementRemovalRequestService>();

builder.Services.AddScoped<
    IEnquiryPackageAdminService,
    EnquiryPackageAdminService>();

// ============================================================
// EMAIL
// ============================================================
//
// Provider selected from:
//
//     Email:Provider
//
// Supported:
//
//     Brevo
//     Gmail
//     Smtp
// ============================================================

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));

var emailProvider =
    builder.Configuration["Email:Provider"]
    ?? "Brevo";

switch (emailProvider.ToLowerInvariant())
{
    case "gmail":

    case "smtp":

        builder.Services.AddScoped<
            IEmailService,
            SmtpEmailService>();

        break;


    case "brevo":

        builder.Services.AddHttpClient<
            IEmailService,
            BrevoEmailService>();

        break;


    default:

        throw new InvalidOperationException(
            $"Unknown email provider '{emailProvider}'.");
}


// ============================================================
// JWT AUTHENTICATION
// ============================================================

var jwtSettings =
    builder.Configuration
        .GetSection("JwtSettings")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JwtSettings configuration is missing.");


// ------------------------------------------------------------
// Validate Secret Key
// ------------------------------------------------------------

if (string.IsNullOrWhiteSpace(
        jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured.");
}


if (jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters.");
}


// ------------------------------------------------------------
// Validate Issuer
// ------------------------------------------------------------

if (string.IsNullOrWhiteSpace(
        jwtSettings.Issuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is not configured.");
}


// ------------------------------------------------------------
// Validate Audience
// ------------------------------------------------------------

if (string.IsNullOrWhiteSpace(
        jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT Audience is not configured.");
}


// ------------------------------------------------------------
// Signing Key
// ------------------------------------------------------------

var signingKey =
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            jwtSettings.SecretKey));


// ------------------------------------------------------------
// Configure JWT Bearer Authentication
// ------------------------------------------------------------

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // ------------------------------------------------
                // ISSUER
                // ------------------------------------------------

                ValidateIssuer = true,

                ValidIssuer =
                    jwtSettings.Issuer,


                // ------------------------------------------------
                // AUDIENCE
                // ------------------------------------------------

                ValidateAudience = true,

                ValidAudience =
                    jwtSettings.Audience,


                // ------------------------------------------------
                // TOKEN LIFETIME
                // ------------------------------------------------

                ValidateLifetime = true,


                // ------------------------------------------------
                // SIGNING KEY
                // ------------------------------------------------

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    signingKey,


                // ------------------------------------------------
                // IMPORTANT CLAIM MAPPINGS
                // ------------------------------------------------
                //
                // Your JWT contains the standard Microsoft
                // identity claims:
                //
                // NameIdentifier
                // Name
                // EmailAddress
                // Role
                //
                // Explicitly tell ASP.NET Core which claims
                // represent the authenticated user's name
                // and roles.
                // ------------------------------------------------

                NameClaimType =
                    ClaimTypes.Name,

                RoleClaimType =
                    ClaimTypes.Role,


                // ------------------------------------------------
                // CLOCK SKEW
                // ------------------------------------------------

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };


        // ========================================================
        // JWT EVENTS
        // ========================================================

        options.Events =
            new JwtBearerEvents
            {
                // ------------------------------------------------
                // Authentication Failed
                // ------------------------------------------------

                OnAuthenticationFailed =
                    context =>
                    {
                        Console.WriteLine(
                            "================================================");

                        Console.WriteLine(
                            "JWT AUTHENTICATION FAILED");

                        Console.WriteLine(
                            $"ERROR: " +
                            $"{context.Exception.Message}");

                        Console.WriteLine(
                            $"EXCEPTION TYPE: " +
                            $"{context.Exception.GetType().Name}");

                        Console.WriteLine(
                            "================================================");

                        return Task.CompletedTask;
                    },


                // ------------------------------------------------
                // Token Validated
                // ------------------------------------------------

                
OnTokenValidated = async context =>
{
    var logger = context.HttpContext.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("JwtBearerEvents");

    var principal = context.Principal;

    var identity = principal?.Identity as ClaimsIdentity;

    var userIdValue = principal?
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (identity is null ||
        !long.TryParse(userIdValue, out var userId) ||
        userId <= 0)
    {
        context.Fail("Invalid authenticated user identity.");
        return;
    }

    try
    {
        var db = context.HttpContext.RequestServices
            .GetRequiredService<ApplicationDbContext>();

        var cancellationToken =
            context.HttpContext.RequestAborted;

        // 1. Verify that the account still exists and is active.
        var account = await db.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId)
            .Select(u => new
            {
                u.AccountStatus
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (account is null ||
            !string.Equals(
                account.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("The user account is inactive or unavailable.");
            return;
        }

        // 2. Load the user's current active roles.
        var roles = await db.UserRoles
            .AsNoTracking()
            .Where(ur =>
                ur.UserID == userId &&
                ur.Role.IsActive)
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 3. Load permissions belonging to active roles,
        //    but only when each permission is active.
        var permissions = await db.UserRoles
            .AsNoTracking()
            .Where(ur =>
                ur.UserID == userId &&
                ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission.PermissionCode))
            .Distinct()
            .ToListAsync(cancellationToken);

        // 4. Remove stale role claims from this JWT identity.
        foreach (var claim in identity
            .FindAll(ClaimTypes.Role)
            .ToList())
        {
            identity.RemoveClaim(claim);
        }

        // 5. Remove stale permission claims.
        foreach (var claim in identity
            .FindAll("permission")
            .ToList())
        {
            identity.RemoveClaim(claim);
        }

        // 6. Apply current database-backed roles.
        foreach (var role in roles)
        {
            identity.AddClaim(
                new Claim(ClaimTypes.Role, role));
        }

        // 7. Apply current database-backed permissions.
        foreach (var permission in permissions)
        {
            identity.AddClaim(
                new Claim("permission", permission));
        }

        logger.LogInformation(
    "AUTHORIZATION DIAGNOSTIC | UserId={UserId} | Roles={Roles} | HasAdvertisementView={HasView} | HasAdvertisementEdit={HasEdit}",
    userId,
    string.Join(", ", roles),
    permissions.Contains("ADVERTISEMENT_VIEW"),
    permissions.Contains("ADVERTISEMENT_EDIT"));

        logger.LogDebug(
            "JWT account and authorization claims validated for user {UserId}.",
            userId);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Database-backed JWT validation failed for user {UserId}.",
            userId);

        // Fail closed: do not authorize using stale token claims.
        context.Fail(
            "Unable to validate current account authorization.");
    }
},



                // ------------------------------------------------
                // Authentication Challenge
                // ------------------------------------------------

                OnChallenge =
                    context =>
                    {
                        Console.WriteLine(
                            "================================================");

                        Console.WriteLine(
                            "JWT AUTHENTICATION CHALLENGE");

                        Console.WriteLine(
                            $"ERROR: " +
                            $"{context.Error}");

                        Console.WriteLine(
                            $"DESCRIPTION: " +
                            $"{context.ErrorDescription}");

                        Console.WriteLine(
                            "================================================");

                        return Task.CompletedTask;
                    }
            };
    });


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization(options =>
{
    // ============================================================
    // ADMIN PROFILE
    // ============================================================
    //
    // Only authenticated administrators can access:
    //
    //     GET    /api/v1/admin/profile
    //     PUT    /api/v1/admin/profile
    //     DELETE /api/v1/admin/profile/sessions/{sessionId}
    //
    // Current application roles:
    //
    //     Admin
    //     SuperAdmin
    //
    // IMPORTANT:
    //
    // Do NOT use "Administrator" because that role does not
    // exist in your current JWT.
    // ============================================================

    options.AddPolicy(
        "ADMIN_PROFILE",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                "Admin",
                "SuperAdmin");
        });


    // ============================================================
    // USER MANAGEMENT
    // ============================================================

    options.AddPolicy(
        "USER_VIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "USER_VIEW");
        });


    options.AddPolicy(
        "USER_EDIT",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "USER_EDIT");
        });


    options.AddPolicy(
        "USER_SUSPEND",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "USER_SUSPEND");
        });


    // ============================================================
    // ADVERTISEMENT MANAGEMENT
    // ============================================================

    options.AddPolicy(
        "ADVERTISEMENT_VIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_VIEW");
        });


    options.AddPolicy(
        "ADVERTISEMENT_CREATE",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_CREATE");
        });


    options.AddPolicy(
        "ADVERTISEMENT_EDIT",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_EDIT");
        });


    options.AddPolicy(
        "ADVERTISEMENT_DELETE",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_DELETE");
        });


    options.AddPolicy(
        "ADVERTISEMENT_APPROVE",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_APPROVE");
        });


    options.AddPolicy(
        "ADVERTISEMENT_REJECT",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_REJECT");
        });


    options.AddPolicy(
        "ADVERTISEMENT_SUSPEND",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ADVERTISEMENT_SUSPEND");
        });

    options.AddPolicy(
    "ROLE_PERMISSION_MANAGE",
    policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(
            "permission",
            "ROLE_PERMISSION_MANAGE");
    });


    // ============================================================
    // REPORTS
    // ============================================================

    options.AddPolicy(
        "REPORT_VIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "REPORT_VIEW");
        });


    options.AddPolicy(
        "REPORT_REVIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "REPORT_REVIEW");
        });


    // ============================================================
    // AUDIT
    // ============================================================

    options.AddPolicy(
        "AUDIT_VIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "AUDIT_VIEW");
        });

        
    // ============================================================
    // ENQUIRY MANAGEMENT
    // ============================================================

    options.AddPolicy(
        "ENQUIRY_VIEW",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "ENQUIRY_VIEW");
        });

    // ============================================================
    // BILLING MANAGEMENT
    // ============================================================

    options.AddPolicy(
        "BILLING_MANAGE",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireClaim(
                "permission",
                "BILLING_MANAGE");
        });

});


// ============================================================
// BUILD APPLICATION
// ============================================================

var app =
    builder.Build();


// ============================================================
// DEPENDENCY INJECTION CHECK
// ============================================================

using (var scope =
       app.Services.CreateScope())
{
    var lookupService =
        scope.ServiceProvider
            .GetRequiredService<
                IAdvertisementLookupService>();

    Console.WriteLine(
        $"DI CHECK: " +
        $"{lookupService.GetType().FullName}");


    var emailService =
        scope.ServiceProvider
            .GetRequiredService<
                IEmailService>();

    Console.WriteLine(
        $"EMAIL PROVIDER: " +
        $"{emailService.GetType().Name}");
}


// ============================================================
// DATABASE MIGRATION / SEEDING
// ============================================================

using (var scope =
       app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<
                ApplicationDbContext>();

        await DatabaseSeeder.SeedAsync(
            context);
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


// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ============================================================
// HTTP PIPELINE
// ============================================================

// HTTPS redirection intentionally disabled
// for local development.
//
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(
    "DevelopmentPolicy");


// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// RUN
// ============================================================

app.Run();