using BH.TelegramBot.Service;

namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //add datacontext
                    services = BH.Data.ServiceInit.AddDatabaseContext(services, hostContext.Configuration);

                    //add mediator
                    services = BH.Application.ServiceInit.AddApplication(services);

                    //add worker / console
                    services.AddHostedService<TelegramBotWorker>();
                })
                .Build();

            host.Run();
        }
    }
}