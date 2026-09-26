using System.Globalization;
using AlipoorBehTask.Domain.DTOs;
using AlipoorBehTask.Domain.Events;
using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.Aggregates;

public sealed class Beneficiary : Entity<Guid>, IBeneficiary
{
    private readonly List<ServiceRequest> requests = [];

    private Beneficiary()
    {
        NationalId = string.Empty;
    }

    public Beneficiary(IBeneficiaryRegistrationDTO registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        Update(registration);
        Id = Guid.NewGuid();
        AddDomainEvent(new BeneficiaryRegisteredEvent(Id));
    }

    public void Update(IBeneficiaryRegistrationDTO registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        NationalId = NormalizeNationalId(registration.NationalId);
        Age = registration.Age ?? throw new DomainRuleException("Age is required.");
        MaritalStatus = registration.MaritalStatus ?? throw new DomainRuleException("Marital status is required.");
        DependentCount = registration.DependentCount ?? throw new DomainRuleException("Dependent count is required.");
        DisabilityType = registration.DisabilityType ?? throw new DomainRuleException("Disability type is required.");
        MonthlyIncome = registration.MonthlyIncome ?? throw new DomainRuleException("Monthly income is required.");

        if (Age is < 0 or > 120)
            throw new DomainRuleException("Age must be between 0 and 120.");
        if (!Enum.IsDefined(MaritalStatus))
            throw new DomainRuleException("Marital status is not supported.");
        if (DependentCount is < 0 or > 50)
            throw new DomainRuleException("Dependent count must be between 0 and 50.");
        if (!Enum.IsDefined(DisabilityType))
            throw new DomainRuleException("Disability type is not supported.");
        if (MonthlyIncome < 0)
            throw new DomainRuleException("Monthly income cannot be negative.");
    }

    public string NationalId { get; private set; }
    public int Age { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public int DependentCount { get; private set; }
    public DisabilityType DisabilityType { get; private set; }
    public decimal MonthlyIncome { get; private set; }
    public IReadOnlyCollection<ServiceRequest> Requests => requests;

    public ServiceRequest AddRequest(
        IServiceRequestRegistrationDTO registration,
        DateOnly registeredOn,
        int initialPriorityScore)
    {
        ArgumentNullException.ThrowIfNull(registration);
        var serviceType = registration.ServiceType
            ?? throw new DomainRuleException("Service type is required.");
        var description = registration.Description;
        if (!Enum.IsDefined(serviceType))
            throw new DomainRuleException("Service type is not supported.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainRuleException("Description is required.");
        if (description.Trim().Length > 1000)
            throw new DomainRuleException("Description cannot exceed 1000 characters.");
        if (initialPriorityScore < 0)
            throw new DomainRuleException("Initial priority score cannot be negative.");

        var request = new ServiceRequest(Id, serviceType, registeredOn, description.Trim(), initialPriorityScore);
        requests.Add(request);
        AddDomainEvent(new ServiceRequestRegisteredEvent(Id, request.Id, serviceType, registeredOn));
        return request;
    }

    public void ChangeRequestStatus(Guid requestId, RequestStatus nextStatus, DateTimeOffset changedAtUtc)
    {
        var request = requests.SingleOrDefault(item => item.Id == requestId)
            ?? throw new DomainRuleException("The request does not belong to this beneficiary.");
        var previousStatus = request.Status;
        request.ChangeStatus(nextStatus);
        AddDomainEvent(new ServiceRequestStatusChangedEvent(Id, request.Id, previousStatus, nextStatus, changedAtUtc));
    }

    private static string NormalizeNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            throw new DomainRuleException("National ID is required.");

        var normalized = new string(nationalId.Trim().Select(character =>
        {
            var digit = CharUnicodeInfo.GetDecimalDigitValue(character);
            return digit is >= 0 and <= 9 ? (char)('0' + digit) : character;
        }).ToArray());

        if (normalized.Length != 10 || normalized.Any(character => !char.IsAsciiDigit(character)))
            throw new DomainRuleException("National ID must contain exactly 10 digits.");

        return normalized;
    }
}