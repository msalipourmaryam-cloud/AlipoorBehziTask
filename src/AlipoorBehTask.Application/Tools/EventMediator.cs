using System.Collections;
using System.Reflection;
using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Application.Tools;

public sealed class EventMediator(IServiceProvider services) : IEventMediator
{
    public async Task TriggerEvents(IEnumerable<IEventNotification> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents.ToArray())
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());
            var collectionType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            if (services.GetService(collectionType) is not IEnumerable handlers)
                continue;

            var handleMethod = handlerType.GetMethod(nameof(IEventHandler<IEventNotification>.HandleAsync))!;
            foreach (var handler in handlers)
            {
                var task = (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                await task.ConfigureAwait(false);
            }
        }
    }
}