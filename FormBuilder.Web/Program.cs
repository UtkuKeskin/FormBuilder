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

    // Add services to the container.
    builder.Services.AddControllersWithViews();

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
    
    // Add UnitOfWork
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


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

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuthenticatedOnly", policy => policy.RequireAuthenticatedUser());
});

    // Health check service
    builder.Services.AddSingleton<StartupHealthCheck>();
    builder.Services.AddHealthChecks()
        .AddCheck<StartupHealthCheck>("startup", tags: new[] { "ready" });

    // Configure for Render
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        serverOptions.ListenAnyIP(int.Parse(port));
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

    app.UseStaticFiles();
    app.UseRouting();

    // Add Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

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