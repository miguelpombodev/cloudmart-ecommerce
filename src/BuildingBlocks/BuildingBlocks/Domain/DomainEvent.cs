using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Domain;

public abstract class DomainEvent : IDomainEvent
{
  protected DomainEvent()
  {
    EventId = Guid.NewGuid();
    OccuredOn = DateTimeOffset.UtcNow;
  }

  public Guid EventId { get; }

  public DateTimeOffset OccuredOn { get; }
}
