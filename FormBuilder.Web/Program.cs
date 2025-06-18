using Serilog;
using Serilog.Events;
using FormBuilder.Web.Middleware;
using FormBuilder.Web.Services;

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
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

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