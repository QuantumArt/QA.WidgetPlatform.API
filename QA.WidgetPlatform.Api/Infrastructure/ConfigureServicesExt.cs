using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using QA.DotNetCore.Engine.CacheTags.Configuration;
using QA.DotNetCore.Engine.Persistent.Interfaces.Settings;
using QA.DotNetCore.Engine.QpData.Configuration;
using QA.WidgetPlatform.Api.Services;
using QA.WidgetPlatform.Api.Services.Abstract;

namespace QA.WidgetPlatform.Api.Infrastructure
{
    public static class ConfigureServicesExt
    {
        public static IServiceCollection ConfigureBaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo {Title = "Widget Platform API", Version = "v1"});
                foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);
                }
            });

            services.AddMemoryCache();
            var qpSettings = configuration.GetSection("QpSettings").Get<QpSettings>();
            services.AddSiteStructure(options =>
            {
                options.UseQpSettings(qpSettings);
            });

            //подключение сервисов для работы кештегов
            services.AddCacheTagServices(options =>
            {
                //настройка стратегии инвалидации по кештегам
                if (qpSettings.IsStage)
                {
                    //при каждом запросе запускать все зарегистрированные ICacheTagTracker,
                    //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                    options.InvalidateByMiddleware(@"^.*\/(__webpack.*|.+\.[a-zA-Z0-9]+)$");//отсекаем левые запросы для статики (для каждого сайта может настраиваться индивидуально)
                }
                else
                {
                    //по таймеру запускать все зарегистрированные ICacheTagTracker,
                    //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                    options.InvalidateByTimer( TimeSpan.FromSeconds(30));
                }
            });
            services.AddScoped<ISiteStructureService, SiteStructureService>();
            services.TryAddSingleton<ITargetingFiltersFactory, EmptyTargetingFiltersFactory>();

            return services;
        } 
    }
}