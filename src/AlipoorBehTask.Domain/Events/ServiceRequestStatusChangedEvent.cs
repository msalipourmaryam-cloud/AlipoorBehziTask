using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.Events;

public sealed record ServiceRequestStatusChangedEvent(
    Guid BeneficiaryId,
    Guid RequestId,
    RequestStatus PreviousStatus,
    RequestStatus NewStatus,
    DateTimeOffset ChangedAtUtc) : IEventNotification;