using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private readonly ILogger<MessageBusSubscriber> _logger;
        private IConnection _connection;
        private IModel _model;
        private string _queueName;

        public MessageBusSubscriber(
            IConfiguration configuration,
            IEventProcessor eventProcessor,
            ILogger<MessageBusSubscriber> logger)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            _logger = logger;
            
            InitializeRabbitMq();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var eventingBasicConsumer = new EventingBasicConsumer(_model);
            
            eventingBasicConsumer.Received += EventingBasicConsumerOnReceived;
            _model.BasicConsume(queue: _queueName, autoAck: true, consumer: eventingBasicConsumer);
            
            return Task.CompletedTask;
        }

        private void EventingBasicConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            _logger.LogInformation("--> Event Received");
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            
            _eventProcessor.ProcessEvent(message);
        }

        private void InitializeRabbitMq()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQHost"],
                Port = Convert.ToInt32(_configuration["RabbitMQPort"])
            };

            _connection = connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            _model.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
            _queueName = _model.QueueDeclare().QueueName;
            _model.QueueBind(queue: _queueName, routingKey: string.Empty, exchange: "trigger");
            
            _logger.LogInformation("--> Listening on the Message Bus");

            _connection.ConnectionShutdown += RabbitMqConnectionShutdown;
        }

        private void RabbitMqConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation("--> Connection Shutdown");
        }

        public override void Dispose()
        {
            if (!_connection.IsOpen) return;
            
            _model.Close();
            _connection.Close();
        }
    }
}