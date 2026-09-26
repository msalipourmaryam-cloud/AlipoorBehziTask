using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class BeneficiaryEdgeCaseTests
{
    [Fact]
    public void Update_RejectsNegativeIncome()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1010101010",
            39,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            200_000m));

        var exception = Assert.Throws<DomainRuleException>(() => beneficiary.Update(new CreateBeneficiaryInput(
            "2020202020",
            39,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            -1m)));

        Assert.Contains("Monthly income", exception.Message);
    }

    [Fact]
    public void Update_RejectsInvalidMaritalStatus()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1010101010",
            39,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            200_000m));

        Assert.Throws<DomainRuleException>(() => beneficiary.Update(new CreateBeneficiaryInput(
            "2020202020",
            39,
            (MaritalStatus)99,
            0,
            DisabilityType.None,
            200_000m)));
    }

    [Fact]
    public void Update_RejectsInvalidDisabilityType()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1010101010",
            39,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            200_000m));

        Assert.Throws<DomainRuleException>(() => beneficiary.Update(new CreateBeneficiaryInput(
            "2020202020",
            39,
            MaritalStatus.Single,
            0,
            (DisabilityType)99,
            200_000m)));
    }

    [Fact]
    public void Update_AcceptsBoundaryValues()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1010101010",
            39,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            200_000m));

        beneficiary.Update(new CreateBeneficiaryInput(
            "2020202020",
            120,
            MaritalStatus.Married,
            50,
            DisabilityType.Severe,
            0m));

        Assert.Equal(120, beneficiary.Age);
        Assert.Equal(50, beneficiary.DependentCount);
        Assert.Equal(DisabilityType.Severe, beneficiary.DisabilityType);
    }
}
