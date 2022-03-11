using Microsoft.AspNetCore.Builder;
using QA.WidgetPlatform.Api.Application.Middleware;
using QA.WidgetPlatform.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureBaseServices(builder.Configuration);
    
var app = builder.Build();

app.UseMiddleware<StatusCodeExceptionHandlerMiddleware>();
app.UseRouting();
app.UseAuthorization();
app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();

app.Run();