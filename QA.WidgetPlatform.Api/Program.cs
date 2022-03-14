using Microsoft.AspNetCore.Builder;
using QA.DotNetCore.Engine.CacheTags;
using QA.DotNetCore.Engine.CacheTags.Configuration;
using QA.WidgetPlatform.Api.Application.Middleware;
using QA.WidgetPlatform.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureBaseServices(builder.Configuration);
    
var app = builder.Build();

app.UseMiddleware<StatusCodeExceptionHandlerMiddleware>();
//мидлвара для инвалидации кештегов
//необходимо, чтобы было подключено services.AddCacheTagServices
app.UseCacheTagsInvalidation(trackers =>
{
    //регистрация одного или нескольких ICacheTagTracker
    //QpContentCacheTracker - уже реализованный ICacheTagTracker, который работает на базе механизма CONTENT_MODIFICATION из QP
    trackers.Register<QpContentCacheTracker>();
});
app.UseRouting();
app.UseAuthorization();
app.UseSwaggerUI();
app.MapControllers();
app.MapSwagger();

app.Run();