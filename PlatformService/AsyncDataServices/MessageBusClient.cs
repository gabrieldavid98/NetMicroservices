using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public sealed class MessageBusClient : IMessageBusClient, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageBusClient> _logger;
        private readonly IConnection _connection;
        private readonly IModel _model;
        private const string Exchange = "trigger";

        public MessageBusClient(IConfiguration configuration, ILogger<MessageBusClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var connectionFactory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQHost"],
                Port = Convert.ToInt32(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = connectionFactory.CreateConnection();
                _model = _connection.CreateModel();

                _model.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMqConnectionShutDown;

                _logger.LogInformation("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not connect to the Message Bus: {Message}", ex.Message);
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (!_connection.IsOpen)
            {
                _logger.LogWarning("--> RabbitMQ connection is closed, message not published");
                return;
            }

            _logger.LogInformation("--> Publishing message: {Message}", message);
            PublishMessage(message);
            _logger.LogInformation("--> Message published", message);
        }

        private void RabbitMqConnectionShutDown(object sender, ShutdownEventArgs ex) =>
            _logger.LogInformation("--> Rabbitmq Connection Shutdown");

        private void PublishMessage(string message)
        {
            var rawMessage = Encoding.UTF8.GetBytes(message);
            _model.BasicPublish(
                exchange: Exchange,
                routingKey: string.Empty,
                basicProperties: null,
                body: rawMessage
            );
        }

        public void Dispose()
        {
            _logger.LogInformation("--> MessageBus disposed");
            
            if (!_connection.IsOpen) return;
            
            _connection.Dispose();
            _model.Dispose();
        }
    }
}