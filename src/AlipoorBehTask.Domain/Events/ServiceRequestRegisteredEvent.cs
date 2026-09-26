using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.Events;

public sealed record ServiceRequestRegisteredEvent(
    Guid BeneficiaryId,
    Guid RequestId,
    ServiceType ServiceType,
    DateOnly RegisteredOn) : IEventNotification;