using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;

namespace AlipoorBehTask.Application.Queries;

public static class ResponseMapping
{
    public static BeneficiaryResponse ToResponse(IBeneficiary beneficiary) => new(
        beneficiary.Id,
        beneficiary.NationalId,
        beneficiary.Age,
        beneficiary.MaritalStatus,
        beneficiary.DependentCount,
        beneficiary.DisabilityType,
        beneficiary.MonthlyIncome);

    public static ServiceRequestResponse ToResponse(
        IServiceRequest request,
        IBeneficiary beneficiary,
        PriorityResult priority) => new(
            request.Id,
            request.BeneficiaryId,
            beneficiary.NationalId,
            request.ServiceType,
            request.RegistrationDate,
            request.Description,
            request.Status,
            request.InitialPriorityScore,
            priority.TotalScore,
            priority.Factors);
}