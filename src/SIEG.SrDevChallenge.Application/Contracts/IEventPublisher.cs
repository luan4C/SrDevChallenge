using SIEG.SrDevChallenge.Domain.Events;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData, string? queueName = default, CancellationToken cancellationToken = default) where T : class;

}