using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Application.Commands;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class ServiceRequestTests
{
    [Fact]
    public void ChangeStatus_EnforcesAllowedTransitions()
    {
        var (beneficiary, request) = CreateRequest();

        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Approved, DateTimeOffset.UtcNow);
        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Completed, DateTimeOffset.UtcNow);

        Assert.Equal(RequestStatus.Completed, request.Status);
        Assert.Throws<DomainRuleException>(() => beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Pending, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ChangeStatus_RejectsDirectCompletionFromPending()
    {
        var (beneficiary, request) = CreateRequest();

        Assert.Throws<DomainRuleException>(() => beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Completed, DateTimeOffset.UtcNow));
        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    private static (Beneficiary Beneficiary, ServiceRequest Request) CreateRequest()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "0123456789", 35, MaritalStatus.Single, 0, DisabilityType.None, 0));
        var request = beneficiary.AddRequest(
            new CreateServiceRequestInput(beneficiary.Id, ServiceType.Wheelchair, "Mobility assistance"),
            new DateOnly(2025, 1, 1),
            0);
        return (beneficiary, request);
    }
}