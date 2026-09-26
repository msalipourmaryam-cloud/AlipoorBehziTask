using AlipoorBehTask.Domain;

namespace AlipoorBehTask.Domain.DTOs;

public interface IBeneficiaryRegistrationDTO
{
    string? NationalId { get; }
    int? Age { get; }
    MaritalStatus? MaritalStatus { get; }
    int? DependentCount { get; }
    DisabilityType? DisabilityType { get; }
    decimal? MonthlyIncome { get; }
}