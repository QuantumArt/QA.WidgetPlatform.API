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
        public static IServiceCollection ConfigureBaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var qpSettings = configuration.GetQpSettings();
            var builder = services.ConfigureBaseServicesWithoutInvalidation(options => options.UseQpSettings(qpSettings));

            //настройка стратегии инвалидации по кештегам
            if (qpSettings.IsStage)
            {
                //при каждом запросе запускать все зарегистрированные ICacheTagTracker,
                //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                _ = builder.WithInvalidationByMiddleware(@"^.*\/(__webpack.*|.+\.[a-zA-Z0-9]+)$");//отсекаем левые запросы для статики (для каждого сайта может настраиваться индивидуально)
            }
            else
            {
                //по таймеру запускать все зарегистрированные ICacheTagTracker,
                //чтобы получить все теги по которым нужно сбросить кеш, и сбросить его
                _ = builder.WithInvalidationByTimer();
            }
            builder.WithCacheTrackers(invalidation =>
            {
                //QpContentCacheTracker - уже реализованный ICacheTagTracker, который работает на базе механизма CONTENT_MODIFICATION из QP
                invalidation.Register<QpContentCacheTracker>();
            });
            return services;
        }

        public static ICacheTagConfigurationBuilder ConfigureBaseServicesWithoutInvalidation(
            this IServiceCollection services,
            Action<SiteStructureOptions> siteStructureOptions)
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
            services.AddSiteStructure(siteStructureOptions);

            services.AddScoped<ISiteStructureService, SiteStructureService>();
            services.TryAddSingleton<ITargetingFiltersFactory, EmptyTargetingFiltersFactory>();
            return services.AddCacheTagServices();
        }

        public static QpSettings GetQpSettings(this IConfiguration configuration) =>
            configuration.GetSection("QpSettings").Get<QpSettings>();
    }
}
