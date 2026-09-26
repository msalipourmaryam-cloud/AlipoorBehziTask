namespace AlipoorBehTask.Domain.Tools;

public abstract class Entity<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
{
    private readonly List<IEventNotification> domainEvents = [];

    public TKey Id { get; protected set; } = default!;
    public IReadOnlyCollection<IEventNotification> DomainEvents => domainEvents;

    public bool IsTransient() => EqualityComparer<TKey>.Default.Equals(Id, default!);

    public void AddDomainEvent(IEventNotification domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IEventNotification domainEvent) => domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => domainEvents.Clear();
}