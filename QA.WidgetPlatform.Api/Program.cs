using QA.WidgetPlatform.Api.Application.Middleware;
using QA.WidgetPlatform.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureBaseServicesWithoutInvalidation(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<StatusCodeExceptionHandlerMiddleware>();
app.UseRouting();
app.UseAuthorization();
app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/health");

app.Run();