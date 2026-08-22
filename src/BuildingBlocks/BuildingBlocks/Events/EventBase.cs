namespace BuildingBlocks.Events;

public abstract class EventBase : IEvent
{
  public Guid Id { get; set; } = Guid.NewGuid();
}
