using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Application.Commands;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class PriorityCalculatorTests
{
    [Fact]
    public void Calculate_AppliesAllAssignmentRulesAndExplainsEachFactor()
    {
        var beneficiary = CreateBeneficiary("0123456789", 71, 2, DisabilityType.Severe, 100);
        var calculator = new PriorityCalculator(1_000);

        var result = calculator.Calculate(
            beneficiary,
            new DateOnly(2025, 1, 31),
            new DateOnly(2025, 3, 31));

        Assert.Equal(104, result.TotalScore);
        Assert.Collection(
            result.Factors,
            factor => Assert.Equal(("age_over_70", 30), (factor.Code, factor.Points)),
            factor => Assert.Equal(("severe_disability", 40), (factor.Code, factor.Points)),
            factor => Assert.Equal(("below_poverty_threshold", 20), (factor.Code, factor.Points)),
            factor => Assert.Equal(("dependents", 10), (factor.Code, factor.Points)),
            factor => Assert.Equal(("waiting_months", 4), (factor.Code, factor.Points)));
    }

    [Fact]
    public void Calculate_CountsOnlyCompletedWaitingMonths()
    {
        var beneficiary = CreateBeneficiary("0123456789", 70, 0, DisabilityType.None, 1_000);

        var result = new PriorityCalculator(1_000).Calculate(
            beneficiary,
            new DateOnly(2025, 1, 31),
            new DateOnly(2025, 3, 30));

        Assert.Equal(2, result.TotalScore);
        Assert.Equal(1, result.Factors.Single(factor => factor.Code == "waiting_months").Points / 2);
    }

    [Fact]
    public void Beneficiary_NormalizesPersianDigitsInNationalId()
    {
        var beneficiary = CreateBeneficiary("۰۱۲۳۴۵۶۷۸۹", 35, 0, DisabilityType.None, 0);

        Assert.Equal("0123456789", beneficiary.NationalId);
    }

    private static Beneficiary CreateBeneficiary(
        string nationalId,
        int age,
        int dependents,
        DisabilityType disabilityType,
        decimal income) => new(new CreateBeneficiaryInput(
            nationalId,
            age,
            MaritalStatus.Single,
            dependents,
            disabilityType,
            income));
}