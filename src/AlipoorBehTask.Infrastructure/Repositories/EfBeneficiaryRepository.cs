using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Domain.IRepositories;
using AlipoorBehTask.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace AlipoorBehTask.Infrastructure.Repositories;

public sealed class EfBeneficiaryRepository(AlipoorBehTaskDbContext context) : IBeneficiaryRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<IBeneficiary?> Get(Guid id, CancellationToken cancellationToken) =>
        await context.Beneficiaries.Include(beneficiary => beneficiary.Requests)
            .FirstOrDefaultAsync(beneficiary => beneficiary.Id == id, cancellationToken);

    public async Task<IBeneficiary?> GetByNationalId(string nationalId, CancellationToken cancellationToken) =>
        await context.Beneficiaries.Include(beneficiary => beneficiary.Requests)
            .FirstOrDefaultAsync(beneficiary => beneficiary.NationalId == nationalId, cancellationToken);

    public async Task<IBeneficiary?> GetByRequestId(Guid requestId, CancellationToken cancellationToken) =>
        await context.Beneficiaries.Include(beneficiary => beneficiary.Requests)
            .FirstOrDefaultAsync(beneficiary => beneficiary.Requests.Any(request => request.Id == requestId), cancellationToken);

    public async Task<IReadOnlyList<IBeneficiary>> GetAll(CancellationToken cancellationToken)
    {
        var beneficiaries = await context.Beneficiaries.AsNoTracking()
            .Include(beneficiary => beneficiary.Requests)
            .OrderBy(beneficiary => beneficiary.NationalId)
            .ToListAsync(cancellationToken);
        return beneficiaries.Cast<IBeneficiary>().ToArray();
    }

    public async Task<IReadOnlyList<IBeneficiary>> GetMany(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return Array.Empty<IBeneficiary>();

        var beneficiaries = await context.Beneficiaries.AsNoTracking()
            .Where(beneficiary => ids.Contains(beneficiary.Id))
            .ToListAsync(cancellationToken);
        return beneficiaries.Cast<IBeneficiary>().ToArray();
    }

    public void Add(IBeneficiary beneficiary)
    {
        if (beneficiary is not Beneficiary model)
            throw new ArgumentException("Unsupported beneficiary aggregate implementation.", nameof(beneficiary));
        context.Beneficiaries.Add(model);
    }

    public void Remove(IBeneficiary beneficiary)
    {
        if (beneficiary is not Beneficiary model)
            throw new ArgumentException("Unsupported beneficiary aggregate implementation.", nameof(beneficiary));
        context.Beneficiaries.Remove(model);
    }
}