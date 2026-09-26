using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Application.Queries;

public sealed class GetPriorityQueueQueryHandler(
    IServiceRequestReadStore serviceRequests,
    TimeProvider timeProvider,
    PriorityCalculator priorityCalculator) : IQueryHandler<GetPriorityQueueQuery, QueuePage>
{
    public async Task<QueuePage> HandleAsync(
        GetPriorityQueueQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1)
            throw new DomainRuleException("Page must be greater than zero.");
        if (query.PageSize is < 1 or > 100)
            throw new DomainRuleException("Page size must be between 1 and 100.");
        if (query.ServiceType is not null && !Enum.IsDefined(query.ServiceType.Value))
            throw new DomainRuleException("Service type is not supported.");
        if (query.Status is not null && !Enum.IsDefined(query.Status.Value))
            throw new DomainRuleException("Request status is not supported.");
        if (!new[] { "priority", "date", "person", "service", "status" }.Contains(query.SortBy, StringComparer.OrdinalIgnoreCase))
            throw new DomainRuleException("Request sort field is not supported.");

        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var result = await serviceRequests.GetQueuePageAsync(
            query.Page,
            query.PageSize,
            query.ServiceType,
            query.Status,
            query.Search,
            query.SortBy,
            query.SortDirection,
            priorityCalculator.PovertyIncomeThreshold,
            asOfDate,
            cancellationToken);
        var items = result.Items
                .Select(item => ResponseMapping.ToResponse(
                    item.Request,
                    item.Beneficiary,
                    priorityCalculator.Calculate(item.Beneficiary, item.Request.RegistrationDate, asOfDate)))
                .ToArray();

        return new QueuePage(result.Page, result.PageSize, result.TotalCount, items);
    }
}

public sealed class GetRequestsSummaryQueryHandler(IBeneficiaryRepository beneficiaries)
    : IQueryHandler<GetRequestsSummaryQuery, RequestsSummary>
{
    public async Task<RequestsSummary> HandleAsync(
        GetRequestsSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var aggregates = await beneficiaries.GetAll(cancellationToken);
        var counts = aggregates
            .SelectMany(beneficiary => beneficiary.Requests)
            .GroupBy(request => (request.ServiceType, request.Status))
            .ToDictionary(group => group.Key, group => group.Count());

        var rows = Enum.GetValues<ServiceType>()
            .SelectMany(serviceType => Enum.GetValues<RequestStatus>()
                .Select(status => new RequestSummaryRow(serviceType, status, counts.GetValueOrDefault((serviceType, status)))))
            .ToArray();
        return new RequestsSummary(rows);
    }
}
