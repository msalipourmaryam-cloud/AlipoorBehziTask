using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain.Events;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Api.Handlers;

public sealed class BeneficiaryRegisteredEventHandler(
    IServiceRequestEventRepository eventRepository,
    TimeProvider timeProvider) : IEventHandler<BeneficiaryRegisteredEvent>
{
    public Task HandleAsync(BeneficiaryRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        eventRepository.Add(
            domainEvent.BeneficiaryId,
            null,
            nameof(BeneficiaryRegisteredEvent),
            "Beneficiary registered",
            timeProvider.GetUtcNow());
        return Task.CompletedTask;
    }
}

public sealed class ServiceRequestRegisteredEventHandler(
    IServiceRequestEventRepository eventRepository,
    TimeProvider timeProvider) : IEventHandler<ServiceRequestRegisteredEvent>
{
    public Task HandleAsync(ServiceRequestRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        eventRepository.Add(
            domainEvent.BeneficiaryId,
            domainEvent.RequestId,
            nameof(ServiceRequestRegisteredEvent),
            $"Request registered for service type {domainEvent.ServiceType}",
            timeProvider.GetUtcNow());
        return Task.CompletedTask;
    }
}

public sealed class ServiceRequestStatusChangedEventHandler(
    IServiceRequestEventRepository eventRepository) : IEventHandler<ServiceRequestStatusChangedEvent>
{
    public Task HandleAsync(ServiceRequestStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        eventRepository.Add(
            domainEvent.BeneficiaryId,
            domainEvent.RequestId,
            nameof(ServiceRequestStatusChangedEvent),
            $"Status changed from {domainEvent.PreviousStatus} to {domainEvent.NewStatus}",
            domainEvent.ChangedAtUtc);
        return Task.CompletedTask;
    }
}