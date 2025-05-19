using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using AstroHandlerService.Configurations;
using AstroHandlerService.Entities;
using AstroHandlerService.Providers;
using AstroHandlerService.RMQ;
using AstroHandlerService.Services;
using AstroHandlerService.Tests;

namespace AstroHandlerService
{
    public class Program
    {
        public static void Main(string[] args)
        {


            


            //TestClass.Test1();

            //var startDate = new DateTime(2025, 1, 12);
            //var endDate = new DateTime(2025, 5, 12);
            //TestClass.Test2(startDate, endDate);



            ////////
            //var rmqConfig = RmqConfig.CreateDefault();
            //var rmqConsumer = new RmqConsumerService();

            //var cancellationToken = new CancellationToken();
            //rmqConsumer.StartAsync(cancellationToken);
            ////////



            var builder = Host.CreateDefaultBuilder(args);

            var host = builder.ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AstroConfig>(hostContext.Configuration.GetSection("AstroConfiguration"));
                    services.Configure<RmqConfig>(hostContext.Configuration.GetSection("RabbitMq"));
                    services.Configure<PostgresConfig>(hostContext.Configuration.GetSection("PostgresConfig"));

                    services.AddSingleton<ApplicationContext>(serviceProvider =>
                    {
                        var config = serviceProvider.GetRequiredService<IOptions<PostgresConfig>>();
                        return new ApplicationContext(config);
                    });

                    services.AddSingleton<IEphemerisProvider, EphemerisProvider>();
                    services.AddSingleton<ISwissEphemerisService, SwissEphemerisService>();
                    services.AddSingleton<IRmqProducer, RmqProducer>();
                    services.AddHostedService<RmqConsumerService>();
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
            ////////////////////////////////////////////////////

            //var builder = Host.CreateApplicationBuilder(args);
            //builder.Services.AddHostedService<Worker>();

            //var host = builder.Build();
            //host.Run();
        }
    }
}