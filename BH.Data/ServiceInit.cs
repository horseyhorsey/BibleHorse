using BH.Application.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BH.Data
{
    public static class ServiceInit
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            // Create the DbContext servive
            services.AddDbContext<BhDataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(BhDataContext).Assembly.FullName)));


            services.AddScoped<IRepository, BHRepository>();

            return services;
        }
    }
}
