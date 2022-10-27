using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BH.Tests
{
    public static class ServicesLoader
    {
        private static ServiceProvider _serviceProvider = null;
        public static ServiceProvider LoadServices()
        {
            if (_serviceProvider == null)
            {
                IConfiguration config = new ConfigurationBuilder()
               //.SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddUserSecrets(typeof(ServicesLoader).Assembly)
               .Build();

                //add the infrastructure services
                IServiceCollection serviceCollection = new ServiceCollection();
                
                serviceCollection = BH.Data.ServiceInit.AddDatabaseContext(serviceCollection, config);
                serviceCollection = BH.Application.ServiceInit.AddApplication(serviceCollection);

                _serviceProvider = serviceCollection.AddLogging().BuildServiceProvider();
            }

            return _serviceProvider;
        }
    }
}
