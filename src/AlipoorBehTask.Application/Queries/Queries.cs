using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;

namespace AlipoorBehTask.Application.Queries;

public sealed record GetBeneficiaryQuery(Guid Id) : IQuery<BeneficiaryResponse>;
public sealed record GetBeneficiariesQuery(
    int Page,
    int PageSize,
    string? Search,
    string SortBy,
    string SortDirection) : IQuery<PagedResult<BeneficiaryResponse>>;
public sealed record GetServiceRequestQuery(Guid Id) : IQuery<ServiceRequestResponse>;
public sealed record GetPriorityQueueQuery(
    int Page,
    int PageSize,
    ServiceType? ServiceType,
    RequestStatus? Status,
    string? Search,
    string SortBy,
    string SortDirection) : IQuery<QueuePage>;
public sealed record GetRequestsSummaryQuery : IQuery<RequestsSummary>;