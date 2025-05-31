using Microsoft.Extensions.Options;
using AstroHandlerService.Configurations;
using AstroHandlerService.Services;

namespace AstroHandlerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AstroConfig _astroConfiguration;
        private readonly ISwissEphemerisService _swissEphemerisService;

        public Worker(
            ILogger<Worker> logger,
            IOptions<AstroConfig> astroConfiguration,
            ISwissEphemerisService swissEphemerisService)//fortest        
        {
            _logger = logger;
            _astroConfiguration = astroConfiguration.Value;
            _swissEphemerisService = swissEphemerisService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            //var a1 = _swissEphemerisService.GetDataTest(new DateTime(2019, 12, 30));

            //var a2 = _swissEphemerisService.GetDataTest(new DateTime(2019, 12, 31));

            //var a3 = _swissEphemerisService.GetDataTest(new DateTime(2020, 1, 1));

            //var a4 = _swissEphemerisService.GetDataTest(new DateTime(2020, 1, 2));

            //_swissEphemerisService.FillEphemeris(
            //    new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            //    new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            //    new TimeSpan(0, 1, 0));



            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
