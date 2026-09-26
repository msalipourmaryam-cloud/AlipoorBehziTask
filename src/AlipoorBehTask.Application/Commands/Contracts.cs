using System.ComponentModel.DataAnnotations;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.DTOs;

namespace AlipoorBehTask.Application.Commands;

public sealed record CreateBeneficiaryInput(
    [param: Required] string? NationalId,
    [param: Required] int? Age,
    [param: Required] MaritalStatus? MaritalStatus,
    [param: Required] int? DependentCount,
    [param: Required] DisabilityType? DisabilityType,
    [param: Required] decimal? MonthlyIncome) : IBeneficiaryRegistrationDTO;

public sealed record CreateServiceRequestInput(
    [param: Required] Guid? BeneficiaryId,
    [param: Required] ServiceType? ServiceType,
    [param: Required] string? Description) : IServiceRequestRegistrationDTO;

public sealed record ChangeRequestStatusInput([param: Required] RequestStatus? Status);

public sealed record BeneficiaryResponse(
    Guid Id,
    string NationalId,
    int Age,
    MaritalStatus MaritalStatus,
    int DependentCount,
    DisabilityType DisabilityType,
    decimal MonthlyIncome);

public sealed record ServiceRequestResponse(
    Guid Id,
    Guid BeneficiaryId,
    string BeneficiaryNationalId,
    ServiceType ServiceType,
    DateOnly RegistrationDate,
    string Description,
    RequestStatus Status,
    int InitialPriorityScore,
    int PriorityScore,
    IReadOnlyList<PriorityFactor> PriorityBreakdown);

public sealed record QueuePage(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<ServiceRequestResponse> Items);

public sealed record PagedResult<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);

public sealed record ServiceRequestQueueRecord(
    AlipoorBehTask.Domain.Aggregates.ServiceRequest Request,
    AlipoorBehTask.Domain.Aggregates.Beneficiary Beneficiary);

public sealed record RequestSummaryRow(ServiceType ServiceType, RequestStatus Status, int Count);
public sealed record RequestsSummary(IReadOnlyList<RequestSummaryRow> Items);