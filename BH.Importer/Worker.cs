using BH.Application.Features.Commands;
using BH.Application.Interface;
using BH.Infrastructure.Parsers;
using MediatR;

namespace BH.Importer
{
    public class Worker : BackgroundService
    {
        private readonly Parser_2001 parser_2001;
        private readonly IServiceProvider services;
        private readonly IHostApplicationLifetime hostApplication;
        private readonly IConfiguration configuration;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider services, IHostApplicationLifetime hostApplication, IConfiguration configuration, ILogger<Worker> logger)
        {
            this.parser_2001 = new Parser_2001();
            this.services = services;
            this.hostApplication = hostApplication;
            this.configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //await Task.Delay(1000, stoppingToken);

                try
                {

                    using (var scope = services.CreateScope())
                    {
                        var mediator =
                            scope.ServiceProvider
                                .GetRequiredService<IMediator>();                        

                        _logger.LogInformation("Parsing 2001 html...");

                        //get all verses from document
                        parser_2001.ConvertHtmlDocument(configuration.GetConnectionString("2001HtmlDocument"));

                        _logger.LogInformation("Adding verses to database...");

                        var repo =
                            scope.ServiceProvider
                                .GetRequiredService<IRepository>();

                        await repo.EnsureCreatedAsync();

                        //create command and add all verses to database
                        var cmd = new AddVersesCommand(parser_2001.Verses);
                        await mediator.Send(cmd);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured importing to database");
                    Console.WriteLine(ex.ToString());
                }

                await StopAsync(stoppingToken);

                hostApplication.StopApplication();
            }
        }
    }
}