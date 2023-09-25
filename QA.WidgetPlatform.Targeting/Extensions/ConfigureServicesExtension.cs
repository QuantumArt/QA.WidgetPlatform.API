using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Targeting.Factories;
using QA.WidgetPlatform.Targeting.Settings;
using System.ComponentModel;
using System.Reflection;

namespace QA.WidgetPlatform.Targeting.Extensions
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureTargetingServices(this IServiceCollection services, IConfiguration configuration)
        {            
            services.Configure<TargetingFilterSettings>(configuration.GetSection("TargetingFilterSettings"));

            //map targeting filter factory
            var targetingFilter = configuration.GetSection("TargetingFilterSettings").Get<TargetingFilterSettings>();
            bool isRegistered = false;


            if (targetingFilter?.TargetingLibrary != null)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetingFilter.TargetingLibrary);

                try
                {
                    var assembly = Assembly.LoadFile(path);

                    foreach (var t in assembly.GetExportedTypes())
                    {
                        if (typeof(ITargetingFiltersFactory).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        {
                            services.TryAddSingleton(typeof(ITargetingFiltersFactory), t);
                            isRegistered = true;
                            break;
                        }
                    }
                }
                catch(ArgumentException)
                {
                }
            }

            if (!isRegistered)
            {
                services.TryAddSingleton<ITargetingFiltersFactory, EmptyTargetingFiltersFactory>();
            }

            return services;
        }
    }
}
