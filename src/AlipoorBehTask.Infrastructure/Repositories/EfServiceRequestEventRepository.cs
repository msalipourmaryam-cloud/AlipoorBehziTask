using AlipoorBehTask.Domain.IRepositories;
using AlipoorBehTask.Infrastructure.Models;

namespace AlipoorBehTask.Infrastructure.Repositories;

public sealed class EfServiceRequestEventRepository(AlipoorBehTaskDbContext context) : IServiceRequestEventRepository
{
    public void Add(
        Guid beneficiaryId,
        Guid? requestId,
        string eventType,
        string details,
        DateTimeOffset occurredAtUtc)
    {
        context.ServiceRequestEvents.Add(new ServiceRequestEvent
        {
            Id = Guid.NewGuid(),
            BeneficiaryId = beneficiaryId,
            RequestId = requestId,
            EventType = eventType,
            Details = details,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime()
        });
    }
}