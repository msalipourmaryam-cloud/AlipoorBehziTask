using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Application.Tools;

public interface IEventMediator
{
    Task TriggerEvents(IEnumerable<IEventNotification> domainEvents, CancellationToken cancellationToken);
}