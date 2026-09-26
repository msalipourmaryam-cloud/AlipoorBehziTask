namespace AlipoorBehTask.Domain.Tools;

public interface IRepository
{
}

public interface IRepository<TAggregate> : IRepository
{
    IUnitOfWork UnitOfWork { get; }
}