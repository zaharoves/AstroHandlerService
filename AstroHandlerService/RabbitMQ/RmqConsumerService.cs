using Microsoft.Extensions.Options;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using AstroHandlerService.Configurations;
using AstroHandlerService.Services;
using Microsoft.Extensions.Hosting;
using AstroHandlerService.Entities.Enums;

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

                        Console.WriteLine($" [x] Received (Protobuf): Id={message.Id}, BirthDateTime={message.BirthDateTime}, StartDateTime={message.StartDateTime}, EndDateTime={message.EndDateTime}");
                    }

                    // Имитация обработки сообщения
                    var sendMessage = ProcessMessage(message);
                    Console.WriteLine($" [x] Обработано: {message.Id}");

                    // Подтверждаем получение и обработку сообщения.
                    // Если это не сделать, сообщение вернется в очередь (или уйдет в dead-letter exchange, если настроено)
                    // после того, как consumer "умрет" или потеряет соединение.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($" [x] Подтверждено: '{message.Id}'");

                    var rmqMessage = new RmqMessage2()
                    {
                        Id = message.Id,
                        MessageText = sendMessage
                    };

                    _rmqProducer.SendMessage(Guid.NewGuid().ToString(), rmqMessage);
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

        private string ProcessMessage0(RmqMessage message)
        {
            var sendMessage = new StringBuilder();

            var natalInfo = _swissEphemerisService.GetData(message.BirthDateTime);

            var intervalInfo = _swissEphemerisService.GetData(message.StartDateTime, message.EndDateTime, new TimeSpan(0,1,0));

            if (intervalInfo == null)
            {
                return sendMessage.ToString();
            }

            //Заменить на хелпер какой-то?
            var daysAspects = _swissEphemerisService.ProcessAspects0(natalInfo, intervalInfo.Values.ToList());

            foreach (var dayAspects in daysAspects)
            {
                var dateTimeString = dayAspects.Key.ToString("dd MMMM yyyy");

                sendMessage.AppendLine($"------------{dateTimeString}------------");
                Console.WriteLine($"------------{dateTimeString}------------");

                foreach (var aspect in dayAspects.Value)
                {
                    sendMessage.AppendLine($"{aspect.TransitPlanet.Planet} - {aspect.Aspect} - {aspect.NatalPlanet.Planet}");
                    Console.WriteLine($"{aspect.TransitPlanet.Planet} - {aspect.Aspect} - {aspect.NatalPlanet.Planet}");
                }

                sendMessage.AppendLine();
                Console.WriteLine();
            }

            return sendMessage.ToString();
        }

        private string ProcessMessage(RmqMessage message)
        {
            var sendMessage = new StringBuilder();

            var natalInfo = _swissEphemerisService.GetData(message.BirthDateTime);

            var intervalInfo = _swissEphemerisService.GetData(message.StartDateTime, message.EndDateTime, new TimeSpan(0, 1, 0));

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
                    if(dt.Value.Count != 0)
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
    }
}
