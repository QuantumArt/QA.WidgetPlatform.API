using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
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
            services.AddSiteStructure(options =>
            {
                options.UseQpSettings(configuration.GetSection("QpSettings").Get<QpSettings>());
            });
            services.AddScoped<ISiteStructureService, SiteStructureService>();
            services.TryAddSingleton<ITargetingFiltersFactory, EmptyTargetingFiltersFactory>();

            return services;
        } 
    }
}