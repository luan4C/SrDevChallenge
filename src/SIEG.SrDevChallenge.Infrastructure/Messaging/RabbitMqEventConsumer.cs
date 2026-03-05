using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SIEG.SrDevChallenge.Application.features.Events.DocumentoFiscalCriado;
using SIEG.SrDevChallenge.Domain.Events;
using SIEG.SrDevChallenge.Infrastructure.Messaging.Models;
using System.Text;

namespace SIEG.SrDevChallenge.Infrastructure.Messaging;

public class RabbitMqEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventConsumer> _logger;

    public RabbitMqEventConsumer(IOptions<RabbitMqConfigurations> configuration, IServiceProvider serviceProvider, ILogger<RabbitMqEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var config = configuration.Value ?? throw new InvalidOperationException("RabbitMQ configuration not found");

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
 
        await SetupDocumentoFiscalCriadoConsumer(stoppingToken);

 
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SetupDocumentoFiscalCriadoConsumer(CancellationToken cancellationToken)
    {
        string queueName = nameof(DocumentoFiscalCriado).ToLowerInvariant();

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var evento = JsonConvert.DeserializeObject<DocumentoFiscalCriado>(message);
                if (evento != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var command = new ProcessDocumentoFiscalCriadoCommand(evento);
                    await mediator.Send(command);

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Event processed successfully: DocumentoFiscalCriado {Id}", evento.DocumentoFiscalId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DocumentoFiscalCriado event: {Message}", message);
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        _logger.LogInformation("Started consuming DocumentoFiscalCriado events from queue: {Queue}", queueName);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}