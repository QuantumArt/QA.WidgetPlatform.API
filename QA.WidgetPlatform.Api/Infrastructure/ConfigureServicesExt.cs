using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using QA.DotNetCore.Caching;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.CacheTags.Configuration;
using QA.DotNetCore.Engine.Persistent.Interfaces.Settings;
using QA.DotNetCore.Engine.QpData.Configuration;
using QA.WidgetPlatform.Api.Services;
using QA.WidgetPlatform.Api.Services.Abstract;
using System.Text.Json.Serialization;
using QA.DotNetCore.Engine.CacheTags;

namespace QA.WidgetPlatform.Api.Infrastructure
{
    public static class ConfigureServicesExt
    {
        public static void ConfigureBaseServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Widget Platform API", Version = "v1" });
                foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);
                }
            });

            services.AddMemoryCache();
            var qpSettings = configuration.GetQpSettings();
            services.AddSiteStructure(options =>
            {
                options.UseQpSettings(qpSettings);
            });

            //подключение сервисов для работы кештегов
            services.AddSingleton<IModificationStateStorage, DefaultModificationStateStorage>();
            services.AddScoped<ISiteStructureService, SiteStructureService>();
            services.TryAddSingleton<ITargetingFiltersFactory, EmptyTargetingFiltersFactory>();   
            
            var cacheTagService = services.AddCacheTagServices();
            //настройка стратегии инвалидации по кештегам
            if (qpSettings.IsStage)
            {
                //при каждом запросе запускать все зарегистрированные ICacheTagTracker,
                //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                _ = cacheTagService.WithInvalidationByMiddleware(@"^.*\/(__webpack.*|.+\.[a-zA-Z0-9]+)$");//отсекаем левые запросы для статики (для каждого сайта может настраиваться индивидуально)
            }
            else
            {
                //по таймеру запускать все зарегистрированные ICacheTagTracker,
                //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                _ = cacheTagService.WithInvalidationByTimer();
            }
            cacheTagService.WithCacheTrackers(invalidation =>
            {
                //QpContentCacheTracker - уже реализованный ICacheTagTracker, который работает на базе механизма CONTENT_MODIFICATION из QP
                invalidation.Register<QpContentCacheTracker>();
            });
        }

        private static QpSettings GetQpSettings(this IConfiguration configuration) =>
            configuration.GetSection("QpSettings").Get<QpSettings>();
    }
}
