namespace AlipoorBehTask.Domain.Aggregates;

public sealed class ServiceRequest : IServiceRequest
{
    private ServiceRequest()
    {
        Description = string.Empty;
    }

    internal ServiceRequest(
        Guid beneficiaryId,
        ServiceType serviceType,
        DateOnly registrationDate,
        string description,
        int initialPriorityScore)
    {
        Id = Guid.NewGuid();
        BeneficiaryId = beneficiaryId;
        ServiceType = serviceType;
        RegistrationDate = registrationDate;
        Description = description;
        Status = RequestStatus.Pending;
        InitialPriorityScore = initialPriorityScore;
    }

    public Guid Id { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public DateOnly RegistrationDate { get; private set; }
    public string Description { get; private set; }
    public RequestStatus Status { get; private set; }
    public int InitialPriorityScore { get; private set; }

    internal void ChangeStatus(RequestStatus nextStatus)
    {
        if (!Enum.IsDefined(nextStatus))
            throw new DomainRuleException("Request status is not supported.");

        var allowed = Status switch
        {
            RequestStatus.Pending => nextStatus is RequestStatus.Approved or RequestStatus.Rejected,
            RequestStatus.Approved => nextStatus == RequestStatus.Completed,
            _ => false
        };

        if (!allowed)
            throw new DomainRuleException($"A request cannot change from {Status} to {nextStatus}.");

        Status = nextStatus;
    }
}