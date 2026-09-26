using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace AlipoorBehTask.Infrastructure.Repositories;

public sealed class EfBeneficiaryReadStore(AlipoorBehTaskDbContext context) : IBeneficiaryReadStore
{
    public async Task<PagedResult<BeneficiaryResponse>> GetPageAsync(
        int page,
        int pageSize,
        string? search,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken)
    {
        var query = context.Beneficiaries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(beneficiary => beneficiary.NationalId.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var ordered = sortBy.ToLowerInvariant() switch
        {
            "age" => descending ? query.OrderByDescending(item => item.Age) : query.OrderBy(item => item.Age),
            "income" => descending ? query.OrderByDescending(item => item.MonthlyIncome) : query.OrderBy(item => item.MonthlyIncome),
            "dependents" => descending ? query.OrderByDescending(item => item.DependentCount) : query.OrderBy(item => item.DependentCount),
            _ => descending ? query.OrderByDescending(item => item.NationalId) : query.OrderBy(item => item.NationalId)
        };

        var offset = (long)(page - 1) * pageSize;
        var beneficiaries = offset >= totalCount
            ? []
            : await ordered.ThenBy(item => item.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return new PagedResult<BeneficiaryResponse>(
            page,
            pageSize,
            totalCount,
            beneficiaries.Select(item => new BeneficiaryResponse(
                item.Id,
                item.NationalId,
                item.Age,
                item.MaritalStatus,
                item.DependentCount,
                item.DisabilityType,
                item.MonthlyIncome)).ToArray());
    }
}

public sealed class EfServiceRequestReadStore(AlipoorBehTaskDbContext context) : IServiceRequestReadStore
{
    public async Task<PagedResult<ServiceRequestQueueRecord>> GetQueuePageAsync(
        int page,
        int pageSize,
        ServiceType? serviceType,
        RequestStatus? status,
        string? search,
        string sortBy,
        string sortDirection,
        decimal povertyIncomeThreshold,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        var baseQuery = from request in context.ServiceRequests.AsNoTracking()
                        join beneficiary in context.Beneficiaries.AsNoTracking()
                            on request.BeneficiaryId equals beneficiary.Id
                        select new { Request = request, Beneficiary = beneficiary };

        if (serviceType is not null)
            baseQuery = baseQuery.Where(item => item.Request.ServiceType == serviceType.Value);
        if (status is not null)
            baseQuery = baseQuery.Where(item => item.Request.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(item => item.Beneficiary.NationalId.Contains(term)
                || item.Request.Description.Contains(term));
        }

        var query = baseQuery.Select(item => new
        {
            item.Request,
            item.Beneficiary,
            PriorityScore = (item.Beneficiary.Age > 70 ? PriorityCalculator.AgeOver70Points : 0)
                + (item.Beneficiary.DisabilityType == DisabilityType.Severe ? PriorityCalculator.SevereDisabilityPoints : 0)
                + (item.Beneficiary.MonthlyIncome < povertyIncomeThreshold ? PriorityCalculator.BelowPovertyThresholdPoints : 0)
                + item.Beneficiary.DependentCount * PriorityCalculator.PerDependentPoints
                + (((asOfDate.Year - item.Request.RegistrationDate.Year) * 12
                    + asOfDate.Month - item.Request.RegistrationDate.Month
                    - (item.Request.RegistrationDate.Day > asOfDate.Day ? 1 : 0)) > 0
                    ? ((asOfDate.Year - item.Request.RegistrationDate.Year) * 12
                        + asOfDate.Month - item.Request.RegistrationDate.Month
                        - (item.Request.RegistrationDate.Day > asOfDate.Day ? 1 : 0)) * PriorityCalculator.PerWaitingMonthPoints
                : 0)
        });

        var totalCount = await query.CountAsync(cancellationToken);
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var ordered = sortBy.ToLowerInvariant() switch
        {
            "date" => descending ? query.OrderByDescending(item => item.Request.RegistrationDate) : query.OrderBy(item => item.Request.RegistrationDate),
            "person" => descending ? query.OrderByDescending(item => item.Beneficiary.NationalId) : query.OrderBy(item => item.Beneficiary.NationalId),
            "service" => descending ? query.OrderByDescending(item => item.Request.ServiceType) : query.OrderBy(item => item.Request.ServiceType),
            "status" => descending ? query.OrderByDescending(item => item.Request.Status) : query.OrderBy(item => item.Request.Status),
            _ => descending ? query.OrderByDescending(item => item.PriorityScore) : query.OrderBy(item => item.PriorityScore)
        };

        if (sortBy.Equals("priority", StringComparison.OrdinalIgnoreCase))
            ordered = (descending ? ordered.ThenBy(item => item.Request.RegistrationDate) : ordered.ThenByDescending(item => item.Request.RegistrationDate))
                .ThenBy(item => item.Request.Id);
        else
            ordered = ordered.ThenBy(item => item.Request.Id);

        var offset = (long)(page - 1) * pageSize;
        var rows = offset >= totalCount
            ? []
            : await ordered.Skip((int)offset).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<ServiceRequestQueueRecord>(
            page,
            pageSize,
            totalCount,
            rows.Select(item => new ServiceRequestQueueRecord(item.Request, item.Beneficiary)).ToArray());
    }
}