using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Application.Queries;

public sealed class GetBeneficiaryQueryHandler(IBeneficiaryRepository beneficiaries)
    : IQueryHandler<GetBeneficiaryQuery, BeneficiaryResponse>
{
    public async Task<BeneficiaryResponse> HandleAsync(GetBeneficiaryQuery query, CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.Get(query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Beneficiary", query.Id.ToString());
        return ResponseMapping.ToResponse(beneficiary);
    }
}

public sealed class GetBeneficiariesQueryHandler(IBeneficiaryReadStore beneficiaries)
    : IQueryHandler<GetBeneficiariesQuery, PagedResult<BeneficiaryResponse>>
{
    public async Task<PagedResult<BeneficiaryResponse>> HandleAsync(
        GetBeneficiariesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1)
            throw new AlipoorBehTask.Domain.DomainRuleException("Page must be greater than zero.");
        if (query.PageSize is < 1 or > 100)
            throw new AlipoorBehTask.Domain.DomainRuleException("Page size must be between 1 and 100.");
        if (!new[] { "nationalId", "age", "income", "dependents" }.Contains(query.SortBy, StringComparer.OrdinalIgnoreCase))
            throw new AlipoorBehTask.Domain.DomainRuleException("Beneficiary sort field is not supported.");

        return await beneficiaries.GetPageAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.SortBy,
            query.SortDirection,
            cancellationToken);
    }
}