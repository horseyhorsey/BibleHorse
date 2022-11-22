using BH.Application.Interface;
using BH.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BH.Infrastructure
{
    public static class ServiceInit
    {
        public static IServiceCollection AddBibleService(this IServiceCollection services)
        {
            services.AddScoped<IBibleService, BibleService>();
            return services;
        }
    }
}
