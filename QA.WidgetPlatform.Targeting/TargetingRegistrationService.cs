using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.Targeting.Configuration;
using QA.DotNetCore.Engine.Targeting.Filters;
using QA.DotNetCore.Engine.Targeting.TargetingProviders;

namespace QA.WidgetPlatform.Targeting
{
    public class TargetingRegistrationService : ITargetingRegistration
    {
        public void RegisterTargetingServices(IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<RegionFilter>();
            services.TryAddSingleton<CultureFilter>();
            services.TryAddSingleton<QueryTargetingProvider>();
        }

        public void ConfigureTargeting(IApplicationBuilder app)
        {
            app.UseTargeting(providers =>
            {
                providers.Register<QueryTargetingProvider>();
            });

            app.UseSiteStructureFilters(filters =>
            {
                filters.Register<RegionFilter>();
                filters.Register<CultureFilter>();
            });
        }
    }
}
