using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Domain.Events;
using SIEG.SrDevChallenge.Infrastructure.Messaging.Models;
using System.Text;

namespace SIEG.SrDevChallenge.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IOptions<RabbitMqConfigurations> rabbitOptions, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        var config = rabbitOptions.Value
            ?? throw new InvalidOperationException("RabbitMQ configuration not found");

        var factory = new ConnectionFactory()
        {
            HostName = config.Host,
            UserName = config.User,
            Password = config.Password,
            Port = config.Port
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public async Task PublishAsync<T>(T eventData, string? queueName = default, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            queueName ??= typeof(T).Name.ToLowerInvariant();
            // Declara a fila se ela não existir
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonConvert.SerializeObject(eventData);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Event published to queue {Queue}: {EventType}", queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to queue {Queue}: {EventType}", queueName, typeof(T).Name);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}