namespace AlipoorBehTask.Infrastructure.Models;

public sealed class ServiceRequestEvent
{
    public Guid Id { get; set; }
    public Guid BeneficiaryId { get; set; }
    public Guid? RequestId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
}