using NLog;
using NLog.Web;
using QA.DotNetCore.Engine.CacheTags.Configuration;
using QA.DotNetCore.Engine.Targeting.Configuration;
using QA.WidgetPlatform.Api.Application.Middleware;
using QA.WidgetPlatform.Api.Infrastructure;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Init WP.API service");
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.WebHost.SuppressStatusMessages(true);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            policyBuilder.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                Array.Empty<string>());
        });
    });

    builder.Services.ConfigureBaseServices(builder.Configuration);
    builder.Services.AddApiTargeting(builder.Configuration);

    var app = builder.Build();

    app.UseMiddleware<StatusCodeExceptionHandlerMiddleware>();
    app.UseCacheTagsInvalidation();
    app.UseRouting();
    app.UseCors();
    app.UseAuthorization();
    app.UseSwaggerUI();
    app.MapControllers();
    app.MapSwagger();
    app.MapHealthChecks("/health");

    app.Run();

}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}

