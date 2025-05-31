using Microsoft.Extensions.Options;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AstroHandlerService.Services;
using AstroHandlerService.Enums;
using AstroHandlerService.Entities;

namespace AstroHandlerService.RMQ
{
    public class RmqConsumerService : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;

        private IConnection _connection;
        private IModel _channel;

        private readonly string _exchangeName;
        private readonly string _routingKey;

        private readonly ISwissEphemerisService _swissEphemerisService;
        private readonly IRmqProducer _rmqProducer;

        public RmqConsumerService(
            IOptions<RmqConfig> rmqConfig,
            ISwissEphemerisService swissEphemerisService,
            IRmqProducer rmqProducer)
        {
            _factory = new ConnectionFactory()
            {
                HostName = rmqConfig.Value.HostName,
                Port = rmqConfig.Value.Port,
                UserName = rmqConfig.Value.UserName,
                Password = rmqConfig.Value.Password,
                VirtualHost = rmqConfig.Value.VirtualHost
            };

            _queueName = rmqConfig.Value.UserInfoQueue;
            _exchangeName = "exchangeName";
            _routingKey = "routingKey";

            _swissEphemerisService = swissEphemerisService;
            _rmqProducer = rmqProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Просто чтобы сервис не завершался сразу
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Объявляем обменник (если он еще не существует)
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);

                // Объявляем очередь (если она еще не существует)
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Привязываем очередь к обменнику с ключом маршрутизации
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();

                    UserInfoMessage message = null;

                    using (var memoryStream = new MemoryStream(body))
                    {
                        message = Serializer.Deserialize<UserInfoMessage>(memoryStream);

                        Console.WriteLine($" [x] Received (Protobuf): Id={message.MessageId}, BirthDateTime={message.DateTime}");
                    }

                    // Имитация обработки сообщения
                    //var sendMessage = ProcessMessage(message);
                    var utcNowDate = DateTime.UtcNow;

                    var aspects = ProcessDailyForecast(message, utcNowDate);
                    var rmqMessage = ConvertToRmqMessage(message.MessageId, aspects, utcNowDate);

                    Console.WriteLine($" [x] Обработано: {message.MessageId}");

                    // Подтверждаем получение и обработку сообщения.
                    // Если это не сделать, сообщение вернется в очередь (или уйдет в dead-letter exchange, если настроено)
                    // после того, как consumer "умрет" или потеряет соединение.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($" [x] Подтверждено: '{message.MessageId}'");

                    _rmqProducer.SendMessage(message.MessageId, rmqMessage);
                };

                // Начинаем потребление сообщений
                _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.WriteLine($"Не удалось подключиться к RabbitMQ: {ex.Message}");
                Console.WriteLine($"Убедитесь, что RabbitMQ запущен и доступен по адресу {_factory.HostName}:{_factory.Port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private List<AspectInfo> ProcessDailyForecast(UserInfoMessage message, DateTime utcDateTime)
        {
            //planets aspects (without moon)
            var natalDateTime = (message.DateTime.Value - message.GmtOffset).Value;

            var natalChart = _swissEphemerisService.GetChartData(natalDateTime);
            var transitChart = _swissEphemerisService.GetChartData(utcDateTime);
            
            var planetsAspects = _swissEphemerisService.GetAspects(
                natalChart, 
                transitChart, 
                PlanetEnum.Sun, 
                PlanetEnum.Mercury, 
                PlanetEnum.Venus, 
                PlanetEnum.Mars, 
                PlanetEnum.Jupiter, 
                PlanetEnum.Saturn, 
                PlanetEnum.Uran, 
                PlanetEnum.Neptune, 
                PlanetEnum.Pluto);

            //moon aspects
            var startUtcDate = utcDateTime;
            var endUtcDate = startUtcDate.AddDays(1);

            var moonAspects = _swissEphemerisService.GetMoonAspects(natalChart, startUtcDate, endUtcDate);

            //result aspects
            var resultAspects = new List<AspectInfo>();
            resultAspects.AddRange(moonAspects);
            resultAspects.AddRange(planetsAspects);

            return resultAspects;
        }

        private DailyForecastMessage ConvertToRmqMessage(string messageId, List<AspectInfo> aspects, DateTime dateTime)
        {
            var rmqMesage = new DailyForecastMessage();

            rmqMesage.Id = messageId;
            rmqMesage.DateTime = dateTime;

            rmqMesage.Aspects = new List<RmqMessageAspect>();

            foreach (var aspect in aspects)
            {
                var rmqMessageAspect = new RmqMessageAspect()
                { 
                    NatalPlanet = aspect.NatalPlanet.Planet.ToString(),
                    NatalZodiac = aspect.NatalPlanet.Zodiac.ToString(),
                    NatalZodiacAngles = aspect.NatalPlanet.ZodiacAngles,
                    IsNatalRetro = aspect.NatalPlanet.IsRetro,

                    TransitPlanet = aspect.TransitPlanet.Planet.ToString(),
                    TransitZodiac = aspect.TransitPlanet.Zodiac.ToString(),
                    TransitZodiacAngles = aspect.TransitPlanet.ZodiacAngles,
                    IsTransitRetro = aspect.TransitPlanet.IsRetro,

                    Aspect = aspect.Aspect.ToString()
                };

                rmqMesage.Aspects.Add(rmqMessageAspect);
            }

            return rmqMesage;
        }
    }
}
