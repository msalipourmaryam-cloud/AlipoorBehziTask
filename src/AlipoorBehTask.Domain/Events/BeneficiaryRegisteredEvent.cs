using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.Events;

public sealed record BeneficiaryRegisteredEvent(Guid BeneficiaryId) : IEventNotification;