using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Application.Tools;

public interface IEventHandler
{
}

public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEventNotification
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}