using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class BeneficiaryDomainTests
{
    [Fact]
    public void Constructor_SetsBeneficiaryProperties()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1234567890",
            42,
            MaritalStatus.Married,
            2,
            DisabilityType.Moderate,
            18_000m));

        Assert.Equal("1234567890", beneficiary.NationalId);
        Assert.Equal(42, beneficiary.Age);
        Assert.Equal(MaritalStatus.Married, beneficiary.MaritalStatus);
        Assert.Equal(2, beneficiary.DependentCount);
        Assert.Equal(DisabilityType.Moderate, beneficiary.DisabilityType);
        Assert.Equal(18_000m, beneficiary.MonthlyIncome);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("abc1234567")]
    public void Update_RejectsInvalidNationalIdFormat(string nationalId)
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1111111111",
            30,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            0m));

        var exception = Assert.Throws<DomainRuleException>(() => beneficiary.Update(new CreateBeneficiaryInput(
            nationalId,
            32,
            MaritalStatus.Single,
            1,
            DisabilityType.None,
            10_000m)));

        Assert.Contains("National ID", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Update_RejectsAgeOutsideRange(int age)
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1111111111",
            30,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            0m));

        var exception = Assert.Throws<DomainRuleException>(() => beneficiary.Update(new CreateBeneficiaryInput(
            "2222222222",
            age,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            10_000m)));

        Assert.Contains("Age", exception.Message);
    }

    [Fact]
    public void AddRequest_RejectsMissingDescription()
    {
        var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
            "1111111111",
            30,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            0m));

        var exception = Assert.Throws<DomainRuleException>(() => beneficiary.AddRequest(
            new CreateServiceRequestInput(Guid.Empty, ServiceType.Wheelchair, string.Empty),
            new DateOnly(2025, 1, 1),
            10));

        Assert.Contains("Description", exception.Message);
    }
}
