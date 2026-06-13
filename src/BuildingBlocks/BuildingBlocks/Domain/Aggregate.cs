using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Domain;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
  private readonly List<IDomainEvent> _domainEvents = new();

  public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public IDomainEvent[] ClearDomainEvents()
  {
    IDomainEvent[] dequeuedEvents = _domainEvents.ToArray();

    _domainEvents.Clear();

    return dequeuedEvents;
  }

  public void AddDomainEvents(IDomainEvent domainEvent) =>
    _domainEvents.Add(domainEvent);
}
