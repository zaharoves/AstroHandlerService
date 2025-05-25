using Microsoft.Extensions.Options;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using AstroHandlerService.Services;
using AstroHandlerService.Entities.Enums;
using AstroHandlerService.Entities;

namespace AstroHandlerService.RMQ
{
    public class RmqConsumerService : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;

        private RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IModel _channel;

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

            _queueName = rmqConfig.Value.QueueName1;
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

                    RmqMessage message = null;

                    using (var memoryStream = new MemoryStream(body))
                    {
                        message = Serializer.Deserialize<RmqMessage>(memoryStream);

                        Console.WriteLine($" [x] Received (Protobuf): Id={message.MessageId}, BirthDateTime={message.DatePickerData.DateTime}");
                    }

                    // Имитация обработки сообщения
                    //var sendMessage = ProcessMessage(message);
                    var nowDateTime = DateTime.Now;
                    var aspects = ProcessDailyForecast(message, nowDateTime);
                    var rmqMessage = ConvertToRmqMessage(message.MessageId, aspects, nowDateTime);

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

        //private string ProcessMessage0(DatePickerData message)
        //{
        //    var sendMessage = new StringBuilder();

        //    var natalInfo = _swissEphemerisService.GetData(message.DateTime BirthDateTime.Date);

        //    var intervalInfo = _swissEphemerisService.GetData(message.StartDateTime, message.EndDateTime, new TimeSpan(0, 1, 0));

        //    if (intervalInfo == null)
        //    {
        //        return sendMessage.ToString();
        //    }

        //    //Заменить на хелпер какой-то?
        //    var daysAspects = _swissEphemerisService.ProcessAspects0(natalInfo, intervalInfo.Values.ToList());

        //    foreach (var dayAspects in daysAspects)
        //    {
        //        var dateTimeString = dayAspects.Key.ToString("dd MMMM yyyy");

        //        sendMessage.AppendLine($"------------{dateTimeString}------------");
        //        Console.WriteLine($"------------{dateTimeString}------------");

        //        foreach (var aspect in dayAspects.Value)
        //        {
        //            sendMessage.AppendLine($"{aspect.TransitPlanet.Planet} - {aspect.Aspect} - {aspect.NatalPlanet.Planet}");
        //            Console.WriteLine($"{aspect.TransitPlanet.Planet} - {aspect.Aspect} - {aspect.NatalPlanet.Planet}");
        //        }

        //        sendMessage.AppendLine();
        //        Console.WriteLine();
        //    }

        //    return sendMessage.ToString();
        //}

        private string ProcessMessage(RmqMessage message)
        {
            var sendMessage = new StringBuilder();

            var natalInfo = _swissEphemerisService.GetChartData(message.DatePickerData.DateTime.Value);

            var intervalInfo = _swissEphemerisService.GetData(new DateTime(1901, 1, 1), new DateTime(1901, 1, 2), new TimeSpan(0, 1, 0));

            if (intervalInfo == null)
            {
                return sendMessage.ToString();
            }

            //Заменить на хелпер какой-то?
            var planetMainList = _swissEphemerisService.ProcessAspects(natalInfo, intervalInfo.Values.ToList());

            foreach (var planetMain in planetMainList)
            {
                sendMessage.AppendLine($"------------{planetMain.Planet.ToString()}------------");
                Console.WriteLine($"------------{planetMain.Planet.ToString()}------------");

                var dictAspectsStr = new Dictionary<DateTime, List<string>>();

                foreach (var dt in planetMain.Aspects)
                {
                    var aspectsStrList = new List<string>();

                    foreach (var aspect in dt.Value)
                    {
                        aspectsStrList.Add($"{aspect.TransitPlanet.Planet} - {aspect.Aspect} - {aspect.NatalPlanet.Planet}");
                    }

                    if (aspectsStrList.Count != 1)
                    {
                        dictAspectsStr.Add(dt.Key, aspectsStrList);
                    }
                }

                foreach (var dt in dictAspectsStr)
                {
                    if (dt.Value.Count != 0)
                    {
                        sendMessage.AppendLine($"{dt.Key.ToString("dd MMMM yyyy")}:");
                        Console.WriteLine($"{dt.Key.ToString("dd MMMM yyyy")}:");
                    }

                    foreach (var aspectStr in dt.Value)
                    {
                        sendMessage.AppendLine($"{aspectStr}");
                        Console.WriteLine($"{aspectStr}");
                    }
                }

                sendMessage.AppendLine();
                Console.WriteLine();
            }

            return sendMessage.ToString();
        }

        private List<AspectInfo> ProcessDailyForecast(RmqMessage message, DateTime dateTime)
        {
            //all aspects
            var natalChart = _swissEphemerisService.GetChartData(message.DatePickerData.DateTime.Value);
            var transitChart = _swissEphemerisService.GetChartData(dateTime);

            var aspects = _swissEphemerisService.GetAspects(natalChart, transitChart);

            //moon aspects
            var currentDate = DateTime.Now;
            var transitChartList = new List<ChartInfo>();

            while (currentDate < dateTime.AddHours(14))
            {
                var currentTransitChart = _swissEphemerisService.GetChartData(dateTime);
                transitChartList.Add(currentTransitChart);

                currentDate = currentDate.AddHours(1);
            }

            var moonAspects = _swissEphemerisService.GetMoonAspects(natalChart[PlanetEnum.Moon], transitChartList);

            //result aspects
            aspects.AddRange(moonAspects);

            return aspects;
        }

        private RmqMessage2 ConvertToRmqMessage(string messageId, List<AspectInfo> aspects, DateTime dateTime)
        {
            var rmqMesage = new RmqMessage2();

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
