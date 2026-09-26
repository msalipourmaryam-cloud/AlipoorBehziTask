using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Domain.Tools;

namespace AlipoorBehTask.Domain.IRepositories;

public interface IBeneficiaryRepository : IRepository<IBeneficiary>
{
    Task<IBeneficiary?> Get(Guid id, CancellationToken cancellationToken);
    Task<IBeneficiary?> GetByNationalId(string nationalId, CancellationToken cancellationToken);
    Task<IBeneficiary?> GetByRequestId(Guid requestId, CancellationToken cancellationToken);
    Task<IReadOnlyList<IBeneficiary>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<IBeneficiary>> GetMany(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    void Add(IBeneficiary beneficiary);
    void Remove(IBeneficiary beneficiary);
}