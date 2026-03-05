using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
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
    private readonly RabbitMqRetryConfiguration _retryConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;
    public RabbitMqEventConsumer(IOptions<RabbitMqConfigurations> configuration,
    IOptions<RabbitMqRetryConfiguration> retryConfig,
    IServiceProvider serviceProvider,
     ILogger<RabbitMqEventConsumer> logger)
    {
        _retryConfig = retryConfig.Value;
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
    private ResiliencePipeline CreateResiliencePipeline()
    {
        var pipelineBuilder = new ResiliencePipelineBuilder();


        pipelineBuilder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = _retryConfig.MaxRetryAttempts,
            BackoffType = DelayBackoffType.Exponential,
            Delay = _retryConfig.InitialDelay,
            MaxDelay = _retryConfig.MaxDelay,
            OnRetry = args =>
            {
                _logger.LogWarning("Retry attempt {Attempt} after {Delay}ms. Exception: {Exception}",
                    args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        });


        if (_retryConfig.EnableCircuitBreaker)
        {
            pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = _retryConfig.CircuitBreakerThreshold,
                BreakDuration = _retryConfig.CircuitBreakerDuration,
                OnOpened = args =>
                {
                    _logger.LogError("Circuit breaker opened due to failures");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker closed");
                    return ValueTask.CompletedTask;
                }
            });
        }

        return pipelineBuilder.Build();
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
        string dlxQueueName = queueName + _retryConfig.DeadLetterSuffix;

        if (_retryConfig.EnableDeadLetter)
        {
            await SetupDeadLetterExchange(queueName, dlxQueueName, cancellationToken);
        }

        var queueArgs = new Dictionary<string, object>();
        if (_retryConfig.EnableDeadLetter)
        {
            queueArgs.Add("x-dead-letter-exchange", "");
            queueArgs.Add("x-dead-letter-routing-key", dlxQueueName);
        }

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
            var deliveryTag = ea.DeliveryTag;
            var retryCount = GetRetryCount(ea.BasicProperties);


            try
            {
                await _resiliencePipeline.ExecuteAsync(async ct =>
                {
                    await ProcessMessage(ea);
                }, cancellationToken);

                await _channel.BasicAckAsync(deliveryTag: deliveryTag, multiple: false);
                _logger.LogInformation("Message processed successfully after {RetryCount} retries", retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message after all retries. RetryCount: {RetryCount}", retryCount);

                if (retryCount >= _retryConfig.MaxRetryAttempts)
                {
                    // Enviar para Dead Letter
                    await _channel.BasicNackAsync(deliveryTag: deliveryTag, multiple: false, requeue: false);
                    _logger.LogError("Message sent to dead letter queue after {MaxRetries} attempts", _retryConfig.MaxRetryAttempts);
                }
                else
                {
                    // Rejeitar para retry
                    await _channel.BasicNackAsync(deliveryTag: deliveryTag, multiple: false, requeue: true);
                }
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        _logger.LogInformation("Started consuming DocumentoFiscalCriado events from queue: {Queue}", queueName);
    }
    private async Task ProcessMessage(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        var evento = JsonConvert.DeserializeObject<DocumentoFiscalCriado>(message);
        if (evento != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new ProcessDocumentoFiscalCriadoCommand(evento);
            await mediator.Send(command);

            _logger.LogInformation("Event processed successfully: DocumentoFiscalCriado {Id}", evento.DocumentoFiscalId);
        }
    }
    private static int GetRetryCount(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers != null &&
            properties.Headers.TryGetValue("x-retry-count", out var retryCountObj))
        {
            return Convert.ToInt32(retryCountObj);
        }
        return 0;
    }

    private static void SetRetryCount(IBasicProperties properties, int retryCount)
    {
        properties.Headers ??= new Dictionary<string, object>();
        properties.Headers["x-retry-count"] = retryCount;
        
    }
    private async Task SetupDeadLetterExchange(string mainQueue, string dlxQueue, CancellationToken cancellationToken)
    {

        await _channel.QueueDeclareAsync(
            queue: dlxQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Dead letter queue created: {DlxQueue}", dlxQueue);
    }
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}