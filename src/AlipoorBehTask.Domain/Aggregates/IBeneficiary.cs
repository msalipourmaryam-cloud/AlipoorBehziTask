using AlipoorBehTask.Domain.DTOs;
using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.Aggregates;

public interface IBeneficiary : IEntity<Guid>
{
    string NationalId { get; }
    int Age { get; }
    MaritalStatus MaritalStatus { get; }
    int DependentCount { get; }
    DisabilityType DisabilityType { get; }
    decimal MonthlyIncome { get; }
    IReadOnlyCollection<ServiceRequest> Requests { get; }
    ServiceRequest AddRequest(IServiceRequestRegistrationDTO request, DateOnly registeredOn, int initialPriorityScore);
    void ChangeRequestStatus(Guid requestId, RequestStatus nextStatus, DateTimeOffset changedAtUtc);
}