namespace AlipoorBehTask.Domain.Aggregates;

public interface IServiceRequest
{
    Guid Id { get; }
    Guid BeneficiaryId { get; }
    ServiceType ServiceType { get; }
    DateOnly RegistrationDate { get; }
    string Description { get; }
    RequestStatus Status { get; }
    int InitialPriorityScore { get; }
}