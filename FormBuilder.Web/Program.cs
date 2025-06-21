using Serilog;
using Serilog.Events;
using FormBuilder.Web.Middleware;
using FormBuilder.Web.Services;
using FormBuilder.Infrastructure.Data;
using FormBuilder.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

    // Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add Identity
    builder.Services.AddDefaultIdentity<User>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        // Mentor uyarısı: Şifre kısıtlaması olmasın
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

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

    // Admin password update from environment
    await UpdateAdminPasswordFromEnvironment(app);

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
    }
}