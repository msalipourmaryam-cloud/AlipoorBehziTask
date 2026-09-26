namespace AlipoorBehTask.Domain;

using AlipoorBehTask.Domain.Aggregates;

public sealed record PriorityFactor(string Code, string Description, int Points);

public sealed record PriorityResult(int TotalScore, IReadOnlyList<PriorityFactor> Factors);

public sealed class PriorityCalculator(decimal povertyIncomeThreshold)
{
    public decimal PovertyIncomeThreshold => povertyIncomeThreshold;
    public const int AgeOver70Points = 30;
    public const int SevereDisabilityPoints = 40;
    public const int BelowPovertyThresholdPoints = 20;
    public const int PerDependentPoints = 5;
    public const int PerWaitingMonthPoints = 2;

    public PriorityResult Calculate(IBeneficiary beneficiary, DateOnly registrationDate, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(beneficiary);
        if (povertyIncomeThreshold < 0)
            throw new DomainRuleException("Poverty income threshold cannot be negative.");

        var waitingMonths = CompletedMonthsBetween(registrationDate, asOfDate);
        var factors = new List<PriorityFactor>
        {
            new("age_over_70", "Age over 70", beneficiary.Age > 70 ? AgeOver70Points : 0),
            new("severe_disability", "Severe disability", beneficiary.DisabilityType == DisabilityType.Severe ? SevereDisabilityPoints : 0),
            new("below_poverty_threshold", "Monthly income below the configured poverty threshold", beneficiary.MonthlyIncome < povertyIncomeThreshold ? BelowPovertyThresholdPoints : 0),
            new("dependents", $"{beneficiary.DependentCount} dependent(s) at 5 points each", beneficiary.DependentCount * PerDependentPoints),
            new("waiting_months", $"{waitingMonths} completed waiting month(s) at 2 points each", waitingMonths * PerWaitingMonthPoints)
        };

        return new PriorityResult(factors.Sum(factor => factor.Points), factors);
    }

    private static int CompletedMonthsBetween(DateOnly start, DateOnly end)
    {
        if (end <= start)
            return 0;

        var months = (end.Year - start.Year) * 12 + end.Month - start.Month;
        if (start.AddMonths(months) > end)
            months--;

        return Math.Max(0, months);
    }
}