using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Domain.Aggregates;

namespace AlipoorBehTask.Application.Queries;

public interface IBeneficiaryReadStore
{
    Task<PagedResult<BeneficiaryResponse>> GetPageAsync(
        int page,
        int pageSize,
        string? search,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken);
}

public interface IServiceRequestReadStore
{
    Task<PagedResult<ServiceRequestQueueRecord>> GetQueuePageAsync(
        int page,
        int pageSize,
        AlipoorBehTask.Domain.ServiceType? serviceType,
        AlipoorBehTask.Domain.RequestStatus? status,
        string? search,
        string sortBy,
        string sortDirection,
        decimal povertyIncomeThreshold,
        DateOnly asOfDate,
        CancellationToken cancellationToken);
}