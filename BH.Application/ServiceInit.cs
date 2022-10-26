using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BH.Application
{
    public static class ServiceInit
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //var assemblies = Assembly.GetExecutingAssembly();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
