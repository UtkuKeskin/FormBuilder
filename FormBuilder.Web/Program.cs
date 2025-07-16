using Serilog;
using Serilog.Events;
using FormBuilder.Web.Middleware;
using FormBuilder.Web.Services;
using FormBuilder.Infrastructure.Data;
using FormBuilder.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Repositories;
using Microsoft.AspNetCore.Localization;
using FormBuilder.Web.Resources;
using Microsoft.Extensions.Localization;
using FormBuilder.Infrastructure.Services;
using FormBuilder.Web.Profiles;
using FormBuilder.Core.Models;
using FormBuilder.Infrastructure.Security;
using AspNetCoreRateLimit;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting FormBuilder application");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog add
    builder.Host.UseSerilog();

    // Connection String handling for Render
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // If connection string is empty, try DATABASE_URL (Render format)
    if (string.IsNullOrEmpty(connectionString))
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            connectionString = ConvertDatabaseUrl(databaseUrl);
        }
    }

    // Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString ?? throw new InvalidOperationException("Connection string not found")));
    builder.Services.AddScoped<ITemplateService, TemplateService>();

    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ISearchService, SearchService>();

    // Add UnitOfWork
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Add CloudinaryService
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

    // Add TagService
    builder.Services.AddScoped<ITagService, TagService>();

    // Dropbox configuration
    builder.Services.Configure<DropboxConfig>(
        builder.Configuration.GetSection("Dropbox"));

    // Support Ticket Services
    builder.Services.AddScoped<IDropboxService, DropboxService>();
    builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();

    // Salesforce configuration
    builder.Services.Configure<SalesforceConfig>(options =>
    {
        options.LoginUrl = Environment.GetEnvironmentVariable("SALESFORCE_LOGIN_URL") 
            ?? builder.Configuration["Salesforce:LoginUrl"];
        options.ClientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_ID") 
            ?? builder.Configuration["Salesforce:ClientId"];
        options.ClientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET") 
            ?? builder.Configuration["Salesforce:ClientSecret"];
        options.Username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME") 
            ?? builder.Configuration["Salesforce:Username"];
        options.Password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD") 
            ?? builder.Configuration["Salesforce:Password"];
        options.SecurityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN") 
            ?? builder.Configuration["Salesforce:SecurityToken"];
        options.ApiVersion = Environment.GetEnvironmentVariable("SALESFORCE_API_VERSION") 
            ?? builder.Configuration["Salesforce:ApiVersion"] ?? "v59.0";
    });

    // Register Salesforce service
    builder.Services.AddScoped<ISalesforceService, SalesforceService>();

    // Add HttpClient for Salesforce
    builder.Services.AddHttpClient<ISalesforceService, SalesforceService>();

    // Add ApiKeyService
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

    // Add AutoMapper
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("OdooPolicy",
            builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    // Rate Limiting Configuration
    builder.Services.Configure<IpRateLimitOptions>(options =>
    {
        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests = false;
        options.HttpStatusCode = 429;
        options.RealIpHeader = "X-Real-IP";
        options.ClientIdHeader = "X-API-Key";
        options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "get:/api/v1/*",
                Period = "1h",
                Limit = 100,
            },
            new RateLimitRule
            {
                Endpoint = "post:/api/support/*",
                Period = "1h",
                Limit = 20,
            }
        };
    });

    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    builder.Services.AddInMemoryRateLimiting();

    // Add Identity
    builder.Services.AddDefaultIdentity<User>(options => {
    // Password Requirements
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    // User Options
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0-9-._@+";
    
    // Lockout Options
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
    
    // Google Authentication
    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
    {
        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
    }

    // Add API Key Authentication
    builder.Services.AddAuthentication()
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

    // Cookie Configuration
    builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "ru" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    
    // Cookie provider
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
});

// MVC configuration
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(Program).Assembly) // Explicit assembly reference
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

    // Shared Resource Localizer
    builder.Services.AddSingleton<IStringLocalizer>(provider =>
    {
        var factory = provider.GetRequiredService<IStringLocalizerFactory>();
        return factory.Create(typeof(SharedResource));
    }); 

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "FormBuilder API", 
        Version = "v1" 
    });
});

// Add Authorization policies
    builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuthenticatedOnly", policy => policy.RequireAuthenticatedUser());
    
    // API Key Policy
    options.AddPolicy("ApiKeyPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("ApiKey");
        policy.RequireAuthenticatedUser();
    });
});

    // Health check service
    builder.Services.AddSingleton<StartupHealthCheck>();
    builder.Services.AddHealthChecks()
        .AddCheck<StartupHealthCheck>("startup", tags: new[] { "ready" });

    // Configure for Render
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
    {
        // Production (Render)
        serverOptions.ListenAnyIP(int.Parse(port));
    }
    else
    {
        // Local development
        serverOptions.ListenLocalhost(5175);
        serverOptions.ListenLocalhost(7286, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    }
});

    var app = builder.Build();

    // Mark application as ready
    var startupHealthCheck = app.Services.GetRequiredService<StartupHealthCheck>();
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        startupHealthCheck.IsReady = true;
    });

    // Serilog request logging
    app.UseSerilogRequestLogging();

    // Custom exception middleware
    app.UseMiddleware<ExceptionMiddleware>();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    // Explicit Swagger Configuration
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FormBuilder API V1");
    });

    // Static files should be after Swagger
    app.UseStaticFiles();

    // Then routing
    app.UseRouting();

    app.UseRequestLocalization();

    // Use CORS
    app.UseCors("OdooPolicy");

    // Use Rate Limiting
    app.UseIpRateLimiting();

    // Then Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // ADD MAPCONTROLLERS FOR API ROUTING 
    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

        // Apply pending migrations at startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }

    // Admin password update from environment
    try
    {
        await UpdateAdminPasswordFromEnvironment(app);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to update admin password at startup");
        // Continue running even if admin password update fails
    }


app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Convert DATABASE_URL to .NET connection string format
static string ConvertDatabaseUrl(string databaseUrl)
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var database = uri.AbsolutePath.TrimStart('/');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var username = userInfo[0];
        var password = userInfo[1];
        
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to convert DATABASE_URL");
        throw;
    }
}

// update Admin password
static async Task UpdateAdminPasswordFromEnvironment(WebApplication app)
{
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        Log.Warning("ADMIN_PASSWORD environment variable not set. Using migration password.");
        return;
    }
    
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    
    try
    {
        var admin = await userManager.FindByEmailAsync("admin@formbuilder.com");
        if (admin != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(admin);
            await userManager.ResetPasswordAsync(admin, token, adminPassword);
            
            Log.Information("Admin password successfully updated.");
        }
        else
        {
            Log.Warning("Admin user not found in database");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to update admin password");
        throw; // Re-throw to be caught by outer try-catch
    }
}