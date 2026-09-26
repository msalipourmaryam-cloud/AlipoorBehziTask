namespace AlipoorBehTask.Domain.Tools;

public interface IUnitOfWork
{
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken);
}