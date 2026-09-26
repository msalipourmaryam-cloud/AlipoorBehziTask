namespace AlipoorBehTask.Domain.IRepositories;

public interface IServiceRequestEventRepository
{
    void Add(Guid beneficiaryId, Guid? requestId, string eventType, string details, DateTimeOffset occurredAtUtc);
}