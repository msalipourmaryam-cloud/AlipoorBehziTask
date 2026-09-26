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

    [Fact]
    public void Update_ChangesBeneficiaryDetails()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "0123456789", 35, MaritalStatus.Single, 0, DisabilityType.None, 0));

        beneficiary.Update(new CreateBeneficiaryInput(
            "9876543210", 42, MaritalStatus.Married, 2, DisabilityType.Severe, 15_000m));

        Assert.Equal("9876543210", beneficiary.NationalId);
        Assert.Equal(42, beneficiary.Age);
        Assert.Equal(MaritalStatus.Married, beneficiary.MaritalStatus);
        Assert.Equal(2, beneficiary.DependentCount);
        Assert.Equal(DisabilityType.Severe, beneficiary.DisabilityType);
        Assert.Equal(15_000m, beneficiary.MonthlyIncome);
    }

    [Fact]
    public void Update_RejectsInvalidNationalId()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "0123456789", 35, MaritalStatus.Single, 0, DisabilityType.None, 0));

        Assert.Throws<DomainRuleException>(() => beneficiary.Update(
            new CreateBeneficiaryInput("12345", 42, MaritalStatus.Married, 1, DisabilityType.None, 10_000m)));
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