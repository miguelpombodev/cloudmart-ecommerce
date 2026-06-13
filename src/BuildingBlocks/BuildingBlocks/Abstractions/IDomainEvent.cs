using MediatR;

namespace BuildingBlocks.Abstractions;

public interface IDomainEvent : INotification
{
  Guid EventId => Guid.NewGuid();

  DateTimeOffset OccuredOn => DateTime.Now;

  string? EventType => GetType().AssemblyQualifiedName;
}
