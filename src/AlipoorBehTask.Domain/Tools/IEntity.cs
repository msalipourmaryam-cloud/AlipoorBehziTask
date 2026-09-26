namespace AlipoorBehTask.Domain.Tools;

public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
    TKey Id { get; }
    bool IsTransient();
    IReadOnlyCollection<IEventNotification> DomainEvents { get; }
    void AddDomainEvent(IEventNotification domainEvent);
    void RemoveDomainEvent(IEventNotification domainEvent);
    void ClearDomainEvents();
}