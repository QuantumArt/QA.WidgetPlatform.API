using QA.WidgetPlatform.Api.Application.Middleware;
using QA.WidgetPlatform.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseMiddleware<StatusCodeExceptionHandlerMiddleware>();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/health");

app.Run();